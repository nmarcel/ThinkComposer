namespace Instrumind.Common.Visualization
{
    using System;
    using System.Diagnostics;
    using System.Text;
    using System.IO;
    using System.Xml;

    /// <summary>
    /// XAML to HTML Converter.
    /// </summary>
    public static class XamlFlowDocumentToHtmlConverter
    {
        /// <summary>
        /// Returns the supplied XAML FlowDocument as HTML.
        /// </summary>
        public static string Convert(string XamlText)
        {
            var Reader = new XmlTextReader(new StringReader(XamlText));
            var Builder = new StringBuilder(XamlText.Length);
            var Writer = new XmlTextWriter(new StringWriter(Builder));

            if (!WriteDocument(Reader, Writer))
                return "";

            var Result = Builder.ToString();
            return Result;
        }

        /// <summary>
        /// Processes a root level element of XAML (normally it's FlowDocument element).
        /// </summary>
        /// <param name="Reader">
        /// XmlTextReader for a source xaml.
        /// </param>
        /// <param name="Writer">
        /// XmlTextWriter producing resulting html
        /// </param>
        private static bool WriteDocument(XmlTextReader Reader, XmlTextWriter Writer)
        {
            // Xaml content is empty - nothing to convert
            if (!AdvanceSegment(Reader))
                return false;

            // Root FlowDocument elemet is missing
            if (Reader.NodeType != XmlNodeType.Element || Reader.Name != "FlowDocument")
                return false;

            // Create a buffer StringBuilder for collecting css properties for inline STYLE attributes
            // on every element level (it will be re-initialized on every level).
            var InlineStyle = new StringBuilder();

            Writer.WriteStartElement("HTML");
            Writer.WriteStartElement("BODY");

            ConvertFormatProps(Reader, Writer, InlineStyle);

            ConvertElement(Reader, Writer, InlineStyle);

            Writer.WriteEndElement();
            Writer.WriteEndElement();

            return true;
        }

        /// <summary>
        /// Reads attributes of the current xaml element and converts
        /// them into appropriate html attributes or css styles.
        /// </summary>
        /// <param name="Reader">
        /// XmlTextReader which is expected to be at XmlNodeType.Element
        /// (opening element tag) position.
        /// The reader will remain at the same level after function complete.
        /// </param>
        /// <param name="Writer">
        /// XmlTextWriter for output html, which is expected to be in
        /// after WriteStartElement state.
        /// </param>
        /// <param name="Style">
        /// String builder for collecting css properties for inline STYLE attribute.
        /// </param>
        private static void ConvertFormatProps(XmlTextReader Reader, XmlTextWriter Writer, StringBuilder Style)
        {
            // Clear string builder for the inline style
            Style.Remove(0, Style.Length);

            if (!Reader.HasAttributes)
                return;

            bool borderSet = false;

            while (Reader.MoveToNextAttribute())
            {
                string css = null;

                switch (Reader.Name)
                {
                    // Character fomatting properties
                    // ------------------------------
                    case "Background":
                        css = "background-color:" + ConvertColor(Reader.Value) + ";";
                        break;
                    case "FontFamily":
                        css = "font-family:" + Reader.Value + ";";
                        break;
                    case "FontStyle":
                        css = "font-style:" + Reader.Value.ToLower() + ";";
                        break;
                    case "FontWeight":
                        css = "font-weight:" + Reader.Value.ToLower() + ";";
                        break;
                    case "FontStretch":
                        break;
                    case "FontSize":
                        css = "font-size:" + Reader.Value + ";";
                        break;
                    case "Foreground":
                        css = "color:" + ConvertColor(Reader.Value) + ";";
                        break;
                    case "TextDecorations":
                        css = "text-decoration:underline;";
                        break;
                    case "TextEffects":
                        break;
                    case "Emphasis":
                        break;
                    case "StandardLigatures":
                        break;
                    case "Variants":
                        break;
                    case "Capitals":
                        break;
                    case "Fraction":
                        break;

                    // Paragraph formatting properties
                    // -------------------------------
                    case "Padding":
                        css = "padding:" + ConvertThickness(Reader.Value) + ";";
                        break;
                    case "Margin":
                        css = "margin:" + ConvertThickness(Reader.Value) + ";";
                        break;
                    case "BorderThickness":
                        css = "border-width:" + ConvertThickness(Reader.Value) + ";";
                        borderSet = true;
                        break;
                    case "BorderBrush":
                        css = "border-color:" + ConvertColor(Reader.Value) + ";";
                        borderSet = true;
                        break;
                    case "LineHeight":
                        break;
                    case "TextIndent":
                        css = "text-indent:" + Reader.Value + ";";
                        break;
                    case "TextAlignment":
                        css = "text-align:" + Reader.Value + ";";
                        break;
                    case "IsKeptTogether":
                        break;
                    case "IsKeptWithNext":
                        break;
                    case "ColumnBreakBefore":
                        break;
                    case "PageBreakBefore":
                        break;
                    case "FlowDirection":
                        break;

                    // Table attributes
                    // ----------------
                    case "Width":
                        css = "width:" + Reader.Value + ";";
                        break;
                    case "ColumnSpan":
                        Writer.WriteAttributeString("COLSPAN", Reader.Value);
                        break;
                    case "RowSpan":
                        Writer.WriteAttributeString("ROWSPAN", Reader.Value);
                        break;
                }

                if (css != null)
                    Style.Append(css);
            }

            if (borderSet)
                Style.Append("border-style:solid;mso-element:para-border-div;");

            // Return the xamlReader back to element level
            Reader.MoveToElement();
        }

        private static string ConvertColor(string Color)
        {
            // Remove transparancy value
            if (Color.StartsWith("#"))
                Color = "#" + Color.Substring(3);

            return Color;
        }

        private static string ConvertThickness(string Thickness)
        {
            var values = Thickness.Split(',');

            for (int i = 0; i < values.Length; i++)
            {
                double value;
                if (double.TryParse(values[i], out value))
                    values[i] = Math.Ceiling(value).ToString();
                else
                    values[i] = "1";
            }

            string cssThickness;
            switch (values.Length)
            {
                case 1:
                    cssThickness = Thickness;
                    break;
                case 2:
                    cssThickness = values[1] + " " + values[0];
                    break;
                case 4:
                    cssThickness = values[1] + " " + values[2] + " " + values[3] + " " + values[0];
                    break;
                default:
                    cssThickness = values[0];
                    break;
            }

            return cssThickness;
        }

        /// <summary>
        /// Reads a content of current xaml element, converts it
        /// </summary>
        /// <param name="Reader">
        /// XmlTextReader which is expected to be at XmlNodeType.Element
        /// (opening element tag) position.
        /// </param>
        /// <param name="Writer">
        /// May be null, in which case we are skipping the xaml element;
        /// witout producing any output to html.
        /// </param>
        /// <param name="Style">
        /// StringBuilder used for collecting css properties for inline STYLE attribute.
        /// </param>
        private static void ConvertElement(XmlTextReader Reader, XmlTextWriter Writer, StringBuilder Style)
        {
            var ContentStarted = false;

            if (Reader.IsEmptyElement)
            {
                if (Writer != null && !ContentStarted && Style.Length > 0)
                {
                    // Output STYLE attribute and clear inlineStyle buffer.
                    Writer.WriteAttributeString("STYLE", Style.ToString());
                    Style.Remove(0, Style.Length);
                }
                ContentStarted = true;
            }
            else
            {
                while (AdvanceSegment(Reader) && Reader.NodeType != XmlNodeType.EndElement)
                {
                    switch (Reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (Reader.Name.Contains("."))
                            {
                                AppendProp(Reader, Style);
                            }
                            else
                            {
                                if (Writer != null && !ContentStarted && Style.Length > 0)
                                {
                                    // Output STYLE attribute and clear inlineStyle buffer.
                                    Writer.WriteAttributeString("STYLE", Style.ToString());
                                    Style.Remove(0, Style.Length);
                                }
                                ContentStarted = true;
                                WriteElement(Reader, Writer, Style);
                            }
                            break;
                        case XmlNodeType.Comment:
                            if (Writer != null)
                            {
                                if (!ContentStarted && Style.Length > 0)
                                {
                                    Writer.WriteAttributeString("STYLE", Style.ToString());
                                }
                                Writer.WriteComment(Reader.Value);
                            }
                            ContentStarted = true;
                            break;
                        case XmlNodeType.CDATA:
                        case XmlNodeType.Text:
                        case XmlNodeType.SignificantWhitespace:
                            if (Writer != null)
                            {
                                if (!ContentStarted && Style.Length > 0)
                                {
                                    Writer.WriteAttributeString("STYLE", Style.ToString());
                                }
                                Writer.WriteString(Reader.Value);
                            }
                            ContentStarted = true;
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Converts an element notation of complex property into
        /// </summary>
        /// <param name="Reader">
        /// On entry this XmlTextReader must be on Element start tag;
        /// on exit - on EndElement tag.
        /// </param>
        /// <param name="Style">
        /// StringBuilder containing a value for STYLE attribute.
        /// </param>
        private static void AppendProp(XmlTextReader Reader, StringBuilder Style)
        {
            if (Style != null && Reader.Name.EndsWith(".TextDecorations"))
                Style.Append("text-decoration:underline;");

            // Skip the element representing the complex property
            ConvertElement(Reader, /*htmlWriter:*/null, /*inlineStyle:*/null);
        }

        /// <summary>
        /// Converts a xaml element into an appropriate html element.
        /// </summary>
        /// <param name="Reader">
        /// On entry this XmlTextReader must be on Element start tag;
        /// on exit - on EndElement tag.
        /// </param>
        /// <param name="Writer">
        /// May be null, in which case we are skipping xaml content
        /// without producing any html output
        /// </param>
        /// <param name="Style">
        /// StringBuilder used for collecting css properties for inline STYLE attributes on every level.
        /// </param>
        private static void WriteElement(XmlTextReader Reader, XmlTextWriter Writer, StringBuilder Style)
        {
            // Skipping mode; recurse into the xaml element without any output
            if (Writer == null)
                ConvertElement(Reader, /*htmlWriter:*/null, null);
            else
            {
                string htmlElementName = null;

                switch (Reader.Name)
                {
                    case "Run" :
                    case "Span":
                        htmlElementName = "SPAN";
                        break;
                    case "InlineUIContainer":
                        htmlElementName = "SPAN";
                        break;
                    case "Bold":
                        htmlElementName = "B";
                        break;
                    case "Italic" :
                        htmlElementName = "I";
                        break;
                    case "Paragraph" :
                        htmlElementName = "P";
                        break;
                    case "BlockUIContainer":
                        htmlElementName = "DIV";
                        break;
                    case "Section":
                        htmlElementName = "DIV";
                        break;
                    case "Table":
                        htmlElementName = "TABLE";
                        break;
                    case "TableColumn":
                        htmlElementName = "COL";
                        break;
                    case "TableRowGroup" :
                        htmlElementName = "TBODY";
                        break;
                    case "TableRow" :
                        htmlElementName = "TR";
                        break;
                    case "TableCell" :
                        htmlElementName = "TD";
                        break;
                    case "List" :
                        string marker = Reader.GetAttribute("MarkerStyle");
                        if (marker == null || marker == "None" || marker == "Disc" || marker == "Circle" || marker == "Square" || marker == "Box")
                        {
                            htmlElementName = "UL";
                        }
                        else
                        {
                            htmlElementName = "OL";
                        }
                        break;
                    case "ListItem" :
                        htmlElementName = "LI";
                        break;
                    default :
                        htmlElementName = null; // Ignore the element
                        break;
                }

                if (Writer != null && htmlElementName != null)
                {
                    Writer.WriteStartElement(htmlElementName);

                    ConvertFormatProps(Reader, Writer, Style);

                    ConvertElement(Reader, Writer, Style);

                    Writer.WriteEndElement();
                }
                else
                {
                    // Skip this unrecognized xaml element
                    ConvertElement(Reader, /*htmlWriter:*/null, null);
                }
            }
        }

        // Reader advance helpers
        // ----------------------
                 
        /// <summary>
        /// Reads several items from xamlReader skipping all non-significant stuff.
        /// </summary>
        /// <param name="Reader">
        /// XmlTextReader from tokens are being read.
        /// </param>
        /// <returns>
        /// True if new token is available; false if end of stream reached.
        /// </returns>
        private static bool AdvanceSegment(XmlReader Reader)
        {
            while (Reader.Read())
            {
                switch (Reader.NodeType)
                {
                    case XmlNodeType.Element: 
                    case XmlNodeType.EndElement:
                    case XmlNodeType.None:
                    case XmlNodeType.CDATA:
                    case XmlNodeType.Text:
                    case XmlNodeType.SignificantWhitespace:
                        return true;

                    case XmlNodeType.Whitespace:
                        if (Reader.XmlSpace == XmlSpace.Preserve)
                        {
                            return true;
                        }
                        // ignore insignificant whitespace
                        break;

                    case XmlNodeType.EndEntity:
                    case XmlNodeType.EntityReference:
                        //  Implement entity reading
                        //xamlReader.ResolveEntity();
                        //xamlReader.Read();
                        //ReadChildNodes( parent, parentBaseUri, xamlReader, positionInfo);
                        break; // for now we ignore entities as insignificant stuff

                    case XmlNodeType.Comment:
                        return true;
                    case XmlNodeType.ProcessingInstruction:
                    case XmlNodeType.DocumentType:
                    case XmlNodeType.XmlDeclaration:
                    default:
                        // Ignorable stuff
                        break;
                }
            }
            return false;
        }

    }
}
