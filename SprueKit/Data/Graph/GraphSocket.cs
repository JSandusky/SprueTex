using System.Linq;
using Color = Microsoft.Xna.Framework.Color;
using Vector4 = Microsoft.Xna.Framework.Vector4;

namespace SprueKit.Data.Graph
{
    public static class SocketTypeID
    {
        public static uint Grayscale = 1;
        public static uint Color = (1 << 1);
        public static uint Channel = 3;
        public static uint Model = 1 << 2;
    }

    public class GraphSocket
    {
        #region Caching

        // THIS Socket is an input, the socket here is the OUTPUT connected to it
        public GraphSocket CachedInputSource;

        #endregion

        private GraphSocket() { }
        public GraphSocket(GraphNode owner)
        {
            Node = owner;
        }

        [Notify.TrackMember(IsExcluded = true)]
        public double VisualX { get; set; }
        [Notify.TrackMember(IsExcluded = true)]
        public double VisualY { get; set; }

        [Notify.TrackMember(IsExcluded =true)]
        [PropertyData.PropertyIgnore]
        public GraphNode Node { get; private set; }
        [PropertyData.PropertyIgnore]
        [Notify.TrackMember(IsExcluded = true)]
        public int SocketID { get; set; } = -1;

        public string Name { get; set; } = "";
        public bool IsInput { get; set; } = true;
        public bool IsOutput { get; set; } = false;

        /// Flow sockets control program flow
        public bool IsFlow { get; set; } = false;
        uint typeID_ = 1;

        [Notify.TrackMember(IsExcluded = true)]
        public uint TypeID { get { return typeID_; } set { typeID_ = value; if (Node != null) Node.SignalChanged(); } }

        [Notify.TrackMember(IsExcluded = true)]
        public object Data { get; set; } = null;

        public bool CanConnectTo(GraphSocket rhs)
        {
            if (IsInput == rhs.IsInput)
                return false;
            //if (IsOutput == rhs.IsOutput)
            //    return false;
            // Must be same class of flow or non-flow
            if (IsFlow != rhs.IsFlow)
                return false;

            // Verify this connection is allowed
            if ((TypeID & rhs.TypeID) > 0)
                return true;
            return false;
        }

        public bool ConnectTo(GraphSocket rhs)
        {
            if (CanConnectTo(rhs))
            {
                var inputSocket = rhs.IsInput ? rhs : this;
                var outputSocket = rhs.IsOutput ? rhs : this;

                // Single entry graphs only allow one entry, exception for input flow sockets
                if (Node.Graph.IsSingleEntry || inputSocket.IsFlow)
                {
                    // Input sockets are only allowed one entry point
                    var othersEntering = Node.Graph.Connections.Where(o => o.ToSocket == inputSocket);
                    if (othersEntering != null)
                    {
                        // remove all other sockets entering us
                        foreach (var item in othersEntering)
                            Node.Graph.Connections.Remove(item);
                    }
                }

                // Check if there is anything to do
                var existing = Node.Graph.Connections.Where(o => o.ToSocket == inputSocket && o.FromSocket == outputSocket);
                if (existing != null && existing.Count() > 0)
                    return false;

                GraphConnection conn = new GraphConnection
                {
                    FromNode = outputSocket.Node,
                    FromSocket = outputSocket,
                    ToNode = inputSocket.Node,
                    ToSocket = inputSocket
                };

                Node.Graph.Connections.Add(conn);
            }
            return false;
        }

        public bool HasConnections()
        {
            if (Node != null && Node.Graph != null)
                return Node.Graph.HasConnections(this);
            return false;
        }

        public int DownstreamConnectionCount()
        {
            if (Node != null && Node.Graph != null)
                return Node.Graph.GetDownstreamConnectionCount(this);
            return 0;
        }

        public int UpstreamConnectionCount()
        {
            if (Node != null && Node.Graph != null)
                return Node.Graph.GetUpstreamConnectionCount(this);
            return 0;
        }

        public void DisconnectAll()
        {
            if (Node != null && Node.Graph != null)
                Node.Graph.Disconnect(this);
        }

        public bool GetBool(bool defVal = false)
        {
            if (Data == null)
                return defVal;
            if (Data is bool)
                return (bool)Data;
            else if (Data is int)
                return (int)Data != 0;
            else if (Data is float)
                return (float)Data != 0.0f;
            else if (Data is Vector4)
                return ((Vector4)Data).LengthSquared() > 0;
            else if (Data is Color)
                return ((Color)Data) != Color.TransparentBlack;

            return defVal;
        }

        public int GetInt(int defVal = 0)
        {
            if (Data == null)
                return defVal;
            if (Data is bool)
                return (bool)Data ? 1 : 0;
            else if (Data is int)
                return (int)Data;
            else if (Data is float)
                return (int)((float)Data);
            else if (Data is Vector4)
                return (int)((Vector4)Data).X;
            else if (Data is Color)
                return ((Color)Data).R;
            return defVal;
        }

        public float GetFloatData(float defVal = 0.0f)
        {
            if (Data == null)
                return defVal;
            if (Data is float)
                return (float)Data;
            else if (Data is Vector4)
                return ((Vector4)Data).X;
            else if (Data is Color)
                return ((Color)Data).R / 255.0f;
            return defVal;
        }

        public Color GetColor(Color defVal = new Color())
        {
            if (Data == null)
                return defVal;
            if (Data is float)
            {
                float fVal = (float)Data;
                return new Color(fVal, fVal, fVal, 1.0f);
            }
            else if (Data is Vector4)
                return Color.FromNonPremultiplied((Vector4)Data);
            else if (Data is Color)
                return (Color)Data;
            return defVal;
        }

        public Vector4 GetVector4(Vector4 defVal = new Vector4())
        {
            if (Data == null)
                return defVal;
            if (Data is float)
            {
                float fVal = (float)Data;
                return new Vector4(fVal, fVal, fVal, 1.0f);
            }
            else if (Data is Vector4)
                return (Vector4)Data;
            else if (Data is Color)
                return ((Color)Data).ToVector4();
            return defVal;
        }

        internal void Serialize(System.Xml.XmlElement elem)
        {
            System.Xml.XmlElement socketElem = elem.CreateChild("socket");
            socketElem.AddStringElement("name", Name);
            socketElem.AddStringElement("id", SocketID.ToString());
            socketElem.AddStringElement("input", IsInput.ToString());
            socketElem.AddStringElement("output", IsOutput.ToString());
            socketElem.AddStringElement("flow", IsFlow.ToString());
            socketElem.AddStringElement("type", TypeID.ToString());
            socketElem.AddStringElement("visualx", VisualX.ToString());
            socketElem.AddStringElement("visualy", VisualX.ToString());
        }

        internal static GraphSocket Deserialize(GraphNode node, System.Xml.XmlElement elem)
        {
            GraphSocket socket = new GraphSocket() { Node = node };
            socket.Name = elem.GetStringElement("name");
            socket.SocketID = elem.GetIntElement("id");
            socket.IsInput = elem.GetBoolElement("input");
            socket.IsOutput = elem.GetBoolElement("output");
            socket.IsFlow = elem.GetBoolElement("flow");
            socket.TypeID = elem.GetUIntElement("type");
            socket.VisualX = (double)elem.GetFloatElement("visualx");
            socket.VisualY = (double)elem.GetFloatElement("visualy");
            return socket;
        }

        internal void Serialize(System.IO.BinaryWriter strm)
        {
            strm.Write(Name);
            strm.Write(SocketID);
            strm.Write(IsInput);
            strm.Write(IsOutput);
            strm.Write(IsFlow);
            strm.Write(TypeID);
            strm.Write(VisualX);
            strm.Write(VisualY);
        }

        static internal GraphSocket Deserialize(GraphNode node, System.IO.BinaryReader strm)
        {
            GraphSocket socket = new GraphSocket() { Node = node };
            socket.Name = strm.ReadString();
            socket.SocketID = strm.ReadInt32();
            socket.IsInput = strm.ReadBoolean();
            socket.IsOutput = strm.ReadBoolean();
            socket.IsFlow = strm.ReadBoolean();
            socket.TypeID = strm.ReadUInt32();
            socket.VisualX = strm.ReadDouble();
            socket.VisualY = strm.ReadDouble();
            return socket;
        }

        internal GraphSocket Clone(GraphNode into)
        {
            return new GraphSocket(into)
            {
                SocketID = this.SocketID,
                TypeID = this.TypeID,
                Name = this.Name,
                IsInput = this.IsInput,
                IsOutput = this.IsOutput,
                IsFlow = this.IsFlow,
                VisualX = this.VisualX,
                VisualY = this.VisualY,
            };
        }

        public GraphSocket GetUpstreamSource()
        {
            if (Node != null && Node.Graph != null)
                return Node.Graph.GetUpstreamSource(this);
            return null;
        }
    }
}
