using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace SprueKit.Controls.GraphParts
{
    public class ShaderGraphNode : GraphNode
    {
        public ShaderGraphNode(GraphControl owner, Data.Graph.GraphNode node) : base(owner, node)
        {
            Label descriptionLabel = new Label() { IsHitTestVisible = false };
            descriptionLabel.SetBinding(Label.VisibilityProperty, new Binding("HasDescription") { Source = node, Converter = new BooleanToVisibilityConverter() });
            descriptionLabel.SetBinding(Label.ContentProperty, new Binding("Description") { Source = node });
            trueMasterGrid.Children.Add(descriptionLabel);
            descriptionLabel.Margin = new System.Windows.Thickness(0, -24, 0, 0);

            Header.SetBinding(System.Windows.Controls.DockPanel.BackgroundProperty, new System.Windows.Data.Binding(".") { Converter = new HeaderColorConverter() });
        }

        public class HeaderColorConverter : IValueConverter
        {
            static Dictionary<string, System.Windows.Media.Brush> Brushes = new Dictionary<string, System.Windows.Media.Brush>()
            {
                {"Transparent", new SolidColorBrush(Colors.Transparent) },

                {"Uniforms", new SolidColorBrush(Colors.DarkMagenta) },
                {"Constants", new SolidColorBrush(Colors.DarkBlue) },
                {"Basic Math", new SolidColorBrush(Colors.DarkGreen) },
                {"Trigonometry", new SolidColorBrush(Color.FromRgb(15,45,70)) }, //<----------------
                {"Math", new SolidColorBrush(Colors.DarkOliveGreen) },
                {"Ranges", new SolidColorBrush(Color.FromRgb(45,65,15)) }, //<----------------
                {"Vectors", new SolidColorBrush(Colors.DarkRed) },
                {"Matrices", new SolidColorBrush(Color.FromRgb(60,45,15)) }, //<----------------
                {"Misc", new SolidColorBrush(Colors.DarkCyan) }, //<----------------???

            };
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value != null)
                {
                    Type t = value.GetType();
                    foreach (var grp in Data.ShaderGen.ShaderGenDocument.NodeGroups)
                    {
                        if (grp.Types.Contains(t))
                        {
                            if (Brushes.ContainsKey(grp.Name))
                                return Brushes[grp.Name];
                        }
                    }
                }
                return Brushes["Transparent"];
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return null;
            }
        }

        public class IconConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return GetIcon((Type)parameter);
            }

            public static System.Windows.Media.Imaging.BitmapImage GetIcon(Type t)
            {
                return WPFExt.GetEmbeddedImage("Images/TextureNodes/Generic.png", true);
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return null;
            }
        }
    }
}
