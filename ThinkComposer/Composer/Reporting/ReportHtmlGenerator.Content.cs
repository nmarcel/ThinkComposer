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
using Instrumind.ThinkComposer.Model.InformationModel;
using Instrumind.ThinkComposer.Model.VisualModel;
using Instrumind.ThinkComposer.MetaModel;

/// Provides features for report generation.
namespace Instrumind.ThinkComposer.Composer.Reporting
{
    /// <summary>
    /// Generates comprehensive HTML reports from Compositions.
    /// </summary>
    public partial class ReportHtmlGenerator
    {
        public const double LINES_VIEW_THICKNESS = 2.0;
        public const double LINES_CARD_THICKNESS = 1.0;
        public const double LINES_PROP_THICKNESS = 1.0;
        public const double LINES_LIST_THICKNESS = 1.0;
        public const double LINES_LISTROW_THICKNESS = 0.5;

        public const double CARD_TEXT_PADDING = 2.0;

        public const string TEMP_REF_PREFIX = "$REF:";

        // -----------------------------------------------------------------------------------------
        public void CreateTitleSection(bool CountPage)
        {
            this.PageWrite("<br />");
            this.PageWrite("<div id='main-title'>");
                this.IncreaseIndent();
                this.PageWrite("<h1>" + InterpretText(this.Configuration.Document_Title) + "</h1>");
                this.DecreaseIndent();
            this.PageWrite("</div>");
            this.PageWrite("<br />");

            this.PageWrite("<br />");

            this.PageWrite("<br />");
            this.PageWrite("<div id='main-subtitle'>");
                this.IncreaseIndent();
                this.PageWrite("<h2>" + InterpretText(this.Configuration.Document_Subtitle) + "</h2>");
                this.DecreaseIndent();
            this.PageWrite("</div>");
            this.PageWrite("<br />");
        }

        // -----------------------------------------------------------------------------------------
        public void CreateTableOfContents()
        {
            // How to update pending fields (page-number and page-count)...
            // - Per each content page, return a framework-element (or more?)
            //   representing pending fields.
            // - Later create the table-of-content pages and count all the pages,
            //   also returning pending fields per page.
            // - Update the pending fields (possibly update-layout per each page).
        }

        // -----------------------------------------------------------------------------------------
        public void CreateCompositeContent(Idea Source, double ProgressPercentageStart, double ProgressPercentageEnd)
        {
            this.CurrentWorker.ReportProgress((int)ProgressPercentageStart, "Generating " + Source.ToStringAlways());

            this.PageWrite("<br/>");
            this.PageWrite("<br/>");
            this.CreateTextLabel("subject-title", Source.Definitor.NameCaption + ": " + // Source.SelfKind.Name
                                                  Source.NameCaption);

            // Main Info
            var Card = (Source is Composition ? this.Configuration.Composition_Card
                                              : (Source is Concept ? this.Configuration.CompositeIdea_Concepts_Card
                                                                   : this.Configuration.CompositeIdea_Relationships_Card));

            if (Card.Show)
                this.CreateContentCard(Source, Card);

            // .................. COMPOSITES *CONTENT* CREATION ................................
            // Determine Concepts and Relationships to expose
            var Concepts = Source.CompositeConcepts.OrderBy(idea => idea.Name).ToList();
            var Relationships = Source.CompositeRelationships.OrderBy(idea => idea.Name).ToList();

            // Determine progress
            var ProgressStep = ((ProgressPercentageEnd - ProgressPercentageStart) /
                            (double)(Concepts.Count + Relationships.Count));
            var ProgressPercentage = ProgressPercentageStart;

            if (this.Configuration.CompositeIdea_Concepts_ReportCompositeContent && Concepts.Any())
                foreach (var Composite in Concepts)
                {
                    var Page = this.PushPage(Composite);

                    ProgressPercentage += ProgressStep;
                    CreateCompositeContent(Composite, ProgressPercentage, (ProgressPercentage + ProgressStep));

                    this.PopPage();
                    this.PrepareHtmlDocument(Page);
                }

            // Composing Relationships Contents
            if (this.Configuration.CompositeIdea_Relationships_ReportCompositeContent && Relationships.Any())
                foreach (var Composite in Relationships)
                {
                    var Page = this.PushPage(Composite);

                    ProgressPercentage += ProgressStep;
                    CreateCompositeContent(Composite, ProgressPercentage, (ProgressPercentage + ProgressStep));

                    this.PopPage();
                    this.PrepareHtmlDocument(Page);
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
                        return Snapshot;
                    });

                if (ViewSnapshot != null)
                {
                    this.PageWrite("<br/>");
                    this.CreateTextLabel("section-title", (View.__ClassDefinitor.Name + ": " + Source.CompositeActiveView.ToString()).ToHtmlEncoded());
                    this.CreateContentView(Source.CompositeActiveView, ViewSnapshot);
                }
            }

            // Details...
            var Details = Source.Details.Where(det => !(det is InternalLink)).OrderBy(det => det.Designation.Name);
            if (this.Configuration.CompositeIdea_Details && Details.Any())
            {
                this.PageWrite("<br/>");
                this.CreateTextLabel("section-title", "Details");
                this.CreateIdeaDetails(Details);
            }

            // Markers...
            if (this.Configuration.CompositeIdea_Markers_List.Show && Source.Markings.Count > 0)
            {
                this.PageWrite("<br/>");
                this.CreateTextLabel("section-title", "Markers");
                this.CreateListOfMarkers(Source, Source.Markings.OrderBy(reg => (reg.Descriptor == null ? "~" : reg.Descriptor.Name)));
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
                    this.PageWrite("<br/>");
                    this.CreateTextLabel("section-title", "Related From...");
                }

                if (OriginCounterpartLinks.Any())
                    this.CreateCollectionOfIdeaCounterpartLinks(ERoleType.Origin.GetFieldName(), ERoleType.Target.GetFieldName(),
                                                                OriginCounterpartLinks);

                if (this.Configuration.CompositeIdea_IncludeTargetCompanions && TargetCompanionLinks.Any())
                    this.CreateCollectionOfIdeaCompanionLinks(TargetCompanionLinks);
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
                    this.PageWrite("<br/>");
                    this.CreateTextLabel("section-title", "Related To...");
                }

                if (TargetCounterpartLinks.Any())
                    this.CreateCollectionOfIdeaCounterpartLinks(ERoleType.Target.GetFieldName(), ERoleType.Origin.GetFieldName(),
                                                                TargetCounterpartLinks);

                if (this.Configuration.CompositeIdea_IncludeOriginCompanions && OriginCompanionLinks.Any())
                    this.CreateCollectionOfIdeaCompanionLinks(OriginCompanionLinks);
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
                    this.PageWrite("<br/>");
                    this.CreateTextLabel("section-title", "Grouped Ideas");
                    this.CreateContentList(GroupedIdeas, Source,
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
                    this.PageWrite("<br/>");
                    this.CreateTextLabel("section-title", "Complements");
                    this.CreateCollectionOfComplements(Complements);
                }
            }

            // Relationship Links (only when this Idea is a Relationship)
            var SourceRelationship = Source as Relationship;
            if (SourceRelationship != null && SourceRelationship.Links.Count > 0
                && this.Configuration.CompositeRelationship_Links_Collection)
            {
                this.PageWrite("<br/>");
                this.CreateTextLabel("section-title", "Links");
                this.CreateCollectionOfRelationshipLinks(SourceRelationship.Links);
            }

            // .................. COMPOSITES *LIST* SEGMENT ................................
            if (this.Configuration.CompositeIdea_Concepts_List.Show && Concepts.Any())
            {
                this.PageWrite("<br/>");
                this.CreateTextLabel("section-title", "List of composing Concepts");
                this.CreateContentList(Concepts, Source,
                                       this.Configuration.CompositeIdea_Concepts_List,
                                       Concept.__ConceptDefinitor.TechName,
                                       Concept.__ConceptDefinitor.Name);
            }

            // Composing Relationships List
            if (this.Configuration.CompositeIdea_Relationships_List.Show && Relationships.Any())
            {
                this.PageWrite("<br/>");
                this.CreateTextLabel("section-title", "List of composing Relationships");
                this.CreateContentList(Relationships, Source,
                                       this.Configuration.CompositeIdea_Relationships_List,
                                       Relationship.__RelationshipDefinitor.TechName,
                                       Relationship.__RelationshipDefinitor.Name);
            }
        }

        // -----------------------------------------------------------------------------------------
        public void CreateDomainContent(Domain Source)
        {
            this.PageWrite("<br/>");
            this.PageWrite("<br/>");
            this.PageWrite("<div id='subject-title'>Domain: " + Source.NameCaption + "</div>");
            this.PageWrite("<br/>");

            if (this.Configuration.Domain_Concept_Defs_List.Show)
            {
                this.PageWrite("<br/>");
                this.CreateTextLabel("section-title", "Concept Definitions");
                this.CreateContentList(Source.ConceptDefinitions, Source.OwnerComposition, this.Configuration.Domain_Concept_Defs_List, null, null,
                                       Source.TechName, "ConceptDefinitions",
                                       new Tuple<string, string, double, Func<IMModelClass, object>>("Cluster", "Cluster", 15.0,
                                                    ins => Source.ConceptDefClusters.FirstOrDefault(clu => clu == ((IdeaDefinition)ins).Cluster)
                                                                                        .Get(clu => clu.NameCaption, "")));
            }

            if (this.Configuration.Domain_Relationship_Defs_List.Show)
            {
                this.PageWrite("<br/>");
                this.CreateTextLabel("section-title", "Relationship Definitions");
                this.CreateContentList(Source.RelationshipDefinitions, Source.OwnerComposition, this.Configuration.Domain_Relationship_Defs_List, null, null,
                                       Source.TechName, "RelationshipDefinitions",
                                       new Tuple<string, string, double, Func<IMModelClass, object>>("Cluster", "Cluster", 15.0,
                                                    ins => Source.RelationshipDefClusters.FirstOrDefault(clu => clu == ((IdeaDefinition)ins).Cluster)
                                                                                        .Get(clu => clu.NameCaption, "")));
            }

            if (this.Configuration.Domain_Marker_Defs_List.Show)
            {
                this.PageWrite("<br/>");
                this.CreateTextLabel("section-title", "Marker Definitions");
                this.CreateContentList(Source.MarkerDefinitions, Source.OwnerComposition, this.Configuration.Domain_Marker_Defs_List, null, null,
                                       Source.TechName, "MarkerDefinitions");
            }
        }

        // -----------------------------------------------------------------------------------------
        public void CreateContentView(View SourceView, Tuple<DrawingGroup, Rect> SourceSnapshot)
        {
            this.CreateUniqueRelativeLocation(SourceView.OwnerCompositeContainer, SourceView.TechName,
                                              (SourceView.TechName + ".png").TextToUrlIdentifier());
            var RefName = this.GetRelativeLocationOf(SourceView.OwnerCompositeContainer, SourceView.TechName);

            var Location = this.GetPhysicalLocationOf(SourceView.OwnerCompositeContainer, SourceView.TechName);
            var SourceImage = this.CurrentWorker.AtOriginalThreadGetFrozen(SourceSnapshot.Item1);
            var Content = SourceImage.RenderToDrawingVisual();    // .RenderToBitmap((int)SourceImage.GetWidth(), (int)SourceImage.GetHeight()).ToBytes();

            this.PrepareToWrite(Location);
            Display.ExportImageTo(Location, Content, (int)SourceSnapshot.Item2.Width, (int)SourceSnapshot.Item2.Height);

            // Generate Map for view Ideas
            var Ideas = SourceView.ViewChildren.Select(vch => vch.Key).CastAs<VisualSymbol, object>()
                                                .Select(vsm => vsm.OwnerRepresentation.RepresentedIdea);

            var Areas = new List<Capsule<Rect, Idea>>();

            foreach (var Child in SourceView.ViewChildren)
            {
                var Symbol = Child.Key as VisualSymbol;
                if (Symbol == null)
                    continue;

                Areas.Add(Capsule.Create(Symbol.TotalArea, Symbol.OwnerRepresentation.RepresentedIdea));
            }

            var MapName = (SourceView.OwnerCompositeContainer.TechName + "_" +
                           SourceView.TechName + "_map").TextToUrlIdentifier();

            this.PageWrite("<map name='" + MapName + "'>");
                this.IncreaseIndent();
                foreach (var Area in Areas)
                {
                    Area.Value0 = new Rect(Math.Round(Area.Value0.X - SourceSnapshot.Item2.X, 0),
                                           Math.Round(Area.Value0.Y - SourceSnapshot.Item2.Y, 0),
                                           Math.Round(Area.Value0.Width, 0), Math.Round(Area.Value0.Height, 0));

                    var AreaName = Area.Value1.NameCaption.ToHtmlEncoded();
                    var AreaHRef = this.GetRelativeLocationOf(Area.Value1);

                    if (AreaHRef == null)
                        AreaHRef = TEMP_REF_PREFIX + Guid.NewGuid().ToString();

                    var Line = this.PageWrite("<area href='" + AreaHRef + "'");
                    if (AreaHRef.StartsWith(TEMP_REF_PREFIX))
                        this.RegisterPendingReference(Line, AreaHRef, Area.Value1);

                    this.PageWrite("      alt='" + AreaName + "' title='" + AreaName + "'");
                    this.PageWrite("      shape=rect coords='" +
                                   Area.Value0.X.ToString() + "," + Area.Value0.Y.ToString() + "," +
                                   (Area.Value0.X + Area.Value0.Width).ToString() + "," +
                                   (Area.Value0.Y + Area.Value0.Height).ToString() + "'/>");
                }
                this.DecreaseIndent();
            this.PageWrite("</map>");

            // Generate Image reference
            var ImgAlt = (SourceView.OwnerCompositeContainer.NameCaption + " - " + SourceView.NameCaption).ToHtmlEncoded();
            this.PageWrite("<img alt='" + ImgAlt + "' Src='" + RefName + "' usemap='#" + MapName + "'>");
        }

        public string CreateImage(ImageSource SourceImage, IRecognizableComposite Source,
                                  string MemberName, IRecognizableComposite Requester = null)
        {
            var Result = this.CreateImage(SourceImage, Source, Source, MemberName, Requester);
            return Result;
        }

        public string CreateImage(ImageSource SourceImage, IRecognizableComposite SourceOwner, object Source,
                                  string MemberName, IRecognizableComposite Requester = null)
        {
            if (SourceImage == null)
                return "";

            var Creation = (SourceOwner == Source 
                            ? this.CreateUniqueRelativeLocationDirect(SourceOwner, MemberName, MemberName + ".png")
                            : this.CreateUniqueRelativeLocation(SourceOwner, Source, MemberName,
                                    /*+? Source.ToStringAlways().TextToUrlIdentifier() + "." + */ MemberName + ".png"));

            var RefName = this.GetRelativeLocationOf(Source, MemberName, Requester);

            if (Creation)
            {
                var Location = this.GetPhysicalLocationOf(Source, MemberName);
                var Content = SourceImage.ToVisual();    // .RenderToBitmap((int)SourceImage.GetWidth(), (int)SourceImage.GetHeight()).ToBytes();

                this.PrepareToWrite(Location);
                Display.ExportImageTo(Location, Content, (int)SourceImage.GetWidth(), (int)SourceImage.GetHeight());
            }

            return RefName;
        }

        // -----------------------------------------------------------------------------------------
    }
}