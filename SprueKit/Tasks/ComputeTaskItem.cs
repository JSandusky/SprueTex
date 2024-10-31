using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SprueKit.Tasks
{
    /// <summary>
    /// Baseclass for a threading task that requires access to an OpenCL compute device.
    /// </summary>
    public abstract class ComputeTaskItem : TaskItem
    {
        static SprueBindings.ComputeDevice computeDevice_;

        public ComputeTaskItem(Action mainAction, Action finishAction = null) : base(mainAction, finishAction)
        {
        }

        public ComputeTaskItem(CancellationToken token) : base(token)
        {

        }

        /// WARNING: only call this from the thread intendend to host the compute device - or bad things will happen
        protected SprueBindings.ComputeDevice GetComputeDevice()
        {
            if (computeDevice_ == null)
            {
                computeDevice_ = new SprueBindings.ComputeDevice();
                if (computeDevice_.Initialize())
                {
                    ErrorHandler.inst().Debug("Initialized compute device");
                }
                else
                    ErrorHandler.inst().Error("Failed to initialize compute device");
            }
            return computeDevice_;
        }
    }
}
