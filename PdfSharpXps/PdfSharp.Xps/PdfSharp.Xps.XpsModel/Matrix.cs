using System;
using System.Globalization;
using System.Collections.Generic;
using System.Text;
using PdfSharp.Internal;
using PdfSharp.Drawing;

namespace PdfSharp.Xps.XpsModel
{
  /// <summary>
  /// Represents a transformation matrix.
  /// </summary>
  struct Matrix
  {
    public Matrix(double m11, double m12, double m21, double m22, double offsetX, double offsetY)
    {
      this.m11 = m11;
      this.m12 = m12;
      this.m21 = m21;
      this.m22 = m22;
      this.offsetX = offsetX;
      this.offsetY = offsetY;
    }

    /// <summary>
    /// Parses the specified value.
    /// </summary>
    public static Matrix Parse(string value)
    {
      Matrix matrix = new Matrix();
      IFormatProvider formatProvider = CultureInfo.InvariantCulture;
      TokenizerHelper helper = new TokenizerHelper(value, formatProvider);
      matrix.m11 = Convert.ToDouble(helper.NextTokenRequired(), formatProvider);
      matrix.m12 = Convert.ToDouble(helper.NextTokenRequired(), formatProvider);
      matrix.m21 = Convert.ToDouble(helper.NextTokenRequired(), formatProvider);
      matrix.m22 = Convert.ToDouble(helper.NextTokenRequired(), formatProvider);
      matrix.offsetX = Convert.ToDouble(helper.NextTokenRequired(), formatProvider);
      matrix.offsetY = Convert.ToDouble(helper.NextTokenRequired(), formatProvider);
      return matrix;
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="PdfSharp.Xps.XpsModel.Matrix"/> to <see cref="PdfSharp.Drawing.XMatrix"/>.
    /// </summary>
    public static implicit operator XMatrix(Matrix matrix)
    {
      return new XMatrix(matrix.m11, matrix.m12, matrix.m21, matrix.m22, matrix.offsetX, matrix.offsetY);
    }

    internal double m11;
    internal double m12;
    internal double m21;
    internal double m22;
    internal double offsetX;
    internal double offsetY;
  }
}
