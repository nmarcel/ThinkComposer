using System;

namespace PdfSharp.Xps.XpsModel
{
  enum ColorType
  {
    /// <summary>
    /// "#RRGGBB"
    /// </summary>
    sRGB,

    /// <summary>
    /// "#AARRGGBB"
    /// </summary>
    sRGBwithAlpha,

    /// <summary>
    /// "sc#RedFloat, GreenFloat,BlueFloat"
    /// </summary>
    scRGB,

    /// <summary>
    /// "sc#AlphaFloat,RedFloat, GreenFloat,BlueFloat"
    /// </summary>
    scRGBwithAlpha,

    /// <summary>
    /// "ContextColor ProfileURI AlphaFloat, Chan0Float, Chan1Float, Chan2Float, Chan3Float"
    /// </summary>
    CMYKwithAlpha,

    /// <summary>
    /// "ContextColor ProfileURI AlphaFloat, Chan0Float, ..., ChanN-1Float"
    /// </summary>
    NChannelWithAlpha,

    /// <summary>
    /// "ContextColor ProfileURI AlphaFloat, TintFloat, 0, 0"
    /// </summary>
    NamedColorWithAlpha,
  }
}