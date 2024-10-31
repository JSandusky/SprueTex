using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using SprueKit.Data;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace SprueKit.Controls
{
    public class ResponseCurveCtrl : BaseCurveCtrl
    {
        public static readonly DependencyProperty CurveProperty =
            DependencyProperty.Register(
                "Curve",
                typeof(ResponseCurve),
                typeof(ResponseCurveCtrl),
                new PropertyMetadata(new ResponseCurve(), OnCurveChanged));

        public ResponseCurve Curve
        {
            get { return (ResponseCurve)GetValue(CurveProperty); }
            set { SetValue(CurveProperty, value); }
        }

        private static void OnCurveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ResponseCurveCtrl self = d as ResponseCurveCtrl;
            self.UpdateGrid();
        }

        public ResponseCurveCtrl()
        {
            ToolTipService.SetInitialShowDelay(this, 0);
            ToolTipService.SetBetweenShowDelay(this, 0);
            ToolTipService.SetShowDuration(this, 10000000);
            SizeChanged += ResponseCurveCtrl_SizeChanged;
        }

        private void ResponseCurveCtrl_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            UpdateGrid();            
        }

        protected override void DrawCurve()
        {
            float step = (float)(16.0f / ActualWidth);

            if (Curve != null)
            {
                // Draw the curve
                float lastX = 0.0f;
                float lastY = safetyCheck(Clamp01(1.0f - Curve.GetValue(0.0f)) * (float)ActualHeight);

                SolidColorBrush green = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                for (float f = step; f <= 1.0f; )
                {
                    float nextX = Clamp01(f) * (float)ActualWidth;
                    float nextY = safetyCheck(Clamp01(1.0f - Curve.GetValue(f))) * (float)ActualHeight;
                    Children.Add(new Line { X1 = lastX, Y1 = lastY, X2 = nextX, Y2 = nextY, Stroke = green, StrokeThickness = 2, IsHitTestVisible = false });

                    lastX = nextX;
                    lastY = nextY;

                    if (f == 1.0f)
                        break;
                    else if (f + step > 1.0f)
                        f = 1.0f;
                    else
                        f += step;
                }
            }
        }

        float safetyCheck(float input) { return float.IsNaN(input) ? 0.0f : input; }

        static float Clamp01(float input)
        {
            return Math.Max(0.0f, Math.Min(input, 1.0f));
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            float xPos = (float)(e.GetPosition(this).X / ActualWidth);
            float y = Curve.GetValue(xPos);
            ToolTip = string.Format("X: {0:0.000} Y: {1:0.000}", xPos, y);
        }
    }
}
