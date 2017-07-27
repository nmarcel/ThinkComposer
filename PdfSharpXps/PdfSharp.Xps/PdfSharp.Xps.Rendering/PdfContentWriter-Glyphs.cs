using System;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;
using System.Text;
using PdfSharp.Xps.XpsModel;
using PdfSharp.Pdf;
using PdfSharp.Internal;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Internal;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Pdf;
using PdfSharp.Fonts.OpenType;

#pragma warning disable 414, 169, 649 // incomplete code state

namespace PdfSharp.Xps.Rendering
{
  partial class PdfContentWriter  // TODO: refactor to a PdfGlyphsWriter class
  {
    /// <summary>
    /// Writes a Glyphs to the content stream.
    /// </summary>
    private void WriteGlyphs(Glyphs glyphs)
    {
      WriteSaveState("begin Glyphs", glyphs.Name);

      // Transform also affects clipping and opacity mask
      bool transformed = glyphs.RenderTransform != null;
      if (transformed)
        WriteRenderTransform(glyphs.RenderTransform);

      bool clipped = glyphs.Clip != null;
      if (clipped)
        WriteClip(glyphs.Clip);

      if (glyphs.Opacity < 1)
        MultiplyOpacity(glyphs.Opacity);

      if (glyphs.OpacityMask != null)
        WriteOpacityMask(glyphs.OpacityMask);

      XMatrix textMatrix = new XMatrix();

      textMatrix.TranslatePrepend(glyphs.OriginX, glyphs.OriginY);
      glyphs.OriginX = glyphs.OriginY = 0; // HACK: do not change model

      double emSize = glyphs.FontRenderingEmSize;
      textMatrix.ScalePrepend(glyphs.FontRenderingEmSize);
      glyphs.FontRenderingEmSize = 1; // HACK: do not change model


      bool boldSimulation = (glyphs.StyleSimulations & StyleSimulations.BoldSimulation) == StyleSimulations.BoldSimulation;

      // just a draft...
      if (boldSimulation)
      {
        boldSimulation = true;

        // draw black stroke if it is not a solid color brush
        XColor color = XColor.FromArgb(0, 0, 0);
        if (glyphs.Fill is SolidColorBrush)
        {
          SolidColorBrush brush = glyphs.Fill as SolidColorBrush;
          color = brush.Color;
        }
        WriteLiteral(String.Format(CultureInfo.InvariantCulture, "{0:0.###} {1:0.###} {2:0.###}  RG\n", color.R / 255.0, color.G / 255.0, color.B / 255.0));
        WriteLiteral("{0:0.###} w\n", emSize / 50);
      }

      if ((glyphs.StyleSimulations & StyleSimulations.ItalicSimulation) == StyleSimulations.ItalicSimulation)
      {
        textMatrix.SkewPrepend(-20, 0);
      }

      XForm xform = null;
      XImage ximage = null;
      RealizeFill(glyphs.Fill, ref xform, ref ximage);
      RealizeFont(glyphs);

      if (boldSimulation)
        WriteLiteral("2 Tr\n", 1);

      double x = glyphs.OriginX;
      double y = glyphs.OriginY;


      //switch (format.Alignment)
      //{
      //  case XStringAlignment.Near:
      //    // nothing to do
      //    break;

      //  case XStringAlignment.Center:
      //    x += (rect.Width - width) / 2;
      //    break;

      //  case XStringAlignment.Far:
      //    x += rect.Width - width;
      //    break;
      //}

      PdfFont realizedFont = this.graphicsState.realizedFont;
      Debug.Assert(realizedFont != null);
      realizedFont.AddChars(glyphs.UnicodeString);

      OpenTypeDescriptor descriptor = realizedFont.FontDescriptor.descriptor;

      //if (bold && !descriptor.IsBoldFace)
      //{
      //  // TODO: emulate bold by thicker outline
      //}

      //if (italic && !descriptor.IsBoldFace)
      //{
      //  // TODO: emulate italic by shearing transformation
      //}

#if true
      string s2 = "";
      string s = glyphs.UnicodeString;
      if (!String.IsNullOrEmpty(s))
      {
        int length = s.Length;
        for (int idx = 0; idx < length; idx++)
        {
          char ch = s[idx];
          int glyphID = 0;
          if (descriptor.fontData.cmap.symbol)
          {
            glyphID = (int)ch + (descriptor.fontData.os2.usFirstCharIndex & 0xFF00);
            glyphID = descriptor.CharCodeToGlyphIndex((char)glyphID);
          }
          else
            glyphID = descriptor.CharCodeToGlyphIndex(ch);
          s2 += (char)glyphID;
        }
      }
      s = s2;
#endif

      byte[] bytes = PdfEncoders.RawUnicodeEncoding.GetBytes(s);
      bytes = PdfEncoders.FormatStringLiteral(bytes, true, false, true, null);
      string text = PdfEncoders.RawEncoding.GetString(bytes);
      if (glyphs.IsSideways)
      {
        textMatrix.RotateAtPrepend(-90, new XPoint(x, y));
        XPoint pos = new XPoint(x, y);
        AdjustTextMatrix(ref pos);
        //WriteTextTransform(textMatrix);
        WriteLiteral("{0} Tj\n", text);
      }
      else
      {
#if true
        //if (glyphs.BidiLevel % 2 == 1)
        //  WriteLiteral("-1 Tc\n");

        if (!textMatrix.IsIdentity)
          WriteTextTransform(textMatrix);

        WriteGlyphsInternal(glyphs, null);
#else
        XPoint pos = new XPoint(x, y);
        AdjustTextMatrix(ref pos);
        WriteLiteral("{0:0.###} {1:0.###} Td {2} Tj\n", pos.x, pos.y, text);
        //PdfEncoders.ToStringLiteral(s, PdfStringEncoding.RawEncoding, null));
#endif
      }
      WriteRestoreState("end Glyphs", glyphs.Name);
    }

    // ...on the way to handle Indices...
    private void WriteGlyphsInternal(Glyphs glyphs, string text)
    {
      GlyphIndicesComplexity complexity = GlyphIndicesComplexity.None;
      if (glyphs.Indices != null)
        complexity = glyphs.Indices.Complexity;
      complexity = GlyphIndicesComplexity.ClusterMapping;
      switch (complexity)
      {
        case GlyphIndicesComplexity.None:
          break;

        case GlyphIndicesComplexity.DistanceOnly:
          WriteGlyphs_DistanceOnly(glyphs);
          break;

        case GlyphIndicesComplexity.GlyphIndicesAndDistanceOnly:
          break;

        case GlyphIndicesComplexity.ClusterMapping:
          WriteGlyphs_ClusterMapping(glyphs);
          break;
      }
    }

    private void WriteGlyphs_None(Glyphs glyphs, string text)
    {
      // TODO:
    }

    private void WriteGlyphs_DistanceOnly(Glyphs glyphs, string text)
    {
      // TODO:
    }

    private void WriteGlyphs_GlyphIndicesAndDistanceOnly(Glyphs glyphs, string text)
    {
      // TODO:
    }


    /// <summary>
    /// This is just a draft to see what to do in detail.
    /// </summary>
    private void WriteGlyphs_DistanceOnly(Glyphs glyphs)
    {
      string unicodeString = glyphs.UnicodeString;
#if DEBUG_
      if (!String.IsNullOrEmpty(unicodeString))
      {
        if (unicodeString.StartsWith("abc"))
          GetType();
      }
#endif

      bool boldSimulation = (glyphs.StyleSimulations & StyleSimulations.BoldSimulation) == StyleSimulations.BoldSimulation;
      double boldSimulationFactor = 1;
      if (boldSimulation)
        boldSimulationFactor = 1;

      bool RightToLeft = glyphs.BidiLevel % 2 == 1;  // TODOWPF: why is this a level?? what means "bidirectional nesting"?

      GlyphIndices indices = glyphs.Indices;
      if (indices == null)
        indices = new GlyphIndices();
      int codeIdx = 0;
      int codeCount = String.IsNullOrEmpty(unicodeString) ? 0 : unicodeString.Length;
      int glyphCount = indices.Count;
      int glyphIdx = 0;
      bool stop = false;

      PdfFont realizedFont = this.graphicsState.realizedFont;
      OpenTypeDescriptor descriptor = realizedFont.FontDescriptor.descriptor;
      int glyphIndex;

      double x = glyphs.OriginX;
      double y = glyphs.OriginY;
      XPoint pos = new XPoint(x, y); // accumulation may lead to rounding error -> check it!

      StringBuilder outputText = new StringBuilder();
      double accumulatedWidth = 0;
      int outputGlyphCount = 0;
      bool mustRender = false;
      bool hasOffset = false;

      do
      {
        GlyphIndices.GlyphMapping clusterMapping = new GlyphIndices.GlyphMapping(42);
        if (glyphIdx < glyphCount)
          clusterMapping = indices[glyphIdx];

        for (int clusterGlyphIdx = 0; clusterGlyphIdx < clusterMapping.ClusterGlyphCount; clusterGlyphIdx++)
        {
          GlyphIndices.GlyphMapping mapping = new GlyphIndices.GlyphMapping(42);
          if (glyphIdx + clusterGlyphIdx < glyphCount)
            mapping = indices[glyphIdx + clusterGlyphIdx];

          Debug.Assert(mustRender == false);

          // Determine whether to render accumulated glyphs
          if (outputGlyphCount > 0 && (hasOffset || mapping.HasAdvanceWidthOrOffset))
          {
            outputText.Append('>');

            WriteLiteral("{0:0.####} {1:0.####} Td {2}Tj\n", pos.x, pos.y, outputText.ToString());

            //double width = descriptor.GlyphIndexToPdfWidth(glyphIndex);
            //if (!PdfSharp.Internal.DoubleUtil.IsNaN(mapping.AdvanceWidth))
            //  width = mapping.AdvanceWidth * 10;
            //pos = new XPoint(accumulatedWidth + width / 1000 * glyphs.FontRenderingEmSize, 0);
            pos = new XPoint(accumulatedWidth, 0);

            // reset values
            accumulatedWidth = 0;
            outputGlyphCount = 0;
            outputText.Length = 0;
            mustRender = false;
          }

          mustRender = mapping.HasAdvanceWidth;
          //mustRender = true;

          // get index of current glyph
          if (mapping.HasGlyphIndex)
            glyphIndex = mapping.GlyphIndex;
          else
            glyphIndex = descriptor.CharCodeToGlyphIndex(unicodeString[codeIdx]);

          // add glyph index to the fonts 'used glyph table'
          realizedFont.AddGlyphIndices(new string((char)glyphIndex, 1));

          if (outputGlyphCount == 0)
            outputText.Append('<');
          outputText.AppendFormat("{0:X2}{1:X2}", (byte)(glyphIndex >> 8), (byte)glyphIndex);

          // At the end of the glyph run we must always render
          if (!mustRender)
            mustRender = codeIdx + clusterMapping.ClusterCodeUnitCount >= codeCount  // is it the last code unit cluster
              && glyphIdx + clusterGlyphIdx + 1 >= glyphCount;  // is it the last glyph index

          //mustRender = true;
          if (mustRender)
          {
            outputText.Append('>');

            WriteLiteral("{0:0.####} {1:0.####} Td {2}Tj\n", pos.x, pos.y, outputText.ToString());

            double width = descriptor.GlyphIndexToPdfWidth(glyphIndex);
            if (!PdfSharp.Internal.DoubleUtil.IsNaN(mapping.AdvanceWidth))
              width = mapping.AdvanceWidth * 10;
            pos = new XPoint(accumulatedWidth + width * boldSimulationFactor / 1000 * glyphs.FontRenderingEmSize, 0);

            // reset values
            accumulatedWidth = 0;
            outputGlyphCount = 0;
            outputText.Length = 0;
            mustRender = false;
          }
          else // deferred rendering
          {
            // accumulate width
            Debug.Assert(DoubleUtil.IsNaN(mapping.AdvanceWidth));
            double width = descriptor.GlyphIndexToPdfWidth(glyphIndex);
            width = width * boldSimulationFactor / 1000 * glyphs.FontRenderingEmSize;
            accumulatedWidth += width;

            outputGlyphCount++;
          }
        }
        codeIdx += clusterMapping.ClusterCodeUnitCount;
        glyphIdx += clusterMapping.ClusterGlyphCount;

        if (codeIdx >= codeCount && glyphIdx >= glyphCount)
          stop = true;
      }
      while (!stop);
    }

    /// <summary>
    /// This is just a draft to see what to do in detail.
    /// </summary>
    private void WriteGlyphs_ClusterMapping(Glyphs glyphs)
    {
      string unicodeString = glyphs.UnicodeString;
#if DEBUG_
      if (!String.IsNullOrEmpty(unicodeString))
      {
        if (unicodeString.StartsWith("abc"))
          GetType();
      }
#endif

      bool boldSimulation = (glyphs.StyleSimulations & StyleSimulations.BoldSimulation) == StyleSimulations.BoldSimulation;
      double boldSimulationFactor = 1;
      if (boldSimulation)
        boldSimulationFactor = 1;

      bool RightToLeft = glyphs.BidiLevel % 2 == 1;  // TODOWPF: why is this a level?? what means "bidirectional nesting"?

      GlyphIndices indices = glyphs.Indices;
      if (indices == null)
        indices = new GlyphIndices();
      int codeIdx = 0;
      int codeCount = String.IsNullOrEmpty(unicodeString) ? 0 : unicodeString.Length;
      int glyphCount = indices.Count;
      int glyphIdx = 0;
      bool stop = false;

      PdfFont realizedFont = this.graphicsState.realizedFont;
      OpenTypeDescriptor descriptor = realizedFont.FontDescriptor.descriptor;
      int glyphIndex;

      double x = glyphs.OriginX;
      double y = glyphs.OriginY;
      XPoint pos = new XPoint(x, y); // accumulation may lead to rounding error -> check it!
      double uOffset = 0;
      double vOffset = 0;

      StringBuilder outputText = new StringBuilder();
      double accumulatedWidth = 0;
      int outputGlyphCount = 0;
      bool mustRender = false;
      bool hasOffset = false;

      do
      {
        GlyphIndices.GlyphMapping clusterMapping = new GlyphIndices.GlyphMapping(42);
        if (glyphIdx < glyphCount)
          clusterMapping = indices[glyphIdx];

        for (int clusterGlyphIdx = 0; clusterGlyphIdx < clusterMapping.ClusterGlyphCount; clusterGlyphIdx++)
        {
          GlyphIndices.GlyphMapping mapping = new GlyphIndices.GlyphMapping(42);
          if (glyphIdx + clusterGlyphIdx < glyphCount)
            mapping = indices[glyphIdx + clusterGlyphIdx];

          Debug.Assert(mustRender == false);

          // Determine whether to render accumulated glyphs
          if (outputGlyphCount > 0 && (hasOffset || mapping.HasAdvanceWidthOrOffset))
          {
            outputText.Append('>');

            WriteLiteral("{0:0.####} {1:0.####} Td {2}Tj\n", pos.x, pos.y, outputText.ToString());

            //double width = descriptor.GlyphIndexToPdfWidth(glyphIndex);
            //if (!PdfSharp.Internal.DoubleUtil.IsNaN(mapping.AdvanceWidth))
            //  width = mapping.AdvanceWidth * 10;
            //pos = new XPoint(accumulatedWidth + width / 1000 * glyphs.FontRenderingEmSize, 0);
            pos = new XPoint(accumulatedWidth, 0);

            // reset values
            accumulatedWidth = 0;
            outputGlyphCount = 0;
            outputText.Length = 0;
            mustRender = false;
          }

          mustRender = mapping.HasAdvanceWidth;
          //mustRender = true;

          // Adjust former uOffset
          if (uOffset != 0)
          {
            pos.x -= uOffset;
            uOffset = 0;
            mustRender = true;
          }

          // Adjust position by current former uOffset
          if (mapping.HasUOffset)
          {
            uOffset = mapping.UOffset * glyphs.FontRenderingEmSize / 100;
            pos.x += uOffset;
            mustRender = true;
            hasOffset = true;
          }

          // Adjust former vOffset
          if (vOffset != 0)
          {
            pos.y += vOffset;
            vOffset = 0;
            mustRender = true;
          }

          // Adjust position by current former vOffset
          if (mapping.HasVOffset)
          {
            vOffset = mapping.VOffset * glyphs.FontRenderingEmSize / 100;
            pos.y -= vOffset;
            mustRender = true;
            hasOffset = true;
          }


          // get index of current glyph
          if (mapping.HasGlyphIndex)
            glyphIndex = mapping.GlyphIndex;
          else
            glyphIndex = descriptor.CharCodeToGlyphIndex(unicodeString[codeIdx]);

          // add glyph index to the fonts 'used glyph table'
          realizedFont.AddGlyphIndices(new string((char)glyphIndex, 1));

#if true
          if (outputGlyphCount == 0)
            outputText.Append('<');
          outputText.AppendFormat("{0:X2}{1:X2}", (byte)(glyphIndex >> 8), (byte)glyphIndex);
#else
          byte[] bytes = new byte[2] { (byte)(glyphIndex >> 8), (byte)glyphIndex };
          bytes = PdfEncoders.FormatStringLiteral(bytes, true, false, true, null);
          string output = PdfEncoders.RawEncoding.GetString(bytes);
#endif

          // At the end of the glyph run we must always render
          if (!mustRender)
            mustRender = codeIdx + clusterMapping.ClusterCodeUnitCount >= codeCount  // is it the last code unit cluster
              && glyphIdx + clusterGlyphIdx + 1 >= glyphCount;  // is it the last glyph index

          //mustRender = true;
          if (mustRender)
          {
            outputText.Append('>');

            WriteLiteral("{0:0.####} {1:0.####} Td {2}Tj\n", pos.x, pos.y, outputText.ToString());

            double width = descriptor.GlyphIndexToPdfWidth(glyphIndex);
            if (!PdfSharp.Internal.DoubleUtil.IsNaN(mapping.AdvanceWidth))
              width = mapping.AdvanceWidth * 10;
            pos = new XPoint(accumulatedWidth + width * boldSimulationFactor / 1000 * glyphs.FontRenderingEmSize, 0);

            // reset values
            accumulatedWidth = 0;
            outputGlyphCount = 0;
            outputText.Length = 0;
            mustRender = false;
          }
          else // deferred rendering
          {
            // accumulate width
            Debug.Assert(DoubleUtil.IsNaN(mapping.AdvanceWidth));
            double width = descriptor.GlyphIndexToPdfWidth(glyphIndex);
            width = width * boldSimulationFactor / 1000 * glyphs.FontRenderingEmSize;
            accumulatedWidth += width;

            outputGlyphCount++;
          }
        }
        codeIdx += clusterMapping.ClusterCodeUnitCount;
        glyphIdx += clusterMapping.ClusterGlyphCount;

        if (codeIdx >= codeCount && glyphIdx >= glyphCount)
          stop = true;
      }
      while (!stop);
    }
  }
}
