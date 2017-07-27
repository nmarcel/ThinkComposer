using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.MetaModel;

namespace Instrumind.ThinkComposer.ApplicationProduct.Widgets
{
    public static class WidgetsHelper
    {
        /// <summary>
        /// Size of a color-sample rectangle, including margin.
        /// </summary>
        public static readonly Size COLOR_SAMPLE_SIZE = new Size(78.0, 20.0);

        /// <summary>
        /// Margin uniform thickness for color-sample rectangles.
        /// </summary>
        public const double COLOR_SAMPLE_MARGIN = 1.0;

        /// <summary>
        /// Gets image objects to be shown as selectable graphic Styles sample for items-selection widgets.
        /// </summary>
        public static IEnumerable<Image> GenerateGraphicStyleSamples()
        {
            var SampleGeometry = new RectangleGeometry(new Rect(new Size(COLOR_SAMPLE_SIZE.Width / 2.2 - 6, COLOR_SAMPLE_SIZE.Height - 4)), 3.0, 3.0);

            foreach (var Style in Domain.ApplicableGraphicStyles)
            {
                var Pencil = new Pen(Style.Item1, Style.Item2);
                Pencil.DashStyle = Style.Item3;

                var SampleDrawing = new GeometryDrawing(Style.Item4, Pencil, SampleGeometry);
                SampleDrawing.ToDrawingImage();

                var SampleResult = new System.Windows.Controls.Image();
                SampleResult.Stretch = Stretch.None;
                SampleResult.Source = SampleDrawing.ToDrawingImage();
                SampleResult.Width = COLOR_SAMPLE_SIZE.Width / 2.1 - COLOR_SAMPLE_MARGIN * 2;
                SampleResult.Height = COLOR_SAMPLE_SIZE.Height - COLOR_SAMPLE_MARGIN * 2;
                SampleResult.Margin = new Thickness(COLOR_SAMPLE_MARGIN);
                SampleResult.Tag = Style;

                yield return SampleResult;
            }
        }
    }
}
