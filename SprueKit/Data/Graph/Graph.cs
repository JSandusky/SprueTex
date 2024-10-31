using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;

namespace SprueKit.Data.Graph
{
    public class ConnectivityChange
    {
        public GraphSocket From { get; set; }
        public GraphSocket To { get; set; }
        public bool IsDisconnect { get; set; } = false;
    }

    public class Graph : BaseClass
    {
        [PropertyData.PropertyIgnore]
        public bool IsSingleEntry { get; set; }

        int nextID_ = 0;
        public int GetNextID() { return ++nextID_; }
        
        public EventHandler<ConnectivityChange> ConnectivityChanged;
        public EventHandler NodesChanged;

        public Dictionary<string, string> CustomData = new Dictionary<string, string>();

        public string GetCustomData(string key, string defaultValue = "")
        {
            if (CustomData.ContainsKey(key))
                return CustomData[key];
            return defaultValue;
        }

        [PropertyData.PropertyIgnore]
        public ObservableCollection<GraphNode> EntryPoints { get; private set; } = new ObservableCollection<GraphNode>();
        [PropertyData.PropertyIgnore]
        public ObservableCollection<GraphNode> EventEntryPoints { get; private set; } = new ObservableCollection<GraphNode>();

        /// <summary>
        /// Contains all of the nodes of the graph
        /// </summary>
        [PropertyData.PropertyIgnore]
        public ObservableCollection<GraphNode> Nodes { get; private set; } = new ObservableCollection<GraphNode>();

        /// <summary>
        /// Contains all of the connections across the graph
        /// </summary>
        [PropertyData.PropertyIgnore]
        public ObservableCollection<GraphConnection> Connections { get; private set; } = new ObservableCollection<GraphConnection>();

        [PropertyData.PropertyIgnore]
        public ObservableCollection<GraphBox> Boxes { get; private set; } = new ObservableCollection<GraphBox>();

        [PropertyData.PropertyIgnore]
        public PluginLib.ObservableDictionary<GraphConnection, ConnectionRouting> Routes { get; private set; } = new PluginLib.ObservableDictionary<GraphConnection, ConnectionRouting>();

        public void AddNode(GraphNode node)
        {
            node.Graph = this;
            if (node.NodeID == -1)
            {
                node.NodeID = GetNextID();
                node.UpdateSocketIDs();
            }
            Nodes.Add(node);
            if (node.EntryPoint && !node.EventPoint)
                EntryPoints.Add(node);
            else if (node.EventPoint)
                EventEntryPoints.Add(node);
            if (NodesChanged != null)
                NodesChanged(this, null);
        }

        public void RemoveNode(GraphNode node)
        {
            Nodes.Remove(node);
            EntryPoints.Remove(node);
            EventEntryPoints.Remove(node);
            foreach (var socket in node.InputSockets)
                Disconnect(socket);
            foreach (var socket in node.OutputSockets)
                Disconnect(socket);
            if (NodesChanged != null)
                NodesChanged(this, null);
        }

        public virtual bool CanConnect(GraphSocket from, GraphSocket to)
        {
            if (from == null || to == null)
                return false;
            if (from.CanConnectTo(to))
                return !IsConnected(from, to);
            return false;
        }

        public bool IsConnected(GraphSocket from, GraphSocket to)
        {
            var hits = Connections.Where(p => 
                (p.FromSocket == from && p.ToSocket == to) ||
                (p.ToSocket == from && p.FromSocket == to));
            return hits.Count() > 0;
        }

        public virtual bool Connect(GraphSocket from, GraphSocket to)
        {
            if (CanConnect(from, to))
            {
                bool ret = false;
                if (!to.IsFlow && HasConnections(to))
                    Disconnect(to);
                if (from.IsInput)
                    ret = InternalConnect(to, from);
                else
                    ret = InternalConnect(from, to);
                if (ConnectivityChanged != null)
                    ConnectivityChanged(this, new ConnectivityChange { From = from, To = to });
                return ret;
            }
            return false;
        }

        public virtual bool HasConnections(GraphSocket socket)
        {
            return Connections.Where(p => p.ToSocket == socket).Count() > 0;
        }

        public virtual int GetDownstreamConnectionCount(GraphSocket socket)
        {
            return Connections.Where(p => p.FromSocket == socket).Count();
        }

        public virtual int GetUpstreamConnectionCount(GraphSocket socket)
        {
            return Connections.Where(p => p.ToSocket == socket).Count();
        }

        public virtual int GetTotalConnectionCount(GraphSocket socket)
        {
            return Math.Max(GetUpstreamConnectionCount(socket), GetDownstreamConnectionCount(socket));
        }

        public GraphSocket GetUpstreamSource(GraphSocket toSocket)
        {
            var found = Connections.FirstOrDefault(p => p.ToSocket == toSocket);
            if (found != null)
                return found.FromSocket;
            return null;
        }

        public virtual bool Disconnect(GraphSocket socket)
        {
            var found = Connections.Where(con => con.FromSocket == socket || con.ToSocket == socket).ToArray();
            if (found != null && found.Length > 0)
            {
                foreach (var conn in found)
                {
                    Connections.Remove(conn);
                    Routes.Remove(conn);
                }
            }
            bool ret = found != null && found.Length > 0;
            if (ret != false && ConnectivityChanged != null)
                ConnectivityChanged(this, null);
            return ret;
        }

        public virtual bool Disconnect(GraphSocket a, GraphSocket b)
        {
            var found = Connections.Where(p => (p.FromSocket == a && p.ToSocket == b) || (p.FromSocket == b && p.ToSocket == a)).ToArray();
            if (found != null && found.Length > 0)
            {
                foreach (var conn in found)
                {
                    Connections.Remove(conn);
                    Routes.Remove(conn);
                }
            }
            bool ret = found != null && found.Length > 0;
            if (ret && ConnectivityChanged != null)
                ConnectivityChanged(this, new ConnectivityChange { From = a, To = b, IsDisconnect = true });
            return ret;
        }

        protected bool InternalConnect(GraphSocket outSocket, GraphSocket inSocket)
        {
            var found = Connections.Where(p => p.FromSocket == outSocket && p.ToSocket == inSocket);
            if (found.Count() > 0)
                return false;

            inSocket.Node.SocketConnectivityChanged(inSocket);
            Connections.Add(new Data.Graph.GraphConnection { FromSocket = outSocket, ToSocket = inSocket, FromNode = outSocket.Node, ToNode = inSocket.Node });
            return true;
        }

        public ConnectionRouting GetRoute(GraphConnection conn)
        {
            if (conn == null)
                return null;
            ConnectionRouting ret = null;
            Routes.TryGetValue(conn, out ret);
            return ret;
        }

        public ConnectionRouting GetOrCreateRoute(GraphConnection conn)
        {
            ConnectionRouting ret = GetRoute(conn);
            if (ret != null)
                return ret;
            ret = new ConnectionRouting();
            Routes[conn] = ret;
            return ret;
        }

        #region Execution Management

        public void Prime(object param)
        {
            // clear all cached input sources
            foreach (var node in Nodes)
                foreach (var input in node.InputSockets)
                    input.CachedInputSource = null;

            // Cache the input sources
            foreach (var con in Connections)
                con.ToSocket.CachedInputSource = con.FromSocket;

            foreach (var node in Nodes)
                node.PrimeEarly(param);

            foreach (var node in Nodes)
                node.PrimeBeforeExecute(param);
        }

        #endregion

        #region Serialization and Cloning

        public void InitializeIDs()
        {
            foreach (var node in Nodes)
            {
                if (node.NodeID == -1)
                    node.NodeID = GetNextID();
                node.UpdateSocketIDs();
            }
        }

        void CreateConnFromIDs(int fromNode, int fromSocket, int toNode, int toSocket)
        {
            var newFrom = Nodes.FirstOrDefault(n => n.NodeID == fromNode);
            var newTo = Nodes.FirstOrDefault(n => n.NodeID == toNode);
            if (newFrom == null || newTo == null)
                return;

            var newFromSocket = newFrom.OutputSockets.FirstOrDefault(s => s.SocketID == fromSocket);
            var newToSocket = newTo.InputSockets.FirstOrDefault(s => s.SocketID == toSocket);
            if (newFromSocket != null && newToSocket != null)
            {
                Connections.Add(new GraphConnection
                {
                    FromNode = newFrom,
                    ToNode = newTo,
                    FromSocket = newFromSocket,
                    ToSocket = newToSocket
                });
            }
        }

        public virtual Graph Clone()
        {
            Graph ret = new Data.Graph.Graph();

            foreach (var nd in Nodes)
            {
                var newNode = nd.Clone();
                ret.AddNode(newNode);
            }

            foreach (var con in Connections)
                ret.CreateConnFromIDs(con.FromNode.NodeID, con.FromSocket.SocketID, con.ToNode.NodeID, con.ToSocket.SocketID);

            return ret;
        }

        public void Serialize(SerializationContext ctx, System.Xml.XmlElement intoElem)
        {
            var graphElem = intoElem.CreateChild("graph");
            foreach (var key in CustomData)
                graphElem.SetAttribute(key.Key, key.Value);
                    
            graphElem.AddStringElement("next_id", nextID_.ToString());
            foreach (var node in Nodes)
            {
                var nodeElem = graphElem.CreateChild("node");
                nodeElem.SetAttribute("type", node.GetType().FullName);
                node.Serialize(ctx, nodeElem);
            }

            foreach (var conn in Connections)
            {
                var conElem = graphElem.CreateChild("connection");
                conElem.AddStringElement("from_node", conn.FromNode.NodeID.ToString());
                conElem.AddStringElement("from_socket", conn.FromSocket.SocketID.ToString());
                conElem.AddStringElement("to_node", conn.ToNode.NodeID.ToString());
                conElem.AddStringElement("to_socket", conn.ToSocket.SocketID.ToString());
            }

            foreach (var box in Boxes)
            {
                var boxElem = graphElem.CreateChild("box");
                boxElem.SetAttribute("x", box.VisualX.ToString());
                boxElem.SetAttribute("y", box.VisualY.ToString());
                boxElem.SetAttribute("width", box.VisualWidth.ToString());
                boxElem.SetAttribute("height", box.VisualHeight.ToString());
                boxElem.AddStringElement("name", box.Name);
                boxElem.AddStringElement("note", box.Note);
                boxElem.AddStringElement("color", box.BoxColor.ToTightString());
            }

            foreach (var route in Routes)
            {
                var routeElem = graphElem.CreateChild("route");
                routeElem.SetAttribute("conn", Connections.IndexOf(route.Key).ToString());
                foreach (var pt in route.Value.RoutingPoints)
                    routeElem.AddStringElement("pt", pt.ToTightString());
            }
        }

        public void Deserialize(SerializationContext ctx, System.Xml.XmlElement fromElem)
        {
            if (fromElem.Attributes != null)
            {
                for (int i = 0; i < fromElem.Attributes.Count; ++i)
                    CustomData[fromElem.Attributes.Item(i).Name] = fromElem.Attributes.Item(i).Value;
            }
            nextID_ = fromElem.GetIntElement("next_id", 0);
            foreach (var node in fromElem.ChildNodes)
            {
                var elem = node as System.Xml.XmlElement;
                if (elem != null)
                {
                    if (elem.Name.Equals("node"))
                    {
                        Type t = Type.GetType(elem.GetAttribute("type"));
                        GraphNode gNode = Activator.CreateInstance(t) as GraphNode;
                        gNode.Deserialize(ctx, elem);
                        AddNode(gNode);
                    }
                    else if (elem.Name.Equals("connection"))
                    {
                        CreateConnFromIDs(
                            elem.GetIntElement("from_node"),
                            elem.GetIntElement("from_socket"),
                            elem.GetIntElement("to_node"),
                            elem.GetIntElement("to_socket")
                            );
                    }
                    else if (elem.Name.Equals("box"))
                    {
                        GraphBox b = new GraphBox();
                        b.VisualX = double.Parse(elem.GetAttribute("x"));
                        b.VisualY = double.Parse(elem.GetAttribute("y"));
                        b.VisualWidth = double.Parse(elem.GetAttribute("width"));
                        b.VisualHeight = double.Parse(elem.GetAttribute("height"));

                        b.Name = elem.GetStringElement("name", "");
                        b.Note = elem.GetStringElement("note", "");
                        b.BoxColor = elem.GetStringElement("color", "").ToColor();
                        Boxes.Add(b);
                    }
                    else if (elem.Name.Equals("route"))
                    {
                        ConnectionRouting route = new ConnectionRouting();
                        int conID = elem.GetIntAttribute("conn");
                        var conn = Connections[conID];
                        var pts = elem.SelectNodes("pt");
                        foreach (System.Xml.XmlElement ptElem in pts)
                            route.RoutingPoints.Add(ptElem.InnerText.ToVector2());
                        Routes[conn] = route;
                    }
                }
            }
        }

        public void Serialize(SerializationContext ctx, System.IO.BinaryWriter strm)
        {
            strm.Write("GRAPH");
            strm.Write(nextID_);
            strm.Write(Nodes.Count);
            foreach (var node in Nodes)
            {
                SerializeNode(ctx, strm, node);
            }
            strm.Write(Connections.Count);
            foreach (var conn in Connections)
            {
                strm.Write(conn.FromNode.NodeID);
                strm.Write(conn.FromSocket.SocketID);
                strm.Write(conn.ToNode.NodeID);
                strm.Write(conn.ToSocket.SocketID);
            }
            strm.Write(Boxes.Count);
            foreach (var box in Boxes)
            {
                strm.Write(box.VisualX);
                strm.Write(box.VisualY);
                strm.Write(box.VisualWidth);
                strm.Write(box.VisualHeight);
                strm.Write(box.Name);
                strm.Write(box.Note);
                strm.Write(box.BoxColor);
            }
            strm.Write(Routes.Count);
            foreach (var route in Routes)
            {
                strm.Write(Connections.IndexOf(route.Key));
                strm.Write(route.Value.RoutingPoints.Count);
                foreach (var pt in route.Value.RoutingPoints)
                    strm.Write(pt);
            }
        }

        public static void SerializeNode(SerializationContext ctx, System.IO.BinaryWriter strm, GraphNode node)
        {
            strm.Write(node.Version);
            strm.Write(node.GetType().FullName);
            node.Serialize(ctx, strm);
        }

        public static GraphNode DeserializeNode(SerializationContext ctx, System.IO.BinaryReader strm)
        {
            int version = strm.ReadInt32();
            Type t = Type.GetType(strm.ReadString());
            GraphNode newNode = Activator.CreateInstance(t) as GraphNode;
            newNode.Deserialize(ctx, strm);
            return newNode;
        }

        public void Deserialize(SerializationContext ctx, System.IO.BinaryReader strm)
        {
            if (strm.ReadString().Equals("GRAPH"))
            {
                nextID_ = strm.ReadInt32();
                int nodeCt = strm.ReadInt32();
                for (int i = 0; i < nodeCt; ++i)
                {
                    AddNode(DeserializeNode(ctx, strm));
                }
                int conns = strm.ReadInt32();
                for (int i = 0; i < conns; ++i)
                {
                    int fromNodeID = strm.ReadInt32();
                    int fromSocketID = strm.ReadInt32();
                    int toNodeID = strm.ReadInt32();
                    int toSocketID = strm.ReadInt32();
                    CreateConnFromIDs(fromNodeID, fromSocketID, toNodeID, toSocketID);
                }
                int boxes = strm.ReadInt32();
                for (int i = 0; i < boxes; ++i)
                {
                    GraphBox b = new GraphBox();
                    b.VisualX = strm.ReadDouble();
                    b.VisualY = strm.ReadDouble();
                    b.VisualWidth = strm.ReadDouble();
                    b.VisualHeight = strm.ReadDouble();
                    b.Name = strm.ReadString();
                    b.Note = strm.ReadString();
                    b.BoxColor = strm.ReadColor();
                    Boxes.Add(b);
                }
                int routeCt = strm.ReadInt32();
                for (int i = 0; i < routeCt; ++i)
                {
                    ConnectionRouting route = new ConnectionRouting();
                    var conn = Connections[strm.ReadInt32()];
                    int ptCt = strm.ReadInt32();
                    for (int j = 0; j < ptCt; ++j)
                        route.RoutingPoints.Add(strm.ReadVector2());
                }
            }
        }

        #endregion

        /// <summary>
        /// Graph contents will be re-indexed for tightly packed indices
        /// WARNING: this should not be used on graphs whose nodes or sockets are externally
        /// referenced by ID, although this generally SHOULD NOT be done
        /// </summary>
        public void CompressIndices()
        {
            nextID_ = 0;
            foreach (var node in Nodes)
            {
                node.NodeID = GetNextID();
                foreach (var socket in node.InputSockets)
                    socket.SocketID = GetNextID();
                foreach (var socket in node.OutputSockets)
                    socket.SocketID = GetNextID();
            }
        }

        public int StructuralHash()
        {
            int cur = Util.HashHelper.Start(1337);
            foreach (var node in Nodes)
                cur = Util.HashHelper.Hash(cur, node.GetHashCode());
            foreach (var conn in Connections)
                cur = Util.HashHelper.Hash(cur, conn.GetHashCode());
            return cur;
        }
    }
}
