using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit
{
    public class DisposalQueue
    {
        static DisposalQueue inst_ = new DisposalQueue();
        Queue<IDisposable> queue_ = new Queue<IDisposable>();

        private DisposalQueue() { }

        public static DisposalQueue Inst { get { return inst_; } }

        public void Queue(IDisposable tgt)
        {
            lock (queue_)
                queue_.Enqueue(tgt);
        }

        public void Clear()
        {
            lock (queue_)
            {
                while (queue_.Count > 0)
                    queue_.Dequeue().Dispose();
            }
        }
    }
}
