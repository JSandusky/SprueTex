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
using System.Reflection;

using SprueKit.Data;
using SprueKit.Controls;

using Microsoft.Xna.Framework;
using System.Windows.Threading;
using System.Timers;
using System.Collections;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace SprueKit.Controls
{
    /// <summary>
    /// Interaction logic for Properties.xaml
    /// </summary>
    public partial class Properties : UserControl
    {
        static readonly System.Windows.Media.Color[] VectorColor = {
            Colors.Red,
            Colors.LimeGreen,
            Colors.Cyan,
            Colors.HotPink
        };

        public static readonly DependencyProperty PendingContextProperty =
            DependencyProperty.Register(
                "PendingContext",
                typeof(object),
                typeof(Properties),
                new PropertyMetadata(null, OnPendingContextChanged));

        public object PendingContext
        {
            get { return (object)GetValue(PendingContextProperty); }
            set { SetValue(PendingContextProperty, value); }
        }

        Timer timer_;
        bool needsCheck_ = false;
        private static void OnPendingContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Properties self = d as Properties;
            self.needsCheck_ = true;
            //object target = self.PendingContext;
            //self.DataContext = self.PendingContext;

            //Timer timer_ = new Timer();
            //if (self.timer_ == null)
            //{
            //    self.timer_ = new Timer();
            //    self.timer_.Interval = 20;
            //    self.timer_.Elapsed += (o, evt) =>
            //    {
                    self.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (self.DataContext != self.PendingContext)
                            self.DataContext = self.PendingContext;
                        self.needsCheck_ = false;
                    }));
                    //timer_.Stop();
                //};
                //self.timer_.Start();
            //}
            //Properties self = d as Properties;
            //object target = self.PendingContext;
            //System.Threading.Thread.Sleep(2);
            //await Task.Delay(10);
            //object target = self.PendingContext;
            //self.DataContext = self.PendingContext;
        }

        public Properties()
        {
            InitializeComponent();

            DataContextChanged += Properties_DataContextChanged;
            var documentManager = new IOCDependency<DocumentManager>();
            documentManager.Object.OnActiveDocumentChanged += Object_OnActiveDocumentChanged;
        }

        public void Object_OnActiveDocumentChanged(Document newDoc, Document oldDoc)
        {
            if (newDoc != null)
                SetBinding(PendingContextProperty, new Binding("Selection.MostRecentlySelected") { Source = newDoc, Delay=50 });
            else
                SetBinding(PendingContextProperty, new Binding("Selection.MostRecentlySelected") { Source = null });
        }

        private void Properties_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //using (var delay = Dispatcher.DisableProcessing())
            {
                formGrid.Children.Clear();
                if (DataContext == null)
                    header.Content = null;

                if (DataContext != null)
                {
                    bool forceReadOnly = false;

                    //HACK
                    if (DataContext is SpruePiece)
                        forceReadOnly = ((SpruePiece)DataContext).IsLocked;

                    List<FrameworkElement> controls = new List<FrameworkElement>();

                    header.Content = DataContext.GetType().Name.SplitCamelCase();

                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(string.Format("pack://application:,,,/{0};component/Images/ticket.png", App.AppName), UriKind.RelativeOrAbsolute);
                    bmp.EndInit();

                    var propertyGroups = SprueKit.PropertyHelpers.GetGrouped(DataContext.GetType());
                    var propertyCmds = SprueKit.PropertyHelpers.GetCommands(DataContext.GetType());
                    foreach (var grp in propertyGroups)
                    {
                        Separator sep = new Separator();
                        formGrid.Children.Add(sep);
                        Expander expander = new Expander();
                        expander.Header = new Label { Content = grp.GroupName, FontWeight = FontWeights.Bold };

                        StackPanel targetPanel = new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness(2, 0, 0, 0) };

                        if (propertyGroups.Count > 1)
                        {
                            expander.Content = targetPanel;
                            expander.IsExpanded = Data.Converters.ExpanderConverter.IsExpanded(grp.GroupName);
                            expander.Expanded += (o, exp) => { Data.Converters.ExpanderConverter.SetExpanded(grp.GroupName, true); };
                            expander.Collapsed += (o, col) => { Data.Converters.ExpanderConverter.SetExpanded(grp.GroupName, false); };
                            //expander.SetBinding(Expander.IsExpandedProperty, new Binding(".") { Converter = new Data.Converters.ExpanderConverter(), ConverterParameter = grp.GroupName });
                            targetPanel.Margin = new Thickness(10, 0, 0, 0);
                            formGrid.Children.Add(expander);
                        }
                        else
                        {
                            formGrid.Children.Add(targetPanel);
                        }

                        //var properties = SprueKit.PropertyHelpers.Sort(DataContext.GetType(), DataContext.GetType().GetProperties().ToList());

                        int row = 0;
                        foreach (CachedPropertyInfo propInfo in grp.Properties)
                        {
                            PropertyInfo pi = propInfo.Property;
                            BindingMode bindMode = BindingMode.TwoWay;

                            var setter = pi.GetSetMethod(true);
                            bool writable = (pi.CanWrite && setter != null && setter.IsPublic) && !forceReadOnly;
                            if (!writable)
                                bindMode = BindingMode.OneWay;

                            Label lbl = new Label { Content = propInfo.DisplayName };
                            lbl.ToolTip = propInfo.Tip;
                            lbl.FontWeight = FontWeights.DemiBold;
                            if (writable)
                                lbl.SetBinding(Label.ForegroundProperty, new Binding(pi.Name) { Converter = new Controls.Converters.DefaultPropertyColorConverter(pi), Source = DataContext, ConverterParameter = DataContext });

                            Button permsButton = null;

                            DockPanel lblGrid = new DockPanel();
                            lblGrid.Margin = new Thickness(3);
                            lbl.SetValue(DockPanel.DockProperty, Dock.Left);
                            lblGrid.Children.Add(lbl);

                            if (writable && TypeAllowsPermutations(pi.PropertyType) && FieldAllowsPermutations(pi))
                            {
                                permsButton = new StyledButton();
                                permsButton.Content = new Image() { Source = bmp };
                                permsButton.ToolTip = "Permutations";
                                permsButton.HorizontalAlignment = HorizontalAlignment.Right;
                                permsButton.Padding = new Thickness(1);
                                permsButton.Width = 18;
                                permsButton.Height = 18;
                                permsButton.SetValue(DockPanel.DockProperty, Dock.Right);

                                permsButton.Click += (o, evt) =>
                                {
                                    PermutationsDlg dlg = new PermutationsDlg(DataContext, pi);
                                    // set the datacontext
                                    dlg.DataContext = DataContext;
                                    dlg.Show();
                                };
                                lblGrid.Children.Add(permsButton);
                            }

                            if (!writable)
                            {
                                TextBlock propLabel = new TextBlock() { Margin = new Thickness(5, 0, 5, 0), Foreground = Brushes.Gray };
                                propLabel.TextWrapping = TextWrapping.Wrap;
                                propLabel.SetBinding(TextBlock.TextProperty, new Binding(pi.Name));

                                targetPanel.Children.Add(lblGrid);
                                targetPanel.Children.Add(propLabel);
                                controls.Add(propLabel);
                            }
                            else if (pi.PropertyType == typeof(int) || pi.PropertyType == typeof(float))
                            {
                                NumericTextBox tb = new NumericTextBox();
                                if (pi.PropertyType == typeof(int))
                                    tb.IsInteger = true;
                                tb.SetBinding(TextBox.TextProperty, new Binding(pi.Name) { Mode = bindMode, UpdateSourceTrigger = UpdateSourceTrigger.LostFocus });
                                tb.Tag = pi;

                                targetPanel.Children.Add(lblGrid);
                                targetPanel.Children.Add(tb);
                                controls.Add(tb);
                            }
                            else if (pi.PropertyType == typeof(uint))
                            {
                                var flagsAttr = pi.GetCustomAttribute<PropertyData.PropertyFlagsAttribute>();
                                if (flagsAttr != null)
                                {
                                    CheckboxMatrix matrix = new CheckboxMatrix();
                                    matrix.SetBinding(CheckboxMatrix.ValueProperty, new Binding(pi.Name) { Mode = bindMode, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
                                    matrix.ToolMethod = (int idx) => { return UserData.inst().BitNames.GetFieldName(flagsAttr.BitNames, idx); };
                                    //FlagsMatrix matrix = new FlagsMatrix();
                                    targetPanel.Children.Add(lblGrid);
                                    targetPanel.Children.Add(matrix);
                                    controls.Add(matrix);
                                }
                                else
                                {
                                    // Regular UInt
                                    NumericTextBox tb = new NumericTextBox();
                                    tb.IsInteger = true;
                                    tb.SetBinding(TextBox.TextProperty, new Binding(pi.Name) { Mode = bindMode, UpdateSourceTrigger = UpdateSourceTrigger.LostFocus });
                                    tb.Tag = pi;

                                    targetPanel.Children.Add(lblGrid);
                                    targetPanel.Children.Add(tb);
                                    controls.Add(tb);
                                }
                            }
                            else if (pi.PropertyType == typeof(bool))
                            {
                                CheckBox cb = new CheckBox();
                                cb.ToolTip = propInfo.Tip;
                                cb.Margin = new Thickness(2, 2, 2, 2);
                                cb.Content = pi.Name.SplitCamelCase();
                                Binding binding = new Binding(pi.Name) { Mode = bindMode };
                                cb.SetBinding(CheckBox.IsCheckedProperty, binding);
                                cb.Tag = pi;

                                targetPanel.Children.Add(cb);
                                controls.Add(cb);
                            }
                            else if (pi.PropertyType == typeof(string))
                            {
                                TextBox tb = new TextBox();
                                tb.SetBinding(TextBox.TextProperty, new Binding(pi.Name) { Mode = bindMode, UpdateSourceTrigger = UpdateSourceTrigger.LostFocus });
                                tb.Tag = pi;

                                targetPanel.Children.Add(lblGrid);
                                targetPanel.Children.Add(tb);
                                controls.Add(tb);
                            }
                            else if (pi.PropertyType == typeof(Microsoft.Xna.Framework.Color))
                            {
                                var bindings = new Binding[]
                                {
                                new Binding(pi.Name) { Converter = new ColorElemConverter(0, DataContext, pi), ValidatesOnExceptions = true, ValidatesOnDataErrors = true, UpdateSourceTrigger = UpdateSourceTrigger.LostFocus },
                                new Binding(pi.Name) { Converter = new ColorElemConverter(1, DataContext, pi), ValidatesOnExceptions = true, ValidatesOnDataErrors = true, UpdateSourceTrigger = UpdateSourceTrigger.LostFocus },
                                new Binding(pi.Name) { Converter = new ColorElemConverter(2, DataContext, pi), ValidatesOnExceptions = true, ValidatesOnDataErrors = true, UpdateSourceTrigger = UpdateSourceTrigger.LostFocus },
                                new Binding(pi.Name) { Converter = new ColorElemConverter(3, DataContext, pi), ValidatesOnExceptions = true, ValidatesOnDataErrors = true, UpdateSourceTrigger = UpdateSourceTrigger.LostFocus },
                                };

                                var textBoxes = new TextBox[]
                                {
                                new NumericTextBox() { ToolTip = "Red", IsInteger=true },
                                new NumericTextBox() { ToolTip = "Green", IsInteger=true },
                                new NumericTextBox() { ToolTip = "Blue", IsInteger=true },
                                new NumericTextBox() { ToolTip = "Alpha", IsInteger=true },
                                };

                                StackPanel sp = new StackPanel { Orientation = Orientation.Vertical };
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
                                sp.Children.Add(grid);

                                Button btn = new Button { MinWidth = 30 };
                                sp.Children.Add(btn);
                                btn.SetBinding(Button.BackgroundProperty, new Binding(pi.Name) { Converter = new Data.ColorToBrushConverter() });
                                btn.Click += (o, args) =>
                                {
                                    PopupHelper popup = PopupHelper.Create();
                                    var canvas = new ColorPickerLib.Controls.ColorCanvas();
                                    canvas.Background = new SolidColorBrush(Colors.Transparent);
                                    canvas.MinHeight = 320;
                                    canvas.MinWidth = 320;
                                    canvas.Width = 320;
                                    canvas.SelectedColor = Colors.Red;
                                    canvas.SetBinding(ColorPickerLib.Controls.ColorCanvas.SelectedColorProperty, new Binding(pi.Name) { Converter = new Data.XNAColorConverter() });
                                    canvas.DataContext = DataContext;
                                    popup.Grid.Children.Add(canvas);
                                    popup.ShowAtMouse();
                                };

                                for (int i = 0; i < 4; ++i)
                                    controls.Add(textBoxes[i]);
                                targetPanel.Children.Add(lblGrid);
                                targetPanel.Children.Add(sp);
                            }
                            else if (pi.PropertyType == typeof(PluginLib.IntVector2))
                            {
                                TextBox x = new NumericTextBox() { Name = "xEdit" },
                                    y = new NumericTextBox() { Name = "yEdit" };

                                x.MinWidth = y.MinWidth = 60;
                                x.HorizontalAlignment = HorizontalAlignment.Stretch;
                                y.HorizontalAlignment = HorizontalAlignment.Stretch;

                                var xBinding = new Binding(pi.Name) { Converter = new Vec2ElemConverter(0, DataContext, pi), ValidatesOnExceptions = true, ValidatesOnDataErrors = true, UpdateSourceTrigger = UpdateSourceTrigger.LostFocus };
                                var yBinding = new Binding(pi.Name) { Converter = new Vec2ElemConverter(1, DataContext, pi), ValidatesOnExceptions = true, ValidatesOnDataErrors = true, UpdateSourceTrigger = UpdateSourceTrigger.LostFocus };
                                xBinding.ValidationRules.Add(new FloatValidationRule());
                                yBinding.ValidationRules.Add(new FloatValidationRule());
                                x.SetBinding(TextBox.TextProperty, xBinding);
                                y.SetBinding(TextBox.TextProperty, yBinding);

                                targetPanel.Children.Add(lblGrid);

                                Grid stack = new Grid();
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

                                xBorder.Margin = yBorder.Margin = new Thickness(2);

                                stack.Children.Add(xBorder);
                                stack.Children.Add(yBorder);
                                targetPanel.Children.Add(stack);

                                controls.Add(x);
                                controls.Add(y);
                            }
                            else if (pi.PropertyType == typeof(Vector2))
                            {
                                TextBox x = new NumericTextBox() { Name = "xEdit" },
                                    y = new NumericTextBox() { Name = "yEdit" };

                                x.MinWidth = y.MinWidth = 60;
                                x.HorizontalAlignment = HorizontalAlignment.Stretch;
                                y.HorizontalAlignment = HorizontalAlignment.Stretch;

                                var xBinding = new Binding(pi.Name) { Converter = new Vec2ElemConverter(0, DataContext, pi), ValidatesOnExceptions = true, ValidatesOnDataErrors = true, UpdateSourceTrigger = UpdateSourceTrigger.LostFocus };
                                var yBinding = new Binding(pi.Name) { Converter = new Vec2ElemConverter(1, DataContext, pi), ValidatesOnExceptions = true, ValidatesOnDataErrors = true, UpdateSourceTrigger = UpdateSourceTrigger.LostFocus };
                                xBinding.ValidationRules.Add(new FloatValidationRule());
                                yBinding.ValidationRules.Add(new FloatValidationRule());
                                x.SetBinding(TextBox.TextProperty, xBinding);
                                y.SetBinding(TextBox.TextProperty, yBinding);

                                targetPanel.Children.Add(lblGrid);

                                Grid stack = new Grid();
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

                                xBorder.Margin = yBorder.Margin = new Thickness(2);

                                stack.Children.Add(xBorder);
                                stack.Children.Add(yBorder);
                                targetPanel.Children.Add(stack);

                                controls.Add(x);
                                controls.Add(y);
                            }
                            else if (pi.PropertyType == typeof(Vector3))
                            {
                                TextBox x = new NumericTextBox() { Name = "xEdit" },
                                    y = new NumericTextBox() { Name = "yEdit" },
                                    z = new NumericTextBox() { Name = "zEdit" };

                                x.MinWidth = y.MinWidth = z.MinWidth = 60;
                                x.HorizontalAlignment = HorizontalAlignment.Stretch;
                                y.HorizontalAlignment = HorizontalAlignment.Stretch;
                                z.HorizontalAlignment = HorizontalAlignment.Stretch;

                                var xBinding = new Binding(pi.Name) { Converter = new Vec3ElemConverter(0, DataContext, pi), ValidatesOnExceptions = true, ValidatesOnDataErrors = true, UpdateSourceTrigger = UpdateSourceTrigger.LostFocus };
                                var yBinding = new Binding(pi.Name) { Converter = new Vec3ElemConverter(1, DataContext, pi), ValidatesOnExceptions = true, ValidatesOnDataErrors = true, UpdateSourceTrigger = UpdateSourceTrigger.LostFocus };
                                var zBinding = new Binding(pi.Name) { Converter = new Vec3ElemConverter(2, DataContext, pi), ValidatesOnExceptions = true, ValidatesOnDataErrors = true, UpdateSourceTrigger = UpdateSourceTrigger.LostFocus };
                                xBinding.ValidationRules.Add(new FloatValidationRule());
                                yBinding.ValidationRules.Add(new FloatValidationRule());
                                zBinding.ValidationRules.Add(new FloatValidationRule());
                                x.SetBinding(TextBox.TextProperty, xBinding);
                                y.SetBinding(TextBox.TextProperty, yBinding);
                                z.SetBinding(TextBox.TextProperty, zBinding);

                                targetPanel.Children.Add(lblGrid);

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

                                targetPanel.Children.Add(stack);

                                controls.Add(x);
                                controls.Add(y);
                                controls.Add(z);
                            }
                            else if (pi.PropertyType == typeof(Microsoft.Xna.Framework.Vector4))
                            {
                                IValueConverter[] converters =
                                {
                                null,
                                null,
                                null,
                                null
                            };

                                Binding[] converterBindings =
                                    {
                                null,
                                null,
                                null,
                                null
                            };

                                Binding[] tipBindings =
                                {
                                null,
                                null,
                                null,
                                null
                            };

                                ShapeDataAttribute isShapeData = pi.GetCustomAttribute<ShapeDataAttribute>();
                                int elemCount = 4;
                                if (isShapeData != null)
                                {
                                    CachedPropertyInfo prop = grp.Properties.First((p) => { return p.Property.Name.Equals(isShapeData.PropertyName); });
                                    converters[0] = new Data.Converters.ShapeVisibilityConverter(prop.Property, 0);
                                    converters[1] = new Data.Converters.ShapeVisibilityConverter(prop.Property, 1);
                                    converters[2] = new Data.Converters.ShapeVisibilityConverter(prop.Property, 2);
                                    converters[3] = new Data.Converters.ShapeVisibilityConverter(prop.Property, 3);

                                    converterBindings[0] = new Binding(prop.Property.Name) { Converter = converters[0], NotifyOnSourceUpdated = true, UpdateSourceTrigger = UpdateSourceTrigger.LostFocus };
                                    converterBindings[1] = new Binding(prop.Property.Name) { Converter = converters[1], NotifyOnSourceUpdated = true, UpdateSourceTrigger = UpdateSourceTrigger.LostFocus };
                                    converterBindings[2] = new Binding(prop.Property.Name) { Converter = converters[2], NotifyOnSourceUpdated = true, UpdateSourceTrigger = UpdateSourceTrigger.LostFocus };
                                    converterBindings[3] = new Binding(prop.Property.Name) { Converter = converters[3], NotifyOnSourceUpdated = true, UpdateSourceTrigger = UpdateSourceTrigger.LostFocus };

                                    lbl.SetBinding(Label.ContentProperty, new Binding(prop.Property.Name) { Converter = new SprueKit.Data.Converters.ShapeTypeTooltipConverter(4), Mode = BindingMode.OneWay });
                                    tipBindings[0] = new Binding(prop.Property.Name) { Converter = new SprueKit.Data.Converters.ShapeTypeTooltipConverter(0) };
                                    tipBindings[1] = new Binding(prop.Property.Name) { Converter = new SprueKit.Data.Converters.ShapeTypeTooltipConverter(1) };
                                    tipBindings[2] = new Binding(prop.Property.Name) { Converter = new SprueKit.Data.Converters.ShapeTypeTooltipConverter(2) };
                                    tipBindings[3] = new Binding(prop.Property.Name) { Converter = new SprueKit.Data.Converters.ShapeTypeTooltipConverter(3) };

                                    //if (prop != null)
                                    //{
                                    //    SprueKit.Data.ShapeFunctionType value = (SprueKit.Data.ShapeFunctionType)prop.GetValue(DataContext);
                                    //    switch (value)
                                    //    {
                                    //        case ShapeFunctionType.Sphere:
                                    //            elemCount = 1;
                                    //            break;
                                    //        case ShapeFunctionType.Cone:
                                    //        case ShapeFunctionType.Capsule:
                                    //        case ShapeFunctionType.Torus:
                                    //            elemCount = 2;
                                    //            break;
                                    //        case ShapeFunctionType.Ellipsoid:
                                    //        case ShapeFunctionType.Box:
                                    //            elemCount = 3;
                                    //            break;
                                    //        case ShapeFunctionType.RoundedBox:
                                    //        case ShapeFunctionType.SuperShape:
                                    //        case ShapeFunctionType.Plane:
                                    //            elemCount = 4;
                                    //            break;
                                    //    }
                                    //}
                                }


                                targetPanel.Children.Add(lblGrid);

                                AutoGrid.StackPanel stack = new AutoGrid.StackPanel { Orientation = Orientation.Horizontal };
                                //Grid stack = new Grid();
                                //stack.HorizontalAlignment = HorizontalAlignment.Stretch;
                                //for (int i = 0; i < elemCount; ++i)
                                //    stack.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star), MinWidth=0 });

                                for (int i = 0; i < elemCount; ++i)
                                {
                                    TextBox x = new NumericTextBox();
                                    x.MinWidth = 50;
                                    x.HorizontalAlignment = HorizontalAlignment.Stretch;
                                    var xBinding = new Binding(pi.Name) { Converter = new Vec4ElemConverter(i, DataContext, pi), ValidatesOnExceptions = true, ValidatesOnDataErrors = true, UpdateSourceTrigger = UpdateSourceTrigger.LostFocus };
                                    xBinding.ValidationRules.Add(new FloatValidationRule());
                                    x.SetBinding(TextBox.TextProperty, xBinding);

                                    Border xBorder = new Border();
                                    xBorder.HorizontalAlignment = HorizontalAlignment.Stretch;
                                    xBorder.BorderThickness = new Thickness(0, 0, 0, 2);
                                    xBorder.BorderBrush = new SolidColorBrush(VectorColor[i]);
                                    xBorder.Child = x;
                                    xBorder.SetValue(Grid.ColumnProperty, i);
                                    xBorder.Margin = new Thickness(2);

                                    if (converterBindings[i] != null)
                                    {
                                        xBorder.SetBinding(Border.VisibilityProperty, converterBindings[i]);
                                        x.SetBinding(TextBox.VisibilityProperty, converterBindings[i]);
                                        x.SetBinding(TextBox.ToolTipProperty, tipBindings[i]);
                                        xBorder.SetValue(AutoGrid.StackPanel.FillProperty, AutoGrid.StackPanelFill.Fill);
                                    }

                                    stack.Children.Add(xBorder);
                                    controls.Add(x);
                                }

                                targetPanel.Children.Add(stack);
                            }
                            else if (pi.PropertyType.IsEnum)
                            {
                                ComboBox cb = new ComboBox();
                                var enumValues = Enum.GetValues(pi.PropertyType);
                                string[] enumNames = null;
                                if (pi.PropertyType.GetCustomAttribute<EnumNamesAttribute>() != null)
                                    enumNames = pi.PropertyType.GetCustomAttribute<EnumNamesAttribute>().EnumNames;
                                else
                                {
                                    List<string> ens = new List<string>();
                                    foreach (var en in enumValues)
                                        ens.Add(en.ToString().SplitCamelCase());
                                    enumNames = ens.ToArray();
                                }
                                List<KeyValuePair<object, string>> items = new List<KeyValuePair<object, string>>();
                                for (int i = 0; i < enumNames.Length; ++i)
                                    items.Add(new KeyValuePair<object, string>(enumValues.GetValue(i), enumNames[i]));
                                cb.ItemsSource = items;
                                cb.SelectedValuePath = "Key";
                                cb.DisplayMemberPath = "Value";
                                cb.SetBinding(ComboBox.SelectedValueProperty, new Binding(pi.Name));

                                cb.SetBinding(ComboBox.SelectedItemProperty, new Binding(pi.Name) { Mode = bindMode });
                                cb.Tag = pi;

                                targetPanel.Children.Add(lblGrid);
                                targetPanel.Children.Add(cb);
                                controls.Add(cb);
                            }
                            else if (pi.PropertyType == typeof(ResponseCurve))
                            {
                                ResponseCurveCtrl ctrl = new ResponseCurveCtrl() { MinHeight = 60, HorizontalAlignment = HorizontalAlignment.Stretch };
                                ctrl.SetBinding(ResponseCurveCtrl.CurveProperty, new Binding(pi.Name) { NotifyOnTargetUpdated = true });
                                ctrl.DataContext = DataContext;

                                Button btn = new Button { Content = ctrl, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0, 0, 4, 0), Padding = new Thickness(0) };
                                ctrl.SetBinding(ResponseCurveCtrl.WidthProperty, new Binding("ActualWidth") { Source = btn, NotifyOnTargetUpdated = true });

                                targetPanel.Children.Add(lblGrid);
                                targetPanel.Children.Add(btn);
                                btn.Click += (o, args) =>
                                {
                                    var popup = PopupHelper.Create();
                                    ResponseCurveEditor cctrl = new ResponseCurveEditor { MinHeight = 60, MinWidth = 60 };
                                    cctrl.SetBinding(ResponseCurveEditor.CurveProperty, new Binding(pi.Name) { UpdateSourceTrigger = UpdateSourceTrigger.Explicit });
                                    cctrl.DataContext = DataContext;
                                    popup.Grid.Children.Add(cctrl);
                                    popup.Popup.Closed += (ob, ars) => { ctrl.DataContext = null; ctrl.DataContext = DataContext; pi.SetValue(DataContext, cctrl.Curve.Clone()); };
                                    popup.ShowAtMouse();
                                };
                            }
                            else if (pi.PropertyType == typeof(Data.ColorRamp))
                            {
                                GradientRampEditor ctrl = new GradientRampEditor() { MinHeight = 20, HorizontalAlignment = HorizontalAlignment.Stretch };
                                ctrl.Ramp = ((Data.ColorRamp)pi.GetValue(DataContext)).Clone();
                                ctrl.RampChanged += (o, r) =>
                                {
                                    pi.SetValue(DataContext, r.Clone());
                                };
                                targetPanel.Children.Add(lblGrid);
                                targetPanel.Children.Add(ctrl);
                            }
                            else if (pi.PropertyType == typeof(Data.ColorCurves))
                            {
                                ColorCurvesEditor ctrl = new ColorCurvesEditor() { MinHeight = 60, HorizontalAlignment = HorizontalAlignment.Stretch };
                                ctrl.Curves = ((Data.ColorCurves)pi.GetValue(DataContext)).Clone();
                                ctrl.CurveChanged += (o, c) =>
                                {
                                    pi.SetValue(DataContext, c.Clone());
                                };
                                targetPanel.Children.Add(lblGrid);
                                targetPanel.Children.Add(ctrl);
                            }
                            else if (pi.PropertyType == typeof(Uri))
                            {
                                Grid subGrid = new Grid();
                                subGrid.ColumnDefinitions.Add(new ColumnDefinition());
                                subGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(32) });
                                subGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(24) });

                                string fileMask = null;
                                Regex[] regexList= null;
                                if (pi.Name.Equals("Texture"))
                                {
                                    regexList = FileData.ImageRegex;
                                    fileMask = FileData.ImageFileMask;

                                    var attr = pi.GetCustomAttribute<PropertyData.ResourceTagAttribute>();
                                    if (attr != null)
                                    {
                                        if (attr.Type == PropertyData.ResourceTagType.SVGTexture)
                                        {
                                            regexList = FileData.SVGRegex;
                                            fileMask = FileData.SVGFileMask;
                                        }
                                    }

                                    subGrid.RowDefinitions.Add(new RowDefinition());
                                    subGrid.RowDefinitions.Add(new RowDefinition());
                                    Image img = new Image() { MaxWidth = 64, MaxHeight = 64, Stretch = Stretch.Uniform };
                                    img.Margin = new Thickness(5, 2, 5, 2);
                                    img.HorizontalAlignment = HorizontalAlignment.Left;
                                    Grid.SetRow(img, 1);
                                    Grid.SetColumnSpan(img, 3);
                                    img.SetBinding(Image.SourceProperty, new Binding("Thumbnail") { Mode = BindingMode.OneWay, Source = DataContext });
                                    subGrid.Children.Add(img);
                                    controls.Add(img);

                                    img.MouseUp += (oo, mEvt) =>
                                    {
                                        var pop = PopupHelper.Create();
                                        Image popupImage = new Image() { MaxWidth = 512, MaxHeight = 512, MinWidth = 0, MinHeight = 0, Stretch = Stretch.Uniform };
                                        popupImage.SetBinding(Image.SourceProperty, new Binding("Thumbnail") { Source = DataContext });
                                        popupImage.SetBinding(Image.WidthProperty, new Binding("Thumbnail.Width") { Source = DataContext });
                                        popupImage.SetBinding(Image.HeightProperty, new Binding("Thumbnail.Height") { Source = DataContext });
                                        pop.Grid.Children.Add(popupImage);
                                        pop.ShowAtMouse();
                                    };
                                }
                                else if (pi.Name.Equals("SprueModel"))
                                {
                                    regexList = FileData.SprueModelRegex;
                                    fileMask = FileData.SprueModelMask;
                                }
                                else
                                {
                                    // TODO: only models so far
                                    regexList = FileData.ModelRegex;
                                    fileMask = FileData.ModelFileMask;
                                }

                                UriTextBox uriTextBox = new UriTextBox();
                                uriTextBox.RegexChecks = regexList;
                                uriTextBox.IsReadOnly = true;
                                uriTextBox.SetBinding(TextBox.TextProperty, new Binding(pi.Name) { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
                                subGrid.Children.Add(uriTextBox);

                                Button btn = new Button { Content = "...", Padding = new Thickness(0) };
                                btn.Click += (src, args) => {
                                    var dlg = new System.Windows.Forms.OpenFileDialog();
                                    dlg.Filter = fileMask;
                                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                                        propInfo.Property.SetValue(DataContext, new Uri(dlg.FileName));
                                };

                                Label xLbl = new Label { Content = "X", FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Colors.Red), IsHitTestVisible = true };
                                xLbl.MouseLeftButtonDown += (o, be) => { propInfo.Property.SetValue(DataContext, null); };
                                Grid.SetColumn(xLbl, 2);
                                Grid.SetColumn(btn, 1);
                                subGrid.Children.Add(btn);
                                subGrid.Children.Add(xLbl);

                                targetPanel.Children.Add(lblGrid);
                                targetPanel.Children.Add(subGrid);
                                controls.Add(uriTextBox);
                            }
                            else if (typeof(ObservableCollection<TextureMap>).IsAssignableFrom(pi.PropertyType))
                            {
                                MaterialList matList = new Controls.MaterialList();
                                matList.SetBinding(MaterialList.DataContextProperty, new Binding(pi.Name) { Source = DataContext });
                                targetPanel.Children.Add(lblGrid);
                                targetPanel.Children.Add(matList);
                                //controls.Add(matList);
                            }
                            else if (pi.PropertyType == typeof(ForeignModel))
                            {
                                ModelControl mdlList = new ModelControl();
                                mdlList.SetBinding(ModelControl.DataContextProperty, new Binding(pi.Name) { Source = DataContext });
                                targetPanel.Children.Add(lblGrid);
                                targetPanel.Children.Add(mdlList);
                            }
                            else if (pi.PropertyType == typeof(PluginLib.Mat3x3))
                            {
                                Grid g = new Grid();
                                g.RowDefinitions.Add(new RowDefinition());
                                g.RowDefinitions.Add(new RowDefinition());
                                g.RowDefinitions.Add(new RowDefinition());
                                g.ColumnDefinitions.Add(new ColumnDefinition());
                                g.ColumnDefinitions.Add(new ColumnDefinition());
                                g.ColumnDefinitions.Add(new ColumnDefinition());

                                for (int x = 0; x < 3; ++x)
                                {
                                    for (int y = 0; y < 3; ++y)
                                    {
                                        var xBinding = new Binding(pi.Name) {
                                            Converter = new Mat3x3ElemConverter(x, y, DataContext, pi), ValidatesOnExceptions = true, ValidatesOnDataErrors = true, UpdateSourceTrigger = UpdateSourceTrigger.LostFocus };

                                        TextBox tb = new TextBox();
                                        tb.SetBinding(TextBox.TextProperty, xBinding);
                                        Grid.SetRow(tb, x);
                                        Grid.SetColumn(tb, y);
                                        g.Children.Add(tb);
                                    }
                                }
                                targetPanel.Children.Add(lblGrid);
                                targetPanel.Children.Add(g);
                            }
                            else if (pi.PropertyType == typeof(Data.FontSpec))
                            {
                                var value = pi.GetValue(DataContext) as Data.FontSpec;
                                Label valLbl = new Label { Content = value.ToDisplayString() };
                                Button btn = new Button { Content = "Select font", HorizontalAlignment = HorizontalAlignment.Right, MaxWidth = 120 };
                                btn.Click += (o, c) =>
                                {
                                    System.Windows.Forms.FontDialog dlg = new System.Windows.Forms.FontDialog();
                                    dlg.Font = value.GetFont();
                                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                                    {
                                        value = new Data.FontSpec(dlg.Font);
                                        pi.SetValue(DataContext, value);
                                        valLbl.Content = value.ToDisplayString();
                                    }
                                };

                                targetPanel.Children.Add(lblGrid);
                                targetPanel.Children.Add(valLbl);
                                targetPanel.Children.Add(btn);
                            }
                            else
                            {
                                //ErrorHandler.inst().Debug(string.Format("Unhandled property type {0}", pi.PropertyType.Name));
                                //System.Diagnostics.Debug.Assert(false, string.Format("Unhandled property type {0}", pi.PropertyType.Name));
                            }

                            ++row;
                        }
                    }

                    foreach (var cmdInfo in propertyCmds)
                    {
                        Button btn = new StyledButton();
                        btn.Content = cmdInfo.DisplayText;
                        if (!string.IsNullOrWhiteSpace(cmdInfo.Tip))
                            btn.ToolTip = cmdInfo.Tip;
                        btn.Click += (o, ee) => { cmdInfo.Execute(DataContext); };
                        formGrid.Children.Add(btn);
                    }

                    foreach (FrameworkElement ctrl in controls)
                        ctrl.DataContext = DataContext;
                }
            }
        }

        private void Img_MouseUp(object sender, MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }

        static bool TypeAllowsPermutations(Type t)
        {
            if (t == typeof(ObservableCollection<TextureMap>))
                return false;
            return true;
        }

        static bool FieldAllowsPermutations(PropertyInfo pi)
        {
            return pi.GetCustomAttribute<PropertyData.AllowPermutationsAttribute>() != null;
        }
    }
}
