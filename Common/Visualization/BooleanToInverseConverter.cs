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
    /// Returns the inverse of the supplied boolean.
    /// </summary>
    public class BooleanToInverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool))
                throw new InvalidOperationException("Target to convert is not a Boolean.");

            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (!(value is bool))
                throw new InvalidOperationException("Target to convert-back is not a Boolean.");

            return !(bool)value;
        }
    }
}
