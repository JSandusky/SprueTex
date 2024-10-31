using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace SprueKit.Controls
{
    public class Marquee
    {
        public Point StartPoint { get; set; }
        public Point EndPoint { get; set; }

        public Rect GetRect()
        {
            Point endPoint = EndPoint != null ? EndPoint : StartPoint;

            double minX = Math.Min(StartPoint.X, endPoint.X);
            double minY = Math.Min(StartPoint.Y, endPoint.Y);
            double maxX = Math.Max(StartPoint.X, endPoint.X);
            double maxY = Math.Max(StartPoint.Y, endPoint.Y);

            return new Rect
            {
                X = minX, Y = minY,
                Width = maxX - minX,
                Height = maxY - minY
            };
        }

        static SolidColorBrush rectBrush = new SolidColorBrush(new Color {
            R = Colors.Cyan.R,
            G = Colors.Cyan.G,
            B = Colors.Cyan.B,
            A = 150
        });

        static Pen rectPen = new Pen(new SolidColorBrush(new Color {
            R = Colors.Cyan.R,
            G = Colors.Cyan.G,
            B = Colors.Cyan.B,
            A = Colors.Cyan.A
        }), 2);

        public void Draw(DrawingContext drawingContext)
        {
            drawingContext.DrawRectangle(rectBrush, rectPen, GetRect());
        }
    }
}
