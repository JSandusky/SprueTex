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

    public class SimpleCircleAdorner : Adorner
    {

        // Be sure to call the base class constructor.

        public SimpleCircleAdorner(UIElement adornedElement)
            : base(adornedElement) { }

        protected override void OnRender(DrawingContext drawingContext)
        {
            Rect adornedElementRect = new Rect(this.AdornedElement.DesiredSize);
            // Some arbitrary drawing implements.
            SolidColorBrush renderBrush = new SolidColorBrush(Colors.Green);
            renderBrush.Opacity = 0.2;
            Pen renderPen = new Pen(new SolidColorBrush(Colors.Navy), 1.5);
            double renderRadius = 5.0;
            // Draw a circle at each corner.
            drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.TopLeft, renderRadius, renderRadius);
            drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.TopRight, renderRadius, renderRadius);
            drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.BottomLeft, renderRadius, renderRadius);
            drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.BottomRight, renderRadius, renderRadius);
        }
    }


    public class TextureGraphNode : GraphNode
    {
        public TextureGraphNode(GraphControl owner, Data.Graph.GraphNode node) : base(owner, node)
        {
            // Warp nodes do not use previews
            if (!(node is Data.TexGen.WarpingNode) && !(node is Data.TexGen.ModelNode))
            {
                System.Windows.Controls.Image imgCtrl = new System.Windows.Controls.Image() { DataContext = node };
                imgCtrl.Margin = new System.Windows.Thickness(-16, 0, 0, 0);
                imgCtrl.SetBinding(System.Windows.Controls.Image.SourceProperty, "Preview");
                Content.Children.Add(imgCtrl);
                iconImg.SetBinding(System.Windows.Controls.Image.SourceProperty, new Binding(".") { Converter = new IconConverter(), ConverterParameter = node.GetType() });
            }
            else
            {
                Grid.SetRowSpan(Header, 4);
                headerSeperator.Visibility = Visibility.Collapsed;
                if (!(node is Data.TexGen.WarpOut))
                    inputSocketGrid.Visibility = Visibility.Collapsed;
                else
                    inputSocketGrid.Margin = new Thickness(0, -24, 0, 0);

                Content.Visibility = Visibility.Collapsed;
                outputSocketsList.Margin = new Thickness(0, -24, 0, 0);
            }

            Label descriptionLabel = new Label() { IsHitTestVisible = false };
            descriptionLabel.SetBinding(Label.VisibilityProperty, new Binding("HasDescription") { Source = node, Converter = new BooleanToVisibilityConverter() });
            descriptionLabel.SetBinding(Label.ContentProperty, new Binding("Description") { Source = node });
            trueMasterGrid.Children.Add(descriptionLabel);
            descriptionLabel.Margin = new System.Windows.Thickness(0, -24, 0, 0);
            Header.SetBinding(System.Windows.Controls.DockPanel.BackgroundProperty, new System.Windows.Data.Binding(".") { Converter = new HeaderColorConverter() });
            Loaded += TextureGraphNode_Loaded;
        }

        private void TextureGraphNode_Loaded(object sender, RoutedEventArgs e)
        {
            if (!(BackingData is Data.TexGen.WarpingNode) && !(BackingData is Data.TexGen.SampleControl))
            {
                mainGrid.ContextMenu.Items.Add(new Separator());

                if (Dlg.TexVariationDlg.CanShowVariations(BackingData))
                {
                    var viewOptionsItem = new MenuItem { Header = "View Variations" };
                    mainGrid.ContextMenu.Items.Add(viewOptionsItem);
                    viewOptionsItem.Click += (o, ee) =>
                    {
                        var dlg = new Dlg.TexVariationDlg(BackingData.Graph, BackingData as Data.TexGen.TexGenNode);
                        dlg.ShowDialog();
                    };
                }

                var exportItem = new MenuItem { Header = "Export Image" };
                mainGrid.ContextMenu.Items.Add(exportItem);

                exportItem.Click += (o, ee) =>
                {
                    Dlg.ExportImageDlg.Show((int w, int h) => {
                        ((Data.TexGen.TexGenNode)BackingData).Graph.Prime(null);
                        return ((Data.TexGen.TexGenNode)BackingData).GeneratePreview(w, h);
                    });
                };

                if (BackingData is Data.TexGen.GradientRampTextureModifier)
                {
                    var exportRamp = new MenuItem { Header = "Export Ramp" };
                    mainGrid.ContextMenu.Items.Add(exportRamp);
                    exportRamp.Click += (o, eee) =>
                    {
                        Dlg.ExportImageDlg.Show("Export Ramp", (int w, int h) => {
                            Data.TexGen.GradientRampTextureModifier ramp = (Data.TexGen.GradientRampTextureModifier)BackingData;
                            return ramp.Ramp.GenerateBitmap(w, h);
                        });
                    };
                }

                if (BackingData is Data.TexGen.CurveTextureModifier)
                {
                    var exportRamp = new MenuItem { Header = "Export Curve" };
                    mainGrid.ContextMenu.Items.Add(exportRamp);
                    exportRamp.Click += (o, eee) =>
                    {
                        Dlg.ExportImageDlg.Show("Export Curve as Ramp", (int w, int h) => {
                            Data.TexGen.CurveTextureModifier ramp = (Data.TexGen.CurveTextureModifier)BackingData;
                            return ramp.Curves.GenerateBitmap(w, h);
                        });
                    };
                }

                var exportMeshItem = new MenuItem { Header = "Export Mesh" };
                mainGrid.ContextMenu.Items.Add(exportMeshItem);
                exportMeshItem.Click += (o, ee) =>
                {
                    var clone = ((Data.TexGen.TexGenNode)BackingData).Graph.Clone();
                    int id = ((Data.TexGen.TexGenNode)BackingData).NodeID;
                    Dlg.ExportTexMeshDlg.Show((string fileName, int meshType, int w, int h, float scale, bool decimate, float power) =>
                    {
                        string displayName = meshType == 0 ? "Generating height field mesh" : "Generating marching squares mesh";
                        Parago.Windows.ProgressDialog.Execute(null, "Exporting mesh", displayName, new Action(() =>
                        {
                            var src = clone.Nodes.FirstOrDefault(n => n.NodeID == id);
                            if (src != null)
                            {
                                clone.Prime(null);
                                var bmp = ((Data.TexGen.TexGenNode)src).GeneratePreview(w, h);
                                if (meshType == 0)
                                    Data.MeshData.CreateFromHeightField(bmp, fileName, scale, decimate, power);
                                else
                                    Data.MeshData.CreateMarchingSquares(bmp, meshType == 2, fileName, (byte)(scale * 255), decimate, power);
                                bmp.Dispose();
                            }
                        }));
                    });
                };

                var prefabItem = new MenuItem { Header = "Make into Prefab" };
                mainGrid.ContextMenu.Items.Add(prefabItem);
                prefabItem.Click += (o, ee) =>
                {
                    string outName = Dlg.InputDlg.Show("Create Prefab", "Name the prefab");
                    if (!string.IsNullOrWhiteSpace(outName))
                        OwnerControl.SavePrefab(outName);
                };
            }
        }


        public class HeaderColorConverter : IValueConverter
        {
            static System.Windows.Media.Color TrueDarkGray = System.Windows.Media.Color.FromRgb(30,30,30);
            static System.Windows.Media.Color TrueDarkTurquoise = System.Windows.Media.Color.FromRgb(10, 20, 50);
            static Dictionary<string, System.Windows.Media.Brush> Brushes = new Dictionary<string, System.Windows.Media.Brush>()
            {
                {"Transparent", new LinearGradientBrush(TrueDarkGray, Colors.Transparent,   45) },
                {"Basic Math", new LinearGradientBrush(TrueDarkGray, Colors.DarkMagenta,    45) },
                {"Generators", new LinearGradientBrush(TrueDarkGray, Colors.DarkRed,        45) },
                {"Color", new LinearGradientBrush(TrueDarkGray, Colors.DarkMagenta,         45) },
                {"Filters", new LinearGradientBrush(TrueDarkGray, Colors.DarkBlue,          45) },
                {"Math", new LinearGradientBrush(TrueDarkGray, Colors.DarkMagenta,          45) },
                {"Values", new LinearGradientBrush(TrueDarkGray, Colors.DarkGreen,          45) },
                {"Mesh Bakers", new LinearGradientBrush(TrueDarkGray, TrueDarkTurquoise,    45) },
            };
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value != null)
                {
                    Type t = value.GetType();
                    foreach (var grp in Data.TexGen.TextureGenDocument.NodeGroups)
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
                string typeName = t.Name;
                string imgName = string.Format("Images/TextureNodes/{0}.png", typeName);
                var ret = WPFExt.GetEmbeddedImage(imgName, true);
                if (ret != null)
                    return ret;

                // Special exceptions
                if (t.Name.Contains("Baker"))
                    return WPFExt.GetEmbeddedImage("Images/TextureNodes/generic_baker.png", true, imgName);
                else if (t.Name.Contains("FBM"))
                    return WPFExt.GetEmbeddedImage("Images/TextureNodes/FBMGenerator.png", true, imgName);
                else if (t.Name.Contains("Brick"))
                    return WPFExt.GetEmbeddedImage("Images/TextureNodes/BrickGenerator.png", true, imgName);
                else if (t.Name.Contains("Voronoi"))
                    return WPFExt.GetEmbeddedImage("Images/TextureNodes/VoronoiGenerator.png", true, imgName);

                return WPFExt.GetEmbeddedImage("Images/TextureNodes/Generic.png", true, imgName);
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return null;
            }
        }
    }
}
