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
    /// Parses a StoryFragmentReference element.
    /// </summary>
    StoryFragmentReference ParseStoryFragmentReference()
    {
      Debug.Assert(this.reader.Name == "");
      bool isEmptyElement = this.reader.IsEmptyElement;
      StoryFragmentReference storyFragmentReference = new StoryFragmentReference();
      while (MoveToNextAttribute())
      {
        switch (this.reader.Name)
        {
          case "Page":
            storyFragmentReference.Page = int.Parse(this.reader.Value);
            break;

          case "FragmentName":
            storyFragmentReference.FragmentName = this.reader.Value;
            break;

          default:
            UnexpectedAttribute(this.reader.Name);
            break;
        }
      }
      MoveToNextElement();
      return storyFragmentReference;
    }
  }
}