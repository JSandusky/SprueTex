using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit
{
    public static class RunDevCommand
    {
        public static bool RunCommand(string command, string paramString)
        {
            if (string.IsNullOrEmpty(command))
                return false;

            if (string.IsNullOrWhiteSpace(paramString))
                paramString = "";

            bool ret = false;
            MethodInfo[] methods = typeof(DevCommands).GetMethods(System.Reflection.BindingFlags.Static | BindingFlags.Public);
            var selMethod = methods.FirstOrDefault(m => m.Name.ToLowerInvariant().Equals(command.ToLowerInvariant()));
            if (selMethod != null)
            {
                try
                {
                    ParameterInfo[] paramList = selMethod.GetParameters();
                    string[] paramData = paramString.Split(' ');
                    object[] paramValues = null;
                    if (paramList.Length > 0)
                    {
                        paramValues = new object[paramData.Length];
                        for (int i = 0; i < paramData.Length && i < paramList.Length; ++i)
                            paramValues[i] = paramData[i];
                    }
                    ret = (bool)selMethod.Invoke(null, paramValues);
                }
                catch (Exception ex)
                {
                    ErrorHandler.inst().Error(ex);
                    return false;
                }
            }
            else
                ret = false;

            if (ret == false)
                ErrorHandler.inst().Error(string.Format("Executed: {0} {1}", command, paramString));
            else
                ErrorHandler.inst().Info(string.Format("Executed: {0} {1}", command, paramString));

            return ret;
        }
    }

    public static class DevCommands
    {
        public static bool List()
        {
            MethodInfo[] methods = typeof(DevCommands).GetMethods(System.Reflection.BindingFlags.Static | BindingFlags.Public);

            StringBuilder msg = new StringBuilder();

            msg.Append("Usage: [i]\"dev <command> <param1> <param2>\"[/i]\n\n");

            foreach (var meth in methods)
            {
                ParameterInfo[] paramList = meth.GetParameters();
                List<string> paramStrings = new List<string>();
                if (paramList != null && paramList.Length > 0)
                {
                    for (int i = 0; i < paramList.Length; ++i)
                    {
                        string paramStr = "          " + paramList[i].Name;
                        var pdesc = paramList[i].GetCustomAttribute<DescriptionAttribute>();
                        if (pdesc != null)
                            paramStr += string.Format(", [i]({0})[/i]", pdesc.Description);
                        paramStr += "\n";
                        paramStrings.Add(paramStr);
                    }
                }
                else
                    paramStrings.Add("          no parameters\n");

                msg.AppendFormat("[b][u]{0}[/u][/b]\n", meth.Name);

                var desc = meth.GetCustomAttribute<DescriptionAttribute>();
                if (desc != null)
                    msg.AppendFormat("    {0}\n", desc.Description);

                foreach (var str in paramStrings)
                    msg.AppendFormat("{0}", str);
            }
            Dlg.InfoDlg.Show(msg.ToString(), "Dev Commands");
            return true;
        }

        [Description("Writes all output textures to the 'outputFile' as a strip")]
        public static bool TexMaps(string outputFile)
        {
            IOCDependency<DocumentManager> docMan = new IOCDependency<DocumentManager>();
            var doc = docMan.Object.ActiveDocument as Data.TexGen.TextureGenDocument;
            if (doc == null)
                return false;
            var bmp = Data.Reports.TextureGraphReport.GenerateStrip(doc.DataRoot);
            bmp.Save(outputFile, System.Drawing.Imaging.ImageFormat.Png);
            System.Diagnostics.Process.Start(outputFile);
            return true;
        }

        [Description("Writes all output textures to the clipboard in a strip as Base64 text")]
        public static bool TexMapBase64()
        {
            IOCDependency<DocumentManager> docMan = new IOCDependency<DocumentManager>();
            var doc = docMan.Object.ActiveDocument as Data.TexGen.TextureGenDocument;
            if (doc == null)
                return false;
            var bmp = Data.Reports.TextureGraphReport.GenerateStrip(doc.DataRoot);
            ClipboardUtil.SetDataObject(bmp.ToBase64(), false);
            return true;
        }

        [Description("Fllls current document with all possible nodes for testing")]
        public static bool TexCreateAll()
        {
            IOCDependency<DocumentManager> docMan = new IOCDependency<DocumentManager>();
            var doc = docMan.Object.ActiveDocument as Data.TexGen.TextureGenDocument;
            if (doc == null)
                return false;

            double x = 0;
            double y = 0;
            double xAdd = 180;
            double yAdd = 220;
            int xCt = 0;
            foreach (var grp in Data.TexGen.TextureGenDocument.NodeGroups)
            {
                foreach (var type in grp.Types)
                {
                    var newNode = Activator.CreateInstance(type) as Data.TexGen.TexGenNode;
                    newNode.VisualX = x;
                    newNode.VisualY = y;
                    newNode.Construct();
                    newNode.Name = type.Name;
                    x += xAdd;
                    ++xCt;
                    if (xCt >= 12)
                    {
                        xCt = 0;
                        x = 0;
                        y += yAdd;
                    }
                    doc.DataRoot.AddNode(newNode);
                }
            }

            return true;
        }

        [Description("Saves the output of the currently selected texture node to the given 'outputFile'")]
        public static bool TexCurrent(string outputFile)
        {
            IOCDependency<DocumentManager> docMan = new IOCDependency<DocumentManager>();
            var doc = docMan.Object.ActiveDocument as Data.TexGen.TextureGenDocument;
            if (doc == null)
                return false;

            var node = doc.Selection.MostRecentlySelected as Data.TexGen.TexGenNode;
            if (node == null)
                return false;

            var cloneDoc = doc.DataRoot.Clone();
            if (cloneDoc == null)
                return false;
            var cloneNode = cloneDoc.Nodes.FirstOrDefault(n => n.NodeID == node.NodeID) as Data.TexGen.TexGenNode;
            if (cloneNode == null)
                return false;

            cloneDoc.Prime(new Microsoft.Xna.Framework.Vector2(128, 128));
            var bmp = cloneNode.GeneratePreview(128, 128);
            bmp.Save(outputFile);
            System.Diagnostics.Process.Start(outputFile);
            return true;
        }

        [Description("Saves the current texture node image to the clipboard as Base64 text")]
        public static bool TexCurrentBase64()
        {
            IOCDependency<DocumentManager> docMan = new IOCDependency<DocumentManager>();
            var doc = docMan.Object.ActiveDocument as Data.TexGen.TextureGenDocument;
            if (doc == null)
                return false;

            var node = doc.Selection.MostRecentlySelected as Data.TexGen.TexGenNode;
            if (node == null)
                return false;

            var cloneDoc = doc.DataRoot.Clone();
            if (cloneDoc == null)
                return false;
            var cloneNode = cloneDoc.Nodes.FirstOrDefault(n => n.NodeID == node.NodeID) as Data.TexGen.TexGenNode;
            if (cloneNode == null)
                return false;

            cloneDoc.Prime(new Microsoft.Xna.Framework.Vector2(128, 128));
            var bmp = cloneNode.GeneratePreview(128, 128);
            ClipboardUtil.SetDataObject(bmp.ToBase64(), false);
            return true;
        }

        [Description("Generates a property help image for the current texture node and property, possibly in 'filter' mode")]
        public static bool TexPropHelp(string outputFile, string property, [Description("true/false")] string isFilter)
        {
            IOCDependency<DocumentManager> docMan = new IOCDependency<DocumentManager>();
            var doc = docMan.Object.ActiveDocument as Data.TexGen.TextureGenDocument;
            if (doc == null)
                return false;

            var node = doc.Selection.MostRecentlySelected as Data.TexGen.TexGenNode;
            if (node == null)
                return false;

            PropertyInfo[] props = node.GetType().GetProperties();
            var selProp = props.FirstOrDefault(p => p.Name.ToLowerInvariant().Equals(property.ToLowerInvariant()));
            if (selProp != null)
            {
                var frames = Util.HelpBuilder.GenerateFrames(node.GetType(), selProp, isFilter.ToLowerInvariant().Equals("true"));
                var outBmp = frames.BuildStrip(128);
                outBmp.Save(outputFile);
                System.Diagnostics.Process.Start(outputFile);
                return true;
            }

            return false;
        }

        [Description("As TexPropHelp except to the clipboard as base64")]
        public static bool TexPropHelpBase64(string property, [Description("true/false")] string isFilter)
        {
            IOCDependency<DocumentManager> docMan = new IOCDependency<DocumentManager>();
            var doc = docMan.Object.ActiveDocument as Data.TexGen.TextureGenDocument;
            if (doc == null)
                return false;

            var node = doc.Selection.MostRecentlySelected as Data.TexGen.TexGenNode;
            if (node == null)
                return false;

            PropertyInfo[] props = node.GetType().GetProperties();
            var selProp = props.FirstOrDefault(p => p.Name.ToLowerInvariant().Equals(property.ToLowerInvariant()));
            if (selProp != null)
            {
                var frames = Util.HelpBuilder.GenerateFrames(node.GetType(), selProp, isFilter.ToLowerInvariant().Equals("true"));
                var outBmp = frames.BuildStrip(128);
                ClipboardUtil.SetDataObject(outBmp.ToBase64(), false);
                return true;
            }

            return false;
        }

        [Description("Writes an image strip of variations of the currently selected texture node for the given 'property'")]
        public static bool TexPropVars(string outputFile, string property)
        {
            IOCDependency<DocumentManager> docMan = new IOCDependency<DocumentManager>();
            var doc = docMan.Object.ActiveDocument as Data.TexGen.TextureGenDocument;
            if (doc == null)
                return false;

            var node = doc.Selection.MostRecentlySelected as Data.TexGen.TexGenNode;
            if (node == null)
                return false;

            var cloneDoc = doc.DataRoot.Clone();
            if (cloneDoc == null)
                return false;
            var cloneNode = cloneDoc.Nodes.FirstOrDefault(n => n.NodeID == node.NodeID && n != node) as Data.TexGen.TexGenNode;
            if (cloneNode == null)
                return false;

            PropertyInfo[] props = cloneNode.GetType().GetProperties();
            var selProp = props.FirstOrDefault(p => p.Name.ToLowerInvariant().Equals(property.ToLowerInvariant()));
            if (selProp != null)
            {
                var frames = Util.HelpBuilder.GenerateInstanceFrames(cloneDoc, cloneNode, node.GetType(), selProp, 256);
                var outBmp = frames.BuildStrip(256);
                outBmp.Save(outputFile);
                System.Diagnostics.Process.Start(outputFile);
                return true;
            }

            return false;
        }

        [Description("Prints a list of property names and types for the currently selected object")]
        public static bool Properties()
        {
            IOCDependency<DocumentManager> docMan = new IOCDependency<DocumentManager>();
            var doc = docMan.Object.ActiveDocument as Data.TexGen.TextureGenDocument;
            if (doc == null)
                return false;

            var node = doc.Selection.MostRecentlySelected;
            if (node != null)
            {
                var props = PropertyHelpers.GetAlphabetical(node.GetType());
                StringBuilder msg = new StringBuilder();

                if (props != null)
                {
                    foreach (var prop in props)
                        msg.AppendFormat("[b]{0}[/b]: {1}, alias [i]{2}[/i]\n", prop.Property.Name, prop.Property.PropertyType.Name, prop.DisplayName);
                }

                Dlg.InfoDlg.Show(msg.ToString(), node.GetType() + " properties");
            }
            return true;
        }

        [Description("Generates serializer code for the given type")]
        public static bool GenSerialization(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type == null)
                return false;

            StringBuilder sb = new StringBuilder();
            Util.SerializationGenerator.ProcessType(sb, type);
            ClipboardUtil.SetText(sb.ToString());

            return true;
        }

        [Description("Generates serialization code for all texgen types")]
        public static bool TexGenSerial()
        {
            StringBuilder text = new StringBuilder();
            foreach (var grp in Data.TexGen.TextureGenDocument.NodeGroups)
                Util.SerializationGenerator.ProcessTypes(text, grp.Types);
            Util.SerializationGenerator.ProcessType(text, typeof(Data.TexGen.BlendNode));
            ClipboardUtil.SetText(text.ToString());
            return true;
        }

        [Description("Generates serialization code for all shadergen types")]
        public static bool ShaderGenSerial()
        {
            StringBuilder shaderText = new StringBuilder();
            foreach (var grp in Data.ShaderGen.ShaderGenDocument.NodeGroups)
                Util.SerializationGenerator.ProcessTypes(shaderText, grp.Types);
            ClipboardUtil.SetText(shaderText.ToString());
            return true;
        }

        [Description("Generates help XML data files with embedded images")]
        public static bool BuildInternalHelp()
        {
            Util.DocHelpBuilder.BuildInternalHelp();
            return true;
        }

        [Description("Outputs all XNA colors to the clipboard")]
        public static bool DumpXnaColors()
        {
            var colors = typeof(Microsoft.Xna.Framework.Color).GetProperties(BindingFlags.Static | BindingFlags.Public);
            StringBuilder list = new StringBuilder();

            foreach (var col in colors)
            {
                //list.AppendFormat("else if (strcmp(colorText, \"{0}\") == 0)\r\n", col.Name.ToLowerInvariant());
                Microsoft.Xna.Framework.Color colVal = (Microsoft.Xna.Framework.Color)col.GetValue(null);
                //list.AppendFormat("    return ImColor({0}, {1}, {2}, {3});\r\n", colVal.R, colVal.G, colVal.B, colVal.A);
                list.AppendFormat("    nameToColorTable[\"{4}\"] = ImColor({0}, {1}, {2}, {3});\r\n", colVal.R, colVal.G, colVal.B, colVal.A, col.Name.ToLowerInvariant());
            }


            ClipboardUtil.SetText(list.ToString());
            return true;
        }

        [Description("Outputs all XNA colors to the clipboard as Urho3D colors")]
        public static bool DumpUrhoColors()
        {
            var colors = typeof(Microsoft.Xna.Framework.Color).GetProperties(BindingFlags.Static | BindingFlags.Public);
            StringBuilder list = new StringBuilder();

            foreach (var col in colors)
            {
                //list.AppendFormat("else if (strcmp(colorText, \"{0}\") == 0)\r\n", col.Name.ToLowerInvariant());
                Microsoft.Xna.Framework.Color colVal = (Microsoft.Xna.Framework.Color)col.GetValue(null);
                //list.AppendFormat("    return ImColor({0}, {1}, {2}, {3});\r\n", colVal.R, colVal.G, colVal.B, colVal.A);
                list.AppendFormat("    nameToColorTable[StringHash(\"{4}\")] = Color({0}, {1}, {2}, {3});\r\n", colVal.R / 255.0f, colVal.G / 255.0f, colVal.B / 255.0f, colVal.A / 255.0f, col.Name.ToLowerInvariant());
            }

            list.Append("\r\n\r\n");
            foreach (var col in colors)
            {
                list.AppendFormat("static const Color {0};\r\n", col.Name.Replace(" ", ""));
            }

            list.Append("\r\n\r\n");
            foreach (var col in colors)
            {
                //list.AppendFormat("else if (strcmp(colorText, \"{0}\") == 0)\r\n", col.Name.ToLowerInvariant());
                Microsoft.Xna.Framework.Color colVal = (Microsoft.Xna.Framework.Color)col.GetValue(null);
                //list.AppendFormat("    return ImColor({0}, {1}, {2}, {3});\r\n", colVal.R, colVal.G, colVal.B, colVal.A);
                list.AppendFormat("const Color Color::{4} = Color({0}, {1}, {2}, {3});\r\n", colVal.R / 255.0f, colVal.G / 255.0f, colVal.B / 255.0f, colVal.A / 255.0f, col.Name);
            }


            ClipboardUtil.SetText(list.ToString());
            return true;
        }
    }
}
