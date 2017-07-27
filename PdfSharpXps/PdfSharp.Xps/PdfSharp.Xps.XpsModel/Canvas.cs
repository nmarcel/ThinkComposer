using System;
using System.Collections.Generic;
using System.Text;

namespace PdfSharp.Xps.XpsModel
{
  /// <summary>
  /// Groups <FixedPage> descendant elements together.
  /// </summary>
  class Canvas : XpsElement
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="Canvas"/> class.
    /// </summary>
    public Canvas()
    {
      Name = String.Empty;
      Opacity = 1;
    }

    /// <summary>
    /// Establishes a new coordinate frame for the child and descendant elements of the canvas,
    /// such as another canvas. Also affects clip and opacity mask. 
    /// </summary>
    public MatrixTransform RenderTransform { get; set; }

    /// <summary>
    /// Limits the rendered region of the element. 
    /// </summary>
    public PathGeometry Clip { get; set; }

    /// <summary>
    /// Defines the uniform transparency of the canvas. Values range from 0 (fully transparent) to
    /// 1 (fully opaque), inclusive. Values outside of this range are invalid. 
    /// </summary>
    public double Opacity { get; set; }

    /// <summary>
    /// Specifies a mask of alpha values that is applied to the canvas in the same fashion as the
    /// Opacity attribute, but allowing different alpha values for different areas of the element. 
    /// </summary>
    public Brush OpacityMask { get; set; }

    /// <summary>
    /// Contains a string value that identifies the current element as a named, addressable point
    /// in the document for the purpose of hyperlinking. 
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Controls how edges of paths within the canvas are rendered. The only valid value is Aliased.
    /// Omitting this attribute causes the edges to be rendered in the consumer's default manner. 
    /// </summary>
    public string RenderOptions_EdgeMode { get; set; }

    /// <summary>
    /// Associates a hyperlink URI with the element. May be a relative reference or a URI that
    /// addresses a resource that is internal to or external to the package. 
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
    /// A brief description of the <Canvas> contents for accessibility purposes, particularly if filled
    /// with a set of vector graphics and text elements intended to comprise a single vector graphic. 
    /// </summary>
    public string AutomationProperties_Name { get; set; }

    /// <summary>
    /// A detailed description of the <Canvas> contents for accessibility purposes, particularly if filled
    /// with a set of graphics and text elements intended to comprise a single vector graphic. 
    /// </summary>
    public string AutomationProperties_HelpText { get; set; }

    /// <summary>
    /// Contains the resource dictionary for the <Canvas> element. 
    /// </summary>
    public ResourceDictionary Resources { get; set; }

    /// <summary>
    /// A collection of Path, Glyphs, and Canvas objects.
    /// </summary>
    public XpsElementCollection Content = new XpsElementCollection();
  }
}