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
    /// Interaction logic for ConfirmDlg.xaml
    /// </summary>
    public partial class ConfirmDlg : ModernDialog
    {
        public static bool Show(string msg, string confBtn = "Confirm", string title = "Confirm")
        {
            ConfirmDlg dlg = new ConfirmDlg(msg, confBtn, title);
            if (dlg.ShowDialog() == true)
                return true;
            return false;
        }

        public ConfirmDlg(string msg, string confBtnText = "Confirm", string title = "Confirm")
        {
            InitializeComponent();
            txtMsg.Text = msg;
            Title = title;
            Buttons = new Button[] {
                new Button {
                    Content = confBtnText,
                    Style = FindResource("StyledButton") as Style
                },
                CancelButton
            };
            Buttons.Last().Style = FindResource("StyledButton") as Style;
            ((Button)Buttons.First()).Click += onOK;
        }

        void onOK(object sender, EventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
