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
    /// Used for drawing connected shapes.
    /// These shapes may be unbounded
    /// </summary>
    public partial class ShapeCanvas : UserControl
    {
        /// <summary>
        /// If true then the shape is expected to be a closed polygon: for extrusion, surface of revolution, or swept path.
        /// If false then the shape is assumed to be a polyline, such as for a swept shape
        /// </summary>
        public static DependencyProperty ClosedShapeModeProperty = DependencyProperty.Register("ClosedShapeMode", 
            typeof(bool), 
            typeof(ShapeCanvas));

        /// <summary>
        /// Range will be bound to the +X domain for creating a surface of revolution, segments lying on the X:0 are discarded during revolution
        /// </summary>
        public static DependencyProperty RevoluteModeProperty = DependencyProperty.Register("RevoluteMode",
            typeof(bool),
            typeof(ShapeCanvas));

        public bool ClosedShapeMode { get { return (bool)GetValue(ClosedShapeModeProperty); } set { SetValue(ClosedShapeModeProperty, value); } }

        public bool RevoluteMode { get { return (bool)GetValue(RevoluteModeProperty); } set { SetValue(RevoluteModeProperty, value); } }

        public ShapeCanvas()
        {
            InitializeComponent();
        }
    }
}
