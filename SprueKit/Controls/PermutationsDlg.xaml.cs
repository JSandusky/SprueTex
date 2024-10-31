using FirstFloor.ModernUI.Windows.Controls;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SprueKit.Controls
{
    /// <summary>
    /// Interaction logic for PermutationsDlg.xaml
    /// </summary>
    public partial class PermutationsDlg : ModernDialog
    {
        string propertyKey_;
        object source_;
        PropertyInfo property_;

        public PermutationsDlg(object src, PropertyInfo property)
        {
            Owner = new IOCDependency<MainWindow>().Object;
            InitializeComponent();

            source_ = src;
            property_ = property;

            Title = string.Format("Permutations: {0}", property.Name.SplitCamelCase());
            propertyKey_ = property.Name;
            permsEditor.SetTarget(src, property);
            DataContextChanged += PermutationsDlg_DataContextChanged;

            Buttons = new Button[] {
                new Button {
                    Content = "Add Permutation",
                    ToolTip = "Add the current value as a new permutation",
                    Style = FindResource("StyledButton") as Style,
                },
                new Button
                {
                    Content = "Close",
                    IsCancel = true,
                    Style = FindResource("StyledButton") as Style,
                }
            };
            Buttons.First().Click += OnNewPermutation;
            Buttons.Last().Click += (o, e) => { Close(); };
        }

        private void OnNewPermutation(object sender, RoutedEventArgs e)
        {
            var piece = ((SprueKit.Data.IPermutable)DataContext);
            if (piece == null)
                return;

            Data.PermutationSet dataSet = null;
            if (piece.Permutations.TryGetValue(propertyKey_, out dataSet))
            {
                dataSet.AddNew();
                dataSet.Values.Last().Value = property_.GetValue(source_);
                piece.SignalPermutationChange();
            }
        }

        private void PermutationsDlg_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var piece = ((SprueKit.Data.IPermutable)DataContext);
            Data.PermutationSet dataSet = null;
            if (!piece.Permutations.TryGetValue(propertyKey_, out dataSet))
            {
                dataSet = new Data.PermutationSet(piece.GetType().GetProperty(propertyKey_).PropertyType, propertyKey_);
                piece.Permutations[propertyKey_] = dataSet;
            }

            Binding itemsBinding = new Binding() { Source = dataSet.Values };
            permsEditor.permutationsList.SetBinding(ItemsControl.ItemsSourceProperty, itemsBinding);
        }
    }
}
