using FirstFloor.ModernUI.Presentation;
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
    /// Interaction logic for ReadEulaDlg.xaml
    /// </summary>
    public partial class ReadEulaDlg : ModernDialog
    {
        public ReadEulaDlg()
        {
            InitializeComponent();

            text.LinkNavigator.Commands.Add(new Uri("cmd://showQuickGuide", UriKind.Absolute), new RelayCommand(o => ShowGuide() ));
            text.LinkNavigator.Commands.Add(new Uri("cmd://diveIn", UriKind.Absolute), new RelayCommand(o => DiveIn() ));
            text.LinkNavigator.Commands.Add(new Uri("cmd://readEULA", UriKind.Absolute), new RelayCommand(o => ReadEULA() ));

            Buttons = new Button[]
            {
                CloseButton
            };
        }

        void ShowGuide()
        {
            ((MainWindow)App.Current.MainWindow).showGuide();
            Close();
        }

        void DiveIn()
        {
            ((MainWindow)App.Current.MainWindow).DoDefaultNewDocument();
            Close();
        }

        void ReadEULA()
        {
            ((MainWindow)App.Current.MainWindow).readEULA();
            Close();
        }
    }
}
