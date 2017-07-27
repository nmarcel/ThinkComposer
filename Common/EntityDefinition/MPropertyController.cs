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
// File   : MPropertyController.cs
// Object : Instrumind.Common.EntityDefinition.MPropertyController (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.08.06 Néstor Sánchez A.  Creation
//

using System;
using System.Collections;

using Instrumind.Common.EntityBase;
using Instrumind.Common.Visualization;

/// Provides structures, components and services for defining and exposing business entities.
namespace Instrumind.Common.EntityDefinition
{
    /// <summary>
    /// Base class for the consumption, from external views, of an entity property.
    /// (Intended to work around problems by lack of support for contra/co+variance)
    /// </summary>
    public abstract class MPropertyController : MMemberController
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public MPropertyController(MEntityInstanceController InstanceController)
             : base(InstanceController)
        {
        }

        public override MModelMemberDefinitor Definition { get { return this.Definitor; } }

        public abstract MModelPropertyDefinitor Definitor { get; }
    }
}