using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SprueKit.Graphics.Sculpt
{
    public class SculptingView : ViewportDelegate
    {
        static readonly Guid GUID = new Guid("94327f4a-f321-4cfc-872c-6f0d4f916595");

#region implement ViewportDelegate
        public override string ViewportName { get { return "Sculpt"; } }
        public override Guid GetID() { return GUID; }
        #endregion

        protected Camera camera_;
        protected SprueKit.Graphics.Controllers.CameraController cameraController_;

        private SprueKit.Graphics.DebugRenderer debugDraw_;

        public SculptingView(BaseScene scene) : base(scene)
        {
            scene_.SizeChanged += Scene__SizeChanged;
        }

        private void Scene__SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            if (camera_ != null && GraphicsDevice != null)
            {
                if (GraphicsDevice.Viewport.Width != (int)scene_.ActualWidth || GraphicsDevice.Viewport.Height != (int)scene_.ActualHeight)
                    GraphicsDevice.Viewport = new Viewport(0, 0, (int)scene_.ActualWidth, (int)scene_.ActualHeight, 0, 1);
                camera_.SetToPerspective(GraphicsDevice, 45);
                //orthoCamera.SetToOrthoGraphic(GraphicsDevice, 0, 0);
            }
        }

        #region Lifecycle
        public override void Initialize()
        {
            bool firstInit = firstInit_;
            base.Initialize();

            if (firstInit)
            {
                debugDraw_ = new DebugRenderer();
                debugDraw_.SetEffect(new BasicEffect(GraphicsDevice) { LightingEnabled = false, VertexColorEnabled = true, TextureEnabled = false });

                camera_ = new Camera(GraphicsDevice, 45);
                if (cameraController_ == null)
                    cameraController_ = new SprueKit.Graphics.Controllers.CameraController(scene_, this, camera_);
                else
                    cameraController_.camera = camera_;
                camera_.LookAtPoint(new Vector3(15, 15, 0), new Vector3(0, 5, 0));
                cameraController_.Focus();
            }
        }

        public override void Dispose()
        {
            if (debugDraw_ != null)
                debugDraw_.Dispose();
            debugDraw_ = null;
            base.Dispose();
        }
        #endregion

        public override void Draw(GameTime time)
        {
            if (!scene_.IsVisible)
                return;

            base.Draw(time);

            float deltaTime = time.ElapsedGameTime.Milliseconds / 1000.0f;
            camera_.UpdateAnimations(deltaTime);

            // Line drawing
            debugDraw_.Begin();
            GraphicsDevice.BlendState = BlendState.NonPremultiplied;

            // Floor grid
            debugDraw_.DrawWireGrid(new Vector3(64, 0, 0), new Vector3(0, 0, 64), new Vector3(-32, 0, -32), 64, 64, new Color(10, 10, 10));

            // Axis Indicators
            Vector3 offset = Vector3.UnitY * 0.02f;
            debugDraw_.DrawLine(Vector3.Zero + offset, Vector3.UnitX * 16 + offset, Color.Red);
            debugDraw_.DrawLine(Vector3.Zero + offset, Vector3.UnitZ * 16 + offset, Color.CornflowerBlue);
            debugDraw_.DrawLine(new Vector3(-2.0f, offset.Y, 1.0f * 12) + offset, Vector3.UnitZ * 16 + offset, Color.CornflowerBlue);
            debugDraw_.DrawLine(new Vector3(2.0f, offset.Y, 1.0f * 12) + offset, Vector3.UnitZ * 16 + offset, Color.CornflowerBlue);

            if (cameraController_ != null)
                cameraController_.Update(time.ElapsedGameTime.Milliseconds / 1000.0f);

            debugDraw_.Render(GraphicsDevice, camera_.CombinedMatrix);
        }
    }
}
