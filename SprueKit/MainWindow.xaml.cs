using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Timers;

using FirstFloor.ModernUI;
using FirstFloor.ModernUI.Presentation;
using FirstFloor.ModernUI.Windows.Controls;
using FirstFloor.ModernUI.Windows;
using System.Reflection;
using System.Windows.Controls.Primitives;
using FirstFloor.ModernUI.Windows.Navigation;
using System.Windows.Media.Effects;

namespace SprueKit
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ModernWindow, IFrameLocator
    {
        Timer errTimer;
        IOCDependency<DocumentManager> documentManager = new IOCDependency<DocumentManager>();

        public MainWindow()
        {
            // Broadcast ourself to everyone else
            IOCLite.Register(this);

            documentManager.Object.OnActiveDocumentChanged += Object_OnActiveDocumentChanged;
            documentManager.Object.OnDocumentClosed += Object_OnDocumentClosed;
            documentManager.Object.OnDocumentOpened += Object_OnDocumentOpened;

            TitleLinks.Add(new Link { DisplayName = "Reports", Source = new Uri("/Pages/ReportsScreen.xaml", UriKind.Relative) });
            TitleLinks.Add(new Link { DisplayName = "Guide", Source = new Uri("cmd://showGuide") });

                  this.ContentLoader = new ContentLoader();
            FirstFloor.ModernUI.Presentation.AppearanceManager.Current.ThemeSource = FirstFloor.ModernUI.Presentation.AppearanceManager.DarkThemeSource;

            TextBlock tb = new TextBlock();
            tb.Opacity = 0.025;
            tb.Text = App.DisplayAppName + " v" + Assembly.GetAssembly(typeof(MainWindow)).GetName().Version.ToString();//.Major + "." + Assembly.GetAssembly(typeof(MainWindow)).GetName().Version.Minor + "." + Assembly.GetAssembly(typeof(MainWindow)).GetName().Version.MinorRevision;
            tb.FontSize = 48;
            tb.FontWeight = FontWeights.Bold;
            tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            tb.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            BackgroundContent = tb;

            InitializeComponent();
            Title = App.DisplayAppName;

            LinkNavigator.Commands.Add(new Uri("cmd://showPlugins", UriKind.Absolute), new RelayCommand(o => showPlugins()));
            LinkNavigator.Commands.Add(new Uri("cmd://showGuide", UriKind.Absolute), new RelayCommand(o => showGuide()));
            LinkNavigator.Commands.Add(new Uri("cmd://update", UriKind.Absolute), new RelayCommand(o => doUpdate()));
            LinkNavigator.Commands.Add(new Uri("cmd://openFile", UriKind.Absolute), new RelayCommand(o => doDocumentsList()));

            ContentSource = new Uri("Pages/LaunchScreen.xaml", UriKind.Relative);

            Closing += MainWindow_Closing;
            Loaded += MainWindow_Loaded;
            PreviewKeyDown += MainWindow_PreviewKeyDown;
            PreviewKeyUp += MainWindow_PreviewKeyUp;
        }

        public void ShowDialog(string fileName)
        {
            if (!string.IsNullOrWhiteSpace(fileName))
                Loaded += (o, evt) => { Pages.LaunchScreen.OpenString(fileName); };
            base.ShowDialog();
        }

        private void MainWindow_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            var focused = FocusManager.GetFocusedElement(this);
            if (focused is TextBox)
                return;
            if (e.Key == Key.Space && Keyboard.Modifiers == ModifierKeys.Control)
            {
                Dlg.QuickAction qa = new Dlg.QuickAction();
                qa.ShowDialog();
                e.Handled = true;
            }
            else
            {
                if (App.BlockShortCuts)
                    return;
                object currentSelection = documentManager.Object.ActiveDocument != null ? documentManager.Object.ActiveDocument.Selection.MostRecentlySelected : null;

                var query = from grp in App.ShortCuts
                            where grp.AppliesToType == null || (currentSelection != null && grp.AppliesToType.IsAssignableFrom(currentSelection.GetType()))
                            from sc in grp.ShortCuts
                            where sc.Key == e.Key && sc.Modifiers == Keyboard.Modifiers
                            select sc;

                if (query != null)
                {
                    foreach (var sc in query)
                    {
                        if (sc.Command != null && sc.Command.Action != null)
                            sc.Command.Action(documentManager.Object.ActiveDocument, currentSelection);
                    }
                }
            }
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var focused = FocusManager.GetFocusedElement(this);
            if (focused is TextBox)
                return;
            if (e.Key == Key.Space && Keyboard.Modifiers == ModifierKeys.Control)
                e.Handled = true;
            else
            {
                object currentSelection = documentManager.Object.ActiveDocument != null ? documentManager.Object.ActiveDocument.Selection.MostRecentlySelected : null;
                
                var query = from grp in App.ShortCuts
                            where grp.AppliesToType == null || (currentSelection != null && grp.AppliesToType.IsAssignableFrom(currentSelection.GetType()))
                            from sc in grp.ShortCuts
                            where sc.Key == e.Key && sc.Modifiers == Keyboard.Modifiers
                            select sc;

                if (query != null && query.Count() > 0)
                    e.Handled = true;
            }
        }

        void UpdateDocumentText()
        {
            if (documentManager.Object.OpenDocuments.Count == 0)
            {
                Link docLink = null;
                foreach (var link in TitleLinks)
                {
                    if (link.DisplayName.StartsWith("Documents"))
                    {
                        docLink = link;
                        break;
                    }
                }
                TitleLinks.Remove(docLink);
            }
            else
            {
                foreach (var link in TitleLinks)
                {
                    if (link.DisplayName.StartsWith("Documents"))
                    {
                        link.DisplayName = string.Format("Documents ({0})", documentManager.Object.OpenDocuments.Count);
                        return;
                    }
                }

                TitleLinks.Insert(0, new Link { DisplayName = "Documents (1)", Source = new Uri("cmd://openFile", UriKind.Absolute) });
                var links = TitleLinks;
                TitleLinks = null;
                TitleLinks = links;
            }
        }

        private void Object_OnDocumentOpened(Document newDoc)
        {
            UpdateDocumentText();
        }

        private void Object_OnDocumentClosed(Document closing)
        {
            UpdateDocumentText();
        }

        private void Object_OnActiveDocumentChanged(Document newDoc, Document oldDoc)
        {
            UpdateDocumentText();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            TitleText.MouseDown += (o, ee) => { ContentSource = new Uri("/Pages/LaunchScreen.xaml", UriKind.Relative); };
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!(new IOCDependency<DocumentManager>().Object.CloseAllFiles()))
                e.Cancel = true;
        }

        public void showPlugins()
        {
            Blur();
            Dlg.PluginDlg dlg = new Dlg.PluginDlg();
            dlg.ShowDialog();
            UnBlur();
        }

        public void showGuide()
        {
            Dlg.HelpWindow.ShowWindow();
        }

        public void DoDefaultNewDocument()
        {
            var newDoc = ((App)App.Current).DocumentTypes[0].CreateNewDocument();
            if (newDoc != null)
            {
                ((ModernWindow)Application.Current.MainWindow).ContentSource = new Uri("Pages/DesignScreen.xaml", UriKind.Relative);
                new IOCDependency<DocumentManager>().Object.AddDocument(newDoc);
            }
        }

        public void readEULA()
        {
            ContentSource = new Uri("/Pages/SettingsScreen.xaml#EULA", UriKind.Relative);
        }

        public void doUpdate()
        {

        }

        public void doDocumentsList()
        {
            var pop = PopupHelper.Create();
            pop.Grid.Children.Add(new Controls.DocumentList());
            pop.ShowAtMouse();
        }

        public ModernFrame TargetFrame()
        {
            var ret = Template.FindName("ContentFrame", this) as ModernFrame;
            return ret;
        }

        public static void Blur()
        {
            var mainWindow = new IOCDependency<MainWindow>().Object;

            BlurEffect effect = new BlurEffect();
            effect.Radius = 10;
            effect.KernelType = KernelType.Gaussian;
            mainWindow.Effect = effect;
        }

        public static void UnBlur()
        {
            var mainWindow = new IOCDependency<MainWindow>().Object;
            mainWindow.Effect = null;
        }

        public void BringToForeground()
        {
            if (WindowState == WindowState.Minimized || Visibility == Visibility.Hidden)
            {
                Show();
                WindowState = WindowState.Normal;
            }
            Activate();
            Topmost = true;
            Topmost = false;
            Focus();
        }
    }
}
