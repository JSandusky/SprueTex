using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using SprueKit.Commands;

namespace SprueKit.Pages.Settings
{
    /// <summary>
    /// Interaction logic for ShortCuts.xaml
    /// </summary>
    public partial class ShortCuts : UserControl
    {
        public ShortCuts()
        {
            InitializeComponent();
            shortCutGroups.ItemsSource = App.ShortCuts;
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void TextBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            // DO NOT RESPOND TO SYSTEM KEYS!
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
                return;
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
                return;
            if (e.Key == Key.LeftAlt || e.Key == Key.RightAlt)
                return;

            ShortCut sc = ((TextBox)sender).DataContext as ShortCut;
            e.Handled = true;
            if (Keyboard.Modifiers != ModifierKeys.None && e.Key != Key.Escape && e.Key != Key.None)
            {
                sc.Key = e.Key;
                sc.Modifiers = Keyboard.Modifiers;
            }
            else if (e.Key == Key.Escape)
            {
                sc.Key = Key.None;
                sc.Modifiers = ModifierKeys.None;
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            App.BlockShortCuts = true;
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            App.BlockShortCuts = false;
        }
    }
}
