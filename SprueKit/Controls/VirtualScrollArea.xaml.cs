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
    /// Interaction logic for VirtualScrollArea.xaml
    /// </summary>
    public partial class VirtualScrollArea : UserControl
    {
        public static readonly DependencyProperty ScrollableContentProperty = DependencyProperty.Register(
            "ScrollableContent", 
            typeof(object), 
            typeof(VirtualScrollArea), 
            new UIPropertyMetadata(null, OnScrollableContentChanged));

        private static void OnScrollableContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = d as VirtualScrollArea;
            if (self == null)
                return;
            Timeline t = self.ScrollableContent as Timeline;
            SprueKit.Graphics.Controls.Timeline gT = self.ScrollableContent as SprueKit.Graphics.Controls.Timeline;
            IVirtualControl ctrl = self.ScrollableContent as IVirtualControl;
            ctrl.Area = self;

            if (ctrl != null && t != null)
            {
                t.StartRender += (o, evt) =>
                {
                    var size = ctrl.RequiredArea();
                    try
                    {
                        self.SetHorizontalBarState(size.Width > t.ActualWidth);
                        self.SetVerticalBarState(size.Height > t.ActualHeight);

                        self.horizontalBar.ViewportSize = size.Width;
                        self.verticalBar.ViewportSize = size.Height;
                        self.horizontalBar.Maximum = size.Width - t.ActualWidth;
                        self.verticalBar.Maximum = size.Height - t.ActualHeight;
                        if (self.horizontalBar.Value > self.horizontalBar.Maximum)
                        {
                            self.horizontalBar.Value = self.horizontalBar.Maximum;
                        }
                        if (self.verticalBar.Value > self.verticalBar.Maximum)
                        {
                            self.verticalBar.Value = self.verticalBar.Maximum;
                        }

                    } catch (Exception exception) { }
                };
            }
            else if (ctrl != null && gT != null)
            {
                gT.StartRender += (o, evt) =>
                {
                    var size = ctrl.RequiredArea();
                    try
                    {
                        self.SetHorizontalBarState(size.Width > gT.ActualWidth);
                        self.SetVerticalBarState(size.Height > gT.ActualHeight);

                        self.horizontalBar.ViewportSize = size.Width;
                        self.verticalBar.ViewportSize = size.Height;
                        self.horizontalBar.Maximum = size.Width - gT.ActualWidth;
                        self.verticalBar.Maximum = size.Height - gT.ActualHeight;
                        if (self.horizontalBar.Value > self.horizontalBar.Maximum)
                        {
                            self.horizontalBar.Value = self.horizontalBar.Maximum;
                        }
                        if (self.verticalBar.Value > self.verticalBar.Maximum)
                        {
                            self.verticalBar.Value = self.verticalBar.Maximum;
                        }

                    }
                    catch (Exception exception) { }
                };
            }
        }

        public double SysScrollHeight {
            get { return SystemParameters.ScrollHeight; }
        }

        public double SysScrollWidth {
            get { return SystemParameters.ScrollWidth; }
        }

        public object ScrollableContent
        {
            get { return (object)GetValue(ScrollableContentProperty); }
            set { SetValue(ScrollableContentProperty, value); }
        }

        void SetHorizontalBarState(bool visible)
        {
            if (!visible)
                mainGrid.RowDefinitions[1].Height = new GridLength(0);
            else
                mainGrid.RowDefinitions[1].Height = new GridLength(SysScrollHeight);
        }

        void SetVerticalBarState(bool visible)
        {
            if (!visible)
                mainGrid.ColumnDefinitions[1].Width = new GridLength(0);
            else
                mainGrid.ColumnDefinitions[1].Width = new GridLength(SysScrollWidth);
        }

        public VirtualScrollArea()
        {
            InitializeComponent();
            SetHorizontalBarState(false);
            horizontalBar.Minimum = horizontalBar.Maximum = 0;
            verticalBar.Minimum = verticalBar.Maximum = 0;
            horizontalBar.SmallChange = verticalBar.SmallChange = 1;
            horizontalBar.LargeChange = verticalBar.LargeChange = 10;

            verticalBar.ValueChanged += scrollBar_ValueChanged;
            verticalBar.SizeChanged += (o, evt) =>
            {
                scrollBar_ValueChanged(this, null);
            };
            horizontalBar.ValueChanged += scrollBar_ValueChanged;
        }

        private void scrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var target = ScrollableContent as IVirtualControl;
            if (target != null)
                target.SetScrollOffset(new Size(horizontalBar.Value, verticalBar.Value));
        }

        public void Specify(double x, double y)
        {
            horizontalBar.Value = x;
            horizontalBar.Value = y;
        }

        public void Pan(float xPosition)
        {
            if (horizontalBar.IsVisible && xPosition != 0)
                horizontalBar.Value += xPosition;
        }

        public void ExternalScroll(float yPosition)
        {
            if (verticalBar.IsVisible && yPosition != 0)
                verticalBar.Value += yPosition;
        }

        public void PageUp()
        {
            if (verticalBar.IsVisible)
                verticalBar.Value -= verticalBar.LargeChange;
        }

        public void PageDown()
        {
            if (verticalBar.IsVisible)
                verticalBar.Value += verticalBar.LargeChange;
        }
    }
}
