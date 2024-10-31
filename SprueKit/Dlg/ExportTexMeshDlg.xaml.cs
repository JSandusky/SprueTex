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
    public delegate void ExportTexMeshFunc(string fileName, int meshType, int width, int height, float heightScale, bool decimate, float decimatePower);

    /// <summary>
    /// Interaction logic for ExportTexMeshDlg.xaml
    /// </summary>
    public partial class ExportTexMeshDlg : ModernDialog
    {
        ExportTexMeshFunc func_;
        static int width_ = 32;
        static int height_ = 32;
        static bool decimate_ = false;
        static float heightScale_ = 0.5f;
        static float decimationFactor_ = 0.5f;
        static int meshingIdx_ = 0;

        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
        public float HeightScale { get; set; }
        public bool Decimate { get; set; }
        public float DecimationFactor { get; set; }

        public ExportTexMeshDlg(ExportTexMeshFunc func)
        {
            InitializeComponent();
            func_ = func;

            ImageWidth = width_;
            ImageHeight = height_;
            Decimate = decimate_;
            DecimationFactor = decimationFactor_;
            HeightScale = heightScale_;

            txtWidth.SetBinding(TextBox.TextProperty, new Binding("ImageWidth") { Source = this });
            txtHeight.SetBinding(TextBox.TextProperty, new Binding("ImageHeight") { Source = this });
            txtDecimateTarget.SetBinding(TextBox.TextProperty, new Binding("DecimationFactor") { Source = this });
            chkDecimate.SetBinding(CheckBox.IsCheckedProperty, new Binding("Decimate") { Source = this });
            txtHeightScale.SetBinding(TextBox.TextProperty, new Binding("HeightScale") { Source = this });
            comboType.SelectedIndex = meshingIdx_;

            var exportBtn = new StyledButton { Content = "Export", IsDefault = true };
            exportBtn.Click += ExportBtn_Click;
            Buttons = new Button[]
            {
                exportBtn,
                CancelButton
            };
        }

        public static void Show(ExportTexMeshFunc func)
        {
            var dlg = new ExportTexMeshDlg(func);
            dlg.ShowDialog();
        }

        private void ExportBtn_Click(object sender, RoutedEventArgs e)
        {
            width_ = ImageWidth;
            height_ = ImageHeight;
            decimate_ = Decimate;
            decimationFactor_ = DecimationFactor;
            heightScale_ = HeightScale;
            meshingIdx_ = comboType.SelectedIndex;

            System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();
            dlg.Filter = "OBJ Models (*.obj)|*.obj";
            dlg.AddExtension = true;
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                func_(dlg.FileName, meshingIdx_, Math.Max(1, ImageWidth), Math.Max(1, ImageHeight), HeightScale, Decimate, DecimationFactor);
            
            Close();
        }
    }
}
