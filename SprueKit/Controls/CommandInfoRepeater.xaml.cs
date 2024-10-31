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
    /// Uses a collection of CommandInfo objects in an an items control to provide
    /// an array of command buttons.
    /// 
    /// Used for:
    ///     Viewport selection context commands (ie. 'Add before', 'Add after', etc)
    ///     Document commands (far left side vertical strip of the application0
    ///     
    /// </summary>
    public partial class CommandInfoRepeater : ItemsControl
    {
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            "Orientation", 
            typeof(Orientation), 
            typeof(CommandInfoRepeater),
            new FrameworkPropertyMetadata(Orientation.Vertical));

        public Orientation Orientation {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        public static readonly DependencyProperty TargetProperty = DependencyProperty.Register(
            "Target",
            typeof(object),
            typeof(CommandInfoRepeater),
            new PropertyMetadata(null));

        public object Target { get { return GetValue(TargetProperty); } set { SetValue(TargetProperty, value); } }

        IOCDependency<DocumentManager> DocumentManager = new IOCDependency<SprueKit.DocumentManager>();

        public CommandInfoRepeater()
        {
            InitializeComponent();
        }

        private void itemButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null && btn.Tag != null)
            {
                Commands.CommandInfo cmdInfo = btn.Tag as Commands.CommandInfo;
                if (cmdInfo != null && cmdInfo.Action != null)
                    cmdInfo.Action(DocumentManager.Object.ActiveDocument, Target);
            }
        }
    }
}
