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
    /// Parses a StoryFragment element.
    /// </summary>
    StoryFragment ParseStoryFragment()
    {
      Debug.Assert(this.reader.Name == "");
      bool isEmptyElement = this.reader.IsEmptyElement;
      StoryFragment storyFragment = new StoryFragment();
      while (MoveToNextAttribute())
      {
        switch (this.reader.Name)
        {
          case "StoryName":
            storyFragment.StoryName = this.reader.Value;
            break;

          case "FragmentName":
            storyFragment.FragmentName = this.reader.Value;
            break;

          case "FragmentType":
            storyFragment.FragmentType = this.reader.Value;
            break;
          
          default:
            UnexpectedAttribute(this.reader.Name);
            break;
        }
      }
      MoveToNextElement();
      return storyFragment;
    }
  }
}