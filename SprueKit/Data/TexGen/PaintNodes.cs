using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Color = Microsoft.Xna.Framework.Color;

namespace SprueKit.Data.TexGen
{
    public partial class Paint2DNode : TexGenNode
    {
        public override void Construct()
        {
            base.Construct();
            Name = "Paint 2D";
            AddOutput(new Data.Graph.GraphSocket(this) { Name = "Out", IsInput = false, IsOutput = true, TypeID = Data.Graph.SocketTypeID.Channel });
        }

        public override void Execute(object param)
        {
            base.Execute(param);
            OutputSockets[0].Data = Color.Red;
        }
    }
}
