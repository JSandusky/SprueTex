using FirstFloor.ModernUI.Windows;
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
using System.Xml;
using FirstFloor.ModernUI.Windows.Navigation;

namespace SprueKit.Controls
{
    public class ImageMapData
    {
        public List<KeyValuePair<string, Rect>> Hotspots = new List<KeyValuePair<string, Rect>>();
        public Dictionary<string, XmlElement> TextBody = new Dictionary<string, XmlElement>();
        public string DetailsLink { get; set; }
        public XmlElement Footer { get; set; }
        public string Image { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public ImageMapData(string file)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(WPFExt.GetEmbeddedFile(file));
            Image = doc.DocumentElement.GetAttribute("image");
            DetailsLink = doc.DocumentElement.GetAttribute("details");
            Width = double.Parse(doc.DocumentElement.GetAttribute("width"));
            Height = double.Parse(doc.DocumentElement.GetAttribute("height"));
            Footer = doc.DocumentElement.SelectSingleNode("//footer") as XmlElement;

            var imgAreas = doc.DocumentElement.SelectNodes("//area");
            foreach (var area in imgAreas)
                Hotspots.Add(GetRect(area as XmlElement));

            var text = doc.DocumentElement.SelectNodes("//section");
            foreach (var txt in text)
            {
                var elem = txt as XmlElement;
                string title = elem.GetAttribute("name");
                string textValue = elem.InnerText;
                TextBody[title] = txt as XmlElement;
            }
        }

        public KeyValuePair<string,Rect> GetRect(XmlElement elem)
        {
            string title = elem.GetAttribute("title");
            string[] coords = elem.GetAttribute("coords").Split(',');
            double[] pos = new double[]
            {
                double.Parse(coords[0]),
                double.Parse(coords[1]),
                double.Parse(coords[2]),
                double.Parse(coords[3]),
            };
            double minX = Math.Min(pos[0], pos[2]);
            double maxX = Math.Max(pos[0], pos[2]);
            double minY = Math.Min(pos[1], pos[3]);
            double maxY = Math.Max(pos[1], pos[3]);

            return new KeyValuePair<string,Rect>(title, new Rect(minX, minY, maxX - minX, maxY - minY));
        }
    }


    /// <summary>
    /// Interaction logic for ImageMap.xaml
    /// </summary>
    public partial class ImageMap : UserControl, IContent
    {
        static SolidColorBrush Brush = new SolidColorBrush(Color.FromArgb(48, Colors.CornflowerBlue.R, Colors.CornflowerBlue.G, Colors.CornflowerBlue.B));
        public ImageMapData Data { get; set; }
        public ImageMap()
        {
            InitializeComponent();
            //ShapeCanvas.LayoutUpdated += ShapeCanvas_LayoutUpdated;
            ShapeCanvas.SizeChanged += ShapeCanvas_LayoutUpdated;
        }

        private void ShapeCanvas_LayoutUpdated(object sender, EventArgs e)
        {
            UpdateCanvas();
        }

        void UpdateCanvas()
        {
            while (ShapeCanvas.Children.Count > 0)
                ShapeCanvas.Children.Remove(ShapeCanvas.Children[0] as UIElement);

            foreach (var hotspot in Data.Hotspots)
            {
                Rectangle r = new Rectangle
                {
                    Fill = Brush,
                    Width = Mathf.Normalize(hotspot.Value.Width, 0, Data.Width) * ShapeCanvas.ActualWidth,
                    Height = Mathf.Normalize(hotspot.Value.Height, 0, Data.Height) * ShapeCanvas.ActualHeight,
                    IsHitTestVisible = true,
                    Tag = hotspot
                };
                Canvas.SetLeft(r, Mathf.Normalize(hotspot.Value.X, 0, Data.Width) * ShapeCanvas.ActualWidth);
                Canvas.SetTop(r, Mathf.Normalize(hotspot.Value.Y, 0, Data.Height) * ShapeCanvas.ActualHeight);
                ShapeCanvas.Children.Add(r);
                r.MouseLeftButtonDown += R_MouseLeftButtonDown;
            }
        }

        private void R_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var r = sender as Rectangle;
            var kvp = (KeyValuePair<string, Rect>)r.Tag;
            if (Data.TextBody.ContainsKey(kvp.Key))
            {
                var popup = PopupHelper.Create();
                StackPanel sp = new StackPanel { Margin = new Thickness(5), MaxWidth = 300 };
                popup.Grid.Children.Add(sp);
                QuickGuide.QuickGuide.FillStackPanel(sp, Data.TextBody[kvp.Key]);
                popup.ShowAtMouse();
            }
        }

        public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e)
        {
            Data = new ImageMapData(e.Fragment);
            DisplayImage.Source = WPFExt.GetEmbeddedImage(Data.Image);
            DataContext = Data;
            Footer.Children.Clear();
            if (Data.Footer != null)
                QuickGuide.QuickGuide.FillStackPanel(Footer, Data.Footer);
            UpdateCanvas();
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
