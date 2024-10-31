using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SprueKit.Tasks;

namespace SprueKit.Data.Sprue
{
    /// <summary>
    /// This task is reponsible for taking multiple texture generation results and compositing them together
    /// </summary>
    public class TextureCompositeTask : TaskItem
    {
        public override string TaskName { get { return "texture compositing"; } }

        public TextureCompositeTask() : base(null)
        {

        }

        public override void TaskLaunch()
        {
            base.TaskLaunch();
        }

        public override void TaskEnd()
        {
            base.TaskEnd();
        }
    }
}
