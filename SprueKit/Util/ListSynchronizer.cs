using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Data;

namespace SprueKit.Util
{
    /// <summary>
    /// Connects an ObservableCollection to a UIElementCollection via a control factory method.
    /// The generated control must have it's 'Tag' set to the object it was created from,
    /// this will be enforced by the ListSynchronizer regardless of what the factory method chooses to do.
    /// </summary>
    public class ListSynchronizer<T> where T : class
    {
        CompositeCollection coll;
        public ListSynchronizer(Func<T, FrameworkElement> controlFactorMethod, ObservableCollection<T> sourceList, UIElementCollection ctrlTarget)
        {
            ControlFactory = controlFactorMethod;
            ControlCollection = ctrlTarget;
            SourceCollection = sourceList;

            FillTarget();

            SourceCollection.CollectionChanged += SourceList_CollectionChanged;
        }

        public void FillTarget()
        {
            foreach (var item in SourceCollection)
            {
                var ctrl = ControlFactory(item);
                if (ctrl != null)
                {
                    ctrl.Tag = item;
                    ControlCollection.Add(ctrl);
                }
            }
        }

        private void SourceList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
        // WARNING: Not all possible transmutations of a list are covered!
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                // Add any new controls as needed
                int idx = e.NewStartingIndex;
                foreach (var item in e.NewItems)
                {
                    var newControl = ControlFactory(item as T);
                    if (newControl != null)
                    {
                        newControl.Tag = item;
                        ControlCollection.Insert(idx, newControl);
                        ++idx;
                    }
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                // Remove any disappearing elements
                foreach (var item in e.OldItems)
                {
                    foreach (var tgt in ControlCollection)
                    {
                        FrameworkElement elem = tgt as FrameworkElement;
                        if (elem != null)
                        {
                            if (elem.Tag == item)
                            {
                                ControlCollection.Remove(elem);
                                break;
                            }
                        }
                    }
                }
            }            
        }

        Func<T, FrameworkElement> ControlFactory { get; set; }
        ObservableCollection<T> SourceCollection { get; set; }
        UIElementCollection ControlCollection { get; set; }
    }
}
