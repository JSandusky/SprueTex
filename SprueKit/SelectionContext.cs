using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace SprueKit
{
    /// <summary>
    /// Manages the current state of selection, used by Document classes.
    /// </summary>
    public class SelectionContext : BaseClass
    {
        bool blockSignals = false;
        public ObservableCollection<object> Selected { get; private set; } = new ObservableCollection<object>();

        /// <summary>
        /// "Detail" objects are sub-thing selections.
        /// </summary>
        public ObservableCollection<object> DetailedSelected { get; private set; } = new ObservableCollection<object>();

        public SelectionContext()
        {
            Selected.CollectionChanged += (o, e) => {
                if (!blockSignals)
                    OnPropertyChanged("MostRecentlySelected");
            };

            DetailedSelected.CollectionChanged += (o, e) =>
            {
                if (!blockSignals)
                    OnPropertyChanged("MostRecentDetail");
            };
        }

        /// <summary>
        /// Clears any existing selection and then adds the new selection
        /// </summary>
        /// <param name="obj"></param>
        public void SetSelected(object obj)
        {
            blockSignals = true;
            Selected.Clear();
            blockSignals = false;
            // Passing null will just clear all selections
            if (obj != null)
                Selected.Add(obj);
        }

        public void SetDetailSelected(object obj)
        {
            blockSignals = true;
            DetailedSelected.Clear();
            blockSignals = false;
            if (obj != null)
                DetailedSelected.Add(obj);
        }

        public object MostRecentlySelected { get { return Selected.LastOrDefault(); } }

        public object MostRecentDetail { get { return DetailedSelected.LastOrDefault(); } }


        /// <summary>
        /// Get the object at the given index as T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="index"></param>
        /// <returns></returns>
        public T GetSelected<T>(int index = 0) where T : class
        {
            return Selected[index] as T;
        }

        public T GetDetail<T>(int index = 0) where T : class
        {
            return DetailedSelected[index] as T;
        }

        public void Toggle(object sel)
        {
            if (Selected.Contains(sel))
                Selected.Remove(sel);
            else
                Selected.Add(sel);
        }

        /// <summary>
        /// Returns the first encountered object of the given type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T FirstSelectedOf<T>() where T: class
        {
            for (int i = 0; i < Selected.Count; ++i)
            {
                T val = Selected[i] as T;
                if (val != null)
                    return val;
            }
            return default(T);
        }

        public bool IsSelected(object o)
        {
            return Selected.Contains(o) || DetailedSelected.Contains(o);
        }

        /// <summary>
        /// Remove all items derived from type T from the list
        /// </summary>
        public void RemoveSelected<T>() where T : class
        {
            blockSignals = true;
            var toRemove = Selected.Where(p => typeof(T).IsAssignableFrom(p.GetType())).ToArray();
            if (toRemove != null)
            {
                foreach (var item in toRemove)
                    Selected.Remove(item);
            }
            blockSignals = false;

            if (toRemove != null && toRemove.Length > 0)
            {
                OnPropertyChanged("Selected");
                OnPropertyChanged("MostRecentlySelected");
            }
        }
    }
}
