using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace SprueKit.Graphics.Materials
{
    public class MatCapEffect : Effect, ICommonEffect
    {
        public MatCapEffect(GraphicsDevice device, ContentManager content) :
            base(content.Load<Effect>("Effects/MatcapShader"))
        {
            // load default matcap
            MatCap = content.Load<Texture2D>("Textures/Matcap/Matcap");
            CurrentTechnique = Techniques[0];
        }

        public Matrix WorldViewProjection { get; set; }
        public Matrix WorldView { get; set; }
        public Matrix Transform { get; set; } = Matrix.Identity;

        public Texture2D MatCap { get; set; }

        protected override void OnApply()
        {
            Parameters["WorldViewProjection"].SetValue(WorldViewProjection);
            Parameters["InverseWorldView"].SetValue(Matrix.Invert(WorldView));
            Parameters["MatCapTex"].SetValue(MatCap);
            Parameters["Transform"].SetValue(Transform);
        }

        public void Begin(GraphicsDevice device) { }
        public void End(GraphicsDevice device) { }
    }
}
