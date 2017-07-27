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
    /// Parses a TableCellStructure element.
    /// </summary>
    TableCellStructure ParseTableCellStructure()
    {
      Debug.Assert(this.reader.Name == "");
      bool isEmptyElement = this.reader.IsEmptyElement;
      TableCellStructure tableCellStructure = new TableCellStructure();
      while (MoveToNextAttribute())
      {
        switch (this.reader.Name)
        {
          case "RowSpan":
            tableCellStructure.RowSpan = int.Parse(this.reader.Value);
            break;

          case "ColumnSpan":
            tableCellStructure.ColumnSpan = int.Parse(this.reader.Value);
            break;

          default:
            UnexpectedAttribute(this.reader.Name);
            break;
        }
      }
      MoveToNextElement();
      return tableCellStructure;
    }
  }
}