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
// File   : Plugs.cs
// Object : Instrumind.ThinkComposer.MetaModel.VisualMetaModel.Plugs (Enum)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.08.31 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.Definitor.DefinitorUI;

/// Base abstractions for the user metadefinition of Visual schemas
namespace Instrumind.ThinkComposer.MetaModel.VisualMetaModel
{
    /// <summary>
    /// Codes for predefined connector shapes (start/end points between lines and symbols).
    /// </summary>
    public static class Plugs
    {
        public const string None = "None";
        public const string FilledArrow = "FilledArrow";
        public const string DoubleFilledArrow = "DoubleFilledArrow";
        public const string EmptyArrow = "EmptyArrow";
        public const string DoubleEmptyArrow = "DoubleEmptyArrow";
        public const string SimpleArrow = "SimpleArrow";
        public const string DoubleSimpleArrow = "DoubleSimpleArrow";
        public const string ElectroWave = "ElectroWave";
        public const string FilledCircle = "FilledCircle";
        public const string EmptyCircle = "EmptyCircle";
        public const string FilledRhomb = "FilledRhomb";
        public const string EmptyRhomb = "EmptyRhomb";
        public const string LineDash = "LineDash";
        public const string LineDoubleDash = "LineDoubleDash";
        public const string TrilineCircle = "TrilineCircle";
        public const string TrilineDash = "TrilineDash";
        public const string FilledCircleArrow = "FilledCircleArrow";
        public const string EmptyCircleArrow = "EmptyCircleArrow";
        public const string EmptyCircleSimpleArrow = "EmptyCircleSimpleArrow";
        public const string LineX = "LineX";
        public const string PointerArrow = "PointerArroow";
        public const string Chevron = "Chevron";
        public const string PlumbBob = "PlumbBob";
        public const string CircleDash = "CircleDash";
        public const string CirclePlus = "CirclePlus";
        public const string CircleMinus = "CircleMinus";
        public const string CircleAsterisk = "CircleAsterisk";

        public static readonly List<SimplePresentationElement> PredefinedPlugs = new List<SimplePresentationElement>();

        static Plugs()
        {
            var PlugPen = new Pen(Brushes.Black, 1.0);
            var PlugTarget = new Point(0.0, 8.0);
            var PlugSource = new Point(32.0, 8.0);

            PredefinedPlugs.Add(new SimplePresentationElement("<None>", None, "", PlugDrawer.CreatePlug(None, PlugPen, PlugTarget, PlugSource, 1.0, false).ToDrawingImage()));
            PredefinedPlugs.Add(new SimplePresentationElement("Filled-Arrow", FilledArrow, "", PlugDrawer.CreatePlug(FilledArrow, PlugPen, PlugTarget, PlugSource, 1.0, false).ToDrawingImage()));
            PredefinedPlugs.Add(new SimplePresentationElement("Double-Filled-Arrow", DoubleFilledArrow, "", PlugDrawer.CreatePlug(DoubleFilledArrow, PlugPen, PlugTarget, PlugSource, 1.0, false).ToDrawingImage()));
            PredefinedPlugs.Add(new SimplePresentationElement("Empty-Arrow", EmptyArrow, "", PlugDrawer.CreatePlug(EmptyArrow, PlugPen, PlugTarget, PlugSource, 1.0, false).ToDrawingImage()));
            PredefinedPlugs.Add(new SimplePresentationElement("Double-Empty-Arrow", DoubleEmptyArrow, "", PlugDrawer.CreatePlug(DoubleEmptyArrow, PlugPen, PlugTarget, PlugSource, 1.0, false).ToDrawingImage()));
            PredefinedPlugs.Add(new SimplePresentationElement("Simple-Arrow", SimpleArrow, "", PlugDrawer.CreatePlug(SimpleArrow, PlugPen, PlugTarget, PlugSource, 1.0, false).ToDrawingImage()));
            PredefinedPlugs.Add(new SimplePresentationElement("Double-Simple-Arrow", DoubleSimpleArrow, "", PlugDrawer.CreatePlug(DoubleSimpleArrow, PlugPen, PlugTarget, PlugSource, 1.0, false).ToDrawingImage()));
            // POSTPONED: PredefinedPlugs.Add(new SimplePresentationElement("Electro-Wave", ElectroWave, "", PlugDrawer.CreatePlug(ElectroWave, PlugPen, PlugTarget, PlugSource, 1.0, false).ToDrawingImage()));
            PredefinedPlugs.Add(new SimplePresentationElement("Filled-Circle", FilledCircle, "", PlugDrawer.CreatePlug(FilledCircle, PlugPen, PlugTarget, PlugSource, 1.0, false).ToDrawingImage()));
            PredefinedPlugs.Add(new SimplePresentationElement("Empty-Circle", EmptyCircle, "", PlugDrawer.CreatePlug(EmptyCircle, PlugPen, PlugTarget, PlugSource, 1.0, false).ToDrawingImage()));
            PredefinedPlugs.Add(new SimplePresentationElement("Filled-Rhomb", FilledRhomb, "", PlugDrawer.CreatePlug(FilledRhomb, PlugPen, PlugTarget, PlugSource, 1.0, false).ToDrawingImage()));
            PredefinedPlugs.Add(new SimplePresentationElement("Empty-Rhomb", EmptyRhomb, "", PlugDrawer.CreatePlug(EmptyRhomb, PlugPen, PlugTarget, PlugSource, 1.0, false).ToDrawingImage()));
            PredefinedPlugs.Add(new SimplePresentationElement("Line-Dash", LineDash, "", PlugDrawer.CreatePlug(LineDash, PlugPen, PlugTarget, PlugSource, 1.0, false).ToDrawingImage()));
            PredefinedPlugs.Add(new SimplePresentationElement("Line-Double-Dash", LineDoubleDash, "", PlugDrawer.CreatePlug(LineDoubleDash, PlugPen, PlugTarget, PlugSource, 1.0, false).ToDrawingImage()));
            PredefinedPlugs.Add(new SimplePresentationElement("Triline-Circle", TrilineCircle, "", PlugDrawer.CreatePlug(TrilineCircle, PlugPen, PlugTarget, PlugSource, 1.0, false).ToDrawingImage()));
            PredefinedPlugs.Add(new SimplePresentationElement("Triline-Dash", TrilineDash, "", PlugDrawer.CreatePlug(TrilineDash, PlugPen, PlugTarget, PlugSource, 1.0, false).ToDrawingImage()));
            PredefinedPlugs.Add(new SimplePresentationElement("Filled-Circle-Arrow", FilledCircleArrow, "", PlugDrawer.CreatePlug(FilledCircleArrow, PlugPen, PlugTarget, PlugSource, 1.0, false).ToDrawingImage()));
            PredefinedPlugs.Add(new SimplePresentationElement("Empty-Circle-Arrow", EmptyCircleArrow, "", PlugDrawer.CreatePlug(EmptyCircleArrow, PlugPen, PlugTarget, PlugSource, 1.0, false).ToDrawingImage()));
            PredefinedPlugs.Add(new SimplePresentationElement("Empty-Circle-Simple-Arrow", EmptyCircleSimpleArrow, "", PlugDrawer.CreatePlug(EmptyCircleSimpleArrow, PlugPen, PlugTarget, PlugSource, 1.0, false).ToDrawingImage()));
            PredefinedPlugs.Add(new SimplePresentationElement("Line-X", LineX, "", PlugDrawer.CreatePlug(LineX, PlugPen, PlugTarget, PlugSource, 1.0, false).ToDrawingImage()));
            PredefinedPlugs.Add(new SimplePresentationElement("Pointer-Arrow", PointerArrow, "", PlugDrawer.CreatePlug(PointerArrow, PlugPen, PlugTarget, PlugSource, 1.0, false).ToDrawingImage()));
            PredefinedPlugs.Add(new SimplePresentationElement("Chevron", Chevron, "", PlugDrawer.CreatePlug(Chevron, PlugPen, PlugTarget, PlugSource, 1.0, false).ToDrawingImage()));
            PredefinedPlugs.Add(new SimplePresentationElement("Plumb-Bob", PlumbBob, "", PlugDrawer.CreatePlug(PlumbBob, PlugPen, PlugTarget, PlugSource, 1.0, false).ToDrawingImage()));
            PredefinedPlugs.Add(new SimplePresentationElement("Circle-Dash", CircleDash, "", PlugDrawer.CreatePlug(CircleDash, PlugPen, PlugTarget, PlugSource, 1.0, false).ToDrawingImage()));
            PredefinedPlugs.Add(new SimplePresentationElement("Circle-Plus", CirclePlus, "", PlugDrawer.CreatePlug(CirclePlus, PlugPen, PlugTarget, PlugSource, 1.0, false).ToDrawingImage()));
            PredefinedPlugs.Add(new SimplePresentationElement("Circle-Minus", CircleMinus, "", PlugDrawer.CreatePlug(CircleMinus, PlugPen, PlugTarget, PlugSource, 1.0, false).ToDrawingImage()));
            PredefinedPlugs.Add(new SimplePresentationElement("Circle-Asterisk", CircleAsterisk, "", PlugDrawer.CreatePlug(CircleAsterisk, PlugPen, PlugTarget, PlugSource, 1.0, false).ToDrawingImage()));
        }
    }
}
