using FirstFloor.ModernUI.Windows.Controls;
using FirstFloor.ModernUI.Windows.Navigation;
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
    /// Interaction logic for HelpWindow.xaml
    /// </summary>
    public partial class HelpWindow : ModernWindow, IFrameLocator
    {
        static HelpWindow instance_;

        public HelpWindow()
        {
            InitializeComponent();
            this.MenuLinkGroups.Add(new FirstFloor.ModernUI.Presentation.LinkGroup() { DisplayName = "Manual" });
            this.MenuLinkGroups.Add(new FirstFloor.ModernUI.Presentation.LinkGroup() { DisplayName = "Video Tutorials" });
            TitleLinks.Add(new FirstFloor.ModernUI.Presentation.Link { DisplayName = "Quick Guide", Source = new Uri("../Controls/ImageMap.xaml#SprueKit.QuickGuide.MainScreenHelp.xml", UriKind.Relative) });
            TitleLinks.Add(new FirstFloor.ModernUI.Presentation.Link { DisplayName = "Manual", Source = new Uri("../QuickGuide/QuickGuidePages.xaml", UriKind.Relative) });
            //TitleLinks.Add(new FirstFloor.ModernUI.Presentation.Link { DisplayName = "Video Tutorials", Source = new Uri("http://www.youtube.com", UriKind.Absolute) });

            IsTitleVisible = true;
            // Get rid of our instance when we close
            Closed += (o, evt) => { instance_ = null; };
            Loaded += (o, evt) => { instance_.Focus(); instance_.BringIntoView(); };
        }

        public static void ShowWindow(Uri target = null)
        {
            if (instance_ != null)
            {
                instance_.Focus();
                instance_.BringIntoView();
            }
            else
            {
                instance_ = new HelpWindow();
                instance_.Show();
            }

            // are we opening to specific target? if not then infer our target
            if (target != null)
                instance_.ContentSource = target;
            else if (((MainWindow)App.Current.MainWindow).ContentSource.ToString().Contains("DesignScreen"))
                instance_.ContentSource = new Uri("Controls/ImageMap.xaml#SprueKit.QuickGuide.DesignScreen.xml", UriKind.Relative);
            else if (((MainWindow)App.Current.MainWindow).ContentSource.ToString().Contains("Launch"))
                instance_.ContentSource = new Uri("Controls/ImageMap.xaml#SprueKit.QuickGuide.MainScreenHelp.xml", UriKind.Relative);
            else if (((MainWindow)App.Current.MainWindow).ContentSource.ToString().Contains("SettingsScreen"))
                instance_.ContentSource = new Uri("Controls/ImageMap.xaml#SprueKit.QuickGuide.SettingsScreen.xml", UriKind.Relative);
            else if (((MainWindow)App.Current.MainWindow).ContentSource.ToString().Contains("ReportsScreen"))
                instance_.ContentSource = new Uri("Controls/ImageMap.xaml#SprueKit.QuickGuide.Reports.xml", UriKind.Relative);
            else // default to the manual
                instance_.ContentSource = new Uri("QuickGuide/QuickGuidePages.xaml", UriKind.Relative);
        }

        public ModernFrame TargetFrame()
        {
            var ret = Template.FindName("ContentFrame", this) as ModernFrame;
            return ret;
        }
    }
}
