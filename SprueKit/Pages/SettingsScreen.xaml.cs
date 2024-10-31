using FirstFloor.ModernUI.Windows;
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
using FirstFloor.ModernUI.Windows.Navigation;

namespace SprueKit.Pages
{
    /// <summary>
    /// Interaction logic for SettingsScreen.xaml
    /// </summary>
    public partial class SettingsScreen : UserControl, IContent
    {
        public SettingsScreen()
        {
            InitializeComponent();
            if (settingsPages.SelectedSource == null)
                settingsPages.SelectedSource = new Uri("/Controls/ReflectivePage.xaml#general", UriKind.Relative);
        }

        public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e)
        {
            if (e.Fragment.Equals("EULA"))
                settingsPages.SelectedSource = new Uri("/Pages/Settings/EULA.xaml", UriKind.Relative);
        }

        public void OnNavigatedFrom(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e)
        {
            
        }

        public void OnNavigatedTo(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e)
        {
            
        }

        public void OnNavigatingFrom(FirstFloor.ModernUI.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (settingsPages.SelectedSource == null)
                settingsPages.SelectedSource = new Uri("/Controls/ReflectivePage.xaml#general", UriKind.Relative);
        }
    }
}
