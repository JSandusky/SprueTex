using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Framework.WpfInterop;
using MonoGame.Framework.WpfInterop.Input;

using SprueKit;
using SprueKit.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Windows;
using GongSolutions.Wpf.DragDrop;
using System.Collections.Generic;
using SprueKit.Data;

namespace SprueKit.Graphics.SprueModel
{
    /// <summary>
    /// Source: http://msdn.microsoft.com/en-us/library/bb203926(v=xnagamestudio.40).aspx
    /// Note that this is just an example implementation of <see cref="WpfGame"/>.
    /// </summary>
    public class ModelScene : ViewportDelegate, IDropTarget
    {
        static readonly Guid GUID = new Guid("5573f24c-8c0f-46c4-bb06-2d0afa11f2a0");

        IOCDependency<DocumentManager> documentManager = new IOCDependency<DocumentManager>();

        public static readonly DependencyProperty GizmoModeProperty = DependencyProperty.Register(
            "GizmoMode",
            typeof(GizmoMode),
            typeof(BaseScene),
            new PropertyMetadata(GizmoMode.Translation));

        public GizmoMode GizmoMode
        {
            get { return (GizmoMode)GetValue(GizmoModeProperty); }
            set
            {
                SetValue(GizmoModeProperty, value);
                if (gizmo_ != null)
                    gizmo_.State.Mode = value;
            }
        }

        public static readonly DependencyProperty GizmoWorldSpaceProperty = DependencyProperty.Register(
           "GizmoWorldSpace",
           typeof(bool),
           typeof(BaseScene),
           new PropertyMetadata(false));

        public bool GizmoWorldSpace
        {
            get { return (bool)GetValue(GizmoWorldSpaceProperty); }
            set
            {
                SetValue(GizmoWorldSpaceProperty, value);
                if (gizmo_ != null)
                    gizmo_.State.WorldSpace = value;
            }
        }

        public static readonly DependencyProperty RenderModeProperty = DependencyProperty.Register(
            "RenderMode",
            typeof(SprueKit.Graphics.Materials.RenderStyle),
            typeof(ModelScene), new PropertyMetadata(SprueKit.Graphics.Materials.RenderStyle.MatCap));
        public SprueKit.Graphics.Materials.RenderStyle RenderMode
        {
            get { return (SprueKit.Graphics.Materials.RenderStyle)GetValue(RenderModeProperty); }
            set { SetValue(RenderModeProperty, value); }
        }

        public static readonly DependencyProperty RenderTextureChannelProperty = DependencyProperty.Register(
            "RenderChannel",
            typeof(Materials.RenderTextureChannel), typeof(ModelScene), new PropertyMetadata(Materials.RenderTextureChannel.DiffuseOnly));
        public Materials.RenderTextureChannel RenderChannel
        {
            get { return (Materials.RenderTextureChannel)GetValue(RenderTextureChannelProperty); }
            set { SetValue(RenderTextureChannelProperty, value); }
        }

        public static readonly DependencyProperty DrawHelpersProperty = DependencyProperty.Register(
            "DrawHelpers",
            typeof(bool), typeof(ModelScene), new PropertyMetadata(true));
        public bool DrawHelpers {
            get { return (bool)GetValue(DrawHelpersProperty); }
            set { SetValue(DrawHelpersProperty, value); }
        }

        public static readonly DependencyProperty DrawBonesProperty = DependencyProperty.Register(
            "DrawBones",
            typeof(bool), typeof(ModelScene), new PropertyMetadata(true));
        public bool DrawBones
        {
            get { return (bool)GetValue(DrawBonesProperty); }
            set { SetValue(DrawBonesProperty, value); }
        }

        public static readonly DependencyProperty DrawControlPointsProperty = DependencyProperty.Register(
            "DrawControlPoints",
            typeof(bool), typeof(ModelScene), new PropertyMetadata(true));
        public bool DrawControlPoints
        {
            get { return (bool)GetValue(DrawControlPointsProperty); }
            set { SetValue(DrawControlPointsProperty, value); }
        }

        public static readonly DependencyProperty AnimateLightProperty = DependencyProperty.Register(
            "AnimateLight",
            typeof(bool), typeof(ModelScene), new PropertyMetadata(true));
        public bool AnimateLight { get { return (bool)GetValue(AnimateLightProperty); } set { SetValue(AnimateLightProperty, value); } }

        #region Fields

        private SprueKit.Graphics.Materials.WireFrameEffect wireFrameEffect_;
        private SprueKit.Graphics.Materials.MatCapEffect matCapEffect_;
        private SprueKit.Graphics.Materials.NormalEffect normalEffect_;
        private SprueKit.Graphics.Materials.UVChartEffect chartEffect_;
        private SprueKit.Graphics.Materials.BoneWeightEffect boneEffect_;
        private SprueKit.Graphics.Materials.TexturedEffect texturedEffect_;
        private SprueKit.Graphics.Materials.PBREffect pbrEffect_;

        protected Gizmo gizmo_;
        protected bool gizmoUsedMouse = false;
        protected Camera camera_;
        protected SprueKit.Graphics.Controllers.CameraController cameraController_;
        private BasicEffect _basicEffect;
        private Camera orthoCamera;
        private Matrix _worldMatrix;
        private bool _disposed;

        private SprueKit.Graphics.DebugDraw debugDraw;
        private SprueKit.Graphics.DebugMesh debugMesh;
        SprueKit.Graphics.DebugRenderer debugRender;
        SprueKit.Data.SprueModel model_;
        SpriteFont font_;
        SpriteBatch batch_;
        SelectionColorer selColorer;

        #endregion

        #region Properties

        public Camera Camera { get { return camera_; } }

        #endregion

        #region Methods

        public ModelScene(BaseScene scene, SprueKit.Data.SprueModel model) : base(scene)
        {
            GizmoMode = GizmoMode.Translation;
            model_ = model;

            scene_.MouseMove += (o, evt) =>
            {
                if (IsActive)
                    evt.Handled = false;
            };

            scene_.LayoutUpdated += (o, e) =>
            {
                //sizeChanged_ = true;
                //UpdateViewport();
            };
            scene_.SizeChanged += (o, e) => {
                sizeChanged_ = true;
                //UpdateViewport();
            };

            scene_.MouseLeftButtonUp += DemoScene_MouseLeftButtonUp;
        }

        bool sizeChanged_ = true;
        void UpdateViewport()
        {
            if (camera_ != null && GraphicsDevice != null)
            {
                if (GraphicsDevice.Viewport.Width != (int)scene_.ActualWidth || GraphicsDevice.Viewport.Height != (int)scene_.ActualHeight)
                    GraphicsDevice.Viewport = new Viewport(0, 0, (int)scene_.ActualWidth, (int)scene_.ActualHeight, 0, 1);
                camera_.SetToPerspective(GraphicsDevice, 45);
                orthoCamera.SetToOrthoGraphic(GraphicsDevice, 0, 0);
            }
            sizeChanged_ = false;
        }

        private void DemoScene_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!IsActive)
                return;

            if (gizmoUsedMouse)
                return;
            var pt = e.GetPosition(scene_);

            // CTRL click to toggle
            if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftCtrl) || System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftCtrl))
            {
                var hitPiece = SprueKit.Data.SceneHelpers.PickRay_After(model_, camera_.GetPickRay(GraphicsDevice.Viewport, (float)pt.X, (float)pt.Y), documentManager.Object.ActiveDocument.Selection.FirstSelectedOf<SprueKit.Data.SpruePiece>());
                if (hitPiece != null)
                    documentManager.Object.ActiveDocument.Selection.Toggle(hitPiece);
            }
            else
            {
                var hitPiece = SprueKit.Data.SceneHelpers.PickRay_After(model_, camera_.GetPickRay(GraphicsDevice.Viewport, (float)pt.X, (float)pt.Y), documentManager.Object.ActiveDocument.Selection.FirstSelectedOf<SprueKit.Data.SpruePiece>());
                if (hitPiece != null)
                    documentManager.Object.ActiveDocument.Selection.SetSelected(hitPiece);
                else
                    documentManager.Object.ActiveDocument.Selection.RemoveSelected<SprueKit.Data.SpruePiece>();
            }
        }

        private SprueKit.Data.SpruePiece HoverObject()
        {
            // Only check if mouse-over
            if (scene_.IsMouseOver)
            {
                var pt = System.Windows.Input.Mouse.GetPosition(scene_);
                return SprueKit.Data.SceneHelpers.PickRay_After(model_, camera_.GetPickRay(GraphicsDevice.Viewport, (float)pt.X, (float)pt.Y), documentManager.Object.ActiveDocument.Selection.FirstSelectedOf<SprueKit.Data.SpruePiece>());
            }
            return null;
        }

        public override void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _basicEffect.Dispose();
            debugDraw.Dispose();
            debugRender.Dispose();
            debugMesh.Dispose();

            wireFrameEffect_.Dispose();
            matCapEffect_.Dispose();
            normalEffect_.Dispose();
            chartEffect_.Dispose();
            boneEffect_.Dispose();
            texturedEffect_.Dispose();
            pbrEffect_.Dispose();
        }

        bool blockNextSelectionChange = false;
        Color background = new Color(33, 33, 33, 255);
        public override void Draw(GameTime time)
        {
            if (!scene_.IsVisible)
                return;

            if (sizeChanged_)
                UpdateViewport();

            debugRender.Begin();
            base.Draw(time);

            gizmoUsedMouse = false;
            float deltaTime = time.ElapsedGameTime.Milliseconds / 1000.0f;

            camera_.UpdateAnimations(deltaTime);

            _basicEffect.View = camera_.ViewMatrix;
            _basicEffect.Projection = camera_.ProjectionMatrix;

            SprueKit.Data.SprueModelDocument doc = documentManager.Object.ActiveDocument as SprueKit.Data.SprueModelDocument;
            if (doc != null)
            {
                var chosenEffect = GetChosenRenderEffect();
                chosenEffect.Begin(GraphicsDevice);
                chosenEffect.WorldViewProjection = camera_.CombinedMatrix;
                chosenEffect.WorldView = camera_.ViewMatrix;
                chosenEffect.Transform = Matrix.Identity;
                if (chosenEffect is Materials.TexturedEffect)
                    ((Materials.TexturedEffect)chosenEffect).ViewChannel = RenderChannel;
                if (doc.DataRoot.MeshData != null)
                {
                    doc.DataRoot.MeshData.Effect = (Effect)chosenEffect;
                    doc.DataRoot.MeshData.Draw(GraphicsDevice, camera_.ViewMatrix, camera_.ProjectionMatrix);
                }
                doc.DataRoot.Render(GraphicsDevice, (Effect)chosenEffect, camera_.ViewMatrix, camera_.ProjectionMatrix);

                chosenEffect.End(GraphicsDevice);
            }

        // Line drawing
            debugDraw.Begin(camera_.ViewMatrix, camera_.ProjectionMatrix);
            GraphicsDevice.BlendState = BlendState.NonPremultiplied;

        // Floor grid
            debugDraw.DrawWireGrid(new Vector3(64, 0, 0), new Vector3(0,0,64), new Vector3(-32,0,-32), 64, 64, new Color(45,45,45));

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
                SprueKit.Data.SpruePiece piece = documentManager.Object.ActiveDocument.Selection.MostRecentlySelected as SprueKit.Data.SpruePiece;
                if ((gizmo_ == null && piece != null && !piece.IsLocked) || // previously had no selection, or going after a locked object
                    (gizmo_ != null && gizmo_.State.GizmoObject != piece && piece != null && !blockNextSelectionChange && !piece.IsLocked)) // selection has been changed
                {
                    blockNextSelectionChange = false;
                    gizmo_ = new Gizmo(camera_, debugRender);
                    gizmo_.State.Mode = this.GizmoMode;
                    gizmo_.State.WorldSpace = this.GizmoWorldSpace;
                    gizmo_.State.GizmoObject = piece;
                    gizmo_.SetTransform(piece.Transform);
                    gizmo_.TransformChanged += (o, t) =>
                    {
                        (gizmo_.State.GizmoObject as Data.SpruePiece).Transform = t;
                    };
                    gizmo_.DoClone += (o, t) =>
                    {
                        if (gizmo_.State.GizmoObject is Data.SprueModel)
                            return;
                        if (gizmo_.State.GizmoObject is Data.ChainPiece.ChainBone)
                        {
                            var bone = gizmo_.State.GizmoObject as Data.ChainPiece.ChainBone;
                            var chain = bone.Parent as Data.ChainPiece;
                            Data.ChainPiece newChain = new Data.ChainPiece { Symmetric = Data.SymmetricAxis.XAxis, Parent = bone };
                            newChain.Bones.Add(new Data.ChainPiece.ChainBone { Parent = newChain, Position = bone.Position });
                            newChain.Bones.Add(new Data.ChainPiece.ChainBone { Parent = newChain, Position = bone.Position });
                            bone.Children.Add(newChain);
                            blockNextSelectionChange = true;
                            documentManager.Object.ActiveDocument.Selection.SetSelected(newChain.Bones[1]);
                            gizmo_.State.GizmoObject = newChain.Bones[1];
                            return;
                        }
                        var myPiece = gizmo_.State.GizmoObject as Data.SpruePiece;
                        var newPiece = myPiece.Clone();
                        newPiece.Parent = myPiece.Parent;
                        blockNextSelectionChange = true;
                        myPiece.Parent.GetAppropriateList(newPiece).Add(newPiece);
                        //documentManager.Object.ActiveDocument.Selection.SetSelected(newPiece);
                    };
                    piece.PropertyChanged += gizmo_.OnTransformChanged;
                }
                else if (gizmo_ != null && piece == null)
                    gizmo_ = null;

                if (gizmo_ != null)
                {
                    blockNextSelectionChange = false;
                    gizmo_.State.Mode = this.GizmoMode;
                    gizmo_.State.ShiftHeld = System.Windows.Input.Keyboard.Modifiers.HasFlag(System.Windows.Input.ModifierKeys.Shift);
                    gizmo_.State.AltHeld = System.Windows.Input.Keyboard.Modifiers.HasFlag(System.Windows.Input.ModifierKeys.Alt);
                    gizmo_.State.WorldSpace = this.GizmoWorldSpace;
                    gizmo_.State.MouseDown = scene_.IsMouseDirectlyOver && System.Windows.Input.Mouse.LeftButton == System.Windows.Input.MouseButtonState.Pressed;
                    gizmo_.State.ObjectBounds = new BoundingBox(new Vector3(-2, -2, -2), new Vector3(2, 2, 2));
                    var pt = System.Windows.Input.Mouse.GetPosition(scene_);
                    gizmo_.State.PickingRay = camera_.GetPickRay(GraphicsDevice.Viewport, (float)pt.X, (float)pt.Y);
                    //if (!scene_.IsMouseDirectlyOver)
                    //    gizmo_.State.PickingRay = null;

                    gizmo_.State.DeltaTime += deltaTime;
                    gizmoUsedMouse = gizmo_.Draw();
                }
            }

        // If we didn't use the gizmo, then display it
            if (cameraController_ != null)
                cameraController_.Update(time.ElapsedGameTime.Milliseconds / 1000.0f);

        // Draw our underlying debug geometry
            debugDraw.Begin(camera_.ViewMatrix, camera_.ProjectionMatrix);
            DrawPieceRecurse(debugDraw, model_, gizmoUsedMouse ? null : HoverObject());

            if (model_.MeshData != null && model_.MeshData.Skeleton != null && DrawBones)
            {
                // SprueModels have correct bone positions
                try
                {
                    DrawBoneRecurse(model_.MeshData.Skeleton.Root, Matrix.Identity);
                } catch (Exception ex) { }
            }

            debugDraw.End();

            // Draw our current message
            this.DrawApplicationMessage(GraphicsDevice, batch_, font_);

            // Depth-less drawing
            //GraphicsDevice.Clear(ClearOptions.DepthBuffer, Color.White, 1.0f, 0);
            //debugDraw.Begin(orthoCamera.ViewMatrix, orthoCamera.ProjectionMatrix);
            //debugDraw.DrawCylinder(Matrix.Identity, 3, 3, Color.Cyan);
            //debugDraw.End();

            var oldX = GraphicsDevice.Viewport.X;
            var oldY = GraphicsDevice.Viewport.Y;
            var oldW = GraphicsDevice.Viewport.Width;
            var oldH = GraphicsDevice.Viewport.Height;

            debugRender.Render(GraphicsDevice, camera_.CombinedMatrix);

            Viewport vpt = new Viewport { X = GraphicsDevice.Viewport.Width - 64, Y = GraphicsDevice.Viewport.Height - 64, Height = 64, Width = 64 };
            var oldViewport = GraphicsDevice.Viewport;
            GraphicsDevice.Viewport = vpt;
            Camera cm = new Camera(GraphicsDevice, 45);
            cm.Position = new Vector3(0, 0, 5);
            cm.LookAtPoint(Vector3.Zero);
            debugDraw.Begin(cm.ViewMatrix, cm.ProjectionMatrix);
            debugDraw.DrawLine(Vector3.Zero, Vector3.Transform(Vector3.UnitX, camera_.ViewMatrix.Rotation), Color.Red);
            debugDraw.DrawLine(Vector3.Zero, Vector3.Transform(Vector3.UnitZ, camera_.ViewMatrix.Rotation), Color.CornflowerBlue);
            debugDraw.DrawLine(Vector3.Zero, Vector3.Transform(Vector3.UnitY, camera_.ViewMatrix.Rotation), Color.Green);
            debugDraw.End();

            GraphicsDevice.Viewport = oldViewport;
        }

        void DrawPieceRecurse(DebugDraw draw, SprueKit.Data.SpruePiece piece, object hoverObj)
        {
            if (piece == null)
                return;
            if (piece.IsEnabled)
            {
                if (documentManager.Object.ActiveDocument == null)
                    return;

                if (documentManager.Object.ActiveDocument.Selection.Selected.Contains(piece))
                    piece.DebugDraw(debugRender, SprueKit.Data.DebugDrawMode.Selected);
                else if (piece == hoverObj)
                    piece.DebugDraw(debugRender, SprueKit.Data.DebugDrawMode.Hover);
                else if (DrawHelpers || (piece is Data.ChainPiece.ChainBone && DrawBones))
                    piece.DebugDraw(debugRender, SprueKit.Data.DebugDrawMode.Passive);

                if (piece is Data.ModelPiece)
                {
                    var model = piece as Data.ModelPiece;
                    List<MeshData> meshes = model.GetMeshes();
                    if (meshes != null && meshes.Count > 0)
                    {
                        foreach (var mesh in meshes)
                            if (mesh != null && mesh.Skeleton != null && DrawBones)
                                DrawBoneRecurse(mesh.Skeleton.Root, model.Transform); // ModelPieces do not have correct bone transforms
                    }
                }

                foreach (var child in piece.FlatChildren)
                    DrawPieceRecurse(draw, child as SprueKit.Data.SpruePiece, hoverObj);
            }
        }

        public override void Initialize()
        {
            bool firstInit = firstInit_;
            base.Initialize();

            _worldMatrix = Matrix.Identity;// Matrix.CreateRotationX(tilt) * Matrix.CreateRotationY(tilt);
            if (firstInit)
            {
                camera_ = new Camera(GraphicsDevice, 45);
                if (cameraController_ == null)
                    cameraController_ = new SprueKit.Graphics.Controllers.CameraController(scene_, this, camera_);
                else
                    cameraController_.camera = camera_;
                camera_.LookAtPoint(new Vector3(15, 15, 0), new Vector3(0, 5, 0));
                cameraController_.Focus();

                orthoCamera = new Camera(GraphicsDevice, 45);
                orthoCamera.SetToOrthoGraphic(GraphicsDevice, (float)scene_.ActualWidth, (float)scene_.ActualHeight);
                orthoCamera.Position = new Vector3(0, 15, -25);
                orthoCamera.LookAtPoint(new Vector3(0, 0, 0));

                _basicEffect = new BasicEffect(GraphicsDevice);
                _basicEffect.World = _worldMatrix;
                _basicEffect.View = camera_.ViewMatrix;
                _basicEffect.Projection = camera_.ProjectionMatrix;

                debugDraw = new SprueKit.Graphics.DebugDraw(GraphicsDevice);
                debugMesh = new SprueKit.Graphics.DebugMesh(GraphicsDevice);
                debugRender = new DebugRenderer();
                debugRender.SetEffect(new BasicEffect(GraphicsDevice) { LightingEnabled = false, VertexColorEnabled = true, TextureEnabled = false });
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

                if (_basicEffect.LightingEnabled)
                {
                    _basicEffect.DirectionalLight0.Enabled = true; // enable each light individually
                    if (_basicEffect.DirectionalLight0.Enabled)
                    {
                        // x direction
                        _basicEffect.DirectionalLight0.DiffuseColor = new Vector3(1, 0, 0); // range is 0 to 1
                        _basicEffect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(-1, 0, 0));
                        // points from the light to the origin of the scene
                        _basicEffect.DirectionalLight0.SpecularColor = Vector3.One;
                    }

                    _basicEffect.DirectionalLight1.Enabled = true;
                    if (_basicEffect.DirectionalLight1.Enabled)
                    {
                        // y direction
                        _basicEffect.DirectionalLight1.DiffuseColor = new Vector3(0, 0.75f, 0);
                        _basicEffect.DirectionalLight1.Direction = Vector3.Normalize(new Vector3(0, -1, 0));
                        _basicEffect.DirectionalLight1.SpecularColor = Vector3.One;
                    }

                    _basicEffect.DirectionalLight2.Enabled = true;
                    if (_basicEffect.DirectionalLight2.Enabled)
                    {
                        // z direction
                        _basicEffect.DirectionalLight2.DiffuseColor = new Vector3(0, 0, 0.5f);
                        _basicEffect.DirectionalLight2.Direction = Vector3.Normalize(new Vector3(0, 0, -1));
                        _basicEffect.DirectionalLight2.SpecularColor = Vector3.One;
                    }
                }
            }

            matCapEffect_ = new SprueKit.Graphics.Materials.MatCapEffect(GraphicsDevice, scene_.Content);
            normalEffect_ = new SprueKit.Graphics.Materials.NormalEffect(GraphicsDevice, scene_.Content);
            wireFrameEffect_ = new SprueKit.Graphics.Materials.WireFrameEffect(GraphicsDevice, scene_.Content);
            chartEffect_ = new SprueKit.Graphics.Materials.UVChartEffect(GraphicsDevice, scene_.Content);
            boneEffect_ = new Materials.BoneWeightEffect(GraphicsDevice, scene_.Content);
            texturedEffect_ = new Materials.TexturedEffect(GraphicsDevice, scene_.Content);
            pbrEffect_ = new Materials.PBREffect(GraphicsDevice, scene_.Content);
        }

        void DrawBoneRecurse(PluginLib.JointData joint, Matrix transform)
        {
            Color drawColor = documentManager.Object.ActiveDocument.Selection.IsSelected(joint) ? Color.Gold : Color.Cyan;
            if (!joint.HasChildren)
                debugDraw.DrawCross(Vector3.Transform(joint.Position, transform), 0.25f, drawColor);
            foreach (var child in joint.Children)
            {
                debugDraw.DrawLine(Vector3.Transform(joint.Position, transform), Vector3.Transform(child.Position, transform), drawColor);
                DrawBoneRecurse(child, transform);
            }
        }

        #endregion

        #region Drawing data sourcing functions

        SprueKit.Graphics.Materials.ICommonEffect GetChosenRenderEffect()
        {
            switch (this.RenderMode)
            {
                case SprueKit.Graphics.Materials.RenderStyle.Wireframe:
                    return wireFrameEffect_;
                case SprueKit.Graphics.Materials.RenderStyle.MatCap:
                    return matCapEffect_;
                case SprueKit.Graphics.Materials.RenderStyle.Normals:
                    {
                        normalEffect_.CamUp = camera_.Up;
                        normalEffect_.CamRight = camera_.Right;
                        return normalEffect_;
                    }
                case SprueKit.Graphics.Materials.RenderStyle.UVStretch:
                    return chartEffect_;
                case SprueKit.Graphics.Materials.RenderStyle.Textured:
                    {
                        switch (RenderChannel)
                        {
                            case Materials.RenderTextureChannel.PBRCombined:
                                {
                                    pbrEffect_.CameraPosition = camera_.Position;
                                    return pbrEffect_;
                                }
                            default:
                                return texturedEffect_;
                        }
                    }
                case SprueKit.Graphics.Materials.RenderStyle.BoneWeights:
                    {
                        if (documentManager.Object.ActiveDocument != null && documentManager.Object.ActiveDocument.Selection.MostRecentlySelected != null)
                        {
                            if (documentManager.Object.ActiveDocument.Selection.MostRecentlySelected is PluginLib.JointData)
                                boneEffect_.BoneIndex = ((PluginLib.JointData)documentManager.Object.ActiveDocument.Selection.MostRecentlySelected).Index;
                            else
                                boneEffect_.BoneIndex = -2;
                        }
                        else
                            boneEffect_.BoneIndex = -2;
                    }
                    return boneEffect_;
            }
            return wireFrameEffect_;
        }

        #endregion

        public override Guid GetID()
        {
            return GUID;
        }

        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Data is Data.FileData)
            {
                var data = dropInfo.Data as Data.FileData;
                string path = data.FilePath;
                if (path.EndsWith(".obj") || path.EndsWith(".fbx"))
                {
                    dropInfo.Effects = DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link;
                    return;
                }
            }
            else if (dropInfo.Data is DataObject)
            {
                if (((DataObject)dropInfo.Data).ContainsFileDropList())
                {
                    var data = dropInfo.Data as DataObject;
                    var paths = data.GetFileDropList();
                    if (paths.Count > 0)
                    {
                        var path = paths[0];
                        if (path.EndsWith(".obj") || path.EndsWith(".fbx"))
                        {
                            dropInfo.Effects = DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link;
                            return;
                        }
                    }
                }
            }
            dropInfo.Effects = DragDropEffects.None;
        }

        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Data is Data.FileData)
            {
                var data = dropInfo.Data as Data.FileData;
                string path = data.FilePath;
                if (path.EndsWith(".obj") || path.EndsWith(".fbx"))
                {
                    DropFile(new Uri(path), dropInfo.DropPosition);
                    return;
                }
            }
            else if (dropInfo.Data is DataObject)
            {
                if (((DataObject)dropInfo.Data).ContainsFileDropList())
                {
                    var data = dropInfo.Data as DataObject;
                    var paths = data.GetFileDropList();
                    if (paths.Count > 0)
                    {
                        var path = paths[0];
                        if (path.EndsWith(".obj") || path.EndsWith(".fbx"))
                        {
                            DropFile(new Uri(path), dropInfo.DropPosition);
                            return;
                        }
                    }
                }
            }
        }

        void DropFile(Uri path, System.Windows.Point pt)
        {
            Ray pickingRay = camera_.GetPickRay(GraphicsDevice.Viewport, (float)pt.X, (float)pt.Y);
            float hitDist = SprueKit.Data.SceneHelpers.PickRay_Distance(model_, pickingRay);
            Vector3 hitPt = Vector3.Zero;
            if (hitDist < 1000.0f)
                hitPt = (pickingRay.Position + (pickingRay.Direction * hitDist));

            Data.SpruePiece targetParent = model_;
            if (documentManager.Object.ActiveDocument.Selection.MostRecentlySelected != null)
            {
                var piece = documentManager.Object.ActiveDocument.Selection.MostRecentlySelected as Data.SpruePiece;
                if (piece != null)
                    targetParent = piece;
            }

            Data.ModelPiece model = new Data.ModelPiece { Parent = targetParent, Position = hitPt };
            model.ModelFile.ModelFile = path;
            targetParent.Children.Add(model);
        }

        public override string ViewportName { get { return "Model View"; } }
    }
}
