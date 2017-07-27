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
// File   : IModelClass.cs
// Object : Instrumind.Common.EntityDefinition.IModelClass (Interface)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.07.21 Néstor Sánchez A.  Creation
//

using System;

using Instrumind.Common;
using Instrumind.Common.EntityBase;

/// Provides structures, components and services for defining and exposing business entities.
namespace Instrumind.Common.EntityDefinition
{
    /// <summary>
    /// Represents a business entity, having static level definitors for its classes and properties.
    /// </summary>
    /// <typeparam name="TModelClass">Type of the model class</typeparam>
    public interface IModelClass<TModelClass> : IMModelClass where TModelClass : class, IMModelClass
    {
        /// <summary>
        /// The definitor object for the model class type of the implementing instance.
        /// </summary>
        ModelClassDefinitor<TModelClass> ClassDefinitor { get; }

        /// <summary>
        /// Creates and returns a clone of this source model instance, plus optionally indicating the cloning-scope and active status.
        /// Where "active" means that the cloned object will be able to notify changes and store them for undo/redo.
        /// </summary>
        TModelClass CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true);

        /// <summary>
        /// Populates properties of this model class with these from the supplied one.
        /// </summary>
        /// <param name="Source">Source model class from which get property values.</param>
        /// <param name="UpdateContent">Indicates whether to update the current content with original source values (true), or with duplicates according to cloning-scope (false)</param>
        /// <param name="DirectOwner">References the direct owner of this target instance.</param>
        /// <param name="CloningScope">Indicates to make a copy of values to populate through the dependant object hierarchy in a certain way.</param>
        /// <param name="MemberNames">Optional explicit member names to be populated (when empty, all members are populated).</param>
        /// <returns>The populated model instance.</returns>
        TModelClass PopulateFrom(TModelClass Source, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames);
    }
}