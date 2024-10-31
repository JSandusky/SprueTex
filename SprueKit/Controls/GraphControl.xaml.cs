using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

using FirstFloor.ModernUI.Windows.Controls;
using SprueKit.Controls.GraphParts;
using System.Diagnostics;
using GongSolutions.Wpf.DragDrop;
using System.Windows.Data;
using System.Windows.Controls.Primitives;
using System.Collections;
using System.Windows.Threading;

namespace SprueKit.Controls
{
    /// <summary>
    /// Interaction logic for GraphControl.xaml
    /// </summary>
    public partial class GraphControl : UserControl, IDropTarget, IClipboardControl
    {
        #region Constants

        static DrawingBrush backgroundBrush_;

        public EventHandler<GraphParts.GraphNode> OnRegenerate;

        static Brush GetBGBrush()
        {
            if (backgroundBrush_ != null)
                return backgroundBrush_;

            backgroundBrush_ = new DrawingBrush();
            backgroundBrush_.TileMode = TileMode.Tile;
            backgroundBrush_.Viewport = new Rect(-10, -10, 40, 40);
            backgroundBrush_.ViewportUnits = BrushMappingMode.Absolute;
            GeometryDrawing drawing = new GeometryDrawing();
            drawing.Geometry = new RectangleGeometry(new Rect(0, 0, 50, 50));
            drawing.Pen = new Pen(new SolidColorBrush(Color.FromRgb(30,30,30)), 1);
            backgroundBrush_.Drawing = drawing;

            return backgroundBrush_;
        }
        Rectangle background_;

        static float[] ZoomLevels =
        {
            0.25f,
            0.50f,
            0.75f,
            1.00f,
            1.25f,
            1.50f,
            1.75f,
            2.00f,
            2.50f,
            3.00f,
            3.50f,
            4.00f,
            4.50f,
            5.00f
        };

        #endregion

        IOCDependency<DocumentManager> documentManager_;
        public Data.Graph.Graph BackingGraph { get; set; }

        #region Events

        public class SocketEventArgs : EventArgs
        {
            public GraphParts.GraphSocket From { get; set; }
            public GraphParts.GraphSocket To { get; set; }
            public bool Handled { get; set; } = false;
        }

        public EventHandler<SocketEventArgs> ConnectionMade;
        public bool CheckConnectionMade(SocketEventArgs args)
        {
            ConnectionMade(this, args);
            return args.Handled;
        }

        public EventHandler<SocketEventArgs> ConnectionDeleted;
        public bool CheckConnectionDelete(SocketEventArgs args)
        {
            if (ConnectionDeleted != null)
                ConnectionDeleted(this, args);
            return args.Handled;
        }

        public delegate void NodeDeletedEventHandler(GraphParts.GraphNode node);
        public event NodeDeletedEventHandler NodeDeleted;

        #endregion

        #region Dependency properties
        public static readonly DependencyProperty NodeSourceProperty = DependencyProperty.Register(
            "NodeSource", typeof(object), typeof(GraphControl), new PropertyMetadata(null));

        public object NodeSource { get { return GetValue(NodeSourceProperty); } set { SetValue(NodeSourceProperty, value); } }

        public static readonly DependencyProperty ConnectionSourceProperty = DependencyProperty.Register(
            "ConnectionSource", typeof(object), typeof(GraphControl), new PropertyMetadata(null));

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title", typeof(string), typeof(GraphControl), new PropertyMetadata(null));

        public string Title { get { return (string)GetValue(TitleProperty); } set { SetValue(TitleProperty, value); } }
        

        public object ConnectionSource
        {
            get { return GetValue(ConnectionSourceProperty); }
            set { SetValue(ConnectionSourceProperty, value); }
        }

        #endregion

        bool firstLoad_ = true;
        public GraphControl(Document doc)
        {
            InitializeComponent();
            canvas.Tag = this;
            PreviewKeyDown += GraphControl_PreviewKeyDown;
            KeyDown += GraphControl_KeyDown;
            KeyUp += GraphControl_KeyUp;
            Loaded += GraphControl_Loaded;
            LayoutUpdated += GraphControl_LayoutUpdated;
            LostMouseCapture += GraphControl_LostMouseCapture;
            background_ = new Rectangle();
            Canvas.SetZIndex(background_, -1000);
            background_.Fill = GetBGBrush();
            canvas.Children.Add(background_);

            doc.Selection.Selected.CollectionChanged += Selected_CollectionChanged;

            SetCanvasTransform(1.0f, 0.0, 0.0);
            OffsetChildren(0, 0);

            
        }
        bool firstLayout_ = true;
        private void GraphControl_LayoutUpdated(object sender, EventArgs e)
        {
            if (firstLayout_)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() => { RebuildConnectors(); }));
                //RebuildConnectors();
                firstLayout_ = false;
            }
        }

        private void Selected_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Action<IList, int> checker = new Action<IList, int>((IList list, int isRemove) =>
            {
                if (list == null)
                    return;

                foreach (var item in list)
                {
                    foreach (var child in canvas.Children)
                    {
                        if (child is GraphNode && ((GraphNode)child).BackingData == item)
                        {
                            if (isRemove == 1 && SelectedNodes.Contains(child))
                            {
                                ((GraphNode)child).IsSelected = false;
                                SelectedNodes.Remove(child as GraphNode);
                            }
                            else if (isRemove == 2 && !SelectedNodes.Contains(child))
                            {
                                ((GraphNode)child).IsSelected = true;
                                SelectedNodes.Add(child as GraphNode);
                            }
                            else if (isRemove == 0)
                            {
                                ((GraphNode)child).IsSelected = false;
                                SelectedNodes.Remove(child as GraphNode);
                            }
                        }
                    }
                }
            });

            checker(e.OldItems, 1);
            checker(e.NewItems, 2);

            // double null means a Clear()
            if (e.NewItems == null && e.OldItems == null)
                ClearLocalSelection();
        }

        private void GraphControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (firstLoad_)
            {
                SetCanvasTransform(1.0f, 0.0, 0.0);
                OffsetChildren(0, 0);
            }
            firstLoad_ = false;
        }

        public List<GraphParts.GraphNode> SelectedNodes = new List<GraphNode>();
        public List<GraphParts.ConnectorShape> SelectedConnectors = new List<ConnectorShape>();

        public float Scale { get; set; } = 1.0f;
        public bool Dragging { get; set; } = false;
        public double AmountDragged { get; set; } = 0.0;
        Vector clickPosition_;

        void UpdateChildren()
        {

        }
        
        private void canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Focus();
            AmountDragged = 0.0;
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                SelectionBox.Visibility = Visibility.Visible;
                rectStart = e.GetPosition(canvas);
                SetRect(SelectionBox, rectStart, rectStart);
                return;
            }
            else if (!Dragging)
            {
                canvas.Focus();
                Dragging = true;
                clickPosition_ = MapMousePos(e.GetPosition(canvas));
                canvas.CaptureMouse();
                canvas.Cursor = Cursors.SizeAll;
            }
        }

        private void canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (SelectionBox.Visibility != Visibility.Hidden)
            {
                Rect selBox = GetRectOfObject(SelectionBox);
                if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                    ClearSelection();
                foreach (var child in canvas.Children)
                {
                    if (child is GraphNode)
                    { 
                        GraphNode node = child as GraphNode;
                        Rect r = GetRectOfObject(node);//new Rect(node.X, node.Y, node.ActualWidth, node.ActualHeight);
                        if (r.IntersectsWith(selBox))
                            SetSelected(node, true);
                    }
                }
            }
            else if (AmountDragged < 10)
                ClearSelection();

            SelectionBox.Visibility = Visibility.Hidden;
            canvas.Focus();
            Dragging = false;
            canvas.ReleaseMouseCapture();
            canvas.Cursor = null;
            GraphNode.FlushDragging();
        }

        private void GraphControl_LostMouseCapture(object sender, MouseEventArgs e)
        {
            SelectionBox.Visibility = Visibility.Hidden;
            Dragging = false;
            canvas.Cursor = null;
            GraphNode.FlushDragging();
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            // are we dragging?
            if (SelectionBox.Visibility == Visibility.Visible && Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                var pt = e.GetPosition(canvas);
                SetRect(SelectionBox, rectStart, pt);
                return;
            }
            SelectionBox.Visibility = Visibility.Hidden;

            if (Dragging)
            {
                var currentPosition = e.GetPosition(canvas);
                var unmappedClick = RemapMousePos(clickPosition_);
                double DeltaX = currentPosition.X - unmappedClick.X;
                double DeltaY = currentPosition.Y - unmappedClick.Y;
                AmountDragged += Math.Max(Math.Abs(DeltaX), Math.Abs(DeltaY));
                OffsetChildren(DeltaX, DeltaY);
                clickPosition_ = MapMousePos(e.GetPosition(canvas));
            }
        }

        private void canvas_PrepareContextMenu(object sender, MouseButtonEventArgs e)
        {
            canvasCtxMenu.Items.Clear();
        }

        Point rectStart = new Point();
        void SetRect(Rectangle rect, Point a, Point b)
        {
            Point tl = new Point(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
            Point br = new Point(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));

            Canvas.SetLeft(rect, tl.X);
            Canvas.SetTop(rect, tl.Y);

            rect.Width = Math.Max(br.X - tl.X, 0);
            rect.Height = Math.Max(br.Y - tl.Y, 0);
        }

        private void Master_Loaded(object sender, RoutedEventArgs e)
        {
            documentManager_ = new IOCDependency<DocumentManager>();

            Canvas.SetZIndex(SelectionBox, 300);
            int left = 0;
            List<GraphNode> addedNodes = new List<GraphNode>();
            for (int i = 0; i < 10; ++i)
            {
                //GraphNode sp = new GraphNode(this, new Data.Graph.GraphNode(BackingGraph) { Name = "Sobel Edge" }, left, 0);
                //canvas.Children.Add(sp);
                //addedNodes.Add(sp);
                //BackingGraph.AddNode(sp.BackingData);
                ////sp.SetValue(Canvas.ZIndexProperty, 5000);
                //left += 90;
            }

            canvas.KeyUp += (o, evt) =>
            {
                //if (IsMouseDirectlyOver)// && IsFocused || canvas.IsFocused)
                {
                    if (evt.Key == Key.Home)
                    {
                        SetChildrenToZero();
                        evt.Handled = true;
                    }
                }
            };
        }

        #region Canvas Manipulation

        public void UpdatePositions(bool force = false)
        {
            foreach (var child in canvas.Children)
            {
                /*if (child is GraphNode)
                    ((GraphNode)child).UpdateConnections();
                else */if (child is ConnectorShape)
                    ((ConnectorShape)child).UpdateConnectivity(force);
            }
        }

        void UpdateSizing()
        {
            // Compute min, after < 0
            Point min = new Point(double.MaxValue, double.MaxValue);
            foreach (var child in canvas.Children)
            {
                if (child is GraphNode)
                {
                    GraphNode ch = child as GraphNode;
                    var transform = ch.RenderTransform as TranslateTransform;
                    if (transform == null)
                        ch.RenderTransform = transform = new TranslateTransform();

                    min.X = Math.Min(min.X, transform.X);
                    min.Y = Math.Min(min.Y, transform.Y);
                }
            }

            // Compute shifted max
            Point max = new Point(double.MinValue, double.MinValue);
            foreach (var child in canvas.Children)
            {
                if (child is GraphNode)
                {
                    GraphNode ch = child as GraphNode;
                    var transform = ch.RenderTransform as TranslateTransform;
                    if (transform == null)
                        ch.RenderTransform = transform = new TranslateTransform();

                    if (min.X < 0)
                        transform.X += Math.Abs(min.X) * 0.1f;
                    else if (min.X > 50)
                        transform.X -= 2;
                    if (min.Y < 0)
                        transform.Y += Math.Abs(min.Y) * 0.1f;
                    else if (min.Y > 50)
                        transform.Y -= 2;

                    max.X = Math.Max(max.X, transform.X + ch.ActualWidth);
                    max.Y = Math.Max(max.Y, transform.Y + ch.ActualHeight);
                }
            }
            UpdatePositions();
            canvas.InvalidateMeasure();
        }

        public TranslateTransform nodeTranslation = new TranslateTransform(0, 0);
        void OffsetChildren(double x, double y)
        {
            foreach (var obj in canvas.Children)
            {
                if (obj is GraphNode)
                {
                    GraphNode node = obj as GraphNode;
                    TranslateTransform t = node.RenderTransform as TranslateTransform;
                    if (t == null)
                        node.RenderTransform = nodeTranslation;// t = new TranslateTransform(0, 0);
                    //t.X += x;
                    //t.Y += y;
                }
            }
            nodeTranslation.X += x;
            nodeTranslation.Y += y;
            UpdatePositions();
            UpdateBackground(x, y);
        }

        void UpdateBackground(double x, double y)
        {
            TranslateTransform bgt = background_.RenderTransform as TranslateTransform;
            if (bgt == null)
            {
                background_.RenderTransform = bgt = new TranslateTransform(0, 0);
                bgt.X = -25000;
                bgt.Y = -25000;
            }
            bgt.X += x;
            bgt.Y += y;
            background_.Width = 50000;
            background_.Height = 50000;   
        }

        void SetChildrenToZero()
        {
            //Point min = new Point(double.MaxValue, double.MaxValue);
            //foreach (var obj in canvas.Children)
            //{
            //    GraphNode node = obj as GraphNode;
            //    if (node != null)
            //    {
            //        TranslateTransform t = node.RenderTransform as TranslateTransform;
            //        if (t == null)
            //            node.RenderTransform = t = new TranslateTransform(0, 0);
            //        min.X = Math.Min(min.X, t.X);
            //        min.Y = Math.Min(min.Y, t.Y);
            //    }
            //}
            //
            //foreach (var obj in canvas.Children)
            //{
            //    GraphNode node = obj as GraphNode;
            //    if (node != null)
            //    {
            //        TranslateTransform t = node.RenderTransform as TranslateTransform;
            //        if (t == null)
            //            node.RenderTransform = t = new TranslateTransform(0, 0);
            //        t.X -= (min.X + node.ActualWidth);
            //        t.Y -= (min.Y + node.ActualHeight);
            //    }
            //}
            nodeTranslation.X = 0;
            nodeTranslation.Y = 0;

            UpdatePositions();
        }

        public void SetCanvasTransform(float scale, double offsetX, double offsetY)
        {
            Matrix mat = new Matrix();
            mat.ScaleAtPrepend(scale, scale, offsetX, offsetY);
            MatrixTransform scalingTransform = new MatrixTransform(scale, 0, 0, scale, 0, 0);
            canvas.LayoutTransform = scalingTransform;
        }

        public void RebuildConnectors()
        {
            List<UIElement> toRemove = new List<UIElement>();
            foreach (var child in canvas.Children)
                if (child is ConnectorShape)
                    toRemove.Add(child as UIElement);
                else if (child is RouteThumb)
                    toRemove.Add(child as UIElement);

            using (var d = Dispatcher.DisableProcessing())
            {
                foreach (var obj in toRemove)
                    canvas.Children.Remove(obj);

                foreach (var con in BackingGraph.Connections)
                {
                    var fromNode = FindNode(con.FromNode);
                    var toNode = FindNode(con.ToNode);
                    Debug.Assert(fromNode != null && toNode != null);
                    if (fromNode != null && toNode != null)
                    {
                        var fromSocket = fromNode.FindSocket(con.FromSocket);
                        var toSocket = toNode.FindSocket(con.ToSocket);
                        Debug.Assert(fromSocket != null && toSocket != null);
                        if (fromSocket != null && toSocket != null)
                        {
                            fromSocket.OwnerNode = fromNode;
                            toSocket.OwnerNode = toNode;
                            ConnectorShape shape = new ConnectorShape(this, fromSocket, toSocket) { Connection = con };
                            shape.RenderTransform = nodeTranslation;
                            Canvas.SetZIndex(shape, -2);
                            shape.B = toSocket;
                            shape.DragMode = false;
                            shape.UpdateConnectivity(true);
                            canvas.Children.Add(shape);
                        }
                        else
                            throw new Exception("Failed to find matching");
                    }
                }

                foreach (var route in BackingGraph.Routes)
                {
                    for (int i = 0; i < route.Value.RoutingPoints.Count; ++i)
                    {
                        RouteThumb th = new RouteThumb(this, BackingGraph, route.Key, route.Value, i);
                        th.RenderTransform = nodeTranslation;
                        canvas.Children.Add(th);
                    }
                }
            }
            InvalidateVisual();
        }

        public void RebuildNodes()
        {
            bool anyRebuild = false;
            using (var d = Dispatcher.DisableProcessing())
            {
                for (int i = 0; i < canvas.Children.Count; ++i)
                {
                    if (canvas.Children[i] is GraphNode)
                    {
                        if (!BackingGraph.Nodes.Contains(((GraphNode)canvas.Children[i]).BackingData))
                        {
                            canvas.Children.RemoveAt(i);
                            --i;
                        }
                    }
                }

                foreach (var graphNode in BackingGraph.Nodes)
                {
                    if (FindNode(graphNode) == null)
                    {
                        var newNode = CreateNode(graphNode, false);
                        canvas.Children.Add(newNode);
                        anyRebuild = true;
                    }
                }
            }
            if (anyRebuild)
            {
                UpdateLayout();
                InvalidateMeasure();
                InvalidateVisual();
            }
        }

        public void RebuildBoxes()
        {
            using (var d = Dispatcher.DisableProcessing())
            {
                List<UIElement> toRemove = new List<UIElement>();
                foreach (var child in canvas.Children)
                {
                    if (child is GraphParts.GraphBoxControl)
                        toRemove.Add(child as UIElement);
                }
                foreach (var elem in toRemove)
                    canvas.Children.Remove(elem);
                foreach (var box in BackingGraph.Boxes)
                {
                    var gc = new GraphParts.GraphBoxControl { DataContext = box, canvas = canvas, graph = BackingGraph  };
                    gc.RenderTransform = nodeTranslation;
                    canvas.Children.Add(gc);
                }
            }
        }

        public GraphParts.GraphNode FindNode(Data.Graph.GraphNode node)
        {
            foreach (var child in canvas.Children)
            {
                if (child is GraphNode)
                {
                    if (((GraphNode)child).BackingData == node)
                        return child as GraphNode;
                }
            }
            return null;
        }

        #endregion

        #region Misc Utility

        float GetNextZoomLevel(float current, bool down)
        {
            for (int i = 0; i < ZoomLevels.Length; ++i)
            {
                if (ZoomLevels[i] == current)
                {
                    if (down && i == 0)
                        return ZoomLevels[0];
                    if (!down && i == ZoomLevels.Length - 1)
                        return ZoomLevels[ZoomLevels.Length - 1];
                    return ZoomLevels[i + (down ? -1 : 1)];
                }
            }
            return 1.0f;
        }

        Vector MapMousePos(Point pt)
        {
            return new Vector(pt.X /*- scroller.HorizontalOffset*/, pt.Y /*- scroller.VerticalOffset*/);
        }

        Point RemapMousePos(Vector pt)
        {
            return new Point(pt.X /*+ scroller.HorizontalOffset*/, pt.Y /*+ scroller.VerticalOffset*/);
        }

        #endregion

        #region Selection Functions

        // Only peforms the tasks THIS CONTROL needs to deal with, doesn't tell anyone about diddly
        protected void ClearLocalSelection()
        {
            foreach (var node in SelectedNodes)
                node.IsSelected = false;
            foreach (var conn in SelectedConnectors)
                conn.IsSelected = false;
            SelectedNodes.Clear();
            SelectedConnectors.Clear();
        }

        public void ClearSelection()
        {
            ClearLocalSelection();
            documentManager_.Object.ActiveDocument.Selection.Selected.Clear();
        }

        public void SetSelected(GraphParts.GraphNode node, bool state)
        {
            node.IsSelected = state;
            if (!state)
            {
                documentManager_.Object.ActiveDocument.Selection.Selected.Remove(node.BackingData);
                SelectedNodes.Remove(node);
            }
            else if (!SelectedNodes.Contains(node))
            {
                SelectedNodes.Add(node);
                documentManager_.Object.ActiveDocument.Selection.Selected.Add(node.BackingData);
            }
        }

        public void SetSelected(GraphParts.ConnectorShape conn, bool state)
        {
            conn.IsSelected = state;
            if (!state)
                SelectedConnectors.Remove(conn);
            else if (!SelectedConnectors.Contains(conn))
                SelectedConnectors.Add(conn);
        }

        #endregion

        private void canvas_MouseWheel(object sender, MouseWheelEventArgs args)
        {
            var pos = args.GetPosition(canvas);

            float oldFactor = Scale;

            if (args.Delta / 30 > 0)
                Scale = GetNextZoomLevel(Scale, false);
            else if (args.Delta / 30 < 0)
                Scale = GetNextZoomLevel(Scale, true);

            SetCanvasTransform(Scale, pos.X, pos.Y);
            var offsetX = ((pos.X * Scale) - (pos.X * oldFactor)) / Scale;
            var offsetY = ((pos.Y * Scale) - (pos.Y * oldFactor)) / Scale;
            OffsetChildren(-offsetX, -offsetY);
            //RebuildConnectors();
        }

        void ZoomIn()
        {
            Point pos = new Point(canvas.ActualWidth / 2, canvas.ActualHeight / 2);
            float oldFactor = Scale;
            Scale = GetNextZoomLevel(Scale, false);
            SetCanvasTransform(Scale, pos.X, pos.Y);
            var offsetX = ((pos.X * Scale) - (pos.X * oldFactor)) / Scale;
            var offsetY = ((pos.Y * Scale) - (pos.Y * oldFactor)) / Scale;
            OffsetChildren(-offsetX, -offsetY);
        }

        void ZoomOut()
        {
            Point pos = new Point(canvas.ActualWidth / 2, canvas.ActualHeight / 2);
            float oldFactor = Scale;
            Scale = GetNextZoomLevel(Scale, true);
            SetCanvasTransform(Scale, pos.X, pos.Y);
            var offsetX = ((pos.X * Scale) - (pos.X * oldFactor)) / Scale;
            var offsetY = ((pos.Y * Scale) - (pos.Y * oldFactor)) / Scale;
            OffsetChildren(-offsetX, -offsetY);
        }

        bool hadRepeat = false;
        private void GraphControl_KeyDown(object sender, KeyEventArgs e)
        {
            hadRepeat = false;
            // zoom
            if (e.Key == Key.OemPlus || e.Key == Key.OemMinus)
                e.Handled = true;
            else if (e.Key == Key.PageUp || e.Key == Key.PageDown)
                e.Handled = true;
            // pan
            else if (e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Down)
                e.Handled = true;
            else if (e.Key == Key.Delete || e.Key == Key.Back)
                e.Handled = true;
        }

        private void GraphControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.IsRepeat)
            {
                if (e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Down)
                {
                    GraphControl_KeyUp(sender, e);
                    e.Handled = true;
                    hadRepeat = true;
                }
            }
        }

        private void GraphControl_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete || e.Key == Key.Back)
            {
                DeleteSelected();
                e.Handled = true;
            }
            else if (e.Key == Key.OemPlus || e.Key == Key.OemMinus || e.Key == Key.PageUp || e.Key == Key.PageDown)
            {
                if (e.Key == Key.OemPlus || e.Key == Key.PageUp)
                {
                    ZoomIn();
                }
                else
                {
                    ZoomOut();
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Down || e.Key == Key.Up)
            {
                //NOTE: IsRepeat is being checked so we don't go flying off into la-la land when holding down shift
                double multiplier = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) ? (e.IsRepeat || hadRepeat ? 2.0f : 10.0f) : 1.0f;
                Vector vec = new Vector();
                if (e.Key == Key.Left)
                    vec.X = 20 * multiplier;
                else if (e.Key == Key.Right)
                    vec.X = -20 * multiplier;
                else if (e.Key == Key.Up)
                    vec.Y = 20 * multiplier;
                else if (e.Key == Key.Down)
                    vec.Y = -20 * multiplier;
                nodeTranslation.X += vec.X;
                nodeTranslation.Y += vec.Y;
                UpdatePositions();
                e.Handled = true;
            }
        }

        private Rect GetRectOfObject(FrameworkElement _element)
        {
            Rect rectangleBounds = new Rect();
            rectangleBounds = _element.RenderTransform.TransformBounds(new Rect(Canvas.GetLeft(_element), Canvas.GetTop(_element), _element.ActualWidth, _element.ActualHeight));
            return rectangleBounds;
            //return new Rect(Canvas.GetLeft(_element), Canvas.GetTop(_element), rectangleBounds.Width, rectangleBounds.Height);
        }

        protected virtual GraphNode CreateNode(Data.Graph.GraphNode forNode, bool isNew)
        {
            var ret = new GraphNode(this, forNode);
            ret.RenderTransform = nodeTranslation;
            ret.InvalidateVisual();
            return ret;
        }

        public virtual void DragOver(IDropInfo dropInfo)
        {
            dropInfo.Effects = DragDropEffects.None;
        }

        public virtual void Drop(IDropInfo dropInfo)
        {
            
        }

        Thumb CreateRouteThumb(Data.Graph.GraphConnection conn, int idx)
        {
            Thumb ret = new Thumb();
            ret.Width = 16;
            ret.Height = 16;
            ret.Tag = new KeyValuePair<Data.Graph.GraphConnection, int>(conn, idx);
            

            return ret;
        }

        #region Clipboard Management

        public void InsertPrefab(string name, Point at)
        {
            string readPath = name;// App.ProgramPath("Prefabs/" + name);
            byte[] bytes = System.IO.File.ReadAllBytes(readPath);

            Data.SerializationContext ctx = new Data.SerializationContext(null);
            using (var memStream = new System.IO.MemoryStream(bytes))
            {
                using (System.IO.BinaryReader rdr = new System.IO.BinaryReader(memStream))
                {
                    List<Data.Graph.GraphNode> newNodes = new List<Data.Graph.GraphNode>();
                    List<Data.Graph.GraphConnection> newConnections = new List<Data.Graph.GraphConnection>();
                    Point minPoint = new Point(double.MaxValue, double.MaxValue);

                    int nodeCt = rdr.ReadInt32();
                    for (int i = 0; i < nodeCt; ++i)
                    {
                        var nd = Data.Graph.Graph.DeserializeNode(ctx, rdr);
                        if (nd != null)
                        {
                            minPoint.X = Math.Min(minPoint.X, nd.VisualX);
                            minPoint.Y = Math.Min(minPoint.Y, nd.VisualY);
                            newNodes.Add(nd);
                        }
                    }

                    bool anyNewConnections = false;
                    int conCt = rdr.ReadInt32();
                    for (int i = 0; i < conCt; ++i)
                    {
                        int fromNode = rdr.ReadInt32();
                        int fromSocket = rdr.ReadInt32();
                        int toNode = rdr.ReadInt32();
                        int toSocket = rdr.ReadInt32();

                        Data.Graph.GraphConnection con = new Data.Graph.GraphConnection();
                        con.FromNode = newNodes.FirstOrDefault(n => n.NodeID == fromNode);
                        con.ToNode = newNodes.FirstOrDefault(n => n.NodeID == toNode);
                        if (con.FromNode != null && con.ToNode != null)
                        {
                            con.FromSocket = con.FromNode.OutputSockets.FirstOrDefault(s => s.SocketID == fromSocket);
                            con.ToSocket = con.ToNode.InputSockets.FirstOrDefault(s => s.SocketID == toSocket);

                            if (con.FromSocket != null && con.ToSocket != null)
                            {
                                newConnections.Add(con);
                                anyNewConnections = true;
                            }
                        }
                    }

                    if (newNodes.Count == 0)
                    {
                        ErrorHandler.inst().Error("Failed to paste nodes. Unable to reconstruct data from clipboard.");
                        return;
                    }

                    int[] OldIdTable = new int[newNodes.Count];

                    using (var macro = new Commands.MacroCommandBlock(string.Format("Paste {0} nodes", newNodes.Count)))
                    {
                        for (int i = 0; i < newNodes.Count; ++i)
                        {
                            OldIdTable[i] = newNodes[i].NodeID;
                            newNodes[i].VisualX = at.X - minPoint.X + newNodes[i].VisualX - nodeTranslation.X;
                            newNodes[i].VisualY = at.Y - minPoint.Y + newNodes[i].VisualY - nodeTranslation.Y;
                            newNodes[i].NodeID = BackingGraph.GetNextID();
                            newNodes[i].UpdateSocketIDs();
                            BackingGraph.AddNode(newNodes[i]);
                        }

                        foreach (var conn in newConnections)
                            BackingGraph.Connections.Add(conn);
                    }

                    foreach (var nd in newNodes)
                        NodePasted(nd);

                    if (anyNewConnections)
                        RebuildConnectors();
                }
            }
        }

        public void SavePrefab(string name)
        {
            if (SelectedNodes.Count > 0)
            {
                Data.SerializationContext ctx = new Data.SerializationContext(null);
                using (var memStream = new System.IO.MemoryStream())
                {
                    using (System.IO.BinaryWriter writer = new System.IO.BinaryWriter(memStream))
                    {
                        writer.Write(SelectedNodes.Count);
                        foreach (var nd in SelectedNodes)
                            Data.Graph.Graph.SerializeNode(ctx, writer, nd.BackingData);

                        var saveConnectors = BackingGraph.Connections.Where(con => SelectedNodes.Count(nd => nd.BackingData == con.FromNode) > 0 && SelectedNodes.Count(nd =>nd.BackingData == con.ToNode) > 0);
                        writer.Write(saveConnectors.Count());
                        //writer.Write(SelectedConnectors.Count);
                        foreach (var con in saveConnectors)
                        {
                            writer.Write(con.FromNode.NodeID);
                            writer.Write(con.FromSocket.SocketID);
                            writer.Write(con.ToNode.NodeID);
                            writer.Write(con.ToSocket.SocketID);
                        }
                    }

                    string writePath = App.ProgramPath("Prefabs/" + name);
                    System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(writePath));
                    System.IO.File.WriteAllBytes(writePath, memStream.ToArray());
                    //LocalClipboard[GetType()] = memStream.ToArray();
                    //Clipboard.SetData("GRAPH_CONTROL", "LOCAL_OBJECT");
                }
            }
        }

        public void CutSelected()
        {
            CopySelected();
            DeleteSelected();
        }

        public void DeleteSelected()
        {
            using (var d = Dispatcher.DisableProcessing())
            {
                var doc = documentManager_.Object.ActiveDocument;
                using (var batcher = new Commands.MacroCommandBlock())
                {
                    for (int i = 0; i < SelectedConnectors.Count; ++i)
                    {
                        if (CheckConnectionDelete(new SocketEventArgs { From = SelectedConnectors[i].A, To = SelectedConnectors[i].B }))
                        {
                            canvas.Children.Remove(SelectedConnectors[i]);
                            SelectedConnectors.Remove(SelectedConnectors[i]);
                            --i;
                        }
                    }
                    foreach (var node in SelectedNodes)
                    {
                        Canvas owner = node.OwnerCanvas;
                        node.OwnerControl.BackingGraph.RemoveNode(node.BackingData);
                        doc.Selection.Selected.Remove(node.BackingData);
                    }
                    ClearSelection();
                }
            }
            RebuildConnectors();
        }

        public void CopySelected()
        {
            if (SelectedNodes.Count > 0)
            {
                Data.SerializationContext ctx = new Data.SerializationContext(null);
                using (var memStream = new System.IO.MemoryStream())
                {
                    using (System.IO.BinaryWriter writer = new System.IO.BinaryWriter(memStream))
                    {
                        writer.Write(SelectedNodes.Count);
                        foreach (var nd in SelectedNodes)
                            Data.Graph.Graph.SerializeNode(ctx, writer, nd.BackingData);


                        var saveConnectors = BackingGraph.Connections.Where(con => SelectedNodes.Count(nd => nd.BackingData == con.FromNode) > 0 && SelectedNodes.Count(nd => nd.BackingData == con.ToNode) > 0);
                        writer.Write(saveConnectors.Count());
                        //writer.Write(SelectedConnectors.Count);
                        foreach (var con in saveConnectors)
                        {
                            writer.Write(con.FromNode.NodeID);
                            writer.Write(con.FromSocket.SocketID);
                            writer.Write(con.ToNode.NodeID);
                            writer.Write(con.ToSocket.SocketID);
                        }
                        //writer.Write(SelectedConnectors.Count);
                        //foreach (var con in SelectedConnectors)
                        //{
                        //    writer.Write(con.Connection.FromNode.NodeID);
                        //    writer.Write(con.Connection.FromSocket.SocketID);
                        //    writer.Write(con.Connection.ToNode.NodeID);
                        //    writer.Write(con.Connection.ToSocket.SocketID);
                        //}
                    }

                    LocalClipboard[GetType()] = memStream.ToArray();
                    Clipboard.SetData("GRAPH_CONTROL", "LOCAL_OBJECT");
                }
            }
        }

        protected virtual void NodePasted(Data.Graph.GraphNode nd)
        {

        }

        public void SurroundSelectedWithCommentBox()
        {
            if (SelectedNodes.Count == 0)
                return;

            Point minPoint = new Point(double.MaxValue, double.MaxValue);
            Point maxPoint = new Point(double.MinValue, double.MinValue);
            foreach (var node in SelectedNodes)
            {
                minPoint.X = Math.Min(minPoint.X, node.BackingData.VisualX);
                minPoint.Y = Math.Min(minPoint.Y, node.BackingData.VisualY);
                maxPoint.X = Math.Max(maxPoint.X, node.BackingData.VisualX);
                maxPoint.Y = Math.Max(maxPoint.Y, node.BackingData.VisualY);
            }

            int padSize = new IOCDependency<Settings.TextureGraphSettings>().Object.PreviewResolution == Settings.TextureGraphPreviewResolution.Large ? 256 : 128;
            padSize += 40;
            double w = maxPoint.X - minPoint.X;
            double h = maxPoint.Y - minPoint.Y;
            Random r = new Random();
            BackingGraph.Boxes.Add(new Data.Graph.GraphBox { VisualX = minPoint.X - 20, VisualY = minPoint.Y - 20, VisualWidth = w + padSize, VisualHeight = h + padSize, BoxColor = new Microsoft.Xna.Framework.Color(r.Next(80, 255), r.Next(80, 255), r.Next(80, 255), 255) });
        }

        public bool Paste(Point at)
        {
            if (LocalClipboard != null)
            {
                var obj = Clipboard.GetData("GRAPH_CONTROL");
                if (obj != null && obj.ToString().Equals("LOCAL_OBJECT"))
                {
                    Data.SerializationContext ctx = new Data.SerializationContext(null);

                    byte[] data = null;
                    if (!LocalClipboard.TryGetValue(GetType(), out data))
                        return false;

                    using (var memStream = new System.IO.MemoryStream(data, false))
                    {
                        using (var rdr = new System.IO.BinaryReader(memStream))
                        {
                            List<Data.Graph.GraphNode> newNodes = new List<Data.Graph.GraphNode>();
                            List<Data.Graph.GraphConnection> newConnections = new List<Data.Graph.GraphConnection>();
                            Point minPoint = new Point(double.MaxValue, double.MaxValue);

                            int nodeCt = rdr.ReadInt32();
                            for (int i = 0; i < nodeCt; ++i)
                            {
                                var nd = Data.Graph.Graph.DeserializeNode(ctx, rdr);
                                if (nd != null)
                                {
                                    minPoint.X = Math.Min(minPoint.X, nd.VisualX);
                                    minPoint.Y = Math.Min(minPoint.Y, nd.VisualY);
                                    newNodes.Add(nd);
                                }
                            }

                            bool anyNewConnections = false;
                            int conCt = rdr.ReadInt32();
                            for (int i = 0; i < conCt; ++i)
                            {
                                int fromNode = rdr.ReadInt32();
                                int fromSocket = rdr.ReadInt32();
                                int toNode = rdr.ReadInt32();
                                int toSocket = rdr.ReadInt32();

                                Data.Graph.GraphConnection con = new Data.Graph.GraphConnection();
                                con.FromNode = newNodes.FirstOrDefault(n => n.NodeID == fromNode);
                                con.ToNode = newNodes.FirstOrDefault(n => n.NodeID == toNode);
                                if (con.FromNode != null && con.ToNode != null)
                                {
                                    con.FromSocket = con.FromNode.OutputSockets.FirstOrDefault(s => s.SocketID == fromSocket);
                                    con.ToSocket = con.ToNode.InputSockets.FirstOrDefault(s => s.SocketID == toSocket);

                                    if (con.FromSocket != null && con.ToSocket != null)
                                    {
                                        newConnections.Add(con);
                                        anyNewConnections = true;
                                    }
                                }
                            }

                            if (newNodes.Count == 0)
                            {
                                ErrorHandler.inst().Error("Failed to paste nodes. Unable to reconstruct data from clipboard.");
                                return false;
                            }

                            int[] OldIdTable = new int[newNodes.Count];

                            using (var macro = new Commands.MacroCommandBlock(string.Format("Paste {0} nodes", newNodes.Count)))
                            {
                                for (int i = 0; i < newNodes.Count; ++i)
                                {
                                    OldIdTable[i] = newNodes[i].NodeID;
                                    newNodes[i].VisualX = at.X - minPoint.X + newNodes[i].VisualX - nodeTranslation.X;
                                    newNodes[i].VisualY = at.Y - minPoint.Y + newNodes[i].VisualY - nodeTranslation.Y;
                                    newNodes[i].NodeID = BackingGraph.GetNextID();
                                    newNodes[i].UpdateSocketIDs();
                                    BackingGraph.AddNode(newNodes[i]);
                                }

                                foreach (var conn in newConnections)
                                    BackingGraph.Connections.Add(conn);
                            }

                            foreach (var nd in newNodes)
                                NodePasted(nd);

                            if (anyNewConnections)
                                RebuildConnectors();

                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public bool Cut()
        {
            if (SelectedNodes.Count > 0)
            {
                CutSelected();
                return true;
            }
            return false;
        }

        public bool Copy()
        {
            if (SelectedNodes.Count > 0)
            {
                CopySelected();
                return true;
            }
            return false;
        }

        public bool Paste()
        {
            Point center = new Point(ActualWidth / 2, ActualHeight / 2);
            Paste(center);
            return true;
        }

        static Dictionary<Type, byte[]> LocalClipboard = new Dictionary<Type, byte[]>();
        #endregion

        public void SignalRegenerate(GraphParts.GraphNode node)
        {
            if (OnRegenerate != null)
                OnRegenerate(this, node);
        }

        private void legend_Click(object sender, RoutedEventArgs e)
        {
            var helper = PopupHelper.Create();

            helper.Grid.RowDefinitions.Add(new RowDefinition());
            helper.Grid.RowDefinitions.Add(new RowDefinition());
            helper.Grid.RowDefinitions.Add(new RowDefinition());
            helper.Grid.RowDefinitions.Add(new RowDefinition());
            helper.Grid.RowDefinitions.Add(new RowDefinition());

            {
                StackPanel rgbaPanel = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(2) };
                rgbaPanel.Children.Add(new Ellipse { Width = 16, Height = 16, Fill = new SolidColorBrush(Colors.Gold) });
                rgbaPanel.Children.Add(new BBCodeBlock { BBCode = "[b]RGBA[/b], only connects to other RGBA", Margin = new Thickness(2, 0, 2, 0) });
                helper.Grid.Children.Add(rgbaPanel);
            }
            {
                StackPanel rgbaPanel = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(2) };
                rgbaPanel.Children.Add(new Ellipse { Width = 16, Height = 16, Fill = new SolidColorBrush(Colors.Gray) });
                rgbaPanel.Children.Add(new BBCodeBlock { BBCode = "[b]Grayscale[/b], only connects to other grayscale", Margin = new Thickness(2, 0, 2, 0) });
                helper.Grid.Children.Add(rgbaPanel);
                Grid.SetRow(rgbaPanel, 1);
            }
            {
                StackPanel rgbaPanel = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(2) };
                rgbaPanel.Children.Add(new Ellipse { Width = 16, Height = 16, Fill = new SolidColorBrush(Colors.Orange) });
                rgbaPanel.Children.Add(new BBCodeBlock { BBCode = "[b]Channel[/b], connects to either RGBA or Grayscale", Margin = new Thickness(2, 0, 2, 0) });
                Grid.SetRow(rgbaPanel, 2);
                helper.Grid.Children.Add(rgbaPanel);
            }
            {
                StackPanel rgbaPanel = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(2) };
                rgbaPanel.Children.Add(new Ellipse { Width = 16, Height = 16, Fill = new SolidColorBrush(Colors.LimeGreen) });
                rgbaPanel.Children.Add(new BBCodeBlock { BBCode = "[b]Model[/b], transfers meshes for bakers and Relief nodes", Margin = new Thickness(2, 0, 2, 0) });
                Grid.SetRow(rgbaPanel, 3);
                helper.Grid.Children.Add(rgbaPanel);
            }

            StackPanel textPanel = new StackPanel() { Orientation = Orientation.Vertical, Margin = new Thickness(2) };
            Grid.SetRow(textPanel, 4);
            textPanel.Children.Add(new BBCodeBlock() { BBCode = "[b]Left-mouse[/b]: select, move selection, pan view" });
            textPanel.Children.Add(new BBCodeBlock() { BBCode = "[b]Right-mouse[/b]: context menu" });
            textPanel.Children.Add(new BBCodeBlock() { BBCode = "[b]Mouse-wheel[/b]: zoom" });
            helper.Grid.Children.Add(textPanel);

            helper.ShowAtMouse();
        }
    }
}
