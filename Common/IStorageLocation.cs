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
// File   : IStorageLocation.cs
// Object : Instrumind.Common.IStorageLocation (Interface)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.07.13 Néstor Sánchez A.  Creation
//

using System;

/// Library of general purpose components and features across Instrumind products.
namespace Instrumind.Common
{
    /// <summary>
    /// References an object for external storage of content.
    /// </summary>
    public interface IStorageLocation
    {
        /// <summary>
        /// Fully qualified location of the destination storage (e.g.: a file path).
        /// </summary>
        Uri FullLocation { get; }

        /// <summary>
        /// User easily understandable location of the destination storage (e.g: "file name (folder name)").
        /// </summary>
        string SimplifiedLocation { get; }
    }
}