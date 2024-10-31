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
    /// Contains a form that is populated via reflection.
    /// </summary>
    public partial class ReflectiveDlg : ModernDialog
    {
        public bool enterAccepts = false;
        public ReflectiveDlg(object dataSource, string title, string okayButtonText, string cancelButtonText = null)
        {
            InitializeComponent();
            Title = title;
            Controls.ReflectivePage.FillLayout(stackContent, dataSource);

            Button[] btns = null;
            this.Activated += ReflectiveDlg_Activated;

            if (cancelButtonText == null)
            {
                btns = new Button[] {
                    new Button {
                        Content = okayButtonText,
                        Style = FindResource("StyledButton") as Style,
                    }
                };
                btns[0].Click += (o, evt) =>
                {
                    DialogResult = true;
                    Close();
                };
                Buttons = btns;
            }
            else
            {
                btns = new Button[] {
                    new Button {
                        Content = okayButtonText,
                        Style = FindResource("StyledButton") as Style,
                    },
                    new Button {
                        Content = cancelButtonText,
                        Style = FindResource("StyledButton") as Style
                    }
                };
                btns[0].Click += (o, evt) =>
                {
                    DialogResult = true;
                    Close();
                };
                btns[1].Click += (o, evt) =>
                {
                    DialogResult = false;
                    Close();
                };
                Buttons = btns;
            }
        }

        private void ReflectiveDlg_Activated(object sender, EventArgs e)
        {
            if (stackContent.Children.Count == 0)
                return;
            var panel = ((StackPanel)stackContent.Children[0]);
            if (panel == null)
                return;
            TextBox subChild = panel.Children[1] as TextBox;
            if (subChild != null)
            {
                subChild.Focus();
                subChild.SelectAll();
            }
        }

        protected override void OnPreviewKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.Return && enterAccepts)
            {
                DialogResult = true;
                Close();
            }
        }
    }
}
