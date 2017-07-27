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
// File   : IFormalizedElement.cs
// Object : Instrumind.Common.IFormalizedElement (Interface)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.08.12 Néstor Sánchez A.  Creation
//

using System;
using System.Windows.Documents;

/// Library of general purpose components and features across Instrumind products.
namespace Instrumind.Common
{
    /// <summary>
    /// Represents an identifiable instance with unique tech-name, name, summary, description, classification and version information.
    /// </summary>
    public interface IFormalizedElement : IUniqueElement, IIdentifiableElement
    {
        /// <summary>
        /// Detailed description text (rich) of the element.
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Classification information store (commonly attached only to complex entities like Models or Diagrams).
        /// </summary>
        ClassificationCard Classification { get; set; }

        /// <summary>
        /// Version information store (commonly attached only to complex entities like Models or Diagrams).
        /// </summary>
        VersionCard Version { get; set; }
    }
}