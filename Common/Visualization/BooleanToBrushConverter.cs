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
using System.Windows.Media;

namespace Instrumind.Common.Visualization
{
    /// <summary>
    /// Returns either a brush or other, from a boolean.
    /// </summary>
    public class BooleanToBrushConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 3)
                return null;

            var SelectThePrimary = (bool)values[0];
            var PrimaryBrush = (Brush)values[1];
            var SecondaryBrush = (Brush)values[2];

            return (SelectThePrimary ? PrimaryBrush : SecondaryBrush);
        }

        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
