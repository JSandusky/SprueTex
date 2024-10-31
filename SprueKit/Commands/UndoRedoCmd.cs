using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

using Microsoft.Xna.Framework;

namespace SprueKit.Commands
{
    public abstract class UndoRedoCmd : BaseClass, IDisposable
    {
        bool isFirstRedo_ = true;
        bool lastActionWasUndo_ = false;
        DateTime time_;

        public UndoRedoCmd()
        {
            time_ = DateTime.Now;
        }

        public UndoStack OwningStack { get; set; }

        public bool IsCurrent {
            get
            {
                return OwningStack != null ? OwningStack.Undo.LastOrDefault() == this : false;
            }
        }

        public bool IsUndo { get { return lastActionWasUndo_ == false; } }
        public bool IsRedo { get { return lastActionWasUndo_ == true; } }
        

        public virtual void MadeCurrent()
        {

        }

        public virtual void Dispose() { }

        public void Redo()
        {
            DoRedo();
            isFirstRedo_ = false;
            lastActionWasUndo_ = false;
        }

        public virtual bool ShouldMerge(UndoRedoCmd cmd)
        {
            if (!RecentEnough())
                return false;
            return false;
        }

        public bool RecentEnough()
        {
            // 10 second window
            return DateTime.Now.Subtract(time_).TotalSeconds < 10;
        }

        public void Undo()
        {
            DoUndo();
            lastActionWasUndo_ = true;
        }

        protected bool IsFirstRedo { get { return isFirstRedo_; } }

        protected virtual void DoRedo() { Execute(true); }
        protected virtual void DoUndo() { Execute(false); }

        protected abstract void Execute(bool isRedo);
        public abstract void Merge(UndoRedoCmd cmd);

        string message_;
        public string Message { get { return message_; } set { message_ = value; OnPropertyChanged(); } }

        protected string GetObjectName(object obj)
        {
            if (obj is SprueKit.Data.SpruePiece)
                return ((SprueKit.Data.SpruePiece)obj).DisplayName;
            return obj.GetType().Name;
        }

        public abstract System.Windows.Media.ImageSource Icon { get; }
    }

    public class SpoofUndoRedoCmd : UndoRedoCmd
    {
        public SpoofUndoRedoCmd(string txt)
        {
            Message = string.Format("Changed '{0}'", txt);
        }

        public override bool ShouldMerge(UndoRedoCmd cmd)
        {
            return cmd.Message.Equals(Message);
        }

        public override void Merge(UndoRedoCmd cmd)
        {
            
        }

        protected override void Execute(bool isRedo)
        {
            
        }

        public override System.Windows.Media.ImageSource Icon { get { return null; } }
    }

    /// <summary>
    /// Simple undo/redo memento for working with properties.
    /// </summary>
    public class BasicPropertyCmd : UndoRedoCmd
    {
        string propertyName_;
        object object_, oldValue_, newValue_;

        public BasicPropertyCmd(string propertyName, object obj, object oldValue, object newValue)
        {
            propertyName_ = propertyName;
            object_ = obj;
            oldValue_ = oldValue;
            newValue_ = newValue;

            SetText();
        }

        void SetText()
        {
            string objectName = GetObjectName(object_);
            if (!string.IsNullOrEmpty(objectName))
                Message = string.Format("[{2}] Set '{0}' to {1}", propertyName_, newValue_ != null ? GetValueText(newValue_) : "null", objectName);
            else
                Message = string.Format("Set '{0}' to {1}", propertyName_, newValue_ != null ? GetValueText(newValue_) : "null");
        }

        public override bool ShouldMerge(UndoRedoCmd cmd)
        {
            Debug.Assert(cmd != null);
            if (!RecentEnough())
                return false;

            BasicPropertyCmd rhs = cmd as BasicPropertyCmd;
            if (rhs != null && object_ == rhs.object_)
            {
                if (propertyName_.Equals(rhs.propertyName_))
                    return true;
            }
            return false;
        }

        public override void Merge(UndoRedoCmd cmd)
        {
            BasicPropertyCmd rhs = cmd as BasicPropertyCmd;
            if (rhs != null)
            {
                newValue_ = rhs.newValue_;
                Message = rhs.Message;
            }
        }

        protected override void Execute(bool isRedo)
        {
            Debug.Assert(object_ != null);
            if (isRedo && !IsFirstRedo)
                object_.GetType().GetProperty(propertyName_).SetValue(object_, newValue_);
            else if (!isRedo)
                object_.GetType().GetProperty(propertyName_).SetValue(object_, oldValue_);
        }

        string GetValueText(object value)
        {
            if (value.GetType() == typeof(Matrix))
            {
                Matrix mat = (Matrix)value;
                Vector3 scl = new Vector3();
                Vector3 pos = new Vector3();
                Quaternion rot = new Quaternion();
                mat.Decompose(out scl, out rot, out pos);

                return string.Format("{0}, {1}, {2}", pos, rot, scl);
            }
            return value.ToString();
        }

        public override System.Windows.Media.ImageSource Icon {
            get {
                return WPFExt.GetEmbeddedImage("Images/icon_edit_yellow.png");
            }
        }
    }
}
