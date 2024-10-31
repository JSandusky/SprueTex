using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace SprueKit.Controls.GraphParts
{
    public class ConnectorShape : Shape
    {
        IOCDependency<DocumentManager> documentManager_ = new IOCDependency<DocumentManager>();

        static SolidColorBrush UnselectedBrush = new SolidColorBrush(Colors.CornflowerBlue);
        static SolidColorBrush SelectedBrush = new SolidColorBrush(Colors.Yellow);

        public static readonly DependencyProperty StartProperty = DependencyProperty.Register("Start",
            typeof(System.Windows.Point), typeof(ConnectorShape));
        public static readonly DependencyProperty EndProperty = DependencyProperty.Register("End",
            typeof(System.Windows.Point), typeof(ConnectorShape));
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected",
            typeof(bool), typeof(ConnectorShape), new PropertyMetadata(false));

        public System.Windows.Point Start { get { return (System.Windows.Point)GetValue(StartProperty); } set { SetValue(StartProperty, value); InvalidateVisual(); } }
        public System.Windows.Point End { get { return (System.Windows.Point)GetValue(EndProperty); } set { SetValue(EndProperty, value); InvalidateVisual(); } }
        public bool IsSelected { get { return (bool)GetValue(IsSelectedProperty); } set {
                SetValue(IsSelectedProperty, value);
                Stroke = IsSelected ? SelectedBrush : UnselectedBrush;
        } }
        public bool DragMode { get; set; } = true;
        public GraphControl Graph { get; private set; }
        public GraphSocket A { get; set; }
        public GraphSocket B { get; set; }
        public Data.Graph.GraphConnection Connection { get; set; }

        public ConnectorShape(GraphControl graph, GraphSocket a)
        {
            VisualEdgeMode = EdgeMode.Aliased;
            Stroke = new SolidColorBrush(Colors.CornflowerBlue);
            StrokeThickness = 4;
            Graph = graph;
            A = a;
            Canvas.SetZIndex(this, 2);
            MouseLeftButtonDown += ConnectorShape_MouseLeftButtonDown;
            MouseLeftButtonUp += ConnectorShape_MouseLeftButtonUp;
            UpdateConnectivity(true);
            RenderTransform = graph.nodeTranslation;
        }

        public ConnectorShape(GraphControl graph, GraphSocket a, GraphSocket b)
        {
            VisualEdgeMode = EdgeMode.Aliased;
            Stroke = new SolidColorBrush(Colors.CornflowerBlue);
            StrokeThickness = 4;
            Graph = graph;
            A = a;
            B = b;
            MouseLeftButtonDown += ConnectorShape_MouseLeftButtonDown;
            MouseLeftButtonUp += ConnectorShape_MouseLeftButtonUp;
            UpdateConnectivity(true);
            RenderTransform = graph.nodeTranslation;
        }

        public void ClearGeometryCache()
        {
            cache_ = null;
        }

        private void ConnectorShape_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void ConnectorShape_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Graph != null)
                Graph.Focus();
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
            {
                var route = Graph.BackingGraph.GetOrCreateRoute(Connection);
                var hitPt = e.GetPosition(Graph.canvas);
                var hitAsVec2 = new Vector2((float)(hitPt.X - Graph.nodeTranslation.X), (float)(hitPt.Y - Graph.nodeTranslation.Y));

                List <Vector2> effectivePts = new List<Vector2>();
                effectivePts.Add(Start.ToVector2());
                effectivePts.AddRange(route.RoutingPoints);
                effectivePts.Add(End.ToVector2());

                

                float shortest = float.MaxValue;
                int shortestIDX = 0;
                for (int i = 0; i < effectivePts.Count - 1; ++i)
                {
                    var closestPt = hitAsVec2.ClosestPoint(effectivePts[i], effectivePts[i + 1]);
                    float dist = Vector2.DistanceSquared(closestPt, hitAsVec2);
                    if (dist < shortest)
                    {
                        shortest = dist;
                        shortestIDX = i;
                    }
                }

                if (shortestIDX == 0) // if in first segment then insert at front
                    route.RoutingPoints.Insert(0, hitAsVec2);
                else if (shortestIDX == effectivePts.Count - 2) // if in last segment then we go at the end
                    route.RoutingPoints.Add(hitAsVec2);
                else // if in a different segment then insert before the second shortest
                    route.RoutingPoints.Insert(shortestIDX, hitAsVec2);

                Graph.BackingGraph.Routes.SignalRecordChange();

                int idx = route.RoutingPoints.IndexOf(hitAsVec2);
                documentManager_.Object.ActiveDocument.UndoRedo.Add(new Commands.RouteUndo(Graph.BackingGraph, Connection, hitAsVec2, idx, true));
            }
            else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                Graph.SetSelected(this, !IsSelected);
            else
            {
                Graph.ClearSelection();
                Graph.SetSelected(this, true);
            }
            e.Handled = true;
        }

        static Pen hitTestPen = null;
        public bool Hit(System.Windows.Point pt)
        {
            if (hitTestPen == null)
                hitTestPen = new Pen(new SolidColorBrush(), 8);
            return DefiningGeometry.StrokeContains(hitTestPen, pt);
        }

        Geometry cache_;
        protected override Geometry DefiningGeometry
        {
            get
            {
                if (cache_ == null)
                {
                    PathGeometry geometry = new PathGeometry();

                    Point graphOffset = new Point(Graph.nodeTranslation.X, Graph.nodeTranslation.Y);
                    var diff = End - Start;

                    bool midIsOnRight = (diff.X * 0.5 > 0) || DragMode;

                    if ((midIsOnRight && diff.X < 80 && Math.Abs(diff.Y) < 60) || (Math.Abs(diff.X) < 10 && Math.Abs(diff.Y) < 10))
                    {
                        PathFigure path = new PathFigure();
                        path.StartPoint = Start;
                        path.IsClosed = false;
                        geometry.Figures.Add(path);

                        LineSegment line = new LineSegment(Start, true);
                        path.Segments.Add(line);
                        line = new LineSegment(End, true);
                        path.Segments.Add(line);
                        cache_ = geometry;
                        return cache_;
                    }

                    PathFigure pathFigure = new PathFigure();
                    pathFigure.StartPoint = Start;
                    pathFigure.IsClosed = false;
                    geometry.Figures.Add(pathFigure);
                    var route = Graph.BackingGraph.GetRoute(Connection);
                    if (route != null && route.RoutingPoints.Count > 0)
                    {
                        pathFigure.Segments.Add(new LineSegment(Start, true));
                        pathFigure.Segments.Add(new LineSegment(Start + new Vector(20, 0), true));
                        for (int i = 0; i < route.RoutingPoints.Count; ++i)
                            pathFigure.Segments.Add(new LineSegment(route.RoutingPoints[i].ToWindowsPoint(), true));
                        pathFigure.Segments.Add(new LineSegment(End - new Vector(20, 0), true));
                        pathFigure.Segments.Add(new LineSegment(End, true));
                    }
                    else
                    {
                        PolyBezierSegment curve = new PolyBezierSegment(new System.Windows.Point[]{
                        Start,
                        Start + new System.Windows.Vector(midIsOnRight ? 50 : 150, 0),
                        Start + diff * 0.5,
                        End - diff * 0.5,
                        End - new System.Windows.Vector(midIsOnRight ? 50 : 150, 0),
                        End
                    }, true);
                        pathFigure.Segments.Add(curve);
                    }
                    geometry.Freeze();
                    //BezierSegment curve = new BezierSegment(Start, Start + diff * 0.5, End, true);
                    cache_ = geometry;
                }
                return cache_;
            }
        }

        public void UpdateConnectivity(bool force)
        {
            if (A != null && B != null)
            {
                bool changed = false;
                var newA = SocketLocation(A);
                var newB = SocketLocation(B);
                if (newA.Distance(Start) > 0.001f)
                    changed = true;
                if (newB.Distance(End) > 0.001f)
                    changed = true;
                Start = SocketLocation(A);
                End = SocketLocation(B);
                if (changed || force)
                    cache_ = null;
            }
        }

        private Point SocketLocation(GraphSocket socket)
        {
            var pt = socket.area.TransformToAncestor(socket.OwnerNode.OwnerCanvas).Transform(new Point(socket.area.ActualWidth / 2, socket.area.ActualHeight / 2));
            pt.X -= socket.OwnerNode.OwnerControl.nodeTranslation.X;
            pt.Y -= socket.OwnerNode.OwnerControl.nodeTranslation.Y;
            return pt;
            //return socket.area.TransformToAncestor(socket.OwnerNode.OwnerCanvas).Transform(new Point(socket.area.ActualWidth / 2, socket.area.ActualHeight / 2));
        }
    }
}
