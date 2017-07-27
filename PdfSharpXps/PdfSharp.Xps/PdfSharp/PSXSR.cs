#region PDFsharp - A .NET library for processing PDF
//
// Authors:
//   Stefan Lange (mailto:Stefan.Lange@pdfsharp.com)
//
// Copyright (c) 2005-2009 empira Software GmbH, Cologne (Germany)
//
// http://www.pdfsharp.com
// http://sourceforge.net/projects/pdfsharp
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included
// in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Diagnostics;
using System.Resources;
using System.Reflection;
using System.Text.RegularExpressions;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

#pragma warning disable 1591

namespace PdfSharp
{
  /// <summary>
  /// The Pdf-Sharp-XPS-String-Resources.
  /// </summary>
  static class PSXSR
  {
    #region General messages

    public static string IndexOutOfRange
    {
      get { return "The index is out of range."; }
    }

    public static string InvalidValue(int val, string name, int min, int max)
    {
      return Format("{0} is not a valid value for {1}. {1} should be greater than or equal to {2} and less than or equal to {3}.",
        val, name, min, max);
    }

    public static string PageMustBelongToPdfDocument
    {
      get { return "The page do not belong to the PDF document."; }
    }

    #endregion

    #region XPS Parser

    /// <summary>
    /// "Content must start with element or comment."
    /// </summary>
    public static string ElementExpected
    {
      get { return "Content must start with element or comment."; }
    }

    /// <summary>
    /// "Must stand on element."
    /// </summary>
    public static string MustStandOnElement
    {
      get { return "Must stand on element."; }
    }

    ///// <summary>
    ///// "Unexpected element."
    ///// </summary>
    //public static string UnexpectedElement
    //{
    //  get { return "Unexpected element."; }
    //}

    /// <summary>
    /// "Unexpected attribute '{0}'."
    /// </summary>
    public static string UnexpectedAttribute(string name)
    {
      return String.Format("Unexpected attribute '{0}'.", name);
    }

    /// <summary>
    /// "Unexpected element. Expected '{0}', but found '{1}'"
    /// </summary>
    public static string UnexpectedElement(string expected, string found)
    {
      return String.Format("Unexpected element. Expected '{0}', but found '{1}'", expected, found);
    }

    #endregion

    #region Helper functions
    /// <summary>
    /// Loads the message from the resource associated with the enum type and formats it
    /// using 'String.Format'. Because this function is intended to be used during error
    /// handling it never raises an exception.
    /// </summary>
    /// <param name="id">The type of the parameter identifies the resource
    /// and the name of the enum identifies the message in the resource.</param>
    /// <param name="args">Parameters passed through 'String.Format'.</param>
    /// <returns>The formatted message.</returns>
    public static string Format(PSXMsgID id, params object[] args)
    {
      string message;
      try
      {
        message = PSXSR.GetString(id);
        if (message != null)
          message = Format(message, args);
        else
          message = "INTERNAL ERROR: Message not found in resources.";
        return message;
      }
      catch (Exception ex)
      {
        message = String.Format("UNEXPECTED ERROR while formatting message with ID {0}: {1}", id.ToString(), ex.ToString());
      }
      return message;
    }

    public static string Format(string format, params object[] args)
    {
      if (format == null)
        throw new ArgumentNullException("format");

      string message;
      try
      {
        message = String.Format(format, args);
      }
      catch (Exception ex)
      {
        message = String.Format("UNEXPECTED ERROR while formatting message '{0}': {1}", format, ex.ToString());
      }
      return message;
    }

    /// <summary>
    /// Gets the localized message identified by the specified DomMsgID.
    /// </summary>
    public static string GetString(PSXMsgID id)
    {
      return PSXSR.ResMngr.GetString(id.ToString());
    }
    #endregion

    #region Resource manager

    /// <summary>
    /// Gets the resource manager for this module.
    /// </summary>
    public static ResourceManager ResMngr
    {
      get
      {
        if (PSXSR.resmngr == null)
        {
#if true_
          // Force the english language, even on a German PC.
          System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;
#endif
          PSXSR.resmngr = new ResourceManager("PdfSharp.Resources.Messages", Assembly.GetExecutingAssembly());
        }
        return PSXSR.resmngr;
      }
    }
    static ResourceManager resmngr;

    /// <summary>
    /// Writes all messages defined by PSXMsgID.
    /// </summary>
    [Conditional("DEBUG")]
    public static void TestResourceMessages()
    {
      string[] names = Enum.GetNames(typeof(PSXMsgID));
      foreach (string name in names)
      {
        string message = String.Format("{0}: '{1}'", name, ResMngr.GetString(name));
        Debug.Assert(message != null);
        Debug.WriteLine(message);
      }
    }

    static PSXSR()
    {
      TestResourceMessages();
    }

    #endregion
  }
}