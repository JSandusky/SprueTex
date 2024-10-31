using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;

namespace SprueKit
{
    public class AppCommandLine
    {
        bool HasSwitch(string key, params string[] args)
        {
            foreach (var str in args)
                if (str.ToLowerInvariant().Equals(key))
                    return true;
            return false;
        }

        string GetParam(string key, params string[] args)
        {
            foreach (var str in args)
                if (str.ToLowerInvariant().StartsWith(key)) //ditch the equals sign, then eat quotes
                    return str.Substring(key.Length + 1).Replace("\"", "");
            return "";
        }

        string GetPath(string str)
        {
            if (!System.IO.Path.IsPathRooted(str))
                str = System.IO.Path.Combine(Environment.CurrentDirectory, str);
            if (System.IO.Directory.Exists(str))
                return str;
            else if (System.IO.File.Exists(str))
                return str;
            return null;
        }

        public AppCommandLine(params string[] args)
        {
            if (args[0].ToLowerInvariant().Equals("texture"))
            {
                Console.WriteLine("");
                string file = GetPath(args[1]);
                string outPath = args[2];
                bool randomize = HasSwitch("-random", args);
                bool captureMode = HasSwitch("-capture", args);
                string outputTarget = GetParam("-node", args);
                string setPerm = GetParam("-perm", args);

                XmlDocument doc = new XmlDocument();
                doc.Load(file);
                Data.SerializationContext ctx = new Data.SerializationContext(new Uri(System.IO.Path.GetDirectoryName(file)));
                Data.Graph.Graph graph = new Data.Graph.Graph();
                graph.Deserialize(ctx, doc.DocumentElement.FirstChild as XmlElement);
                Console.WriteLine(string.Format("Loaded {0} nodes", graph.Nodes.Count));

                if (randomize)
                {
                    foreach (var node in graph.Nodes)
                        Data.PermutationHelpers.Randomize(node as Data.TexGen.TexGenNode);
                }
                if (!string.IsNullOrEmpty(setPerm))
                {
                    foreach (var node in graph.Nodes)
                        Data.PermutationHelpers.ApplyName(node as Data.TexGen.TexGenNode, setPerm);
                }

                if (ctx.BrokenPaths.Count > 0)
                {
                    Console.WriteLine("ERROR: file contains broken resource links");
                    return;
                }

                string outputPath = System.IO.Path.GetDirectoryName(outPath);
                string baseName = System.IO.Path.GetFileNameWithoutExtension(outPath);
                int idx = 1;
                if (outPath.EndsWith(".tga"))
                    idx = 2;
                else if (outPath.EndsWith(".dds"))
                    idx = 3;
                else if (outPath.EndsWith(".jpg") || outPath.EndsWith(".jpeg"))
                    idx = 4;
                else if (outPath.EndsWith(".hdr"))
                    idx = 5;
                Data.TexGen.TextureGenDocument.DoExport(graph, baseName, outputPath, outputTarget, idx, captureMode);
            }
            else if (args[0].ToLowerInvariant().Equals("report"))
            {
                Console.WriteLine("");
                string p = GetPath(args[1]);
                string title = args[2];
                string outPath = args[3];
                bool isRecurse = HasSwitch("-s", args);
                bool isTexReport = HasSwitch("-tex", args);
                bool isModelReport = HasSwitch("-mdl", args);
                isTexReport = true;
                if (!isTexReport && !isModelReport)
                {
                    Console.WriteLine("ERROR: reporting type not specified, use -tex or -mdl");
                    return;
                }
                try
                {
                    if (System.IO.Directory.Exists(outPath) && File.GetAttributes(outPath).HasFlag(FileAttributes.Directory))
                    {
                        Console.WriteLine("ERROR: Improperly specified outputfile");
                        return;
                    }
                }
                catch (Exception ex) { Console.WriteLine("ERROR: Improperly specified outputfile"); return; }

                if (isModelReport)
                {
                    if (p != null && (System.IO.File.Exists(p) || System.IO.Directory.Exists(p)))
                    {
                        Data.Reports.ModelReportSettings settings = new Data.Reports.ModelReportSettings();
                        settings.ReportTitle = title;
                        settings.ReportStyle = Data.Reports.ModelReportType.Summary;
                        if (HasSwitch("-compare", args))
                            settings.ReportStyle = Data.Reports.ModelReportType.ThumbnailsOnly;
                        if (HasSwitch("-details", args))
                            settings.ReportStyle = Data.Reports.ModelReportType.Details;

                        if (File.GetAttributes(p).HasFlag(FileAttributes.Directory))
                        {
                            List<string> dirs = new List<string>();
                            settings.FolderList.Paths.Add(p);
                        }
                        else
                            settings.FileList.Paths.Add(p);
                        var rpt = new Data.Reports.ModelReport(settings, outPath);
                        Console.WriteLine(string.Format("OUTPUT: {0}", outPath));
                    }
                    else
                    {
                        Console.WriteLine("ERROR: incorrect path specified");
                    }
                }

                if (isTexReport)
                {
                    if (p != null)
                    {
                        Data.Reports.TextureReportSettings settings = new Data.Reports.TextureReportSettings();
                        settings.ReportTitle = title;
                        settings.ReportStyle = Data.Reports.TextureReportType.Summary;
                        if (HasSwitch("-compare", args))
                            settings.ReportStyle = Data.Reports.TextureReportType.VisualOverview;
                        if (HasSwitch("-details", args))
                            settings.ReportStyle = Data.Reports.TextureReportType.Details;

                        if (File.GetAttributes(p).HasFlag(FileAttributes.Directory))
                        {
                            List<string> dirs = new List<string>();
                            settings.FolderList.Paths.Add(p);
                        }
                        else
                            settings.FileList.Paths.Add(p);
                        var rpt = new Data.Reports.TextureGraphReport(settings, outPath);
                        Console.WriteLine(string.Format("OUTPUT: {0}", outPath));
                    }
                    else
                    {
                        Console.WriteLine("ERROR: incorrect path specified");
                    }
                }
            }
        }
    }
}
