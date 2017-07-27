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
// File   : ModelCollectionDefinitor.cs
// Object : Instrumind.Common.EntityDefinition.ModelCollectionDefinitor (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.10.12 Néstor Sánchez A.  Creation
//

using System;

using Instrumind.Common.EntityDefinition;

/// Provides a foundation of structures and services for business entities management, considering its life cycle, edition and persistence mapping.
namespace Instrumind.Common.EntityBase
{
    /// <summary>
    /// Base class for generic model class collection definitors.
    /// (Intended to work around problems by lack of support for contra/co+variance)
    /// </summary>
    public abstract class MModelCollectionDefinitor : MModelMemberDefinitor
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public MModelCollectionDefinitor(string TechName, string Name, string Summary, Type DeclaringType, EEntityMembership Membership, bool IsRequired, bool IsAdvanced)
            : base(TechName, Name, Summary, DeclaringType, Membership, IsRequired, IsAdvanced)
        {
        }

        /// <summary>
        /// Creates and returns an provider for the defined collection and supplied instance controller.
        /// </summary>
        internal abstract MCollectionController CreateProvider(MEntityInstanceController InstanceController);
    }
}