using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit.Graphics.Paint
{
    public class PaintSceneBase : ViewportDelegate
    {
        static Guid ViewportID = new Guid("bdb9a0a8-b57e-4265-9d2e-968bfa6ad72f");

        public PaintSceneBase(BaseScene scene) : base(scene)
        {
        }

        public override string ViewportName { get { return "Paint"; } }

        public override Guid GetID()
        {
            return ViewportID;
        }
    }
}
