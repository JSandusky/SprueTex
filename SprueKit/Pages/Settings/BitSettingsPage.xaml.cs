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

using FirstFloor.ModernUI;
using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Navigation;

namespace SprueKit.Pages.Settings
{
    /// <summary>
    /// Interaction logic for BitSettingsPage.xaml
    /// </summary>
    public partial class BitSettingsPage : UserControl, IContent
    {
        public BitSettingsPage()
        {
            InitializeComponent();
        }

        public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e)
        {
            string fieldName = "CapabilityNames";
            if (e.Fragment.Equals("capabilities"))
            {
                header.Content = "Capability Names";
            }
            else
            {
                header.Content = "User Flag Names";
                fieldName = "FlagNames";
            }

            textStack.Children.Clear();
            for (int i = 0; i < 32; ++i)
            {
                StackPanel stack = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 5) };
                stack.Children.Add(new Label { Content = string.Format("Bit {0}", i + 1), MinWidth = 80 });
                TextBox text = new TextBox() {  MinWidth = 300, Margin = new Thickness(0, 0, 10, 0) };
                Binding textBinding = new Binding(string.Format("{0}[{1}]", fieldName, i)) { Source = UserData.inst().BitNames };
                text.SetBinding(TextBox.TextProperty, textBinding);
                stack.Children.Add(text);
                stack.Children.Add(new TextBox { Text = string.Format("0x{0:X} / {2} / (1 << {1})", ((uint)1) << i, i, ((uint)1) << i), IsReadOnly=true, Background=new SolidColorBrush(Colors.Transparent), IsTabStop=false });

                textStack.Children.Add(stack);
            }
        }

        public void OnNavigatedFrom(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e)
        {
            
        }

        public void OnNavigatedTo(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e)
        {
            
        }

        public void OnNavigatingFrom(FirstFloor.ModernUI.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            
        }
    }
}
