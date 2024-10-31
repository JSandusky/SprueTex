using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Windows;
using SprueKit.Graphics.Materials;

using MonoGame.Extended.BitmapFonts;

namespace SprueKit.Graphics.SprueModel
{
    public class UVScene : ViewportDelegate
    {
        static readonly Guid GUID = new Guid("9834d45e-5a5b-4b79-9c32-7783a53c4b38");

        static readonly DependencyProperty DrawingModeProperty = DependencyProperty.Register("DrawingMode",
            typeof(Graphics.Materials.UVRenderMode),
            typeof(UVScene), new PropertyMetadata(Graphics.Materials.UVRenderMode.Wireframe));

        public UVRenderMode DrawingMode
        {
            get { return (UVRenderMode)GetValue(DrawingModeProperty); }
            set { SetValue(DrawingModeProperty, value); }
        }

        IOCDependency<DocumentManager> documentManager = new IOCDependency<DocumentManager>();

        SprueKit.Data.SprueModel model_;
        private SprueKit.Graphics.DebugDraw debugDraw_;
        SprueKit.Graphics.DebugMesh debugMesh_;
        SprueKit.Graphics.Camera orthographicCamera_;
        Controllers.OrthographicCameraController orthoController_;
        SpriteFont font_;
        SpriteBatch batch_;

        UVNormalsEffect uvNormalsEffect_;
        UVWireframeEffect uvWireEffect_;


        public UVScene(BaseScene elem, SprueKit.Data.SprueModel model) : base(elem)
        {
            model_ = model;
        }

        public override void Initialize()
        {
            bool firstInit = firstInit_;
            base.Initialize();

            if (firstInit)
            {
                orthographicCamera_ = Graphics.Camera.CreateOrtho(GraphicsDevice);
                orthographicCamera_.Position = new Vector3(0.5f, 0.5f, 1);
                orthoController_ = new Controllers.OrthographicCameraController(scene_, this, orthographicCamera_);
                debugDraw_ = new DebugDraw(GraphicsDevice);
                debugMesh_ = new DebugMesh(GraphicsDevice);

                font_ = scene_.Content.Load<SpriteFont>("Fonts/Main12");
                batch_ = new SpriteBatch(GraphicsDevice);

                uvWireEffect_ = new UVWireframeEffect(GraphicsDevice, scene_.Content);
                uvNormalsEffect_ = new UVNormalsEffect(GraphicsDevice, scene_.Content);
            }
        }

        static Vector3[] BoxVerts =
        {
            new Vector3(0,0,0),
            new Vector3(0,1,0),
            new Vector3(1,1,0),
            new Vector3(1,0,0)
        };

        static Vector2[] BoxUV =
        {
            new Vector2(0, 0),
            new Vector2(0, 1),
            new Vector2(1, 1),
            new Vector2(1, 0)
        };

        static Color TransLimeGreen = new Color(50, 205, 50, 25);

        public override void Draw(GameTime time)
        {
            base.Draw(time);
            float frameTime = time.ElapsedGameTime.Milliseconds / 1000.0f;

            orthographicCamera_.UpdateAnimations(frameTime);
            orthoController_.Update(frameTime);

            orthographicCamera_.SetToOrthoGraphic(GraphicsDevice, 0, 0);

            if (model_ != null)
            {
                float currentX = 0;

                if (model_.MeshData != null)
                {
                    var indices = model_.MeshData.GetIndices();
                    if (DrawingMode == UVRenderMode.Normals)
                    {
                        uvNormalsEffect_.Begin(GraphicsDevice);
                        uvNormalsEffect_.OffsetScale = new Vector4(currentX, 0, 0, 0);
                        uvNormalsEffect_.WorldView = orthographicCamera_.CombinedMatrix;
                        uvNormalsEffect_.Transform = Matrix.Identity;
                        uvNormalsEffect_.Draw(model_.MeshData);
                        uvNormalsEffect_.End(GraphicsDevice);
                    }
                    else if (DrawingMode == UVRenderMode.Textured && model_.MeshData.Texture != null)
                    {
                        debugMesh_.Begin(orthographicCamera_.ViewMatrix, orthographicCamera_.ProjectionMatrix);
                        debugMesh_.BasicEffect.Texture = model_.MeshData.Texture.DiffuseTexture;
                        debugMesh_.BasicEffect.TextureEnabled = true;
                        debugMesh_.DrawTexturedQuad(BoxVerts[0], BoxVerts[1], BoxVerts[2], BoxVerts[3], BoxUV[0], BoxUV[1], BoxUV[2], BoxUV[3]);
                        debugMesh_.End();
                        debugMesh_.BasicEffect.TextureEnabled = false;
                    }
                    else if (DrawingMode == UVRenderMode.Wireframe)
                    {
                        uvWireEffect_.Begin(GraphicsDevice);
                        uvWireEffect_.OffsetScale = new Vector4(currentX, 0, 0, 0);
                        uvWireEffect_.WorldView = orthographicCamera_.CombinedMatrix;
                        uvWireEffect_.Transform = Matrix.Identity;
                        uvWireEffect_.Draw(model_.MeshData);
                        uvWireEffect_.End(GraphicsDevice);
                    }

                    debugDraw_.Begin(orthographicCamera_.ViewMatrix, orthographicCamera_.ProjectionMatrix);
                    string meshName = string.Format("Mesh {0}", 0);
                    debugDraw_.DrawLine(BoxVerts[0], BoxVerts[1], Color.LightGray);
                    debugDraw_.DrawLine(BoxVerts[1], BoxVerts[2], Color.LightGray);
                    debugDraw_.DrawLine(BoxVerts[2], BoxVerts[3], Color.LightGray);
                    debugDraw_.DrawLine(BoxVerts[0], BoxVerts[3], Color.LightGray);
                    debugDraw_.End();

                    Vector3 pos = GraphicsDevice.Viewport.Project(new Vector3(0, 0, 0), orthographicCamera_.ProjectionMatrix, orthographicCamera_.ViewMatrix, Matrix.Identity);
                    batch_.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null, null);
                    batch_.DrawString(base.font_, meshName, new Vector2((int)pos.X, (int)pos.Y), Color.White, 0, new Vector2(0,0), 1, SpriteEffects.None, 0);
                    batch_.End();

                    currentX += 1.2f;
                }

                List<Data.MeshData> hitMeshes = new List<Data.MeshData>();
                bool alternateText = false;
                model_.VisitChildren<Data.ModelPiece>(new Action<Data.ModelPiece>((Data.ModelPiece target) =>
                {
                    List<SprueKit.Data.MeshData> meshes = target.GetMeshes();
                    foreach (var mesh in meshes)
                    {
                        if (!hitMeshes.Contains(mesh) && mesh != null)
                        {
                            alternateText = !alternateText;
                            string meshName = System.IO.Path.GetFileName(target.ModelFile.ModelFile.ToString());

                            Vector3 offset = new Vector3(currentX, 0, 0);

                            var indices = mesh.GetIndices();

                            if (DrawingMode == UVRenderMode.Normals)
                            {
                                uvNormalsEffect_.Begin(GraphicsDevice);
                                uvNormalsEffect_.OffsetScale = new Vector4(currentX, 0, 0, 0);
                                uvNormalsEffect_.WorldView = orthographicCamera_.CombinedMatrix;
                                uvNormalsEffect_.Transform = Matrix.Identity;
                                uvNormalsEffect_.Draw(mesh);
                                uvNormalsEffect_.End(GraphicsDevice);
                            }
                            else if (DrawingMode == UVRenderMode.Textured && mesh.Texture != null)
                            {
                                debugMesh_.Begin(orthographicCamera_.ViewMatrix, orthographicCamera_.ProjectionMatrix);
                                debugMesh_.BasicEffect.Texture = mesh.Texture.DiffuseTexture;
                                debugMesh_.BasicEffect.TextureEnabled = true;
                                debugMesh_.DrawTexturedQuad(BoxVerts[0], BoxVerts[1], BoxVerts[2], BoxVerts[3], BoxUV[0], BoxUV[1], BoxUV[2], BoxUV[3]);
                                debugMesh_.End();
                                debugMesh_.BasicEffect.TextureEnabled = false;
                            }
                            else if (DrawingMode == UVRenderMode.Wireframe)
                            {
                                uvWireEffect_.Begin(GraphicsDevice);
                                uvWireEffect_.OffsetScale = new Vector4(currentX, 0, 0, 0);
                                uvWireEffect_.WorldView = orthographicCamera_.CombinedMatrix;
                                uvWireEffect_.Transform = Matrix.Identity;
                                uvWireEffect_.Draw(mesh);
                                uvWireEffect_.End(GraphicsDevice);
                            }

                            debugDraw_.Begin(orthographicCamera_.ViewMatrix, orthographicCamera_.ProjectionMatrix);

                            debugDraw_.DrawLine(BoxVerts[0] + offset, BoxVerts[1] + offset, Color.LightGray);
                            debugDraw_.DrawLine(BoxVerts[1] + offset, BoxVerts[2] + offset, Color.LightGray);
                            debugDraw_.DrawLine(BoxVerts[2] + offset, BoxVerts[3] + offset, Color.LightGray);
                            debugDraw_.DrawLine(BoxVerts[0] + offset, BoxVerts[3] + offset, Color.LightGray);

                            Color drawColor = DrawingMode != UVRenderMode.Wireframe ? TransLimeGreen : Color.LimeGreen;

                            //for (int i = 0; i < indices.Count; i += 3)
                            //{
                            //    var a = mesh.vertices[indices[i]];
                            //    var b = mesh.vertices[indices[i + 1]];
                            //    var c = mesh.vertices[indices[i + 2]];
                            //
                            //    debugDraw_.DrawLine(a.TextureCoordinate.ToVec3() + offset, b.TextureCoordinate.ToVec3() + offset, drawColor);
                            //    debugDraw_.DrawLine(b.TextureCoordinate.ToVec3() + offset, c.TextureCoordinate.ToVec3() + offset, drawColor);
                            //    debugDraw_.DrawLine(c.TextureCoordinate.ToVec3() + offset, a.TextureCoordinate.ToVec3() + offset, drawColor);
                            //}

                            debugDraw_.End();

                            Vector3 pos = GraphicsDevice.Viewport.Project(new Vector3(offset.X, alternateText ? 1.0f : 0.0f, 0), orthographicCamera_.ProjectionMatrix, orthographicCamera_.ViewMatrix, Matrix.Identity);
                            batch_.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearClamp, null, null, null, null);

                            if (alternateText)
                                pos.Y -= base.font_.MeasureString("T").Height;
                            batch_.DrawString(base.font_, meshName, new Vector2((int)pos.X, (int)pos.Y), Color.White, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0);
                            batch_.End();

                            hitMeshes.Add(mesh);

                            currentX += 1.2f;
                        }
                    }
                }));

                this.DrawApplicationMessage(GraphicsDevice, batch_, font_);
            }
        }

        public override Guid GetID()
        {
            return GUID;
        }

        public override string ViewportName { get { return "UV View"; } }
    }
}
