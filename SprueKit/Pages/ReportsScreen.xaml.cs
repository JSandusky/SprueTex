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
    /// Interaction logic for ReportsScreen.xaml
    /// </summary>
    public partial class ReportsScreen : UserControl, IContent
    {
        Controls.ReflectivePage reflector_;

        public ReportsScreen()
        {
            InitializeComponent();

            reportsList.SetBinding(ListView.ItemsSourceProperty, new Binding("Reports.Reports") { Source = App.Current });
            reportsList.SelectionChanged += ReportsList_SelectionChanged;

            contentStack.Children.Add(reflector_ = new Controls.ReflectivePage());
        }

        private void ReportsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var sel = reportsList.SelectedItem;
            reflector_.DataContext = sel;
            if (reportsList.SelectedItem == null)
                reportControls.Visibility = Visibility.Collapsed;
            else
                reportControls.Visibility = Visibility.Visible;
        }

        public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e)
        {
            if (e.Fragment != null)
            {
                string fragString = e.Fragment.Replace("_", " ");
                var rpt = ((App)App.Current).Reports.Reports.LastOrDefault(r => r.ReportTitle.Equals(fragString));
                if (rpt != null)
                    reportsList.SelectedItem = rpt;
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

        private void runReport_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();
            dlg.Filter = "HTML Report (*.html)|*.html";
            dlg.FilterIndex = 1;

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            { 
                var rpt = new Data.Reports.TextureGraphReport(((TextBlock)reportsList.SelectedItem).Tag as Data.Reports.TextureReportSettings, dlg.FileName);
                System.Diagnostics.Process.Start(dlg.FileName);
            }
        }

        private void deleteReport_Click(object sender, RoutedEventArgs e)
        {
            if (reportsList.SelectedItem != null)
                ((App)App.Current).Reports.Reports.Remove(reportsList.SelectedItem as Data.Reports.TextureReportSettings);
        }

        private void newReport_Click(object sender, RoutedEventArgs e)
        {
            ((App)App.Current).Reports.Reports.Add(new Data.Reports.TextureReportSettings());
        }
    }
}
