// -------------------------------------------------------------------------------------------
// Instrumind ThinkComposer
//
// Copyright (C) Néstor Marcel Sánchez Ahumada. Santiago, Chile.
// http://thinkcomposer.codeplex.com
//
// This file is part of ThinkComposer, which is free software licensed under the GNU General Public License.
// It is provided without any warranty. You should find a copy of the license in the root directory of this software product.
// -------------------------------------------------------------------------------------------
//
// Project: Instrumind ThinkComposer v1.0
// File   : IIndicatesAlteration.cs
// Object : Instrumind.Common.IIndicatesAlteration (Interface)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2010.01.14 Néstor Sánchez A.  Creation
//

using System;

/// Library of general purpose components and features across Instrumind products.
namespace Instrumind.Common
{
    /// <summary>
    /// Represents an object capable of inform whether its content differs from the initial default.
    /// </summary>
    public interface IIndicatesAlteration
    {
        /// <summary>
        /// Indicates whether the instance has been changed respect its default values, therefore requiring to be reevaluated for later use.
        /// </summary>
        bool IsAltered { get; }
    }
}