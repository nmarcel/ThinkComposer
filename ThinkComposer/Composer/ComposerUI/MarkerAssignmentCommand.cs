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
// File   : MarkerAssignmentCommand.cs
// Object : Instrumind.ThinkComposer.Composer.ComposerUI.MarkerAssignmentCommand (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2010.12.21 Néstor Sánchez A.  Creation
//

using System;
using System.Collections;
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

using Instrumind.ThinkComposer;
using Instrumind.ThinkComposer.ApplicationProduct;
using Instrumind.ThinkComposer.Composer.ComposerUI;
using Instrumind.ThinkComposer.MetaModel;
using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;
using Instrumind.ThinkComposer.Model;
using Instrumind.ThinkComposer.Model.GraphModel;
using Instrumind.ThinkComposer.Model.VisualModel;

/// Provides the user-interface components for the Composition Composer.
namespace Instrumind.ThinkComposer.Composer.ComposerUI
{
    /// <summary>
    /// Creates a Concept, with its visual representation, for a given composite Idea.
    /// </summary>
    public class MarkerAssignmentCommand : WorkCommandInteractive<MouseEventArgs>
    {
        public CompositionEngine ContextEngine { get; protected set; }
        public MarkerDefinition MarkerDef { get; protected set; }

        public MarkerAssignment NewMarker { get; protected set; }

        public MarkerAssignmentCommand(CompositionEngine TargetEngine, MarkerDefinition MarkerDef)
            : base("Assign Marker: " + MarkerDef.Name)
        {
            this.ContextEngine = TargetEngine;
            this.MarkerDef = MarkerDef;

            this.Initialize();
        }

        VisualRepresentation TargetRepresentation = null;
        Point TargetLocation = Display.NULL_POINT;

        public override void Initialize()
        {
            base.Initialize();

            var Loader = Display.GetResource<TextBlock>("CursorLoaderArrowMarker");   // Trick for assign custom cursors.
            Display.GetCurrentWindow().Cursor = Loader.Cursor;

            PointingAssistant.Start(this.ContextEngine.CurrentView, new ImageDrawing(this.MarkerDef.Pictogram, new Rect(MarkerDefinition.StandardMarkerIconSize)));
        }

        public override void Execute(MouseEventArgs Parameter = null)
        {
            base.Execute(Parameter);
        }

        public override bool Continue(MouseEventArgs Parameter, bool IsDefinitive = true)
        {
            var Go = base.Continue(Parameter, IsDefinitive);
            if (!Go)
                return false;

            TargetLocation = Parameter.GetPosition(this.ContextEngine.CurrentView.PresenterControl);
            TargetRepresentation = this.ContextEngine.GetPointedRepresentation(TargetLocation, false);

            if (IsDefinitive && TargetRepresentation != null)
            {
                var CreationResult = CreateMarker(this.ContextEngine, this.MarkerDef, this.ContextEngine.CurrentView, TargetRepresentation);

                if (CreationResult.WasSuccessful)
                {
                    var CapturedTargetRep = TargetRepresentation;  // Needed because TargetRepresentation is overwritten by fast mouse movement

                    this.ContextEngine.CurrentView.Presenter
                        .PostCall(pres =>   // Post-called in order to be executed after the symbol heading-content area is established (on first draw).
                                    {
                                        pres.OwnerView.Manipulator.ApplySelection(CapturedTargetRep.MainSymbol);
                                    });
                }
                else
                {
                    Console.WriteLine("Cannot create Marker: " + CreationResult.Message.AbsentDefault("?"));
                    this.Terminate();
                    return false;
                }
            }
            
            /* Stop the command...
            this.Terminate(true, Parameter);
            return false;
            */

            // Continue the command...
            return true;
        }

        public override void Terminate(bool IsNormalTermination = false, MouseEventArgs Parameter = null)
        {
            base.Terminate(IsNormalTermination, Parameter);
            PointingAssistant.Finish();

            Display.GetCurrentWindow().Cursor = Cursors.Arrow;

            ProductDirector.MarkerPaletteControl.ClearSelection();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Creates and assign a Marker to either the explicitly specified Representator or the currently selected ones.
        /// </summary>
        public static OperationResult<MarkerAssignment>
                      CreateMarker(EntityEditEngine Engine, MarkerDefinition Definitor, View TargetView, VisualRepresentation TargetRepresentator = null)
        {
            General.ContractRequiresNotNull(Engine, Definitor, TargetView);

            Engine.StartCommandVariation("Create Marker");

            var NewMarker = CreateMarkerAssignment(Engine, Definitor);

            if (NewMarker != null)
            {
                var RedisplayView = false;

                if (!TargetView.ShowMarkers)
                    /* if (Display.DialogMessage("Attention", "View is not currently showing Markers.\nDo you want to show them?",
                                              EMessageType.Question, MessageBoxButton.YesNo, MessageBoxResult.Yes) == MessageBoxResult.Yes) */
                    {
                        TargetView.ShowMarkers = true;
                        RedisplayView = true;
                    }

                if (TargetRepresentator == null)
                    foreach (var Representator in TargetView.SelectedRepresentations)
                    {
                        Representator.RepresentedIdea.Markings.Add(NewMarker);
                        if (!RedisplayView)
                            Representator.Render();
                    }
                else
                {
                    TargetRepresentator.RepresentedIdea.Markings.Add(NewMarker);
                    if (!RedisplayView)
                        TargetRepresentator.Render();
                }

                if (RedisplayView)
                    TargetView.ShowAll();
            }

            if (TargetRepresentator != null)
                TargetRepresentator.RepresentedIdea.UpdateVersion();

            Engine.CompleteCommandVariation();

            return OperationResult.Success(NewMarker);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public static MarkerAssignment CreateMarkerAssignment(EntityEditEngine Engine, MarkerDefinition Definitor)
        {
            var Result = new MarkerAssignment(Engine, Definitor);

            var EditOnCreationConfigured = AppExec.GetConfiguration<bool>("IdeaEditing", "MarkerEditing.EditOnCreation", false);
            var EditOnCreationExplicit = (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl));
            
            if (EditOnCreationConfigured || EditOnCreationExplicit)
                if (!EditMarkerAssignmentDescriptor(Engine, Result))
                    return null;

            return Result;
        }

        public static bool EditMarkerAssignmentDescriptor(EntityEditEngine Engine, MarkerAssignment Target)
        {
            var Descriptor = Target.Descriptor.NullDefault(new SimplePresentationElement("", ""));
            var EditResult = ((CompositionEngine)Engine).EditSimplePresentationElement(Descriptor, "Edit '" + Target.Definitor.Name + "' marker");

            if (EditResult)
                // NOTE: In the future, this should also attach the descriptor if any new editable property is populated
                if (Descriptor.AutoPopulate() || Descriptor.Pictogram != null)
                    Target.Descriptor = Descriptor;
                else
                    Target.Descriptor = null;

            // Indicate applied or cancelled
            return EditResult;
        }

    }
}
