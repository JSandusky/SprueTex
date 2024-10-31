using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit.Data.Sprue
{
    public class SkeletonBuilderTask : SprueKit.Tasks.TaskItem
    {
        public override string TaskName { get { return "skeleton construction"; } }

        SprueModel target_;
        PluginLib.SkeletonData skeleton_;

        public SkeletonBuilderTask(SprueModel target) : base(null)
        {
            target_ = target;
        }

        public override void TaskLaunch()
        {
            skeleton_ = Processing.SkeletonBuilder.BuildSkeleton(target_);
        }

        public override void TaskEnd()
        {
            if (target_ != null &&  // was the input even valid
                target_.MeshData != null &&     // was the input even valid?
                skeleton_ != null &&            // did we succeed?
                skeleton_.Inline.Count > 1)     // must have a reason for a skeleton
            {
                target_.MeshData.Skeleton = skeleton_;
            }
            else if (target_ != null && target_.MeshData == null && skeleton_ != null && skeleton_.Inline.Count > 1) // probably just an external mesh issue
            {
                target_.MeshData = new MeshData(new List<int>(), new List<PluginLib.VertexData>());
                target_.MeshData.Skeleton = skeleton_;
            }
        }
    }
}
