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
// File   : ShortcutCreationCommand.cs
// Object : Instrumind.ThinkComposer.Composer.ComposerUI.ShortcutCreationCommand (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2011.12.24 Néstor Sánchez A.  Creation
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
    /// Creates a Complement for a given View.
    /// </summary>
    public class ShortcutCreationCommand : WorkCommandInteractive<MouseEventArgs>
    {
        public CompositionEngine ContextEngine { get; protected set; }
        public Idea TargetIdea { get; protected set; }

        public VisualRepresentation NewShortcut { get; protected set; }

        public ShortcutCreationCommand(CompositionEngine TargetEngine, Idea TargetIdea)
            : base("Create Shortcut.")
        {
            this.ContextEngine = TargetEngine;
            this.TargetIdea = TargetIdea;

            this.Initialize();
        }

        Point TargetLocation = Display.NULL_POINT;
        Cursor PreviousCursor = null;

        public override void Initialize()
        {
            base.Initialize();

            var Loader = Display.GetResource<TextBlock>("CursorLoaderArrowShortcut");   // Trick for assign custom cursors.
            this.PreviousCursor = ProductDirector.ContentTreeControl.Cursor;
            ProductDirector.ContentTreeControl.Cursor = Loader.Cursor;

            var ImgSrc = Display.GetAppImage("shortcut_big.png");
            PointingAssistant.Start(this.ContextEngine.CurrentView,
                                    new ImageDrawing(ImgSrc, new Rect(0, 0, ImgSrc.GetWidth(), ImgSrc.GetHeight())));
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
            if (this.ContextEngine.CurrentView.SnapToGrid)
                TargetLocation = this.ContextEngine.CurrentView.GetGridSnappedPosition(TargetLocation, true);

            if (IsDefinitive)
            {
                this.ContextEngine.StartCommandVariation("Create Shortcut");
                var VisRepShortcut = this.ContextEngine.AppendShortcut(this.ContextEngine.CurrentView, TargetIdea, TargetLocation);

                if (VisRepShortcut == null)
                {
                    this.ContextEngine.DiscardCommandVariation();
                    return false;
                }

                this.ContextEngine.CurrentView.SelectObject(VisRepShortcut.MainSymbol);
                VisRepShortcut.Render();

                this.ContextEngine.CurrentView.UnselectAllObjects();
                this.ContextEngine.CurrentView.Manipulator.ApplySelection(VisRepShortcut.MainSymbol);

                this.ContextEngine.CurrentView.UpdateVersion();
                this.ContextEngine.CompleteCommandVariation();

                /*? this.ContextEngine.CurrentView.Presenter
                    .PostCall(pres => VisualComplement.Edit(CreationResult.Result, true)); */
            }
            
            // // Stop the command...
            // this.Terminate(true, Parameter);
            // return false;

            // Continue the command...
            return true;
        }

        public override void Terminate(bool IsNormalTermination = false, MouseEventArgs Parameter = null)
        {
            base.Terminate(IsNormalTermination, Parameter);
            PointingAssistant.Finish();

            ProductDirector.ContentTreeControl.Cursor = this.PreviousCursor;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}
