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

using FirstFloor.ModernUI;
using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Navigation;

using System.Reflection;
using System.ComponentModel;

namespace SprueKit.Controls
{
    /// <summary>
    /// Interaction logic for ReflectivePage.xaml
    /// </summary>
    public partial class ReflectivePage : UserControl, IContent
    {
        public ReflectivePage()
        {
            InitializeComponent();
            DataContextChanged += ReflectivePage_DataContextChanged;
        }

        public static void FillLayout(StackPanel mainStack, object target)
        {
            var orderedProperties = PropertyHelpers.GetOrdered(target.GetType());

            int row = 0;
            foreach (var cachedProperty in orderedProperties)
            {
                var property = cachedProperty.Property;
                StackPanel pnl = new StackPanel();
                Label lbl = new Label();
                lbl.Content = property.Name.SplitCamelCase();
                lbl.Margin = new Thickness(5);

                Label tipLbl = new Label { HorizontalAlignment = HorizontalAlignment.Right };
                DockPanel dp = new DockPanel();
                DockPanel.SetDock(lbl, Dock.Left);
                DockPanel.SetDock(tipLbl, Dock.Right);
                dp.Children.Add(lbl);

                var attr = property.GetCustomAttribute<DescriptionAttribute>();
                if (attr != null)
                {
                    tipLbl.Content = attr.Description;
                    dp.Children.Add(tipLbl);
                }

                Grid.SetColumn(lbl, 0);
                Grid.SetRow(lbl, row);
                pnl.Children.Add(dp);

                if (property.PropertyType == typeof(string) || property.PropertyType == typeof(int) || property.PropertyType == typeof(float))
                {
                    TextBox box = new TextBox();
                    box.MinWidth = 160;

                    box.SetBinding(TextBox.TextProperty, new Binding(property.Name));
                    Grid.SetColumn(box, 1);
                    Grid.SetRow(box, row);
                    box.DataContext = target;
                    pnl.Children.Add(box);
                }
                else if (property.PropertyType == typeof(DateTime))
                {
                    DatePicker picker = new DatePicker();
                    picker.SetBinding(DatePicker.SelectedDateProperty, new Binding(property.Name));
                    picker.DataContext = target;
                    picker.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    picker.MaxWidth = 200;
                    Grid.SetColumn(picker, 1);
                    Grid.SetRow(picker, row);
                    pnl.Children.Add(picker);
                }
                else if (property.PropertyType == typeof(bool))
                {
                    CheckBox cb = new CheckBox();
                    cb.SetBinding(CheckBox.IsCheckedProperty, new Binding(property.Name));
                    cb.DataContext = target;
                    cb.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                    Grid.SetColumn(cb, 1);
                    Grid.SetRow(cb, row);
                    pnl.Children.Add(cb);
                }
                else if (property.PropertyType == typeof(Microsoft.Xna.Framework.Color))
                {

                }
                else if (property.PropertyType.IsEnum)
                {
                    ComboBox cb = new ComboBox();
                    if (property.PropertyType.GetCustomAttribute<EnumNamesAttribute>() != null)
                    {
                        string[] enumNames = property.PropertyType.GetCustomAttribute<EnumNamesAttribute>().EnumNames;
                        var enumValues = Enum.GetValues(property.PropertyType);
                        List<KeyValuePair<object,string> > items = new List<KeyValuePair<object, string>>();
                        for (int i = 0; i < enumNames.Length; ++i)
                            items.Add(new KeyValuePair<object, string>(enumValues.GetValue(i), enumNames[i]));
                        cb.ItemsSource = items;
                        cb.SelectedValuePath = "Key";
                        cb.DisplayMemberPath = "Value";
                        cb.SetBinding(ComboBox.SelectedValueProperty, new Binding(property.Name));
                        cb.DataContext = target;
                        pnl.Children.Add(cb);
                    }
                    else
                    {
                        cb.ItemsSource = Enum.GetValues(property.PropertyType);
                        cb.SetBinding(ComboBox.SelectedItemProperty, new Binding(property.Name));
                        cb.DataContext = target;
                        pnl.Children.Add(cb);
                    }
                }
                else if (property.PropertyType == typeof(Data.UriList))
                {
                    var isFileList = property.GetCustomAttribute<PropertyData.IsFileListAttribute>();
                    PathListControl ctrl = new PathListControl();
                    ctrl.IsFileMode = isFileList != null;
                    if (isFileList != null)
                    {
                        ctrl.FileFilter = isFileList.Filter;
                        ctrl.dirButton.Content = "Add File";
                    }
                    ctrl.DataContext = target;
                    string propertyPath = string.Format("{0}.Paths", property.Name);
                    ctrl.pathGrid.SetBinding(ItemsControl.DataContextProperty, new Binding(property.Name));
                    ctrl.pathGrid.SetBinding(ItemsControl.ItemsSourceProperty, new Binding(propertyPath) { Source = target });
                    pnl.Children.Add(ctrl);
                }
                mainStack.Children.Add(pnl);
            }
        }

        private void ReflectivePage_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            mainStack.Children.Clear();
            if (DataContext != null)
                FillLayout(mainStack, DataContext);
        }

        public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e)
        {
            DataContext = null;
            if (e.Fragment.Equals("meshing"))
            {
                DataContext = UserData.inst().MeshingSettings;
                header.Content = "Meshing Settings";
            }
            else if (e.Fragment.Equals("general"))
            {
                DataContext = UserData.inst().GeneralSettings;
                header.Content = "General Settings";
            }
            else if (e.Fragment.Equals("uvgeneration"))
            {
                DataContext = UserData.inst().UVGenerationSettings;
                header.Content = "UV Generation Settings";
            }
            else if (e.Fragment.Equals("viewport"))
            {
                DataContext = UserData.inst().ViewportSettings;
                header.Content = "Viewport Settings";
            }
            else if (e.Fragment.Equals("texturegraphsettings"))
            {
                DataContext = UserData.inst().TextureGraphSettings;
                header.Content = "Texture Graph Settings";
            }
        }

        public void OnNavigatedFrom(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e)
        {
        }

        public void OnNavigatedTo(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e)
        {
        }

        public void OnNavigatingFrom(FirstFloor.ModernUI.Windows.Navigation.NavigatingCancelEventArgs e)
        {

        }

    }
}
