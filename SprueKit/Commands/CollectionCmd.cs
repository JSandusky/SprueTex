using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit.Commands
{
    public abstract class BaseCollectionCmd : UndoRedoCmd
    {
        protected IList owningList_;
        protected object changedObject_;
        protected object listOwner_;
        protected int index_;
    }

    public class CollectionAddRemoveCmd : BaseCollectionCmd
    {
        bool isAdd = true;
        int index_ = -1;

        public CollectionAddRemoveCmd(bool isAdd, IList list, int idx, object whoAdded, object owner)
        {
            this.isAdd = isAdd;
            owningList_ = list;
            changedObject_ = whoAdded;
            listOwner_ = owner;
            index_ = idx;

            SetText();
        }

        public override void Merge(UndoRedoCmd cmd)
        {
            
        }

        protected override void Execute(bool isRedo)
        {
            if (!isAdd)
                isRedo = !isRedo;
            if (isRedo)
            {
                if (index_ != -1 && index_ < owningList_.Count)
                    owningList_.Insert(index_, changedObject_);
                else
                    owningList_.Add(changedObject_);
            }
            else
                owningList_.Remove(changedObject_);
        }

        void SetText()
        {
            if (isAdd)
                Message = string.Format("Add {0} to {1}", GetContentName(), GetObjectName(listOwner_));
            else
                Message = string.Format("Remove {0} from {1}", GetContentName(), GetObjectName(listOwner_));
        }

        string GetContentName()
        {
            return changedObject_.GetType().Name;
        }

        public override System.Windows.Media.ImageSource Icon
        {
            get
            {
                if (isAdd)
                    return WPFExt.GetEmbeddedImage("Images/icon_add_green.png");
                else
                    return WPFExt.GetEmbeddedImage("Images/icon_remove_red.png");
            }
        }
    }

    public class CollectionMoveCmd : UndoRedoCmd
    {
        protected IList originalList_;
        protected IList newList_;
        protected object originalListOwner_;
        protected object newListOwner_;
        protected int originalIndex_;
        protected int newIndex_;
        protected IList movedObjects_;

        public CollectionMoveCmd(IList objects, IList oList, object oOwner, int oIndex, IList nList, object nOwner, int nIndex)
        {
            movedObjects_ = objects;

            originalList_ = oList;
            originalListOwner_ = oOwner;
            originalIndex_ = oIndex;

            newList_ = nList;
            newListOwner_ = nOwner;
            newIndex_ = nIndex;

            SetText();
        }

        void SetText()
        {
            List<object> stuff = new List<object>();
            foreach (var o in movedObjects_)
                stuff.Add(o);

            string contentList = string.Join(", ", stuff.Select(o => GetObjectName(o)));
            if (newList_ == originalList_)
                Message = string.Format("Reorder {0} in {1}", contentList, GetObjectName(originalListOwner_));
            else
                Message = string.Format("Move {0} from {1} to {2}", contentList, GetObjectName(originalListOwner_), GetObjectName(newListOwner_));
        }

        protected override void Execute(bool isRedo)
        {
            if (isRedo)
            {
                int index = newIndex_;
                foreach (var item in movedObjects_)
                {
                    originalList_.Remove(item);
                    if (index > newList_.Count)
                        newList_.Add(item);
                    else
                        newList_.Insert(index, item);
                    SetParent(item, newListOwner_);
                    ++index;
                }
            }
            else
            {
                int index = originalIndex_;
                foreach (var item in movedObjects_)
                {
                    newList_.Remove(item);
                    if (index > newList_.Count)
                        originalList_.Add(item);
                    else
                        originalList_.Insert(index, item);
                    SetParent(item, originalListOwner_);
                    ++index;
                }
            }
        }

        void SetParent(object item, object parent)
        {
            if (item is SprueKit.Data.SpruePiece && parent is SprueKit.Data.SpruePiece)
                ((SprueKit.Data.SpruePiece)item).Parent = ((SprueKit.Data.SpruePiece)parent);
        }

        public override void Merge(UndoRedoCmd cmd) { }

        public override System.Windows.Media.ImageSource Icon {
            get { 
                return WPFExt.GetEmbeddedImage("Images/icon_blend_yellow.png");
            }
        }
    }
}
