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
// 2009.09.07 Néstor Sánchez A.  Creation
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
    /// Provides services for drawing connector paths.
    /// </summary>
    public static class PathDrawer
    {
        /// <summary>
        /// Creates and returns a Path using the supplied Path style, Corner style, Pen, Brush, Target and Source positions, Intermedaite positions (if any) and Magnitude factor.
        /// </summary>
        public static Drawing CreatePath(EPathStyle PathStyle, EPathCorner CornerStyle, Pen PathPen, Brush PathBrush,
                                         Point TargetPosition, Point SourcePosition, IEnumerable<Point> IntermediatePositions = null, double Magnitude = 1.0)
        {
            var Line = new LineGeometry(TargetPosition, SourcePosition);

            // PENDING: Create intermediate segments

            var Result = new GeometryDrawing(PathBrush, PathPen, Line);

            return Result;
        }

    }
}