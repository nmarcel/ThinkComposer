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
// File   : CompositionEngine.Editing.cs
// Object : Instrumind.ThinkComposer.Composer.CompositionEngine (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.07.11 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using Instrumind.ThinkComposer.Definitor;
using Instrumind.ThinkComposer.Definitor.DefinitorMaintenance;
using Instrumind.ThinkComposer.Composer.ComposerUI;
using Instrumind.ThinkComposer.Composer.ComposerUI.Widgets;
using Instrumind.ThinkComposer.Composer.Generation;
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
    /// Takes care of the edition of a particular Composition instance (Editing partial-file).
    /// </summary>
    public partial class CompositionEngine : DocumentEngine
    {
        // Keys of the Tabs shown
        public const string TABKEY_REL_LINKS = "LNKS";
        public const string TABKEY_TBL_DATA = "DATA";
        public const string TABKEY_ATTACHMENT = "ATTC";
        public const string TABKEY_FORMAT = "FRMT";

        // ----------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Currently selected visual representation for later be used as format source.
        /// </summary>
        public VisualRepresentation CurrentVisRepSelectedAsFormatSource { get; set; }

        /// <summary>
        /// Currently selected visual complement for later be used as format source.
        /// </summary>
        public VisualComplement CurrentVisCmpSelectedAsFormatSource { get; set; }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Opens the properties interactive editor of the Composition.
        /// </summary>
        public void EditCompositionProperties()
        {
            // POSTPONED: EDIT AS CONCEPT WHEN THE DETAILS CAN BE USED/REFERENCED.
            // this.EditConceptProperties(this.TargetComposition, null, false, true);

            var EditingController = EntityInstanceController.AssignInstanceController(this.TargetComposition);
            EditingController.StartEdit();


            // Show summary of total Ideas in the Composition...
            var Summarizer = new StackPanel();
            Summarizer.Background = Display.GetResource<Brush, EntitledPanel>("PanelBrush");
            Summarizer.Orientation = Orientation.Horizontal;
            Summarizer.Margin = new Thickness(4);

            int TotConcepts = 0, TotRelationships = 0;
            this.TargetComposition.DeclaredIdeas.ForEach(idea => { if (idea is Concept) TotConcepts++; else TotRelationships++; });

            var Foreground = Display.GetResource<Brush, EntitledPanel>("PanelTextBrush");
            var Text = new TextBlock();
            Text.Text = "Total Ideas: " + (TotConcepts + TotRelationships).ToString();
            Text.Foreground = Foreground;
            Text.Margin = new Thickness(2);
            Text.FontWeight = FontWeights.Bold;
            Summarizer.Children.Add(Text);

            Text = new TextBlock();
            Text.Text = "(" + TotConcepts.ToString() + " Concepts + " + TotRelationships.ToString() + " Relationships)";
            Text.Foreground = Foreground;
            Text.Margin = new Thickness(2);
            Summarizer.Children.Add(Text);

            // Show Composability depth levels used...
            var CompoDepth = new TextBlock();
            CompoDepth.Text = "Composition depth levels: " + this.TargetComposition.GetCompositeSubLevelsCount().ToString();
            CompoDepth.Foreground = Foreground;
            CompoDepth.Padding = new Thickness(2);
            CompoDepth.Margin = new Thickness(4);
            CompoDepth.FontWeight = FontWeights.Bold;
            CompoDepth.Background = Display.GetResource<Brush, EntitledPanel>("PanelBrush");

            // .................................................

            var EditPanel = Display.CreateEditPanel(this.TargetComposition,
                                                    IncludeDescriptionTab: true,
                                                    IncludeVersioningTab: true,
                                                    IncludeTechSpecTab: true,
                                                    ExtraGeneralContents: Summarizer.Concatenate<UIElement>(CompoDepth).ToArray());
            if (this.TargetComposition.IdeaDefinitor.CanGenerateFiles)
                EditPanel.AppendExtraButton("", "Preview file generation...", Display.GetAppImage("fgen_prev.png"),
                                            ins => GenerationManager.ShowGenerationFilePreview(this.TargetComposition));

            if (EditingController.Edit(EditPanel, "Edit Composition - " + this.TargetComposition.ToString()).IsTrue())
                ProductDirector.EntitleApplication(this.TargetComposition.ToStringAlways());
        }

        /// <summary>
        /// Opens the properties interactive editor of the specified Concept, via its agent.
        /// Plus main Symbol, indication of acccess only to local detail tables (for Domain) and indication to edit custom-looks.
        /// Optionally, an initial opened tab key and marker-assignment can be specified.
        /// </summary>
        public void EditConceptProperties(Concept ExposingConcept, VisualSymbol ExposingSymbol,
                                          bool AccessOnlyToLocalDetailTablesForDomain, bool IsComposition = false,
                                          string OpenedTabKey = null, MarkerAssignment PointedMarkerAssignment = null)
        {
            General.ContractRequires(ExposingSymbol == null || ExposingConcept == ExposingSymbol.OwnerRepresentation.RepresentedIdea);

            var DetLisEd = LocalDetailsEditor.CreateLocalDetailsEditor(ExposingConcept, ExposingSymbol,
                                                                       !AccessOnlyToLocalDetailTablesForDomain,
                                                                       AccessOnlyToLocalDetailTablesForDomain);

            var InstanceController = EntityInstanceController.AssignInstanceController(ExposingConcept,
                                                (current, previous, editpanels) =>
                                                {
                                                    var CurrentDetailsEditor = (LocalDetailsEditor)editpanels.First(editpanel => editpanel is LocalDetailsEditor);
                                                    return CurrentDetailsEditor.UpdateRelatedDetails((Concept)current, ExposingSymbol);
                                                });
            InstanceController.StartEdit();

            var SpecTabs = new List<TabItem>();
            SpecTabs.Add(TabbedEditPanel.CreateTab(Display.TABKEY_DETAILS,
                                                   (AccessOnlyToLocalDetailTablesForDomain ? "Tables" : "Details"),
                                                   (AccessOnlyToLocalDetailTablesForDomain ? "Table based information." : "Detailed information."),
                                                   DetLisEd));

            if (!AccessOnlyToLocalDetailTablesForDomain)
            {
                var MarkingsMaintainer =
                    new MarkingsEditor(ExposingConcept,
                                       (() => MarkerAssignmentCommand
                                                .CreateMarkerAssignment(ExposingConcept.EditEngine,
                                                                        ExposingConcept.OwnerComposition.CompositeContentDomain.MarkerDefinitions.FirstOrDefault())),
                                       ((marker) => MarkerAssignmentCommand.EditMarkerAssignmentDescriptor(this, marker)),
                                       PointedMarkerAssignment);

                SpecTabs.Add(TabbedEditPanel.CreateTab(Display.TABKEY_MARKINGS, "Markers", "Assigned markers.", MarkingsMaintainer));
            }

            if (ExposingConcept.IdeaDefinitor.IsVersionable && ExposingConcept.Version == null)
                ExposingConcept.Version = new VersionCard();

            var EditPanel = Display.CreateEditPanel(ExposingConcept, SpecTabs, false, OpenedTabKey, null,
                                                    true && !AccessOnlyToLocalDetailTablesForDomain,
                                                    false && !AccessOnlyToLocalDetailTablesForDomain,
                                                    (IsComposition || ExposingConcept.Version != null) && !AccessOnlyToLocalDetailTablesForDomain,
                                                    !AccessOnlyToLocalDetailTablesForDomain);

            if (ExposingConcept.IdeaDefinitor.CanGenerateFiles)
                EditPanel.AppendExtraButton("", "Preview file generation...", Display.GetAppImage("fgen_prev.png"),
                                            ins => GenerationManager.ShowGenerationFilePreview(ExposingConcept));

            if (AccessOnlyToLocalDetailTablesForDomain)
            {
                var TabbedPanel = EditPanel.Content as TabbedEditPanel;

                foreach (var Tab in TabbedPanel.Tabs)
                    if (Tab.Key != Display.TABKEY_DETAILS)
                        Tab.Value.SetVisible(false);

                var TargetTab = TabbedPanel.GetTab(Display.TABKEY_DETAILS);
                TargetTab.IsSelected = true;

                // Postcalled in order to let the template be applied on control's load
                TargetTab.PostCall(tabitem => tabitem.GetTemplateChild<Grid>("BackPanel").SetVisible(false));
            }

            var Result = InstanceController.Edit(EditPanel, (AccessOnlyToLocalDetailTablesForDomain
                                                             ? "Edit base tables of '" + this.TargetComposition.CompositeContentDomain.Name + "'"
                                                             : "Edit " + (IsComposition ? "Composition" : "Concept")
                                                                       + " - " + ExposingConcept.ToString()),
                                                 false, null, 700);
            if (!AccessOnlyToLocalDetailTablesForDomain && Result.IsTrue() && ExposingSymbol != null)
            {
                ExposingSymbol.RenderElement();

                if (this.CurrentView.Manipulator.WorkingAdorner != null
                    && this.CurrentView.Manipulator.WorkingAdorner.ManipulatedObject == ExposingSymbol)
                    this.CurrentView.Manipulator.WorkingAdorner.Visualize();
            }

            InstanceController.FinishEdit();
        }

        public bool EditSimplePresentationElement(SimplePresentationElement Element, string TitlePrefix)
        {
            var InstanceController = EntityInstanceController.AssignInstanceController(Element);
            InstanceController.StartEdit();

            var EditPanel = Display.CreateEditPanel(Element, null, true, IncludeTechSpecTab :true);

            var TitleTail = Element.ToString();
            var Result = InstanceController.Edit(EditPanel, TitlePrefix + (TitleTail.IsAbsent() ? "" : " - " + TitleTail)).IsTrue();
            return Result;
        }

        /// <summary>
        /// Opens the properties interactive editor of the specified Relationship main symbol via its agent.
        /// Optionally, an initial opened tab key and pointed marker-assignment can be specified.
        /// </summary>
        public void EditRelationshipProperties(VisualSymbol ExposingSymbol, string OpenedTabKey = null,
                                               MarkerAssignment PointedMarkerAssignment = null)
        {
            var Target = ExposingSymbol.OwnerRepresentation.RepresentedIdea as Relationship;
            if (Target == null)
                throw new UsageAnomaly("For 'Edit Relationship' the supplied symbol must represent a Relationship.", ExposingSymbol);

            var ExposingRelationship = ExposingSymbol.OwnerRepresentation.RepresentedIdea as Relationship;
            var DetLisEd = LocalDetailsEditor.CreateLocalDetailsEditor(ExposingRelationship, ExposingSymbol,
                                                                       true, false);

            var InstanceController = EntityInstanceController.AssignInstanceController(Target,
                (current, previous, editpanels) =>
                {
                    var CurrentDetailsEditor = (LocalDetailsEditor)editpanels.First(editpanel => editpanel is LocalDetailsEditor);
                    return CurrentDetailsEditor.UpdateRelatedDetails((Relationship)current, ExposingSymbol);
                });
            InstanceController.StartEdit();

            var MarkingsMaintainer = new MarkingsEditor(ExposingRelationship,
                                                        (() => MarkerAssignmentCommand.CreateMarkerAssignment(Target.EditEngine,
                                                                                                              Target.OwnerComposition.CompositeContentDomain.MarkerDefinitions.FirstOrDefault())),
                                                        ((marker) => MarkerAssignmentCommand.EditMarkerAssignmentDescriptor(this, marker)),
                                                        PointedMarkerAssignment);

            var LinksSubform = ItemsGridMaintainer.CreateItemsGridControl(Target, ExposingRelationship.Links,
                                  null /*A (item) => ProductDirector.ConfirmImmediateApply("Relationship Links Definition", "DomainEdit.RelationshipLinksDefinition",
                                                                                  "ApplyTypingChangesDirectly", "This text-box") */ );
            LinksSubform.VisualControl.CanEditItemsDirectly = false;
            LinksSubform.EditItemOperation = ((rel, roles, link) => link.DoEditDescriptor(lnk => ExposingSymbol.OwnerRepresentation.Render()));

            var SpecTabs = TabbedEditPanel.CreateTab(Display.TABKEY_DETAILS, "Details", "Detailed information.", DetLisEd ).Concatenate(
                           TabbedEditPanel.CreateTab(Display.TABKEY_MARKINGS, "Markers", "Assigned markers.", MarkingsMaintainer),
                           TabbedEditPanel.CreateTab(TABKEY_REL_LINKS, "Links", "Role-based Links of the Relationship", LinksSubform.VisualControl));

            if (ExposingRelationship.IdeaDefinitor.IsVersionable && ExposingRelationship.Version == null)
                ExposingRelationship.Version = new VersionCard();

            var EditPanel = Display.CreateEditPanel(ExposingRelationship, SpecTabs, false, OpenedTabKey, null, true, false, ExposingRelationship.Version != null, true);

            if (ExposingRelationship.IdeaDefinitor.CanGenerateFiles)
                EditPanel.AppendExtraButton("", "Preview file generation...", Display.GetAppImage("fgen_prev.png"),
                                            ins => GenerationManager.ShowGenerationFilePreview(ExposingRelationship));

            var Result = InstanceController.Edit(EditPanel, "Edit Relationship - " + Target.ToString(), false, null, 700);
            if (Result.IsTrue())
            {
                ExposingSymbol.RenderElement();

                if (this.CurrentView.Manipulator.WorkingAdorner != null
                    && this.CurrentView.Manipulator.WorkingAdorner.ManipulatedObject == ExposingSymbol)
                    this.CurrentView.Manipulator.WorkingAdorner.Visualize();
            }

            InstanceController.FinishEdit();
        }

        /*- public bool DeleteSymbolDetail(ContainedDetailRepresentation TargetContainedDetailRep)
        {
            var Result = Display.DialogMessage("Confirmation",
                                               "Are you sure you want to Delete the '" + TargetContainedDetailRep.DetailReference.ContentDesignator.Name + "' detail?\n" +
                                               "NOTE: For only hide the detail you can uncheck the 'Is Displayed' option field.",
                                               EMessageType.Question,
                                               System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxResult.No);

            if (Result != System.Windows.MessageBoxResult.Yes)
                return false;

            var Container = TargetContainedDetailRep.DetailReference.OwnerContainer;

            Container.Details.Remove(TargetContainedDetailRep.DetailReference);

            return true;
        } */

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Converts the supplied Ideas as based on an alternate Idea Definition.
        /// </summary>
        /// <param name="TargetIdeas"></param>
        public void ConvertIdeasToAlternateDefinition(IEnumerable<Idea> TargetIdeas)
        {
            if (TargetIdeas == null || !TargetIdeas.Any())
                return;

            this.StartCommandVariation("Convert Ideas to Alternate Definition");

            var TargetConcepts = TargetIdeas.CastAs<Concept, Idea>();
            if (TargetConcepts.Any())
            {
                var SelectedConceptDefCode = Display.DialogMultiOption("Convert Concepts into...",
                                                                       "Set the new base Concept Definition for the selected Ideas.", null, null, true, null,
                                                                       this.TargetComposition.CompositeContentDomain.ConceptDefinitions.ToArray());
                if (!SelectedConceptDefCode.IsAbsent())
                {
                    var NewConceptDef = this.TargetComposition.CompositeContentDomain.ConceptDefinitions.FirstOrDefault(def => def.TechName == SelectedConceptDefCode);

                    foreach (var Target in TargetConcepts)
                        Target.ApplyConceptDefinitionChange(NewConceptDef);
                }
            }

            var TargetRelationships = TargetIdeas.CastAs<Relationship, Idea>();
            if (TargetRelationships.Any())
            {
                var SelectedRelationshipDefCode = Display.DialogMultiOption("Convert Relationships into...",
                                                              "Set the new base Relationship Definition for the selected Ideas.", null, null, true, null,
                                                              this.TargetComposition.CompositeContentDomain.RelationshipDefinitions.ToArray());
                if (!SelectedRelationshipDefCode.IsAbsent())
                {
                    var NewRelationshipDef = this.TargetComposition.CompositeContentDomain.RelationshipDefinitions.FirstOrDefault(def => def.TechName == SelectedRelationshipDefCode);

                    foreach (var Target in TargetRelationships)
                        Target.ApplyRelationshipDefinitionChange(NewRelationshipDef);
                }
            }

            this.CompleteCommandVariation();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Creates and returns a new Idea Detail, based on the supplied Owner, Target Idea and Existent Designators.
        /// Plus, an indication of only create tables and preselected-kind can be specified.
        /// </summary>
        public ContainedDetail CreateIdeaDetail(Ownership<IdeaDefinition, Idea> Owner, Idea TargetIdea,
                                                IEnumerable<DetailDesignator> ExistentDesignators,
                                                bool OnlyCreateTables = false, string PreselectedKindToCreate = null)
        {
            ContainedDetail Result = null;
            /*- if (!ProductDirector.ConfirmImmediateApply("IdeaEditing.DetailAdd", "ApplyDialogChangesDirectly"))
                return; */

            /*- var ExtraOptions = ExtensionPanel.Create()
                .AddGroupPanel("Definition options...")
                .AddSelectableOption(General.OWNERSHIP_LOCAL, "Local", true, "This detail type will not alter the Idea's Definition, thus making it exclusive for this specific Idea.")
                .AddSelectableOption(General.OWNERSHIP_GLOBAL, "Global", false, "This detail type will be added to the Idea's Definition, therefore being shared across all Ideas of the same kind."); */

            var TableOption = new SimplePresentationElement(TableDetailDesignator.KindTitle, TableDetailDesignator.KindName, TableDetailDesignator.KindSummary, TableDetailDesignator.KindPictogram);
            var DetailToCreate = TableOption.TechName;

            if (PreselectedKindToCreate != null)
                DetailToCreate = PreselectedKindToCreate;
            else
            {
                if (!OnlyCreateTables)
                {
                    var Options = new List<IRecognizableElement>();

                    Options.Add(new SimplePresentationElement(AttachmentDetailDesignator.KindTitle, AttachmentDetailDesignator.KindName, AttachmentDetailDesignator.KindSummary, AttachmentDetailDesignator.KindPictogram));
                    Options.Add(new SimplePresentationElement(LinkDetailDesignator.KindTitle, LinkDetailDesignator.KindName, LinkDetailDesignator.KindSummary, LinkDetailDesignator.KindPictogram));

                    if (ProductDirector.ValidateEditionPermission(AppExec.LIC_EDITION_LITE, "create Table detail", false))
                        Options.Add(TableOption);
                    
                    DetailToCreate = Display.DialogMultiOption("Detail creation", "Select the type of detail to be created...", "",
                                                               null /*- ExtraOptions*/, true, TableDetailDesignator.KindName,
                                                               Options.ToArray());
                }
                //- bool IsExclusive = !ExtraOptions.GetSelectableOption().IsEqual(General.OWNERSHIP_GLOBAL);
            }

            if (DetailToCreate == null)
                Result = null;

            string InitialName = "";

            if (DetailToCreate == TableDetailDesignator.KindName)
            {
                InitialName = Table.__ClassDefinitor.Name + (ExistentDesignators.Count(dsn => dsn is TableDetailDesignator) + 1).ToString() /*- .SubstituteFor("1", "") */ ;
                Result = this.CreateIdeaDetailTable(Owner, TargetIdea, InitialName);
            }
            else
                if (DetailToCreate == AttachmentDetailDesignator.KindName)
                {
                    InitialName = Attachment.__ClassDefinitor.Name + (ExistentDesignators.Count(dsn => dsn is AttachmentDetailDesignator) + 1).ToString() /*- .SubstituteFor("1", "") */ ;
                    Result = this.CreateIdeaDetailAttachment(Owner, TargetIdea, InitialName);
                }
                else
                    if (DetailToCreate == LinkDetailDesignator.KindName)
                    {
                        InitialName = Link.__ClassDefinitor.Name + (ExistentDesignators.Count(dsn => dsn is LinkDetailDesignator) + 1).ToString() /*- .SubstituteFor("1", "") */ ;
                        Result = this.CreateIdeaDetailLink(Owner, TargetIdea, InitialName);
                    }

            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Creates a new detail link for the supplied target Owner, Idea and Initial Name.
        /// Returns the created object or null if cancelled.
        /// </summary>
        public Link CreateIdeaDetailLink(Ownership<IdeaDefinition, Idea> Owner, Idea TargetIdea, string InitialName)
        {
            var InitialLink = new ResourceLink(TargetIdea, new Assignment<DetailDesignator>/*L*/(DomainServices.CreateLinkDesignation(Owner, InitialName), true));

            var EditedLink = DetailLinkEditor.ShowDialog("Select or enter the new Link...", InitialLink.AssignedDesignator, TargetIdea, InitialLink);
            if (EditedLink == null)
                return null;

            EditedLink.UpdateDesignatorIdentification();

            return EditedLink;
        }

        /// <summary>
        /// Edits the specified Link for the supplied Idea, Detail and Custom-Look.
        /// Returns indication of application (true) or cancellation (false), plus the changed Detail.
        /// </summary>
        public Tuple<bool, ContainedDetail> EditIdeaDetailLink(Assignment<DetailDesignator>/*L*/ AssignedLink, Idea TargetIdea,
                                                               Link Detail, LinkAppearance CustomLook)
        {
            var Result = DetailLinkEditor.ShowDialog("Select or enter the Link...", AssignedLink, TargetIdea, Detail);

            bool DetailWasChanged = (Result != null);

            return (Tuple.Create<bool, ContainedDetail>(DetailWasChanged, Result));
        }

        // .........................................................................................
        /// <summary>
        /// Creates a new detail attachment for the supplied Designation-Owner, Idea, Initial Name,
        /// Location and optional Content.
        /// Returns the created object or null if cancelled.
        /// </summary>
        public Attachment CreateIdeaDetailAttachment(Ownership<IdeaDefinition, Idea> DesignationOwner, Idea TargetIdea,
                                                     string InitialName, Uri Location = null, byte[] Content = null, string MimeType = null)
        {
            if (Location == null)
            {
                Location = Display.DialogGetOpenFile("Open Attachment...");

                if (Location == null)
                    return null;
            }

            string NewName = Location.OriginalString.GetSimplifiedResourceName().AbsentDefault(InitialName);

            var Designator = new Assignment<DetailDesignator>(DomainServices.CreateAttachmentDesignation(DesignationOwner, NewName), true);

            var Route = (Location.IsAbsoluteUri && Location.IsFile ? Location.LocalPath : Location.OriginalString);
            var NewAttachment = new Attachment(TargetIdea, Designator, Route, MimeType);

            if (Content == null && Location.IsAbsoluteUri && Location.IsFile)
            {
                try
                {
                    NewAttachment.Content = General.FileToBytes(Location.LocalPath);
                }
                catch (Exception Problem)
                {
                    Console.WriteLine("Cannot attach file. Problem: " + Problem.Message);
                }
            }
            else
                NewAttachment.Content = Content;

            return NewAttachment;
        }

        /// <summary>
        /// Edits the specified Attachment for the supplied Idea and Detail and Custom-Look.
        /// Returns indication of application (true) or cancellation (false), plus the changed Detail.
        /// </summary>
        public Tuple<bool, ContainedDetail> EditIdeaDetailAttachment(Assignment<DetailDesignator> AssignedAttachment, Idea TargetIdea,
                                                                     Attachment Detail, AttachmentAppearance CustomLook = null)
        {
            bool Changed = false;

            try
            {
                Uri Location = Display.DialogGetOpenFile("Open Attachment...");

                if (Location != null)
                {
                    Detail = new Attachment(TargetIdea, AssignedAttachment, Location.LocalPath);
                    Detail.Content = General.FileToBytes(Location.LocalPath);
                    Changed = true;
                }
            }
            catch (Exception Problem)
            {
                Console.WriteLine("Cannot open attachment.\nProblem: " + Problem.Message);
                Changed = false;
            }

            return (Tuple.Create<bool, ContainedDetail>(Changed, Detail));
        }

        /// <summary>
        /// Invokes the external editing of the specified Attachment for the supplied Idea, Detail and Custom-Look.
        /// Returns indication of application (true) or cancellation (false), plus the changed Detail.
        /// </summary>
        public Tuple<bool, ContainedDetail> ExternalEditDetailAttachment(Assignment<DetailDesignator> AssignedAttachment, Idea TargetIdea,
                                                                         Attachment Detail, AttachmentAppearance CustomLook)
        {
            bool Changed = false;

            if (Detail == null)
            {
                var Result = EditIdeaDetailAttachment(AssignedAttachment, TargetIdea, Detail, CustomLook);
                if (Result.Item1)
                    Detail = (Attachment)Result.Item2;
            }

            if (Detail != null)
            {
                Display.PushCursor(Cursors.Wait);
                DetailAttachmentEditor.ExposeAttachmentAsTemporalFile(Detail);
                Display.PopCursor();
                Changed = true;
            }

            return (Tuple.Create<bool, ContainedDetail>(Changed, Detail));
        }

        // .........................................................................................
        /// <summary>
        /// Creates a new detail table for the supplied Owner, Idea and Initial Name.
        /// Returns the created object or null if cancelled.
        /// </summary>
        public Table CreateIdeaDetailTable(Ownership<IdeaDefinition, Idea> Owner, Idea TargetIdea, string InitialName)
        {
            var Result = TableDetailDesignator.KindName;
            bool IsExclusive = !Owner.IsGlobal;

            // .....................................................................................
            var Designation = DomainServices.CreateTableDesignation(TargetIdea.EditEngine, Owner, InitialName);
            if (Designation == null)
                return null;

            var Designator = new Assignment<DetailDesignator>(Designation, !Owner.IsGlobal);

            if (Owner.IsGlobal)
            {
                if (TargetIdea == Owner.OwnerGlobal.OwnerDomain.BaseContentRoot
                    && !ProductDirector.ValidateEditionPermission(AppExec.LIC_EDITION_PROFESSIONAL, "define Base Tables"))
                return null;

                TargetIdea.IdeaDefinitor.DetailDesignators.Add(Designator.Value);
            }

            var EditResult = DomainServices.EditDetailDesignator(Designator.Value, false, this, true);
            if (!EditResult.Item1.IsTrue())
                return null;

            Table DetailTable = null;

            if (EditResult.Item2 != null)
                DetailTable = Table.CreateTableFrom(Designator, EditResult.Item2,
                                                    Owner.GetValue(ideadef => ideadef.OwnerDomain.BaseContentRoot, idea => idea));

            // Register the new Table-Definition in its related Domain
            var RelatedDomain = Owner.GetValue<Domain>(ideadef => ideadef.OwnerDomain, idea => idea.IdeaDefinitor.OwnerDomain);
            RelatedDomain.TableDefinitions.AddNew(Designation.DeclaringTableDefinition);

            // Edit data content
            var EditingResult = EditIdeaDetailTable(Designator, TargetIdea, DetailTable, null);
            if (EditingResult.Item1)
                return EditingResult.Item2 as Table;

            return null;
        }

        /// <summary>
        /// Edits the specified Table for the supplied Idea, Detail and Custom-Look.
        /// Returns indication of application (true) or cancellation (false), plus the target Detail.
        /// </summary>
        public Tuple<bool, ContainedDetail> EditIdeaDetailTable(Assignment<DetailDesignator> Designation, Idea TargetIdea,
                                                                Table Detail, TableAppearance CustomLook)
        {
            //- TargetIdea.EditEngine.StartCommandVariation("Edit table detail");

            if (Detail == null)
                Detail = new Table(TargetIdea, Designation);

            var TableDesignator = Designation.Value as TableDetailDesignator;
            var ChangesApplied = DetailTableEditor.Edit(Detail, TableDesignator, CustomLook.NullDefault(TableDesignator.TableLook));
            //- TargetIdea.EditEngine.CompleteCommandVariation();

            //- if (!ChangesApplied)
            //-     TargetIdea.EditEngine.Undo();

            return (Tuple.Create<bool, ContainedDetail>(ChangesApplied, Detail));
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Edits an existent detail link for the supplied Designator, target Idea, Detail and Custom-Look.
        /// Returns indication of application (true) or cancellation (false), plus the changed Detail.
        /// </summary>
        public Tuple<bool, ContainedDetail> EditIdeaDetail(Assignment<DetailDesignator> Designator, Idea TargetIdea,
                                                           ContainedDetail Detail, DetailAppearance CustomLook = null)
        {
            General.ContractRequiresNotNull(Designator, TargetIdea);

            // The supplied Designator must be the same of a non-null Detail.
            General.ContractRequires(Detail == null || Detail.ContentDesignator == Designator);

            Tuple<bool, ContainedDetail> Result = Tuple.Create<bool, ContainedDetail>(false, null);

            // NOTE: Undoable/redoable command variation are inside each EditDetail{DetailKind} operation, when needed.

            if (Designator.Value is LinkDetailDesignator)
                Result = EditIdeaDetailLink(Designator, TargetIdea, Detail as Link, CustomLook as LinkAppearance);
            else
                if (Designator.Value is AttachmentDetailDesignator)
                    Result = EditIdeaDetailAttachment(Designator, TargetIdea, Detail as Attachment, CustomLook as AttachmentAppearance);
                else
                    if (Designator.Value is TableDetailDesignator)
                        Result = EditIdeaDetailTable(Designator, TargetIdea, Detail as Table, CustomLook as TableAppearance);
                    else
                        Console.WriteLine("Unknown Designator type: {0}", Designator.Value.GetType().Name);

            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Goes to the referenced object, related to the supplied target Idea.
        /// </summary>
        public void GoToLink(Link Reference, Idea TargetIdea)
        {
            if (Reference is ResourceLink)
            {
                var ResLink = Reference as ResourceLink;
                AppExec.CallExternalProcess(ResLink.TargetLocation);
                return;
            }

            var IntLink = Reference as InternalLink;
            if (IntLink == null)
                Console.WriteLine("There is no linked object.");
            else
                DetailInternalPropertyEditor.Edit(this, IntLink.TargetProperty, TargetIdea);
        }

        /// <summary>
        /// Goes to the referenced internal property, related to the supplied target Idea, declared in the specified Link Designator.
        /// </summary>
        public void GoToInternalLink(LinkDetailDesignator LinkDesignator, MModelPropertyDefinitor PropertyDef, Idea TargetIdea)
        {
            if (PropertyDef == null)
            {
                Console.WriteLine("Cannot navigate to default link target.");
                return;
            }

            DetailInternalPropertyEditor.Edit(this, PropertyDef, TargetIdea);
        }

        /// <summary>
        /// Exports the specified Attachment from the supplied target Idea.
        /// </summary>
        public void ExportAttachment(Attachment Annex, Idea TargetIdea)
        {
            General.ContractRequiresNotNull(Annex, TargetIdea);

            if (Annex.Content == null)
            {
                Console.WriteLine("Attachment content is empty.");
                return;
            }

            var FileName = Path.GetFileName(Annex.Source);
            var Extension = Path.GetExtension(Annex.Source);

            var Location = Display.DialogGetSaveFile("Export attachment to...", Extension, null, FileName);
            if (Location == null)
                return;

            try
            {
                General.BytesToFile(Location.LocalPath, Annex.Content);
            }
            catch (Exception Problem)
            {
                Display.DialogMessage("Error!", "Cannot export attachment.\nProblem: " + Problem.Message, EMessageType.Warning);
                return;
            }

            Console.WriteLine("Data was exported successfully!");
        }

        /// <summary>
        /// Opens the properties interactive editor of the specified View via its agent.
        /// </summary>
        public static bool EditViewProperties(View Target)
        {
            var InstanceController = EntityInstanceController.AssignInstanceController(Target);
            InstanceController.StartEdit();

            var PrevGridSize = Target.GridSize;
            var PrevBackBrush = Target.BackgroundBrush;
            var PrevBackImage = Target.BackgroundImage;

            var Extras = new List<UIElement>();

            var Expositor = new EntityPropertyExpositor(View.__GridSize.TechName);
            Expositor.LabelMinWidth = 100;
            Extras.Add(Expositor);

            Expositor = new EntityPropertyExpositor(View.__BackgroundBrush.TechName);
            Expositor.LabelMinWidth = 100;
            Extras.Add(Expositor);

            Expositor = new EntityPropertyExpositor(View.__BackgroundImage.TechName);
            Expositor.LabelMinWidth = 100;
            Extras.Add(Expositor);

            var EditPanel = Display.CreateEditPanel(Target, null, true, null, null, true, false, false, true, Extras.ToArray());

            var Result = InstanceController.Edit(EditPanel, "Edit View - " + Target.ToString(), false,
                                                 null, 500, 450).IsTrue();
            if (Result && (PrevGridSize != Target.GridSize ||
                           PrevBackBrush != Target.BackgroundBrush ||
                           PrevBackImage != Target.BackgroundImage))
                Target.ShowAll();

            InstanceController.FinishEdit();

            return Result;
        }

        // ----------------------------------------------------------------------------------------------------------------------------------------
        public const string DELETE_SELECTION_IDEAS = "I";
        public const string DELETE_SELECTION_VISUALREPS = "V";

        /* POSTPONED: LET THE USER DECIDE BETWEEN SEMANTIC OR REPRESENTATION DELETION IF THERE CAN BE MORE THAT ONE VISUAL-REP PER IDEA.
        /// <summary>
        /// Delete the selected representations
        /// </summary>
        public void DeleteSelection(View TargetView)
        {
            var WhatToDelete = DELETE_SELECTION_IDEAS;
            var SelectedNumber = TargetView.SelectedObjects.Count;
              
            Display.DialogMultiOption("Confirmation", "Please indicate what to Delete of the " + SelectedNumber.ToString() + " selected objects.", "",
                                                    null, DELETE_SELECTION_IDEAS,
                                                    new SimplePresentationElement("Ideas and Visual Representations.", DELETE_SELECTION_IDEAS,
                                                                                "Selected semantic Ideas, plus their content and visual represenatations will be deleted.",
                                                                                Display.GetImage("lightbulb_delete.png")),
                                                    new SimplePresentationElement("Only Visual Representations.", DELETE_SELECTION_VISUALREPS,
                                                                                "Selected visual representations will be deleted. Related semantic Ideas and their content will remain.",
                                                                                Display.GetImage("eye_delete.png")));
        if (WhatToDelete == null)
            return;

        if (WhatToDelete == DELETE_SELECTION_IDEAS)
            DeleteIdeas(TargetView.SelectedObjects);
        else
            DeleteRepresentations(TargetView.SelectedObjects);
        } */

        /* POSTPONED:
        /// <summary>
        /// Deletes the specified Representations (visual).
        /// </summary>
        public void DeleteRepresentations(IEnumerable<VisualRepresentation> TargetRepresentations)
        {
            this.CurrentView.Manipulator.UnpointElement();  // Important to avoid crash by pointing to null.
          
            this.StartCommandVariation("Delete Selected Representations");

            var Targets = TargetRepresentations.ToList();
            foreach (var Target in Targets)
                Target.DisplayingView.DeleteRepresentation(Target);

            this.CompleteCommandVariation();
        } */

        /// <summary>
        /// Deletes the specified objetcs, either as only Visual Representation (when shortcut) or as Idea (semantic).
        /// </summary>
        public void DeleteObjects(IEnumerable<VisualObject> TargetObjects, View TargetView = null)
        {
            if (this.CurrentView == TargetView)
                this.CurrentView.Manipulator.UnpointObject();  // Important to avoid crash by pointing to null.

            this.StartCommandVariation("Delete Objects");

            // PENDING: Confirmation when composites exists, plus clear/refresh of selection indicators layer.

            var Targets = TargetObjects.ToList();

            var Complements = Targets.CastAs<VisualComplement, VisualObject>();
            var Connectors = Targets.CastAs<VisualConnector, VisualObject>();
            var Representations = Targets.Where(trg => trg is VisualSymbol)
                                    .Select(trg => ((VisualSymbol)trg).OwnerRepresentation)
                                        .Distinct();

            foreach (var TargetComplement in Complements)
                DeleteComplement(TargetComplement);

            foreach (var TargetConnector in Connectors)
                DeleteRelationshipLink(TargetConnector.RepresentedLink);

            var AffectedRelReps = Connectors.Where(conn => conn.RepresentedLink.OwnerRelationship.Links.Count < 1)
                                            .Select(conn => conn.OwnerRelationshipRepresentation).Distinct();
            Representations = Representations.Concat(AffectedRelReps).Distinct();

            var AffectedViews = Representations.Where(rep => !rep.IsShortcut)
                                       .SelectMany(rep => rep.RepresentedIdea.GetNestedCompositeIdeas(true)
                                                            .SelectMany(idea => idea.CompositeViews)).ToList();
            var ShownViews = ProductDirector.DocumentVisualizerControl.GetAllViews(this.TargetComposition);
            var AffectedOpenViews = AffectedViews.Intersect(ShownViews).ToList();

            foreach (var AffectedView in AffectedOpenViews)
                ProductDirector.DocumentVisualizerControl.DiscardView(AffectedView.GlobalId);

            foreach (var TargetRepresentation in Representations)
                if (TargetRepresentation.IsShortcut)
                    this.DeleteShortcut(TargetRepresentation);
                else
                    TargetRepresentation.RepresentedIdea.RemoveFromComposite();

            if (this.CurrentView == TargetView)
                ProductDirector.EditorInterrelationsControl.SetTarget(this.CurrentView.SelectedObjects.FirstOrDefault());

            foreach(var Target in Targets)
                this.CurrentView.SelectedObjects.Remove(Target);

            this.CompleteCommandVariation();
        }

        /// <summary>
        /// Deletes the specified Complement
        /// </summary>
        public void DeleteComplement(VisualComplement TargetComplement)
        {
            if (!TargetComplement.Target.IsGlobal)
                TargetComplement.Target.OwnerLocal.RemoveComplement(TargetComplement);

            TargetComplement.GetDisplayingView().UpdateVersion();
            TargetComplement.GetDisplayingView().Clear(TargetComplement);
        }

        /// <summary>
        /// Deletes role-based Link and propagate changes.
        /// </summary>
        public void DeleteRelationshipLink(RoleBasedLink RoleLink)
        {
            this.StartCommandVariation("Delete Relationship Link");

            RoleLink.OwnerRelationship.DeleteRelationshipAssociation(RoleLink.AssociatedIdea);

            /*-
            foreach (var Representator in RoleLink.OwnerRelationship.VisualRepresentators)
            {
                // Determine visual connectors representating associating Links
                var Connectors = Representator.VisualParts.CastAs<VisualConnector, VisualElement>()
                                            .Where(conn => conn.RepresentedLink.IsEqual(RoleLink))
                                                .ToList();

                // Remove visual connectors
                foreach (var Connector in Connectors)
                {
                    Connector.ClearElement();
                    Connector.Disconnect();
                    Representator.VisualParts.Remove(Connector);
                }
            }

            // Remove semantic Link
            RoleLink.OwnerRelationship.Links.Remove(RoleLink);

            // Update visuals
            RoleLink.AssociatedIdea.UpdateVisualRepresentators();
            Container.UpdateVisualRepresentators();
            */

            RoleLink.UpdateVersion();

            this.CompleteCommandVariation();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Deletes the specified shortcut Representation, preserving the represented semantic Idea.
        /// </summary>
        /// <param name="Target"></param>
        protected void DeleteShortcut(VisualRepresentation Representation)
        {
            if (!Representation.IsShortcut)
                throw new UsageAnomaly("Cannot delete non-shortcut visual representation.");

            var AssociatedConnectors = Representation.MainSymbol.TargetConnections
                                        .Concat(Representation.MainSymbol.OriginConnections).ToList();

            foreach (var Connector in AssociatedConnectors)
                Connector.OwnerRelationshipRepresentation.RepresentedRelationship.DeleteRelationshipAssociation(Representation.RepresentedIdea);
                //- this.DeleteRelationshipLink(Connector.OwnerRelationshipRepresentation.RepresentedRelationship, Connector.RepresentedLink);

            Representation.DisplayingView.UpdateVersion();
            Representation.Clear();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public void RemoveCompositeView(View Target)
        {
            if (Target.OwnerCompositeContainer == Target.OwnerCompositeContainer.OwnerComposition)
                return;

            /* Cancelled: This would requiered to confirm all delete operations, but there exists Undo in case of user mistake.
            if ((Target.ViewChildren.Count - 1) > 0
                && Display.DialogMessage("Confirmation", "You are about to Remove the '" + Target.Name + "' composite View.\n\n" +
                                         "(Note: There are still " + (Target.ViewChildren.Count - 1).ToString() + " objects on it)",
                                         EMessageType.Warning, MessageBoxButton.OKCancel, MessageBoxResult.Cancel) != MessageBoxResult.OK)
                return; */

            this.StartCommandVariation("Remove composite View");

            Target.SelectMultipleObjects();
            this.DeleteObjects(Target.SelectedObjects, Target);

            ProductDirector.DocumentVisualizerControl.DiscardView(Target.GlobalId);

            Target.OwnerCompositeContainer.CompositeViews.Remove(Target);
            Target.OwnerCompositeContainer.CompositeActiveView = Target.OwnerCompositeContainer.CompositeViews.FirstOrDefault();

            Target.OwnerCompositeContainer.UpdateVisualRepresentators();

            this.CompleteCommandVariation();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public void ShowCompositeAsView(VisualSymbol SourceSymbol)
        {
            this.StartCommandVariation("Show Composite-Content as View");

            SourceSymbol.OwnerRepresentation.RepresentedIdea.OpenCompositeView();
            SourceSymbol.RenderElement();

            this.CompleteCommandVariation();
        }

        public void ShowCompositeAsView(Idea SourceIdea)
        {
            this.StartCommandVariation("Open Composite-Content as View");

            SourceIdea.OpenCompositeView();

            this.CompleteCommandVariation();
        }
        
        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public void GoToShortcutTarget(VisualSymbol SourceShortcutSymbol)
        {
            var TargetRepresentation = SourceShortcutSymbol.OwnerRepresentation.RepresentedIdea.VisualRepresentators.FirstOrDefault(vrep => !vrep.IsShortcut)
                                          .NullDefault(SourceShortcutSymbol.OwnerRepresentation.RepresentedIdea.VisualRepresentators.FirstOrDefault());

            if (TargetRepresentation == null)
            {
                Console.WriteLine("No shortcut target found.");
                return;
            }

            this.StartCommandVariation("Go to Shortcut Target");

            TargetRepresentation.RepresentedIdea.OwnerComposition.Engine.ShowView(TargetRepresentation.DisplayingView);
            TargetRepresentation.DisplayingView.Presenter.BringIntoView(TargetRepresentation.MainSymbol.BaseArea);
            TargetRepresentation.DisplayingView.Presenter.PostCall(
                vpres => vpres.OwnerView.Manipulator.PointObject(TargetRepresentation.MainSymbol));

            this.CompleteCommandVariation();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}