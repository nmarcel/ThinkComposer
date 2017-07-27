using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using PdfSharp.Xps.XpsModel;

namespace PdfSharp.Xps.Parsing
{
  static class ParserHelper
  {
    /// <summary>
    /// Parses a double value element.
    /// </summary>
    public static double ParseDouble(string value)
    {
      return Double.Parse(value, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Parses an enum value element.
    /// </summary>
    public static T ParseEnum<T>(string value) where T : struct
    {
      return (T)Enum.Parse(typeof(T), value);
    }
  }
}