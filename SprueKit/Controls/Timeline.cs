using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Timers;

using SprueKit.Data;
using System.Globalization;

namespace SprueKit.Controls
{
    public class Timeline : Control, IVirtualControl
    {
        public VirtualScrollArea Area { get; set; }

        public static readonly int KEY_WIDTH = 10;
        public static readonly int TRACK_HEIGHT = 30;
        public static readonly int HALF_KEY_WIDTH = 5;
        public static readonly int TOP_TRACK_HEIGHT = 14;
        public static readonly int TITLE_WIDTH = 100;

        private static float[] PossibleValueSteps = { 10000, 5000, 1000, 500, 100, 50, 10, 5, 1, 0.5f, 0.1f, 0.05f, 0.01f, 0.005f, 0.001f, 0.0005f, 0.0001f };

        static Pen keyConnectorLinePen_ = null;
        static Pen indicatorPen_ = null;
        static Pen majorIndicatorPen_ = null;
        static Pen timePen_ = null;
        private static Pen[] NumberTrackColours = { new Pen(Brushes.ForestGreen, 2), new Pen(Brushes.DarkCyan, 2), new Pen(Brushes.DarkViolet, 2), new Pen(Brushes.DarkOrange, 2) };

        public static readonly DependencyProperty BackgroundBrushProperty = DependencyProperty.Register(
            "BackgroundBrush", typeof(Brush), typeof(Timeline), 
            new FrameworkPropertyMetadata(new SolidColorBrush(Colors.Transparent)));// new Color { R = 31, G = 31, B = 31, A = 255 })));

        public Brush BackgroundBrush { get { return (Brush)GetValue(BackgroundBrushProperty); }  set { SetValue(BackgroundBrushProperty, value); } }

        public static readonly DependencyProperty MajorIndicatorBrushProperty = DependencyProperty.Register(
            "MajorIndicatorBrush", typeof(Brush), typeof(Timeline),
            new FrameworkPropertyMetadata(new SolidColorBrush(new Color { R = 70, G = 70, B = 70, A = 255 })));

        public static readonly DependencyProperty IndicatorBrushProperty = DependencyProperty.Register(
            "IndicatorBrush", typeof(Brush), typeof(Timeline),
            new FrameworkPropertyMetadata(new SolidColorBrush(new Color { R = 50, G = 50, B = 50, A = 255 })));

        public Brush IndicatorBrush { get { return (Brush)GetValue(IndicatorBrushProperty); } set { SetValue(IndicatorBrushProperty, value); } }

        public Brush MajorIndicatorBrush { get { return (Brush)GetValue(MajorIndicatorBrushProperty); } set { SetValue(MajorIndicatorBrushProperty, value); } }

        public static readonly DependencyProperty SelectedBrushProperty = DependencyProperty.Register(
            "SelectedBrush", typeof(Brush), typeof(Timeline),
            new FrameworkPropertyMetadata(new SolidColorBrush(Colors.Gold)));

        public Brush SelectedBrush { get { return (Brush)GetValue(SelectedBrushProperty); } set { SetValue(SelectedBrushProperty, value); } }

        public static readonly DependencyProperty UnselectedBrushProperty = DependencyProperty.Register(
            "UnselectedBrush", typeof(Brush), typeof(Timeline),
            new FrameworkPropertyMetadata(new SolidColorBrush(Colors.Green)));

        public Brush UnselectedBrush { get { return (Brush)GetValue(UnselectedBrushProperty); } set { SetValue(UnselectedBrushProperty, value); } }

        public static readonly DependencyProperty FontBrushProperty = DependencyProperty.Register(
            "FontBrush", typeof(Brush), typeof(Timeline),
            new FrameworkPropertyMetadata(new SolidColorBrush(Colors.White)));

        public Brush FontBrush { get { return (Brush)GetValue(FontBrushProperty); } set { SetValue(FontBrushProperty, value); } }

        public static readonly DependencyProperty PopupBackgroundBrushProperty = DependencyProperty.Register("PopupBackgroundBrush", typeof(Brush), typeof(Timeline));

        public Brush PopupBackgroundBrush { get { return (Brush)GetValue(PopupBackgroundBrushProperty); } set { SetValue(PopupBackgroundBrushProperty, value); } }

        public static readonly DependencyProperty PopupBorderBrushProperty = DependencyProperty.Register("PopupBorderBrush", typeof(Brush), typeof(Timeline));

        public Brush PopupBorderBrush { get { return (Brush)GetValue(PopupBorderBrushProperty); } set { SetValue(PopupBorderBrushProperty, value); } }

        public static readonly DependencyProperty KeyframeBackgroundBrushProperty = DependencyProperty.Register(
            "KeyframeBackgroundBrush", typeof(Brush), typeof(Timeline),
            new FrameworkPropertyMetadata(new SolidColorBrush(Colors.Transparent)));

        public Brush KeyframeBackgroundBrush { get { return (Brush)GetValue(KeyframeBackgroundBrushProperty); } set { SetValue(KeyframeBackgroundBrushProperty, value); } }

        public static readonly DependencyProperty KeyframeSelectedBackgroundBrushProperty = DependencyProperty.Register(
            "KeyframeSelectedBackgroundBrush", typeof(Brush), typeof(Timeline),
            new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromArgb(128, Colors.Gold.R, Colors.Gold.G, Colors.Gold.B))));

        public Brush KeyframeSelectedBackgroundBrush { get { return (Brush)GetValue(KeyframeSelectedBackgroundBrushProperty); } set { SetValue(KeyframeSelectedBackgroundBrushProperty, value); } }

        public static readonly DependencyProperty SnapEnabledProperty = DependencyProperty.Register(
            "SnapEnabled", typeof(bool), typeof(Timeline),
            new FrameworkPropertyMetadata(false));

        public bool SnapEnabled { get { return (bool)GetValue(SnapEnabledProperty); } set { SetValue(SnapEnabledProperty, value); } }

        public static readonly DependencyProperty TimelineProperty = DependencyProperty.Register(
            "SourceTimeline",
            typeof(SprueKit.Data.Timeline),
            typeof(Timeline));

        public SprueKit.Data.Timeline SourceTimeline { get { return (SprueKit.Data.Timeline)GetValue(TimelineProperty); } set { SetValue(TimelineProperty, value); } }

        public static readonly DependencyProperty CurrentTimeProperty = DependencyProperty.Register(
            "CurrentTime",
            typeof(float),
            typeof(Timeline),
            new FrameworkPropertyMetadata(-100.0f));

        public float CurrentTime { get { return (float)GetValue(CurrentTimeProperty); } set { SetValue(CurrentTimeProperty, value); } }

        public static readonly DependencyProperty LoopBeginProperty = DependencyProperty.Register(
            "LoopBegin",
            typeof(float),
            typeof(Timeline),
            new PropertyMetadata(-100.0f));
        
        public float LoopBegin { get { return (float)GetValue(LoopBeginProperty); } set { SetValue(LoopBeginProperty, value); } }
        
        public static readonly DependencyProperty TimelineEndProperty = DependencyProperty.Register(
            "TimelineEnd",
            typeof(float),
            typeof(Timeline),
            new PropertyMetadata(-100.0f));
        
        public float TimelineEnd { get { return (float)GetValue(TimelineEndProperty); } set { SetValue(TimelineEndProperty, value); } }

        List<double> snapLines = new List<double>();

        double startPos = 0;
        double panPos = 0;
        Point scrollOffset = new Point(0, 0);

        bool isDragging = false;
        bool isPanning = false;

        Marquee selectionMarquee;
        List<KeyframeItem> selectedKeys = new List<KeyframeItem>();
        KeyframeItem lastSelected = null;
        KeyframeItem mouseOverItem;
        double dragActionOffset = 0.0;

        Timer redrawTimer;
        bool dirty = false;
        bool isRedrawing = false;
        float viewingRange = -1;

        public event EventHandler<Timeline> StartRender;
        public event EventHandler<Point> OffsetsChanged;

        public Timeline()
        {
            this.CacheMode = null;
            this.VisualCacheMode = null;
            VisualEdgeMode = EdgeMode.Aliased;
            DataContextChanged += Timeline_DataContextChanged;
            SourceTimeline = new Data.Timeline();
            LostMouseCapture += Timeline_LostMouseCapture;
            StartTimer();
        }

        ~Timeline()
        {
            if (redrawTimer != null)
                redrawTimer.Stop();
        }

        void StartTimer()
        {
            if (redrawTimer != null)
                redrawTimer.Stop();
            redrawTimer = new Timer();
            redrawTimer.Interval = (1.0f / 30.0f) * 1000;
            redrawTimer.Elapsed += RedrawTimer_Elapsed;
            redrawTimer.Start();
        }

        private float FindBestIndicatorStep()
        {
            foreach (var step in PossibleValueSteps)
            {
                var steps = Math.Floor(viewingRange / step);

                if (steps > 5 && step < SourceTimeline.MaxTime)
                {
                    return step;
                }
            }

            return 0.5f;//PossibleValueSteps.Last();
        }

        private double GetKeyframeWidth(KeyframeItem keyframe)
        {
            double pixelsASecond = GetPixelsPerSecond();
            if (keyframe.Duration > 0f)
                return keyframe.Duration * pixelsASecond;

            //var preview = keyframe.GetImagePreview();
            //if (preview != null)
            //    return ActualHeight - 20;
            //else
                return 10;
        }

        #region Rendering

        protected double GetPixelsPerSecond()
        {
            if (viewingRange == -1)
                return 160.0;
            return ActualWidth / viewingRange;
        }

        protected void DrawLineBetweenFrames(DrawingContext drawingContext, TimelineTrack track, KeyframeItem from, KeyframeItem to)
        {
            if (keyConnectorLinePen_ == null)
            {
                keyConnectorLinePen_ = new Pen(new SolidColorBrush(Color.FromArgb(128, Colors.Cyan.R, Colors.Cyan.G, Colors.Cyan.B)), 1);
                keyConnectorLinePen_.Freeze();
            }

            bool drawCurve = true;
            if (drawCurve)
            {
                Rect thisKeyRect = GetKeyRect(track, from);
                Rect nextKeyRect = GetKeyRect(track, to);

                if (nextKeyRect.Right < TITLE_WIDTH || thisKeyRect.Left > ActualWidth)
                    return;

                

                float distanceBetween = (float)(nextKeyRect.X - thisKeyRect.Right);

                Point left = new Point(thisKeyRect.Right, thisKeyRect.VerticalMiddle());
                Point right = new Point(nextKeyRect.Left, nextKeyRect.VerticalMiddle());

                Point lowerLeft = new Point(left.X, thisKeyRect.Bottom);
                Point upperRight = new Point(right.X, nextKeyRect.Top);

                if (distanceBetween < 20)
                {
                    // too short, so just draw a straight line
                    drawingContext.DrawLine(keyConnectorLinePen_, left, right);
                }
                else
                {
                    left.X += 5;
                    right.X -= 5;

                    drawingContext.DrawLine(keyConnectorLinePen_, lowerLeft, left);
                    drawingContext.DrawLine(keyConnectorLinePen_, left, right);
                    drawingContext.DrawLine(keyConnectorLinePen_, right, upperRight);
                }
            }
            else
            {
                Rect thisKeyRect = GetKeyRect(track, from);
                Rect nextKeyRect = GetKeyRect(track, to);
                drawingContext.DrawLine(keyConnectorLinePen_, new Point(thisKeyRect.Right, thisKeyRect.VerticalMiddle()), new Point(nextKeyRect.Left, nextKeyRect.VerticalMiddle()));
            }
        }

        Typeface typeface_;
        protected override void OnRender(DrawingContext drawingContext)
        {
            if (StartRender != null)
                StartRender(this, this);

            if (SourceTimeline == null || isRedrawing || ActualHeight <= 0 || ActualWidth <= 0)
                return;

            isRedrawing = true;

            //base.OnRender(drawingContext);
            
            drawingContext.PushClip(new RectangleGeometry(new Rect(0, 0, ActualWidth, ActualHeight)));
            drawingContext.DrawRectangle(BackgroundBrush, null, new Rect(0, 0, ActualWidth, ActualHeight));

            if (indicatorPen_ == null)
            {
                indicatorPen_ = new Pen(IndicatorBrush, 1);
                indicatorPen_.Freeze();
            }

            if (majorIndicatorPen_ == null)
            {
                majorIndicatorPen_ = new Pen(MajorIndicatorBrush, 1);
                majorIndicatorPen_.Freeze();
            }

            if (typeface_ == null)
                typeface_ = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);

            double pps = GetPixelsPerSecond();

            SourceTimeline.OrganizeKeyframes();

            // Draw headers and move the clip rect
            RenderHeaders(drawingContext, typeface_);
            drawingContext.PushClip(new RectangleGeometry(new Rect(TITLE_WIDTH, 0, ActualWidth - TITLE_WIDTH, ActualHeight)));

            // Draw the indicators
            double bestStep = FindBestIndicatorStep();
            double indicatorStep = bestStep * pps;
            double tpos = scrollOffset.X;



            if (scrollOffset.X < 0)
            {
                var remainder = Math.Abs(scrollOffset.X) - Math.Floor(Math.Abs(scrollOffset.X) / indicatorStep) * indicatorStep;
                tpos = -remainder;
            }

            tpos += KEY_WIDTH + TITLE_WIDTH; // minimal offset for zero so we can always see keyframes at 0
            double oldTPos = tpos;
            while (tpos < ActualWidth)
            {
                var time = Math.Round(((tpos - scrollOffset.X + TITLE_WIDTH) / pps) / bestStep) * bestStep;
                drawingContext.DrawLine(majorIndicatorPen_, new Point(tpos, TOP_TRACK_HEIGHT), new Point(tpos, ActualHeight));
                tpos += indicatorStep;
                time = Math.Round(((tpos - scrollOffset.X + TITLE_WIDTH) / pps) / bestStep) * bestStep;

                // minor indicators
                for (int i = 0; i < 5; i++)
                {
                    var minorStep = indicatorStep / 6;
                    var mpos = (tpos - indicatorStep) + i * minorStep + minorStep;
                    drawingContext.DrawLine(indicatorPen_, new Point(mpos, 20), new Point(mpos, ActualHeight));
                }
            }

            // Draw the current time indicator and loop indicators
            if (CurrentTime > 0)
            {
                if (timePen_ == null)
                {
                    timePen_ = new Pen(new SolidColorBrush(Colors.DarkRed), 2);
                    timePen_.Freeze();
                }
                var ptX = CurrentTime * pps + scrollOffset.X + TITLE_WIDTH + KEY_WIDTH;
                drawingContext.DrawTriangle(timePen_, new Point(ptX, 0), 5, 5);
                drawingContext.DrawTriangle(timePen_, new Point(ptX, ActualHeight), 5, -5);
                drawingContext.DrawLine(timePen_, new Point(ptX, 0), new Point(ptX, ActualHeight));
            }

            // Draw the timeline text
            tpos = oldTPos;
            while (tpos < ActualWidth)
            {
                var time = Math.Round(((tpos - scrollOffset.X - TITLE_WIDTH) / pps) / bestStep) * bestStep;
                string timeText = time.ToString();
                FormattedText text = new FormattedText(timeText, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface_, 10, FontBrush);

                // Draw text along top
                drawingContext.DrawText(text, new Point(tpos - (text.Width / 2.0), 0));
                tpos += indicatorStep;
            }

            // Draw the keyframes, but only if there's room to see them
            if (ActualHeight > TOP_TRACK_HEIGHT)
            {
                drawingContext.PushClip(new RectangleGeometry(new Rect(0, TOP_TRACK_HEIGHT, ActualWidth, ActualHeight - TOP_TRACK_HEIGHT)));
                int yOffset = (int)scrollOffset.Y;
                for (int i = 0; i < SourceTimeline.Tracks.Count; ++i)
                {
                    RenderTrack(drawingContext, SourceTimeline.Tracks[i]);
                    if (i < SourceTimeline.Tracks.Count - 1)
                    {
                        float trackTop = GetTrackTop(SourceTimeline.Tracks[i + 1]);
                        drawingContext.DrawLine(indicatorPen_, new Point(0, trackTop), new Point(ActualWidth, trackTop));
                    }

                    yOffset += TRACK_HEIGHT;
                }
            }

            if (selectionMarquee != null)
                selectionMarquee.Draw(drawingContext);

            isRedrawing = false;
        }

        protected void RenderHeaders(DrawingContext context, Typeface typeface)
        {
            if (SourceTimeline == null)
                return;

            int yOffset = TOP_TRACK_HEIGHT;
            for (int i = 0; i < SourceTimeline.Tracks.Count; ++i)
            {
                var track = SourceTimeline.Tracks[i];

                FormattedText text = new FormattedText(track.Name, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, 12, FontBrush);
                text.MaxLineCount = 1;
                text.MaxTextWidth = TITLE_WIDTH;

                int yAdj = (int)((TRACK_HEIGHT - text.LineHeight) / 2);

                context.DrawText(text, new Point(0, yAdj + yOffset + scrollOffset.Y));

                yOffset += TRACK_HEIGHT;
            }
        }

        protected void RenderTrack(DrawingContext drawingContext, TimelineTrack track)
        {
            float top = GetTrackTop(track);
            float bottom = top + TRACK_HEIGHT;

            // don't render tracks outside of view
            if (top > ActualHeight)
                return;
            if (bottom < 0)
                return;

            for (int k = 0; k < track.Keyframes.Count; ++k)
            {
                KeyframeItem keyframe = track.Keyframes[k];

                bool isMouseOverFrame = keyframe == mouseOverItem;
                bool isSelected = selectedKeys.Contains(keyframe);

                var background = isSelected ? KeyframeSelectedBackgroundBrush : KeyframeBackgroundBrush;
                var thickness = isMouseOverFrame ? 2 : 1;
                var pen = isSelected ? new Pen(SelectedBrush, thickness) : new Pen(UnselectedBrush, thickness);
                var width = GetKeyframeWidth(keyframe);

                // frame iconography
                if (false)
                {

                }
                else
                {
                    // Trivial frame drawing
                    Rect thisKeyRect = GetKeyRect(track, keyframe);

                    // Only draw the keyframe if in view
                    if (thisKeyRect.Right >= TITLE_WIDTH && thisKeyRect.Left < ActualWidth)
                        drawingContext.DrawRectangle(background, pen, thisKeyRect);

                    if (k < track.Keyframes.Count - 1)
                    {
                        var nextKey = track.Keyframes[k + 1];
                        DrawLineBetweenFrames(drawingContext, track, keyframe, nextKey);
                    }
                }
            }
        }

        #endregion

        #region Input handling functions

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            base.OnPreviewMouseWheel(e);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                Area.PageUp();
            else if (e.Delta < 0)
                Area.PageDown();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(this);

            // Clicks in the header do different thigns
            if (pos.Y < TOP_TRACK_HEIGHT || pos.X < TITLE_WIDTH)
                return;

            bool shfitDown = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);

            // TODO click void space and drag to pan
            if (e.MiddleButton == MouseButtonState.Pressed || (e.LeftButton == MouseButtonState.Pressed && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))))
            {
                panPos = pos.X;
                isPanning = true;
                e.Handled = true;
                dirty = true;
                //InvalidateVisual();
            }
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (shfitDown && selectionMarquee == null)
                {
                    selectionMarquee = new Marquee { StartPoint = e.GetPosition(this), EndPoint = e.GetPosition(this) };
                    StartDrag();
                    dirty = true;
                    //InvalidateVisual();
                    return;
                }

                if (selectedKeys.Count > 0)
                {
                    //isDragging = true;
                    //e.Handled = true;
                    //startPos = pos.X + leftPad - HALF_KEY_WIDTH;
                    dirty = true;
                    //InvalidateVisual();
                }
            }
        }

        void External_MouseUp(object sender, MouseButtonEventArgs e)
        {
            OnMouseUp(e);   
        }


        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            isDragging = false;
            isPanning = false;
            EndDrag();

            var clickPos = e.GetPosition(this);

            if (e.ChangedButton == MouseButton.Right && clickPos.Y < TOP_TRACK_HEIGHT)
            {
                clickPos.X -= scrollOffset.X;
                clickPos.X -= (TITLE_WIDTH + KEY_WIDTH);
                CurrentTime = (float)(clickPos.X / GetPixelsPerSecond());
                dirty = true;
                //InvalidateVisual();
            }
        }

        private void Timeline_LostMouseCapture(object sender, MouseEventArgs e)
        {
            isDragging = false;
            isPanning = false;
            EndDrag();
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (SourceTimeline == null)
                return;

            var pos = e.GetPosition(this);

            if (pos.X < TITLE_WIDTH || pos.Y < TOP_TRACK_HEIGHT)
                return;

            var clickPos = pos.X + scrollOffset.X - HALF_KEY_WIDTH;

            double pixelsASecond = GetPixelsPerSecond();

            bool ctrlDown = (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl));
            bool shiftDown = (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift));

            // remove selected keys
            //if (selectedKeys.Count > 0 && !shiftDown && !ctrlDown)
            //    selectedKeys.Clear();

            bool anyHit = false;
            foreach (var track in SourceTimeline.Tracks)
            {
                foreach (var keyframe in track.Keyframes)
                {
                    if (GetKeyRect(track, keyframe).Contains(pos))
                    {
                        /*if (selectedKeys.Contains(keyframe) && ctrlDown)
                            selectedKeys.Remove(keyframe);
                        else */
                        startPos = clickPos;
                        dragActionOffset = (clickPos / pixelsASecond) - keyframe.Time;
                        anyHit = true;
                        if (!ctrlDown && !shiftDown && !selectedKeys.Contains(keyframe))
                            lastSelected = keyframe;

                        if (!selectedKeys.Contains(keyframe))
                        {
                            selectedKeys.Add(keyframe);
                            //InvalidateVisual();
                        }
                    }
                }
            }

            if (!anyHit)
                selectedKeys.Clear();

            if (selectedKeys.Count > 0)
                GenerateSnapList();

            //e.Handled = true;
            dirty = true;
            //InvalidateVisual();
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (selectionMarquee != null)
                return;

            lastSelected = null;
            bool ctrlDown = (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl));
            bool shiftDown = (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift));

            var pt = e.GetPosition(this);

            if (selectedKeys.Count > 0 && !isDragging)
            {
                var hitFrame = GetHitKeyframe(pt);
                if (hitFrame != null)
                {
                    if (ctrlDown)
                    {
                     //do nothign   ToggleSelectedFrame(hitFrame, true);
                    }
                    else if (shiftDown)
                        SetSelectedFrame(hitFrame, true);
                    else
                        SetSelectedFrame(hitFrame, false);
                }
                else
                    selectedKeys.Clear();

                dirty = true;
                //InvalidateVisual();
            }

            base.OnPreviewMouseLeftButtonUp(e);
        }

        protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
        {
            if (SourceTimeline == null)
                return;

            var pos = e.GetPosition(this);
            var clickPos = pos.X - scrollOffset.X;
            double pixelsASecond = GetPixelsPerSecond();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (Mouse.OverrideCursor != null && e.LeftButton != MouseButtonState.Pressed)
                EndDrag();

            if (SourceTimeline == null)
                return;

            if (selectionMarquee != null)
            {
                selectionMarquee.EndPoint = e.GetPosition(this);
                var selRect = selectionMarquee.GetRect();

                selectedKeys.Clear();
                foreach (var track in SourceTimeline.Tracks)
                {
                    foreach (var key in track.Keyframes)
                    {
                        if (selRect.IntersectsWith(GetKeyRect(track, key)))
                            selectedKeys.Add(key);
                    }
                }

                dirty = true;
                //InvalidateVisual();
                return;
            }

            if (e.GetPosition(this).Y < TOP_TRACK_HEIGHT && e.RightButton == MouseButtonState.Pressed)
            {
                double x = e.GetPosition(this).X;
                x -= scrollOffset.X;
                x -= (TITLE_WIDTH + KEY_WIDTH);
                CurrentTime = (float)(x / GetPixelsPerSecond());

                dirty = true;
                //InvalidateVisual();
                return;
            }

            if (isPanning)
            {
                StartDrag();

                if (e.LeftButton != MouseButtonState.Pressed && e.MiddleButton != MouseButtonState.Pressed)
                {
                    EndDrag();
                }
                else
                {
                    double delta = panPos - e.GetPosition(this).X;
                    Area.Pan((float)delta);
                    //scrollOffset.X -= delta;
                    //if (scrollOffset.X > 0)
                    //    scrollOffset.X = 0;
                    panPos = e.GetPosition(this).X;
                    //dirty = true;
                    e.Handled = true;
                    return;
                }
                dirty = true;
            }

            bool setCursor = false;
            var pos = e.GetPosition(this);
            var clickPos = pos.X + scrollOffset.X - HALF_KEY_WIDTH;
            double pps = GetPixelsPerSecond();

            if (!isDragging && selectedKeys.Count > 0 && e.LeftButton == MouseButtonState.Pressed)
            {
                if (lastSelected != null)
                    SetSelectedFrame(lastSelected);
                StartDrag();
                Mouse.OverrideCursor = Cursors.SizeWE;
                isDragging = true;
                startPos = clickPos;
                dirty = true;
            }

            if (isDragging && selectedKeys.Count > 0)
            {
                double delta = clickPos - startPos;
                double clickTime = clickPos * pps;
                foreach (var key in selectedKeys)
                    key.Time = Math.Max(0, (float)(key.Time + (float)(delta / pps)));

                startPos = clickPos;
                e.Handled = true;
                SourceTimeline.OrganizeKeyframes();
                dirty = true;
            }
            else
            {
                if (pos.X < TITLE_WIDTH || pos.Y < TOP_TRACK_HEIGHT)
                    return;

                GetHitKeyframe(pos);
                e.Handled = true;
                dirty = true;
            }

            dirty = true;
            //InvalidateVisual();
        }

        void StartDrag()
        {
            CaptureMouse();
            Mouse.AddMouseUpHandler(this, External_MouseUp);
        }

        void EndDrag()
        {
            bool wasDragging = isDragging;
            isDragging = false;
            isPanning = false;

            ReleaseMouseCapture();
            Mouse.RemoveMouseUpHandler(this, External_MouseUp);

            Mouse.OverrideCursor = null;

            if (wasDragging && SourceTimeline != null)
            {
                SourceTimeline.OrganizeKeyframes();
                dirty = true;
                //InvalidateVisual();
            }

            if (selectionMarquee != null)
            {
                selectionMarquee = null;
                dirty = true;
                //InvalidateVisual();
            }
        }

        public void ZoomToBestFit()
        {
            if (SourceTimeline == null)
                return;

            var min = SourceTimeline.MinTime;
            var max = SourceTimeline.MaxTime;

            var diff = max - min;
            if (diff < 1)
                diff = 1;

            float pixelsASecond = (float)(GetPixelsPerSecond());

            viewingRange = diff + 20 / pixelsASecond;

            pixelsASecond = (float)(ActualWidth / SourceTimeline.Range);

            scrollOffset.X = (min * pixelsASecond - 10) * -1;
            if (scrollOffset.X > 10)
                scrollOffset.X = 10;

            dirty = true;
            //InvalidateVisual();
        }

        #endregion

        #region Event functions

        public void OnExternalPropertyChange(object sender, EventArgs e)
        {
            dirty = true;
            InvalidateVisual();
        }

        private void Timeline_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
            {

            }

            if (e.NewValue != null)
                viewingRange = ((SprueKit.Data.Timeline)e.NewValue).Range * 1.1f;
            StartTimer();
        }

        int snuffedRedraws = 0;
        private void RedrawTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (!this.isRedrawing)
                {
                    snuffedRedraws = Math.Max(0, snuffedRedraws - 1);
                    dirty = false;
                    if (Application.Current != null && Application.Current.Dispatcher != null)
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            InvalidateVisual();
                        }));
                }
                else
                    snuffedRedraws += 1;
            }
            catch (Exception)
            {
                snuffedRedraws = 0;
                try
                {
                    dirty = false;
                    if (Application.Current != null && Application.Current.Dispatcher != null)
                        Application.Current.Dispatcher.Invoke(new Action(() => {
                            InvalidateVisual();
                        }));
                }
                catch (Exception ex)
                {

                }
            }
            snuffedRedraws = Math.Max(0, snuffedRedraws - 1);
        }

        #endregion

        #region Line related

        private void GenerateSnapList()
        {
            snapLines.Clear();

            foreach (var track in SourceTimeline.Tracks)
            {
                foreach (KeyframeItem keyframe in track.Keyframes)
                {
                    if (!selectedKeys.Contains(keyframe))
                    {
                        var time = keyframe.Time;
                        if (!snapLines.Contains(time)) snapLines.Add(time);
                        if (keyframe.Duration > 0)
                        {
                            time = keyframe.EndTime;
                            if (!snapLines.Contains(time)) snapLines.Add(time);
                        }
                    }
                }
            }

            snapLines.Sort();
        }

        private double Snap(double time)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                double bestStep = FindBestIndicatorStep() / 6;
                var roundedTime = Math.Floor(time / bestStep) * bestStep;
                time = roundedTime;
            }
            else
            {
                double pixelsASecond = GetPixelsPerSecond();
                
                double bestSnapTime = -1;
                double bestSnapDist = 10.0 / pixelsASecond;
                
                foreach (var line in snapLines)
                {
                    double diff = Math.Abs(line - time);
                    if (diff < bestSnapDist)
                    {
                        bestSnapDist = diff;
                        bestSnapTime = line;
                    }
                }
                
                if (bestSnapTime > -1)
                {
                    time = bestSnapTime;
                }
            }

            return time;
        }

        #endregion

        #region Keyframe functions

        float GetTrackTop(TimelineTrack track)
        {
            return (float)(20 + SourceTimeline.Tracks.IndexOf(track) * TRACK_HEIGHT + scrollOffset.Y);
        }

        float GetTrackBottom(TimelineTrack track)
        {
            return GetTrackTop(track) + TRACK_HEIGHT;
        }

        Rect GetKeyRect(TimelineTrack track, KeyframeItem key)
        {
            int trackOffset = (int)(SourceTimeline.Tracks.IndexOf(track) * TRACK_HEIGHT + scrollOffset.Y);
            return new Rect(key.Time * GetPixelsPerSecond() + scrollOffset.X + HALF_KEY_WIDTH + TITLE_WIDTH, 
                20 + trackOffset, 
                GetKeyframeWidth(key), 
                TRACK_HEIGHT - 5);
        }

        KeyframeItem GetHitKeyframe(Point pt)
        {
            var oldItem = mouseOverItem;
            mouseOverItem = null;
            if (SourceTimeline != null)
            {
                foreach (var track in SourceTimeline.Tracks)
                {
                    if (mouseOverItem != null)
                        break;
                    foreach (var key in track.Keyframes)
                    {
                        if (GetKeyRect(track, key).Contains(pt))
                        {
                            mouseOverItem = key;
                            break;
                        }
                    }
                }

                if (mouseOverItem != oldItem)
                {
                    dirty = true;
                    //InvalidateVisual();
                }
                return mouseOverItem;
            }
            return null;
        }

        void ToggleSelectedFrame(KeyframeItem item, bool inclusive = false)
        {
            if (selectedKeys.Contains(item))
                selectedKeys.Remove(item);
            else if (inclusive)
                selectedKeys.Add(item);
            else
            {
                selectedKeys.Clear();
                selectedKeys.Add(item);
            }
        }

        void SetSelectedFrame(KeyframeItem frame, bool inclusive = false)
        {
            if (inclusive && !selectedKeys.Contains(frame))
                selectedKeys.Add(frame);
            else if (!inclusive)
            {
                selectedKeys.Clear();
                selectedKeys.Add(frame);
            }
        }

        public Size RequiredArea()
        {
            if (SourceTimeline != null)
            {
                return new Size(SourceTimeline.MaxTime * 1.25f * GetPixelsPerSecond() + TITLE_WIDTH, TOP_TRACK_HEIGHT*2 + (TRACK_HEIGHT * SourceTimeline.Tracks.Count));
            }
            return new Size(0, 0);
        }

        public void SetScrollOffset(Size size)
        {
            if (isDragging)
                startPos -= scrollOffset.X;

            scrollOffset.X = -size.Width;
            scrollOffset.Y = -size.Height;

            if (isDragging)
                startPos += scrollOffset.X;
            dirty = true;
            InvalidateVisual();
        }

        public float XToTime(float x)
        {
            return (float)(x / GetPixelsPerSecond() - scrollOffset.X - HALF_KEY_WIDTH - TITLE_WIDTH);
        }

        public float TimeToX(float time)
        {
            return (float)(time * GetPixelsPerSecond() + scrollOffset.X + HALF_KEY_WIDTH + TITLE_WIDTH);
        }

        #endregion
    }
}
