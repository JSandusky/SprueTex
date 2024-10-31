using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit.Graphics.Materials
{
    public enum RenderStyle
    {
        Wireframe,
        MatCap,
        Normals,
        UVStretch,
        BoneWeights,
        Textured
    }

    public enum RenderTextureChannel
    {
        PBRCombined,
        DiffuseOnly,
        NormalMapOnly,
        RoughnessOnly,
        MetalnessOnly,
        HeightOnly
    }

    public interface ICommonEffect
    {
        Matrix Transform { get; set; }
        Matrix WorldViewProjection { get; set; }
        Matrix WorldView { get; set; }

        void Begin(GraphicsDevice device);
        void End(GraphicsDevice device);
    }
}
