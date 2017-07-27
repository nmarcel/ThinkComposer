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
// File   : IUniqueElement.cs
// Object : Instrumind.Common.IUniqueElement (Interface)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.08.19 Néstor Sánchez A.  Creation
//

using System;

/// Library of general purpose components and features across Instrumind products.
namespace Instrumind.Common
{
    /// <summary>
    /// Represents a full globally unique identifiable object.
    /// </summary>
    public interface IUniqueElement : IComparable
    {
        /// <summary>
        /// Global unique identificator of the element instance.
        /// </summary>
        Guid GlobalId { get; }
    }
}