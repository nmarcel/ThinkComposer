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
// File   : ITechSpecifier.cs
// Object : Instrumind.Common.ITechSpecifier (Interface)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2012.06.28 Néstor Sánchez A.  Creation
//

using System;
using System.ComponentModel;

/// Library of general purpose components and features across Instrumind products.
namespace Instrumind.Common
{
    /// <summary>
    /// Represents an object with technical-specification.
    /// </summary>
    public interface ITechSpecifier : IComparable
    {
        /// <summary>
        /// Technical-Specification of the subject. Intended as a machine-level representation for computation (i.e. for use as script, template, formula or other kind of expression).
        /// </summary>
        string TechSpec { get; set; }
    }
}