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
// File   : VisualShape.DrawDetails.cs
// Object : Instrumind.ThinkComposer.Model.VisualModel.VisualSymbol (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.08.25 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.Definitor.DefinitorUI;
using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.MetaModel.InformationMetaModel;
using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;
using Instrumind.ThinkComposer.Model.InformationModel;
using Instrumind.ThinkComposer.Model.GraphModel;

/// Base abstractions for the visual representation of Graph entities
namespace Instrumind.ThinkComposer.Model.VisualModel
{
    /// <summary>
    /// A drawing, thus a single vector-based shape or a collection of it.
    /// Draw-Details part.
    /// </summary>
    public partial class VisualSymbol : VisualElement, IModelEntity, IModelClass<VisualSymbol>
    {
        /// <summary>
        /// Predefined size for the decorators
        /// </summary>
        public const double DECORATOR_SIZE = 16.0;

        /// <summary>
        /// Size for the detail title expander/collapser.
        /// </summary>
        public const double DETAIL_TITLE_EXPANDER_SIZE = 12.0;

        // Prefixes for Decorators Symbols
        public const string DECO_Composite = "Composite";
        public const string DECO_Related = "Related";
        public const string DECO_Properties = "Properties";
        public const string DECO_Details = "Details";
        public const string DECO_DetailExpander = "DetailExpander";

        /// <summary>
        /// Details margin (space from content-area to detail poster).
        /// </summary>
        public const double DETS_MARGIN_SIZE = 2.0;

        /// <summary>
        /// Details padding (space from content-area to the content itself).
        /// </summary>
        public const double DETS_PADDING_SIZE = 2.0;

        /// <summary>
        /// Minimum required available detail space.
        /// </summary>
        public const double DETS_MIN_AVAILABLE_SPACE = 8.0;

        /// <summary>
        /// Width of the detail expander zone
        /// </summary>
        public const double EXPANDER_ZONE_WIDTH = 20;

        /// <summary>
        /// Default width of table field when empty.
        /// </summary>
        public const double DEFEMPTY_TBLFIELD_WIDTH = 16.0;

        /// <summary>
        /// Details separator height.
        /// </summary>
        public const double DETS_SEPARATOR_HEIGHT = 4.0;

        // Cues for details content
        public const string DETAIL_INDIC_EMPTY = "";
        public const string DETAIL_INDIC_EXPAND = "...";
        public const string DETAIL_INDIC_UNKNOWN_FORMAT = "?";

        /// <summary>
        /// Maximum number of records shown
        /// </summary>
        public static int MaxTableRecordsShown = 50;

        /// <summary>
        /// Indicates whether to show detail (designator) headers even if the content is empty.
        /// </summary>
        public static bool ShowAllDetailHeaders = true;

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Draws the related Idea contained Details on the specified drawing Context and Available Area.
        /// Returns the the drawn detail designators with their titles and content areas (which can be null if not drawn), or null if nothing drawn.
        /// The content areas are for detail-title-designate, detail-title-expand, detail-content-assign and detail-content-edit.
        /// </summary>
        public Dictionary<DetailDesignator, Tuple<Rect, Rect, Rect, Rect>> DrawDetails(DrawingContext Context, Rect AvailableArea,
                                                                                       bool IncludeDetailsSeparators, double ContainerWidth, Pen ContainerPen)
        {
            var Result = new Dictionary<DetailDesignator, Tuple<Rect, Rect, Rect, Rect>>();

            var GlobalsFirst = VisualSymbolFormat.GetShowGlobalDetailsFirst(this);
            var StoredDetails = this.OwnerRepresentation.RepresentedIdea.GetAllDetailDesignatorsAndFinalContents()
                                            .OrderBy(dsn => !(dsn.Item1.Owner.IsGlobal == GlobalsFirst)).ToList();   // Negation because order-by puts false before true.

            /* if (GlobalsFirst)
                StoredDetails = StoredDetails.OrderBy(designator => !designator.Item1.IsIn(this.OwnerRepresentation.RepresentedIdea
                                                                                            .IdeaDefinitor.DetailDesignators)).ToList(); */

            var Margin = DETS_MARGIN_SIZE;
            var Padding = DETS_PADDING_SIZE;

            var CaptionTextFormat = VisualSymbolFormat.GetTextFormat(this, ETextPurpose.DetailCaption);
            var ContentTextFormat = VisualSymbolFormat.GetTextFormat(this, ETextPurpose.DetailContent);
            var CaptionPen = new Pen(VisualSymbolFormat.GetDetailCaptionForeground(this), 1.0);
            var ContentPen = new Pen(VisualSymbolFormat.GetDetailContentForeground(this), 1.0);
            var CaptionBackground = VisualSymbolFormat.GetDetailCaptionBackground(this);
            var ContentBackground = VisualSymbolFormat.GetDetailContentBackground(this);

            var DetailArea = new Rect(AvailableArea.X + Margin, AvailableArea.Y + Margin,
                                      AvailableArea.Width - (Margin * 2.0), AvailableArea.Height - (Margin * 2.0));

            var UsedHeight = Margin;
            var OutOfSpace = false;
            var CanSeparateWithLine = false;

            foreach (var StoredDetail in StoredDetails)
            {
                var ContentExists = (StoredDetail.Item2 != null && StoredDetail.Item2.ValueExists);

                if (!ShowAllDetailHeaders && !ContentExists)
                    continue;

                var DetailLook = this.OwnerRepresentation.RepresentedIdea.GetDetailLook(StoredDetail.Item1, this);

                if (!ContentExists && !DetailLook.ShowTitle)
                    continue;

                Rect TitleDesignateArea = Rect.Empty;
                Rect TitleExpandArea = Rect.Empty;
                Rect ContentAssignArea = Rect.Empty;
                Rect ContentEditArea = Rect.Empty;
                var ShowDetail = DetailLook.IsDisplayed;

                if (IncludeDetailsSeparators && CanSeparateWithLine
                    && (DetailLook.ShowTitle || ShowDetail))
                {
                    var PosX = DetailArea.X - ((ContainerWidth - DetailArea.Width) / 2.0);
                    var PosY = DetailArea.Y + (UsedHeight - 1.0) + (ContainerPen.Thickness / 2.0);
                    Context.DrawLine(ContainerPen, new Point(PosX, PosY), new Point(PosX + ContainerWidth, PosY));
                    UsedHeight += ContainerPen.Thickness + 1.0;
                }

                // Title...
                if (DetailLook.ShowTitle)
                {
                    var TitleFmt = VisualSymbolFormat.GetTextFormat(this, ETextPurpose.DetailHeading);
                    TitleDesignateArea = DrawDetailTitle(Context, StoredDetail.Item1.Name, DetailLook.IsDisplayed, TitleFmt,
                                                         new Rect(DetailArea.X, DetailArea.Y + UsedHeight,
                                                                  DetailArea.Width, (DetailArea.Height - UsedHeight)), Padding,
                                                         VisualSymbolFormat.GetDetailHeadingBackground(this),
                                                         VisualSymbolFormat.GetDetailHeadingForeground(this));

                    if (TitleDesignateArea != Rect.Empty)
                    {
                        UsedHeight += TitleDesignateArea.Height;

                        // For usability, the Title and Expander areas will be of the same size.
                        TitleExpandArea = new Rect(TitleDesignateArea.Right - EXPANDER_ZONE_WIDTH.EnforceMaximum(TitleDesignateArea.Width),
                                                   TitleDesignateArea.Y,
                                                   EXPANDER_ZONE_WIDTH.EnforceMaximum(TitleDesignateArea.Width),
                                                   TitleDesignateArea.Height);
                        TitleDesignateArea = new Rect(TitleDesignateArea.X, TitleDesignateArea.Y,
                                                   (TitleDesignateArea.Width - EXPANDER_ZONE_WIDTH).EnforceMinimum(0), TitleDesignateArea.Height);
                    }
                    else
                        ShowDetail = false; // Needed to be consistent: Avoid showing a tiny detail (such as a mini image) when the Title doesn't fit.
                }

                //T Console.WriteLine("Drawing detail '{0}': {1}", StoredDetail.Item1.Name, ShowDetail);

                if (ShowDetail)
                {
                    OutOfSpace = (UsedHeight >= DetailArea.Height - DETS_MIN_AVAILABLE_SPACE);
                    if (!OutOfSpace)
                    {
                        // Content...
                        var ContentLook = this.OwnerRepresentation.RepresentedIdea.GetDetailLook(StoredDetail.Item1, this);

                        ContentEditArea = DrawDetailContent(Context, this.OwnerRepresentation.RepresentedIdea, StoredDetail.Item1, StoredDetail.Item2,
                                                            new Rect(DetailArea.X - 0.5, DetailArea.Y + UsedHeight,
                                                                     DetailArea.Width + 1, (DetailArea.Height - UsedHeight)), Padding,
                                                            ContentLook, CaptionTextFormat, ContentTextFormat, CaptionPen, ContentPen,
                                                            CaptionBackground, ContentBackground);

                        if (ContentEditArea != Rect.Empty)
                        {
                            ContentAssignArea = new Rect(ContentEditArea.X + ContentEditArea.Width * 0.25, ContentEditArea.Y,
                                                         ContentEditArea.Width * 0.5, ContentEditArea.Height);

                            UsedHeight += ContentEditArea.Height;
                        }
                    }
                }

                // Skip separator space...
                if (DetailLook.ShowTitle || ShowDetail)
                {
                    UsedHeight += DETS_SEPARATOR_HEIGHT;
                    CanSeparateWithLine = true;
                }

                OutOfSpace = (UsedHeight >= DetailArea.Height - DETS_MIN_AVAILABLE_SPACE);

                if (TitleDesignateArea != Rect.Empty || TitleExpandArea != Rect.Empty || ContentEditArea != Rect.Empty)
                    Result.Add(StoredDetail.Item1, Tuple.Create<Rect, Rect, Rect, Rect>(TitleDesignateArea, TitleExpandArea, ContentAssignArea, ContentEditArea));

                if (OutOfSpace)
                    break;
            }

            return (Result.Count < 1 ? null : Result);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// For a given drawing-context, generates and draws a Detail Title Label for the specified Text and Expanded/Collapsed state, considering Format, Available-Area and Padding parameters.
        /// Returns the area of the drawn label.
        /// </summary>
        public static Rect DrawDetailTitle(DrawingContext Context, string Text, bool IsExpanded, TextFormat Format, Rect AvailableArea, double Padding,
                                           Brush DetailHeadingBackground, Brush DetailHeadingForeground)
        {
            if (AvailableArea.Width < DETAIL_TITLE_EXPANDER_SIZE || AvailableArea.Height < DETAIL_TITLE_EXPANDER_SIZE)
                return Rect.Empty;

            var LabelArea = MasterDrawer.DrawTextLabel(Context, Text, Format, new Rect(AvailableArea.X, AvailableArea.Y, AvailableArea.Width, AvailableArea.Height),
                                                       DetailHeadingBackground,
                                                       new Pen(DetailHeadingForeground, 1.0),
                                                       Padding, Padding, Padding + DETAIL_TITLE_EXPANDER_SIZE);

            if (LabelArea != Rect.Empty)
            {
                // Draw the Expander/Collapser actioner depending on state
                var DecGeom = Display.GetResource<GeometryGroup>(SymbolDrawer.RES_PFX_DECORATORS + DECO_DetailExpander, true);

                var PosX = LabelArea.X + LabelArea.Width - (DETAIL_TITLE_EXPANDER_SIZE + 1.5);
                var PosY = LabelArea.Y + 1.5;
                var ExpanderPosition = new Point(PosX, PosY);

                var Expander = SymbolDrawer.CreateDecorator(DecGeom, DetailHeadingBackground, DetailHeadingBackground, DetailHeadingBackground,
                                                            Format.ForegroundBrush,
                                                            ExpanderPosition, DETAIL_TITLE_EXPANDER_SIZE, DETAIL_TITLE_EXPANDER_SIZE,
                                                            (IsExpanded ? 0.0 : 270.0));

                Context.DrawDrawing(Expander);
            }

            return LabelArea;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// For a given drawing-context, generates and draws a Detail Content for the specified Designator and Content, considering Available-Area and Padding parameters.
        /// Returns the area of the drawn content.
        /// </summary>
        public static Rect DrawDetailContent(DrawingContext Context, Idea SourceIdea, DetailDesignator Designator, ContainedDetail Content, Rect AvailableArea, double Padding,
                                             DetailAppearance Look, TextFormat CaptionTextFormat, TextFormat ContentTextFormat,
                                             Pen CaptionPen, Pen ContentPen, Brush CaptionBackground, Brush ContentBackground)
        {
            Rect Result = Rect.Empty;

            Context.PushClip(new RectangleGeometry(AvailableArea));

            if (Designator is AttachmentDetailDesignator)
                Result = DrawDetailAttachment(Context, Designator as AttachmentDetailDesignator, Content as Attachment, AvailableArea, Padding,
                                              Look, CaptionTextFormat, ContentTextFormat,
                                              CaptionPen, ContentPen, ContentBackground);
            else
                if (Designator is LinkDetailDesignator)
                    Result = DrawDetailLink(Context, SourceIdea, Designator as LinkDetailDesignator, Content as Link, AvailableArea, Padding,
                                            Look, CaptionTextFormat, ContentTextFormat,
                                            CaptionPen, ContentPen, ContentBackground);
                else
                    if (Designator is TableDetailDesignator)
                        Result = DrawDetailTable(Context, Designator as TableDetailDesignator, Content as Table, AvailableArea, Padding,
                                                 Look as TableAppearance, CaptionTextFormat, ContentTextFormat,
                                                 CaptionPen, ContentPen, CaptionBackground, ContentBackground);

            Context.Pop();

            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// For the given drawing Context, Designator, Content, Available-Area and Padding, draw the supplied Attachment.
        /// Returns the area of the drawn content.
        /// </summary>
        public static Rect DrawDetailAttachment(DrawingContext Context, AttachmentDetailDesignator Designator, Attachment Content, Rect AvailableArea, double Padding,
                                                DetailAppearance Look, TextFormat CaptionTextFormat, TextFormat ContentTextFormat,
                                                Pen CaptionPen, Pen ContentPen, Brush ContentBackground)
        {
            Rect Result = Rect.Empty;

            if (Content == null)
                Result = MasterDrawer.DrawTextLabel(Context, DETAIL_INDIC_EMPTY, ContentTextFormat, AvailableArea, ContentBackground,
                                                    ContentPen, Padding, Padding, Padding);
            else
            {
                if (Content.MimeType.StartsWith("image/"))
                    Result = MasterDrawer.DrawImage(Context, Content.Content,
                                       new Rect(AvailableArea.X,
                                                AvailableArea.Y,
                                                AvailableArea.Width,
                                                AvailableArea.Height.EnforceMaximum(AvailableArea.Width)),  // Limits the height to let other details be displayed (for tall images)
                                       true, false, false, true);
                else
                {
                    string Text = DETAIL_INDIC_EXPAND;

                    if (Content.MimeType.StartsWith("text/"))
                        Text = Content.Content.BytesToString();

                    Result = MasterDrawer.DrawTextLabel(Context, Text, ContentTextFormat, AvailableArea, ContentBackground,
                                                        ContentPen, Padding, Padding, Padding);
                }
            }

            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// For the given drawing Context, Source-Idea, Designator, Content, Available-Area and Padding, draw the supplied Link.
        /// Returns the area of the drawn content.
        /// </summary>
        public static Rect DrawDetailLink(DrawingContext Context, Idea SourceIdea, LinkDetailDesignator Designator, Link Content, Rect AvailableArea, double Padding,
                                          DetailAppearance Look, TextFormat CaptionTextFormat, TextFormat ContentTextFormat,
                                          Pen CaptionPen, Pen ContentPen, Brush ContentBackground)
        {
            Rect Result = Rect.Empty;

            if (Content == null)
                Result = MasterDrawer.DrawTextLabel(Context, DETAIL_INDIC_EMPTY, ContentTextFormat, AvailableArea, ContentBackground,
                                                    ContentPen, Padding, Padding, Padding);
            else
            {
                string Text = DETAIL_INDIC_UNKNOWN_FORMAT;

                if (Content is ResourceLink)
                    Text = ((ResourceLink)Content).TargetLocation;
                else
                    if (Content is InternalLink)
                        Text = ((InternalLink)Content).TargetProperty.Read(SourceIdea).ToStringAlways();

                // PENDING: IF THE LINK REFERS TO A WEBPAGE, THEN SHOW SCREENSHOT.
                Result = MasterDrawer.DrawTextLabel(Context, Text, ContentTextFormat, AvailableArea, ContentBackground,
                                                    ContentPen, Padding, Padding, Padding);
            }

            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// For the given drawing Context, Designator, Content, Available-Area and Padding, draw the supplied Table.
        /// Returns the area of the drawn content.
        /// </summary>
        public static Rect DrawDetailTable(DrawingContext Context, TableDetailDesignator Designator, Table Content, Rect AvailableArea, double Padding,
                                           TableAppearance Look, TextFormat CaptionTextFormat, TextFormat ContentTextFormat,
                                           Pen CaptionPen, Pen ContentPen, Brush CaptionBackground, Brush ContentBackground)
        {
            Rect Result = Rect.Empty;
            Look = Look.NullDefault(new TableAppearance());
            
            if (Content == null)
                Result = MasterDrawer.DrawTextLabel(Context, DETAIL_INDIC_EMPTY, CaptionTextFormat, AvailableArea, ContentBackground,
                                                    ContentPen, Padding, Padding, Padding);
            else
            {
                Result = MasterDrawer.DrawTextLabel(Context, DETAIL_INDIC_EXPAND, CaptionTextFormat, AvailableArea, ContentBackground,
                                                    ContentPen, Padding, Padding, Padding);

                if (Look.Layout == ETableLayoutStyle.Simple)
                    Result = DrawDetailTableList(Context, Designator, Content, AvailableArea, Padding,
                                                 Look, CaptionTextFormat, ContentTextFormat, CaptionPen, ContentPen, ContentBackground);
                else
                    /* POSTPONED if (Look.Layout == ETableLayoutStyle.Hierarchical)
                        Result = DrawDetailTableTree(Context, Designator, Content, AvailableArea, Padding);
                    else */
                    if (Look.Layout == ETableLayoutStyle.Transposed)
                            Result = DrawDetailTableGridTransposed(Context, Designator, Content, AvailableArea, Padding,
                                                                   Look, CaptionTextFormat, ContentTextFormat, CaptionPen, ContentPen, CaptionBackground, ContentBackground);
                        else
                            Result = DrawDetailTableGridConventional(Context, Designator, Content, AvailableArea, Padding,
                                                                     Look, CaptionTextFormat, ContentTextFormat, CaptionPen, ContentPen, CaptionBackground, ContentBackground);
            }

            return Result;
        }

        /// <summary>
        /// For the given drawing Context, Designator, Content, Available-Area and Padding, draw the supplied Table as a List.
        /// Returns the area of the drawn content.
        /// </summary>
        public static Rect DrawDetailTableList(DrawingContext Context, TableDetailDesignator Designator, Table Content, Rect AvailableArea, double Padding,
                                               TableAppearance Look, TextFormat CaptionTextFormat, TextFormat ContentTextFormat,
                                               Pen CaptionPen, Pen ContentPen, Brush ContentBackground)
        {
            Rect Result = Rect.Empty;
            Look = Look.NullDefault(new TableAppearance());
            double UsedHeight = 0;
            double RowHeight = ContentTextFormat.GenerateFormattedText("A", AvailableArea.Width, (AvailableArea.Height - UsedHeight)).Height + (Padding * 2);

            var MaxRecords = ((int)Math.Ceiling((AvailableArea.Height - RowHeight) / RowHeight)).EnforceRange(0, Look.IsMultiRecord ? MaxTableRecordsShown : 1);
            var RecordsSource = Content.GetRecordsSorted().Take(MaxRecords).ToList();

            foreach (var Record in RecordsSource)
            {
                MasterDrawer.DrawTextLabel(Context, Record.Label, ContentTextFormat, new Rect(AvailableArea.X, AvailableArea.Y + UsedHeight, AvailableArea.Width, RowHeight),
                                           ContentBackground, ContentPen, Padding, Padding, Padding);
                UsedHeight += RowHeight;

                if (AvailableArea.Height - UsedHeight < Display.CHAR_PXHEIGHT_DEFAULT)
                    break;
            }

            // ...................................................................................
            // Notice that Available width is not changed (Symbol's Details Poster shows content stacked towards bottom)
            Result = new Rect(AvailableArea.X, AvailableArea.Y, AvailableArea.Width, UsedHeight);

            return Result;
        }

        /* PENDING
        /// <summary>
        /// For the given drawing Context, Designator, Content, Available-Area and Padding, draw the supplied Table as a Tree.
        /// Returns the area of the drawn content.
        /// </summary>
        public Rect DrawDetailTableTree(DrawingContext Context, TableDetailDesignator Designator, Table Content, Rect AvailableArea, double Padding)
        {
            Rect Result = Rect.Empty;
            var Source = Content.GetRecordsHierarchized();
            var Look = Content.OwnerIdea.GetDetailLook(Designator, this);

            // PENDING: SHOW AS TREE.
            Result = ...

            return Result;
        } */

        /// <summary>
        /// For the given drawing Context, Designator, Content, Available-Area and Padding, draw the supplied Table as a Conventional Grid.
        /// Shows records on rows (one upon the other, in the vertical axis) and fields on columns (side by side, in the horizontal axis).
        /// Returns the area of the drawn content.
        /// </summary>
        public static Rect DrawDetailTableGridConventional(DrawingContext Context, TableDetailDesignator Designator, Table Content, Rect AvailableArea, double Padding,
                                                           TableAppearance Look, TextFormat CaptionTextFormat, TextFormat ContentTextFormat,
                                                           Pen CaptionPen, Pen ContentPen, Brush CaptionBackground, Brush ContentBackground)
        {
            Rect Result = Rect.Empty;
            Look = Look.NullDefault(new TableAppearance());

            double UsedWidth = 0, UsedHeight = 0, MaxUsedWidth = 0;
            var DisplayingFieldDefs = Content.Definition.FieldDefinitions.Where(fdef => !fdef.HideInDiagram).ToList();
            var FormattedTextTitles = new Dictionary<FormattedText, Rect>(DisplayingFieldDefs.Count);

            // Determines the Height for all Rows (no wrapping adjustment for long texts requiring more than one line).
            double TitleRowHeight = CaptionTextFormat.GenerateFormattedText("A", AvailableArea.Width, AvailableArea.Height).Height + (Padding * 2);
            double StandardRowHeight = ContentTextFormat.GenerateFormattedText("A", AvailableArea.Width, (AvailableArea.Height - UsedHeight)).Height + (Padding * 2);

            // ...................................................................................
            // Prepare Columns Widths, always related to Field Definitions
            var ColumnsWidths = new Dictionary<FieldDefinition, double>(DisplayingFieldDefs.Count);

            foreach (var FieldDef in DisplayingFieldDefs)
                ColumnsWidths.Add(FieldDef, 0);

            // ...................................................................................
            // Determine Titles
            // (It stores strings instead of FormattedTexts because for centered-text it must be regenerated with final max-width)
            var FormattedTitles = new Dictionary<FieldDefinition, string>(DisplayingFieldDefs.Count);

            if (Look.ShowFieldTitles)
            {
                foreach (var FieldDef in DisplayingFieldDefs)
                {
                    var MaxColumnWidth = Math.Min(AvailableArea.Width - UsedWidth, FieldDef.GetEstimatedColumnPixelsWidth(Display.CHAR_PXWIDTH_DEFAULT));

                    var Caption = FieldDef.Name;
                    var FormattedTitle = CaptionTextFormat.GenerateFormattedText(Caption, MaxColumnWidth, AvailableArea.Height.EnforceRange(1, TitleRowHeight), Padding, Padding, Padding);

                    if (FormattedTitle != null)
                    {
                        FormattedTitles.Add(FieldDef, Caption);
                        ColumnsWidths[FieldDef] = FormattedTitle.Width + 4 + (Padding * 2); // That 4 is to allow dates to be shown complete

                        UsedWidth += FormattedTitle.Width + (Padding * 2);

                        UsedHeight = FormattedTitle.Height + (Padding * 2);
                        if (UsedHeight > TitleRowHeight)
                            TitleRowHeight = UsedHeight;
                    }

                    if (AvailableArea.Width - UsedWidth < (Display.CHAR_PXWIDTH_DEFAULT * 2))
                        break;
                }

                MaxUsedWidth = UsedWidth;
            }

            // Determine Records
            // (It stores strings instead of FormattedTexts because for centered-text it must be regenerated with final max-width)
            var MaxRecords = ((int)Math.Ceiling((AvailableArea.Height - StandardRowHeight) / StandardRowHeight)).EnforceRange(0, Look.IsMultiRecord ? MaxTableRecordsShown : 1);
            var RecordsSource = Content.GetRecordsSorted().Take(MaxRecords).ToList();
            var FormattedRecords = new Dictionary<TableRecord, Dictionary<FieldDefinition, object>>(RecordsSource.Count);
            UsedWidth = 0;
            UsedHeight = 0;

            if (AvailableArea.Width > 0 && (AvailableArea.Height - TitleRowHeight) > 1)
            {
                foreach (var Record in RecordsSource)
                {
                    FormattedRecords.Add(Record, new Dictionary<FieldDefinition, object>(Content.Definition.FieldDefinitions.Count));

                    foreach (var FieldDef in DisplayingFieldDefs)
                    {
                        var MaxColumnWidth = Math.Min(AvailableArea.Width - UsedWidth, FieldDef.GetEstimatedColumnPixelsWidth(Display.CHAR_PXWIDTH_DEFAULT));
                        var ValueWidth = 0.0;

                        if (FieldDef.FieldType.IsEqual(DataType.DataTypePicture))
                        {
                            var FieldContent = Record.GetStoredValue(FieldDef) as ImageAssignment;
                            FormattedRecords[Record].Add(FieldDef, FieldContent);
                            ValueWidth = DEFEMPTY_TBLFIELD_WIDTH * 3;   // 48.0
                        }
                        else
                        {
                            var FieldContent = Record.GetStoredValueForDisplay(FieldDef);
                            FormattedRecords[Record].Add(FieldDef, FieldContent);
                            var FormattedValue = ContentTextFormat.GenerateFormattedText(FieldContent, MaxColumnWidth, StandardRowHeight, Padding, Padding, Padding);
                            ValueWidth = (FormattedValue == null ? DEFEMPTY_TBLFIELD_WIDTH : FormattedValue.Width);
                        }

                        var Size = Math.Max(ColumnsWidths[FieldDef], ValueWidth + (Padding * 2));
                        ColumnsWidths[FieldDef] = Size;

                        UsedWidth += Size;
                        if (AvailableArea.Width - UsedWidth < (Display.CHAR_PXWIDTH_DEFAULT * 2))
                            break;
                    }

                    if (UsedWidth > MaxUsedWidth)
                        MaxUsedWidth = UsedWidth;

                    UsedWidth = 0;
                    UsedHeight += StandardRowHeight;

                    if (AvailableArea.Height - UsedHeight <= 1)
                        break;
                }
            }

            // Determine distributable width (only for text columns)
            double DistributableWidth = 0;

            foreach (var ColWidth in ColumnsWidths)
                if (ColWidth.Key.FieldType is TextType)
                    DistributableWidth += ColWidth.Value;

            double AvailableDistributableWidth = DistributableWidth + (AvailableArea.Width - MaxUsedWidth);

            // Adjust columns width distributions
            if (ColumnsWidths.Count > 0 && AvailableArea.Width > MaxUsedWidth + 1 && DistributableWidth > 0)
                for (int ColWidthIndex = 0; ColWidthIndex < ColumnsWidths.Count; ColWidthIndex++)
                {
                    var ColWidthKey = DisplayingFieldDefs[ColWidthIndex];

                    if (ColWidthKey.FieldType is TextType)
                        ColumnsWidths[ColWidthKey] = (ColumnsWidths[ColWidthKey] * AvailableDistributableWidth) / DistributableWidth;
                }

            // ...................................................................................
            // Draw Titles
            UsedHeight = 0;

            if (FormattedTitles.Count > 0 && AvailableArea.Height - UsedHeight >= 1)
            {
                // Draw titles backround
                var RowArea = new Rect(AvailableArea.X, AvailableArea.Y, AvailableArea.Width, TitleRowHeight);
                Context.DrawRectangle(CaptionBackground, ContentPen, RowArea);

                UsedHeight += TitleRowHeight;
                UsedWidth = 0;

                // Draw titles text
                foreach (var FormattedTitle in FormattedTitles)
                {
                    var SizeWidth = ColumnsWidths[FormattedTitle.Key].EnforceRange(1, AvailableArea.Width - UsedWidth);
                    var Frame = new Rect(AvailableArea.X + UsedWidth, AvailableArea.Y, SizeWidth, TitleRowHeight.EnforceRange(1, AvailableArea.Height - UsedHeight));
                    MasterDrawer.DrawTextLabel(Context, FormattedTitle.Value, CaptionTextFormat, Frame, CaptionBackground,
                                               CaptionPen, Padding, Padding, Padding, false);
                    UsedWidth += SizeWidth;
                    if (AvailableArea.Width - UsedWidth < Display.CHAR_PXWIDTH_DEFAULT)
                        break;
                }
            }

            // Draw Records
            UsedWidth = 0.0;

            if (FormattedRecords.Count > 0 && AvailableArea.Height - UsedHeight >= 1)
                foreach (var FormattedRecord in FormattedRecords)
                {
                    var ImageRowHeight = StandardRowHeight * 3.0;
                    var MaxRowHeight = (FormattedRecord.Value.Any(val => val.Value is ImageAssignment) ? ImageRowHeight : StandardRowHeight);

                    if (FormattedRecord.Value.Any())
                    {
                        var RecordArea = new Rect(AvailableArea.X, AvailableArea.Y + UsedHeight, AvailableArea.Width, MaxRowHeight);
                        Context.DrawRectangle(ContentBackground, ContentPen /*T new Pen(Brushes.Red, 1.0)*/, RecordArea);
                    }

                    foreach (var FormattedValue in FormattedRecord.Value)
                    {
                        var SizeWidth = ColumnsWidths[FormattedValue.Key].EnforceRange(1, AvailableArea.Width - UsedWidth);

                        var StoredImage = FormattedValue.Value as ImageAssignment;
                        if (StoredImage != null)
                        {
                            var Frame = new Rect(AvailableArea.X + UsedWidth, AvailableArea.Y + UsedHeight, SizeWidth, ImageRowHeight.EnforceRange(1, AvailableArea.Height - UsedHeight));
                            var ImageValue = (StoredImage.Image == null ? null : StoredImage.Image.ToBytes());
                            MasterDrawer.DrawImage(Context, ImageValue, Frame);
                            Context.DrawRectangle(null, ContentPen, Frame);
                        }
                        else
                        {
                            var Frame = new Rect(AvailableArea.X + UsedWidth, AvailableArea.Y + UsedHeight, SizeWidth, MaxRowHeight.EnforceRange(1, AvailableArea.Height - UsedHeight));
                            var Text = FormattedValue.Value.ToStringAlways().AbsentDefault(" ");  // This is necessary to show at least the empty area.
                            var Alignment = (FormattedValue.Key.FieldType is BasicDataType
                                             ? ((BasicDataType)FormattedValue.Key.FieldType).DisplayAlignment
                                             : ContentTextFormat.Alignment);
                            var FormattedText = ContentTextFormat.GenerateFormattedText(Text, Frame.Width, Frame.Height, Alignment, Padding, Padding, Padding);
                            /*- if (FormattedText.Width > 0 && RecordArea == Rect.Empty)
                            {
                                // Draw the Record Area, once, just prior the first text to be displayed.
                                RecordArea = new Rect(AvailableArea.X, AvailableArea.Y + UsedHeight, AvailableArea.Width, StandardRowHeight);
                                Context.DrawRectangle(ContentBackground, ContentPen, RecordArea);
                            } */

                            MasterDrawer.DrawTextLabel(Context, FormattedText, Frame, ContentBackground, ContentPen, Padding, Padding, Padding, false);
                            //- Context.DrawRectangle(ContentBackground, ContentPen, FieldRowArea);
                        }

                        UsedWidth += SizeWidth;
                        if (AvailableArea.Width - UsedWidth < Display.CHAR_PXWIDTH_DEFAULT)
                            break;
                    }

                    UsedWidth = 0;
                    UsedHeight += MaxRowHeight;

                    if (AvailableArea.Height - UsedHeight < Display.CHAR_PXHEIGHT_DEFAULT)
                        break;
                }

            // ...................................................................................
            // Notice that Available width is not changed (Symbol's Details Poster shows content stacked towards bottom)
            Result = new Rect(AvailableArea.X, AvailableArea.Y, AvailableArea.Width, UsedHeight);

            return Result;
        }

        /// <summary>
        /// For the given drawing Context, Designator, Content, Available-Area and Padding, draw the supplied Table as a Transposed Grid.
        /// Shows fields on rows (one upon the other, in the vertical axis) and records on columns (side by side, in the horizontal axis).
        /// Returns the area of the drawn content.
        /// </summary>
        public static Rect DrawDetailTableGridTransposed(DrawingContext Context, TableDetailDesignator Designator, Table Content, Rect AvailableArea, double Padding,
                                                         TableAppearance Look, TextFormat CaptionTextFormat, TextFormat ContentTextFormat,
                                                         Pen CaptionPen, Pen ContentPen, Brush CaptionBackground, Brush ContentBackground)
        {
            Rect Result = Rect.Empty;
            Look = Look.NullDefault(new TableAppearance());

            double UsedWidth = 0, UsedHeight = 0, MaxUsedHeight = 0;
            var DisplayingFieldDefs = Content.Definition.FieldDefinitions.Where(fdef => !fdef.HideInDiagram).ToList();
            var FormattedTextTitles = new Dictionary<FormattedText, Rect>(DisplayingFieldDefs.Count);
            CaptionTextFormat = CaptionTextFormat.CreateClone(ECloneOperationScope.Deep, null);
            CaptionTextFormat.Alignment = TextAlignment.Left;   // Better look for transposing (avoids ugly trimming when centered or right-aligned).
            Rect Zone = Rect.Empty;

            // Determines a fixed width for all columns
            var StandardColumnWidth = MModelPropertyDefinitor.GetEstimatedCharactersFor(EPropertyKind.Name) * Display.CHAR_PXWIDTH_DEFAULT * 0.65;

            // Determines the Height for all Rows (no wrapping adjustment for long texts requiring more than one line).
            var BaseRowHeight = Math.Max(CaptionTextFormat.GenerateFormattedText("A", AvailableArea.Width, AvailableArea.Height).Height,
                                         ContentTextFormat.GenerateFormattedText("A", AvailableArea.Width, AvailableArea.Height).Height) + (Padding * 2) + 2;

            var RowHeights = new Dictionary<FieldDefinition, double>();

            foreach (var FieldDef in DisplayingFieldDefs)
            {
                var TextTypeDecl = FieldDef.FieldType as TextType;
                RowHeights.Add(FieldDef, (TextTypeDecl != null && TextTypeDecl.SizeLimit > BasicDataType.MAX_SINGLELINE_TEXT_LENGTH
                                          ? BaseRowHeight * 2 : (FieldDef.FieldType is PictureType ? BaseRowHeight * 3 : BaseRowHeight)));
            }

            // ...................................................................................
            var MaxColumns = ((int)Math.Ceiling(AvailableArea.Width / Display.CHAR_PXWIDTH_DEFAULT)).EnforceRange(0, Look.IsMultiRecord ? MaxTableRecordsShown : 1);
            var RecordsSource = Content.GetRecordsSorted().Take(MaxColumns).ToList();
            MaxColumns = (Look.ShowFieldTitles ? 1 : 0) + RecordsSource.Count;

            // Prepare Columns Widths, related to titles and records.
            var ColumnsWidths = new List<double>(MaxColumns);

            // ...................................................................................
            // Determine Cells
            var FormattedTitles = new Dictionary<FieldDefinition, string>(DisplayingFieldDefs.Count);
            double MaxColumnWidth = Display.CHAR_PXWIDTH_DEFAULT;
            double TotalWidth = 0;
            int RecordIndex = 0;
            int MaxFieldDefRows = 0;

            for (int ColumnIndex = 0; ColumnIndex < MaxColumns; ColumnIndex++)
            {
                if (ColumnIndex == 0 && Look.ShowFieldTitles)
                {
                    // Determine Titles Widths
                    foreach (var FieldDef in DisplayingFieldDefs)
                    {
                        MaxFieldDefRows++;   // Just perform once per row/field-def, either here in the titles or when there is no titles.
                        var Caption = FieldDef.Name;
                        var CellText = CaptionTextFormat.GenerateFormattedText(Caption, AvailableArea.Width, RowHeights[FieldDef], Padding, Padding, Padding);
                        if (CellText != null)
                        {
                            var CellWidth = CellText.Width + (Padding * 2);
                            if (CellWidth > MaxColumnWidth)
                                MaxColumnWidth = CellWidth;
                            UsedHeight += RowHeights[FieldDef];
                        }
                        else
                            break;

                        if (RowHeights[FieldDef] > AvailableArea.Height - UsedHeight)
                            break;
                    }
                }
                else
                {
                    // Determine Records Widths
                    foreach (var FieldDef in DisplayingFieldDefs)
                    {
                        if (ColumnIndex == 0)   // Just perform once per row/field-def, either in the titles or here when there is no titles.
                            MaxFieldDefRows++;

                        var Record = RecordsSource[RecordIndex];

                        if (FieldDef.FieldType.IsEqual(DataType.DataTypePicture))
                            UsedHeight = RowHeights[FieldDef];
                        else
                        {
                            var FieldContent = Record.GetStoredValueForDisplay(FieldDef);
                            var CellText = CaptionTextFormat.GenerateFormattedText(FieldContent, AvailableArea.Width, RowHeights[FieldDef], Padding, Padding, Padding);
                            if (CellText != null)
                            {
                                var CellWidth = CellText.Width + (Padding * 2);
                                if (CellWidth > MaxColumnWidth)
                                    MaxColumnWidth = CellWidth;
                                UsedHeight += RowHeights[FieldDef];
                            }
                            /*? else
                                    break; */
                        }

                        if (RowHeights[FieldDef] > AvailableArea.Height - UsedHeight)
                            break;
                    }

                    RecordIndex++;

                    if (AvailableArea.Width - MaxColumnWidth <= 0)
                        break;

                }

                UsedHeight = 0;
                TotalWidth += MaxColumnWidth;
                ColumnsWidths.Add(MaxColumnWidth);

                MaxColumnWidth = Display.CHAR_PXWIDTH_DEFAULT;
            }

            // Adjust columns widths distribution
            if (TotalWidth > AvailableArea.Width)
            {
                UsedWidth = 0;

                for (int ColumnIndex = 0; ColumnIndex < ColumnsWidths.Count; ColumnIndex++)
                {
                    if (AvailableArea.Width - UsedWidth > 1)
                    {
                        if (ColumnIndex == 0 && Look.ShowFieldTitles)
                            ColumnsWidths[ColumnIndex] = Math.Min(ColumnsWidths[ColumnIndex], AvailableArea.Width / 2.0).EnforceRange(1, AvailableArea.Width);
                        else
                            ColumnsWidths[ColumnIndex] = Math.Min(ColumnsWidths[ColumnIndex], StandardColumnWidth).EnforceRange(1, AvailableArea.Width - UsedWidth);

                        UsedWidth += ColumnsWidths[ColumnIndex];
                    }
                    else
                    {
                        ColumnsWidths.RemoveRange(ColumnIndex, ColumnsWidths.Count - ColumnIndex);
                        break;
                    }
                }
            }
            else
                UsedWidth = TotalWidth;

            if (Look.ShowFieldTitles)
            {
                if (ColumnsWidths.Count > 1)
                {
                    var BaseWidth = UsedWidth - ColumnsWidths[0];
                    var DistributableWidth = AvailableArea.Width - ColumnsWidths[0];

                    if (DistributableWidth > BaseWidth + 1)
                        for (int ColWidthIndex = 1; ColWidthIndex < ColumnsWidths.Count; ColWidthIndex++)
                            ColumnsWidths[ColWidthIndex] = (ColumnsWidths[ColWidthIndex] * DistributableWidth) / BaseWidth;
                }
            }
            else
                if (ColumnsWidths.Count > 0 && AvailableArea.Width > UsedWidth + 1)
                    for (int ColWidthIndex = 0; ColWidthIndex < ColumnsWidths.Count; ColWidthIndex++)
                        ColumnsWidths[ColWidthIndex] = (ColumnsWidths[ColWidthIndex] * AvailableArea.Width) / UsedWidth;

            // ...................................................................................
            // Show Cells
            MaxColumns = ColumnsWidths.Count;
            UsedWidth = 0;
            UsedHeight = 0;
            RecordIndex = 0;
            Rect FieldRowArea = Rect.Empty;

            for (int ColumnIndex = 0; ColumnIndex < MaxColumns; ColumnIndex++)
            {
                if (ColumnIndex == 0 && Look.ShowFieldTitles)
                {
                    // Titles...
                    for (int FieldDefRowIndex = 0; FieldDefRowIndex < MaxFieldDefRows; FieldDefRowIndex++)
                    {
                        var FieldDef = DisplayingFieldDefs[FieldDefRowIndex];
                        var Text = FieldDef.Name;
                        var FormattedCellText = CaptionTextFormat.GenerateFormattedText(Text, ColumnsWidths[0], RowHeights[FieldDef], Padding, Padding, Padding);

                        if (FormattedCellText != null)
                        {
                            Zone = new Rect(AvailableArea.X, AvailableArea.Y + UsedHeight, ColumnsWidths[0], RowHeights[FieldDef]);
                            MasterDrawer.DrawTextLabel(Context, FormattedCellText, Zone, CaptionBackground,
                                                       CaptionPen, Padding, Padding, Padding, false);
                        }

                        UsedHeight += RowHeights[FieldDef];
                    }
                }
                else
                {
                    // Records...
                    for (int FieldDefRowIndex = 0; FieldDefRowIndex < MaxFieldDefRows; FieldDefRowIndex++)
                    {
                        var FieldDef = DisplayingFieldDefs[FieldDefRowIndex];

                        if (RecordIndex == 0)
                        {
                            // Draw the Field Area, once, just prior the first text to be displayed.
                            FieldRowArea = new Rect(AvailableArea.X + UsedWidth, AvailableArea.Y + UsedHeight, AvailableArea.Width - UsedWidth, RowHeights[FieldDef]);
                            Context.DrawRectangle(ContentBackground, ContentPen, FieldRowArea);
                        }

                        var Record = RecordsSource[RecordIndex];

                        // Explicit Alignment (ugly for transposed)...
                        // var Alignment = (FieldDef.FieldType is BasicDataType ? ((BasicDataType)FieldDef.FieldType).DisplayAlignment : ContentTextFormat.Alignment);

                        Zone = new Rect(AvailableArea.X + UsedWidth, AvailableArea.Y + UsedHeight, ColumnsWidths[ColumnIndex], RowHeights[FieldDef]);

                        if (FieldDef.FieldType.IsEqual(DataType.DataTypePicture))
                        {
                            var StoredImage = Record.GetStoredValue(FieldDef) as ImageAssignment;
                            var ImageValue = (StoredImage == null ? null : StoredImage.Image.ToBytes());
                            MasterDrawer.DrawImage(Context, ImageValue, Zone, false, false, false);
                            Context.DrawRectangle(null, ContentPen, Zone);
                        }
                        else
                        {
                            var Text = Record.GetStoredValueForDisplay(FieldDef).AbsentDefault(" "); // This is necessary to show at least the empty area.
                            var FormattedCellText = ContentTextFormat.GenerateFormattedText(Text, ColumnsWidths[ColumnIndex], RowHeights[FieldDef], Padding, Padding, Padding);

                            if (FormattedCellText != null)
                                MasterDrawer.DrawTextLabel(Context, FormattedCellText, Zone, ContentBackground,
                                                           ContentPen, Padding, Padding, Padding, false);
                        }

                        UsedHeight += RowHeights[FieldDef];
                    }

                    RecordIndex++;
                }

                if (UsedHeight > MaxUsedHeight)
                    MaxUsedHeight = UsedHeight;

                UsedHeight = 0;
                UsedWidth += ColumnsWidths[ColumnIndex];
            }

            // Notice that Available width is not changed (Symbol's Details Poster shows content stacked towards bottom)
            Result = new Rect(AvailableArea.X, AvailableArea.Y, AvailableArea.Width, MaxUsedHeight);

            return Result;
        }
    }
}