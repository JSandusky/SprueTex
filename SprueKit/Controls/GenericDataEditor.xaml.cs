using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SprueKit.Controls
{
    /// <summary>
    /// Interaction logic for GenericDataEditor.xaml
    /// </summary>
    public partial class GenericDataEditor : UserControl
    {
        bool constructed = false;

        public static readonly DependencyProperty TargetObjectProperty = 
            DependencyProperty.Register("TargetObject", typeof(object), typeof(GenericDataEditor),
                new PropertyMetadata(null, OnTargetChanged));

        private static void OnTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = d as GenericDataEditor;
            if (self.DataContext != null && !self.constructed)
            {
                self.contentGrid.Children.Clear();
                self.constructed = true;
                bool wantsLabel = false;
                object editing = self.GetValue(TargetObjectProperty);
                self.contentGrid.Children.Add(Editors.FieldEditorBuilder.CreateControl("", new Binding("Value") { Source = self.DataContext }, editing.GetType(), self.DataContext.GetType().GetProperty("Value"), out wantsLabel));
            }
        }

        public object TargetObject
        {
            get { return GetValue(TargetObjectProperty); }
            set { SetValue(TargetObjectProperty, value); }
        }

        public GenericDataEditor()
        {
            InitializeComponent();
            DataContextChanged += GenericDataEditor_DataContextChanged;
        }

        private void GenericDataEditor_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //if (DataContext != null)
            //{
            //    bool wantsLabel = false;
            //    object editing = GetValue(TargetObjectProperty);
            //    contentGrid.Children.Add(Editors.FieldEditorBuilder.CreateControl("", new Binding("Value") { Source = DataContext }, editing.GetType(), DataContext.GetType().GetProperty("Value"), out wantsLabel));
            //}
        }
    }
}
