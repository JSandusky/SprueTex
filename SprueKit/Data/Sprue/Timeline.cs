using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace SprueKit.Data
{
    /// <summary>
    /// Basetype for storage in a timeline
    /// </summary>
    public abstract class KeyframeItem : BaseClass
    {
        float time_;
        float duration_ = 0.0f;

        public float Time {
            get { return time_; }
            set { time_ = value; OnPropertyChanged(); }
        }

        public float EndTime
        {
            get { return time_ + duration_; }
        }

        public float Duration
        {
            get { return duration_; }
            set { duration_ = value; OnPropertyChanged(); }
        }
    }

    public class DummyKeyframe : KeyframeItem
    {

    }

    /// <summary>
    /// Common type for animation timelines.
    /// Specialized versions deal with retargeting specifics.
    /// </summary>
    public class TimelineTrack : BaseClass
    {
        string name_;

        public string Name {
            get { return name_; }
            set { name_ = value; OnPropertyChanged(); }
        }
        
        public ObservableCollection<KeyframeItem> Keyframes { get; private set; } = new ObservableCollection<KeyframeItem>();

        public TimelineTrack()
        {
            Name = "Test Track w/ an Extremely long name";
            Random r = new Random();
            Keyframes.Add(new DummyKeyframe { Time = (float)(r.NextDouble() * 6) });
            Keyframes.Add(new DummyKeyframe { Time = (float)(r.NextDouble() * 6) });
            Keyframes.Add(new DummyKeyframe { Time = (float)(r.NextDouble() * 6) });
            Keyframes.Add(new DummyKeyframe { Time = (float)(r.NextDouble() * 6) });
            Keyframes.Add(new DummyKeyframe { Time = (float)(r.NextDouble() * 6) });
            //Keyframes.Add(new DummyKeyframe { Time = (float)(r.NextDouble() * 10) });
            //Keyframes.Add(new DummyKeyframe { Time = (float)(r.NextDouble() * 10) });
            //Keyframes.Add(new DummyKeyframe { Time = (float)(r.NextDouble() * 10) });
            //Keyframes.Add(new DummyKeyframe { Time = (float)(r.NextDouble() * 10) });
            //Keyframes.Add(new DummyKeyframe { Time = (float)(r.NextDouble() * 10) });
        }

        public void OrganizeKeyframes()
        {
            using (var block = new Notify.Tracker.TrackingSideEffects())
            {
                List<KeyframeItem> temp = new List<KeyframeItem>(Keyframes.OrderBy(o => o.Time));
                Keyframes.Clear();
                foreach (var key in temp)
                    Keyframes.Add(key);
                OnPropertyChanged("Keyframes");
            }
        }

        public float MaxTime
        {
            get
            {
                return Keyframes.Max(p => p.Time + p.Duration);
            }
        }

        public float MinTime
        {
            get
            {
                return Keyframes.Min(p => p.Time);
            }
        }

        public float Range
        {
            get
            {
                return MaxTime - MinTime;
            }
        }
    }

    public class Timeline
    {
        public ObservableCollection<TimelineTrack> Tracks { get; private set; } = new ObservableCollection<TimelineTrack>();

        public Timeline()
        {
            Tracks.Add(new TimelineTrack());
            Tracks.Add(new TimelineTrack());
            Tracks.Add(new TimelineTrack());
            Tracks.Add(new TimelineTrack());
            Tracks.Add(new TimelineTrack());
            Tracks.Add(new TimelineTrack());
            Tracks.Add(new TimelineTrack());
            Tracks.Add(new TimelineTrack());
            Tracks.Add(new TimelineTrack());
            Tracks.Add(new TimelineTrack());
            Tracks.Add(new TimelineTrack());
            Tracks.Add(new TimelineTrack());
            Tracks.Add(new TimelineTrack());
            Tracks.Add(new TimelineTrack());
            Tracks.Add(new TimelineTrack());
            Tracks.Add(new TimelineTrack());
        }

        public float MaxTime
        {
            get
            {
                if (Tracks.Count == 0)
                    return 0.0f;
                return Tracks.Max(t => t.MaxTime);
            }
        }

        public float MinTime
        {
            get
            {
                if (Tracks.Count == 0)
                    return 0.0f;
                return Tracks.Min(t => t.MinTime);
            }
        }

        public float Range
        {
            get
            {
                if (Tracks.Count == 0)
                    return 0.0f;
                return MaxTime - MinTime;
            }
        }

        public void OrganizeKeyframes()
        {
            using (var block = new Notify.Tracker.TrackingSideEffects())
            {
                foreach (var track in Tracks)
                    track.OrganizeKeyframes();
            }
        }
    }
}
