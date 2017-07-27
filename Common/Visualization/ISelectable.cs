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
// File   : ISelectable.cs
// Object : Instrumind.Common.Visualization.ISelectable (Interface)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2011.02.08 Néstor Sánchez A.  Creation
//

using System;

using Instrumind.Common;

/// Specialized WPF components and features across Instrumind products.
namespace Instrumind.Common.Visualization
{
    /// <summary>
    /// Represents an object that can be visually selected.
    /// </summary>
    public interface ISelectable
    {
        bool IsSelected { get; set; }
    }
}
