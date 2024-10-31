using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SprueKit.Graphics.Controls
{
    /// <summary>
    /// Interaction logic for GizmoControlBox.xaml
    /// </summary>
    public partial class GizmoControlBox : StackPanel
    {
        public static readonly DependencyProperty ModeProperty = DependencyProperty.Register("Mode",
            typeof(GizmoMode),
            typeof(GizmoControlBox),
            new PropertyMetadata(GizmoMode.Translation));

        public GizmoMode Mode {
            get { return (GizmoMode)GetValue(ModeProperty); }
            set { SetValue(ModeProperty, value); }
        }

        public static readonly DependencyProperty WorldModeProperty = DependencyProperty.Register("WorldMode",
            typeof(bool),
            typeof(GizmoControlBox),
            new PropertyMetadata(false));

        public bool WorldMode
        {
            get { return (bool)GetValue(WorldModeProperty); }
            set { SetValue(WorldModeProperty, value); }
        }

        public GizmoControlBox()
        {
            InitializeComponent();
            var btnSel = new ToggleButton { Content = new Image { Source = WPFExt.GetEmbeddedImage("Images/godot/icon_tool_select.png"), Width = 16, Height = 16 }, ToolTip = "Select" };
            var btnMov = new ToggleButton { Content = new Image { Source = WPFExt.GetEmbeddedImage("Images/godot/icon_tool_move.png"), Width = 16, Height = 16 }, ToolTip = "Move" };
            btnMov.IsChecked = true;
            var btnRot = new ToggleButton { Content = new Image { Source = WPFExt.GetEmbeddedImage("Images/godot/icon_tool_rotate.png"), Width = 16, Height = 16 }, ToolTip = "Rotate" };
            var btnScl = new ToggleButton { Content = new Image { Source = WPFExt.GetEmbeddedImage("Images/godot/icon_tool_scale.png"), Width = 16, Height = 16 }, ToolTip = "Scale" };
            var btnWorldMode = new ToggleButton { Content = new Image { Source = WPFExt.GetEmbeddedImage("Images/icon_world_white.png"), Width = 16, Height = 16 }, ToolTip = "World space mode" }; ;
            var btnSnap = new Button { Content = new Image { Source = WPFExt.GetEmbeddedImage("Images/godot/icon_snap.png"), Width = 16, Height = 16 }, ToolTip = "Snap Settings" };

            Children.Add(btnSel);
            Children.Add(btnMov);
            Children.Add(btnRot);
            Children.Add(btnScl);
            Children.Add(btnWorldMode);
            Children.Add(btnSnap);

            SprueKit.Util.ToggleButtonGroup.Setup(btnSel, btnMov, btnRot, btnScl);

            btnSel.Checked += (o, evt) => {
                Mode = GizmoMode.None;
            };
            btnMov.Checked += (o, evt) => {
                Mode = GizmoMode.Translation;
            };
            btnRot.Checked += (o, evt) => {
                Mode = GizmoMode.Rotation;
            };
            btnScl.Checked += (o, evt) => {
                Mode = GizmoMode.Scale;
            };

            btnSnap.Click += (o, evt) =>
            {
                var popup = PopupHelper.Create();
                var mainGrid = new Grid { Margin = new System.Windows.Thickness(5) };
                mainGrid.ColumnDefinitions.Add(new ColumnDefinition());
                mainGrid.ColumnDefinitions.Add(new ColumnDefinition());
                mainGrid.RowDefinitions.Add(new RowDefinition()); // main label 0 
                mainGrid.RowDefinitions.Add(new RowDefinition()); // pos check 1
                mainGrid.RowDefinitions.Add(new RowDefinition()); // pos values 2
                mainGrid.RowDefinitions.Add(new RowDefinition()); // rot check 3
                mainGrid.RowDefinitions.Add(new RowDefinition()); // rot vlaues 4

                popup.Grid.MinWidth = 150;
                popup.Grid.Children.Add(mainGrid);

                Label snappingLabel = new Label { Content = "Snapping", HorizontalAlignment = HorizontalAlignment.Center };
                Grid.SetRow(snappingLabel, 0);
                Grid.SetColumnSpan(snappingLabel, 2);
                mainGrid.Children.Add(snappingLabel);

                CheckBox snapPos = new CheckBox() { Content = "Position" };
                Grid.SetRow(snapPos, 1);
                Grid.SetColumnSpan(snapPos, 2);
                TextBox posAmt = new TextBox() { VerticalContentAlignment = VerticalAlignment.Center };

                var viewportSettings = new IOCDependency<Settings.ViewportSettings>().Object;
                snapPos.SetBinding(CheckBox.IsCheckedProperty, new Binding("PositionSnapActive") { Source = viewportSettings });
                posAmt.SetBinding(TextBox.TextProperty, new Binding("PositionSnap") { Source = viewportSettings });

                Label unitsLabel = new Label { Content = "Units", VerticalContentAlignment = VerticalAlignment.Center };
                Grid.SetRow(unitsLabel, 2);
                Grid.SetRow(posAmt, 2);
                Grid.SetColumn(posAmt, 1);

                mainGrid.Children.Add(snapPos);
                mainGrid.Children.Add(unitsLabel);
                mainGrid.Children.Add(posAmt);

                Label degreesLabel = new Label { Content = "Degrees", VerticalContentAlignment = VerticalAlignment.Center };
                CheckBox snapRot = new CheckBox() { Content = "Rotation" };
                TextBox rotAmt = new TextBox() { VerticalContentAlignment = VerticalAlignment.Center };
                snapRot.SetBinding(CheckBox.IsCheckedProperty, new Binding("RotationSnapActive") { Source = viewportSettings });
                rotAmt.SetBinding(TextBox.TextProperty, new Binding("RotationSnap") { Source = viewportSettings });

                Grid.SetRow(snapRot, 3);
                Grid.SetColumnSpan(snapRot, 2);
                Grid.SetRow(degreesLabel, 4);
                Grid.SetRow(rotAmt, 4);
                Grid.SetColumn(rotAmt, 1);

                mainGrid.Children.Add(snapRot);
                mainGrid.Children.Add(degreesLabel);
                mainGrid.Children.Add(rotAmt);

                popup.ShowAtMouse();
            };

            SetBinding(WorldModeProperty, new Binding("IsChecked") { Source = btnWorldMode });
        }
    }
}
