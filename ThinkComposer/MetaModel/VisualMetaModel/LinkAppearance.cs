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
// File   : LinkAppearance.cs
// Object : Instrumind.ThinkComposer.MetaModel.VisualMetaModel.LinkAppearance (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2010.02.09 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;

using Instrumind.ThinkComposer.Model.InformationModel;

/// Base abstractions for the user metadefinition of Visual schemas
namespace Instrumind.ThinkComposer.MetaModel.VisualMetaModel
{
    /// <summary>
    /// Stored appearance data for a link to be displayed as part of a details poster of a symbol.
    /// </summary>
    [Serializable]
    public class LinkAppearance : DetailAppearance, IModelEntity, IModelClass<LinkAppearance>, IIndicatesAlteration
    {
        /// <summary>
        /// Static Constructor.
        /// </summary>
        static LinkAppearance()
        {
            __ClassDefinitor = new ModelClassDefinitor<LinkAppearance>("LinkAppearance", DetailAppearance.__ClassDefinitor, "Link Appearance",
                                                                       "Stored appearance data for a link to be displayed as part of a details poster of a symbol.");
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public LinkAppearance(bool IsDisplayed = DEFVAL_IsDisplayed, bool ShowTitle = DEFVAL_ShowTitle)
             : base(IsDisplayed, ShowTitle)
        {
        }

        protected LinkAppearance()
        {
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public override DetailAppearance Clone()
        {
            var Result = this.CreateClone(ECloneOperationScope.Slight, null);
            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelClass<LinkFormat> Members

        public new MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public new ModelClassDefinitor<LinkAppearance> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly new ModelClassDefinitor<LinkAppearance> __ClassDefinitor = null;

        public override object CreateCopy(ECloneOperationScope CloningScope, IMModelClass DirectOwner) { return this.CreateClone(CloningScope, DirectOwner); }
        public new LinkAppearance CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((LinkAppearance)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public LinkAppearance PopulateFrom(LinkAppearance SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion
    }
}