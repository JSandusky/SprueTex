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

using SprueKit;
using SprueKit.Data;

namespace SprueKit.Controls
{
    /// <summary>
    /// Interaction logic for ResponseCurveEditor.xaml
    /// </summary>
    public partial class ResponseCurveEditor : UserControl
    {
        public static readonly DependencyProperty CurveProperty =
            DependencyProperty.Register(
                "Curve",
                typeof(ResponseCurve),
                typeof(ResponseCurveEditor),
                new PropertyMetadata(new ResponseCurve(), OnCurveChanged));

        public ResponseCurve Curve
        {
            get { return (ResponseCurve)GetValue(CurveProperty); }
            set { SetValue(CurveProperty, value); }
        }

        private static void OnCurveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (ResponseCurveEditor)d;

            self.updateSuspend = true;
            self.pickType.SelectedIndex = (int)self.Curve.CurveShape;
            self.txtX.Text = self.Curve.XIntercept.ToString();
            self.txtY.Text = self.Curve.YIntercept.ToString();
            self.txtSlope.Text = self.Curve.SlopeIntercept.ToString();
            self.txtExponent.Text = self.Curve.Exponent.ToString();
            self.chkFlipX.IsChecked = self.Curve.FlipX;
            self.chkFlipY.IsChecked = self.Curve.FlipY;
            self.updateSuspend = false;
            self.updateGrid();
        }

        public ResponseCurveEditor()
        {
            InitializeComponent();
            drawingCanvas.Background = new SolidColorBrush(Color.FromRgb(64, 64, 64));
            drawingCanvas.MouseMove += DrawingCanvas_MouseMove;
            Curve = new ResponseCurve();

            ToolTipService.SetInitialShowDelay(drawingCanvas, 0);
            ToolTipService.SetBetweenShowDelay(drawingCanvas, 0);
            ToolTipService.SetShowDuration(drawingCanvas, 1000000);

            ResponseCurveEditor_DataContextChanged(null, new DependencyPropertyChangedEventArgs());

            DataContextChanged += ResponseCurveEditor_DataContextChanged;

            pickType.SelectionChanged += typeChanged;
            txtX.TextChanged += txtX_TextChanged;
            txtY.TextChanged += txtY_TextChanged;
            txtExponent.TextChanged += exp_TextChanged;
            txtSlope.TextChanged += slope_TextChanged;

            chkFlipX.Checked += flipX_Checked;
            chkFlipX.Unchecked += flipX_Checked;
            chkFlipX.ToolTip = "Flips curve horizontally".Localize();
            chkFlipY.Checked += flipY_Checked;
            chkFlipY.Unchecked += flipY_Checked;
            chkFlipY.ToolTip = "Flips curve vertically".Localize();

            pickType.ToolTip = txtX.ToolTip = txtY.ToolTip = txtSlope.ToolTip = txtExponent.ToolTip = "Placeholder";
            pickType.ToolTipOpening += prepareToolTip;
            txtX.ToolTipOpening += prepareToolTip;
            txtY.ToolTipOpening += prepareToolTip;
            txtSlope.ToolTipOpening += prepareToolTip;
            txtExponent.ToolTipOpening += prepareToolTip;


            txtX.KeyUp += incTextBox;
            txtY.KeyUp += incTextBox;
            txtExponent.KeyUp += incTextBox;
            txtSlope.KeyUp += incTextBox;
        }

        private void DrawingCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            float xPos = (float)(e.GetPosition(drawingCanvas).X / ActualWidth);
            float y = Curve.GetValue(xPos);
            drawingCanvas.ToolTip = string.Format("X: {0:0.000} Y: {1:0.000}", xPos, y);
        }

        void incTextBox(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
                IncrementTextBox(sender as TextBox, 0.2f);
            else if (e.Key == Key.Down)
                IncrementTextBox(sender as TextBox, -0.2f);
            else if (e.Key == Key.PageUp)
                IncrementTextBox(sender as TextBox, 1.0f);
            else if (e.Key == Key.PageDown)
                IncrementTextBox(sender as TextBox, -1.0f);
        }


        bool updateSuspend = false;
        void ResponseCurveEditor_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            updateSuspend = true;
            pickType.SelectedIndex = (int)Curve.CurveShape;
            txtX.Text = Curve.XIntercept.ToString();
            txtY.Text = Curve.YIntercept.ToString();
            txtSlope.Text = Curve.SlopeIntercept.ToString();
            txtExponent.Text = Curve.Exponent.ToString();
            chkFlipX.IsChecked = Curve.FlipX;
            chkFlipY.IsChecked = Curve.FlipY;
            updateSuspend = false;
            updateGrid();
        }

        private void updateCurveEvent(object sender, EventArgs args)
        {
            if (!updateSuspend)
                updateGrid();
        }

        private void updateGrid()
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
                    drawingCanvas.Children.Add(new Line { X1 = width * 0.25f * x, Y1 = lower, X2 = width * 0.25f * x, Y2 = upper, Stroke = lightGrey, IsHitTestVisible = false });
                for (int y = 1; y <= 3; ++y) // Horizontal
                    drawingCanvas.Children.Add(new Line { X1 = 0.0f, Y1 = height * 0.25f * y, X2 = width, Y2 = height * 0.25f * y, Stroke = lightGrey, IsHitTestVisible = false });
                // draw diagonal slope
                drawingCanvas.Children.Add(new Line { X1 = 0, Y1 = height, X2 = width, Y2 = 0, Stroke = lightGrey, IsHitTestVisible = false });
            }

            // Draw red lines for 0.0 edges
            SolidColorBrush red = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            drawingCanvas.Children.Add(new Line { X1 = 0, Y1 = 0, X2 = 0, Y2 = height, Stroke = red, StrokeThickness = 2, IsHitTestVisible = false });
            drawingCanvas.Children.Add(new Line { X1 = 0, Y1 = height, X2 = width, Y2 = height, Stroke = red, StrokeThickness = 2, IsHitTestVisible = false });

            // Draw cyan lines for 1.0 edges
            SolidColorBrush blue = new SolidColorBrush(Color.FromRgb(0, (byte)(0.4787f * 255), (byte)(0.8f * 255)));
            drawingCanvas.Children.Add(new Line { X1 = width, Y1 = 0, X2 = width, Y2 = height, Stroke = blue, StrokeThickness = 2, IsHitTestVisible = false });
            drawingCanvas.Children.Add(new Line { X1 = 0, Y1 = 0, X2 = width, Y2 = 0, Stroke = blue, StrokeThickness = 2, IsHitTestVisible = false });

            if (Curve != null)
            {
                // Draw the curve
                float lastX = 0.0f;
                float lastY = safetyCheck(Clamp01(1.0f - Curve.GetValue(0.0f)) * height);

                SolidColorBrush green = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                for (float f = step; f <= 1.0f; f += step)
                {
                    float nextX = Clamp01(f) * width;
                    float nextY = safetyCheck(Clamp01(1.0f - Curve.GetValue(f))) * height;
                    drawingCanvas.Children.Add(new Line { X1 = lastX, Y1 = lastY, X2 = nextX, Y2 = nextY, Stroke = green, StrokeThickness = 2, IsHitTestVisible = false });

                    lastX = nextX;
                    lastY = nextY;
                }
            }
        }

        float safetyCheck(float input) { return float.IsNaN(input) ? 0.0f : input; }

        static float Clamp01(float input)
        {
            return Math.Max(0.0f, Math.Min(input, 1.0f));
        }

        // Change handlers
        void txtX_TextChanged(object sender, TextChangedEventArgs e)
        {
            float junk = 0.0f;
            if (float.TryParse(txtX.Text, out junk))
            {
                Curve.XIntercept = junk;
                updateGrid();
                //GetBindingExpression(CurveProperty).UpdateTarget();
                GetBindingExpression(CurveProperty).UpdateSource();
            }
        }
        void txtY_TextChanged(object sender, TextChangedEventArgs e)
        {
            float junk = 0.0f;
            if (float.TryParse(txtY.Text, out junk))
            {
                Curve.YIntercept = junk;
                updateGrid();
                //GetBindingExpression(CurveProperty).UpdateTarget();
                GetBindingExpression(CurveProperty).UpdateSource();
            }
        }
        void slope_TextChanged(object sender, TextChangedEventArgs e)
        {
            float junk = 0.0f;
            if (float.TryParse(txtSlope.Text, out junk))
            {
                Curve.SlopeIntercept = junk;
                updateGrid();
                //GetBindingExpression(CurveProperty).UpdateTarget();
                GetBindingExpression(CurveProperty).UpdateSource();
            }
        }
        void exp_TextChanged(object sender, TextChangedEventArgs e)
        {
            float junk = 0.0f;
            if (float.TryParse(txtExponent.Text, out junk))
            {
                Curve.Exponent = junk;
                updateGrid();
                //GetBindingExpression(CurveProperty).UpdateTarget();
                GetBindingExpression(CurveProperty).UpdateSource();
            }
        }
        void flipX_Checked(object sender, EventArgs e)
        {
            Curve.FlipX = chkFlipX.IsChecked.GetValueOrDefault();
            updateGrid();
            //GetBindingExpression(CurveProperty).UpdateTarget();
            GetBindingExpression(CurveProperty).UpdateSource();
        }
        void flipY_Checked(object sender, EventArgs e)
        {
            Curve.FlipY = chkFlipY.IsChecked.GetValueOrDefault();
            updateGrid();
            //GetBindingExpression(CurveProperty).UpdateTarget();
            GetBindingExpression(CurveProperty).UpdateSource();
        }
        void typeChanged(object sender, EventArgs e)
        {
            Curve.CurveShape = (CurveType)pickType.SelectedIndex;
            updateGrid();
            //GetBindingExpression(CurveProperty).UpdateTarget();
            GetBindingExpression(CurveProperty).UpdateSource();
        }

        // Drag value handling
        void activateDragCursor(object sender, EventArgs e)
        {
            Mouse.SetCursor(Cursors.SizeWE);
        }
        void deactivateDragCursor(object sender, EventArgs e)
        {
            Mouse.SetCursor(Cursors.Arrow);
        }

        void IncrementTextBox(TextBox tb, float delta)
        {
            float junk = 0.0f;
            if (float.TryParse(tb.Text, out junk))
            {
                junk += delta;
                tb.Text = junk.ToString();
            }
        }

        public static readonly string[] CurveDescriptions = {
            "Returns a fixed value at all times",                                                   // Constant
            "Linear curve - uses X,Y, and Slope only - slope pivots the line around the intercept", // Linear
            "Quadratic curve for nice arcs",                                                        // Quadratic
            "Sigmoid style S-curve, uses all parameters",                                           // Logistic
            "Vertically oriented Sigmoid style S-curve, uses all parameters",                       // Logit
            "\"Steps\" as 0 or 1 the value based on the X-Intercept",                               // Threshold
            "Plots a sine wave - difficult to use well",                                            // Sine
            "Evaluates a parabolic function - uses all parameters",                                 // Parabolic
            "Evaluates a normal distribution function (PDF) - uses all parameters",                 // Normal Distribution
            "Evaluates a normal distribution function (PDF) - uses all parameters",                 // Polynomial
            "Creates a bouncing arc pattern of decay",                                              // Bounce
            "Creates an arc that wavers at the apex",                                               // Berp
        };

        public static readonly string[] XInterceptDescriptions =
        {
            "Unused",              // Constant
            "Offsets the X input", // Linear
            "Offsets the X input", // Quadratic
            "Offsets the X input", // Logistic
            "Offsets the X input", // Logit
            "Offsets the X input", // Threshold
            "Offsets the X input", // Sine
            "Offsets the X input", // Parabolic
            "Offsets the X input", // Normal Distribution
            "Offsets the X input", // Bounce
        };

        public static readonly string[] YInterceptDescriptions =
        {
            "Sets the value that will be returned", // Constant
            "Offsets the returned value of Y",      // Linear
            "Offsets the returned value of Y",      // Quadratic
            "Offsets the returned value of Y",      // Logistic
            "Offsets the returned value of Y",      // Logit
            "Adjusts the upperbound return value",  // Threshold
            "Offsets the returned value of Y",      // Sine
            "Offsets the returned value of Y",      // Parabolic
            "Offsets the returned value of Y",      // Normal Distribution
            "Offsets the returned value of Y",      // Bounce
        };

        public static readonly string[] SlopeDescriptions =
        {
            "Unused",                                    // Constant
            "Adjusts the slope of the line",             // Linear
            "Adjusts the tightness of the curve",        // Quadratic
            "Adjusts the tightness of the curve",        // Logistic
            "Adjusts the intensity of the outer slopes", // Logit
            "Adjusts the lower bound return value",      // Threshold
            "Scales the wave",                           // Sine
            "Adjusts the width of the parabola",         // Parabolic
            "Adjusts the slope of the distribution",     // Normal Distribution
            "Adjusts the apex of each bounce",           // Bounce
        };

        public static readonly string[] ExponentDescriptions =
        {
            "Unused",                                       // Constant
            "Unused",                                       // Linear
            "Adjusts the slope",                            // Quadratic
            "Adjusts the slope",                            // Logistic
            "Rotates the curve around the rightmost edge",  // Logit
            "Unsued",                                       // Threshold
            "Intensely scales the sine repeat rate",        // Sine
            "Adjusts XY in an arc pattern",                 // Parabolic
            "Adjusts the squash of the normal distribution",// Normal Distribution
            "Adjusts the number of bounces",                // Bounce
        };

        void prepareToolTip(object sender, ToolTipEventArgs e)
        {
            if (sender == txtX)
                txtX.ToolTip = getToolTip(XInterceptDescriptions).Localize();
            if (sender == txtY)
                txtY.ToolTip = getToolTip(YInterceptDescriptions).Localize();
            if (sender == txtSlope)
                txtSlope.ToolTip = getToolTip(SlopeDescriptions).Localize();
            if (sender == txtExponent)
                txtExponent.ToolTip = getToolTip(ExponentDescriptions).Localize();
            if (sender == pickType)
                pickType.ToolTip = getToolTip(CurveDescriptions).Localize();
        }

        string getToolTip(string[] list)
        {
            return list[pickType.SelectedIndex];
        }
    }
}
