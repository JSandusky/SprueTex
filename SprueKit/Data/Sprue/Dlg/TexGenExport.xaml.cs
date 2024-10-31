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
using FirstFloor.ModernUI.Windows.Controls;
using System.Windows.Forms;

namespace SprueKit.Dlg.Sprue
{
    /// <summary>
    /// Interaction logic for TexGenExport.xaml
    /// </summary>
    public partial class TexGenExport : ModernDialog
    {
        public TexGenExport()
        {
            InitializeComponent();

            Buttons = new List<System.Windows.Controls.Button>
            {
                new System.Windows.Controls.Button
                {
                    Content = "Export",
                    IsDefault = true,
                    Style = FindResource("StyledButton") as Style,
                },
                new System.Windows.Controls.Button
                {
                    Content = "Close",
                    IsCancel = true,
                    Style = FindResource("StyledButton") as Style,
                }
            };

            Buttons.First().Click += ExportDlg_Click;
            Buttons.Last().Click += (o, e) => { Close(); };
        }

        private void ExportDlg_Click(object sender, RoutedEventArgs e)
        {
            var docMan = new IOCDependency<DocumentManager>().Object;
            if (docMan != null && docMan.ActiveDocument != null && docMan.ActiveDocument is Data.TexGen.TextureGenDocument)
            {
                System.Windows.Forms.SaveFileDialog dlg = new SaveFileDialog();
                switch (exporterCombo.SelectedIndex)
                {
                    case 0: // png
                        break;
                    case 1: // tga
                        break;
                    case 2: // jpg
                        break;
                    case 3: // dds
                        break;
                    case 4: // hdr
                        break;
                }
            }
        }
    }
}
