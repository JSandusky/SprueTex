using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Microsoft.Xna.Framework;

namespace SprueKit.Commands
{
    public class GraphUndo : UndoRedoCmd
    {
        public double X;
        public double Y;
        public override ImageSource Icon { get { return WPFExt.GetEmbeddedImage("Images/icon_edit_yellow.png"); } }
        public HashSet<Controls.GraphParts.GraphNode> Nodes = new HashSet<Controls.GraphParts.GraphNode>();
        Controls.GraphControl graph_;

        public GraphUndo(Controls.GraphControl graph)
        {
            graph_ = graph;
        }

        public void SetText()
        {
            if (Nodes.Count == 1)
                this.Message = string.Format("Move {0}", Nodes.FirstOrDefault().BackingData.DisplayName);
            else
                this.Message = string.Format("Move {0} nodes", Nodes.Count);
        }

        public override bool ShouldMerge(UndoRedoCmd cmd)
        {
            if (cmd is GraphUndo)
            {
                var dupe = ((GraphUndo)cmd).Nodes.Intersect(Nodes);
                return dupe.Count() == Nodes.Count;
            }
            return false;
        }

        public override void Merge(UndoRedoCmd cmd)
        {
            X += ((GraphUndo)cmd).X;
            Y += ((GraphUndo)cmd).Y;
        }

        protected override void Execute(bool isRedo)
        {
            if (isRedo)
            {
                foreach (var node in Nodes)
                {
                    node.X += X;
                    node.Y += Y;
                }
            }
            else
            {
                foreach (var node in Nodes)
                {
                    node.X -= X;
                    node.Y -= Y;
                }
            }
            foreach (var node in Nodes)
                node.UpdateConnections();
            graph_.RebuildNodes();
            graph_.RebuildConnectors();
            graph_.UpdateLayout();
        }
    }

    public class RouteUndo : UndoRedoCmd
    {
        Data.Graph.Graph graph_;
        Data.Graph.GraphConnection connection_;
        bool wasCreate_;
        Vector2 point_;
        int ptIndex_ = 0;

        public RouteUndo(Data.Graph.Graph graph, Data.Graph.GraphConnection forCon, Vector2 location, int ptIndex, bool isCreate)
        {
            ptIndex_ = ptIndex;
            graph_ = graph;
            connection_ = forCon;
            point_ = location;
            wasCreate_ = isCreate;

            if (wasCreate_)
                Message = "Create routing point";
            else
                Message = "Remove routing point";
        }

        public override ImageSource Icon { get {
                return wasCreate_ ?
                WPFExt.GetEmbeddedImage("Images/icon_add_green.png") :
                WPFExt.GetEmbeddedImage("Images/icon_remove_red.png");
            }
        }

        public override bool ShouldMerge(UndoRedoCmd cmd) { return false; }
        public override void Merge(UndoRedoCmd cmd) { }

        protected override void Execute(bool isRedo)
        {
            //??if (IsFirstRedo && isRedo)
            //??    return;

            if ((isRedo && wasCreate_) || (!isRedo && !wasCreate_))
            {
                var route = graph_.GetOrCreateRoute(connection_);
                route.RoutingPoints.Insert(ptIndex_, point_);
            }
            else
            {
                Data.Graph.ConnectionRouting route = null;
                if (graph_.Routes.TryGetValue(connection_, out route))
                {
                    route.RoutingPoints.RemoveAt(ptIndex_);
                    if (route.RoutingPoints.Count == 0)
                    {
                        var match = graph_.Routes.Where(k => k.Value == route).FirstOrDefault();
                        if (match.Key != null && match.Value != null)
                            graph_.Routes.Remove(match);
                    }
                }
            }
            graph_.Routes.SignalRecordChange();
        }
    }

    public class RouteMoveUndo : UndoRedoCmd
    {
        Data.Graph.Graph graph_;
        Data.Graph.GraphConnection connection_;
        Vector2 oldPos_;
        Vector2 newPos_;
        int ptIndex_;

        public RouteMoveUndo(Data.Graph.Graph graph, Data.Graph.GraphConnection conn, Vector2 oldPos, Vector2 newPos, int idx)
        {
            graph_ = graph;
            connection_ = conn;
            oldPos_ = oldPos;
            newPos_ = newPos;
            ptIndex_ = idx;
            Message = string.Format("Move graph route point from {0} to {1}", oldPos_.ToTightString(), newPos_.ToTightString());
        }

        public override ImageSource Icon { get { return WPFExt.GetEmbeddedImage("Images/icon_edit_yellow.png"); } }

        public override bool ShouldMerge(UndoRedoCmd cmd)
        {
            RouteMoveUndo u = cmd as RouteMoveUndo;
            if (u != null)
                return ptIndex_ == u.ptIndex_ && connection_ == u.connection_;
            return false;
        }

        public override void Merge(UndoRedoCmd cmd)
        {
            newPos_ = ((RouteMoveUndo)cmd).newPos_;
        }

        protected override void Execute(bool isRedo)
        {
            //??if (IsFirstRedo && isRedo)
            //??    return;

            if (isRedo)
            {
                var route = graph_.GetOrCreateRoute(connection_);
                route.RoutingPoints[ptIndex_] = newPos_;
            }
            else
            {
                var route = graph_.GetOrCreateRoute(connection_);
                route.RoutingPoints[ptIndex_] = oldPos_;
            }
            graph_.Routes.SignalRecordChange();
        }
    }
}
