using System;
using System.Collections.Generic;
using System.Text;

namespace PdfSharp.Xps.XpsModel
{
  /// <summary>
  /// Fills a region with a drawing. The drawing may be specified as either a child of the <VisualBrush>
  /// element, or as a resource reference. Drawing content is expressed using <Canvas>, <Path>,
  /// and <Glyphs> elements. 
  /// </summary>
  class VisualBrush : Brush
  {
    public VisualBrush()
    {
      Opacity = 1;
    }

    /// <summary>
    /// Defines the uniform transparency of the brush fill. Values range from 0 (fully transparent)
    /// to 1 (fully opaque), inclusive. Values outside of this range are invalid.
    /// </summary>
    public double Opacity { get; set; }

    /// <summary>
    /// Specifies a name for a resource in a resource dictionary. x:Key MUST be present when the
    /// current element is defined in a resource dictionary. x:Key MUST NOT be specified outside of
    /// a resource dictionary [M6.4].
    /// </summary>
    // x:Key
    public string Key { get; set; }

    /// <summary>
    /// Describes the matrix transformation applied to the coordinate space of the brush.
    /// The Transform property is concatenated with the current effective render transform to yield
    /// an effective render transform local to the brush. The viewport for the brush is transformed
    /// using that local effective render transform. 
    /// </summary>
    public MatrixTransform Transform { get; set; }

    /// <summary>
    /// Specifies the position and dimensions of the brush's source content. Specifies four
    /// comma-separated real numbers (x, y, Width, Height), where width and height are
    /// non-negative. The dimensions specified are relative to the image’s physical dimensions
    /// expressed in units of 1/96". The corners of the viewbox are mapped to the corners of the
    /// viewport, thereby providing the default clipping and transform for the brush’s source content.
    /// </summary>
    public Rect Viewbox { get; set; }

    /// <summary>
    /// Specifies the region in the containing coordinate space of the prime brush tile that is
    /// (possibly repeatedly) applied to fill the region to which the brush is applied. Specifies four
    /// comma-separated real numbers (x, y, Width, Height), where width and height are non-negative.
    /// The alignment of the brush pattern is controlled by adjusting the x and y values.
    /// </summary>
    public Rect Viewport { get; set; }

    /// <summary>
    /// Specifies how contents will be tiled in the filled region. Valid values are None,
    /// Tile, FlipX, FlipY, and FlipXY. 
    /// </summary>
    public TileMode TileMode { get; set; }

    /// <summary>
    /// Specifies the relationship of the viewbox coordinates to the containing coordinate space. 
    /// </summary>
    public ViewUnits ViewboxUnits { get; set; }

    /// <summary>
    /// Specifies the relationship of the viewport coordinates to the containing coordinate space.
    /// </summary>
    public ViewUnits ViewportUnits { get; set; }

    /// <summary>
    /// Specifies resource reference to a <Path>, <Glyphs>, or <Canvas> element defined in a resource
    /// dictionary and used to draw the brush’s source content. 
    /// </summary>
    public Visual Visual { get; set; }
  }
}