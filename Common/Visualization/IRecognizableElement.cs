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
// File   : IRecognizableElement.cs
// Object : Instrumind.Common.Visualization.IRecognizableElement (Interface)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.06.30 Néstor Sánchez A.  Creation
//

using System;

using Instrumind.Common;
using System.Windows.Media;

/// Specialized WPF components and features across Instrumind products.
namespace Instrumind.Common.Visualization
{
    /// <summary>
    /// Identifiable instance, with tech-name, name and summary, which also has a representative pictogram.
    /// </summary>
    public interface IRecognizableElement : IIdentifiableElement
    {
        /// <summary>
        /// Graphic representation of the object.
        /// </summary>
        ImageSource Pictogram { get; }
    }
}