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

using FirstFloor.ModernUI;
using FirstFloor.ModernUI.Windows.Controls;

namespace SprueKit.Pages
{
    /// <summary>
    /// Interaction logic for LaunchScreen.xaml
    /// </summary>
    public partial class LaunchScreen : UserControl
    {
        public LaunchScreen()
        {
            InitializeComponent();
            Loaded += LaunchScreen_Loaded;

            // Load recent files, only show 5 and only 5 that exist
            int newFileCt = 0;
            for (int i = 0; newFileCt < 5 && i < UserData.inst().RecentFiles.Paths.Count; ++i)
            {
                string path = UserData.inst().RecentFiles.Paths[UserData.inst().RecentFiles.Paths.Count - i - 1];
                if (System.IO.File.Exists(path))
                {
                    Button tb = new Button { Content = System.IO.Path.GetFileName(path), Tag = path };
                    tb.Style = this.FindResource("StyledButton") as Style;
                    recentFiles.Children.Add(tb);
                    tb.Click += tb_Click;
                    newFileCt++;
                }
            }
            infoStack.DataContext = ((App)Application.Current).Messages;

            var docTypes = ((App)App.Current).DocumentTypes;
            newDocsItems.ItemsSource = docTypes;

            foreach (var doc in docTypes)
            {
                var reports = doc.GetSignificantReports();
                if (reports != null)
                {
                    foreach (var kvp in reports)
                    {
                        Button btn = new Button { Content = kvp.Key, Margin = new Thickness(2), Tag = new KeyValuePair<DocumentRecord, string>(doc, kvp.Value) };
                        btn.Click += RptBtn_Click;
                        reportStack.Children.Add(btn);
                    }
                }
            }
        }

        private void RptBtn_Click(object sender, RoutedEventArgs e)
        {
            var rpt = (KeyValuePair<DocumentRecord, string>)((Button)sender).Tag;
            rpt.Key.DoReport(rpt.Value);
        }

        private void LaunchScreen_Loaded(object sender, RoutedEventArgs e)
        {
            if (UserData.inst().IsFirstRun)
            {
                UserData.inst().IsFirstRun = false;
                Dlg.ReadEulaDlg dlg = new Dlg.ReadEulaDlg();
                dlg.ShowDialog();
            }
        }

        void tb_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            OpenString(btn.Tag.ToString());
        }

        private void btnOpenProject_Click(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }

        public static void OpenString(string filePath)
        {
            if (!System.IO.Path.IsPathRooted(filePath))
            {
                filePath = System.IO.Path.Combine(Environment.CurrentDirectory, filePath);
                if (!System.IO.File.Exists(filePath))
                    return;
            }

            if (System.IO.File.Exists(filePath))
            {
                if (System.IO.Path.GetExtension(filePath).Equals(".xml") || System.IO.Path.GetExtension(filePath).Equals(".sprm"))
                {
                    new IOCDependency<DocumentManager>().Object.AddDocument(new Data.SprueModelDocument(new Uri(filePath)));
                    ((ModernWindow)Application.Current.MainWindow).ContentSource = new Uri("Pages/DesignScreen.xaml", UriKind.Relative);
                    UserData.inst().AddRecentFile(new Uri(filePath));
                }
                else if (System.IO.Path.GetExtension(filePath).Equals(".txml") || System.IO.Path.GetExtension(filePath).Equals(".texg"))
                {
                    new IOCDependency<DocumentManager>().Object.AddDocument(new Data.TexGen.TextureGenDocument(new Uri(filePath, UriKind.RelativeOrAbsolute), false));
                    ((ModernWindow)Application.Current.MainWindow).ContentSource = new Uri("Pages/DesignScreen.xaml", UriKind.Relative);
                    UserData.inst().AddRecentFile(new Uri(filePath));
                }
            }
        }

        public static void OpenFile()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = "xml";
            var docTypeList = ((App)App.Current).DocumentTypes;
            StringBuilder nameStr = new StringBuilder();
            StringBuilder filtStr = new StringBuilder();
            for (int i = 0; i < docTypeList.Count; ++i)
            {
                if (i > 0)
                {
                    filtStr.Append("|");
                    nameStr.Append(", ");
                }
                filtStr.Append(docTypeList[i].OpenFileMask);
                if (i == docTypeList.Count - 1 && docTypeList.Count > 0)
                    nameStr.Append("and ");
                nameStr.Append(docTypeList[i].DocumentName);
            }
            dlg.Title = string.Format("Open {0}", nameStr.ToString());
            dlg.Filter = filtStr.ToString();

            if (dlg.ShowDialog() == true)
            {
                if (System.IO.File.Exists(dlg.FileName))
                {
                    int docIndex = dlg.FilterIndex - 1;
                    if (docIndex >= 0 && docIndex < docTypeList.Count)
                    {
                        var newDoc = docTypeList[docIndex].OpenDocument(new Uri(dlg.FileName));
                        if (newDoc != null)
                        {
                            new IOCDependency<DocumentManager>().Object.AddDocument(newDoc);
                            ((ModernWindow)Application.Current.MainWindow).ContentSource = new Uri("Pages/DesignScreen.xaml", UriKind.Relative);
                            UserData.inst().AddRecentFile(new Uri(dlg.FileName));
                        }
                        else
                            throw new Exception(string.Format("Failed to open file: {0}", dlg.FileName));
                    }
                }
                else
                    throw new Exception(string.Format("File does not exist to open: {0}", dlg.FileName));
            }
        }

        private void btnNewSculpt_Click(object sender, RoutedEventArgs e)
        {
            ((ModernWindow)Application.Current.MainWindow).ContentSource = new Uri("Pages/DesignScreen.xaml", UriKind.Relative);
            new IOCDependency<DocumentManager>().Object.AddDocument(new Data.Sculpt.SculptDocument());
        }

        private void btnNewTextureGraph_Click(object sender, RoutedEventArgs e)
        {
            ((ModernWindow)Application.Current.MainWindow).ContentSource = new Uri("Pages/DesignScreen.xaml", UriKind.Relative);
            new IOCDependency<DocumentManager>().Object.AddDocument(new Data.TexGen.TextureGenDocument());
        }

        private void btnNewAnimation_Click(object sender, RoutedEventArgs e)
        {
            ((ModernWindow)Application.Current.MainWindow).ContentSource = new Uri("Pages/DesignScreen.xaml", UriKind.Relative);
            new IOCDependency<DocumentManager>().Object.AddDocument(new Document());
        }

        private void btnNewSprueModel_Click(object sender, RoutedEventArgs e)
        {
            ((ModernWindow)Application.Current.MainWindow).ContentSource = new Uri("Pages/DesignScreen.xaml", UriKind.Relative);
            new IOCDependency<DocumentManager>().Object.AddDocument(new Data.SprueModelDocument());
        }

        private void btnNewShader_Click(object sender, RoutedEventArgs e)
        {
            ((ModernWindow)Application.Current.MainWindow).ContentSource = new Uri("Pages/DesignScreen.xaml", UriKind.Relative);
            new IOCDependency<DocumentManager>().Object.AddDocument(new Data.ShaderGen.ShaderGenDocument());
        }

        private void btnCreateNewDoc_Click(object sender, RoutedEventArgs e)
        {
            var rec = ((Button)sender).Tag as DocumentRecord;
            var newDoc = rec.CreateNewDocument();
            if (newDoc != null)
            {
                ((ModernWindow)Application.Current.MainWindow).ContentSource = new Uri("Pages/DesignScreen.xaml", UriKind.Relative);
                new IOCDependency<DocumentManager>().Object.AddDocument(newDoc);
            }
        }

        public class BullshitMenuhack : MenuItem
        {
            public void SetHighlight(bool state) { IsHighlighted = state; }
        }

        private void FillStackMenu(PopupHelper src, DocumentRecord docRecord, MenuItem parent, StackPanel stack, string curPath)
        {
            foreach (var str in System.IO.Directory.EnumerateDirectories(curPath))
            {
                BullshitMenuhack thisMenu = new BullshitMenuhack { Header = System.IO.Path.GetFileName(str) };
                if (parent != null)
                    parent.Items.Add(thisMenu);
                else
                    stack.Children.Add(thisMenu);
                thisMenu.Focusable = false;
                thisMenu.MouseEnter += (oo, ee) => { thisMenu.IsSubmenuOpen = true; };
                thisMenu.MouseLeave += (ooo, eee) => { thisMenu.IsSubmenuOpen = false; thisMenu.SetHighlight(false); };
                FillStackMenu(src, docRecord, thisMenu, stack, str);
            }

            foreach (var str in System.IO.Directory.EnumerateFiles(curPath))
            {
                if (str.EndsWith(".txml") || str.EndsWith(".texg"))
                {
                    MenuItem thisTemplate = new MenuItem { Header = System.IO.Path.GetFileName(str) };
                    thisTemplate.Click += (o, evt) => {
                        src.Hide();
                        var newDoc = docRecord.CreateFromTemplate(new Uri(str));
                        if (newDoc != null)
                        {
                            ((ModernWindow)Application.Current.MainWindow).ContentSource = new Uri("Pages/DesignScreen.xaml", UriKind.Relative);
                            new IOCDependency<DocumentManager>().Object.AddDocument(newDoc);
                        }
                    };
                    if (parent != null)
                        parent.Items.Add(thisTemplate);
                    else
                        stack.Children.Add(thisTemplate);
                }
            }
        }

        private void btnCreateNewTemplate_Click(object sender, RoutedEventArgs e)
        {
            var rec = ((Button)sender).Tag as DocumentRecord;
            var templateDir = App.ProgramPath("Templates");
            if (System.IO.Directory.Exists(templateDir))
            {
                var popup = PopupHelper.Create();
                StackPanel sp = new StackPanel { Orientation = Orientation.Vertical };
                popup.Grid.Children.Add(sp);
                FillStackMenu(popup, rec, null, sp, templateDir);
                popup.ShowAtMouse();
            }
            else
                Dlg.ErrorDlg.ShowMessage(string.Format("Template folder missing from {0} directory.", App.AppName), "Error", MessageBoxButton.OK);
        }

        private void removeNewsItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            (((Label)sender).Tag as InfoMessage).Rejected = true;
        }

        private void menuQuickStart_Click(object sender, RoutedEventArgs e)
        {
            Dlg.HelpWindow.ShowWindow();
        }

        private void menuManual_Click(object sender, RoutedEventArgs e)
        {
            Dlg.HelpWindow.ShowWindow(new Uri("QuickGuide/QuickGuidePages.xaml", UriKind.Relative));
        }

        private void menuVideos_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.youtube.com");
        }
    }
}
