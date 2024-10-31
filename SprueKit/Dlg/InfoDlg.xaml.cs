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
    /// Interaction logic for InfoDlg.xaml
    /// </summary>
    public partial class InfoDlg : ModernDialog
    {
        public InfoDlg(string msg, string title)
        {
            InitializeComponent();

            PreviewKeyUp += InfoDlg_PreviewKeyUp;

            Title = title;
            bbCode.BBCode = msg;

            Buttons = new Button[]
            {
                OkButton
            };
        }

        private void InfoDlg_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }

        public static void Show(string msg, string title)
        {
            var dlg = new InfoDlg(msg, title);
            dlg.ShowDialog();
        }
    }
}
