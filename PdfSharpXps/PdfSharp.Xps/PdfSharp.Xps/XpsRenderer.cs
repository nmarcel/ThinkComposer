using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Packaging;
using System.Xml;
using System.Windows.Xps;
using PdfSharp.Pdf;
using PdfSharp.Xps.Rendering;
using PdfSharp.Xps.XpsModel;
using IOPath = System.IO.Path;

namespace PdfSharp.Xps
{
#if true
  /// <summary>
  /// Provides functionality to convert WPF graphic to PDF files.
  /// </summary>
  static class XpsRenderer // declare deprecated
  {
    /// <summary>
    /// A first hack to do the job...
    /// </summary>
    public static void RenderPage_Test01(PdfPage page, string xpsFilename)
    {
      //XpsDocument xpsdoc = new XpsDocument(xpsFilename, System.IO.FileAccess.Read);
      //FixedDocument fds = xpsdoc.GetFixedDocument();
      //DocumentReferenceCollection docrefs = fds.References;
      //DocumentReference docref = docrefs[0];
      //Uri uri1 = docref.Source;
      //FixedDocument fixeddoc = docref.GetDocument(false);
      //PageContent content = fixeddoc.Pages[0];
      //Uri uri2 = content.Source;
      //FixedPage fixedPage = content.Child;
      //xpsdoc.Close();
      // /Documents/1/Pages/1.fpage

      try
      {
#if true
        XpsDocument doc = XpsDocument.Open(xpsFilename);
        FixedPage fpage = doc.GetDocument().GetFixedPage(0);

        //ZipPackage pack = ZipPackage.Open(xpsFilename) as ZipPackage;
        Uri uri = new Uri("/Documents/1/Pages/1.fpage", UriKind.Relative);
        ZipPackagePart part = doc.Package.GetPart(uri) as ZipPackagePart;
        using (Stream stream = part.GetStream())
        {
          using (StreamReader sr = new StreamReader(stream))
          {
            string xml = sr.ReadToEnd();
#if true
            string xmlPath = IOPath.Combine(IOPath.GetDirectoryName(xpsFilename), IOPath.GetFileNameWithoutExtension(xpsFilename)) + ".xml";
            using (StreamWriter sw = new StreamWriter(xmlPath))
            {
              sw.Write(xml);
            }
#endif
            DocumentRenderingContext context = new DocumentRenderingContext(page.Owner);
            //XpsElement el = PdfSharp.Xps.Parsing.XpsParser.Parse(xml);
            PdfRenderer renderer = new PdfRenderer();
            renderer.RenderPage(page, fpage);
          }
        }
#else
        ZipPackage pack = ZipPackage.Open(xpsFilename) as ZipPackage;
        Uri uri = new Uri("/Documents/1/Pages/1.fpage", UriKind.Relative);
        ZipPackagePart part = pack.GetPart(uri) as ZipPackagePart;
        using (Stream stream = part.GetStream())
        {
          using (StreamReader sr = new StreamReader(stream))
          {
            string xml = sr.ReadToEnd();
#if true
            string xmlPath = IOPath.Combine(IOPath.GetDirectoryName(xpsFilename), IOPath.GetFileNameWithoutExtension(xpsFilename)) + ".xml";
            using (StreamWriter sw = new StreamWriter(xmlPath))
            {
              sw.Write(xml);
            }
#endif
            XpsElement el = PdfSharp.Xps.Parsing.XpsParser.Parse(xml);
            PdfRenderer renderer = new PdfRenderer();
            renderer.RenderPage(page, el as PdfSharp.Xps.XpsModel.FixedPage);
          }
        }
#endif
      }
      catch
      {
        //DaSt :
      }
    }

#if true_
    /// <summary>
    /// 
    /// </summary>
    public static void Parser_Test01(string xpsFilename)
    {
      //XpsDocument xpsdoc = new XpsDocument(xpsFilename, System.IO.FileAccess.Read);
      //FixedDocument fds = xpsdoc.GetFixedDocument();
      //DocumentReferenceCollection docrefs = fds.References;
      //DocumentReference docref = docrefs[0];
      //Uri uri1 = docref.Source;
      //FixedDocument fixeddoc = docref.GetDocument(false);
      //PageContent content = fixeddoc.Pages[0];
      //Uri uri2 = content.Source;
      //FixedPage fixedPage = content.Child;
      //xpsdoc.Close();
      // /Documents/1/Pages/1.fpage

      try
      {
        XpsDocument document = new PdfSharp.Xps.XpsModel.XpsDocument(xpsFilename);

        FixedDocument fdoc = document.GetDocument();
        FixedPage fpage = fdoc.GetFixedPage(0);
        //string s = fpage.c.Source;
        //s.GetType();

        //Uri uri;
        //ZipPackage package = ZipPackage.Open(xpsFilename) as ZipPackage;

        //PackagePartCollection parts = package.GetParts();
        //PackageRelationshipCollection relships = package.GetRelationships();


        //uri = new Uri("/FixedDocSeq.fdseq", UriKind.Relative);
        //string x = GetPartAsString(package, uri);
        ////ZipPackagePart part = pack.GetPart(uri) as ZipPackagePart;


        //PdfSharp.Xps.Parsing.XpsParser.Parse(GetPartAsXmlReader(package, uri));


        //        Uri uri = new Uri("/Documents/1/Pages/1.fpage", UriKind.Relative);
        //        ZipPackagePart part = pack.GetPart(uri) as ZipPackagePart;
        //        using (Stream stream = part.GetStream())
        //        {
        //          using (StreamReader sr = new StreamReader(stream))
        //          {
        //            string xml = sr.ReadToEnd();
        //#if true
        //            string xmlPath = IOPath.Combine(IOPath.GetDirectoryName(xpsFilename), IOPath.GetFileNameWithoutExtension(xpsFilename)) + ".xml";
        //            using (StreamWriter sw = new StreamWriter(xmlPath))
        //            {
        //              sw.Write(xml);
        //            }
        //#endif
        //            XpsElement el = PdfSharp.Xps.Parsing.XpsParser.Parse(xml);
        //            PdfRenderer renderer = new PdfRenderer();
        //            renderer.RenderPage(page, el as PdfSharp.Xps.XpsModel.FixedPage);
        //          }
        //        }
      }

      catch (Exception ex)
      {
        Debug.WriteLine("Exception: " + ex.Message);
      }
    }
#endif

    /// <summary>
    /// Gets the specified part as string. Returns an empty string if the part not exists.
    /// </summary>
    static string GetPartAsString(ZipPackage package, Uri uri)
    {
      ZipPackagePart part = package.GetPart(uri) as ZipPackagePart;
      string xml = String.Empty;
      using (Stream stream = part.GetStream())
      {
        using (StreamReader sr = new StreamReader(stream))
        {
          xml = sr.ReadToEnd();
#if true_
          string xmlPath = IOPath.Combine(IOPath.GetDirectoryName(xpsFilename), IOPath.GetFileNameWithoutExtension(xpsFilename)) + ".xml";
          using (StreamWriter sw = new StreamWriter(xmlPath))
          {
            sw.Write(xml);
          }
#endif
          //XpsElement el = PdfSharp.Xps.Parsing.XpsParser.Parse(xml);
          //PdfRenderer renderer = new PdfRenderer();
          //renderer.RenderPage(page, el as PdfSharp.Xps.XpsModel.FixedPage);
        }
      }
      return xml;
    }

    /// <summary>
    /// Gets an XML reader for the specified part, or null, if the part not exists.
    /// </summary>
    static XmlTextReader GetPartAsXmlReader(ZipPackage package, Uri uri)
    {
      ZipPackagePart part = package.GetPart(uri) as ZipPackagePart;
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
  }
#endif
}