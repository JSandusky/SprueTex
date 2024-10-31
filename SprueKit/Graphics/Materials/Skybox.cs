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
    public class SkyboxMaterial : Effect, ICommonEffect
    {
        public SkyboxMaterial(GraphicsDevice device, ContentManager content) :
            base(content.Load<Effect>("Effects/Skybox"))
        {
            // load default matcap
            CurrentTechnique = Techniques[0];
        }

        public Matrix View { get; set; }
        public Matrix Projection { get; set; }
        public Matrix WorldViewProjection { get; set; }
        public TextureCube SkyBox { get; set; }
        public Vector3 CameraPosition { get; set; }

        public Matrix Transform
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public Matrix WorldView
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        protected override void OnApply()
        {
            Parameters["World"].SetValue(Matrix.CreateScale(250) * Matrix.CreateTranslation(CameraPosition));
            Parameters["View"].SetValue(View);
            Parameters["Projection"].SetValue(Projection);
            //Parameters["WorldViewProjection"].SetValue(WorldViewProjection);
            if (Parameters["SkyBoxTexture"] != null)
                Parameters["SkyBoxTexture"].SetValue(SkyBox);
            Parameters["CameraPosition"].SetValue(CameraPosition);
        }

        public void Begin(GraphicsDevice device)
        {
            
        }

        public void End(GraphicsDevice device)
        {
            
        }
    }
}
