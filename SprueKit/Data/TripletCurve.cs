using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit.Data
{
    public class TripletCurve
    {
        public Curve XCurve { get; set; } = new Curve();
        public Curve YCurve { get; set; } = new Curve();
        public Curve ZCurve { get; set; } = new Curve();

        public void AddKey(float pos, Vector3 value)
        {
            XCurve.Keys.Add(new CurveKey(pos, value.X));
            YCurve.Keys.Add(new CurveKey(pos, value.Y));
            ZCurve.Keys.Add(new CurveKey(pos, value.Z));
        }

        public void ComputeTangents()
        {
            XCurve.ComputeTangents(CurveTangent.Smooth);
            YCurve.ComputeTangents(CurveTangent.Smooth);
            ZCurve.ComputeTangents(CurveTangent.Smooth);
        }

        public Vector3 Evaluate(float td)
        {
            return new Vector3(XCurve.Evaluate(td), YCurve.Evaluate(td), ZCurve.Evaluate(td));
        }
    }
}
