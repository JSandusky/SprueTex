using System.Collections.Generic;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace SprueKit.Data.Graph
{
    /// <summary>
    /// Connectivity between a pair of nodes.
    /// </summary>
    public class GraphConnection
    {
        public GraphNode FromNode { get; set; }
        public GraphSocket FromSocket { get; set; }
        public GraphNode ToNode { get; set; }
        public GraphSocket ToSocket { get; set; }

        public bool IsValid { get { return FromNode != null && ToNode != null && FromSocket != null && ToSocket != null; } }

        public double StartVisualX { get { return FromSocket.VisualX; } }
        public double StartVisualY { get { return FromSocket.VisualY; } }

        public double EndVisualX { get { return ToSocket.VisualX; } }
        public double EndVisualY { get { return ToSocket.VisualY; } }
    }

    /// <summary>
    /// Routes are stored in a dictionary keyed by GraphConnection
    /// </summary>
    public class ConnectionRouting
    {
        public List<Vector2> RoutingPoints { get; private set; } = new List<Vector2>();
    }
}
