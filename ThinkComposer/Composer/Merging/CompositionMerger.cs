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
// File   : FileGenerator.cs
// Object : Instrumind.ThinkComposer.Composer.Merging.Merger (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2013.10.03 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Text;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;
using Instrumind.Common.Visualization.Widgets;

using Instrumind.ThinkComposer.ApplicationProduct;
using Instrumind.ThinkComposer.MetaModel.Configurations;
using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.MetaModel.InformationMetaModel;
using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;
using Instrumind.ThinkComposer.Model;
using Instrumind.ThinkComposer.Model.GraphModel;
using Instrumind.ThinkComposer.MetaModel;
using Instrumind.ThinkComposer.Model.InformationModel;

// Provides features for Compositions Merging
namespace Instrumind.ThinkComposer.Composer.Merging
{
    /// <summary>
    /// Merges content and meta-definitions from a Source Composition into a Target Composition.
    /// </summary>
    public partial class CompositionMerger
    {
        // -----------------------------------------------------------------------------------------
        static CompositionMerger()
        {
        }

        // -----------------------------------------------------------------------------------------
        public Composition SourceComposition { get; protected set; }
        public Composition TargetComposition { get; protected set; }

        public readonly List<IModelEntity> SourceEntities = new List<IModelEntity>();

        public readonly List<ConceptDefinition> SelectedConceptDefs = new List<ConceptDefinition>();
        public readonly List<RelationshipDefinition> SelectedRelationshipDefs = new List<RelationshipDefinition>();
        public readonly List<SimplePresentationElement> SelectedLinkRoleVariantDefs = new List<SimplePresentationElement>();
        public readonly List<Table> SelectedBaseTables = new List<Table>();
        public readonly List<ExternalLanguageDeclaration> SelectedExternalLanguages = new List<ExternalLanguageDeclaration>();
        public readonly List<FormalPresentationElement> SelectedConceptDefClusters = new List<FormalPresentationElement>();
        public readonly List<FormalPresentationElement> SelectedRelationshipDefClusters = new List<FormalPresentationElement>();
        public readonly List<MarkerDefinition> SelectedMarkerDefs = new List<MarkerDefinition>();
        public readonly List<IModelEntity> SelectedOtherEntities = new List<IModelEntity>();
        
        /// <summary>
        /// Stores the equivalent entities (key=source-entity, value=target-entity)
        /// </summary>
        public readonly Dictionary<IModelEntity, IModelEntity> EquivalentEntities = new Dictionary<IModelEntity, IModelEntity>();

        private ThreadWorker<int> CurrentWorker { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CompositionMerger(Composition SourceComposition,
                                 Composition TargetComposition)
        {
            this.SourceComposition = SourceComposition;
            this.TargetComposition = TargetComposition;
        }

        /// <summary>
        /// Generates Files based on the current Configuration.
        /// Returns operation-result.
        /// </summary>
        public OperationResult<int> Merge(ThreadWorker<int> Worker)
        {
            General.ContractRequiresNotNull(Worker);
            var MergedObjects = 0;

            try
            {
                this.CurrentWorker = Worker;
                this.CurrentWorker.ReportProgress(0, "Starting.");

                // 1. Determine source objects...
                // 1.1. Determine source meta-definitions (declared in Domain) and their equivalents
                this.CurrentWorker.ReportProgress(10, "Determining Domain equivalents");
                this.DetermineDomainEquivalents();

                // 1.2. Determine source objects and their equivalents
                // (excluding Domain's meta-definitions, but including possible floating/orphan meta-definitions)

                var ProgressPercentageStart = 10.0;
                var ProgressPercentageEnd = 40.0;

                var ProgressStep = ((ProgressPercentageEnd - ProgressPercentageStart) /
                                    (double)SourceEntities.Count);
                var ProgressPercentage = ProgressPercentageStart;

                foreach (var Entity in SourceEntities)
                {
                    this.CurrentWorker.ReportProgress((int)ProgressPercentage, "Reading object '" + Entity.ToStringAlways() + "'");
                    ProgressPercentage += ProgressStep;

                    DetermineSourceChildren(Entity);
                }

                // this.MergeIdea(this.SourceComposition, 1.0, 99.0);

                // Get selected Ideas (first Concepts, then Relationships and the related Ideas by the later)
                // Recursively, get the selected composite Ideas

                ProgressPercentageStart = ProgressPercentageEnd;
                ProgressPercentageEnd = 66.6;

                ProgressStep = ((ProgressPercentageEnd - ProgressPercentageStart) /
                                (double)SourceEntities.Count);
                ProgressPercentage = ProgressPercentageStart;

                foreach (var Entity in this.EquivalentEntities.Keys)
                {
                    this.CurrentWorker.ReportProgress((int)ProgressPercentage, "Matching object '" + Entity.ToStringAlways() + "'");
                    ProgressPercentage += ProgressStep;

                    DetermineEquivalent(Entity);
                }

                // 3. Determine Target equivalents, if any, to later reappoint references

                // 4. At Target Composition Domain, create clones of dominant meta-definitions + base-content

                // 5. At Target Composition container Ideas, create clones of source Ideas

                // Finish
                this.CurrentWorker.ReportProgress(100, "Merge complete.");
                this.CurrentWorker = null;
            }
            catch (Exception Problem)
            {
                this.CurrentWorker = null;
                return OperationResult.Failure<int>("Cannot complete merge.\nProblem: " + Problem.Message, Result: MergedObjects);
            }

            return OperationResult.Success<int>(MergedObjects, "Merge executed");
        }

        // -----------------------------------------------------------------------------------------
        public void DetermineDomainEquivalents()
        {

        }

        // -----------------------------------------------------------------------------------------
        public void DetermineSourceChildren(IModelEntity Source)
        {
            if (!this.EquivalentEntities.AddNew(Source, null))
                return;

            // PENDING: GET ONLY THE MINIMUM OWNED AND DOMINANT OBJECTS

            foreach (var PropDef in Source.ClassDefinition.Properties)
                if (!PropDef.IsStoreBoxBased)
                {
                    var ValueObject = PropDef.Read(Source);
                    if (ValueObject == null)
                        continue;

                    var ValueEntity = ValueObject as IModelEntity;
                    if (ValueEntity == null)
                    {
                        var ValueAssignment = ValueObject as MAssignment;
                        if (ValueAssignment != null)
                            ValueEntity = ValueAssignment.AssignedValue as IModelEntity;
                        /* Not required (the owner is outside the owned children and dominant dependencies)
                        else
                        {
                            var ValueOwnership = ValueObject as MOwnership;
                            if (ValueOwnership != null)
                                ValueEntity = ValueOwnership.Owner as IModelEntity;
                        } */
                    }

                    if (ValueEntity != null)
                        DetermineSourceChildren(ValueEntity);
                    else
                        Console.WriteLine("Prop{{" + PropDef.QualifiedTechName + "}}: [" + ValueObject.ToStringAlways() + "]");
                }

            // Collections are typically owned
            foreach (var CollDef in Source.ClassDefinition.Collections)
            {
                var SourceCollection = (EditableCollection)CollDef.Read(Source) as IEnumerable<object>;
                if (SourceCollection == null)
                    continue;

                var Index = 0;
                foreach (var Item in SourceCollection)
                {
                    var EntityItem = Item as IModelEntity;
                    if (EntityItem != null)
                        DetermineSourceChildren(EntityItem);
                    else
                        if (Item != null)
                            Console.WriteLine("CollItem{{" + CollDef.QualifiedTechName + "(" + Index.ToString() + ")}}: [" + Item.ToStringAlways() + "]");

                    Index++;
                }
            }

        }

        // -----------------------------------------------------------------------------------------
        public void DetermineEquivalent(IModelEntity SourceEntity)
        {
            IModelEntity Equivalent = null;

            if (SourceEntity is Composition)
                Equivalent = this.TargetComposition;
            else
                if (SourceEntity is Domain)
                    Equivalent = this.TargetComposition.CompositeContentDomain;
                else
                    if (SourceEntity is MetaDefinition) // Pending: Also consider Simple/Formal-PresentationElements at Domain level
                    {
                        // Find by GlobalId at Domain level
                        // - Concept Definitions
                        // - Relationship Definitions
                        // - Link-Role Variant Definitions
                        // - Table-Structure Definitions (replace only if no tables uses it, else add with new name)
                        // - External Languages
                        // - Idea-Definition Clusters
                        // - Marker Definitions
                        // - Base-Content (Base Tables)
                    }
                    else
                        if (SourceEntity is Idea)
                        {
                            // Find by GlobalId *at the same* Composition level
                        }

            if (Equivalent != null)
                this.EquivalentEntities[SourceEntity] = Equivalent;
        }

        // -----------------------------------------------------------------------------------------
        public void MergeIdea(Idea SourceIdea, double ProgressPercentageStart, double ProgressPercentageEnd)
        {
        }

        /*- public void DetermineAllSourceObjects()
        {
            var InitialSelection = this.SourceEntities;

            while (true)
            {
                var Extras = DetermineIncludedExtras(InitialSelection);
                if (Extras.Count < 1)
                    break;

                foreach(var Extra in Extras)
                    this.SourceEntities.AddNew(Extra);

                InitialSelection = Extras;
            }
        }

        public List<IModelEntity> DetermineIncludedExtras(List<IModelEntity> InitialSelection)
        {
            var ExtraInclusion = new List<IModelEntity>();

            foreach (var Selected in InitialSelection)
            {
                var SelIdea = Selected as Idea;
                if (SelIdea != null)
                {
                    ExtraInclusion.AddNew(SelIdea.IdeaDefinitor);

                    var SelRel = Selected as Relationship;
                    if (SelRel != null)
                    {
                        ExtraInclusion.AddRange(SelRel.AssociatingLinks);
                        ExtraInclusion.AddRange();
                        ExtraInclusion.AddRange(SelRel.AssociatingLinks.Select(lnk => lnk.AssociatedIdea));
                    }
                }
            }

            return ExtraInclusion;
        } */

        // -----------------------------------------------------------------------------------------
    }
}
