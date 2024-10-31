using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit.Graphics.TexGraph
{
    /// <summary>
    /// Viewport is used for manipulating the contents of an arbitrary scene
    /// </summary>
    public class ReliefViewport : ViewportDelegate
    {
        IOCDependency<DocumentManager> documentManager = new IOCDependency<DocumentManager>();

        private SprueKit.Graphics.Materials.MatCapEffect matCapEffect_;

        Data.TexGen.ReliefBakeNode reliefNode_;
        Data.TexGen.TextureGenDocument document_;
        List<Data.TexGen.ModelNode> modelNodes_;

        protected Gizmo gizmo_;
        protected bool gizmoUsedMouse = false;
        protected Camera camera_;
        protected SprueKit.Graphics.Controllers.CameraController cameraController_;

        private SprueKit.Graphics.DebugDraw debugDraw;
        private SprueKit.Graphics.DebugMesh debugMesh;
        bool sizeChanged_ = true;

        public ReliefViewport(BaseScene scene, Data.TexGen.TextureGenDocument doc, Data.TexGen.ReliefBakeNode node) : base(scene)
        {
            document_ = doc;
            reliefNode_ = node;

            // Collect our upstream model nodes
            reliefNode_.TraceUpstream(new Action<Data.Graph.GraphNode, int>((n, i) =>
            {
                if (n is Data.TexGen.ModelNode)
                    modelNodes_.Add(n as Data.TexGen.ModelNode);
            }));

            scene_.MouseMove += (o, evt) =>
            {
                if (IsActive)
                    evt.Handled = false;
            };
            scene_.SizeChanged += (o, e) => {
                sizeChanged_ = true;
                //UpdateViewport();
            };
            scene_.MouseLeftButtonUp += MouseLeftButtonUp;
        }

        private void MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!IsActive)
                return;

            if (gizmoUsedMouse)
                return;
            var pt = e.GetPosition(scene_);
        }

        public override string ViewportName
        {
            get { return "Relief Scene Setup"; }
        }

        static Guid GUID = Guid.Parse("40ac008d-1e96-4fd1-a146-3701395dde1a");
        public override Guid GetID()
        {
            return GUID;
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public override void Draw(GameTime time)
        {
            if (!scene_.IsVisible)
                return;

            if (sizeChanged_)
                UpdateViewport();

            base.Draw(time);

            gizmoUsedMouse = false;
            float deltaTime = time.ElapsedGameTime.Milliseconds / 1000.0f;

            camera_.UpdateAnimations(deltaTime);

        // Line drawing
            debugDraw.Begin(camera_.ViewMatrix, camera_.ProjectionMatrix);
            GraphicsDevice.BlendState = BlendState.NonPremultiplied;

        // Floor grid
            debugDraw.DrawWireGrid(new Vector3(64, 0, 0), new Vector3(0, 0, 64), new Vector3(-32, 0, -32), 64, 64, new Color(45, 45, 45));

        // Axis Indicators
            Vector3 offset = Vector3.UnitY * 0.02f;
            debugDraw.DrawLine(Vector3.Zero + offset, Vector3.UnitX * 16 + offset, Color.Red);
            debugDraw.DrawLine(Vector3.Zero + offset, Vector3.UnitZ * 16 + offset, Color.CornflowerBlue);
            debugDraw.DrawLine(new Vector3(-2.0f, offset.Y, 1.0f * 12) + offset, Vector3.UnitZ * 16 + offset, Color.CornflowerBlue);
            debugDraw.DrawLine(new Vector3(2.0f, offset.Y, 1.0f * 12) + offset, Vector3.UnitZ * 16 + offset, Color.CornflowerBlue);
            debugDraw.End();

        // Gizmo Rendering
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            if (documentManager.Object.ActiveDocument != null)
            {
            }

        // Draw the models
            matCapEffect_.Begin(GraphicsDevice);
            foreach (var mdlNode in modelNodes_)
            {
                if (mdlNode.ModelFile != null)
                {
                    var meshes = mdlNode.ModelFile.GetMeshes();
                    if (meshes != null && meshes.Count > 0)
                    {
                        for (int i = 0; i < meshes.Count; ++i)
                            meshes[i].Draw(GraphicsDevice, matCapEffect_);
                    }
                }
            }
            matCapEffect_.End(GraphicsDevice);
        }

        void UpdateViewport()
        {
            if (camera_ != null && GraphicsDevice != null)
            {
                if (GraphicsDevice.Viewport.Width != (int)scene_.ActualWidth || GraphicsDevice.Viewport.Height != (int)scene_.ActualHeight)
                    GraphicsDevice.Viewport = new Viewport(0, 0, (int)scene_.ActualWidth, (int)scene_.ActualHeight, 0, 1);
                camera_.SetToPerspective(GraphicsDevice, 45);
                //orthoCamera.SetToOrthoGraphic(GraphicsDevice, 0, 0);
            }
            sizeChanged_ = false;
        }
    }
}
