using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using BoundingBox = Microsoft.Xna.Framework.BoundingBox;
using Plane = Microsoft.Xna.Framework.Plane;
using Ray = Microsoft.Xna.Framework.Ray;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace MapLib
{
    public abstract partial class MapElement
    {
        public int Index { get; set; }
        public int Tag { get; set; }
        public Dictionary<string, UniField> Fields { get; private set; } = new Dictionary<string, UniField>();

        protected void ReadFields(XmlElement elem)
        {
            var fieldsRoot = elem.SelectSingleNode("fields");
            if (fieldsRoot != null)
            {
                for (int i = 0; i < fieldsRoot.ChildNodes.Count; ++i)
                {
                    var asElem = (XmlElement)fieldsRoot.ChildNodes.Item(i);
                    string name = asElem.GetAttribute("name");
                    string typeName = asElem.GetAttribute("type");
                    string dataValue = asElem.InnerText;

                    Fields[name] = new UniField(int.Parse(typeName), dataValue);
                    //TODO: resolve UDMF fields
                }
            }
        }

        internal virtual void PostMapLoad(Map map)
        {

        }
    }

    public partial class MapThing : MapElement
    {
        public int Type { get; set; } = 0;
        public int Action { get; set; } = 0;
        public Vector3 Position { get; set; }
        public Vector2 Position2D { get { return new Vector2(Position.X, Position.Z); } }
        public float Angle { get; set; }
        public Sector CurrentSector { get; set; }

        public MapThing(XmlElement elem)
        {
            Index = int.Parse(elem.GetAttribute("idx"));
            Tag = int.Parse(elem.GetAttribute("tag"));

            Type = int.Parse(elem.GetAttribute("type"));
            Action = int.Parse(elem.GetAttribute("action"));

            var pos = elem.SelectSingleNode("pos") as XmlElement;

            Position = new Vector3(float.Parse(pos.GetAttribute("x")), float.Parse(pos.GetAttribute("y")), float.Parse(pos.GetAttribute("z")));
            Angle = float.Parse(pos.GetAttribute("angle-float"));

            var flags = elem.SelectSingleNode("flags");
            if (flags != null)
            {
                for (int i = 0; i < flags.ChildNodes.Count; ++i)
                {
                    string flagName = ((XmlElement)flags.ChildNodes[i]).GetAttribute("name");
                    string flagValue = ((XmlElement)flags.ChildNodes[i]).GetAttribute("value");
                }
            }

            ReadFields(elem);
        }
    }

    public partial class MapVertex : MapElement
    {
        public Vector2 Position { get; set; }

        public MapVertex(XmlElement fromElem)
        {
            Index = int.Parse(fromElem.GetAttribute("idx"));
            Position = new Vector2(float.Parse(fromElem.GetAttribute("x")), float.Parse(fromElem.GetAttribute("y")));

            ReadFields(fromElem);
        }
    }

    public partial class SideDef : MapElement
    {
        public Sector Sector { get; set; }
        public SideDef Opposite { get { if (IsFront) return Line.Back; return Line.Front; } }
        public LineDef Line { get; set; }
        public List<SideDefAction> Actions { get; set; }
        public float OffsetX { get; set; } = 0.0f;
        public float OffsetY { get; set; } = 0.0f;

        public bool IsFront { get { return Line.Front == this; } }

        public SideDef(Map map, XmlElement elem)
        {
            Index = int.Parse(elem.GetAttribute("idx"));
            int lineIdx = int.Parse(elem.GetAttribute("line"));
            Line = map.Lines.FirstOrDefault(l => l.Index == lineIdx);

            OffsetX = elem.GetFloatElement("offset-x");
            OffsetY = elem.GetFloatElement("offset-y");
            //??IsFront = elem.GetStringElement("front").Equals("true");

            ReadFields(elem);
        }
    }

    public enum LineDefSpecial
    {
        None,
        SlopeStart,
        SlopeEnd
    }
    
    public partial class LineDef : MapElement
    {
        public MapVertex A { get; set; }
        public MapVertex B { get; set; }
        public SideDef Front { get; set; }
        public SideDef Back { get; set; }
        public int ActionCode { get; set; }
        public int[] ActionArgs { get; set; } = new int[] { 0, 0, 0, 0 };
        public int ActivationSettings { get; set; }
        public LineDefSpecial Special { get; set; }

        public bool DoubleSided { get { return Front != null && Back != null; } }
        public bool Passable { get { return false; } }
        public bool Hidden { get; set; }

        public bool IsConnectTo(LineDef rhs)
        {
            return rhs.A == A || rhs.B == A || rhs.B == A || rhs.B == B;
        }

        public Vector2 MidPoint { get { return Vector2.Lerp(A.Position, B.Position, 0.5f); } }

        public LineDef(Map map, XmlElement fromElem)
        {
            Index = int.Parse(fromElem.GetAttribute("idx"));
            Tag = int.Parse(fromElem.GetAttribute("tag"));
            ActionCode = int.Parse(fromElem.GetAttribute("action"));
            ActivationSettings = int.Parse(fromElem.GetAttribute("activate"));

            int vA = int.Parse(fromElem.GetAttribute("va"));
            int vB = int.Parse(fromElem.GetAttribute("vb"));
            A = map.Vertices.FirstOrDefault(v => v.Index == vA);
            B = map.Vertices.FirstOrDefault(v => v.Index == vB);

            var flags = fromElem.SelectSingleNode("flags");
            if (flags != null)
            {
                for (int i = 0; i < flags.ChildNodes.Count; ++i)
                {
                    string flagName = ((XmlElement)flags.ChildNodes[i]).GetAttribute("name");
                    string flagValue = ((XmlElement)flags.ChildNodes[i]).GetAttribute("value");
                }
            }

            ReadFields(fromElem);
        }

        internal override void PostMapLoad(Map map)
        {
            base.PostMapLoad(map);
            Front = map.Sides.FirstOrDefault(s => s.IsFront && s.Line == this);
            Back = map.Sides.FirstOrDefault(s => !s.IsFront && s.Line == this);
        }

        public bool IsNullLine()
        {
            return Front == null && Back == null;
        }

        internal void RemoveSide(SideDef side)
        {
            if (side.IsFront)
                Front = null;
            else
                Back = null;
        }

        public BoundingBox CalculateBounds()
        {
            BoundingBox bb = new BoundingBox();
		    bb.Min.X = Math.Min(A.Position.X, B.Position.X);
		    bb.Min.Z = Math.Min(A.Position.Y, B.Position.Y);
            if (Back != null)
                bb.Min.Y = Math.Min(Front.Sector.FloorHeight, Back.Sector.FloorHeight);
            else
                bb.Min.Y = Front.Sector.FloorHeight;
		    
		    bb.Max.X = Math.Max(A.Position.X, B.Position.X);
		    bb.Max.Z = Math.Max(A.Position.Y, B.Position.Y);
            if (Back != null)
                bb.Max.Y = Math.Max(Front.Sector.CeilingHeight, Back.Sector.CeilingHeight);
            else
                bb.Max.Y = Front.Sector.CeilingHeight;
            return bb;
        }

        float SideOfLine(Vector2 p)
        {
            Vector2 v1 = A.Position;
            Vector2 v2 = B.Position;

            return (p.Y - v1.Y) * (v2.X - v1.X) - (p.X - v1.Y) * (v2.X - v1.Y);
        }

        public bool IsOnFrontSide(Vector2 p)
        {
            return SideOfLine(p) > 0;
        }

        public bool IsOnBackside(Vector2 p)
        {
            return SideOfLine(p) < 0;
        }

        float SideOfLine(Vector3 p)
        {
            return (p.Z - A.Position.Y) * (B.Position.X - A.Position.X) - (p.X - A.Position.X) * (B.Position.Y - A.Position.Y);
        }

        public bool IsOnFrontSide(Vector3 p)
        {
            return SideOfLine(p) > 0;
        }

        public bool IsOnBackside(Vector3 p)
        {
            return SideOfLine(p) < 0;
        }

        public static LineDef GetRightMost(List<LineDef> lines)
        {
            Vector2 cur = new Vector2(float.MinValue, 0);
            LineDef curBest = null;
            for (int i = 0; i < lines.Count; ++i)
            {
                var l = lines[i];
                if (l.A.Position.X > cur.X)
                {
                    cur.X = l.A.Position.X;
                    curBest = l;
                }
                if (l.B.Position.X > cur.X)
                {
                    cur.X = l.B.Position.X;
                    curBest = l;
                }
            }
            return curBest; //???
        }

        public static LineDef GetNext(LineDef current, List<LineDef> lines)
        {
            for (int i = 0; i < lines.Count; ++i)
            {
                var l = lines[i];
                if (l.A.Equals(current.B))
                    return l;
            }
            return null;
        }

        public bool CanCross(MapThing thing, int stepHeight, int objectHeight)
        {
            if (!DoubleSided)
                return false;
            Sector from = null, to = null;
            if (IsOnFrontSide(thing.Position))
            {
                from = Front.Sector;
                to = Back.Sector;
            }
            else
            {
                from = Back.Sector;
                to = Front.Sector;
            }
            if (to.CanEnter(from, stepHeight, objectHeight))
                return true;
            return false;
        }

        static Vector2 tmp = Vector2.Zero;
        public bool Intersects(Vector2 start, Vector2 end)
        {
            if (Intersector.IntersectLines(A.Position, B.Position, start, end, ref tmp))
                return true;
            return false;
        }

        public bool Intersects(Vector2 start, Vector2 end, ref Vector2 holder)
        {
            if (Intersector.IntersectLines(A.Position, B.Position, start, end, ref holder))
                return true;
            return false;
        }
    }

    public partial class Sector : MapElement
    {
        // A sector can have multiple actions acting on it at any given time
        public List<SectorAction> Actions { get; set; }
        public LineDef[] Lines { get; set; }
        public SideDef[] Sides { get; set; }

        float floorHeight_ = 0.0f;
        float currentFloorHeight_ = 0.0f;
        float ceilingHeight_ = 0.0f;
        float currentCeilingHeight_ = 0.0f;
        public float FloorHeight { get { return floorHeight_; } set { floorHeight_ = value; currentFloorHeight_ = value; } }
        public float CeilingHeight { get { return ceilingHeight_; } set { ceilingHeight_ = value; currentCeilingHeight_ = value; } }
        public float CurrentFloorHeight { get { return currentCeilingHeight_; } }
        public float CurrentCeilingHeight { get { return CurrentCeilingHeight; } }
        public Rectangle BoundingRect { get; set; }

        public int Brightness { get; set; } = 128;
        public int SectorSpecial { get; set; } = 0;

        int[] tempSides;
        Plane? floorProjectionPlane_;
        Plane? ceilingProjectionPlane_;

        public Sector(Map map, XmlElement elem)
        {
            Index = int.Parse(elem.GetAttribute("idx"));
            Tag = int.Parse(elem.GetAttribute("tag"));
            CeilingHeight = float.Parse(elem.GetAttribute("ceiling-height"));
            FloorHeight = float.Parse(elem.GetAttribute("floor-height"));
            Brightness = int.Parse(elem.GetAttribute("lighting"));
            SectorSpecial = int.Parse(elem.GetAttribute("special"));

            //TODO textures

            string[] sidesList = elem.GetStringElement("sides").Split(',');
            if (sidesList != null && sidesList.Length > 0)
            {
                tempSides = new int[sidesList.Length];
                for (int i = 0; i < sidesList.Length; ++i)
                    tempSides[i] = int.Parse(sidesList[i]);
            }

            ReadFields(elem);
        }

        internal override void PostMapLoad(Map map)
        {
            base.PostMapLoad(map);
            if (tempSides != null && tempSides.Length > 0)
            {
                Lines = new LineDef[tempSides.Length];
                Sides = new SideDef[tempSides.Length];
                for (int i = 0; i < tempSides.Length; ++i)
                {
                    var side = map.Sides.FirstOrDefault(s => s.Index == tempSides[i]);
                    if (side != null)
                    {
                        Lines[i] = side.Line;
                        Sides[i] = side;
                        side.Sector = this;
                    }
                    else
                        throw new Exception("Map load failure with corrupted sides stored in sector");
                }
            }

            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;

            for (int i = 0; i < Lines.Length; ++i)
            {
                minX = Math.Min(minX, Lines[i].A.Position.X);
                minX = Math.Min(minX, Lines[i].B.Position.X);
                maxX = Math.Max(maxX, Lines[i].A.Position.X);
                maxX = Math.Max(maxX, Lines[i].B.Position.X);

                minY = Math.Min(minY, Lines[i].A.Position.Y);
                minY = Math.Min(minY, Lines[i].B.Position.Y);
                maxY = Math.Max(maxY, Lines[i].A.Position.Y);
                maxY = Math.Max(maxY, Lines[i].B.Position.Y);
            }

            minX = (float)Math.Floor(minX);
            minY = (float)Math.Floor(minY);
            maxX = (float)Math.Ceiling(maxX);
            maxY = (float)Math.Ceiling(maxY);

            BoundingRect = new Rectangle((int)minX, (int)minY, (int)(maxX - minX), (int)(maxY - minY));
        }

        public bool CheckHeight(float height) //check that something fits into this sector
        {
            return currentFloorHeight_ + height < currentCeilingHeight_;
        }

        // Check if something can cross into this sector from another one
        public bool CanEnter(Sector from, int stepHeight, int objectHeight)
        {
            if (!CheckHeight(objectHeight))
                return false;

            float difference = Math.Max(from.currentFloorHeight_, currentFloorHeight_) - Math.Min(from.currentFloorHeight_, currentFloorHeight_);
            bool iAmLower = currentFloorHeight_ < from.currentFloorHeight_;

            if (iAmLower)
                return true;
            if (difference <= stepHeight)
                return true;

            return false;
        }

        /** gets a list of neighboring sectors that are not in the exclusion array (if it is not null)*/
        public void GetNeighbors(List<Sector> holder, List<Sector> exclude)
        {
            for (int i = 0; i < Sides.Length; ++i)
            {
                SideDef side = Sides[i];
                var opp = side.Opposite;
                if (opp != null)
                {
                    if ((exclude == null || !exclude.Contains(opp.Sector)) && !holder.Contains(opp.Sector))
                        holder.Add(opp.Sector);
                }
            }
        }

        private static Stack<Sector> _eventStack_ = new Stack<Sector>();

        /** propagates a floor-filling event through the map sectors */
        public void SendEvent(SectorEvent sEvent)
        {
            SendEvent_(sEvent);
            _eventStack_.Clear();
        }

        private bool SendEvent_(SectorEvent sEvent) 
        {
            if (sEvent.DoEvent(this))
			    return true;

            _eventStack_.Push(this);
            for (int i = 0; i < Sides.Length; ++i)
            {
                SideDef side = Sides[i];
                if (side.Line.DoubleSided)
                {
                    if (!_eventStack_.Contains(side.Opposite.Sector))
                    {
                        if (side.Opposite.Sector.SendEvent_(sEvent))
                            return true;
                    }
                }
                sEvent.Popped();
            }
		    return false;
	    }

        /** a shallow ray cast that only returns the first intesecting side */
        public SideDef CastRay(Vector2 from, Vector2 to)
        {
            for (int i = 0; i < Sides.Length; ++i)
            {
                var side = Sides[i];
                if (side.Line.Intersects(from, to))
                    return side;
            }
            return null;
        }

        /** deeply casts through sectors until it hits a one sided line */
        public SideDef CastRayDeep(Vector2 from, Vector2 to)
        {
            return CastRayDeep_(from, to, null);
        }

        /** repeated casting of a line segment */
        SideDef CastRayDeep_(Vector2 from, Vector2 to, SideDef passedThrough)
        {
            for (int i = 0; i < Sides.Length; ++i)
            {
                SideDef side = Sides[i];
                if (side != passedThrough)
                {
                    if (side.Line.Intersects(from, to))
                    {
                        if (side.Line.DoubleSided)
                            return side.Opposite.Sector.CastRayDeep_(from, to, side);
                        return side;
                    }
                }
            }
            return null;
        }

        static Ray tmpRay = new Ray();
        /** gets an accurate floor height using either currentFloorHeight or using the projection plane */
        public float GetFloorHeight(Vector2 at)
        {
            if (floorProjectionPlane_.HasValue)
            {
                tmpRay.Position.X = at.X;
                tmpRay.Position.Z = at.Y;
                tmpRay.Position.Y = -10000;
                tmpRay.Direction = Vector3.UnitY;
                float? hitDist = tmpRay.Intersects(floorProjectionPlane_.Value);
                if (hitDist.HasValue)
                    return tmpRay.Position.Y + tmpRay.Direction.Y * hitDist.Value;
            }
            return currentFloorHeight_;
        }

        /** gets an accurate ceiling height using either currentCeilingHeight or using the projection plane */
        public float GetCeilingHeight(Vector2 at)
        {
            if (ceilingProjectionPlane_.HasValue)
            {
                tmpRay.Position.X = at.X;
                tmpRay.Position.Z = at.Y;
                tmpRay.Position.Y = -10000;
                tmpRay.Direction = Vector3.UnitY;
                float? hitDist = tmpRay.Intersects(floorProjectionPlane_.Value);
                if (hitDist.HasValue)
                    return tmpRay.Position.Y + tmpRay.Direction.Y * hitDist.Value;
            }
            return currentCeilingHeight_;
        }

        /// Returns the amount of space between the floor and ceiling at the given point
        public float SpaceAvailable(Vector2 at)
        {
            return GetCeilingHeight(at) - GetFloorHeight(at);
        }
    }

    public partial class Map
    {
        public List<MapVertex> Vertices { get; set; } = new List<MapVertex>();
        public List<LineDef> Lines { get; set; } = new List<LineDef>();
        public List<SideDef> Sides { get; set; } = new List<SideDef>();
        public List<Sector> Sectors { get; set; } = new List<Sector>();

        public List<MapThing> Things { get; set; } = new List<MapThing>();

        public Map(XmlElement element)
        {
            var vertsElem = element.SelectSingleNode("vertices") as XmlElement;
            for (int i = 0; i < vertsElem.ChildNodes.Count; ++i)
                Vertices.Add(new MapVertex((XmlElement)vertsElem.ChildNodes.Item(i)));

            var sectorsElem = element.SelectSingleNode("sectors") as XmlElement;
            for (int i = 0; i < sectorsElem.ChildNodes.Count; ++i)
                Sectors.Add(new Sector(this, (XmlElement)sectorsElem.ChildNodes.Item(i)));

            var linesElem = element.SelectSingleNode("lines") as XmlElement;
            for (int i = 0; i < linesElem.ChildNodes.Count; ++i)
                Lines.Add(new LineDef(this, (XmlElement)linesElem.ChildNodes.Item(i)));

            var sidesElem = element.SelectSingleNode("sides") as XmlElement;
            for (int i = 0; i < sidesElem.ChildNodes.Count; ++i)
                Sides.Add(new SideDef(this, (XmlElement)sidesElem.ChildNodes.Item(i)));

            var thingsElem = element.SelectSingleNode("things") as XmlElement;
            for (int i = 0; i < sidesElem.ChildNodes.Count; ++i)
                Things.Add(new MapThing((XmlElement)sidesElem.ChildNodes.Item(i)));

            for (int i = 0; i < Vertices.Count; ++i)
                Vertices[i].PostMapLoad(this);
            for (int i = 0; i < Lines.Count; ++i)
                Lines[i].PostMapLoad(this);
            for (int i = 0; i < Sides.Count; ++i)
                Sides[i].PostMapLoad(this);

            for (int i = 0; i < Sectors.Count; ++i)
                Sectors[i].PostMapLoad(this);
            for (int i = 0; i < Things.Count; ++i)
                Things[i].PostMapLoad(this);
        }
    }

    // Used for effects such as making a wall protrude to crush, shoot a fireball, etc
    public abstract class SideDefAction
    {
        public abstract bool Update(float td);
    }

    public static class XmlExt
    {
        public static string GetStringElement(this XmlElement elem, string childName, string defaultValue = "")
        {
            var found = elem.SelectSingleNode(childName);
            if (found == null)
                return defaultValue;
            return found.InnerText;
        }

        public static float GetFloatElement(this XmlElement elem, string child, float defaultValue = 0.0f)
        {
            var found = elem.SelectSingleNode(child);
            if (found == null)
                return defaultValue;
            return float.Parse(found.InnerText);
        }

        public static int GetIntElement(this XmlElement elem, string child, int defaultValue = 0)
        {
            var found = elem.SelectSingleNode(child);
            if (found == null)
                return defaultValue;
            return int.Parse(found.InnerText);
        }
    }
}
