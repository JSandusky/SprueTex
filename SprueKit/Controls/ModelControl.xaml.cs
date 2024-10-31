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

namespace SprueKit.Controls
{
    /// <summary>
    /// Interaction logic for ModelControl.xaml
    /// </summary>
    public partial class ModelControl : UserControl
    {
        public ModelControl()
        {
            InitializeComponent();
        }

        private void btnLoadModel_Click(object sender, RoutedEventArgs e)
        {
            var mdl = (sender as Button).Tag as Data.ForeignModel;
            var dlg = new System.Windows.Forms.OpenFileDialog();
            if (mdl.ModelFile != null)
                dlg.InitialDirectory = System.IO.Path.GetDirectoryName(mdl.ModelFile.AbsolutePath);
            dlg.Filter = Data.FileData.ModelFileMask;
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                mdl.ModelFile = new Uri(dlg.FileName);
        }

        private void modelFileUri_Loaded(object sender, RoutedEventArgs e)
        {
            (sender as UriTextBox).RegexChecks = Data.FileData.ModelRegex;
        }
    }
}
