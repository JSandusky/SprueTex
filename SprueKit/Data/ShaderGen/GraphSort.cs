using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SprueKit.Data.Graph;

namespace SprueKit.Data.ShaderGen
{
    public static class GraphSort
    {
        public static Dictionary<Graph.GraphNode, int> GetUpstreamDepths(Graph.GraphNode relativeTo)
        {
            Dictionary<Graph.GraphNode, int> depths = new Dictionary<Graph.GraphNode, int>();

            relativeTo.TraceUpstream(new Action<Graph.GraphNode, int>((Graph.GraphNode nd, int depth) =>
            {
                if (depths.ContainsKey(nd))
                    depths[nd] = Math.Max(depths[nd], depth);
                else
                    depths[nd] = depth;
            }));

            return depths;
        }

        public static List<Graph.GraphNode> UpstreamDepthSort(Graph.GraphNode relativeTo)
        {
            var depthTable = GetUpstreamDepths(relativeTo);
            List<Graph.GraphNode> ret = new List<Graph.GraphNode>();
            foreach (var kvp in depthTable)
                ret.Add(kvp.Key);
            ret.Sort(new MaxDepthComparer(depthTable));
            return ret;
        }

        internal class MaxDepthComparer : IComparer<Graph.GraphNode>
        {
            Dictionary<Graph.GraphNode, int> depthTable_;
            internal MaxDepthComparer(Dictionary<Graph.GraphNode, int> depthTable)
            {
                depthTable_ = depthTable;
            }

            public int Compare(GraphNode x, GraphNode y)
            {
                int lhs = depthTable_[x];
                int rhs = depthTable_[y];
                if (lhs > rhs)
                    return -1;
                else if (rhs > lhs)
                    return 1;
                return 0;
            }
        }

    }
}
