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
// File   : PathDrawer.cs
// Object : Instrumind.ThinkComposer.Definitor.DefinitorUI.PathDrawer (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.09.04 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;

/// Provides user-interface common services, plus the components for the Domain related Definitions.
namespace Instrumind.ThinkComposer.Definitor.DefinitorUI
{
    /// <summary>
    /// Provides services for drawing connector plugs.
    /// </summary>
    public static class PlugDrawer
    {
        /// <summary>
        /// Prefix for the defined resource Plugs.
        /// </summary>
        public const string RES_PFX_PLUGS = "PLG_";

        /// <summary>
        /// Standard square size for the Plug.
        /// </summary>
        public const double STD_PLUG_SQR_SIZE = 21;

        /// <summary>
        /// Standard size for the Plugs.
        /// </summary>
        public static readonly Size STD_PLUG_SIZE = new Size(STD_PLUG_SQR_SIZE, STD_PLUG_SQR_SIZE);

        /// <summary>
        /// Standard coordinate for the Plug target/destination point.
        /// </summary>
        public static readonly Point STD_PLUG_TARGET = new Point(0, 10);

        /// <summary>
        /// Standard coordinate for the Plug source/origin point.
        /// </summary>
        public static readonly Point STD_PLUG_SOURCE = new Point(20, 10);

        public const string GEOM_ClosedArrow = "ClosedArrow";
        public const string GEOM_DoubleClosedArrow = "DoubleClosedArrow";
        public const string GEOM_OpenArrow = "OpenArrow";
        public const string GEOM_DoubleOpenArrow = "DoubleOpenArrow";
        public const string GEOM_ElectroWave = "ElectroWave";
        public const string GEOM_Circle = "Circle";
        public const string GEOM_Rhomb = "Rhomb";
        public const string GEOM_LineDash = "LineDash";
        public const string GEOM_LineDoubleDash = "LineDoubleDash";
        public const string GEOM_TrilineCircle = "TrilineCircle";
        public const string GEOM_TrilineDash = "TrilineDash";
        public const string GEOM_CircleClosedArrow = "CircleClosedArrow";
        public const string GEOM_CircleOpenArrow = "CircleOpenArrow";
        public const string GEOM_LineX = "LineX";
        public const string GEOM_PointerArrow = "PointerArrow";
        public const string GEOM_Chevron = "Chevron";
        public const string GEOM_PlumbBob = "PlumbBob";
        public const string GEOM_CircleDash = "CircleDash";
        public const string GEOM_CirclePlus = "CirclePlus";
        public const string GEOM_CircleMinus = "CircleMinus";
        public const string GEOM_CircleAsterisk = "CircleAsterisk";

        /// <summary>
        /// Stores the default brush used to paint empty plugs.
        /// </summary>
        public static Brush DefaultPlugEmptyBrush = Brushes.White;

        /// <summary>
        /// Defines the required Geometries and fill flags to be used for making connector Plugs.
        /// </summary>
        public static readonly Dictionary<string, Tuple<string, bool>> PlugDefinitors =
            new Dictionary<string, Tuple<string, bool>>()
            { { Plugs.None, null },
              { Plugs.FilledArrow, Tuple.Create<string, bool>(GEOM_ClosedArrow, true) },
              { Plugs.DoubleFilledArrow, Tuple.Create<string, bool>(GEOM_DoubleClosedArrow, true) },
              { Plugs.EmptyArrow, Tuple.Create<string, bool>(GEOM_ClosedArrow, false) },
              { Plugs.DoubleEmptyArrow, Tuple.Create<string, bool>(GEOM_DoubleClosedArrow, false) },
              { Plugs.SimpleArrow, Tuple.Create<string, bool>(GEOM_OpenArrow, false) },
              { Plugs.DoubleSimpleArrow, Tuple.Create<string, bool>(GEOM_DoubleOpenArrow, false) },
              // POSTPONED: { Plugs.ElectroWave, Tuple.Create<string, bool>(GEOM_ElectroWave, false) },
              { Plugs.FilledCircle, Tuple.Create<string, bool>(GEOM_Circle, true) },
              { Plugs.EmptyCircle, Tuple.Create<string, bool>(GEOM_Circle, false) },
              { Plugs.FilledRhomb, Tuple.Create<string, bool>(GEOM_Rhomb, true) },
              { Plugs.EmptyRhomb, Tuple.Create<string, bool>(GEOM_Rhomb, false) },
              { Plugs.LineDash, Tuple.Create<string, bool>(GEOM_LineDash, false) },
              { Plugs.LineDoubleDash, Tuple.Create<string, bool>(GEOM_LineDoubleDash, false) },
              { Plugs.TrilineCircle, Tuple.Create<string, bool>(GEOM_TrilineCircle, false) },
              { Plugs.TrilineDash, Tuple.Create<string, bool>(GEOM_TrilineDash, false) },
              { Plugs.FilledCircleArrow, Tuple.Create<string, bool>(GEOM_CircleClosedArrow, true) },
              { Plugs.EmptyCircleArrow, Tuple.Create<string, bool>(GEOM_CircleClosedArrow, false) },
              { Plugs.EmptyCircleSimpleArrow, Tuple.Create<string, bool>(GEOM_CircleOpenArrow, false) },
              { Plugs.LineX, Tuple.Create<string, bool>(GEOM_LineX, false) },
              { Plugs.PointerArrow, Tuple.Create<string, bool>(GEOM_PointerArrow, false) },
              { Plugs.Chevron, Tuple.Create<string, bool>(GEOM_Chevron, false) },
              { Plugs.PlumbBob, Tuple.Create<string, bool>(GEOM_PlumbBob, false) },
              { Plugs.CircleDash, Tuple.Create<string, bool>(GEOM_CircleDash, false) },
              { Plugs.CirclePlus, Tuple.Create<string, bool>(GEOM_CirclePlus, false) },
              { Plugs.CircleMinus, Tuple.Create<string, bool>(GEOM_CircleMinus, false) },
              { Plugs.CircleAsterisk, Tuple.Create<string, bool>(GEOM_CircleAsterisk, false) } };

        /// <summary>
        /// Creates and returns a Plug using the supplied Plug type, Pen, Target (head) Position, Source (tail) Position and Magnitude factor,
        /// plus indication of Inclusion of Outside Content.
        /// Null is returned when the supplied plug type is "None".
        /// </summary>
        public static Drawing CreatePlug(string PlugType, Pen PlugPen, Point TargetPosition, Point SourcePosition,
                                         double Magnitude = 1.0, bool IncludeOutsideContent = false, bool ShowLineWhenEmpty = false)
        {
            Tuple<string, bool> PlugDefinitor = null;
            PlugType = PlugType.NullDefault("");
            //T PlugType = "NonExistent";

            if (!PlugDefinitors.ContainsKey(PlugType))
            {
                Console.WriteLine("Connector plug '" + PlugType.NullDefault("") + "' not recognized. Please update ThinkComposer.");
                PlugType = Plugs.None;
            }

            var PlugBrush = PlugPen.Brush;

            PlugDefinitor = PlugDefinitors[PlugType];
            if (PlugDefinitor == null && !ShowLineWhenEmpty)
                return null;

            if (PlugDefinitor != null && !PlugDefinitor.Item2)
                PlugBrush = DefaultPlugEmptyBrush;

            GeometryGroup PlugGeometry = null;
            
            if (PlugDefinitor == null)
            {
                PlugGeometry = new GeometryGroup();
                PlugGeometry.Children.Add(new LineGeometry(new Point(0,10), new Point(20,10)));
            }
            else
                PlugGeometry = Display.GetResource<GeometryGroup>(RES_PFX_PLUGS + PlugDefinitor.Item1, true);

            double RotationAngle = Display.DeterminePointingAngleTo(SourcePosition, TargetPosition);
            // Console.WriteLine("From [{0}] to [{1}] the Rotation Angle is [{2}]", SourcePosition, TargetPosition, RotationAngle);

            PlugGeometry.Transform = Display.CreateTransform(TargetPosition.X + 1, TargetPosition.Y - (Math.Floor(STD_PLUG_SQR_SIZE / 2.0) * Magnitude),
                                                             Magnitude, Magnitude, RotationAngle, TargetPosition.X, TargetPosition.Y);

            Drawing Result = new GeometryDrawing(PlugBrush, PlugPen, PlugGeometry);

            // Cut outside drawing content
            if (IncludeOutsideContent)
            {
                var Group = new DrawingGroup();
                Group.Children.Add(new GeometryDrawing(Brushes.Transparent, null,
                                                       new RectangleGeometry(new Rect(0, 0, STD_PLUG_SIZE.Width, STD_PLUG_SIZE.Height))));
                Group.Children.Add(Result);
                Result = Group;
            }

            return Result;
        }
    }
}