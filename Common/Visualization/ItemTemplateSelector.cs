using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Instrumind.Common.Visualization
{
    /// <summary>
    /// Provides the standard item templates based on data-type.
    /// </summary>
    public class ItemTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate
            SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement Element = container as FrameworkElement;
            if (Element == null)
                return null;

            if (item is SimplePresentationElement)
                return Display.GetResource<DataTemplate>("TplSimplePresentationElement");

            if (item is FormalPresentationElement)
                return Display.GetResource<DataTemplate>("TplFormalPresentationElement");

            return null;
        }
    }
}
