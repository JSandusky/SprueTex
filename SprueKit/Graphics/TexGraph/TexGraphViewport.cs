using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SprueKit.Graphics.TexGraph
{
    public class TexGraphViewport : ViewportDelegate
    {
        static readonly Guid GUID = new Guid("8320102d-8d6b-4d53-960f-629b036da229");

        static readonly DependencyProperty ShaderIndexProperty = DependencyProperty.Register(
            "ShaderIndex", typeof(int), typeof(TexGraphViewport), new PropertyMetadata(0));

        static readonly DependencyProperty PrimitiveProperty = DependencyProperty.Register(
            "Primitive", typeof(MeshPrimitiveType), typeof(TexGraphViewport), new PropertyMetadata(MeshPrimitiveType.Box));

        static readonly DependencyProperty AnimateLightProperty = DependencyProperty.Register(
            "AnimateLight", typeof(bool), typeof(TexGraphViewport), new PropertyMetadata(false));

        static readonly DependencyProperty LowLightProperty = DependencyProperty.Register(
            "LowLight", typeof(bool), typeof(TexGraphViewport), new PropertyMetadata(false));

        static readonly DependencyProperty UseHeightProperty = DependencyProperty.Register(
            "UseHeight", typeof(bool), typeof(TexGraphViewport), new PropertyMetadata(false));

        static readonly DependencyProperty SkyBoxIndexProperty = DependencyProperty.Register(
            "SkyBoxIndex", typeof(int), typeof(TexGraphViewport), new PropertyMetadata(0));

        public int ShaderIndex { get { return (int)GetValue(ShaderIndexProperty); } set { SetValue(ShaderIndexProperty, value); } }
        public MeshPrimitiveType Primitive { get { return (MeshPrimitiveType)GetValue(PrimitiveProperty); } set { SetValue(PrimitiveProperty, value); } }
        public bool AnimateLight { get { return (bool)GetValue(AnimateLightProperty); } set { SetValue(AnimateLightProperty, value); } }
        public bool LowLight { get { return (bool)GetValue(LowLightProperty); } set { SetValue(LowLightProperty, value); } }
        public bool UseHeight { get { return (bool)GetValue(UseHeightProperty); } set { SetValue(UseHeightProperty, value); } }
        public int SkyBoxIndex { get { return (int)GetValue(SkyBoxIndexProperty); } set { SetValue(SkyBoxIndexProperty, value); } }
        public Data.ForeignModel CustomModel { get; set; } = new Data.ForeignModel();

        public override string ViewportName { get { return "Texture Preview"; } }
        public override Guid GetID() { return GUID; }

        protected Camera camera_;
        protected SprueKit.Graphics.Controllers.CameraController cameraController_;
        VectorOscillator lightOscillator_ = new VectorOscillator(new Vector3(2, -4, -7), new Vector3(2, -4, 7));
        VectorOscillator lowLightOscillator_ = new VectorOscillator(new Vector3(2, 0, -7), new Vector3(2, 0, 7));
        SprueKit.Graphics.DebugDraw debugDraw_;
        SprueKit.Graphics.DebugMesh debugMesh_;
        SpriteBatch batch_;
        SpriteFont font_;
        SprueKit.Data.MeshData selectedMesh_ = null;
        Graphics.Materials.PBREffect pbrEffect_ = null;
        Materials.SkyboxMaterial skyBoxEffect_ = null;
        Texture2D[] textures_ = new Texture2D[11];
        TextureCube[] iblMaps_ = new TextureCube[3];
        float[] iblBrightness_ = new float[] { 0.5f, 0.4f, 0.3f };
        TextureCube[] skyMaps_ = new TextureCube[3];
        Data.TexGen.TextureGenDocument document_;

        static List<Data.MeshData> Meshes = new List<Data.MeshData>();

        public TexGraphViewport(Data.TexGen.TextureGenDocument doc, BaseScene scene) : base(scene)
        {
            scene_.SizeChanged += Scene__SizeChanged;
            document_ = doc;
        }

        private void Scene__SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            if (camera_ != null && GraphicsDevice != null)
            {
                if (GraphicsDevice.Viewport.Width != (int)scene_.ActualWidth || GraphicsDevice.Viewport.Height != (int)scene_.ActualHeight)
                    GraphicsDevice.Viewport = new Viewport(0, 0, (int)scene_.ActualWidth, (int)scene_.ActualHeight, 0, 1);
                camera_.SetToPerspective(GraphicsDevice, 45);
            }
        }

        #region Lifecycle
        public override void Initialize()
        {
            bool firstInit = firstInit_;
            base.Initialize();

            if (firstInit)
            {
                batch_ = new SpriteBatch(GraphicsDevice);
                font_ = scene_.Content.Load<SpriteFont>("Fonts/Main12");
                debugDraw_ = new DebugDraw(GraphicsDevice);
                debugMesh_ = new Graphics.DebugMesh(GraphicsDevice);
                pbrEffect_ = new Materials.PBREffect(GraphicsDevice, scene_.Content);
                pbrEffect_.LightDirection = Vector3.Normalize(new Vector3(0, -1, 1));
                skyBoxEffect_ = new Materials.SkyboxMaterial(GraphicsDevice, scene_.Content);
                // TODO: get a real skybox
                // TODO: use multiple material sources
                iblMaps_[0] = scene_.Content.Load<TextureCube>("Textures/IBL/DayIBL");
                iblMaps_[1] = scene_.Content.Load<TextureCube>("Textures/IBL/ForestIBL");
                iblMaps_[2] = scene_.Content.Load<TextureCube>("Textures/IBL/CityNightIBL");
                skyBoxEffect_.SkyBox = skyMaps_[0] = scene_.Content.Load<TextureCube>("Textures/IBL/Day");
                skyMaps_[1] = scene_.Content.Load<TextureCube>("Textures/IBL/Forest");
                skyMaps_[2] = scene_.Content.Load<TextureCube>("Textures/IBL/CityNight");

                camera_ = new Camera(GraphicsDevice, 45);
                if (cameraController_ == null)
                    cameraController_ = new SprueKit.Graphics.Controllers.CameraController(scene_, this, camera_);
                else
                    cameraController_.camera = camera_;
                cameraController_.AnimationDistanceMultipler = 0.05f;
                camera_.LookAtPoint(new Vector3(15, 15, 0), new Vector3(0, 5, 0));
                cameraController_.Focus();
            }
        }

        public override void Dispose()
        {
            batch_.Dispose();
            pbrEffect_.Dispose();
            skyBoxEffect_.Dispose();
            foreach (var map in skyMaps_)
                map.Dispose();
            foreach (var map in iblMaps_)
                map.Dispose();
            if (debugDraw_ != null)
                debugDraw_.Dispose();
            if (debugMesh_ != null)
                debugMesh_.Dispose();
            debugDraw_ = null;
            debugMesh_ = null;
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
            debugDraw_.Begin(camera_.ViewMatrix, camera_.ProjectionMatrix);
            GraphicsDevice.BlendState = BlendState.NonPremultiplied;
            // Floor grid
            if (document_.DrawGrid)
            {
                debugDraw_.DrawWireGrid(new Vector3(64, 0, 0), new Vector3(0, 0, 64), new Vector3(-32, 0, -32), 64, 64, new Color(10, 10, 10));
                // Axis Indicators
                Vector3 offset = Vector3.UnitY * 0.02f;
                debugDraw_.DrawLine(Vector3.Zero + offset, Vector3.UnitX * 16 + offset, Color.Red);
                debugDraw_.DrawLine(Vector3.Zero + offset, Vector3.UnitZ * 16 + offset, Color.CornflowerBlue);
                debugDraw_.DrawLine(new Vector3(-2.0f, offset.Y, 1.0f * 12) + offset, Vector3.UnitZ * 16 + offset, Color.CornflowerBlue);
                debugDraw_.DrawLine(new Vector3(2.0f, offset.Y, 1.0f * 12) + offset, Vector3.UnitZ * 16 + offset, Color.CornflowerBlue);
            }

            if (document_.DebugTangents)
                DrawTangentFrames(GraphicsDevice, debugDraw_);

            debugDraw_.End();

            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            skyBoxEffect_.SkyBox = skyMaps_[SkyBoxIndex];
            skyBoxEffect_.View = camera_.ViewMatrix;
            skyBoxEffect_.Projection = camera_.ProjectionMatrix;
            skyBoxEffect_.WorldViewProjection = camera_.ViewMatrix * camera_.ProjectionMatrix;
            skyBoxEffect_.CameraPosition = camera_.Position;
            Primitives.GetPrimitive(MeshPrimitiveType.Box).Effect = skyBoxEffect_;
            Primitives.GetPrimitive(MeshPrimitiveType.Box).Draw(GraphicsDevice, camera_.ViewMatrix, camera_.ProjectionMatrix);
            //Primitives.GetPrimitive(MeshPrimitiveType.Box).Draw(GraphicsDevice, skyBoxEffect_);
            GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;

            GraphicsDevice.BlendState = BlendState.NonPremultiplied;
            GraphicsDevice.RasterizerState = document_.FlipCulling ? RasterizerState.CullCounterClockwise : RasterizerState.CullClockwise;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            PreviewMesh.Effect = pbrEffect_;
            cameraController_.OrbitOrigin = PreviewMesh.Bounds.Centroid();
            pbrEffect_.Begin(GraphicsDevice);
            pbrEffect_.UVTiling = new Vector2(document_.UTiling, document_.VTiling);

            var lightOsc = lightOscillator_.Get(AnimateLight ? deltaTime : 0.0f);
            var lowLightOsc = lowLightOscillator_.Get(AnimateLight ? deltaTime : 0.0f);

            pbrEffect_.FlipCulling = document_.FlipCulling;
            pbrEffect_.LightDirection = Vector3.Normalize(LowLight ? lowLightOsc : lightOsc);
            pbrEffect_.WorldViewProjection = camera_.CombinedMatrix;
            pbrEffect_.WorldView = camera_.ViewMatrix;
            pbrEffect_.Transform = Matrix.CreateTranslation(0, 0.05f, 0);
            pbrEffect_.CameraPosition = camera_.Position;
            pbrEffect_.IBLMap = iblMaps_[SkyBoxIndex];
            pbrEffect_.Ambient = iblBrightness_[SkyBoxIndex];
            pbrEffect_.DiffuseTexture = textures_[(int)Data.TextureChannel.Diffuse];
            pbrEffect_.RoughnessTexture = textures_[(int)Data.TextureChannel.Roughness];
            pbrEffect_.MetalnessTexture = textures_[(int)Data.TextureChannel.Metallic];
            pbrEffect_.NormalMapTexture = textures_[(int)Data.TextureChannel.NormalMap];
            pbrEffect_.SpecularTexture = textures_[(int)Data.TextureChannel.Specular];
            pbrEffect_.GlossinessTexture = textures_[(int)Data.TextureChannel.Glossiness];
            pbrEffect_.AOTexture = textures_[(int)Data.TextureChannel.AmbientOcclusion];
            pbrEffect_.HeightTexture = textures_[(int)Data.TextureChannel.Displacement];
            pbrEffect_.SubsurfaceColorTexture = textures_[(int)Data.TextureChannel.SubsurfaceColor];
            pbrEffect_.SubsurfaceDepthTexture = textures_[(int)Data.TextureChannel.SubsurfaceDepth];
            pbrEffect_.EmissiveMaskTexture = textures_[(int)Data.TextureChannel.EmissiveMask];
            //pbrEffect_.ClearcoatTexture = textures_[(int)Data.TextureChannel.Clearcoat];

            pbrEffect_.CurrentTechnique = pbrEffect_.Techniques[TechniqueIndex];
            DrawPreviewMesh(GraphicsDevice, pbrEffect_, camera_.ViewMatrix, camera_.ProjectionMatrix, true);
            pbrEffect_.End(GraphicsDevice);

            if (cameraController_ != null)
                cameraController_.Update(time.ElapsedGameTime.Milliseconds / 1000.0f);

            // Draw our current message
            this.DrawApplicationMessage(GraphicsDevice, batch_, font_);
        }

        int TechniqueIndex { get
            {
                return ((int)ShaderIndex) + (UseHeight ? 4 : 0);
            }
        }

        Data.MeshData PreviewMesh
        {
            get
            {
                if (selectedMesh_ == null)
                    return Primitives.GetPrimitive(Primitive);
                return selectedMesh_;
            }
        }

        static Vector3 tangentFrameOffset = new Vector3(0.0f, 0.05f, 0.0f);
        void DrawTangentFrames(GraphicsDevice device, DebugDraw debugDraw)
        {
            if (Primitive != MeshPrimitiveType.Custom)
                PreviewMesh.DrawTangentFrames(debugDraw, tangentFrameOffset);
            else if (CustomModel.ModelData != null)
            {
                var meshes = CustomModel.GetMeshes();
                foreach (var mesh in meshes)
                    mesh.DrawTangentFrames(debugDraw, tangentFrameOffset);
            }
        }

        void DrawPreviewMesh(GraphicsDevice graphics, Effect effect, Matrix viewMat, Matrix projMat, bool leaveTextures)
        {
            if (Primitive != MeshPrimitiveType.Custom)
                PreviewMesh.Draw(graphics, viewMat, projMat, leaveTextures);
            else if (CustomModel.ModelData != null)
            {
                var meshes = CustomModel.GetMeshes();
                foreach (var mesh in meshes)
                {
                    mesh.Effect = effect;
                    mesh.Draw(graphics, viewMat, projMat, leaveTextures);
                }
            }
        }

        public void SetTexture(Data.TextureChannel channel, System.Drawing.Bitmap bmp)
        {
            int idx = (int)channel;
            if (textures_[idx] != null)
                textures_[idx].Dispose();
            textures_[idx] = bmp.BitmapToTexture2D(GraphicsDevice);
        }
    }
}
