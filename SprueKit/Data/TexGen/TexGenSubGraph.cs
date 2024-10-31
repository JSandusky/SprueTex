using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SprueKit.Data.Graph;

namespace SprueKit.Data.TexGen
{
    public class TexGenSubGraphInputNode : SubGraphInputNode
    {
        public override string DisplayName
        {
            get
            {
                return string.Format("In:\n{0}", Name);
            }
        }

        public override void Construct()
        {
            base.Construct();
            AddOutput(new GraphSocket(this) { Name = "In", TypeID = SocketTypeID.Channel, IsInput = false, IsOutput = false });
        }
    }

    public class TexGenSubGraphOutputNOde : SubGraphOutputNode
    {
        public override void Construct()
        {
            base.Construct();
            AddInput(new GraphSocket(this) { Name = "Out", TypeID = SocketTypeID.Channel });
        }
    }
}
