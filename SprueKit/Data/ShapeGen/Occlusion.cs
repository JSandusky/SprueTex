using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit.Data.ShapeGen
{
    public class Occlusion
    {
        public BoundingBox Bounds { get; set; }
        public List<BoundingBox> Boxes { get; private set; } = new List<BoundingBox>();

        public bool IsOccluded(Vector3 pt)
        {
            if (Bounds != null && Bounds.Contains(pt) == ContainmentType.Contains)
            {
                for (int i = 0; i < Boxes.Count; ++i)
                {
                    if (Boxes[i].Contains(pt) == ContainmentType.Contains)
                        return true;
                }
            }
            return false;
        }

        public bool IsOccluded(BoundingBox bounds)
        {
            if (Bounds != null && Bounds.Contains(bounds) > 0)
            {
                for (int i = 0; i < Boxes.Count; ++i)
                {
                    if (Boxes[i].Contains(bounds) > 0)
                        return true;
                }
            }
            return false;
        }

        public void Add(BoundingBox bounds)
        {
            if (Bounds == null)
            {
                Bounds = bounds;
                Boxes.Add(bounds);
            }
            else
            {
                Bounds = Bounds.Extend(bounds);
                Boxes.Add(bounds);
            }
        }
    }
}
