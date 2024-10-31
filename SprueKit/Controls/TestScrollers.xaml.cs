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

namespace SprueKit.Controls
{
    /// <summary>
    /// Interaction logic for TestScrollers.xaml
    /// </summary>
    public partial class TestScrollers : UserControl
    {
        public TestScrollers()
        {
            InitializeComponent();
            rightScroll.ScrollChanged += RightScroll_ScrollChanged;
        }

        private void RightScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            leftScroll.ScrollToVerticalOffset(rightScroll.VerticalOffset);
        }
    }
}
