using System;
using System.Windows.Controls;

namespace SprueKit.Controls.GraphParts
{
    /// <summary>
    /// Interaction logic for GraphBoxControl.xaml
    /// </summary>
    public partial class GraphBoxControl : UserControl
    {
        public Canvas canvas;
        public Data.Graph.Graph graph;

        public GraphBoxControl()
        {
            InitializeComponent();
            Canvas.SetZIndex(this, -20);
            DataContextChanged += GraphBoxControl_DataContextChanged;
        }

        private void GraphBoxControl_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            UpdateBox();
        }

        private void leftThumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            var box = DataContext as Data.Graph.GraphBox;
            box.VisualX += e.HorizontalChange;
            box.VisualWidth -= e.HorizontalChange;
            UpdateBox();
            e.Handled = true;
        }

        private void rightThumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            var box = DataContext as Data.Graph.GraphBox;
            box.VisualWidth += e.HorizontalChange;
            UpdateBox();
            e.Handled = true;
        }

        private void topThumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            var box = DataContext as Data.Graph.GraphBox;
            box.VisualY += e.VerticalChange;
            box.VisualHeight -= e.VerticalChange;
            UpdateBox();
            e.Handled = true;
        }

        private void bottomThumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            var box = DataContext as Data.Graph.GraphBox;
            box.VisualHeight += e.VerticalChange;
            UpdateBox();
            e.Handled = true;
        }

        private void moveThumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            var box = DataContext as Data.Graph.GraphBox;
            box.VisualX += e.HorizontalChange;
            box.VisualY += e.VerticalChange;
            UpdateBox();
        }

        void UpdateBox()
        {
            var box = DataContext as Data.Graph.GraphBox;
            Canvas.SetLeft(this, box.VisualX);
            Canvas.SetTop(this, box.VisualY);
            Canvas.SetRight(this, box.VisualX + box.VisualWidth);
            Canvas.SetBottom(this, box.VisualY + box.VisualHeight);
        }

        private void moveThumb_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            new IOCDependency<DocumentManager>().Object.ActiveDocument.Selection.SetSelected(DataContext);
            e.Handled = true;
        }

        private void deleteCmd_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var box = DataContext as Data.Graph.GraphBox;
            graph.Boxes.Remove(box);
        }
    }
}
