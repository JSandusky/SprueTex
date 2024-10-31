using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit
{
    public class PluginInfo
    {
        public PluginInfo(string aName, string[] nameParts)
        {
            Name = aName;
            Parts = nameParts;
            Components = new List<string>();
        }

        public string Name { get; private set; }
        public string[] Parts { get; private set; }

        public List<string> Components { get; private set; }
    }

    public class PluginTypeList<T> : List<T>
    {
        public void Process(PluginInfo plug, Type[] types)
        {
            foreach (Type t in types)
            {
                if (t.GetInterface(typeof(T).Name) != null)
                {
                    Add((T)Activator.CreateInstance(t));
                    plug.Components.Add(t.Name);
                }
            }
        }
    }

    public class PluginManager
    {
        static PluginManager inst_;

        public PluginTypeList<PluginLib.IMeshingPlugin> VoxelGenerators { get; private set; } = new PluginTypeList<PluginLib.IMeshingPlugin>();
        public PluginTypeList<PluginLib.IModelExporter> ModelExporters { get; private set; } = new PluginTypeList<PluginLib.IModelExporter>();
        public PluginTypeList<PluginLib.IModelFilter> ModelFilters { get; private set; } = new PluginTypeList<PluginLib.IModelFilter>();

        // List of loaded plugins
        List<PluginInfo> assemblies = new List<PluginInfo>();

        /// <summary>
        /// Constructs and scans a directory for compatible plugins
        /// </summary>
        /// <param name="dir">Directory to scan for DLLs</param>
        public PluginManager(string dir)
        {
            inst_ = this;

            string path = Path.Combine(Directory.GetCurrentDirectory(), dir);

            if (!Directory.Exists(path))
                return;

            foreach (string file in Directory.GetFiles(path))
            {
                if (Path.GetExtension(file).Equals(".dll") && File.Exists(file))
                {
                    try
                    {
                        Assembly asm = Assembly.LoadFile(file);
                        Type[] types = asm.GetExportedTypes();
                        FileVersionInfo myFileVersionInfo = FileVersionInfo.GetVersionInfo(asm.Location);
                        PluginInfo plugin = new PluginInfo(asm.ManifestModule.Name, new string[] { myFileVersionInfo.ProductName, myFileVersionInfo.ProductVersion, myFileVersionInfo.CompanyName, myFileVersionInfo.Comments });

                        VoxelGenerators.Process(plugin, types);
                        ModelExporters.Process(plugin, types);
                        ModelFilters.Process(plugin, types);

                        if (plugin.Components.Count > 0)
                            assemblies.Add(plugin);
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.inst().Error(ex);
                    }
                }
            }
        }

        public static PluginManager inst()
        {
            if (inst_ == null)
                new PluginManager(App.ProgramPath("Plugins"));
            return inst_;
        }

        public List<PluginInfo> InstalledPlugins
        {
            get
            {
                return assemblies;
            }
        }

        /*
        public PluginLib.IFileEditor GetFileEditor(string path)
        {
            foreach (PluginLib.IFileEditor editor in FileEditors)
            {
                if (editor.CanEditFile(path, System.IO.Path.GetExtension(path)))
                    return editor;
            }
            return null;
        }*/
    }

}
