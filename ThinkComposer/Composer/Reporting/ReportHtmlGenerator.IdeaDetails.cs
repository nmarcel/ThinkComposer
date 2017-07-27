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
// 2012.01.29 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
using Instrumind.ThinkComposer.Model.InformationModel;

/// Provides features for report generation.
namespace Instrumind.ThinkComposer.Composer.Reporting
{
    /// <summary>
    /// Generates comprehensive HTML reports from Compositions.
    /// </summary>
    public partial class ReportHtmlGenerator
    {
        public const double INTER_DETAILS_FILLING = 12;
        public const double INTER_SEGMENTS_FILLING = 8;
        public const int MAX_TRANSPOSED_COLUMNS = 7; // The first is for the field-name.

        // -----------------------------------------------------------------------------------------
        public void CreateIdeaDetails(IEnumerable<ContainedDetail> Details)
        {
            foreach (var Detail in Details)
                CreateDetailSubsection(Detail);
        }

        // -----------------------------------------------------------------------------------------
        public void CreateDetailSubsection(ContainedDetail Detail)
        {
            var Title = (Detail is Link ? Link.__ClassDefinitor.Name : Detail.Kind.Name) + ": " + Detail.Designation.NameCaption
                        + (Detail is Table ? " [Structure: " + Detail.Designation.Definitor.Name.RemoveNewLines() + "]" : "");

            this.CreateTextLabel("detail-title", Title);

            var Representator = Detail.OwnerIdea.VisualRepresentators.FirstOrDefault();
            var Look = (Representator != null
                        ? Detail.OwnerIdea.GetDetailLook(Detail.Designation, Representator.MainSymbol)
                        : null);

            if (Detail is Link && this.Configuration.CompositeIdea_DetailsIncludeLinksTarget
                && !((Link)Detail).Target.ToStringAlways().IsAbsent())
                this.CreateDetailContent((Link)Detail);
            else
                if (Detail is Attachment && this.Configuration.CompositeIdea_DetailsIncludeAttachmentsContent
                    && ((Attachment)Detail).Content.NullDefault(new byte[0]).Length > 0 )
                    this.CreateDetailContent((Attachment)Detail);
                else
                    if (Detail is Table && ((Table)Detail).Count > 0
                        && this.Configuration.CompositeIdea_DetailsIncludeTablesData)
                        this.CreateDetailContent((Table)Detail, (TableAppearance)Look);
        }

        // -----------------------------------------------------------------------------------------
        public void CreateDetailContent(Link Detail)
        {
            var Content = Detail.Target.ToStringAlways().ToHtmlEncoded();
            Content = "<a href='" + Content + "' target='_blank'>" + Content + "</a>";

            this.PageWrite("<div id='detail-content'>");
                this.IncreaseIndent();
                this.PageWrite(Content);
                this.DecreaseIndent();
            this.PageWrite("</div>");
        }

        // -----------------------------------------------------------------------------------------
        public void CreateDetailContent(Attachment Detail)
        {
            var Content = "";

            if (Detail.MimeType.StartsWith("image/"))
            {
                var Picture = Detail.Content.ToImageSource();

                if (Picture != null)
                {
                    var PictureRef = this.CreateImage(Picture, Detail.OwnerIdea,
                                                      Detail.Designation.TechName + "_" + Detail.OwnerIdea.Details.IndexOf(Detail).ToString());

                    Content = "<img Alt='" + Detail.Designation.NameCaption.ToHtmlEncoded() + "' Src='" + PictureRef + "' />";
                }
            }
            else
            {
                string Text = VisualSymbol.DETAIL_INDIC_EXPAND;

                if (Detail.MimeType.StartsWith("text/"))
                    Text = Detail.Content.BytesToString();

                // PENDING: Extract attachment as file and link it

                Content = Text.ToHtmlEncoded();
            }

            this.PageWrite("<div id='detail-content'>");
                this.IncreaseIndent();
                this.PageWrite(Content);
                this.DecreaseIndent();
            this.PageWrite("</div>");
        }

        // -----------------------------------------------------------------------------------------
        public void CreateDetailContent(Table Detail, TableAppearance Look)
        {
            if (Look.Layout == ETableLayoutStyle.Transposed)    // Mostly used by custom-fields
                CreateDetailContentTableTransposed(Detail, Look);
            else
                CreateDetailContentTableConventional(Detail, Look);
        }

        // -----------------------------------------------------------------------------------------
        public void CreateDetailContentTableTransposed(Table Detail, TableAppearance Look)
        {
            this.PageWrite("<table class='tbl_dettbl_records'>");
                this.IncreaseIndent();

                for (int RecIndex = 0; RecIndex <= Detail.Count; RecIndex++)
                    this.PageWrite("<col style='width: 150px;' />");

                this.PageWrite("<tbody>");
                    this.IncreaseIndent();

                    foreach (var ColDef in Detail.Definition.FieldDefinitions)
                    {
                        this.PageWrite("<tr>");
                            this.IncreaseIndent();
                            
                            this.PageWrite("<td>" + ColDef.NameCaption.ToHtmlEncoded() + "</td>");

                            for (int ColIndex = 0; ColIndex < Detail.Count; ColIndex++)
                            {
                                var Record = Detail[ColIndex];
                                var Value = (ColDef.FieldType.IsEqual(DataType.DataTypePicture)
                                             ? GetStoredPictureAsHtmlImageRef(Record.GetStoredValue(ColDef) as ImageAssignment, Detail.OwnerIdea, Record)
                                             : Record.GetStoredValueForDisplay(ColDef).ToHtmlEncoded());
                                this.PageWrite("<td>" + Value + "</td>");
                            }

                            this.DecreaseIndent();
                        this.PageWrite("</tr>");
                    }

                    this.DecreaseIndent();
                this.PageWrite("</tbody>");

                this.DecreaseIndent();
            this.PageWrite("</table>");
        }

        // -----------------------------------------------------------------------------------------
        public void CreateDetailContentTableConventional(Table Detail, TableAppearance Look)
        {
            var TableColDefs = Detail.Definition.FieldDefinitions
                                .Select(def => Capsule.Create(def.TechName, def.NameCaption.ToHtmlEncoded(),
                                                              def.GetEstimatedColumnPixelsWidth(5)));

            var TotalWidth = TableColDefs.Sum(def => def.Value2);
            TableColDefs.ForEach(def => def.Value2 = ((def.Value2 * 100) / TotalWidth));

            var TableRowVals = Detail
                    .Select(rec => Detail.Definition.FieldDefinitions
                                    .Select(fd => (fd.FieldType.IsEqual(DataType.DataTypePicture)
                                                   ? GetStoredPictureAsHtmlImageRef(rec.GetStoredValue(fd) as ImageAssignment, Detail.OwnerIdea, rec)
                                                   : rec.GetStoredValueForDisplay(fd).ToHtmlEncoded())));

            this.PageWriteTable("tbl_dettbl_records", TableRowVals, TableColDefs.ToArray());
        }

        // -----------------------------------------------------------------------------------------
        public string GetStoredPictureAsHtmlImageRef(ImageAssignment Picture, IRecognizableComposite RecordsOwner, TableRecord RecordObject)
        {
            var Result = "";

            if (Picture != null)
            {
                var PictureImage = this.CurrentWorker.AtOriginalThreadGetFrozen(Picture.Image);
                if (PictureImage != null)
                {
                    var PictureRef = this.CreateImage(PictureImage, RecordsOwner, RecordObject,
                                                      RecordObject.OwnerTable.Designation.TechName + "_" +
                                                      RecordObject.OwnerTable.OwnerIdea.Details.IndexOf(RecordObject.OwnerTable).ToString() + "_" +
                                                      RecordObject.Index.ToString());

                    Result = "<img Alt='' Src='" + PictureRef.ToHtmlEncoded() +
                                "' style='max-width:" + ReportConfiguration.TBLF_PICT_MAX_WIDTH.ToString() +
                                "px; max-height:" + ReportConfiguration.TBLF_PICT_MAX_HEIGHT.ToString() + "px;' />"; 
                }
            }

            return Result;
        }

        // -----------------------------------------------------------------------------------------
        /* Cancelled: Text continaing tables (as tab-separated, comma-separated, etc.)
         *            should be imported/pasted as Table detail, not as plain text.
        public void CreateDetailContentTableFromText(string Detail)
        {
        } */

        // -----------------------------------------------------------------------------------------
    }
}
