using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for PathListControl.xaml
    /// </summary>
    public partial class PathListControl : UserControl
    {
        public bool IsFileMode { get; set; } = false;
        public string FileFilter { get; set; }

        public PathListControl()
        {
            InitializeComponent();
            DataContextChanged += PathListControl_DataContextChanged;
        }

        private void PathListControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            
        }

        private void OnAddDirectory(object sender, RoutedEventArgs e)
        {
            if (!IsFileMode)
            {
                System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
                var result = dlg.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    var list = pathGrid.DataContext as Data.UriList;
                    list.Paths.Add(dlg.SelectedPath);
                }
            }
            else
            {
                System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
                dlg.Filter = FileFilter;
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var list = pathGrid.DataContext as Data.UriList;
                    list.Paths.Add(dlg.FileName);
                }
            }
        }

        private void OnOpenDirectory(object sender, RoutedEventArgs e)
        {
            string path = (sender as Button).Tag as string;
            if (!string.IsNullOrEmpty(path))
            {
                if (IsFileMode)
                {
                    if (System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(path)))
                        Process.Start(System.IO.Path.GetDirectoryName(path));
                }
                else if (System.IO.Directory.Exists(path))
                    Process.Start(path);
            }
        }

        private void OnDeleteDirectory(object sender, RoutedEventArgs e)
        {
            string path = (sender as Button).Tag as string;
            var paths = pathGrid.DataContext as Data.UriList;
            if (paths != null)
                paths.Paths.Remove(path);
        }
    }
}
