using FirstFloor.ModernUI.Windows.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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

namespace SprueKit.Dlg
{
    /// <summary>
    /// Interaction logic for TexVariationDlg.xaml
    /// </summary>
    public partial class TexVariationDlg : ModernDialog
    {
        Data.Graph.Graph doc_;
        Data.TexGen.TexGenNode node_;

        public static bool CanShowVariations(object node)
        {
            var nodeType = node.GetType();
            var properties = PropertyHelpers.GetOrdered(nodeType).Where(p => Util.DocHelpBuilder.CanEmbedGIF(nodeType, p.Property));
            return properties.Count() > 0;
        }

        public TexVariationDlg(Data.Graph.Graph graph, Data.TexGen.TexGenNode node)
        {
            doc_ = graph;
            node_ = node;
            InitializeComponent();

            FillList(node.GetType());

            var nodeType = node.GetType();
            
            var exportBtn = new StyledButton { Content = "View", IsDefault = true };
            exportBtn.Click += ExportBtn_Click;
            Buttons = new Button[] {
                exportBtn,
                CancelButton
            };
        }

        void FillList(Type type)
        {
            var properties = PropertyHelpers.GetOrdered(type).Where(p => Util.DocHelpBuilder.CanEmbedGIF(type, p.Property));
            foreach (var prop in properties)
            {
                itemsList.Items.Add(new ListBoxItem
                {
                    Content = prop.DisplayName,
                    Tag = prop
                });
            }
        }

        private void ExportBtn_Click(object sender, RoutedEventArgs e)
        {
            if (itemsList.SelectedItem != null)
            {
                var selPropInfo = ((ListBoxItem)itemsList.SelectedItem).Tag as CachedPropertyInfo;
                var cloneDoc = doc_.Clone();
                if (cloneDoc == null)
                    return;
                var cloneNode = cloneDoc.Nodes.FirstOrDefault(n => n.NodeID == node_.NodeID) as Data.TexGen.TexGenNode;
                if (cloneNode == null)
                    return;
                Parago.Windows.ProgressDialog.Execute(null, "Generating variation preview", "Processing", new Action(() =>
                {
                    var frames = Util.HelpBuilder.GenerateInstanceFrames(cloneDoc, cloneNode, cloneNode.GetType(), selPropInfo.Property, 256);
                    var outBmp = frames.BuildStrip(256);
                    outBmp.Save("~vars.png");
                    System.Diagnostics.Process.Start("~vars.png");
                }));
                Close();
            }
        }

        private void itemsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ExportBtn_Click(null, null);
        }
    }
}
