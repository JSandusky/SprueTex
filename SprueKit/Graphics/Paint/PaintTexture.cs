using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SprueKit.Graphics.Paint
{
    /// <summary>
    /// A render-target/bitmap that is blitted to via shaders
    /// Basically it's a C# port of Milton
    /// </summary>
    public class PaintTexture : IDisposable
    {
        RenderTarget2D target_;
        Viewport viewport_;
        int width_ = 64;
        int height_ = 64;

        public PaintTexture(GraphicsDevice device, int x, int y)
        {
            width_ = x;
            height_ = y;
            Create(device);
        }

        ~PaintTexture()
        {
            Dispose();
        }

        public void Dispose()
        {
            lock (this)
            {
                if (target_ != null)
                    target_.Dispose();
                target_ = null;
            }
        }

        public RenderTarget2D RenderTarget { get { return target_; } }
        public Viewport Viewport { get { return viewport_; } }

        void Create(GraphicsDevice device)
        {
            lock (this)
            {
                if (target_ != null)
                    target_.Dispose();
                viewport_ = new Viewport(0, 0, width_, height_);
                target_ = new RenderTarget2D(device, width_, height_, false, SurfaceFormat.Color, DepthFormat.None);
            }
        }

        public System.Drawing.Bitmap GetImage()
        {
            lock (this)
            {
                if (target_ == null)
                    return null;
                return ((Texture2D)target_).Texture2DToBitmap();
            }
        }

        // Blits this render-target into the destination target
        public void BlitInto(GraphicsDevice device, PaintTexture target)
        {

        }


    }
}
