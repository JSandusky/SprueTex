using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace SprueKit.Controls
{
    public class KeyTextBox : TextBox
    {
        public static readonly DependencyProperty KeyProperty = DependencyProperty.Register(
          "Key",
          typeof(System.Windows.Input.Key),
          typeof(KeyTextBox)
        );

        public System.Windows.Input.Key Key { get { return (Key)GetValue(KeyProperty); } set { SetValue(KeyProperty, value); } }

        public KeyTextBox()
        {
            PreviewKeyDown += KeyTextBox_PreviewKeyDown;

            SetBinding(TextProperty, new Binding("Key") { Source = this, Mode = BindingMode.OneWay });
        }

        private void KeyTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            Key = e.Key;
            e.Handled = true;
        }
    }
}
