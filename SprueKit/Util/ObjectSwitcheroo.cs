using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SprueKit.Util
{
    /// <summary>
    /// An intermediate object that when a 'Source' binding changes results in an evaluated 'Value' binding being set.
    /// Value converter can sort of do this.
    /// </summary>
    public class ObjectSwitcheroo : DependencyObject
    {
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(object), typeof(ObjectSwitcheroo), new PropertyMetadata(null, ObjectSwitcheroo.OnSourceChanged));

        public object Source { get { return GetValue(SourceProperty); } set { SetValue(SourceProperty, value); } }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(ObjectSwitcheroo), new PropertyMetadata(null));

        public object Value { get { return GetValue(ValueProperty); } set { SetValue(ValueProperty, value); } }

        public Func<object, object> TranslationFunction { get; set; }

        static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ObjectSwitcheroo self = d as ObjectSwitcheroo;
            if (self != null)
            {
                if (self.TranslationFunction != null)
                    self.Value = self.TranslationFunction(self.Source);
                else
                    self.Value = null;
            }
        }
    }
}
