using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SprueKit.Data;
using SprueKit.Graphics;

namespace SprueKit.Data.IKAnim
{
    public class IKScene : ViewportDelegate
    {
        IOCDependency<DocumentManager> documentManager = new IOCDependency<DocumentManager>();
        IKRig rig_;
        static readonly Guid GUID = new Guid("ec2d405e-cc63-471c-bd9c-3d4501106cf8");

        private SprueKit.Graphics.Materials.MatCapEffect normalEffect_;
        protected Camera camera_;
        protected Camera orthoCamera_;
        private BasicEffect _basicEffect;
        protected SprueKit.Graphics.Controllers.CameraController cameraController_;
        private SprueKit.Graphics.DebugRenderer debugDraw;
        SpriteFont font_;
        SpriteBatch batch_;
        SelectionColorer selColorer;
        bool gizmoUsedMouse = false;
        private bool _disposed;
        IKAnimDocument document_;

        public override string ViewportName { get { return "Rig View"; } }

        public IKScene(BaseScene scene, IKRig rig, IKAnimDocument doc) : base(scene)
        {
            document_ = doc;
            rig_ = rig;

            scene_.MouseMove += (o, evt) =>
            {
                if (IsActive)
                    evt.Handled = false;
            };

            scene_.SizeChanged += (o, e) => { sizeChanged_ = true; };

            scene_.MouseLeftButtonUp += Scene__MouseLeftButtonUp;
        }

        public override Guid GetID() { return GUID; }

        public override void Initialize()
        {
            bool firstInit = firstInit_;
            base.Initialize();

            if (firstInit)
            {
                camera_ = new Camera(GraphicsDevice, 45);
                if (cameraController_ == null)
                    cameraController_ = new SprueKit.Graphics.Controllers.CameraController(scene_, this, camera_);
                else
                    cameraController_.camera = camera_;
                cameraController_.PrimaryIsOrbit = false;

                camera_.LookAtPoint(new Vector3(15, 15, 0), new Vector3(0, 5, 0));
                cameraController_.Focus();

                orthoCamera_ = new Camera(GraphicsDevice, 45);
                orthoCamera_.SetToOrthoGraphic(GraphicsDevice, (float)scene_.ActualWidth, (float)scene_.ActualHeight);
                orthoCamera_.Position = new Vector3(0, 15, -25);
                orthoCamera_.LookAtPoint(new Vector3(0, 0, 0));

                _basicEffect = new BasicEffect(GraphicsDevice);
                _basicEffect.World = Matrix.Identity;
                _basicEffect.View = camera_.ViewMatrix;
                _basicEffect.Projection = camera_.ProjectionMatrix;

                debugDraw = new SprueKit.Graphics.DebugRenderer();
                debugDraw.SetEffect(new BasicEffect(GraphicsDevice) { LightingEnabled = false, VertexColorEnabled = true, TextureEnabled = false });
                //debugMesh = new SprueKit.Graphics.DebugMesh(GraphicsDevice, debugDraw.basicEffect);
                font_ = scene_.Content.Load<SpriteFont>("Fonts/Main12");
                batch_ = new SpriteBatch(GraphicsDevice);

                // primitive color
                _basicEffect.AmbientLightColor = new Vector3(0.1f, 0.1f, 0.1f);
                _basicEffect.DiffuseColor = new Vector3(1.0f, 1.0f, 1.0f);
                _basicEffect.SpecularColor = new Vector3(0.25f, 0.25f, 0.25f);
                _basicEffect.SpecularPower = 5.0f;
                _basicEffect.Alpha = 1.0f;
                _basicEffect.LightingEnabled = true;

                normalEffect_ = new SprueKit.Graphics.Materials.MatCapEffect(GraphicsDevice, scene_.Content);
            }
        }

        public override void Dispose()
        {
            if (_disposed)
                return;

            base.Dispose();
            _disposed = true;
            _basicEffect.Dispose();
            debugDraw.Dispose();
            normalEffect_.Dispose();
        }

        bool sizeChanged_ = true;
        void UpdateViewport()
        {
            if (camera_ != null && GraphicsDevice != null)
            {
                if (GraphicsDevice.Viewport.Width != (int)scene_.ActualWidth || GraphicsDevice.Viewport.Height != (int)scene_.ActualHeight)
                    GraphicsDevice.Viewport = new Viewport(0, 0, (int)scene_.ActualWidth, (int)scene_.ActualHeight, 0, 1);
                camera_.SetToPerspective(GraphicsDevice, 45);
                orthoCamera_.SetToOrthoGraphic(GraphicsDevice, 0, 0);
            }
            sizeChanged_ = false;
        }

        public override void Draw(GameTime time)
        {
            if (!scene_.IsVisible)
                return;

            if (sizeChanged_)
                UpdateViewport();

            debugDraw.Begin();
            base.Draw(time);
            gizmoUsedMouse = false;

            float deltaTime = time.ElapsedGameTime.Milliseconds / 1000.0f;
            camera_.UpdateAnimations(deltaTime);

            _basicEffect.View = camera_.ViewMatrix;
            _basicEffect.Projection = camera_.ProjectionMatrix;

            SprueKit.Data.IKAnim.IKAnimDocument doc = documentManager.Object.ActiveDocument as SprueKit.Data.IKAnim.IKAnimDocument;
            if (doc != null)
            {
                GraphicsDevice.RasterizerState = RasterizerState.CullNone;
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                normalEffect_.Begin(GraphicsDevice);
                normalEffect_.WorldViewProjection = camera_.CombinedMatrix;
                normalEffect_.WorldView = camera_.ViewMatrix;
                normalEffect_.Transform = Matrix.Identity;
                if (doc.MeshData != null)
                    doc.MeshData.Draw(GraphicsDevice, normalEffect_);
                normalEffect_.End(GraphicsDevice);

                if (doc.MeshData != null && doc.MeshData.Skeleton != null)
                {
                    GraphicsDevice.DepthStencilState = DepthStencilState.None;
                    DrawBoneRecurse(doc.MeshData.Skeleton.Root, Matrix.Identity, PickJoint(GetPickingRay()));
                }
            }

            // Line drawing

            // Floor grid
            debugDraw.DrawWireGrid(new Vector3(64, 0, 0), new Vector3(0, 0, 64), new Vector3(-32, 0, -32), 64, 64, new Color(45, 45, 45));

            // Axis Indicators
            Vector3 offset = Vector3.UnitY * 0.02f;
            debugDraw.DrawLine(Vector3.Zero + offset, Vector3.UnitX * 16 + offset, Color.Red);
            debugDraw.DrawLine(Vector3.Zero + offset, Vector3.UnitZ * 16 + offset, Color.CornflowerBlue);
            debugDraw.DrawLine(new Vector3(-2.0f, offset.Y, 1.0f * 12) + offset, Vector3.UnitZ * 16 + offset, Color.CornflowerBlue);
            debugDraw.DrawLine(new Vector3(2.0f, offset.Y, 1.0f * 12) + offset, Vector3.UnitZ * 16 + offset, Color.CornflowerBlue);

            // If we didn't use the gizmo, then display it
            if (cameraController_ != null)
                cameraController_.Update(time.ElapsedGameTime.Milliseconds / 1000.0f);

            // Draw our current message
            this.DrawApplicationMessage(GraphicsDevice, batch_, font_);

            var oldX = GraphicsDevice.Viewport.X;
            var oldY = GraphicsDevice.Viewport.Y;
            var oldW = GraphicsDevice.Viewport.Width;
            var oldH = GraphicsDevice.Viewport.Height;

            debugDraw.Render(GraphicsDevice, camera_.CombinedMatrix);
            debugDraw.Begin();

            Viewport vpt = new Viewport { X = GraphicsDevice.Viewport.Width - 64, Y = GraphicsDevice.Viewport.Height - 64, Height = 64, Width = 64 };
            var oldViewport = GraphicsDevice.Viewport;
            GraphicsDevice.Viewport = vpt;
            Camera cm = new Camera(GraphicsDevice, 45);
            cm.Position = new Vector3(0, 0, 5);
            cm.LookAtPoint(Vector3.Zero);
            
            debugDraw.DrawLine(Vector3.Zero, Vector3.Transform(Vector3.UnitX, camera_.ViewMatrix.Rotation), Color.Red);
            debugDraw.DrawLine(Vector3.Zero, Vector3.Transform(Vector3.UnitZ, camera_.ViewMatrix.Rotation), Color.CornflowerBlue);
            debugDraw.DrawLine(Vector3.Zero, Vector3.Transform(Vector3.UnitY, camera_.ViewMatrix.Rotation), Color.Green);
            debugDraw.Render(GraphicsDevice, cm.CombinedMatrix);
            GraphicsDevice.Viewport = oldViewport;
        }

        void DrawBoneRecurse(PluginLib.JointData joint, Matrix transform, PluginLib.JointData hit)
        {
            Color drawColor = documentManager.Object.ActiveDocument.Selection.IsSelected(joint) ? Color.Gold : Color.Cyan;
            //if (!joint.HasChildren)
            {
                float scl = joint.Scale.MaxElement();
                //debugDraw.DrawLine(joint.Position, joint.Position + Vector3.TransformNormal(Vector3.UnitX, joint.Transform) * scl, Color.Red);
                //debugDraw.DrawLine(joint.Position, joint.Position + Vector3.TransformNormal(Vector3.UnitY, joint.Transform) * scl, Color.Green);
                //debugDraw.DrawLine(joint.Position, joint.Position + Vector3.TransformNormal(Vector3.UnitZ, joint.Transform) * scl, Color.Cyan);
                debugDraw.DrawCross(Vector3.Transform(joint.Position, transform), 0.1f * joint.Scale.MaxElement(), joint == hit ? Color.Gold : drawColor);
            }
            //else if (joint == hit)
            //    debugDraw.DrawCross(Vector3.Transform(joint.Position, transform), 0.1f, Color.Gold);

            foreach (var child in joint.Children)
            {
                debugDraw.DrawLine(Vector3.Transform(joint.Position, transform), Vector3.Transform(child.Position, transform), drawColor);
                DrawBoneRecurse(child, transform, hit);
            }
        }

        Ray GetPickingRay()
        {
            var pt = System.Windows.Input.Mouse.GetPosition(scene_);
            return camera_.GetPickRay(GraphicsDevice.Viewport, (float)pt.X, (float)pt.Y);
        }

        PluginLib.JointData PickJoint(Ray ray)
        {
            var doc = documentManager.Object.ActiveDoc<IKAnimDocument>();
            PluginLib.JointData nearest = null;
            float nearestDist = float.MaxValue;
            if (doc != null)
            {
                if (doc.MeshData != null && doc.MeshData.Skeleton != null)
                {
                    for (int i = 0; i < doc.MeshData.Skeleton.Inline.Count; ++i)
                    {
                        var j = doc.MeshData.Skeleton.Inline[i];
                        BoundingSphere sph = new BoundingSphere(j.Position, 0.1f);
                        float? val = ray.Intersects(sph);
                        if (val.HasValue && nearestDist > val.Value)
                        {
                            nearest = j;
                            nearestDist = val.Value;
                        }
                    }
                }
            }
            return nearest;
        }

        private void Scene__MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!IsActive)
                return;

            if (gizmoUsedMouse)
                return;

            var pt = e.GetPosition(scene_);
            // CTRL click to toggle
            if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftCtrl) || System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftCtrl))
            {
                var j = PickJoint(GetPickingRay());
                if (j != null)
                    documentManager.Object.ActiveDocument.Selection.Toggle(j);
            }
            else
            {
                var j = PickJoint(GetPickingRay());
                if (j != null)
                    documentManager.Object.ActiveDocument.Selection.SetSelected(j);
                else
                    documentManager.Object.ActiveDocument.Selection.Selected.Clear();
            }
        }
    }
}
