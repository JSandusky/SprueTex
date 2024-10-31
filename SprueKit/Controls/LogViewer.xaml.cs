using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for LogViewer.xaml
    /// </summary>
    public partial class LogViewer : UserControl
    {
        public DependencyProperty ScrollLockProperty = DependencyProperty.Register("ScrollLock", typeof(bool), typeof(LogViewer),
            new FrameworkPropertyMetadata(false));

        public DependencyProperty OwningControlProperty = DependencyProperty.Register("OwningControl", typeof(object), typeof(LogViewer),
            new FrameworkPropertyMetadata(null));

        public bool ScrollLock {
            get { return (bool)GetValue(ScrollLockProperty); }
            set { SetValue(ScrollLockProperty, value); }
        }

        /// <summary>
        /// Bindable field for who owns this.
        /// </summary>
        public object OwningControl
        {
            get { return (object)GetValue(OwningControlProperty); }
            set { SetValue(OwningControlProperty, value); }
        }

        public LogViewer()
        {
            InitializeComponent();
            logDataGrid.ItemsSource = ErrorHandler.inst().Items;
            ErrorHandler.inst().Items.CollectionChanged += Items_CollectionChanged;
        }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ObservableCollection<LogItem> col = sender as ObservableCollection<LogItem>;
            if (!ScrollLock)
                logDataGrid.ScrollIntoView(col.LastOrDefault());
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            DoSearch(txtSearch.Text, false);
        }

        private bool DoSearch(string searchText, bool searchNext)
        {
            string lowerSearch = searchText.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(lowerSearch))
                return false;
            if (searchNext && logDataGrid.SelectedItem != null)
            {
                bool takeNext = false;
                foreach (var item in logDataGrid.Items)
                {
                    if (item == logDataGrid.SelectedItem)
                    {
                        takeNext = true;
                    }
                    else if (takeNext)
                    {
                        if (((LogItem)item).Text.ToLowerInvariant().Contains(lowerSearch))
                        {
                            logDataGrid.SelectedItem = item;
                            return true;
                        }
                    }
                }

                if (takeNext && logDataGrid.Items.Count > 0)
                    logDataGrid.SelectedItem = logDataGrid.Items[0];
            }
            else
            {
                foreach (var item in logDataGrid.Items)
                {
                    if (((LogItem)item).Text.ToLowerInvariant().Contains(lowerSearch))
                    {
                        logDataGrid.SelectedItem = item;
                        return true;
                    }
                }

            }
            if (logDataGrid.SelectedItem != null)
                logDataGrid.ScrollIntoView(logDataGrid.SelectedItem);
            return true;
        }

        private void btnNextResult_Click(object sender, RoutedEventArgs e)
        {
            DoSearch(txtSearch.Text, true);
        }

        private void ctxCopyMessage_Click(object sender, RoutedEventArgs e)
        {
            if (logDataGrid.SelectedItems != null)
            {
                StringBuilder sb = new StringBuilder();
                foreach (LogItem item in logDataGrid.SelectedItems)
                    sb.AppendLine(string.Format("{0} : {1}", item.Time, item.Text));

                //ClipboardUtil.SetText(sb.ToString());
                //Clipboard.SetText(sb.ToString());
                ClipboardUtil.SetDataObject(sb.ToString(), false);
            }
        }

        private void Copy(object sender, ExecutedRoutedEventArgs e)
        {
            ctxCopyMessage_Click(sender, null);
            //Clipboard.SetData(DataFormats.Text, string.Join(",", numbers));
        }

        private void CanCopy(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void logDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            //??e.Row.Header = (e.Row.GetIndex()).ToString();
        }
    }
}
