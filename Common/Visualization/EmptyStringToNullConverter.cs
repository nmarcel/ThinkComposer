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
    /// Returns null if string is empty.
    /// </summary>
    public class EmptyStringToNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string Text = value.ToStringAlways().Trim();
            Text = (Text.IsAbsent() ? null : Text);
            return Text;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((value is string) ? value.ToStringAlways() : null);
        }
    }
}
