using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Instrumind.Common.Visualization.Widgets
{
    public class ExtendedDataGridTemplateColumn : DataGridTemplateColumn
    {
        protected override object PrepareCellForEdit(FrameworkElement EditingElement, RoutedEventArgs EditingEventArgs)
        {
            EditingElement.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));

            return base.PrepareCellForEdit(EditingElement, EditingEventArgs);
        } 
    }
}
