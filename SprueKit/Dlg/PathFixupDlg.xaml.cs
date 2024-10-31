using FirstFloor.ModernUI.Windows.Controls;
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
using System.Windows.Shapes;

namespace SprueKit.Dlg
{
    /// <summary>
    /// Interaction logic for PathFixupDlg.xaml
    /// </summary>
    public partial class PathFixupDlg : ModernDialog
    {
        Data.SerializationContext context_;

        public PathFixupDlg(Data.SerializationContext context)
        {
            context_ = context;
            InitializeComponent();

            this.pathList.ItemsSource = context_.BrokenPaths;

            Buttons = new Button[]
            {
                new StyledButton
                {
                    Content = "Close",
                    IsCancel = true,
                }
            };

            Buttons.Last().Click += (o, e) => { Close(); };
        }

        private void onFixPath(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            if (b != null)
            {
                Data.SerializationBrokenPath path = b.Tag as Data.SerializationBrokenPath;
                if (path != null)
                {
                    System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
                    dlg.Filter = path.ExtensionMask;
                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK && System.IO.File.Exists(dlg.FileName))
                    {
                        if (path.Fix(dlg.FileName))
                            context_.BrokenPaths.Remove(path);
                        if (context_.BrokenPaths.Count == 0)
                            Close();
                    }
                }
            }
        }

        private void onIgnorePath(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            if (b != null)
            {
                Data.SerializationBrokenPath path = b.Tag as Data.SerializationBrokenPath;
                if (path != null)
                    context_.BrokenPaths.Remove(path);
            }

            if (context_.BrokenPaths.Count == 0)
                Close();
        }
    }
}
