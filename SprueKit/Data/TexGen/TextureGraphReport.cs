using SprueKit.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SprueKit.Data.TexGen;
using System.Diagnostics;

namespace SprueKit.Data.Reports
{
    public class TextureGraphReport
    {
        public HTMLBuilder HTML { get; set; }

        public TextureGraphReport(TextureReportSettings settings, Data.Graph.Graph texGraph, string outputPath)
        {
            List<string> paths = new List<string>();
            HTML = new HTMLBuilder(settings.ReportTitle);

            HTML.Anchor("top");
            HTML.Header(settings.ReportTitle, 1);
            BuildSubreport(settings, texGraph, false);
            HTML.Close();
            System.IO.File.WriteAllText(outputPath, HTML.HTMLText);
        }

        public TextureGraphReport(TextureReportSettings settings, string outputPath)
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
                ErrorHandler.inst().Warning(string.Format("Unable to produce texture graph report: '{0}', there were not files to process", settings.ReportTitle));
            }
            HTML = new HTMLBuilder(settings.ReportTitle);
            HTML.Anchor("top");
            HTML.Header(settings.ReportTitle, 1);
            BuildReport(settings, paths);
            HTML.Close();
            System.IO.File.WriteAllText(outputPath, HTML.HTMLText);
        }

        void RecurseDir(string dir, List<string> paths, bool recurseMode)
        {
            foreach (var file in System.IO.Directory.EnumerateFiles(dir))
            {
                if (System.IO.Path.GetExtension(file).Equals(".txml") || System.IO.Path.GetExtension(file).Equals(".texg"))
                    paths.Add(file);
            }
            if (recurseMode)
            {
                foreach (var subDir in System.IO.Directory.EnumerateDirectories(dir))
                    RecurseDir(subDir, paths, recurseMode);
            }
        }

        void BuildReport(TextureReportSettings settings, List<string> paths)
        {
            HTML = new HTMLBuilder(settings.ReportTitle);

            HTML.Anchor("top");
            HTML.Header(settings.ReportTitle, 1);

            foreach (var path in paths)
            {
                Data.Graph.Graph textureGraph = GetGraph(path);
                if (textureGraph != null)
                {
                    string fileName = System.IO.Path.GetFileName(path);
                    HTML.Header(fileName, 2);
                    BuildSubreport(settings, textureGraph, true);
                }
                else
                {
                    HTML.Header(3, "error");
                    HTML.Text(string.Format("Unable to load texture graph for processing: {0}", path));
                    HTML.PopTag();
                }
            }
        }

        Data.Graph.Graph GetGraph(string path)
        {
            try { 
                Data.Graph.Graph textureGraph = new Graph.Graph();

                SerializationContext ctx = new SerializationContext(new Uri(System.IO.Path.GetDirectoryName(path)));
                if (path.EndsWith(".txml"))
                {
                    System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                    doc.Load(path);
                    var graphElem = doc.DocumentElement.SelectSingleNode("graph") as System.Xml.XmlElement;
                    textureGraph.Deserialize(ctx, graphElem);
                }
                else if (path.EndsWith(".texg"))
                {
                    using (System.IO.FileStream stream = new System.IO.FileStream(path, System.IO.FileMode.Open))
                    {
                        using (System.IO.BinaryReader reader = new System.IO.BinaryReader(stream))
                            textureGraph.Deserialize(ctx, reader);
                    }
                }

                if (ctx.BrokenPaths.Count > 0)
                {
                    StringBuilder brokenPaths = new StringBuilder();
                    for (int i = 0; i < ctx.BrokenPaths.Count; ++i)
                    {
                        if (i > 0)
                            brokenPaths.Append(", ");
                        brokenPaths.Append(ctx.BrokenPaths[i].BrokenPath);
                    }
                    throw new Exception(string.Format("Broken file paths prevented correct loading: {0}", brokenPaths.ToString()));
                }
                return textureGraph;
            }
            catch (Exception ex)
            {
            }
            return null;
        }

        bool BuildSubreport(TextureReportSettings settings, Data.Graph.Graph textureGraph, bool isMulti)
        {
            // common item header
            if (settings.ReportStyle == TextureReportType.Summary || settings.ReportStyle == TextureReportType.Details)
            {
                HTML.Table();
                HTML.Tr();
                GenerateOutputs(textureGraph);
                HTML.PopTag();
                HTML.PopTag();

                HTML.Anchor("basic_info");
                HTML.Header(2);
                HTML.Text("Basic Information");
                HTML.PopTag();

                HTML.P();
                HTML.Bold("Number of Nodes");
                HTML.Text(": ");
                HTML.Text(string.Format("{0}", textureGraph.Nodes.Count));
                HTML.PopTag();

                int outputCt = textureGraph.Nodes.Count(p => p is Data.TexGen.TextureOutputNode);
                HTML.P();
                HTML.Bold("Number of Outputs");
                HTML.Text(": ");
                HTML.Text(string.Format("{0}", outputCt));
                HTML.PopTag();

                HTML.P();
                HTML.Bold("Number of Connections");
                HTML.Text(": ");
                HTML.Text(string.Format("{0}", textureGraph.Connections.Count));
                HTML.PopTag();
            }

            // detailed contents breakdown
            if (settings.ReportStyle == TextureReportType.Details)
            {
                HTML.Anchor("nodes_used");
                HTML.Header(2);
                HTML.Text("Nodes Used");
                HTML.PopTag();

                Dictionary<Type, int> nodeCts = new Dictionary<Type, int>();
                foreach (var node in textureGraph.Nodes)
                {
                    bool found = nodeCts.ContainsKey(node.GetType());
                    if (found == true)
                        nodeCts[node.GetType()] = nodeCts[node.GetType()] + 1;
                    else
                        nodeCts[node.GetType()] = 1;
                }

                HTML.Table();
                HTML.Tr();
                HTML.Th(); HTML.Text("#"); HTML.PopTag();
                HTML.Th(); HTML.Raw("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;"); HTML.Text("Node Type"); HTML.PopTag();
                HTML.PopTag();
                List<KeyValuePair<Type, int>> flippedNodes = ToList(nodeCts);
                for (int i = flippedNodes.Count - 1; i >= 0; --i)
                {
                    var item = flippedNodes[i];
                    HTML.Tr();
                    HTML.Td();
                    HTML.Text(item.Value.ToString());
                    HTML.PopTag();
                    HTML.Td();
                    HTML.Text(item.Key.Name.SplitCamelCase());
                    HTML.PopTag();
                    HTML.PopTag();
                }
                HTML.PopTag();

                HTML.Anchor("external_res");
                HTML.Header(2);
                HTML.Text("External Resources");
                HTML.PopTag();

                Dictionary<Uri, int> usedResources = new Dictionary<Uri, int>();

                foreach (var node in textureGraph.Nodes)
                {
                    var props = node.GetType().GetProperties();
                    foreach (var pi in props)
                    {
                        if (pi.PropertyType == typeof(Uri))
                        {
                            Uri val = pi.GetValue(node) as Uri;
                            if (val != null)
                            {
                                if (usedResources.ContainsKey(val))
                                    usedResources[val] = usedResources[val] + 1;
                                else
                                    usedResources[val] = 1;
                            }
                        }
                    }
                }

                var usedResourcesList = ToList(usedResources);
                HTML.UL();
                for (int i = usedResourcesList.Count - 1; i >= 0; --i)
                {
                    var item = usedResourcesList[i];
                    HTML.LI();
                    HTML.Bold(string.Format("{0}x - ", item.Value));
                    HTML.Link(item.Key.ToString());
                    HTML.Text(item.Key.AbsolutePath);
                    HTML.PopTag();
                    HTML.PopTag();
                }
                HTML.PopTag();

                {
                    HTML.Anchor("permutations");
                    HTML.Header(2);
                    HTML.Text("Permutations");
                    HTML.PopTag();
                    HashSet<string> permName = new HashSet<string>();
                    foreach (var node in textureGraph.Nodes)
                        Data.PermutationHelpers.GetUsedPermutationNames(node as IPermutable, permName);

                    HTML.UL();
                    if (permName.Count > 0)
                    {
                        foreach (var str in permName)
                        {
                            HTML.LI();
                            HTML.Text(str);
                            HTML.PopTag();
                        }
                    }
                    else
                    {
                        HTML.LI();
                        HTML.Text("No permutations");
                        HTML.PopTag();
                    }
                    HTML.PopTag();
                }

                {
                    HTML.Anchor("permutation_flags");
                    HTML.Header(2);
                    HTML.Text("Permutation Flags");
                    HTML.PopTag();
                    HashSet<string> permFlags = new HashSet<string>();
                    foreach (var node in textureGraph.Nodes)
                        Data.PermutationHelpers.GetUsedPermutationFlags(node as IPermutable, permFlags);

                    HTML.UL();
                    if (permFlags.Count > 0)
                    {
                        foreach (var str in permFlags)
                        {
                            HTML.LI();
                            HTML.Text(str);
                            HTML.PopTag();
                        }
                    }
                    else
                    {
                        HTML.LI();
                        HTML.Text("No permutation flags are used");
                        HTML.PopTag();
                    }
                    HTML.PopTag();
                }
            }
            // only output into a table based on the outputs
            else if (settings.ReportStyle == TextureReportType.VisualOverview)
            {
                HTML.Table();
                HTML.Tr();
                GenerateOutputs(textureGraph);
                HTML.PopTag();
                HTML.PopTag();
            }
            return true;
        }

        public static System.Drawing.Bitmap GenerateStrip(Data.Graph.Graph texGraph)
        {
            int dim = 256;

            List<Data.TexGen.TextureOutputNode> outputs = new List<TexGen.TextureOutputNode>();
            foreach (var nd in texGraph.Nodes)
                if (nd is Data.TexGen.TextureOutputNode)
                    outputs.Add(nd as Data.TexGen.TextureOutputNode);
            outputs.Sort(new OutputNodeSourter());

            var bmp = new System.Drawing.Bitmap(dim * outputs.Count, dim);

            for (int i = 0; i < outputs.Count; ++i)
            {
                var nd = outputs[i];
                texGraph.Prime(new Microsoft.Xna.Framework.Vector2(dim, dim));
                var subBmp = nd.GeneratePreview(dim, dim);
                for (int y = 0; y < subBmp.Height; ++y)
                    for (int x = 0; x < subBmp.Width; ++x)
                        bmp.SetPixel(x + i * dim, y, subBmp.GetPixel(x, y));
            }

            return bmp;
        }

        void GenerateOutputs(Data.Graph.Graph texGraph)
        {
            List<Data.TexGen.TextureOutputNode> outputs = new List<TexGen.TextureOutputNode>();
            foreach (var nd in texGraph.Nodes)
                if (nd is Data.TexGen.TextureOutputNode)
                    outputs.Add(nd as Data.TexGen.TextureOutputNode);
            outputs.Sort(new OutputNodeSourter());

            HTML.Tr();
            foreach (var nd in outputs)
            {
                HTML.Td();
                HTML.Bold();
                HTML.Text(nd.Name);
                HTML.PopTag();
                HTML.PopTag();
            }
            HTML.PopTag();

            int[] nodeTraces = new int[outputs.Count];
            for (int i = 0; i < outputs.Count; ++i)
            {
                int ct = 0;
                outputs[i].TraceUpstream((n, depth) => { ++ct; });
                nodeTraces[i] = ct - 1;
            }

            Stopwatch totalWatch = new Stopwatch();
            totalWatch.Start();

            TimeSpan[] times = new TimeSpan[outputs.Count];
            HTML.Tr();
            for (int i = 0; i < outputs.Count; ++i)
            {
                var nd = outputs[i];
                HTML.Td();
                if (!nd.InputSockets[0].HasConnections())
                {
                    HTML.Text("Nothing to preview for this node");
                }
                else
                {
                    Stopwatch subTime = new Stopwatch();
                    subTime.Start();
                    texGraph.Prime(new Microsoft.Xna.Framework.Vector2(256, 256));
                    var bmp = nd.GeneratePreview(256, 256);
                    subTime.Stop();
                    times[i] = subTime.Elapsed;
                    HTML.ImgEmbedded(bmp, 256);
                }
                HTML.PopTag();
            }
            totalWatch.Stop();
            HTML.PopTag();

            HTML.Tr();
            for (int i = 0; i < outputs.Count; ++i)
            {
                HTML.Td();
                HTML.Text(times[i].ToString());
                HTML.PopTag();
            }
            HTML.PopTag();

            HTML.Tr();
            for (int i = 0; i < outputs.Count; ++i)
            {
                HTML.Td();
                if (nodeTraces[i] == 1)
                    HTML.Text("1 node traced");
                else if (nodeTraces[i] == 0)
                    HTML.Text("Not processed, no inputs");
                else
                    HTML.Text(string.Format("{0} nodes traced", nodeTraces[i]));
                HTML.PopTag();
            }
            HTML.PopTag();

            HTML.Tr();
            HTML.Td(outputs.Count);
            HTML.Bold();
            HTML.Text("Total generation time: ");
            HTML.PopTag();
            HTML.Text(totalWatch.Elapsed.ToString());
            HTML.PopTag();
            HTML.PopTag();
        }

        public List<KeyValuePair<T, int>> ToList<T>(Dictionary<T, int> dict)
        {
            List<KeyValuePair<T, int>> flippedNodes = new List<KeyValuePair<T, int>>();
            foreach (var item in dict)
                flippedNodes.Add(item);
            flippedNodes.Sort(new Sorter<T>());
            return flippedNodes;
        }

        class OutputNodeSourter : IComparer<Data.TexGen.TextureOutputNode>
        {
            public int Compare(TextureOutputNode x, TextureOutputNode y)
            {
                if (x.OutputChannel < y.OutputChannel)
                    return -1;
                else if (x.OutputChannel > y.OutputChannel)
                    return 1;
                return 0;
            }
        }

        class Sorter<T> : IComparer<KeyValuePair<T, int>>
        {
            public int Compare(KeyValuePair<T, int> x, KeyValuePair<T, int> y)
            {
                if (x.Value < y.Value)
                    return -1;
                else if (x.Value > y.Value)
                    return 1;
                return 0;
            }
        }
    }
}
