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
// File   : IEntityView.cs
// Object : Instrumind.Common.EntityDefinition.IEntityView (Interface)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.08.04 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;

using Instrumind.Common.EntityBase;

/// Provides structures, components and services for defining and exposing business entities.
namespace Instrumind.Common.EntityDefinition
{
    /// <summary>
    /// Represents a view capable of expose entiy instances and properties, via its agents.
    /// </summary>
    public interface IEntityView
    {
        /// <summary>
        /// Associated entity instance to be shown.
        /// </summary>
        IModelEntity AssociatedEntity { get; }

        /// <summary>
        /// Occurs when the entity is exposed for view consumption.
        /// </summary>
        event Action EntityExposedForView;

        /// <summary>
        /// Indicates to show/hide advanced members.
        /// </summary>
        bool ShowAdvancedMembers { get; }

        /// <summary>
        /// Occurs when the associated entity agent indicates to show/hide advanced members.
        /// </summary>
        event Action<bool> ShowAdvancedMembersChanged;

        /// <summary>
        /// Shows the supplied message and attached data, if any.
        /// </summary>
        void ShowMessage(string Title, string Message, EMessageType MessageType = EMessageType.Information, IDictionary<string, object> AttachedData = null);

        /// <summary>
        /// Operation to be called after a view member value has been edited by user.
        /// </summary>
        void ReactToMemberEdited(MModelMemberDefinitor MemberDef, object Value);
    }
}