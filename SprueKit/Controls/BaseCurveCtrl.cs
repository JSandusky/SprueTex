using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SprueKit.Controls
{
    public abstract class BaseCurveCtrl : Canvas
    {
        public BaseCurveCtrl()
        {
            Background = new SolidColorBrush(Colors.Transparent);
        }

        public void UpdateGrid()
        {
            Children.Clear();
            DrawBackground();
            DrawCurve();
        }

        protected void DrawBackground()
        {
            double strokeWidth = 2;
            float step = (float)(1.0f / ActualWidth);
            SolidColorBrush lightGrey = new SolidColorBrush(Color.FromRgb(128, 128, 128));
            // Draw vertical gray lines
            float lower = (float)ActualHeight;
            float upper = (float)0;
            float width = (float)ActualWidth;
            float height = (float)ActualHeight;

            // Draw grey gridlines for 0.25,0.5,0.75 values on both axes
            {
                for (int x = 1; x <= 3; ++x) // Vertical
                    Children.Add(new Line { X1 = width * 0.25f * x, Y1 = lower, X2 = width * 0.25f * x, Y2 = upper, Stroke = lightGrey, IsHitTestVisible = false });
                for (int y = 1; y <= 3; ++y) // Horizontal
                    Children.Add(new Line { X1 = 0.0f, Y1 = height * 0.25f * y, X2 = width, Y2 = height * 0.25f * y, Stroke = lightGrey, IsHitTestVisible = false });
                // draw diagonal slope
                Children.Add(new Line { X1 = 0, Y1 = height, X2 = width, Y2 = 0, Stroke = lightGrey, IsHitTestVisible=false });
            }

            // Draw red lines for 0.0 edges
            SolidColorBrush red = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            Children.Add(new Line { X1 = 0, Y1 = 0, X2 = 0, Y2 = height, Stroke = red, StrokeThickness = strokeWidth, IsHitTestVisible = false });
            Children.Add(new Line { X1 = 0, Y1 = height, X2 = width, Y2 = height, Stroke = red, StrokeThickness = strokeWidth, IsHitTestVisible = false });

            // Draw cyan lines for 1.0 edges
            SolidColorBrush blue = new SolidColorBrush(Color.FromRgb(0, (byte)(0.4787f * 255), (byte)(0.8f * 255)));
            Children.Add(new Line { X1 = width, Y1 = 0, X2 = width, Y2 = height, Stroke = blue, StrokeThickness = strokeWidth, IsHitTestVisible = false });
            Children.Add(new Line { X1 = 0, Y1 = 0, X2 = width, Y2 = 0, Stroke = blue, StrokeThickness = strokeWidth, IsHitTestVisible = false });
        }

        protected abstract void DrawCurve();
    }
}
