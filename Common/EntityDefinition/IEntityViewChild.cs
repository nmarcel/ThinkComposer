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
// File   : IEntityViewChild.cs
// Object : Instrumind.Common.EntityDefinition.IEntityViewChild (Interface)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.12.25 Néstor Sánchez A.  Creation
//

using System;

/// Provides structures, components and services for defining and exposing business entities.
namespace Instrumind.Common.EntityDefinition
{
    /// <summary>
    /// Represents a dependant property, child of an entity view.
    /// </summary>
    public interface IEntityViewChild
    {
        /// <summary>
        /// References the parent entity-view.
        /// </summary>
        IEntityView ParentEntityView { get; set; }

        /// <summary>
        /// Name of the child property referenced.
        /// </summary>
        string ChildPropertyName { get; }

        /// <summary>
        /// Refresh the exposed child content.
        /// </summary>
        void Refresh();

        /// <summary>
        /// Apply the exposed child content and indicate success/failure.
        /// </summary>
        bool Apply();
    }
}