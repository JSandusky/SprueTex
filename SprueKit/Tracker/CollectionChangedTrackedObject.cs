namespace Notify
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Wraps <see cref="INotifyCollectionChanged"/>. Responsibility:
    /// <list type="bullet">
    /// <item>    
    ///     <description>
    ///         Handle <see cref="INotifyCollectionChanged.CollectionChanged"/> and fire <see cref="Changed"/>.
    ///     </description>
    /// </item>
    /// <item>    
    ///     <description>
    ///         Create a <see cref="TrackedObject"/> for every existing or newly added element in the collection
    ///         and handle its <see cref="Changed"/> event. Keep track of these elements in 
    ///         <see cref="_registeredElements"/> so that they can be cleaned up later.
    ///     </description>
    /// </item>
    /// <item>    
    ///     <description>
    ///         If there are deleted elements, dispose the associated <see cref="TrackedObject"/>s 
    ///         in <see cref="_registeredElements"/>
    ///         so that they can be garbage collected.
    ///     </description>
    /// </item>
    /// </list>
    /// </summary>
    public class CollectionChangedTrackObject : TrackedObject
    {
        private readonly Dictionary<object, TrackCount> _registeredElements =
            new Dictionary<object, TrackCount>(ObjectReferenceEqualityComparer<object>.Default);

        public TrackedObject Owner { get; private set; }
        public object TrackedOwner { get; private set; }

        public NotifyCollectionChangedAction RecentAction { get; private set; }
        public NotifyCollectionChangedEventArgs RecentArgs { get; private set; }

        internal CollectionChangedTrackObject(object tracked, TrackedObject owner)
            : base(tracked)
        {
            Owner = owner;
            if (owner != null)
                TrackedOwner = owner.Tracked;
        }

        internal override void RegisterTrackedObject(TrackedObject owner)
        {
            foreach (var element in (IEnumerable)Tracked)
                RegisterElement(element);
            ((INotifyCollectionChanged)Tracked).CollectionChanged += OnCollectionChanged;
        }

        internal override void UnregisterTrackedObject()
        {
            RemoveAllElements();
            ((INotifyCollectionChanged)Tracked).CollectionChanged -= OnCollectionChanged;
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            RecentAction = args.Action;
            RecentArgs = args;
            if (args.Action == NotifyCollectionChangedAction.Reset)
                RemoveAllElements();
            else
            {
                if (args.OldItems != null)
                    foreach (var oldItem in args.OldItems)
                        RemoveElement(oldItem);

                if (args.NewItems != null)
                    foreach (var newElement in args.NewItems)
                        RegisterElement(newElement);
            }
            OnChange(null, this, "");
        }

        private void RegisterElement(object element)
        {
            if (!IsValidObjectType(element)) return;

            var trackedObject = Create(element);
            if (!_registeredElements.ContainsKey(element))
            {
                _registeredElements.Add(element, new TrackCount(trackedObject, 0));
                trackedObject.Changed += OnChange;
            }
            _registeredElements[element].Count++;
        }

        private void RemoveElement(object element)
        {
            if (_registeredElements.ContainsKey(element))
            {
                var trackCount = _registeredElements[element];
                trackCount.Count--;
                if (trackCount.Count == 0)
                {
                    _registeredElements.Remove(element);
                    trackCount.TrackedObject.Dispose();
                }
            }
        }

        private void RemoveAllElements()
        {
            _registeredElements.Keys.ToList().ForEach(RemoveElement);
        }

        private class TrackCount
        {
            public readonly TrackedObject TrackedObject;
            public int Count;

            public TrackCount(TrackedObject trackedObject, int count)
            {
                TrackedObject = trackedObject;
                Count = count;
            }
        }

        private class ObjectReferenceEqualityComparer<T> : EqualityComparer<T> where T : class
        {
            public static new readonly IEqualityComparer<T> Default =
                new ObjectReferenceEqualityComparer<T>();

            public override bool Equals(T x, T y)
            {
                return ReferenceEquals(x, y);
            }

            public override int GetHashCode(T obj)
            {
                return RuntimeHelpers.GetHashCode(obj);
            }
        }
    }
}
