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
// File   : IVersionChanger.cs
// Object : Instrumind.Common.IVersionChanger (Interface)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2011.06.01 Néstor Sánchez A.  Creation
//

using System;

namespace Instrumind.Common
{
    /// <summary>
    /// Represents an object which at change must update the version of another one, tipically a owner of it.
    /// </summary>
    public interface IVersionChanger
    {
        /// <summary>
        /// Gets the formalized-element whose version must be updated after a change.
        /// </summary>
        IFormalizedElement VersionableTarget { get; }
    }
}