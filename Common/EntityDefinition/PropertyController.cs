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
// File   : PropertyController.cs
// Object : Instrumind.Common.EntityDefinition.PropertyController (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.07.31 Néstor Sánchez A.  Creation
//

using System;

using Instrumind.Common.EntityBase;

/// Provides structures, components and services for defining and exposing business entities.
namespace Instrumind.Common.EntityDefinition
{
    /// <summary>
    /// Manages the consumption, from external views, of an entity property.
    /// </summary>
    /// <typeparam name="TModelClass">Type of the declaring entity.</typeparam>
    /// <typeparam name="TValue">Type of the property value.</typeparam>
    public class PropertyController<TModelClass, TValue> : MPropertyController where TModelClass : class, IModelClass<TModelClass>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public PropertyController(MEntityInstanceController InstanceController)
             : base(InstanceController)
        {
        }

        /// <summary>
        /// Static level definition of the entity property exposed for consumption.
        /// </summary>
        public ModelPropertyDefinitor<TModelClass, TValue> PropertyDefinitor { get; internal set; }

        /// <summary>
        /// Gets the default value to assign to the entity instance property.
        /// </summary>
        public TValue DefaultValue { get; internal set; }

        public override MModelPropertyDefinitor Definitor { get { return this.PropertyDefinitor; } }
    }
}