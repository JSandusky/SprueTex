using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit.Data
{
    public enum TextureChannel
    {
        Diffuse,
        // Rough-metal
        Roughness,
        Metallic,
        // Specular-glossiness
        Specular,
        Glossiness,
        NormalMap,
        // Effect
        SubsurfaceColor,
        SubsurfaceDepth,
        Displacement,
        AmbientOcclusion,
        EmissiveMask,
    }

    public enum TextureBlend
    {
        Overwrite,
        Additive,
        Subtractive,
        Multiply,
        Modulate,
    }

    public enum TexturePass
    {
        Base,
        Standard,
        Final,
    }

    public class Material : IDisposable
    {
        public static string[] TextureNames =
        {
            "diffuse",
            "normal",
            "roughness",
            "metallic",
            "displacement"
        };

        Texture2D diffuseTexture_;
        Texture2D normalMapTexture_;
        Texture2D roughnessTexture_;
        Texture2D metallicTexture_;
        Texture2D displacementTexture_;

        public SprueBindings.ImageData DiffuseData { get; set; }
        public SprueBindings.ImageData NormalMapData { get; set; }
        public SprueBindings.ImageData RoughnessData { get; set; }
        public SprueBindings.ImageData MetallicData { get; set; }
        public SprueBindings.ImageData DisplacementData { get; set; }

        public Texture2D DiffuseTexture { get { return diffuseTexture_; } private set { diffuseTexture_ = value; } }
        public Texture2D NormalMapTexture { get { return normalMapTexture_; } private set { normalMapTexture_ = value; } }
        public Texture2D RoughnessTexture { get { return roughnessTexture_; } private set { roughnessTexture_ = value; } }
        public Texture2D MetallicTexture { get { return metallicTexture_; } private set { metallicTexture_ = value; } }
        public Texture2D DisplacementTexture { get { return displacementTexture_; } private set { displacementTexture_ = value; } }

        public Material()
        {
        }

        public bool AnyNeedInitialization
        {
            get
            {
                return 
                    NeedsInit(DiffuseTexture, DiffuseData) ||
                    NeedsInit(NormalMapTexture, NormalMapData) ||
                    NeedsInit(RoughnessTexture, RoughnessData) ||
                    NeedsInit(MetallicTexture, MetallicData) ||
                    NeedsInit(DisplacementTexture, DisplacementData);
            }
        }

        public void SetTextureData(TextureChannel channel, SprueBindings.ImageData data)
        {
            switch (channel)
            {
                case TextureChannel.Diffuse:
                    if (DiffuseTexture != null) DiffuseTexture.Dispose();
                    DiffuseData = data;
                    break;
                case TextureChannel.Roughness:
                    if (RoughnessTexture != null) RoughnessTexture.Dispose();
                    RoughnessData = data;
                    break;
                case TextureChannel.Metallic:
                    if (MetallicTexture != null) MetallicTexture.Dispose();
                    MetallicData = data;
                    break;
                case TextureChannel.Displacement:
                    if (DisplacementTexture != null) DisplacementTexture.Dispose();
                    DisplacementData = data;
                    break;
            }
        }

        bool NeedsInit(Texture2D texture, SprueBindings.ImageData data)
        {
            if (texture == null && data != null)
                return true;
            if (texture != null && texture.IsDisposed)
                return true;
            return false;
        }

        public void Initialize(GraphicsDevice device)
        {
            if (NeedsInit(diffuseTexture_, DiffuseData))
                InitializeTexture(ref diffuseTexture_, DiffuseData, device);
            if (NeedsInit(normalMapTexture_, NormalMapData))
                InitializeTexture(ref normalMapTexture_, NormalMapData, device);
            if (NeedsInit(roughnessTexture_, RoughnessData))
                InitializeTexture(ref roughnessTexture_, RoughnessData, device);
            if (NeedsInit(metallicTexture_, MetallicData))
                InitializeTexture(ref metallicTexture_, MetallicData, device);
            if (NeedsInit(displacementTexture_, DisplacementData))
                InitializeTexture(ref displacementTexture_, DisplacementData, device);
            //if (Texture == null && ImageData != null)
            //{
            //    Texture = new Texture2D(device, ImageData.Width, ImageData.Height);
            //    Texture.SetData(ImageData.Pixels);
            //}
        }

        void InitializeTexture(ref Texture2D texture, SprueBindings.ImageData data, GraphicsDevice device)
        {
            if ((texture == null || texture.IsDisposed) && data != null)
            {
                texture = new Texture2D(device, data.Width, data.Height);
                texture.SetData(data.Pixels);
            }
        }

        public void Dispose()
        {
            DisposeTexture(DiffuseTexture);
            DisposeTexture(NormalMapTexture);
            DisposeTexture(RoughnessTexture);
            DisposeTexture(MetallicTexture);
            DisposeTexture(DisplacementTexture);
        }

        void DisposeTexture(Texture2D texture)
        {
            if (texture != null)
                texture.Dispose();
        }

        public void SaveImages(string intoFolder, string baseName)
        {
            if (DiffuseData != null)
                SaveImage(intoFolder, baseName, "diffuse", DiffuseData);
            if (NormalMapData != null)
                SaveImage(intoFolder, baseName, "normal", NormalMapData);
            if (RoughnessData != null)
                SaveImage(intoFolder, baseName, "roughness", RoughnessData);
            if (MetallicData != null)
                SaveImage(intoFolder, baseName, "metallic", MetallicData);
            if (DisplacementData != null)
                SaveImage(intoFolder, baseName, "displacement", DisplacementData);
        }

        void SaveImage(string intoFolder, string baseName, string name, SprueBindings.ImageData data)
        {
            string mixString = string.IsNullOrEmpty(baseName) ? "{0}.png" : "{1}_{0}.png";
            SprueBindings.ImageData.Save(System.IO.Path.Combine(intoFolder, string.Format(mixString, name, baseName)), data, ErrorHandler.inst());
        }
    }
}
