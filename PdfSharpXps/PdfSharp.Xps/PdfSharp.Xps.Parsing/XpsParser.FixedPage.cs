using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;
using System.IO;
using PdfSharp.Xps.XpsModel;

namespace PdfSharp.Xps.Parsing
{
  partial class XpsParser
  {
    /// <summary>
    /// Parses a FixedPage element.
    /// </summary>
    FixedPage ParseFixedPage()
    {
      Debug.Assert(this.reader.Name == "FixedPage");
      FixedPage fpage = new FixedPage();
      try
      {
        bool isEmptyElement = this.reader.IsEmptyElement;
        Debug.Assert(this.fpage == null);
        this.fpage = fpage;
        while (this.reader.MoveToNextAttribute())
        {
          switch (this.reader.Name)
          {
            case "Name":
              fpage.Name = this.reader.Value;
              break;

            case "Width":
              fpage.Width = ParseDouble(this.reader.Value);
              break;

            case "Height":
              fpage.Height = ParseDouble(this.reader.Value);
              break;

            case "ContentBox":
              fpage.ContentBox = Rect.Parse(this.reader.Value);
              break;

            case "BleedBox":
              fpage.BleedBox = Rect.Parse(this.reader.Value);
              break;

            case "xmlns":
              break;

            case "xmlns:xps":
              break;

            case "xmlns:false":
              break;

            case "xmlns:mc":
              break;

            case "xmlns:x":
              break;

            case "xmlns:xml":
              break;

            case "xmlns:xsi":
              break;

            case "xmlns:v2":
              break;

            case "xml:lang":
              fpage.Lang = this.reader.Value;
              break;

            case "xml:space":
              break;

            case "xsi:schemaLocation":
              break;

            case "mc:MustUnderstand":
              break;

            default:
              UnexpectedAttribute(this.reader.Name);
              break;
          }
        }
        if (!isEmptyElement)
        {
          MoveToNextElement();
          while (this.reader.IsStartElement())
          {
            XpsElement element = null;
            switch (reader.Name)
            {
              case "Path":
                element = ParsePath();
                fpage.Content.Add(element);
                element.Parent = fpage;
                break;

              case "Canvas":
                element = ParseCanvas();
                fpage.Content.Add(element);
                element.Parent = fpage;
                break;

              case "Glyphs":
                element = ParseGlyphs();
                fpage.Content.Add(element);
                element.Parent = fpage;
                break;

              case "MatrixTransform":
                Debugger.Break();
                ParseMatrixTransform();
                //fpage.
                //element = ParseGlyphs();
                //fpage.Content.Add(element);
                break;

              case "FixedPage.Resources":
                MoveToNextElement();
                ResourceDictionary rd = new ResourceDictionary();
                fpage.Resources = rd;
                rd.Parent = fpage;
                rd.ResourceParent = ResourceDictionaryStack.Current;
                ResourceDictionaryStack.Push(rd);
                ParseResourceDictionary(rd);
                MoveToNextElement();
                break;

              case "mc:AlternateContent":
                MoveToNextElement();
                break;

              case "mc:Choice":
                MoveToNextElement();
                break;

              case "v2:Circle":
                MoveToNextElement();
                break;

              case "v2:Watermark":
                MoveToNextElement();
                break;

              case "v2:Blink":
                MoveToNextElement();
                break;

              default:
                Debugger.Break();
                break;
            }
          }
        }
        MoveToNextElement();
      }
      finally
      {
        // If the current ResourceDictionary is from this FixedPage, pop it.
        if (fpage != null && fpage.Resources != null)
        {
          if (Object.ReferenceEquals(fpage.Resources, ResourceDictionaryStack.Current))
            ResourceDictionaryStack.Pop();
        }
      }
      return fpage;
    }
  }
}