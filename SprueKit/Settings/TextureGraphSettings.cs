using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit.Settings
{

    [EnumNames("64x64,128x128")]
    public enum TextureGraphPreviewResolution
    {
        [Description("64x64")]
        Small,
        [Description("128x128")]
        Large,
    }

    [Serializable]
    public class TextureGraphSettings
    {
        [Description("Resolution to use for previews in the graph node canvas, requires restart")]
        public TextureGraphPreviewResolution PreviewResolution { get; set; } = TextureGraphPreviewResolution.Small;

        [Description("Suffix to append to exported diffuse textures")]
        public string DiffuseTextureSuffix { get; set; } = "_d";
        [Description("Suffix to append to exported roughness textures")]
        public string RoughnessTextureSuffix { get; set; } = "_r";

        [Description("Suffix to append to exported glossiness textures")]
        public string GlossinessTextureSuffix { get; set; } = "_gloss";

        [Description("Suffix to append to exported specular color textures")]
        public string SpecularTextureSuffix { get; set; } = "_spec";

        [Description("Suffix to append to exported metalness textures")]
        public string MetallicTextureSuffix { get; set; } = "_m";

        [Description("Suffix to append to exported normal map textures")]
        public string NormalMapTextureSuffix { get; set; } = "_n";

        [Description("Suffix to append to exported displacement textures")]
        public string DisplacementTextureSuffix { get; set; } = "_disp";

        [Description("Suffix to append to exported subsurface color textures")]
        public string SubsurfaceColorTextureSuffix { get; set; } = "_subs";

        [Description("Suffix to append to exported subsurface depth/thickness textures")]
        public string SubsurfaceDepthTextureSuffix { get; set; } = "_depth";

        [Description("Suffix to append to exported ambient occlusion textures")]
        public string AmbientOcclusionTextureSuffix { get; set; } = "_ao";

        //[Description("Suffix to append to exported packed rough-metal textures")]
        //public string PackedRoughMetalSuffix { get; set; } = "_rm";

        //[Description("Roughness and Metalness will be packed into R and G channels of one texture")]
        //public bool PackRoughMetal { get; set; } = false;

        public string GetSuffix(Data.TextureChannel channel)
        {
            switch (channel)
            {
                case Data.TextureChannel.Diffuse:
                    return DiffuseTextureSuffix;
                case Data.TextureChannel.Roughness:
                    return RoughnessTextureSuffix;
                case Data.TextureChannel.Metallic:
                    return MetallicTextureSuffix;
                case Data.TextureChannel.NormalMap:
                    return NormalMapTextureSuffix;
                case Data.TextureChannel.Displacement:
                    return DisplacementTextureSuffix;
                case Data.TextureChannel.Glossiness:
                    return GlossinessTextureSuffix;
                case Data.TextureChannel.Specular:
                    return SpecularTextureSuffix;
                case Data.TextureChannel.SubsurfaceColor:
                    return SubsurfaceColorTextureSuffix;
                case Data.TextureChannel.SubsurfaceDepth:
                    return SubsurfaceDepthTextureSuffix;
                case Data.TextureChannel.AmbientOcclusion:
                    return AmbientOcclusionTextureSuffix;
            }
            return DiffuseTextureSuffix;
        }
    }
}
