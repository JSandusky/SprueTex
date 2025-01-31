﻿/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

namespace ColorPickerLib.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Data;
    using System.Globalization;
    using System.Windows
    ;
    /// <summary>
    /// Sets the margin for the thumb grip, the top buttons, or for the content border in the WindowControl.
    /// </summary>
    public class WindowContentBorderMarginConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double horizontalContentBorderOffset = (double)values[0];
            double verticalContentBorderOffset = (double)values[1];

            switch ((string)parameter)
            {
                // Content Border Margin in the WindowControl
                case "0":
                    return new Thickness(horizontalContentBorderOffset
                                        , 0d
                                        , horizontalContentBorderOffset
                                        , verticalContentBorderOffset);
                // Thumb Grip Margin in the WindowControl
                case "1":
                    return new Thickness(0d
                                        , 0d
                                        , horizontalContentBorderOffset
                                        , verticalContentBorderOffset);
                // Header Buttons Margin in the WindowControl
                case "2":
                    return new Thickness(0d
                                        , 0d
                                        , horizontalContentBorderOffset
                                        , 0d);
                default:
                    throw new NotSupportedException("'parameter' for WindowContentBorderMarginConverter is not valid.");
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
