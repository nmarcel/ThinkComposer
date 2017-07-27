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
// File   : EShape.cs
// Object : Instrumind.ThinkComposer.MetaModel.VisualMetaModel.EShape (Enum)
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
    /// Codes for predefined symbol shapes.
    /// </summary>
    public static class Shapes
    {
        public const string None = "None";
        public const string Poster = "Poster";
        public const string Signboard = "Signboard";
        public const string Person = "Person";
        public const string Rectangle = "Rectangle";
        public const string Gears = "Gears";
        public const string Piece = "Piece";
        public const string RectCrossedHorizontal = "RectCrossedHorizontal";
        public const string RectCrossedVertical = "RectCrossedVertical";
        public const string Ellipse = "Ellipse";
        public const string Envelope = "Envelope";
        public const string RoundedRectangle = "RoundedRectangle";
        public const string Rhomb = "Rhomb";
        public const string HexagonHorizontal = "HexagonHorizontal";
        public const string HexagonVertical = "HexagonVertical";
        public const string Capsule = "Capsule";
        public const string Folder = "Folder";
        public const string File = "File";
        public const string Document = "Document";
        public const string Card = "Card";
        public const string Flag = "Flag";
        public const string Drum = "Drum";
        public const string Barrel = "Barrel";
        public const string Standard = "Standard";
        public const string Trapezium = "Trapezium";
        public const string Parallelogram = "Parallelogram";
        public const string Banner = "Banner";
        public const string Triangle = "Triangle";
        public const string Component = "Component";
        public const string ChevronVertical = "ChevronVertical";
        public const string ChevronHorizontal = "ChevronHorizontal";
        public const string Octagon = "Octagon";
        public const string Block = "Block";
        public const string BowTie = "BowTie";
        public const string RectDistorted = "RectDistorted";
        public const string RectCurved = "RectCurved";
        public const string RectCrossedCorner = "RectCrossedCorner";
        public const string Plate = "Plate";
        public const string Spin = "Spin";
        public const string Dome = "Dome";
        public const string RectEnclosed = "RectEnclosed";
        public const string EllipseEnclosed = "EllipseEnclosed";
        public const string RectIntercrossed = "RectIntercrossed";
        public const string EllipseIntercrossed = "EllipseIntercrossed";
        public const string RectIntercrossedDiagonal = "RectIntercrossedDiagonal";
        public const string EllipseIntercrossedDiagonal = "EllipseIntercrossedDiagonal";
        public const string RhombCrossed = "RhombCrossed";
        public const string OppositeTriangles = "OppositeTriangles";
        public const string RectCrossed = "RectCrossed";
        public const string Tape = "Tape";
        public const string XMark = "XMark";
        public const string StraightParallelLines = "StraightParallelLines";
        public const string StraightUnderLine = "StraightUnderLine";
        public const string BracketsSquare = "BracketsSquare";
        public const string BracketsCurved = "BracketsCurved";
        public const string BracketsCurly = "BracketsCurly";
        public const string Pentagon = "Pentagon";
        public const string Bin = "Bin";
        public const string RectDiagonal = "RectDiagonal";
        public const string Button = "Button";
        public const string RectCrossedTop = "RectCrossedTop";
        public const string Wrapper = "Wrapper";
        public const string Funnel = "Funnel";
        public const string Cloud = "Cloud";
        public const string Arrow = "Arrow";
        public const string ArrowDouble = "ArrowDouble";
        public const string ArrowRegular = "ArrowRegular";
        public const string ArrowRegularDouble = "ArrowRegularDouble";

        public static readonly List<SimplePresentationElement> PredefinedShapes = new List<SimplePresentationElement>();
        public static bool PredefinedShapesInitialized { get; private set; }

        static Shapes()
        {
            var ShapePen = new Pen(Brushes.Black, 1.0);
            var ShapeBrush = Brushes.White;
            var ShapeCenter = new Point(16.0, 8.0);
            var ShapeWidth = 32.0;
            var ShapeHeight = 16.0;

            PredefinedShapes.Add(new SimplePresentationElement("<None>", None, "", SymbolDrawer.CreateSymbol(None, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Poster", Poster, "", SymbolDrawer.CreateSymbol(Poster, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Signboard", Signboard, "", SymbolDrawer.CreateSymbol(Signboard, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Person", Person, "", SymbolDrawer.CreateSymbol(Person, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Rectangle", Rectangle, "", SymbolDrawer.CreateSymbol(Rectangle, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Gears", Gears, "", SymbolDrawer.CreateSymbol(Gears, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Piece", Piece, "", SymbolDrawer.CreateSymbol(Piece, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Rect-Crossed-Horizontal", RectCrossedHorizontal, "", SymbolDrawer.CreateSymbol(RectCrossedHorizontal, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Rect-Crossed-Vertical", RectCrossedVertical, "", SymbolDrawer.CreateSymbol(RectCrossedVertical, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Ellipse", Ellipse, "", SymbolDrawer.CreateSymbol(Ellipse, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Envelope", Envelope, "", SymbolDrawer.CreateSymbol(Envelope, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Rounded-Rectangle", RoundedRectangle, "", SymbolDrawer.CreateSymbol(RoundedRectangle, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Rhomb", Rhomb, "", SymbolDrawer.CreateSymbol(Rhomb, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Hexagon-Horizontal", HexagonHorizontal, "", SymbolDrawer.CreateSymbol(HexagonHorizontal, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Hexagon-Vertical", HexagonVertical, "", SymbolDrawer.CreateSymbol(HexagonVertical, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Capsule", Capsule, "", SymbolDrawer.CreateSymbol(Capsule, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Folder", Folder, "", SymbolDrawer.CreateSymbol(Folder, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("File", File, "", SymbolDrawer.CreateSymbol(File, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Document", Document, "", SymbolDrawer.CreateSymbol(Document, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Card", Card, "", SymbolDrawer.CreateSymbol(Card, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Flag", Flag, "", SymbolDrawer.CreateSymbol(Flag, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Drum", Drum, "", SymbolDrawer.CreateSymbol(Drum, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Barrel", Barrel, "", SymbolDrawer.CreateSymbol(Barrel, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Standard", Standard, "", SymbolDrawer.CreateSymbol(Standard, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Trapezium", Trapezium, "", SymbolDrawer.CreateSymbol(Trapezium, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Parallelogram", Parallelogram, "", SymbolDrawer.CreateSymbol(Parallelogram, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Banner", Banner, "", SymbolDrawer.CreateSymbol(Banner, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Triangle", Triangle, "", SymbolDrawer.CreateSymbol(Triangle, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Component", Component, "", SymbolDrawer.CreateSymbol(Component, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Chevron-Vertical", ChevronVertical, "", SymbolDrawer.CreateSymbol(ChevronVertical, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Chevron-Horizontal", ChevronHorizontal, "", SymbolDrawer.CreateSymbol(ChevronHorizontal, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Octagon", Octagon, "", SymbolDrawer.CreateSymbol(Octagon, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Block", Block, "", SymbolDrawer.CreateSymbol(Block, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("BowTie", BowTie, "", SymbolDrawer.CreateSymbol(BowTie, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Rect-Distorted", RectDistorted, "", SymbolDrawer.CreateSymbol(RectDistorted, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Rect-Curved", RectCurved, "", SymbolDrawer.CreateSymbol(RectCurved, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Rect-Crossed-Corner", RectCrossedCorner, "", SymbolDrawer.CreateSymbol(RectCrossedCorner, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Plate", Plate, "", SymbolDrawer.CreateSymbol(Plate, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Spin", Spin, "", SymbolDrawer.CreateSymbol(Spin, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Dome", Dome, "", SymbolDrawer.CreateSymbol(Dome, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Rect-Enclosed", RectEnclosed, "", SymbolDrawer.CreateSymbol(RectEnclosed, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Ellipse-Enclosed", EllipseEnclosed, "", SymbolDrawer.CreateSymbol(EllipseEnclosed, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Rect-Intercrossed", RectIntercrossed, "", SymbolDrawer.CreateSymbol(RectIntercrossed, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Ellipse-Intercrossed", EllipseIntercrossed, "", SymbolDrawer.CreateSymbol(EllipseIntercrossed, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Rect-Intercrossed-Diagonal", RectIntercrossedDiagonal, "", SymbolDrawer.CreateSymbol(RectIntercrossedDiagonal, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Ellipse-Intercrossed-Diagonal", EllipseIntercrossedDiagonal, "", SymbolDrawer.CreateSymbol(EllipseIntercrossedDiagonal, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Rhomb-Crossed", RhombCrossed, "", SymbolDrawer.CreateSymbol(RhombCrossed, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Opposite-Triangles", OppositeTriangles, "", SymbolDrawer.CreateSymbol(OppositeTriangles, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Rect-Crossed", RectCrossed, "", SymbolDrawer.CreateSymbol(RectCrossed, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Tape", Tape, "", SymbolDrawer.CreateSymbol(Tape, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("X-Mark", XMark, "", SymbolDrawer.CreateSymbol(XMark, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Straight-Parallel-Lines", StraightParallelLines, "", SymbolDrawer.CreateSymbol(StraightParallelLines, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Straight-Under-Line", StraightUnderLine, "", SymbolDrawer.CreateSymbol(StraightUnderLine, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Brackets-Square", BracketsSquare, "", SymbolDrawer.CreateSymbol(BracketsSquare, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Brackets-Curved", BracketsCurved, "", SymbolDrawer.CreateSymbol(BracketsCurved, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Brackets-Curly", BracketsCurly, "", SymbolDrawer.CreateSymbol(BracketsCurly, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Pentagon", Pentagon, "", SymbolDrawer.CreateSymbol(Pentagon, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Bin", Bin, "", SymbolDrawer.CreateSymbol(Bin, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Rect-Diagonal", RectDiagonal, "", SymbolDrawer.CreateSymbol(RectDiagonal, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Button", Button, "", SymbolDrawer.CreateSymbol(Button, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Rect-Crossed-Top", RectCrossedTop, "", SymbolDrawer.CreateSymbol(RectCrossedTop, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Wrapper", Wrapper, "", SymbolDrawer.CreateSymbol(Wrapper, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Funnel", Funnel, "", SymbolDrawer.CreateSymbol(Funnel, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Cloud", Cloud, "", SymbolDrawer.CreateSymbol(Cloud, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("Arrow", Arrow, "", SymbolDrawer.CreateSymbol(Arrow, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("ArrowDouble", ArrowDouble, "", SymbolDrawer.CreateSymbol(ArrowDouble, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("ArrowRegular", ArrowRegular, "", SymbolDrawer.CreateSymbol(ArrowRegular, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));
            PredefinedShapes.Add(new SimplePresentationElement("ArrowRegularDouble", ArrowRegularDouble, "", SymbolDrawer.CreateSymbol(ArrowRegularDouble, ShapePen, ShapeBrush, ShapeCenter, ShapeWidth, ShapeHeight).Item1.ToDrawingImage()));

            // IMPORTANT: Do not reference Predefined-Shapes until initialized.
            PredefinedShapesInitialized = true;
        }
    }
}