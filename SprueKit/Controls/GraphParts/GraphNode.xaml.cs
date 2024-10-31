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

namespace SprueKit.Controls.GraphParts
{
    /// <summary>
    /// Interaction logic for GraphNode.xaml
    /// </summary>
    public partial class GraphNode : UserControl
    {
        static SolidColorBrush selectedBrush = new SolidColorBrush(Colors.Yellow);
        static SolidColorBrush unselectedBrush = new SolidColorBrush(Colors.CornflowerBlue);

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected",
            typeof(bool), typeof(GraphNode), new PropertyMetadata(false));
        public bool IsSelected { get { return (bool)GetValue(IsSelectedProperty); } set {
                SetValue(IsSelectedProperty, value);
                borderRect.BorderBrush = IsSelected ? selectedBrush : unselectedBrush;
            } }


        GraphControl ownerControl_;
        public GraphControl OwnerControl { get { return ownerControl_; } }
        public Canvas OwnerCanvas { get { return Parent as Canvas; } }
        public Data.Graph.GraphNode BackingData { get; private set; }

        public double X { get { return Canvas.GetLeft(this); } set { Canvas.SetLeft(this, value); BackingData.VisualX = value; } }
        public double Y { get { return Canvas.GetTop(this); } set { Canvas.SetTop(this, value); BackingData.VisualY = value; } }

        bool dragging_ = false;

        public GraphNode(GraphControl owner, Data.Graph.GraphNode data)
        {
            InitializeComponent();
            DataContext = data;
            ownerControl_ = owner;
            BackingData = data;
            X =  BackingData.VisualX;
            Y = BackingData.VisualY;
            CommonConstruct();
        }

        private void Data_NodeChanged(object sender, EventArgs e)
        {
            
        }

        public GraphNode(GraphControl owner, Data.Graph.GraphNode backingData, int x, int y)
        {
            InitializeComponent();
            DataContext = backingData;
            ownerControl_ = owner;
            BackingData = backingData;
            X = x;
            Y = y;
            CommonConstruct();
        }

        void CommonConstruct()
        {
            Canvas.SetZIndex(this, 1);
            BackingData.NodeChanged += BackingData_NodeChanged;
            MouseLeftButtonDown += GraphNode_MouseLeftButtonDown;
            MouseLeftButtonUp += GraphNode_MouseLeftButtonUp;
            MouseMove += GraphNode_MouseMove;
            LostMouseCapture += GraphNode_LostMouseCapture;
        }

        private void GraphNode_LostMouseCapture(object sender, MouseEventArgs e)
        {
            if (undo_ != null)
            {
                undo_.SetText();
                new IOCDependency<DocumentManager>().Object.ActiveDocument.UndoRedo.Add(undo_);
                undo_ = null;
            }
            dragging_ = false;
            DraggedNode = null;
            Cursor = null;
            lastPoint = null;
            startPoint = null;
        }

        public virtual void UpdateConnections()
        {
            //??foreach (GraphSocket socket in inputSockets.Children)
            //??    socket.UpdateConnections();
            //??foreach (GraphSocket socket in outputSockets.Children)
            //??    socket.UpdateConnections();
        }

        public static GraphNode DraggedNode = null;

        private void GraphNode_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var mousePt = e.GetPosition(Parent as Control);

            if (undo_ != null)
            {
                undo_.SetText();
                new IOCDependency<DocumentManager>().Object.ActiveDocument.UndoRedo.Add(undo_);
                undo_ = null;
            }

            if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Control)
                && startPoint.HasValue &&
                startPoint.Value.Distance(mousePt) < 10)
            {
                OwnerControl.ClearSelection();
                OwnerControl.SetSelected(this, true);
                e.Handled = true;
            }
            else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                e.Handled = true;

            if (dragging_ && DraggedNode == this)
            {
                DraggedNode = null;
                dragging_ = false;
                ReleaseMouseCapture();
                Cursor = null;
                lastPoint = null;
                startPoint = null;
                e.Handled = true;
            }
            Cursor = null;
            lastPoint = null;
        }

        public static void FlushDragging()
        {
            if (DraggedNode != null)
                DraggedNode.dragging_ = false;
            DraggedNode = null;
        }

        static Commands.GraphUndo undo_;
        private void GraphNode_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ownerControl_ != null)
                ownerControl_.Focus();

            var mousePt = e.GetPosition(Parent as Control);
            if (!dragging_ && DraggedNode == null)
            {
                dragging_ = true;
                DraggedNode = this;
                CaptureMouse();
                Cursor = Cursors.SizeAll;
                lastPoint = startPoint = mousePt;
                e.Handled = true;
            }
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                OwnerControl.SetSelected(this, !IsSelected);
                if (this == DraggedNode)
                {
                    dragging_ = false;
                    lastPoint = null;
                    DraggedNode = null;
                    ReleaseMouseCapture();
                }
                e.Handled = true;
            }
            else if (!IsSelected)
            {
                OwnerControl.ClearSelection();
                OwnerControl.SetSelected(this, true);
                e.Handled = true;
            }
        }

        Point? lastPoint;
        Point? startPoint;
        private void GraphNode_MouseMove(object sender, MouseEventArgs e)
        {
            if (DraggedNode == this && lastPoint.HasValue)
            {
                Point newPoint = e.GetPosition(this.Parent as Control);
                double deltaX = newPoint.X - lastPoint.Value.X;
                double deltaY = newPoint.Y - lastPoint.Value.Y;

                if (Math.Abs(deltaX) < 200 && Math.Abs(deltaY) < 200)
                {
                    Point preTransPoint = new Point(deltaX, deltaY);
                    var transMat = OwnerCanvas.GetScalingMatrix();
                    transMat.Invert();
                    var postTransPoint = transMat.Transform(preTransPoint);

                    if (undo_ == null)
                    {
                        undo_ = new Commands.GraphUndo(OwnerControl) { X = 0, Y = 0 };
                        foreach (var sel in OwnerControl.SelectedNodes)
                            undo_.Nodes.Add(sel);
                    }

                    foreach (var selNode in OwnerControl.SelectedNodes)
                    {
                        selNode.X = selNode.X + postTransPoint.X;// + deltaX);
                        selNode.Y = selNode.Y + postTransPoint.Y;//+ deltaY);
                        undo_.X += postTransPoint.X;
                        undo_.Y += postTransPoint.Y;
                    }
                }
                lastPoint = newPoint;
                e.Handled = true;
                OwnerControl.UpdatePositions();
            }
        }

        public void Delete_Node(object sender, RoutedEventArgs e)
        {
            using (var macro = new Commands.MacroCommandBlock())
            {
                Canvas owner = OwnerCanvas;
                OwnerControl.BackingGraph.RemoveNode(BackingData);
                //OwnerCanvas.Children.Remove(this);
                OwnerControl.RebuildConnectors();

                // remove it if we must
                new IOCDependency<DocumentManager>().Object.ActiveDocument.Selection.Selected.Remove(BackingData);
            }
        }

        private void BackingData_NodeChanged(object sender, EventArgs e)
        {

        }

        public GraphParts.GraphSocket FindSocket(Data.Graph.GraphSocket socket)
        {
            foreach (Data.Graph.GraphSocket dataSocket in inputSocketsList.Items)
            {
                if (dataSocket == socket)
                    return inputSocketsList.FindItemsControlObject<GraphSocket>(dataSocket);
            }
            foreach (Data.Graph.GraphSocket dataSocket in outputSocketsList.Items)
            {
                if (dataSocket == socket)
                    return outputSocketsList.FindItemsControlObject<GraphSocket>(dataSocket);
            }
            return null;
        }

        private void Copy_Nodes(object sender, RoutedEventArgs e)
        {
            OwnerControl.CopySelected();
        }

        private void Regenerate_Node(object sender, RoutedEventArgs e)
        {
            OwnerControl.SignalRegenerate(this);
        }

        private void CommnentBox_Click(object sender, RoutedEventArgs e)
        {
            OwnerControl.SurroundSelectedWithCommentBox();
        }
    }
}
