// -------------------------------------------------------------------------------------------
// Instrumind ThinkComposer
//
// Copyright (C) 2011-2015 Néstor Marcel Sánchez Ahumada.
// http://thinkcomposer.codeplex.com
//
// This file is part of ThinkComposer, which is free software licensed under the GNU General Public License.
// It is provided without any warranty. You should find a copy of the license in the root directory of this software product.
// -------------------------------------------------------------------------------------------
//
// Project: Instrumind ThinkComposer v1.0
// File   : ReportHtmlGenerator.cs
// Object : Instrumind.ThinkComposer.Composer.Reporting.ReportHtmlGenerator (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2012.09.26 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Text;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.Visualization;
using Instrumind.Common.Visualization.Widgets;

using Instrumind.ThinkComposer.ApplicationProduct;
using Instrumind.ThinkComposer.MetaModel.Configurations;
using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.MetaModel.InformationMetaModel;
using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;
using Instrumind.ThinkComposer.Model;
using Instrumind.ThinkComposer.Model.GraphModel;
using Instrumind.ThinkComposer.Model.VisualModel;

/// Provides features for report generation.
namespace Instrumind.ThinkComposer.Composer.Reporting
{
    /// <summary>
    /// Generates comprehensive HTML reports from Compositions.
    /// </summary>
    public partial class ReportHtmlGenerator
    {
        public const int HTML_STD_PAGE_WIDTH = 1000;

        // -----------------------------------------------------------------------------------------
        public void CreateHtmlStyleSheet()
        {
            var StyleSheet = new StringBuilder();

            StyleSheet.AppendLine(
            @"* { 
            font-size: 100%; 
            font-family: Arial; 
            }");

            StyleSheet.AppendLine("h1, h2, h3, h4, h5, h6 {float: left; width: " + HTML_STD_PAGE_WIDTH.ToString() + "px;}");

            StyleSheet.AppendLine("h1 {" + GetCssProperties(this.Configuration.FmtMainTitle) + "}");

            StyleSheet.AppendLine("h2 {" + GetCssProperties(this.Configuration.FmtMainSubtitle) + "}");

            StyleSheet.AppendLine("td.header-left {text-align: left;}");
            StyleSheet.AppendLine("td.header-center {text-align: center;}");
            StyleSheet.AppendLine("td.header-right {text-align: right;}");
            StyleSheet.AppendLine("td.header-left, td.header-center, td.header-right {" + GetCssProperties(this.Configuration.FmtPageHeaderLabels, false) + "}");

            StyleSheet.AppendLine("td.footer-left {float: left; text-align: left;}");
            StyleSheet.AppendLine("td.footer-center {overflow: hidden; text-align: center;}");
            StyleSheet.AppendLine("td.footer-right {float: right; text-align: right;}");
            StyleSheet.AppendLine("td.footer-left, td.footer-center, td.footer-right {" + GetCssProperties(this.Configuration.FmtPageFooterLabels, false) + "}");

            // StyleSheet.AppendLine("#main-title {" + GetCssProperties(this.Configuration.FmtMainTitle) + "}");       // Duplicated in "h1"
            // StyleSheet.AppendLine("#main-subtitle {" + GetCssProperties(this.Configuration.FmtMainSubtitle) + "}"); // Duplicated in "h2"

            StyleSheet.AppendLine(".extras {" + GetCssProperties(this.Configuration.FmtExtras) + "}");

            StyleSheet.AppendLine("#subject-title {float: left; width: " + HTML_STD_PAGE_WIDTH.ToString() + "px;" +
                                  GetCssProperties(this.Configuration.FmtSubjectTitle) + "}");
            StyleSheet.AppendLine("#section-title {float: left; width: " + HTML_STD_PAGE_WIDTH.ToString() + "px;" +
                                  GetCssProperties(this.Configuration.FmtSectionTitle) + "}");

            StyleSheet.AppendLine("#detail-title {" + GetCssProperties(this.Configuration.FmtCardFieldLabel) + "}");
            StyleSheet.AppendLine("#detail-caption {" + GetCssProperties(this.Configuration.FmtDetailFieldLabel) + "}");
            StyleSheet.AppendLine("#detail-content {" + GetCssProperties(this.Configuration.FmtDetailFieldValue) + "}");

            StyleSheet.AppendLine(
            @"table.tbl_card_props {
                width: " + HTML_STD_PAGE_WIDTH.ToString() + @"px;
                border-width: 1px;
                border-color: SlateGray;
                border-collapse: collapse;
            }
            table.tbl_card_props th {" + GetCssProperties(this.Configuration.FmtCardFieldLabel) + @"
                border-width: 1px;
                padding: 4px;
                border-style: solid;
                border-color: " + this.Configuration.FmtCardLinesForeground.ToHtmlColor() + @";
                background-color: " + this.Configuration.FmtFieldLabelBackground.ToHtmlColor() + @";
            }
            table.tbl_card_props td {" + GetCssProperties(this.Configuration.FmtCardFieldValue) + @"
                border-width: 1px;
                padding: 4px;
                border-style: solid;
                border-color: " + this.Configuration.FmtCardLinesForeground.ToHtmlColor() + @";
                background-color: " + this.Configuration.FmtFieldValueBackground.ToHtmlColor() + @";
            }");

            StyleSheet.AppendLine(
            @"table.tbl_list_objects {
                width: " + HTML_STD_PAGE_WIDTH.ToString() + @"px;
                border-width: 1px;
                border-color: SlateGray;
                border-collapse: collapse;
            }
            table.tbl_list_objects th {" + GetCssProperties(this.Configuration.FmtListFieldLabel) + @"
                border-width: 1px;
                padding: 4px;
                border-style: solid;
                border-color: " + this.Configuration.FmtListLinesForeground.ToHtmlColor() + @";
                background-color: " + this.Configuration.FmtFieldLabelBackground.ToHtmlColor() + @";
            }
            table.tbl_list_objects td {" + GetCssProperties(this.Configuration.FmtListFieldValue) + @"
                border-width: 1px;
                padding: 4px;
                border-style: solid;
                border-color: " + this.Configuration.FmtListRowLinesForeground.ToHtmlColor() + @";
                background-color: " + this.Configuration.FmtFieldValueBackground.ToHtmlColor() + @";
            }");

            // Notice the setting of "min-width" because columns are undetermined
            StyleSheet.AppendLine(
            @"table.tbl_dettbl_records {
                min-width: " + HTML_STD_PAGE_WIDTH.ToString() + @"px;
                border-width: 1px;
                border-color: SlateGray;
                border-collapse: collapse;
            }
            table.tbl_dettbl_records th {" + GetCssProperties(this.Configuration.FmtDetailFieldLabel) + @"
                border-width: 1px;
                padding: 4px;
                border-style: solid;
                border-color: " + this.Configuration.FmtListLinesForeground.ToHtmlColor() + @";
                background-color: " + this.Configuration.FmtFieldLabelBackground.ToHtmlColor() + @";
            }
            table.tbl_dettbl_records td {" + GetCssProperties(this.Configuration.FmtDetailFieldValue) + @"
                border-width: 1px;
                padding: 4px;
                border-style: solid;
                border-color: " + this.Configuration.FmtListRowLinesForeground.ToHtmlColor() + @";
                background-color: " + this.Configuration.FmtFieldValueBackground.ToHtmlColor() + @";
            }");

            StyleSheet.Replace('\'', '"');
            var Text = StyleSheet.ToString();

            if (!this.CreateUniqueRelativeLocation(this.SourceComposition, STYLE_SHEET_FILE))
                throw new UsageAnomaly("Generating same CSS document again for: " + this.SourceComposition.Name + " [" + STYLE_SHEET_FILE + "]");

            var Location = this.GetPhysicalLocationOf(this.SourceComposition, STYLE_SHEET_FILE);

            this.PrepareToWrite(Location);
            General.StringToFile(Location, Text);
        }

        // -----------------------------------------------------------------------------------------
        public string GetCssProperties(TextFormat Source, bool IncludeAlign = true)
        {
            var Result = "color: " + Source.ForegroundBrush.ToHtmlColor() + ";" + Environment.NewLine + " ".Replicate(12) +
                         "font-family: " + Source.FontFamilyName + ";" + Environment.NewLine + " ".Replicate(12) +
                         "font-size: " + Source.FontSize.ToString() + "px;" + Environment.NewLine + " ".Replicate(12) +
                         "font-style: " + (Source.IsItalic ? "italic" : "normal") + ";" + Environment.NewLine + " ".Replicate(12) +
                         "font-weight: " + (Source.IsBold ? "bold" : "normal") + ";" + Environment.NewLine + " ".Replicate(12) +
                         "vertical-align: top;" + Environment.NewLine + " ".Replicate(12) +
                         (IncludeAlign
                          ? ("text-align: " + Source.Alignment.ToHtmlTextAlign() + ";" + Environment.NewLine + " ".Replicate(12))
                          : "") +
                         "text-decoration: " + (Source.IsStrikethrough
                                                ? "line-through"
                                                : (Source.IsUnderline
                                                   ? "underline"
                                                   : "none")) + ";";
            return Result;
        }
    }
}
