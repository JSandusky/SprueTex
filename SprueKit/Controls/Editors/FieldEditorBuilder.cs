using Microsoft.Xna.Framework;
using SprueKit.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace SprueKit.Controls.Editors
{
    public class FieldEditorBuilder
    {
        static readonly System.Windows.Media.Color[] VectorColor = {
            Colors.Red,
            Colors.LimeGreen,
            Colors.Cyan,
            Colors.HotPink
        };

        public static UIElement CreateControl(string labelText, Binding binding, Type fieldType, PropertyInfo targetProp /*may be null*/, out bool desireLabel)
        {
            desireLabel = true;
            if (fieldType == typeof(bool))
            {
                CheckBox cb = new CheckBox();
                cb.Content = labelText;
                cb.SetBinding(CheckBox.IsCheckedProperty, binding);
                desireLabel = false;
                return cb;
            }
            else if (fieldType == typeof(int))
            {
                NumericTextBox tb = new NumericTextBox();
                tb.IsInteger = true;
                tb.SetBinding(TextBox.TextProperty, binding);
                return tb;
            }
            else if (fieldType == typeof(uint))
            {
                NumericTextBox tb = new NumericTextBox();
                tb.IsInteger = true;
                tb.SetBinding(TextBox.TextProperty, binding);
                return tb;
            }
            else if (fieldType == typeof(float))
            {
                NumericTextBox tb = new NumericTextBox();
                tb.SetBinding(TextBox.TextProperty, binding);
                return tb;
            }
            else if (fieldType == typeof(string))
            {
                TextBox tb = new TextBox();
                tb.SetBinding(TextBox.TextProperty, binding);
                return tb;
            }
            else if (fieldType == typeof(Vector2))
            {
                TextBox x = new NumericTextBox() { Name = "xEdit" };
                TextBox y = new NumericTextBox() { Name = "yEdit" };

                x.MinWidth = y.MinWidth = 60;
                x.HorizontalAlignment = HorizontalAlignment.Stretch;
                y.HorizontalAlignment = HorizontalAlignment.Stretch;

                var xBinding = new Binding(binding.Path.Path) { Source = binding.Source, Converter = new Vec2ElemConverter(0, binding.Source, targetProp) };
                var yBinding = new Binding(binding.Path.Path) { Source = binding.Source, Converter = new Vec2ElemConverter(1, binding.Source, targetProp) };
                xBinding.ValidationRules.Add(new FloatValidationRule());
                yBinding.ValidationRules.Add(new FloatValidationRule());
                x.SetBinding(TextBox.TextProperty, xBinding);
                y.SetBinding(TextBox.TextProperty, yBinding);

                Grid vecGrid = new Grid();
                vecGrid.ColumnDefinitions.Add(new ColumnDefinition());
                vecGrid.ColumnDefinitions.Add(new ColumnDefinition());

                Border xBorder = new Border();
                xBorder.BorderThickness = new Thickness(0, 0, 0, 2);
                xBorder.BorderBrush = new SolidColorBrush(Colors.Red);
                xBorder.Child = x;
                xBorder.SetValue(Grid.ColumnProperty, 0);

                Border yBorder = new Border();
                yBorder.BorderThickness = new Thickness(0, 0, 0, 2);
                yBorder.BorderBrush = new SolidColorBrush(Colors.Chartreuse);
                yBorder.Child = y;
                yBorder.SetValue(Grid.ColumnProperty, 1);

                xBorder.Margin = yBorder.Margin = new Thickness(2);

                vecGrid.Children.Add(xBorder);
                vecGrid.Children.Add(yBorder);

                return vecGrid;
            }
            else if (fieldType == typeof(Vector3))
            {
                TextBox x = new NumericTextBox() { Name = "xEdit" },
                                y = new NumericTextBox() { Name = "yEdit" },
                                z = new NumericTextBox() { Name = "zEdit" };

                x.MinWidth = y.MinWidth = z.MinWidth = 60;
                x.HorizontalAlignment = HorizontalAlignment.Stretch;
                y.HorizontalAlignment = HorizontalAlignment.Stretch;
                z.HorizontalAlignment = HorizontalAlignment.Stretch;

                var xBinding = new Binding(binding.Path.Path) { Source = binding.Source, Converter = new Vec3ElemConverter(0, binding.Source, targetProp), ValidatesOnExceptions = true, ValidatesOnDataErrors = true, UpdateSourceTrigger = UpdateSourceTrigger.LostFocus };
                var yBinding = new Binding(binding.Path.Path) { Source = binding.Source, Converter = new Vec3ElemConverter(1, binding.Source, targetProp), ValidatesOnExceptions = true, ValidatesOnDataErrors = true, UpdateSourceTrigger = UpdateSourceTrigger.LostFocus };
                var zBinding = new Binding(binding.Path.Path) { Source = binding.Source, Converter = new Vec3ElemConverter(2, binding.Source, targetProp), ValidatesOnExceptions = true, ValidatesOnDataErrors = true, UpdateSourceTrigger = UpdateSourceTrigger.LostFocus };
                xBinding.ValidationRules.Add(new FloatValidationRule());
                yBinding.ValidationRules.Add(new FloatValidationRule());
                zBinding.ValidationRules.Add(new FloatValidationRule());
                x.SetBinding(TextBox.TextProperty, xBinding);
                y.SetBinding(TextBox.TextProperty, yBinding);
                z.SetBinding(TextBox.TextProperty, zBinding);

                Grid stack = new Grid();
                stack.ColumnDefinitions.Add(new ColumnDefinition());
                stack.ColumnDefinitions.Add(new ColumnDefinition());
                stack.ColumnDefinitions.Add(new ColumnDefinition());

                Border xBorder = new Border();
                xBorder.BorderThickness = new Thickness(0, 0, 0, 2);
                xBorder.BorderBrush = new SolidColorBrush(Colors.Red);
                xBorder.Child = x;
                xBorder.SetValue(Grid.ColumnProperty, 0);

                Border yBorder = new Border();
                yBorder.BorderThickness = new Thickness(0, 0, 0, 2);
                yBorder.BorderBrush = new SolidColorBrush(Colors.Chartreuse);
                yBorder.Child = y;
                yBorder.SetValue(Grid.ColumnProperty, 1);

                Border zBorder = new Border();
                zBorder.BorderThickness = new Thickness(0, 0, 0, 2);
                zBorder.BorderBrush = new SolidColorBrush(Colors.Aqua);
                zBorder.Child = z;
                zBorder.SetValue(Grid.ColumnProperty, 2);

                xBorder.Margin = yBorder.Margin = zBorder.Margin = new Thickness(2);

                stack.Children.Add(xBorder);
                stack.Children.Add(yBorder);
                stack.Children.Add(zBorder);

                return stack;
            }
            else if (fieldType == typeof(Microsoft.Xna.Framework.Color))
            {
                var bindings = new Binding[]
                {
                    new Binding(binding.Path.Path) { Converter = new ColorElemConverter(0, binding.Source, targetProp), ValidatesOnExceptions = true, ValidatesOnDataErrors = true, UpdateSourceTrigger = UpdateSourceTrigger.LostFocus },
                    new Binding(binding.Path.Path) { Converter = new ColorElemConverter(1, binding.Source, targetProp), ValidatesOnExceptions = true, ValidatesOnDataErrors = true, UpdateSourceTrigger = UpdateSourceTrigger.LostFocus },
                    new Binding(binding.Path.Path) { Converter = new ColorElemConverter(2, binding.Source, targetProp), ValidatesOnExceptions = true, ValidatesOnDataErrors = true, UpdateSourceTrigger = UpdateSourceTrigger.LostFocus },
                    new Binding(binding.Path.Path) { Converter = new ColorElemConverter(3, binding.Source, targetProp), ValidatesOnExceptions = true, ValidatesOnDataErrors = true, UpdateSourceTrigger = UpdateSourceTrigger.LostFocus },
                };
                var textBoxes = new TextBox[]
                {
                    new NumericTextBox() { ToolTip = "Red"  , IsInteger = true },
                    new NumericTextBox() { ToolTip = "Green", IsInteger = true },
                    new NumericTextBox() { ToolTip = "Blue" , IsInteger = true },
                    new NumericTextBox() { ToolTip = "Alpha", IsInteger = true },
                };

                Grid grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                for (int i = 0; i < 4; ++i)
                {
                    textBoxes[i].SetBinding(TextBox.TextProperty, bindings[i]);
                    Border xBorder = new Border();
                    xBorder.HorizontalAlignment = HorizontalAlignment.Stretch;
                    xBorder.BorderThickness = new Thickness(0, 0, 0, 2);
                    xBorder.BorderBrush = new SolidColorBrush(VectorColor[i]);
                    xBorder.Child = textBoxes[i];
                    Grid.SetColumn(xBorder, i);
                    xBorder.Margin = new Thickness(2);
                    grid.Children.Add(xBorder);
                }

                StackPanel sp = new StackPanel { Orientation = Orientation.Vertical };
                sp.Children.Add(grid);

                Button btn = new Button { MinWidth = 30 };
                sp.Children.Add(btn);
                btn.SetBinding(Button.BackgroundProperty, new Binding(binding.Path.Path) { Converter = new Data.ColorToBrushConverter() });
                btn.Click += (o, args) =>
                {
                    PopupHelper popup = PopupHelper.Create();
                    var canvas = new ColorPickerLib.Controls.ColorCanvas();
                    canvas.Background = new SolidColorBrush(Colors.Transparent);
                    canvas.MinHeight = 320;
                    canvas.MinWidth = 320;
                    canvas.Width = 320;
                    canvas.SelectedColor = Colors.Red;
                    canvas.SetBinding(ColorPickerLib.Controls.ColorCanvas.SelectedColorProperty, new Binding(binding.Path.Path) { Converter = new Data.XNAColorConverter() });
                    canvas.DataContext = binding.Source;
                    popup.Grid.Children.Add(canvas);
                    popup.ShowAtMouse();
                };

                return sp;
            }
            else if (fieldType == typeof(Vector4))
            {
                TextBox x = new NumericTextBox() { Name = "xEdit" },
                                y = new NumericTextBox() { Name = "yEdit" },
                                z = new NumericTextBox() { Name = "zEdit" },
                                w = new NumericTextBox() { Name = "wEdit" };

                x.MinWidth = y.MinWidth = z.MinWidth = w.MinWidth = 60;
                x.HorizontalAlignment = HorizontalAlignment.Stretch;
                y.HorizontalAlignment = HorizontalAlignment.Stretch;
                z.HorizontalAlignment = HorizontalAlignment.Stretch;
                w.HorizontalAlignment = HorizontalAlignment.Stretch;

                var xBinding = new Binding(binding.Path.Path) { Source = binding.Source, Converter = new Vec4ElemConverter(0, binding.Source, targetProp), ValidatesOnExceptions = true, ValidatesOnDataErrors = true, UpdateSourceTrigger = UpdateSourceTrigger.LostFocus };
                var yBinding = new Binding(binding.Path.Path) { Source = binding.Source, Converter = new Vec4ElemConverter(1, binding.Source, targetProp), ValidatesOnExceptions = true, ValidatesOnDataErrors = true, UpdateSourceTrigger = UpdateSourceTrigger.LostFocus };
                var zBinding = new Binding(binding.Path.Path) { Source = binding.Source, Converter = new Vec4ElemConverter(2, binding.Source, targetProp), ValidatesOnExceptions = true, ValidatesOnDataErrors = true, UpdateSourceTrigger = UpdateSourceTrigger.LostFocus };
                var wBinding = new Binding(binding.Path.Path) { Source = binding.Source, Converter = new Vec4ElemConverter(3, binding.Source, targetProp), ValidatesOnExceptions = true, ValidatesOnDataErrors = true, UpdateSourceTrigger = UpdateSourceTrigger.LostFocus };
                xBinding.ValidationRules.Add(new FloatValidationRule());
                yBinding.ValidationRules.Add(new FloatValidationRule());
                zBinding.ValidationRules.Add(new FloatValidationRule());
                wBinding.ValidationRules.Add(new FloatValidationRule());
                x.SetBinding(TextBox.TextProperty, xBinding);
                y.SetBinding(TextBox.TextProperty, yBinding);
                z.SetBinding(TextBox.TextProperty, zBinding);
                w.SetBinding(TextBox.TextProperty, wBinding);

                Grid stack = new Grid();
                stack.ColumnDefinitions.Add(new ColumnDefinition());
                stack.ColumnDefinitions.Add(new ColumnDefinition());
                stack.ColumnDefinitions.Add(new ColumnDefinition());
                stack.ColumnDefinitions.Add(new ColumnDefinition());

                Border xBorder = new Border();
                xBorder.BorderThickness = new Thickness(0, 0, 0, 2);
                xBorder.BorderBrush = new SolidColorBrush(Colors.Red);
                xBorder.Child = x;
                xBorder.SetValue(Grid.ColumnProperty, 0);

                Border yBorder = new Border();
                yBorder.BorderThickness = new Thickness(0, 0, 0, 2);
                yBorder.BorderBrush = new SolidColorBrush(Colors.Chartreuse);
                yBorder.Child = y;
                yBorder.SetValue(Grid.ColumnProperty, 1);

                Border zBorder = new Border();
                zBorder.BorderThickness = new Thickness(0, 0, 0, 2);
                zBorder.BorderBrush = new SolidColorBrush(Colors.Aqua);
                zBorder.Child = z;
                zBorder.SetValue(Grid.ColumnProperty, 2);

                Border wBorder = new Border();
                wBorder.BorderThickness = new Thickness(0, 0, 0, 2);
                wBorder.BorderBrush = new SolidColorBrush(Colors.Aqua);
                wBorder.Child = w;
                wBorder.SetValue(Grid.ColumnProperty, 3);

                xBorder.Margin = yBorder.Margin = zBorder.Margin = wBorder.Margin = new Thickness(2);

                stack.Children.Add(xBorder);
                stack.Children.Add(yBorder);
                stack.Children.Add(zBorder);
                stack.Children.Add(wBorder);

                return stack;
            }
            else if (fieldType == typeof(PluginLib.IntVector2))
            {
                TextBox x = new NumericTextBox() { Name = "xEdit", IsInteger = true };
                TextBox y = new NumericTextBox() { Name = "yEdit", IsInteger = true };

                x.MinWidth = y.MinWidth = 60;
                x.HorizontalAlignment = HorizontalAlignment.Stretch;
                y.HorizontalAlignment = HorizontalAlignment.Stretch;

                var xBinding = new Binding(binding.Path.Path) { Source = binding.Source, Converter = new Vec2ElemConverter(0, binding.Source, targetProp) };
                var yBinding = new Binding(binding.Path.Path) { Source = binding.Source, Converter = new Vec2ElemConverter(1, binding.Source, targetProp) };
                xBinding.ValidationRules.Add(new FloatValidationRule());
                yBinding.ValidationRules.Add(new FloatValidationRule());
                x.SetBinding(TextBox.TextProperty, xBinding);
                y.SetBinding(TextBox.TextProperty, yBinding);

                Grid vecGrid = new Grid();
                vecGrid.ColumnDefinitions.Add(new ColumnDefinition());
                vecGrid.ColumnDefinitions.Add(new ColumnDefinition());

                Border xBorder = new Border();
                xBorder.BorderThickness = new Thickness(0, 0, 0, 2);
                xBorder.BorderBrush = new SolidColorBrush(Colors.Red);
                xBorder.Child = x;
                xBorder.SetValue(Grid.ColumnProperty, 0);

                Border yBorder = new Border();
                yBorder.BorderThickness = new Thickness(0, 0, 0, 2);
                yBorder.BorderBrush = new SolidColorBrush(Colors.Chartreuse);
                yBorder.Child = y;
                yBorder.SetValue(Grid.ColumnProperty, 1);

                xBorder.Margin = yBorder.Margin = new Thickness(2);

                vecGrid.Children.Add(xBorder);
                vecGrid.Children.Add(yBorder);

                return vecGrid;
            }
            else if (fieldType == typeof(PluginLib.IntVector4))
            {

            }
            else if (fieldType == typeof(Quaternion))
            {

            }
            else if (fieldType == typeof(Microsoft.Xna.Framework.Matrix))
            {

            }
            else if (fieldType.IsEnum)
            {
                ComboBox cb = new ComboBox();
                cb.ItemsSource = Enum.GetValues(fieldType);
                cb.SetBinding(ComboBox.SelectedItemProperty, binding);
                return cb;
            }
            return null;
        }

    }
}
