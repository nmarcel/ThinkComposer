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
using System.Globalization;
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
using Instrumind.ThinkComposer.MetaModel;
using Instrumind.ThinkComposer.MetaModel.Configurations;
using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.MetaModel.InformationMetaModel;
using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;
using Instrumind.ThinkComposer.Model;
using Instrumind.ThinkComposer.Model.GraphModel;
using Instrumind.ThinkComposer.Model.InformationModel;
using Instrumind.ThinkComposer.Model.VisualModel;

/// Provides features for report generation.
namespace Instrumind.ThinkComposer.Composer.Reporting
{
    /// <summary>
    /// Generates comprehensive HTML reports from Compositions.
    /// </summary>
    public partial class ReportHtmlGenerator
    {
        // -----------------------------------------------------------------------------------------
        public void CreateContentCard(IRecognizableComposite Source, DisplayCard DispCard)
        {
            var TableColDefs = new List<Capsule<string, string, double>>();
            var TableRowVals = new List<string>();

            if (DispCard.PropName)
            {
                TableColDefs.Add(Capsule.Create(FormalPresentationElement.__Name.TechName.ToHtmlEncoded(),
                                                FormalPresentationElement.__Name.Name.ToHtmlEncoded(), 30.0));
                TableRowVals.Add(Source.Name.RemoveNewLines().ToHtmlEncoded());
            }

            if (DispCard.PropTechName)
            {
                TableColDefs.Add(Capsule.Create(FormalPresentationElement.__TechName.TechName.ToHtmlEncoded(),
                                                FormalPresentationElement.__TechName.Name.ToHtmlEncoded(), 30.0));
                TableRowVals.Add(Source.TechName.ToHtmlEncoded());
            }

            if (DispCard.PropPictogram && Source is IRecognizableElement)
            {
                var LocalPicture = this.CurrentWorker.AtOriginalThreadInvoke<ImageSource>(
                    () =>
                    {
                        var Picture = ((IRecognizableElement)Source).Pictogram;
                        if (Picture == null)
                            if (Source is IdeaDefinition)
                                Picture = ((IdeaDefinition)Source).Pictogram;
                            else
                                if (Source is Idea && ((Idea)Source).IdeaDefinitor.DefaultSymbolFormat.UseDefinitorPictogramAsNullDefault)
                                    Picture = ((Idea)Source).IdeaDefinitor.Pictogram;

                        if (Picture != null && !Picture.IsFrozen)
                            Picture.Freeze();

                        return Picture;
                    });

                if (LocalPicture != null)
                {
                    var PictureRef = this.CreateImage(LocalPicture, Source, FormalPresentationElement.__Pictogram.TechName);

                    TableColDefs.Add(Capsule.Create(FormalPresentationElement.__Pictogram.TechName.ToHtmlEncoded(),
                                                    FormalPresentationElement.__Pictogram.Name.ToHtmlEncoded(), 40.0));
                    TableRowVals.Add("<img Alt='" + Source.Name.RemoveNewLines().ToHtmlEncoded() + "' Src='" + PictureRef +
                                     "' style='max-width:" + ReportConfiguration.PICTOGRAM_MAX_WIDTH.ToString() +
                                     "px; max-height:" + ReportConfiguration.PICTOGRAM_MAX_HEIGHT.ToString() + "px;' />");
                    /* This works on IE, but only if allowed by user (it says that 'IE has blocked execution of ActiveX or Script code')...
                                     "width: expression(this.width > " + ReportConfiguration.PICTOGRAM_MAX_WIDTH.ToString() + " ? " + ReportConfiguration.PICTOGRAM_MAX_WIDTH.ToString() + ": true);" +
                                     "height: expression(this.height > " + ReportConfiguration.PICTOGRAM_MAX_HEIGHT.ToString() + " ? " + ReportConfiguration.PICTOGRAM_MAX_HEIGHT.ToString() + ": true);' />"); */
                }
            }

            this.PageWriteTable("tbl_card_props", TableRowVals.IntoEnumerable(), TableColDefs.ToArray());

            // ..........................................................
            if (DispCard.PropSummary)
                this.PageWriteTable("tbl_card_props", Source.Summary.ToHtmlEncoded().IntoEnumerable().IntoEnumerable(),
                                    Capsule.Create(FormalPresentationElement.__Summary.TechName.ToHtmlEncoded(),
                                                   FormalPresentationElement.__Summary.Name.ToHtmlEncoded(), 100.0));

            // ..........................................................
            TableColDefs.Clear();
            TableRowVals.Clear();

            if (DispCard.Definitor && Source is Idea)
            {
                TableColDefs.Add(Capsule.Create("KindName", ((Idea)Source).BaseKind.Name.ToHtmlEncoded(), 30.0));
                TableRowVals.Add(((Idea)Source).IdeaDefinitor.Name.ToHtmlEncoded());
            }

            if (DispCard.PropGlobalId && Source is IUniqueElement)
            {
                TableColDefs.Add(Capsule.Create(UniqueElement.__GlobalId.TechName.ToHtmlEncoded(),
                                                UniqueElement.__GlobalId.Name.ToHtmlEncoded(), 30.0));
                TableRowVals.Add(((IUniqueElement)Source).GlobalId.ToString().ToHtmlEncoded());
            }

            if (DispCard.Definitor && Source is View)
            {
                TableColDefs.Add(Capsule.Create(View.__ClassDefinitor.TechName.ToHtmlEncoded(),
                                                View.__ClassDefinitor.Name.ToHtmlEncoded(), 30.0));
                TableRowVals.Add(((Idea)Source).IdeaDefinitor.Name.ToHtmlEncoded());
            }

            this.PageWriteTable("tbl_card_props", TableRowVals.IntoEnumerable(), TableColDefs.ToArray());

            // ..........................................................
            if (DispCard.Route & Source is Idea)
                this.PageWriteTable("tbl_card_props", ((Idea)Source).GetContainmentRoute().ToHtmlEncoded().IntoEnumerable().IntoEnumerable(),
                                    Capsule.Create("Route", "Route", 100.0));

            // ..........................................................
            if (DispCard.PropDescription && Source is IFormalizedElement)
            {
                var Element = (IFormalizedElement)Source;

                // Description
                if (!Element.Description.IsAbsent())
                {
                    var TextDocument = Display.XamlRichTextTo(Element.Description, DataFormats.Html);
                    this.PageWriteTable("tbl_card_props", TextDocument.IntoEnumerable().IntoEnumerable(),
                                        Capsule.Create(FormalElement.__Description.TechName.ToHtmlEncoded(),
                                                       FormalElement.__Description.Name.ToHtmlEncoded(), 100.0));
                }

                // Classification (Pending)

                // Versioning
                if (Element.Version != null)
                {
                    TableColDefs.Clear();
                    TableRowVals.Clear();

                    var VerInfo = new Grid();
                    VerInfo.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.13, GridUnitType.Star) });    // Version Number
                    VerInfo.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.13, GridUnitType.Star) });    // Version Sequence
                    VerInfo.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.13, GridUnitType.Star) });    // Creation
                    VerInfo.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.24, GridUnitType.Star) });    // Creator
                    VerInfo.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.13, GridUnitType.Star) });    // Last Modification
                    VerInfo.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.24, GridUnitType.Star) });    // Last Modifier

                    TableColDefs.Add(Capsule.Create(VersionCard.__VersionNumber.TechName.ToHtmlEncoded(),
                                                    VersionCard.__VersionNumber.Name.ToHtmlEncoded(), 13.0));
                    TableRowVals.Add(Element.Version.VersionNumber.ToHtmlEncoded());

                    TableColDefs.Add(Capsule.Create(VersionCard.__VersionSequence.TechName.ToHtmlEncoded(),
                                                    VersionCard.__VersionSequence.Name.ToHtmlEncoded(), 13.0));
                    TableRowVals.Add(Element.Version.VersionSequence.ToString());

                    TableColDefs.Add(Capsule.Create(VersionCard.__Creation.TechName.ToHtmlEncoded(),
                                                    VersionCard.__Creation.Name.ToHtmlEncoded(), 13.0));
                    TableRowVals.Add(Element.Version.Creation.ToString().ToHtmlEncoded());

                    TableColDefs.Add(Capsule.Create(VersionCard.__Creator.TechName.ToHtmlEncoded(),
                                                    VersionCard.__Creator.Name.ToHtmlEncoded(), 24.0));
                    TableRowVals.Add(Element.Version.Creator.ToHtmlEncoded());

                    TableColDefs.Add(Capsule.Create(VersionCard.__LastModification.TechName.ToHtmlEncoded(),
                                                    VersionCard.__LastModification.Name.ToHtmlEncoded(), 13.0));
                    TableRowVals.Add(Element.Version.LastModification.ToString().ToHtmlEncoded());

                    TableColDefs.Add(Capsule.Create(VersionCard.__LastModifier.TechName.ToHtmlEncoded(),
                                                    VersionCard.__LastModifier.Name.ToHtmlEncoded(), 24.0));
                    TableRowVals.Add(Element.Version.LastModifier.ToHtmlEncoded());

                    this.PageWriteTable("tbl_card_props", TableRowVals.IntoEnumerable(), TableColDefs.ToArray());

                    if (!Element.Version.Annotation.IsAbsent())
                        this.PageWriteTable("tbl_card_props", Element.Version.Annotation.ToHtmlEncoded().IntoEnumerable().IntoEnumerable(),
                                            Capsule.Create(VersionCard.__Annotation.TechName.ToHtmlEncoded(),
                                                           VersionCard.__Annotation.Name.ToHtmlEncoded(), 100.0));
                }
            }

            if (DispCard.PropTechSpec && Source is ITechSpecifier)
            {
                var Element = (ITechSpecifier)Source;

                if (!Element.TechSpec.IsAbsent())
                    this.PageWriteTable("tbl_card_props", Element.TechSpec.ToHtmlEncoded().IntoEnumerable().IntoEnumerable(),
                                        Capsule.Create(FormalPresentationElement.__TechSpec.TechName.ToHtmlEncoded(),
                                                       FormalPresentationElement.__TechSpec.Name.ToHtmlEncoded(), 100.0));
            }
        }

        // public FrameworkElement CreateContentCardPropertyExpositor(string Title, object Value,
        //                                                           TextFormat TitleFormat, Brush TitleBackground,
        //                                                           TextFormat ValueFormat = null, Brush ValueBackground = null)
        //{
        //    var Result = new Border();
        //    Result.BorderBrush = Configuration.FmtCardLinesForeground;
        //    Result.BorderThickness = new Thickness(LINES_CARD_THICKNESS);
        //    Result.VerticalAlignment = VerticalAlignment.Stretch;
        //    // NO! it becomes very ugly... Result.HorizontalAlignment = HorizontalAlignment.Left;

        //    var Frame = new DockPanel();

        //    FrameworkElement TitleLabel = null;
            
        //    if (!Title.IsAbsent())
        //        TitleLabel = CreateText(Title, TitleFormat, HorizontalAlignment.Stretch, VerticalAlignment.Top,
        //                                TitleBackground);

        //    FrameworkElement ValueLabel = null;

        //    if (Value is ImageSource)
        //    {
        //        var PictureValue = this.CurrentWorker.AtOriginalThreadGetFrozen((ImageSource)Value);

        //        var PictureLabel = new Border();
        //        var SquareSize = Math.Min(PictureValue.Width, PictureValue.Height);
        //        var Picture = new Image();
        //        Picture.MaxWidth = SquareSize;
        //        Picture.MaxHeight = SquareSize;
        //        Picture.Source = PictureValue;
        //        Picture.Stretch = Stretch.Uniform;
        //        PictureLabel.Child = Picture;

        //        ValueLabel = PictureLabel;
        //    }
        //    else
        //        if (Value is FlowDocument)
        //        {
        //            var DocViewer = new FlowDocumentScrollViewer();
        //            DocViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
        //            DocViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
        //            DocViewer.VerticalAlignment = VerticalAlignment.Top;
        //            DocViewer.VerticalContentAlignment = VerticalAlignment.Top;
        //            DocViewer.Document = (FlowDocument)Value;
        //            ValueLabel = DocViewer;
        //        }
        //        else
        //            ValueLabel = CreateText(Value.ToStringAlways(), ValueFormat.NullDefault(TitleFormat), HorizontalAlignment.Stretch, VerticalAlignment.Top,
        //                                                            ValueBackground.NullDefault(TitleBackground));

        //    if (TitleLabel != null)
        //        Frame.Children.Add(TitleLabel);

        //    Frame.Children.Add(ValueLabel);

        //    if (TitleLabel != null)
        //        DockPanel.SetDock(TitleLabel, Dock.Top);

        //    Result.Child = Frame;

        //    return Result;
        //}

        // -----------------------------------------------------------------------------------------
    }
}
