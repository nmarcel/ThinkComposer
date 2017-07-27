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
// File   : IModelEntity.cs
// Object : Instrumind.Common.EntityBase.IModelEntity (Interface)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.07.22 Néstor Sánchez A.  Creation
//

using System;
using System.ComponentModel;

using Instrumind.Common.EntityDefinition;

/// Provides a foundation of structures and services for business entities management, considering its life cycle, edition and persistence mapping.
namespace Instrumind.Common.EntityBase
{
    /// <summary>
    /// Represents a business entity to be consumed by referrals, for edition, persistence mapping, and life cycle control purposes.
    /// </summary>
    public interface IModelEntity : IMModelClass
    {
        // Redefines base member for simpler editing.
        new MModelClassDefinitor ClassDefinition { get; }

        /// <summary>
        /// Engine in charge of editing this entity instance.
        /// </summary>
        EntityEditEngine EditEngine { get; set; }

        /// <summary>
        /// Refreshes this instance and any related dependencies.
        /// To be called post state change and when propagation is desired.
        /// </summary>
        void RefreshEntity();

        /// <summary>
        /// Controller for the entity instance.
        /// </summary>
        MEntityInstanceController Controller { get; set; }
    }
}