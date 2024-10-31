namespace Notify
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;

    /// <summary>
    /// Represents a tracked object and wraps the original object.
    /// </summary>
    public abstract class TrackedObject : IDisposable
    {
        public event Changed Changed;
        public readonly object Tracked;
        protected Tracker tracker;

        protected TrackedObject(object tracked)
        {
            Tracked = tracked;
        }

        /// <summary>
        /// Factory method to create a correct subclass of <see cref="TrackedObject"/>.
        /// </summary>
        /// <param name="obj">The object to be tracked.</param>
        /// <returns>An instance of a subclass of <see cref="TrackedObject"/>.</returns>
        internal static TrackedObject Create(object obj, TrackedObject owner = null)
        {
            if (!IsValidObjectType(obj))
                throw new ArgumentException("null or invalid object type");

            TrackedObject trackedObject;
            //if (obj is INotifyCollectionChanged && obj is INotifyPropertyChanged)
            //    trackedObject = new DualTrackedObject(obj);
            if (obj is INotifyCollectionChanged)
                trackedObject = new CollectionChangedTrackObject(obj, owner);
            else
                trackedObject = new PropertyChangedTrackedObject(obj, owner);

            trackedObject.RegisterTrackedObject(owner);
            return trackedObject;
        }

        protected static bool IsValidObjectType(object obj)
        {
            return obj is INotifyPropertyChanged ||
                   (obj is INotifyCollectionChanged && obj is IEnumerable);
        }

        internal abstract void RegisterTrackedObject(TrackedObject owner);

        internal abstract void UnregisterTrackedObject();

        protected void OnChange(Tracker tracker, TrackedObject who, string prop)
        {
            if (Changed != null)
                Changed(tracker, who, prop);
        }

        public void Dispose()
        {
            Changed = null;
            UnregisterTrackedObject();
        }
    }
}
