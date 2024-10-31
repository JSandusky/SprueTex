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
    public class WireFrameEffect : Effect, ICommonEffect
    {
        public WireFrameEffect(GraphicsDevice device, ContentManager content) :
            base(content.Load<Effect>("Effects/WireframeShader"))
        {
            CurrentTechnique = Techniques[0];
        }

        public Matrix WorldViewProjection { get; set; }
        public Matrix WorldView { get; set; }
        public Matrix Transform { get; set; } = Matrix.Identity;

        public Texture2D MatCap { get; set; }

        protected override void OnApply()
        {
            Parameters["WorldViewProjection"].SetValue(WorldViewProjection);
            Parameters["Transform"].SetValue(Transform);
            //CurrentTechnique = Techniques["Main"];
        }

        RasterizerState oldState_;
        RasterizerState state_;
        public void Begin(GraphicsDevice device)
        {
            oldState_ = device.RasterizerState;
            if (state_ == null)
                state_ = new RasterizerState { FillMode = FillMode.WireFrame };
            device.RasterizerState = state_;
        }

        public void End(GraphicsDevice device)
        {
            device.RasterizerState = oldState_; ;
        }
    }
}
