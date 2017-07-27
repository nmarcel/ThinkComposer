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
// File   : CompositionEngine.Interaction.cs
// Object : Instrumind.ThinkComposer.Composer.CompositionEngine (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.07.11 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
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
    /// Takes care of the edition of a particular Composition instance (Interaction partial-file).
    /// </summary>
    public partial class CompositionEngine : DocumentEngine
    {
        static List<Tuple<SimplePresentationElement, Func<View, FrameworkElement, bool?>, Action<View>>> ContextMenuOptionsForViews
            = new List<Tuple<SimplePresentationElement, Func<View, FrameworkElement, bool?>, Action<View>>>();

        static List<Tuple<SimplePresentationElement, Func<VisualSymbol, FrameworkElement, bool?>, Action<VisualSymbol>>> ContextMenuOptionsForVisualSymbols
            = new List<Tuple<SimplePresentationElement, Func<VisualSymbol, FrameworkElement, bool?>, Action<VisualSymbol>>>();

        static List<Tuple<SimplePresentationElement, Func<VisualConnector, FrameworkElement, bool?>, Action<VisualConnector>>> ContextMenuOptionsForVisualConnectors
            = new List<Tuple<SimplePresentationElement, Func<VisualConnector, FrameworkElement, bool?>, Action<VisualConnector>>>();

        static List<Tuple<SimplePresentationElement, Func<VisualComplement, FrameworkElement, bool?>, Action<VisualComplement, SimplePresentationElement>, List<SimplePresentationElement>>> ContextMenuOptionsForVisualComplements
            = new List<Tuple<SimplePresentationElement, Func<VisualComplement, FrameworkElement, bool?>, Action<VisualComplement, SimplePresentationElement>, List<SimplePresentationElement>>>();

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        private static void DefineContextMenuOptions()
        {
            // Views....................................................................................................
            ContextMenuOptionsForViews.Add(new Tuple<SimplePresentationElement, Func<View, FrameworkElement, bool?>, Action<View>>
                            (new SimplePresentationElement("Go to Parent", "GoToParent", "Goes back to the parent Composite-Content View.", Display.GetAppImage("page_view_back.png")),
                             (target, vexpo) => target.OwnerCompositeContainer.OwnerContainer != null,
                             (target) => target.Engine.ShowCompositeAsView(target.OwnerCompositeContainer.OwnerContainer)));

            ContextMenuOptionsForViews.Add(new Tuple<SimplePresentationElement, Func<View, FrameworkElement, bool?>, Action<View>>
                            (new SimplePresentationElement("Go to View", "GoToView", "Goes to the pointed Composite-Content View.", Display.GetAppImage("page_view.png")),
                             (target, vexpo) => (target == target.Engine.CurrentView ? (bool?)null : true),
                             (target) => target.Engine.ShowView(target)));

            ContextMenuOptionsForViews.Add(null);   // Separator

            ContextMenuOptionsForViews.Add(new Tuple<SimplePresentationElement, Func<View, FrameworkElement, bool?>, Action<View>>
                            (new SimplePresentationElement("Paste", "Paste", "Paste Clipboard content.", Display.GetAppImage("paste_plain.png")),
                             (target, vexpo) => (vexpo != target.Presenter) ? (bool?)null : target.Engine.ClipboardHasPasteableContent(),
                             (target) => target.Engine.ClipboardPaste(target, false, CurrentMousePosition)));

            ContextMenuOptionsForViews.Add(new Tuple<SimplePresentationElement, Func<View, FrameworkElement, bool?>, Action<View>>
                            (new SimplePresentationElement("Paste Shortcut", "PasteShortcut", "Paste copied Ideas as Shortcut.", Display.GetAppImage("paste_shortcut.png")),
                             (target, vexpo) => (vexpo != target.Presenter) ? (bool?)null : General.Execute(() => Clipboard.ContainsData(CompositionEngine.IdeaTransferFormat.Name),
                                                                                                            "Cannot access Windows Clipboard!").Result,
                             (target) => target.Engine.ClipboardPaste(target, true)));

            ContextMenuOptionsForViews.Add(new Tuple<SimplePresentationElement, Func<View, FrameworkElement, bool?>, Action<View>>
                            (new SimplePresentationElement("Select All", "SelectAll", "Sellect all visual objects of the View.", Display.GetAppImage("view_select_all.png")),
                             (target, vexpo) => (vexpo != target.Presenter) ? (bool?)null : true,
                             (target) => target.SelectMultipleObjects()));

            ContextMenuOptionsForViews.Add(null);   // Separator

            ContextMenuOptionsForViews.Add(new Tuple<SimplePresentationElement, Func<View, FrameworkElement, bool?>, Action<View>>
                            (new SimplePresentationElement("Remove", "RemoveView", "Removes the entire View.", Display.GetAppImage("delete.png")),
                             (target, vexpo) => (!ProductDirector.DocumentVisualizerControl.GetAllViews(target.Engine.TargetComposition).CountsAtLeast(2)
                                                 ? (bool?)null : target.OwnerCompositeContainer != target.OwnerCompositeContainer.OwnerComposition),
                             (target) => target.Engine.RemoveCompositeView(target)));

            ContextMenuOptionsForViews.Add(new Tuple<SimplePresentationElement, Func<View, FrameworkElement, bool?>, Action<View>>
                            (new SimplePresentationElement("View Properties", "ViewProperties", "Edits the properties of the View.", Display.GetAppImage("page_white_edit.png")),
                             (target, vexpo) => true,
                             (target) => CompositionEngine.EditViewProperties(target)));

            // Symbols...................................................................................................
            ContextMenuOptionsForVisualSymbols.Add(new Tuple<SimplePresentationElement, Func<VisualSymbol, FrameworkElement, bool?>, Action<VisualSymbol>>
                            (new SimplePresentationElement("Show/Hide Details", "ShowHideDetails", "Open/Close the symbol Details poster.", Display.GetAppImage("detail_poster.png")),
                             (target, vexpo) => (vexpo != target.GetDisplayingView().Presenter) ? (bool?)null : true,
                             (target) => { var Eng = target.GetDisplayingView().Engine; Eng.CurrentView.SelectObject(target); Eng.Manager.CommandSwitchDetails_Execution(null); }));

            ContextMenuOptionsForVisualSymbols.Add(new Tuple<SimplePresentationElement, Func<VisualSymbol, FrameworkElement, bool?>, Action<VisualSymbol>>
                            (new SimplePresentationElement("Convert", "Convert", "Converts the Idea, as based on other Idea Definition type.", Display.GetAppImage("wand_convert.png")),
                             (target, vexpo) => true,
                             (target) => { var Eng = target.GetDisplayingView().Engine; Eng.CurrentView.SelectObject(target); Eng.ConvertIdeasToAlternateDefinition(Eng.CurrentView.SelectedRepresentations.Select(vrep => vrep.RepresentedIdea)); }));

            ContextMenuOptionsForVisualSymbols.Add(null);   // Separator
            
            ContextMenuOptionsForVisualSymbols.Add(new Tuple<SimplePresentationElement, Func<VisualSymbol, FrameworkElement, bool?>, Action<VisualSymbol>>
                            (new SimplePresentationElement("Expand/Collapse Related Origin Ideas", "ExpandCollapseRelatedOriginIdeas", "Expand/Collapse the Symbol supertree of Related Originating Ideas.", Display.GetAppImage("rel_sources.png")),
                             (target, vexpo) => ((vexpo != target.GetDisplayingView().Presenter) ? (bool?)null : target.OwnerRepresentation.OriginRepresentations.Any()),
                             (target) => target.GetDisplayingView().Manipulator.SwitchRelated(target, false)));

            ContextMenuOptionsForVisualSymbols.Add(new Tuple<SimplePresentationElement, Func<VisualSymbol, FrameworkElement, bool?>, Action<VisualSymbol>>
                            (new SimplePresentationElement("Expand/Collapse Related Target Ideas", "ExpandCollapseRelatedTargetIdeas", "Expand/Collapse the Symbol subtree of Related Targeted Ideas.", Display.GetAppImage("rel_targets.png")),
                             (target, vexpo) => ((vexpo != target.GetDisplayingView().Presenter) ? (bool?)null : target.OwnerRepresentation.TargetRepresentations.Any()),
                             (target) => target.GetDisplayingView().Manipulator.SwitchRelated(target, true)));

            //- ContextMenuOptionsForVisualSymbols.Add(null);   // Separator

            ContextMenuOptionsForVisualSymbols.Add(new Tuple<SimplePresentationElement, Func<VisualSymbol, FrameworkElement, bool?>, Action<VisualSymbol>>
                            (new SimplePresentationElement("Show Composite-Content [F3]", "ShowCompositeContent", "Show the Idea Composite-Content as View.", Display.GetAppImage("show_composite.png")),
                             (target, vexpo) => (vexpo != target.GetDisplayingView().Presenter) ? (bool?)null : target.OwnerRepresentation.RepresentedIdea.IdeaDefinitor.IsComposable || target.OwnerRepresentation.RepresentedIdea.CompositeIdeas.Count > 0,
                             (target) => target.GetDisplayingView().Engine.ShowCompositeAsView(target)));

            ContextMenuOptionsForVisualSymbols.Add(new Tuple<SimplePresentationElement, Func<VisualSymbol, FrameworkElement, bool?>, Action<VisualSymbol>>
                            (new SimplePresentationElement("Display/Hide Composite-Content as Detail", "ShowCompositeContentAsDetail", "Display/Hide the Composite-Content View instead of Details.", Display.GetAppImage("composite_view.png")),
                             (target, vexpo) => ((vexpo != target.GetDisplayingView().Presenter) ? (bool?)null : target.OwnerRepresentation.RepresentedIdea.CompositeViews.Count > 0),
                             (target) => target.GetDisplayingView().Manipulator.ShowCompositeAsDetail(target)));

            ContextMenuOptionsForVisualSymbols.Add(null);   // Separator

            ContextMenuOptionsForVisualSymbols.Add(new Tuple<SimplePresentationElement, Func<VisualSymbol, FrameworkElement, bool?>, Action<VisualSymbol>>
                            (new SimplePresentationElement("Add Attachment Detail", "AddAttachmentDetail", "Add an Attachment Detail to the Idea.", AttachmentDetailDesignator.KindPictogram),
                             (target, vexpo) => true,
                             (target) => target.GetDisplayingView().AppendDetailToVisualRepresentation(target.OwnerRepresentation, AttachmentDetailDesignator.KindName)));

            ContextMenuOptionsForVisualSymbols.Add(new Tuple<SimplePresentationElement, Func<VisualSymbol, FrameworkElement, bool?>, Action<VisualSymbol>>
                            (new SimplePresentationElement("Add Link Detail", "AddLinkDetail", "Add a Link Detail to the Idea.", LinkDetailDesignator.KindPictogram),
                             (target, vexpo) => true,
                             (target) => target.GetDisplayingView().AppendDetailToVisualRepresentation(target.OwnerRepresentation, LinkDetailDesignator.KindName)));

            ContextMenuOptionsForVisualSymbols.Add(new Tuple<SimplePresentationElement, Func<VisualSymbol, FrameworkElement, bool?>, Action<VisualSymbol>>
                            (new SimplePresentationElement("Add Table Detail", "AddTableDetail", "Add a Table Detail to the Idea.", TableDetailDesignator.KindPictogram),
                             (target, vexpo) => true,
                             (target) => target.GetDisplayingView().AppendDetailToVisualRepresentation(target.OwnerRepresentation, TableDetailDesignator.KindName)));

            ContextMenuOptionsForVisualSymbols.Add(new Tuple<SimplePresentationElement, Func<VisualSymbol, FrameworkElement, bool?>, Action<VisualSymbol>>
                            (new SimplePresentationElement("Edit Details [F6]", "EditDetails", "Opens the tab for edit the details of this Idea", Display.GetAppImage("detail_edit.png")),
                             (target, vexpo) => true,
                             (target) => target.GetDisplayingView().EditPropertiesOfVisualRepresentation(target.OwnerRepresentation, Display.TABKEY_DETAILS)));

            ContextMenuOptionsForVisualSymbols.Add(null);   // Separator

            ContextMenuOptionsForVisualSymbols.Add(new Tuple<SimplePresentationElement, Func<VisualSymbol, FrameworkElement, bool?>, Action<VisualSymbol>>
                            (new SimplePresentationElement("Delete", "Delete", "Delete the selected objects.", Display.GetAppImage("lightbulb_delete.png")),
                             (target, vexpo) => true,
                             (target) => { var Eng = target.GetDisplayingView().Engine; Eng.CurrentView.SelectObject(target); Eng.DeleteObjects(Eng.CurrentView.SelectedObjects); }));

            ContextMenuOptionsForVisualSymbols.Add(new Tuple<SimplePresentationElement, Func<VisualSymbol, FrameworkElement, bool?>, Action<VisualSymbol>>
                            (new SimplePresentationElement("Cut", "Cut", "Cut the selected objects and let a copy in the Clipboard.", Display.GetAppImage("cut.png")),
                             (target, vexpo) => true,
                             (target) => { var Eng = target.GetDisplayingView().Engine; Eng.CurrentView.SelectObject(target); Eng.ClipboardCut(Eng.CurrentView); }));

            ContextMenuOptionsForVisualSymbols.Add(new Tuple<SimplePresentationElement, Func<VisualSymbol, FrameworkElement, bool?>, Action<VisualSymbol>>
                            (new SimplePresentationElement("Copy", "Copy", "Copy the selected objects to the Clipboard.", Display.GetAppImage("page_white_copy.png")),
                             (target, vexpo) => true,
                             (target) => { var Eng = target.GetDisplayingView().Engine; Eng.CurrentView.SelectObject(target); Eng.ClipboardCopy(Eng.CurrentView); }));

            ContextMenuOptionsForVisualSymbols.Add(new Tuple<SimplePresentationElement, Func<VisualSymbol, FrameworkElement, bool?>, Action<VisualSymbol>>
                            (new SimplePresentationElement("Paste", "Paste", "Paste Clipboard content into current View.", Display.GetAppImage("paste_plain.png")),
                             (target, vexpo) => target.GetDisplayingView().Engine.ClipboardHasPasteableContent(),
                             (target) => { var Eng = target.GetDisplayingView().Engine; Eng.CurrentView.SelectObject(target); Eng.ClipboardPaste(Eng.CurrentView); }));

            ContextMenuOptionsForVisualSymbols.Add(null);   // Separator

            ContextMenuOptionsForVisualSymbols.Add(new Tuple<SimplePresentationElement, Func<VisualSymbol, FrameworkElement, bool?>, Action<VisualSymbol>>
                            (new SimplePresentationElement("Edit Markers [F7]", "EditMarkers", "Opens the tab for edit the markers of this Idea", Display.GetAppImage("award_star_gold_1.png")),
                             (target, vexpo) => true,
                             (target) => target.GetDisplayingView().EditPropertiesOfVisualRepresentation(target.OwnerRepresentation, Display.TABKEY_MARKINGS)));

            ContextMenuOptionsForVisualSymbols.Add(new Tuple<SimplePresentationElement, Func<VisualSymbol, FrameworkElement, bool?>, Action<VisualSymbol>>
                            (new SimplePresentationElement("Properties [F4]", "IdeaProperties", "Edit the Properties of this Idea", Display.GetAppImage("page_white_edit.png")),
                             (target, vexpo) => true,
                             (target) => target.GetDisplayingView().EditPropertiesOfVisualRepresentation(target.OwnerRepresentation)));

            // Connectors................................................................................................
            ContextMenuOptionsForVisualConnectors.Add(new Tuple<SimplePresentationElement, Func<VisualConnector, FrameworkElement, bool?>, Action<VisualConnector>>
                            (new SimplePresentationElement("Link Descriptor", "LinkDescriptor", "Edit the Descriptor of this Link", Display.GetAppImage("page_white_edit.png")),
                             (target, vexpo) => true,
                             (target) => target.DoEditDescriptor()));
            ContextMenuOptionsForVisualConnectors.Add(new Tuple<SimplePresentationElement, Func<VisualConnector, FrameworkElement, bool?>, Action<VisualConnector>>
                            (new SimplePresentationElement("Relationship Properties", "RelationshipProperties", "Edit the Descriptor of this Relationship", Display.GetAppImage("page_edit.png")),
                             (target, vexpo) => true,
                             (target) => target.GetDisplayingView().EditPropertiesOfVisualRepresentation(target.OwnerRepresentation)));

            // Complements...............................................................................................
            ContextMenuOptionsForVisualComplements.Add(new Tuple<SimplePresentationElement, Func<VisualComplement, FrameworkElement, bool?>, Action<VisualComplement, SimplePresentationElement>, List<SimplePresentationElement>>
                            (new SimplePresentationElement("Delete", "Delete", "Delete the selected objects.", Display.GetAppImage("lightbulb_delete.png")),
                             (target, vexpo) => true,
                             (target, selection) => { var Eng = target.GetDisplayingView().Engine; Eng.CurrentView.SelectObject(target); Eng.DeleteObjects(Eng.CurrentView.SelectedObjects); },
                             null));

            ContextMenuOptionsForVisualComplements.Add(new Tuple<SimplePresentationElement, Func<VisualComplement, FrameworkElement, bool?>, Action<VisualComplement, SimplePresentationElement>, List<SimplePresentationElement>>
                            (new SimplePresentationElement("Cut", "Cut", "Cut the selected objects and let a copy in the Clipboard.", Display.GetAppImage("cut.png")),
                             (target, vexpo) => true,
                             (target, selection) => { var Eng = target.GetDisplayingView().Engine; Eng.CurrentView.SelectObject(target); Eng.ClipboardCut(Eng.CurrentView); },
                             null));

            ContextMenuOptionsForVisualComplements.Add(new Tuple<SimplePresentationElement, Func<VisualComplement, FrameworkElement, bool?>, Action<VisualComplement, SimplePresentationElement>, List<SimplePresentationElement>>
                            (new SimplePresentationElement("Copy", "Copy", "Copy the selected objects to the Clipboard.", Display.GetAppImage("page_white_copy.png")),
                             (target, vexpo) => true,
                             (target, selection) => { var Eng = target.GetDisplayingView().Engine; Eng.CurrentView.SelectObject(target); Eng.ClipboardCopy(Eng.CurrentView); },
                             null));

            ContextMenuOptionsForVisualComplements.Add(null);   // Separator

            ContextMenuOptionsForVisualComplements.Add(new Tuple<SimplePresentationElement, Func<VisualComplement, FrameworkElement, bool?>, Action<VisualComplement, SimplePresentationElement>, List<SimplePresentationElement>>
                            (new SimplePresentationElement("Edit Content", "EditContent", "Edits the Content of this Complement or its owner Idea.", Display.GetAppImage("image_edit.png")),
                             (target, vexpo) => true,
                             (target, selection) => VisualComplement.Edit(target),
                             null));

            ContextMenuOptionsForVisualComplements.Add(null);   // Separator

            /*? Superseded by appropriate controls in line/connector brush (now format) selector.
            ContextMenuOptionsForVisualComplements.Add(new Tuple<SimplePresentationElement, Func<VisualComplement, bool>, Action<VisualComplement, SimplePresentationElement>, List<SimplePresentationElement>>
                            (new SimplePresentationElement("Line Thickness", "LineThickness", "Changes the thickness of the Complement lines.", Display.GetAppImage("line_thickness.png")),
                             (target) => true,
                             (target, selection) => VisualComplement.ChangeLineThickness(target, double.Parse(selection.TechName)),
                             General.CreateList(new SimplePresentationElement("0.5", "0.5"),
                                                new SimplePresentationElement("1.0", "1"),
                                                new SimplePresentationElement("1.5", "1.5"),
                                                new SimplePresentationElement("2.0", "2"),
                                                new SimplePresentationElement("3.0", "3"),
                                                new SimplePresentationElement("4.0", "4"),
                                                new SimplePresentationElement("5.0", "5"),
                                                new SimplePresentationElement("6.0", "6"),
                                                new SimplePresentationElement("7.0", "7"),
                                                new SimplePresentationElement("8.0", "8"),
                                                new SimplePresentationElement("9.0", "9"),
                                                new SimplePresentationElement("10.0", "10"))));

            ContextMenuOptionsForVisualComplements.Add(new Tuple<SimplePresentationElement, Func<VisualComplement, bool>, Action<VisualComplement, SimplePresentationElement>, List<SimplePresentationElement>>
                            (new SimplePresentationElement("Line Dash-Style", "LineDashStyle", "Changes the dash-style of the Complement lines.", Display.GetAppImage("line_dashstyle.png")),
                             (target) => true,
                             (target, selection) => VisualComplement.ChangeLineDashStyle(target, Display.DeclaredDashStyles[int.Parse(selection.TechName)].Item1),
                             Display.DeclaredDashStyles.Select(dds =>
                                 new SimplePresentationElement(dds.Item2, Display.DeclaredDashStyles.IndexOfMatch(d => d.Item1.IsEqual(dds.Item1)).ToString())).ToList())); */

            ContextMenuOptionsForVisualComplements.Add(new Tuple<SimplePresentationElement, Func<VisualComplement, FrameworkElement, bool?>, Action<VisualComplement, SimplePresentationElement>, List<SimplePresentationElement>>
                            (new SimplePresentationElement("Change Axis", "ChangeAxis", "Changes the axis (vertical/horizontal) of this Group Line Complement.", Display.GetAppImage("rod_change_axis.png")),
                             (target, vexpo) => target.IsComplementGroupLine,
                             (target, selection) => VisualComplement.ChangeAxis(target),
                             null));
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Shows the appropriate context menu, depending on the supplied Visual Context, for the specified pointed object.
        /// Returns the generated context menu.
        /// </summary>
        public ContextMenu ShowContextMenu(FrameworkElement VisualContext, UniqueElement PointedObject, UniqueElement DefaultPointedObject = null, Action CloseAction = null)
        {
            ContextMenu Result = null;

            if (PointedObject == null)
                PointedObject = DefaultPointedObject;

            if (PointedObject is Idea)
                PointedObject = ((Idea)PointedObject).MainSymbol;

            if (PointedObject is View)
                Result = CurrentView.Presenter.DisplayContextMenu<View>((View)PointedObject, ContextMenuOptionsForViews, CloseAction);
            else
                if (PointedObject is VisualSymbol)
                    Result = CurrentView.Presenter.DisplayContextMenu<VisualSymbol>((VisualSymbol)PointedObject, ContextMenuOptionsForVisualSymbols, CloseAction);
                else
                    if (PointedObject is VisualConnector)
                        Result = CurrentView.Presenter.DisplayContextMenu<VisualConnector>((VisualConnector)PointedObject, ContextMenuOptionsForVisualConnectors, CloseAction);
                    else
                        if (PointedObject is VisualComplement)
                            Result = CurrentView.Presenter.DisplayContextMenu<VisualComplement>((VisualComplement)PointedObject, ContextMenuOptionsForVisualComplements, CloseAction);

            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}