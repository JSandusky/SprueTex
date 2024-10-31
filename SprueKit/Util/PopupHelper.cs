using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace SprueKit
{

    public class PopupHelper : BaseClass
    {
        public Popup Popup { get; private set; }
        public Grid Grid { get; private set; }

        Point pt_;
        public Point Pos { get { return pt_; } set { pt_ = value; OnPropertyChanged(); } }

        protected PopupHelper() { }

        public static PopupHelper Create()
        {
            PopupHelper ret = new PopupHelper();

            ret.Popup = new Popup();
            //ret.Popup.KeyUp += (o, e) => { if (e.Key == Key.Escape) pop.IsOpen = false; };

            Border winBrdr = new Border { BorderThickness = new Thickness(1) };
            winBrdr.SetResourceReference(Border.BackgroundProperty, "PopupBackground");
            winBrdr.SetResourceReference(Border.BorderBrushProperty, "WindowBorderActive");
            ret.Grid = new Grid() { Focusable = true };
            ret.Grid.Margin = new Thickness(4);
            winBrdr.Child = ret.Grid;
            ret.Popup.Child = winBrdr;

            return ret;
        }

        public void ShowAtMouse(bool sticky = false)
        {
            ShowAtMouse(sticky, Application.Current.MainWindow);
        }

        public void ShowAtMouse(bool sticky, Control win)
        {
            Popup.Placement = PlacementMode.MousePoint;
            Popup.HorizontalAlignment = HorizontalAlignment.Center;
            Popup.PlacementTarget = win;
            Popup.IsOpen = true;
            Popup.StaysOpen = sticky;
            Popup.Focusable = true;
            Pos = Mouse.GetPosition(win);
            if (!sticky)
                LinkEscape(Popup.PlacementTarget as Control);
            Grid.Focus();
        }

        public void ShowAtMouse(bool sticky, Window win)
        {
            Popup.Placement = PlacementMode.MousePoint;
            Popup.HorizontalAlignment = HorizontalAlignment.Center;
            Popup.PlacementTarget = win;
            Popup.IsOpen = true;
            Popup.StaysOpen = sticky;
            Popup.Focusable = true;
            Pos = Mouse.GetPosition(win);
            if (!sticky)
                LinkEscape(Grid);
            Grid.Focus();
        }

        public void Hide()
        {
            Popup.IsOpen = false;
        }

        public void LinkEscape(UIElement ctrl)
        {
            if (ctrl == null)
                return;

            Popup.PreviewKeyDown += (o,e) => { if (e.Key == Key.Escape) { Popup.IsOpen = false; e.Handled = true; } };
            Popup.PreviewKeyUp += (o, e) => { if (e.Key == Key.Escape) { Popup.IsOpen = false; e.Handled = true; } };
        }
    }
}
