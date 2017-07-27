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
using Instrumind.ThinkComposer.Model.InformationModel;
using Instrumind.ThinkComposer.Model.VisualModel;
using Instrumind.ThinkComposer.MetaModel;

/// Provides features for report generation.
namespace Instrumind.ThinkComposer.Composer.Reporting
{
    /// <summary>
    /// Generates comprehensive Standard reports from Compositions.
    /// </summary>
    public partial class ReportStandardGenerator
    {
        public const double LINES_VIEW_THICKNESS = 2.0;
        public const double LINES_CARD_THICKNESS = 1.0;
        public const double LINES_PROP_THICKNESS = 1.0;
        public const double LINES_LIST_THICKNESS = 1.0;
        public const double LINES_LISTROW_THICKNESS = 0.5;

        public const double CARD_TEXT_PADDING = 2.0;

        public const double COMPOSITE_NESTING_MARGIN = 20;

        public const double MAX_NESTED_LEVELS = 10;

        // -----------------------------------------------------------------------------------------
        public FixedPage CreateTitlePage(bool CountPage)
        {
            var Content = new StackPanel();
            Content.HorizontalAlignment = HorizontalAlignment.Stretch;
            Content.VerticalAlignment = VerticalAlignment.Center;

            var Text = this.CreateText(this.Configuration.Document_Title, this.Configuration.FmtMainTitle,
                                       InterpretTextAsVariable: true);
            Content.Children.Add(Text);
            Content.Children.Add(new Border { Width=10, Height=50 });   // Separator

            Text = this.CreateText(this.Configuration.Document_Subtitle, this.Configuration.FmtMainSubtitle,
                                   InterpretTextAsVariable: true);
            Content.Children.Add(Text);

            var Page = CreatePage(Content, true, CountPage);
            return Page;
        }

        // -----------------------------------------------------------------------------------------
        public IEnumerable<FixedPage> CreateTableOfContents()
        {
            var Result = new List<FixedPage>();

            // How to update pending fields (page-number and page-count)...
            // - Per each content page, return a framework-element (or more?)
            //   representing pending fields.
            // - Later create the table-of-content pages and count all the pages,
            //   also returning pending fields per page.
            // - Update the pending fields (possibly update-layout per each page).

            return Result;
        }

        // -----------------------------------------------------------------------------------------
        public void CreateCompositeContent(ReportStandardPagesMaker PagesMaker, Idea Source, double LocalNestingMargin,
                                           double ProgressPercentageStart, double ProgressPercentageEnd)
        {
            this.CurrentWorker.ReportProgress((int)ProgressPercentageStart, "Generating " + Source.ToStringAlways());

            PagesMaker.NestingMargin = LocalNestingMargin;

            var IdeaGroupKeySuffix = Source.GlobalId.ToString();

            var Text = CreateText(Source.Definitor.NameCaption /* Source.SelfKind.Name */ + ": " +
                                  Source.NameCaption, this.Configuration.FmtSubjectTitle);
            PagesMaker.AppendContent(Text, true, "Idea" + IdeaGroupKeySuffix);

            // Main Info
            var Card = (Source is Composition ? this.Configuration.Composition_Card
                                              : (Source is Concept ? this.Configuration.CompositeIdea_Concepts_Card
                                                                   : this.Configuration.CompositeIdea_Relationships_Card));

            if (Card.Show)
            {
                var CompositionCardSections = this.CreateContentCard(Source, Card);

                CompositionCardSections.ForEachIndexing(
                    (section, index) => PagesMaker.AppendContent(
                                            section, false, (index > 0 ? null : "Idea" + IdeaGroupKeySuffix)));
            }

            // View
            if (this.Configuration.CompositeIdea_View_Diagram
                && Source.CompositeActiveView != null)
            {
                var ViewSnapshot = this.CurrentWorker.AtOriginalThreadInvoke(
                    () =>
                    {
                        var Snapshot = Source.CompositeActiveView.ToSnapshot();
                        if (Snapshot == null || Snapshot.Item1 == null)
                            return null;

                        Snapshot.Item1.Freeze();
                        return Snapshot.Item1;
                    });

                if (ViewSnapshot != null)
                {
                    Text = CreateText(View.__ClassDefinitor.Name + ": " + Source.CompositeActiveView, this.Configuration.FmtSectionTitle);
                    PagesMaker.AppendContent(Text, true, "View" + IdeaGroupKeySuffix);

                    var ViewImage = ViewSnapshot.ToDrawingImage();
                    var ViewContent = this.CreateContentView(ViewImage, this.WorkingPageContentWidth, this.WorkingPageContentHeight * 0.95);

                    PagesMaker.AppendContent(ViewContent, false, "View" + IdeaGroupKeySuffix, true);
                }
            }

            // Details...
            var Details = Source.Details.Where(det => !(det is InternalLink)).OrderBy(det => det.Designation.Name);
            if (this.Configuration.CompositeIdea_Details && Details.Any())
            {
                Text = CreateText("Details", this.Configuration.FmtSectionTitle);
                PagesMaker.AppendContent(Text, true, "Details" + IdeaGroupKeySuffix);
                this.CreateIdeaDetails(PagesMaker, LocalNestingMargin, Details);
            }

            // Markers...
            if (this.Configuration.CompositeIdea_Markers_List.Show && Source.Markings.Count > 0)
            {
                Text = CreateText("Markers", this.Configuration.FmtSectionTitle);
                PagesMaker.AppendContent(Text, true, "Markers" + IdeaGroupKeySuffix);
                this.CreateListOfMarkers(PagesMaker, "Markers" + IdeaGroupKeySuffix,
                                         Source.Markings.OrderBy(reg => (reg.Descriptor == null ? "~" : reg.Descriptor.Name)));
            }

            // Related from...
            if (this.Configuration.CompositeIdea_RelatedFrom_Collection)
            {
                var OriginCounterpartLinks = Source.AssociatingLinks.Where(lnk => lnk.RoleDefinitor.RoleType == ERoleType.Target)
                                                .SelectMany(lnk => lnk.OwnerRelationship.Links.Where(rlk => rlk.RoleDefinitor.RoleType == ERoleType.Origin)
                                                                                                .Select(rlk => Tuple.Create(lnk, rlk)));

                var TargetCompanionLinks = Source.AssociatingLinks.Where(lnk => lnk.RoleDefinitor.RoleType == ERoleType.Target)
                                                .SelectMany(lnk => lnk.OwnerRelationship.Links.Where(rlk => rlk.RoleDefinitor.RoleType == ERoleType.Target
                                                                                                            && !rlk.AssociatedIdea.IsEqual(Source)));

                if (OriginCounterpartLinks.Any() || TargetCompanionLinks.Any())
                {
                    Text = CreateText("Related From...", this.Configuration.FmtSectionTitle);
                    PagesMaker.AppendContent(Text, true, "RelatedFrom" + IdeaGroupKeySuffix);
                }

                if (OriginCounterpartLinks.Any())
                    this.CreateCollectionOfIdeaCounterpartLinks(PagesMaker, ERoleType.Origin.GetFieldName(), ERoleType.Target.GetFieldName(),
                                                                OriginCounterpartLinks, "RelatedFrom" + IdeaGroupKeySuffix);

                if (this.Configuration.CompositeIdea_IncludeTargetCompanions && TargetCompanionLinks.Any())
                    this.CreateCollectionOfIdeaCompanionLinks(PagesMaker, TargetCompanionLinks, "TargetCompanions" + IdeaGroupKeySuffix);
            }

            // Related to...
            if (this.Configuration.CompositeIdea_RelatedTo_Collection)
            {
                var TargetCounterpartLinks = Source.AssociatingLinks.Where(lnk => lnk.RoleDefinitor.RoleType == ERoleType.Origin)
                                                .SelectMany(lnk => lnk.OwnerRelationship.Links.Where(rlk => rlk.RoleDefinitor.RoleType == ERoleType.Target)
                                                                                                .Select(rlk => Tuple.Create(lnk, rlk)));

                var OriginCompanionLinks = Source.AssociatingLinks.Where(lnk => lnk.RoleDefinitor.RoleType == ERoleType.Origin)
                                                .SelectMany(lnk => lnk.OwnerRelationship.Links.Where(rlk => rlk.RoleDefinitor.RoleType == ERoleType.Origin
                                                                                                            && !rlk.AssociatedIdea.IsEqual(Source)));

                if (TargetCounterpartLinks.Any() || OriginCompanionLinks.Any())
                {
                    Text = CreateText("Related To...", this.Configuration.FmtSectionTitle);
                    PagesMaker.AppendContent(Text, true, "RelatedTo" + IdeaGroupKeySuffix);
                }

                if (TargetCounterpartLinks.Any())
                    this.CreateCollectionOfIdeaCounterpartLinks(PagesMaker, ERoleType.Target.GetFieldName(), ERoleType.Origin.GetFieldName(),
                                                                TargetCounterpartLinks, "RelatedTo" + IdeaGroupKeySuffix);

                if (this.Configuration.CompositeIdea_IncludeOriginCompanions && OriginCompanionLinks.Any())
                    this.CreateCollectionOfIdeaCompanionLinks(PagesMaker, OriginCompanionLinks, "OriginCompanions" + IdeaGroupKeySuffix);
            }

            // Grouped Ideas...
            if (this.Configuration.CompositeIdea_GroupedIdeas_List.Show)
            {
                var GroupedIdeas = Source.VisualRepresentators
                    .SelectMany(vrep => vrep.MainSymbol.GetGroupedSymbols()
                        .Select(rep => rep.OwnerRepresentation.RepresentedIdea))
                            .Distinct().OrderBy(idea => idea.Name);

                if (GroupedIdeas.Any())
                {
                    Text = CreateText("Grouped Ideas", this.Configuration.FmtSectionTitle);
                    PagesMaker.AppendContent(Text, true, "GroupedIdeas" + IdeaGroupKeySuffix);
                    this.CreateContentList(PagesMaker, "GroupedIdeas" + IdeaGroupKeySuffix, GroupedIdeas,
                                           this.Configuration.CompositeIdea_GroupedIdeas_List,
                                           "Definition", "Definition");
                }
            }

            // Complements collection...
            if (this.Configuration.CompositeIdea_Complements)
            {
                // Consider symbol's appended floaters: call-outs and quotes
                var Complements = Source.VisualRepresentators
                                    .SelectMany(vrep => vrep.MainSymbol.AttachedComplements.Where(atc => atc.IsAttachedFloater));

                // Consider composite view's free complements (notes, texts, stamps)
                if (Source.CompositeViews.Count > 0)
                    Complements = Complements.Concat(Source.CompositeViews
                                    .SelectMany(cvw => cvw.GetFreeComplements().Where(frc =>
                                        frc.IsComplementNote || frc.IsComplementStamp || frc.IsComplementText)));

                if (Complements.Any())
                {
                    Text = CreateText("Complements", this.Configuration.FmtSectionTitle);
                    PagesMaker.AppendContent(Text, true, "IdeaComplements" + IdeaGroupKeySuffix);
                    this.CreateCollectionOfComplements(PagesMaker, "IdeaComplements" + IdeaGroupKeySuffix, Complements);
                }
            }

            // Relationship Links (only when this Idea is a Relationship)
            var SourceRelationship = Source as Relationship;
            if (SourceRelationship != null && SourceRelationship.Links.Count > 0
                && this.Configuration.CompositeRelationship_Links_Collection)
            {
                Text = CreateText("Links", this.Configuration.FmtSectionTitle);
                PagesMaker.AppendContent(Text, true, "RelLinks" + IdeaGroupKeySuffix);
                this.CreateCollectionOfRelationshipLinks(PagesMaker, SourceRelationship.Links, "RelLinks" + IdeaGroupKeySuffix);
            }

            // Composing Concepts List
            var Concepts = Source.CompositeConcepts.OrderBy(idea => idea.Name).ToList();
            if (this.Configuration.CompositeIdea_Concepts_List.Show && Concepts.Any())
            {
                Text = CreateText("List of composing Concepts", this.Configuration.FmtSectionTitle);
                PagesMaker.AppendContent(Text, true, "Concepts" + IdeaGroupKeySuffix);
                this.CreateContentList(PagesMaker, "Concepts" + IdeaGroupKeySuffix, Concepts,
                                       this.Configuration.CompositeIdea_Concepts_List,
                                       Concept.__ConceptDefinitor.TechName,
                                       Concept.__ConceptDefinitor.Name);
            }

            // Composing Relationships List
            var Relationships = Source.CompositeRelationships.OrderBy(idea => idea.Name).ToList();
            if (this.Configuration.CompositeIdea_Relationships_List.Show && Relationships.Any())
            {
                Text = CreateText("List of composing Relationships", this.Configuration.FmtSectionTitle);
                PagesMaker.AppendContent(Text, true, "Relationships" + IdeaGroupKeySuffix);
                this.CreateContentList(PagesMaker, "Relationships" + IdeaGroupKeySuffix, Relationships,
                                       this.Configuration.CompositeIdea_Relationships_List,
                                       Relationship.__RelationshipDefinitor.TechName,
                                       Relationship.__RelationshipDefinitor.Name);
            }

            // Determine progress
            var ProgressStep = ((ProgressPercentageEnd - ProgressPercentageStart) /
                                (double)(Concepts.Count + Relationships.Count));
            var ProgressPercentage = ProgressPercentageStart;

            // Composing Concepts Contents
            if (this.Configuration.CompositeIdea_Concepts_ReportCompositeContent && Concepts.Any())
            {
                Text = CreateText("Content of the composing Concepts", this.Configuration.FmtSectionTitle);
                PagesMaker.AppendContent(Text, true);
                foreach (var Composite in Concepts)
                {
                    ProgressPercentage += ProgressStep;
                    CreateCompositeContent(PagesMaker, Composite,
                                           (LocalNestingMargin + COMPOSITE_NESTING_MARGIN).EnforceMaximum(COMPOSITE_NESTING_MARGIN * MAX_NESTED_LEVELS),
                                           ProgressPercentage, (ProgressPercentage + ProgressStep));
                }
            }

            PagesMaker.NestingMargin = LocalNestingMargin;   // Reset to current-level nesting

            // Composing Relationships Contents
            if (this.Configuration.CompositeIdea_Relationships_ReportCompositeContent && Relationships.Any())
            {
                Text = CreateText("Content of the composing Relationships", this.Configuration.FmtSectionTitle);
                PagesMaker.AppendContent(Text, true);
                {
                    ProgressPercentage += ProgressStep;
                    foreach (var Composite in Relationships)
                        CreateCompositeContent(PagesMaker, Composite,
                                               (LocalNestingMargin + COMPOSITE_NESTING_MARGIN).EnforceMaximum(COMPOSITE_NESTING_MARGIN * MAX_NESTED_LEVELS),
                                               ProgressPercentage, (ProgressPercentage + ProgressStep));
                }
            }

            PagesMaker.NestingMargin = LocalNestingMargin;   // Reset to current-level nesting
        }

        // -----------------------------------------------------------------------------------------
        public IEnumerable<FixedPage> CreateDomainContent(ReportStandardPagesMaker PagesMaker, Domain Source)
        {
            var Result = new List<FixedPage>();

            PagesMaker.AppendPageBreak();
            PagesMaker.NestingMargin = 0;
            PagesMaker.AppendContent(CreateText("Domain: " + Source.NameCaption,
                                                this.Configuration.FmtSubjectTitle), true, "Domain");

            if (this.Configuration.Domain_Concept_Defs_List.Show)
            {
                PagesMaker.AppendContent(CreateText("Concept Definitions", this.Configuration.FmtSectionTitle), true, "ConceptDefs");
                this.CreateContentList(PagesMaker, "ConceptDefs", Source.ConceptDefinitions,
                                       this.Configuration.Domain_Concept_Defs_List, null, null,
                                       new Tuple<string, string, double, Func<IMModelClass, object>>("Cluster", "Cluster", 0.15,
                                                    ins => Source.ConceptDefClusters.FirstOrDefault(clu => clu == ((IdeaDefinition)ins).Cluster)
                                                                                        .Get(clu => clu.NameCaption, "")));
            }

            if (this.Configuration.Domain_Relationship_Defs_List.Show)
            {
                PagesMaker.AppendContent(CreateText("Relationship Definitions", this.Configuration.FmtSectionTitle), true, "RelationshipDefs");
                this.CreateContentList(PagesMaker, "RelationshipDefs", Source.RelationshipDefinitions,
                                       this.Configuration.Domain_Relationship_Defs_List, null, null,
                                       new Tuple<string, string, double, Func<IMModelClass, object>>("Cluster", "Cluster", 0.15,
                                                    ins => Source.RelationshipDefClusters.FirstOrDefault(clu => clu == ((IdeaDefinition)ins).Cluster)
                                                                                        .Get(clu => clu.NameCaption, "")));
            }

            if (this.Configuration.Domain_Marker_Defs_List.Show)
            {
                PagesMaker.AppendContent(CreateText("Marker Definitions", this.Configuration.FmtSectionTitle), true, "MarkerDefs");
                this.CreateContentList(PagesMaker, "MarkerDefs", Source.MarkerDefinitions,
                                       this.Configuration.Domain_Marker_Defs_List, null, null);
            }

            return Result;
        }

        // -----------------------------------------------------------------------------------------
        public FrameworkElement CreateContentView(ImageSource Source, double MaxWidth, double MaxHeight)
        {
            // NOTE: Do not apply Max-Width and Max-Height to border, because does not work when measuring page.
            var Result = new Border();

            Result.BorderBrush = this.Configuration.FmtViewLinesForeground;
            Result.BorderThickness = new Thickness(LINES_VIEW_THICKNESS);
            Result.Padding = new Thickness(1);

            var Content = new Image();
            Content.Source = Source;
            Content.Stretch = Stretch.Uniform;
            Content.MaxWidth = MaxWidth;
            Content.MaxHeight = MaxHeight;

            Result.Child = Content;

            return Result;
        }

        // -----------------------------------------------------------------------------------------
    }
}