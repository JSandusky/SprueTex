using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using MonoGame.Extended.BitmapFonts;

namespace SprueKit.Graphics
{
    public abstract class ViewportDelegate : FrameworkElement, IDisposable
    {
        protected Effect sdfFontEffect_;
        protected SpriteBatch spriteBatch_;
        protected MonoGame.Extended.BitmapFonts.BitmapFont font_;
        protected Texture2D background_;
        protected BaseScene scene_;
        protected bool firstInit_ = true;

        /// <summary>
        /// Implement to return unique identifier for finding this viewport
        /// </summary>
        public abstract Guid GetID();

        public abstract string ViewportName { get; }

        public GraphicsDevice GraphicsDevice { get; set; }

        public bool IsActive { get; set; }

        public ViewportDelegate(BaseScene scene)
        {
            scene_ = scene;
            GraphicsDevice = scene_.GraphicsDevice;
        }

        /// <summary>
        /// Called when rendering
        /// </summary>
        /// <param name="scene"></param>
        float time_;
        public virtual void Draw(GameTime scene)
        {
            DisposalQueue.Inst.Clear();

            time_ = (float)scene.ElapsedGameTime.Milliseconds / 100.0f;

            var curState = GraphicsDevice.DepthStencilState;
            spriteBatch_.Begin(SpriteSortMode.Deferred, null, SamplerState.LinearClamp);
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            spriteBatch_.Draw(background_, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);
            spriteBatch_.End();
            GraphicsDevice.DepthStencilState = curState;
        }

        public virtual void Initialize()
        {
            if (firstInit_)
            {
                GraphicsDevice = scene_.GraphicsDevice;
                background_ = scene_.Content.Load<Texture2D>("Textures/Background");
                font_ = scene_.Content.Load<MonoGame.Extended.BitmapFonts.BitmapFont>("Fonts/SegoeRegular");
                spriteBatch_ = new SpriteBatch(GraphicsDevice);
                sdfFontEffect_ = scene_.Content.Load<Effect>("Effects/SDFFontEffect");
            }

            firstInit_ = false;
        }

        public virtual void Activated()
        {

        }

        public virtual void Deactivated()
        {

        }

        public virtual void Dispose()
        {
            if (spriteBatch_ != null)
            {
                spriteBatch_.Dispose();
                spriteBatch_ = null;
            }

            if (sdfFontEffect_ != null)
            {
                sdfFontEffect_.Dispose();
                sdfFontEffect_ = null;
            }
        }

        public void DrawApplicationMessage(GraphicsDevice graphicsDevice, SpriteBatch batch, SpriteFont font)
        {
            App.RunWindowMessages(time_);
            if (App.WindowMessages.Count > 0)
            {
                batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp);

                int curY = graphicsDevice.Viewport.Height - 36;
                lock (App.WindowMessages)
                {
                    foreach (var msg in App.WindowMessages)
                    {
                        if (msg.Text.StartsWith("Failed:"))
                            batch.DrawString(font_, msg.Text, new Vector2(0, curY), Color.OrangeRed, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0);
                        else
                            batch.DrawString(font_, msg.Text, new Vector2(0, curY), Color.White, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0);
                        curY -= 32;
                    }
                }
                batch.End();
            }
        }
    }

    public class ViewportDelegateCollection : List<ViewportDelegate>
    {

        public ViewportDelegate Activate(int index)
        {
            if (this[index].IsActive)
                return this[index];

            foreach (var view in this)
            {
                if (view.IsActive)
                    view.Deactivated();
                view.IsActive = false;
            }

            this[index].Activated();
            this[index].IsActive = true;
            return this[index];
        }

        public ViewportDelegate Activate(Guid id)
        {
            // Ignore if changing to the current view
            foreach (var view in this)
                if (view.IsActive && view.GetID() == id)
                    return view;

            // Deactivate active view and set all views to inactive
            foreach (var view in this)
            {
                if (view.IsActive)
                    view.Deactivated();
                view.IsActive = false;
            }

            // Activate the appropriate view
            foreach (var view in this)
                if (view.GetID() == id)
                {
                    view.IsActive = true;
                    view.Activated();
                    return view;
                }

            // if we failed completely then return the first view
            return this[0];
        }

        
    }
}
