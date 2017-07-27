using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.IO;
using System.IO.Packaging;
using System.Xml;
using PdfSharp.Drawing;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Xps.Parsing;
using PdfSharp.Xps.Rendering;
using System.Windows.Media.Imaging;
using IOPath = System.IO.Path;

namespace PdfSharp.Xps.XpsModel
{
  /// <summary>
  /// The container of fixed documents.
  /// The current implementation assumes only one fixed payload per XPS document.
  /// </summary>
  class FixedPayload
  {
    public FixedPayload(XpsDocument document)
    {
      this.resMngr = new ResourceManager(this);
      this.document = document;

      // HACK: find fdseq
      string fdseqString = null;
      foreach (ZipPackagePart part in XpsDocument.Parts)
      {
        //string uriString = part.Uri.OriginalString;
        //if (uriString.EndsWith("fdseq", true, CultureInfo.InvariantCulture))

        //File extension for fdseq file can be different.
        //[Content_Types].xml contains the informations needed and already pared by ZipPackage.
        if (part.ContentType.Contains("application/vnd.ms-package.xps-fixeddocumentsequence+xml"))
        {
          fdseqString = part.Uri.OriginalString;
          break;
        }
      }
      Debug.Assert(fdseqString != null);

      this.fdseq = XpsParser.Parse(GetPartAsXmlReader(fdseqString)) as FixedDocumentSequence;
      this.fdocs = new FixedDocument[this.fdseq.DocumentReferences.Count];
    }

    /// <summary>
    /// Gets the XPS document that owns this payload.
    /// </summary>
    public XpsDocument XpsDocument
    {
      get { return this.document; }
    }
    XpsDocument document;

    /// <summary>
    /// Gets the number of fixed documents in this payload.
    /// </summary>
    public int DocumentCount
    {
      get { return this.fdseq.DocumentReferences.Count; }
    }

    /// <summary>
    /// Gets the document with the specified index.
    /// </summary>
    public FixedDocument GetDocument(int index)
    {
      if (index < 0 || index > this.fdocs.Length - 1)
        throw new ArgumentOutOfRangeException("index");
      FixedDocument fdoc = this.fdocs[index];
      if (fdoc == null)
      {
        string source = this.fdseq.DocumentReferences[index].Source;
        fdoc = Parsing.XpsParser.Parse(GetPartAsXmlReader(source)) as FixedDocument;
        fdoc.Payload = this;
        source = IOPath.GetDirectoryName(source);
        source = source.Replace('\\', '/');
        if (!source.StartsWith("/"))
          source = "/" + source;
        fdoc.UriString = source;
        this.fdocs[index] = fdoc;
      }
      return fdoc;
    }

    /// <summary>
    /// Gets the underlying ZIP package.
    /// </summary>
    public ZipPackage Package
    {
      get { return this.document.Package; }
    }

    /// <summary>
    /// Gets the font data from the resource package part.
    /// </summary>
    public byte[] GetFontData(string uriString)
    {
      //ZipPackagePart part = Package.GetPart(new Uri(uriString, UriKind.Relative)) as ZipPackagePart;
      byte[] fontData = GetPartAsBytes(uriString);

      // HACK: Is checking for odttf enough?
      if (uriString.EndsWith("odttf", true, CultureInfo.InvariantCulture))
      {
        Guid guid = new Guid(IOPath.GetFileNameWithoutExtension(uriString));
        byte[] bytes = guid.ToByteArray();
        // See XPS 1.0 section 2.1.7.3 'Embedded Font Obfuscation'
        for (int idx = 0; idx < 32; idx++)
          fontData[idx] ^= bytes[odidx[idx]];
      }
      return fontData;
    }
    static int[] odidx = new int[] { 15, 14, 13, 12, 11, 10, 09, 08, 06, 07, 04, 05, 00, 01, 02, 03, 15, 14, 13, 12, 11, 10, 09, 08, 06, 07, 04, 05, 00, 01, 02, 03 };

#if deobfuscate_myself
    void DeObfuscateFontProgram(string guidString, byte[] fontData)
    {
      //51013C90-4D4C-4045-C231-3F3858250F68
      //55eb0f35-817d-486f-982e-603e2beb9d9a

      Guid guid = new Guid(guidString);
      byte[] bytes0 = guid.ToByteArray();
      byte[] bytes = new byte[16];
      bytes0.CopyTo(bytes, 0);
      //bytes[00] = bytes0[03];
      //bytes[01] = bytes0[02];
      //bytes[02] = bytes0[01];
      //bytes[03] = bytes0[00];
      //bytes[04] = bytes0[05];
      //bytes[05] = bytes0[04];
      //bytes[06] = bytes0[07];
      //bytes[07] = bytes0[06];


      // return bytes;

      // The 16 bytes of the 128-bit GUID are referred to in the following text by the placeholder names 
      //   0     1     2     3      4     5     6     7      8     9     10    11   12    13   14        15
      // B00, B01, B02, B03; B10, B11; B20, B21; B30, B31, B32, B33, B34, B35, B36, and B37. <-- Names
      //  
      // “B03B02B01B00-B11B10-B21B20-B30B31-B32B33B34B35B36B37” // The last segment of the part name MUST be of the form
      // 
      //   0    1    2    3     4    5    6    7    8    9    10   11  12  13   14   15
      // B03 B02 B01 B00 B11 B10 B21 B20 B30 B31 B32 B33 B34 B35 B36 B37  <-- uuid
      //
      // B37, B36, B35, B34, B33, B32, B31, B30, B20, B21, B10, B11, B00, B01, B02, and B03  <-- XOR
#if true
      fontData[00] ^= bytes[15]; // B37
      fontData[01] ^= bytes[14]; // B36
      fontData[02] ^= bytes[13]; // B35
      fontData[03] ^= bytes[12]; // B34
      fontData[04] ^= bytes[11]; // B33
      fontData[05] ^= bytes[10]; // B32
      fontData[06] ^= bytes[09]; // B31
      fontData[07] ^= bytes[08]; // B30
      fontData[08] ^= bytes[06]; // B20
      fontData[09] ^= bytes[07]; // B21
      fontData[10] ^= bytes[04]; // B10
      fontData[11] ^= bytes[05]; // B11
      fontData[12] ^= bytes[00]; // B00
      fontData[13] ^= bytes[01]; // B01
      fontData[14] ^= bytes[02]; // B02
      fontData[15] ^= bytes[03]; // B03
      fontData[16] ^= bytes[15]; // B37
      fontData[17] ^= bytes[14]; // B36
      fontData[18] ^= bytes[13]; // B35
      fontData[19] ^= bytes[12]; // B34
      fontData[20] ^= bytes[11]; // B33
      fontData[21] ^= bytes[10]; // B32
      fontData[22] ^= bytes[09]; // B31
      fontData[23] ^= bytes[08]; // B30
      fontData[24] ^= bytes[06]; // B20
      fontData[25] ^= bytes[07]; // B21
      fontData[26] ^= bytes[04]; // B10
      fontData[27] ^= bytes[05]; // B11
      fontData[28] ^= bytes[00]; // B00
      fontData[29] ^= bytes[01]; // B01
      fontData[30] ^= bytes[02]; // B02
      fontData[31] ^= bytes[03]; // B03
#else
      fontData[00] ^= bytes[15]; // B37
      fontData[01] ^= bytes[14]; // B36
      fontData[02] ^= bytes[13]; // B35
      fontData[03] ^= bytes[12]; // B34
      fontData[04] ^= bytes[11]; // B33
      fontData[05] ^= bytes[10]; // B32
      fontData[06] ^= bytes[09]; // B31
      fontData[07] ^= bytes[08]; // B30
      fontData[08] ^= bytes[07]; // B20
      fontData[09] ^= bytes[06]; // B21
      fontData[10] ^= bytes[05]; // B10
      fontData[11] ^= bytes[04]; // B11
      fontData[12] ^= bytes[03]; // B00
      fontData[13] ^= bytes[02]; // B01
      fontData[14] ^= bytes[01]; // B02
      fontData[15] ^= bytes[00]; // B03
      fontData[16] ^= bytes[15]; // B37
      fontData[17] ^= bytes[14]; // B36
      fontData[18] ^= bytes[13]; // B35
      fontData[19] ^= bytes[12]; // B34
      fontData[20] ^= bytes[11]; // B33
      fontData[21] ^= bytes[10]; // B32
      fontData[22] ^= bytes[09]; // B31
      fontData[23] ^= bytes[08]; // B30
      fontData[24] ^= bytes[07]; // B20
      fontData[25] ^= bytes[06]; // B21
      fontData[26] ^= bytes[05]; // B10
      fontData[27] ^= bytes[04]; // B11
      fontData[28] ^= bytes[03]; // B00
      fontData[29] ^= bytes[02]; // B01
      fontData[30] ^= bytes[01]; // B02
      fontData[31] ^= bytes[00]; // B03
#endif
    }
#endif

    /// <summary>
    /// Gets the bitmap image data from the resource package part.
    /// </summary>
    public byte[] GetImageData(string uriString)
    {
      byte[] imageData = GetPartAsBytes(uriString);
      return imageData;
    }

    XmlTextReader GetPartAsXmlReader(string uriString)
    {
      return this.document.GetPartAsXmlReader(uriString);
    }

    byte[] GetPartAsBytes(string uriString)
    {
      return this.document.GetPartAsBytes(uriString);
    }

    /// <summary>
    /// Root of all documents in this payload.
    /// </summary>
    FixedDocumentSequence fdseq;
    FixedDocument[] fdocs;

    //public string GetFontName(string uriString, PdfContentWriter writer, out PdfFont pdfFont)
    //{
    //  string baseName = IOPath.GetFileNameWithoutExtension(uriString);

    //  string fontName = writer.TryGetFontName(baseName, out pdfFont);
    //  if (fontName == null)
    //  {
    //    byte[] fontData = GetFontData(uriString);
    //    fontName = writer.GetFontName(baseName, fontData, out pdfFont);
    //  }
    //  Debug.Assert(pdfFont != null);
    //  return fontName;
    //}

    public Font GetFont(string uriString)
    {
      return this.resMngr.GetFont(uriString);
    }

    public BitmapSource GetImage(string uriString)
    {
      return this.resMngr.GetImage(uriString);
    }
    ResourceManager resMngr;

    internal class ResourceManager
    {
      public ResourceManager(FixedPayload payload)
      {
        this.payload = payload;
      }
      FixedPayload payload;

      public Font GetFont(string uriString)
      {
        string baseName = IOPath.GetFileNameWithoutExtension(uriString);
        Font font;
        if (this.fonts.TryGetValue(baseName, out font))
          return font;

        byte[] fontData = this.payload.GetFontData(uriString);

        // Create helper name if font data does not contain a name table.
        string name = String.Format("XPS-Font-{0:00}", ++fontCount);
        name = PdfFont.CreateEmbeddedFontSubsetName(name);
        font = new Font(name, fontData);
        this.fonts.Add(baseName, font);
        return font;
      }
      Dictionary<string, Font> fonts = new Dictionary<string, Font>();
      static int fontCount = 0;

      public BitmapSource GetImage(string uriString)
      {
        string baseName = IOPath.GetFileNameWithoutExtension(uriString);

        byte[] imageData = this.payload.GetImageData(uriString);

        MemoryStream stream = new MemoryStream(imageData);
        BitmapDecoder decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.Default);

        BitmapFrame frame = decoder.Frames[0];


        //Font font;
        //if (this.fonts.TryGetValue(baseName, out font))
        //  return font;

        //BitmapImage bitmapImage = new BitmapImage();

        return frame;

        //// Create helper name if font data does not contain a name table.
        //string name = String.Format("XPS-Font-{0:00}", ++fontCount);
        //name = PdfFont.CreateEmbeddedFontSubsetName(name);
        //font = new Font(name, fontData);
        //this.fonts.Add(baseName, font);
        //return font;
      }
      //Dictionary<string, Font> fonts = new Dictionary<string, Font>();
      //static int fontCount = 0;
    }
  }
}