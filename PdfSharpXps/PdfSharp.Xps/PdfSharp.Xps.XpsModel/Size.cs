using System;
using System.Collections.Generic;
using System.Text;
using PdfSharp.Drawing;
using PdfSharp.Internal;
using PdfSharp.Xps.Parsing;

namespace PdfSharp.Xps.XpsModel
{
  /// <summary>
  /// Represents the width and height of an object.
  /// </summary>
  public struct Size
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="Size"/> struct.
    /// </summary>
    public Size(double width, double height)
      : this()
    {
      Width = width;
      Height = height;
    }

    /// <summary>
    /// Gets or sets the width.
    /// </summary>
    public double Width { get; set; }

    /// <summary>
    /// Gets or sets the height.
    /// </summary>
    public double Height { get; set; }

    /// <summary>
    /// Performs an implicit conversion from <see cref="PdfSharp.Xps.XpsModel.Size"/> to <see cref="PdfSharp.Drawing.XSize"/>.
    /// </summary>
    public static implicit operator XSize(Size size)
    {
      return new XSize(size.Width, size.Height);
    }

    /// <summary>
    /// Parses the specified value.
    /// </summary>
    public static Size Parse(string value)
    {
      Size size = new Size();
      TokenizerHelper tokenizer = new TokenizerHelper(value);
      size.Width = ParserHelper.ParseDouble(tokenizer.NextTokenRequired());
      size.Height = ParserHelper.ParseDouble(tokenizer.NextTokenRequired());
      return size;
    }
  }
}
