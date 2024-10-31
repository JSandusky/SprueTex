using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit.Graphics.Paint
{
    public class PaintScene3D : PaintSceneBase
    {
        static Guid ViewportID = new Guid("5b54fe0b-cde7-4279-95c2-5387902a2a0f");

        public PaintScene3D(BaseScene scene) : base(scene)
        {
        }

        public override Guid GetID()
        {
            return ViewportID;
        }
    }
}
