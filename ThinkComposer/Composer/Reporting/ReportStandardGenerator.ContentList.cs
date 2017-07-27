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
    /// Generates comprehensive Standard reports from Compositions.
    /// </summary>
    public partial class ReportStandardGenerator
    {
        public void CreateContentList(ReportStandardPagesMaker PagesMaker, string GroupKey,
                                      IEnumerable<IMModelClass> Source, DisplayList DispList,
                                      string DefinitorTechName, string DefinitorName,
                                      params Tuple<string, string, double, Func<IMModelClass, object>>[] ExtraColDefs)
        {
            var ColumnHeaderDefs = new Dictionary<string, Capsule<string, double, Func<IMModelClass, object>>>();

            if (DispList.PropName)
                ColumnHeaderDefs.Add(FormalElement.__Name.TechName, Capsule.Create(FormalElement.__Name.Name.RemoveNewLines(), 0.25, (Func<IMModelClass, object>)null));

            if (DispList.PropTechName)
                ColumnHeaderDefs.Add(FormalElement.__TechName.TechName, Capsule.Create(FormalElement.__TechName.Name, 0.2, (Func<IMModelClass, object>)null));

            if (DispList.PropSummary)
                ColumnHeaderDefs.Add(FormalElement.__Summary.TechName,
                                     Capsule.Create(FormalElement.__Summary.Name, (!DispList.PropTechName ? 0.5 : 0.3), (Func<IMModelClass, object>)null));

            if (DispList.PropPictogram)
                ColumnHeaderDefs.Add(FormalPresentationElement.__Pictogram.TechName,
                                     Capsule.Create("Pict." /* FormalPresentationElement.__Pictogram.Name*/, 0.05, (Func<IMModelClass, object>)null));

            if (DispList.Definitor && !(DefinitorTechName.IsAbsent() || DefinitorName.IsAbsent()))
                ColumnHeaderDefs.Add(DefinitorTechName, Capsule.Create(DefinitorName, (!DispList.PropPictogram ? 0.28 : 0.2), (Func<IMModelClass, object>)null));

            if (ExtraColDefs != null)
                foreach(var ExtraColDef in ExtraColDefs)
                    ColumnHeaderDefs.Add(ExtraColDef.Item1, Capsule.Create(ExtraColDef.Item2, ExtraColDef.Item3, ExtraColDef.Item4));

            CreateListOfModelObjects(PagesMaker, GroupKey, Source, ColumnHeaderDefs);

            PagesMaker.PageBreakStartCreator = null;
        }

        public void CreateListOfModelObjects(ReportStandardPagesMaker PagesMaker, string GroupKey, IEnumerable<IMModelClass> Source,
                                             Dictionary<string, Capsule<string, double, Func<IMModelClass, object>>> ColumnHeaderDefs)
        {
            // Create initial Header
            var Header = CreateListHeader(PagesMaker, ColumnHeaderDefs,
                                          this.Configuration.FmtListFieldLabel,
                                          this.Configuration.FmtFieldLabelBackground,
                                          this.Configuration.FmtListLinesForeground);

            PagesMaker.PageBreakStartCreator = (() => CreateListHeader(PagesMaker, ColumnHeaderDefs,
                                                                       this.Configuration.FmtListFieldLabel,
                                                                       this.Configuration.FmtFieldLabelBackground,
                                                                       this.Configuration.FmtListLinesForeground).Item1);
            PagesMaker.AppendContent(Header.Item1, false, GroupKey);

            var ColumnValueDefs = Header.Item2;
            var VerticalAlign = VerticalAlignment.Stretch;
            var HorizontalAlign = HorizontalAlignment.Stretch;

            // Travel Records...
            Source.ForEachIndexing((record, index) =>
            {
                var RecordFrame = new StackPanel(); // Grid is too expensive for each record
                RecordFrame.Orientation = Orientation.Horizontal;

                // Travel Colum-Defs...
                foreach (var ColValueDef in ColumnValueDefs)
                {
                    var ColDef = record.ClassDefinition.GetPropertyDef(ColValueDef.Key, false);
                    var MaxValueWidth = double.PositiveInfinity;
                    var MaxValueHeight = double.PositiveInfinity;
                    object Value = null;

                    if (ColDef != null && record is IRecognizableElement
                        && ColDef.TechName == FormalPresentationElement.__Pictogram.TechName)  // Used as "Formal..." but could be a "Simple..."
                        Value = this.CurrentWorker.AtOriginalThreadInvoke(
                            () =>
                            {
                                var Picture = ((IRecognizableElement)record).Pictogram;
                                if (Picture == null)
                                    if (record is IdeaDefinition)
                                        Picture = ((IdeaDefinition)record).Pictogram;
                                    else
                                        if (record is Idea && ((Idea)record).IdeaDefinitor.DefaultSymbolFormat.UseDefinitorPictogramAsNullDefault)
                                            Picture = ((Idea)record).IdeaDefinitor.Pictogram;

                                if (Picture != null && !Picture.IsFrozen)
                                    Picture.Freeze();

                                MaxValueWidth = ReportConfiguration.PICTOGRAM_MAX_WIDTH;
                                MaxValueHeight = ReportConfiguration.PICTOGRAM_MAX_HEIGHT;

                                return Picture;
                            });
                    else
                        Value = (ColValueDef.Value.Value1 != null
                                 ? ColValueDef.Value.Value1(record)
                                 : (ColDef == null ? null : ColDef.Read(record)));

                    var Cell = CreateListCell(Value, ColValueDef.Value.Value0, HorizontalAlign, VerticalAlign,
                                              null, null, null, LINES_LIST_THICKNESS,
                                              MaxValueWidth, MaxValueHeight);
                    RecordFrame.Children.Add(Cell);
                }

                PagesMaker.AppendContent(RecordFrame, false, (index > 0 ? null : GroupKey));
            });

            PagesMaker.PageBreakStartCreator = null;
        }

        public FrameworkElement CreateListCell(object Value, double Width,
                                               HorizontalAlignment HorizontalAlign, VerticalAlignment VerticalAlign,
                                               Brush CellBackground = null, Brush CellForeground = null,
                                               TextFormat ValueFormat = null, double LinesThicknes = LINES_LIST_THICKNESS,
                                               double MaxValueWidth = double.PositiveInfinity,
                                               double MaxValueHeight = double.PositiveInfinity)
        {
            var BrdCell = new Border();
            BrdCell.Width = Width;   // Must be fixed applied

            BrdCell.Background = CellBackground.NullDefault(this.Configuration.FmtFieldValueBackground);
            BrdCell.BorderBrush = CellForeground.NullDefault(this.Configuration.FmtListRowLinesForeground);
            BrdCell.BorderThickness = new Thickness(LinesThicknes);
            BrdCell.VerticalAlignment = VerticalAlign;
            BrdCell.HorizontalAlignment = HorizontalAlign;
            BrdCell.Padding = new Thickness(CARD_TEXT_PADDING);

            BrdCell.Child = GetPropertyValueExpositor(Value, ValueFormat);

            return BrdCell;
        }

        // -----------------------------------------------------------------------------------------
        public Tuple<FrameworkElement, Dictionary<string, Capsule<double, Func<IMModelClass, object>>>>
                            CreateListHeader(ReportStandardPagesMaker PagesMaker,
                                             Dictionary<string, Capsule<string, double, Func<IMModelClass, object>>> ColumnDefs, // TechName + (Name + ColStarWidth)
                                             TextFormat ColTitleFormat, Brush TitleBackground,
                                             Brush BorderForeground = null,
                                             VerticalAlignment VerticalAlign = VerticalAlignment.Stretch,
                                             HorizontalAlignment HorizontalAlign = HorizontalAlignment.Stretch)
        {
            var Frame = new Grid();
            Frame.Width = this.WorkingPageContentWidth - PagesMaker.NestingMargin;

            var ColSpecs = new Dictionary<string, Capsule<double, Func<IMModelClass, object>>>();

            if (BorderForeground == null)
                BorderForeground = ColTitleFormat.ForegroundBrush;

            foreach(var ColDef in ColumnDefs)
                Frame.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(ColDef.Value.Value1, GridUnitType.Star) });

            var ColIndex = 0;
            foreach (var ColDef in ColumnDefs)
            {
                ColSpecs.Add(ColDef.Key, Capsule.Create(0.0, ColDef.Value.Value2));

                var BrdCell = new Border();
                BrdCell.Tag = ColDef.Key;
                BrdCell.Background = TitleBackground;
                BrdCell.BorderBrush = BorderForeground;
                BrdCell.BorderThickness = new Thickness(LINES_LIST_THICKNESS);
                BrdCell.VerticalAlignment = VerticalAlign;
                BrdCell.HorizontalAlignment = HorizontalAlign;
                BrdCell.Padding = new Thickness(CARD_TEXT_PADDING);

                var TxbTitle = new TextBlock();
                TxbTitle.Text = ColDef.Value.Value0;
                TxbTitle.FontFamily = ColTitleFormat.CurrentTypeface.FontFamily;
                TxbTitle.FontSize = ColTitleFormat.FontSize;
                TxbTitle.TextAlignment = ColTitleFormat.Alignment;
                TxbTitle.FontWeight = ColTitleFormat.CurrentTypeface.Weight;
                TxbTitle.FontStyle = ColTitleFormat.CurrentTypeface.Style;
                TxbTitle.TextDecorations = ColTitleFormat.GetCurrentDecorations();
                TxbTitle.TextWrapping = TextWrapping.Wrap;

                BrdCell.Child = TxbTitle;

                Frame.Children.Add(BrdCell);
                Grid.SetColumn(BrdCell, ColIndex);

                ColIndex++;
            }

            Frame.UpdateVisualization(this.WorkingPageContentWidth - PagesMaker.NestingMargin,
                                      PagesMaker.PageRemainingHeight /*? this.WorkingPageContentHeight*/);

            foreach (var Label in Frame.Children.OfType<FrameworkElement>())
                ColSpecs[Label.Tag.ToStringAlways()].Value0 = Label.ActualWidth;

            Frame.Tag = ReportStandardPagesMaker.PAGE_TOP_CONTENT_TAG;  // Declare the result as page-top content.
            var Result = Tuple.Create((FrameworkElement)Frame, ColSpecs);
            return Result;
        }

        // -----------------------------------------------------------------------------------------
        public FrameworkElement GetPropertyValueExpositor(object Value, TextFormat Format = null,
                                                          double MaxValueWidth = double.PositiveInfinity,
                                                          double MaxValueHeight = double.PositiveInfinity)
        {
            FrameworkElement Result = null;

            if (Value is MAssignment)
                Value = ((MAssignment)Value).AssignedValue;

            if (Value is ImageSource)
            {
                var PictureLabel = new Border();
                // PictureLabel.BorderThickness = new Thickness(LINES_PROP_THICKNESS);
                // PictureLabel.BorderBrush = this.Configuration.FmtCardLinesForeground;
                var Picture = new Image();
                Picture.Source = this.CurrentWorker.AtOriginalThreadGetFrozen((ImageSource)Value);
                PictureLabel.Child = Picture;

                Result = PictureLabel;
            }
            else
            {
                var TxbValue = new TextBlock();
                TxbValue.Text = (Value is IIdentifiableElement
                                 ? ((IIdentifiableElement)Value).Name
                                 : Value.ToStringAlways());

                if (Format == null)
                    Format = this.Configuration.FmtListFieldValue;

                TxbValue.FontFamily = Format.CurrentTypeface.FontFamily;
                TxbValue.FontSize = Format.FontSize;
                TxbValue.TextAlignment = Format.Alignment;
                TxbValue.FontWeight = Format.CurrentTypeface.Weight;
                TxbValue.FontStyle = Format.CurrentTypeface.Style;
                TxbValue.TextDecorations = Format.GetCurrentDecorations();
                TxbValue.TextWrapping = TextWrapping.Wrap;

                Result = TxbValue;
            }

            Result.MaxWidth = MaxValueWidth;
            Result.MaxHeight = MaxValueHeight;

            return Result;
        }

        // -----------------------------------------------------------------------------------------
    }
}
