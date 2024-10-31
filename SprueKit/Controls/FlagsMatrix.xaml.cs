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
    /// Interaction logic for FlagsMatrix.xaml
    /// </summary>
    public partial class FlagsMatrix : UserControl
    {
        public FlagsMatrix()
        {
            InitializeComponent();

            for (int i = 0; i < 8; ++i)
                matrixGrid.ColumnDefinitions.Add(new ColumnDefinition());

            matrixGrid.RowDefinitions.Add(new RowDefinition());
            matrixGrid.RowDefinitions.Add(new RowDefinition());
            matrixGrid.RowDefinitions.Add(new RowDefinition());
            matrixGrid.RowDefinitions.Add(new RowDefinition());

            for (int r = 0; r < 4; ++r)
            {
                for (int i = 0; i < 8; ++i)
                {
                    CheckBox cb = new CheckBox() { MinWidth = 16, MinHeight = 16, Margin = new Thickness(2) };
                    Grid.SetColumn(cb, i);
                    Grid.SetRow(cb, r);
                    matrixGrid.Children.Add(cb);
                }
            }
        }
    }
}
