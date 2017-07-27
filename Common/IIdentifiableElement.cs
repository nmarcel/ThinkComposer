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
// File   : IIdentifiableElement.cs
// Object : Instrumind.Common.IIdentifiableElement (Interface)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.07.02 Néstor Sánchez A.  Creation
//

using System;
using System.ComponentModel;

/// Library of general purpose components and features across Instrumind products.
namespace Instrumind.Common
{
    /// <summary>
    /// Represents an identifiable instance with unique tech-name, name and summary.
    /// </summary>
    public interface IIdentifiableElement : IComparable
    {
        /// <summary>
        /// Name or Title of the object.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Technical-Name of the object. Must be schema unique.
        /// Intended for machine-level usage as code, identifier or name for files/tables/programs.
        /// </summary>
        string TechName { get; set; }

        /// <summary>
        /// Summary of the object.
        /// </summary>
        string Summary { get; set; }
    }
}