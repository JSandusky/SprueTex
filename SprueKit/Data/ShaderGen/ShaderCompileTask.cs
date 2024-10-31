using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SprueKit.Tasks;

namespace SprueKit.Data.ShaderGen
{
    public class ShaderCompileTask : TaskItem
    {
        Graph.Graph sourceGraph_;
        Graph.Graph cloneGraph_;

        public override string TaskName { get { return "Regenerating shader"; } }

        public ShaderCompileTask(Graph.Graph graph) : base(null)
        {
            sourceGraph_ = graph;
            cloneGraph_ = sourceGraph_.Clone();
        }

        public override void TaskLaunch()
        {
            ShaderCompiler compiler = new ShaderCompiler(cloneGraph_);
            string codeText = compiler.GetCode();
            System.Console.Write(codeText);
        }
    }
}
