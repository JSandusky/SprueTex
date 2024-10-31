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

using SprueKit.Commands;

namespace SprueKit.Controls
{
    /// <summary>
    /// Interaction logic for UndoHistoryCtrl.xaml
    /// </summary>
    public partial class UndoHistoryCtrl : UserControl
    {
        public UndoHistoryCtrl()
        {
            InitializeComponent();
            DataContext = null;
            DataContextChanged += UndoHistoryCtrl_DataContextChanged;
        }

        private void UndoHistoryCtrl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext != null && DataContext is Document)
                ListView.DataContext = ((Document)DataContext).UndoRedo;
            else
                ListView.DataContext = null;
        }

        UndoStack stack_;
        public UndoStack Stack { get { return stack_; } set { stack_ = value; DataContext = null;  DataContext = this; } }

        public List<UndoRedoCmd> UndoStack
        {
            get
            {
                List<UndoRedoCmd> ret = new List<UndoRedoCmd>();

                var stack = Stack;
                if (stack != null)
                {
                    ret.AddRange(stack.Undo);
                    ret.AddRange(stack.Redo);
                }

                return ret;
            }
        }

        public void UndoUntil(UndoRedoCmd cmd)
        {
            Stack.UndoUntil(cmd);
        }

        public void RedoUntil(UndoRedoCmd cmd)
        {
            Stack.RedoUntil(cmd);
        }

        private void OnUndoTo(object sender, RoutedEventArgs e)
        {
            UndoRedoCmd target = ((MenuItem)sender).Tag as UndoRedoCmd;
            ((UndoStack)ListView.DataContext).UndoUntil(target);
        }

        private void OnRedoTo(object sender, RoutedEventArgs e)
        {
            UndoRedoCmd target = ((MenuItem)sender).Tag as UndoRedoCmd;
            ((UndoStack)ListView.DataContext).RedoUntil(target);
        }

        private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            UndoRedoCmd target = ((Label)sender).Tag as UndoRedoCmd;
            UndoStack stack = ((UndoStack)ListView.DataContext);
            if (stack.Undo.Contains(target))
                stack.UndoUntil(target);
            else if (stack.Redo.Contains(target))
                stack.RedoUntil(target);
        }
    }
}
