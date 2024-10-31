using FirstFloor.ModernUI.Windows.Controls;
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
using System.Windows.Shapes;

using System.Windows.Forms;

namespace SprueKit.Dlg.Sprue
{
    /// <summary>
    /// Interaction logic for ExportDlg.xaml
    /// </summary>
    public partial class ExportDlg : ModernDialog
    {
        public ExportDlg()
        {
            InitializeComponent();

            Buttons = new List<System.Windows.Controls.Button>
            {
                new System.Windows.Controls.Button
                {
                    Content = "Export",
                    IsDefault = true,
                    Style = FindResource("StyledButton") as Style,
                },
                new System.Windows.Controls.Button
                {
                    Content = "Close",
                    IsCancel = true,
                    Style = FindResource("StyledButton") as Style,
                }
            };

            Buttons.First().Click += ExportDlg_Click;
            Buttons.Last().Click += (o, e) => { Close(); };
        }

        private void ExportDlg_Click(object sender, RoutedEventArgs e)
        {
            var docMan = new IOCDependency<DocumentManager>().Object;
            if (docMan != null && docMan.ActiveDocument != null && docMan.ActiveDocument is Data.SprueModelDocument)
            {
                var modelDoc = docMan.ActiveDocument as Data.SprueModelDocument;
                var model = Data.BindingUtil.ToModel(modelDoc.DataRoot);
                if (model.Meshes.Count == 0)
                    return;

                System.Windows.Forms.SaveFileDialog dlg = new SaveFileDialog();
                switch (exporterCombo.SelectedIndex)
                {
                    case 0: // Autodesk FBX 2016
                    case 1: // Autodesk FBX 2013
                        {
                            dlg.AddExtension = true;
                            dlg.Filter = "Autodesk FBX Model (*.fbx)|*.fbx";
                            dlg.Title = "Save Model";
                            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            {
                                int fbxType = exporterCombo.SelectedIndex;
                                string fileName = dlg.FileName;
                                Parago.Windows.ProgressDialogResult result = Parago.Windows.ProgressDialog.Execute(null, "Exporting", "Exporting model...", (a, b) =>
                                {
                                    SprueBindings.ModelData.SaveFBX(model, fileName, fbxType == 0, ErrorHandler.inst());
                                    if (modelDoc.DataRoot.MeshData != null && modelDoc.DataRoot.MeshData.Texture != null)
                                        modelDoc.DataRoot.MeshData.Texture.SaveImages(System.IO.Path.GetDirectoryName(fileName), modelDoc.DataRoot.Name);
                                    modelDoc.DataRoot.VisitChildren<SprueKit.Data.ModelPiece>((SprueKit.Data.ModelPiece p) =>
                                    {

                                    });
                                    if (modelDoc.DataRoot.GenerateReportWithExport == true)
                                        new Data.Reports.ModelReport(modelDoc.DataRoot, System.IO.Path.ChangeExtension(fileName, ".html"));
                                });
                                Close();
                            }
                        }
                        break;
                    case 2: // OBJ
                        {
                            dlg.AddExtension = true;
                            dlg.Filter = "Wavefront OBJ (*.obj)|*.obj";
                            dlg.Title = "Save Model";
                            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            {
                                Parago.Windows.ProgressDialogResult result = Parago.Windows.ProgressDialog.Execute(null, "Exporting", "Exporting model...", (a, b) =>
                                {
                                    SprueBindings.ModelData.SaveOBJ(model, dlg.FileName, ErrorHandler.inst());
                                    if (modelDoc.DataRoot.MeshData != null && modelDoc.DataRoot.MeshData.Texture != null)
                                        modelDoc.DataRoot.MeshData.Texture.SaveImages(System.IO.Path.GetDirectoryName(dlg.FileName), modelDoc.DataRoot.Name);
                                });
                                Close();
                            }
                        }
                        break;
                    case 3: // Transforms list
                        {
                            dlg.AddExtension = true;
                            dlg.Filter = "Transforms list (*.csv)|*.csv";
                            dlg.Title = "Save Transforms";
                            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            {
                                Parago.Windows.ProgressDialogResult result = Parago.Windows.ProgressDialog.Execute(null, "Exporting", "Exporting model...", (a, b) =>
                                {
                                    StringBuilder sb = new StringBuilder();
                                    sb.AppendLine("Name, Position, Rotation, Scale, Capabilities, Flags");
                                    modelDoc.DataRoot.VisitAll((Data.SpruePiece piece) =>
                                    {
                                        sb.AppendLine(string.Format("\"{0}\", {1}, {2}, {3}, {4}, {5}",
                                            piece.Name, //0
                                            piece.Position.ToTightString(), //1
                                            piece.Rotation.ToEuler().ToTightString(), //2
                                            piece.Scale.ToTightString(), //3
                                            piece.Capabilities.ToString(), //4
                                            piece.Flags.ToString() //5
                                            ));
                                    });

                                    if (sb.Length > 0)
                                        System.IO.File.WriteAllText(dlg.FileName, sb.ToString());
                                    else
                                        ErrorHandler.inst().Error("No data to write to transforms list");
                                });
                                Close();
                            }
                        }
                        break;
                }
            }
        }
    }
}
