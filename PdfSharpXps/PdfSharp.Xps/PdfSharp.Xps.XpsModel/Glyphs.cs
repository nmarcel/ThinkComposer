using System;
using System.Collections.Generic;
using System.Text;

namespace PdfSharp.Xps.XpsModel
{
  /// <summary>
  /// Represents a run of text from a single font.
  /// </summary>
  class Glyphs : XpsElement
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="Glyphs"/> class.
    /// </summary>
    public Glyphs()
    {
      Name = String.Empty;
      Opacity = 1;
    }

    /// <summary>
    /// Specifies the Unicode algorithm bidirectional nesting level. Even values imply left-to-right layout,
    /// odd values imply right-to-left layout. Right-to-left layout places the run origin at the right side
    /// of the first glyph, with positive advance widths (representing advances to the left) placing
    /// subsequent glyphs to the left of the previous glyph. Valid values range from 0 to 61, inclusive. 
    /// </summary>
    public int BidiLevel { get; set; }

    /// <summary>
    /// Identifies the positions within the sequence of Unicode characters at which a text-selection
    /// tool may place a text-editing caret. Potential caret-stop positions are identified by their indices
    /// into the UTF-16 code units represented by the UnicodeString attribute value. When this attribute
    /// is missing, the text in the UnicodeString attribute value MUST be interpreted as having a caret
    /// stop between every Unicode UTF-16 code unit and at the beginning and end of the text [M5.1].
    /// The value SHOULD indicate that the caret cannot stop in front of most combining marks or in
    /// front of the second UTF-16 code unit of UTF-16 surrogate pairs [S5.1].
    /// </summary>
    public string CaretStops { get; set; }

    /// <summary>
    /// Uniquely identifies a specific device font. The identifier is typically defined by a hardware vendor
    /// or font vendor. 
    /// </summary>
    public string DeviceFontName { get; set; }

    /// <summary>
    /// Describes the brush used to fill the shape of the rendered glyphs. 
    /// </summary>
    public Brush Fill { get; set; }

    /// <summary>
    /// Specifies the font size in drawing surface units, expressed as a float in units of the effective
    /// coordinate space. A value of 0 results in no visible text. 
    /// </summary>
    public double FontRenderingEmSize    { get; set; }

    /// <summary>
    /// The URI of the physical font from which all glyphs in the run are drawn. The URI MUST reference
    /// a font contained in the package [M2.1]. If the physical font referenced is a TrueType Collection
    /// (containing multiple font faces), the fragment portion of the URI is a 0-based index indicating
    /// which font face of the TrueType Collection should be used.
    /// </summary>
    public string FontUri { get; set; }

    /// <summary>
    /// Specifies the x coordinate of the first glyph in the run, in units of the effective coordinate space.
    /// The glyph is placed so that the leading edge of its advance vector and its baseline intersect with
    /// the point defined by the OriginX and OriginY attributes. 
    /// </summary>
    public double OriginX { get; set; }

    /// <summary>
    /// Specifies the y coordinate of the first glyph in the run, in units of the effective coordinate space.
    /// The glyph is placed so that the leading edge of its advance vector and its baseline intersect with
    /// the point defined by the OriginX and OriginY attributes. 
    /// </summary>
    public double OriginY { get; set; }

    /// <summary>
    /// Indicates that a glyph is turned on its side, with the origin being defined as the top center
    /// of the unturned glyph.
    /// </summary>
    public bool IsSideways { get; set; }

    /// <summary>
    /// Specifies a series of glyph indices and their attributes used for rendering the glyph run.
    /// If the UnicodeString attribute specifies an empty string (“” or “{}”) and the Indices attribute
    /// is not specified or is also empty, a consumer MUST generate an error [M5.2]. 
    /// </summary>
    public GlyphIndices Indices { get; set; }

    /// <summary>
    /// Contains the string of text rendered by the  <see cref="Glyphs"/> element. The text is specified as Unicode
    /// code points. 
    /// </summary>
    public string UnicodeString { get; set; }

    /// <summary>
    /// Specifies a style simulation. Valid values are None, ItalicSimulation, BoldSimulation,
    /// and BoldItalicSimulation. 
    /// </summary>
    public StyleSimulations StyleSimulations { get; set; }

    /// <summary>
    /// Establishes a new coordinate frame for the glyph run specified by the <Glyphs> element.
    /// The render transform affects clip, opacity mask, fill, x origin, y origin, the actual shape of
    /// individual glyphs, and the advance widths. The render transform also affects the font size
    /// and values specified in the Indices attribute. 
    /// </summary>
    public MatrixTransform RenderTransform { get; set; }

    /// <summary>
    /// Limits the rendered region of the element. Only portions of the <Glyphs> element that fall
    /// within the clip region (even partially clipped characters) produce marks on the page. 
    /// </summary>
    public PathGeometry Clip { get; set; }

    /// <summary>
    /// Defines the uniform transparency of the glyph element. Values range from 0 (fully transparent)
    /// to 1 (fully opaque), inclusive. Values outside of this range are invalid. 
    /// </summary>
    public double Opacity { get; set; }

    /// <summary>
    /// Specifies a mask of alpha values that is applied to the glyphs in the same fashion as the
    /// Opacity attribute, but allowing different alpha values for different areas of the element.
    /// </summary>
    public Brush OpacityMask { get; set; }

    /// <summary>
    /// Contains a string value that identifies the current element as a named, addressable point in
    /// the document for the purpose of hyperlinking. 
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Associates a hyperlink URI with the element. May be a relative reference or a URI that
    /// addresses a resource that is internal to or external to the package. 
    /// </summary>
    public string FixedPage_NavigateUri { get; set; }

    /// <summary>
    /// Specifies the default language used for the current element. The language is specified according
    /// to RFC 3066.
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
  }
}