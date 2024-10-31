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
    /// Interaction logic for GenericTreeControl.xaml
    /// </summary>
    public partial class GenericTreeControl : UserControl
    {
        SelectionContext selectionContext;
        bool selectionSignalBlocked = false;

        public GenericTreeControl(Document document)
        {
            InitializeComponent();

            DataContextChanged += GenericTreeControl_DataContextChanged;
            selectionContext = document.Selection;
            selectionContext.Selected.CollectionChanged += (o, e) =>
            {
                selectionSignalBlocked = true;
                //selectionBehave.SelectedItems = selectionContext.Selected;
                treeControl.MatchSelection(selectionContext);
                selectionSignalBlocked = false;
            };

            treeControl.SelectedItemChanged += TreeControl_SelectedItemChanged;
        }

        private void TreeControl_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (selectionSignalBlocked == false)
            {
                selectionContext.SetSelected(treeControl.SelectedItem);
            }
        }

        private void GenericTreeControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            treeControl.DataContext = DataContext;
        }

        private void tree_Expanded(object sender, RoutedEventArgs e)
        {
            //treeControl.ZebraStripe();
        }
    }
}
