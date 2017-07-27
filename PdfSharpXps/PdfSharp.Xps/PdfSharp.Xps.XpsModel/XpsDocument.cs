using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Packaging;
using System.Text;
using System.Xml;
using PdfSharp.Xps.Parsing;

namespace PdfSharp.Xps.XpsModel
{
  /// <summary>
  /// Represents an XpsDocument that can be converted to PDF by PDFsharp.
  /// </summary>
  public sealed class XpsDocument : IDisposable
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="XpsDocument"/> class from a stream.
    /// </summary>
    XpsDocument(Stream stream)
    {
      this.package = ZipPackage.Open(stream) as ZipPackage;
      Initialize();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XpsDocument"/> class from a path.
    /// </summary>
    XpsDocument(string path)
    {
      // N. Sánchez. 2015-02-11: Modified to avoid crash due to other process accessing the file after creation (maybe the anti-virus).
      this.package = ZipPackage.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read) as ZipPackage;
      this.path = path;
      Initialize();
    }

    /// <summary>
    /// Opens an XPS document from the specifed stream.
    /// </summary>
    public static XpsDocument Open(Stream stream)
    {
      return new XpsDocument(stream);
    }

    /// <summary>
    /// Opens an XPS document from the specifed path.
    /// </summary>
    public static XpsDocument Open(string path)
    {
      return new XpsDocument(path);
    }

    /// <summary>
    /// Initializes the primary fixed payload.
    /// </summary>
    void Initialize()
    {
      this.parts = this.package.GetParts();
      // Assume only one fixed payload 
      this.fpayload = new FixedPayload(this);
    }

    void IDisposable.Dispose()
    {
      if (!this.disposed)
      {
        try
        {
          this.parts = null;
          this.fpayload = null;
          if (this.package != null)
          {
            this.package.Close();
            this.package = null;
          }
        }
        finally
        {
          this.disposed = true;
        }
        GC.SuppressFinalize(this);
      }
    }
    bool disposed;

    /// <summary>
    /// Closes this instance and the underlying ZIP package.
    /// </summary>
    public void Close()
    {
      ((IDisposable)this).Dispose();
    }

    internal string Path
    {
      get { return this.path; }
    }
    string path;

    /// <summary>
    /// Gets the number of fixed documents.
    /// </summary>
    public int DocumentCount
    {
      get { return this.fpayload.DocumentCount; }
    }

    /// <summary>
    /// Gets a read-only collection of the fixed documents of this XPS document.
    /// </summary>
    public FixedDocumentCollection Documents
    {
      get
      {
        if (this.documents == null)
          this.documents = new FixedDocumentCollection(this.fpayload);
        return this.documents;
      }
    }
    FixedDocumentCollection documents;

    /// <summary>
    /// Gets the first document document from the fixed document sequence.
    /// </summary>
    public FixedDocument GetDocument()
    {
      return GetDocument(0);
    }

    /// <summary>
    /// Gets the document with the specified index.
    /// </summary>
    public FixedDocument GetDocument(int index)
    {
      return this.fpayload.GetDocument(index);
    }

    internal XmlTextReader GetPartAsXmlReader(string uri)
    {
      return GetPartAsXmlReader(this.package, uri);
    }

    internal static XmlTextReader GetPartAsXmlReader(ZipPackage package, string uriString)
    {
      // HACK: just make it work...
      if (!uriString.StartsWith("/"))
        uriString = "/" + uriString;

      // Documents with relative uri exists.
      if (uriString.StartsWith("/.."))
        uriString = uriString.Substring(3);

      ZipPackagePart part = package.GetPart(new Uri(uriString, UriKind.Relative)) as ZipPackagePart;
      string xml = String.Empty;
      using (Stream stream = part.GetStream())
      {
        using (StreamReader sr = new StreamReader(stream))
        {
          xml = sr.ReadToEnd();
        }
      }
      XmlTextReader reader = new XmlTextReader(new StringReader(xml));
      return reader;
    }

    internal byte[] GetPartAsBytes(string uriString)
    {
      return GetPartAsBytes(this.package, uriString);
    }

    internal static byte[] GetPartAsBytes(ZipPackage package, string uriString)
    {
      Uri target = new Uri(uriString, UriKind.Relative);
#if true
      if (!uriString.StartsWith("/"))
      {
        //target = PackUriHelper.ResolvePartUri(package.t.GetRelationship Uri("/documents2/3/Pages", UriKind.RelativeOrAbsolute), target);
        //PackagePartCollection coll = package.GetParts();
        // HACK: introduce a "CurrentPart"
        target = PackUriHelper.ResolvePartUri(new Uri("/documents/1/Pages/1.page", UriKind.RelativeOrAbsolute), target);
      }
#else
      // HACK: just make it go...
      if (!uriString.StartsWith("/"))
        uriString = "/" + uriString;

      // Documents with relative uri exists.
      if (uriString.StartsWith("/.."))
        uriString = uriString.Substring(3);
#endif
      ZipPackagePart part = package.GetPart(target) as ZipPackagePart;

      byte[] bytes = null;
      using (Stream stream = part.GetStream())
      {
        int length = (int)stream.Length;
        bytes = new byte[length];
        stream.Read(bytes, 0, length);
      }
      return bytes;
    }

    /// <summary>
    /// Gets the underlying ZIP package.
    /// </summary>
    internal ZipPackage Package
    {
      get { return this.package; }
    }
    ZipPackage package;

    /// <summary>
    /// Gets the underlying ZIP package parts collection.
    /// </summary>
    internal PackagePartCollection Parts
    {
      get { return this.parts; }
    }
    PackagePartCollection parts;

    FixedPayload fpayload;
  }
}