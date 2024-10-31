using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Vector2 = Microsoft.Xna.Framework.Vector2;


namespace SprueKit.Controls
{
    /// <summary>
    /// Interaction logic for ColorCurvesEditor.xaml
    /// </summary>
    public partial class ColorCurvesEditor : UserControl
    {
        bool updateSuspend_ = false;
        public static readonly DependencyProperty CurveProperty =
            DependencyProperty.Register(
                "Curves",
                typeof(Data.ColorCurves),
                typeof(ColorCurvesEditor),
                new PropertyMetadata(new Data.ColorCurves(), OnCurveChanged));

        public Data.ColorCurves Curves
        {
            get { return (Data.ColorCurves)GetValue(CurveProperty); }
            set { SetValue(CurveProperty, value); }
        }

        public EventHandler<Data.ColorCurves> CurveChanged;

        void NotifyChange(Data.ColorCurves curve)
        {
            if (CurveChanged != null)
                CurveChanged(this, curve);
        }

        public ColorCurvesEditor()
        {
            InitializeComponent();
            DataContextChanged += ColorCurvesEditor_DataContextChanged;
            drawingCanvas.Background = new SolidColorBrush(Color.FromRgb(64, 64, 64));
            drawingCanvas.MouseLeftButtonUp += DrawingCanvas_MouseLeftButtonUp;
            drawingCanvas.MouseMove += DrawingCanvas_MouseMove;
            drawingCanvas.LostMouseCapture += DrawingCanvas_LostMouseCapture;
            Loaded += ColorCurvesEditor_Loaded;
        }

        private void ColorCurvesEditor_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateGrid();
        }

        private void ColorCurvesEditor_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //UpdateGrid();
        }

        private void makeLinear_Click(object sender, RoutedEventArgs e)
        {
            GetSelectedCurve(true).MakeLinear();
            if (GetSelectedCurve() == null)
                Curves.SetAll(GetSelectedCurve(true));
            NotifyChange(Curves);
            UpdateGrid();
        }

        private void matchAll_Click(object sender, RoutedEventArgs e)
        {
            Curves.SetAll(GetSelectedCurve());
            NotifyChange(Curves);
            UpdateGrid();
        }

        private void channelCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateGrid();
        }

        private static void OnCurveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (ColorCurvesEditor)d;
            self.UpdateGrid();
        }

        public void UpdateGrid()
        {
            drawingCanvas.Children.Clear();

            float step = (float)(1.0f / drawingCanvas.Width);
            SolidColorBrush lightGrey = new SolidColorBrush(Color.FromRgb(128, 128, 128));
            // Draw vertical gray lines
            float lower = (float)drawingCanvas.Height;
            float upper = (float)0;
            float width = (float)drawingCanvas.Width;
            float height = (float)drawingCanvas.Height;

            // Draw grey gridlines for 0.25,0.5,0.75 values on both axes
            {
                for (int x = 1; x <= 3; ++x) // Vertical
                    drawingCanvas.Children.Add(new Line { X1 = width * 0.25f * x, Y1 = lower, X2 = width * 0.25f * x, Y2 = upper, Stroke = lightGrey });
                for (int y = 1; y <= 3; ++y) // Horizontal
                    drawingCanvas.Children.Add(new Line { X1 = 0.0f, Y1 = height * 0.25f * y, X2 = width, Y2 = height * 0.25f * y, Stroke = lightGrey });
                // draw diagonal slope
                drawingCanvas.Children.Add(new Line { X1 = 0, Y1 = height, X2 = width, Y2 = 0, Stroke = lightGrey });
            }

            // Draw red lines for 0.0 edges
            SolidColorBrush red = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            drawingCanvas.Children.Add(new Line { X1 = 0, Y1 = 0, X2 = 0, Y2 = height, Stroke = red, StrokeThickness = 2 });
            drawingCanvas.Children.Add(new Line { X1 = 0, Y1 = height, X2 = width, Y2 = height, Stroke = red, StrokeThickness = 2 });

            // Draw cyan lines for 1.0 edges
            SolidColorBrush blue = new SolidColorBrush(Color.FromRgb(0, (byte)(0.4787f * 255), (byte)(0.8f * 255)));
            drawingCanvas.Children.Add(new Line { X1 = width, Y1 = 0, X2 = width, Y2 = height, Stroke = blue, StrokeThickness = 2 });
            drawingCanvas.Children.Add(new Line { X1 = 0, Y1 = 0, X2 = width, Y2 = 0, Stroke = blue, StrokeThickness = 2 });

            DrawCurve();
            DrawKnots(GetSelectedCurve(true));
        }

        Data.ColorCurve GetSelectedCurve(bool noNull = false)
        {
            switch (channelCombo.SelectedIndex)
            {
                case 0:
                    return Curves.R;
                case 1:
                    return Curves.G;
                case 2:
                    return Curves.B;
                case 3:
                    return Curves.A;
                case 4:
                    return noNull ? Curves.R : null;
            }
            return Curves.R;
        }

        void DrawCurve(Data.ColorCurve curve, Color lineColor)
        {
            if (curve != null)
            {
                float step = (float)(4.0f / drawingCanvas.ActualWidth);
                // Draw the curve
                float lastX = 0.0f;
                float lastY = safetyCheck(Clamp01(1.0f - curve.GetValue(0.0f)) * (float)drawingCanvas.ActualHeight);

                SolidColorBrush green = new SolidColorBrush(lineColor);
                for (float f = step; f <= 1.0f; f += step)
                {
                    float nextX = Clamp01(f) * (float)drawingCanvas.ActualWidth;
                    float nextY = safetyCheck(Clamp01(1.0f - curve.GetValue(f))) * (float)drawingCanvas.ActualHeight;
                    drawingCanvas.Children.Add(new Line { X1 = lastX, Y1 = lastY, X2 = nextX, Y2 = nextY, Stroke = green, StrokeThickness = 2 });

                    lastX = nextX;
                    lastY = nextY;
                }
            }
        }

        protected void DrawCurve()
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
                    DrawCurve(Curves.R, Colors.DarkRed);
                    DrawCurve(selectedCurve, Colors.LimeGreen);
                }
                else if (selectedCurve == Curves.B)
                {
                    DrawCurve(Curves.A, Colors.DarkMagenta);
                    DrawCurve(Curves.G, Colors.DarkGreen);
                    DrawCurve(Curves.R, Colors.DarkRed);
                    DrawCurve(selectedCurve, Colors.Cyan);
                }
                else if (selectedCurve == Curves.A)
                {
                    DrawCurve(Curves.B, Colors.DarkBlue);
                    DrawCurve(Curves.G, Colors.DarkGreen);
                    DrawCurve(Curves.R, Colors.DarkRed);
                    DrawCurve(selectedCurve, Colors.Magenta);
                }
                else
                {
                    DrawCurve(Curves.R, Colors.White);
                }
            }
        }

        const double KnotSize = 10.0;
        const double HalfKnotSize = 5.0;
        void DrawKnots(Data.ColorCurve curve)
        {
            for (int i = 0; i < curve.Knots.Count; ++i)
            {
                var knot = curve.Knots[i];
                var rect = new Rectangle { Fill = new SolidColorBrush(Colors.White), Width = KnotSize, Height = KnotSize };
                rect.MouseLeftButtonDown += Rect_MouseLeftButtonDown;
                rect.Tag = i;
                Canvas.SetLeft(rect, knot.X * drawingCanvas.ActualWidth - HalfKnotSize);
                Canvas.SetTop(rect, (1.0f - knot.Y) * drawingCanvas.ActualHeight - HalfKnotSize);
                drawingCanvas.Children.Add(rect);
            }
        }

        Point mousePt_;
        int knotIndex_ = -1;
        private void Rect_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            mousePt_ = e.GetPosition(drawingCanvas);
            knotIndex_ = (int)(sender as Rectangle).Tag;
            drawingCanvas.CaptureMouse();
        }

        private void DrawingCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (knotIndex_ != -1)
            {
                var newPos = e.GetPosition(drawingCanvas);
                var delta = newPos - mousePt_;
                if (delta.Length <= 0.01)
                    return;
                mousePt_ = newPos;
                GetSelectedCurve(true).Knots[knotIndex_] = 
                    new Vector2(
                        Mathf.Clamp01(Mathf.Normalize((float)newPos.X, 0, (float)drawingCanvas.ActualWidth)), 
                        Mathf.Clamp01(1.0f - Mathf.Normalize((float)newPos.Y, 0, (float)drawingCanvas.ActualHeight)));
                int currentHashCode = GetSelectedCurve(true).Knots[knotIndex_].GetHashCode();
                GetSelectedCurve(true).SortKnots();
                knotIndex_ = GetSelectedCurve(true).Knots.IndexOf(GetSelectedCurve(true).Knots.FirstOrDefault(v => v.GetHashCode() == currentHashCode));
                if (channelCombo.SelectedIndex == 4)
                    Curves.SetAll(GetSelectedCurve(true));
                UpdateGrid();
                NotifyChange(Curves);
            }
        }

        private void DrawingCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                if (knotIndex_ != -1)
                {
                    if (GetSelectedCurve(true).Knots.Count > 0)
                    {
                        GetSelectedCurve(true).Knots.RemoveAt(knotIndex_);
                        GetSelectedCurve(true).CalculateDerivatives();
                        if (channelCombo.SelectedIndex == 4)
                            Curves.SetAll(GetSelectedCurve(true));
                        UpdateGrid();
                        NotifyChange(Curves);
                    }
                }
                else
                {
                    var newPos = e.GetPosition(drawingCanvas);
                    var knotPos = new Vector2(
                            Mathf.Clamp01(Mathf.Normalize((float)newPos.X, 0, (float)drawingCanvas.ActualWidth)),
                            Mathf.Clamp01(1.0f - Mathf.Normalize((float)newPos.Y, 0, (float)drawingCanvas.ActualHeight)));

                    GetSelectedCurve(true).Knots.Add(knotPos);
                    GetSelectedCurve(true).SortKnots();
                    UpdateGrid();
                    NotifyChange(Curves);
                }
            }
            e.Handled = true;
            knotIndex_ = -1;
            drawingCanvas.ReleaseMouseCapture();
        }

        private void DrawingCanvas_LostMouseCapture(object sender, MouseEventArgs e)
        {
            knotIndex_ = -1;
        }

        float safetyCheck(float input) { return float.IsNaN(input) ? 0.0f : input; }

        static float Clamp01(float input)
        {
            return Math.Max(0.0f, Math.Min(input, 1.0f));
        }
    }
}
