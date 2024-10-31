using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Vector4 = Microsoft.Xna.Framework.Vector4;
using Color = Microsoft.Xna.Framework.Color;

namespace SprueKit.Data.TexGen
{
    public class SubstanceLoader
    {
        public SubstanceLoader(string file)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(file);

            var instances = doc.SelectNodes("//compInstance");
            var filters = doc.SelectNodes("//compFilter");
        }

        public static string ReadSBSFiles()
        {
            string basePath = "C:/dev/SubstanceDatabase";
            StringBuilder sb = new StringBuilder();
            RecurseSBSFiles(basePath, sb, new HashSet<string>());
            return sb.ToString();    
        }

        static void RecurseSBSFiles(string curDir, StringBuilder sb, HashSet<string> hasSet)
        {
            foreach (var file in System.IO.Directory.EnumerateFiles(curDir))
            {
                if (file.EndsWith(".sbs"))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(file);
                    var instances = doc.SelectNodes("//compInstance");
                    foreach (var inst in instances)
                    {
                        var val = ((XmlElement)inst).GetV("path/value");
                        if (!hasSet.Contains(val) && !val.Contains("dependency"))
                        {
                            sb.AppendLine(string.Format("{{ \"{0}\", null }}, ", val));
                            hasSet.Add(val);
                        }
                    }
                    var filters = doc.SelectNodes("//compFilter");
                    foreach (var filter in filters)
                    {
                        var val = ((XmlElement)filter).GetV("filter");
                        if (!hasSet.Contains(val))
                        {
                            sb.AppendLine(string.Format("{{ \"{0}\", null }}, ", val));
                            hasSet.Add(val);
                        }
                    }
                }
            }
            foreach (var dir in System.IO.Directory.EnumerateDirectories(curDir))
                RecurseSBSFiles(dir, sb, hasSet);
        }
    }

    public static class SBSExt
    {
        public static string GetV(this XmlElement elem, string name)
        {
            var nd = elem.SelectSingleNode(name);
            if (nd != null)
            {
                if (((XmlElement)nd).HasAttribute("v"))
                    return ((XmlElement)nd).GetAttribute("v");
            }
            return "";
        }

        public static string GetV(this XmlElement elem)
        {
            if (elem.HasAttribute("v"))
                return elem.GetAttribute("v");
            return "";
        }

        public static float GetIntParameter(this XmlElement elem, string name, float defVal)
        {
            var nd = elem.SelectSingleNode("//constantValueInt32");
            if (nd != null)
            {
                int.Parse((nd.SelectSingleNode("value") as XmlElement).GetAttribute("v"));
            }
            return defVal;
        }

        public static float GetFloatParameter(this XmlElement elem, string name, float defVal)
        {
            var nd = elem.SelectSingleNode("//constantValueFloat1");
            if (nd != null)
            {
                float.Parse((nd.SelectSingleNode("value") as XmlElement).GetAttribute("v"));
            }
            return defVal;
        }

        public static Vector2 GetVector2Parameter(this XmlElement elem, string name, Vector2 defVal)
        {
            var nd = elem.SelectSingleNode("//constantValueFloat2");
            if (nd != null)
            {
                float.Parse((nd.SelectSingleNode("value") as XmlElement).GetAttribute("v"));
            }
            return defVal;
        }

        public static Vector3 GetVector3Parameter(this XmlElement elem, string name, Vector3 defVal)
        {
            var nd = elem.SelectSingleNode("//constantValueFloat3");
            if (nd != null)
            {
                float.Parse((nd.SelectSingleNode("value") as XmlElement).GetAttribute("v"));
            }
            return defVal;
        }

        public static Vector4 GetVector2Parameter(this XmlElement elem, string name, Vector4 defVal)
        {
            var nd = elem.SelectSingleNode("//constantValueFloat4");
            if (nd != null)
            {
                float.Parse((nd.SelectSingleNode("value") as XmlElement).GetAttribute("v"));
            }
            return defVal;
        }

        public static Color GetColorParameter(this XmlElement elem, string name, Color defVal)
        {
            var nd = elem.SelectSingleNode("//constantValueFloat1");
            if (nd != null)
            {
                float.Parse((nd.SelectSingleNode("value") as XmlElement).GetAttribute("v"));
            }
            return defVal;
        }
    }

    public static class SBSNameTable
    {
        public static Dictionary<string, Type> Table = new Dictionary<string, Type>
        {
            { "warp", typeof(WarpModifier) },
            { "blend", typeof(BlendNode) },
            { "levels", typeof(LevelsFilterNode) },
            { "transformation", typeof(TransformModifier) },
            
            { "gradient", typeof(GradientRampTextureModifier) },

            { "pkg://Base_Filters/Blur/Blur_HQ_Grayscale", typeof(BlurModifier) },
            {"pkg://Base_Filters/Blur/Non_Uniform_Blur_Grayscale", typeof(BlurModifier) },

            { "pkg://Base_Elements/Noises/Moisture_Noise", null },
            { "pkg://Base_Elements/Noises/Crystal_2", null },
            { "pkg://Base_Elements/Noises/Noise_Zoom", null },
            { "pkg://Base_Elements/Noises/Cells", null },
            { "pkg://Base_Elements/Noises/Clouds", null },
            { "pkg://Base_Elements/Noises/Fractal_Sum_Base", typeof(FBMNoiseGenerator) },
            { "pkg://Base_Elements/Noises/Fractal_Sum_Base_4", typeof(FBMNoiseGenerator) },

            { "levels", typeof(LevelsFilterNode) },
            { "hsl", null },
            { "transformation", typeof(TransformModifier) },
            { "blend", null },
            { "gradient", null },
            { "warp", typeof(WarpModifier) },
            { "emboss", typeof(EmbossModifier) },
            { "blur", typeof(BlurModifier) },
            { "uniform", null },
            { "fxmaps", null },
            { "grayscaleconversion", null },
            { "directionalwarp", null },
            { "motionblur", null },
            { "shuffle", null },
            { "normal", null },
            { "sharpen", null },
            { "dirmotionblur", null },
            { "bitmap", null },
            { "svg", null },
            { "dyngradient", null },
            { "distance", null },

        };
    }
}
