using System;
using System.Collections.Generic;
using System.Text;

namespace PdfSharp.Xps.XpsModel
{
  /// <summary>
  /// Contains markup that describes the rendering of a single page of content.
  /// </summary>
  public class FixedPage : XpsElement
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="FixedPage"/> class.
    /// </summary>
    public FixedPage()
    {
      Name = String.Empty;
    }

    /// <summary>
    /// Gets the path to this page.
    /// </summary>
    public string UriString
    {
      get { return this.uriString; }
      internal set { this.uriString = value; }
    }
    string uriString;

    /// <summary>
    /// Gets or sets the owning document.
    /// </summary>
    public FixedDocument Document
    {
      get { return this.document; }
      internal set { this.document = value; }
    }
    FixedDocument document;

    public void LoadResources()
    {
      //if (Resources != null)
      //  Resources.LoadResources();
    }

    /// <summary>
    /// Contains a string value that identifies the current element as a named, addressable
    /// point in the document for the purpose of hyperlinking. 
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Width of the page, expressed as a real number in units of the effective coordinate space.
    /// </summary>
    public double Width { get; set; }

    /// <summary>
    /// Gets the width of the page in point (1/72 of an inch).
    /// </summary>
    public double PointWidth 
    {
      get { return Width * 3 / 4; }
    }

    /// <summary>
    /// Height of the page, expressed as a real number in units of the effective coordinate space.
    /// </summary>
    public double Height { get; set; }

    /// <summary>
    /// Gets the height of the page in point (1/72 of an inch).
    /// </summary>
    public double PointHeight
    {
      get { return Height * 3 / 4; }
    }

    /// <summary>
    /// Specifies the area of the page containing imageable content that is to be fit within the 
    /// imageable area when printing or viewing. Contains a list of four coordinate values
    /// (ContentOriginX, ContentOriginY, ContentWidth, ContentHeight), expressed as comma-separated
    /// real numbers. Specifying a value is RECOMMENDED [S3.1]. If omitted, the default value
    /// is (0,0,Width,Height). 
    /// </summary>
    public Rect ContentBox { get; set; }

    /// <summary>
    /// Specifies the area including crop marks that extends outside of the physical page.
    /// Contains a list of four coordinate values (BleedOriginX, BleedOriginY, BleedWidth, BleedHeight),
    /// expressed as comma-separated real numbers. If omitted, the default value is (0,0,Width,Height). 
    /// </summary>
    public Rect BleedBox { get; set; }

    /// <summary>
    /// A collection of Path, Glyphs, and Canvas objects.
    /// </summary>
    internal XpsElementCollection Content = new XpsElementCollection();

    /// <summary>
    /// Specifies the default language used for the current element and for any child or descendant
    /// elements. The language is specified according to RFC 3066. 
    /// </summary>
    public string Lang { get; set; } //xml:lang

    /// <summary>
    /// Contains the resource dictionary for the <FixedPage> element.
    /// </summary>
    internal ResourceDictionary Resources { get; set; }
  }
}