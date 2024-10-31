using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using SprueKit.Data.TexGen;
using GongSolutions.Wpf.DragDrop;
using System.IO;
using System.Reflection;
using System.ComponentModel;
using System.Windows.Media;

namespace SprueKit.Controls.TexGen
{
    /// <summary>
    /// Interaction logic for TextureGenTree.xaml
    /// </summary>
    public partial class TextureGenTree : UserControl, IDragSource
    {
        TreeViewItem prefabsItem;

        public void ResetPrefabs()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                prefabsItem.Items.Clear();
                try
                {
                    FillPrefabItem(prefabsItem, App.ProgramPath("Prefabs"));
                }
                catch (Exception) { }
            }));
        }

        void FillPrefabItem(TreeViewItem parent, string dir)
        {
            foreach (var str in System.IO.Directory.EnumerateFiles(dir))
            {
                var newItem = new TreeViewItem { Header = System.IO.Path.GetFileNameWithoutExtension(str), Tag = string.Format("PREFAB:{0}", str) };
                parent.Items.Add(newItem);
            }
            foreach (var subDir in System.IO.Directory.EnumerateDirectories(dir))
            {
                var newItem = new TreeViewItem { Header = new DirectoryInfo(subDir).Name };
                parent.Items.Add(newItem);
                FillPrefabItem(newItem, subDir);
            }
        }

        FileSystemWatcher prefabWatcher;
        public TextureGenTree()
        {
            InitializeComponent();

            try
            {
                prefabWatcher = new FileSystemWatcher(App.ProgramPath("Prefabs"));
                prefabWatcher.Changed += PrefabWatcher_Changed;
                prefabWatcher.Renamed += PrefabWatcher_Renamed;
                prefabWatcher.Deleted += PrefabWatcher_Deleted;
                prefabWatcher.Created += PrefabWatcher_Created;
                prefabWatcher.EnableRaisingEvents = true;
                
            } catch (Exception) {
                ErrorHandler.inst().Warning("Unable to observe prefabs directory");
            }
        }

        private void PrefabWatcher_Created(object sender, FileSystemEventArgs e)
        {
            ResetPrefabs();
        }

        private void PrefabWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            ResetPrefabs();
        }

        private void PrefabWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            ResetPrefabs();
        }

        private void PrefabWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            ResetPrefabs();
        }

        public bool CanStartDrag(IDragInfo dragInfo)
        {
            if (((TreeViewItem)dragInfo.SourceItem).DataContext is Type)
            {
                var t = ((TreeViewItem)dragInfo.SourceItem).DataContext as Type;
#if IS_DEMO
                if (Data.TexGen.TextureGenDocument.DemoRestricted.Contains(t))
                    return false;
#endif
                return true;
            }
            else if (((TreeViewItem)dragInfo.SourceItem).Tag is string)
                return true;
            return false;
        }

        public void DragCancelled() { }

        public void Dropped(IDropInfo dropInfo) { }

        GongSolutions.Wpf.DragDrop.DefaultDragHandler defDrag_ = new DefaultDragHandler();
        public void StartDrag(IDragInfo dragInfo)
        {
            defDrag_.StartDrag(dragInfo);
        }

        public bool TryCatchOccurredException(Exception exception)
        {
            return false;
        }

        TreeViewItem CreateItemFor(Type t)
        {
            DockPanel dock = new DockPanel();
            Image img = new Image();
            img.Margin = new Thickness(0, 0, 6, 0);
            img.MaxWidth = 16;
            img.MaxHeight = 16;
            img.Source = GraphParts.TextureGraphNode.IconConverter.GetIcon(t);
            Label lbl = new Label { Content = t.Name.Replace("Node", "").SplitCamelCase() };

            DockPanel.SetDock(img, Dock.Left);
            DockPanel.SetDock(lbl, Dock.Right);
            dock.Children.Add(img);
            dock.Children.Add(lbl);

            bool isDemoRestricted = false;
#if IS_DEMO
            isDemoRestricted = Data.TexGen.TextureGenDocument.DemoRestricted.Contains(t);
            if (isDemoRestricted)
                lbl.Foreground = new SolidColorBrush(Colors.Red);
#endif

            string tip = null;
            var descAttr = t.GetCustomAttribute<DescriptionAttribute>();
            if (descAttr != null)
            {
                tip = descAttr.Description;
                if (isDemoRestricted)
                    tip = "DEMO LOCKED: " + tip;
            }
            else if (isDemoRestricted)
                tip = "Unavailable in demo";

            TreeViewItem subItem = new TreeViewItem { Header = dock, ToolTip = tip };
            subItem.Tag = t;
            subItem.DataContext = t;
            return subItem;
        }

        TreeViewItem CreateItemFor(Type t, string display, object tag)
        {
            DockPanel dock = new DockPanel();
            Image img = new Image();
            img.Margin = new Thickness(0, 0, 6, 0);
            img.MaxWidth = 16;
            img.MaxHeight = 16;
            img.Source = GraphParts.TextureGraphNode.IconConverter.GetIcon(t);
            Label lbl = new Label { Content = display };

            bool isDemoRestricted = false;
#if IS_DEMO
            isDemoRestricted = Data.TexGen.TextureGenDocument.DemoRestricted.Contains(t);
            if (isDemoRestricted)
                lbl.Foreground = new SolidColorBrush(Colors.Red);
#endif

            DockPanel.SetDock(img, Dock.Left);
            DockPanel.SetDock(lbl, Dock.Right);
            dock.Children.Add(img);
            dock.Children.Add(lbl);

            string tip = null;
            var descAttr = t.GetCustomAttribute<DescriptionAttribute>();
            if (descAttr != null)
            {
                tip = descAttr.Description;
                if (isDemoRestricted)
                    tip = "DEMO LOCKED: " + tip;
            }
            else if (isDemoRestricted)
                tip = "Unavailable in demo";

            TreeViewItem subItem = new TreeViewItem { Header = dock, ToolTip = tip };
            subItem.Tag = tag;
            subItem.DataContext = t;
            return subItem;
        }

        private void tree_Loaded(object sender, RoutedEventArgs e)
        {
            if (tree.Items.IsEmpty)
            {
                TreeViewItem Blend = new TreeViewItem { Header = "Blend", Tag = typeof(Data.TexGen.BlendNode), DataContext = typeof(Data.TexGen.BlendNode) };
                tree.Items.Add(Blend);
                foreach (var grp in TextureGenDocument.NodeGroups)
                {
                    TreeViewItem item = new TreeViewItem { Header = grp.Name };
                    foreach (var type in grp.Types)
                    {
                        TreeViewItem subItem = CreateItemFor(type);
                        item.Items.Add(subItem);
                    }
                    tree.Items.Add(item);
                }

                prefabsItem = new TreeViewItem { Header = "Prefabs" };
                tree.Items.Add(prefabsItem);
                ResetPrefabs();
            }
        }
    }
}
