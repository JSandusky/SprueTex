using GongSolutions.Wpf.DragDrop;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Framework.WpfInterop;
using MonoGame.Framework.WpfInterop.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SprueKit.Graphics
{
    public class BullshitGraphicsDeviceService : IGraphicsDeviceService
    {
        public BullshitGraphicsDeviceService(GraphicsDevice device)
        {
            GraphicsDevice = device;
        }

        public GraphicsDevice GraphicsDevice { set; get; }

        public event EventHandler<EventArgs> DeviceCreated;
        public event EventHandler<EventArgs> DeviceDisposing;
        public event EventHandler<EventArgs> DeviceReset;
        public event EventHandler<EventArgs> DeviceResetting;
    }

    public class BaseScene : WpfGame, IInitializable, IDropTarget
    {
        protected WpfKeyboard keyboard_;
        protected KeyboardState keyboardState_;
        //protected WpfMouse mouse_;
        //protected MouseState mouseState_;
        protected bool disposed_ = false;

        public static readonly DependencyProperty ActiveViewportProperty = DependencyProperty.Register(
            "ActiveViewport",
            typeof(ViewportDelegate),
            typeof(BaseScene),
            new PropertyMetadata(null));

        ViewportDelegate activeViewport_;
        public ViewportDelegate ActiveViewport {
            get { return GetValue(ActiveViewportProperty) as ViewportDelegate; }
            set {
                ViewportDelegate oldValue = activeViewport_;
                if (oldValue != null)
                {
                    oldValue.IsActive = false;
                    oldValue.Deactivated();
                }
                SetValue(ActiveViewportProperty, value);
                bool changed = value != oldValue;
                activeViewport_ = value;
                if (value != null)
                {
                    value.IsActive = true;
                    if (!firstInit_)
                        value.Initialize();
                    if (changed)
                        value.Activated();
                }
                OnPropertyChanged(new DependencyPropertyChangedEventArgs(BaseScene.ActiveViewportProperty, oldValue, activeViewport_));
            }
        }

        public BaseScene() : base("Content")
        {
            VisualEdgeMode = System.Windows.Media.EdgeMode.Aliased;
            Content = new ContentManager(Services, "Content");
            GongSolutions.Wpf.DragDrop.DragDrop.SetIsDropTarget(this, true);
            GongSolutions.Wpf.DragDrop.DragDrop.SetDropHandler(this, this);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposed_)
                return;
            disposed_ = true;
        }

        Color background = new Color(33, 33, 33, 255);
        protected override void Draw(GameTime time)
        {
            if (this.ActualWidth <= 0 || this.ActualHeight <= 0)
                Visibility = Visibility.Hidden;
            else
                Visibility = Visibility.Visible;
            if (!this.IsVisible)
                return;

            float deltaTime = time.ElapsedGameTime.Milliseconds / 1000.0f;
            GraphicsDevice.Clear(background);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            if (ActiveViewport != null)
                ActiveViewport.Draw(time);
        }

        protected bool firstInit_ = true;
        protected override void Initialize()
        {
            Services.AddService<IGraphicsDeviceService>(new BullshitGraphicsDeviceService(GraphicsDevice));
            base.Initialize();

            if (firstInit_)
            {
                keyboard_ = new WpfKeyboard(this);
                //mouse_ = new WpfMouse(this);
                //mouse_.CaptureMouseWithin = false;

                if (ActiveViewport != null)
                    ActiveViewport.Initialize();
            }
            firstInit_ = false;
        }

        protected override void Update(GameTime gameTime)
        {
            //mouseState_ = mouse_.GetState();
            keyboardState_ = keyboard_.GetState();
        }

        void IInitializable.Initialize()
        {
            
        }

        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            if (ActiveViewport is IDropTarget)
            {
                ((IDropTarget)ActiveViewport).DragOver(dropInfo);
                return;
            }
            dropInfo.Effects = DragDropEffects.None;
        }

        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            if (ActiveViewport is IDropTarget)
            {
                ((IDropTarget)ActiveViewport).Drop(dropInfo);
            }
        }
    }
}
