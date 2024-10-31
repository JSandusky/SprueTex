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
    public class UVChartEffect : Effect, ICommonEffect
    {
        public UVChartEffect(GraphicsDevice device, ContentManager content) :
            base(content.Load<Effect>("Effects/TexCoordShader"))
        {
            // load default matcap
            UVChartImage = content.Load<Texture2D>("Textures/uvmap");
            CurrentTechnique = Techniques[0];
        }

        public Matrix Transform  { get; set; }
        public Matrix WorldView { get; set; }
        public Matrix WorldViewProjection { get; set; }
        public Texture2D UVChartImage { get; set; }

        protected override void OnApply()
        {
            Parameters["WorldViewProjection"].SetValue(WorldViewProjection);
            Parameters["UVChartTex"].SetValue(UVChartImage);
            Parameters["Transform"].SetValue(Transform);
        }

        public void Begin(GraphicsDevice device) { }

        public void End(GraphicsDevice device) { }
    }
}
