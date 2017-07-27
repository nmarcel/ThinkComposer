using System;

namespace PdfSharp.Xps.Rendering
{
  enum RenderingComplexity  // not used anymore
  {
    /// <summary>
    /// All alpha values 100%, opacity 100%, no OpacityMask
    /// </summary>
    SolidColorsOnly,

    /// <summary>
    /// All alpha values 100%, opacity less than 100%, no OpacityMask
    /// </summary>
    ObjectOpacity,

    /// <summary>
    /// At least one alpha values less than 100%, opacity less than or equal 100%, no OpacityMask
    /// </summary>
    AlphaTransparancy,

    /// <summary>
    /// ???
    /// </summary>
    OpacityMask,
  }
}