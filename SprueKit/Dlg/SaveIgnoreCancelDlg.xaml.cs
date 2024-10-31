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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SprueKit.Dlg
{
    /// <summary>
    /// Interaction logic for SaveIgnoreCancelDlg.xaml
    /// </summary>
    public partial class SaveIgnoreCancelDlg : ModernDialog
    {
        MessageBoxResult result_;

        public static MessageBoxResult Show(string msg, string title)
        {
            var dlg = new SaveIgnoreCancelDlg(msg, title);
            dlg.ShowDialog();
            return dlg.result_;
        }

        public SaveIgnoreCancelDlg(string msg, string title)
        {
            InitializeComponent();
            txtMsg.Text = msg;
            Title = title;
            Button[] btns = new Button[] {
                new Button {
                    Content = "_Save",
                    Style = FindResource("StyledButton") as Style,
                },
                new Button {
                    Content = "_Ignore",
                    Style = FindResource("StyledButton") as Style
                },
                new Button
                {
                    Content = "_Cancel",
                    Style = FindResource("StyledButton") as Style
                }
            };

            Buttons = btns;
            Buttons.Last().Style = FindResource("StyledButton") as Style;
            btns[0].Click += onSave;
            btns[1].Click += onIgnore;
            btns[2].Click += onCancel;
        }

        private void onSave(object sender, RoutedEventArgs e)
        {
            result_ = MessageBoxResult.Yes;
            Close();
        }

        private void onIgnore(object sender, RoutedEventArgs e)
        {
            result_ = MessageBoxResult.None;
            Close();
        }

        private void onCancel(object sender, RoutedEventArgs e)
        {
            result_ = MessageBoxResult.Cancel;
            Close();
        }
    }
}
