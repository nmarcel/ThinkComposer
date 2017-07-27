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
    /// Generates comprehensive Standard reports from Compositions.
    /// </summary>
    public partial class ReportStandardGenerator
    {
        // -----------------------------------------------------------------------------------------
        public IList<FrameworkElement> CreateContentCard(IIdentifiableElement Source, DisplayCard DispCard)
        {
            var Result = new List<FrameworkElement>();

            var BasicInfo = new DockPanel();

            if (DispCard.PropName)
            {
                var Expositor = CreateContentCardPropertyExpositor(FormalPresentationElement.__Name.Name, Source.Name.RemoveNewLines(),
                                                                   this.Configuration.FmtCardFieldLabel, this.Configuration.FmtFieldLabelBackground,
                                                                   this.Configuration.FmtCardFieldValue, this.Configuration.FmtFieldValueBackground);
                BasicInfo.Children.Add(Expositor);
                DockPanel.SetDock(Expositor, Dock.Top);
            }

            if (DispCard.PropTechName)
            {
                var Expositor = CreateContentCardPropertyExpositor(FormalPresentationElement.__TechName.Name, Source.TechName,
                                                                   this.Configuration.FmtCardFieldLabel, this.Configuration.FmtFieldLabelBackground,
                                                                   this.Configuration.FmtCardFieldValue, this.Configuration.FmtFieldValueBackground);
                BasicInfo.Children.Add(Expositor);
                DockPanel.SetDock(Expositor, Dock.Top);
            }

            if (DispCard.PropSummary)
            {
                var Expositor = CreateContentCardPropertyExpositor(FormalPresentationElement.__Summary.Name, Source.Summary,
                                                                   this.Configuration.FmtCardFieldLabel, this.Configuration.FmtFieldLabelBackground,
                                                                   this.Configuration.FmtCardFieldValue, this.Configuration.FmtFieldValueBackground);
                BasicInfo.Children.Add(Expositor);
                DockPanel.SetDock(Expositor, Dock.Top);
            }

            var MainInfo = new Grid();
            var ColDef = new ColumnDefinition();
            MainInfo.ColumnDefinitions.Add(ColDef); // new ColumnDefinition { Width = new GridLength(0.8, GridUnitType.Star) });
            // MainInfo.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.2, GridUnitType.Star) });

            MainInfo.Children.Add(BasicInfo);
            Grid.SetColumn(BasicInfo, 0);

            if (DispCard.PropPictogram && Source is IRecognizableElement)
            {
                ColDef.Width = new GridLength(0.8, GridUnitType.Star);
                MainInfo.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.2, GridUnitType.Star) });

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

                var PictureExpositor = CreateContentCardPropertyExpositor(FormalPresentationElement.__Pictogram.Name, LocalPicture,
                                                                          this.Configuration.FmtCardFieldLabel, this.Configuration.FmtFieldLabelBackground,
                                                                          this.Configuration.FmtCardFieldValue, this.Configuration.FmtFieldValueBackground,
                                                                          ReportConfiguration.PICTOGRAM_MAX_WIDTH, ReportConfiguration.PICTOGRAM_MAX_HEIGHT);
                MainInfo.Children.Add(PictureExpositor);
                Grid.SetColumn(PictureExpositor, 1);
            }

            // ..........................................................
            var IdInfo = new List<FrameworkElement>();

            var DefinitorAndGlobalIdPanel = new DockPanel();

            if (DispCard.PropGlobalId && Source is IUniqueElement)
            {
                var Expositor = CreateContentCardPropertyExpositor(UniqueElement.__GlobalId.Name, ((IUniqueElement)Source).GlobalId,
                                                                   this.Configuration.FmtCardFieldLabel, this.Configuration.FmtFieldLabelBackground,
                                                                   this.Configuration.FmtCardFieldValue, this.Configuration.FmtFieldValueBackground);
                DefinitorAndGlobalIdPanel.Children.Add(Expositor);
                DockPanel.SetDock(Expositor, Dock.Right);
            }

            if (DispCard.Definitor && Source is Idea)
            {
                var Expositor = CreateContentCardPropertyExpositor(((Idea)Source).BaseKind.Name,
                                                                   ((Idea)Source).IdeaDefinitor.Name,
                                                                   this.Configuration.FmtCardFieldLabel, this.Configuration.FmtFieldLabelBackground,
                                                                   this.Configuration.FmtCardFieldValue, this.Configuration.FmtFieldValueBackground);
                DefinitorAndGlobalIdPanel.Children.Add(Expositor);
            }

            if (DispCard.Definitor && Source is View)
            {
                var Expositor = CreateContentCardPropertyExpositor(View.__ClassDefinitor.Name,
                                                                   ((Idea)Source).IdeaDefinitor.Name,
                                                                   this.Configuration.FmtCardFieldLabel, this.Configuration.FmtFieldLabelBackground,
                                                                   this.Configuration.FmtCardFieldValue, this.Configuration.FmtFieldValueBackground);
                DefinitorAndGlobalIdPanel.Children.Add(Expositor);
            }

            if (DefinitorAndGlobalIdPanel.Children.Count > 0)
                IdInfo.Add(DefinitorAndGlobalIdPanel);

            if (DispCard.Route & Source is Idea)
            {
                var Route = ((Idea)Source).GetContainmentRoute();
                var Expositor = CreateContentCardPropertyExpositor("Route", Route,
                                                                   this.Configuration.FmtCardFieldLabel, this.Configuration.FmtFieldLabelBackground,
                                                                   this.Configuration.FmtCardFieldValue, this.Configuration.FmtFieldValueBackground);
                IdInfo.Add(Expositor);
            }

            // ..........................................................
            var Extras = new List<FrameworkElement>();

            if (DispCard.PropDescription && Source is IFormalizedElement)
            {
                var Element = (IFormalizedElement)Source;

                // Description
                if (!Element.Description.IsAbsent())
                {
                    var TextDocument = Display.XamlRichTextToFlowDocument(Element.Description);
                    /*T var Range = new TextRange(TextDocument.ContentStart, TextDocument.ContentEnd);
                    var PlainText = Range.Text; */

                    var Expositor = CreateContentCardPropertyExpositor(FormalElement.__Description.Name, TextDocument,
                                                                       this.Configuration.FmtCardFieldLabel, this.Configuration.FmtFieldLabelBackground,
                                                                       this.Configuration.FmtCardFieldValue, this.Configuration.FmtFieldValueBackground);
                    Extras.Add(Expositor);
                }

                // Classification (Pending)

                // Versioning
                if (Element.Version != null)
                {
                    var VerPanel = new List<FrameworkElement>();

                    var VerInfo = new Grid();
                    VerInfo.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.13, GridUnitType.Star) });    // Version Number
                    VerInfo.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.13, GridUnitType.Star) });    // Version Sequence
                    VerInfo.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.13, GridUnitType.Star) });    // Creation
                    VerInfo.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.24, GridUnitType.Star) });    // Creator
                    VerInfo.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.13, GridUnitType.Star) });    // Last Modification
                    VerInfo.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.24, GridUnitType.Star) });    // Last Modifier

                    var Expositor = CreateContentCardPropertyExpositor(VersionCard.__VersionNumber.Name, Element.Version.VersionNumber,
                                                                       this.Configuration.FmtCardFieldLabel, this.Configuration.FmtFieldLabelBackground,
                                                                       this.Configuration.FmtCardFieldValue, this.Configuration.FmtFieldValueBackground);
                    VerInfo.Children.Add(Expositor);
                    Grid.SetColumn(Expositor, 0);

                    Expositor = CreateContentCardPropertyExpositor(VersionCard.__VersionSequence.Name, Element.Version.VersionSequence,
                                                                   this.Configuration.FmtCardFieldLabel, this.Configuration.FmtFieldLabelBackground,
                                                                   this.Configuration.FmtCardFieldValue, this.Configuration.FmtFieldValueBackground);
                    VerInfo.Children.Add(Expositor);
                    Grid.SetColumn(Expositor, 1);

                    Expositor = CreateContentCardPropertyExpositor(VersionCard.__Creation.Name, Element.Version.Creation,
                                                                   this.Configuration.FmtCardFieldLabel, this.Configuration.FmtFieldLabelBackground,
                                                                   this.Configuration.FmtCardFieldValue, this.Configuration.FmtFieldValueBackground);
                    VerInfo.Children.Add(Expositor);
                    Grid.SetColumn(Expositor, 2);

                    Expositor = CreateContentCardPropertyExpositor(VersionCard.__Creator.Name, Element.Version.Creator,
                                                                   this.Configuration.FmtCardFieldLabel, this.Configuration.FmtFieldLabelBackground,
                                                                   this.Configuration.FmtCardFieldValue, this.Configuration.FmtFieldValueBackground);
                    VerInfo.Children.Add(Expositor);
                    Grid.SetColumn(Expositor, 3);

                    Expositor = CreateContentCardPropertyExpositor(VersionCard.__LastModification.Name, Element.Version.LastModification,
                                                                   this.Configuration.FmtCardFieldLabel, this.Configuration.FmtFieldLabelBackground,
                                                                   this.Configuration.FmtCardFieldValue, this.Configuration.FmtFieldValueBackground);
                    VerInfo.Children.Add(Expositor);
                    Grid.SetColumn(Expositor, 4);

                    Expositor = CreateContentCardPropertyExpositor(VersionCard.__LastModifier.Name, Element.Version.LastModifier,
                                                                   this.Configuration.FmtCardFieldLabel, this.Configuration.FmtFieldLabelBackground,
                                                                   this.Configuration.FmtCardFieldValue, this.Configuration.FmtFieldValueBackground);
                    VerInfo.Children.Add(Expositor);
                    Grid.SetColumn(Expositor, 5);

                    VerPanel.Add(VerInfo);

                    if (!Element.Version.Annotation.IsAbsent())
                    {
                        Expositor = CreateContentCardPropertyExpositor(VersionCard.__Annotation.Name, Element.Version.Annotation,
                                                                       this.Configuration.FmtCardFieldLabel, this.Configuration.FmtFieldLabelBackground,
                                                                       this.Configuration.FmtCardFieldValue, this.Configuration.FmtFieldValueBackground);

                        VerPanel.Add(Expositor);
                    }

                    Extras.AddRange(VerPanel);
                }
            }

            if (DispCard.PropTechSpec && Source is ITechSpecifier)
            {
                var Element = (ITechSpecifier)Source;

                if (!Element.TechSpec.IsAbsent())
                {
                    var Expositor = CreateContentCardPropertyExpositor(FormalPresentationElement.__TechSpec.Name, Element.TechSpec,
                                                                       this.Configuration.FmtCardFieldLabel, this.Configuration.FmtFieldLabelBackground,
                                                                       this.Configuration.FmtCardFieldValue, this.Configuration.FmtFieldValueBackground);
                    Extras.Add(Expositor);
                }
            }

            // ..........................................................
            Result.Add(MainInfo);
            Result.AddRange(IdInfo);
            Result.AddRange(Extras);

            return Result;
        }

        public FrameworkElement CreateContentCardPropertyExpositor(string Title, object Value,
                                                                   TextFormat TitleFormat, Brush TitleBackground,
                                                                   TextFormat ValueFormat = null, Brush ValueBackground = null,
                                                                   double MaxValueWidth = double.PositiveInfinity,
                                                                   double MaxValueHeight = double.PositiveInfinity)
        {
            var Result = new Border();
            Result.BorderBrush = Configuration.FmtCardLinesForeground;
            Result.BorderThickness = new Thickness(LINES_CARD_THICKNESS);
            Result.VerticalAlignment = VerticalAlignment.Stretch;
            // NO! it becomes very ugly... Result.HorizontalAlignment = HorizontalAlignment.Left;

            var Frame = new DockPanel();

            FrameworkElement TitleLabel = null;
            
            if (!Title.IsAbsent())
                TitleLabel = CreateText(Title, TitleFormat, HorizontalAlignment.Stretch, VerticalAlignment.Top,
                                        TitleBackground);

            FrameworkElement ValueLabel = null;

            if (Value is ImageSource)
            {
                var PictureValue = this.CurrentWorker.AtOriginalThreadGetFrozen((ImageSource)Value);

                var PictureLabel = new Border();
                var SquareSize = Math.Min(PictureValue.Width, PictureValue.Height);
                var Picture = new Image();
                Picture.MaxWidth = SquareSize;
                Picture.MaxHeight = SquareSize;
                Picture.Source = PictureValue;
                Picture.Stretch = Stretch.Uniform;
                PictureLabel.Child = Picture;

                ValueLabel = PictureLabel;
            }
            else
                if (Value is FlowDocument)
                {
                    var DocViewer = new FlowDocumentScrollViewer();
                    DocViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                    DocViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                    DocViewer.VerticalAlignment = VerticalAlignment.Top;
                    DocViewer.VerticalContentAlignment = VerticalAlignment.Top;
                    DocViewer.Document = (FlowDocument)Value;
                    ValueLabel = DocViewer;
                }
                else
                    ValueLabel = CreateText(Value.ToStringAlways(), ValueFormat.NullDefault(TitleFormat), HorizontalAlignment.Stretch, VerticalAlignment.Top,
                                                                    ValueBackground.NullDefault(TitleBackground));

            ValueLabel.MaxWidth = MaxValueWidth;
            ValueLabel.MaxHeight = MaxValueHeight;

            if (TitleLabel != null)
                Frame.Children.Add(TitleLabel);

            Frame.Children.Add(ValueLabel);

            if (TitleLabel != null)
                DockPanel.SetDock(TitleLabel, Dock.Top);

            Result.Child = Frame;

            return Result;
        }

        // -----------------------------------------------------------------------------------------
    }
}
