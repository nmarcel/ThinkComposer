using System;
using System.Collections.Generic;
using System.Text;

namespace PdfSharp.Xps.XpsModel
{
  /// <summary>
  /// Defines a single graphical effect to be rendered to the page. It paints a geometry with a brush and draws a stroke around it.
  /// </summary>
  class Path : XpsElement
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="Path"/> class.
    /// </summary>
    public Path()
    {
      Name = String.Empty;
      Opacity = 1;
    }

    /// <summary>
    /// Describes the geometry of the path. 
    /// </summary>
    public PathGeometry Data { get; set; }

    /// <summary>
    /// Describes the brush used to paint the geometry specified by the Data property of the path. 
    /// </summary>
    public Brush Fill { get; set; }

    /// <summary>
    /// Establishes a new coordinate frame for all attributes of the path and for all child elements of 
    /// the path, such as the geometry defined by the <Path.Data> property element.
    /// </summary>
    public MatrixTransform RenderTransform { get; set; }

    /// <summary>
    /// Limits the rendered region of the element.
    /// </summary>
    public PathGeometry Clip { get; set; }

    /// <summary>
    /// Defines the uniform transparency of the path element. Values range from 0 (fully transparent) to 
    /// 1 (fully opaque), inclusive. Values outside of this range are invalid. 
    /// </summary>
    public double Opacity { get; set; }

    /// <summary>
    /// Specifies a mask of alpha values that is applied to the path in the same fashion as the Opacity
    /// attribute, but allowing different alpha values for different areas of 
    /// </summary>
    public Brush OpacityMask { get; set; }

    /// <summary>
    /// Specifes the brush used to draw the stroke.
    /// </summary>
    public Brush Stroke { get; set; }

    /// <summary>
    ///  Specifies the length of dashes and gaps of the outline stroke. These values are specified as 
    ///  multiples of the stroke thickness as a space-separated list with an even number of 
    ///  non-negative values. When a stroke is drawn, the dashes and gaps specified by these values
    ///  are repeated to cover the length of the stroke. If this attribute is omitted, the stroke is drawn
    ///  solid, without any gaps. 
    /// </summary>
    public string StrokeDashArray { get; set; }

    /// <summary>
    /// Specifies how the ends of each dash are drawn. Valid values are Flat, Round, Square, and Triangle. 
    /// </summary>
    public string StrokeDashCap { get; set; }

    /// <summary>
    /// Adjusts the start point for repeating the dash array pattern. If this value is omitted, the dash 
    /// array aligns with the origin of the stroke. Values are specified as multiples of the stroke thickness. 
    /// </summary>
    public double StrokeDashOffset { get; set; }

    /// <summary>
    /// Defines the shape of the end of the last dash in a stroke. Valid values are Flat, Square, Round,
    /// and Triangle. 
    /// </summary>
    public LineCap StrokeEndLineCap { get; set; }

    /// <summary>
    /// Defines the shape of the beginning of the first dash in a stroke. Valid values are Flat, Square,
    /// Round, and Triangle. 
    /// </summary>
    public LineCap StrokeStartLineCap { get; set; }

    /// <summary>
    /// Specifies how a stroke is drawn at a corner of a path. Valid values are Miter, Bevel, and Round.
    /// If Miter is selected, the value of StrokeMiterLimit is used in drawing the stroke. 
    /// </summary>
    public LineJoin StrokeLineJoin { get; set; }

    /// <summary>
    /// The ratio between the maximum miter length and half of the stroke thickness. This value is
    /// significant only if the StrokeLineJoin attribute specifies Miter. 
    /// </summary>
    public double StrokeMiterLimit { get; set; }

    /// <summary>
    /// Specifies the thickness of a stroke, in units of the effective coordinate space (includes the path's
    /// render transform). The stroke is drawn on top of the boundary of the geometry specified by the
    /// <Path> element’s Data property. Half of the StrokeThickness extends outside of the geometry
    /// specified by the Data property and the other half extends inside of the geometry. 
    /// </summary>
    public double StrokeThickness { get; set; }

    /// <summary>
    /// Contains a string value that identifies the current element as a named, addressable point in
    /// the document for the purpose of hyperlinking.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Associates a hyperlink URI with the element. Can be a relative reference or a URI that addresses
    /// a resource that is internal to or external to the package. 
    /// </summary>
    public string FixedPage_NavigateUri { get; set; }

    /// <summary>
    /// Specifies the default language used for the current element and for any child or descendant elements.
    /// The language is specified according to RFC 3066. 
    /// </summary>
    // xml:lang
    public string lang { get; set; }

    /// <summary>
    /// Specifies a name for a resource in a resource dictionary. x:Key MUST be present when the 
    /// current element is defined in a resource dictionary. x:Key MUST NOT be specified outside of 
    /// a resource dictionary [M5.3]. 
    /// </summary>
    // x:Key
    public string Key { get; set; }    

    /// <summary>
    /// A brief description of the <Path> for accessibility purposes, particularly if filled with an <ImageBrush>. 
    /// </summary>
    public string AutomationProperties_Name { get; set; }

    /// <summary>
    /// A detailed description of the <Path> for accessibility purposes, particularly if filled with an <ImageBrush>. 
    /// </summary>
    public string AutomationProperties_HelpText { get; set; }

    /// <summary>
    /// On Anti-aliasing consumers controls if control points snap to the nearest device pixels. Valid values are ‘false’ and ‘true’. Consumers MAY ignore this attribute [O4.1]. 
    /// </summary>
    public bool SnapsToDevicePixels{ get; set; }
  }
}