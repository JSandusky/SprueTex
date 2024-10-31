using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SprueKit.Controls
{
    public static class LayoutExt
    {
        public static void AddControls(this Grid grid, params UIElement[] children)
        {
            if (children.Length == 1)
            {
                grid.RowDefinitions.Add(new RowDefinition());
                Grid.SetColumn(children[0], 0);
                Grid.SetColumnSpan(children[0], grid.ColumnDefinitions.Count);
            }
            else
            {
                grid.RowDefinitions.Add(new RowDefinition());
                for (int i = 0; i < children.Length; ++i)
                {
                    int span = 1;
                    if (i == children.Length - 1)
                        span = grid.ColumnDefinitions.Count - children.Length;
                    Grid.SetColumn(children[i], i);
                    Grid.SetColumnSpan(children[i], span);
                }
            }
        }
    }
}
