﻿/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

namespace ColorPickerLib
{
    using System;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Base class for controls that represents controls that can spin.
    /// </summary>
    public abstract class Spinner : Control
    {
        #region Properties

        /// <summary>
        /// Identifies the ValidSpinDirection dependency property.
        /// </summary>
        public static readonly DependencyProperty ValidSpinDirectionProperty = DependencyProperty.Register("ValidSpinDirection", typeof(ValidSpinDirections), typeof(Spinner), new PropertyMetadata(ValidSpinDirections.Increase | ValidSpinDirections.Decrease, OnValidSpinDirectionPropertyChanged));
        public ValidSpinDirections ValidSpinDirection
        {
            get
            {
                return (ValidSpinDirections)GetValue(ValidSpinDirectionProperty);
            }
            set
            {
                SetValue(ValidSpinDirectionProperty, value);
            }
        }

        /// <summary>
        /// ValidSpinDirectionProperty property changed handler.
        /// </summary>
        /// <param name="d">ButtonSpinner that changed its ValidSpinDirection.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnValidSpinDirectionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Spinner source = (Spinner)d;
            ValidSpinDirections oldvalue = (ValidSpinDirections)e.OldValue;
            ValidSpinDirections newvalue = (ValidSpinDirections)e.NewValue;
            source.OnValidSpinDirectionChanged(oldvalue, newvalue);
        }

        #endregion //Properties

        /// <summary>
        /// Occurs when spinning is initiated by the end-user.
        /// </summary>
        public event EventHandler<SpinEventArgs> Spin;

        /// <summary>
        /// Initializes a new instance of the Spinner class.
        /// </summary>
        protected Spinner()
        {
        }

        /// <summary>
        /// Raises the OnSpin event when spinning is initiated by the end-user.
        /// </summary>
        /// <param name="e">Spin event args.</param>
        protected virtual void OnSpin(SpinEventArgs e)
        {
            ValidSpinDirections valid = e.Direction == SpinDirection.Increase ? ValidSpinDirections.Increase : ValidSpinDirections.Decrease;

            //Only raise the event if spin is allowed.
            if ((ValidSpinDirection & valid) == valid)
            {
                EventHandler<SpinEventArgs> handler = Spin;
                if (handler != null)
                {
                    handler(this, e);
                }
            }
        }

        /// <summary>
        /// Called when valid spin direction changed.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnValidSpinDirectionChanged(ValidSpinDirections oldValue, ValidSpinDirections newValue)
        {
        }
    }
}
