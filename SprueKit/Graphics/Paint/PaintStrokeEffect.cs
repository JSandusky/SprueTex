using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace SprueKit.Graphics.Paint
{
    public class PaintStrokeEffect : Effect
    {
        public PaintStrokeEffect(GraphicsDevice device, ContentManager content) :
            base(content.Load<Effect>("Effects/StrokeShader"))
        {
            CurrentTechnique = Techniques[0];
        }

        public Matrix WorldViewProjection { get; set; }

        protected override void OnApply()
        {
            Parameters["WorldViewProjection"].SetValue(WorldViewProjection);
        }
    }
}
