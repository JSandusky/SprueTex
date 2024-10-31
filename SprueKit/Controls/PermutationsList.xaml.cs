using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    /// Interaction logic for PermutationsList.xaml
    /// </summary>
    public partial class PermutationsList : UserControl
    {
        public static readonly DependencyProperty PendingContextProperty =
            DependencyProperty.Register(
                "PendingContext",
                typeof(object),
                typeof(PermutationsList),
                new PropertyMetadata(null, OnPendingContextChanged));

        public object PendingContext
        {
            get { return (object)GetValue(PendingContextProperty); }
            set { SetValue(PendingContextProperty, value); }
        }

        private static async void OnPendingContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PermutationsList self = d as PermutationsList;
            object target = self.PendingContext;
            if (target == null)
                self.DataContext = target;
            else
            {
                await Task.Delay(40);
                while (self.PendingContext != target)
                {
                    target = self.PendingContext;
                    await Task.Delay(40);
                }
                if (self.PendingContext != self.DataContext)
                {
                    self.Dispatcher.Invoke(new Action(() => {
                        self.DataContext = self.PendingContext;
                    }));
                }
            }
        }

        public PermutationsList()
        {
            InitializeComponent();
            var documentManager = new IOCDependency<DocumentManager>();
            documentManager.Object.OnActiveDocumentChanged += Object_OnActiveDocumentChanged;
        }

        object targetObject_;
        PropertyInfo targetProperty_;

        public void SetTarget(object obj, PropertyInfo pi)
        {
            targetProperty_ = pi;
            targetObject_ = obj;
        }

        private void Object_OnActiveDocumentChanged(Document newDoc, Document oldDoc)
        {
            if (newDoc != null)
                SetBinding(PendingContextProperty, new Binding("Selection.MostRecentlySelected") { Source = newDoc, Delay = 50 });
            else
                SetBinding(PendingContextProperty, new Binding("Selection.MostRecentlySelected") { Source = null });
        }

        private void UsePermClick(object sender, RoutedEventArgs e)
        {
            var value = ((Button)sender).Tag as Data.PermutationValue;
            if (targetObject_ != null && targetProperty_ != null)
                targetProperty_.SetValue(targetObject_, value.Value);
        }

        private void DeletePermClick(object sender, RoutedEventArgs e)
        {
            var value = ((Button)sender).Tag as Data.PermutationValue;
            ((System.Collections.IList)permutationsList.ItemsSource).Remove(value);

            var target = targetObject_ as SprueKit.Data.IPermutable;
            if (target != null)
                target.SignalPermutationChange();
        }
    }
}
