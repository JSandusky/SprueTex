using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Controls;
using GongSolutions.Wpf.DragDrop;
using SprueKit.Controls.GraphParts;
using SprueKit.Commands;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows;
using System.Reflection;
using Microsoft.Xna.Framework;
using System.Windows.Media;

namespace SprueKit.Data.TexGen
{
    public class TextureGenGraphControl : Controls.GraphControl, IQuickActionSource
    {
        TextureGenDocument doc_;
        Graph.Graph graph_;
        public TextureGenGraphControl(TextureGenDocument doc, Graph.Graph g) : base(doc)
        {
            doc_ = doc;
            graph_ = g;
            OnRegenerate += OnRegen;
        }

        protected void OnRegen(object sender, Controls.GraphParts.GraphNode node)
        {
            doc_.InvalidateNode(node.BackingData as TexGenNode, false);
        }

        protected override Controls.GraphParts.GraphNode CreateNode(Graph.GraphNode forNode, bool isNew)
        {
            var ret = new TextureGraphNode(this, forNode);
            ret.RenderTransform = nodeTranslation;
            return ret;
        }

        protected override void NodePasted(Data.Graph.GraphNode nd)
        {
            doc_.InvalidateNode(nd as TexGenNode, false); // will be called on each node, trace is waste
        }

        void CreateNode(Type t)
        {
            Data.Graph.GraphNode nd = Activator.CreateInstance(t) as Data.Graph.GraphNode;
            if (nd != null)
            {
                nd.Graph = graph_;
                nd.Construct();
                nd.VisualX = ActualWidth / 2 - nodeTranslation.X;
                nd.VisualY = ActualHeight / 2 - nodeTranslation.Y;
                graph_.AddNode(nd);
                nd.UpdateSocketIDs();
                doc_.InvalidateNode((TexGenNode)nd, false); // should not need to trace
            }
        }

        public override void Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Data is TreeViewItem && ((TreeViewItem)dropInfo.Data).DataContext is Type)
            {
                Type t = ((TreeViewItem)dropInfo.Data).DataContext as Type;
                if (typeof(TexGenNode).IsAssignableFrom(t))
                {
                    Data.Graph.GraphNode nd = Activator.CreateInstance(t) as Data.Graph.GraphNode;
                    if (nd != null)
                    {
                        nd.Graph = graph_;
                        nd.Construct();
                        nd.VisualX = dropInfo.DropPosition.X - nodeTranslation.X;
                        nd.VisualY = dropInfo.DropPosition.Y - nodeTranslation.Y;

                        if (nd is TexGen.TextureOutputNode)
                        {
                            ((TexGen.TextureOutputNode)nd).OutputChannel = (Data.TextureChannel)((TreeViewItem)dropInfo.Data).Tag;
                            nd.Name = ((TexGen.TextureOutputNode)nd).OutputChannel.ToString();
                        }

                        graph_.AddNode(nd);
                        nd.UpdateSocketIDs();
                        doc_.InvalidateNode((TexGenNode)nd, false); // this is a new node, should not trace
                    }
                }
            }
            else if (dropInfo.Data is TreeViewItem && ((TreeViewItem)dropInfo.Data).Tag.ToString().StartsWith("PREFAB:"))
            {
                var treeITem = ((TreeViewItem)dropInfo.Data);
                string prefabName = ((TreeViewItem)dropInfo.Data).Tag.ToString().Replace("PREFAB:", "");

                //var dropPt = new System.Windows.Point(dropInfo.DropPosition.X - nodeTranslation.X, dropInfo.DropPosition.Y - nodeTranslation.Y);
                InsertPrefab(prefabName, dropInfo.DropPosition);
            }
            else if (dropInfo.Data is Data.FileData)
            {
                var fileData = dropInfo.Data as Data.FileData;
                if (Data.FileData.IsImage(fileData.FilePath))
                {
                    var newNode = new TexGen.TextureNode();
                    newNode.Graph = graph_;
                    newNode.Construct();
                    newNode.VisualX = dropInfo.DropPosition.X - nodeTranslation.X;
                    newNode.VisualY = dropInfo.DropPosition.Y - nodeTranslation.Y;
                    newNode.Name = System.IO.Path.GetFileNameWithoutExtension(fileData.FilePath);
                    newNode.Texture = new Uri(fileData.FilePath);
                    graph_.AddNode(newNode);
                    doc_.InvalidateNode(newNode, false);
                }
                else if (Data.FileData.IsModel(fileData.FilePath))
                {
                    var newNode = new TexGen.ModelNode();
                    newNode.Graph = graph_;
                    newNode.Construct();
                    newNode.VisualX = dropInfo.DropPosition.X - nodeTranslation.X;
                    newNode.VisualY = dropInfo.DropPosition.Y - nodeTranslation.Y;
                    newNode.Name = System.IO.Path.GetFileNameWithoutExtension(fileData.FilePath);
                    newNode.ModelFile.ModelFile = new Uri(fileData.FilePath);
                    graph_.AddNode(newNode);
                    doc_.InvalidateNode(newNode, false);
                }
            }
            else if (dropInfo.Data is DataObject)
            {
                if (((DataObject)dropInfo.Data).ContainsFileDropList())
                {
                    var data = dropInfo.Data as DataObject;
                    var paths = data.GetFileDropList();
                    if (paths.Count > 0)
                    {
                        var path = paths[0];
                        if (Data.FileData.IsImage(path))
                        {
                            var newNode = new TexGen.TextureNode();
                            newNode.Graph = graph_;
                            newNode.Construct();
                            newNode.VisualX = dropInfo.DropPosition.X - nodeTranslation.X;
                            newNode.VisualY = dropInfo.DropPosition.Y - nodeTranslation.Y;
                            newNode.Name = System.IO.Path.GetFileNameWithoutExtension(path);
                            newNode.Texture = new Uri(path);
                            graph_.AddNode(newNode);
                            doc_.InvalidateNode(newNode, false);
                        }
                        else if (Data.FileData.IsModel(path))
                        {
                            var newNode = new TexGen.ModelNode();
                            newNode.Graph = graph_;
                            newNode.Construct();
                            newNode.VisualX = dropInfo.DropPosition.X - nodeTranslation.X;
                            newNode.VisualY = dropInfo.DropPosition.Y - nodeTranslation.Y;
                            newNode.Name = System.IO.Path.GetFileNameWithoutExtension(path);
                            newNode.ModelFile.ModelFile = new Uri(path);
                            graph_.AddNode(newNode);
                            doc_.InvalidateNode(newNode, false);
                        }
                    }
                }
            }
        }

        public override void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Data is TreeViewItem && ((TreeViewItem)dropInfo.Data).DataContext is Type)
            {
                Type t = ((TreeViewItem)dropInfo.Data).DataContext as Type;
                if (typeof(TexGenNode).IsAssignableFrom(t))
                {
                    dropInfo.Effects = System.Windows.DragDropEffects.Copy;
                    return;
                }
            }
            else if (dropInfo.Data is Data.FileData)
            {
                var fileData = dropInfo.Data as Data.FileData;
                if (Data.FileData.IsImage(fileData.FilePath) || Data.FileData.IsModel(fileData.FilePath))
                {
                    dropInfo.Effects = DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link;
                    return;
                }
            }
            else if (dropInfo.Data is TreeViewItem && ((TreeViewItem)dropInfo.Data).Tag.ToString().StartsWith("PREFAB:"))
            {
                dropInfo.Effects = DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link;
                return;
            }
            else if (dropInfo.Data is DataObject)
            {
                if (((DataObject)dropInfo.Data).ContainsFileDropList())
                {
                    var data = dropInfo.Data as DataObject;
                    var paths = data.GetFileDropList();
                    if (paths.Count > 0)
                    {
                        var path = paths[0];
                        if (Data.FileData.IsImage(path) || Data.FileData.IsModel(path))
                        {
                            dropInfo.Effects = DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link;
                            return;
                        }
                    }
                }
            }
            base.DragOver(dropInfo);
        }

        static CommandInfo[] info_;
        public CommandInfo[] GetCommands()
        {
            if (info_ == null)
            {
                List<CommandInfo> infos = new List<CommandInfo>();
                infos.Add(new CommandInfo { Name = "Create Output", Action = (o,c) =>
                {
                    CreateNode(typeof(TexGen.BlendNode));
                } });

                foreach (var grp in TextureGenDocument.NodeGroups)
                {
                    foreach (var node in grp.Types)
                    {
                        bool isDemoRestricted = false;
#if IS_DEMO
                        isDemoRestricted = TextureGenDocument.DemoRestricted.Contains(node);
                        if (isDemoRestricted)
                            continue;
#endif

                        infos.Add(new CommandInfo
                        {
                            Name = string.Format("Create {0}", node.Name.Replace("Node", "").SplitCamelCase()),
                            Action = (o, c) =>
                            {
                                CreateNode(node);
                            }
                        });
                    }
                }
                info_ = infos.ToArray();
            }
            return info_;
        }
    }

    public class TextureGenDocument : Document
    {
        static Grid sharedLeftSide_;
        static Graphics.BaseScene sharedScene_;
        static Graphics.Paint.PaintScene2D paint2D_;

        Graphics.TexGraph.TexGraphViewport previewViewport_;
        Controls.GraphControl graphControl_;
        public Graph.Graph DataRoot { get; set; }

        float uTiling_ = 1.0f;
        float vTiling_ = 1.0f;
        public float UTiling { get { return uTiling_; } set { uTiling_ = value; OnPropertyChanged(); } }
        public float VTiling { get { return vTiling_; } set { vTiling_ = value; OnPropertyChanged(); } }

        bool flipCull_ = false;
        public bool FlipCulling { get { return flipCull_; } set { flipCull_ = value; OnPropertyChanged(); } }

        bool debugTangents_ = false;
        public bool DebugTangents { get { return debugTangents_; } set { debugTangents_ = value; OnPropertyChanged(); } }

        bool drawGrid_ = true;
        public bool DrawGrid { get { return drawGrid_; }set { drawGrid_ = value; OnPropertyChanged(); } }

        public Graphics.TexGraph.TexGraphViewport Viewport { get { return previewViewport_; } }

        public TextureGenDocument()
        {
            int stepSize = 130;
            int xPos = 225;
            var graph = new Graph.Graph();
            graph.AddNode(new TexGen.TextureOutputNode() { Name = "Diffuse", OutputChannel = TextureChannel.Diffuse, VisualX = xPos, VisualY = 0 });
            graph.AddNode(new TexGen.TextureOutputNode() { Name = "Roughness", OutputChannel = TextureChannel.Roughness, VisualX = xPos, VisualY = stepSize});
            graph.AddNode(new TexGen.TextureOutputNode() { Name = "Metallic", OutputChannel = TextureChannel.Metallic, VisualX = xPos, VisualY = stepSize * 2});
            graph.AddNode(new TexGen.TextureOutputNode() { Name = "Normal Map", OutputChannel = TextureChannel.NormalMap, VisualX = xPos, VisualY = stepSize * 3});
            graph.AddNode(new TexGen.ColorNode() { Name = "Default Normal", Color = new Microsoft.Xna.Framework.Color(128, 128, 255, 255), VisualX = xPos - 200, VisualY = stepSize * 3 });

        // Setup some random generators so they don't start with a blank canvas
            Random r = new Random();
            var diffuseGenerator = Activator.CreateInstance(NodeGroups[2].Types.Random(r)) as TexGenNode;
            diffuseGenerator.Name = diffuseGenerator.GetType().Name.Replace("Generator", "").SplitCamelCase();
            diffuseGenerator.VisualX = xPos - 200;
            diffuseGenerator.VisualY = 0;
            Util.TypeRandomizer.Randomize(diffuseGenerator);
            graph.AddNode(diffuseGenerator);

            var roughnessGenerator = Activator.CreateInstance(NodeGroups[2].Types.Random(r)) as TexGenNode;
            roughnessGenerator.Name = roughnessGenerator.GetType().Name.Replace("Generator", "").SplitCamelCase();
            roughnessGenerator.VisualX = xPos - 200;
            roughnessGenerator.VisualY = stepSize;
            Util.TypeRandomizer.Randomize(roughnessGenerator);
            graph.AddNode(roughnessGenerator);

            foreach (var node in graph.Nodes)
            {
                string n = node.Name;
                node.Construct();
                node.Name = n;
            }
            graph.Connect(graph.Nodes[4].OutputSockets[0], graph.Nodes[3].InputSockets[0]);
            graph.Connect(diffuseGenerator.OutputSockets[0], graph.Nodes[0].InputSockets[0]);
            graph.Connect(roughnessGenerator.OutputSockets[0], graph.Nodes[1].InputSockets[0]);

            DataRoot = graph;
            CommonConstruct(new Dictionary<string, object>());

            foreach (var nd in DataRoot.Nodes)
            {
                //string n = nd.Name;
                //nd.Construct();
                //nd.Name = n;
                InvalidateNode((TexGenNode)nd, false); // for loop so no trace
            }
        }

        public TextureGenDocument(Uri filePath, bool isTemplate)
        {
            Dictionary<string, object> paramData = new Dictionary<string, object>();

            DataRoot = new Graph.Graph();
            string path = filePath.AbsolutePath;
            if (!isTemplate)
                FileURI = filePath;
            IsDirty = isTemplate;

            SerializationContext ctx = new SerializationContext(new Uri(System.IO.Path.GetDirectoryName(path)));
            if (path.EndsWith(".txml"))
            {
                System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                doc.Load(filePath.AbsolutePath);
                var graphElem = doc.DocumentElement.SelectSingleNode("graph") as System.Xml.XmlElement;
                DataRoot.Deserialize(ctx, graphElem);
            }
            else if (path.EndsWith(".texg"))
            {
                using (System.IO.FileStream stream = new System.IO.FileStream(path, System.IO.FileMode.Open))
                {
                    using (System.IO.BinaryReader reader = new System.IO.BinaryReader(stream))
                    {
                        DataRoot.Deserialize(ctx, reader);
                    }
                }
            }

            if (ctx.BrokenPaths.Count > 0)
            {
                Dlg.PathFixupDlg dlg = new Dlg.PathFixupDlg(ctx);
                dlg.ShowDialog();
            }

            CommonConstruct(paramData);

            ErrorHandler.inst().Info(string.Format("Opened texture graph {0}", filePath));

            foreach (var node in DataRoot.Nodes)
                InvalidateNode((TexGenNode)node, false); // for loop so no trace
        }

        void CommonConstruct(Dictionary<string, object> parameters)
        {
            DataRoot.InitializeIDs();
            DocumentTypeName = "Texture Graph";
            FileMask = "Texture Graph XML (*.txml)|*.txml|Texture Graph Binary (*.texg)|*.texg";
            

            if (sharedLeftSide_ == null)
            {
                sharedLeftSide_ = new Grid();
                sharedLeftSide_.RowDefinitions.Add(new RowDefinition());
                sharedLeftSide_.RowDefinitions.Add(new RowDefinition() { Height = new System.Windows.GridLength(6) });
                sharedLeftSide_.RowDefinitions.Add(new RowDefinition());

                sharedScene_ = new Graphics.BaseScene();

                GridSplitter splitter = new GridSplitter();
                splitter.MouseDoubleClick += (so, sevt) =>
                {
                    if (so == splitter)
                    {
                        if (sharedLeftSide_.RowDefinitions[2].Height.Value == 0)
                            sharedLeftSide_.RowDefinitions[2].Height = new System.Windows.GridLength(sharedLeftSide_.ActualHeight * 0.3f, System.Windows.GridUnitType.Pixel);
                        else
                            sharedLeftSide_.RowDefinitions[2].Height = new System.Windows.GridLength(0, System.Windows.GridUnitType.Pixel);
                    }
                };
                Grid.SetRow(splitter, 1);
                Grid subGrid = new Grid();
                subGrid.Children.Add(sharedScene_);
                sharedLeftSide_.Children.Add(subGrid);
                sharedLeftSide_.Children.Add(splitter);
                var nodeTree = new Controls.TexGen.TextureGenTree();
                Grid.SetRow(nodeTree, 2);
                sharedLeftSide_.Children.Add(nodeTree);

                /*
                Button lightSwitch = new Button { Content = new Image { Source = WPFExt.GetEmbeddedImage("Images/icon_light.png"), Width = 16, Height = 16 }, Focusable = false, Margin = new Thickness(10) };
                lightSwitch.HorizontalAlignment = HorizontalAlignment.Left;
                lightSwitch.VerticalAlignment = VerticalAlignment.Top;
                subGrid.Children.Add(lightSwitch);
                lightSwitch.Click += (o, evt) =>
                {
                    var docManager = new IOCDependency<DocumentManager>().Object;

                    var popup = PopupHelper.Create();
                    var stack = new StackPanel { Orientation = Orientation.Vertical, Margin = new System.Windows.Thickness(5) };
                    popup.Grid.MinWidth = 150;
                    popup.Grid.Children.Add(stack);

                    CheckBox animLight = new CheckBox() { Content = "Animate Light", ToolTip = "Scene light will oscillate back and forth" };
                    animLight.SetBinding(CheckBox.IsCheckedProperty, new Binding("ActiveDocument.Viewport.AnimateLight") { Source = docManager });
                    stack.Children.Add(animLight);

                    CheckBox animLowLight = new CheckBox() { Content = "Low Altitude Light", ToolTip = "Light will move at a lower altitude, ideal for models with less rounded tops" };
                    animLowLight.SetBinding(CheckBox.IsCheckedProperty, new Binding("ActiveDocument.Viewport.LowLight") { Source = docManager });
                    stack.Children.Add(animLowLight);

                    stack.Children.Add(new Label { Content = "IBL Cube", HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(2) });
                    ComboBox iblCombo = new ComboBox();
                    iblCombo.Items.Add("Desert Day");
                    iblCombo.Items.Add("Forest");
                    iblCombo.Items.Add("City Night");
                    iblCombo.SetBinding(ComboBox.SelectedIndexProperty, new Binding("ActiveDocument.Viewport.SkyBoxIndex") { Source = docManager });
                    stack.Children.Add(iblCombo);

                    popup.ShowAtMouse(false, lightSwitch);
                };*/

                Button viewSwitch = new Button { Content = new Image { Source = WPFExt.GetEmbeddedImage("Images/godot/icon_visible.png"), Width = 16, Height = 16 }, Focusable = false };
                viewSwitch.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                StackPanel sp = new StackPanel { Orientation = Orientation.Vertical, HorizontalAlignment = System.Windows.HorizontalAlignment.Right, Margin = new System.Windows.Thickness(10) };
                sp.Children.Add(viewSwitch);
                subGrid.Children.Add(sp);
                viewSwitch.Click += (o, evt) =>
                {
                    var popup = PopupHelper.Create();
                    var stack = new StackPanel { Orientation = Orientation.Vertical, Margin = new System.Windows.Thickness(5) };
                    popup.Grid.MinWidth = 150;
                    popup.Grid.Children.Add(stack);

                    stack.Children.Add(new Label { Content = "Shape", HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(2) });
                    var docManager = new IOCDependency<DocumentManager>().Object;
                    var prims = Enum.GetValues(typeof(Graphics.MeshPrimitiveType));
                    for (int i = 0; i < prims.Length - 1; ++i)
                    {
                        var enumValue = (Graphics.MeshPrimitiveType)prims.GetValue(i);
                        ToggleButton btn = new ToggleButton { Content = enumValue.ToString() };
                        btn.SetBinding(ToggleButton.IsCheckedProperty, new Binding("ActiveDocument.Viewport.Primitive") { Source = docManager, Converter = new Controls.Converters.BoolValueCompareConverter(), ConverterParameter = enumValue, Mode = BindingMode.OneWay });
                        stack.Children.Add(btn);
                        btn.Click += (sender, eventData) =>
                        {
                            var doc = docManager.ActiveDoc<TextureGenDocument>();
                            if (doc != null)
                                doc.Viewport.Primitive = enumValue;
                        };
                    }
                    Button custom = new Button() { Content = "Custom" };
                    stack.Children.Add(custom);
                    custom.Click += (sender, eventData) =>
                    {
                        try
                        {
                            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
                            dlg.Filter = Data.FileData.ModelFileMask;
                            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            {
                                var doc = docManager.ActiveDoc<TextureGenDocument>();
                                if (doc != null)
                                {
                                    doc.Viewport.CustomModel.ModelFile = new Uri(dlg.FileName);
                                    if (doc.Viewport.CustomModel.ModelData != null)
                                        doc.Viewport.Primitive = Graphics.MeshPrimitiveType.Custom;
                                    else
                                        doc.Viewport.Primitive = Graphics.MeshPrimitiveType.Sphere;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ErrorHandler.inst().Error(ex);
                        }
                    };

                // BEGIN LIGHTING

                    CheckBox animLight = new CheckBox() { Content = "Animate Light", ToolTip = "Scene light will oscillate back and forth" };
                    animLight.SetBinding(CheckBox.IsCheckedProperty, new Binding("ActiveDocument.Viewport.AnimateLight") { Source = docManager });
                    stack.Children.Add(animLight);

                    CheckBox animLowLight = new CheckBox() { Content = "Low Altitude Light", ToolTip = "Light will move at a lower altitude, ideal for models with less rounded tops" };
                    animLowLight.SetBinding(CheckBox.IsCheckedProperty, new Binding("ActiveDocument.Viewport.LowLight") { Source = docManager });
                    stack.Children.Add(animLowLight);

                    stack.Children.Add(new Label { Content = "IBL Cube", HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(2) });
                    ComboBox iblCombo = new ComboBox();
                    iblCombo.Items.Add("Desert Day");
                    iblCombo.Items.Add("Forest");
                    iblCombo.Items.Add("City Night");
                    iblCombo.SetBinding(ComboBox.SelectedIndexProperty, new Binding("ActiveDocument.Viewport.SkyBoxIndex") { Source = docManager });
                    stack.Children.Add(iblCombo);

                // END LIGHTING

                    Label tiling = new Label { Content = "Tiling", HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(2) };
                    Grid tileGrid = new Grid();
                    tileGrid.RowDefinitions.Add(new RowDefinition());
                    tileGrid.RowDefinitions.Add(new RowDefinition());
                    tileGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                    tileGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    var ulbl = new Label { Content = "U", Margin = new Thickness(5), VerticalAlignment = VerticalAlignment.Center };
                    Grid.SetColumn(ulbl, 0);
                    var vlbl = new Label { Content = "V", Margin = new Thickness(5), VerticalAlignment = VerticalAlignment.Center };
                    Grid.SetColumn(vlbl, 0);
                    Grid.SetRow(vlbl, 1);

                    TextBox uTileTxt = new TextBox();
                    Grid.SetColumn(uTileTxt, 1);
                    TextBox vTileTxt = new TextBox();
                    Grid.SetColumn(vTileTxt, 1);
                    Grid.SetRow(vTileTxt, 1);

                    uTileTxt.SetBinding(TextBox.TextProperty, new Binding("ActiveDocument.UTiling") { Source = docManager });
                    vTileTxt.SetBinding(TextBox.TextProperty, new Binding("ActiveDocument.VTiling") { Source = docManager });

                    tileGrid.Children.Add(ulbl);
                    tileGrid.Children.Add(vlbl);
                    tileGrid.Children.Add(uTileTxt);
                    tileGrid.Children.Add(vTileTxt);

                    stack.Children.Add(tiling);
                    stack.Children.Add(tileGrid);

                    stack.Children.Add(new Label { Content = "Shader", HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(2), ToolTip = "Selects the shading method to use, such as Rough-Metal or Glossy-Specular" });
                    ComboBox shaderCombo = new ComboBox();
                    shaderCombo.Items.Add("Rough - Metal");
                    shaderCombo.Items.Add("Rough - Specular");
                    shaderCombo.Items.Add("Glossy - Metal");
                    shaderCombo.Items.Add("Glossy - Specular");
                    shaderCombo.SetBinding(ComboBox.SelectedIndexProperty, new Binding("ActiveDocument.Viewport.ShaderIndex") { Source = docManager });
                    stack.Children.Add(shaderCombo);

                    CheckBox useHeightCheck = new CheckBox() { Content = "Use Displacement", ToolTip = "Displacement will be rendered, this is slow on low-spec PCs" };
                    useHeightCheck.SetBinding(CheckBox.IsCheckedProperty, new Binding("ActiveDocument.Viewport.UseHeight") { Source = docManager });
                    stack.Children.Add(useHeightCheck);

                // Debug and troubleshooting
                    stack.Children.Add(new Label { Content = "Troubleshooting", HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(2) });
                    CheckBox showTangets = new CheckBox { Content = "Show Tangent Frame", ToolTip = "Normal, tangent, and bitangent frames will be drawn" };
                    showTangets.SetBinding(CheckBox.IsCheckedProperty, new Binding("ActiveDocument.DebugTangents") { Source = docManager });
                    stack.Children.Add(showTangets);

                    CheckBox flipCulling = new CheckBox { Content = "Flip Culling", ToolTip = "CCW triangles will be culled instead of CW triangles" };
                    flipCulling.SetBinding(CheckBox.IsCheckedProperty, new Binding("ActiveDocument.FlipCulling") { Source = docManager });
                    stack.Children.Add(flipCulling);

                    CheckBox drawGrid = new CheckBox { Content = "Draw Grid", ToolTip = "Toggles the display of the viewport grid" };
                    drawGrid.SetBinding(CheckBox.IsCheckedProperty, new Binding("ActiveDocument.DrawGrid") { Source = docManager });
                    stack.Children.Add(drawGrid);

                    popup.ShowAtMouse(false, viewSwitch);
                };
            }

            previewViewport_ = new Graphics.TexGraph.TexGraphViewport(this, sharedScene_);

            // Restore our shading index
            int shaderIdx = 0;
            if (int.TryParse(DataRoot.GetCustomData("shading", "0"), out shaderIdx))
                previewViewport_.ShaderIndex = shaderIdx;

            paint2D_ = new Graphics.Paint.PaintScene2D(sharedScene_);
            sharedScene_.ActiveViewport = previewViewport_;
            Controls.LeftPanelControl = sharedLeftSide_;

            this.Selection.Selected.CollectionChanged += (doc, selSet) =>
            {
                if (this.Selection.Selected.Count == 1 && Selection.MostRecentlySelected.GetType() == typeof(TexGen.Paint2DNode))
                    sharedScene_.ActiveViewport = paint2D_;
                else
                    sharedScene_.ActiveViewport = previewViewport_;
            };

            graphControl_ = new TextureGenGraphControl(this, DataRoot);
            graphControl_.SetBinding(SprueKit.Controls.GraphControl.TitleProperty, new Binding("DocumentName") { Source = this });
            graphControl_.BackingGraph = DataRoot;
            Controls.ContentControl = graphControl_;

            graphControl_.Loaded += (o, e) => { graphControl_.RebuildNodes(); /*graphControl_.RebuildConnectors();*/ graphControl_.RebuildBoxes(); };
            graphControl_.canvas.PreviewMouseRightButtonDown += GraphControl__PreviewMouseRightButtonDown;
            graphControl_.canvas.PreviewMouseRightButtonUp += GraphControl__PreviewMouseRightButtonUp;
            graphControl_.ConnectionMade += OnConnectionMade;
            graphControl_.ConnectionDeleted += OnConnectionDeleted;
            DataRoot.ConnectivityChanged += OnGraphConnectivityChange;
            DataRoot.NodesChanged += OnGraphNodesChange;
            DataRoot.Nodes.CollectionChanged += Nodes_CollectionChanged;
            DataRoot.Connections.CollectionChanged += Connections_CollectionChanged;
            DataRoot.Boxes.CollectionChanged += Boxes_CollectionChanged;
            DataRoot.Routes.CollectionChanged += (o, e) => { graphControl_.RebuildConnectors(); IsDirty = true; };
            DataRoot.Routes.RecordChanged += (o, e) => { graphControl_.RebuildConnectors(); IsDirty = true; };
            Tracker_UndoRedo.Track(DataRoot);
            Tracker_UndoRedo.Changed += Tracker_UndoRedo_Changed;
            UndoRedo.UndoRedoActionPerformed += UndoRedo_UndoRedoActionPerformed;

            DocumentCommands = new Commands.CommandInfo[]
            {
                new Commands.CommandInfo { Name = "Save", Action = (doc, obj) => {
                    this.Save();
                }, ToolTip = "Save the current texture graph", Icon = WPFExt.GetEmbeddedImage("Images/save_white.png") },
                new Commands.CommandInfo { Name = "Export",  Action = (doc, obj) => {
                    this.Export();
                }, ToolTip = "Export the texture graph to raster", Icon = WPFExt.GetEmbeddedImage("Images/cabinet_white.png") },
                SprueKit.ShortCutExt.GetCommand("History", "Undo"),
                SprueKit.ShortCutExt.GetCommand("History", "Redo"),
            };
        }

        public override void OnActivate()
        {
            base.OnActivate();
            if (Selection.Selected.Count == 1 && Selection.MostRecentlySelected.GetType() == typeof(TexGen.Paint2DNode))
                sharedScene_.ActiveViewport = paint2D_;
            else
                sharedScene_.ActiveViewport = previewViewport_;
        }

        public override bool WriteFile(Uri path)
        {
            if (path == null)
                return false;

            string filePath = path.AbsolutePath;
            if (System.IO.Path.GetExtension(filePath).Equals(".txml"))
            {
                System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                try
                {
                    SerializationContext ctx = new SerializationContext(new Uri(System.IO.Path.GetDirectoryName(filePath)));

                    var root = doc.CreateElement("texture_graph");
                    doc.AppendChild(root);

                    DataRoot.CustomData["shading"] = Viewport.ShaderIndex.ToString();

                    DataRoot.Serialize(ctx, root);
                    doc.Save(filePath);
                    return true;
                }
                catch (Exception ex)
                {
                    ErrorHandler.inst().Error(ex);
                }
            }
            else if (System.IO.Path.GetExtension(filePath).Equals(".texg"))
            {
                try
                {
                    SerializationContext ctx = new SerializationContext(new Uri(System.IO.Path.GetDirectoryName(filePath)));

                    using (System.IO.FileStream stream = new System.IO.FileStream(filePath, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write))
                    using (System.IO.BinaryWriter writer = new System.IO.BinaryWriter(stream))
                        DataRoot.Serialize(ctx, writer);
                    return true;
                }
                catch (Exception ex)
                {
                    ErrorHandler.inst().Error(ex);
                }
            }
                return false;
            }

        public override void Export()
        {
            System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();
            dlg.Filter = "Portable Network Graphics (*.png)|*.png|Truevision TGA (*.tga)|*.tga|Direct Draw Surface (*.dds)|*.dds|JPEG/JPG (*.jpg)|*.jpg|16-bit HDR (*.hdr)|*.hdr";
            dlg.FilterIndex = 0;
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string baseName = System.IO.Path.GetFileNameWithoutExtension(dlg.FileName);
                string outputPath = System.IO.Path.GetDirectoryName(dlg.FileName);
                Parago.Windows.ProgressDialogResult result = Parago.Windows.ProgressDialog.Execute(null, "Exporting", "Exporting texture images...", (a, b) =>
                    {
                        Data.Graph.Graph clone = DataRoot.Clone();
                        if (clone != null)
                        {
                            int exportCt = 0;
                            foreach (var node in clone.Nodes)
                            {
                                if (node is TexGen.TextureOutputNode)
                                {
                                    bool hasWork = false;
                                    foreach (var socket in node.InputSockets)
                                        if (socket.HasConnections())
                                            hasWork = true;

                                    if (!hasWork)
                                        continue;
                                    ++exportCt;
                                    Parago.Windows.ProgressDialog.Report(a, string.Format("Processing {0}...", node.Name));
                                    var outputNode = node as TexGen.TextureOutputNode;
                                    clone.Prime(new Vector2(outputNode.TargetSize.X, outputNode.TargetSize.Y));
                                    var bmp = outputNode.GeneratePreview(outputNode.TargetSize.X, outputNode.TargetSize.Y, (float f) =>
                                    {
                                        Parago.Windows.ProgressDialog.Report(a, (int)(f * 100), string.Format("Processing {0}...", node.Name));
                                        return true;
                                    });
                                    string outSuffix = UserData.inst().TextureGraphSettings.GetSuffix(outputNode.OutputChannel);
                                    switch (dlg.FilterIndex)
                                    {
                                        case 1: // PNG
                                            bmp.Save(System.IO.Path.Combine(outputPath, string.Format("{0}{1}.png", baseName, outSuffix)), System.Drawing.Imaging.ImageFormat.Png);
                                            break;
                                        case 2: // TGA
                                            SprueBindings.ImageData.Save(System.IO.Path.Combine(outputPath, string.Format("{0}{1}.tga", baseName, outSuffix)), bmp.ToImageData(), ErrorHandler.inst());
                                            break;
                                        case 3: // DDS
                                            SprueBindings.ImageData.Save(System.IO.Path.Combine(outputPath, string.Format("{0}{1}.dds", baseName, outSuffix)), bmp.ToImageData(), ErrorHandler.inst());
                                            break;
                                        case 4: // JPG
                                            bmp.Save(System.IO.Path.Combine(outputPath, string.Format("{0}{1}.jpg", baseName, outSuffix)), System.Drawing.Imaging.ImageFormat.Jpeg);
                                            break;
                                        case 5: // HDR
                                            SprueBindings.ImageData.Save(System.IO.Path.Combine(outputPath, string.Format("{0}{1}.hdr", baseName, outSuffix)), bmp.ToImageData(), ErrorHandler.inst());
                                            break;
                                    }
                                    ErrorHandler.inst().Info(string.Format("Exported texture {0}", string.Format("{0}{1}", baseName, outSuffix)));
                                }
                            }

                            if (exportCt == 0)
                                ErrorHandler.inst().Error("Unable to export texture graph, there were no output nodes with active connections");
                            else
                                ErrorHandler.inst().Info("Finished exporting texture graph");

                            Parago.Windows.ProgressDialog.Report(a, "Generating report...");
                            var rpt = new Reports.TextureGraphReport(
                                new Reports.TextureReportSettings {
                                    ReportTitle = baseName,
                                    ReportStyle = Reports.TextureReportType.Details
                                }, 
                                clone, System.IO.Path.Combine(outputPath, string.Format("{0}.html", baseName)));
                        }
                        else
                        {
                            ErrorHandler.inst().Error("Failed to export textures, unable to clone graph for isolated processing");
                        }
                    }
                );
            }
        }

        static char[] Splitter = new char[] { ',' };
        public static void DoExport(Data.Graph.Graph graph, string baseName, string outputPath, string targetName, int filterIndex, bool hasCapture)
        {
            int exportCt = 0;
            string[] targetNames = null;
            if (!string.IsNullOrWhiteSpace(targetName))
                targetNames = targetName.Split(Splitter, StringSplitOptions.RemoveEmptyEntries);
            foreach (var node in graph.Nodes)
            {
                if (node is TexGen.TextureOutputNode)
                {
                    bool hasWork = false;
                    foreach (var socket in node.InputSockets)
                        if (socket.HasConnections())
                            hasWork = true;

                    if (targetNames != null)
                    {
                        bool found = false;
                        foreach (string str in targetNames)
                            if (node.Name.ToLowerInvariant().Equals(targetName))
                                found = true;
                        if (!found)
                            continue;
                    }

                    if (!hasWork)
                        continue;
                    ++exportCt;
                    var outputNode = node as TexGen.TextureOutputNode;
                    Console.Write(string.Format("Generating {0}", outputNode.Name));
                    graph.Prime(new Vector2(outputNode.TargetSize.X, outputNode.TargetSize.Y));
                    var bmp = outputNode.GeneratePreview(outputNode.TargetSize.X, outputNode.TargetSize.Y, (float f) =>
                    {
                        if (!hasCapture)
                            Console.Write(string.Format("\rGenerating {0} ... {1}%", outputNode.Name, (int)f * 100));
                        return true;
                    });
                    string outSuffix = UserData.inst().TextureGraphSettings.GetSuffix(outputNode.OutputChannel);
                    switch (filterIndex)
                    {
                        case 1: // PNG
                            bmp.Save(System.IO.Path.Combine(outputPath, string.Format("{0}{1}.png", baseName, outSuffix)), System.Drawing.Imaging.ImageFormat.Png);
                            break;
                        case 2: // TGA
                            SprueBindings.ImageData.Save(System.IO.Path.Combine(outputPath, string.Format("{0}{1}.tga", baseName, outSuffix)), bmp.ToImageData(), ErrorHandler.inst());
                            break;
                        case 3: // DDS
                            SprueBindings.ImageData.Save(System.IO.Path.Combine(outputPath, string.Format("{0}{1}.dds", baseName, outSuffix)), bmp.ToImageData(), ErrorHandler.inst());
                            break;
                        case 4: // JPG
                            bmp.Save(System.IO.Path.Combine(outputPath, string.Format("{0}{1}.jpg", baseName, outSuffix)), System.Drawing.Imaging.ImageFormat.Jpeg);
                            break;
                        case 5: // HDR
                            SprueBindings.ImageData.Save(System.IO.Path.Combine(outputPath, string.Format("{0}{1}.hdr", baseName, outSuffix)), bmp.ToImageData(), ErrorHandler.inst());
                            break;
                    }
                    if (!hasCapture)
                        Console.WriteLine(string.Format("\rExported texture {0}", string.Format("{0}{1}", baseName, outSuffix)));
                    else
                        Console.WriteLine(string.Format("Exported texture {0}", string.Format("{0}{1}", baseName, outSuffix)));
                }
            }

            if (exportCt == 0)
                Console.WriteLine("ERROR: Unable to export texture graph, there were no output nodes with active connections");
            else
                Console.WriteLine("FINISHED");
        }

        public override void Dispose()
        {
            base.Dispose();
        }

#region Graph Control Events

        void OnConnectionMade(object sender, Controls.GraphControl.SocketEventArgs args)
        {
            if (DataRoot.Connect(args.From.Tag as Graph.GraphSocket, args.To.Tag as Graph.GraphSocket))
                args.Handled = true;
        }

        void OnConnectionDeleted(object sender, Controls.GraphControl.SocketEventArgs args)
        {
            if (args.From == null || args.To == null)
                args.Handled = true;
            else if (DataRoot.Disconnect(args.From.Tag as Graph.GraphSocket, args.To.Tag as Graph.GraphSocket))
                args.Handled = true;
        }

        public void InvalidateNode(TexGenNode nd, bool trace = true)
        {
            if (nd == null)
                return;

            if (trace)
            {
                nd.TraceDownstream((n, i) =>
                {
                    if (n is Data.TexGen.WarpOut)
                    {
                        string myLower = ((Data.TexGen.WarpOut)n).WarpKey.ToLower();
                        var foundNode = DataRoot.Nodes.Where(other => other is WarpIn && ((WarpIn)other).WarpKey.ToLowerInvariant().Equals(myLower));
                        if (foundNode != null)
                        {
                            foreach (var found in foundNode)
                                InvalidateNode(found as Data.TexGen.TexGenNode, true);
                        }
                    }
                    else if (n is Data.TexGen.SampleControl || n is Data.TexGen.WarpIn)
                    {
                        // do nothing
                    }
                    else
                        App.EnqueueOptimal(new TexGenRegenerateTask(previewViewport_, n.Graph, ((TexGenNode)n)));
                });
            }
            else
            {
                if (nd is WarpingNode || nd is SampleControl)
                {
                    // do nothing
                }
                else
                    App.EnqueueOptimal(new TexGenRegenerateTask(previewViewport_, DataRoot, nd));
            }
        }

        void OnGraphConnectivityChange(object sender, Data.Graph.ConnectivityChange e)
        {
            if (e != null)
            {
            }
            graphControl_.RebuildConnectors();
            IsDirty = true;
        }

        void OnGraphNodesChange(object sender, EventArgs e)
        {
            graphControl_.RebuildNodes();
            IsDirty = true;
        }

        private void Nodes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnGraphNodesChange(DataRoot, null);
            IsDirty = true;
        }

        void Connections_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Graph.GraphConnection con in e.NewItems)
                    if (con.ToNode != null)
                        InvalidateNode((TexGenNode)con.ToNode);
            }
            if (e.OldItems != null)
            {
                foreach (Graph.GraphConnection con in e.OldItems)
                    if (con.ToNode != null)
                        InvalidateNode((TexGenNode)con.ToNode);
            }
            IsDirty = true;
            //OnGraphConnectivityChange(DataRoot, null);
        }

        void Boxes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            graphControl_.RebuildBoxes();
        }

        private void UndoRedo_UndoRedoActionPerformed(Commands.UndoStack stack)
        {
            graphControl_.RebuildConnectors();
        }

        private void Tracker_UndoRedo_Changed(Notify.Tracker tracker, object who, string name)
        {
            var undoRedoStack = Commands.MacroCommandBlock.GetUndoStorage(UndoRedo);
            bool dontWork = false;
            IsDirty = true;
            if (who is Notify.PropertyChangedTrackedObject)
            {
                var obj = who as Notify.PropertyChangedTrackedObject;
                var pi = obj.Tracked.GetType().GetProperty(name);

                // Check for exception cases where we don't want to invalidate
                if (Attribute.GetCustomAttribute(pi, typeof(Notify.DontSignalWorkAttribute)) != null)
                    dontWork = true;
                var typeAttr = pi.GetCustomAttribute<PropertyData.VisualConsequenceAttribute>();
                if (typeAttr != null && typeAttr.Stage == PropertyData.VisualStage.None)
                    dontWork = true;

                undoRedoStack.Add(new Commands.BasicPropertyCmd(name, obj.Tracked, pi.GetValue(obj.Snapshot), pi.GetValue(obj.Tracked)));

                if (obj.Tracked is TexGenNode && !dontWork)
                    InvalidateNode((TexGenNode)obj.Tracked);
                else if (obj.Owner != null && obj.Owner.Tracked is TexGenNode && !dontWork)
                    InvalidateNode((TexGenNode)obj.Owner.Tracked);
            }
            else if (who is Notify.CollectionChangedTrackObject)
            {
                var obj = who as Notify.CollectionChangedTrackObject;
                if (obj.RecentAction == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    foreach (var v in obj.RecentArgs.NewItems)
                        undoRedoStack.Add(new Commands.CollectionAddRemoveCmd(true, obj.Tracked as System.Collections.IList, obj.RecentArgs.NewStartingIndex, v, obj.TrackedOwner));
                }
                else if (obj.RecentAction == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                {
                    foreach (var v in obj.RecentArgs.OldItems)
                        undoRedoStack.Add(new Commands.CollectionAddRemoveCmd(false, obj.Tracked as System.Collections.IList, obj.RecentArgs.OldStartingIndex, v, obj.TrackedOwner));
                }
                else if (obj.RecentAction == System.Collections.Specialized.NotifyCollectionChangedAction.Move)
                {
                    //???
                }
            }
            else
                undoRedoStack.Add(new Commands.SpoofUndoRedoCmd(name));
        }

#endregion

#region Context Menu for node creation

        public class NodeGroup
        {
            public string Name;
            public Type[] Types;
        }


        public static Type[] DemoRestricted = new Type[]
        {
// This #if is for 100% assurance that even if DemoRestricted checking code is there,
// that it will not fuck with real builds
#if IS_DEMO
            typeof(TexGen.ChainGenerator),
            typeof(TexGen.ChainMailGenerator),
            typeof(TexGen.GaborNoise),
            typeof(TexGen.TextGenerator),
            typeof(TexGen.UberNoise),
            typeof(TexGen.WeaveGenerator),

            typeof(TexGen.GradientLookupTable),
            typeof(TexGen.LevelsFilterNode),
            typeof(TexGen.LookupTableRemapNode),

            typeof(TexGen.FromNormalizedRangeNode),
            typeof(TexGen.ToNormalizedRangeNode),

            typeof(TexGen.AnisotropicBlur), typeof(TexGen.BlurModifier),
            typeof(TexGen.CartesianToPolarModifier), typeof(TexGen.PolarToCartesianModifier),

            typeof(TexGen.DominantPlaneBaker), typeof(TexGen.FacetBaker),
            typeof(TexGen.CylindricalTextureBaker), typeof(TexGen.VolumetricFBM),
            typeof(TexGen.VolumetricPerlin),
#endif
        };

        // 108 nodes
        public static NodeGroup[] NodeGroups = new NodeGroup[]
        {
            new NodeGroup { Name = "Values", Types = new Type[] { // 9 nodes
                typeof(TexGen.FloatNode), typeof(TexGen.ColorNode),
                typeof(TexGen.TextureNode), typeof(TexGen.IDMapGenerator),
                typeof(TexGen.SVGNode), typeof(TexGen.ModelNode),
                typeof(TexGen.PBREnforcer), typeof(TexGen.CombineNode),
                typeof(TexGen.SplitNode), typeof(TexGen.Paint2DNode),
                
            } },
            new NodeGroup { Name = "Basic Math", Types = new Type[] { // 4 nodes
                typeof(TexGen.AddNode), typeof(TexGen.SubtractNode),
                typeof(TexGen.MultiplyNode), typeof(TexGen.DivideNode),
            } },
            new NodeGroup { Name = "Generators", Types = new Type[] { // 19 nodes
                typeof(TexGen.BricksGenerator), typeof(TexGen.ChainGenerator),
                typeof(TexGen.ChainMailGenerator), typeof(TexGen.CheckerGenerator),
                typeof(TexGen.FBMNoiseGenerator), typeof(TexGen.GaborNoise),
                typeof(TexGen.GradientGenerator), typeof(TexGen.PerlinNoiseGenerator),
                typeof(TexGen.PolygonGenerator),
                typeof(TexGen.RowsGenerator), typeof(TexGen.ScalesGenerator),
                typeof(TexGen.ScratchesGenerator), typeof(TexGen.TextGenerator),
                typeof(TexGen.TextureBombGenerator), typeof(TexGen.TextureFunction2D),
                typeof(TexGen.UberNoise), typeof(TexGen.VoronoiGenerator),
                typeof(TexGen.WeaveGenerator), typeof(TexGen.WhiteNoiseGenerator)
            } },
            new NodeGroup { Name = "Color", Types = new Type[] { // 15 nodes
                typeof(TexGen.AverageRGBNode), typeof(TexGen.BinarizeNode),
                typeof(TexGen.ContrastNode), typeof(TexGen.BrightnessNode),
                typeof(TexGen.BrightnessRGBNode), typeof(TexGen.CurveTextureModifier),
                typeof(TexGen.GradientRampTextureModifier), typeof(TexGen.GradientLookupTable),
                typeof(TexGen.LevelsFilterNode), typeof(TexGen.LookupTableRemapNode),
                typeof(TexGen.ReplaceColorModifier), typeof(TexGen.SaturationFilterNode),
                typeof(TexGen.SelectColorModifier),
                typeof(TexGen.FromGammaNode), typeof(TexGen.ToGammaNode),
            } },
            new NodeGroup { Name = "Math", Types = new Type[] { // 14 nodes
                typeof(TexGen.CosNode), typeof(TexGen.ACosNode),
                typeof(TexGen.SinNode), typeof(TexGen.ASinNode),
                typeof(TexGen.TanNode), typeof(TexGen.ATanNode),
                typeof(TexGen.Clamp01Node), typeof(TexGen.ExpNode),
                typeof(TexGen.MaxRGBNode), typeof(TexGen.MinRGBNode),
                typeof(TexGen.PowNode), typeof(TexGen.SqrtNode),
                typeof(TexGen.FromNormalizedRangeNode), typeof(TexGen.ToNormalizedRangeNode), 
            } },
            new NodeGroup { Name = "Filters", Types = new Type[] { // 25 nodes
                typeof(TexGen.AnisotropicBlur), typeof(TexGen.BlurModifier),
                typeof(TexGen.ClipTextureModifier), typeof(TexGen.ConvolutionFilter),
                typeof(TexGen.DivModifier), typeof(TexGen.EmbossModifier),
                typeof(TexGen.ErosionModifier), typeof(TexGen.FillEmpty),
                typeof(TexGen.InvertTextureModifier), 
                typeof(TexGen.OctaveSum),
                typeof(TexGen.HatchFilter),
                typeof(TexGen.PosterizeModifier),
                typeof(TexGen.SharpenFilter), typeof(TexGen.SobelTextureModifier),
                typeof(TexGen.SolarizeTextureModifier), typeof(TexGen.StreakModifier),
                typeof(TexGen.TransformModifier), typeof(TexGen.SimpleTransformModifier),
                typeof(TexGen.SplatMapNode), typeof(TexGen.TrimModifier),
                typeof(TexGen.TileModifier), typeof(TexGen.WarpModifier),
                typeof(TexGen.WaveFilter),
                typeof(TexGen.CartesianToPolarModifier), typeof(TexGen.PolarToCartesianModifier),
            } },
            new NodeGroup { Name = "Mesh Bakers", Types = new Type[] // 15 nodes
            {
                typeof(TexGen.AmbientOcclusionBaker), typeof(TexGen.CurvatureBaker),
                typeof(TexGen.DominantPlaneBaker), typeof(TexGen.FacetBaker),
                typeof(TexGen.FauxLightBaker), typeof(TexGen.ModelSpaceGradientBaker),
                typeof(TexGen.ModelSpaceNormalBaker), typeof(TexGen.ModelSpacePositionBaker),
                typeof(TexGen.ThicknessBaker), typeof(TexGen.TriplanarTextureBaker),
                typeof(TexGen.CylindricalTextureBaker), typeof(TexGen.VolumetricFBM),
                typeof(TexGen.VolumetricPerlin), typeof(TexGen.VolumetricVoronoi),
                typeof(TexGen.SimpleReliefNode)
            } },
            new NodeGroup { Name = "Normal Maps", Types = new Type[] // 5 nodes
            {
                typeof(TexGen.NormalMapDeviation), typeof(TexGen.NormalMapNormalize),
                typeof(TexGen.NormalPower), typeof(TexGen.ToNormalMap),
                typeof(TexGen.RotateNormals),
            } },
            new NodeGroup { Name ="Graph Control", Types = new Type[] // 4 nodes
            {
                typeof(TexGen.WarpOut), typeof(TexGen.WarpIn),
                typeof(TexGen.SampleControl), typeof(TexGen.TextureOutputNode)
            } }
        };

        bool hadRButton_ = false;
        private void GraphControl__PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            hadRButton_ = true;
        }

        System.Windows.Point ctxPoint_ = new System.Windows.Point();
        private void GraphControl__PreviewMouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ctxPoint_ = e.GetPosition(graphControl_.canvas);
            if (graphControl_.canvasCtxMenu == null)
                graphControl_.canvasCtxMenu = new ContextMenu();
            if (!graphControl_.canvasCtxMenu.Items.IsEmpty)
                return;

            if (!hadRButton_)
            {
                e.Handled = true;
                graphControl_.canvasCtxMenu = null;
                return;
            }
            hadRButton_ = false;

            MenuItem pasteItem = new MenuItem { Header = "Paste" };
            pasteItem.Click += (oo, evtArgs) => {
                graphControl_.Paste(ctxPoint_);
            };
            graphControl_.canvasCtxMenu.Items.Add(pasteItem);

            graphControl_.canvasCtxMenu.Items.Add(new Separator());

            MenuItem blendItem = new MenuItem { Header = "Blend" };
            graphControl_.canvasCtxMenu.Items.Add(blendItem);
            blendItem.Click += (o, eargs) =>
            {
                var createPt = ctxPoint_;
                Data.Graph.GraphNode nd = new BlendNode();
                if (nd != null)
                {
                    nd.Graph = DataRoot;
                    nd.VisualX = createPt.X - graphControl_.nodeTranslation.X;
                    nd.VisualY = createPt.Y - graphControl_.nodeTranslation.Y;
                    DataRoot.AddNode(nd);
                    nd.Construct();
                    InvalidateNode((TexGenNode)nd, false); // new node, trace is a waste
                }
            };

            foreach (var grp in NodeGroups)
            {
                MenuItem grpItem = new MenuItem { Header = grp.Name };
                foreach (var type in grp.Types)
                {
                    Label lbl = new Label { Content = type.Name.Replace("Node", "").SplitCamelCase() };

                    bool isDemoBlocked = false;
                    isDemoBlocked = TextureGenDocument.DemoRestricted.Contains(type);
                    if (isDemoBlocked)
                        lbl.Foreground = new SolidColorBrush(Colors.Red);

                    MenuItem typeItem = new MenuItem {
                        Header = lbl,
                        Icon = new Image { Source = SprueKit.Controls.GraphParts.TextureGraphNode.IconConverter.GetIcon(type), MaxWidth = 16, MaxHeight=16 },
                        IsEnabled = !isDemoBlocked
                    };
                    typeItem.Click += (o, args) =>
                    {
                        var createPt = ctxPoint_;
                        Data.Graph.GraphNode nd = Activator.CreateInstance(type) as Data.Graph.GraphNode;
                        if (nd != null)
                        {
                            if (nd is Data.TexGen.TextureOutputNode)
                            {
                                var existing = DataRoot.Nodes.Where(n => n is Data.TexGen.TextureOutputNode);
                                int curIdx = 0;
                                var values = Enum.GetValues(typeof(TextureChannel));
                                for (; curIdx < values.Length; ++curIdx)
                                {
                                    bool good = true;
                                    foreach (var n in existing)
                                    {
                                        if ((int)((Data.TexGen.TextureOutputNode)n).OutputChannel == curIdx)
                                        {
                                            good = false;
                                            break;
                                        }
                                    }
                                    if (good)
                                        break;
                                }

                                if (curIdx < values.Length)
                                    ((Data.TexGen.TextureOutputNode)nd).OutputChannel = (TextureChannel)curIdx;
                            }
                            
                            nd.Graph = DataRoot;
                            nd.Construct();
                            nd.VisualX = createPt.X - graphControl_.nodeTranslation.X;
                            nd.VisualY = createPt.Y - graphControl_.nodeTranslation.Y;
                            DataRoot.AddNode(nd);
                            nd.UpdateSocketIDs();
                            InvalidateNode((TexGenNode)nd, false); // new node, trace is a waste
                        }
                    };
                    grpItem.Items.Add(typeItem);
                }
                graphControl_.canvasCtxMenu.Items.Add(grpItem);
            }

            MenuItem newGrp = new MenuItem { Header = "Create comment box" };
            newGrp.Click += (o, args) =>
            {
                Random r = new Random();
                var createPt = ctxPoint_;
                Graph.GraphBox box = new Graph.GraphBox {
                    VisualX = createPt.X - graphControl_.nodeTranslation.X,
                    VisualY = createPt.Y - graphControl_.nodeTranslation.Y,
                    Name = "New comment", BoxColor = new Microsoft.Xna.Framework.Color(r.Next(80,255), r.Next(80, 255), r.Next(80, 255),255) };
                DataRoot.Boxes.Add(box);
            };
            graphControl_.canvasCtxMenu.Items.Add(newGrp);
        }

#endregion
    }

    public class TexGenDocumentRecord : DocumentRecord
    {
        public override string DocumentName { get { return "Texture Graph"; } }

        public override string OpenFileMask { get { return "Texture Graph (*.txml; *.texg)|*.txml;*.texg"; } }

        public override Document CreateFromTemplate(Uri templateUri)
        {
            return new TextureGenDocument(templateUri, true);
        }

        public override Document CreateNewDocument()
        {
            return new TextureGenDocument();
        }

        public override Document OpenDocument(Uri documentUri)
        {
            return new TextureGenDocument(documentUri, false);
        }

        public override KeyValuePair<string, string>[] GetSignificantReports()
        {
            return new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string,string>("Details Report", "DETAILS"),
                new KeyValuePair<string,string>("Comparison Report", "COMPARISON")
            };
        }

        public override void DoReport(string value)
        {
            if (value.Equals("DETAILS"))
            {
                var dlg = new System.Windows.Forms.OpenFileDialog();
                dlg.Filter = this.OpenFileMask;

                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {

                    var rpt = new Data.Reports.TextureReportSettings
                    {
                        ReportTitle = "New Details Report",
                        ReportStyle = Reports.TextureReportType.Details
                    };
                    foreach (var str in dlg.FileNames)
                        rpt.FileList.Paths.Add(str);

                    ((App)App.Current).Reports.Reports.Add(rpt);
                    App.Navigate(new Uri("/Pages/ReportsScreen.xaml#New_Details_Report", UriKind.RelativeOrAbsolute));
                }
            }
            else if (value.Equals("COMPARISON"))
            {
                System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var rpt = new Data.Reports.TextureReportSettings
                    {
                        ReportTitle = "New Comparison Report",
                        ReportStyle = Reports.TextureReportType.VisualOverview
                    };
                    rpt.FolderList.Paths.Add(dlg.SelectedPath);
                    App.Navigate(new Uri("/Pages/ReportsScreen.xaml#New_Comparison_Report", UriKind.RelativeOrAbsolute));
                }
            }
        }
    }
}
