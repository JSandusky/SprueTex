using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SprueKit.Controls.GraphParts
{
    /// <summary>
    /// Interaction logic for RouteThumb.xaml
    /// </summary>
    public partial class RouteThumb : Thumb
    {
        Data.Graph.Graph graph_;
        Data.Graph.GraphConnection connection_;
        Data.Graph.ConnectionRouting route_;
        GraphControl control_;
        int ptIndex_;
        public RouteThumb(GraphControl ctrl, Data.Graph.Graph graph, Data.Graph.GraphConnection conn, Data.Graph.ConnectionRouting route, int thumbIndex)
        {
            InitializeComponent();
            control_ = ctrl;
            connection_ = conn;
            Canvas.SetZIndex(this, -1);
            graph_ = graph;
            route_ = route;
            ptIndex_ = thumbIndex;

            SetPosition();

            this.AddHandler(Control.PreviewMouseDoubleClickEvent,
             new MouseButtonEventHandler((target, args) =>
             {
                 IOCDependency<DocumentManager> docMan = new IOCDependency<DocumentManager>();
                 docMan.Object.ActiveDocument.UndoRedo.Add(new Commands.RouteUndo(graph_, connection_, route_.RoutingPoints[ptIndex_], ptIndex_, false));

                 route_.RoutingPoints.RemoveAt(ptIndex_);
                 if (route_.RoutingPoints.Count == 0)
                 {
                     var match = graph_.Routes.Where(k => k.Value == route_).FirstOrDefault();
                     if (match.Key != null && match.Value != null)
                         graph_.Routes.Remove(match);
                 }
                 else
                     graph_.Routes.SignalRecordChange();
                 args.Handled = true;
             }), false);
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var pt = route_.RoutingPoints[ptIndex_];
            var oldPt = pt;
            pt.X += (float)e.HorizontalChange;
            pt.Y += (float)e.VerticalChange;
            route_.RoutingPoints[ptIndex_] = pt;
            SetPosition();
            control_.UpdatePositions(true);
            e.Handled = true;

            IOCDependency<DocumentManager> docMan = new IOCDependency<DocumentManager>();
            docMan.Object.ActiveDocument.UndoRedo.Add(new Commands.RouteMoveUndo(graph_, connection_, oldPt, pt, ptIndex_));
        }

        void SetPosition()
        {
            var pt = route_.RoutingPoints[ptIndex_];
            Canvas.SetLeft(this, pt.X);
            Canvas.SetTop(this, pt.Y);
        }
    }
}
