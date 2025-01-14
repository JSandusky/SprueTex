﻿namespace Notify
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq;

    /// <summary>
    /// Creates an instance of this class to track changes to as many objects as needed.
    /// It automatically drills down and track changes to all objects reachable from the 
    /// explicitly tracked objects. In a MVVM application, it should be sufficient to track 
    /// just the root view model.
    /// </summary>
    /// <remarks>
    /// This class is not thread-safe. If you want to access instances of this class
    /// from multiple threads, you need to synchronize properly.
    /// </remarks>
    public class Tracker : IDisposable
    {
        private readonly List<TrackedObject> _roots = new List<TrackedObject>();

        /// <summary>
        /// Handle this event to receive change notification. 
        /// </summary>
        public event Changed Changed;

        internal static Tracker context;

        public static int InSideEffects { get; set; }

        public class TrackingSideEffects : IDisposable
        {
            public TrackingSideEffects()
            {
                ++InSideEffects;
            }

            public void Dispose()
            {
                --InSideEffects;
            }
        }

        /// <summary>
        /// Tracks one or more objects and all of their properties, including collections, recursively.
        /// Can be invoked multiple times for different objects.
        /// </summary>
        /// <param name="objects">Objects to be tracked.
        /// These objects must not be <c>null</c> and must implements one or both of these interfaces:
        /// <list type="bullet">
        ///     <item><description><see cref="INotifyPropertyChanged"/></description></item>
        ///     <item><description><see cref="INotifyCollectionChanged"/> (and must also be an <see cref="IEnumerable"/>)</description></item>
        /// </list>
        /// </param>
        /// <returns>This tracker object.</returns>
        public Tracker Track(params object[] objects)
        {
            if (objects == null || objects.Length == 0)
                throw new ArgumentException("No object to track");

            context = this;
            var toBeTracked = objects.Select(o => TrackedObject.Create(o, null)).ToList();
            toBeTracked.ForEach(o => {
                o.Changed += (src,who,prop)  => {
                    if (InSideEffects == 0)
                    {
                        if (Changed != null)
                            Changed(this, who, prop);
                    }
                };
                _roots.Add(o);
            });
            return this;
        }

        /// <summary>
        /// Cleans up the tracker and all tracked objects.
        /// </summary>
        public void Dispose()
        {
            Changed = null;
            _roots.ForEach(o => o.Dispose());
            _roots.Clear();
        }
    }
}
