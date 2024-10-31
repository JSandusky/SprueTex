using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

namespace SprueKit.Data
{
    public static class SceneHelpers
    {
        /// <summary>
        /// Picks the closest object along a ray
        /// </summary>
        public static SpruePiece PickRay(SpruePiece piece, Ray ray)
        {
            SortedSet<KeyValuePair<float, SpruePiece>> hits = new SortedSet<KeyValuePair<float, SpruePiece>>(new Util.FuncComparer<KeyValuePair<float, SpruePiece>>((l, r) => { return l.Key.CompareTo(r.Key); }));
            PickRay(piece, ray, hits);
            if (hits.Count > 0)
                return hits.First().Value; // take the closest
            return null;
        }

        public static float PickRay_Distance(SpruePiece piece, Ray ray)
        {
            SortedSet<KeyValuePair<float, SpruePiece>> hits = new SortedSet<KeyValuePair<float, SpruePiece>>(new Util.FuncComparer<KeyValuePair<float, SpruePiece>>((l, r) => { return l.Key.CompareTo(r.Key); }));
            PickRay(piece, ray, hits);
            if (hits.Count > 0)
                return hits.First().Key; // take the closest
            return float.MaxValue;
        }

        /// <summary>
        /// Picks the first object that comes after the given current object.
        /// Use to produce a "cycle through" selection mode
        /// </summary>
        public static SpruePiece PickRay_After(SpruePiece piece, Ray ray, SpruePiece current)
        {
            SortedSet<KeyValuePair<float, SpruePiece>> hits = new SortedSet<KeyValuePair<float, SpruePiece>>(new Util.FuncComparer<KeyValuePair<float, SpruePiece>>((l, r) => { return l.Key.CompareTo(r.Key); }));
            PickRay(piece, ray, hits);
            if (hits.Count > 0)
            {
                bool takeNext = false;
                var list = hits.ToList();
                for (int i = 0; i < list.Count; ++i)
                {
                    if (list[i].Value == current)
                        takeNext = true;
                    else if (takeNext || current == null)
                        return list[i].Value;
                }
                return list[0].Value;
            }
            return null;
        }

        /// <summary>
        /// Picks the first object that IS NOT the given 'current' object
        /// </summary>
        public static SpruePiece PickRay_Not(SpruePiece piece, Ray ray, SpruePiece current)
        {
            SortedSet<KeyValuePair<float, SpruePiece>> hits = new SortedSet<KeyValuePair<float, SpruePiece>>(new Util.FuncComparer<KeyValuePair<float, SpruePiece>>((l, r) => { return l.Key.CompareTo(r.Key); }));
            PickRay(piece, ray, hits);
            if (hits.Count > 0)
                return hits.FirstOrDefault((a) => { return a.Value != current; }).Value;
            return null;
        }

        static void PickRay(SpruePiece piece, Ray ray, SortedSet<KeyValuePair<float, SpruePiece>> hits)
        {
            if (piece is IMousePickable)
            {
                float hitDist = float.MaxValue;
                if (((IMousePickable)piece).DoMousePick(ray, out hitDist))
                    hits.Add(new KeyValuePair<float, SpruePiece>(hitDist, piece));
            }

            foreach (SpruePiece child in piece.Children)
                PickRay(child, ray, hits);

            if (piece is ChainPiece)
            {
                foreach (SpruePiece child in ((ChainPiece)piece).Bones)
                    PickRay(child, ray, hits);
            }
        }
    }
}
