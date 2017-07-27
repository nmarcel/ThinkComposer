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
// File   : ReportStandardGenerator.cs
// Object : Instrumind.ThinkComposer.Composer.Reporting.ReportStandardGenerator (Class)
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
    /// Generates comprehensive Standard reports from Compositions.
    /// </summary>
    public partial class ReportStandardGenerator
    {
        public const double INTER_DETAILS_FILLING = 12;
        public const double INTER_SEGMENTS_FILLING = 8;
        public const int MAX_TRANSPOSED_COLUMNS = 7; // The first is for the field-name.

        // -----------------------------------------------------------------------------------------
        public void CreateIdeaDetails(ReportStandardPagesMaker PagesMaker, double LocalNestingMargin,
                                      IEnumerable<ContainedDetail> Details)
        {
            var DetailIndex = 0;
            foreach (var Detail in Details)
            {
                /* var DetailPanel = new StackPanel();

                var DetailHeader = new Grid();
                DetailHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.1, GridUnitType.Star) });
                DetailHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.15, GridUnitType.Star) });
                DetailHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.15, GridUnitType.Star) });
                DetailHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.4, GridUnitType.Star) });
                DetailHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.2, GridUnitType.Star) });

                var Expositor = CreateContentCardPropertyExpositor("Type", Detail.KindName,
                                                                   this.Configuration.FmtCardFieldLabel, this.Configuration.FmtFieldLabelBackground,
                                                                   this.Configuration.FmtCardFieldValue, this.Configuration.FmtFieldValueBackground);
                DetailHeader.Children.Add(Expositor);

                Expositor = CreateContentCardPropertyExpositor(FormalPresentationElement.__Name.Name, Detail.Designation.Name,
                                                               this.Configuration.FmtCardFieldLabel, this.Configuration.FmtFieldLabelBackground,
                                                               this.Configuration.FmtCardFieldValue, this.Configuration.FmtFieldValueBackground);
                Grid.SetColumn(Expositor, 1);
                DetailHeader.Children.Add(Expositor);

                Expositor = CreateContentCardPropertyExpositor(FormalPresentationElement.__TechName.Name, Detail.Designation.TechName,
                                                               this.Configuration.FmtCardFieldLabel, this.Configuration.FmtFieldLabelBackground,
                                                               this.Configuration.FmtCardFieldValue, this.Configuration.FmtFieldValueBackground);
                Grid.SetColumn(Expositor, 2);
                DetailHeader.Children.Add(Expositor);

                Expositor = CreateContentCardPropertyExpositor(FormalPresentationElement.__Summary.Name, Detail.Designation.Summary,
                                                               this.Configuration.FmtCardFieldLabel, this.Configuration.FmtFieldLabelBackground,
                                                               this.Configuration.FmtCardFieldValue, this.Configuration.FmtFieldValueBackground);
                Grid.SetColumn(Expositor, 3);
                DetailHeader.Children.Add(Expositor);

                Expositor = CreateContentCardPropertyExpositor("Definition", Detail.Designation.Definitor.Name,
                                                               this.Configuration.FmtCardFieldLabel, this.Configuration.FmtFieldLabelBackground,
                                                               this.Configuration.FmtCardFieldValue, this.Configuration.FmtFieldValueBackground);
                Grid.SetColumn(Expositor, 4);
                DetailHeader.Children.Add(Expositor);

                DetailPanel.Children.Add(DetailHeader);
                DetailPanel.Children.Add(CreateDetailBody(Detail));

                PagesMaker.AppendContent(DetailPanel, true); */

                if (DetailIndex > 0)
                {
                    var Filler = new Border();
                    Filler.Height = INTER_DETAILS_FILLING;
                    PagesMaker.AppendContent(Filler);
                }

                CreateDetailSubsection(PagesMaker, LocalNestingMargin, Detail);   // Detail is directly generated into pages-maker.
                DetailIndex++;
            }
        }

        // -----------------------------------------------------------------------------------------
        public void CreateDetailSubsection(ReportStandardPagesMaker PagesMaker, double LocalNestingMargin, ContainedDetail Detail)
        {
            var Result = new StackPanel();

            var TitleBorder = new Border();
            TitleBorder.Margin = new Thickness(0, 0, 0, 0);
            TitleBorder.BorderBrush = this.Configuration.FmtCardLinesForeground;
            TitleBorder.BorderThickness = new Thickness(LINES_CARD_THICKNESS);
            TitleBorder.VerticalAlignment = VerticalAlignment.Stretch;
            var Title = (Detail is Link ? Link.__ClassDefinitor.Name : Detail.Kind.Name) + ": " + Detail.Designation.NameCaption
                        + (Detail is Table ? " [Structure: " + Detail.Designation.Definitor.Name.RemoveNewLines() + "]" : "");
            TitleBorder.Child = CreateText(Title,
                                           this.Configuration.FmtCardFieldLabel, HorizontalAlignment.Stretch, VerticalAlignment.Top,
                                           this.Configuration.FmtFieldLabelBackground);

            var GroupKey = "Detail_" + (new Random()).Next().ToString();
            PagesMaker.AppendContent(TitleBorder, false, GroupKey);

            FrameworkElement Content = null;
            var Representator = Detail.OwnerIdea.VisualRepresentators.FirstOrDefault();
            var Look = (Representator != null
                        ? Detail.OwnerIdea.GetDetailLook(Detail.Designation, Representator.MainSymbol)
                        : null);

            if (Detail is Link && this.Configuration.CompositeIdea_DetailsIncludeLinksTarget
                && !((Link)Detail).Target.ToStringAlways().IsAbsent())
                Content = CreateDetailContent((Link)Detail);
            else
                if (Detail is Attachment && this.Configuration.CompositeIdea_DetailsIncludeAttachmentsContent
                    && ((Attachment)Detail).Content.NullDefault(new byte[0]).Length > 0 )
                    Content = CreateDetailContent((Attachment)Detail);
                else
                    if (Detail is Table && ((Table)Detail).Count > 0
                        && this.Configuration.CompositeIdea_DetailsIncludeTablesData)
                        // Notice that no Content is returned (records appended to pages-maker inside)
                        CreateDetailContent(PagesMaker, LocalNestingMargin, GroupKey, (Table)Detail, (TableAppearance)Look);

            if (Content != null)
            {
                var Container = new Border();
                Container.BorderThickness = new Thickness(LINES_CARD_THICKNESS);
                Container.BorderBrush = this.Configuration.FmtCardLinesForeground;
                Container.Child = Content;

                PagesMaker.AppendContent(Container, false, GroupKey);
            }
        }

        // -----------------------------------------------------------------------------------------
        public FrameworkElement CreateDetailContent(Link Detail)
        {
            var Result = new TextBlock();
            Result.Text = Detail.Target.ToStringAlways();
            return Result;
        }

        // -----------------------------------------------------------------------------------------
        public FrameworkElement CreateDetailContent(Attachment Detail)
        {
            FrameworkElement Result = null;

            if (Detail.MimeType.StartsWith("image/"))
            {
                var PictureLabel = new Border();
                var PictureValue = Detail.Content.ToImageSource();
                var SquareSize = Math.Min(PictureValue.Width, PictureValue.Height);
                var Picture = new Image();
                Picture.MaxWidth = SquareSize;
                Picture.MaxHeight = SquareSize;
                Picture.Source = PictureValue;
                Picture.Stretch = Stretch.Uniform;
                PictureLabel.Child = Picture;

                Result = PictureLabel;
            }
            else
            {
                string Text = VisualSymbol.DETAIL_INDIC_EXPAND;

                if (Detail.MimeType.StartsWith("text/"))
                    Text = Detail.Content.BytesToString();

                // PENDING: Consider to return null and directly append text lines to pages-maker (as with table records).
                Result = CreateText(Text, this.Configuration.FmtDetailFieldValue);
            }

            return Result;
        }

        // -----------------------------------------------------------------------------------------
        public void CreateDetailContent(ReportStandardPagesMaker PagesMaker, double LocalNestingMargin, string GroupKey,
                                        Table Detail, TableAppearance Look)
        {
            if (Look.Layout == ETableLayoutStyle.Transposed)    // Mostly used by custom-fields
                CreateDetailContentTableTransposed(PagesMaker, LocalNestingMargin, GroupKey, Detail, Look);
            else
                CreateDetailContentTableConventional(PagesMaker, LocalNestingMargin, GroupKey, Detail, Look);
        }

        // -----------------------------------------------------------------------------------------
        public void CreateDetailContentTableTransposed(ReportStandardPagesMaker PagesMaker, double LocalNestingMargin, string GroupKey,
                                                       Table Detail, TableAppearance Look)
        {
            var MaxColumns = Math.Min(Detail.Count + 1, MAX_TRANSPOSED_COLUMNS);
            var SegmentsCount = (int)Math.Ceiling((double)Detail.Count / (double)(MaxColumns - 1));
            var SegmentKey = GroupKey;
            var FieldIndex = 0;
            var SegmentIndex = 0;
            var ColumnIndex = 0;
            var RecordIndex = 0;
            var RecordStart = 0;
            var ColumnWidth = ((this.WorkingPageContentWidth - LocalNestingMargin) / (double)MaxColumns);

            while (SegmentIndex < SegmentsCount)
            {
                if (SegmentIndex > 0)
                {
                    var Filler = new Border();
                    Filler.Height = INTER_SEGMENTS_FILLING;
                    PagesMaker.AppendContent(Filler);
                }

                while (FieldIndex < Detail.Definition.FieldDefinitions.Count)
                {
                    var RowPanel = new StackPanel();
                    RowPanel.Orientation = Orientation.Horizontal;

                    var FieldDef = Detail.Definition.FieldDefinitions[FieldIndex];
                    var LabelCell = CreateListCell(FieldDef.NameCaption, ColumnWidth,
                                                   HorizontalAlignment.Left, VerticalAlignment.Stretch,
                                                   this.Configuration.FmtFieldLabelBackground, this.Configuration.FmtListRowLinesForeground,
                                                   this.Configuration.FmtDetailFieldLabel, LINES_LISTROW_THICKNESS);
                    RowPanel.Children.Add(LabelCell);

                    RecordIndex = RecordStart;

                    var ColumnsLimit = Math.Min(Detail.Count, MaxColumns - 1);
                    while (ColumnIndex < ColumnsLimit && RecordIndex < Detail.Count)
                    {
                        var Value = (FieldDef.FieldType.IsEqual(DataType.DataTypePicture)
                                     ? (object)(Detail[RecordIndex].GetStoredValue(FieldDef) as ImageAssignment).Get(ias => ias.Image)
                                     : Detail[RecordIndex].GetStoredValueForDisplay(FieldDef));

                        // NOTE: As when drawing transposed records on symbols, it is better to let the values as left-aligned.
                        var ValueCell = CreateListCell(Value, ColumnWidth,
                                                       HorizontalAlignment.Left, VerticalAlignment.Stretch,
                                                       this.Configuration.FmtFieldValueBackground, this.Configuration.FmtListRowLinesForeground,
                                                       this.Configuration.FmtDetailFieldLabel, LINES_LISTROW_THICKNESS);
                        RowPanel.Children.Add(ValueCell);

                        RecordIndex++;
                        ColumnIndex++;
                    }

                    PagesMaker.AppendContent(RowPanel, false, SegmentKey);
                    SegmentKey = GroupKey + "_" + SegmentIndex.ToString();
                    
                    ColumnIndex = 0;
                    FieldIndex++;
                }

                RecordStart += (MaxColumns - 1);
                FieldIndex = 0;
                SegmentIndex++;
            }
        }

        // -----------------------------------------------------------------------------------------
        public void CreateDetailContentTableConventional(ReportStandardPagesMaker PagesMaker, double LocalNestingMargin, string GroupKey,
                                                         Table Detail, TableAppearance Look)
        {
            var AvailableWidth = this.WorkingPageContentWidth - LocalNestingMargin;

            // Determine total column/field-def header rows and their widths needed per row.
            var MultiLineRows = new List<List<Capsule<FieldDefinition, double>>>();
            var NewMultiLineCols = new List<Capsule<FieldDefinition, double>>();
            MultiLineRows.Add(NewMultiLineCols);
            var ConsumedWidth = 0.0;
            var WidthAdjustFactor = 1.0;

            foreach (var FieldDef in Detail.Definition.FieldDefinitions)
            {
                var ColumnWidth = FieldDef.GetEstimatedColumnPixelsWidth(5);
                ConsumedWidth += ColumnWidth;

                if (ConsumedWidth > AvailableWidth)
                {
                    WidthAdjustFactor = AvailableWidth / (ConsumedWidth - ColumnWidth);
                    foreach (var RowCols in NewMultiLineCols)
                        RowCols.Value1 = RowCols.Value1 * WidthAdjustFactor;

                    NewMultiLineCols = new List<Capsule<FieldDefinition, double>>();
                    MultiLineRows.Add(NewMultiLineCols);
                    ConsumedWidth = ColumnWidth;
                }

                NewMultiLineCols.Add(Capsule.Create(FieldDef, ColumnWidth));
            }

            WidthAdjustFactor = AvailableWidth / ConsumedWidth;
            foreach (var RowCols in NewMultiLineCols)
                RowCols.Value1 = RowCols.Value1 * WidthAdjustFactor;

            // Create list-header panel creator (to be re-used on each page-break)
            Func<FrameworkElement> ListHeaderCreator =
                (() =>
                    {
                        var HeaderPanel = new StackPanel();

                        foreach (var RowCols in MultiLineRows)
                        {
                            var RowPanel = new StackPanel();
                            RowPanel.Orientation = Orientation.Horizontal;

                            foreach(var FieldColumn in RowCols)
                            {
                                var LabelCell = CreateListCell(FieldColumn.Value0.NameCaption, FieldColumn.Value1,
                                                               HorizontalAlignment.Left, VerticalAlignment.Stretch,
                                                               this.Configuration.FmtFieldLabelBackground, this.Configuration.FmtListRowLinesForeground,
                                                               this.Configuration.FmtDetailFieldLabel, LINES_LISTROW_THICKNESS);

                                RowPanel.Children.Add(LabelCell);
                            }

                            HeaderPanel.Children.Add(RowPanel);
                        }

                        return HeaderPanel;
                    });

            PagesMaker.PageBreakStartCreator = ListHeaderCreator;

            // Start creating the main header
            var MainHeaderPanel = ListHeaderCreator();
            PagesMaker.AppendContent(MainHeaderPanel, false, GroupKey);

            // Generate list-rows
            foreach (var Record in Detail)
            {
                var RecordRowsPanel = new StackPanel();

                if (MultiLineRows.Count > 1)
                {
                    var LineSeparator = new Border { Height = 0.5, Background = Brushes.DimGray };
                    RecordRowsPanel.Children.Add(LineSeparator);
                }

                foreach (var RowCols in MultiLineRows)
                {
                    var RowPanel = new StackPanel();
                    RowPanel.Orientation = Orientation.Horizontal;

                    foreach (var FieldColumn in RowCols)
                    {
                        var Alignment = (FieldColumn.Value0.FieldType is BasicDataType
                                         ? ((BasicDataType)FieldColumn.Value0.FieldType).DisplayAlignment
                                         : TextAlignment.Left).ToHorizontalAlignment();

                        var Value = (FieldColumn.Value0.FieldType.IsEqual(DataType.DataTypePicture)
                                     ? (object)(Record.GetStoredValue(FieldColumn.Value0) as ImageAssignment).Get(ias => ias.Image)
                                     : Record.GetStoredValueForDisplay(FieldColumn.Value0));

                        var ValueCell = CreateListCell(Value, FieldColumn.Value1,
                                                       Alignment, VerticalAlignment.Stretch,
                                                       this.Configuration.FmtFieldValueBackground, this.Configuration.FmtListRowLinesForeground,
                                                       this.Configuration.FmtDetailFieldLabel, LINES_LISTROW_THICKNESS);

                        RowPanel.Children.Add(ValueCell);
                    }

                    RecordRowsPanel.Children.Add(RowPanel);
                }

                PagesMaker.AppendContent(RecordRowsPanel, false, GroupKey);

                /*? Separation by little filler
                if (MultiLineRows.Count > 1)
                {
                    var Filler = new Border();
                    Filler.Height = INTER_SEGMENTS_FILLING / 4.0;
                    PagesMaker.AppendContent(Filler);
                } */
            }

            // Reset page-break header creator
            PagesMaker.PageBreakStartCreator = null;
        }

        // -----------------------------------------------------------------------------------------
    }
}
