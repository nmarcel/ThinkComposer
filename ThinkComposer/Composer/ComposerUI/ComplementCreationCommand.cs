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
// File   : ComplementCreationCommand.cs
// Object : Instrumind.ThinkComposer.Composer.ComposerUI.ComplementCreationCommand (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.09.29 Néstor Sánchez A.  Creation
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
    public class ComplementCreationCommand : WorkCommandInteractive<MouseEventArgs>
    {
        public CompositionEngine ContextEngine { get; protected set; }
        public SimplePresentationElement ComplementKind { get; protected set; }

        public VisualComplement NewComplement { get; protected set; }

        public ComplementCreationCommand(CompositionEngine TargetEngine, SimplePresentationElement ComplementKind)
            : base("Create Complement '" + ComplementKind.Name + "'.")
        {
            this.ContextEngine = TargetEngine;
            this.ComplementKind = ComplementKind;

            this.Initialize();
        }

        VisualSymbol TargetSymbol = null;
        Point TargetLocation = Display.NULL_POINT;

        public override void Initialize()
        {
            base.Initialize();

            var Loader = Display.GetResource<TextBlock>("CursorLoaderArrowComplement");   // Trick for assign custom cursors.
            Display.GetCurrentWindow().Cursor = Loader.Cursor;

            PointingAssistant.Start(this.ContextEngine.CurrentView,
                                    new ImageDrawing(this.ComplementKind.Pictogram,
                                                     new Rect(0, 0, this.ComplementKind.Pictogram.GetWidth(), this.ComplementKind.Pictogram.GetHeight())));
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

            TargetSymbol = this.ContextEngine.GetPointedVisualObject(TargetLocation) as VisualSymbol;

            var InitialWidth = 0.0;

            if (IsDefinitive)
            {
                var CreationResult = new OperationResult<VisualComplement>();

                if (this.ComplementKind.TechName == Domain.ComplementDefCallout.TechName
                    || this.ComplementKind.TechName == Domain.ComplementDefQuote.TechName)
                {
                    if (TargetSymbol == null)
                    {
                        Console.WriteLine(this.ComplementKind.Name + " must point to a Symbol");
                        return true;
                    }

                    if (this.ComplementKind.TechName == Domain.ComplementDefCallout.TechName)
                        TargetLocation = new Point(TargetSymbol.BaseCenter.X + 20 + VisualComplement.CALLOUT_INI_WIDTH / 2.0,
                                                   TargetSymbol.BaseTop - 20 - VisualComplement.CALLOUT_INI_HEIGHT / 2.0);
                    else
                        TargetLocation = new Point(TargetSymbol.BaseCenter.X - 25 - VisualComplement.QUOTE_INI_WIDTH / 2.0,
                                                   TargetSymbol.BaseTop - 15 - VisualComplement.QUOTE_INI_HEIGHT / 2.0);

                    if (this.ContextEngine.CurrentView.SnapToGrid)
                        TargetLocation = this.ContextEngine.CurrentView.GetGridSnappedPosition(TargetLocation, true);
                }

                if (this.ComplementKind.TechName == Domain.ComplementDefGroupRegion.TechName
                    || this.ComplementKind.TechName == Domain.ComplementDefGroupLine.TechName)
                {
                    if (TargetSymbol == null)
                    {
                        Console.WriteLine(this.ComplementKind.Name + " must belong to a Symbol");
                        return true;
                    }

                    /*-
                    if (this.ComplementKind.TechName == Domain.ComplementDefGroupRegion.TechName
                        && TargetSymbol.AttachedComplements.Count(comp => comp.IsComplementGroupRegion) > 0)
                    {
                        Console.WriteLine("Symbol cannot have more than one Group Region.");
                        return true;
                    } */

                    if (this.ComplementKind.TechName == Domain.ComplementDefGroupLine.TechName
                        && TargetSymbol.AttachedComplements.Count(comp => comp.IsComplementGroupLine) > 1)
                    {
                        Console.WriteLine("Symbol cannot have more than two Group Lines.");
                        return true;
                    }

                    if (this.ComplementKind.TechName == Domain.ComplementDefGroupRegion.TechName)
                    {
                        var Params = VisualComplement.GetGroupRegionInitialParams(TargetSymbol);
                        TargetLocation = Params.Item1;
                        InitialWidth = Params.Item2;
                    }
                    else
                        TargetLocation = VisualComplement.GetGroupLineInitialPosition(TargetSymbol);
                }

                CreationResult = CreateComplement(this.ContextEngine.CurrentView.VisualizedCompositeIdea, this.ComplementKind, this.ContextEngine.CurrentView, TargetSymbol as VisualSymbol, TargetLocation, InitialWidth);

                if (CreationResult.WasSuccessful)
                {
                    this.ContextEngine.CurrentView.Manipulator.ApplySelection(CreationResult.Result);

                    this.ContextEngine.CurrentView.Presenter
                        .PostCall(pres => VisualComplement.Edit(CreationResult.Result, true));

                    if (this.ComplementKind.TechName == Domain.ComplementDefImage.TechName)
                        return false;
                }
                else
                {
                    Console.WriteLine("Cannot create Complement: " + CreationResult.Message.AbsentDefault("?"));
                    this.Terminate();
                    return false;
                }

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

            Display.GetCurrentWindow().Cursor = Cursors.Arrow;

            ProductDirector.ComplementPaletteControl.ClearSelection();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public static OperationResult<VisualComplement> CreateComplement(Idea DestinationComposite, SimplePresentationElement Kind, View TargetView,
                                                                         VisualSymbol TargetSymbol, Point Position, double InitialWidth = 0.0)
        {
            General.ContractRequiresNotNull(DestinationComposite, Kind, TargetView);

            if (DestinationComposite.IdeaDefinitor.CompositeContentDomain == null)
                return OperationResult.Failure<VisualComplement>("Destination Container doest not accept Composite-Content.", DestinationComposite);

            DestinationComposite.EditEngine.StartCommandVariation("Create Complement");

            VisualComplement NewComplement = null;

            var Owner = (Kind.TechName.IsOneOf(Domain.ComplementDefCallout.TechName, Domain.ComplementDefQuote.TechName,
                                               Domain.ComplementDefGroupRegion.TechName, Domain.ComplementDefGroupLine.TechName)
                         ? Ownership.Create<View, VisualSymbol>(TargetSymbol)
                         : Ownership.Create<View, VisualSymbol>(TargetView));

            NewComplement = new VisualComplement(Kind, Owner, Position, InitialWidth);

            if (!Owner.IsGlobal && TargetSymbol != null)
                TargetSymbol.AddComplement(NewComplement);

            TargetView.PutComplement(NewComplement);

            DestinationComposite.UpdateVersion();
            DestinationComposite.EditEngine.CompleteCommandVariation();

            return OperationResult.Success(NewComplement);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}
