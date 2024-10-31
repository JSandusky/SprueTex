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
    public delegate System.Drawing.Bitmap ExportImageBitmapFunc(int width, int height);

    /// <summary>
    /// Interaction logic for ExportImageDlg.xaml
    /// </summary>
    public partial class ExportImageDlg : ModernDialog
    {
        ExportImageBitmapFunc func_;
        static int width_ = 256;
        static int height_ = 256;

        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }

        public ExportImageDlg(ExportImageBitmapFunc func)
        {
            InitializeComponent();
            func_ = func;

            ImageWidth = width_;
            ImageHeight = height_;
            txtWidth.SetBinding(TextBox.TextProperty, new Binding("ImageWidth") { Source = this });
            txtHeight.SetBinding(TextBox.TextProperty, new Binding("ImageHeight") { Source = this });

            var exportBtn = new StyledButton { Content = "Export", IsDefault=true };
            exportBtn.Click += ExportBtn_Click;
            Buttons = new Button[]
            {
                exportBtn,
                CancelButton
            };
        }

        public static void Show(string title, ExportImageBitmapFunc func)
        {
            var dlg = new ExportImageDlg(func);
            dlg.Title = title;
            dlg.ShowDialog();
        }

        public static void Show(ExportImageBitmapFunc func)
        {
            var dlg = new ExportImageDlg(func);
            dlg.ShowDialog();
        }

        private void ExportBtn_Click(object sender, RoutedEventArgs e)
        {
            height_ = ImageHeight;
            width_ = ImageWidth;
            System.Drawing.Bitmap bmp = func_(Math.Max(1, ImageWidth), Math.Max(1, ImageHeight));
            if (bmp != null)
            {
                System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();
                dlg.Filter = "PNG Images (*.png)|*.png";
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    bmp.Save(dlg.FileName, System.Drawing.Imaging.ImageFormat.Png);
                bmp.Dispose();
            }
            Close();
        }
    }
}
