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
// File   : CompositionsManager.EditingCommands.cs
// Object : Instrumind.ThinkComposer.Composer.CompositionsManager (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.07.06 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.Visualization;
using Instrumind.Common.Visualization.Widgets;

using Instrumind.ThinkComposer.ApplicationProduct;
using Instrumind.ThinkComposer.Composer.ComposerUI;
using Instrumind.ThinkComposer.Composer.ComposerUI.Widgets;
using Instrumind.ThinkComposer.Definitor;
using Instrumind.ThinkComposer.MetaModel;
using Instrumind.ThinkComposer.MetaModel.InformationMetaModel;
using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;
using Instrumind.ThinkComposer.Model.VisualModel;

/// Provides edition, processing and dynamic in-memory storage access for Composition Graphs of Ideas (concepts and relationships) and its Visual representation.
namespace Instrumind.ThinkComposer.Composer
{
    /// <summary>
    /// Manages the edition of user-defined Compositions, working as an intermediary for external consumers.
    /// Creation Commands part.
    /// </summary>
    public partial class CompositionsManager : WorkSphere
    {
        // -------------------------------------------------------------------------------------------------------------------------
        public void CommandUndo_CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            General.ContractRequiresNotNull(sender, args);

            var Doc = (EntityEditEngine)WorkspaceDirector.ActiveDocumentEngine;
            args.CanExecute = (Doc != null && Doc.HasUndoableVariations);
            args.Handled = true;
        }

        public void CommandUndo_Executed(object sender, ExecutedRoutedEventArgs args)
        {
            string CommandName = ((RoutedCommand)args.Command).Name;
            //T AppExec.LogMessage("The '" + CommandName + "' command has been invoked");

            var CompoEngine = WorkspaceDirector.ActiveDocumentEngine as CompositionEngine;
            if (CompoEngine == null)
                return;

            // Cancel running command
            if (CompoEngine.RunningMouseCommand != null)
                CompoEngine.RunningMouseCommand.Terminate(false);

            if (CompoEngine.CurrentView != null && CompoEngine.CurrentView.InPlaceEditingTarget != null)
                CompoEngine.CurrentView.StopEditInplace();

            // Disconnect properties editor
            ProductDirector.EditorInterrelationsControl.SetTarget(null);

            // Clear selection indicators
            var CurrentView = CompoEngine.CurrentView;
            if (CurrentView != null)
            {
                CurrentView.UnselectAllObjects();

                if (CurrentView.Manipulator.WorkingAdorner != null)
                    CurrentView.Manipulator.WorkingAdorner.ClearAllIndicators();
            }

            Console.Write("Undo: ");
            CompoEngine.Undo();
            ProductDirector.UpdateMenuToolbar();    // Update controls of type check-box and others
        }

        // -------------------------------------------------------------------------------------------------------------------------
        public void CommandRedo_CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            General.ContractRequiresNotNull(sender, args);

            var Doc = (EntityEditEngine)WorkspaceDirector.ActiveDocumentEngine;
            args.CanExecute = (Doc != null && Doc.HasRedoableVariations);
            args.Handled = true;
        }

        public void CommandRedo_Executed(object sender, ExecutedRoutedEventArgs args)
        {
            string CommandName = ((RoutedCommand)args.Command).Name;
            //T AppExec.LogMessage("The '" + CommandName + "' command has been invoked");

            var CompoEngine = WorkspaceDirector.ActiveDocumentEngine as CompositionEngine;
            if (CompoEngine == null)
                return;

            // Cancel running command
            if (CompoEngine.RunningMouseCommand != null)
                CompoEngine.RunningMouseCommand.Terminate(false);

            if (CompoEngine.CurrentView != null && CompoEngine.CurrentView.InPlaceEditingTarget != null)
                CompoEngine.CurrentView.StopEditInplace();

            // Disconnect properties editor
            ProductDirector.EditorInterrelationsControl.SetTarget(null);

            // Clear selection indicators
            var CurrentView = CompoEngine.CurrentView;
            if (CurrentView != null)
            {
                CurrentView.UnselectAllObjects();

                if (CurrentView.Manipulator.WorkingAdorner != null)
                    CurrentView.Manipulator.WorkingAdorner.ClearAllIndicators();
            }

            Console.Write("Redo: ");
            CompoEngine.Redo();
            ProductDirector.UpdateMenuToolbar();    // Update controls of type check-box and others
        }

        // ----------------------------------------------------------------------------------------------------------------------------------------
        public void CommandDelete_CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            General.ContractRequiresNotNull(sender, args);

            var Eng = (CompositionEngine)WorkspaceDirector.ActiveDocumentEngine;
            args.CanExecute = (Eng != null && Eng.CurrentView != null && Eng.CurrentView.SelectedObjects.Any());
            args.Handled = true;
        }

        public void CommandDelete_Executed(object sender, ExecutedRoutedEventArgs args)
        {
            string CommandName = ((RoutedCommand)args.Command).Name;
            //T AppExec.LogMessage("The '" + CommandName + "' command has been invoked");

            var Eng = (CompositionEngine)WorkspaceDirector.ActiveDocumentEngine;
            if (Eng == null || Eng.CurrentView == null || !Eng.CurrentView.SelectedObjects.Any())
                return;

            //T Console.WriteLine("Deleting...");
            Eng.DeleteObjects(Eng.CurrentView.SelectedObjects);
        }

        // ----------------------------------------------------------------------------------------------------------------------------------------
        public void CommandCut_CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            General.ContractRequiresNotNull(sender, args);

            var Eng = (CompositionEngine)WorkspaceDirector.ActiveDocumentEngine;
            args.CanExecute = (Eng != null && Eng.CurrentView != null && Eng.CurrentView.SelectedObjects.Any());
            args.Handled = true;
        }

        public void CommandCut_Executed(object sender, ExecutedRoutedEventArgs args)
        {
            string CommandName = ((RoutedCommand)args.Command).Name;
            //T AppExec.LogMessage("The '" + CommandName + "' command has been invoked");

            var Eng = (CompositionEngine)WorkspaceDirector.ActiveDocumentEngine;
            if (Eng == null || Eng.CurrentView == null || !Eng.CurrentView.SelectedObjects.Any())
                return;

            //T Console.WriteLine("Cutting...");
            Eng.ClipboardCut(Eng.CurrentView);
        }

        // ----------------------------------------------------------------------------------------------------------------------------------------
        public void CommandCopy_CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            General.ContractRequiresNotNull(sender, args);

            var Eng = (CompositionEngine)WorkspaceDirector.ActiveDocumentEngine;
            args.CanExecute = (Eng != null && Eng.CurrentView != null && Eng.CurrentView.SelectedObjects.Any());
            args.Handled = true;
        }

        public void CommandCopy_Executed(object sender, ExecutedRoutedEventArgs args)
        {
            string CommandName = ((RoutedCommand)args.Command).Name;
            //T AppExec.LogMessage("The '" + CommandName + "' command has been invoked");

            var Eng = (CompositionEngine)WorkspaceDirector.ActiveDocumentEngine;
            if (Eng == null || Eng.CurrentView == null || !Eng.CurrentView.SelectedObjects.Any())
                return;

            //T Console.WriteLine("Copying...");
            Eng.ClipboardCopy(Eng.CurrentView);
        }

        // ----------------------------------------------------------------------------------------------------------------------------------------
        public void CommandPaste_CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            General.ContractRequiresNotNull(sender, args);

            var Eng = (CompositionEngine)WorkspaceDirector.ActiveDocumentEngine;
            if (Eng == null)
                return;

            args.CanExecute = Eng.ClipboardHasPasteableContent();
            args.Handled = true;
        }

        public void CommandPaste_Executed(object sender, ExecutedRoutedEventArgs args)
        {
            // string CommandName = ((RoutedCommand)args.Command).Name;
            //T AppExec.LogMessage("The '" + CommandName + "' command has been invoked");

            var Eng = (CompositionEngine)WorkspaceDirector.ActiveDocumentEngine;
            if (Eng == null || Eng.CurrentView == null)
                return;

            //T Console.WriteLine("Pasting...");
            Eng.ClipboardPaste(Eng.CurrentView, false, CompositionEngine.CurrentMousePosition);
        }

        // -------------------------------------------------------------------------------------------------------------------------
        public bool CommandSwitchDetails_IsEnabled(object Parameter)
        {
            var Eng = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;
            return (Eng != null && Eng.CurrentView != null && Eng.CurrentView.SelectedRepresentations.Any());
        }

        public void CommandSwitchDetails_Execution(object Parameter)
        {
            var Eng = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;
            if (Eng == null || Eng.CurrentView == null || !Eng.CurrentView.SelectedRepresentations.Any())
                return;

            //T Console.WriteLine("Switching details...");
            var Show = !Eng.CurrentView.SelectedRepresentations.First().MainSymbol.AreDetailsShown;

            Eng.CurrentView.EditEngine.StartCommandVariation("Switch Details on multiple targets");

            foreach (var Selection in Eng.CurrentView.SelectedRepresentations)
                Eng.CurrentView.Manipulator.SwitchDetails(Selection.MainSymbol, Show, false);

            Eng.CurrentView.UpdateVersion();
            Eng.CurrentView.ShowAll();

            Eng.CurrentView.EditEngine.CompleteCommandVariation();
        }

        // -------------------------------------------------------------------------------------------------------------------------
        public bool CommandSelectFillBrush_IsEnabled(object Parameter)
        {
            var Eng = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;
            return (Eng != null && Eng.CurrentView != null && Eng.CurrentView.SelectedObjects.Any());
        }

        public void CommandSelectFillBrush_Execution(object Parameter)
        {
            var Eng = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;
            if (Eng == null || Eng.CurrentView == null || !Eng.CurrentView.SelectedObjects.Any())
                return;

            //T Console.WriteLine("Selecting fill brush...");
            Tuple<Brush> CurrentBrush = Eng.CurrentView.SelectedObjects.Select(selobj =>
                                         (selobj is VisualSymbol
                                          ? Tuple.Create(VisualElementFormat.GetMainBackground((VisualSymbol)selobj))
                                          : ((selobj is VisualComplement && ((VisualComplement)selobj).CanSetColors)
                                             ? Tuple.Create(((VisualComplement)selobj).GetPropertyField<Brush>(VisualComplement.PROP_FIELD_BACKGROUND))
                                             : null))).FirstOrDefault(brush => brush != null);
            if (CurrentBrush == null)
                return;

            var Result = Display.DialogSelectBrush(CurrentBrush.Item1);
            if (Result == null || !Result.Item1)
                return;

            Eng.StartCommandVariation("Change Fill Brush");

            foreach (var Selection in Eng.CurrentView.SelectedObjects)
                if (Selection is VisualSymbol)
                {
                    var SelectedSymbol = Selection as VisualSymbol;
                    VisualElementFormat.SetMainBackground(SelectedSymbol, Result.Item2);
                    SelectedSymbol.RenderElement();
                    SelectedSymbol.OwnerRepresentation.RepresentedIdea.NotifyPropertyChange("RepresentativePicture");
                }
                else
                    if (Selection is VisualComplement && ((VisualComplement)Selection).CanSetColors)
                    {
                        ((VisualComplement)Selection).SetPropertyField(VisualComplement.PROP_FIELD_BACKGROUND, Result.Item2);
                        ((VisualComplement)Selection).Render();
                    }

            Eng.CurrentView.UpdateVersion();
            Eng.CompleteCommandVariation();
        }

        // -------------------------------------------------------------------------------------------------------------------------
        public bool CommandSelectLineBrush_IsEnabled(object Parameter)
        {
            var Eng = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;
            return (Eng != null && Eng.CurrentView != null && Eng.CurrentView.SelectedObjects.Any());
        }

        public void CommandSelectLineBrush_Execution(object Parameter)
        {
            var Eng = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;
            if (Eng == null || Eng.CurrentView == null || !Eng.CurrentView.SelectedObjects.Any())
                return;

            //T Console.WriteLine("Selecting Line brush...");
            Tuple<Brush, double, DashStyle> CurrentFormat =
                Eng.CurrentView.SelectedObjects.Select(selobj =>
                                         (selobj is VisualSymbol
                                          ? Tuple.Create(VisualElementFormat.GetLineBrush((VisualSymbol)selobj),
                                                         VisualElementFormat.GetLineThickness((VisualSymbol)selobj),
                                                         VisualElementFormat.GetLineDash((VisualSymbol)selobj))
                                          : ((selobj is VisualComplement && ((VisualComplement)selobj).CanSetColors)
                                             ? Tuple.Create(((VisualComplement)selobj).GetPropertyField<Brush>(VisualComplement.PROP_FIELD_FOREGROUND),
                                                            ((VisualComplement)selobj).GetPropertyField<double>(VisualComplement.PROP_FIELD_LINETHICK),
                                                            ((VisualComplement)selobj).GetPropertyField<DashStyle>(VisualComplement.PROP_FIELD_LINEDASH))
                                             : null))).FirstOrDefault(brush => brush != null);
            if (CurrentFormat == null)
                return;

            var Result = DomainServices.DialogSelectFormat(CurrentFormat.Item1, CurrentFormat.Item2, CurrentFormat.Item3);
            if (Result == null || !Result.Item1)
                return;

            Eng.StartCommandVariation("Change Line Format");

            foreach (var Selection in Eng.CurrentView.SelectedObjects)
                if (Selection is VisualSymbol)
                {
                    var SelectedSymbol = Selection as VisualSymbol;
                    VisualElementFormat.SetLineBrush(SelectedSymbol, Result.Item2);
                    VisualElementFormat.SetLineThickness(SelectedSymbol, Result.Item3);
                    VisualElementFormat.SetLineDash(SelectedSymbol, Result.Item4);
                    SelectedSymbol.RenderElement();
                    SelectedSymbol.OwnerRepresentation.RepresentedIdea.NotifyPropertyChange("RepresentativePicture");
                }
                else
                    if (Selection is VisualComplement && ((VisualComplement)Selection).CanSetColors)
                    {
                        ((VisualComplement)Selection).SetPropertyField(VisualComplement.PROP_FIELD_FOREGROUND, Result.Item2);
                        ((VisualComplement)Selection).SetPropertyField(VisualComplement.PROP_FIELD_LINETHICK, Result.Item3);
                        ((VisualComplement)Selection).SetPropertyField(VisualComplement.PROP_FIELD_LINEDASH, Result.Item4);
                        ((VisualComplement)Selection).Render();
                    }

            Eng.CurrentView.UpdateVersion();
            Eng.CompleteCommandVariation();
        }

        // -------------------------------------------------------------------------------------------------------------------------
        public bool CommandSelectConnectorBrush_IsEnabled(object Parameter)
        {
            var Eng = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;
            return (Eng != null && Eng.CurrentView != null && Eng.CurrentView.SelectedRepresentations.Any(obj => obj is RelationshipVisualRepresentation));
        }

        public void CommandSelectConnectorBrush_Execution(object Parameter)
        {
            var Eng = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;
            if (Eng == null || Eng.CurrentView == null || !Eng.CurrentView.SelectedRepresentations.Any())
                return;

            //T Console.WriteLine("Selecting Connector brush...");
            var SelectedReps = Eng.CurrentView.SelectedRepresentations.CastAs<RelationshipVisualRepresentation, VisualRepresentation>()
                                        .Where(relrep => relrep.VisualConnectorsCount > 0);
            var SelectedConn = SelectedReps.First().VisualConnectors.FirstOrDefault();
            if (SelectedConn == null)
                return;

            var CurrentBrush = VisualConnectorsFormat.GetLineBrush(SelectedConn);
            var CurrentThickness = VisualConnectorsFormat.GetLineThickness(SelectedConn);
            var CurrentDashStyle = VisualConnectorsFormat.GetLineDash(SelectedConn);

            var Result = DomainServices.DialogSelectFormat(CurrentBrush, CurrentThickness, CurrentDashStyle);
            if (Result == null || !Result.Item1)
                return;

            Eng.StartCommandVariation("Change Connector Format");

            foreach (var Selection in SelectedReps)
                foreach(var Connector in Selection.VisualConnectors)
                {
                    VisualConnectorsFormat.SetLineBrush(Connector, Result.Item2);
                    VisualConnectorsFormat.SetLineThickness(Connector, Result.Item3);
                    VisualConnectorsFormat.SetLineDash(Connector, Result.Item4);
                    Selection.Render();
                }

            Eng.CurrentView.UpdateVersion();
            Eng.CompleteCommandVariation();
        }

        // -------------------------------------------------------------------------------------------------------------------------
        public bool CommandSelectTextBrush_IsEnabled(object Parameter)
        {
            var Eng = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;
            return (Eng != null && Eng.CurrentView != null && Eng.CurrentView.SelectedObjects.Any());
        }

        public void CommandSelectTextBrush_Execution(object Parameter)
        {
            var Eng = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;
            if (Eng == null || Eng.CurrentView == null || !Eng.CurrentView.SelectedObjects.Any())
                return;

            //T Console.WriteLine("Selecting title color...");
            Tuple<TextFormat> CurrentFormat = Eng.CurrentView.SelectedObjects.Select(selobj =>
                                              (selobj is VisualSymbol
                                               ? Tuple.Create(VisualSymbolFormat.GetTextFormat((VisualSymbol)selobj, ETextPurpose.Title))
                                               : ((selobj is VisualComplement && ((VisualComplement)selobj).CanSetText)
                                                  ? Tuple.Create(((VisualComplement)selobj).GetPropertyField<TextFormat>(VisualComplement.PROP_FIELD_TEXTFORMAT))
                                                  : null))).FirstOrDefault(format => format != null);
            if (CurrentFormat == null)
                return;

            var Result = Display.DialogSelectBrush(CurrentFormat.Item1.ForegroundBrush);
            if (Result == null || !Result.Item1)
                return;

            Eng.StartCommandVariation("Change Text Brush");

            foreach (var Selection in Eng.CurrentView.SelectedObjects)
                if (Selection is VisualSymbol)
                {
                    var SelectedSymbol = Selection as VisualSymbol;
                    var New = new TextFormat(VisualSymbolFormat.GetTextFormat(SelectedSymbol, ETextPurpose.Title));
                    New.ForegroundBrush = Result.Item2;
                    VisualSymbolFormat.SetTextFormat(SelectedSymbol, ETextPurpose.Title, New);
                    SelectedSymbol.RenderElement();
                }
                else
                    if (Selection is VisualComplement && ((VisualComplement)Selection).CanSetText)
                    {
                        CurrentFormat.Item1.ForegroundBrush = Result.Item2;
                        ((VisualComplement)Selection).SetPropertyField(VisualComplement.PROP_FIELD_TEXTFORMAT, CurrentFormat.Item1);
                        ((VisualComplement)Selection).Render();
                    }

            Eng.CurrentView.UpdateVersion();
            Eng.CompleteCommandVariation();
        }

        // -------------------------------------------------------------------------------------------------------------------------
        public bool CommandSelectTextFormat_IsEnabled(object Parameter)
        {
            var Eng = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;
            return (Eng != null && Eng.CurrentView != null && Eng.CurrentView.SelectedObjects.Any());
        }

        public void CommandSelectTextFormat_Execution(object Parameter)
        {
            var Eng = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;
            if (Eng == null || Eng.CurrentView == null || !Eng.CurrentView.SelectedObjects.Any())
                return;

            //T Console.WriteLine("Selecting title format...");
            Tuple<TextFormat> CurrentFormat = Eng.CurrentView.SelectedObjects.Select(selobj =>
                                              (selobj is VisualSymbol
                                               ? Tuple.Create(VisualSymbolFormat.GetTextFormat((VisualSymbol)selobj, ETextPurpose.Title))
                                               : ((selobj is VisualComplement && ((VisualComplement)selobj).CanSetText)
                                                  ? Tuple.Create(((VisualComplement)selobj).GetPropertyField<TextFormat>(VisualComplement.PROP_FIELD_TEXTFORMAT))
                                                  : null))).FirstOrDefault(format => format != null);
            if (CurrentFormat == null)
                return;

            var Result = TextFormatSelector.SelectFor(CurrentFormat.Item1);
            if (Result == null || !Result.Item1)
                return;

            Eng.StartCommandVariation("Change Text Format");

            foreach (var Selection in Eng.CurrentView.SelectedObjects)
                if (Selection is VisualSymbol)
                {
                    var SelectedSymbol = Selection as VisualSymbol;
                    VisualSymbolFormat.SetTextFormat(SelectedSymbol, ETextPurpose.Title, Result.Item2);
                    SelectedSymbol.RenderElement();
                }
                else
                    if (Selection is VisualComplement && ((VisualComplement)Selection).CanSetText)
                    {
                        ((VisualComplement)Selection).SetPropertyField(VisualComplement.PROP_FIELD_TEXTFORMAT, Result.Item2);
                        ((VisualComplement)Selection).Render();
                    }

            Eng.CurrentView.UpdateVersion();
            Eng.CompleteCommandVariation();
        }

        // -------------------------------------------------------------------------------------------------------------------------
        public bool CommandGetFormat_IsEnabled(object Parameter)
        {
            var Eng = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;
            return (Eng != null && Eng.CurrentView != null
                    && Eng.CurrentView.SelectedObjects.Count(vo => (vo is VisualElement) || (vo is VisualComplement)) == 1);
        }

        public void CommandGetFormat_Execution(object Parameter)
        {
            var Eng = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;
            if (Eng == null || Eng.CurrentView == null || Eng.CurrentView.SelectedObjects.Count(vo => (vo is VisualElement) || (vo is VisualComplement)) != 1)
                return;

            //T Console.WriteLine("Getting format...");
            Eng.CurrentVisRepSelectedAsFormatSource = Eng.CurrentView.SelectedObjects.FirstOrDefault(vo => vo is VisualElement).Get(ve => ((VisualElement)ve).OwnerRepresentation);
            Eng.CurrentVisCmpSelectedAsFormatSource = Eng.CurrentView.SelectedObjects.FirstOrDefault(vo => vo is VisualComplement) as VisualComplement;

            Console.WriteLine("Format obtained. Now select objects and apply it.");
        }

        public bool CommandApplyFormat_IsEnabled(object Parameter)
        {
            var Eng = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;
            return (Eng != null && Eng.CurrentView != null
                    && (Eng.CurrentVisRepSelectedAsFormatSource != null || Eng.CurrentVisCmpSelectedAsFormatSource != null)
                    && (Eng.CurrentView.SelectedObjects.Any(vo => (vo is VisualElement) || (vo is VisualComplement))));
        }

        public void CommandApplyFormat_Execution(object Parameter)
        {
            var Eng = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;

            if (!(Eng != null && Eng.CurrentView != null
                  && (Eng.CurrentVisRepSelectedAsFormatSource != null || Eng.CurrentVisCmpSelectedAsFormatSource != null)
                  && (Eng.CurrentView.SelectedObjects.Any(vo => (vo is VisualElement) || (vo is VisualComplement)))))
                return;

            //T Console.WriteLine("Applying Format...");

            Eng.StartCommandVariation("Apply Format");

            var TextPurposes = Enum.GetValues(typeof(ETextPurpose));

            var FmtSource = Eng.CurrentVisRepSelectedAsFormatSource.Get(cvr => cvr.MainSymbol);

            var Selection = Eng.CurrentView.SelectedObjects.Where(so => ((so is VisualElement) || (so is VisualComplement)));
            foreach (var SelVisObj in Selection)
            {
                var TargetVisElm = (SelVisObj as VisualElement);
                var TargetVisRep = (TargetVisElm == null ? null : TargetVisElm.OwnerRepresentation);
                if (TargetVisRep != null)
                {
                    VisualSymbolFormat.SetMainBackground(TargetVisRep.MainSymbol,
                     FmtSource != null ? VisualSymbolFormat.GetMainBackground(FmtSource)
                                       : Eng.CurrentVisCmpSelectedAsFormatSource.GetPropertyField<Brush>(VisualComplement.PROP_FIELD_BACKGROUND));

                    VisualSymbolFormat.SetLineBrush(TargetVisRep.MainSymbol,
                     FmtSource != null ? VisualSymbolFormat.GetLineBrush(FmtSource)
                                       : Eng.CurrentVisCmpSelectedAsFormatSource.GetPropertyField<Brush>(VisualComplement.PROP_FIELD_FOREGROUND));

                    VisualSymbolFormat.SetLineDash(TargetVisRep.MainSymbol,
                     FmtSource != null ? VisualSymbolFormat.GetLineDash(FmtSource)
                                       : Eng.CurrentVisCmpSelectedAsFormatSource.GetPropertyField<DashStyle>(VisualComplement.PROP_FIELD_LINEDASH));

                    VisualSymbolFormat.SetLineThickness(TargetVisRep.MainSymbol,
                     FmtSource != null ? VisualSymbolFormat.GetLineThickness(FmtSource)
                                       : Eng.CurrentVisCmpSelectedAsFormatSource.GetPropertyField<double>(VisualComplement.PROP_FIELD_LINETHICK));

                    if (FmtSource == null)
                        VisualSymbolFormat.SetTextFormat(TargetVisRep.MainSymbol, ETextPurpose.Title,
                                                         Eng.CurrentVisCmpSelectedAsFormatSource.GetPropertyField<TextFormat>(VisualComplement.PROP_FIELD_TEXTFORMAT));
                    else
                    {
                        foreach (var TextPurpose in TextPurposes)
                            VisualSymbolFormat.SetTextFormat(TargetVisRep.MainSymbol, (ETextPurpose)TextPurpose,
                             VisualSymbolFormat.GetTextFormat(FmtSource, (ETextPurpose)TextPurpose));

                        VisualSymbolFormat.SetDetailCaptionBackground(TargetVisRep.MainSymbol,
                         VisualSymbolFormat.GetDetailCaptionBackground(FmtSource));

                        VisualSymbolFormat.SetDetailCaptionForeground(TargetVisRep.MainSymbol,
                         VisualSymbolFormat.GetDetailCaptionForeground(FmtSource));

                        VisualSymbolFormat.SetDetailContentBackground(TargetVisRep.MainSymbol,
                         VisualSymbolFormat.GetDetailContentBackground(FmtSource));

                        VisualSymbolFormat.SetDetailContentForeground(TargetVisRep.MainSymbol,
                         VisualSymbolFormat.GetDetailContentForeground(FmtSource));

                        VisualSymbolFormat.SetDetailHeadingBackground(TargetVisRep.MainSymbol,
                         VisualSymbolFormat.GetDetailHeadingBackground(FmtSource));

                        VisualSymbolFormat.SetDetailHeadingForeground(TargetVisRep.MainSymbol,
                         VisualSymbolFormat.GetDetailHeadingForeground(FmtSource));

                        VisualSymbolFormat.SetDetailsPosterIsHanging(TargetVisRep.MainSymbol,
                         VisualSymbolFormat.GetDetailsPosterIsHanging(FmtSource));

                        /*-
                        // not included, it's only for creation start
                        VisualSymbolFormat.SetInitialHeight(Selection.MainSymbol,
                         VisualSymbolFormat.GetInitialHeight(FmtSource));

                        // not included, it's only for creation start
                        VisualSymbolFormat.SetInitialWidth(Selection.MainSymbol,
                         VisualSymbolFormat.GetInitialWidth(FmtSource));

                        // not included, target may not have origin behavior
                        VisualSymbolFormat.SetHasFixedWidth(Selection.MainSymbol,
                         VisualSymbolFormat.GetHasFixedWidth(FmtSource));

                        // not included, target may not have origin behavior
                        VisualSymbolFormat.SetHasFixedHeight(Selection.MainSymbol,
                         VisualSymbolFormat.GetHasFixedHeight(FmtSource));

                        // not included, target may not have origin behavior
                        VisualSymbolFormat.SetInPlaceEditingIsMultiline(Selection.MainSymbol,
                         VisualSymbolFormat.GetInPlaceEditingIsMultiline(FmtSource));

                        // not included, it's only for creation start
                        VisualSymbolFormat.SetInitiallyFlippedHorizontally(Selection.MainSymbol,
                            VisualSymbolFormat.GetInitiallyFlippedHorizontally(FmtSource));

                        // not included, it's only for creation start
                        VisualSymbolFormat.SetInitiallyFlippedVertically(Selection.MainSymbol,
                            VisualSymbolFormat.GetInitiallyFlippedVertically(FmtSource));
                 
                        // not included, it's only for creation start + Consider carefully a "Rotate" feature.
                        VisualSymbolFormat.SetInitiallyTilted(Selection.MainSymbol,
                            VisualSymbolFormat.GetInitiallyTilted(FmtSource));
                        */

                        VisualSymbolFormat.SetAsMultiple(TargetVisRep.MainSymbol,
                            VisualSymbolFormat.GetAsMultiple(FmtSource));

                        VisualSymbolFormat.SetLineCap(TargetVisRep.MainSymbol,
                         VisualSymbolFormat.GetLineCap(FmtSource));

                        VisualSymbolFormat.SetLineJoin(TargetVisRep.MainSymbol,
                         VisualSymbolFormat.GetLineJoin(FmtSource));

                        VisualSymbolFormat.SetOpacity(TargetVisRep.MainSymbol,
                         VisualSymbolFormat.GetOpacity(FmtSource));

                        VisualSymbolFormat.SetPictogramVisualDisposition(TargetVisRep.MainSymbol,
                         VisualSymbolFormat.GetPictogramVisualDisposition(FmtSource));

                        /*- VisualSymbolFormat.SetRegionBackground(Selection.MainSymbol,
                         VisualSymbolFormat.GetRegionBackground(FmtSource));

                        VisualSymbolFormat.SetRegionForeground(Selection.MainSymbol,
                         VisualSymbolFormat.GetRegionForeground(FmtSource));
                 
                         ...plus Dash and Thickness*/

                        VisualSymbolFormat.SetShowGlobalDetailsFirst(TargetVisRep.MainSymbol,
                         VisualSymbolFormat.GetShowGlobalDetailsFirst(FmtSource));

                        VisualSymbolFormat.SetSubtitleVisualDisposition(TargetVisRep.MainSymbol,
                         VisualSymbolFormat.GetSubtitleVisualDisposition(FmtSource));

                        VisualSymbolFormat.SetUseDefinitorPictogramAsNullDefault(TargetVisRep.MainSymbol,
                         VisualSymbolFormat.GetUseDefinitorPictogramAsNullDefault(FmtSource));

                        VisualSymbolFormat.SetUseNameAsMainTitle(TargetVisRep.MainSymbol,
                         VisualSymbolFormat.GetUseNameAsMainTitle(FmtSource));

                        VisualSymbolFormat.SetUsePictogramAsSymbol(TargetVisRep.MainSymbol,
                         VisualSymbolFormat.GetUsePictogramAsSymbol(FmtSource));

                        /* VisualSymbolFormat.SetMainBackground(SelVisRep.MainSymbol,
                        VisualSymbolFormat.GetMainBackground(FmtSource));

                        VisualSymbolFormat.SetLineBrush(SelVisRep.MainSymbol,
                         VisualSymbolFormat.GetLineBrush(FmtSource));

                        VisualSymbolFormat.SetLineDash(SelVisRep.MainSymbol,
                         VisualSymbolFormat.GetLineDash(FmtSource));

                        VisualSymbolFormat.SetLineThickness(SelVisRep.MainSymbol,
                         VisualSymbolFormat.GetLineThickness(FmtSource)); */

                        var RelRepSelection = TargetVisRep as RelationshipVisualRepresentation;
                        if (RelRepSelection != null && Eng.CurrentVisRepSelectedAsFormatSource is RelationshipVisualRepresentation)
                        {
                            var RelRepSource = (RelationshipVisualRepresentation)Eng.CurrentVisRepSelectedAsFormatSource;

                            if (RelRepSource.VisualConnectorsCount > 0)
                            {
                                var SourceConnector = RelRepSource.VisualConnectors.First();

                                foreach (var Connector in RelRepSelection.VisualConnectors)
                                {
                                    VisualConnectorsFormat.SetLabelLinkDefinitor(Connector,
                                     VisualConnectorsFormat.GetLabelLinkDefinitor(SourceConnector));

                                    VisualConnectorsFormat.SetLabelLinkDescriptor(Connector,
                                     VisualConnectorsFormat.GetLabelLinkDescriptor(SourceConnector));

                                    VisualConnectorsFormat.SetLabelLinkVariant(Connector,
                                     VisualConnectorsFormat.GetLabelLinkVariant(SourceConnector));

                                    VisualConnectorsFormat.SetPathCorner(Connector,
                                     VisualConnectorsFormat.GetPathCorner(SourceConnector));

                                    VisualConnectorsFormat.SetPathStyle(Connector,
                                     VisualConnectorsFormat.GetPathStyle(SourceConnector));
                                }
                            }
                        }
                    }

                    TargetVisRep.RepresentedIdea.NotifyPropertyChange("RepresentativePicture");
                    TargetVisRep.Render();
                }

                var TargetVisCmp = (SelVisObj as VisualComplement);
                if (TargetVisCmp != null)
                {
                    if (FmtSource == null)
                    {
                        TargetVisCmp.SetPropertyField(VisualComplement.PROP_FIELD_BACKGROUND,
                            Eng.CurrentVisCmpSelectedAsFormatSource.GetPropertyField<Brush>(VisualComplement.PROP_FIELD_BACKGROUND));
                        TargetVisCmp.SetPropertyField(VisualComplement.PROP_FIELD_FOREGROUND,
                            Eng.CurrentVisCmpSelectedAsFormatSource.GetPropertyField<Brush>(VisualComplement.PROP_FIELD_FOREGROUND));
                        TargetVisCmp.SetPropertyField(VisualComplement.PROP_FIELD_LINEDASH,
                            Eng.CurrentVisCmpSelectedAsFormatSource.GetPropertyField<DashStyle>(VisualComplement.PROP_FIELD_LINEDASH));
                        TargetVisCmp.SetPropertyField(VisualComplement.PROP_FIELD_LINETHICK,
                            Eng.CurrentVisCmpSelectedAsFormatSource.GetPropertyField<double>(VisualComplement.PROP_FIELD_LINETHICK));
                        TargetVisCmp.SetPropertyField(VisualComplement.PROP_FIELD_TEXTFORMAT,
                            Eng.CurrentVisCmpSelectedAsFormatSource.GetPropertyField<TextFormat>(VisualComplement.PROP_FIELD_TEXTFORMAT));
                    }
                    else
                    {
                        TargetVisCmp.SetPropertyField(VisualComplement.PROP_FIELD_BACKGROUND, VisualSymbolFormat.GetMainBackground(FmtSource));
                        TargetVisCmp.SetPropertyField(VisualComplement.PROP_FIELD_FOREGROUND, VisualSymbolFormat.GetLineBrush(FmtSource));
                        TargetVisCmp.SetPropertyField(VisualComplement.PROP_FIELD_LINEDASH, VisualSymbolFormat.GetLineDash(FmtSource));
                        TargetVisCmp.SetPropertyField(VisualComplement.PROP_FIELD_LINETHICK, VisualSymbolFormat.GetLineThickness(FmtSource));
                        TargetVisCmp.SetPropertyField(VisualComplement.PROP_FIELD_TEXTFORMAT, VisualSymbolFormat.GetTextFormat(FmtSource, ETextPurpose.Title));
                    }

                    TargetVisCmp.Render();
                }
            }

            Eng.CurrentView.UpdateVersion();
            Eng.CompleteCommandVariation();
        }

        // -------------------------------------------------------------------------------------------------------------------------
        public bool CommandMultiPosMin2_IsEnabled(object Parameter)
        {
            var Eng = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;
            return (Eng != null && Eng.CurrentView != null && Eng.CurrentView.SelectedObjects.Count >= 2);
        }

        public bool CommandMultiPosMin3_IsEnabled(object Parameter)
        {
            var Eng = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;
            return (Eng != null && Eng.CurrentView != null && Eng.CurrentView.SelectedObjects.Count >= 3);
        }

        public void CommandAlignTop_Execution(object Parameter)
        {
            var Eng = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;
            if (Eng == null || Eng.CurrentView == null || !Eng.CurrentView.SelectedObjects.Any())
                return;

            //T Console.WriteLine("Aligning to Top...");
            Eng.StartCommandVariation("Align Top");

            var FirstVisObj = Eng.CurrentView.SelectedObjects.First();
            var AlignmentPosition = FirstVisObj.BaseTop;

            foreach (var Selection in Eng.CurrentView.SelectedObjects.Skip(1))
            {
                var NewPos = AlignmentPosition + (Selection.BaseHeight / 2.0);
                Selection.MoveTo(Selection.BaseCenter.X, NewPos, true);
            }

            Eng.CompleteCommandVariation();
        }

        public void CommandAlignLeft_Execution(object Parameter)
        {
            var Eng = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;
            if (Eng == null || Eng.CurrentView == null || !Eng.CurrentView.SelectedObjects.Any())
                return;

            //T Console.WriteLine("Aligning to Left...");
            Eng.StartCommandVariation("Align Left");

            var FirstVisObj = Eng.CurrentView.SelectedObjects.First();
            var AlignmentPosition = FirstVisObj.BaseLeft;

            foreach (var Selection in Eng.CurrentView.SelectedObjects.Skip(1))
            {
                var NewCenter = AlignmentPosition + (Selection.BaseWidth / 2.0);
                Selection.MoveTo(NewCenter, Selection.BaseCenter.Y, true);
            }

            Eng.CompleteCommandVariation();
        }

        public void CommandAlignBottom_Execution(object Parameter)
        {
            var Eng = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;
            if (Eng == null || Eng.CurrentView == null || !Eng.CurrentView.SelectedObjects.Any())
                return;

            //T Console.WriteLine("Aligning to Bottom...");
            Eng.StartCommandVariation("Align Bottom");

            var FirstVisObj = Eng.CurrentView.SelectedObjects.First();
            var AlignmentPosition = FirstVisObj.BaseTop +
                                    (FirstVisObj is VisualSymbol
                                     ? ((VisualSymbol)FirstVisObj).TotalHeight
                                     : FirstVisObj.BaseHeight);

            foreach (var Selection in Eng.CurrentView.SelectedObjects.Skip(1))
            {
                var NewMiddle = AlignmentPosition - ((Selection.BaseHeight / 2.0)
                                                     + (Selection is VisualSymbol && ((VisualSymbol)Selection).AreDetailsShown
                                                        ? ((VisualSymbol)Selection).DetailsPosterHeight
                                                        : 0));
                Selection.MoveTo(Selection.BaseCenter.X, NewMiddle, true);
            }

            Eng.CompleteCommandVariation();
        }

        public void CommandAlignRight_Execution(object Parameter)
        {
            var Eng = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;
            if (Eng == null || Eng.CurrentView == null || !Eng.CurrentView.SelectedObjects.Any())
                return;

            //T Console.WriteLine("Aligning to Right...");
            Eng.StartCommandVariation("Align Right");

            var FirstVisObj = Eng.CurrentView.SelectedObjects.First();
            var AlignmentPosition = FirstVisObj.BaseCenter.X + (FirstVisObj.BaseWidth / 2.0);

            foreach (var Selection in Eng.CurrentView.SelectedObjects.Skip(1))
            {
                var NewCenter = AlignmentPosition - (Selection.BaseWidth / 2.0);
                Selection.MoveTo(NewCenter, Selection.BaseCenter.Y, true);
            }

            Eng.CompleteCommandVariation();
        }

        public void CommandAlignCenter_Execution(object Parameter)
        {
            var Eng = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;
            if (Eng == null || Eng.CurrentView == null || !Eng.CurrentView.SelectedObjects.Any())
                return;

            //T Console.WriteLine("Aligning to Center...");
            Eng.StartCommandVariation("Align Center");

            var AlignmentPosition = Eng.CurrentView.SelectedObjects.First().BaseCenter.X;

            foreach (var Selection in Eng.CurrentView.SelectedObjects.Skip(1))
                Selection.MoveTo(AlignmentPosition, Selection.BaseCenter.Y, true);

            Eng.CompleteCommandVariation();
        }

        public void CommandAlignMiddle_Execution(object Parameter)
        {
            var Eng = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;
            if (Eng == null || Eng.CurrentView == null || !Eng.CurrentView.SelectedObjects.Any())
                return;

            //T Console.WriteLine("Aligning to Middle...");
            Eng.StartCommandVariation("Align Middle");

            var FirstVisObj = Eng.CurrentView.SelectedObjects.First();
            var AlignmentPosition = FirstVisObj.BaseTop +
                                    ((FirstVisObj is VisualSymbol
                                      ? ((VisualSymbol)FirstVisObj).TotalHeight
                                      : FirstVisObj.BaseHeight) / 2.0);

            foreach (var Selection in Eng.CurrentView.SelectedObjects.Skip(1))
            {
                var NewMiddle = AlignmentPosition - (((Selection is VisualSymbol
                                                       ? ((VisualSymbol)Selection).TotalHeight
                                                       : Selection.BaseHeight) / 2.0)
                                                     - (Selection.BaseHeight / 2.0));
                Selection.MoveTo(Selection.BaseCenter.X, NewMiddle, true);
            }

            Eng.CompleteCommandVariation();
        }

        public void CommandAlignSameWidth_Execution(object Parameter)
        {
            var Eng = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;
            if (Eng == null || Eng.CurrentView == null || !Eng.CurrentView.SelectedObjects.Any())
                return;

            //T Console.WriteLine("Aligning to Same Width...");
            Eng.StartCommandVariation("Align Same Width");

            ApplySameWidth(Eng.CurrentView.SelectedObjects.First(),
                           Eng.CurrentView.SelectedObjects.Skip(1));

            Eng.CompleteCommandVariation();
        }
        private void ApplySameWidth(VisualObject BaseObject, IEnumerable<VisualObject> Targets)
        {
            foreach (var Selection in Targets)
                Selection.ResizeTo(BaseObject.BaseWidth, Selection.BaseHeight);
        }

        public void CommandAlignSameHeight_Execution(object Parameter)
        {
            var Eng = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;
            if (Eng == null || Eng.CurrentView == null || !Eng.CurrentView.SelectedObjects.Any())
                return;

            //T Console.WriteLine("Aligning to Same Height...");
            Eng.StartCommandVariation("Align Same Height");

            ApplySameHeight(Eng.CurrentView.SelectedObjects.First(),
                            Eng.CurrentView.SelectedObjects.Skip(1));

            Eng.CompleteCommandVariation();
        }
        private void ApplySameHeight(VisualObject BaseObject, IEnumerable<VisualObject> Targets)
        {
            foreach (var Selection in Targets)
            {
                if (Selection is VisualSymbol && BaseObject is VisualSymbol)
                    ((VisualSymbol)Selection).DetailsPosterHeight = ((VisualSymbol)BaseObject).DetailsPosterHeight;

                Selection.ResizeTo(Selection.BaseWidth, BaseObject.BaseHeight);
            }
        }

        public void CommandAlignSameSize_Execution(object Parameter)
        {
            var Eng = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;
            if (Eng == null || Eng.CurrentView == null || !Eng.CurrentView.SelectedObjects.Any())
                return;

            //T Console.WriteLine("Aligning to Same Size...");
            Eng.StartCommandVariation("Align Same Size");

            ApplySameWidth(Eng.CurrentView.SelectedObjects.First(),
                           Eng.CurrentView.SelectedObjects.Skip(1));
            ApplySameHeight(Eng.CurrentView.SelectedObjects.First(),
                            Eng.CurrentView.SelectedObjects.Skip(1));

            Eng.CompleteCommandVariation();
        }

        public void CommandSameSeparationDistanceHorizontal_Execution(object Parameter)
        {
            var Eng = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;
            if (Eng == null || Eng.CurrentView == null || Eng.CurrentView.SelectedObjects.Count < 3)
                return;

            //T Console.WriteLine("Separating same distance horizontal...");
            Eng.StartCommandVariation("Separate Same Horizontal Distance");
            ApplySeparateSameHorizontalDistance(Eng);
            Eng.CompleteCommandVariation();
        }

        public void CommandSameSeparationDistanceVertical_Execution(object Parameter)
        {
            var Eng = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;
            if (Eng == null || Eng.CurrentView == null || Eng.CurrentView.SelectedObjects.Count < 3)
                return;

            //T Console.WriteLine("Separating same distance vertical...");
            Eng.StartCommandVariation("Separate Same Vertical Distance");
            ApplySeparateSameVerticalDistance(Eng);
            Eng.CompleteCommandVariation();
        }

        private void ApplySeparateSameHorizontalDistance(CompositionEngine Eng)
        {
            var SelectedElements = Eng.CurrentView.SelectedObjects
                                    .Where(vobj => !(vobj is VisualSymbol) || !((VisualSymbol)vobj).IsHidden)
                                    .OrderBy(vobj => vobj.TotalArea.Left).ToList();
            var Separation = Math.Abs(SelectedElements[1].TotalArea.Left - SelectedElements[0].TotalArea.Right);

            var PreviousPos = SelectedElements[1].TotalArea.Right;  // Calculated locally because visual update is "lazy".
            foreach (var Selection in SelectedElements.Skip(2))
            {
                var NewPos = PreviousPos + Separation + (Selection.TotalArea.Width / 2.0);
                Selection.MoveTo(NewPos, Selection.BaseCenter.Y, true);
                PreviousPos += Selection.TotalArea.Width + Separation;
            }
        }

        private void ApplySeparateSameVerticalDistance(CompositionEngine Eng)
        {
            var SelectedElements = Eng.CurrentView.SelectedObjects
                                    .Where(vobj => !(vobj is VisualSymbol) || !((VisualSymbol)vobj).IsHidden)
                                    .OrderBy(vobj => vobj.TotalArea.Top).ToList();
            var Separation = Math.Abs(SelectedElements[1].TotalArea.Top - SelectedElements[0].TotalArea.Bottom);

            var PreviousPos = SelectedElements[1].TotalArea.Bottom;  // Calculated locally because visual update is "lazy".
            foreach (var Selection in SelectedElements.Skip(2))
            {
                var NewPos = PreviousPos + Separation + (Selection.TotalArea.Height / 2.0);
                Selection.MoveTo(Selection.BaseCenter.X, NewPos, true);
                PreviousPos += Selection.TotalArea.Height + Separation;
            }
        }

        public void CommandSameSeparationDistanceBoth_Execution(object Parameter)
        {
            var Eng = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;
            if (Eng == null || Eng.CurrentView == null || Eng.CurrentView.SelectedObjects.Count < 3)
                return;

            //T Console.WriteLine("Aligning to Same Separation Distance Both...");
            Eng.StartCommandVariation("Separate Same Horizontal and Vertical Distances");
            ApplySeparateSameHorizontalDistance(Eng);
            ApplySeparateSameVerticalDistance(Eng);
            Eng.CompleteCommandVariation();
        }

        public void CommandBringToFront_Execution(object Parameter)
        {
            var Eng = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;
            if (Eng == null || Eng.CurrentView == null || !Eng.CurrentView.SelectedObjects.Any())
                return;

            //T Console.WriteLine("Bringing to Front...");
            Eng.StartCommandVariation("Bring to Front");

            var SelectedObjects = Eng.CurrentView.SelectedObjects.OrderBy(visobj => visobj.ZOrder * -1);
            foreach (var Selection in SelectedObjects)
                Eng.CurrentView.SendUpwards(Selection, true);

            Eng.CompleteCommandVariation();
        }

        public void CommandSendToBack_Execution(object Parameter)
        {
            var Eng = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;
            if (Eng == null || Eng.CurrentView == null || !Eng.CurrentView.SelectedObjects.Any())
                return;

            //T Console.WriteLine("Sending to Back...");
            Eng.StartCommandVariation("Send to Back");

            var SelectedObjects = Eng.CurrentView.SelectedObjects.OrderBy(visobj => visobj.ZOrder);
            foreach (var Selection in SelectedObjects)
                Eng.CurrentView.SendBackwards(Selection, true);

            Eng.CompleteCommandVariation();
        }

        public void CommandBringForward_Execution(object Parameter)
        {
            var Eng = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;
            if (Eng == null || Eng.CurrentView == null || !Eng.CurrentView.SelectedObjects.Any())
                return;

            //T Console.WriteLine("Bringing Forwards...");
            Eng.StartCommandVariation("Bring Forward");

            var SelectedObjects = Eng.CurrentView.SelectedObjects.OrderBy(visobj => visobj.ZOrder * -1);
            foreach (var Selection in SelectedObjects)
                Eng.CurrentView.SendUpwards(Selection);

            Eng.CompleteCommandVariation();
        }

        public void CommandSendBackward_Execution(object Parameter)
        {
            var Eng = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;
            if (Eng == null || Eng.CurrentView == null || !Eng.CurrentView.SelectedObjects.Any())
                return;

            //T Console.WriteLine("Sending Backwards...");
            Eng.StartCommandVariation("Send Backward");

            var SelectedObjects = Eng.CurrentView.SelectedObjects.OrderBy(visobj => visobj.ZOrder);
            foreach (var Selection in SelectedObjects)
                Eng.CurrentView.SendBackwards(Selection);

            Eng.CompleteCommandVariation();
        }

        public void CommandFlipHorizontally_Execution(object Parameter)
        {
            var Eng = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;
            if (Eng == null || Eng.CurrentView == null || !Eng.CurrentView.SelectedRepresentations.Any())
                return;

            //T Console.WriteLine("Flipping Horizontally...");
            Eng.StartCommandVariation("Flip Horizontally");
            ApplyFlipHorizontally(Eng);
            Eng.CurrentView.UpdateVersion();
            Eng.CompleteCommandVariation();
        }

        public void CommandFlipVertically_Execution(object Parameter)
        {
            var Eng = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;
            if (Eng == null || Eng.CurrentView == null || !Eng.CurrentView.SelectedRepresentations.Any())
                return;

            //T Console.WriteLine("Flipping Vertically...");
            Eng.StartCommandVariation("Flip Vertically");
            ApplyFlipVertically(Eng);
            Eng.CurrentView.UpdateVersion();
            Eng.CompleteCommandVariation();
        }

        public void ApplyFlipHorizontally(CompositionEngine Eng)
        {
            var SelectedObjects = Eng.CurrentView.SelectedRepresentations.OrderBy(vrep => vrep.MainSymbol.ZOrder);
            foreach (var Selection in SelectedObjects)
            {
                Selection.MainSymbol.IsHorizontallyFlipped = !Selection.MainSymbol.IsHorizontallyFlipped.IsTrue();
                Selection.MainSymbol.RenderElement();
            }
        }
        public void ApplyFlipVertically(CompositionEngine Eng)
        {
            var SelectedObjects = Eng.CurrentView.SelectedRepresentations.OrderBy(vrep => vrep.MainSymbol.ZOrder);
            foreach (var Selection in SelectedObjects)
            {
                Selection.MainSymbol.IsVerticallyFlipped = !Selection.MainSymbol.IsVerticallyFlipped.IsTrue();
                Selection.MainSymbol.RenderElement();
            }
        }

        public void CommandTilt_Execution(object Parameter)
        {
            var Eng = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;
            if (Eng == null || Eng.CurrentView == null || !Eng.CurrentView.SelectedRepresentations.Any())
                return;

            Eng.StartCommandVariation("Tilt");
            var SelectedObjects = Eng.CurrentView.SelectedRepresentations.OrderBy(vrep => vrep.MainSymbol.ZOrder);
            foreach (var Selection in SelectedObjects)
            {
                Selection.MainSymbol.IsTilted = !Selection.MainSymbol.IsTilted.IsTrue();
                Selection.MainSymbol.RenderElement();
            }
            Eng.CurrentView.UpdateVersion();
            Eng.CompleteCommandVariation();
        }

        public void CommandShowAsMultiple_Execution(object Parameter)
        {
            var Eng = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;
            if (Eng == null || Eng.CurrentView == null || !Eng.CurrentView.SelectedRepresentations.Any())
                return;

            Eng.StartCommandVariation("Show as Multiple");
            var SelectedObjects = Eng.CurrentView.SelectedRepresentations.OrderBy(vrep => vrep.MainSymbol.ZOrder);
            foreach (var Selection in SelectedObjects)
            {
                Selection.MainSymbol.ShowAsMultiple = !Selection.MainSymbol.ShowAsMultiple.IsTrue();
                Selection.MainSymbol.RenderElement();
            }
            Eng.CurrentView.UpdateVersion();
            Eng.CompleteCommandVariation();
        }

        public bool CommandChangeGraphicStyle_IsEnabled(object Parameter)
        {
            var Eng = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;
            return (Eng != null && Eng.CurrentView != null && Eng.CurrentView.SelectedObjects.Any());
        }

        public void CommandChangeGraphicStyle_Execution(object Parameter)
        {
            // Contains: line-foreground, line-thickness, line-dash-style, shape-background
            var Style = Parameter as Tuple<Brush, double, DashStyle, Brush>;

            var Eng = (CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine;

            if (Style == null || Eng == null || Eng.CurrentView == null
                || !Eng.CurrentView.SelectedObjects.Any())
                return;

            // Console.WriteLine("Change Graphic Style...");

            Eng.StartCommandVariation("Change Graphic Style");

            foreach (var Selection in Eng.CurrentView.SelectedObjects)
            {
                var Element = Selection as VisualElement;

                if (Element != null)
                {
                    VisualElementFormat.SetLineBrush(Element, Style.Item1);
                    VisualElementFormat.SetLineThickness(Element, Style.Item2);
                    VisualElementFormat.SetLineDash(Element, Style.Item3);
                    VisualElementFormat.SetMainBackground(Element, Style.Item4);

                    Element.RenderElement();

                    var SelectedConnectors = (Element.OwnerRepresentation is RelationshipVisualRepresentation
                                              ? ((RelationshipVisualRepresentation)Element.OwnerRepresentation).VisualConnectors : null);

                    if (SelectedConnectors != null)
                        foreach (var Connector in SelectedConnectors)
                        {
                            // When style foreground has no-thickness, then use what is visible to user: the background brush.
                            VisualConnectorsFormat.SetLineBrush(Connector, (Style.Item2 > 0 ? Style.Item1 : Style.Item4));

                            if (Style.Item2 > 0)    // Omit no-thickness to avoid invisibility
                                VisualConnectorsFormat.SetLineThickness(Connector, Style.Item2);

                            VisualConnectorsFormat.SetLineDash(Connector, Style.Item3);

                            VisualConnectorsFormat.SetMainBackground(Connector, Style.Item4);

                            Connector.RenderElement();
                        }

                    Element.OwnerRepresentation.RepresentedIdea.NotifyPropertyChange("RepresentativePicture");
                }
                else
                {
                    var Complement = Selection as VisualComplement;
                    if (Complement.CanSetColors)
                    {
                        Complement.SetPropertyField(VisualComplement.PROP_FIELD_FOREGROUND, Style.Item1);
                        Complement.SetPropertyField(VisualComplement.PROP_FIELD_LINETHICK, Style.Item2);
                        Complement.SetPropertyField(VisualComplement.PROP_FIELD_LINEDASH, Style.Item3);
                        Complement.SetPropertyField(VisualComplement.PROP_FIELD_BACKGROUND, Style.Item4);
                        Complement.Render();
                    }
                }
            }

            /*
            var TextPurposes = Enum.GetValues(typeof(ETextPurpose));
            foreach(var TextPurpose in TextPurposes)
                VisualSymbolFormat.SetTextFormat(Selection.MainSymbol, (ETextPurpose)TextPurpose,
                    VisualSymbolFormat.GetTextFormat(Eng.CurrentVisRepSelectedAsFormatSource.MainSymbol, (ETextPurpose)TextPurpose));
            */

            Eng.CurrentView.UpdateVersion();
            Eng.CompleteCommandVariation();
        }
    }
}