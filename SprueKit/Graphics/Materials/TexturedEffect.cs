using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit.Graphics.Materials
{
    class TexturedEffect : Effect, ICommonEffect
    {
        public TexturedEffect(GraphicsDevice device, ContentManager content) :
            base(content.Load<Effect>("Effects/TexturedShader"))
        {
            // load default matcap
            CurrentTechnique = Techniques[0];
        }

        public RenderTextureChannel ViewChannel { get; set; } = RenderTextureChannel.DiffuseOnly;
        public Matrix WorldViewProjection { get; set; }
        public Matrix WorldView { get; set; }
        public Matrix Transform { get; set; } = Matrix.Identity;

        public Texture2D DiffuseTexture { get; set; }

        protected override void OnApply()
        {
            Parameters["WorldViewProjection"].SetValue(WorldViewProjection);
            //Parameters["InverseWorldView"].SetValue(Matrix.Invert(WorldView));
            Parameters["DiffuseTex"].SetValue(DiffuseTexture);
            Parameters["Transform"].SetValue(Transform);
        }

        public void Begin(GraphicsDevice device) { }
        public void End(GraphicsDevice device) { }
    }
}
