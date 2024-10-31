using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using SprueKit.Data;
using GongSolutions.Wpf.DragDrop;
using System.Globalization;

namespace SprueKit.Controls
{
    public class SceneTreeIconConverter : IValueConverter
    {
        static Dictionary<Type, string> Paths = new Dictionary<Type, string>
        {
            { typeof(SprueKit.Data.SprueModel),             string.Format("pack://application:,,,/{0};component/Images/puzzle_white.png", App.AppName) },
            { typeof(SprueKit.Data.SimplePiece),            string.Format("pack://application:,,,/{0};component/Images/icon_box_shape_white.png", App.AppName) },
            { typeof(SprueKit.Data.InstancePiece),          string.Format("pack://application:,,,/{0};component/Images/godot/icon_instance.png", App.AppName) },
            { typeof(SprueKit.Data.ChainPiece),             string.Format("pack://application:,,,/{0};component/Images/icon_path_white.png", App.AppName) },
            { typeof(SprueKit.Data.ChainPiece.ChainBone),   string.Format("pack://application:,,,/{0};component/Images/icon_bone_white.png", App.AppName) },
            { typeof(SprueKit.Data.ModelPiece),             string.Format("pack://application:,,,/{0};component/Images/icon_mesh_white.png", App.AppName) },
            { typeof(SprueKit.Data.MarkerPiece),            string.Format("pack://application:,,,/{0};component/Images/icon_point_white.png", App.AppName) },

            { typeof(SprueKit.Data.DecalTextureComponent),      string.Format("pack://application:,,,/{0};component/Images/icon_plane_green.png", App.AppName) },
            { typeof(SprueKit.Data.ColorCubeTextureComponent),  string.Format("pack://application:,,,/{0};component/Images/color_cube.png", App.AppName) },
            { typeof(SprueKit.Data.BoxTextureComponent),        string.Format("pack://application:,,,/{0};component/Images/decal_cube.png", App.AppName) },
            { typeof(SprueKit.Data.GradientTextureComponent),   string.Format("pack://application:,,,/{0};component/Images/gradient_cube.png", App.AppName) },
            { typeof(SprueKit.Data.DomeTextureComponent),       string.Format("pack://application:,,,/{0};component/Images/texture_dome.png", App.AppName) },
            { typeof(SprueKit.Data.CylinderTextureComponent),   string.Format("pack://application:,,,/{0};component/Images/cylinder_green.png", App.AppName) },
        };

        static Dictionary<string, Type> StrPaths = new Dictionary<string, Type>();

        static Dictionary<Type, BitmapImage> Bitmaps = new Dictionary<Type, BitmapImage>();


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (StrPaths.Count == 0)
            {
                foreach (var kvp in Paths)
                    StrPaths[kvp.Key.Name] = kvp.Key;
            }

            Type valueType = null;
            if (parameter != null)
                valueType = StrPaths[parameter.ToString()];
            else
                valueType = value.GetType();

            string uriPath = null;
            if (valueType != null)
            {
                if (Paths.TryGetValue(valueType, out uriPath))
                {
                    BitmapImage bmp = null;
                    if (Bitmaps.TryGetValue(valueType, out bmp))
                        return bmp;

                    try
                    {
                        bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.UriSource = new Uri(uriPath, UriKind.RelativeOrAbsolute);
                        bmp.EndInit();
                        Bitmaps[valueType] = bmp;
                        return bmp;
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Interaction logic for SceneTree.xaml
    /// </summary>
    public partial class SceneTree : UserControl, IDocumentControl, IDropTarget, IDragSource
    {
        bool selectionSignalBlocked = false;

        public EventHandler<object> SelectionChanged;

        SelectionContext selectionContext;
        //SprueKit.Behaviors.TreeViewMultipleSelectionAttached selectionBehave;

        public SceneTree(Document document)
        {
            InitializeComponent();
            //System.Windows.Interactivity.Interaction.GetBehaviors(tree).Add(selectionBehave = new Behaviors.TreeViewMultipleSelectionAttached());

            Document = document;
            tree.PreviewMouseDown += Tree_PreviewMouseDown;
            selectionContext = document.Selection;
            //selectionBehave.SelectedItems = selectionContext.Selected;

            selectionContext.Selected.CollectionChanged += (o, e) =>
            {
                selectionSignalBlocked = true;
                //selectionBehave.SelectedItems = selectionContext.Selected;
                tree.MatchSelection(selectionContext);
                selectionSignalBlocked = false;
            };
        }

        private void Tree_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && tree.SelectedItem != null)
            {
                //selectionBehave.SelectedItems = new List<object>();
                tree.ClearTreeSelection();
                e.Handled = true;
            }
        }

        private void tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (selectionSignalBlocked == false)
            {
                //if (SelectionChanged != null)
                //    SelectionChanged(this, tree.SelectedItem);
                //if (selectionBehave.SelectedItems.Count > 0)
                //    Document.Selection.SetSelected(selectionBehave.SelectedItems[0]);
                //System.Collections.IList selected = selectionBehave.SelectedItems;
                //if (selected != null && selected.Count > 0)
                //{
                //    Document.Selection.SetSelected(selected);
                //}
                Document.Selection.SetSelected(tree.SelectedItem);
            }
        }

        public Document Document { get; set; }

        void onDelete(object sender, EventArgs e)
        {
            MenuItem item = sender as MenuItem;
            Data.SpruePiece target = item.DataContext as Data.SpruePiece;
            doDelete(target);
        }

        void doDelete(SpruePiece target)
        { 
            if (target.Parent != null)
            {
                var list = target.Parent.GetAppropriateList(target);
                if (target is ChainPiece.ChainBone && list.Count <= 2)
                    return;
                list.Remove(target);
            }
        }

        void onNewSimplePiece(object sender, EventArgs e)
        {
            MenuItem item = sender as MenuItem;
            Data.SpruePiece target = item.DataContext as Data.SpruePiece;
            target.Children.Add(new SimplePiece() { Parent = target });
        }

        void onNewSpinePiece(object sender, EventArgs e)
        {
            MenuItem item = sender as MenuItem;
            Data.SpruePiece target = item.DataContext as Data.SpruePiece;
            var p = ChainPiece.New();
            p.Parent = target;
            target.Children.Add(p);
        }

        void onNewReference(object sender, EventArgs e)
        {
            MenuItem item = sender as MenuItem;
            Data.SpruePiece target = item.DataContext as Data.SpruePiece;
            target.Children.Add(new InstancePiece() { Parent = target });
        }

        void onNewMesh(object sender, EventArgs e)
        {
            MenuItem item = sender as MenuItem;
            Data.SpruePiece target = item.DataContext as Data.SpruePiece;
            target.Children.Add(new ModelPiece() { Parent = target });
        }

        void onNewMarker(object sender, EventArgs e)
        {
            MenuItem item = sender as MenuItem;
            Data.SpruePiece target = item.DataContext as Data.SpruePiece;
            target.Children.Add(new MarkerPiece() { Parent = target });
        }

        void onNewDecal(object sender, EventArgs e)
        {
            MenuItem item = sender as MenuItem;
            Data.SpruePiece target = item.DataContext as Data.SpruePiece;
            target.Components.Add(new DecalTextureComponent());
        }
        void onNewBoxProjector(object sender, EventArgs e)
        {
            MenuItem item = sender as MenuItem;
            Data.SpruePiece target = item.DataContext as Data.SpruePiece;
            target.Components.Add(new BoxTextureComponent() { Parent = target });
        }
        void onNewCylinder(object sender, EventArgs e)
        {
            MenuItem item = sender as MenuItem;
            Data.SpruePiece target = item.DataContext as Data.SpruePiece;
            target.Components.Add(new CylinderTextureComponent() { Parent = target });
        }
        void onNewDecalStrip(object sender, EventArgs e)
        {
            MenuItem item = sender as MenuItem;
            Data.SpruePiece target = item.DataContext as Data.SpruePiece;
            target.Components.Add(new DecalStripTextureComponent() { Parent = target });
        }
        void onNewHemisphere(object sender, EventArgs e)
        {
            MenuItem item = sender as MenuItem;
            Data.SpruePiece target = item.DataContext as Data.SpruePiece;
            target.Components.Add(new DomeTextureComponent() { Parent = target });
        }
        void onNewGradientMap(object sender, EventArgs e)
        {
            MenuItem item = sender as MenuItem;
            Data.SpruePiece target = item.DataContext as Data.SpruePiece;
            target.Components.Add(new GradientTextureComponent() { Parent = target });
        }
        void onNewColorCube(object sender, EventArgs e)
        {
            MenuItem item = sender as MenuItem;
            Data.SpruePiece target = item.DataContext as Data.SpruePiece;
            target.Components.Add(new ColorCubeTextureComponent() { Parent = target });
        }

        void onRegenerateMesh(object sender, EventArgs e)
        {
            var modelDoc = Document as SprueKit.Data.SprueModelDocument;
            if (modelDoc != null)
                modelDoc.RegenerateModel();
        }

        void onAddChainBone(object sender, EventArgs e)
        {
            selectionSignalBlocked = true;
            MenuItem item = sender as MenuItem;
            Data.ChainPiece target = item.DataContext as Data.ChainPiece;
            Data.ChainPiece.ChainBone newBone = null;
            if (target != null)
                newBone = target.AddSpineBone();
            selectionSignalBlocked = false;
            if (newBone != null)
                Document.Selection.SetSelected(newBone);
        }

        void onAddBoneAfter(object sender, EventArgs e)
        {
            selectionSignalBlocked = true;
            MenuItem item = sender as MenuItem;
            Data.ChainPiece.ChainBone target = item.DataContext as Data.ChainPiece.ChainBone;
            Data.ChainPiece.ChainBone newBone = null;
            if (target != null && target.Parent != null)
                newBone = ((Data.ChainPiece)target.Parent).AddSpineBone(target);
            else
                ErrorHandler.inst().PublishError("ChainBone has been corrupted, loss of parent information", 2);

            selectionSignalBlocked = false;
            if (newBone != null)
                Document.Selection.SetSelected(newBone);
        }

        void onAddBoneBefore(object sender, EventArgs e)
        {
            selectionSignalBlocked = true;
            MenuItem item = sender as MenuItem;
            Data.ChainPiece.ChainBone target = item.DataContext as Data.ChainPiece.ChainBone;
            Data.ChainPiece.ChainBone newBone = null;
            if (target != null && target.Parent != null)
                newBone = ((Data.ChainPiece)target.Parent).AddSpineBoneBefore(target);
            else
                ErrorHandler.inst().PublishError("ChainBone has been corrupted, loss of parent information", 2);

            selectionSignalBlocked = false;
            if (newBone != null)
                Document.Selection.SetSelected(newBone);
        }

        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            var dropPiece = dropInfo.TargetItem as SprueKit.Data.SpruePiece;
            if (dropPiece == null || dropPiece.IsLocked)
                return;

            if (dropInfo.Data is DataObject)
            {
                var data = dropInfo.Data as DataObject;
                if (data.ContainsFileDropList())
                {
                    var files = data.GetFileDropList();
                    bool anyGood = false;
                    foreach (var file in files)
                    {
                        if (file.EndsWith(".obj") || file.EndsWith(".fbx"))
                            anyGood = true;
                    }
                    if (anyGood)
                    {
                        dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                        dropInfo.Effects = DragDropEffects.Link;
                    }
                }
                return;
            }
            else if (dropInfo.Data is FileData)
            {
                var data = dropInfo.Data as FileData;
                if (data.FilePath.EndsWith(".fbx") || data.FilePath.EndsWith(".obj"))
                {
                    dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                    dropInfo.Effects = DragDropEffects.Link | DragDropEffects.Move;
                }
                return;
            }

            var dragPiece = dropInfo.Data as SprueKit.Data.SpruePiece;

            if (dragPiece == dropPiece)
                return;

            if (dragPiece.Parent == null)
                return;

            if (dragPiece != null && dropPiece != null)
            {
                if (dropInfo.InsertPosition.HasFlag(RelativeInsertPosition.TargetItemCenter))
                    dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                else
                    dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                dropInfo.Effects = DragDropEffects.Move;
            }
        }

        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            var dropPiece = dropInfo.TargetItem as SprueKit.Data.SpruePiece;
            if (dropPiece == null)
                return;

            if (dropInfo.Data is DataObject)
            {
                var data = dropInfo.Data as DataObject;
                if (data.ContainsFileDropList())
                {
                    var files = data.GetFileDropList();
                    foreach (var file in files)
                    {
                        if (file.EndsWith(".fbx") || file.EndsWith(".obj"))
                        {
                            ModelPiece model = new ModelPiece { Parent = dropPiece };
                            model.ModelFile.ModelFile = new Uri(file);
                            dropPiece.Children.Add(model);
                        }
                    }
                }
                return;
            }
            else if (dropInfo.Data is FileData)
            {
                var data = dropInfo.Data as FileData;
                if (data.FilePath.EndsWith(".fbx") || data.FilePath.EndsWith(".obj"))
                {
                    ModelPiece model = new ModelPiece { Parent = dropPiece };
                    model.ModelFile.ModelFile = new Uri(data.FilePath);
                    dropPiece.Children.Add(model);
                }
                return;
            }

            var dragPiece = dropInfo.Data as SprueKit.Data.SpruePiece;

            if (dragPiece == dropPiece)
                return;

            if (dragPiece != null && dropPiece != null)
            {
                if ((Keyboard.Modifiers & ModifierKeys.Alt) != 0)
                {
                    dragPiece = dragPiece.Clone();
                    if (dragPiece != null)
                    {
                        //??WTF var dragPieceList = dragPiece.Parent.GetAppropriateList(dragPiece);

                        if (dropInfo.InsertPosition.HasFlag(RelativeInsertPosition.TargetItemCenter))
                        {
                            dragPiece.Parent = dropPiece;
                            dragPiece.Parent.Children.Add(dragPiece);
                        }
                        else
                        {
                            dragPiece.Parent = dropPiece.Parent;
                            dragPiece.Parent.Children.Insert(dragPiece.Parent.Children.IndexOf(dropPiece) + 1, dragPiece);
                        }
                    }
                    return;
                }

                if (dropInfo.InsertPosition.HasFlag(RelativeInsertPosition.TargetItemCenter))
                {
                    var dragPieceList = dragPiece.Parent.GetAppropriateList(dragPiece);
                    var dropPieceList = dropPiece.GetAppropriateList(dropPiece);
                    if (dragPieceList == null || dropPieceList == null)
                        return;

                    var cmd = new Commands.CollectionMoveCmd(new List<object> { dragPiece }, 
                        dragPieceList, dragPiece.Parent, dragPieceList.IndexOf(dragPiece),
                        dropPieceList, dropPiece, dropPiece.Parent != null ? dropPieceList.Count : 0);

                    Document.UndoRedo.Add(cmd);

                    using (var v = new Notify.Tracker.TrackingSideEffects())
                    {
                        cmd.Redo();
                        //dragPieceList.Remove(dragPiece);
                        //dragPiece.Parent = dropPiece;
                        //dropPieceList.Add(dragPiece);
                    }
                }
                else
                {
                    System.Collections.IList dragPieceList = dragPiece.Parent.GetAppropriateList(dragPiece);
                    System.Collections.IList dropPieceList = dropPiece.Parent.GetAppropriateList(dragPiece);
                    if (dragPieceList == null || dropPieceList == null)
                        return;

                    var cmd = new Commands.CollectionMoveCmd(new List<object> { dragPiece }, 
                        dragPieceList, dragPiece.Parent, dragPieceList.IndexOf(dragPiece),
                        dropPieceList, dropPiece.Parent, dropInfo.InsertIndex);

                    Document.UndoRedo.Add(cmd);

                    using (var v = new Notify.Tracker.TrackingSideEffects())
                    {
                        cmd.Redo();
                        //dragPieceList.Remove(dragPiece);
                        //dragPiece.Parent = dropPiece.Parent;
                        //if (dropInfo.InsertIndex < dropPiece.Parent.Children.Count)
                        //    dropPieceList.Insert(dropInfo.InsertIndex, dragPiece);
                        //else
                        //    dropPieceList.Add(dragPiece);
                    }
                }
            }
        }

        private void tree_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem tvi = e.OriginalSource as TreeViewItem;
            if (tvi != null)
                ((SpruePiece)tvi.DataContext).Expanded = tvi.IsExpanded;

            tree.ZebraStripe();
        }

        private void onCopy(object sender, RoutedEventArgs e)
        {
            SpruePiece piece = null;

            if (sender is MenuItem)
                piece = (sender as MenuItem).DataContext as SpruePiece;
            else if (sender is TreeView)
                piece = ((TreeView)sender).SelectedValue as SpruePiece;

            if (piece != null && !(piece is SprueModel))
            {
                var clone = piece.Clone();
                if (clone != null)
                {
                    clipObject = clone;
                    Clipboard.SetData("SCENE_TREE", "LOCAL_OBJECT");
                    ErrorHandler.inst().Info(string.Format("Copied {0} to clipboard", clone.Name));
                    return;
                }
                clipObject = null;
            }
        }
        object clipObject;
        private void onPaste(object sender, RoutedEventArgs e)
        {
            SpruePiece target = null;
            if (sender is MenuItem)
                target = (sender as MenuItem).DataContext as SpruePiece;
            else if (sender is TreeView)
                target = ((TreeView)sender).SelectedValue as SpruePiece;
            else
                target = tree.SelectedValue as SpruePiece;

            if (target is SprueComponent)
                target = target.Parent;

            if (target != null && clipObject != null && Clipboard.ContainsData("SCENE_TREE"))
            {
                var data = clipObject as SpruePiece;
                var codeData = Clipboard.GetData("SCENE_TREE").ToString();
                if (codeData.Equals("LOCAL_OBJECT"))
                {
                    var newData = data.Clone();
                    newData.Parent = target;
                    target.GetAppropriateList(newData).Add(newData);
                }
            }
        }

        private void canPaste(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
            e.Handled = true;

            SpruePiece target = null;
            if (sender is MenuItem)
                target = (sender as MenuItem).DataContext as SpruePiece;
            else if (sender is TreeView)
                target = ((TreeView)sender).SelectedValue as SpruePiece;

            if (target is SprueComponent)
                target = target.Parent;

            if (target != null && clipObject != null && Clipboard.ContainsData("SCENE_TREE"))
            {
                var data = clipObject as SpruePiece;
                var codeData = Clipboard.GetData("SCENE_TREE").ToString();
                if (codeData.Equals("LOCAL_OBJECT"))
                    e.CanExecute = true;
            }
        }

        private void tree_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete || e.Key == Key.Back)
            {
                var selected = tree.SelectedItem as SpruePiece;
                if (selected != null && selected.Parent != null)
                    doDelete(selected);
            }
        }

        GongSolutions.Wpf.DragDrop.DefaultDragHandler defDrag_ = new DefaultDragHandler();
        public void StartDrag(IDragInfo dragInfo)
        {
            defDrag_.StartDrag(dragInfo);
            //SpruePiece piece = dragInfo.SourceItem as SpruePiece;
            //if (piece != null)
            //{
            //    dragInfo.Data = piece;
            //}
        }

        public bool CanStartDrag(IDragInfo dragInfo)
        {
            SpruePiece piece = dragInfo.SourceItem as SpruePiece;
            if (piece != null)
                return !piece.IsLocked;
            return true;
        }

        public void Dropped(IDropInfo dropInfo)
        {
            
        }

        public void DragCancelled()
        {
            
        }

        public bool TryCatchOccurredException(Exception exception)
        {
            throw new NotImplementedException();
        }
    }
}
