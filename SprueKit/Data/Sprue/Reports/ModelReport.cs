using System;
using System.Collections.Generic;
using System.Xml;
using SprueKit.Util;
using Microsoft.Xna.Framework;
using System.Collections.ObjectModel;
using System.Text;
using System.ComponentModel;

namespace SprueKit.Data.Reports
{

    public enum ModelReportType
    {
        Summary,
        Details,
        ThumbnailsOnly
    }

    public class ModelReportSettings : ReportSettings
    {
        public ModelReportType ReportStyle { get; set; }

        public override void Deserialize(XmlElement fromElem)
        {
            ReportTitle = fromElem.GetStringElement("title");
            ReportStyle = fromElem.GetEnumElement<ModelReportType>("type", ModelReportType.Details);
        }

        public override void Serialize(XmlElement intoElem)
        {
            var me = intoElem.CreateChild("model_report");
            me.AddStringElement("title", ReportTitle);
            me.AddEnumElement<ModelReportType>("type", ReportStyle);
        }
    }

    public enum TextureReportType
    {
        Summary,
        Details,
        VisualOverview,
    }

    public class TextureReportSettings : ReportSettings
    {
        [Description("Visual summary is very useful for art direction")]
        [PropertyData.PropertyPriority(1)]
        public TextureReportType ReportStyle { get; set; }

        public override string ToString()
        {
            if (!String.IsNullOrWhiteSpace(ReportTitle))
                return ReportTitle;
            return "<unnamed report>";
        }

        public override void Deserialize(XmlElement fromElem)
        {
            ReportTitle = fromElem.GetStringElement("title");
            ReportStyle = fromElem.GetEnumElement<TextureReportType>("type", TextureReportType.Details);
        }

        public override void Serialize(XmlElement intoElem)
        {
            var me = intoElem.CreateChild("texture_report");
            me.AddStringElement("title", ReportTitle);
            me.AddEnumElement<TextureReportType>("type", ReportStyle);
        }
    }

    public class ModelReport
    {
        public HTMLBuilder HTML { get; set; }
        public string OutDir { get; private set; }
        public ModelReport(SprueModel model, string outputPath)
        {
            HTML = new HTMLBuilder(model.DisplayName);
            RunReport(model, ModelReportType.Details);
            OutDir = outputPath;
            HTML.Close();
            System.IO.File.WriteAllText(outputPath, HTML.HTMLText);
        }

        public ModelReport(ModelReportSettings settings, string outputPath)
        {
            List<string> paths = new List<string>();
            foreach (var record in settings.FileList.Paths)
            {
                if (System.IO.File.Exists(record))
                    paths.Add(record);
            }

            foreach (var dir in settings.FolderList.Paths)
            {
                if (System.IO.Directory.Exists(dir))
                    RecurseDir(dir, paths, settings.RecurseFolders);
            }

            if (paths.Count == 0)
            {
                Console.WriteLine("ERROR: " + string.Format("Unable to produce texture graph report: '{0}', there were not files to process", settings.ReportTitle));
                ErrorHandler.inst().Warning(string.Format("Unable to produce texture graph report: '{0}', there were not files to process", settings.ReportTitle));
            }

            HTML = new HTMLBuilder(settings.ReportTitle);
            HTML.Anchor("top");
            HTML.Header(settings.ReportTitle, 1);

            List<SprueModel> models = new List<SprueModel>();
            foreach (var path in paths)
            {
                Console.WriteLine(path);
                var mdl = GetModel(path);
                if (mdl != null)
                    models.Add(mdl);
            }

            if (models.Count == 0)
                Console.WriteLine("ERROR: failed to successfully load any models");
            // Multiple items, write TOC
            if (models.Count > 1)
            {
                for (int i = 0; i < models.Count; ++i)
                    HTML.AnchorLink(string.Format("model_{0}", i), models[i].DisplayName);
            }

            for (int i = 0; i < models.Count; ++i)
            {
                RunReport(models[i], settings.ReportStyle, models.Count > 1 ? i : -1);
                if (i < models.Count - 1 && models.Count > 0)
                    HTML.Hr();
            }

            HTML.Close();
            System.IO.File.WriteAllText(outputPath, HTML.HTMLText);
        }

        public ModelReport(List<SprueModel> models, string outputPath)
        {
            HTML = new HTMLBuilder("Sprue Models Report");

            HTML.Anchor("top");
            HTML.Header("Sprue Models Report", 1);

            for (int i = 0; i < models.Count; ++i)
                HTML.AnchorLink(string.Format("model_{0}", i), models[i].DisplayName);

            for (int i = 0; i < models.Count; ++i)
            {
                RunReport(models[i], ModelReportType.Details, i);
                if (i < models.Count - 1)
                    HTML.Hr();
            }
            HTML.Close();
        }

        void RunReport(SprueModel model, ModelReportType type, int part = -1)
        {
            if (part > -1)
                HTML.Anchor(string.Format("model_{0}", part));
            HTML.Header(model.DisplayName, 1);

            RenderImage(model);

            if (type == ModelReportType.Summary || type == ModelReportType.Details)
            {
                HTML.Header("Metrics", 3);
                WriteMetrics(model);
            }

            if (type == ModelReportType.Summary || type == ModelReportType.Details)
            {
                HTML.Header("External Files", 3);
                WriteExternalFiles(model);
            }

            if (type == ModelReportType.Details)
            {
                HTML.Header("Object Types Used", 3);
                WriteObjectUsage(model);
            }

            if (type == ModelReportType.Details)
            {
                HTML.Header("Tree", 3);
                WriteTree(model);
            }

            if (part != -1)
                HTML.AnchorLink("top", "Back to top");
        }

        void WriteMetrics(SprueModel model)
        {
            HTML.Table();

            HTML.Tr();
                HTML.Td();
                    HTML.Bold("Vertices");
                HTML.PopTag();
                HTML.Td();
                    HTML.Text(model.TotalVertexCount.ToString());
                HTML.PopTag();
            HTML.PopTag();

            HTML.Tr();
                HTML.Td();
                    HTML.Bold("Triangles");
                HTML.PopTag();
                HTML.Td();
                    HTML.Text(model.TotalTriangleCount.ToString());
                HTML.PopTag();
            HTML.PopTag();

            HTML.PopTag();
        }

        void WriteObjectUsage(SprueModel model)
        {
            Dictionary<string, int> names = new Dictionary<string,int>();
            model.VisitAll((SpruePiece p) =>
            {
                names.Increment(p.GetType().Name.SplitCamelCase());
            });

            if (names.Count > 0)
            {
                HTML.UL();
                foreach (var name in names)
                {
                    HTML.LI();
                    HTML.Bold(); HTML.Text(name.Value.ToString()); HTML.PopTag();
                    HTML.Text("x - "); HTML.Text(name.Key);
                    HTML.PopTag();
                }
                HTML.PopTag();
            }
            else
            {
                HTML.Bold(); HTML.Text("< no objects in this file >"); HTML.PopTag();
            }
        }

        void WriteExternalFiles(SprueModel model)
        {
            HTML.Header(4);
            HTML.Text("Models");
            HTML.PopTag();

            {
                Dictionary<Uri, int> models = new Dictionary<Uri, int>();
                model.VisitAll<ModelPiece>((ModelPiece p) =>
                {
                    if (p.ModelFile != null && p.ModelFile.ModelData != null)
                    {
                        Uri uri = p.ModelFile.ModelFile;
                        models.Increment(uri);
                    }
                });

                if (models.Count == 0)
                    HTML.Text("< no models used >");
                else
                {
                    HTML.UL();
                    foreach (var mdl in models)
                    {
                        HTML.LI();
                        HTML.Bold(string.Format("{0}x - ", mdl.Value));
                        HTML.Link(mdl.Key.ToString());
                        HTML.Text(mdl.Key.AbsolutePath);
                        HTML.PopTag();
                        HTML.PopTag();
                    }
                    HTML.PopTag();
                }
            }

            HTML.Header(4);
            HTML.Text("Textures");
            HTML.PopTag();

            {
                Dictionary<Uri, int> texturesUsed = new Dictionary<Uri, int>();
                model.VisitAll<BasicTextureComponent>((BasicTextureComponent comp) =>
                {
                    foreach (var map in comp.TextureMaps)
                    {
                        if (map.Image != null && System.IO.File.Exists(map.Image.AbsolutePath))
                            texturesUsed.Increment(map.Image);
                    }
                });

                if (texturesUsed.Count == 0)
                    HTML.Text("< no textures used >");
                else
                {
                    HTML.UL();
                    foreach (var tex in texturesUsed)
                    {
                        HTML.LI();
                        HTML.Bold(string.Format("{0}x - ", tex.Value));
                        HTML.Link(tex.Key.ToString());
                        HTML.Text(tex.Key.AbsolutePath);
                        HTML.PopTag();
                        HTML.PopTag();
                    }
                    HTML.PopTag();
                }
            }

            HTML.Header(4);
            HTML.Text("Output Textures");
            HTML.PopTag();

            if (model.MeshData != null && model.MeshData.Texture != null)
                WriteTextures(model.Name, model.MeshData.Texture);
        }

        void WriteTree(SpruePiece piece)
        {
            HTML.UL();
            HTML.ListItem(piece.DisplayName);
            foreach (var child in piece.FlatChildren)
            {
                WriteTree(child as SpruePiece);
            }
            HTML.PopTag();
        }

        void WriteTextures(string name, Material mat)
        {
            int imgDim = 256;
            string expr = string.IsNullOrWhiteSpace(name) ? "{0}.png" : "{1}_{0}.png";
            HTML.Table();

            HTML.Tr();
            HTML.TD_Bold("Diffuse");
            HTML.TD_Bold("Normal Map");
            HTML.TD_Bold("Roughness");
            HTML.TD_Bold("Metallic");
            HTML.TD_Bold("Displacement");
            HTML.PopTag();

            HTML.Tr();
            HTML.Td();
            if (mat.DiffuseData != null)
                HTML.ImgEmbedded(mat.DiffuseData, 256);
            else
                HTML.Text("< unused >");
            
            HTML.PopTag();

            HTML.Td();
            if (mat.NormalMapData != null)
                HTML.ImgEmbedded(mat.NormalMapData, 256);
            else
                HTML.Text("< unused >");
            HTML.PopTag();

            HTML.Td();
            if (mat.RoughnessData != null)
                HTML.ImgEmbedded(mat.RoughnessData, 256);
            else
                HTML.Text("< unused >");
            HTML.PopTag();

            HTML.Td();
            if (mat.MetallicData != null)
                HTML.ImgEmbedded(mat.MetallicData, 256);
            else
                HTML.Text("< unused >");
            HTML.PopTag();

            HTML.Td();
            if (mat.DisplacementData != null)
                HTML.ImgEmbedded(mat.DisplacementData, 256);
            else
                HTML.Text("< unused >");
            HTML.PopTag();
            HTML.PopTag();

            HTML.PopTag();
        }

        void RenderImage(SprueModel model)
        {
            SoftwareRenderer render = new SoftwareRenderer(256, 256, 45);
            List<Vector3> vertexPositions = new List<Vector3>();
            model.VisitAll((p) => { vertexPositions.Add(p.Position); });
            render.FocusOnCloud(vertexPositions);
            bool anythingWritten = false;

            if (model.MeshData != null)
                anythingWritten |= render.RasterizeTriangles(model.MeshData.GetIndices(), model.MeshData.GetVertices(), false);
            model.VisitAll<ModelPiece>((m) =>
            {
                var meshes = m.GetMeshes();
                foreach (var mesh in meshes)
                    anythingWritten |= render.RasterizeTriangles(mesh.GetIndices(), mesh.GetVertices(), false);
            });

            var img = render.GetDepthImage();
            if (img != null)
                HTML.ImgEmbedded(img, 256);
        }

        Data.SprueModel GetModel(string path)
        {
            try
            {
                Data.SprueModel sprueModel = Data.SprueModel.LoadFile(path);

                if (sprueModel != null)
                {
                    Console.WriteLine(string.Format("Generating mesh: {0}", path));
                    var task = new Data.Sprue.ComputeMeshingTask(null, sprueModel);
                    task.TaskLaunch();
                    task.TaskEnd();
                    task.Consequences[0].TaskLaunch();
                    task.Consequences[0].TaskEnd();
                }

                return sprueModel;
                //SerializationContext ctx = new SerializationContext(new Uri(System.IO.Path.GetDirectoryName(path)));
                //if (path.EndsWith(".xml"))
                //{
                //    System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                //    doc.Load(path);
                //    var graphElem = doc.DocumentElement.SelectSingleNode("spruemodel") as System.Xml.XmlElement;
                //    sprueModel.Deserialize(ctx, graphElem);
                //}
                //else if (path.EndsWith(".sprm"))
                //{
                //    using (System.IO.FileStream stream = new System.IO.FileStream(path, System.IO.FileMode.Open))
                //    {
                //        using (System.IO.BinaryReader reader = new System.IO.BinaryReader(stream))
                //            sprueModel.Deserialize(ctx, reader);
                //    }
                //}
                //
                //if (ctx.BrokenPaths.Count > 0)
                //{
                //    StringBuilder brokenPaths = new StringBuilder();
                //    for (int i = 0; i < ctx.BrokenPaths.Count; ++i)
                //    {
                //        if (i > 0)
                //            brokenPaths.Append(", ");
                //        brokenPaths.Append(ctx.BrokenPaths[i].BrokenPath);
                //    }
                //    throw new Exception(string.Format("Broken file paths prevented correct loading: {0}", brokenPaths.ToString()));
                //}
                return sprueModel;
            }
            catch (Exception ex)
            {
            }
            return null;
        }

        void RecurseDir(string dir, List<string> paths, bool isRecurse)
        {
            foreach (var file in System.IO.Directory.EnumerateFiles(dir))
            {
                if (System.IO.Path.GetExtension(file).Equals(".xml") || System.IO.Path.GetExtension(file).Equals(".sprm"))
                    paths.Add(file);
            }
            if (isRecurse)
            {
                foreach (var subDir in System.IO.Directory.EnumerateDirectories(dir))
                    RecurseDir(subDir, paths, isRecurse);
            }
        }
    }
}
