using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Packaging;
using IOPath = System.IO.Path;

namespace PdfSharp.Xps.XpsModel
{
  /// <summary>
  /// Binds an ordered sequence of fixed pages together into a single multi-page document.
  /// </summary>
  public class FixedDocument : XpsElement
  {
    /// <summary>
    /// Gets the number of fixed pages in this document.
    /// </summary>
    public int PageCount
    {
      get { return this.PageContentUriStrings.Count; }
    }

    /// <summary>
    /// Gets the XPS document that owns this document.
    /// </summary>
    public XpsDocument XpsDocument
    {
      get { return this.fpayload.XpsDocument; }
    }

    /// <summary>
    /// Gets the path to this document.
    /// </summary>
    public string UriString
    {
      get { return this.uriString; }
      internal set { this.uriString = value; }
    }
    string uriString;

    /// <summary>
    /// Gets the fixed payload that owns this document.
    /// </summary>
    internal FixedPayload Payload
    {
      get { return this.fpayload; }
      set { this.fpayload = value; }
    }
    FixedPayload fpayload;

    /// <summary>
    /// Gets the underlying ZIP package.
    /// </summary>
    internal ZipPackage Package
    {
      get { return Payload.Package; }
    }

    internal List<string> PageContentUriStrings = new List<string>();

    //internal string partUri;

    /// <summary>
    /// Gets a read-only collection of the fixed pages of this fixed documents .
    /// </summary>
    public FixedPageCollection Pages
    {
      get
      {
        if (this.pages == null)
          this.pages = new FixedPageCollection(this);
        return this.pages;
      }
    }
    FixedPageCollection pages;


    /// <summary>
    /// Gets the fixed page with the specified index.
    /// </summary>
    public FixedPage GetFixedPage(int index)
    {
      if (FixedPages == null)
        FixedPages = new FixedPage[PageContentUriStrings.Count];

      // Parse on demand
      FixedPage fpage = FixedPages[index];
      if (fpage == null)
      {
        string source = IOPath.Combine(UriString, PageContentUriStrings[index]);
        source = source.Replace('\\', '/');
        fpage = Parsing.XpsParser.Parse(XpsDocument.GetPartAsXmlReader(Package, source)) as FixedPage;
        if (fpage != null)
        {
          fpage.Parent = this;
          fpage.Document = this;
          source = IOPath.GetDirectoryName(source);
          source = source.Replace('\\', '/');
          fpage.UriString = source;
          fpage.LoadResources();
          FixedPages[index] = fpage;
        }
      }
      return fpage;
    }

    /// <summary>
    /// A collection of document references.
    /// </summary>
    //internal XpsElementCollection<PageContent> PageContents;
    FixedPage[] FixedPages;
  }
}