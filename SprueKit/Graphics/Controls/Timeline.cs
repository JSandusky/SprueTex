using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.WpfInterop;
using MonoGame.Framework.WpfInterop.Input;
using SprueKit.Data;

namespace SprueKit.Graphics.Controls
{
    public class TimelineState
    {
        public float CurrentTime;
        public System.Windows.Point ScrollOffset;
        public List<KeyframeItem> selectedKeys = new List<KeyframeItem>();
    }

    public class Timeline : WpfGame, SprueKit.Controls.IVirtualControl
    {
        public static readonly DependencyProperty IsPlayingProperty = DependencyProperty.Register(
            "IsPlaying",
            typeof(bool),
            typeof(Timeline),
            new PropertyMetadata(false));

        public bool IsPlaying { get { return (bool)GetValue(IsPlayingProperty); } set { SetValue(IsPlayingProperty, value); } }

        public static readonly DependencyProperty IsLoopingProperty = DependencyProperty.Register(
            "IsLooping",
            typeof(bool),
            typeof(Timeline),
            new PropertyMetadata(false));

        public bool IsLooping { get { return (bool)GetValue(IsLoopingProperty); } set { SetValue(IsLoopingProperty, value); } }

        public static readonly DependencyProperty IsWrapAroundProperty = DependencyProperty.Register(
            "IsWrapAround",
            typeof(bool),
            typeof(Timeline),
            new PropertyMetadata(true));

        public bool IsWrapAround { get { return (bool)GetValue(IsWrapAroundProperty); } set { SetValue(IsWrapAroundProperty, value); } }

        int zoomLevel_ = 4;
        static readonly float[] ZoomLevels = { 10.0f, 20.0f, 40.0f, 80.0f, 160.0f, 240.0f, 600.0f };
        static readonly float[] ValueSteps = { 10.0f, 4.0f, 2.0f, 1.0f, 0.5f, 0.25f , 0.1f };
        static readonly int[] SubTickCount = { 5, 4, 4, 4, 5, 5, 4 };
        private static float[] PossibleValueSteps = { 10000, 5000, 1000, 500, 100, 50, 10, 5, 1, 0.5f, 0.1f, 0.05f, 0.01f, 0.005f, 0.001f, 0.0005f, 0.0001f };

        public static readonly int HALF_KEY_WIDTH = 5;

        static TimelineState defaultState_ = new Controls.TimelineState();
        TimelineState state_ = new TimelineState();
        public TimelineState State {
            get { return state_; }
            set
            {
                if (value == null)
                    state_ = defaultState_;
                else
                state_ = value;
                if (Area != null)
                    Area.Specify(state_.ScrollOffset.X, state_.ScrollOffset.Y);
            }
        }

        public SprueKit.Controls.VirtualScrollArea Area { get; set; }
        SprueKit.Controls.Marquee selectionMarquee_;
        SprueKit.Controls.Marquee spanMarquee_;
        bool isDragging_ = false;
        bool isPanning_ = false;
        bool isSpanSelecting_ = false;
        bool isHeaderSpanSelecting_ = false;
        SpriteFont font_;
        SpriteBatch batch_;
        DebugDraw debugDraw_;
        DebugMesh debugMesh_;
        Camera orthoCamera_;
        bool firstInit_ = true;
        SprueKit.Data.Timeline sourceTimeline_;

        public SprueKit.Data.Timeline SourceTimeline {
            get { return sourceTimeline_; }
            set {
                sourceTimeline_ = value;
                if (sourceTimeline_ != null)
                    sourceTimeline_.OrganizeKeyframes();
                EndDrag();
            }
        }

        public event EventHandler<Timeline> StartRender;
        double startPos = 0;
        double panPos = 0;
        double dragActionOffset = 0.0;

        KeyframeItem lastSelected = null;
        KeyframeItem mouseOverItem;

        public Timeline()
        {
            ToolTipService.SetInitialShowDelay(this, 0);
            ToolTipService.SetBetweenShowDelay(this, 0);
            ToolTipService.SetShowDuration(this, 1000000);

            state_ = defaultState_;
            SourceTimeline = new SprueKit.Data.Timeline();
            VisualEdgeMode = System.Windows.Media.EdgeMode.Aliased;
            Content = new ContentManager(Services, "Content");

            MouseDown += Timeline_MouseDown;
            MouseUp += Timeline_MouseUp;
            MouseMove += Timeline_MouseMove;
            MouseWheel += Timeline_MouseWheel;
            LostMouseCapture += Timeline_LostMouseCapture;
        }

        private void Timeline_LostMouseCapture(object sender, MouseEventArgs e)
        {
            EndDrag();
        }

        private void Timeline_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                int sign = Math.Sign(e.Delta);
                zoomLevel_ = Math.Max(0, Math.Min(zoomLevel_ + sign, ZoomLevels.Length - 1));
            }
            else
            {
                Area.ExternalScroll(-e.Delta);
            }
        }

        private void Timeline_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (Mouse.OverrideCursor != null && e.LeftButton != MouseButtonState.Pressed && e.MiddleButton != MouseButtonState.Pressed)
                EndDrag();

            if (sourceTimeline_ == null)
                return;

            if (selectionMarquee_ != null)
            {
                selectionMarquee_.EndPoint = e.GetPosition(this);
                var selRect = selectionMarquee_.GetRect();

                state_.selectedKeys.Clear();
                foreach (var track in sourceTimeline_.Tracks)
                {
                    foreach (var key in track.Keyframes)
                    {
                        if (selRect.IntersectsWith(GetKeyRect(track, key)))
                            state_.selectedKeys.Add(key);
                    }
                }

                e.Handled = true;
                return;
            }

            if (e.GetPosition(this).Y < trackHeight_ && e.RightButton == MouseButtonState.Pressed)
            {
                double x = e.GetPosition(this).X;
                x -= state_.ScrollOffset.X;
                x -= (headerWidth_);
                state_.CurrentTime = Math.Max((float)(x / GetPixelsPerSecond()), 0);
                return;
            }
            else if (((e.GetPosition(this).X < headerWidth_ && e.GetPosition(this).X > headerWidth_ - 60 && !isDragging_) && e.LeftButton == MouseButtonState.Pressed) || isHeaderSpanSelecting_)
            {
                if (!isHeaderSpanSelecting_)
                {
                    spanMarquee_ = new SprueKit.Controls.Marquee
                    {
                        StartPoint = new System.Windows.Point(int.MinValue, e.GetPosition(this).Y),
                        EndPoint = new System.Windows.Point(int.MaxValue, e.GetPosition(this).Y),
                    };
                }
                else
                {
                    spanMarquee_.EndPoint = new System.Windows.Point(int.MaxValue, e.GetPosition(this).Y);
                    var selRect = spanMarquee_.GetRect();

                    state_.selectedKeys.Clear();
                    foreach (var track in sourceTimeline_.Tracks)
                    {
                        foreach (var key in track.Keyframes)
                        {
                            if (selRect.IntersectsWith(GetKeyRect(track, key)))
                                state_.selectedKeys.Add(key);
                        }
                    }
                }
                StartDrag();
                isHeaderSpanSelecting_ = true;
                e.Handled = true;
                return;
            }
            else if ((e.GetPosition(this).Y < trackHeight_ && e.LeftButton == MouseButtonState.Pressed) || isSpanSelecting_)
            {
                if (!isSpanSelecting_)
                {
                    spanMarquee_ = new SprueKit.Controls.Marquee {
                        StartPoint = new System.Windows.Point(e.GetPosition(this).X, int.MinValue),
                        EndPoint = new System.Windows.Point(e.GetPosition(this).X, int.MaxValue),
                    };
                }
                else
                {
                    spanMarquee_.EndPoint = new System.Windows.Point(e.GetPosition(this).X, int.MaxValue);
                    var selRect = spanMarquee_.GetRect();

                    state_.selectedKeys.Clear();
                    foreach (var track in sourceTimeline_.Tracks)
                    {
                        foreach (var key in track.Keyframes)
                        {
                            if (selRect.IntersectsWith(GetKeyRect(track, key)))
                                state_.selectedKeys.Add(key);
                        }
                    }
                }
                StartDrag();
                isSpanSelecting_ = true;
                e.Handled = true;
                return;
            }

            if (isPanning_)
            {
                StartDrag();

                if (e.LeftButton != MouseButtonState.Pressed && e.MiddleButton != MouseButtonState.Pressed)
                {
                    EndDrag();
                }
                else if (e.MiddleButton == MouseButtonState.Pressed)
                {
                    double delta = panPos - e.GetPosition(this).X;
                    Mouse.OverrideCursor = Cursors.SizeWE;
                    if (Area != null)
                        Area.Pan((float)delta);
                    panPos = e.GetPosition(this).X;
                    e.Handled = true;
                    return;
                }
            }

            var pos = e.GetPosition(this);
            var clickPos = pos.X + state_.ScrollOffset.X - HALF_KEY_WIDTH;
            double pps = GetPixelsPerSecond();

            if (!isDragging_ && state_.selectedKeys.Count > 0 && e.LeftButton == MouseButtonState.Pressed)
            {
                if (lastSelected != null)
                    SetSelectedFrame(lastSelected, Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl));
                StartDrag();
                Mouse.OverrideCursor = Cursors.SizeWE;
                isDragging_ = true;
                startPos = clickPos;
                e.Handled = true;
            }

            bool isAltDown = state_.selectedKeys.Count > 1 && (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt));

            if (isDragging_ && state_.selectedKeys.Count > 0)
            {
                double delta = clickPos - startPos;
                double clickTime = clickPos * pps;

                if (isAltDown)
                {
                    Vector2 range = GetSelectionRange();
                    Vector2 newRange = new Vector2(range.X, range.Y + (float)(delta/pps));
                    ScaleSelectedKeys(range, newRange);
                }
                else
                {
                    foreach (var key in state_.selectedKeys)
                    {
                        if (IsWrapAround)
                        {
                            key.Time = (float)(key.Time + (float)(delta / pps));
                            while (key.Time < 0)
                                key.Time += sourceTimeline_.MaxTime;
                        }
                        else
                            key.Time = Math.Max(0, (float)(key.Time + (float)(delta / pps)));
                    }
                }

                startPos = clickPos;
                e.Handled = true;
                sourceTimeline_.OrganizeKeyframes();
                e.Handled = true;
            }
            else
            {
                if (pos.X < headerWidth_ || pos.Y < trackHeight_)
                    return;

                GetHitKeyframe(pos);
                e.Handled = true;
            }
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            if (ToolTip == null)
                ToolTip = new ToolTip() { IsOpen = false };

            var pos = e.GetPosition(this);
            if (pos.X < headerWidth_ || pos.Y < trackHeight_)
            {
                ((ToolTip)ToolTip).IsOpen = false;
                return;
            }

            if (!isPanning_ && !isHeaderSpanSelecting_ && !isSpanSelecting_ && selectionMarquee_ == null && spanMarquee_ == null)
            {
                var hitKey = GetHitKeyframe(pos);
                if (hitKey != null)
                {
                    var ts = TimeSpan.FromSeconds(hitKey.Time);
                    ((ToolTip)ToolTip).Content = string.Format("{0}:{1:00}.{2:0###}", (int)ts.TotalMinutes, ts.Seconds, ts.Milliseconds);
                    ((ToolTip)ToolTip).HorizontalOffset = 20;
                    ((ToolTip)ToolTip).VerticalOffset = -20;
                    ((ToolTip)ToolTip).IsOpen = true;
                    return;
                }
            }
            ((ToolTip)ToolTip).IsOpen = false;
        }

        class KeyTimeSettings
        {
            public float Time { get; set; } = 0.0f;
        }
        class KeyOffsetSettings
        {
            public float Amount { get; set; } = 0.0f;
        }
        private void Timeline_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                ContextMenu = new ContextMenu();

                if (state_.selectedKeys.Count > 0)
                {
                    var setKeyTime = new MenuItem { Header = "Set Time" };
                    setKeyTime.Click += (o, ee) =>
                    {
                        KeyTimeSettings settings = new KeyTimeSettings();
                        Dlg.ReflectiveDlg dlg = new Dlg.ReflectiveDlg(settings, "Set Time", "Apply", null);
                        dlg.enterAccepts = true;
                        if (dlg.ShowDialog().Value)
                        {
                            float startingKeyTime = float.MaxValue; ;
                            for (int i = 0; i < state_.selectedKeys.Count; ++i)
                                startingKeyTime = Math.Min(startingKeyTime, state_.selectedKeys[i].Time);
                            for (int i = 0; i < state_.selectedKeys.Count; ++i)
                                state_.selectedKeys[i].Time = Math.Max(0, state_.selectedKeys[i].Time - startingKeyTime + settings.Time);
                        }
                    };

                    var shiftKeys = new MenuItem { Header = "Offset Keys" };
                    shiftKeys.Click += (o, ee) =>
                    {
                        KeyOffsetSettings settings = new KeyOffsetSettings();
                        Dlg.ReflectiveDlg dlg = new Dlg.ReflectiveDlg(settings, "Offset Keys", "Apply", null);
                        dlg.enterAccepts = true;
                        if (dlg.ShowDialog().Value)
                        {
                            for (int i = 0; i < state_.selectedKeys.Count; ++i)
                            {
                                state_.selectedKeys[i].Time += settings.Amount;
                                if (IsWrapAround)
                                {
                                    while (state_.selectedKeys[i].Time > sourceTimeline_.MaxTime)
                                        state_.selectedKeys[i].Time -= sourceTimeline_.MaxTime;
                                    while (state_.selectedKeys[i].Time < 0)
                                        state_.selectedKeys[i].Time += sourceTimeline_.MaxTime;
                                }
                                else
                                    state_.selectedKeys[i].Time = Math.Max(0, State.selectedKeys[i].Time);
                            }
                            sourceTimeline_.OrganizeKeyframes();
                        }
                    };

                    ContextMenu.Items.Add(setKeyTime);
                    ContextMenu.Items.Add(shiftKeys);
                    ContextMenu.Items.Add(new Separator());
                }

                MenuItem wrapMode = new MenuItem
                {
                    Header = "Wrap Around Mode",
                    IsChecked = IsWrapAround
                };
                wrapMode.Click += (o, ee) => { IsWrapAround = !IsWrapAround; };
                ContextMenu.Items.Add(wrapMode);

                ContextMenu.Items.Add(new Separator());
                ContextMenu.Items.Add(new MenuItem { Header = "Loop Points" });
                ContextMenu.Items.Add(new MenuItem { Header = "Set Loop Start Here" });
                ContextMenu.Items.Add(new MenuItem { Header = "Set Loop End Here" });
                ContextMenu.Items.Add(new MenuItem { Header = "Set Animation End Here" });

                ContextMenu.IsOpen = true;

                return;
            }
            else
                ContextMenu = null;

            if (selectionMarquee_ != null || spanMarquee_ != null)
            {
                EndDrag();
                e.Handled = true;
                return;
            }

            if (e.ChangedButton == MouseButton.Middle)
            {
                EndDrag();
                e.Handled = true;
                return;
            }

            lastSelected = null;
            bool ctrlDown = (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl));
            bool shiftDown = (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift));

            var pt = e.GetPosition(this);

            if (state_.selectedKeys.Count > 0 && !isDragging_)
            {
                var hitFrame = GetHitKeyframe(pt);
                if (hitFrame != null)
                {
                    if (ctrlDown)
                    {
                        //do nothing ToggleSelectedFrame(hitFrame, true);
                    }
                    else if (shiftDown)
                        SetSelectedFrame(hitFrame, true);
                    else
                        SetSelectedFrame(hitFrame, false);
                }
                else
                    state_.selectedKeys.Clear();
            }
        }

        private void Timeline_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(this);

            // Clicks in the header do different thigns
            if (pos.Y < trackHeight_ || pos.X < headerWidth_)
            {
                if (e.ChangedButton == MouseButton.Right && pos.Y < trackHeight_)
                {
                    double x = e.GetPosition(this).X;
                    x -= state_.ScrollOffset.X;
                    x -= (headerWidth_);
                    state_.CurrentTime = Math.Max((float)(x / GetPixelsPerSecond()), 0);
                    return;
                }
                return;
            }

            EndDrag();

            if (e.RightButton == MouseButtonState.Pressed)
            {
                e.Handled = true;
                return;
            }

            var clickPos = e.GetPosition(this);

            // TODO click void space and drag to pan
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                panPos = pos.X;
                isPanning_ = true;
                e.Handled = true;
            }
            else
            {
                if (clickPos.X < headerWidth_ || clickPos.Y < trackHeight_)
                    return;

                double pixelsASecond = GetPixelsPerSecond();
                bool ctrlDown = (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl));
                bool shiftDown = (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift));

                if (shiftDown && selectionMarquee_ == null)
                {
                    selectionMarquee_ = new SprueKit.Controls.Marquee { StartPoint = e.GetPosition(this), EndPoint = e.GetPosition(this) };
                    StartDrag();
                    e.Handled = true;
                    return;
                }

                double clickX = clickPos.X - HALF_KEY_WIDTH;

                bool anyHit = false;
                foreach (var track in sourceTimeline_.Tracks)
                {
                    foreach (var keyframe in track.Keyframes)
                    {
                        if (GetKeyRect(track, keyframe).Contains(clickPos))
                        {
                            /*if (selectedKeys.Contains(keyframe) && ctrlDown)
                                selectedKeys.Remove(keyframe);
                            else */
                            startPos = clickX;
                            dragActionOffset = (clickX / pixelsASecond) - keyframe.Time;
                            anyHit = true;
                            if (!ctrlDown && !shiftDown && !state_.selectedKeys.Contains(keyframe))
                                lastSelected = keyframe;

                            if (!state_.selectedKeys.Contains(keyframe))
                            {
                                state_.selectedKeys.Add(keyframe);
                            }
                        }
                    }
                }

                if (!anyHit)
                    state_.selectedKeys.Clear();
            }
        }

        protected override void Initialize()
        {
            Services.AddService<IGraphicsDeviceService>(new BullshitGraphicsDeviceService(GraphicsDevice));
            base.Initialize();
            if (firstInit_)
            {
                debugDraw_ = new DebugDraw(GraphicsDevice);
                debugMesh_ = new DebugMesh(GraphicsDevice);
                font_ = Content.Load<SpriteFont>("Fonts/Main8");
                batch_ = new SpriteBatch(GraphicsDevice);

                orthoCamera_ = Graphics.Camera.CreateOrtho(GraphicsDevice);
                orthoCamera_.Position = new Vector3(0.0f, 0.0f, 1);
            }
            firstInit_ = false;
        }

        int trackHeight_ = 30;
        int headerWidth_ = 280;
        int keyWidth_ = 20;
        int primaryKeyInterval_ = 5;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        float ToY(float val)
        {
            return GraphicsDevice.Viewport.Height - val;
        }

        static Color timePosKeyColor = new Color(Color.IndianRed, 0.5f);
        static Color KeyframeColor = new Color(Color.Green, 128);
        static Color SelectedKeyframeColor = new Color(Color.Gold, 128);
        static Color marqueeColor = new Color(Color.Cyan, 150);
        static Color background = new Color(33, 33, 33, 255);
        RasterizerState contentRasterizerState = new RasterizerState { ScissorTestEnable = true, CullMode = CullMode.None, FillMode = FillMode.Solid };
        protected override void Draw(GameTime time)
        {
            if (this.ActualWidth <= 0 || this.ActualHeight <= 0)
                Visibility = System.Windows.Visibility.Hidden;
            else
                Visibility = System.Windows.Visibility.Visible;
            if (!this.IsVisible)
                return;

            if (StartRender != null)
                StartRender(this, this);

            orthoCamera_.SetToOrthoGraphicsExact(GraphicsDevice, 0, 0);

            float deltaTime = time.ElapsedGameTime.Milliseconds / 1000.0f;
            if ((IsPlaying || IsLooping) && sourceTimeline_ != null)
            {
                if (state_.CurrentTime < 0)
                    state_.CurrentTime = 0;
                state_.CurrentTime += deltaTime;
                if (state_.CurrentTime > sourceTimeline_.MaxTime && !IsLooping)
                {
                    state_.CurrentTime = 0;
                    IsPlaying = false;
                }
                while (state_.CurrentTime > sourceTimeline_.MaxTime)
                    state_.CurrentTime -= sourceTimeline_.MaxTime;
            }

            GraphicsDevice.Clear(background);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            int HEIGHT = GraphicsDevice.Viewport.Height;
            int WIDTH = GraphicsDevice.Viewport.Width;

            RenderHeaders();

            // Draw the indicators
            float pps = GetPixelsPerSecond();
            float bestStep = FindBestIndicatorStep();
            float indicatorStep = bestStep * pps;
            float tpos = (float)state_.ScrollOffset.X;

            RenderTimeTrack();

            debugDraw_.Begin(orthoCamera_.ViewMatrix, orthoCamera_.ProjectionMatrix);
            debugDraw_.DrawLine(new Vector3(0, ToY(trackHeight_), 0), new Vector3(WIDTH, ToY(trackHeight_), 0), Color.DimGray);
            debugDraw_.End();

            if (sourceTimeline_ != null)
            {
                debugDraw_.Begin(orthoCamera_.ViewMatrix, orthoCamera_.ProjectionMatrix);
                GraphicsDevice.RasterizerState = contentRasterizerState;
                GraphicsDevice.ScissorRectangle = new Rectangle(headerWidth_, trackHeight_, (int)ActualWidth, (int)ActualHeight);
                for (int i = 0; i < sourceTimeline_.Tracks.Count; ++i)
                {
                    var track = sourceTimeline_.Tracks[i];
                    for (int x = 0; x < track.Keyframes.Count - 1; ++x)
                        RenderLinesBetweenFrames(track, track.Keyframes[x], track.Keyframes[x + 1]);
                }
                debugDraw_.End();
            }

            RenderKeyframes();

            if (state_.CurrentTime >= 0.0f)
            {
                debugDraw_.Begin(orthoCamera_.ViewMatrix, orthoCamera_.ProjectionMatrix);
                GraphicsDevice.RasterizerState = contentRasterizerState;
                GraphicsDevice.ScissorRectangle = new Rectangle(headerWidth_, 0, (int)ActualWidth, (int)ActualHeight);
                float x = GetPixelsPerSecond() * state_.CurrentTime + headerWidth_ + (float)state_.ScrollOffset.X;
                debugDraw_.DrawLine(new Vector3(x, ToY(trackHeight_), 0), new Vector3(x, 0, 0), Color.Red);

                // arrow
                debugDraw_.DrawLine(new Vector3(x - 4, ToY(trackHeight_ / 2), 0), new Vector3(x + 4, ToY(trackHeight_/2), 0), Color.Red);
                debugDraw_.DrawLine(new Vector3(x - 4, ToY(trackHeight_/2), 0), new Vector3(x, ToY(trackHeight_), 0), Color.Red);
                debugDraw_.DrawLine(new Vector3(x + 4, ToY(trackHeight_/2), 0), new Vector3(x, ToY(trackHeight_), 0), Color.Red);

                debugDraw_.End();
            }

            if (selectionMarquee_ != null)
            {
                debugMesh_.Begin(orthoCamera_.ViewMatrix, orthoCamera_.ProjectionMatrix);
                GraphicsDevice.RasterizerState = contentRasterizerState;
                var r = selectionMarquee_.GetRect();
                debugMesh_.DrawBorderedQuad(new Vector2((float)r.Left, ToY((float)r.Top)), new Vector2((float)r.Right, ToY((float)r.Bottom)), marqueeColor, Color.Cyan);
                debugMesh_.End();
            }

            if (spanMarquee_ != null)
            {
                debugMesh_.Begin(orthoCamera_.ViewMatrix, orthoCamera_.ProjectionMatrix);
                GraphicsDevice.RasterizerState = contentRasterizerState;
                var r = spanMarquee_.GetRect();
                debugMesh_.DrawBorderedQuad(new Vector2((float)r.Left, ToY((float)r.Top)), new Vector2((float)r.Right, ToY((float)r.Bottom)), marqueeColor, Color.Cyan);
                debugMesh_.End();
            }
        }

        RasterizerState textRas;
        void RenderHeaders()
        {
            if (sourceTimeline_ == null)
                return;

            if (textRas == null)
                textRas = new RasterizerState { ScissorTestEnable = true, CullMode = CullMode.None, FillMode = FillMode.Solid };

            var oldRas = GraphicsDevice.RasterizerState;
            batch_.GraphicsDevice.ScissorRectangle = new Rectangle(0, trackHeight_, headerWidth_, GraphicsDevice.Viewport.Height);
            batch_.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp);
            batch_.GraphicsDevice.RasterizerState = textRas;
            int yOffset = trackHeight_;
            for (int i = 0; i < sourceTimeline_.Tracks.Count; ++i)
            {
                var track = sourceTimeline_.Tracks[i];

                int yAdj = (int)((trackHeight_ - 12) / 2);
                batch_.DrawString(font_, track.Name, new Vector2(4, ((float)(int)(yAdj + yOffset + state_.ScrollOffset.Y))), Color.White, 0, Vector2.Zero, 1.0f, SpriteEffects.None, 0);
                yOffset += trackHeight_;
            }
            batch_.End();

            GraphicsDevice.RasterizerState = oldRas;
        }

        Color majorIndicatorColor = new Color(70, 70, 70, 255);
        Color indicatorColor = new Color(50, 50, 50, 255);
        void RenderTimeTrack()
        {
            debugDraw_.Begin(orthoCamera_.ViewMatrix, orthoCamera_.ProjectionMatrix);
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            debugDraw_.DrawLine(new Vector3(headerWidth_, ToY(trackHeight_), 0), new Vector3(headerWidth_, (float)ActualHeight, 0), Color.DimGray);
            debugDraw_.End();

            debugDraw_.Begin(orthoCamera_.ViewMatrix, orthoCamera_.ProjectionMatrix);
            GraphicsDevice.RasterizerState = contentRasterizerState;
            GraphicsDevice.ScissorRectangle = new Rectangle(headerWidth_, 0, (int)ActualWidth, (int)ActualHeight);

            // Draw the indicators
            float pps = GetPixelsPerSecond();
            float bestStep = FindBestIndicatorStep();
            float indicatorStep = bestStep * pps;
            float tpos = (float)state_.ScrollOffset.X;
            tpos += headerWidth_;

            float oldTPos = tpos;
            while (tpos < ActualWidth)
            {
                var time = Math.Round(((tpos - state_.ScrollOffset.X + headerWidth_) / pps) / bestStep) * bestStep;
                debugDraw_.DrawLine(new Vector3(tpos, ToY(0), 0), new Vector3(tpos, ToY((float)ActualHeight), 0), majorIndicatorColor);
                tpos += indicatorStep;
                time = Math.Round(((tpos - state_.ScrollOffset.X + headerWidth_) / pps) / bestStep) * bestStep;

                // minor indicators
                for (int i = 0; i < 4; i++)
                {
                    var minorStep = indicatorStep / 6;
                    var mpos = (tpos - indicatorStep) + i * minorStep + minorStep;
                    debugDraw_.DrawLine(new Vector3(mpos, ToY(20), 0), new Vector3(mpos, ToY((float)ActualHeight), 0), indicatorColor);
                }
            }

            if (sourceTimeline_ != null)
            {
                for (int i = 0; i < sourceTimeline_.Tracks.Count; ++i)
                {
                    var track = sourceTimeline_.Tracks[i];
                    for (int x = 0; x < track.Keyframes.Count; ++x)
                    {
                        var key = track.Keyframes[x];
                        var keyRect = GetKeyRect(track, key);
                        debugDraw_.DrawLine(new Vector3((float)keyRect.HorizontalMiddle(), ToY(10), 0), new Vector3((float)keyRect.HorizontalMiddle(), ToY(trackHeight_), 0), timePosKeyColor);
                    }
                }
            }

            debugDraw_.End();

            batch_.GraphicsDevice.ScissorRectangle = new Rectangle(0, trackHeight_, headerWidth_, GraphicsDevice.Viewport.Height);
            batch_.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp);
            tpos = oldTPos;
            while (tpos < ActualWidth)
            {
                var time = Math.Round(((tpos - state_.ScrollOffset.X - headerWidth_) / pps) / bestStep) * bestStep;
                string timeText = String.Format("{0:0.##}", time);
                float textWidth = font_.MeasureString(timeText).X * 1.0f;
                // Draw text along top
                batch_.DrawString(font_, timeText, new Vector2((int)(tpos - (textWidth / 2.0f)), 0), Color.White, 0, Vector2.Zero, 1.0f, SpriteEffects.None, 0);
                tpos += indicatorStep;
            }

            batch_.End();
        }

        void RenderKeyframes()
        {
            debugMesh_.Begin(orthoCamera_.ViewMatrix, orthoCamera_.ProjectionMatrix);
            GraphicsDevice.RasterizerState = contentRasterizerState;
            GraphicsDevice.ScissorRectangle = new Rectangle(headerWidth_, trackHeight_, (int)ActualWidth, (int)ActualHeight);
            if (sourceTimeline_ != null)
            {
                for (int trackIdx = 0; trackIdx < sourceTimeline_.Tracks.Count; ++trackIdx)
                {
                    float trackY = trackHeight_ + trackHeight_ * trackIdx;
                    TimelineTrack track = sourceTimeline_.Tracks[trackIdx];
                    for (int keyIdx = 0; keyIdx < track.Keyframes.Count; ++keyIdx)
                    {
                        KeyframeItem key = track.Keyframes[keyIdx];
                        Rect keyRect = GetKeyRect(track, key);
                        var drawColor = state_.selectedKeys.Contains(key) ? SelectedKeyframeColor : KeyframeColor;
                        var borderColor = state_.selectedKeys.Contains(key) ? Color.Gold : Color.Green;
                        debugMesh_.DrawBorderedDiamon(new Vector2((float)keyRect.Left, ToY((float)keyRect.Top)), new Vector2((float)keyRect.Right, ToY((float)keyRect.Bottom)), drawColor, borderColor);
                        //debugMesh_.DrawBorderedQuad(new Vector2((float)keyRect.Left, ToY((float)keyRect.Top)), new Vector2((float)keyRect.Right, ToY((float)keyRect.Bottom)), drawColor, borderColor);
                    }
                }
            }
            debugMesh_.End();
        }

        void RenderLinesBetweenFrames(TimelineTrack track, KeyframeItem from, KeyframeItem to)
        {
            bool drawCurve = true;
            if (drawCurve)
            {
                Rect thisKeyRect = GetKeyRect(track, from);
                Rect nextKeyRect = GetKeyRect(track, to);

                float distanceBetween = (float)(nextKeyRect.X - thisKeyRect.Right);

                Vector3 left = new Vector3((float)thisKeyRect.Right, ToY((float)thisKeyRect.VerticalMiddle()), 0);
                Vector3 right = new Vector3((float)nextKeyRect.Left, ToY((float)nextKeyRect.VerticalMiddle()), 0);

                Vector3 lowerLeft = new Vector3((float)left.X,   ToY((float)thisKeyRect.Bottom), 0);
                Vector3 upperRight = new Vector3((float)right.X, ToY((float)nextKeyRect.Top), 0);

                if (distanceBetween < 20)
                {
                    // too short, so just draw a straight line
                    debugDraw_.DrawLine(left, right, marqueeColor);
                }
                else
                {
                    left.X += 5;
                    right.X -= 5;

                    debugDraw_.DrawLine(lowerLeft, left, marqueeColor);
                    debugDraw_.DrawLine(left, right, marqueeColor);
                    debugDraw_.DrawLine(right, upperRight, marqueeColor);
                }
            }
            else
            {
                Rect thisKeyRect = GetKeyRect(track, from);
                Rect nextKeyRect = GetKeyRect(track, to);
                debugDraw_.DrawLine(
                    new Vector3((float)thisKeyRect.Right, ToY((float)thisKeyRect.VerticalMiddle()), 0), 
                    new Vector3((float)nextKeyRect.Left, ToY((float)nextKeyRect.VerticalMiddle()), 0), marqueeColor);
            }
        }

        float viewingRange = -1;
        protected float GetPixelsPerSecond()
        {
            if (sourceTimeline_ == null)
                return ZoomLevels[zoomLevel_];
            //return (float)ActualWidth / sourceTimeline_.MaxTime;
            return ZoomLevels[zoomLevel_];
        }

        Rect GetKeyRect(TimelineTrack track, KeyframeItem key)
        {
            int trackOffset = (int)(sourceTimeline_.Tracks.IndexOf(track) * trackHeight_ + state_.ScrollOffset.Y);
            return new Rect(key.Time * GetPixelsPerSecond() + state_.ScrollOffset.X + headerWidth_ - HALF_KEY_WIDTH,
                trackHeight_ + trackOffset + 2,
                GetKeyframeWidth(key),
                trackHeight_ - 5);
        }

        private float GetKeyframeWidth(KeyframeItem keyframe)
        {
            float pixelsASecond = GetPixelsPerSecond();
            if (keyframe.Duration > 0f)
                return Math.Max(3.0f, keyframe.Duration * pixelsASecond);
            return 10.0f;
        }

        void StartDrag()
        {
            CaptureMouse();
            //??Mouse.AddMouseUpHandler(this, External_MouseUp);
        }

        void EndDrag()
        {
            bool wasDragging = isDragging_;
            isDragging_ = false;
            isPanning_ = false;
            isSpanSelecting_ = false;
            isHeaderSpanSelecting_ = false;

            ReleaseMouseCapture();
            //??Mouse.RemoveMouseUpHandler(this, External_MouseUp);

            Mouse.OverrideCursor = null;

            if (wasDragging && sourceTimeline_ != null)
            {
                sourceTimeline_.OrganizeKeyframes();
            }

            selectionMarquee_ = null;
            spanMarquee_ = null;
        }

        void SetSelectedFrame(KeyframeItem frame, bool inclusive = false)
        {
            if (inclusive && !state_.selectedKeys.Contains(frame))
                state_.selectedKeys.Add(frame);
            else if (!inclusive)
            {
                state_.selectedKeys.Clear();
                state_.selectedKeys.Add(frame);
            }
        }

        KeyframeItem GetHitKeyframe(System.Windows.Point pt)
        {
            var oldItem = mouseOverItem;
            mouseOverItem = null;
            if (sourceTimeline_ != null)
            {
                foreach (var track in sourceTimeline_.Tracks)
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
                return mouseOverItem;
            }
            return null;
        }

        public Size RequiredArea()
        {
            if (sourceTimeline_ != null)
            {
                return new Size(sourceTimeline_.MaxTime * 1.25f * GetPixelsPerSecond() + headerWidth_, trackHeight_ * 2 + (trackHeight_ * sourceTimeline_.Tracks.Count));
            }
            return new Size(0, 0);
        }

        public void ZoomToBestFit()
        {
            if (sourceTimeline_ == null)
                return;

            var min = sourceTimeline_.MinTime;
            var max = sourceTimeline_.MaxTime;

            var diff = max - min;
            if (diff < 1)
                diff = 1;

            float pixelsASecond = (float)(GetPixelsPerSecond());

            viewingRange = diff + 20 / pixelsASecond;

            pixelsASecond = (float)(ActualWidth / sourceTimeline_.MaxTime);

            state_.ScrollOffset.X = (min * pixelsASecond - 10) * -1;
            if (state_.ScrollOffset.X > 10)
                state_.ScrollOffset.X = 10;
        }

        public float XToTime(float x)
        {
            return (float)(x / GetPixelsPerSecond() - state_.ScrollOffset.X - HALF_KEY_WIDTH - headerWidth_);
        }

        public float TimeToX(float time)
        {
            return (float)(time * GetPixelsPerSecond() + state_.ScrollOffset.X + HALF_KEY_WIDTH + headerWidth_);
        }

        public void SetScrollOffset(Size size)
        {
            if (isDragging_)
                startPos -= state_.ScrollOffset.X;

            state_.ScrollOffset.X = -size.Width;
            state_.ScrollOffset.Y = -size.Height;

            if (isDragging_)
                startPos += state_.ScrollOffset.X;
        }

        float GetTrackTop(TimelineTrack track)
        {
            return (float)(20 + sourceTimeline_.Tracks.IndexOf(track) * trackHeight_ + state_.ScrollOffset.Y);
        }

        float GetTrackBottom(TimelineTrack track) { return GetTrackTop(track) + trackHeight_; }

        private float FindBestIndicatorStep() { return ValueSteps[zoomLevel_]; }

        Vector2 GetSelectionRange()
        {
            Vector2 v = new Vector2(int.MaxValue, int.MinValue);
            for (int i = 0; i < state_.selectedKeys.Count; ++i)
            {
                v.X = Math.Min(v.X, state_.selectedKeys[i].Time);
                v.Y = Math.Max(v.X, state_.selectedKeys[i].Time);
            }
            return v;
        }

        void ScaleSelectedKeys(Vector2 oldRange, Vector2 newRange)
        {
            for (int i = 0; i < state_.selectedKeys.Count; ++i)
            {
                float normVal = Mathf.Normalize(state_.selectedKeys[i].Time, oldRange.X, oldRange.Y);
                state_.selectedKeys[i].Time = Mathf.Denormalize(normVal, newRange.X, newRange.Y);
            }
        }
    }
}
