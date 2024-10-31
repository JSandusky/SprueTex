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
    public class NormalEffect : Effect, ICommonEffect
    {
        public NormalEffect(GraphicsDevice device, ContentManager content) :
            base(content.Load<Effect>("Effects/NormalsShader"))
        {
            CurrentTechnique = Techniques[0];
        }

        public Matrix WorldViewProjection { get; set; }
        public Matrix WorldView { get; set; }
        public Matrix Transform { get; set; } = Matrix.Identity;
        public Vector3 CamForward { get; set; }
        public Vector3 CamUp { get; set; }
        public Vector3 CamRight { get; set; }

        public Texture2D MatCap { get; set; }

        protected override void OnApply()
        {
            Parameters["WorldViewProjection"].SetValue(WorldViewProjection);
            Parameters["Transform"].SetValue(Transform);
            Parameters["CamUp"].SetValue(CamUp);
            Parameters["CamRight"].SetValue(CamRight);
        }

        public void Begin(GraphicsDevice device) { }
        public void End(GraphicsDevice device) { }
    }
}
