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
// File   : EEntityMembership.cs
// Object : Instrumind.Common.EntityBase.EEntityMembership (Enum)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2010.04.06 Néstor Sánchez A.  Creation
//

using System;

/// Provides a foundation of structures and services for business entities management, considering its life cycle, edition and persistence mapping.
namespace Instrumind.Common.EntityBase
{
    /// <summary>
    /// Possible declarations of value-containment or instance-referencing, hence implicating graph-ownership and cloneability, that members of an entity can have.
    /// </summary>
    public enum EEntityMembership : byte
    {
        /// <summary>
        /// Marks an entity member as having direct entity-core level content which belongs only to the entity, hence is a cloneable value or reference to a cloneable object.
        /// </summary>
        InternalCoreExclusive = ((byte)'C'),

        /// <summary>
        /// Marks an entity reference member as pointing to direct entity-core level content whose belonging is not declared by the member, hence is a reference to a non-cloneable object.
        /// </summary>
        InternalCoreShared = ((byte)'S'),

        /// <summary>
        /// Marks an entity member as having direct entity-graph (beyond core) level content, usually masive, which belongs only to the entity, hence is a cloneable value or reference to a cloneable object.
        /// </summary>
        InternalBulk = ((byte)'B'),

        /// <summary>
        /// Marks an entity reference member as not having direct entity-graph level content, hence is a reference to a non-cloneable object.
        /// </summary>
        External = ((byte)'X')
    }
}