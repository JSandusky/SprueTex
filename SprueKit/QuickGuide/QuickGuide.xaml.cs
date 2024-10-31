using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
using System.Xml;

namespace SprueKit.QuickGuide
{
    /// <summary>
    /// Interaction logic for QuickGuide.xaml
    /// </summary>
    public partial class QuickGuide : UserControl, IContent
    {
        Dictionary<string, FrameworkElement> scrollTargets = new Dictionary<string, FrameworkElement>();

        string LastFragment { get; set; }

        public QuickGuide()
        {
            InitializeComponent();
        }

        public static void FillStackPanel(StackPanel target, XmlElement source, Dictionary<string, FrameworkElement> scrollTargets = null, ScrollViewer scroller = null)
        {
            foreach (XmlElement text in source.ChildNodes)
            {
                if (text.Name.Equals("img"))
                {
                    try
                    {
                        Image img = new Image();
                        BitmapImage bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.UriSource = new Uri(string.Format("pack://application:,,,/{1};component/{0}", text.InnerText, App.AppName), UriKind.Absolute);
                        bmp.EndInit();
                        img.Source = bmp;
                        img.Stretch = Stretch.Uniform;
                        img.Width = bmp.PixelWidth / 2;
                        img.Height = bmp.PixelHeight / 2;
                        img.MaxWidth = bmp.PixelWidth / 2;
                        img.MaxHeight = bmp.PixelHeight / 2;
                        if (text.HasAttribute("align") && text.GetAttribute("align").Equals("left"))
                            img.HorizontalAlignment = HorizontalAlignment.Left;

                        target.Children.Add(img);
                    }
                    catch (Exception ex) {
                        ErrorHandler.inst().Error(ex);
                    }
                }
                else if (text.Name.Equals("raw_image"))
                {
                    try
                    {
                        double scale = 1.0;
                        if (text.HasAttribute("scale"))
                            scale = double.Parse(text.GetAttribute("scale"));

                        var bmp = WPFExt.BitmapFromBase64(text.InnerText);
                        Image img = new Image();
                        img.Source = bmp;
                        img.Stretch = Stretch.Uniform;
                        img.Width =     (bmp.PixelWidth);
                        img.Height =    (bmp.PixelHeight);
                        img.MaxWidth =  (bmp.PixelWidth) ;
                        img.MaxHeight = (bmp.PixelHeight);
                        if (text.HasAttribute("align") && text.GetAttribute("align").Equals("left"))
                            img.HorizontalAlignment = HorizontalAlignment.Left;

                        target.Children.Add(img);
                    }
                    catch (Exception) { }
                }
                else if (text.Name.Equals("seperator"))
                {
                    target.Children.Add(new Separator());
                }
                else if (text.Name.Equals("toc"))
                {
                    WrapPanel wrap = new WrapPanel() { Orientation = Orientation.Horizontal, MaxWidth = 600, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(20,20,20,20) };
                    foreach (XmlElement elem in text.ChildNodes)
                    {
                        Button btn = new Button { Content = elem.InnerText, Tag = elem.GetAttribute("key") };
                        btn.Click += (o, evt) =>
                        {
                            scrollTargets[btn.Tag.ToString()].BringIntoView(new Rect(0,0, 50, 200));
                        };
                        wrap.Children.Add(btn);
                    }
                    target.Children.Add(wrap);
                }
                else if (text.Name.Equals("sub"))
                {
                    StackPanel subStack = new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness(20, 5, 20, 5) };
                    target.Children.Add(subStack);
                    FillStackPanel(subStack, text);
                }
                else
                {
                    BBCodeBlock block = new BBCodeBlock();
                    block.FontSize = 14;
                    block.Width = Double.NaN;
                    block.HorizontalAlignment = HorizontalAlignment.Left;
                    block.TextWrapping = TextWrapping.Wrap;
                    block.Margin = new Thickness(5);
                    block.BBCode = text.InnerText;
                    if (scroller != null)
                        block.SetBinding(BBCodeBlock.WidthProperty, new Binding("ActualWidth") { Source = scroller, Converter = new Controls.Converters.ValueSubtractConverter(40) });
                    if (text.HasAttribute("size"))
                        block.FontSize = double.Parse(text.GetAttribute("size"));
                    if (text.HasAttribute("class"))
                    {
                        if (text.GetAttribute("class").Equals("code"))
                        {
                            block.FontFamily = new FontFamily("Courier New");
                            block.Background = new SolidColorBrush(Colors.Black);
                            block.Foreground = new SolidColorBrush(Colors.CornflowerBlue);
                        }
                    }
                    if (text.Name.ToLower().Equals("h"))
                    {
                        if (scrollTargets != null)
                            scrollTargets[text.GetAttribute("key")] = block;
                        block.FontSize = 20;
                        block.FontWeight = FontWeights.Bold;
                        target.Children.Add(new Separator());
                    }
                    target.Children.Add(block);
                }
            }
        }

        static XmlDocument cached_;
        public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e)
        {
            if (e.Fragment.Equals(LastFragment))
                return;
            contentStack.Children.Clear();

            LastFragment = e.Fragment;
            string[] parts = e.Fragment.Split(':');
            int idx = int.Parse(parts[1]);
            
            if (cached_ == null)
            {
                string filePath = string.Format("SprueKit.QuickGuide.{0}.xml", parts[0], App.AppName);
                string code = GetResourceTextFile(filePath, this);
                cached_ = new XmlDocument();
                cached_.LoadXml(code);
            }
            XmlElement root = cached_.DocumentElement;
            int node = 0;
            foreach (XmlElement elem in root.ChildNodes)
            {
                if (node == idx)
                {
                    scrollTargets.Clear();
                    FillStackPanel(contentStack, elem, scrollTargets, scroller);
                    scroller.ScrollToTop();
                    return;
                }
                ++node;
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

        public static string GetResourceTextFile(string filename, object ctx)
        {
            string result = string.Empty;

            using (Stream stream = ctx.GetType().Assembly.GetManifestResourceStream(filename))
            {
                using (StreamReader sr = new StreamReader(stream))
                {
                    result = sr.ReadToEnd();
                }
            }
            return result;
        }
    }
}
