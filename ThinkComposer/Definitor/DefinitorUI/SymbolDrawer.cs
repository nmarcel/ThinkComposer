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
// Object : Instrumind.ThinkComposer.Definitor.DefinitorUI.SymbolDrawer (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.09.04 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;

/// Provides user-interface common services, plus the components for the Domain related Definitions.
namespace Instrumind.ThinkComposer.Definitor.DefinitorUI
{
    /// <summary>
    /// Provides services for drawing symbols.
    /// </summary>
    public static class SymbolDrawer
    {
        /// <summary>
        /// Prefix for the defined resource Symbols.
        /// </summary>
        public const string RES_PFX_SYMBOLS = "SYM_";

        /// <summary>
        /// Prefix for the defined resource Decorators.
        /// </summary>
        public const string RES_PFX_DECORATORS = "DEC_";

        /// <summary>
        /// Creates and returns a Symbol and its Content Area using the supplied Shape type, Pen, Brush, Center Position, Size, As-Multiple and Flip factors.
        /// Returns the generated drawing plus content-rectangle, or null when the supplied shape type is not found.
        /// </summary>
        public static Tuple<Drawing, Rect> CreateSymbol(string ShapeType, Pen SymbolPen, Brush SymbolBrush, Point CenterPosition,
                                                        double Width = double.NaN, double Height = double.NaN, bool AsMultiple = false,
                                                        bool FlipHorizontally = false, bool FlipVertically = false, bool Tilted = false)
        {
            ShapeType = ShapeType.NullDefault("");
            //T ShapeType = "NonExistent";

            if (ShapeType == Shapes.None)
            {
                ShapeType = Shapes.Rectangle;
                SymbolPen = new Pen(Brushes.Transparent, 1.0);
                SymbolBrush = Brushes.Transparent;
            }
            else
                // IMPORTANT: Do not reference Predefined-Shapes until initialized (recursive call, because this method is used to create sample Pictogram).
                if (Shapes.PredefinedShapesInitialized && !Shapes.PredefinedShapes.Any(shape => shape.TechName == ShapeType))
                {
                    Console.WriteLine("Symbol shape '" + ShapeType.NullDefault("") + "' not recognized. Please update ThinkComposer.");
                    ShapeType = Shapes.Rectangle;
                }

            var SymbolGeometry = Display.GetResource<GeometryGroup>(RES_PFX_SYMBOLS + ShapeType, true);
            if (SymbolGeometry == null)
                return null;

            Width = (Width.IsNan() ? SymbolGeometry.Bounds.Width : Width);
            Height = (Height.IsNan() ? SymbolGeometry.Bounds.Height : Height);

            Transform Transformation = null;

            if (!Tilted)
            {
                var OffsetX = CenterPosition.X - (Width / 2.0);
                var OffsetY = CenterPosition.Y - (Height / 2.0);
                var ScaleX = Width.SizeToScale(SymbolGeometry.Bounds.Width);
                var ScaleY = Height.SizeToScale(SymbolGeometry.Bounds.Height);
                Transformation = Display.CreateTransform(OffsetX, OffsetY, ScaleX, ScaleY);
            }
            else
            {
                var OffsetX = CenterPosition.X - (Height / 2.0);
                var OffsetY = CenterPosition.Y - (Width / 2.0);
                var ScaleX = Height.SizeToScale(SymbolGeometry.Bounds.Width);
                var ScaleY = Width.SizeToScale(SymbolGeometry.Bounds.Height);
                Transformation = Display.CreateTransform(OffsetX, OffsetY, ScaleX, ScaleY, 90,
                                                         OffsetX + Height / 2.0, OffsetY + Width / 2.0);
            }


            SymbolGeometry.Transform = Transformation;  // Must be before flipping

            if (FlipHorizontally || FlipVertically)
            {
                var Group = new TransformGroup();
                Group.Children.Add(Transformation);

                var AppliedTransform = new ScaleTransform(FlipHorizontally ? -1 : 1,
                                                   FlipVertically ? -1 : 1,
                                                   FlipHorizontally ? SymbolGeometry.Bounds.Left + SymbolGeometry.Bounds.Width / 2.0 : 0.0,
                                                   FlipVertically ? SymbolGeometry.Bounds.Top + SymbolGeometry.Bounds.Height / 2.0 : 0.0);

                Group.Children.Add(AppliedTransform);

                Transformation = Group;
                SymbolGeometry.Transform = Transformation;
            } 
            
            var ContentArea = (RectangleGeometry)SymbolGeometry.Children[0];
            SymbolGeometry.Children.RemoveAt(0);
            ContentArea.Transform = Transformation;

            Drawing DrawingResult = new GeometryDrawing(SymbolBrush, SymbolPen, SymbolGeometry);
            Rect ContentAreaResult = ContentArea.Bounds;

            if (AsMultiple)
            {
                var MultiDrawing = new DrawingGroup();

                var CloneDrawing = new DrawingGroup();
                CloneDrawing.Children.Add(DrawingResult);

                CloneDrawing.Transform = Display.CreateTransform(6, -6);
                MultiDrawing.Children.Add(CloneDrawing);

                CloneDrawing = CloneDrawing.Clone();

                CloneDrawing.Transform = Display.CreateTransform(3, -3);
                MultiDrawing.Children.Add(CloneDrawing);

                MultiDrawing.Children.Add(DrawingResult);

                DrawingResult = MultiDrawing;
            }

            var Result = Tuple.Create<Drawing, Rect>(DrawingResult, ContentAreaResult);

            return Result;
        }

        /// <summary>
        /// Creates and returns a Decorator symbol using the supplied Decorator Geometry, Brushes, Center Position, Size factors and Rotation Angle.
        /// Note: For coloring purposes... the first child of the geometry will be used as background, and the second as its inside shape.
        /// </summary>
        public static Drawing CreateDecorator(GeometryGroup DecoratorGeometry, Brush BackLineBrush, Brush ShapeLineBrush, Brush BackBrush, Brush ShapeBrush,
                                              Point Position, double Width = double.NaN, double Height = double.NaN, double RotationAngle = 0)
        {
            Width = (Width.IsNan() ? DecoratorGeometry.Bounds.Width : Width);
            Height = (Height.IsNan() ? DecoratorGeometry.Bounds.Height : Height);

            var OffsetX = Position.X;
            var OffsetY = Position.Y;
            var ScaleX = Width.SizeToScale(DecoratorGeometry.Bounds.Width);
            var ScaleY = Height.SizeToScale(DecoratorGeometry.Bounds.Height);
            var Transformation = Display.CreateTransform(OffsetX, OffsetY, ScaleX, ScaleY, RotationAngle,
                                                         (RotationAngle == 0 ? double.NaN : OffsetX + (Width / 2.0)),
                                                         (RotationAngle == 0 ? double.NaN : OffsetY + (Height / 2.0)));

            var BackGeom = DecoratorGeometry.Children[0];
            DecoratorGeometry.Children.Remove(BackGeom);

            BackGeom.Transform = Transformation;
            DecoratorGeometry.Transform = Transformation;

            var Result = new DrawingGroup();
            Result.Children.Add(new GeometryDrawing(BackBrush, new Pen(BackLineBrush, 1.0), BackGeom));
            Result.Children.Add(new GeometryDrawing(ShapeBrush, new Pen(ShapeLineBrush, 1.0), DecoratorGeometry));

            return Result;
        }
    }
}