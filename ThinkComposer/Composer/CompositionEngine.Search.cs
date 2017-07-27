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
// File   : CompositionEngine.Clipboard.cs
// Object : Instrumind.ThinkComposer.Composer.CompositionEngine (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2010.06.01 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;
using Instrumind.Common.Visualization.Widgets;

using Instrumind.ThinkComposer;
using Instrumind.ThinkComposer.ApplicationProduct;
using Instrumind.ThinkComposer.Definitor;
using Instrumind.ThinkComposer.Composer.ComposerUI;
using Instrumind.ThinkComposer.Composer.ComposerUI.Widgets;
using Instrumind.ThinkComposer.MetaModel;
using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.MetaModel.InformationMetaModel;
using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;
using Instrumind.ThinkComposer.Model;
using Instrumind.ThinkComposer.Model.GraphModel;
using Instrumind.ThinkComposer.Model.InformationModel;
using Instrumind.ThinkComposer.Model.VisualModel;

/// Provides edition, processing and dynamic in-memory storage access for Composition Graphs of Ideas (concepts and relationships) and its Visual representation.
namespace Instrumind.ThinkComposer.Composer
{
    /// <summary>
    /// Takes care of the edition of a particular Composition instance (Transfer operations partial-file).
    /// </summary>
    public partial class CompositionEngine : DocumentEngine
    {
        public static string TextExtractorWithoutPropertyAccessor(IMModelClass Target)
        {
            var Source = Target as VisualComplement;
            var Text = Source.GetPropertyField<string>(VisualComplement.PROP_FIELD_TEXT);
            return Text;
        }

        public static string ObjectTypeDetector(IMModelClass SourceObject)
        {
            string Result = null;

            var SourceClass = SourceObject as IMModelClass;
            if (SourceClass == null)
                Result = SourceObject.GetType().Name;
            else
            {
                var SourceIdea = SourceObject as Idea;
                if (SourceIdea == null)
                    Result = SourceClass.ClassDefinition.Name;
                else
                    Result = SourceIdea.IdeaDefinitor.Name + " (" + SourceClass.ClassDefinition.Name + ")";
            }

            if (Result == SimplePresentationElement.__ClassDefinitor.TechName)
            {
                var Engine = ApplicationProduct.ProductDirector.WorkspaceDirector.ActiveDocumentEngine as CompositionEngine;

                // Too costly?
                if (Engine != null && Engine.TargetComposition.CompositeContentDomain.LinkRoleVariants.Contains(SourceObject))
                    Result = Domain.__LinkRoleVariants.Name;
            }

            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Finds objects, within certain scope, having the specified text within the specified properties.
        /// Returns hints on the found objects.
        /// </summary>
        public IList<FoundObjectHint> FindTextInObjects(string SearchedText,
                                                        bool IsCaseSensitive, bool IsInspectingWholeWord,
                                                        bool InScopeComposition, bool InScopeDomain,
                                                        bool IncludeConcepts, bool IncludeRelationships,
                                                        bool IncludeMarkers, bool IncludeOthers,
                                                        bool InPropertyName, bool InPropertyTechName,
                                                        bool InPropertySummary, bool InPropertyDescription,
                                                        bool InPropertyDetailDesignation, bool InPropertyDetailContent,
                                                        bool InPropertyTechSpec)
        {
            DocumentEngine.SearchProviderOfTextNameWithoutPropertyAccessor = VisualComplement.PROP_FIELD_TEXT;
            DocumentEngine.SearchProviderOfTextExtractorWithoutPropertyAccessor = TextExtractorWithoutPropertyAccessor;
            DocumentEngine.SearchProviderOfObjectTypeDetector = ObjectTypeDetector;

            FoundObjectHint.ClearTemporalDocuments();

            var SearchTrace = new List<object>();
            var Results = new List<FoundObjectHint>();

            if (InScopeComposition)
                this.FindTextInTargetObject(SearchedText, this.TargetComposition, Results, SearchTrace, "",
                                            IncludeConcepts, IncludeRelationships, IncludeMarkers, IncludeOthers,
                                            IsCaseSensitive, IsInspectingWholeWord, InPropertyName, InPropertyTechName,
                                            InPropertySummary, InPropertyDescription, InPropertyDetailDesignation,
                                            InPropertyDetailContent, InPropertyTechSpec);

            if (InScopeDomain)
            {
                if (IncludeConcepts)
                    this.TargetComposition.CompositeContentDomain.ConceptDefinitions.ForEach(condef =>
                        this.FindTextInTargetObject(SearchedText, condef, Results, SearchTrace, "",
                                                    IncludeConcepts, IncludeRelationships, IncludeMarkers, IncludeOthers,
                                                    IsCaseSensitive, IsInspectingWholeWord, InPropertyName, InPropertyTechName,
                                                    InPropertySummary, InPropertyDescription, InPropertyDetailDesignation,
                                                    InPropertyDetailContent, InPropertyTechSpec));

                if (IncludeRelationships)
                    this.TargetComposition.CompositeContentDomain.RelationshipDefinitions.ForEach(reldef =>
                        this.FindTextInTargetObject(SearchedText, reldef, Results, SearchTrace, "",
                                                    IncludeConcepts, IncludeRelationships, IncludeMarkers, IncludeOthers,
                                                    IsCaseSensitive, IsInspectingWholeWord, InPropertyName, InPropertyTechName,
                                                    InPropertySummary, InPropertyDescription, InPropertyDetailDesignation,
                                                    InPropertyDetailContent, InPropertyTechSpec));

                if (IncludeMarkers)
                    this.TargetComposition.CompositeContentDomain.MarkerDefinitions.ForEach(mkrdef =>
                        this.FindTextInTargetObject(SearchedText, mkrdef, Results, SearchTrace, "",
                                                    IncludeConcepts, IncludeRelationships, IncludeMarkers, IncludeOthers,
                                                    IsCaseSensitive, IsInspectingWholeWord, InPropertyName, InPropertyTechName,
                                                    InPropertySummary, InPropertyDescription, InPropertyDetailDesignation,
                                                    InPropertyDetailContent, InPropertyTechSpec));

                if (IncludeOthers)
                {
                    this.FindTextInTargetObject(SearchedText, this.TargetComposition.CompositeContentDomain, Results, SearchTrace, "",
                                                IncludeConcepts, IncludeRelationships, IncludeMarkers, IncludeOthers,
                                                IsCaseSensitive, IsInspectingWholeWord, InPropertyName, InPropertyTechName,
                                                InPropertySummary, InPropertyDescription, InPropertyDetailDesignation,
                                                InPropertyDetailContent, InPropertyTechSpec);

                    this.TargetComposition.CompositeContentDomain.TableDefinitions.Where(tdef => tdef.Alterability != EAlterability.System).ForEach(tabdef =>
                        this.FindTextInTargetObject(SearchedText, tabdef, Results, SearchTrace, "",
                                                    IncludeConcepts, IncludeRelationships, IncludeMarkers, IncludeOthers,
                                                    IsCaseSensitive, IsInspectingWholeWord, InPropertyName, InPropertyTechName,
                                                    InPropertySummary, InPropertyDescription, InPropertyDetailDesignation,
                                                    InPropertyDetailContent, InPropertyTechSpec));

                    this.TargetComposition.CompositeContentDomain.BaseTables.ForEach(bastab =>
                        this.FindTextInTargetObject(SearchedText, bastab, Results, SearchTrace, "",
                                                    IncludeConcepts, IncludeRelationships, IncludeMarkers, IncludeOthers,
                                                    IsCaseSensitive, IsInspectingWholeWord, InPropertyName, InPropertyTechName,
                                                    InPropertySummary, InPropertyDescription, InPropertyDetailDesignation,
                                                    InPropertyDetailContent, InPropertyTechSpec));

                    this.TargetComposition.CompositeContentDomain.LinkRoleVariants.ForEach(variant =>
                        this.FindTextInTargetObject(SearchedText, variant, Results, SearchTrace, "",
                                                    IncludeConcepts, IncludeRelationships, IncludeMarkers, IncludeOthers,
                                                    IsCaseSensitive, IsInspectingWholeWord, InPropertyName, InPropertyTechName,
                                                    InPropertySummary, InPropertyDescription, InPropertyDetailDesignation,
                                                    InPropertyDetailContent, InPropertyTechSpec));

                    this.TargetComposition.CompositeContentDomain.ExternalLanguages.ForEach(lang =>
                        this.FindTextInTargetObject(SearchedText, lang, Results, SearchTrace, "",
                                                    IncludeConcepts, IncludeRelationships, IncludeMarkers, IncludeOthers,
                                                    IsCaseSensitive, IsInspectingWholeWord, InPropertyName, InPropertyTechName,
                                                    InPropertySummary, InPropertyDescription, InPropertyDetailDesignation,
                                                    InPropertyDetailContent, InPropertyTechSpec));

                    this.TargetComposition.CompositeContentDomain.ConceptDefClusters.ForEach(lang =>
                        this.FindTextInTargetObject(SearchedText, lang, Results, SearchTrace, "",
                                                    IncludeConcepts, IncludeRelationships, IncludeMarkers, IncludeOthers,
                                                    IsCaseSensitive, IsInspectingWholeWord, InPropertyName, InPropertyTechName,
                                                    InPropertySummary, InPropertyDescription, InPropertyDetailDesignation,
                                                    InPropertyDetailContent, InPropertyTechSpec));

                    this.TargetComposition.CompositeContentDomain.RelationshipDefClusters.ForEach(lang =>
                        this.FindTextInTargetObject(SearchedText, lang, Results, SearchTrace, "",
                                                    IncludeConcepts, IncludeRelationships, IncludeMarkers, IncludeOthers,
                                                    IsCaseSensitive, IsInspectingWholeWord, InPropertyName, InPropertyTechName,
                                                    InPropertySummary, InPropertyDescription, InPropertyDetailDesignation,
                                                    InPropertyDetailContent, InPropertyTechSpec));
                }
            }

            return Results;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        protected void FindTextInTargetObject(string SearchedText, IMModelClass Target,
                                              List<FoundObjectHint> OngoingResults, List<object> SearchTrace, string TraveledPath,
                                              bool IncludeConcepts, bool IncludeRelationships,
                                              bool IncludeMarkers, bool IncludeOthers,
                                              bool IsCaseSensitive, bool IsInspectingWholeWord,
                                              bool InPropertyName, bool InPropertyTechName,
                                              bool InPropertySummary, bool InPropertyDescription,
                                              bool InPropertyDetailDesignation, bool InPropertyDetailContent,
                                              bool InPropertyTechSpec)
        {
            if (SearchTrace.Contains(Target))
                return;

            SearchTrace.Add(Target);

            // IMPORTANT: Do not use Reflection or MModelPropertyDefinitor/MModelCollectionDefinitor
            //            to search into string fields, there is risk to change system information.

            if (Target is IIdentifiableElement)
            {
                var TarIdentif = Target as IIdentifiableElement;

                if (Target is SimpleElement)
                {
                    var TarSimple = Target as SimpleElement;

                    if (InPropertyName)
                        FindTextInTargetProperty(SearchedText, TarSimple, SimpleElement.__Name, OngoingResults, TraveledPath,
                                                 IsCaseSensitive, IsInspectingWholeWord);
                    if (InPropertyTechName)
                        FindTextInTargetProperty(SearchedText, TarSimple, SimpleElement.__TechName, OngoingResults, TraveledPath,
                                                 IsCaseSensitive, IsInspectingWholeWord);
                    if (InPropertySummary)
                        FindTextInTargetProperty(SearchedText, TarSimple, SimpleElement.__Summary, OngoingResults, TraveledPath,
                                                 IsCaseSensitive, IsInspectingWholeWord);
                    return;
                }

                if (!(Target is FormalElement))
                    return;

                var TarFormal = Target as FormalElement;

                if (InPropertyName)
                    FindTextInTargetProperty(SearchedText, TarFormal, FormalElement.__Name, OngoingResults, TraveledPath,
                                                IsCaseSensitive, IsInspectingWholeWord);
                if (InPropertyTechName)
                    FindTextInTargetProperty(SearchedText, TarFormal, FormalElement.__TechName, OngoingResults, TraveledPath,
                                                IsCaseSensitive, IsInspectingWholeWord);
                if (InPropertySummary)
                    FindTextInTargetProperty(SearchedText, TarFormal, FormalElement.__Summary, OngoingResults, TraveledPath,
                                                IsCaseSensitive, IsInspectingWholeWord);

                if (InPropertyDescription)
                    FindTextInTargetProperty(SearchedText, TarFormal, FormalElement.__Description, OngoingResults, TraveledPath,
                                                IsCaseSensitive, IsInspectingWholeWord);

                if (InPropertyTechSpec)
                    FindTextInTargetProperty(SearchedText, TarFormal, FormalElement.__TechSpec, OngoingResults, TraveledPath,
                                                IsCaseSensitive, IsInspectingWholeWord);

                if (IncludeOthers && Target is IdeaDefinition)
                {
                    var TarIdeaDef = Target as IdeaDefinition;

                    // Templates
                    TarIdeaDef.OutputTemplates.ForEach(tpl => FindTextInTargetProperty(SearchedText, tpl, Instrumind.ThinkComposer.MetaModel.Configurations.TextTemplate.__Text,
                                                                                        OngoingResults, TraveledPath, IsCaseSensitive, IsInspectingWholeWord));

                    if (Target is Domain)
                    {
                        var TarDomain = Target as Domain;

                        TarDomain.OutputTemplatesForConcepts.ForEach(tpl => FindTextInTargetProperty(SearchedText, tpl, Instrumind.ThinkComposer.MetaModel.Configurations.TextTemplate.__Text,
                                                                                                     OngoingResults, TraveledPath, IsCaseSensitive, IsInspectingWholeWord));

                        TarDomain.OutputTemplatesForRelationships.ForEach(tpl => FindTextInTargetProperty(SearchedText, tpl, Instrumind.ThinkComposer.MetaModel.Configurations.TextTemplate.__Text,
                                                                                                          OngoingResults, TraveledPath, IsCaseSensitive, IsInspectingWholeWord));
                    }
                }

                if (Target is Idea)
                {
                    var TarIdea = Target as Idea;

                    if (IncludeOthers)
                    {
                        // View's Complements
                        TarIdea.CompositeViews.SelectMany(view => view.ViewChildren.Select(child => child.Key).CastAs<VisualComplement,object>(vcomp => vcomp.Target.IsGlobal))
                            .ForEach(vcomp => FindTextInTargetProperty(SearchedText, vcomp, null, OngoingResults, TraveledPath,
                                                                       IsCaseSensitive, IsInspectingWholeWord));

                        // Attached Complements
                        TarIdea.VisualRepresentators.SelectMany(vrep => vrep.MainSymbol.AttachedComplements)
                            .ForEach(vcomp => FindTextInTargetProperty(SearchedText, vcomp, null, OngoingResults, TraveledPath,
                                                                       IsCaseSensitive, IsInspectingWholeWord));
                    }

                    // Dependants...
                    TraveledPath = TraveledPath + "\\" + TarIdentif.Name.RemoveNewLines();   // Only extend path for dependant collections (not before properties)

                    // Others
                    if (IncludeOthers)
                        TarIdea.CompositeViews.ForEach(
                            view => FindTextInTargetObject(SearchedText, view, OngoingResults, SearchTrace, TraveledPath,
                                                           IncludeConcepts, IncludeRelationships, IncludeMarkers, IncludeOthers,
                                                           IsCaseSensitive, IsInspectingWholeWord, InPropertyName, InPropertyTechName,
                                                           InPropertySummary, InPropertyDescription, InPropertyDetailDesignation,
                                                           InPropertyDetailContent, InPropertyTechSpec));

                    // Markers
                    if (IncludeMarkers)
                        TarIdea.Markings.Where(mark => mark.MarkerHasDescriptor).ForEach(
                            mark => FindTextInTargetObject(SearchedText, mark.Descriptor, OngoingResults, SearchTrace, TraveledPath,
                                                           IncludeConcepts, IncludeRelationships, IncludeMarkers, IncludeOthers,
                                                           IsCaseSensitive, IsInspectingWholeWord, InPropertyName, InPropertyTechName,
                                                           InPropertySummary, InPropertyDescription, InPropertyDetailDesignation,
                                                           InPropertyDetailContent, InPropertyTechSpec));

                    // Detail Designations
                    if (InPropertyDetailDesignation)
                        TarIdea.Details.Where(det => det.AssignedDesignator.IsLocal).ForEach(
                            detdsn => FindTextInTargetObject(SearchedText, detdsn.Designation, OngoingResults, SearchTrace, TraveledPath,
                                                             IncludeConcepts, IncludeRelationships, IncludeMarkers, IncludeOthers,
                                                             IsCaseSensitive, IsInspectingWholeWord, InPropertyName, InPropertyTechName,
                                                             InPropertySummary, InPropertyDescription, InPropertyDetailDesignation,
                                                             InPropertyDetailContent, InPropertyTechSpec));

                    // Detail Content
                    // PENDING: Complex for Tables and dangerous (?) for attachments

                    if (Target is Relationship)
                    {
                        var TarRel = Target as Relationship;

                        // Links
                        TarRel.Links.ForEach(
                            link => FindTextInTargetObject(SearchedText, link.Descriptor, OngoingResults, SearchTrace, TraveledPath,
                                                           IncludeConcepts, IncludeRelationships, IncludeMarkers, IncludeOthers,
                                                           IsCaseSensitive, IsInspectingWholeWord, InPropertyName, InPropertyTechName,
                                                           InPropertySummary, InPropertyDescription, InPropertyDetailDesignation,
                                                           InPropertyDetailContent, InPropertyTechSpec));
                    }

                    // Composite content
                    var Composites = TarIdea.CompositeIdeas.Where(idea => (IncludeConcepts && idea is Concept)
                                                                       || (IncludeRelationships && idea is Relationship));
                    foreach(var Composite in Composites)
                        this.FindTextInTargetObject(SearchedText, Composite, OngoingResults, SearchTrace, TraveledPath,
                                                    IncludeConcepts, IncludeRelationships, IncludeMarkers, IncludeOthers,
                                                    IsCaseSensitive, IsInspectingWholeWord, InPropertyName, InPropertyTechName,
                                                    InPropertySummary, InPropertyDescription, InPropertyDetailDesignation,
                                                    InPropertyDetailContent, InPropertyTechSpec);
                }
            }
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        protected static IList<System.Windows.Documents.Run> CachedReplaceableDocumentTextRuns = null;

        /// <summary>
        /// Replaces text in the specified found object Hints.
        /// Returns indication of change.
        /// </summary>
        public bool ReplaceTextOfFoundObjects(IEnumerable<FoundObjectHint> Hints, string SearchedText, string NewText)
        {
            this.StartCommandVariation("Replace Text");

            // PENDING: For Rich-Documents (XAML or RTF based) see...
            // http://shevaspace.blogspot.com/2007/11/how-to-search-text-in-wpf-flowdocument.html
            // http://stackoverflow.com/questions/9228414/flowdocument-replace-text-in-xamlpackage

            CachedReplaceableDocumentTextRuns = null;

            var Changes = new List<FoundObjectHint>();
            int Offset = 0;

            foreach (var Hint in Hints)
            {
                Offset = (Hint.MustBeDisplacedOnReplace
                          ? Offset + (NewText.Length - SearchedText.Length)
                          : 0);

                if (Hint.SourceAccessor != null)
                {
                    if (!(Hint.SourceObject is IMModelClass))
                        continue;

                    var Text = Hint.SourceAccessor.Read(Hint.SourceObject as IMModelClass) as string;
                    if (Text.IsAbsent())
                        continue;

                    if (Hint.RichDocumentRunSource != null)
                        Text = Hint.RichDocumentRunSource.Item2.Text;

                    Text = Text.ReplaceAt(Hint.TextPosition + Offset, SearchedText.Length, NewText);

                    if (Hint.RichDocumentRunSource != null)
                        Hint.RichDocumentRunSource.Item2.Text = Text;
                    else
                        Hint.SourceAccessor.Write(Hint.SourceObject as IMModelClass, Text);

                    if (!Hint.MustBeDisplacedOnReplace)
                    {
                        if (Hint.SourceObject is Idea)
                            ((Idea)Hint.SourceObject).VisualRepresentators.ForEach(vrep => vrep.Render());

                        if (Hint.SourceObject is IVersionUpdater)
                            ((IVersionUpdater)Hint.SourceObject).UpdateVersion();
                    }

                    Changes.Add(Hint);
                }
                else
                {
                    var TargetComplement = Hint.SourceObject as VisualComplement;
                    if (TargetComplement == null)
                        continue;

                    var Text = TargetComplement.GetPropertyField<string>(VisualComplement.PROP_FIELD_TEXT).NullDefault("");
                    Text = Text.ReplaceAt(Hint.TextPosition + Offset, SearchedText.Length, NewText);

                    TargetComplement.SetPropertyField(VisualComplement.PROP_FIELD_TEXT, Text);

                    if (!Hint.MustBeDisplacedOnReplace)
                        TargetComplement.GetDisplayingView().UpdateVersion();

                    TargetComplement.Render();

                    Changes.Add(Hint);
                }
            }

            FoundObjectHint.ApplyChanges(Changes);

            CachedReplaceableDocumentTextRuns = null;

            this.CompleteCommandVariation();

            Console.WriteLine("Text Replaced in {0} occurrences, from '{1}' to '{2}'.", Changes.Count, SearchedText, NewText);

            return (Changes.Count > 0);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Goes to the specified object either in a diagram View and/or its maintenance editor.
        /// Returns indication of successful goto.
        /// </summary>
        public bool GoToObject(object Target)
        {
            if (Target is Idea)
            {
                var TargetIdea = Target as Idea;
                var Representator = TargetIdea.VisualRepresentators.FirstOrDefault();

                if (Representator == null)
                    return false;

                Representator.DisplayingView.Engine.ShowView(Representator.DisplayingView);

                Representator.DisplayingView.Presenter.PostCall(
                    vpres =>
                    {
                        vpres.OwnerView.Manipulator.ApplySelection(Representator.MainSymbol);
                        vpres.OwnerView.Presenter.BringIntoView(Representator.MainSymbol.BaseArea);
                    });

                return true;
            }

            if (Target is VisualComplement)
            {
                var TargetComplement = Target as VisualComplement;

                var TargetView = TargetComplement.GetDisplayingView();
                TargetView.Engine.ShowView(TargetView);

                TargetView.Presenter.PostCall(
                    vpres =>
                    {
                        vpres.OwnerView.Manipulator.ApplySelection(TargetComplement);
                        vpres.OwnerView.Presenter.BringIntoView(TargetComplement.BaseArea);
                    });

                return true;
            }

            if (Target is View)
            {
                var TargetView = Target as View;
                CompositionEngine.EditViewProperties(TargetView);
                return true;
            }

            if (Target is Composition)
            {
                var TargetCompo = Target as Composition;
                TargetCompo.Engine.EditCompositionProperties();
                return true;
            }

            if (Target is Domain)
            {
                var TargetDomain = Target as Domain;
                DomainServices.DomainEdit(TargetDomain);
                return true;
            }

            if (Target is ConceptDefinition)
            {
                var TargetDef = Target as ConceptDefinition;
                DomainServices.DefineDomainConcepts(TargetDef.OwnerDomain, TargetDef);
                return true;
            }

            if (Target is RelationshipDefinition)
            {
                var TargetDef = Target as RelationshipDefinition;
                DomainServices.DefineDomainRelationships(TargetDef.OwnerDomain, TargetDef);
                return true;
            }

            if (Target is MarkerDefinition)
            {
                var TargetDef = Target as MarkerDefinition;
                DomainServices.DefineDomainMarkers(this.TargetComposition.CompositeContentDomain, TargetDef);
                return true;
            }

            if (Target is TableDefinition)
            {
                var TargetDef = Target as TableDefinition;
                DomainServices.EditDomainTableDefinitions(TargetDef.OwnerDomain, TargetDef);
                return true;
            }

            if (Target is SimplePresentationElement)    // Link-Role Variant
            {
                var TargetDef = Target as SimplePresentationElement;

                if (this.TargetComposition.CompositeContentDomain.LinkRoleVariants.Contains(TargetDef))
                {
                    DomainServices.DefineDomainLinkRoleVariants(this.TargetComposition.CompositeContentDomain, TargetDef);
                    return true;
                }
            }

            // PENDING: Find "Base Tables" and "Descriptors" 
            // (would require to have two target objects in the finding operation, one for the owner, other for the descriptor).

            return false;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}