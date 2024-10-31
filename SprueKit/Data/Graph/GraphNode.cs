using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;

namespace SprueKit.Data.Graph
{
    public class GraphNode : BaseClass
    {
        /// <summary>
        /// Construct, note that the Construct() method and Graph.AddNode() need to be called
        /// to truly construct a node
        /// </summary>
        public GraphNode() { }

        /// <summary>
        /// Construct is responsible for initial construction of a node. 
        /// Sockets are serialized, this is necessary for version updates as well as variable sockets.
        /// </summary>
        public virtual void Construct() { }

        /// <summary>
        /// Graph this node belongs to, may be 'late' or 'previous'
        /// </summary>
        [Notify.TrackMember(IsExcluded = true)]
        [PropertyData.PropertyIgnore]
        public Graph Graph { get; set; }

        [Notify.TrackMember(IsExcluded = true)]
        [PropertyData.PropertyIgnore]
        public int Version { get; set; } = 1;

        /// <summary>
        /// Node that this node was cloned from. 
        /// Cloning of graphs should be used when threading operations or performing graph cuts.
        /// </summary>
        [Notify.TrackMember(IsExcluded =true)]
        [PropertyData.PropertyIgnore]
        public GraphNode CloneSource { get; protected set; }

        /// <summary>
        /// Identifying name of this node. A default name should be specified in Construct()
        /// </summary>
        [PropertyData.PropertyPriority(0)]
        [PropertyData.VisualConsequence(PropertyData.VisualStage.None)]
        public string Name { get { return name_; } set { name_ = value; OnPropertyChanged("DisplayName"); OnPropertyChanged(); } }
        string name_ = "";

        [PropertyData.PropertyIgnore]
        public virtual string DisplayName { get { return name_; } }

        string description_ = "";
        [PropertyData.PropertyPriority(1)]
        [PropertyData.VisualConsequence(PropertyData.VisualStage.None)]
        public string Description { get { return description_; } set { description_ = value; OnPropertyChanged(); OnPropertyChanged("HasDescription"); } }

        [PropertyData.PropertyIgnore]
        [Notify.TrackMember(IsExcluded =true)]
        public bool HasDescription { get { return !string.IsNullOrWhiteSpace(description_); } }

        /// <summary>
        /// Internal identifier of this node. Important for cloning and serialization.
        /// </summary>
        [PropertyData.PropertyIgnore]
        [Notify.TrackMember(IsExcluded = true)]
        public int NodeID { get; set; } = -1;

        /// <summary>
        /// GUI-side visual position
        /// </summary>
        [PropertyData.PropertyIgnore]
        [Notify.TrackMember(IsExcluded = true)]
        public double VisualX { get; set; }
        /// <summary>
        /// GUI-side visual position
        /// </summary>
        [PropertyData.PropertyIgnore]
        [Notify.TrackMember(IsExcluded = true)]
        public double VisualY { get; set; }

        /// <summary>
        /// Indicates if this node is an 'Entrypoint'
        /// EntryPoints are redundantly stored in a list in the Graph so that queries for
        /// entry points aren't penalized by large graph sizes.
        /// </summary>
        [PropertyData.PropertyIgnore]
        [Notify.TrackMember(IsExcluded = true)]
        public bool EntryPoint { get; protected set; } = false;

        /// <summary>
        /// Indicates this node is an 'EventPoint'
        /// Similar reasons to an entrypoint ... probably redundant on top of EntryPoint
        /// Consider removal
        /// </summary>
        [PropertyData.PropertyIgnore]
        [Notify.TrackMember(IsExcluded = true)]
        public bool EventPoint { get; protected set; } = false;

        /// <summary>
        /// List of input sockets, DO NOT DIRECTLY ADD OR REMOVE SOCKETS without knowing what you're doing
        /// </summary>
        [PropertyData.PropertyIgnore]
        public ObservableCollection<GraphSocket> InputSockets { get; private set; } = new ObservableCollection<GraphSocket>();
        /// <summary>
        /// List of output sockets, DO NOT DIRECTLY ADD OR REMOVE SOCKETS without knowing what you're doing
        /// </summary>
        [PropertyData.PropertyIgnore]
        public ObservableCollection<GraphSocket> OutputSockets { get; private set; } = new ObservableCollection<GraphSocket>();

        #region Editor GUI Code
        public event EventHandler NodeChanged;

        /// <summary>
        /// Notify the GUI that this node has changed.
        /// </summary>
        public void SignalChanged()
        {
            if (NodeChanged != null)
                NodeChanged(this, null);
        }
        #endregion

        /// <summary>
        /// Adds an input socket to the node, use this method to take care of IDs at runtime
        /// </summary>
        public void AddInput(GraphSocket socket)
        {
            // prevent errors
            socket.IsInput = true;
            socket.IsOutput = false;
            if (socket.SocketID == -1 && Graph != null)
                socket.SocketID = Graph.GetNextID();
            InputSockets.Add(socket);
        }

        /// <summary>
        /// Adds an output socket to the node, use this method to take care of IDs at runtime
        /// </summary>
        public void AddOutput(GraphSocket socket)
        {
            // prevent errors
            socket.IsInput = false;
            socket.IsOutput = true;
            if (socket.SocketID == -1 && Graph != null)
                socket.SocketID = Graph.GetNextID();
            OutputSockets.Add(socket);
        }

        /// <summary>
        /// Removes a socket from this nodes
        /// </summary>
        public void RemoveSocket(GraphSocket socket)
        {
            if (InputSockets.Contains(socket))
            {
                Graph.Disconnect(socket);
                InputSockets.Remove(socket);
            }
            else if (OutputSockets.Contains(socket))
            {
                Graph.Disconnect(socket);
                OutputSockets.Remove(socket);
            }
        }

        /// <summary>
        /// Called when a connectivity change is made
        /// </summary>
        public virtual void SocketConnectivityChanged(GraphSocket socket)
        {

        }

        /// <summary>
        /// Updates all the socket IDs of those in need
        /// </summary>
        public void UpdateSocketIDs()
        {
            foreach (var socket in InputSockets)
                if (socket.SocketID == -1 && Graph != null)
                    socket.SocketID = Graph.GetNextID();
            foreach (var socket in OutputSockets)
                if (socket.SocketID == -1 && Graph != null)
                    socket.SocketID = Graph.GetNextID();
        }

        /// <summary>
        /// Nodes that will need to manual control graph execution should return true to
        /// prevent the default graph evaluation from executing. 
        /// Prevents wasted work and work that would mangle any caching.
        /// </summary>
        /// <returns>True if this node will force execute</returns>
        public virtual bool WillForceExecute() { return false; }

        // Identifier is used to prevent circular calls
        protected int lastExecutionContext = -1;

        /// <summary>
        /// Upstream execution traces along input sockets (Right -> Left).
        /// Nodes that are upstream are executed first, followed by the current node.
        /// A texture graph is a good example
        /// </summary>
        /// <param name="ctx">The execution context for circular call prevention</param>
        /// <param name="param">Arbitrary parameter data</param>
        public void ExecuteUpstream(int ctx, object param)
        {
            if (ctx != -1 && lastExecutionContext == ctx)
                return;
            lastExecutionContext = ctx;
            if (!WillForceExecute())
            {
                foreach (var input in InputSockets)
                {
                    if (input.CachedInputSource != null)
                    {
                        input.CachedInputSource.Node.ExecuteUpstream(ctx, param);
                        input.Data = input.CachedInputSource.Data;
                    }
                    //??var upstream = Graph.Connections.Where(o => o.ToSocket == input);
                    //??if (upstream != null)
                    //??{
                    //??    foreach (var upSocket in upstream)
                    //??        upSocket.FromNode.ExecuteUpstream(ctx, param);
                    //??    TransferSocketData(upstream, true);
                    //??}
                }
            }
            Execute(param);
        }

        /// <summary>
        /// Executes all upstream sockets and copies data across the connections.
        /// Can detonate from cycles
        /// </summary>
        /// <param name="param">Arbitrary parameter data</param>
        public void ForceExecuteUpstream(object param)
        {
            // Evaluate upstream nodes first
            foreach (var input in InputSockets)
            {
                if (input.CachedInputSource != null)
                {
                    input.CachedInputSource.Node.ExecuteUpstream(-1, param);
                    input.Data = input.CachedInputSource.Data;
                }
                //var upstream = Graph.Connections.Where(o => o.ToSocket == input);
                //if (upstream != null)
                //{
                //    foreach (var upSocket in upstream)
                //        upSocket.FromNode.ExecuteUpstream(-1, param);
                //    TransferSocketData(upstream, true);
                //}
            }
        }

        public void ForceExecuteSocketUpstream(object param, GraphSocket socket)
        {
            // Evaluate upstream nodes first
            if (socket.CachedInputSource != null)
            {
                socket.CachedInputSource.Node.ExecuteUpstream(-1, param);
                socket.Data = socket.CachedInputSource.Data;
            }
        }

        public void ForceExecuteSocketDownstream(GraphSocket socket)
        {

        }

        /// <summary>
        /// Executes the graph in a downstream (Left -> Right) fashion.
        /// The current node is executed first, followed by the nodes connected
        /// to this node's output sockets.
        /// </summary>
        /// <param name="param"></param>
        public void ExecuteDownstream(int ctx, object param)
        {
            if (ctx != -1 && lastExecutionContext == ctx)
                return;
            lastExecutionContext = ctx;
            Execute(param);
            foreach (var output in OutputSockets)
            {
                var outputs = Graph.Connections.Where(o => o.FromSocket == output);
                if (outputs != null)
                {
                    TransferSocketData(outputs, true);
                    foreach (var outSocket in outputs)
                        outSocket.ToNode.ExecuteDownstream(ctx, param);
                }
            }
        }

        /// <summary>
        /// Executes the given flow socket, should only be called from inside Execute
        /// Cycles can detonate it
        /// </summary>
        public void ExecuteFlow(GraphSocket socket, object param)
        {
            if (!socket.IsFlow)
                return;

            // Execute the nodes connected to this socket
            var flowOutput = Graph.Connections.Where(o => o.FromSocket == socket);
            if (flowOutput != null && flowOutput.Count() > 0)
            {
                foreach (var output in flowOutput)
                {
                    // get the socket data this node needs
                    var inputs = Graph.Connections.Where(c => c.ToNode == output.ToNode);
                    if (inputs != null && inputs.Count() > 0)
                        TransferSocketData(inputs, true);

                    // Execute the node
                    output.ToNode.Execute(param);
                }
            }
        }

        /// <summary>
        /// Prime this graph node before executing. Use this as a chance to perform slow tasks that shouldn't
        /// be done for each socket or clear any caches.
        /// </summary>
        /// <param name="param">Arbitrary user data</param>
        public virtual void PrimeBeforeExecute(object param)
        {

        }

        public virtual void PrimeEarly(object param)
        {

        }

        /// <summary>
        /// Performs the main execution work that this specific node needs to perform.
        /// </summary>
        /// <param name="param">Arbitrary user data</param>
        public virtual void Execute(object param)
        {

        }

        /// <summary>
        /// Utility for safe data storage access
        /// </summary>
        public void StoreOutput(int index, object data)
        {
            if (index < OutputSockets.Count)
                OutputSockets[index].Data = data;
        }

        /// <summary>
        /// Utility for safe data storage access
        /// </summary>
        public object GetOutput(int index, object defaultData)
        {
            if (index < OutputSockets.Count)
                return OutputSockets[index].Data != null ? OutputSockets[index].Data : defaultData;
            return defaultData;
        }

        /// <summary>
        /// Utility for safe data storage access
        /// </summary>
        public void StoreInput(int index, object data)
        {
            if (index < InputSockets.Count)
                InputSockets[index].Data = data;
        }

        /// <summary>
        /// Utility for safe data storage access
        /// </summary>
        public object GetInput(int index, object defaultData = null)
        {
            if (index < InputSockets.Count)
                return InputSockets[index].Data != null ? InputSockets[index].Data : defaultData;
            return defaultData;
        }

        /// <summary>
        /// Copies data from one side of a set of connections to the other
        /// </summary>
        /// <param name="connections">List of connections to process</param>
        /// <param name="fromOutputs">Whether copying outputs->inputs or inputs->outputs</param>
        protected void TransferSocketData(IEnumerable<GraphConnection> connections, bool fromOutputs)
        {
            if (fromOutputs)
            {
                foreach (var conn in connections)
                    conn.ToSocket.Data = conn.FromSocket.Data;
            }
            else
            {
                foreach (var conn in connections)
                    conn.FromSocket.Data = conn.ToSocket.Data;
            }
        }

        /// <summary>
        /// Traverses the graph upstream while invoking the given action on all nodes
        /// </summary>
        public void TraceUpstream(Action<GraphNode, int> act) { TraceUpstream(act, new HashSet<GraphNode>()); }
        public void TraceUpstream(Action<GraphNode, int> act, HashSet<GraphNode> hitSet, int depth = 0)
        {
            act(this, depth);

            // We can hit ourself multiple times but we will not follow onward after a rehit
            if (hitSet.Contains(this))
                return;
            hitSet.Add(this);

            foreach (var input in InputSockets)
            {
                var upstream = Graph.Connections.Where(o => o.ToSocket == input);
                if (upstream != null)
                {
                    foreach (var upSocket in upstream)
                        upSocket.FromNode.TraceUpstream(act, hitSet, depth + 1);
                }
            }
        }

        /// <summary>
        /// Traverses the graph downstream while invoking the given action on all nodes
        /// </summary>
        public void TraceDownstream(Action<GraphNode, int> act) { TraceDownstream(act, new HashSet<GraphNode>()); }
        public void TraceDownstream(Action<GraphNode, int> act, HashSet<GraphNode> hitSet, int depth = 0)
        {
            act(this, depth);

            // no recursion allowed, we can hit ourself multiple times but will not trace into a cycle
            if (hitSet.Contains(this))
                return;
            hitSet.Add(this);
            foreach (var output in OutputSockets)
            {
                var outputs = Graph.Connections.Where(o => o.FromSocket == output);
                if (outputs != null)
                {
                    foreach (var outSocket in outputs)
                        outSocket.ToNode.TraceDownstream(act, hitSet, depth + 1);
                }
            }
        }

        /// <summary>
        /// MUST BE IMPLEMENTED IN DERIVED TYPES
        /// </summary>
        public virtual GraphNode Clone()
        {
            return null;
        }

        /// <summary>
        /// Copies this nodes fields into the given node.
        /// </summary>
        protected virtual void CloneFields(GraphNode node)
        {
            node.NodeID = NodeID;
            node.Name = Name;
            node.Description = Description;
            node.EventPoint = EventPoint;
            node.EntryPoint = EntryPoint;
            node.VisualX = VisualX;
            node.VisualY = VisualY;
            foreach (var sock in InputSockets)
                node.InputSockets.Add(sock.Clone(node));
            foreach (var sock in OutputSockets)
                node.OutputSockets.Add(sock.Clone(node));
            node.PostClone(this);
        }

        public virtual void PreSerialize() { }
        public virtual void PostDeserialize() { }
        public virtual void PostClone(GraphNode source) { }

        public virtual void Serialize(SerializationContext ctx, System.Xml.XmlElement into)
        {
            PreSerialize();
            SerializeProperties(ctx, into);
            foreach (var socket in InputSockets)
                socket.Serialize(into);
            foreach (var socket in OutputSockets)
                socket.Serialize(into);
        }

        public virtual void Deserialize(SerializationContext ctx, System.Xml.XmlElement from)
        {
            DeserializeProperties(ctx, from);
            var sockets = from.SelectNodes("socket");
            foreach (System.Xml.XmlElement socketElem in sockets)
            {
                var socket = GraphSocket.Deserialize(this, socketElem);
                if (socket.IsInput)
                    InputSockets.Add(socket);
                else
                    OutputSockets.Add(socket);
            }
            PostDeserialize();
        }

        public virtual void Serialize(SerializationContext ctx, System.IO.BinaryWriter strm)
        {
            PreSerialize();
            SerializeProperties(ctx, strm);
            strm.Write(InputSockets.Count);
            foreach (var socket in InputSockets)
                socket.Serialize(strm);
            strm.Write(OutputSockets.Count);
            foreach (var socket in OutputSockets)
                socket.Serialize(strm);
        }

        public virtual void Deserialize(SerializationContext ctx, System.IO.BinaryReader strm)
        {
            DeserializeProperties(ctx, strm);
            int inputCt = strm.ReadInt32();
            for (int i = 0; i < inputCt; ++i)
                InputSockets.Add(GraphSocket.Deserialize(this, strm));

            int outputCt = strm.ReadInt32();
            for (int i = 0; i < outputCt; ++i)
                OutputSockets.Add(GraphSocket.Deserialize(this, strm));
            PostDeserialize();
        }

        public virtual void SerializeProperties(SerializationContext ctx, System.Xml.XmlElement into) { }
        public virtual void DeserializeProperties(SerializationContext ctx, System.Xml.XmlElement into) { }
        public virtual void SerializeProperties(SerializationContext ctx, System.IO.BinaryWriter strm) { }
        public virtual void DeserializeProperties(SerializationContext ctx, System.IO.BinaryReader strm) { }
    }

    // Marker intermediate base class for identifying the inputs of a sub-graph
    public abstract class SubGraphInputNode : GraphNode
    {

    }

    // Marker intermediate base class for identifying the outputs of a sub-graph
    public abstract class SubGraphOutputNode : GraphNode
    {

    }

    /// <summary>
    /// A subgraph is used to simplify graph complexity by embedding a graph within the graph
    /// </summary>
    public class SubGraphNode : GraphNode
    {
        Graph subGraph_;
        Uri path_;

        [PropertyData.PropertyIgnore]
        [Notify.TrackMember(IsExcluded = true)]
        public Graph SubGraph { get { return subGraph_; } }

        public Uri GraphFile { get { return path_; } set { path_ = value; SubgraphChanged(); OnPropertyChanged(); } }

        // Construct a new subgraph
        public SubGraphNode() { subGraph_ = new Graph(); }

        /// <summary>
        /// Construct a subgraph from the selected collection of nodes
        /// </summary>
        /// <param name="nodes">List of ndoes to transfer</param>
        /// <param name="connections">List of connections to transfer</param>
        /// <param name="doNotRemove">Selected items will be left in the source graph</param>
        public SubGraphNode(List<GraphNode> nodes, List<GraphConnection> connections, bool doNotRemove)
        {
            subGraph_ = new Graph();
            foreach (var node in nodes)
            {
                var clone = node.Clone();
                subGraph_.AddNode(clone);
            }

            foreach (var con in connections)
            {
                var lhsNode = subGraph_.Nodes.FirstOrDefault(n => n.NodeID == con.FromNode.NodeID);
                var rhsNode = subGraph_.Nodes.FirstOrDefault(n => n.NodeID == con.ToNode.NodeID);
                if (lhsNode == null || rhsNode == null)
                    continue;
                var lhsSocket = lhsNode.OutputSockets.FirstOrDefault(s => s.SocketID == con.FromSocket.SocketID);
                var rhsSocket = lhsNode.InputSockets.FirstOrDefault(s => s.SocketID == con.ToSocket.SocketID);
                if (lhsSocket == null || rhsSocket == null)
                    continue;

                subGraph_.Connections.Add(new GraphConnection
                {
                    FromNode = lhsNode,
                    ToNode = rhsNode,
                    FromSocket = lhsSocket,
                    ToSocket = rhsSocket
                });
            }
            subGraph_.CompressIndices();
        }

        public override void Construct()
        {
            base.Construct();
            Name = "Subgraph";
        }

        void SubgraphChanged()
        {
            if (subGraph_ == null)
            {
                while (InputSockets.Count > 0)
                    RemoveSocket(InputSockets[0]);
                while (OutputSockets.Count > 0)
                    RemoveSocket(OutputSockets[0]);
            }
            List<SubGraphInputNode> inputs = new List<SubGraphInputNode>();
            List<SubGraphOutputNode> outputs = new List<SubGraphOutputNode>();
            foreach (var node in subGraph_.Nodes)
            {
                if (node is SubGraphInputNode)
                    inputs.Add(node as SubGraphInputNode);
                else if (node is SubGraphOutputNode)
                    outputs.Add(node as SubGraphOutputNode);
            }

            while (InputSockets.Count > inputs.Count)
                RemoveSocket(InputSockets[InputSockets.Count - 1]);
            while (OutputSockets.Count > inputs.Count)
                RemoveSocket(OutputSockets[OutputSockets.Count - 1]);
            for (int i = 0; i < inputs.Count; ++i)
            {
                if (i < InputSockets.Count) // reuse existing socket
                {
                    InputSockets[i].Name = inputs[i].Name;
                    InputSockets[i].TypeID = inputs[i].OutputSockets[0].TypeID;
                }
                else
                    AddInput(new GraphSocket(this) { Name = inputs[i].Name, TypeID = inputs[i].OutputSockets[0].TypeID });
            }
            for (int i = 0; i < outputs.Count; ++i)
            {
                if (i < OutputSockets.Count) // reuse existing socket
                {
                    OutputSockets[i].Name = inputs[i].Name;
                    OutputSockets[i].TypeID = outputs[i].InputSockets[0].TypeID;
                }
                else
                    AddOutput(new GraphSocket(this) { Name = inputs[i].Name, TypeID = outputs[i].InputSockets[0].TypeID });
            }
        }

        public override void DeserializeProperties(SerializationContext ctx, BinaryReader strm)
        {
            base.DeserializeProperties(ctx, strm);
            bool hasSub = strm.ReadBoolean();
            if (hasSub)
            {
                subGraph_ = new Graph();
                subGraph_.Deserialize(ctx, strm);
            }
        }

        public override void DeserializeProperties(SerializationContext ctx, XmlElement into)
        {
            base.DeserializeProperties(ctx, into);
            var graphElem = into.SelectSingleNode("graph") as XmlElement;
            if (graphElem != null)
            {
                subGraph_ = new Graph();
                subGraph_.Deserialize(ctx, graphElem);
            }
        }

        public override void SerializeProperties(SerializationContext ctx, BinaryWriter strm)
        {
            base.SerializeProperties(ctx, strm);
            strm.Write(subGraph_ != null);
            if (subGraph_ != null)
                subGraph_.Serialize(ctx, strm);
        }

        public override void SerializeProperties(SerializationContext ctx, XmlElement into)
        {
            base.SerializeProperties(ctx, into);
            if (subGraph_ != null)
            {
                var graphElem = into.CreateChild("graph");
                subGraph_.Serialize(ctx, graphElem);
            }
        }

        protected override void CloneFields(GraphNode node)
        {
            base.CloneFields(node);
            ((SubGraphNode)node).subGraph_ = subGraph_.Clone();
        }
    }
}
