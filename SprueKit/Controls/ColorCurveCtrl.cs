using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SprueKit.Controls
{
    public class ColorCurveCtrl : BaseCurveCtrl
    {
        public static readonly DependencyProperty CurveProperty =
            DependencyProperty.Register(
                "Curves",
                typeof(Data.ColorCurves),
                typeof(ResponseCurveCtrl),
                new PropertyMetadata(new Data.ColorCurves(), OnCurveChanged));

        public Data.ColorCurves Curves
        {
            get { return (Data.ColorCurves)GetValue(CurveProperty); }
            set { SetValue(CurveProperty, value); }
        }

        public ColorCurveCtrl()
        {
            SizeChanged += OnSizeChanged;
            Background = new SolidColorBrush(Colors.Transparent);
        }

        Data.ColorCurve GetSelectedCurve()
        {
            return Curves.R;
        }

        void DrawCurve(Data.ColorCurve curve, Color lineColor)
        {
            if (curve != null)
            {
                float step = (float)(1.0f / ActualWidth);
                // Draw the curve
                float lastX = 0.0f;
                float lastY = safetyCheck(Clamp01(1.0f - curve.GetValue(0.0f)) * (float)ActualHeight);

                SolidColorBrush green = new SolidColorBrush(lineColor);
                for (float f = step; f <= 1.0f; f += step)
                {
                    float nextX = Clamp01(f) * (float)ActualWidth;
                    float nextY = safetyCheck(Clamp01(1.0f - curve.GetValue(f))) * (float)ActualHeight;
                    Children.Add(new Line { X1 = lastX, Y1 = lastY, X2 = nextX, Y2 = nextY, Stroke = green, StrokeThickness = 2 });

                    lastX = nextX;
                    lastY = nextY;
                }
            }
        }

        protected override void DrawCurve()
        {
            if (Curves != null)
            {
                var selectedCurve = GetSelectedCurve();
                if (selectedCurve == Curves.R)
                {
                    DrawCurve(Curves.A, Colors.DarkMagenta);
                    DrawCurve(Curves.B, Colors.DarkBlue);
                    DrawCurve(Curves.G, Colors.DarkGreen);
                    DrawCurve(selectedCurve, Colors.Red);
                }
                else if (selectedCurve == Curves.G)
                {
                    DrawCurve(Curves.A, Colors.DarkMagenta);
                    DrawCurve(Curves.B, Colors.DarkBlue);
                    DrawCurve(selectedCurve, Colors.DarkRed);
                    DrawCurve(Curves.G, Colors.LimeGreen);
                }
                else if (selectedCurve == Curves.B)
                {
                    DrawCurve(Curves.A, Colors.DarkMagenta);
                    
                    DrawCurve(Curves.G, Colors.DarkGreen);
                    DrawCurve(selectedCurve, Colors.DarkRed);
                    DrawCurve(Curves.B, Colors.Cyan);
                }
                else if (selectedCurve == Curves.A)
                {
                    DrawCurve(Curves.B, Colors.DarkBlue);
                    DrawCurve(Curves.G, Colors.DarkGreen);
                    DrawCurve(selectedCurve, Colors.DarkRed);
                    DrawCurve(Curves.A, Colors.Magenta);
                }
                else
                {
                    DrawCurve(Curves.R, Colors.White);
                }
            }
        }

        private static void OnCurveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ResponseCurveCtrl self = d as ResponseCurveCtrl;
            self.UpdateGrid();
        }

        private void OnSizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            UpdateGrid();
        }

        float safetyCheck(float input) { return float.IsNaN(input) ? 0.0f : input; }

        static float Clamp01(float input)
        {
            return Math.Max(0.0f, Math.Min(input, 1.0f));
        }
    }
}
