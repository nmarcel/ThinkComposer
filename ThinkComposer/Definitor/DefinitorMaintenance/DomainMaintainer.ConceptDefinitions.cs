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
// File   : DomainMaintainer.ConceptDefinitions.cs
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
using System.Windows.Data;
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
    /// Provides maintenance services for a Domain entity. Concept Definitions part.
    /// </summary>
    public static partial class DomainMaintainer
    {
        public const double CONDEFWND_INI_WIDTH = 790;
        public const double CONDEFWND_INI_HEIGHT = 592;

        public static void SetConceptDefinitionsMaintainer(ItemsGridMaintainer<Domain, ConceptDefinition> TargetMaintainer)
        {
            TargetMaintainer.CreateItemOperation = ConceptDefinitionCreate;
            TargetMaintainer.DeleteItemOperation = ConceptDefinitionDelete;
            TargetMaintainer.EditItemOperation = ConceptDefinitionEdit;
            TargetMaintainer.CloneItemOperation = ConceptDefinitionClone;
        }

        public static ConceptDefinition ConceptDefinitionCreate(Domain OwnerEntity, IList<ConceptDefinition> EditedList)
        {
            if (!ProductDirector.ValidateEditionPermission(AppExec.LIC_EDITION_FREE, "create Concept Definitions"))
                return null;

            var ItemsCount = OwnerEntity.RelationshipDefinitions.Count + EditedList.Count;
            var MaxQuota = AppExec.CurrentLicenseEdition.TechName.SelectCorresponding(LicensingConfig.IdeaDefinitionsCreationQuotas);

            if (!ProductDirector.ValidateEditionLimit(ItemsCount + 1, MaxQuota, "create", "Idea Definitions (Concept Defs. + Relationship Defs.)"))
                return null;

            int NewNumber = EditedList.Count + 1;
            string NewName = "ConceptDef" + NewNumber.ToString();
            var Definitor = new ConceptDefinition(OwnerEntity, Domain.GenericConceptDefinition,
                                                  NewName, NewName.TextToIdentifier(), Shapes.RoundedRectangle);

            if (ConceptDefinitionEdit(OwnerEntity, EditedList, Definitor))
                return Definitor;

            return null;
        }

        public static bool ConceptDefinitionEdit(Domain SourceDomain, IList<ConceptDefinition> EditedList, ConceptDefinition ConceptDef)
        {
            /*- if (!ProductDirector.ConfirmImmediateApply("Concept Definition", "DomainEdit.ConceptDefinition", "ApplyDialogChangesDirectly"))
                return false; */

            var CurrentWindow = Display.GetCurrentWindow();
            CurrentWindow.Cursor = Cursors.Wait;

            var InstanceController = EntityInstanceController.AssignInstanceController(ConceptDef,
                (current, previous, editpanels) =>
                {
                    var CurrentDetailsEditor = (GlobalDetailsDefinitor)editpanels.First(editpanel => editpanel is GlobalDetailsDefinitor);
                    return CurrentDetailsEditor.UpdateRelatedDetailDefinitions(ConceptDef);
                });

            var DetDefEd = GlobalDetailsDefinitor.CreateGlobalDetailsDefinitor(InstanceController.EntityEditor, ConceptDef);
            InstanceController.StartEdit();

            var VisualSymbolFormatter = new VisualSymbolFormatSubform("DefaultSymbolFormat");

            var TemplateEd = new TemplateEditor();
            TemplateEd.Initialize(SourceDomain, SourceDomain.CurrentExternalLanguage.TechName, typeof(Concept), ConceptDef,
                                  IdeaDefinition.__OutputTemplates, Domain.__OutputTemplatesForConcepts, false,
                                  Tuple.Create<string, ImageSource, string, Action<string>>("Insert predefined...", Display.GetAppImage("page_white_wrench.png"), "Inserts a system predefined Output-Template text, at the current selection.",
                                                                                            text => { var tpl = DomainServices.GetPredefinedOutputTemplate(); if (tpl != null) TemplateEd.SteSyntaxEditor.ReplaceTextAtSelection(tpl); }),
                                  Tuple.Create<string, ImageSource, string, Action<string>>("Test", Display.GetAppImage("page_white_wrench.png"), "Test the Template against a source Concept.",
                                                                                            text => RememberedTemplateTestConcept[SourceDomain.OwnerComposition] =
                                                                                                         TemplateTester.TestTemplate(typeof(Concept), ConceptDef, IdeaDefinition.__OutputTemplates.Name,
                                                                                                                                     ConceptDef.GetGenerationFinalTemplate(TemplateEd.CurrentTemplate.Language, text, TemplateEd.ChbExtendsBaseTemplate.IsChecked.IsTrue()),
                                                                                                                                     SourceDomain.OwnerComposition,
                                                                                                                                     RememberedTemplateTestConcept.GetValueOrDefault(SourceDomain.OwnerComposition)
                                                                                                                                         .NullDefault(SourceDomain.OwnerComposition.CompositeIdeas.OrderBy(idea => idea.Name)
                                                                                                                                                         .FirstOrDefault(idea => idea.IdeaDefinitor == ConceptDef)
                                                                                                                                                             .NullDefault(SourceDomain.OwnerComposition.DeclaredIdeas
                                                                                                                                                                 .FirstOrDefault(idea => idea is Concept))))));

            var TemplateTab = TabbedEditPanel.CreateTab(DomainServices.TABKEY_DEF_OUTTEMPLATE, "Output-Templates", "Definitions of Output-Templates", TemplateEd);

            var SpecTabs = General.CreateList(
                            TabbedEditPanel.CreateTab(DomainServices.TABKEY_DEF_ARRANGE, "Arrange", "Settings for relate and group ideas.",
                                                      new ArrangeTabForConceptDef(ConceptDef)),
                            TabbedEditPanel.CreateTab(DomainServices.TABKEY_DEF_FORMAT, "Symbol format", "Definition for the Symbol format.",
                                                      VisualSymbolFormatter),
                            TabbedEditPanel.CreateTab(DomainServices.TABKEY_DEF_DETAILS, "Details", "Details definition.", DetDefEd),
                            TemplateTab);

            var ExtraGeneralContentsPanel = new Grid();
            ExtraGeneralContentsPanel.ColumnDefinitions.Add(new ColumnDefinition());
            ExtraGeneralContentsPanel.ColumnDefinitions.Add(new ColumnDefinition());

            var ExtraGenContentPanelLeft = new StackPanel();
            Grid.SetColumn(ExtraGenContentPanelLeft, 0);
            ExtraGeneralContentsPanel.Children.Add(ExtraGenContentPanelLeft);

            var ExtraGenContentPanelRight = new StackPanel();
            Grid.SetColumn(ExtraGenContentPanelRight, 1);
            ExtraGeneralContentsPanel.Children.Add(ExtraGenContentPanelRight);

            ExtraGenContentPanelLeft = new StackPanel();
            ExtraGenContentPanelLeft.Width = 316;
            ExtraGeneralContentsPanel.Children.Add(ExtraGenContentPanelLeft);

            var Expositor = new EntityPropertyExpositor(ConceptDefinition.__IsComposable.TechName);
            Expositor.LabelMinWidth = 130;
            ExtraGenContentPanelLeft.Children.Add(Expositor);

            Expositor = new EntityPropertyExpositor(ConceptDefinition.__IsVersionable.TechName);
            Expositor.LabelMinWidth = 130;
            ExtraGenContentPanelLeft.Children.Add(Expositor);

            Expositor = new EntityPropertyExpositor(ConceptDefinition.__RepresentativeShape.TechName);
            Expositor.LabelMinWidth = 130;
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
            Expositor.LabelMinWidth = 130;
            ExtraGenContentPanelLeft.Children.Add(Expositor);

            var ClosuredExpositor = new EntityPropertyExpositor(IdeaDefinition.__Cluster.TechName);
            ClosuredExpositor.LabelMinWidth = 130;
            ExtraGenContentPanelRight.Children.Add(ClosuredExpositor);
            var PropCtl = InstanceController.GetPropertyController(IdeaDefinition.__Cluster.TechName);
            PropCtl.ComplexOptionsProviders =
                Tuple.Create<IRecognizableElement, Action<object>>(
                    new SimplePresentationElement("Edit Clusters", "EditClusters", "Edit Clusters", Display.GetAppImage("def_clusters.png")),
                    obj =>
                        {
                            if (DomainServices.DefineDomainIdeaDefClusters(SourceDomain, DomainServices.TABKEY_IDEF_CLUSTER_CONCEPT))
                                ClosuredExpositor.RetrieveAvailableItems();
                        }).IntoArray();

            var EditPanel = Display.CreateEditPanel(ConceptDef, SpecTabs, true, null, Display.TABKEY_TECHSPEC + General.STR_SEPARATOR + DomainServices.TABKEY_DEF_OUTTEMPLATE,
                                                    true, false, true, true, ExtraGeneralContentsPanel);
            EditPanel.Loaded +=
                ((sender, args) =>
                {
                    var OwnerWindow = EditPanel.GetNearestVisualDominantOfType<Window>();
                    OwnerWindow.MinWidth = 770;
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
                                                       VisualSymbolFormatter.VisualElementFormatter.ExpoMainBackground);
            Previewer.AttachSource(ConceptDef);
            Previewer.Margin = new Thickness(4);
            EditPanel.HeaderContent = Previewer;

            var Result = InstanceController.Edit(EditPanel, "Edit Concept Definition - " + ConceptDef.ToString(), true, null,
                                                 CONDEFWND_INI_WIDTH, CONDEFWND_INI_HEIGHT).IsTrue();

            return Result;
        }
        public static Dictionary<Composition, Idea> RememberedTemplateTestConcept = new Dictionary<Composition, Idea>();

        public static bool ConceptDefinitionDelete(Domain OwnerEntity, IList<ConceptDefinition> EditedList, ConceptDefinition ConceptDef)
        {
            var Result = Display.DialogMessage("Confirmation", "Are you sure you want to Delete the '" + ConceptDef.Name + "' Concept Definition?",
                                               EMessageType.Question, System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxResult.No);
            return (Result == MessageBoxResult.Yes);
        }

        public static ConceptDefinition ConceptDefinitionClone(Domain OwnerEntity, IList<ConceptDefinition> EditedList, ConceptDefinition ConceptDef)
        {
            var Result = new ConceptDefinition();
            Result.PopulateFrom(ConceptDef, null, ECloneOperationScope.Deep);

            var NamesWereEquivalent = (Result.TechName == Result.Name.TextToIdentifier());
            Result.Name = Result.Name + "(copy)";   // Auto-update of TechName when equivalents
            if (!NamesWereEquivalent) Result.TechName = Result.TechName + "_copy";

            return Result;
        }
    }
}
