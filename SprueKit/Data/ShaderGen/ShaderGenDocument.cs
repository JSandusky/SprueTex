using GongSolutions.Wpf.DragDrop;
using SprueKit.Commands;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using SprueKit.Controls.GraphParts;

namespace SprueKit.Data.ShaderGen
{
    public class ShaderGenGraphControl : Controls.GraphControl, IQuickActionSource
    {
        Graph.Graph graph_;
        public ShaderGenGraphControl(Document doc, Graph.Graph g) : base(doc)
        {
            graph_ = g;
        }

        protected override Controls.GraphParts.GraphNode CreateNode(Graph.GraphNode forNode, bool isNew)
        {
            var ret = new ShaderGraphNode(this, forNode);
            ret.RenderTransform = nodeTranslation;
            return ret;
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
                //((ShaderGenNode)nd).RefreshPreview();
            }
        }

        public override void Drop(IDropInfo dropInfo)
        {
            //if (dropInfo.Data is TreeViewItem && ((TreeViewItem)dropInfo.Data).DataContext is Type)
            {
                //Type t = ((TreeViewItem)dropInfo.Data).DataContext as Type;
                //if (typeof(TexGenNode).IsAssignableFrom(t))
                //{
                //    Data.Graph.GraphNode nd = Activator.CreateInstance(t) as Data.Graph.GraphNode;
                //    if (nd != null)
                //    {
                //        nd.Graph = graph_;
                //        nd.Construct();
                //        nd.VisualX = dropInfo.DropPosition.X;
                //        nd.VisualY = dropInfo.DropPosition.Y;
                //
                //        if (nd is TexGen.TextureOutputNode)
                //        {
                //            ((TexGen.TextureOutputNode)nd).OutputChannel = (Data.TextureChannel)((TreeViewItem)dropInfo.Data).Tag;
                //            nd.Name = ((TexGen.TextureOutputNode)nd).OutputChannel.ToString();
                //        }
                //
                //        graph_.AddNode(nd);
                //        nd.UpdateSocketIDs();
                //        ((TexGenNode)nd).RefreshPreview();
                //    }
                //}
            }
        }

        public override void DragOver(IDropInfo dropInfo)
        {
            //if (dropInfo.Data is TreeViewItem && ((TreeViewItem)dropInfo.Data).DataContext is Type)
            //{
            //    Type t = ((TreeViewItem)dropInfo.Data).DataContext as Type;
            //    if (typeof(TexGenNode).IsAssignableFrom(t))
            //    {
            //        dropInfo.Effects = System.Windows.DragDropEffects.Copy;
            //        return;
            //    }
            //}
            base.DragOver(dropInfo);
        }

        static CommandInfo[] info_;
        public CommandInfo[] GetCommands()
        {
            if (info_ == null)
            {
                List<CommandInfo> infos = new List<CommandInfo>();
                foreach (var grp in ShaderGenDocument.NodeGroups)
                {
                    foreach (var node in grp.Types)
                    {
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

    public class ShaderGenDocument : Document
    {
        static Grid sharedLeftSide_;
        static Graphics.BaseScene sharedScene_;
        Graphics.TexGraph.TexGraphViewport previewViewport_;
        Controls.GraphControl graphControl_;
        public Graph.Graph DataRoot { get; set; }

        public Graphics.TexGraph.TexGraphViewport Viewport { get { return previewViewport_; } }

        public ShaderGenDocument()
        {
            DataRoot = new Graph.Graph();
            int stepSize = 130;
            int xPos = 225;
            
            foreach (var nd in DataRoot.Nodes)
            {
                string n = nd.Name;
                nd.Construct();
                nd.Name = n;
            }
            CommonConstruct(new Dictionary<string, object>());
        }

        public ShaderGenDocument(Uri filePath, bool isTemplate)
        {
            Dictionary<string, object> paramData = new Dictionary<string, object>();

            DataRoot = new Graph.Graph();
            string path = filePath.AbsolutePath;
            if (!isTemplate)
                FileURI = filePath;
            IsDirty = isTemplate;

            SerializationContext ctx = new SerializationContext(new Uri(System.IO.Path.GetDirectoryName(path)));
            if (path.EndsWith(".mxml"))
            {
                System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                doc.Load(filePath.AbsolutePath);
                var graphElem = doc.DocumentElement.SelectSingleNode("graph") as System.Xml.XmlElement;
                DataRoot.Deserialize(ctx, graphElem);
            }
            else if (path.EndsWith(".matg"))
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

            //??foreach (var node in DataRoot.Nodes)
            //??    ((TexGenNode)node).RefreshPreview();
            ErrorHandler.inst().Info(string.Format("Opened material graph {0}", filePath));
        }

        void CommonConstruct(Dictionary<string, object> parameters)
        {
            DataRoot.InitializeIDs();
            DocumentTypeName = "Material Graph";
            FileMask = "Material Graph XML (*.mxml)|*.txml|Material Graph Binary (*.matg)|*.matg";

            if (sharedLeftSide_ == null)
            {
                sharedLeftSide_ = new Grid();
                sharedLeftSide_.RowDefinitions.Add(new RowDefinition());
                sharedLeftSide_.RowDefinitions.Add(new RowDefinition() { Height = new System.Windows.GridLength(6) });
                sharedLeftSide_.RowDefinitions.Add(new RowDefinition());

                sharedScene_ = new Graphics.BaseScene();
                GridSplitter splitter = new GridSplitter();
                Grid.SetRow(splitter, 1);
                Grid subGrid = new Grid();
                subGrid.Children.Add(sharedScene_);
                sharedLeftSide_.Children.Add(subGrid);
                sharedLeftSide_.Children.Add(splitter);
                var nodeTree = new Controls.TexGen.TextureGenTree();
                Grid.SetRow(nodeTree, 2);
                sharedLeftSide_.Children.Add(nodeTree);

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

                    stack.Children.Add(new Label { Content = "Shape" });
                    var docManager = new IOCDependency<DocumentManager>().Object;
                    foreach (Graphics.MeshPrimitiveType enumValue in Enum.GetValues(typeof(Graphics.MeshPrimitiveType)))
                    {
                        ToggleButton btn = new ToggleButton { Content = enumValue.ToString() };
                        btn.SetBinding(ToggleButton.IsCheckedProperty, new Binding("ActiveDocument.Viewport.Primitive") { Source = docManager, Converter = new Controls.Converters.BoolValueCompareConverter(), ConverterParameter = enumValue, Mode = BindingMode.OneWay });
                        stack.Children.Add(btn);
                        btn.Click += (sender, eventData) =>
                        {
                            var doc = docManager.ActiveDoc<ShaderGenDocument>();
                            if (doc != null)
                                doc.Viewport.Primitive = enumValue;
                        };
                    }
                    CheckBox animLight = new CheckBox() { Content = "Animate Light" };
                    animLight.SetBinding(CheckBox.IsCheckedProperty, new Binding("ActiveDocument.Viewport.AnimateLight") { Source = docManager });
                    stack.Children.Add(animLight);

                    popup.ShowAtMouse(false, viewSwitch);
                };
            }
            previewViewport_ = new Graphics.TexGraph.TexGraphViewport(null, sharedScene_);
            sharedScene_.ActiveViewport = previewViewport_;
            Controls.LeftPanelControl = sharedLeftSide_;

            graphControl_ = new ShaderGenGraphControl(this, DataRoot);
            graphControl_.BackingGraph = DataRoot;
            Controls.ContentControl = graphControl_;

            graphControl_.Loaded += (o, e) => { graphControl_.SetCanvasTransform(1.0f, 0.0, 0.0); graphControl_.RebuildNodes(); graphControl_.RebuildConnectors(); };
            graphControl_.canvas.PreviewMouseRightButtonDown += GraphControl__PreviewMouseRightButtonDown;
            graphControl_.canvas.PreviewMouseRightButtonUp += GraphControl__PreviewMouseRightButtonUp;
            graphControl_.ConnectionMade += OnConnectionMade;
            graphControl_.ConnectionDeleted += OnConnectionDeleted;
            DataRoot.ConnectivityChanged += OnGraphConnectivityChange;
            DataRoot.NodesChanged += OnGraphNodesChange;
            DataRoot.Nodes.CollectionChanged += Nodes_CollectionChanged;
            DataRoot.Connections.CollectionChanged += Connections_CollectionChanged;
            Tracker_UndoRedo.Track(DataRoot);
            Tracker_UndoRedo.Changed += Tracker_UndoRedo_Changed;
            UndoRedo.UndoRedoActionPerformed += UndoRedo_UndoRedoActionPerformed;
        }

        public override void OnActivate()
        {
            base.OnActivate();
            sharedScene_.ActiveViewport = previewViewport_;
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

        void InvalidateNode(ShaderGenNode nd)
        {
            App.MainTaskThread.AddTask(new ShaderCompileTask(DataRoot));
        }

        void OnGraphConnectivityChange(object sender, Data.Graph.ConnectivityChange e)
        {
            if (e != null)
            {
                //if (e.From != null)
                //    ((TexGenNode)e.From.Node).RefreshPreview();
                if (e.To != null)
                    InvalidateNode((ShaderGenNode)e.To.Node);//((TexGenNode)e.To.Node).RefreshPreview();
            }
            graphControl_.RebuildConnectors();
        }

        void OnGraphNodesChange(object sender, EventArgs e)
        {
            graphControl_.RebuildNodes();
        }

        private void Nodes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnGraphNodesChange(DataRoot, null);
        }

        void Connections_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Graph.GraphConnection con in e.NewItems)
                    if (con.ToNode != null)
                        InvalidateNode((ShaderGenNode)con.ToNode);
            }
            if (e.OldItems != null)
            {
                foreach (Graph.GraphConnection con in e.OldItems)
                    if (con.ToNode != null)
                        InvalidateNode((ShaderGenNode)con.ToNode);
            }
            //OnGraphConnectivityChange(DataRoot, null);
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
                if (Attribute.GetCustomAttribute(pi, typeof(Notify.DontSignalWorkAttribute)) != null)
                    dontWork = true;
                undoRedoStack.Add(new Commands.BasicPropertyCmd(name, obj.Tracked, pi.GetValue(obj.Snapshot), pi.GetValue(obj.Tracked)));

                if (obj.Tracked is ShaderGenNode)
                    InvalidateNode((ShaderGenNode)obj.Tracked);
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

        // 68 nodes
        public static NodeGroup[] NodeGroups = new NodeGroup[]
        {
            new NodeGroup { Name = "Uniforms", Types = new Type[]
            {
                typeof(ShaderGen.FloatUniformNode), typeof(ShaderGen.Float2UniformNode),
                typeof(ShaderGen.Float3UniformNode), typeof(ShaderGen.Float4UniformNode),
                typeof(ShaderGen.MatrixUniformNode),
            } },
            new NodeGroup { Name = "Constants", Types = new Type[]
            {
                typeof(ShaderGen.PiNode), typeof(ShaderGen.TauNode),
                typeof(ShaderGen.PhiNode), typeof(ShaderGen.Root2Node),
                typeof(ShaderGen.EulersConstantNode), typeof(ShaderGen.IntConstant),
                typeof(ShaderGen.FloatConstant), typeof(ShaderGen.Float2Constant),
                typeof(ShaderGen.Float3Constant), typeof(ShaderGen.Float4Constant)
            } },
            new NodeGroup {Name="Basic Math", Types = new Type[] { // 4 nodes
                typeof(ShaderGen.AddNode), typeof(ShaderGen.SubtractNode),
                typeof(ShaderGen.MultiplyNode), typeof(ShaderGen.DivideNode),
            } },
            new NodeGroup {Name="Trigonometry", Types = new Type[] { // 13 nodes
                typeof(ShaderGen.CosNode), typeof(ShaderGen.AcosNode),
                typeof(ShaderGen.SinNode), typeof(ShaderGen.AsinNode),
                typeof(ShaderGen.TanNode), typeof(ShaderGen.AtanNode),
                typeof(ShaderGen.Atan2Node)
            } },
            new NodeGroup {Name="Math", Types = new Type[]
            {
                typeof(ShaderGen.PowNode), typeof(ShaderGen.ExpNode),
                typeof(ShaderGen.Exp2Node), typeof(ShaderGen.LogNode),
                typeof(ShaderGen.Log2Node), typeof(ShaderGen.SqrtNode),
                typeof(ShaderGen.RsqrtNode), typeof(ShaderGen.AbsNode),
                typeof(ShaderGen.SignNode), typeof(ShaderGen.FloorNode),
                typeof(ShaderGen.CeilNode), typeof(ShaderGen.FracNode),
                typeof(ShaderGen.FmodNode),
            } },
            new NodeGroup {Name="Ranges", Types = new Type[]
            {
                typeof(ShaderGen.RoundNode), typeof(ShaderGen.MinNode),
                typeof(ShaderGen.MaxNode), typeof(ShaderGen.ClampNode),
                typeof(ShaderGen.LerpNode), typeof(ShaderGen.StepNode),
                typeof(ShaderGen.SmoothstepNode), typeof(ShaderGen.TruncNode),
                typeof(ShaderGen.SaturateNode)
            } },
            new NodeGroup { Name="Vectors", Types= new Type[]
            {
                typeof(ShaderGen.DotNode), typeof(ShaderGen.CrossNode),
                typeof(ShaderGen.NormalizeNode), typeof(ShaderGen.LengthNode),
                typeof(ShaderGen.DistanceNode), typeof(ShaderGen.DstNode),
                typeof(ShaderGen.ReflectNode), typeof(ShaderGen.RefractNode),
                typeof(ShaderGen.FaceforwardNode),
            } },
            new NodeGroup { Name="Matrices", Types= new Type[]
            {
                typeof(ShaderGen.TransformNode), typeof(ShaderGen.TransformNormalNode),
                typeof(ShaderGen.TransposeNode), typeof(ShaderGen.DeterminantNode),
            } },
            new NodeGroup { Name="Misc", Types= new Type[]
            {
                typeof(ShaderGen.DdxNode), typeof(ShaderGen.DdyNode),
                typeof(ShaderGen.IsinfiniteNode), typeof(ShaderGen.IsnanNode)
            } },
            new NodeGroup { Name="Internals", Types= new Type[] {
                typeof(ShaderGen.VertexShaderOutputNode), typeof(ShaderGen.PixelShaderOutputNode),
                typeof(ShaderGen.InputData)
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

            foreach (var grp in NodeGroups)
            {
                MenuItem grpItem = new MenuItem { Header = grp.Name };
                foreach (var type in grp.Types)
                {
                    MenuItem typeItem = new MenuItem
                    {
                        Header = type.Name.Replace("Node", "").SplitCamelCase(),
                        Icon = new Image { Source = SprueKit.Controls.GraphParts.ShaderGraphNode.IconConverter.GetIcon(type), MaxWidth = 16, MaxHeight = 16 }
                    };
                    typeItem.Click += (o, args) =>
                    {
                        var createPt = ctxPoint_;
                        Data.Graph.GraphNode nd = Activator.CreateInstance(type) as Data.Graph.GraphNode;
                        if (nd != null)
                        {
                            nd.Graph = DataRoot;
                            nd.Construct();
                            nd.VisualX = createPt.X - graphControl_.nodeTranslation.X;
                            nd.VisualY = createPt.Y - graphControl_.nodeTranslation.Y;
                            DataRoot.AddNode(nd);
                            nd.UpdateSocketIDs();
                            //((ShaderGenNode)nd).RefreshPreview();
                        }
                    };
                    grpItem.Items.Add(typeItem);
                }
                graphControl_.canvasCtxMenu.Items.Add(grpItem);
            }
        }

        #endregion
    }
}
