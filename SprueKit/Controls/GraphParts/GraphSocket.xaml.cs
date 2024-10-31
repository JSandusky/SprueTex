using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace SprueKit.Controls.GraphParts
{
    /// <summary>
    /// Interaction logic for GraphSocket.xaml
    /// </summary>
    public partial class GraphSocket : UserControl
    {
        #region Visual properties

        static Brush HoverBrush = new SolidColorBrush(Colors.Gold);

        public static DependencyProperty HighlightBrushProperty = DependencyProperty.Register(
            "HighlightBrush",
            typeof(Brush),
            typeof(GraphSocket),
            new PropertyMetadata(new SolidColorBrush(Colors.Gold)));

        public static DependencyProperty FillBrushProperty = DependencyProperty.Register(
            "FillBrush",
            typeof(Brush),
            typeof(GraphSocket),
            new PropertyMetadata(new SolidColorBrush(Colors.Red)));

        public static DependencyProperty RingBrushProperty = DependencyProperty.Register(
            "RingBrush",
            typeof(Brush),
            typeof(GraphSocket),
            new PropertyMetadata(new SolidColorBrush(Colors.Red)));

        public static DependencyProperty ActiveRingBrushProperty = DependencyProperty.Register(
            "ActiveRingBrush",
            typeof(Brush),
            typeof(GraphSocket),
            new PropertyMetadata(new SolidColorBrush(Colors.Red)));

        public static DependencyProperty OwnerNodeProperty = DependencyProperty.Register("OwnerNode",
            typeof(GraphParts.GraphNode),
            typeof(GraphParts.GraphSocket), new PropertyMetadata(null));
        public GraphParts.GraphNode OwnerNode
        {
            get { return (GraphParts.GraphNode)GetValue(OwnerNodeProperty); }
            set { SetValue(OwnerNodeProperty, value); }
        }

        public Brush HighlightBrush
        {
            get { return (Brush)GetValue(HighlightBrushProperty); }
            set { SetValue(HighlightBrushProperty, value); }
        }

        public Brush FillBrush
        {
            get { return (Brush)GetValue(FillBrushProperty); }
            set { SetValue(FillBrushProperty, value); }
        }

        public Brush ActiveRingBrush
        {
            get { return (Brush)GetValue(ActiveRingBrushProperty); }
            set { SetValue(ActiveRingBrushProperty, value); }
        }

        public Brush RingBrush
        {
            get { return (Brush)GetValue(RingBrushProperty); }
            set { SetValue(RingBrushProperty, value); }
        }

        #endregion

        public Data.Graph.GraphSocket DataSocket { get { return DataContext as Data.Graph.GraphSocket; } }
        public double VisualX { get; set; }
        public double VisualY { get; set; }
        public bool IsInput { get { return DataSocket.IsInput; } }

        //public object Tag { get; set; }

        bool hovered_ = false;
        public bool Hovered
        {
            get { return hovered_; }
            set {
                hovered_ = value;
                if (hovered_)
                    ActiveRingBrush = HoverBrush;
                else
                    ActiveRingBrush = RingBrush;
            }
        }

        #region Connectors
        ConnectorShape connectorShape_;
        ConnectorShape CreateConnectorShape(GraphSocket src)
        {
            ConnectorShape ret = new ConnectorShape(OwnerNode.OwnerControl, src);
            ret.Stroke = new SolidColorBrush(Colors.CornflowerBlue);
            ret.StrokeThickness = 4;
            Point pt = area.TransformToAncestor(OwnerNode.OwnerCanvas).Transform(new Point(area.ActualWidth / 2, area.ActualHeight / 2));
            pt.X -= OwnerNode.OwnerControl.nodeTranslation.X;
            pt.Y -= OwnerNode.OwnerControl.nodeTranslation.Y;
            ret.Start = pt;
            ret.End = pt;
            OwnerNode.OwnerCanvas.Children.Add(ret);
            Canvas.SetZIndex(ret, 2);
            return ret;
        }

        #endregion

        public GraphSocket()
        {
            InitializeComponent();
            CommonConstruct();
        }

        public GraphSocket(Color ringColor, Color centerColor)
        {
            InitializeComponent();
            FillBrush = new SolidColorBrush(centerColor);
            ActiveRingBrush = RingBrush = new SolidColorBrush(ringColor);
        }

        void CommonConstruct()
        {
            Canvas.SetZIndex(this, 2);
            MouseLeftButtonDown += GraphSocket_MouseLeftButtonDown;
            MouseLeftButtonUp += GraphSocket_MouseLeftButtonUp;
            MouseMove += GraphSocket_MouseMove;
            LostMouseCapture += GraphSocket_LostMouseCapture;
            IsHitTestVisible = true;
            LayoutUpdated += GraphSocket_LayoutUpdated;
        }

        private void GraphSocket_LostMouseCapture(object sender, MouseEventArgs e)
        {
            if (connectorShape_ != null)
                OwnerNode.OwnerCanvas.Children.Remove(connectorShape_);

            connectorShape_ = null;
            DragSocket = null;
            HoverSocket = null;
        }

        void GraphSocket_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            if (DragSocket == null)
            {
                DragSocket = this;
                CaptureMouse();
                e.Handled = true;
                connectorShape_ = CreateConnectorShape(this);
            }
        }

        void GraphSocket_MouseLeftButtonUp(object sender, MouseEventArgs args)
        {
            var hitSocket = DragSocket != null ? HitSocket(OwnerNode.OwnerCanvas, args.GetPosition(OwnerNode.OwnerCanvas), !DragSocket.IsInput) : null;

            if (hitSocket != null && connectorShape_ != null)
            {
                Point pt = hitSocket.area.TransformToAncestor(OwnerNode.OwnerCanvas).Transform(new Point(hitSocket.area.ActualWidth / 2, hitSocket.area.ActualHeight / 2));
                pt.X -= OwnerNode.OwnerControl.nodeTranslation.X;
                pt.Y -= OwnerNode.OwnerControl.nodeTranslation.Y;
                connectorShape_.End = pt;
                //ConnectorShape_.X2 = pt.X;
                //ConnectorShape_.Y2 = pt.Y;
                // Move to back now
                Canvas.SetZIndex(connectorShape_, 0);
                if (DragSocket.IsInput)
                {
                    var swap = connectorShape_.End;
                    connectorShape_.End = connectorShape_.Start;
                    connectorShape_.Start = swap;
                    var swapA = connectorShape_.A;
                    connectorShape_.A = hitSocket;
                    connectorShape_.B = swapA;
                }
                else
                    connectorShape_.B = hitSocket;

                if (!OwnerNode.OwnerControl.CheckConnectionMade(new GraphControl.SocketEventArgs { From = connectorShape_.A, To = connectorShape_.B }))
                {
                    if (connectorShape_ != null)
                        OwnerNode.OwnerCanvas.Children.Remove(connectorShape_);
                    connectorShape_ = null;
                    DragSocket = null;
                    args.Handled = true;
                    ReleaseMouseCapture();
                    return;
                }

                connectorShape_.DragMode = false;
                connectorShape_ = null;
                DragSocket = null;
                args.Handled = true;
                ReleaseMouseCapture();
            }
            else if (DragSocket == this)
            {
                if (connectorShape_ != null)
                    OwnerNode.OwnerCanvas.Children.Remove(connectorShape_);

                connectorShape_ = null;
                DragSocket = null;
                args.Handled = true;

                ReleaseMouseCapture();
            }
            HoverSocket = null;
        }

        void GraphSocket_MouseMove(object sender, MouseEventArgs args)
        {
            if (DragSocket == this && connectorShape_ != null)
            {
                args.Handled = true;
                Point pos = args.GetPosition(OwnerNode.OwnerCanvas);
                pos.X -= OwnerNode.OwnerControl.nodeTranslation.X;
                pos.Y -= OwnerNode.OwnerControl.nodeTranslation.Y;
                connectorShape_.End = pos;
                connectorShape_.ClearGeometryCache();
                //ConnectorShape_.X2 = pos.X;
                //ConnectorShape_.Y2 = pos.Y;

                HoverSocket = HitSocket(OwnerNode.OwnerCanvas, args.GetPosition(OwnerNode.OwnerCanvas), !DragSocket.IsInput);
            }
        }

        private void GraphSocket_LayoutUpdated(object sender, EventArgs e)
        {
            
        }

        #region Static socket management
        static GraphSocket hoverSocket_ = null;
        public static GraphSocket HoverSocket
        {
            get { return hoverSocket_; }
            set
            {
                if (hoverSocket_ != null)
                    hoverSocket_.Hovered = false;
                hoverSocket_ = value;
                if (hoverSocket_ != null)
                    hoverSocket_.Hovered = true;
            }
        }

        static GraphSocket dragSocket_ = null;
        public static GraphSocket DragSocket
        {
            get { return dragSocket_; }
            set
            {
                if (dragSocket_ != null)
                    dragSocket_.ActiveRingBrush = dragSocket_.RingBrush;
                dragSocket_ = value;
                if (dragSocket_ != null)
                    dragSocket_.ActiveRingBrush = HoverBrush;
            }
        }
        #endregion

        #region Utility

        GraphSocket HitSocket(Canvas canvas, Point at, bool targetType)
        {
            foreach (var child in canvas.Children)
            {
                if (child is GraphNode)
                {
                    GraphNode graphNode = ((GraphNode)child);
                    if (targetType)
                    {
                        foreach (Data.Graph.GraphSocket dataSocket in graphNode.inputSocketsList.Items)
                        {
                            GraphSocket socket = graphNode.inputSocketsList.FindItemsControlObject<GraphSocket>(dataSocket);
                            if (socket == null)
                                continue;
                            Point topLeft = socket.area.TransformToAncestor(canvas).Transform(new Point(0, 0));
                            var rect = new Rect(topLeft, new Size(socket.area.ActualWidth, socket.area.ActualHeight));
                            if (rect.Contains(at))
                                return socket;
                        }
                        //foreach (GraphSocket socket in ((GraphNode)child).inputSockets.Children)
                        //{
                        //    Point topLeft = socket.TransformToAncestor(canvas).Transform(new Point(0, 0));
                        //    var rect = new Rect(topLeft, new Size(socket.ActualWidth, socket.ActualHeight));
                        //    if (rect.Contains(at))
                        //        return socket;
                        //}
                    }
                    else
                    {
                        foreach (Data.Graph.GraphSocket dataSocket in ((GraphNode)child).outputSocketsList.Items)
                        {
                            GraphSocket socket = graphNode.outputSocketsList.FindItemsControlObject<GraphSocket>(dataSocket);
                            if (socket == null)
                                continue;
                            Point topLeft = socket.area.TransformToAncestor(canvas).Transform(new Point(0, 0));
                            var rect = new Rect(topLeft, new Size(socket.area.ActualWidth, socket.area.ActualHeight));
                            if (rect.Contains(at))
                                return socket;
                        }
                        //foreach (GraphSocket socket in ((GraphNode)child).outputSockets.Children)
                        //{
                        //    Point topLeft = socket.TransformToAncestor(canvas).Transform(new Point(0, 0));
                        //    var rect = new Rect(topLeft, new Size(socket.ActualWidth, socket.ActualHeight));
                        //    if (rect.Contains(at))
                        //        return socket;
                        //}
                    }
                }
            }
            return null;
        }

        #endregion
    }

    public class SocketRingBrushValueConverter : IValueConverter
    {
        internal static Dictionary<uint, KeyValuePair<Brush, Brush> > Brushes = new Dictionary<uint, KeyValuePair<Brush, Brush>>()
        {
            { 1, new KeyValuePair<Brush,Brush>(
                new SolidColorBrush(Color.FromRgb(30,30,30)), 
                new SolidColorBrush(Colors.Gray))
            },
            { 2, new KeyValuePair<Brush,Brush>(
                    new LinearGradientBrush(new GradientStopCollection(new GradientStop[] {
                        new GradientStop(Colors.Red, 0.0), new GradientStop(Colors.LimeGreen, 0.5), new GradientStop(Colors.Blue, 1.0) }), 0.0),
                    new SolidColorBrush(Colors.Gold)) 
            },
            { 3, new KeyValuePair<Brush,Brush>(
                new SolidColorBrush(Colors.DarkRed), 
                new SolidColorBrush(Colors.Orange))
            },
            {
                4, new KeyValuePair<Brush, Brush>(
                new SolidColorBrush(Colors.DarkGreen),
                new SolidColorBrush(Colors.LimeGreen))
            }
        };
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ;
            var socket = value as Data.Graph.GraphSocket;
            if (socket != null)
            {
                KeyValuePair<Brush, Brush> ret;
                if (Brushes.TryGetValue(socket.TypeID, out ret))
                    return ret.Key;
                return Colors.CornflowerBlue;
            }
            return null;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { return null; }
    }

    public class SocketFillBrushValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var socket = value as Data.Graph.GraphSocket;
            if (socket != null)
            {
                KeyValuePair<Brush, Brush> ret;
                if (SocketRingBrushValueConverter.Brushes.TryGetValue(socket.TypeID, out ret))
                    return ret.Value;
                return new SolidColorBrush(Colors.White);
            }
            return null;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { return null; }
    }
}
