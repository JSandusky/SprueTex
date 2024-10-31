using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
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
    /// Interaction logic for FileBrowser.xaml
    /// </summary>
    public partial class FileBrowser : UserControl
    {
        public CollectionView FilesViewSource { get; set; }
        public ObservableCollection<Data.FolderData> AssetFolders { get; set; } = new ObservableCollection<Data.FolderData>();

        public FileBrowser()
        {
            InitializeComponent();

            var generalSettings = new IOCDependency<Settings.GeneralSettings>().Object;
            generalSettings.AssetFolders.Paths.CollectionChanged += Paths_CollectionChanged;

            DataContextChanged += FileBrowser_DataContextChanged;

            DataContext = this;// UserData.inst();
            folderTree.Items.Clear();
            folderTree.SelectionChanged += FolderList_SelectionChanged;
            //folderTree.SelectedItemChanged += FolderTree_SelectedItemChanged;
            //fileList.PreviewMouseDown += FileList_PreviewMouseDown;
            //fileList.MouseMove += FileList_MouseMove;
            BuildFolderList();
        }

        private void Paths_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            BuildFolderList();
        }

        void BuildFolderList()
        {
            var generalSettings = new IOCDependency<Settings.GeneralSettings>().Object;
            this.AssetFolders.Clear();

            string[] baseAssetPaths =
            {
                App.ProgramPath("StockAssets")
            };

            foreach (var str in baseAssetPaths)
            {
                if (System.IO.Directory.Exists(str))
                    AssetFolders.Add(new Data.FolderData(new System.IO.DirectoryInfo(str)));
            }

            foreach (var item in generalSettings.AssetFolders.Paths)
            {
                if (System.IO.Directory.Exists(item))
                    AssetFolders.Add(new Data.FolderData(new System.IO.DirectoryInfo(item)));
            }
        }

        System.Windows.Point startPoint;
        private void FileList_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(null);
        }

        private void FileList_MouseMove(object sender, MouseEventArgs e)
        {
            System.Windows.Point mousePos = e.GetPosition(null);
            Vector diff = startPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed && 
                Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance && Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                // Get the dragged ListViewItem
                ListView listView = sender as ListView;
                ListViewItem listViewItem = FindAncestor<ListViewItem>((DependencyObject)e.OriginalSource);

                // Find the data behind the ListViewItem
                Data.FileData fileData = (Data.FileData)listView.ItemContainerGenerator.ItemFromContainer(listViewItem);

                // Initialize the drag & drop operation
                DataObject dragData = new DataObject("SprueKit.Data.FileData", fileData);
                DragDrop.DoDragDrop(listViewItem, dragData, DragDropEffects.Link);
            }
        }

        private void SetItemAsCursor(ListViewItem item)
        {
            
        }

        private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            do
            {
                if (current is T)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        }

        private void FolderTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            fileList.DataContext = folderTree.SelectedItem;
        }

        private void FolderList_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            fileList.DataContext = folderTree.SelectedItem;
            if (fileList.ItemsSource != null)
            {
                var view = CollectionViewSource.GetDefaultView(fileList.ItemsSource);
                view.Filter = FilterFunction;
            }
            //if (folderTree.SelectedItem != null)
            //{
            //    FilesViewSource = new CollectionView(((Data.FolderData)folderTree.SelectedItem).Files);
            //    FilesViewSource.Filter = FilterFunction;
            //    fileList.DataContext = FilesViewSource;
            //}
            //else
            //    fileList.DataContext = null;
        }

        private void FileBrowser_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //??folderTree.DataContext = UserData.inst();
        }

        private void resetFilter_Click(object sender, RoutedEventArgs e)
        {
            filterTextBox.Text = "";
        }

        System.Windows.Media.Brush normalBorderBrush;
        System.Windows.Media.Brush hasFilterBorderBrush;
        private void filterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (normalBorderBrush == null)
            {
                normalBorderBrush = filterTextBox.Background;
                hasFilterBorderBrush = new SolidColorBrush(new System.Windows.Media.Color() { R = 110, G = 30, B = 0, A = 255 });
            }

            filterTextBox.Background = string.IsNullOrWhiteSpace(filterTextBox.Text) ? normalBorderBrush : hasFilterBorderBrush;

            if (fileList.ItemsSource != null)
                CollectionViewSource.GetDefaultView(fileList.ItemsSource).Refresh();
        }

        private bool FilterFunction(object o)
        {
            var file = o as Data.FileData;
            if (file != null)
            {
                if (string.IsNullOrWhiteSpace(filterTextBox.Text))
                    return true;

                char[] splitKeys = new char[] { ' ' };
                string[] terms = filterTextBox.Text.ToLowerInvariant().Split(splitKeys, StringSplitOptions.RemoveEmptyEntries);
                bool anyHits = false;
                string loCasePath = file.FilePath.ToLowerInvariant();
                foreach (var term in terms)
                {
                    anyHits |= loCasePath.Contains(term);
                    anyHits |= file.ContainsMetaFieldValue(term);
                }
                return anyHits;
            }
            return true;
        }
    }
}
