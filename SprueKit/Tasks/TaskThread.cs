using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SprueKit.Tasks
{
    /// <summary>
    /// Provides tasks to a task thread.
    /// </summary>
    public class TaskSource
    {
        public List<TaskItem> Tasks { get; private set; } = new List<TaskItem>();

        public int TaskCount { get { int ret = 0;  lock(this) { ret = Tasks.Count; } return ret; } }

        public virtual void AddTask(TaskItem item)
        {
            lock (this)
            {
                Tasks.Add(item);
            }
        }

        public virtual TaskItem Next()
        {
            lock (this)
            {
                while (Tasks.Count > 0)
                {
                    var testTask = Tasks[0];
                    Tasks.RemoveAt(0);
                    if (!testTask.IsCanceled)
                        return testTask;
                    else
                        testTask.Dispose();
                }
            }
            return null;
        }

        public virtual void FinishTask(TaskItem task)
        {
            if (!task.IsCanceled)
            {
                task.TaskEnd();
                if (task.Consequences.Count > 0)
                {

                    foreach (var subItem in task.Consequences)
                        subItem.ParentFinished(task);

                    lock (this)
                    {
                        Tasks.InsertRange(0, task.Consequences);
                    }
                }
            }
            else
            {
                task.CanceledEnd();
            }

            task.Dispose();
        }
    }

    //[IOCInitialized(Count = 3)]
    public class TaskThread : TaskSource
    {
        TaskSource source_;
        Thread thread_;
        int threadTag_;
        public TaskThread(int threadTag, TaskSource src = null)
        {
            if (src == null)
                source_ = this;
            else
                source_ = src;

            threadTag_ = threadTag;
            thread_ = new Thread(new System.Threading.ThreadStart(() =>
            {
                for (;;)
                {
                    var task = source_.Next(); // grab next task
                    //try
                    {
                        while (task != null) // as long as we have a task then keep working
                        {
                            App.WipeWindowMessage(threadTag_);
                            task.Message = new WinMsg { Text = string.Format("{0}", task.TaskName), Tag = threadTag_, Duration = 0 };
                            App.PushWindowMessage(task.Message);
                            task.TaskLaunch();
                            if (task.MainAction != null)
                                task.MainAction();
                            source_.FinishTask(task);

                            task = source_.Next(); // try to grab next available
                        }
                        App.WipeWindowMessage(threadTag_);
                    }
                    //catch (Exception ex)
                    //{
                    //    if (task != null)
                    //        task.ThrewException(ex);
                    //    App.PushWindowMessage(new WinMsg { Text = string.Format("Failed: {0}", task.ToString()) });
                    //    App.WipeWindowMessage(1);
                    //    ErrorHandler.inst().Error(ex);
                    //}
                    Thread.Sleep(30 + Math.Abs(threadTag_) * 10);
                }
            }));
            thread_.Priority = ThreadPriority.AboveNormal;
            thread_.IsBackground = true;
            thread_.Start();
        }
    }
}
