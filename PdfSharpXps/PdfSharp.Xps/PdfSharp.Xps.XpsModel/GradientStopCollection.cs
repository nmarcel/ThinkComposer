using System;
using System.Collections.Generic;
using System.Text;

namespace PdfSharp.Xps.XpsModel
{
  /// <summary>
  /// Represents a collection of GradientStop objecs.
  /// </summary>
  class GradientStopCollection : List<GradientStop>
  {
    // Currently just a placeholder of a generic list.

    /// <summary>
    /// Gets a value indicating whether at least one color has an alpha value less than 1.
    /// </summary>
    public bool HasTransparency
    {
      get
      {
        for (int idx = 0; idx < Count; idx++)
        {
          if (this[idx].Color.A != 255)
            return true;
        }
        return false;
      }
    }

    /// <summary>
    /// HACK: Gets the average alpha value.
    /// </summary>
    public double GetAverageAlpha()
    {
      double result=0;
      for (int idx = 0; idx < Count; idx++)
      {
        Color clr = this[idx].Color;
        result += clr.A / 255.0;
      }
      result /= Count;
      return result;
    }
  }
}