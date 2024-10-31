using FirstFloor.ModernUI.Presentation;
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

using GColor = Microsoft.Xna.Framework.Color;

namespace SprueKit.Controls
{
    /// <summary>
    /// Interaction logic for GradientRampEditor.xaml
    /// </summary>
    public partial class GradientRampEditor : UserControl
    {
        List<Rectangle> stops = new List<Rectangle>();
        public static readonly DependencyProperty RampProperty = DependencyProperty.Register("Ramp", typeof(Data.ColorRamp), typeof(GradientRampEditor));
        public Data.ColorRamp Ramp { get { return (Data.ColorRamp)GetValue(RampProperty); } set { SetValue(RampProperty, value); } }

        public EventHandler<Data.ColorRamp> RampChanged;
        void NotifyChange()
        {
            if (RampChanged != null)
                RampChanged(this, Ramp);
        }

        public GradientRampEditor()
        {
            InitializeComponent();

            Loaded += GradientRampEditor_Loaded;
            MouseLeftButtonUp += GradientRampEditor_MouseLeftButtonUp;
            MouseMove += GradientRampEditor_MouseMove;
            LostMouseCapture += GradientRampEditor_LostMouseCapture;
        }

        private void GradientRampEditor_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateAll();
        }

        void UpdateAll()
        {
            UpdateGradientCanvas();
            UpdateStops();
        }

        void UpdateGradientCanvas()
        {
            if (Ramp != null)
            {
                LinearGradientBrush brush = new LinearGradientBrush();
                for (int i = 0; i < Ramp.Colors.Count; ++i)
                {
                    brush.GradientStops.Add(new GradientStop
                    {
                        Offset = Ramp.Colors[i].Key,
                        Color = Ramp.Colors[i].Value.ToMediaColor()
                    });
                }
                gradientCanvas.Background = brush;
            }
        }

        const double StopHeight = 20;
        const double StopWidth = 10;
        void UpdateStops()
        {
            gradientCanvas.Children.Clear();
            stops.Clear();
            if (Ramp != null)
            {
                for (int i = 0; i < Ramp.Colors.Count; ++i)
                {
                    Rectangle r = new Rectangle { Width = StopWidth, Height = StopHeight };
                    var col = Ramp.Colors[i].Value.ToMediaColor();
                    r.Fill = new SolidColorBrush(col);
                    r.StrokeThickness = 2;
                    r.Stroke = new SolidColorBrush(Color.FromArgb(255, (byte)(255 - col.R), (byte)(255 - col.G), (byte)(255 - col.B)));
                    r.Tag = i;
                    float x = Ramp.Colors[i].Key;
                    Canvas.SetLeft(r, x * gradientCanvas.ActualWidth - StopWidth / 2);
                    Canvas.SetTop(r, gradientCanvas.ActualHeight - StopHeight * 0.75);
                    gradientCanvas.Children.Add(r);
                    stops.Add(r);
                    r.MouseLeftButtonDown += R_MouseLeftButtonDown;

                    int colorIndex = i;
                    r.InputBindings.Add(new MouseBinding()
                    {
                        MouseAction = MouseAction.LeftDoubleClick,
                        CommandParameter = r,
                        Command = new RelayCommand((o) =>
                        {
                            PopupHelper popup = PopupHelper.Create();
                            var canvas = new ColorPickerLib.Controls.ColorCanvas();
                            canvas.Background = new SolidColorBrush(Colors.Transparent);
                            canvas.MinHeight = 320;
                            canvas.MinWidth = 320;
                            canvas.Width = 320;
                            canvas.SelectedColor = Ramp.Colors[colorIndex].Value.ToMediaColor();
                            //canvas.SetBinding(ColorPickerLib.Controls.ColorCanvas.SelectedColorProperty, new Binding(pi.Name) { Converter = new Data.XNAColorConverter() });
                            canvas.SelectedColorChanged += (c, cc) =>
                            {
                                var val = cc.NewValue.Value;
                                Ramp.Colors[colorIndex] = new KeyValuePair<float, GColor>(Ramp.Colors[colorIndex].Key, new GColor(val.R, val.G, val.B, val.A));
                                UpdateAll();
                                NotifyChange();
                            };
                            canvas.DataContext = DataContext;
                            popup.Grid.Children.Add(canvas);
                            popup.ShowAtMouse();
                        })
                    });
                }
            }
        }

        int dragIndex_ = -1;
        private void R_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (dragIndex_ == -1)
            {
                CaptureMouse();
                dragIndex_ = stops.IndexOf((Rectangle)sender);
            }
        }

        private void GradientRampEditor_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                if (dragIndex_ == -1 && Ramp != null)
                {
                    var pos = e.GetPosition(gradientCanvas);
                    float xPos = Mathf.Clamp01(Mathf.Normalize((float)pos.X, 0.0f, (float)gradientCanvas.ActualWidth));
                    var colValue = Ramp.Get(xPos);
                    Ramp.Colors.Add(new KeyValuePair<float, Microsoft.Xna.Framework.Color>(xPos, colValue));
                }
                else if (Ramp.Colors.Count > 0)
                {
                    Ramp.Colors.RemoveAt(dragIndex_);
                }
                NotifyChange();
                UpdateGradientCanvas();
            }
            if (Ramp != null)
            {
                Ramp.Sort();
                UpdateStops();
            }
            ReleaseMouseCapture();
            dragIndex_ = -1;
        }

        private void GradientRampEditor_LostMouseCapture(object sender, MouseEventArgs e)
        {
            dragIndex_ = -1;
        }

        private void GradientRampEditor_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragIndex_ != -1 && Ramp != null)
            {
                var pos = e.GetPosition(gradientCanvas);
                float xPos = Mathf.Clamp01(Mathf.Normalize((float)pos.X, 0.0f, (float)gradientCanvas.ActualWidth));

                Ramp.Colors[dragIndex_] = new KeyValuePair<float,GColor>(xPos, Ramp.Colors[dragIndex_].Value);
                int currentHashCode = Ramp.Colors[dragIndex_].GetHashCode();
                Ramp.Sort();
                dragIndex_ = Ramp.Colors.IndexOf(Ramp.Colors.FirstOrDefault(r => r.GetHashCode() == currentHashCode));

                // Update our stop position
                Canvas.SetLeft(stops[dragIndex_], xPos * gradientCanvas.ActualWidth - StopWidth / 2);

                // update the canvas color
                UpdateGradientCanvas();
                NotifyChange();
            }
        }
    }
}
