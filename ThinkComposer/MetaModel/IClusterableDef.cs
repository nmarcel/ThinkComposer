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
// File   : IClusterableDef.cs
// Object : Instrumind.ThinkComposer.MetaModel.IClusterableDef (Interface)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2010.12.20 Néstor Sánchez A.  Creation
//

using System;

/// Metadata shared abstractions which conform a Domain definition: Primitives for Composition creation.
namespace Instrumind.ThinkComposer.MetaModel
{
    /// <summary>
    /// Represents a definition clusterable with others of the same kind for visualization or selection purposes.
    /// </summary>
    public interface IClusterableDef
    {
        /// <summary>
        /// Key of the associated Cluster.
        /// </summary>
        string ClusterKey { get;  }
    }
}