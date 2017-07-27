using System;
using System.Collections.Generic;
using System.Text;

namespace PdfSharp.Xps.XpsModel
{
  /// <summary>
  /// Fills a region with a radial gradient.
  /// </summary>
  class RadialGradientBrush : Brush
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="RadialGradientBrush"/> class.
    /// </summary>
    public RadialGradientBrush()
    {
      Opacity = 1;
      Transform = new MatrixTransform();
    }

    /// <summary>
    /// Defines the uniform transparency of the brush fill. Values range from 0 (fully transparent)
    /// to 1 (fully opaque), inclusive. Values outside of this range are invalid.
    public double Opacity { get; set; }

    /// <summary>
    /// Specifies a name for a resource in a resource dictionary. x:Key MUST be present when the
    /// current element is defined in a resource dictionary. x:Key MUST NOT be specified outside of
    /// a resource dictionary [M6.4].
    /// </summary>
    // x:Key
    public string Key { get; set; }    

    /// <summary>
    /// Specifies the gamma function for color interpolation. The gamma adjustment should not be
    /// applied to the alpha component, if specified. Valid values are SRgbLinearInterpolation and
    /// ScRgbLinearInterpolatio n. 
    /// </summary>
    public ClrIntMode ColorInterpolationMode { get; set; }

    /// <summary>
    /// Describes how the brush should fill the content area outside of the primary, initial gradient area.
    /// Valid values are Pad, Reflect and Repeat.
    /// </summary>
    public SpreadMethod SpreadMethod { get; set; }

    /// <summary>
    /// Specifies that the start point and end point are defined in the effective coordinate space
    /// (includes the Transform attribute of the brush). 
    /// </summary>
    public MappingMode MappingMode { get; set; }

    /// <summary>
    /// Describes the matrix transformation applied to the coordinate space of the brush.
    /// The Transform property is concatenated with the current effective render transform to yield
    /// an effective render transform local to the brush. The viewport for the brush is transformed
    /// using that local effective render transform. 
    /// </summary>
    public MatrixTransform Transform { get; set; }

    /// <summary>
    /// Specifies the center point of the radial gradient (that is, the center of the ellipse). The radial
    /// gradient brush interpolates the colors from the gradient origin to the circumference of the
    /// ellipse. The circumference is determined by the center and the radii. 
    /// </summary>
    public Point Center { get; set; }

    /// <summary>
    /// Specifies the origin point of the radial gradient.
    /// </summary>
    public Point GradientOrigin { get; set; }

    /// <summary>
    /// Specifies the radius in the x dimension of the ellipse which defines the radial gradient. 
    /// </summary>
    public double RadiusX { get; set; }

    /// <summary>
    /// Specifies the radius in the y dimension of the ellipse which defines the radial gradient. 
    /// </summary>
    public double RadiusY { get; set; }

    /// <summary>
    /// Holds a sequence of <GradientStop> elements.
    /// </summary>
    public GradientStopCollection GradientStops { get; set; }
  }
}