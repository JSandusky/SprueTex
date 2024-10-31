using System;
using System.ComponentModel;

namespace Notify
{
    [AttributeUsage(AttributeTargets.Property)]
    [Description("Notifier is told to not trigger external work tasks, Undo/redo is recorded - but other events are not fired")]
    public class DontSignalWorkAttribute : Attribute
    {

    }

    /// <summary>
    /// This is an optional attribute. If this attribute is not applied to a tracked object, 
    /// all public properties of that object are automatically tracked.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    [Description("This is an optional attribute. If this attribute is not applied to a tracked object, all public properties of that object are automatically tracked.")]
    public class TrackClassAttribute : Attribute
    {
        private bool _includeBaseProperties = true;

        /// <summary>
        /// If <c>true</c> (default), track up the inheritance hierarchy.
        /// If <c>false</c>, only declared properties are tracked.
        /// </summary>
        [Description("If true (default), track up the inheritance hierarchy. If false, only declared properties are tracked.")]
        public bool IncludeBaseProperties
        {
            get { return _includeBaseProperties; }
            set { _includeBaseProperties = value; }
        }

        private bool _requireExplicitMarking = true;

        /// <summary>
        /// If <c>true</c> (default), only properties marked with <see cref="TrackMemberAttribute"/> are tracked.
        /// If <c>false</c>, public properties are tracked.
        /// </summary>
        [Description("If true (default), only properties marked with 'TrackMemberAttribute' are tracked. If false, public properties are tracked.")]
        public bool RequireExplicitMarking
        {
            get { return _requireExplicitMarking; }
            set { _requireExplicitMarking = value; }
        }
    }

    /// <summary>
    /// This is an optional attribute. Use it to force tracking or not tracking a property.
    /// This attribute can be used independently of <see cref="TrackClassAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    [Description("This is an optional attribute. Use it to force tracking or not tracking a property. This attribute can be used independently of 'TrackClassAttribute'")]
    public class TrackMemberAttribute : Attribute
    {
        /// <summary>
        /// Explicitly request <see cref="Tracker"/> to exclude this property from being tracked.
        /// Default is <c>false</c>.
        /// </summary>
        [Description("Explicitly request 'Tracker' to exclude this property from being tracked. Default is false.")]
        public bool IsExcluded { get; set; }
    }
}
