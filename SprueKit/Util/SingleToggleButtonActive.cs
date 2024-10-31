using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

namespace SprueKit.Util
{
    /// <summary>
    /// Helper class for setting up toggle buttons to only allow a single active button
    /// </summary>
    public class ToggleButtonGroup
    {
        public static void Setup(params ToggleButton[] buttons)
        {
            // Seperate handlers are setup because the logic could get a little convoluted otherwise
            foreach (var btn in buttons)
            {
                // Enforce ourselves as the only button checked
                btn.Checked += (o, evt) => {
                    if (btn.IsChecked.HasValue && btn.IsChecked.Value)
                    {
                        foreach (var b in buttons)
                        {
                            if (b != btn)
                                b.IsChecked = false;
                        }
                    }
                };

                // If no one is checked as a result of unchecking us, then check the first button
                btn.Unchecked += (o, evt) =>
                {
                    foreach (var b in buttons)
                        if (b.IsChecked.HasValue && b.IsChecked.Value)
                            return;
                    buttons[0].IsChecked = true;
                };
            }
        }

        // Allows multiple items to be clustered together into sets
        public static void Setup(ToggleButton[] buttons, int[] setID)
        {
            for (int i = 0; i < buttons.Length; ++i)
            {
                // Enforce ourselves as the only button checked
                var btn = buttons[i];
                btn.Checked += (o, evt) => {
                    if (btn.IsChecked.HasValue && btn.IsChecked.Value)
                    {
                        for (int j = 0; j < buttons.Length; ++j)
                        {
                            if (setID[j] == setID[i])
                                buttons[j].IsChecked = false;
                        }
                    }
                };

                // If no one is checked as a result of unchecking us, then check the first button
                btn.Unchecked += (o, evt) =>
                {
                    for (int j = 0; j < buttons.Length; ++j)
                    {
                        if (setID[j] == setID[i])
                        {
                            var b = buttons[j];
                            if (b.IsChecked.HasValue && b.IsChecked.Value)
                                return;
                        }
                    }

                    // check first one in the set
                    for (int j = 0; j < buttons.Length; ++j)
                    {
                        if (setID[j] == setID[i])
                        {
                            var b = buttons[j];
                            b.IsChecked = true;
                            return;
                        }
                    }
                };
            }
        }
    }
}
