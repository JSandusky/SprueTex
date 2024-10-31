using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Xml;

using Color = Microsoft.Xna.Framework.Color;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Vector4 = Microsoft.Xna.Framework.Vector4;

namespace SprueKit.Util
{
    public class HelpBuilder
    {
        HTMLBuilder builder_;
        public HelpBuilder()
        {
            builder_ = new HTMLBuilder("SprueTex Manual");
        }

        public string GetHTML()
        {
            builder_.Close();
            return builder_.HTMLText;
        }

        public static void BuildHTMLHelp()
        {
            {
                var docBuilder = new Util.HelpBuilder();
                docBuilder.WriteTypes("Basic Math Nodes", true, Data.TexGen.TextureGenDocument.NodeGroups[1].Types);
                System.IO.File.WriteAllText("BasicMathNodes.html", docBuilder.GetHTML());
            }
            {
                var docBuilder = new Util.HelpBuilder();
                docBuilder.WriteTypes("Generator Nodes", false, Data.TexGen.TextureGenDocument.NodeGroups[2].Types);
                System.IO.File.WriteAllText("GeneratorNodes.html", docBuilder.GetHTML());
            }
            {
                var docBuilder = new Util.HelpBuilder();
                docBuilder.WriteTypes("Color Nodes", true, Data.TexGen.TextureGenDocument.NodeGroups[3].Types);
                System.IO.File.WriteAllText("ColorNodes.html", docBuilder.GetHTML());
            }
            {
                var docBuilder = new Util.HelpBuilder();
                docBuilder.WriteTypes("Math Nodes", true, Data.TexGen.TextureGenDocument.NodeGroups[4].Types);
                System.IO.File.WriteAllText("MathNodes.html", docBuilder.GetHTML());
            }
            {
                var docBuilder = new Util.HelpBuilder();
                docBuilder.WriteTypes("Filter Nodes", true, Data.TexGen.TextureGenDocument.NodeGroups[5].Types);
                System.IO.File.WriteAllText("FilterNodes.html", docBuilder.GetHTML());
            }
            {
                var docBuilder = new Util.HelpBuilder();
                docBuilder.WriteTypes("Baker Nodes", true, Data.TexGen.TextureGenDocument.NodeGroups[5].Types);
                System.IO.File.WriteAllText("BakerNodes.html", docBuilder.GetHTML());
            }
            {
                var docBuilder = new Util.HelpBuilder();
                docBuilder.WriteTypes("Normal Map Nodes", true, Data.TexGen.TextureGenDocument.NodeGroups[7].Types);
                System.IO.File.WriteAllText("NormalMapNodes.html", docBuilder.GetHTML());
            }
        }

        public void WriteTypes(string typeGroup, bool filterMode, params Type[] types)
        {
            builder_.Anchor(typeGroup.Replace(" ", ""));
            builder_.Header(typeGroup, 2);
            foreach (var type in types)
            {
                builder_.Anchor(type.Name);
                builder_.Header(type.Name.SplitCamelCase(), 3);
                var desc = type.GetCustomAttribute<DescriptionAttribute>();
                if (desc != null)
                {
                    string descText = IncludePeriod(desc.Description);
                    builder_.P();
                    builder_.Text(descText);
                    if (type.GetCustomAttribute<PropertyData.NoPreviewsAttribute>() == null)
                    {
                        builder_.Br();
                        EmbedGIF(type, null, filterMode);
                    }
                    builder_.PopTag();
                }

                var properties = PropertyHelpers.GetOrdered(type).Where(p => p.Property.GetCustomAttribute<DescriptionAttribute>() != null);
                //var properties = type.GetProperties().Where(p => p.GetCustomAttribute<DescriptionAttribute>() != null);
                if (properties != null && properties.Count() > 0)
                {
                    builder_.DivIndent();
                    builder_.Table();
                    foreach (var propCache in properties)
                    {
                        var prop = propCache.Property;
                        builder_.Tr();
                            builder_.Td();
                                builder_.Bold();
                                    builder_.Text(prop.Name.SplitCamelCase());
                                    // Write out an animated GIF if we can
                                    if (CanEmbedGIF(type, prop) && type.GetCustomAttribute<PropertyData.NoPreviewsAttribute>() == null)
                                    {
                                        builder_.Br();
                                        EmbedGIF(type, prop, filterMode);
                                    }
                                builder_.PopTag();
                            builder_.PopTag();

                            builder_.Td();
                                builder_.P();
                                    builder_.Text(IncludePeriod(prop.GetCustomAttribute<DescriptionAttribute>().Description));                                    
                                builder_.PopTag();
                            builder_.PopTag();
                        builder_.PopTag();
                    }
                    builder_.PopTag();
                    builder_.PopTag();
                }
            }
        }

        public static string IncludePeriod(string input)
        {
            if (!input.EndsWith("."))
                return input + ".";
            return input;
        }

        bool CanEmbedGIF(Type forType, PropertyInfo pi)
        {
            if (typeof(Data.TexGen.TexGenNode).IsAssignableFrom(forType))
            {
                if (pi.PropertyType.IsEnum)
                    return true;
                else if (pi.PropertyType == typeof(bool))
                    return true;
                else if (pi.PropertyType == typeof(Color))
                    return true;
                return pi.GetCustomAttribute<PropertyData.ValidStepAttribute>() != null;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        public static List<System.Drawing.Bitmap> GenerateFrames(Type forType, PropertyInfo pi, bool filterMode)
        {
            var nd = Activator.CreateInstance(forType) as Data.TexGen.TexGenNode;
            Data.Graph.Graph junkGrp = new Data.Graph.Graph();

            junkGrp.AddNode(nd);
            nd.Construct();

            if (filterMode)
            {
                var sub = new Data.TexGen.FBMNoiseGenerator();
                sub.Construct();
                junkGrp.AddNode(sub);
                if (!junkGrp.Connect(sub.OutputSockets[0], nd.InputSockets[0]))
                    junkGrp.Connect(sub.OutputSockets[1], nd.InputSockets[0]);

                if (nd.InputSockets.Count > 1 && forType.GetCustomAttribute<PropertyData.HelpNoChannelsAttribute>() == null)
                {
                    var sub2 = new Data.TexGen.PerlinNoiseGenerator();
                    sub2.Construct();
                    junkGrp.AddNode(sub2);
                    junkGrp.Connect(sub2.OutputSockets[0], nd.InputSockets[1]);
                }

                if (nd.InputSockets.Count > 2 && forType.GetCustomAttribute<PropertyData.HelpNoChannelsAttribute>() == null)
                {
                    var sub3 = new Data.TexGen.VoronoiGenerator() { CellType = FastNoise.CellularReturnType.Distance2Mul };
                    sub3.Construct();
                    junkGrp.AddNode(sub3);
                    junkGrp.Connect(sub3.OutputSockets[0], nd.InputSockets[2]);
                }
            }

            List<System.Drawing.Bitmap> frames = new List<System.Drawing.Bitmap>();

            if (pi == null)
            {
                junkGrp.Prime(new Vector2(128,128));
                frames.Add(nd.GeneratePreview(128, 128));
                return frames;
            }

            var validStep = pi.GetCustomAttribute<PropertyData.ValidStepAttribute>();
            if (pi.PropertyType == typeof(float))
            {
                float step = validStep.Value;
                float iValue = (float)pi.GetValue(nd);
                for (int i = 0; i < 4; ++i)
                {
                    junkGrp.Prime(new Vector2(128, 128));
                    var bmp = nd.GeneratePreview(128, 128);
                    if (bmp != null)
                        DrawTextOnBitmap(bmp, iValue.ToString());
                    frames.Add(bmp);
                    iValue += step;
                    pi.SetValue(nd, iValue);
                }
            }
            else if (pi.PropertyType == typeof(int))
            {
                float step = validStep.Value;
                int iValue = (int)pi.GetValue(nd);
                for (int i = 0; i < 4; ++i)
                {
                    junkGrp.Prime(new Vector2(128, 128));
                    var bmp = nd.GeneratePreview(128, 128);
                    if (bmp != null)
                        DrawTextOnBitmap(bmp, iValue.ToString());
                    frames.Add(bmp);
                    iValue += (int)step;
                    pi.SetValue(nd, iValue);
                }
            }
            else if (pi.PropertyType == typeof(Vector2))
            {
                float step = validStep.Value;
                Vector2 iValue = (Vector2)pi.GetValue(nd);
                for (int i = 0; i < 4; ++i)
                {
                    junkGrp.Prime(new Vector2(128, 128));
                    var bmp = nd.GeneratePreview(128, 128);
                    if (bmp != null)
                        DrawTextOnBitmap(bmp, iValue.ToString());
                    frames.Add(bmp);
                    iValue.X += step;
                    iValue.Y += step;
                    pi.SetValue(nd, iValue);
                }
            }
            else if (pi.PropertyType == typeof(Vector3))
            {
                float step = validStep.Value;
                Vector3 iValue = (Vector3)pi.GetValue(nd);
                for (int i = 0; i < 4; ++i)
                {
                    junkGrp.Prime(new Vector2(128, 128));
                    var bmp = nd.GeneratePreview(128, 128);
                    if (bmp != null)
                        DrawTextOnBitmap(bmp, iValue.ToString());
                    frames.Add(bmp);
                    iValue.X += step;
                    iValue.Y += step;
                    iValue.Z += step;
                    pi.SetValue(nd, iValue);
                }
            }
            else if (pi.PropertyType == typeof(Vector4))
            {
                float step = validStep.Value;
                Vector4 iValue = (Vector4)pi.GetValue(nd);
                for (int i = 0; i < 4; ++i)
                {
                    junkGrp.Prime(new Vector2(128, 128));
                    var bmp = nd.GeneratePreview(128, 128);
                    if (bmp != null)
                        DrawTextOnBitmap(bmp, iValue.ToString());
                    frames.Add(bmp);
                    iValue.X += step;
                    iValue.Y += step;
                    iValue.Z += step;
                    iValue.W += step;
                    pi.SetValue(nd, iValue);
                }
            }
            else if (pi.PropertyType == typeof(Color))
            {
                Color[] colValues = new Color[] { Color.CornflowerBlue, Color.Crimson, Color.DarkGoldenrod };
                string[] colNames = new string[] { "Blue", "Crimson", "Goldenrod" };
                for (int b = 0; b < 3; ++b)
                {
                    pi.SetValue(nd, colValues[b]);
                    junkGrp.Prime(new Vector2(128, 128));
                    var bmp = nd.GeneratePreview(128, 128);
                    if (bmp != null)
                        DrawTextOnBitmap(bmp, colNames[b]);
                    frames.Add(bmp);
                }
            }
            else if (pi.PropertyType.IsEnum)
            {
                var enumValues = Enum.GetValues(pi.PropertyType);
                foreach (var val in enumValues)
                {
                    pi.SetValue(nd, val);
                    junkGrp.Prime(new Vector2(128, 128));
                    var bmp = nd.GeneratePreview(128, 128);
                    if (bmp != null)
                        DrawTextOnBitmap(bmp, val.ToString());
                    frames.Add(bmp);
                }
            }
            else if (pi.PropertyType == typeof(bool))
            {
                for (int i = 0; i < 2; ++i)
                {
                    pi.SetValue(nd, i == 0);
                    junkGrp.Prime(new Vector2(128, 128));
                    var bmp = nd.GeneratePreview(128, 128);
                    if (bmp != null)
                        DrawTextOnBitmap(bmp, (i == 0) ? "On" : "Off");
                    frames.Add(bmp);
                }
            }

            return frames;
        }

        public static List<System.Drawing.Bitmap> GenerateInstanceFrames(Data.Graph.Graph graph, Data.TexGen.TexGenNode instance, Type forType, PropertyInfo pi, int dim)
        {
            List<System.Drawing.Bitmap> frames = new List<System.Drawing.Bitmap>();

            var junkGrp = graph;
            var nd = instance;

            if (pi == null)
            {
                junkGrp.Prime(new Vector2(dim, dim));
                frames.Add(nd.GeneratePreview(dim, dim));
                return frames;
            }

            var validStep = pi.GetCustomAttribute<PropertyData.ValidStepAttribute>();
            if (pi.PropertyType == typeof(float))
            {
                float step = validStep.Value;
                float iValue = (float)pi.GetValue(nd);
                for (int i = 0; i < 4; ++i)
                {
                    junkGrp.Prime(new Vector2(dim, dim));
                    var bmp = nd.GeneratePreview(dim, dim);
                    if (bmp != null)
                        DrawTextOnBitmap(bmp, iValue.ToString());
                    frames.Add(bmp);
                    iValue += step;
                    pi.SetValue(nd, iValue);
                }
            }
            else if (pi.PropertyType == typeof(int))
            {
                float step = validStep.Value;
                int iValue = (int)pi.GetValue(nd);
                for (int i = 0; i < 4; ++i)
                {
                    junkGrp.Prime(new Vector2(dim, dim));
                    var bmp = nd.GeneratePreview(dim, dim);
                    if (bmp != null)
                        DrawTextOnBitmap(bmp, iValue.ToString());
                    frames.Add(bmp);
                    iValue += (int)step;
                    pi.SetValue(nd, iValue);
                }
            }
            else if (pi.PropertyType == typeof(Vector2))
            {
                float step = validStep.Value;
                Vector2 iValue = (Vector2)pi.GetValue(nd);
                for (int i = 0; i < 4; ++i)
                {
                    junkGrp.Prime(new Vector2(dim, dim));
                    var bmp = nd.GeneratePreview(dim, dim);
                    if (bmp != null)
                        DrawTextOnBitmap(bmp, iValue.ToString());
                    frames.Add(bmp);
                    iValue.X += step;
                    iValue.Y += step;
                    pi.SetValue(nd, iValue);
                }
            }
            else if (pi.PropertyType == typeof(Vector3))
            {
                float step = validStep.Value;
                Vector3 iValue = (Vector3)pi.GetValue(nd);
                for (int i = 0; i < 4; ++i)
                {
                    junkGrp.Prime(new Vector2(dim, dim));
                    var bmp = nd.GeneratePreview(dim, dim);
                    if (bmp != null)
                        DrawTextOnBitmap(bmp, iValue.ToString());
                    frames.Add(bmp);
                    iValue.X += step;
                    iValue.Y += step;
                    iValue.Z += step;
                    pi.SetValue(nd, iValue);
                }
            }
            else if (pi.PropertyType == typeof(Vector4))
            {
                float step = validStep.Value;
                Vector4 iValue = (Vector4)pi.GetValue(nd);
                for (int i = 0; i < 4; ++i)
                {
                    junkGrp.Prime(new Vector2(dim, dim));
                    var bmp = nd.GeneratePreview(dim, dim);
                    if (bmp != null)
                        DrawTextOnBitmap(bmp, iValue.ToString());
                    frames.Add(bmp);
                    iValue.X += step;
                    iValue.Y += step;
                    iValue.Z += step;
                    iValue.W += step;
                    pi.SetValue(nd, iValue);
                }
            }
            else if (pi.PropertyType == typeof(Color))
            {
                Color[] colValues = new Color[] { Color.CornflowerBlue, Color.Crimson, Color.DarkGoldenrod };
                string[] colNames = new string[] { "Blue", "Crimson", "Goldenrod" };
                for (int b = 0; b < 3; ++b)
                {
                    pi.SetValue(nd, colValues[b]);
                    junkGrp.Prime(new Vector2(dim, dim));
                    var bmp = nd.GeneratePreview(dim, dim);
                    if (bmp != null)
                        DrawTextOnBitmap(bmp, colNames[b]);
                    frames.Add(bmp);
                }
            }
            else if (pi.PropertyType.IsEnum)
            {
                var enumValues = Enum.GetValues(pi.PropertyType);
                var curValue = pi.GetValue(nd);
                foreach (var val in enumValues)
                {
                    pi.SetValue(nd, val);
                    junkGrp.Prime(new Vector2(dim, dim));
                    var bmp = nd.GeneratePreview(dim, dim);
                    if (bmp != null)
                        DrawTextOnBitmap(bmp, val.ToString());
                    frames.Add(bmp);
                }
                pi.SetValue(nd, curValue);
            }
            else if (pi.PropertyType == typeof(bool))
            {
                for (int i = 0; i < 2; ++i)
                {
                    pi.SetValue(nd, i == 0);
                    junkGrp.Prime(new Vector2(dim, dim));
                    var bmp = nd.GeneratePreview(dim, dim);
                    if (bmp != null)
                        DrawTextOnBitmap(bmp, (i == 0) ? "On" : "Off");
                    frames.Add(bmp);
                }
            }

            return frames;
        }

        void EmbedGIF(Type forType, PropertyInfo pi, bool filterMode)
        {
            List<System.Drawing.Bitmap> frames = GenerateFrames(forType, pi, filterMode);

            if (frames.Count == 1)
                builder_.ImgEmbedded(frames[0], 128);
            else if (frames.Count > 1)
            {
                string base64 = GetGIFData(frames.ToArray());
                builder_.EmbedGIF(base64);
            }
        }

        // Transform a bunch of bitmap images into a GIF animation
        string GetGIFData(params System.Drawing.Bitmap[] frames)
        {
            using (var strm = new System.IO.MemoryStream())
            {
                using (var gifWriter = new GifWriter(strm, 800, 0))
                    foreach (var bmp in frames)
                        gifWriter.WriteFrame(bmp);
                return Convert.ToBase64String(strm.ToArray());
            }
        }

        static System.Drawing.SolidBrush textBrush_;
        static System.Drawing.SolidBrush bgBrush_;
        static void DrawTextOnBitmap(System.Drawing.Bitmap bmp, string text)
        {
            if (textBrush_ == null)
            {
                textBrush_ = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(50, 255, 50));
                bgBrush_ = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(30, 30, 30));
            }

            System.Drawing.RectangleF rectf = new System.Drawing.RectangleF(5, 5, bmp.Width - 5, bmp.Height-5);
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

            var font = new System.Drawing.Font("Tahoma", 8);
            var textMeasure = g.MeasureString(text, font);

            g.FillRectangle(bgBrush_, new System.Drawing.RectangleF(5, 5, textMeasure.Width, textMeasure.Height));
            g.DrawString(text, font, System.Drawing.Brushes.WhiteSmoke, rectf);

            g.Flush();
        }
    }

    public class DocHelpBuilder
    {
        XmlDocument doc_;
        XmlElement root_;

        string outputFile_;
        public DocHelpBuilder(string outputFile)
        {
            outputFile_ = outputFile;
            doc_ = new XmlDocument();
            root_ = doc_.CreateElement("help");
            doc_.AppendChild(root_);
        }

        public static void BuildInternalHelp()
        {
            var docBuilder = new DocHelpBuilder("Guide.xml");
            docBuilder.WriteTypes("Basic Math Nodes", true, Data.TexGen.TextureGenDocument.NodeGroups[1].Types);
            docBuilder.WriteTypes("Generator Nodes", false, Data.TexGen.TextureGenDocument.NodeGroups[2].Types);
            docBuilder.WriteTypes("Color Nodes", true, Data.TexGen.TextureGenDocument.NodeGroups[3].Types);
            docBuilder.WriteTypes("Math Nodes", true, Data.TexGen.TextureGenDocument.NodeGroups[4].Types);
            docBuilder.WriteTypes("Filter Nodes", true, Data.TexGen.TextureGenDocument.NodeGroups[5].Types);
            //docBuilder.WriteTypes("Baker Nodes", true, Data.TexGen.TextureGenDocument.NodeGroups[6].Types);
            docBuilder.WriteTypes("Normal Map Nodes", true, Data.TexGen.TextureGenDocument.NodeGroups[7].Types);
            docBuilder.Save();
        }

        public void Save()
        {
            doc_.Save(outputFile_);
        }

        public void WriteTypes(string typeGroup, bool filterMode, params Type[] types)
        {
            XmlElement sectionElem = root_.CreateChild("section");
            sectionElem.SetAttribute("name", typeGroup);

            var toc = sectionElem.CreateChild("toc");
            foreach (var type in types)
            {
                var tocElem = toc.CreateChild("item");
                tocElem.InnerText = type.Name.SplitCamelCase();
                tocElem.SetAttribute("key", type.Name.SplitCamelCase());
            }

            foreach (var type in types)
            {
                var header = sectionElem.CreateChild("h");
                header.SetAttribute("size", "16");
                header.InnerText = string.Format("[b]{0}[/b]", type.Name.SplitCamelCase());
                header.SetAttribute("key", type.Name.SplitCamelCase());

                var desc = type.GetCustomAttribute<DescriptionAttribute>();
                if (desc != null)
                {
                    var descElem = sectionElem.CreateChild("p");
                    descElem.InnerText = HelpBuilder.IncludePeriod(desc.Description);
                }

                // Generate main preview image?
                if (type.GetCustomAttribute<PropertyData.NoPreviewsAttribute>() == null)
                {
                    EmbedGIF(sectionElem, type, null, filterMode);
                }

                var properties = PropertyHelpers.GetOrdered(type).Where(p => p.Property.GetCustomAttribute<DescriptionAttribute>() != null);
                if (properties != null && properties.Count() > 0)
                {
                    var subSectionElem = sectionElem.CreateChild("sub");

                    foreach (var propCache in properties)
                    {
                        var property = propCache.Property;
                        var propElem = subSectionElem.CreateChild("p");
                        var propDesc = property.GetCustomAttribute<DescriptionAttribute>();
                        if (propDesc != null)
                            propElem.InnerText = string.Format("[b]{0}[/b]: {1}", propCache.DisplayName, HelpBuilder.IncludePeriod(propDesc.Description));
                        else
                            propElem.InnerText = string.Format("[b]{0}[/b]", propCache.DisplayName);

                        //DO IMAGE
                        if (CanEmbedGIF(type, property) && type.GetCustomAttribute<PropertyData.NoPreviewsAttribute>() == null)
                        {
                            EmbedGIF(subSectionElem, type, property, filterMode);
                        }
                    }
                }
            }
        }

        public static bool CanEmbedGIF(Type forType, PropertyInfo pi)
        {
            if (typeof(Data.TexGen.TexGenNode).IsAssignableFrom(forType))
            {
                if (pi.PropertyType.IsEnum)
                    return true;
                else if (pi.PropertyType == typeof(bool))
                    return true;
                else if (pi.PropertyType == typeof(Color))
                    return true;
                return pi.GetCustomAttribute<PropertyData.ValidStepAttribute>() != null;
            }
            return false;
        }

        void EmbedGIF(XmlElement parent, Type forType, PropertyInfo pi, bool filterMode)
        {
            List<System.Drawing.Bitmap> frames = HelpBuilder.GenerateFrames(forType, pi, filterMode);
            if (frames.Count > 0)
            {
                int width = frames.Count * 128;
                System.Drawing.Bitmap newBMP = new System.Drawing.Bitmap(width, 128);

                for (int i = 0; i < frames.Count; ++i)
                {
                    var frame = frames[i];
                    for (int y = 0; y < frame.Height; ++y)
                        for (int x = 0; x < frame.Width; ++x)
                            newBMP.SetPixel(x + i * 128, y, frame.GetPixel(x, y));
                }

                using (var memStream = new System.IO.MemoryStream())
                {
                    newBMP.Save(memStream, System.Drawing.Imaging.ImageFormat.Png);
                    string base64 = Convert.ToBase64String(memStream.ToArray());
                    var imageElem = parent.CreateChild("raw_image");
                    imageElem.SetAttribute("align", "left");
                    imageElem.InnerText = base64;
                }
                // DO SOMETHING WITH IT
            }
        }
    }
}
