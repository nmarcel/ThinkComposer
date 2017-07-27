/*
 * This was just adapted from the standard WPF converter.
 */
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Instrumind.Common.Visualization
{
    /// <summary>
    /// Converts to bool (true) is the String is not empty nor null.
    /// </summary>
    public class StringExistentToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string Text = value.ToStringAlways().Trim();
            return !Text.IsAbsent();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((value is bool) && ((bool)value) ? "." : null);
        }
    }
}
