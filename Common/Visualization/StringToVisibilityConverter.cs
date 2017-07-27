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
    /// Same as BooleanToVisibilityHiddenConverter, but evaluating the existence/abscence of a string.
    /// </summary>
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string Text = value.ToStringAlways().Trim();
            return (Text.IsAbsent() ? Visibility.Collapsed : Visibility.Visible);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((value is Visibility) && (((Visibility)value) == Visibility.Visible) ? "." : null);
        }
    }
}
