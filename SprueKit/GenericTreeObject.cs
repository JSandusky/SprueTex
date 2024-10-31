using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit
{
    /// <summary>
    /// Used for generic read-only trees.
    /// Keeps data simple
    /// </summary>
    public class GenericTreeObject : BaseClass
    {
        object dataObject_;
        public object DataObject { get { return dataObject_; } set { dataObject_ = value; OnPropertyChanged(); } }

        public ObservableCollection<object> Children { get; private set; } = new ObservableCollection<object>();

        public List<T> GetChildren<T>() where T : new()
        {
            List<T> ret = new List<T>();

            foreach (var child in Children)
            {
                if (child is T)
                    ret.Add((T)child);
                else if (child is GenericTreeObject && ((GenericTreeObject)child).DataObject is T)
                    ret.Add((T)((GenericTreeObject)child).DataObject);
            }

            return ret;
        }
    }
}
