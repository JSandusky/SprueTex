using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for MaterialList.xaml
    /// </summary>
    public partial class MaterialList : UserControl
    {
        public MaterialList()
        {
            InitializeComponent();

            Loaded += MaterialList_Loaded;
        }

        private void MaterialList_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void btnDeleteTexture_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null)
            {
                var textureMapList = DataContext as ObservableCollection<Data.TextureMap>;
                textureMapList.Remove(btn.DataContext as Data.TextureMap);
            }
        }

        private void btnAddTexture_Click(object sender, RoutedEventArgs e)
        {
            var textureMapList = DataContext as ObservableCollection<Data.TextureMap>;
            textureMapList.Add(new Data.TextureMap());
        }

        private void thumbImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Image img = sender as Image;
            if (img != null)
            {
                var pop = PopupHelper.Create();
                Image popupImage = new Image() { MaxWidth = 512, MaxHeight = 512, MinWidth = 0, MinHeight = 0, Stretch = Stretch.Uniform };
                popupImage.SetBinding(Image.SourceProperty, new Binding("Thumbnail") { Source = img.DataContext });
                popupImage.SetBinding(Image.WidthProperty, new Binding("Thumbnail.Width") { Source = img.DataContext });
                popupImage.SetBinding(Image.HeightProperty, new Binding("Thumbnail.Height") { Source = img.DataContext });
                pop.Grid.Children.Add(popupImage);
                pop.ShowAtMouse();
            }
        }

        private void browseFileButton_Click(object sender, RoutedEventArgs e)
        {
            var texMap = (sender as Button).Tag as Data.TextureMap;
            var dlg = new System.Windows.Forms.OpenFileDialog();
            if (texMap.Image != null)
                dlg.InitialDirectory = System.IO.Path.GetDirectoryName(texMap.Image.AbsolutePath);
            dlg.Filter = Data.FileData.ImageFileMask;
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                texMap.Image = new Uri(dlg.FileName);

        }

        private void uriTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            (sender as UriTextBox).RegexChecks = Data.FileData.ImageRegex;
        }
    }
}
