using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector4 = Microsoft.Xna.Framework.Vector4;

namespace PluginLib
{
    [Description("Attach this attribute to properties that should be exposed as pins on the graph.")]
    [AttributeUsage(AttributeTargets.Property)]
    public class TexturePinAttribute : System.Attribute
    {
        bool IsInput { get; set; } = true;
    }

    [Description("Interface will be provided to IMultisampleTextureFilter instances to perform sampling")]
    interface ITextureSampler
    {
        Vector4 Sample(Vector2 coord);
    }

    [Description("Implement this interface to expose custom objects for use in the shader graph. MUST BE THREAD SAFE!")]
    interface ITextureFilter
    {
        /// <summary>
        /// Performs a generic graph execute routine on the pixel
        /// </summary>
        /// <param name="coord"></param>
        [Description("Provided as { X, Y, PixelsWide, PixelsTall }, with XY normalized 0-1. Implementation must return a 4 component color as Vector4.")]
        Vector4 Execute(Vector4 coord);
    }

    [Description("Specialized version of the ITextureFilter interface for filters that need to sample a neighborhood of pixels. Can only use 1 INPUT PIN!")]
    interface IMultisampleTextureFilter
    {
        Vector4 Execute(Vector4 coord, ITextureSampler sampler);
    }
}
