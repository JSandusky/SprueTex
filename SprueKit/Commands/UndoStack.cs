using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace SprueKit.Commands
{
    public interface IUndoStorage
    {
        void Add(UndoRedoCmd cmd);
    }

    public class UndoStack : BaseClass, IUndoStorage
    {
        public delegate void UndoRedoActionPerformedHandler(UndoStack stack);

        public event UndoRedoActionPerformedHandler UndoRedoActionPerformed;
               
        public ObservableCollection<UndoRedoCmd> Undo { get; private set; } = new ObservableCollection<UndoRedoCmd>();
        public ObservableCollection<UndoRedoCmd> Redo { get; private set; } = new ObservableCollection<UndoRedoCmd>();

        public List<UndoRedoCmd> InlineUndoRedo { get {
                List<UndoRedoCmd> ret = new List<UndoRedoCmd>();
                ret.AddRange(Undo);
                ret.AddRange(Redo);
                return ret;
            }
        }

        void Signal()
        {
            if (UndoRedoActionPerformed != null)
                UndoRedoActionPerformed(this);
        }

        public void Add(UndoRedoCmd cmd)
        {
            cmd.OwningStack = this;
            if (Undo.Count > 0)
            {
                if (Undo[Undo.Count - 1].RecentEnough() && Undo[Undo.Count - 1].ShouldMerge(cmd))
                {
                    Undo[Undo.Count - 1].Merge(cmd);
                    Redo.Clear();
                    Undo[Undo.Count - 1].MadeCurrent();
                    return;
                }
            }


            //cmd.Redo();
            cmd.MadeCurrent();
            Undo.Add(cmd);
            if (Undo.Count > 64)
                Undo.RemoveAt(0);
            Redo.Clear();
            OnPropertyChanged("InlineUndoRedo");
        }

        public bool IsLatest(UndoRedoCmd cmd)
        {
            if (Undo.Count > 0)
                return Undo[Undo.Count - 1] == cmd;
            return false;
        }

        public void RedoUntil(UndoRedoCmd cmd)
        {
            using (var blocker = new Notify.Tracker.TrackingSideEffects())
            {
                while (Redo.Count > 0)
                {
                    var item = Redo[0];
                    item.Redo();
                    Undo.Add(item);
                    Redo.RemoveAt(0);
                    item.OnPropertyChanged(string.Empty);
                    OnPropertyChanged("InlineUndoRedo");
                    if (item == cmd)
                        break;
                }
            }
            NotifyCurrent();
        }

        public void UndoOne()
        {
            if (Undo.Count > 0)
                UndoUntil(Undo[Undo.Count - 1]);
        }

        public void UndoAll()
        {
            if (Undo.Count > 0)
                UndoUntil(Undo[0]);
        }

        public void RedoOne()
        {
            if (Redo.Count > 0)
                RedoUntil(Redo[0]);
        }

        public void RedoAll()
        {
            if (Redo.Count > 0)
                RedoUntil(Redo[Redo.Count - 1]);
        }

        public void UndoUntil(UndoRedoCmd cmd)
        {
            while (Undo.Count > 0)
            {
                using (var blocker = new Notify.Tracker.TrackingSideEffects())
                {
                    var item = Undo[Undo.Count - 1];
                    item.Undo();
                    Redo.Insert(0, item);
                    Undo.Remove(item);
                    item.OnPropertyChanged(string.Empty);
                    OnPropertyChanged("InlineUndoRedo");
                    if (item == cmd)
                        break;
                }
            }
            NotifyCurrent();
        }

        void NotifyCurrent()
        {
            if (Undo.Count > 0)
                Undo[Undo.Count - 1].MadeCurrent();
            Signal();
        }
    }
}
