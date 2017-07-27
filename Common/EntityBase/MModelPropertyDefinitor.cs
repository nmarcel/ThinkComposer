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
// File   : MModelPropertyDefinitor.cs
// Object : Instrumind.Common.EntityBase.MModelPropertyDefinitor (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.07.18 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;

using Instrumind.Common.EntityDefinition;

/// Provides a foundation of structures and services for business entities management, considering its life cycle, edition and persistence mapping.
namespace Instrumind.Common.EntityBase
{
    /// <summary>
    /// Base class for generic model class property definitors.
    /// (Intended to work around problems by lack of support for contra/co+variance)
    /// </summary>
    public abstract class MModelPropertyDefinitor : MModelMemberDefinitor, ICopyable
    {
        /// <summary>
        /// Returns the standard estimated size, in characters, for the supplied property-kind.
        /// </summary>
        public static int GetEstimatedCharactersFor(EPropertyKind Kind)
        {
            return (Kind == EPropertyKind.Name ? 16 : (Kind == EPropertyKind.Description ? 32 : 8));
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public MModelPropertyDefinitor(string TechName, string Name, string Summary, Type DeclaringType, EEntityMembership Membership,
                                       bool? ReferencesOwner, EPropertyKind Kind, bool IsDirectlyEditable, bool IsAdvanced, bool IsRequired)
            : base(TechName, Name, Summary, DeclaringType, Membership, IsRequired, IsAdvanced)
        {
            this.ReferencesOwner = ReferencesOwner;
            this.Kind = Kind;
            this.IsDirectlyEditable = IsDirectlyEditable;
        }

        /// <summary>
        /// Intended purpose of the property.
        /// </summary>
        public EPropertyKind Kind { get; protected set; }

        /// <summary>
        /// Indicates whether the property points directly (true), indirectly (false), or not points (null) to the owner of the entity.
        /// </summary>
        public bool? ReferencesOwner { get; protected set; }

        /// <summary>
        /// Indicates that the property is to be shown where appropriate for direct manual editing.
        /// </summary>
        public bool IsDirectlyEditable { get; protected set; }

        /// <summary>
        /// Gets the minimum number of characters to be used when displayed (zero for unspecified).
        /// </summary>
        public int DisplayMinLength { get { return GetEstimatedCharactersFor(this.Kind); } }

        /// <summary>
        /// Minimum value for a range of available values to be setted. Only applies when the property is numeric.
        /// </summary>
        public double RangeMin { get; set; }

        /// <summary>
        /// Maximum value for a range of available values to be setted. Only applies when the property is numeric.
        /// </summary>
        public double RangeMax { get; set; }

        /// <summary>
        /// Step to be used for changing the value for a range of available values to be setted. Only applies when Range-Intervals is null and the property is numeric.
        /// </summary>
        public double RangeStep { get { return this.RangeStep_; } set { this.RangeStep_ = value; } }
        private double RangeStep_ = 1.0;

        /// <summary>
        /// Optional explicit range intervals declared (when not null). It takes precedence over Range-Step and only applies when the property is numeric.
        /// </summary>
        public double[] RangeIntervals { get; set; }

        /// <summary>
        /// Indicates whether the propery is available to be linked internally. E.g.: "Summary".
        /// </summary>
        public bool IsLinkeable { get; set; }

        /// <summary>
        /// Indicates whether the propery is used as (not necessarily unique) identifier. E.g.: "Globa-Id", "Name", "Tech-Name".
        /// </summary>
        public bool IsIdentificator { get; set; }

        /// <summary>
        /// Path of the display member of the standard items source to be used by consumers.
        /// </summary>
        public string ItemsSourceDisplayMemberPath { get; set; }

        /// <summary>
        /// Creates and returns a provider for the defined property and supplied instance controller.
        /// </summary>
        internal abstract MPropertyController CreateProvider(MEntityInstanceController InstanceController);

        /// <summary>
        /// Creates and returns a clone object of this source model instance, plus indicating the cloning-scope and direct-owner.
        /// </summary>
        public abstract object CreateCopy(ECloneOperationScope CloningScope, IMModelClass DirectOwner);

        /// <summary>
        /// Indicates whether instance contents of this property are Store-Box based.
        /// </summary>
        public abstract bool IsStoreBoxBased { get; }

        /// <summary>
        /// Indicates whether instance contents of this property contains Rich-Content.
        /// </summary>
        public abstract bool HasRichContent { get; set; }

        /// <summary>
        /// Gets the Store-Box base container of this property for the specified instance.
        /// </summary>
        public abstract StoreBoxBase GetStoreBoxContainer(IMModelClass Instance);

        /// <summary>
        /// Sets the Store-Box base container of this property for the specified instance.
        /// </summary>
        public abstract void SetStoreBoxBaseContainer(IMModelClass Instance, StoreBoxBase Container);
    }
}