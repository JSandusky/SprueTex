using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Xml;

using SprueKit.Graphics.SprueModel;

namespace SprueKit.Data
{
    public class SprueModelDocument : Document
    {
        public SprueModel DataRoot { get; private set; } = new SprueModel();
        public GenericTreeObject ResultRoot { get; private set; } = new GenericTreeObject();

        public ModelScene ModelScene { get; private set; }
        public UVScene UVScene { get; private set; }

        public SprueModelDocument()
        {
            ErrorHandler.inst().Info("Created new Sprue Model");
            ChainPiece initChain = new ChainPiece { Parent = DataRoot, SmoothingLevels = 3, IsSpine = true };
            DataRoot.Children.Add(initChain);

            Random r = new Random();
            float z = -16;
            for (int i = 0; i < 5; ++i)
            {
                float radius = (float)r.NextDouble() * 3 + 2;
                float yPos = (float)r.NextDouble() * 20;
                ChainPiece.ChainBone a = new ChainPiece.ChainBone { Parent = initChain, CrossSection = new Vector3(radius, radius, radius), Position = new Vector3(0, yPos, z) };
                initChain.Bones.Add(a);
                z += 8;
            }

            CommonConstruct(null);
        }

        public SprueModelDocument(Uri filePath, bool isTemplate = false)
        {
            string path = filePath.AbsolutePath;
            if (!isTemplate)
                FileURI = filePath;
            if (string.IsNullOrEmpty(path))
                throw new Exception(string.Format("Cannot open file {0}", filePath));

            SerializationContext ctx = new SerializationContext(new Uri(System.IO.Path.GetDirectoryName(path)));
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            // load XML file
            if (path.EndsWith(".xml") || path.EndsWith(".sxml"))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);
                DataRoot = SprueModel.ReadFrom(ctx, doc.DocumentElement.SelectSingleNode("spruemodel") as XmlElement);
                parameters["camera_view"] = doc.DocumentElement.GetStringElement("camera_view", "").ToMatrix();
                parameters["view_mode"] = doc.DocumentElement.GetEnumElement<SprueKit.Graphics.Materials.RenderStyle>("view_mode", Graphics.Materials.RenderStyle.MatCap);
                parameters["draw_helpers"] = bool.Parse(doc.DocumentElement.GetStringElement("draw_helpers", "true"));
                parameters["draw_bones"] = bool.Parse(doc.DocumentElement.GetStringElement("draw_bones", "true"));
                parameters["draw_control_points"] = bool.Parse(doc.DocumentElement.GetStringElement("draw_control_points", "true"));
            }
            else
            {
                using (System.IO.FileStream stream = new System.IO.FileStream(path, System.IO.FileMode.Open))
                {
                    using (System.IO.BinaryReader reader = new System.IO.BinaryReader(stream))
                    {
                        DataRoot = SprueModel.ReadFrom(ctx, reader);
                    }
                }

                if (DataRoot == null)
                    throw new Exception(string.Format("Cannot open file {0}, failed deserialization", filePath));
            }

            if (ctx.BrokenPaths.Count > 0)
            {
                Dlg.PathFixupDlg dlg = new Dlg.PathFixupDlg(ctx);
                dlg.ShowDialog();
            }

            CommonConstruct(parameters);
            IsDirty = isTemplate;
            ErrorHandler.inst().Info(string.Format("Opened sprue model {0}", filePath));
        }

        void CommonConstruct(Dictionary<string, object> parameters)
        {
            DocumentTypeName = "Sprue Model";
            FileMask = "Sprue Model XML (*.xml)|*.xml|Sprue Model Binary (*.sprm)|*.sprm";

            var sceneTree = new SprueKit.Controls.SceneTree(this);
            sceneTree.tree.ItemsSource = new List<SpruePiece> { DataRoot };

            var genericTree = new SprueKit.Controls.GenericTreeControl(this);
            genericTree.DataContext = ResultRoot;
            
            TabControl tabs = new TabControl();
            tabs.Items.Add(new TabItem { Content = sceneTree, Header = "Model" });
            tabs.Items.Add(new TabItem { Content = genericTree, Header = "Output" });

            //Controls.LeftPanelControl = sceneTree;
            Controls.LeftPanelControl = tabs;
            Grid g = new Grid();

            // Scene views
            var sceneView = new Graphics.BaseScene();
            ModelScene = new ModelScene(sceneView, DataRoot);

            if (parameters != null && parameters.Count > 0)
            {
                ModelScene.RenderMode = parameters.TryFetch("view_mode", Graphics.Materials.RenderStyle.MatCap);
                ModelScene.DrawHelpers = parameters.TryFetch("draw_helpers", true);
                ModelScene.DrawBones = parameters.TryFetch("draw_bones", true);
                ModelScene.DrawControlPoints = parameters.TryFetch("draw_control_points", true);
            }

            sceneView.ActiveViewport = ModelScene;
            UVScene = new UVScene(sceneView, DataRoot);

            var gizmoCtrlBox = new SprueKit.Graphics.Controls.GizmoControlBox();
            ModelScene.SetBinding(ModelScene.GizmoModeProperty, new Binding("Mode") { Source = gizmoCtrlBox, Mode = BindingMode.OneWay });
            ModelScene.SetBinding(ModelScene.GizmoWorldSpaceProperty, new Binding("WorldMode") { Source = gizmoCtrlBox, Mode = BindingMode.OneWay });

            Button viewSwitch = new Button { Content = new Image { Source = WPFExt.GetEmbeddedImage("Images/godot/icon_visible.png"), Width = 16, Height = 16 }, Focusable = false };
            viewSwitch.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            viewSwitch.Click += (o, evt) =>
            {
                var popup = PopupHelper.Create();
                var stack = new StackPanel { Orientation = Orientation.Vertical, Margin = new System.Windows.Thickness(5) };
                popup.Grid.MinWidth = 150;
                popup.Grid.Children.Add(stack);

                Graphics.ViewportDelegate[] views = { ModelScene, UVScene };
                stack.Children.Add(new Label { Content = "View", HorizontalAlignment = System.Windows.HorizontalAlignment.Center });
                foreach (var view in views)
                {
                    ToggleButton btn = new ToggleButton { Content = view.ViewportName };
                    btn.SetBinding(ToggleButton.IsCheckedProperty, new Binding("ActiveViewport") { Source = sceneView, Converter = new Controls.Converters.BoolValueCompareConverter(), ConverterParameter = view, Mode = BindingMode.OneWay });
                    btn.Click += (ooo, eeevt) => { sceneView.ActiveViewport = view; };
                    stack.Children.Add(btn);
                }

                // Stack of controls for model view
                {
                    StackPanel styleStack = new StackPanel { Orientation = Orientation.Vertical, Margin = new System.Windows.Thickness(0, 10, 0, 0) };
                    styleStack.SetBinding(StackPanel.VisibilityProperty, new Binding("ActiveViewport") { Source = sceneView, Converter = new Controls.Converters.VisibilityValueCompareConverter(), ConverterParameter = ModelScene, Mode = BindingMode.OneWay });
                    stack.Children.Add(styleStack);
                    styleStack.Children.Add(new Label { Content = "Render Style", HorizontalAlignment = System.Windows.HorizontalAlignment.Center });
                    Graphics.Materials.RenderStyle[] viewModes = { Graphics.Materials.RenderStyle.Wireframe, Graphics.Materials.RenderStyle.MatCap, Graphics.Materials.RenderStyle.Normals, Graphics.Materials.RenderStyle.UVStretch, Graphics.Materials.RenderStyle.BoneWeights, Graphics.Materials.RenderStyle.Textured };
                    foreach (var style in viewModes)
                    {
                        ToggleButton b = new ToggleButton { Content = style.ToString() };
                        b.SetBinding(ToggleButton.IsCheckedProperty, new Binding("RenderMode") { Source = ModelScene, Converter = new Controls.Converters.EnumValueConverter(), ConverterParameter = style.ToString() });
                        b.Click += (ooo, eeevt) => { ModelScene.RenderMode = style; };
                        styleStack.Children.Add(b);
                    }

                    styleStack.Children.Add(new Label { Content = "Active Channel",
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                        Margin = new System.Windows.Thickness(0, 10, 0, 0)
                    });
                    ComboBox texModeCombo = new ComboBox();
                    texModeCombo.Items.Add("Physically Based");
                    texModeCombo.Items.Add("Diffuse");
                    texModeCombo.Items.Add("Normal Map");
                    texModeCombo.Items.Add("Roughness");
                    texModeCombo.Items.Add("Metalness");
                    texModeCombo.Items.Add("Displacement");
                    texModeCombo.SelectedIndex = (int)ModelScene.RenderChannel;
                    styleStack.Children.Add(texModeCombo);
                    texModeCombo.SelectionChanged += (co, ce) => { ModelScene.RenderChannel = (Graphics.Materials.RenderTextureChannel)texModeCombo.SelectedIndex; ModelScene.RenderMode = Graphics.Materials.RenderStyle.Textured; };

                    CheckBox drawHelpers = new CheckBox { Content = "Show Helpers", Margin = new System.Windows.Thickness(0, 10, 0, 0) };
                    drawHelpers.SetBinding(CheckBox.IsCheckedProperty, new Binding("DrawHelpers") { Source = ModelScene });
                    styleStack.Children.Add(drawHelpers);

                    CheckBox drawCtrlPoints = new CheckBox { Content = "Show Control Points" };
                    drawCtrlPoints.SetBinding(CheckBox.IsCheckedProperty, new Binding("DrawControlPoints") { Source = ModelScene });
                    styleStack.Children.Add(drawCtrlPoints);

                    CheckBox drawBones = new CheckBox { Content = "Show Bones" };
                    drawBones.SetBinding(CheckBox.IsCheckedProperty, new Binding("DrawBones") { Source = ModelScene });
                    styleStack.Children.Add(drawBones);

                    CheckBox animateLight = new CheckBox { Content = "Animate Light", ToolTip = "Lighting for PBR display will be animated" };
                    animateLight.SetBinding(CheckBox.IsCheckedProperty, new Binding("AnimateLight") { Source = ModelScene });
                    styleStack.Children.Add(animateLight);
                }

                // Stack of controls for UV view
                {
                    StackPanel styleStack = new StackPanel { Orientation = Orientation.Vertical, Margin = new System.Windows.Thickness(0, 10, 0, 0) };
                    styleStack.SetBinding(StackPanel.VisibilityProperty, new Binding("ActiveViewport") { Source = sceneView, Converter = new Controls.Converters.VisibilityValueCompareConverter(), ConverterParameter = UVScene, Mode = BindingMode.OneWay });
                    stack.Children.Add(styleStack);
                    styleStack.Children.Add(new Label { Content = "Render Style", HorizontalAlignment = System.Windows.HorizontalAlignment.Center });

                    foreach (var mode in Enum.GetValues(typeof(Graphics.Materials.UVRenderMode)))
                    {
                        Graphics.Materials.UVRenderMode uvMode = (Graphics.Materials.UVRenderMode)mode;
                        ToggleButton b = new ToggleButton { Content = mode.ToString() };
                        b.SetBinding(ToggleButton.IsCheckedProperty, new Binding("DrawingMode") { Source = UVScene, Converter = new Controls.Converters.EnumValueConverter(), ConverterParameter = mode.ToString() });
                        b.Click += (ooo, eeevt) => { UVScene.DrawingMode = uvMode; };
                        styleStack.Children.Add(b);
                    }
                }

                popup.ShowAtMouse(false, viewSwitch);
            };

            StackPanel sp = new StackPanel { Orientation = Orientation.Vertical, HorizontalAlignment = System.Windows.HorizontalAlignment.Right, Margin = new System.Windows.Thickness(10) };

            sp.Children.Add(viewSwitch);
            Label polyCount = new Label { ContentStringFormat = "Tris: {0}", HorizontalAlignment = System.Windows.HorizontalAlignment.Right };
            polyCount.SetBinding(Label.ContentProperty, new Binding("TotalTriangleCount") { Source = DataRoot });
            Label vertCount = new Label { ContentStringFormat = "Verts: {0}", HorizontalAlignment = System.Windows.HorizontalAlignment.Right };
            vertCount.SetBinding(Label.ContentProperty, new Binding("TotalVertexCount") { Source = DataRoot });

            SprueKit.Controls.CommandInfoRepeater rpt = new SprueKit.Controls.CommandInfoRepeater() { Margin = new System.Windows.Thickness(0, 50, 0, 0), HorizontalAlignment = System.Windows.HorizontalAlignment.Left };

            var chainBoneItems = new Commands.CommandInfo[]
            {
                new Commands.CommandInfo { ShortCut="_D", ToolTip = "Delete Bone", Icon = WPFExt.GetEmbeddedImage("Images/godot/icon_remove.png"), Action = (d, o)=> { ChainPiece chain = (o as ChainPiece.ChainBone).Parent as ChainPiece; if (chain.Bones.Count > 2) chain.Bones.Remove(o as ChainPiece.ChainBone); } },
                new Commands.CommandInfo { ShortCut="_B", ToolTip = "Add Bone Before", Icon = WPFExt.GetEmbeddedImage("Images/icon_insert_before.png"), Action = (d, o)=> { Selection.SetSelected(((ChainPiece)((ChainPiece.ChainBone)o).Parent).AddSpineBoneBefore(o as ChainPiece.ChainBone)); } },
                new Commands.CommandInfo { ShortCut="_N", ToolTip = "Add Bone After", Icon = WPFExt.GetEmbeddedImage("Images/icon_insert_after.png"), Action = (d, o)=> { Selection.SetSelected(((ChainPiece)((ChainPiece.ChainBone)o).Parent).AddSpineBone(o as ChainPiece.ChainBone)); } },
            };

            rpt.SetBinding(SprueKit.Controls.CommandInfoRepeater.TargetProperty, new Binding("MostRecentlySelected") { Source = Selection });
            rpt.SetBinding(SprueKit.Controls.CommandInfoRepeater.ItemsSourceProperty, new Binding("MostRecentlySelected") {
                Source = Selection,
                Converter = new SprueKit.Controls.Converters.FunctionalConverter((o) => {
                    if (o is Data.ChainPiece.ChainBone)
                    {
                        
                        return chainBoneItems;
                    }
                    return null;
                })
            });

            g.Children.Add(sceneView);
            g.Children.Add(gizmoCtrlBox);
            sp.Children.Add(polyCount);
            sp.Children.Add(vertCount);
            g.Children.Add(sp);
            g.Children.Add(rpt);

            Controls.ContentControl = g;
            Controls.Disposables.Add(sceneView);
            Controls.Disposables.Add(ModelScene);
            Controls.Disposables.Add(UVScene);

            Tracker_UndoRedo.Track(DataRoot);
            Tracker_UndoRedo.Changed += Tracker_UndoRedo_Changed;
            UndoRedo.UndoRedoActionPerformed += UndoRedo_UndoRedoActionPerformed;

            DocumentCommands = new Commands.CommandInfo[]
            {
                new Commands.CommandInfo { Name = "Save", Action = (doc, obj) => {
                    this.Save();
                }, ToolTip = "Save the current model", Icon = WPFExt.GetEmbeddedImage("Images/save_white.png") },
                new Commands.CommandInfo { Name = "Export", Action = (doc, obj) => {
                    this.Export();
                }, ToolTip = "Export the model to OBJ/FBX", Icon = WPFExt.GetEmbeddedImage("Images/cabinet_white.png") }
            };

            RegenerateModel();
        }

        private void UndoRedo_UndoRedoActionPerformed(Commands.UndoStack stack)
        {
            RegenerateModel();
        }

        private void Tracker_UndoRedo_Changed(Notify.Tracker tracker, object who, string name)
        {
            var undoRedoStack = Commands.MacroCommandBlock.GetUndoStorage(UndoRedo);

            bool dontWork = false;
            bool texOnly = false;
            IsDirty = true;
            if (who is Notify.PropertyChangedTrackedObject)
            {
                var obj = who as Notify.PropertyChangedTrackedObject;
                var pi = obj.Tracked.GetType().GetProperty(name);
                if (Attribute.GetCustomAttribute(pi, typeof(Notify.DontSignalWorkAttribute)) != null)
                    dontWork = true;
                if (obj is TextureComponent)
                    texOnly = true;
                undoRedoStack.Add(new Commands.BasicPropertyCmd(name, obj.Tracked, pi.GetValue(obj.Snapshot), pi.GetValue(obj.Tracked)));
            }
            else if (who is Notify.CollectionChangedTrackObject)
            {
                var obj = who as Notify.CollectionChangedTrackObject;
                if (obj.RecentAction == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    foreach (var v in obj.RecentArgs.NewItems)
                        undoRedoStack.Add(new Commands.CollectionAddRemoveCmd(true, obj.Tracked as System.Collections.IList, obj.RecentArgs.NewStartingIndex, v, obj.TrackedOwner));
                }
                else if (obj.RecentAction == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                {
                    foreach (var v in obj.RecentArgs.OldItems)
                        undoRedoStack.Add(new Commands.CollectionAddRemoveCmd(false, obj.Tracked as System.Collections.IList, obj.RecentArgs.OldStartingIndex, v, obj.TrackedOwner));
                }
                else if (obj.RecentAction == System.Collections.Specialized.NotifyCollectionChangedAction.Move)
                {

                }
            }
            else
                undoRedoStack.Add(new Commands.SpoofUndoRedoCmd(name));

            if (!dontWork || !texOnly)
                RegenerateModel();
            else if (texOnly)
                App.MainTaskThread.AddTask(new Sprue.TextureMapTask(DataRoot));
        }

        public void RegenerateModel()
        {
            if (DataRoot.UseBMesh)
                App.MainTaskThread.AddTask(new Sprue.BMeshTask(this, DataRoot));
            else if (UserData.inst().MeshingSettings.GPUAcceleration)    
                App.MainTaskThread.AddTask(new Sprue.ComputeMeshingTask(this, DataRoot));
            else
                App.MainTaskThread.AddTask(new Sprue.MeshingTask(this, DataRoot, DataRoot));
        }

        public override bool WriteFile(Uri path)
        {
            if (path == null)
                return false;

            string filePath = path.AbsolutePath;
            if (System.IO.Path.GetExtension(filePath).Equals(".xml"))
            {
                XmlDocument doc = new XmlDocument();
                try
                {
                    SerializationContext ctx = new SerializationContext(new Uri(System.IO.Path.GetDirectoryName(filePath)));

                    var root = doc.CreateElement("spruemodeldocument");
                    doc.AppendChild(root);
                    DataRoot.SaveTo(ctx, doc);
                    if (ModelScene != null)
                    {
                        doc.DocumentElement.AddStringElement("camera_view", ModelScene.Camera.ViewMatrix.ToTightString());
                        doc.DocumentElement.AddEnumElement("view_mode", ModelScene.RenderMode);
                        doc.DocumentElement.AddStringElement("draw_helpers", ModelScene.DrawHelpers.ToString());
                        doc.DocumentElement.AddStringElement("draw_bones", ModelScene.DrawBones.ToString());
                        doc.DocumentElement.AddStringElement("draw_control_points", ModelScene.DrawControlPoints.ToString());

                        SoftwareRenderer render = new SoftwareRenderer(128, 128, 45);
                        List<Vector3> vertexPositions = new List<Vector3>();
                        DataRoot.VisitAll((p) => { vertexPositions.Add(p.Position); });
                        render.FocusOnCloud(vertexPositions);
                        bool anythingWritten = false;
                        if (DataRoot.MeshData != null)
                            anythingWritten |= render.RasterizeTriangles(DataRoot.MeshData.GetIndices(), DataRoot.MeshData.GetVertices(), false);

                        //render.DepthTrace((Ray ray) =>
                        //{
                        //    float retVal = float.MaxValue;
                        //    DataRoot.VisitAllDensitySources((density) =>
                        //    {
                        //        float v = 0.0f;
                        //        if (density.TraceRay(ray, out v))
                        //        {
                        //            if (retVal == float.MaxValue)
                        //                retVal = v;
                        //            else
                        //                retVal = Math.Max(retVal, v);
                        //        }
                        //    });
                        //    DataRoot.VisitAllPickableNonDensity((pickable) =>
                        //    {
                        //        float v = 0.0f;
                        //        if (pickable.DoMousePick(ray, out v))
                        //        {
                        //            if (retVal == float.MaxValue)
                        //                retVal = v;
                        //            else
                        //                retVal = Math.Max(retVal, v);
                        //        }
                        //    });
                        //    return retVal;
                        //});
                        if (anythingWritten)
                        {
                            var img = render.GetDepthImage();
                            doc.DocumentElement.AddImageElement("thumbnail", img);
                            //img.Save("Test.png", System.Drawing.Imaging.ImageFormat.Png);
                        }
                    }
                    doc.Save(path.AbsolutePath);
                    return true;
                }
                catch (Exception ex)
                {
                    ErrorHandler.inst().Error(ex);
                }
            }
            else if (System.IO.Path.GetExtension(filePath).Equals(".sprm"))
            {
                try
                {
                    SerializationContext ctx = new SerializationContext(new Uri(System.IO.Path.GetDirectoryName(filePath)));

                    using (System.IO.FileStream stream = new System.IO.FileStream(filePath, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write))
                    using (System.IO.BinaryWriter writer = new System.IO.BinaryWriter(stream))
                        DataRoot.SaveTo(ctx, writer);
                    return true;
                }
                catch (Exception ex)
                {
                    ErrorHandler.inst().Error(ex);
                }
            }
            return false;
        }

        public override void Export()
        {
            Dlg.Sprue.ExportDlg dlg = new Dlg.Sprue.ExportDlg();
            dlg.ShowDialog();
        }

        public override void Dispose()
        {
            base.Dispose();
            DataRoot.VisitAll((SpruePiece p) =>
            {
                if (p is SprueModel)
                {
                    var mdl = p as SprueModel;
                    if (mdl.MeshData != null)
                        mdl.MeshData.Dispose();
                }
            });
        }
    }

    public class SprueModelDocumentRecord : DocumentRecord
    {
        public override string DocumentName { get { return "Sprue Model"; } }

        public override string OpenFileMask { get { return "Sprue Model (*.sxml; *.sprm)|*.sxml;*.sprm"; } }

        public override Document CreateFromTemplate(Uri templateUri)
        {
            return new SprueModelDocument(templateUri, true);
        }

        public override Document CreateNewDocument()
        {
            return new SprueModelDocument();
        }

        public override Document OpenDocument(Uri documentUri)
        {
            return new SprueModelDocument(documentUri, false);
        }

        public override KeyValuePair<string, string>[] GetSignificantReports()
        {
            return new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string,string>("Model Report", "DETAILS"),
            };
        }

        public override void DoReport(string value)
        {
            
        }
    }
}
