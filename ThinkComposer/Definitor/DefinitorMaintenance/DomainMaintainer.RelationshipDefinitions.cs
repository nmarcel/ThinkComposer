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
// File   : DomainMaintainer.RelationshipDefinitions.cs
// Object : Instrumind.ThinkComposer.Composer.DefinitorMaintenance.DomainMaintainer (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2010.02.22 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;
using Instrumind.Common.Visualization.Widgets;

using Instrumind.ThinkComposer;
using Instrumind.ThinkComposer.ApplicationProduct;
using Instrumind.ThinkComposer.Composer;
using Instrumind.ThinkComposer.Composer.Generation;
using Instrumind.ThinkComposer.Definitor.DefinitorMaintenance;
using Instrumind.ThinkComposer.Definitor.DefinitorUI.Widgets;
using Instrumind.ThinkComposer.Model;
using Instrumind.ThinkComposer.Model.GraphModel;
using Instrumind.ThinkComposer.MetaModel;
using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;

/// Maintenance services of Domain related Definitions.
namespace Instrumind.ThinkComposer.Definitor.DefinitorMaintenance
{
    /// <summary>
    /// Provides maintenance services for a Domain entity. Relationship Definitions part.
    /// </summary>
    public static partial class DomainMaintainer
    {
        public const string TABKEY_DEF_REL_LINKROLEDEF_ORIPAR = "DEF_REL_LNKROLDEF_ORIPAR";
        public const string TABKEY_DEF_REL_LINKROLEDEF_TARGET = "DEF_REL_LNKROLDEF_TARGET";
        public const string TABKEY_DEF_REL_CONNFORMAT = "DEF_REL_CONNFMT";

        public const double RELDEFWND_INI_WIDTH = 920;  // 820;
        public const double RELDEFWND_INI_HEIGHT = 715; // 720;

        public static void SetRelationshipDefinitionsMaintainer(ItemsGridMaintainer<Domain, RelationshipDefinition> TargetMaintainer)
        {
            TargetMaintainer.CreateItemOperation = RelationshipDefinitionCreate;
            TargetMaintainer.DeleteItemOperation = RelationshipDefinitionDelete;
            TargetMaintainer.EditItemOperation = RelationshipDefinitionEdit;
            TargetMaintainer.CloneItemOperation = RelationshipDefinitionClone;
        }

        public static RelationshipDefinition RelationshipDefinitionCreate(Domain OwnerEntity, IList<RelationshipDefinition> EditedList)
        {
            if (!ProductDirector.ValidateEditionPermission(AppExec.LIC_EDITION_FREE, "create Relationship Definitions"))
                return null;

            var ItemsCount = OwnerEntity.ConceptDefinitions.Count + EditedList.Count;
            var MaxQuota = AppExec.CurrentLicenseEdition.TechName.SelectCorresponding(LicensingConfig.IdeaDefinitionsCreationQuotas);

            if (!ProductDirector.ValidateEditionLimit(ItemsCount + 1, MaxQuota, "create", "Idea Definitions (Concept Defs. + Relationship Defs.)"))
                return null;

            int NewNumber = EditedList.Count + 1;
            string NewName = "RelationshipDef" + NewNumber.ToString();
            var Definitor = new RelationshipDefinition(OwnerEntity, Domain.GenericRelationshipDefinition,
                                                       NewName, NewName.TextToIdentifier(), Shapes.Ellipse);

            if (RelationshipDefinitionEdit(OwnerEntity, EditedList, Definitor))
                return Definitor;

            return null;
        }

        public static bool RelationshipDefinitionEdit(Domain SourceDomain, IList<RelationshipDefinition> EditedList, RelationshipDefinition RelationshipDef)
        {
            /*- if (!ProductDirector.ConfirmImmediateApply("Relationship Definition", "DomainEdit.RelationshipDefinition", "ApplyDialogChangesDirectly"))
                return false; */

            var CurrentWindow = Display.GetCurrentWindow();
            CurrentWindow.Cursor = Cursors.Wait;

            var InstanceController = EntityInstanceController.AssignInstanceController(RelationshipDef,
                (current, previous, editpanels) =>
                {
                    var CurrentDetailsEditor = (GlobalDetailsDefinitor)editpanels.First(editpanel => editpanel is GlobalDetailsDefinitor);

                    // IMPORTANT: Ensure that at least one linking variant is available.
                    if (current.OriginOrParticipantLinkRoleDef.AllowedVariants.Count < 1)
                        current.OriginOrParticipantLinkRoleDef.AllowedVariants.Add(current.OwnerDomain.LinkRoleVariants.FirstOrDefault());

                    if (current.TargetLinkRoleDef != null && current.TargetLinkRoleDef.AllowedVariants.Count < 1)
                        current.TargetLinkRoleDef.AllowedVariants.Add(current.OwnerDomain.LinkRoleVariants.FirstOrDefault());

                    return CurrentDetailsEditor.UpdateRelatedDetailDefinitions(current);
                });

            var DetDefEd = GlobalDetailsDefinitor.CreateGlobalDetailsDefinitor(InstanceController.EntityEditor, RelationshipDef);

            InstanceController.StartEdit();

            var ExtraGeneralContentsPanel = new Grid();
            ExtraGeneralContentsPanel.ColumnDefinitions.Add(new ColumnDefinition());
            ExtraGeneralContentsPanel.ColumnDefinitions.Add(new ColumnDefinition());

            var ExtraGenContentPanelLeft = new StackPanel();
            Grid.SetColumn(ExtraGenContentPanelLeft, 0);
            ExtraGeneralContentsPanel.Children.Add(ExtraGenContentPanelLeft);

            var ExtraGenContentPanelRight = new StackPanel();
            Grid.SetColumn(ExtraGenContentPanelRight, 1);
            ExtraGeneralContentsPanel.Children.Add(ExtraGenContentPanelRight);

            var Expositor = new EntityPropertyExpositor(RelationshipDefinition.__IsComposable.TechName);
            Expositor.LabelMinWidth = 180;
            ExtraGenContentPanelLeft.Children.Add(Expositor);

            Expositor = new EntityPropertyExpositor(ConceptDefinition.__IsVersionable.TechName);
            Expositor.LabelMinWidth = 180;
            ExtraGenContentPanelLeft.Children.Add(Expositor);

            Expositor = new EntityPropertyExpositor(ConceptDefinition.__HasGroupRegion.TechName);
            Expositor.LabelMinWidth = 180;
            ExtraGenContentPanelLeft.Children.Add(Expositor);

            Expositor = new EntityPropertyExpositor(ConceptDefinition.__HasGroupLine.TechName);
            Expositor.LabelMinWidth = 180;
            ExtraGenContentPanelLeft.Children.Add(Expositor);

            Expositor = new EntityPropertyExpositor(RelationshipDefinition.__RepresentativeShape.TechName);
            Expositor.LabelMinWidth = 180;
            ExtraGenContentPanelLeft.Children.Add(Expositor);
            Expositor.PostCall(expo =>
            {
                var Combo = expo.ValueEditor as ComboBox;
                if (Combo != null)
                {
                    var Panel = new FrameworkElementFactory(typeof(WrapPanel));
                    Panel.SetValue(WrapPanel.WidthProperty, 810.0);
                    Panel.SetValue(WrapPanel.ItemWidthProperty, 200.0);
                    // Don't work as expected: Panel.SetValue(WrapPanel.OrientationProperty, Orientation.Vertical);
                    var Templ = new ItemsPanelTemplate(Panel);
                    Combo.ItemsPanel = Templ;
                }
            }, true);

            Expositor = new EntityPropertyExpositor(ConceptDefinition.__PreciseConnectByDefault.TechName);
            Expositor.LabelMinWidth = 180;
            ExtraGenContentPanelLeft.Children.Add(Expositor);

            var ClosuredExpositor = new EntityPropertyExpositor(IdeaDefinition.__Cluster.TechName);
            ClosuredExpositor.LabelMinWidth = 210;
            ExtraGenContentPanelRight.Children.Add(ClosuredExpositor);
            var PropCtl = InstanceController.GetPropertyController(IdeaDefinition.__Cluster.TechName);
            PropCtl.ComplexOptionsProviders =
                Tuple.Create<IRecognizableElement, Action<object>>(
                    new SimplePresentationElement("Edit Clusters", "EditClusters", "Edit Clusters", Display.GetAppImage("def_clusters.png")),
                    obj =>
                        {
                            if (DomainServices.DefineDomainIdeaDefClusters(SourceDomain, DomainServices.TABKEY_IDEF_CLUSTER_RELATIONSHIP))
                                ClosuredExpositor.RetrieveAvailableItems();
                        }).IntoArray();

            Expositor = new EntityPropertyExpositor(RelationshipDefinition.__CanAutomaticallyCreateRelatedConcepts.TechName);
            Expositor.LabelMinWidth = 210;
            ExtraGenContentPanelRight.Children.Add(Expositor);

            Expositor = new EntityPropertyExpositor(RelationshipDefinition.__IsSimple.TechName);
            Expositor.LabelMinWidth = 210;
            ExtraGenContentPanelRight.Children.Add(Expositor);

            Expositor = new EntityPropertyExpositor(RelationshipDefinition.__HideCentralSymbolWhenSimple.TechName);
            Expositor.LabelMinWidth = 210;
            ExtraGenContentPanelRight.Children.Add(Expositor);

            Expositor = new EntityPropertyExpositor(RelationshipDefinition.__ShowNameIfHidingCentralSymbol.TechName);
            Expositor.LabelMinWidth = 210;
            ExtraGenContentPanelRight.Children.Add(Expositor);

            var VisualSymbolFormatter = new VisualSymbolFormatSubform("DefaultSymbolFormat");
            var VisualConnectorsFormatter = new VisualConnectorsFormatSubform("DefaultConnectorsFormat", RelationshipDef.DefaultConnectorsFormat);

            var TemplateEd = new TemplateEditor();
            TemplateEd.Initialize(SourceDomain, SourceDomain.CurrentExternalLanguage.TechName, typeof(Relationship), RelationshipDef,
                                  IdeaDefinition.__OutputTemplates, Domain.__OutputTemplatesForRelationships, false,
                                  Tuple.Create<string, ImageSource, string, Action<string>>("Insert predefined...", Display.GetAppImage("page_white_wrench.png"), "Inserts a system predefined Output-Template text, at the current selection.",
                                                                                            text => { var tpl = DomainServices.GetPredefinedOutputTemplate(); if (tpl != null) TemplateEd.SteSyntaxEditor.ReplaceTextAtSelection(tpl); }),
                                  Tuple.Create<string, ImageSource, string, Action<string>>("Test", Display.GetAppImage("page_white_wrench.png"), "Test the Template against a source Relationship.",
                                                                                            text => RememberedTemplateTestRelationship[SourceDomain.OwnerComposition] =
                                                                                                         TemplateTester.TestTemplate(typeof(Relationship), RelationshipDef, IdeaDefinition.__OutputTemplates.Name,
                                                                                                                                     RelationshipDef.GetGenerationFinalTemplate(TemplateEd.CurrentTemplate.Language, text, TemplateEd.ChbExtendsBaseTemplate.IsChecked.IsTrue()),
                                                                                                                                     SourceDomain.OwnerComposition,
                                                                                                                                     RememberedTemplateTestRelationship.GetValueOrDefault(SourceDomain.OwnerComposition)
                                                                                                                                         .NullDefault(SourceDomain.OwnerComposition.CompositeIdeas.OrderBy(idea => idea.Name)
                                                                                                                                                         .FirstOrDefault(idea => idea.IdeaDefinitor ==  RelationshipDef)
                                                                                                                                                             .NullDefault(SourceDomain.OwnerComposition.DeclaredIdeas
                                                                                                                                                                 .FirstOrDefault(idea => idea is Relationship))))));
            var TemplateTab = TabbedEditPanel.CreateTab(DomainServices.TABKEY_DEF_OUTTEMPLATE, "Output-Templates", "Definition of Output-Templates", TemplateEd);

            var SpecTabs = General.CreateList(
                            TabbedEditPanel.CreateTab(DomainServices.TABKEY_DEF_ARRANGE, "Arrange", "Settings for relate and group ideas.",
                                                      new ArrangeTabForRelationshipDef(RelationshipDef)),
                            TabbedEditPanel.CreateTab(TABKEY_DEF_REL_LINKROLEDEF_ORIPAR, "Origin/Participant Link-Role Def.", "Definition of Link-Role for Origin or Participant.",
                                                      new LinkRoleDefSpecSubform("OriginOrParticipantLinkRoleDef", false, RelationshipDef.OriginOrParticipantLinkRoleDef, RelationshipDef)),
                            TabbedEditPanel.CreateTab(TABKEY_DEF_REL_LINKROLEDEF_TARGET, "Target Link-Role Def.", "Definition of Link-Role for Target.",
                                                      new LinkRoleDefSpecSubform("TargetLinkRoleDef", true, RelationshipDef.TargetLinkRoleDef, RelationshipDef)),
                            TabbedEditPanel.CreateTab(DomainServices.TABKEY_DEF_FORMAT, "Symbol format", "Definition for the Central/Main-Symbol format.",
                                                      VisualSymbolFormatter),
                            TabbedEditPanel.CreateTab(TABKEY_DEF_REL_CONNFORMAT, "Connectors format", "Definition for the Connectors format.",
                                                      VisualConnectorsFormatter),
                            TabbedEditPanel.CreateTab(DomainServices.TABKEY_DEF_DETAILS, "Details", "Details definition.", DetDefEd),
                            TemplateTab);

            var EditPanel = Display.CreateEditPanel(RelationshipDef, SpecTabs, true, null, Display.TABKEY_TECHSPEC + General.STR_SEPARATOR + DomainServices.TABKEY_DEF_OUTTEMPLATE,
                                                    true, false, true, true, ExtraGeneralContentsPanel);
            EditPanel.Loaded +=
                ((sender, args) =>
                {
                    var OwnerWindow = EditPanel.GetNearestVisualDominantOfType<Window>();
                    OwnerWindow.MinWidth = 750;
                    OwnerWindow.MinHeight = 550;
                    OwnerWindow.PostCall(wnd => CurrentWindow.Cursor = Cursors.Arrow);
                });

            if (IdeaDefinition.__OutputTemplates.IsAdvanced)
                EditPanel.ShowAdvancedMembersChanged +=
                    ((show) =>
                    {
                        TemplateTab.SetVisible(show);
                        if (!show)
                        {
                            var OwnerTabControl = TemplateTab.GetNearestDominantOfType<TabControl>();
                            if (OwnerTabControl.SelectedItem == TemplateTab)
                                OwnerTabControl.SelectedIndex = 0;
                        }
                    });

            var Previewer = new VisualElementPreviewer(VisualSymbolFormatter.VisualElementFormatter.ExpoLineBrush,
                                                       VisualSymbolFormatter.VisualElementFormatter.ExpoLineThickness,
                                                       VisualSymbolFormatter.VisualElementFormatter.ExpoLineDash,
                                                       VisualSymbolFormatter.VisualElementFormatter.ExpoMainBackground,
                                                       VisualConnectorsFormatter.VisualElementFormatter.ExpoLineBrush,
                                                       VisualConnectorsFormatter.VisualElementFormatter.ExpoLineThickness,
                                                       VisualConnectorsFormatter.VisualElementFormatter.ExpoLineDash,
                                                       VisualConnectorsFormatter.VisualElementFormatter.ExpoMainBackground);
            Previewer.AttachSource(RelationshipDef);
            Previewer.Margin = new Thickness(4);
            EditPanel.HeaderContent = Previewer;
            Previewer.PostCall(prv => prv.ShowPreview());   // Required because of initially unpopulated properties of old Domains.

            var Result = InstanceController.Edit(EditPanel, "Edit Relationship Definition - " + RelationshipDef.ToString(), true, null,
                                                 RELDEFWND_INI_WIDTH, RELDEFWND_INI_HEIGHT).IsTrue();
            return Result;
        }
        public static Dictionary<Composition, Idea> RememberedTemplateTestRelationship = new Dictionary<Composition, Idea>();

        public static bool RelationshipDefinitionDelete(Domain OwnerEntity, IList<RelationshipDefinition> EditedList, RelationshipDefinition RelationshipDef)
        {
            var Result = Display.DialogMessage("Confirmation", "Are you sure you want to Delete the '" + RelationshipDef.Name + "' Relationship Definition?",
                                               EMessageType.Question, System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxResult.No);
            return (Result == MessageBoxResult.Yes);
        }

        public static RelationshipDefinition RelationshipDefinitionClone(Domain OwnerEntity, IList<RelationshipDefinition> EditedList, RelationshipDefinition RelationshipDef)
        {
            var Result = new RelationshipDefinition();
            Result.PopulateFrom(RelationshipDef, null, ECloneOperationScope.Deep);

            var NamesWereEquivalent = (Result.TechName == Result.Name.TextToIdentifier());
            Result.Name = Result.Name + "(copy)";   // Auto-update of TechName when equivalents
            if (!NamesWereEquivalent) Result.TechName = Result.TechName + "_copy";

            return Result;
        }
    }
}
