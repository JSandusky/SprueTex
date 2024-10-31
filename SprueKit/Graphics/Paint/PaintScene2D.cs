using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit.Graphics.Paint
{
    public class PaintScene2D : PaintSceneBase
    {
        static Guid ViewportID = new Guid("6a894be2-b90f-43ea-b83a-0999e08c086b");

        SprueKit.Graphics.DebugDraw debugDraw_;
        SprueKit.Graphics.Camera orthographicCamera_;
        Controllers.OrthographicCameraController orthoController_;
        PaintTexture paintTexture_;
        Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch_;
        List<Stroke> strokeBatches_ = new List<Stroke>();
        StrokeBatch strokeBatch_;
        Vector2 brushPosition_ = Vector2.Zero;
        float brushRadius_ = 0.1f;
        System.Windows.Point brushPt_ = new System.Windows.Point();
        SpriteFont font_;

        Color[] strokeColors_ = new Color[]
        {
            Color.Red,
            Color.Yellow,
            Color.LimeGreen,
            Color.Magenta
        };

        public PaintScene2D(BaseScene scene) : base(scene)
        {
            scene.PreviewMouseMove += Scene_PreviewMouseMove;
            scene.MouseDown += Scene_MouseDown;
            scene.MouseUp += Scene_MouseUp;
            scene.MouseMove += Scene_MouseMove;
            scene.LostMouseCapture += Scene_LostMouseCapture;
            scene.PreviewMouseWheel += Scene_PreviewMouseWheel;
            scene.Cursor = System.Windows.Input.Cursors.Cross;
        }

        bool painting_ = false;
        private void Scene_LostMouseCapture(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!IsActive)
                return;
            EndPaint();
        }

        void EndPaint()
        {
            painting_ = false;
            if (stroke_.Count > 1)
                strokeBatches_.Add(stroke_);
            stroke_ = new Stroke();
        }

        private void Scene_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!IsActive)
                return;
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left && painting_)
            {
                scene_.ReleaseMouseCapture();
                EndPaint();
                e.Handled = true;
            }
        }

        private void Scene_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!IsActive)
                return;
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
            {
                stroke_ = new Stroke();
                if (strokeBatches_.Count == strokeColors_.Length * 2)
                    strokeBatches_.Clear();
                scene_.CaptureMouse();
                painting_ = true;
                e.Handled = true;
            }
        }

        public override void Activated()
        {
            scene_.Cursor = System.Windows.Input.Cursors.Cross;
            paintTexture_ = new PaintTexture(scene_.GraphicsDevice, 512, 512);
        }

        public override void Deactivated()
        {
            scene_.Cursor = null;
            paintTexture_.Dispose();
        }

        private void Scene_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (!IsActive)
                return;
            if (System.Windows.Input.Keyboard.Modifiers.HasFlag(System.Windows.Input.ModifierKeys.Control))
            {
                brushRadius_ += 0.01f * (e.Delta > 0 ? 1 : -1);
                brushRadius_ = Mathf.Clamp(brushRadius_, 0.01f, 1.0f);
                e.Handled = true;
            }
        }

        private void Scene_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!IsActive)
                return;
            brushPt_ = e.GetPosition(scene_);
        }

        Stroke stroke_ = new Stroke();
        private void Scene_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!IsActive)
                return;
            if (painting_)
            {
                Microsoft.Xna.Framework.Graphics.Viewport oldView = scene_.GraphicsDevice.Viewport;
                var pt = e.GetPosition(scene_);
                var viewPt = orthographicCamera_.GetViewportPos(GraphicsDevice.Viewport, (float)pt.X, (float)pt.Y);
                if (stroke_.Count == 0)
                {
                    stroke_.Add(new StrokePoint
                    {
                        Position = viewPt,
                        Color = strokeColors_[strokeBatches_.Count % 4],
                        Radius = brushRadius_
                    });
                }
                else
                {
                    if (Vector2.Distance(viewPt, stroke_.Last().Position) > brushRadius_*0.5f || stroke_.Count == 0)
                    {
                        stroke_.Add(new StrokePoint
                        {
                            Position = viewPt,
                            Color = strokeColors_[strokeBatches_.Count % 4],
                            Radius = brushRadius_
                        });
                    }
                }

                if (stroke_.Count > 2)
                {
                    //GraphicsDevice.SetRenderTarget(paintTexture_.RenderTarget);
                    //strokeBatch_.Draw(stroke_);
                    //GraphicsDevice.SetRenderTarget(null);
                }
                e.Handled = true;
            }
        }

        public override Guid GetID()
        {
            return ViewportID;
        }

        public override void Initialize()
        {
            bool firstInit = firstInit_;
            base.Initialize();

            if (firstInit)
            {
                orthographicCamera_ = Graphics.Camera.CreateOrtho(GraphicsDevice);
                orthographicCamera_.Position = new Vector3(0.5f, 0.5f, 1);
                orthoController_ = new Controllers.OrthographicCameraController(scene_, this, orthographicCamera_) { UseLeftToDrag = false };
                debugDraw_ = new DebugDraw(GraphicsDevice);
                strokeBatch_ = new StrokeBatch(GraphicsDevice, scene_.Content);
                spriteBatch_ = new Microsoft.Xna.Framework.Graphics.SpriteBatch(GraphicsDevice);
                font_ = scene_.Content.Load<SpriteFont>("Fonts/Main12");
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            if (debugDraw_ != null)
            { 
                debugDraw_.Dispose();
                debugDraw_ = null;
            }
            if (spriteBatch_ != null)
            {
                spriteBatch_.Dispose();
                spriteBatch_ = null;
            }
            if (strokeBatch_ != null)
            {
                strokeBatch_.Dispose();
                strokeBatch_ = null;
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

        public override void Draw(GameTime time)
        {
            base.Draw(time);
            float frameTime = time.ElapsedGameTime.Milliseconds / 1000.0f;

            orthographicCamera_.UpdateAnimations(frameTime);
            orthoController_.Update(frameTime);

            orthographicCamera_.SetToOrthoGraphic(GraphicsDevice, 0, 0);
            brushPosition_ = orthographicCamera_.GetViewportPos(GraphicsDevice.Viewport, (float)brushPt_.X, (float)brushPt_.Y);

            // draw our paint box outline
            debugDraw_.Begin(orthographicCamera_.ViewMatrix, orthographicCamera_.ProjectionMatrix);
            debugDraw_.DrawLine(BoxVerts[0], BoxVerts[1], Color.LightGray);
            debugDraw_.DrawLine(BoxVerts[1], BoxVerts[2], Color.LightGray);
            debugDraw_.DrawLine(BoxVerts[2], BoxVerts[3], Color.LightGray);
            debugDraw_.DrawLine(BoxVerts[0], BoxVerts[3], Color.LightGray);
            debugDraw_.DrawRing(new Vector3(brushPosition_.X, brushPosition_.Y, 0.0f), Vector3.UnitX*brushRadius_, Vector3.UnitY*brushRadius_, Color.CornflowerBlue);
            //if (stroke_.Count > 0)
            //{
            //    for (int i = 0; i < stroke_.Count-1; ++i)
            //    {
            //        var s = stroke_[i].Position;
            //        var t = stroke_[i+1].Position;
            //        debugDraw_.DrawLine(new Vector3(s.X, s.Y, 0.0f), new Vector3(t.X, t.Y, 0.0f), Color.Yellow);
            //    }
            //}
            debugDraw_.End();            

            UpdatePaintTexture();

            
            strokeBatch_.Effect.WorldViewProjection = orthographicCamera_.CombinedMatrix;

            int stID = 0;
            for (; stID < strokeBatches_.Count; ++stID)
            {
                strokeBatches_[stID].SetColor(strokeColors_[stID % 4]);
                strokeBatch_.Draw(strokeBatches_[stID]);
            }
            
            if (stroke_.Count > 1)    
                strokeBatch_.Draw(stroke_);
            //spriteBatch_.Begin(Microsoft.Xna.Framework.Graphics.SpriteSortMode.Immediate, null, null, null, null, null, orthographicCamera_.CombinedMatrix);
            //spriteBatch_.Draw(paintTexture_.RenderTarget, BoxVerts[0].XY(), null, Color.White, 0.0f, Vector2.Zero, Vector2.One, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0.0f);
            //spriteBatch_.End();

            this.DrawApplicationMessage(GraphicsDevice, spriteBatch_, font_);
        }

        void UpdatePaintTexture()
        {
            
        }
    }
}
