using FirstFloor.ModernUI.Windows.Controls;
using SprueKit.Pages;
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

namespace SprueKit.Controls
{
    /// <summary>
    /// Interaction logic for DocumentList.xaml
    /// </summary>
    public partial class DocumentList : UserControl
    {
        IOCDependency<DocumentManager> documentManager = new IOCDependency<DocumentManager>();

        public DocumentList()
        {
            InitializeComponent();
            documentItems.SetBinding(ItemsControl.ItemsSourceProperty, new Binding("OpenDocuments") { Source = documentManager.Object });
        }

        private void onOpen(object sender, RoutedEventArgs e)
        {
            Button src = sender as Button;
            if (src != null)
            {
                Document doc = src.Tag as Document;
                documentManager.Object.SetActiveDocument(doc);
                ((ModernWindow)Application.Current.MainWindow).ContentSource = new Uri(string.Format("Pages/DesignScreen.xaml", doc.ID), UriKind.Relative);
            }
            else
                ErrorHandler.inst().PublishError("Unable to get button for sender in DocumentList.onOpen", PluginLib.ErrorLevels.DEBUG);
        }

        private void onClose(object sender, RoutedEventArgs e)
        {
            Button src = sender as Button;
            if (src != null)
            {
                Document doc = src.Tag as Document;
                if (doc != null)
                {
                    documentManager.Object.CloseDocument(doc);
                    if (documentManager.Object.OpenDocuments.Count == 0)
                    {
                        new IOCDependency<MainWindow>().Object.ContentSource = new Uri("/Pages/LaunchScreen.xaml", UriKind.Relative);
                    }
                }
            }
        }
    }
}
