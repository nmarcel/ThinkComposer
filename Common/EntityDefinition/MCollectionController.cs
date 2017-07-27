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
// File   : MCollectionController.cs
// Object : Instrumind.Common.EntityDefinition.MCollectionController (Class)
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
    /// Base class for the consumption, from external views, of an entity collection.
    /// (Intended to work around problems by lack of support for contra/co+variance)
    /// </summary>
    public abstract class MCollectionController : MMemberController
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public MCollectionController(MEntityInstanceController InstanceController)
             : base(InstanceController)
        {
        }

        public override MModelMemberDefinitor Definition { get { return this.Definitor; } }

        public abstract MModelCollectionDefinitor Definitor { get; }
    }
}