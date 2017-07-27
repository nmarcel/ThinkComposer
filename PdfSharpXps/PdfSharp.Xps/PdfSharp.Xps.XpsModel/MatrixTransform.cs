using System;
using System.Collections.Generic;
using System.Text;

namespace PdfSharp.Xps.XpsModel
{
  /// <summary>
  /// Creates an arbitrary affine matrix transformation that manipulates objects or coordinate systems
  /// in a two-dimensional plane.
  /// </summary>
  class MatrixTransform : XpsElement
  {
    internal MatrixTransform()
    {
      Matrix = new Matrix(1, 0, 0, 1, 0, 0);
    }

    internal MatrixTransform(Matrix matrix)
    {
      Matrix = matrix;
    }

    /// <summary>
    /// Specifies the matrix structure that defines the transformation. 
    /// </summary>
    public Matrix Matrix { get; set; }

    /// <summary>
    /// Specifies a name for a resource in a resource dictionary. x:Key MUST be present when the
    /// current element is defined in a resource dictionary. x:Key MUST NOT be specified outside
    /// of a resource dictionary [M7.11]. 
    /// </summary>
    public string Key { get; set; }

    public static implicit operator Drawing.XMatrix(MatrixTransform transform)
    {
      return new Drawing.XMatrix(transform.Matrix.m11, transform.Matrix.m12, transform.Matrix.m21, transform.Matrix.m22,
        transform.Matrix.offsetX, transform.Matrix.offsetY);
    }
  }
}