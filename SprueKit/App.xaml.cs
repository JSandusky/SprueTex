using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Windows;
using System.Timers;
using System.IO;
using System.Reflection;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

namespace SprueKit
{
    public class InfoMessage : BaseClass
    {
        int id_;
        string text_;
        string cmd_;
        bool rejected_ = false;
        bool canReject_ = true;
        DateTime expireOn_ = DateTime.MaxValue;

        public bool ShouldShow { get { return rejected_ || DateTime.Now > expireOn_; } set { rejected_ = value; OnPropertyChanged(); } }

        public int ID { get { return id_; } set { id_ = value; OnPropertyChanged(); } }
        public string Text { get { return text_; } set { text_ = value; OnPropertyChanged(); } }
        public string Cmd { get { return cmd_; } set { cmd_ = value; OnPropertyChanged(); } }
        public bool Rejected { get { return rejected_; } set { rejected_ = value; OnPropertyChanged(); OnPropertyChanged("ShouldShow"); } }
        public bool CanReject { get { return canReject_; } set { canReject_ = value; OnPropertyChanged(); } }
        public DateTime Expiration { get { return expireOn_; } set { expireOn_ = value; OnPropertyChanged(); } }
    }

    public class WinMsg
    {
        public string Text { get; set; }
        public int Tag { get; set; } = 0;
        public float Duration { get; set; } = 6.0f;
    }

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, INotifyPropertyChanged
    {
        private static Mutex singleInstanceMutex_;
        public static string AppName { get; set; } = "SprueTex";
        public static string DisplayAppName { get; set; } = "SprueTex";

        public List<DocumentRecord> DocumentTypes { get; set; } = new List<DocumentRecord>();

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        System.Timers.Timer errTimer;

        public class ShortCutGroup : BaseClass
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public Type AppliesToType { get; set; }
            public Commands.ShortCut[] ShortCuts { get; set; }

            public void Serialize(XmlElement into)
            {
                var me = into.CreateChild("group");
                me.SetAttribute("name", Name);
                foreach (var shortCut in ShortCuts)
                    shortCut.Serialize(me);
            }

            public void Deserialize(XmlElement from)
            {
                var nodes = from.SelectNodes("shortcut");
                foreach (XmlElement shortcut in nodes)
                {
                    string cmdName = shortcut.GetStringElement("command");
                    string binding = shortcut.GetStringElement("binding");

                    var target = ShortCuts.FirstOrDefault(s => s.Command.Name.Equals(cmdName));
                    if (target != null)
                        target.SetBinding(binding);
                }
            }
        }

        public static bool BlockShortCuts = false;
        public static List<ShortCutGroup> ShortCuts = new List<ShortCutGroup>();
        public static List<WinMsg> WindowMessages = new List<WinMsg>();
        public static void PushWindowMessage(WinMsg msg) { lock (WindowMessages) { WindowMessages.Add(msg); } }
        public static void RunWindowMessages(float time)
        {
            lock (WindowMessages)
            {
                for (int i = 0; i < WindowMessages.Count; ++i)
                {
                    if (WindowMessages[i].Duration > 0.0f)
                    {
                        WindowMessages[i].Duration -= time;
                        if (WindowMessages[i].Duration <= 0)
                        {
                            WindowMessages.RemoveAt(i);
                            --i;
                        }
                    }
                }
            }
        }
        public static void WipeWindowMessage(int tag)
        {
            lock (WindowMessages)
            {
                for (int i = 0; i < WindowMessages.Count; ++i)
                {
                    if (WindowMessages[i].Tag == tag)
                    {
                        WindowMessages.RemoveAt(i);
                        --i;
                    }
                }
            }
        }
        static App inst_;

        public static Tasks.TaskThread MainTaskThread { get; private set; }
        public static Tasks.TaskSource SecondaryTaskSource { get; private set; } = new Tasks.TaskSource();
        public static List<Tasks.TaskThread> SecondaryTaskThreads { get; private set; } = new List<Tasks.TaskThread>();

        public DocumentManager DocumentManager { get; private set; } = new DocumentManager();

        SelectionContext selectionContext_ = new SelectionContext();
        public SelectionContext Selection { get { return selectionContext_; } set { selectionContext_ = value; OnPropertyChanged(); } }

        public ObservableCollection<InfoMessage> Messages { get; private set; } = new ObservableCollection<InfoMessage>();

        public Data.ReportDatabase Reports { get; set; } = new Data.ReportDatabase();

        public App()
        {
#if IS_DEMO
            DisplayAppName = "SprueTex DEMO";
#endif
            
        }

        EventWaitHandle eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, AppName);
        bool LockApp()
        {
            bool createdNew = false;
            string appName = App.AppName + "Mutex";
            singleInstanceMutex_ = new Mutex(true, appName, out createdNew);
            if (!createdNew)
            {
                //app is already running! Exiting the application  
                eventWaitHandle.Set();
                return false;
            }
            else
            {
                var thread = new Thread(() =>
                {
                    while (eventWaitHandle.WaitOne())
                    {
                        Current.Dispatcher.BeginInvoke((Action)(() => ((MainWindow)Current.MainWindow).BringToForeground()));
                    }
                });
                thread.IsBackground = true;
                thread.Start();
                return true;
            }
        }

        void InitializeMinimal()
        {
            UserData.inst();
            // For direct access, plus possibility to reroute if needed later
            IOCLite.Register(UserData.inst().GeneralSettings);
            IOCLite.Register(UserData.inst().ViewportSettings);
            IOCLite.Register(UserData.inst().MeshingSettings);
            IOCLite.Register(UserData.inst().UVGenerationSettings);
            IOCLite.Register(UserData.inst().TextureGraphSettings);
            IOCLite.Register(UserData.inst().BitNames);
        }

        void InitializeRequired()
        {
            InitShortCuts();
            LoadShortCuts();
            Reports.Deserialize();
            inst_ = this;
            Exit += App_Exit;

#region Documenthandlers

            DocumentTypes.Add(new Data.TexGen.TexGenDocumentRecord());
//#if SPRUE_MODEL
            DocumentTypes.Add(new Data.SprueModelDocumentRecord());
            DocumentTypes.Add(new Data.IKAnim.IKAnimDocumentRecord());
            //#endif

            #endregion

            #region IOC Setup

            IOCLite.Register(PluginManager.inst());
            IOCLite.Register(DocumentManager);
            IOCLite.Register(selectionContext_);
            IOCLite.Register(Data.ResourceCache.inst());

            

            // DO NOT DO! Without NGen this is WAY too slow
            //IOCInitializedAttribute.Execute();

#endregion
        }

        public void AddMessage(InfoMessage msg)
        {
            if (Messages.FirstOrDefault(m => m.ID == msg.ID) != null)
                return;
            Messages.Add(msg);
        }

        void InitializeGUI()
        {
            //System.Runtime.GCSettings.LatencyMode = System.Runtime.GCLatencyMode.Interactive;
            InfoMsgHelper.Load(this);
#if IS_DEMO
            if (Messages.FirstOrDefault(m => m.ID == 0) == null)
                Messages.Add(new SprueKit.InfoMessage { CanReject = false, Text = "You're currently using a demo version, when you're ready get the full version at [url=http://spruekit.itch.io/spruetex]Itch.io[/url]" });
#endif

            // Release builds will attempt to swallow errors as much as possible with the opportunity to report the error
            // Debug builds will not
#if !DEBUG
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            errTimer = new System.Timers.Timer();
            errTimer.Enabled = true;
            errTimer.AutoReset = true;
            errTimer.Interval = 200;
            errTimer.Elapsed += errTimer_Elapsed;
            errTimer.Start();
#endif

            MainTaskThread = new Tasks.TaskThread(-1);
            int curIdx = -2;
            if (UserData.inst().GeneralSettings.CapWorkerThreads)
            {
                SecondaryTaskThreads.Add(new Tasks.TaskThread(-2, SecondaryTaskSource));
                SecondaryTaskThreads.Add(new Tasks.TaskThread(-3, SecondaryTaskSource));
                ErrorHandler.inst().Warning("Starting worker threads in capped processing mode");
            }
            else
            {
                for (int i = 0; i < Environment.ProcessorCount - 1 || i == 0; ++i)
                {
                    var newThread = new Tasks.TaskThread(curIdx, SecondaryTaskSource);
                    curIdx -= 1;
                    SecondaryTaskThreads.Add(newThread);
                }
                ErrorHandler.inst().Debug(string.Format("Started {0} worker threads", SecondaryTaskThreads.Count));
            }

            // Initialize binding
            SprueBindings.System.SetLogCallback(ErrorHandler.inst());

            //Util.DocHelpBuilder.BuildInternalHelp();
            //string sbsText = Data.TexGen.SubstanceLoader.ReadSBSFiles();
        }

        [DllImport("Kernel32.dll")]
        public static extern bool AttachConsole(int processId);
        [DllImport("Kernel32.dll")]
        public static extern bool FreeConsole();

        AppIPCServer mainIPC_ = null;
        protected override void OnStartup(StartupEventArgs e)
        {
            InitializeMinimal();

            // all valid commands will consist of more than 1 argument
            if (e.Args == null || e.Args.Length < 2)
            {
                if (!LockApp())
                {
                    // if we have a path then send that path to the IPC server for the single instance
                    if (e.Args.Length == 1)
                    {
                        // Root our path first, we're the only one that knows are relative current directory
                        string fileName = e.Args[0];
                        if (!System.IO.Path.IsPathRooted(fileName))
                            fileName = System.IO.Path.Combine(Environment.CurrentDirectory, fileName);

                        var ipc = new AppIPC(fileName);
                        while (!ipc.IsFinished)
                            continue;
                    }

                    Application.Current.Shutdown();
                    return;
                }
                // we're going to stay alive, so start an IPC server
                mainIPC_ = new AppIPCServer();
            }

            base.OnStartup(e);
            InitializeRequired();

            bool hasCmdLine = e.Args == null || e.Args.Length > 1 || (e.Args.Length == 1 && e.Args[0].ToLowerInvariant().Contains("help"));
            if (!hasCmdLine)
            {
                SplashScreen screen = new SplashScreen("Images/Splash.png");
                screen.Show(true);
                WPFExt.PrefetchIcons();
                PropertyPrefetch.Run();
                InitializeGUI();

                string filepath = "";
                if (e.Args != null && e.Args.Length == 1)
                    filepath = e.Args[0];

                new MainWindow().ShowDialog(filepath);
            }
            else
            {
                AttachConsole(-1);
                System.Console.WriteLine("");
                System.Console.WriteLine("");
                System.Console.WriteLine("Executing SprueKit in commandline mode");

                if (e.Args[0].ToLowerInvariant().Equals("help") || e.Args[0].ToLowerInvariant().Equals("-help"))
                {
                    System.Console.WriteLine("");
                    System.Console.WriteLine("usage:");
                    System.Console.WriteLine("");
                    System.Console.WriteLine("__________________________________________________________________");
                    System.Console.WriteLine("");
                    System.Console.WriteLine("Texture Graph Rendering");
                    System.Console.WriteLine("");
                    System.Console.WriteLine("    spruekit.exe texture <filename> <outputfilename> [switches]");
                    System.Console.WriteLine("");
                    System.Console.WriteLine("        Render a texture graph to the output");
                    System.Console.WriteLine("");
                    System.Console.WriteLine("        -captured         := disables progress printing for better pipe compatibility");
                    System.Console.WriteLine("        -random           := apply permutations randomly");
                    System.Console.WriteLine("        -node=\"name\"    := only output the named node");
                    System.Console.WriteLine("        -perm=\"name\"    := use specified permutation");

                    System.Console.WriteLine("");
                    System.Console.WriteLine("__________________________________________________________________");
                    System.Console.WriteLine("");
                    System.Console.WriteLine("Reports Generation");
                    System.Console.WriteLine("");
                    System.Console.WriteLine("    spruekit.exe report <filename|directoryname> <title> <outputfilename> [switches]");
                    System.Console.WriteLine("");
                    System.Console.WriteLine("        Generate a report");
                    System.Console.WriteLine("");
                    System.Console.WriteLine("        -compare          := generate a comparison report");
                    System.Console.WriteLine("        -details          := generate a detailed report");
                    System.Console.WriteLine("        -s                := recurse directories");
                    //System.Console.WriteLine("        -perm=\"name\"    := use specified permutation");
                }
                new AppCommandLine(e.Args);
                System.Windows.Forms.SendKeys.SendWait("{ENTER}");
                FreeConsole();
            }
            Shutdown();
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            //System.IO.File.WriteAllText("IconManifest.txt", WPFExt.DumpImageCache());
            ///string outText = Data.ShaderGen.CodeGen.GenerateCode(System.IO.File.ReadAllLines("C:\\dev\\Sprue\\SprueKit\\bin\\Release\\TestNodes.txt"));
            Reports.Serialize();
            SaveShortCuts();
            InfoMsgHelper.Save(this);
            UserData.inst().Save();
        }

        void errTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                this.Dispatcher.Invoke((Action)delegate ()
                {
                    checkErrs();
                });
            }
            catch (Exception) { }
        }

        void checkErrs(params object[] para)
        {
            if (ErrorHandler.inst().Check())
            {
                string msg = ErrorHandler.inst().GetMessage();
#if !DEBUG
                if (msg.Length > 0)
                {
                    var dlg = new Dlg.ErrorDlg(msg);
                    dlg.ShowDialog();
                    //Dlg.ErrorDlg.Show(msg);
                }
#endif
            }
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            if (ex != null)
                ErrorHandler.inst().Error(ex);
        }

        /// <summary>
        ///  Gets the path of something in the User application directory, creates the directories if necessary
        ///  TODO: actually check directory creation, unused path
        /// </summary>
        /// <param name="aPath"></param>
        /// <returns>The path to the file in question</returns>
        public static string DataPath(string aPath)
        {
            string asmName = Assembly.GetExecutingAssembly().GetName().Name;
            string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), string.Format("{0}\\{1}", asmName, aPath));
            if (!File.Exists(fileName))
                Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            return fileName;
        }

        /// <summary>
        /// Gets the path to something contained relative to the Program
        /// </summary>
        /// <param name="aPath"></param>
        /// <returns></returns>
        public static string ProgramPath(string aPath)
        {
            string basePath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            return Path.Combine(basePath, aPath);
        }

        public static string ContentPath(string aPath)
        {
            string basePath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            return Path.Combine(Path.Combine(basePath, "Content"), aPath);
        }

        static void SaveShortCuts()
        {
            string shortcutsPath = DataPath("shortcuts.xml");
            XmlDocument doc = new XmlDocument();
            var root = doc.CreateElement("shortcuts");
            doc.AppendChild(root);

            foreach (var grp in ShortCuts)
                grp.Serialize(root);

            doc.Save(shortcutsPath);
        }

        static void LoadShortCuts()
        {
            string shortcutsPath = DataPath("shortcuts.xml");
            if (System.IO.File.Exists(shortcutsPath))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(shortcutsPath);
                var groups = doc.DocumentElement.SelectNodes("group");
                foreach (XmlElement grp in groups)
                {
                    string grpName = grp.GetAttribute("name");
                    var target = App.ShortCuts.FirstOrDefault(sg => sg.Name.Equals(grpName));
                    if (target != null)
                        target.Deserialize(grp);
                }
            }
        }

        public static void Navigate(Uri path)
        {
            ((FirstFloor.ModernUI.Windows.Controls.ModernWindow)App.Current.MainWindow).ContentSource = path;
        }

        public static bool IsOnPage(Uri path)
        {
            return ((FirstFloor.ModernUI.Windows.Controls.ModernWindow)App.Current.MainWindow).ContentSource.Equals(path);
        }

        public static void EnqueueOptimal(Tasks.TaskItem item)
        {
            SecondaryTaskSource.AddTask(item);
        }

    }

    public static class InfoMsgHelper
    {
        public static void Load(App app)
        {
            string msgDatapath = App.DataPath("msg.xml");
            if (System.IO.File.Exists(msgDatapath))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(msgDatapath);
                var msgNodes = doc.DocumentElement.SelectNodes("//message");
                foreach (XmlElement elem in msgNodes)
                {
                    InfoMessage msg = new InfoMessage
                    {
                        ID = elem.GetIntAttribute("id"),
                        Text = elem.InnerText,
                        Cmd = elem.GetAttribute("cmd"),
                        Rejected = elem.GetIntAttribute("rejected") == 1,
                    };
#if IS_DEMO
                    msg.CanReject = msg.ID != 0;
#else
                    if (msg.ID == 0)
                        msg.Rejected = true;
                    msg.CanReject = true;
#endif
                    app.Messages.Add(msg);
                }
            }
        }

        public static void Save(App app)
        {
            string path = App.DataPath("msg.xml");
            XmlDocument doc = new XmlDocument();
            var root = doc.CreateElement("messages");
            doc.AppendChild(root);
            foreach (var msg in app.Messages)
            {
                var msgElem = doc.CreateElement("message");
                root.AppendChild(msgElem);
                msgElem.SetAttribute("id", msg.ID.ToString());
                msgElem.InnerText = msg.Text;
                msgElem.SetAttribute("cmd", msg.Cmd);
                msgElem.SetAttribute("rejected", msg.Rejected ? "1" : "0");
            }
            doc.Save(path);
        }
    }
}
