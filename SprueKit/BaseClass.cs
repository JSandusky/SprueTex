using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Runtime.CompilerServices;

namespace SprueKit
{
    public class PrePropertyChangedEventArgs
    {
        public string PropertyName { get; set; }
        public object OldValue { get; set; }
        public object NewValue { get; set; }

        public PrePropertyChangedEventArgs(object oldVal, object newVal, string name)
        {
            OldValue = oldVal;
            NewValue = newVal;
            PropertyName = name;
        }
    }

    [Serializable]
    public class BaseClass : INotifyPropertyChanged
    {
        [XmlIgnore]
        [PropertyData.PropertyIgnore]
        public BaseClass Parent { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler<PrePropertyChangedEventArgs> BeforePropertyChanged;

        public virtual void AllPropertiesChanged()
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(string.Empty));
            if (Parent != null)
                Parent.AllPropertiesChanged();
        }

        // Create the OnPropertyChanged method to raise the event 
        public virtual void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(name));
            if (Parent != null)
                Parent.OnPropertyChanged();
        }

        public virtual void PrePropertyChanged(object oldVal, object newVal, [CallerMemberName] string name = "")
        {
            if (BeforePropertyChanged != null)
                BeforePropertyChanged(this, new PrePropertyChangedEventArgs(oldVal, newVal, name));
        }
    }
}
