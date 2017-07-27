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
// File   : MergingManager.cs
// Object : Instrumind.ThinkComposer.Composer.Merging.MergingManager (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2013.09.01 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;
using Instrumind.Common.Visualization.Widgets;

using Instrumind.ThinkComposer.ApplicationProduct;
using Instrumind.ThinkComposer.Composer.Merging.Widgets;
using Instrumind.ThinkComposer.MetaModel;
using Instrumind.ThinkComposer.MetaModel.Configurations;
using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.Model;
using Instrumind.ThinkComposer.Model.GraphModel;
using Instrumind.ThinkComposer.Model.InformationModel;
using Instrumind.ThinkComposer.Model.VisualModel;

// Provides features for Compositions Merging
namespace Instrumind.ThinkComposer.Composer.Merging
{
    /// <summary>
    /// Prepares objects and resources for making Compositions Merge.
    /// </summary>
    public static class MergingManager
    {
        public const string MERGE_ACTION_NONE = "NON";
        public const string MERGE_ACTION_CREATE = "CRE";
        public const string MERGE_ACTION_UPDATE = "UPD";
        public const string MERGE_ACTION_DELETE = "DEL";

        public static readonly List<SimplePresentationElement> MergeActions = new List<SimplePresentationElement>();

        public static Func<object, object, bool> InitiallyByTechNameComparer =
            ((obj1, obj2) =>
            {
                if (obj1 == obj2)
                    return true;

                if (obj1 is IIdentifiableElement && obj2 is IIdentifiableElement)
                    return ((IIdentifiableElement)obj1).TechName == ((IIdentifiableElement)obj2).TechName;

                if (obj1 is IUniqueElement && obj2 is IUniqueElement)
                    return ((IUniqueElement)obj1).GlobalId == ((IUniqueElement)obj2).GlobalId;

                return (obj1.ToStringAlways() == obj2.ToStringAlways());
            });

        public static Func<object, object, bool> InitiallyByGlobalIdComparer =
            ((obj1, obj2) =>
            {
                if (obj1 == obj2)
                    return true;

                if (obj1 is IUniqueElement && obj2 is IUniqueElement)
                    return ((IUniqueElement)obj1).GlobalId == ((IUniqueElement)obj2).GlobalId;

                if (obj1 is IIdentifiableElement && obj2 is IIdentifiableElement)
                    return ((IIdentifiableElement)obj1).TechName == ((IIdentifiableElement)obj2).TechName;

                return (obj1.ToStringAlways() == obj2.ToStringAlways());
            });

        public static Func<object, object, bool> PreferredComparer = InitiallyByGlobalIdComparer;

        public static SchemaMemberGroup GroupConceptDefs { get; private set; }
        public static SchemaMemberGroup GroupRelationshipDefs { get; private set; }
        public static SchemaMemberGroup GroupLinkRoleVariantDefs { get; private set; }
        public static SchemaMemberGroup GroupTableStructureDefs { get; private set; }
        public static SchemaMemberGroup GroupBaseTables { get; private set; }
        public static SchemaMemberGroup GroupExternalLanguages { get; private set; }
        public static SchemaMemberGroup GroupIdeaDefClusters { get; private set; }
        public static SchemaMemberGroup GroupMarkerDefs { get; private set; }

        static MergingManager()
        {
            MergeActions.Add(new SimplePresentationElement("<NONE>", MERGE_ACTION_NONE, "Do nothing. No differences were detected."));
            MergeActions.Add(new SimplePresentationElement("Create", MERGE_ACTION_CREATE, "Create this object in the target, because not exist there.", Display.GetAppImage("add.png")));
            MergeActions.Add(new SimplePresentationElement("Update", MERGE_ACTION_UPDATE, "Update this object in the target, because differences were detected.", Display.GetAppImage("page_white_edit.png")));
            MergeActions.Add(new SimplePresentationElement("Delete", MERGE_ACTION_DELETE, "Delete this object (NOT RECOMMENDED), because not exist is the source.", Display.GetAppImage("delete.png")));

            SchemaMemberSelection.ClearRegisteredMemberTypeOperationsSets();

            // From the most specific to the most general...
            SchemaMemberSelection.RegisterMemberTypeOperationsSet<SchemaMemberGroup>(grp => grp.ChildrenGetter(),
                                                                                     grp => grp.Name,
                                                                                     grp => grp.Summary,
                                                                                     grp => grp.Pictogram);

            SchemaMemberSelection.RegisterMemberTypeOperationsSet<Idea>(//idea => idea.CompositeIdeas.OrderBy(ci => (ci is Concept ? "0" : "1") + ci.Name),
                idea => General.CreateArrayWithoutNulls<IRecognizableElement>
                                                       ((idea.CompositeConcepts.Any()
                                                         ? new SchemaMemberGroup("[CONCEPTS]", Display.GetAppImage("ref_concept.png"),
                                                                                 () => idea.CompositeConcepts.OrderBy(it => it.Name))
                                                         : null),
                                                        (idea.CompositeRelationships.Any()
                                                         ? new SchemaMemberGroup("[RELATIONSHIPS]", Display.GetAppImage("ref_relationship.png"),
                                                                                 () => idea.CompositeRelationships.OrderBy(it => it.Name))
                                                         : null),
                                                        (idea.CompositeActiveView != null
                                                         ? new SchemaMemberGroup("[COMPLEMENTS]", Display.GetAppImage("shapes.png"),
                                                                                 () => idea.CompositeActiveView.GetFreeComplements())
                                                         : null),
                    //- (idea is Composition ? idea.CompositeContentDomain : null),
                                                        (idea is Composition
                                                         ? new SchemaMemberGroup("[DOMAIN]", Display.GetAppImage("page_definition.png"),
                                                                                 () => idea.CompositeContentDomain.IntoEnumerable())
                                                         : null)),
                idea => idea.NameCaption,
                idea => idea.Summary,
                idea => idea.Pictogram,
                idea => idea.DescriptiveCaption,
                idea => idea.Definitor.Pictogram);

            SchemaMemberSelection.RegisterMemberTypeOperationsSet<Domain>(
                dom => General.CreateArrayWithoutNulls(GroupConceptDefs = new SchemaMemberGroup("Concept Definitions", ConceptDefinition.PredefinedDefaultPictogram,
                                                                                                () => dom.ConceptDefinitions.OrderBy(it => it.Name)),
                                                       GroupRelationshipDefs = new SchemaMemberGroup("Relationship Definitions", RelationshipDefinition.PredefinedDefaultPictogram,
                                                                                                     () => dom.RelationshipDefinitions.OrderBy(it => it.Name)),
                                                       GroupLinkRoleVariantDefs = new SchemaMemberGroup("Link-Role Variant Definitions", Display.GetAppImage("link_role_variants.png"),
                                                                                                        () => dom.LinkRoleVariants.OrderBy(it => it.Name)),
                                                       GroupTableStructureDefs = new SchemaMemberGroup("Table-Structure Definitions", Display.GetAppImage("table_alter.png"),
                                                                                                       () => dom.TableDefinitions.OrderBy(it => it.Name)),
                                                       GroupBaseTables = new SchemaMemberGroup("Base Tables", Display.GetAppImage("table_multiple.png"),
                                                                                               () => dom.BaseTables.OrderBy(it => it.Name)),
                                                       GroupExternalLanguages = new SchemaMemberGroup("External Languages", Display.GetAppImage("page_white_code_red.png"),
                                                                                                      () => dom.ExternalLanguages.OrderBy(it => it.Name)),
                                                       GroupIdeaDefClusters = new SchemaMemberGroup("Idea-Definition Clusters", Display.GetAppImage("def_clusters.png"),
                                                                                                    () => dom.ConceptDefClusters.OrderBy(it => it.Name)
                                                                                                              .Concat(dom.RelationshipDefClusters.OrderBy(it => it.Name))),
                                                       GroupMarkerDefs = new SchemaMemberGroup("Marker Definitions", Display.GetAppImage("tag_red.png"),
                                                                                               () => dom.MarkerDefinitions.OrderBy(it => it.Name))),
                dom => dom.NameCaption,
                dom => dom.Summary,
                dom => dom.Pictogram);

            SchemaMemberSelection.RegisterMemberTypeOperationsSet<IdeaDefinition>(idef => null,
                                                                                  idef => idef.NameCaption,
                                                                                  idef => idef.Summary,
                                                                                  idef => idef.Pictogram);

            /*- SchemaMemberSelection.RegisterMemberTypeOperationsSet<MarkerDefinition>(mdef => null,
                                                                                    mdef => mdef.NameCaption,
                                                                                    mdef => mdef.Summary,
                                                                                    mdef => mdef.Pictogram); */

            SchemaMemberSelection.RegisterMemberTypeOperationsSet<IRecognizableElement>(elem => null,
                                                                                        elem => elem.Name,
                                                                                        elem => elem.Summary,
                                                                                        elem => elem.Pictogram);

            SchemaMemberSelection.RegisterMemberTypeOperationsSet<VisualComplement>(vcom => null,
                                                                                    vcom => vcom.ContentAsText.GetTruncatedWithEllipsis(32).RemoveNewLines() +
                                                                                            " [" + vcom.Kind.Name + "]" +
                                                                                            " {" + vcom.GlobalId.ToString() + "}".Trim());

            SchemaMemberSelection.RegisterMemberTypeOperationsSet<IUniqueElement>(elem => null,
                                                                                  elem => "{" + elem.GlobalId.ToString() + "}: " + elem.ToString());

            SchemaMemberSelection.RegisterMemberTypeOperationsSet<object>(obj => null,
                                                                          obj => obj.ToString());
        }

        public static void MergeCompositions()
        {
            // if (!ProductDirector.ValidateEditionPermission(AppExec.LIC_EDITION_PROFESSIONAL, "Generate Files", false, new DateTime(2013, 6, 22)))
            //    return;

            var Target = ProductDirector.WorkspaceDirector.ActiveDocument as Composition;
            if (Target == null)
                return;

            Composition SelectedSource = null;

            var AvailableCompos = ProductDirector.WorkspaceDirector.Documents.CastAs<Composition, ISphereModel>()
                                                    .Except(Target.IntoEnumerable()).ToList<IRecognizableElement>();
            if (AvailableCompos.Any())
            {
                var OpenCode = Guid.NewGuid().ToString();
                AvailableCompos.Insert(0, new SimplePresentationElement("<OPEN>", OpenCode, "Open an external Composition/Domain file.", Display.GetAppImage("folder_page_white.png")));
                var SourceSelection = Display.DialogMultiOption("Select source...", "Please select the Source Composition/Domain from which to extract",
                                                                null, null, true, OpenCode, AvailableCompos.ToArray());
                if (SourceSelection == null)
                    return;

                SelectedSource = AvailableCompos.FirstOrDefault(com => com.TechName == SourceSelection) as Composition;
            }

            System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

            System.Windows.Application.Current.MainWindow.PostCall(
                wnd =>
                {
                    // Evaluated apart because no Compos were available, or because the "<OPEN>" option was choosen.
                    if (SelectedSource == null)
                    {
                        SelectedSource = CompositionsManager.OpenComposition();

                        // Must re-activate target/previous-current Composition
                        if (ProductDirector.WorkspaceDirector.ActiveDocument != Target)
                            ProductDirector.WorkspaceDirector.ActivateDocument(Target);
                    }

                    wnd.PostCall(wd => System.Windows.Input.Mouse.OverrideCursor = null, true);

                    if (SelectedSource == null)
                        return;

                    if (!MergingManager.PrepareAndMergeCompositions(SelectedSource, Target.OwnerComposition))
                        return;
                });
        }
        
        public static bool PrepareAndMergeCompositions(Composition SourceComposition, Composition TargetComposition)
        {
            var InstanceController = EntityInstanceController.AssignInstanceController(TargetComposition);
            InstanceController.StartEdit();

            var Randomizer = new Random();
            var SourceSelection = SchemaMemberSelection.CreateSelectionTree(SourceComposition, null, null,
                                    obj =>
                                    {
                                        var MrgActIdx = Randomizer.Next(0, 3);
                                        return Tuple.Create(MrgActIdx != 0 || !SchemaMemberSelection.GetAccesor(obj)
                                                                .ChildrenGetter(obj).IsEmpty(),                             // Is-Selectable
                                                            MrgActIdx != 0 && MrgActIdx != 3,                               // Is-Selected
                                                            !(obj is SchemaMemberGroup),                                    // Can-show-is-selected
                                                            (IRecognizableElement)MergingManager.MergeActions[MrgActIdx]);  // Merge-Action
                                    });

            var TargetSelection = SchemaMemberSelection.CreateSelectionTree(TargetComposition);
            
            var PreparePanel = new MergePreparer(SourceComposition, TargetComposition, SourceSelection, TargetSelection);
            /*
            InstanceController.PostValidate = ((curr, prev) =>
            {
                if (curr.TargetDirectory.IsAbsent() || !Directory.Exists(curr.TargetDirectory))
                    return "Target directory not set or not found".IntoList();

                return null;
            });
            InstanceController.PreApply = ((curr, prev, args) =>
            {
                curr.ExcludedIdeasGlobalIds.Clear();
                curr.ExcludedIdeasGlobalIds.AddRange(ConfigPanel.TargetConfiguration.CurrentSelection.GetSelection(false)   // Gets the not-selected Ideas
                                                            .Select(sel => sel.TargetIdea.GlobalId.ToString()));
                return true;
            }); */

            var EditPanel = Display.CreateEditPanel(TargetComposition, PreparePanel);

            var Title = "Merge '" + TargetComposition.Name + "' with '" + SourceComposition.Name + "'";
            var DoMerge = InstanceController.Edit(EditPanel, "Merge Compositions/Domains content", true, null, 950, 650).IsTrue();

            if (DoMerge)
            {
                var Merger = CreateMerger(SourceComposition, TargetComposition, SourceSelection);

                var Started = ProgressiveThreadedExecutor<int>.Execute<int>(Title, Merger.Merge,
                    (opresult) =>
                    {
                        if (!opresult.WasSuccessful)
                        {
                            Display.DialogMessage("Merge not completed!", opresult.Message, EMessageType.Warning);
                            return;
                        }

                        if (opresult.Message != null)
                            Console.WriteLine(opresult.Message);

                        Display.DialogMessage("Merge completed", opresult.Message.AbsentDefault("Compositions Merge successfully executed."));
                    });
            }

            return DoMerge;
        }

        private static CompositionMerger CreateMerger(Composition SourceComposition, Composition TargetComposition,
                                                      SchemaMemberSelection Selection)
        {
            var Merger = new CompositionMerger(SourceComposition, TargetComposition);

            var SelectedPairs = Selection.GetSelection(true);
            Merger.SourceEntities.AddRange(SelectedPairs.Select(sel => sel.Key.RefMember)
                                                         .CastAs<IModelEntity, object>());

            foreach (var SelectedPair in SelectedPairs)
            {
                if (SelectedPair.Value.RefMember == GroupConceptDefs && SelectedPair.Key.RefMember is ConceptDefinition)
                    Merger.SelectedConceptDefs.Add((ConceptDefinition)SelectedPair.Key.RefMember);
                else
                    if (SelectedPair.Value.RefMember == GroupRelationshipDefs && SelectedPair.Key.RefMember is RelationshipDefinition)
                        Merger.SelectedRelationshipDefs.Add((RelationshipDefinition)SelectedPair.Key.RefMember);
                    else
                        if (SelectedPair.Value.RefMember == GroupLinkRoleVariantDefs && SelectedPair.Key.RefMember is SimplePresentationElement)
                            Merger.SelectedLinkRoleVariantDefs.Add((SimplePresentationElement)SelectedPair.Key.RefMember);
                        else
                            if (SelectedPair.Value.RefMember == GroupBaseTables && SelectedPair.Key.RefMember is Table)
                                Merger.SelectedBaseTables.Add((Table)SelectedPair.Key.RefMember);
                            else
                                if (SelectedPair.Value.RefMember == GroupExternalLanguages && SelectedPair.Key.RefMember is ExternalLanguageDeclaration)
                                    Merger.SelectedExternalLanguages.Add((ExternalLanguageDeclaration)SelectedPair.Key.RefMember);
                                else
                                    if (SelectedPair.Value.RefMember == GroupIdeaDefClusters && SelectedPair.Key.RefMember is FormalPresentationElement)
                                    {
                                        var IdeaDefCluster = (FormalPresentationElement)SelectedPair.Key.RefMember;
                                        if (SourceComposition.CompositeContentDomain.ConceptDefClusters.Contains(IdeaDefCluster))
                                            Merger.SelectedConceptDefClusters.Add(IdeaDefCluster);
                                        else
                                            Merger.SelectedRelationshipDefClusters.Add(IdeaDefCluster);
                                    }
                                    else
                                        if (SelectedPair.Value.RefMember == GroupMarkerDefs && SelectedPair.Key.RefMember is MarkerDefinition)
                                            Merger.SelectedMarkerDefs.Add((MarkerDefinition)SelectedPair.Key.RefMember);
                                        else
                                            if (SelectedPair.Key.RefMember is IModelEntity)
                                                Merger.SelectedOtherEntities.Add((IModelEntity)SelectedPair.Key.RefMember);
            }

            return Merger;
        }
    }
}
