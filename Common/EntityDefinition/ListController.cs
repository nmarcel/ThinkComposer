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
// File   : PropertyController.cs
// Object : Instrumind.Common.EntityDefinition.ListController (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2010.01.11 Néstor Sánchez A.  Creation
//

using System;

using Instrumind.Common.EntityBase;

/// Provides structures, components and services for defining and exposing business entities.
namespace Instrumind.Common.EntityDefinition
{
    /// <summary>
    /// Manages the consumption, from external views, of an entity list collection.
    /// </summary>
    public class ListController<TModelClass, TItem> : MCollectionController where TModelClass : class, IModelClass<TModelClass>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public ListController(MEntityInstanceController InstanceController)
             : base(InstanceController)
        {
        }

        /// <summary>
        /// Static level definition of the entity list exposed for consumption.
        /// </summary>
        public ModelListDefinitor<TModelClass, TItem> ListDefinitor { get; internal set; }

        public override MModelCollectionDefinitor Definitor { get { return this.ListDefinitor; } }
    }
}