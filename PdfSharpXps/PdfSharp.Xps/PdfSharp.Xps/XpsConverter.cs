using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Packaging;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PdfSharp.Xps.XpsModel;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Pdf;
using PdfSharp.Xps.Rendering;
using FixedDocument = PdfSharp.Xps.XpsModel.FixedDocument;
using FixedDocumentSequence = PdfSharp.Xps.XpsModel.FixedDocumentSequence;
using FixedPage = PdfSharp.Xps.XpsModel.FixedPage;
using IOPath = System.IO.Path;
using Point = PdfSharp.Xps.XpsModel.Point;
using Rect = PdfSharp.Xps.XpsModel.Rect;
using Size = PdfSharp.Xps.XpsModel.Size;

namespace PdfSharp.Xps
{
  /// <summary>
  /// Main class that provides the functionallity to convert an XPS file into a PDF file.
  /// </summary>
  public class XpsConverter
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="XpsConverter"/> class.
    /// </summary>
    /// <param name="pdfDocument">The PDF document.</param>
    /// <param name="xpsDocument">The XPS document.</param>
    public XpsConverter(PdfDocument pdfDocument, XpsDocument xpsDocument)
    {
      if (pdfDocument == null)
        throw new ArgumentNullException("pdfDocument");
      if (xpsDocument == null)
        throw new ArgumentNullException("xpsDocument");

      this.pdfDocument = pdfDocument;
      this.xpsDocument = xpsDocument;

      Initialize();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XpsConverter"/> class.
    /// </summary>
    /// <param name="pdfDocument">The PDF document.</param>
    /// <param name="xpsDocumentPath">The XPS document path.</param>
    public XpsConverter(PdfDocument pdfDocument, string xpsDocumentPath)  // TODO: a constructor with an Uri
    {
      if (pdfDocument == null)
        throw new ArgumentNullException("pdfDocument");
      if (String.IsNullOrEmpty(xpsDocumentPath))
        throw new ArgumentNullException("xpsDocumentPath");

      this.pdfDocument = pdfDocument;
      this.xpsDocument = XpsDocument.Open(xpsDocumentPath);

      Initialize();
    }

    void Initialize()
    {
      this.context = new DocumentRenderingContext(this.pdfDocument);
    }

    DocumentRenderingContext Context
    {
      get { return this.context; }
    }
    DocumentRenderingContext context;

    /// <summary>
    /// HACK
    /// </summary>
    public PdfPage CreatePage(int xpsPageIndex)
    {
      FixedPage fixedPage = this.xpsDocument.GetDocument().GetFixedPage(xpsPageIndex);

      PdfPage page = this.pdfDocument.AddPage();
      page.Width = XUnit.FromPresentation(fixedPage.Width);
      page.Height = XUnit.FromPresentation(fixedPage.Height);
      return page;
    }

    /// <summary>
    /// Renders an XPS document page to the specified PDF page.
    /// </summary>
    /// <param name="page">The target PDF page. The page must belong to the PDF document of this converter.</param>
    /// <param name="xpsPageIndex">The zero-based XPS page number.</param>
    public void RenderPage(PdfPage page, int xpsPageIndex)
    {
      if (page == null)
        throw new ArgumentNullException("page");
      if (!ReferenceEquals(page.Owner, this.pdfDocument))
        throw new InvalidOperationException(PSXSR.PageMustBelongToPdfDocument);
      // Debug.Assert(xpsPageIndex==0, "xpsPageIndex must be 0 at this stage of implementation.");
      try
      {
        FixedPage fpage = this.xpsDocument.GetDocument().GetFixedPage(xpsPageIndex);

        // ZipPackage pack = ZipPackage.Open(xpsFilename) as ZipPackage;
        Uri uri = new Uri("/Documents/1/Pages/1.fpage", UriKind.Relative);
        ZipPackagePart part = this.xpsDocument.Package.GetPart(uri) as ZipPackagePart;
        if (part != null)
        {
          using (Stream stream = part.GetStream())
          using (StreamReader sr = new StreamReader(stream))
          {
            string xml = sr.ReadToEnd();
#if true && DEBUG
            if (!String.IsNullOrEmpty(this.xpsDocument.Path))
            {
              string xmlPath =
                IOPath.Combine(IOPath.GetDirectoryName(this.xpsDocument.Path),
                               IOPath.GetFileNameWithoutExtension(this.xpsDocument.Path)) + ".xml";
              using (StreamWriter sw = new StreamWriter(xmlPath))
              {
                sw.Write(xml);
              }
            }
#endif
            //XpsElement el = PdfSharp.Xps.Parsing.XpsParser.Parse(xml);
            PdfRenderer renderer = new PdfRenderer();
            renderer.RenderPage(page, fpage);
          }
        }
      }
      catch (Exception ex)
      {
        Debug.WriteLine(ex.ToString());
        throw;
      }
    }

    /// <summary>
    /// Gets the PDF document of this converter.
    /// </summary>
    public PdfDocument PdfDocument
    {
      get { return this.pdfDocument; }
    }
    PdfDocument pdfDocument;

    /// <summary>
    /// Gets the XPS document of this converter.
    /// </summary>
    public XpsDocument XpsDocument
    {
      get { return this.xpsDocument; }
    }
    XpsDocument xpsDocument;

    /// <summary>
    /// Converts the specified PDF file into an XPS file. The new file is stored in the same directory.
    /// </summary>
    public static void Convert(string xpsFilename)
    {
      if (String.IsNullOrEmpty(xpsFilename))
        throw new ArgumentNullException("xpsFilename");

      if (!File.Exists(xpsFilename))
        throw new FileNotFoundException("File not found.", xpsFilename);

      string pdfFilename = xpsFilename;
      if (IOPath.HasExtension(pdfFilename))
        pdfFilename = pdfFilename.Substring(0, pdfFilename.LastIndexOf('.'));
      pdfFilename += ".pdf";

      Convert(xpsFilename, pdfFilename, 0);
    }

    /// <summary>
    /// Implements the PDF file to XPS file conversion.
    /// </summary>
    public static void Convert(string xpsFilename, string pdfFilename, int docInde)
    {
      Convert(xpsFilename, pdfFilename, docInde, false);
    }

    /// <summary>
    /// Implements the PDF file to XPS file conversion.
    /// </summary>
    public static void Convert(XpsDocument xpsDocument, string pdfFilename, int docIndex)
    {

        if (xpsDocument == null)
            throw new ArgumentNullException("xpsDocument");

        if (String.IsNullOrEmpty(pdfFilename))
            throw new ArgumentNullException("pdfFilename");

        FixedDocument fixedDocument = xpsDocument.GetDocument();
        PdfDocument pdfDocument = new PdfDocument();
        PdfRenderer renderer = new PdfRenderer();

        int pageIndex = 0;
        foreach (FixedPage page in fixedDocument.Pages)
        {
            if (page == null)
                continue;
            Debug.WriteLine(String.Format("  doc={0}, page={1}", docIndex, pageIndex));
            PdfPage pdfPage = renderer.CreatePage(pdfDocument, page);
            renderer.RenderPage(pdfPage, page);
            pageIndex++;

/* DEBUG constant must be active in order to avoid problem with partial pages.
    * See: http://www.nathanpjones.com/wp/2013/03/output-to-pdf-in-wpf-for-free/
    * So, the next limitation is no longer possible...
#if DEBUG
            // stop at page...
            if (pageIndex == 50)
                break;
#endif */
        }
        pdfDocument.Save(pdfFilename);

    }

    /// <summary>
    /// Implements the PDF file to XPS file conversion.
    /// </summary>
    public static void Convert(string xpsFilename, string pdfFilename, int docIndex, bool createComparisonDocument)
    {
      if (String.IsNullOrEmpty(xpsFilename))
        throw new ArgumentNullException("xpsFilename");

      if (String.IsNullOrEmpty(pdfFilename))
      {
        pdfFilename = xpsFilename;
        if (IOPath.HasExtension(pdfFilename))
          pdfFilename = pdfFilename.Substring(0, pdfFilename.LastIndexOf('.'));
        pdfFilename += ".pdf";
      }

      XpsDocument xpsDocument = null;
      try
      {
        xpsDocument = XpsDocument.Open(xpsFilename);
        FixedDocument fixedDocument = xpsDocument.GetDocument();
        PdfDocument pdfDocument = new PdfDocument();
        PdfRenderer renderer = new PdfRenderer();

        int pageIndex = 0;
        foreach (FixedPage page in fixedDocument.Pages)
        {
          if (page == null)
            continue;
          Debug.WriteLine(String.Format("  doc={0}, page={1}", docIndex, pageIndex));
          PdfPage pdfPage = renderer.CreatePage(pdfDocument, page);
          renderer.RenderPage(pdfPage, page);
          pageIndex++;

/* DEBUG constant must be active in order to avoid problem with partial pages.
    * See: http://www.nathanpjones.com/wp/2013/03/output-to-pdf-in-wpf-for-free/
    * So, the next limitation is no longer possible...
#if DEBUG
          // stop at page...
          if (pageIndex == 50)
            break;
#endif */
        }
        pdfDocument.Save(pdfFilename);
        xpsDocument.Close();
        xpsDocument = null;

        if (createComparisonDocument)
        {
          System.Windows.Xps.Packaging.XpsDocument xpsDoc = new System.Windows.Xps.Packaging.XpsDocument(xpsFilename, FileAccess.Read);
          System.Windows.Documents.FixedDocumentSequence docSeq = xpsDoc.GetFixedDocumentSequence();
          if (docSeq == null)
            throw new InvalidOperationException("docSeq");

          XPdfForm form = XPdfForm.FromFile(pdfFilename);
          PdfDocument pdfComparisonDocument = new PdfDocument();


          pageIndex = 0;
          foreach (PdfPage page in pdfDocument.Pages)
          {
            if (page == null)
              continue;
            Debug.WriteLine(String.Format("  doc={0}, page={1}", docIndex, pageIndex));

            PdfPage pdfPage = /*renderer.CreatePage(pdfComparisonDocument, page);*/pdfComparisonDocument.AddPage();
            double width = page.Width;
            double height = page.Height;
            pdfPage.Width = page.Width * 2;
            pdfPage.Height = page.Height;


            DocumentPage docPage = docSeq.DocumentPaginator.GetPage(pageIndex);
            //byte[] png = PngFromPage(docPage, 96);

            BitmapSource bmsource = BitmapSourceFromPage(docPage, 96 * 2);
            XImage image = XImage.FromBitmapSource(bmsource);

            XGraphics gfx = XGraphics.FromPdfPage(pdfPage);
            form.PageIndex = pageIndex;
            gfx.DrawImage(form, 0, 0, width, height);
            gfx.DrawImage(image, width, 0, width, height);

            //renderer.RenderPage(pdfPage, page);
            pageIndex++;

/* DEBUG constant must be active in order to avoid problem with partial pages.
    * See: http://www.nathanpjones.com/wp/2013/03/output-to-pdf-in-wpf-for-free/
    * So, the next limitation is no longer possible...
#if DEBUG
            // stop at page...
            if (pageIndex == 50)
              break;
#endif */
          }

          string pdfComparisonFilename = pdfFilename;
          if (IOPath.HasExtension(pdfComparisonFilename))
            pdfComparisonFilename = pdfComparisonFilename.Substring(0, pdfComparisonFilename.LastIndexOf('.'));
          pdfComparisonFilename += "-comparison.pdf";

          pdfComparisonDocument.ViewerPreferences.FitWindow = true;
          //pdfComparisonDocument.PageMode = PdfPageMode.
          pdfComparisonDocument.PageLayout = PdfPageLayout.SinglePage;
          pdfComparisonDocument.Save(pdfComparisonFilename);
        }

      }
      catch (Exception ex)
      {
        Debug.WriteLine(ex.Message);
        if (xpsDocument != null)
          xpsDocument.Close();
        throw;
      }
      finally
      {
        if (xpsDocument != null)
          xpsDocument.Close();
      }
    }


    static public void SaveXpsPageToBitmap(string xpsFileName)
    {
      System.Windows.Xps.Packaging.XpsDocument xpsDoc = new System.Windows.Xps.Packaging.XpsDocument(xpsFileName, System.IO.FileAccess.Read);
      System.Windows.Documents.FixedDocumentSequence docSeq = xpsDoc.GetFixedDocumentSequence();

      // You can get the total page count from docSeq.PageCount    
      for (int pageNum = 0; pageNum < docSeq.DocumentPaginator.PageCount; ++pageNum)
      {
        DocumentPage docPage = docSeq.DocumentPaginator.GetPage(pageNum);
        RenderTargetBitmap renderTarget =
            new RenderTargetBitmap((int)docPage.Size.Width,
                                    (int)docPage.Size.Height,
                                    96, // WPF (Avalon) units are 96dpi based    
                                    96,
                                    System.Windows.Media.PixelFormats.Default);

        renderTarget.Render(docPage.Visual);

        BitmapEncoder encoder = new BmpBitmapEncoder();  // Choose type here ie: JpegBitmapEncoder, etc   
        encoder.Frames.Add(BitmapFrame.Create(renderTarget));

        FileStream pageOutStream = new FileStream(xpsFileName + ".Page" + pageNum + ".bmp", FileMode.Create, FileAccess.Write);
        encoder.Save(pageOutStream);
        pageOutStream.Close();
      }
    }

    static public BitmapSource BitmapSourceFromPage(DocumentPage docPage, double resolution)
    {
      double pixelWidth = docPage.Size.Width * resolution / 96;
      double pixelHeight = docPage.Size.Height * resolution / 96;
      RenderTargetBitmap renderTarget = new RenderTargetBitmap((int)pixelWidth, (int)pixelHeight, resolution, resolution, PixelFormats.Default);
      renderTarget.Render(docPage.Visual);

      return renderTarget;

      //PngBitmapEncoder encoder = new PngBitmapEncoder();  // Choose type here ie: JpegBitmapEncoder, etc   
      //encoder.Frames.Add(BitmapFrame.Create(renderTarget));

      //BitmapSource.Create(pageWidth, pageHeight, resolution, resolution, PixelFormats.)

      //return encoder.Preview;
      //encoder.
      //BitmapSource s = Xps;
      ////FileStream pageOutStream = new FileStream(xpsFileName + ".Page" + pageNum + ".bmp", FileMode.Create, FileAccess.Write);
      //MemoryStream memStream = new MemoryStream();
      //encoder.Save(memStream);
      //return memStream.ToArray();
    }

    //byte[] void PngFromPage(FixedDocument fixedDocument, int pageIndex, double resolution)
    //{
    //  if (fixedDocument==null)
    //    throw new ArgumentNullException("fixedDocument");
    //  if ( pageIndex<0|| pageIndex>= fixedDocument.PageCount)
    //    throw new ArgumentOutOfRangeException("pageIndex");

    //  FixedPage page = fixedDocument.Pages[pageIndex];
    //  double pageWidth = page.Width;
    //  double pageHeight= page.Height;

    //  // Create an appropirate render bitmap
    //  const int factor = 3;
    //  int width = (int)(WidthInPoint * factor);
    //  int height = (int)(HeightInPoint * factor);
    //  this.image = new RenderTargetBitmap(width, height, 72 * factor, 72 * factor, PixelFormats.Default);
    //  if (visual is UIElement)
    //  {
    //    // Perform layout on UIElement - otherwise nothing gets rendered
    //    UIElement element = visual as UIElement;
    //    Size size = new Size(WidthInPU, HeightInPU);
    //    element.Measure(size);
    //    element.Arrange(new Rect(new Point(), size));
    //    element.UpdateLayout();
    //  }
    //  this.image.Render(visual);

    //  // Save image as PNG
    //  FileStream stream = new FileStream(Path.Combine(OutputDirectory, Name + ".png"), FileMode.Create);
    //  PngBitmapEncoder encoder = new PngBitmapEncoder();
    //  //string author = encoder.CodecInfo.Author.ToString();
    //  encoder.Frames.Add(BitmapFrame.Create(this.image));
    //  encoder.Save(stream);
    //  stream.Close();
    //}
  }
}