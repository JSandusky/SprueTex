using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SprueKit.Controls
{
    public class IMTreeRecord
    {
        public object Data { get; set; }
        public Rect Bounds { get; set; }
        public Rect? ExpanderBounds;
        public bool IsSelected { get; set; } = false;
        public bool Used { get; set; } = true;
    }

    public class IMTree : Control
    {
        int IndentWidth = 16;

        // Zebra-stripe row background brush
        public static readonly DependencyProperty ZebraBrushProperty = DependencyProperty.Register(
            "ZebraBrush", typeof(Brush), typeof(IMTree),
            new FrameworkPropertyMetadata(new SolidColorBrush(Colors.Transparent)));// new Color { R = 31, G = 31, B = 31, A = 255 })));
        public Brush ZebraBrush { get { return (Brush)GetValue(ZebraBrushProperty); } set { SetValue(ZebraBrushProperty, value); } }

        // Selection color brush
        public static readonly DependencyProperty SelectionBrushProperty = DependencyProperty.Register(
            "SelectionBrush", typeof(Brush), typeof(IMTree),
            new FrameworkPropertyMetadata(new SolidColorBrush(Colors.Transparent)));// new Color { R = 31, G = 31, B = 31, A = 255 })));

        // Zebra-stripe row background brush
        public static readonly DependencyProperty FontBrushProperty = DependencyProperty.Register(
            "FontBrush", typeof(Brush), typeof(IMTree),
            new FrameworkPropertyMetadata(new SolidColorBrush(Colors.Transparent)));// new Color { R = 31, G = 31, B = 31, A = 255 })));
        public Brush FontBrush { get { return (Brush)GetValue(FontBrushProperty); } set { SetValue(FontBrushProperty, value); } }

        public Brush SelectionBrush { get { return (Brush)GetValue(SelectionBrushProperty); } set { SetValue(SelectionBrushProperty, value); } }

        public static readonly DependencyProperty ExpandedIconProperty = DependencyProperty.Register("ExpandedIcon",
            typeof(ImageSource), typeof(IMTree),
            new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty CollapsedIconProperty = DependencyProperty.Register("CollapsedIcon",
            typeof(ImageSource), typeof(IMTree),
            new FrameworkPropertyMetadata(null));
        public ImageSource ExpandedIcon { get { return (ImageSource)GetValue(ExpandedIconProperty); } set { SetValue(ExpandedIconProperty, value); } }
        public ImageSource CollapsedIcon { get { return (ImageSource)GetValue(CollapsedIconProperty); } set { SetValue(CollapsedIconProperty, value); } }

        bool isRedrawing_ = false;
        protected Typeface typeface_ = null;

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (isRedrawing_)
                return;

            stripeCounter_ = 0;
            isRedrawing_ = true;

            base.OnRender(drawingContext);
            drawingContext.PushClip(new RectangleGeometry(new Rect(0, 0, ActualWidth, ActualHeight)));
            //drawingContext.DrawRectangle(BackgroundBrush, null, new Rect(0, 0, ActualWidth, ActualHeight));

            if (typeface_ == null)
                typeface_ = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);

            int currentY = 0;
            if (DataContext != null)
                DrawItem(drawingContext, ref currentY, DataContext, 0);

            isRedrawing_ = false;
        }

        int stripeCounter_ = 0;
        protected bool ShouldStripe()
        {
            bool ret = stripeCounter_ % 2 != 0;
            ++stripeCounter_;
            return ret;
        }

        protected int GetLeftEdge(int indentDepth)
        {
            return IndentWidth * indentDepth;
        }

        protected void DrawItem(DrawingContext context, ref int currentY, object data, int indent)
        {
            DrawSimpleItem(context, ref currentY, data.ToString(), indent);
        }

        protected void DrawSimpleItem(DrawingContext drawingContext, ref int currentY, object textObject, int indent)
        {
            int x = GetLeftEdge(indent);

            // add space because our parent has an expander
            if (indent > 0)
                x += (int)Math.Max(CollapsedIcon.Width, ExpandedIcon.Width);

            int lineHeight = (int)Math.Max(CollapsedIcon.Height, ExpandedIcon.Height);

            FormattedText drawText = new FormattedText(textObject.ToString(), CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface_, 10, FontBrush);

            Point textPoint = new Point(x, currentY + (drawText.Height - lineHeight) * 0.5);
            lineHeight = (int)Math.Max(drawText.Height, lineHeight);

            IMTreeRecord record = MakeOrGetTreeRecord(textObject, new Rect(textPoint, new Size(drawText.Width, drawText.Height)), null, false);
            if (record.IsSelected)
                drawingContext.DrawRectangle(SelectionBrush, null, record.Bounds);
            drawingContext.DrawText(drawText, textPoint);
            currentY += (int)Math.Max(drawText.Height, lineHeight);
        }

        #region Cache methods

        List<IMTreeRecord> Records = new List<IMTreeRecord>();

        IMTreeRecord MakeOrGetTreeRecord(object dataObject, Rect areaRect, Rect? expandBounds, bool selected)
        {
            IMTreeRecord current = Records.FirstOrDefault(p => p.Data == dataObject);
            if (current == null)
            {
                current = new IMTreeRecord { Data = dataObject };
                Records.Add(current);
                current.IsSelected = selected;
            }

            current.Used = true;
            current.Bounds = areaRect;
            current.ExpanderBounds = expandBounds;
            return current;
        }

        #endregion

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {

        }
    }
}
