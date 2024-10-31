using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Data;

using Microsoft.Xna.Framework;
using System.Windows;
using System.Windows.Media;

namespace SprueKit.Controls
{
    /// <summary>
    /// Interaction logic for DataGrid.xaml
    /// </summary>
    public partial class PropertiesDataGrid : UserControl
    {
        IOCDependency<DocumentManager> documentManager;
        public ObservableCollection<object> Objects { get; set; }

        public PropertiesDataGrid()
        {
            InitializeComponent();

            documentManager = new IOCDependency<DocumentManager>();
            documentManager.Object.OnActiveDocumentChanged += Object_OnActiveDocumentChanged;

            if (documentManager.Object.ActiveDocument != null)
                Object_OnActiveDocumentChanged(documentManager.Object.ActiveDocument, null);
        }

        private void Object_OnActiveDocumentChanged(Document newDoc, Document oldDoc)
        {
            DataContext = newDoc;
            typeCombo.IsEnabled = DataContext != null;
            grid.IsEnabled = DataContext != null;

            if (newDoc is Data.SprueModelDocument)
            {
                typeCombo.Items.Clear();
                typeCombo.Items.Add(new ComboBoxItem { Content = "Sprue Model", Tag = typeof(SprueKit.Data.SprueModel) });
                typeCombo.Items.Add(new ComboBoxItem { Content = "Simple Piece", Tag = typeof(SprueKit.Data.SimplePiece) });
                typeCombo.Items.Add(new ComboBoxItem { Content = "Chain Piece", Tag = typeof(SprueKit.Data.ChainPiece) });
                typeCombo.Items.Add(new ComboBoxItem { Content = "Chain Bone", Tag = typeof(SprueKit.Data.ChainPiece.ChainBone) });
                typeCombo.Items.Add(new ComboBoxItem { Content = "Mesh", Tag = typeof(SprueKit.Data.ModelPiece) });
                typeCombo.Items.Add(new ComboBoxItem { Content = "Reference", Tag = typeof(SprueKit.Data.InstancePiece) });
                typeCombo.Items.Add(new ComboBoxItem { Content = "Marker", Tag = typeof(SprueKit.Data.MarkerPiece) });

                typeCombo.Items.Add(new ComboBoxItem { Content = "Gradient Map", Tag = typeof(SprueKit.Data.GradientTextureComponent) });
                typeCombo.Items.Add(new ComboBoxItem { Content = "Color Cube", Tag = typeof(SprueKit.Data.ColorCubeTextureComponent) });
                typeCombo.Items.Add(new ComboBoxItem { Content = "Decal Projector", Tag = typeof(SprueKit.Data.DecalTextureComponent) });
                typeCombo.Items.Add(new ComboBoxItem { Content = "Box Projector", Tag = typeof(SprueKit.Data.BoxTextureComponent) });
                typeCombo.Items.Add(new ComboBoxItem { Content = "Cylinder Projector", Tag = typeof(SprueKit.Data.CylinderTextureComponent) });
                typeCombo.Items.Add(new ComboBoxItem { Content = "Hemisphere Projector", Tag = typeof(SprueKit.Data.DomeTextureComponent) });
            }
            else if (newDoc is Data.TexGen.TextureGenDocument)
            {
                var doc = newDoc as Data.TexGen.TextureGenDocument;
                UpdateTexGenDocCombo(doc);
                doc.DataRoot.Nodes.CollectionChanged += Nodes_CollectionChanged;
            }

            if (oldDoc is Data.TexGen.TextureGenDocument)
                ((Data.TexGen.TextureGenDocument)oldDoc).DataRoot.Nodes.CollectionChanged -= Nodes_CollectionChanged;

            if (newDoc != null)
            {
                if (oldDoc != null)
                    oldDoc.Selection.PropertyChanged -= Selection_PropertyChanged;
                newDoc.Selection.PropertyChanged += Selection_PropertyChanged;
            }
        }

        private void Nodes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateTexGenDocCombo(documentManager.Object.ActiveDoc<Data.TexGen.TextureGenDocument>());
        }

        void UpdateTexGenDocCombo(Data.TexGen.TextureGenDocument doc)
        {
            int curIndex = typeCombo.SelectedIndex;
            typeCombo.Items.Clear();
            HashSet<Type> hitTypes = new HashSet<Type>();
            foreach (var node in doc.DataRoot.Nodes)
            {
                if (!hitTypes.Contains(node.GetType()))
                {
                    typeCombo.Items.Add(new ComboBoxItem
                    {
                        Content = node.GetType().Name.Replace("Node", "").SplitCamelCase(),
                        Tag = node.GetType()
                    });
                    hitTypes.Add(node.GetType());
                }
            }
            typeCombo.SelectedIndex = curIndex;
        }

        bool blockSelectionSignal_ = false;
        private void Selection_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            blockSelectionSignal_ = true;
            grid.SelectedItem = documentManager.Object.ActiveDocument.Selection.MostRecentlySelected;
            blockSelectionSignal_ = false;
        }

        private void typeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (typeCombo.SelectedItem == null)
            {
                grid.Columns.Clear();
                return;
            }

            Type t = ((ComboBoxItem)typeCombo.SelectedItem).Tag as Type;
            if (t != null)
            {
                Bind(t);
                if (documentManager.Object.ActiveDocument != null)
                {
                    var sprueDoc = documentManager.Object.ActiveDoc<Data.SprueModelDocument>();
                    if (sprueDoc != null)
                    {
                        List<Data.SpruePiece> pieces = new List<Data.SpruePiece>();
                        ((SprueKit.Data.SprueModelDocument)documentManager.Object.ActiveDocument).DataRoot.VisitAll((p) =>
                        {
                            if (p.GetType() == t)
                                pieces.Add(p);
                        });
                        grid.ItemsSource = pieces;
                    }

                    var texGenDoc = documentManager.Object.ActiveDoc<Data.TexGen.TextureGenDocument>();
                    if (texGenDoc != null)
                    {
                        List<Data.Graph.GraphNode> nodes = new List<Data.Graph.GraphNode>();
                        nodes.AddRange(texGenDoc.DataRoot.Nodes.Where(n => n.GetType() == t));
                        grid.ItemsSource = nodes;
                    }
                }
            }
        }

        void Bind(Type aType)
        {
            grid.Columns.Clear();
            var properties = SprueKit.PropertyHelpers.GetAlphabetical(aType);

            foreach (var propertyInfo in properties)
            {
                DataGridColumn col = null;

                BindingMode bindMode = BindingMode.TwoWay;
                var setter = propertyInfo.Property.GetSetMethod(true);
                bool writable = (propertyInfo.Property.CanWrite && setter != null && setter.IsPublic);
                if (!writable)
                    bindMode = BindingMode.OneWay;

                Binding binding = new Binding(propertyInfo.Property.Name) { Mode = bindMode };
                Type propertyType = propertyInfo.Property.PropertyType;
                if (propertyType == typeof(int) ||
                    propertyType == typeof(float) ||
                    propertyType == typeof(Microsoft.Xna.Framework.Vector2) ||
                    propertyType == typeof(Microsoft.Xna.Framework.Vector3) ||
                    propertyType == typeof(Microsoft.Xna.Framework.Vector4))
                {
                    col = new DataGridTextColumn();
                    ((DataGridTextColumn)col).Binding = binding;
                }
                else if (propertyType == typeof(string))
                {
                    col = new DataGridTextColumn();
                    ((DataGridTextColumn)col).Binding = binding;
                }
                else if (propertyType == typeof(bool))
                {
                    //col = new DataGridTemplateColumn();
                    //var tplCol = col as DataGridTemplateColumn;
                    //
                    //// Create the TextBlock
                    //FrameworkElementFactory cbFactory = new FrameworkElementFactory(typeof(CheckBox));
                    //
                    //cbFactory.SetBinding(CheckBox.IsCheckedProperty, binding);
                    //DataTemplate cbTemplate = new DataTemplate();
                    //cbTemplate.VisualTree = cbFactory;
                    //
                    //DataTemplate dTpl = new DataTemplate();
                    //dTpl.VisualTree = cbFactory;
                    //
                    //tplCol.CellTemplate = dTpl;

                    binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                    col = new DataGridCheckBoxColumn();
                    ((DataGridCheckBoxColumn)col).Binding = binding;
                    ((DataGridCheckBoxColumn)col).ElementStyle = Application.Current.FindResource(typeof(CheckBox)) as Style;
                }
                else if (propertyType.IsEnum)
                {
                    binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                    col = new DataGridComboBoxColumn();
                    ((DataGridComboBoxColumn)col).SelectedItemBinding = binding;
                    ((DataGridComboBoxColumn)col).ItemsSource = Enum.GetValues(propertyType);
                    ((DataGridComboBoxColumn)col).ElementStyle = Application.Current.FindResource(typeof(ComboBox)) as Style;
                }
                else if (propertyType == typeof(Microsoft.Xna.Framework.Color))
                {
                    col = new DataGridTemplateColumn();
                    // Create The Column
                    DataGridTemplateColumn guidColumn = col as DataGridTemplateColumn;

                    Binding colorText = new Binding(propertyInfo.Property.Name) { Converter = new Data.XNAColorConverter() };
                    colorText.Mode = BindingMode.OneWay;

                    Binding colorBind = new Binding(propertyInfo.Property.Name);
                    colorBind.Converter = new Converters.XNAColorToBrushConverter();
                    colorBind.Mode = BindingMode.OneWay;

                    Binding fgBind = new Binding(propertyInfo.Property.Name);
                    fgBind.Converter = new Converters.XNAColorToInvertedBrushConverter();
                    fgBind.Mode = BindingMode.OneWay;

                    // Create the TextBlock
                    FrameworkElementFactory textFactory = new FrameworkElementFactory(typeof(TextBlock));
                    textFactory.SetBinding(TextBlock.TextProperty, colorText);
                    textFactory.SetBinding(TextBlock.BackgroundProperty, colorBind);
                    textFactory.SetBinding(TextBlock.ForegroundProperty, fgBind);
                    DataTemplate textTemplate = new DataTemplate();
                    textTemplate.VisualTree = textFactory;

                    FrameworkElementFactory viewStack = new FrameworkElementFactory(typeof(StackPanel));
                    viewStack.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);

                    FrameworkElementFactory lblFactory = new FrameworkElementFactory(typeof(TextBlock));
                    lblFactory.SetBinding(TextBlock.TextProperty, colorText);
                    lblFactory.SetBinding(TextBlock.BackgroundProperty, colorBind);
                    lblFactory.SetBinding(TextBlock.ForegroundProperty, fgBind);
                    viewStack.AppendChild(lblFactory);

                    FrameworkElementFactory btnFactory = new FrameworkElementFactory(typeof(StyledButton));
                    btnFactory.SetValue(Button.ContentProperty, "...");
                    btnFactory.SetValue(Button.TagProperty, propertyInfo.Property.Name);
                    btnFactory.AddHandler(Button.ClickEvent, new RoutedEventHandler(onPickColor));

                    viewStack.AppendChild(btnFactory);

                    DataTemplate comboTemplate = new DataTemplate();
                    comboTemplate.VisualTree = viewStack;

                    // Set the Templates to the Column
                    guidColumn.CellTemplate = textTemplate;
                    guidColumn.CellEditingTemplate = comboTemplate;
                }

                if (col != null)
                {
                    col.Header = propertyInfo.DisplayName;
                    grid.Columns.Add(col);
                    if (propertyInfo.DisplayName.Trim().ToLower().Equals("name"))
                        col.DisplayIndex = 0;
                    else if (propertyInfo.DisplayName.Trim().ToLower().Equals("position"))
                        col.DisplayIndex = 1;
                    else if (propertyInfo.DisplayName.Trim().ToLower().Equals("rotation"))
                        col.DisplayIndex = 2;
                    else if (propertyInfo.DisplayName.Trim().ToLower().Equals("scale"))
                        col.DisplayIndex = 2;
                }
            }
        }

        private void grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!blockSelectionSignal_ && e.OriginalSource == sender)
            {
                var activeDoc = documentManager.Object.ActiveDocument;
                if (e.AddedItems.Count > 0)
                {
                    activeDoc.Selection.Selected.Clear();
                    activeDoc.Selection.SetSelected(e.AddedItems[0]);
                }
            }
        }

        private void onPickColor(object sender, EventArgs e)
        {
            var btnSrc = sender as Button;
            var bindPath = btnSrc.Tag as string;
            PopupHelper popup = PopupHelper.Create();
            var canvas = new ColorPickerLib.Controls.ColorCanvas();
            canvas.Background = new SolidColorBrush(Colors.Transparent);
            canvas.MinHeight = 320;
            canvas.MinWidth = 320;
            canvas.Width = 320;
            canvas.SelectedColor = Colors.Red;
            canvas.SetBinding(ColorPickerLib.Controls.ColorCanvas.SelectedColorProperty, new Binding(bindPath) { Converter = new Data.XNAColorConverter(), Source = btnSrc.DataContext });
            canvas.DataContext = DataContext;
            popup.Grid.Children.Add(canvas);
            popup.ShowAtMouse();
        }
    }
}
