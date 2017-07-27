/*
 * This should be implemented in future versions of the .NET Framework!
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
    /// Returns a double from a boolean, either 1.0 for true or 0.0 for false.
    /// </summary>
    public class BooleanToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool))
                throw new InvalidOperationException("Target to convert is not a Boolean.");

            return ((bool)value ? 1.0 : 0.0);
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (!(value is double))
                throw new InvalidOperationException("Target to convert-back is not a Double.");

            return (((double)value) == 1.0);
        }
    }
}
