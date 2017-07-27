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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
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
using Instrumind.ThinkComposer.MetaModel.Configurations;
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
    /// Implements fixes to the Model of Compositions and Domains.
    /// </summary>
    public static class ModelFixes
    {
        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Applies the necessary changes to a Domain and its Composition (if any) based in an old model with bugs to fix.
        /// Returns indication of revision updated (therefore file-save requested).
        /// </summary>
        // IMPORTANT: If changes/fixes are made or not the revision must be registered to avoid test again on each load.
        public static bool ApplyModelFixes(Domain Target)
        {
            var InitialRevision = Target.ModelRevision;

            if (Target.ModelRevision < 1)
            {
                ModelRev1_FixAllDetailDesignators(Target);
                Target.ModelRevision++;
            }

            if (Target.ModelRevision < 2 && Target.OwnerComposition != null)
            {
                ModelRev2_FixConnectors(Target.OwnerComposition);
                Target.ModelRevision++;
            }

            if (Target.ModelRevision < 3)
            {
                ModelRev3_AddSingleFieldTableDefs(Target);
                Target.ModelRevision++;
            }

            if (Target.ModelRevision < 4)
            {
                ModelRev4_FixReportingDefaultConfig(Target);
                Target.ModelRevision++;
            }

            if (Target.ModelRevision < 5)
            {
                ModelRev5_SetCustomFieldsAsOwnedByItsDesignator(Target.OwnerComposition);
                Target.ModelRevision++;
            }

            if (Target.ModelRevision < 6)
            {
                ModelRev6_ChangeVisualComplementsPropertiesStorage(Target.OwnerComposition);
                Target.ModelRevision++;
            }

            if (Target.ModelRevision < 7)
            {
                ModelRev7_FixOwnerReferences(Target.OwnerComposition);
                Target.ModelRevision++;
            }

            if (Target.ModelRevision < 8)
            {
                ModelRev8_FixDuplicatedGlobalIds(Target.OwnerComposition);
                Target.ModelRevision++;
            }

            if (Target.ModelRevision < 9)
            {
                ModelRev9_RemoveDuplicateTableDefs(Target);
                Target.ModelRevision++;
            }

            if (Target.ModelRevision < 10)
            {
                ModelRev10_UpdateSymbolsFlippingProperties(Target.OwnerComposition);
                Target.ModelRevision++;
            }

            // NOTE: Some test Compos has a ModelRevision with some extra numbers added.

            /* NEXT:
            if (Target.ModelRevision < N)
            {
                ModelRevN_Fix...(Target);
                Target.ModelRevision++;
            } */

            if (InitialRevision >= Target.ModelRevision)
                return false;

            Target.EditEngine.ExistenceStatus = EExistenceStatus.Modified;
            Console.WriteLine("Document structure updated.");

            return true;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------        /// <summary>
        /// Fix...
        /// DetailDesignator.Initializer (type ICopyable/StoreBox<MModelPropertyDefinitor>)
        /// Problem: The MModelPropertyDefinitor QualifiedName
        ///           was generated of __Summary.Name + "." + __Summary.TechName (as "Formal Element.Summary")
        ///                 instead of __Summary.TechName + "." + __Summary.TechName.
        /// </summary>
        private static void ModelRev1_FixAllDetailDesignators(Domain Target)
        {
            var Designators = Target.Definitions.SelectMany(def => def.DetailDesignators);

            if (Target.OwnerComposition != null)
                Designators.Concat(Target.OwnerComposition.DeclaredIdeas.SelectMany(item => item.Details.Where(det => det.Designation != null).Select(det => det.Designation)
                                                                                                .Concat(item.DetailsCustomLooks.Select(clk => clk.Key))));
            Designators = Designators.Distinct();

            foreach (var Designator in Designators)
                ModelRev1_FixDetailDesignator(Designator);

            if (Target.OwnerComposition != null)
            {
                var Details = Target.OwnerComposition.DeclaredIdeas.SelectMany(item => item.Details);
                foreach (var Detail in Details)
                    ModelRev1_FixDetail(Detail);
            }
        }

        /// <summary>
        /// Fixes individual detail-designator.
        /// </summary>
        private static void ModelRev1_FixDetailDesignator(DetailDesignator Evaluated)
        {
            if (Evaluated == null)
                return;

            // Initializer must be getted indirectly, because of underlying Store-Box based storage.
            var BoxedProp = DetailDesignator.__Initializer.Get(Evaluated) as StoreBoxBase;
            if (BoxedProp == null)
                return;

            var StoredBytes = BoxedProp.GetStoredValueBytes();
            if (StoredBytes == null || StoredBytes.BytesToString() != "Formal Element.Summary")
                return;

            // Evaluated.Initializer = null;
            Evaluated.Initializer = FormalElement.__Summary;
            //T Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss.fff"));
        }

        /// <summary>
        /// Fixes individual detail-designator. Returns indication of change.
        /// </summary>
        private static bool ModelRev1_FixDetail(ContainedDetail Evaluated)
        {
            var LinkDet = Evaluated as InternalLink;
            if (LinkDet == null)
                return false;

            var BoxedProp = InternalLink.__TargetProperty.GetStoreBoxContainer(LinkDet);
            if (BoxedProp == null)
                return false;

            var StoredBytes = BoxedProp.GetStoredValueBytes();
            if (StoredBytes == null)
                return false;

            var StoredPropName = StoredBytes.BytesToString();
            if (MModelClassDefinitor.DeclaredMemberDefinitors.ContainsKey(StoredPropName))
                return false;

            foreach(var MembDef in MModelClassDefinitor.DeclaredMemberDefinitors)
                if (((MembDef.Value.DeclaringDefinitor == null ? "" : MembDef.Value.DeclaringDefinitor.Name) +
                    "." + MembDef.Value.TechName) == StoredPropName)
                {
                    LinkDet.TargetProperty = MembDef.Value as MModelPropertyDefinitor;
                    return true;
                }

            return false;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        private static void ModelRev2_FixConnectors(Composition Target)
        {
            var Connectors = Target.DeclaredIdeas.CastAs<Relationship, Idea>().SelectMany(rel => rel.VisualRepresentators)
                                .CastAs<RelationshipVisualRepresentation, VisualRepresentation>().SelectMany(vrep => vrep.VisualConnectors)
                                    .Distinct();

            foreach (var Connector in Connectors)
            {
                if ((Connector.OriginPosition == default(Point) || Connector.OriginPosition == Display.NULL_POINT)
                    && Connector.OriginSymbol != null)
                    Connector.OriginPosition = Connector.OriginSymbol.BaseCenter;

                if ((Connector.TargetPosition == default(Point) || Connector.TargetPosition == Display.NULL_POINT)
                    && Connector.TargetSymbol != null)
                    Connector.TargetPosition = Connector.TargetSymbol.BaseCenter;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Appends a standard table-def structure for single-field tables.
        /// </summary>
        // Notice that this is public to be called by the Domain constructor.
        public static bool ModelRev3_AddSingleFieldTableDefs(Domain Target)
        {
            /* if (Target.TableDefinitions.Any(tdef => tdef.FieldDefinitions.Count == 1
                                                    && tdef.FieldDefinitions[0].FieldType.ContainerType == typeof(string)))
                return false; */

            TableDefinition TableDef = null;

            // IMPORTANT: Do not put new tables at first place
            if (!Target.TableDefinitions.Any(td => td.TechName == "SingleTextField" && td.Alterability == EAlterability.System))
            {
                TableDef = new TableDefinition(Target, "Single Text-Field", "SingleTextField", "General purpose storage for simple lists or load plain files where each line will become a single text field value.");
                TableDef.Alterability = EAlterability.System;
                TableDef.FieldDefinitions.Add(new FieldDefinition(TableDef, "Value", "Value", DataType.DataTypeText, "Text Value"));
                TableDef.Categories.Add(Target.DefaultTableDefCategory);
                TableDef.AlterStructure();
                Target.TableDefinitions.Add(TableDef);
            }

            if (!Target.TableDefinitions.Any(td => td.TechName == "SingleNumericField" && td.Alterability == EAlterability.System))
            {
                TableDef = new TableDefinition(Target, "Single Numeric-Field", "SingleNumericField", "General purpose storage for simple lists or load plain files where each line will become a single numeric field value.");
                TableDef.Alterability = EAlterability.System;
                TableDef.FieldDefinitions.Add(new FieldDefinition(TableDef, "Value", "Value", DataType.DataTypeNumber, "Numeric Value"));
                TableDef.Categories.Add(Target.DefaultTableDefCategory);
                TableDef.AlterStructure();
                Target.TableDefinitions.Add(TableDef);
            }

            return true;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Populates the default values for Reporting configuration.
        /// </summary>
        private static void ModelRev4_FixReportingDefaultConfig(Domain Target)
        {
            if (Target.ReportingConfiguration == null)
            {
                Target.ReportingConfiguration = new ReportConfiguration();
                return;
            }

            Target.ReportingConfiguration.CompositeIdea_Details = true;
            Target.ReportingConfiguration.CompositeIdea_DetailsIncludeLinksTarget = true;
            Target.ReportingConfiguration.CompositeIdea_DetailsIncludeAttachmentsContent = true;
            Target.ReportingConfiguration.CompositeIdea_DetailsIncludeTablesData = true;

            Target.ReportingConfiguration.CompositeIdea_RelatedFrom_Collection = true;
            Target.ReportingConfiguration.CompositeIdea_IncludeTargetCompanions = true;

            Target.ReportingConfiguration.CompositeIdea_RelatedTo_Collection = true;
            Target.ReportingConfiguration.CompositeIdea_IncludeOriginCompanions = true;

            Target.ReportingConfiguration.CompositeIdea_GroupedIdeas_List = new DisplayList();
            Target.ReportingConfiguration.CompositeIdea_Complements = true;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Sets all custom-field table definitions as 'owned' in their designators.
        /// </summary>
        private static bool ModelRev5_SetCustomFieldsAsOwnedByItsDesignator(Composition Target)
        {
            var DefinitionDesignators = Target.CompositeContentDomain.Definitions.SelectMany(idef => idef.DetailDesignators)
                                                            .CastAs<TableDetailDesignator, DetailDesignator>();

            foreach (var Designator in DefinitionDesignators)
                if (Designator.Alterability == EAlterability.System && Designator.Owner.IsGlobal)
                    Designator.TableDefIsOwned = true;

            var IdeaDesignators = Target.DeclaredIdeas.SelectMany(idea => idea.Details.Select(det => det.Designation))
                                                           .CastAs<TableDetailDesignator, DetailDesignator>();

            foreach (var Designator in IdeaDesignators)
                if (Designator.Alterability == EAlterability.System && !Designator.Owner.IsGlobal)
                    Designator.TableDefIsOwned = true;

            return true;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Changes the Visual Complements properties storage to a dictionary.
        /// </summary>
        private static bool ModelRev6_ChangeVisualComplementsPropertiesStorage(Composition Target)
        {
            var Views = Target.CompositeViews.Concat(Target.DeclaredIdeas.SelectMany(idea => idea.CompositeViews));

            foreach (var View in Views)
                foreach (var Complement in View.ViewChildren.Select(vch => vch.Key).CastAs<VisualComplement, object>())
                    ModelRev6_UpdateStorage(Complement);

            return true;
        }

        private static void ModelRev6_UpdateStorage(VisualComplement Target)
        {
            if (Target.IsComplementImage)
                Target.SetPropertyField(VisualComplement.PROP_FIELD_IMAGE, Target.Content);
            else
                if (Target.Kind.TechName.IsOneOf(Domain.ComplementDefText.TechName, Domain.ComplementDefNote.TechName, Domain.ComplementDefStamp.TechName))
                {
                    var Register = Target.Content as Tuple<string, TextFormat, StoreBox<Brush>, StoreBox<Brush>>;
                    if (Register != null)
                    {
                        Target.SetPropertyField(VisualComplement.PROP_FIELD_TEXT, Register.Item1);
                        Target.SetPropertyField(VisualComplement.PROP_FIELD_TEXTFORMAT, Register.Item2);
                        Target.SetPropertyField(VisualComplement.PROP_FIELD_FOREGROUND, Register.Item3.Value);
                        Target.SetPropertyField(VisualComplement.PROP_FIELD_BACKGROUND, Register.Item4.Value);
                    }
                }
                else
                    if (Target.IsComplementCallout || Target.IsComplementQuote)
                    {
                        var Register = Target.Content as Tuple<string, TextFormat, StoreBox<Brush>, StoreBox<Brush>, EVecinityQuadrant, double, double>;
                        if (Register != null)
                        {
                            Target.SetPropertyField(VisualComplement.PROP_FIELD_TEXT, Register.Item1);
                            Target.SetPropertyField(VisualComplement.PROP_FIELD_TEXTFORMAT, Register.Item2);
                            Target.SetPropertyField(VisualComplement.PROP_FIELD_FOREGROUND, Register.Item3.Value);
                            Target.SetPropertyField(VisualComplement.PROP_FIELD_BACKGROUND, Register.Item4.Value);
                            Target.SetPropertyField(VisualComplement.PROP_FIELD_QUADRANT, Register.Item5);
                            Target.SetPropertyField(VisualComplement.PROP_FIELD_OFFSETX, Register.Item6);
                            Target.SetPropertyField(VisualComplement.PROP_FIELD_OFFSETY, Register.Item7);
                        }
                    }
                    else
                        if (Target.Kind.TechName.IsOneOf(Domain.ComplementDefInfoCard.TechName, Domain.ComplementDefLegend.TechName,
                                                       Domain.ComplementDefGroupRegion.TechName))
                        {
                            var Register = Target.Content as Tuple<StoreBox<Brush>, StoreBox<Brush>>;
                            if (Register != null)
                            {
                                Target.SetPropertyField(VisualComplement.PROP_FIELD_FOREGROUND, Register.Item1.Value);
                                Target.SetPropertyField(VisualComplement.PROP_FIELD_BACKGROUND, Register.Item2.Value);
                            }
                        }
                        else
                            if (Target.Kind.TechName == Domain.ComplementDefGroupLine.TechName)
                            {
                                var Register = Target.Content as Tuple<StoreBox<Brush>, StoreBox<Brush>, Orientation>;
                                if (Register != null)
                                {
                                    Target.SetPropertyField(VisualComplement.PROP_FIELD_FOREGROUND, Register.Item1.Value);
                                    Target.SetPropertyField(VisualComplement.PROP_FIELD_BACKGROUND, Register.Item2.Value);
                                    Target.SetPropertyField(VisualComplement.PROP_FIELD_ORIENTATION, Register.Item3);
                                }
                            }

            Target.Content = null;  // Field deprecated
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        private static bool ModelRev7_FixOwnerReferences(Composition Target)
        {
            var ExaminedInstances = new List<IMModelClass>();
            var Fixes = new List<Tuple<IMModelClass, MModelPropertyDefinitor, object>>();

            //T Console.WriteLine("#### RUN 1...");
            ModelRev7_ApplyFixOwnerReferences(Target.OwnerComposition, "\\DOM\\", ExaminedInstances, Fixes, Target.OwnerComposition.CompositeContentDomain);
            ModelRev7_ApplyFixOwnerReferences(Target.OwnerComposition, "\\COM\\", ExaminedInstances, Fixes, Target.OwnerComposition);
            ExaminedInstances.Clear();
            Fixes.ForEach(fix => fix.Item2.Write(fix.Item1, fix.Item3));
            Fixes.Clear();

            /*T Second exec. just to test that the first one solved problems...
            if (Fixes.Count > 0)
            {
                Console.WriteLine("#### RUN 2...");
                ModelRev7_ApplyFixOwnerReferences(Target.OwnerComposition, "\\DOM\\", ExaminedInstances, Fixes, Target.OwnerComposition.CompositeContentDomain);
                ModelRev7_ApplyFixOwnerReferences(Target.OwnerComposition, "\\COM\\", ExaminedInstances, Fixes, Target.OwnerComposition);
                ExaminedInstances.Clear();
                Fixes.ForEach(fix => fix.Item2.Write(fix.Item1, fix.Item3));
                Fixes.Clear();
            } */

            return true;
        }

        /// <summary>
        /// Fixes references to owner, which are not pointing to the real ones (resulting of old-buggy cloning operation).
        /// </summary>
        private static bool ModelRev7_ApplyFixOwnerReferences(Composition TargetCompo, string Route, IList<IMModelClass> ExaminedInstances,
                                                              List<Tuple<IMModelClass, MModelPropertyDefinitor, object>> Fixes,
                                                              IMModelClass Target, IMModelClass DirectOwner = null, int Level = 0)
        {
            if (ExaminedInstances.Contains(Target))
                return false;

            ExaminedInstances.Add(Target);

            Route = Route + Target.ToHashCodeAndString() + "~";

            var PropertyDefs = Target.ClassDefinition.Properties;
            foreach (var PropDef in PropertyDefs)
            {
                var Value = PropDef.Read(Target);
                var PropValue = (Value is MOwnership
                                 ? ((MOwnership)Value).Owner
                                 : (Value is MAssignment
                                    ? ((MAssignment)Value).AssignedValue
                                    : Value)) as IMModelClass;

                if (PropValue != null)
                {
                    if (PropDef.ReferencesOwner.HasValue && DirectOwner != null)
                    {
                        var Change = ModelRev7_EvaluateOwnershipDisconnection(TargetCompo, Target, Route, DirectOwner, PropValue);

                        if (Change)
                        {
                            // PENDING: Ensure there is only one claiming owner
                            // (not needed if THE-WHOLE-PROCESS is invoked by 2nd time without generating new changes)

                            object Referencer = null;
                            var IsValid = true; // just until all type-validations are implemented

                            Referencer = (Value is MOwnership
                                            ? (object)(((MOwnership)Value).CreateClone(DirectOwner))
                                            : (Value is MAssignment
                                                ? (object)(((MAssignment)Value).CreateClone(DirectOwner))
                                                : DirectOwner));

                            // Ensure coherence per each treated type
                            var TableOwner = DirectOwner as Table;
                            var TableRefer = PropValue as Table;
                            if (TableOwner != null)
                                if (TableOwner.AssignedDesignator.Value != TableRefer.AssignedDesignator.Value)
                                    IsValid = false;

                            /* var RelDefOwner = DirectOwner as RelationshipDefinition;
                            var RelDefRefer = PropValue as RelationshipDefinition;
                            if (IsValid && RelDefOwner != null)
                            {
                               // validate for reldef
                               IsValid = true;
                            } */
                            
                            if (Referencer != null && IsValid)
                            {
                                Fixes.Add(Tuple.Create(Target, PropDef, Referencer));
                                //T Console.WriteLine("TYPES of Target=[{0}], NewRef=[{1}]", Target.GetType().Name, Referencer.GetType().Name);
                            }
                        }
                    }

                    if (PropDef.Membership != EEntityMembership.External)
                        ModelRev7_ApplyFixOwnerReferences(TargetCompo, Route + PropDef.TechName + "=", ExaminedInstances, Fixes,
                                                     PropValue, (PropDef.Membership == EEntityMembership.External ? null : Target), Level + 1);
                }
            }

            var CollectionDefs = Target.ClassDefinition.Collections;

            foreach (var CollDef in CollectionDefs)
            {
                if (CollDef.Membership == EEntityMembership.External)
                    continue;

                var Collection = (EditableCollection)CollDef.Read(Target);

                var CollList = Collection as IList;

                if (CollList != null)
                {
                    var Index = -1;
                    foreach (var Item in CollList)
                    {
                        Index++;

                        var CollItem = (Item is MOwnership
                                         ? ((MOwnership)Item).Owner
                                         : (Item is MAssignment
                                            ? ((MAssignment)Item).AssignedValue
                                            : Item)) as IMModelClass;

                        if (CollItem != null)
                            ModelRev7_ApplyFixOwnerReferences(TargetCompo, Route + CollDef.TechName + "[" + Index.ToString() + "]=", ExaminedInstances, Fixes,
                                                         CollItem, (CollDef.Membership == EEntityMembership.External ? null : Target), Level + 1);
                    }
                }
                else
                {
                    var CollDict = Collection as IDictionary;

                    if (CollDict != null)
                    {
                        var Index = -1;
                        foreach (var Key in CollDict.Keys)
                        {
                            Index++;

                            var CollKey = (Key is MOwnership
                                             ? ((MOwnership)Key).Owner
                                             : (Key is MAssignment
                                                ? ((MAssignment)Key).AssignedValue
                                                : Key)) as IMModelClass;

                            if (CollKey != null)
                                ModelRev7_ApplyFixOwnerReferences(TargetCompo, Route + CollDef.TechName + "[" + Index.ToString() + "]K=", ExaminedInstances, Fixes,
                                                             CollKey, (CollDef.Membership == EEntityMembership.External ? null : Target), Level + 1);
                        }

                        Index = -1;
                        foreach (var Value in CollDict.Values)
                        {
                            Index++;

                            var CollValue = (Value is MOwnership
                                             ? ((MOwnership)Value).Owner
                                             : (Value is MAssignment
                                                ? ((MAssignment)Value).AssignedValue
                                                : Value)) as IMModelClass;

                            if (CollValue != null)
                                ModelRev7_ApplyFixOwnerReferences(TargetCompo, Route + CollDef.TechName + "[" + Index.ToString() + "]V=", ExaminedInstances, Fixes,
                                                             CollValue, (CollDef.Membership == EEntityMembership.External ? null : Target), Level + 1);
                        }
                    }
                }
            }

            return true;
        }

        // Returns true if the reference must be passed to the direct-owner (claimer)
        private static bool ModelRev7_EvaluateOwnershipDisconnection(Composition TargetCompo, IMModelClass Target, string Route,
                                                                     IMModelClass DirectOwner, IMModelClass RefOwner)
        {
            if (RefOwner == DirectOwner || DirectOwner.GetType() != RefOwner.GetType())
                return false;

            var Claimer = DirectOwner.GetType().Name + ":" + DirectOwner.ToHashCodeAndString();
            var Referer = RefOwner.GetType().Name + ":" + RefOwner.ToHashCodeAndString();

            /*T Console.WriteLine("==> Member, [{0}] claimed by [{1}] referencing to [{2}]",    // IMPORTANT: This references to a wrongly associated owner!
                              Target.GetType().Name + ":" + Target.ToHashCodeAndString(), Claimer, Referer);

            Console.WriteLine("    *LOCATION..:" + Route); */

            //var LocationsOfReferenced = new List<string>();
            //ModelRev7_FindLocationsOf(null, "\\DOM\\", ref LocationsOfReferenced, TargetCompo.CompositeContentDomain /*Always look in the entire Domain*/, RefOwner);
            //ModelRev7_FindLocationsOf(null, "\\COM\\", ref LocationsOfReferenced, TargetCompo /*Always look in the entire Compo*/, RefOwner);
            //foreach (var Result in LocationsOfReferenced)
            //    Console.WriteLine("    *Referenced:" + Result);

            ////var LocationsOfClaimed = new List<string>();
            ////ModelRev7_FindLocationsOf(null, "\\DOM\\", ref LocationsOfClaimed, TargetCompo.CompositeContentDomain /*Always look in the entire Domain*/, DirectOwner);
            ////ModelRev7_FindLocationsOf(null, "\\COM\\", ref LocationsOfClaimed, TargetCompo /*Always look in the entire Compo*/, DirectOwner);
            ////foreach (var Result in LocationsOfClaimed)
            ////    Console.WriteLine("    *Claimer...:" + Result);

            return true;
        }

        private static void ModelRev7_FindLocationsOf(IList<IMModelClass> ExaminedInstances, string Route, ref List<string> Results,
                                                      IMModelClass Target, IMModelClass SearchedObject)
        {
            if (ExaminedInstances == null)
                ExaminedInstances = new List<IMModelClass>();

            if (ExaminedInstances.Contains(Target))
                return;

            if (Target == SearchedObject)
                Results.Add(Route + Target.ToHashCodeAndString());

            ExaminedInstances.Add(Target);

            Route = Route + Target.ToHashCodeAndString() + "~";

            var PropertyDefs = Target.ClassDefinition.Properties;
            foreach (var PropDef in PropertyDefs)
            {
                var Value = PropDef.Read(Target);
                var PropValue = (Value is MOwnership
                                 ? ((MOwnership)Value).Owner
                                 : (Value is MAssignment
                                    ? ((MAssignment)Value).AssignedValue
                                    : Value)) as IMModelClass;

                if (PropValue != null)
                    ModelRev7_FindLocationsOf(ExaminedInstances, Route + PropDef.TechName + "=",
                                              ref Results, PropValue, SearchedObject);
            }

            var CollectionDefs = Target.ClassDefinition.Collections;

            foreach (var CollDef in CollectionDefs)
            {
                var Collection = (EditableCollection)CollDef.Read(Target);

                var CollList = Collection as IList;

                if (CollList != null)
                {
                    var Index = -1;
                    foreach (var Item in CollList)
                    {
                        Index++;

                        var CollItem = (Item is MOwnership
                                         ? ((MOwnership)Item).Owner
                                         : (Item is MAssignment
                                            ? ((MAssignment)Item).AssignedValue
                                            : Item)) as IMModelClass;

                        if (CollItem != null)
                            ModelRev7_FindLocationsOf(ExaminedInstances, Route + CollDef.TechName + "[" + Index.ToString() + "]=" + CollItem.ToHashCodeAndString() + "\\",
                                                      ref Results, CollItem, SearchedObject);
                    }
                }
                else
                {
                    var CollDict = Collection as IDictionary;

                    if (CollDict != null)
                    {
                        var Index = -1;
                        foreach (var Key in CollDict.Keys)
                        {
                            Index++;

                            var CollKey = (Key is MOwnership
                                             ? ((MOwnership)Key).Owner
                                             : (Key is MAssignment
                                                ? ((MAssignment)Key).AssignedValue
                                                : Key)) as IMModelClass;

                            if (CollKey != null)
                                ModelRev7_FindLocationsOf(ExaminedInstances, Route + CollDef.TechName + "[" + Index.ToString() + "]K=" + CollKey.ToHashCodeAndString() + "\\",
                                                          ref Results, CollKey, SearchedObject);
                        }

                        Index = -1;
                        foreach (var Value in CollDict.Values)
                        {
                            Index++;

                            var CollValue = (Value is MOwnership
                                             ? ((MOwnership)Value).Owner
                                             : (Value is MAssignment
                                                ? ((MAssignment)Value).AssignedValue
                                                : Value)) as IMModelClass;

                            if (CollValue != null)
                                ModelRev7_FindLocationsOf(ExaminedInstances, Route + CollDef.TechName + "[" + Index.ToString() + "]V=" + CollValue.ToHashCodeAndString() + "\\",
                                                          ref Results, CollValue, SearchedObject);
                        }
                    }
                }
            }
        }
 
        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Fixes any Unique-Element duplicated Global-Id.
        /// </summary>
        private static bool ModelRev8_FixDuplicatedGlobalIds(Composition TargetCompo, string Route = null, IList<IMModelClass> ExaminedInstances = null,
                                                             IMModelClass Target = null)
        {
            if (Route == null)
                Route = "\\\\";

            if (ExaminedInstances == null)
                ExaminedInstances = new List<IMModelClass>();

            if (Target == null)
                Target = TargetCompo;

            if (ExaminedInstances.Contains(Target))
                return false;

            ExaminedInstances.Add(Target);

            Route = Route + Target.ToHashCodeAndString() + "~";

            var PropertyDefs = Target.ClassDefinition.Properties;
            foreach (var PropDef in PropertyDefs)
            {
                var Value = PropDef.Read(Target);
                var PropValue = (Value is MOwnership
                                 ? ((MOwnership)Value).Owner
                                 : (Value is MAssignment
                                    ? ((MAssignment)Value).AssignedValue
                                    : Value)) as IMModelClass;

                if (PropValue != null)
                    ModelRev8_FixDuplicatedGlobalIds(TargetCompo, Route + PropDef.TechName + "=", ExaminedInstances, PropValue);
            }

            var CollectionDefs = Target.ClassDefinition.Collections;

            foreach (var CollDef in CollectionDefs)
            {
                var Collection = (EditableCollection)CollDef.Read(Target);

                var CollList = Collection as IList;

                if (CollList != null)
                {
                    var Duplicates = new List<UniqueElement>();

                    var Index = -1;
                    foreach (var Item in CollList)
                    {
                        Index++;

                        var CollItem = (Item is MOwnership
                                         ? ((MOwnership)Item).Owner
                                         : (Item is MAssignment
                                            ? ((MAssignment)Item).AssignedValue
                                            : Item)) as IMModelClass;

                        if (CollItem != null)
                        {
                            var ItemId = CollDef.TechName + "[" + Index.ToString() + "]=" + CollItem.ToHashCodeAndString() + "\\";

                            if (Item is UniqueElement)
                            {
                                var Items = ((IEnumerable<object>)CollList).CastAs<UniqueElement, object>();
                                if (Items.Any(item => item != Item && item.GlobalId == ((UniqueElement)Item).GlobalId))
                                {
                                    Duplicates.Add((UniqueElement)Item);
                                    //T Console.WriteLine("%" + Route + ItemId + "=" + ((UniqueElement)Item).GlobalId.ToString() + (Item is FormalElement ? (((FormalElement)Item).Version != null ? "::" + ((FormalElement)Item).Version.Creation.ToString("yyyyMMdd.hhmmss") : "") : ""));
                                }
                            }

                            ModelRev8_FixDuplicatedGlobalIds(TargetCompo, ItemId,
                                                             ExaminedInstances, CollItem);
                        }
                    }

                    foreach (var Duplicate in Duplicates.Skip(1))
                        Duplicate.GlobalId = Guid.NewGuid();
                }
                else
                {
                    var CollDict = Collection as IDictionary;

                    if (CollDict != null)
                    {
                        var Duplicates = new List<UniqueElement>();

                        var Index = -1;
                        foreach (var Key in CollDict.Keys)
                        {
                            Index++;

                            var CollKey = (Key is MOwnership
                                             ? ((MOwnership)Key).Owner
                                             : (Key is MAssignment
                                                ? ((MAssignment)Key).AssignedValue
                                                : Key)) as IMModelClass;

                            if (CollKey != null)
                            {
                                var ItemId = CollDef.TechName + "[" + Index.ToString() + "]K=" + CollKey.ToHashCodeAndString() + "\\";

                                if (CollKey is UniqueElement)
                                {
                                    var Items = ((IEnumerable<object>)CollList).CastAs<UniqueElement, object>();
                                    if (Items.Any(item => item != CollKey && item.GlobalId == ((UniqueElement)CollKey).GlobalId))
                                    {
                                        Duplicates.Add((UniqueElement)CollKey);
                                        //T Console.WriteLine("%" + Route + ItemId + "=" + ((UniqueElement)CollKey).GlobalId.ToString() + (CollKey is FormalElement ? (((FormalElement)CollKey).Version != null ? "::" + ((FormalElement)CollKey).Version.Creation.ToString("yyyyMMdd.hhmmss") : "") : ""));
                                    }
                                }

                                ModelRev8_FixDuplicatedGlobalIds(TargetCompo, ItemId,
                                                                 ExaminedInstances, CollKey);
                            }
                        }

                        Index = -1;
                        foreach (var Value in CollDict.Values)
                        {
                            Index++;

                            var CollValue = (Value is MOwnership
                                             ? ((MOwnership)Value).Owner
                                             : (Value is MAssignment
                                                ? ((MAssignment)Value).AssignedValue
                                                : Value)) as IMModelClass;

                            if (CollValue != null)
                            {
                                var ItemId = CollDef.TechName + "[" + Index.ToString() + "]V=" + CollValue.ToHashCodeAndString() + "\\";

                                if (CollValue is UniqueElement)
                                {
                                    var Items = ((IEnumerable<object>)CollList).CastAs<UniqueElement, object>();
                                    if (Items.Any(item => item != CollValue && item.GlobalId == ((UniqueElement)CollValue).GlobalId))
                                    {
                                        Duplicates.Add((UniqueElement)CollValue);
                                        //T Console.WriteLine("%" + Route + ItemId + "=" + ((UniqueElement)CollValue).GlobalId.ToString() + (CollValue is FormalElement ? (((FormalElement)CollValue).Version != null ? "::" + ((FormalElement)CollValue).Version.Creation.ToString("yyyyMMdd.hhmmss") : "") : ""));
                                    }
                                }

                                ModelRev8_FixDuplicatedGlobalIds(TargetCompo, ItemId,
                                                                 ExaminedInstances, CollValue);
                            }
                        }

                        foreach (var Duplicate in Duplicates.Skip(1))
                            Duplicate.GlobalId = Guid.NewGuid();
                    }
                }
            } 
            
            return true;
        }


        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Removes some duplicate table-definitions (used for single field tables import).
        /// </summary>
        private static bool ModelRev9_RemoveDuplicateTableDefs(Domain Target)
        {
            var Surplus = Target.TableDefinitions.Where(td => td.TechName == "SingleTextField" && td.Alterability == EAlterability.System)
                                    .Skip(1).ToList();
            foreach (var Duplicate in Surplus)
                Target.TableDefinitions.RemoveAt(Target.TableDefinitions.IndexOf(Duplicate));

            Surplus = Target.TableDefinitions.Where(td => td.TechName == "SingleNumericField" && td.Alterability == EAlterability.System)
                                    .Skip(1).ToList();
            foreach (var Duplicate in Surplus)
                Target.TableDefinitions.RemoveAt(Target.TableDefinitions.IndexOf(Duplicate));

            return true;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Updates the Symbols' flipping properties to be Nullable{bool} and therefore support unassigned state.
        /// </summary>
        private static bool ModelRev10_UpdateSymbolsFlippingProperties(Composition Target)
        {
            var Views = Target.CompositeViews.Concat(Target.DeclaredIdeas.SelectMany(idea => idea.CompositeViews));

            foreach (var View in Views)
                foreach (var Symbol in View.ViewChildren.Select(vch => vch.Key).CastAs<VisualSymbol, object>())
                    Symbol.Apply_ModelRev10_UpdateSymbolsFlippingProperties();

            return true;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        // NEXT: private static bool ModelRevN_Fix...

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}