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
// File   : IDefined.cs
// Object : Instrumind.ThinkComposer.MetaModel.IDefined (Interface)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.07.14 Néstor Sánchez A.  Creation
//

using System;

/// Metadata shared abstractions which conform a Domain definition: Primitives for Composition creation.
namespace Instrumind.ThinkComposer.MetaModel
{
    /// <summary>
    /// Represents an object defined by a meta-definitor.
    /// </summary>
    public interface IDefined
    {
        /// <summary>
        /// Gets the associated meta-definitor of the instance.
        /// </summary>
        MetaDefinition Definitor { get; }
    }
}