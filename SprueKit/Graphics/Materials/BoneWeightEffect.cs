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
    public class BoneWeightEffect : Effect, ICommonEffect
    {
        public BoneWeightEffect(GraphicsDevice device, ContentManager content) :
            base(content.Load<Effect>("Effects/BoneWeightsShader"))
        {
            CurrentTechnique = Techniques[0];
        }

        public int BoneIndex { get; set; } = -2;
        public Matrix WorldViewProjection { get; set; }
        public Matrix WorldView { get; set; }
        public Matrix Transform { get; set; } = Matrix.Identity;

        protected override void OnApply()
        {
            Parameters["WorldViewProjection"].SetValue(WorldViewProjection);
            Parameters["Transform"].SetValue(Transform);
            Parameters["SelectedBone"].SetValue(BoneIndex);
        }

        public void Begin(GraphicsDevice device) { }
        public void End(GraphicsDevice device) { }
    }
}
