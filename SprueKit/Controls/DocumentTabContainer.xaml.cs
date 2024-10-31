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
    /// Interaction logic for DocumentTabContainer.xaml
    /// </summary>
    public partial class DocumentTabContainer : UserControl
    {
        IOCDependency<DocumentManager> documentManager = new IOCDependency<DocumentManager>();

        public DocumentTabContainer()
        {
            InitializeComponent();
            Loaded += DocumentTabContainer_Loaded;

            tabs.SelectionChanged += Tabs_SelectionChanged;
            documentManager.Object.OnDocumentOpened += Object_OnDocumentOpened;
            documentManager.Object.OnActiveDocumentChanged += Object_OnActiveDocumentChanged;
            documentManager.Object.OnDocumentClosed += Object_OnDocumentClosed;
        }

        private void DocumentTabContainer_Loaded(object sender, RoutedEventArgs e)
        {
            //???
        }

        TabItem CreateItemFor(Document doc)
        {
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            TextBlock txt = new TextBlock { Text = "Test", Foreground = new SolidColorBrush(Colors.LightGray) };

            Button close = new Button { Content = "X", Padding = new Thickness(0), Foreground = new SolidColorBrush(Colors.LightGray), FontWeight = FontWeights.Bold, VerticalAlignment = System.Windows.VerticalAlignment.Top, HorizontalAlignment = System.Windows.HorizontalAlignment.Right };
            close.MinHeight = close.MinWidth = 18;
            close.MaxHeight = close.MaxWidth = 18;
            close.Background = close.BorderBrush = null;
            close.Click += Close_Click;

            Grid.SetColumn(txt, 0);
            Grid.SetColumn(close, 1);
            grid.Children.Add(txt);
            grid.Children.Add(close);

            return new TabItem { Header = grid, Tag = doc, Content = doc.Controls.ContentControl };
        }

        private void Tabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                TabItem item = e.AddedItems[0] as TabItem;
                if (item != null)
                    documentManager.Object.SetActiveDocument(item.Tag as Document);
            }
        }

        private void Object_OnDocumentOpened(Document newDoc)
        {
            tabs.Items.Add(CreateItemFor(newDoc));
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            TabItem item = null;
            if (sender is TabItem)
                item = sender as TabItem;
            else
                item = ((Grid)((Button)sender).Parent).Parent as TabItem;

            if (item != null)
            {
                documentManager.Object.CloseDocument(item.Tag as Document);
                //if (item.Content is IDisposable)
                //    ((IDisposable)item.Content).Dispose();
            }

            if (tabs.Items.IsEmpty)
            {

            }

            //if (tabs.SelectedItem != null && ((TabItem)tabs.SelectedItem).Content is IInitializable)
            //    ((IInitializable)((TabItem)tabs.SelectedItem).Content).Initialize();
        }

        private void Object_OnDocumentClosed(Document closing)
        {
            if (closing == null)
                return;

            foreach (TabItem item in tabs.Items)
            {
                if (item.Tag == closing)
                {
                    tabs.Items.Remove(item);
                    return;
                }
            }
        }

        private void Object_OnActiveDocumentChanged(Document newDoc, Document oldDoc)
        {
            foreach (TabItem item in tabs.Items)
            {
                if (item.Tag == newDoc)
                {
                    tabs.SelectedItem = item;
                    return;
                }
            }
        }
    }
}
