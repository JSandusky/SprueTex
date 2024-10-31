using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace SprueKit.Controls
{
    public class NumericTextBox : TextBox
    {
        public static readonly DependencyProperty IsIntegerProperty = DependencyProperty.Register("IsInteger", typeof(bool), typeof(NumericTextBox), new PropertyMetadata(false));

        public bool IsInteger { get { return (bool)GetValue(IsIntegerProperty); } set { SetValue(IsIntegerProperty, value); } }

        public NumericTextBox()
        {
            PreviewTextInput += NumericTextBox_PreviewTextInput;

            AddHandler(PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(SelectivelyIgnoreMouseButton), true);
            AddHandler(GotKeyboardFocusEvent, new RoutedEventHandler(SelectAllText), true);
            AddHandler(MouseDoubleClickEvent, new RoutedEventHandler(SelectAllText), true);
            PreviewMouseMove += NumericTextBox_PreviewMouseMove;

            GotFocus += NumericTextBox_GotFocus;
            KeyUp += NumericTextBox_KeyUp;
        }

        private void NumericTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            SelectAll();
        }

        private void NumericTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                if (e.Key == Key.Up)
                    IncrementTextBox(sender as TextBox, 0.5f);
                else if (e.Key == Key.Down)
                    IncrementTextBox(sender as TextBox, -0.5f);
                else if (e.Key == Key.PageUp)
                    IncrementTextBox(sender as TextBox, 5.0f);
                else if (e.Key == Key.PageDown)
                    IncrementTextBox(sender as TextBox, -5.0f);
            }
            else
            {
                if (e.Key == Key.Up)
                    IncrementTextBox(sender as TextBox, 0.2f);
                else if (e.Key == Key.Down)
                    IncrementTextBox(sender as TextBox, -0.2f);
                else if (e.Key == Key.PageUp)
                    IncrementTextBox(sender as TextBox, 1.0f);
                else if (e.Key == Key.PageDown)
                    IncrementTextBox(sender as TextBox, -1.0f);
            }
        }

        void IncrementTextBox(TextBox tb, float delta)
        {
            if (IsInteger)
            {
                int junk = 0;
                if (int.TryParse(tb.Text, out junk))
                {
                    junk += (int)Math.Round(delta, 0, MidpointRounding.AwayFromZero);
                    tb.Text = junk.ToString();

                    BindingExpression be = GetBindingExpression(TextBox.TextProperty);
                    if (be != null)
                        be.UpdateSource();
                }
            }
            else
            { 
                float junk = 0.0f;
                if (float.TryParse(tb.Text, out junk))
                {
                    junk += delta;
                    tb.Text = junk.ToString();

                    BindingExpression be = GetBindingExpression(TextBox.TextProperty);
                    if (be != null)
                        be.UpdateSource();
                }
            }
        }

        private void NumericTextBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            var mousePos = e.GetPosition(this);
            int hitCaret = this.GetCharacterIndexFromPoint(mousePos, false);
            if (hitCaret >= Text.Length && IsFocused)
            {
                this.Cursor = Cursors.SizeWE;
                e.Handled = true;
            }
            else
                this.Cursor = null;
        }

        private static void SelectivelyIgnoreMouseButton(object sender,
                                                     MouseButtonEventArgs e)
        {
            // Find the TextBox
            DependencyObject parent = e.OriginalSource as UIElement;
            while (parent != null && !(parent is TextBox))
                parent = System.Windows.Media.VisualTreeHelper.GetParent(parent);

            if (parent != null)
            {
                var textBox = (TextBox)parent;
                if (!textBox.IsKeyboardFocusWithin)
                {
                    // If the text box is not yet focussed, give it the focus and
                    // stop further processing of this click event.
                    textBox.Focus();
                    e.Handled = true;
                }
            }
        }

        private static void SelectAllText(object sender, RoutedEventArgs e)
        {
            var textBox = e.OriginalSource as TextBox;
            if (textBox != null)
                textBox.SelectAll();
        }

        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            string result = PreProcess(e.Text);
            double number;
            bool isNumber = TryParse(result, out number);

            // prevent textbox from handling new text when result is not a number
            e.Handled = !isNumber;

            base.OnTextInput(e);
        }

        private string PreProcess(string input)
        {
            string original = Text;
            int start = SelectionStart;
            int length = SelectionLength;

            string head = original.Substring(0, start);
            string tail = original.Substring(start + length);

            return head + input + tail;
        }

        private bool TryParse(string text, out double number)
        {
            var culture = CultureInfo.CurrentCulture;
            NumberStyles styles = GetAllowedStyles();

            if (text.StartsWith(culture.NumberFormat.NumberDecimalSeparator))
                text = "0" + text;

            return double.TryParse(text, styles, culture, out number) || 
                ((text.Equals("-") || text.Equals("+"))
                && (styles & NumberStyles.AllowLeadingSign) != 0);
        }

        private NumberStyles GetAllowedStyles()
        {
            NumberStyles current = NumberStyles.None;

            current |= NumberStyles.AllowLeadingSign;

            if (AllowLeadingWhite)
                current |= NumberStyles.AllowLeadingWhite;

            if (AllowTrailingWhite)
                current |= NumberStyles.AllowTrailingWhite;

            if (AllowLeadingSign)
                current |= NumberStyles.AllowLeadingSign;

            if (AllowTrailingSign)
                current |= NumberStyles.AllowTrailingSign;

            if (AllowParentheses)
                current |= NumberStyles.AllowParentheses;

            if (AllowDecimalPoint)
                current |= NumberStyles.AllowDecimalPoint;

            if (AllowThousands)
                current |= NumberStyles.AllowThousands;

            if (AllowCurrencySymbol)
                current |= NumberStyles.AllowCurrencySymbol;

            return current;
        }

        #region AllowLeadingWhite
        public bool AllowLeadingWhite
        {
            get { return (bool)GetValue(AllowLeadingWhiteProperty); }
            set { SetValue(AllowLeadingWhiteProperty, value); }
        }

        public static readonly DependencyProperty AllowLeadingWhiteProperty =
            DependencyProperty.Register(
                "AllowLeadingWhite",
                typeof(bool),
                typeof(NumericTextBox),
                new PropertyMetadata(false));
        #endregion

        #region AllowTrailingWhite
        public bool AllowTrailingWhite
        {
            get { return (bool)GetValue(AllowTrailingWhiteProperty); }
            set { SetValue(AllowTrailingWhiteProperty, value); }
        }

        public static readonly DependencyProperty AllowTrailingWhiteProperty =
            DependencyProperty.Register(
                "AllowTrailingWhite",
                typeof(bool),
                typeof(NumericTextBox),
                new PropertyMetadata(false));
        #endregion

        #region AllowLeadingSign
        public bool AllowLeadingSign
        {
            get { return (bool)GetValue(AllowLeadingSignProperty); }
            set { SetValue(AllowLeadingSignProperty, value); }
        }

        public static readonly DependencyProperty AllowLeadingSignProperty =
            DependencyProperty.Register(
                "AllowLeadingSign",
                typeof(bool),
                typeof(NumericTextBox),
                new PropertyMetadata(true));
        #endregion

        #region AllowTrailingSign
        public bool AllowTrailingSign
        {
            get { return (bool)GetValue(AllowTrailingSignProperty); }
            set { SetValue(AllowTrailingSignProperty, value); }
        }

        public static readonly DependencyProperty AllowTrailingSignProperty =
            DependencyProperty.Register(
                "AllowTrailingSign",
                typeof(bool),
                typeof(NumericTextBox),
                new PropertyMetadata(false));
        #endregion

        #region AllowParentheses
        public bool AllowParentheses
        {
            get { return (bool)GetValue(AllowParenthesesProperty); }
            set { SetValue(AllowParenthesesProperty, value); }
        }

        public static readonly DependencyProperty AllowParenthesesProperty =
            DependencyProperty.Register(
                "AllowParentheses",
                typeof(bool),
                typeof(NumericTextBox),
                new PropertyMetadata(false));
        #endregion

        #region AllowDecimalPoint
        public bool AllowDecimalPoint
        {
            get { return (bool)GetValue(AllowDecimalPointProperty); }
            set { SetValue(AllowDecimalPointProperty, value); }
        }

        public static readonly DependencyProperty AllowDecimalPointProperty =
            DependencyProperty.Register(
                "AllowDecimalPoint",
                typeof(bool),
                typeof(NumericTextBox),
                new PropertyMetadata(true));
        #endregion

        #region AllowThousands
        public bool AllowThousands
        {
            get { return (bool)GetValue(AllowThousandsProperty); }
            set { SetValue(AllowThousandsProperty, value); }
        }

        public static readonly DependencyProperty AllowThousandsProperty =
            DependencyProperty.Register(
                "AllowThousands",
                typeof(bool),
                typeof(NumericTextBox),
                new PropertyMetadata(false));
        #endregion

        #region AllowCurrencySymbol
        public bool AllowCurrencySymbol
        {
            get { return (bool)GetValue(AllowCurrencySymbolProperty); }
            set { SetValue(AllowCurrencySymbolProperty, value); }
        }

        public static readonly DependencyProperty AllowCurrencySymbolProperty =
            DependencyProperty.Register(
                "AllowCurrencySymbol",
                typeof(bool),
                typeof(NumericTextBox),
                new PropertyMetadata(false));
        #endregion

        bool _hasFocus = false;
        private const string DEFAULT_VALUE = "0";
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            _hasFocus = true;

            //double number;
            //TryParse(Text, out number);
            //if (number == 0)
                SelectAll();

            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            _hasFocus = false;

            if (Text == string.Empty || Text.Equals("-") || Text.Equals("+"))
                Text = DEFAULT_VALUE;

            base.OnLostFocus(e);
        }
    }
}
