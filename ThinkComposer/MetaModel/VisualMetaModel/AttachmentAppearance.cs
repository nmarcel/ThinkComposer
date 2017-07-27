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
// File   : AttachmentAppearance.cs
// Object : Instrumind.ThinkComposer.MetaModel.VisualMetaModel.AttachmentAppearance (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2010.01.15 Néstor Sánchez A.  Creation
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
    /// Stored appearance data for an attachment to be displayed as part of a details poster of a symbol.
    /// </summary>
    [Serializable]
    public class AttachmentAppearance : DetailAppearance, IModelEntity, IModelClass<AttachmentAppearance>, IIndicatesAlteration
    {
        /// <summary>
        /// Static Constructor.
        /// </summary>
        static AttachmentAppearance()
        {
            __ClassDefinitor = new ModelClassDefinitor<AttachmentAppearance>("AttachmentAppearance", DetailAppearance.__ClassDefinitor, "Attachment Appearance",
                                                                             "Stored appearance data for an attachment to be displayed as part of a details poster of a symbol.");
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public AttachmentAppearance(bool IsDisplayed = DEFVAL_IsDisplayed, bool ShowTitle = DEFVAL_ShowTitle)
             : base(IsDisplayed, ShowTitle)
        {
        }

        protected AttachmentAppearance()
        {
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public override DetailAppearance Clone()
        {
            var Result = this.CreateClone(ECloneOperationScope.Slight, null);
            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelClass<AttachmentFormat> Members

        public new MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public new ModelClassDefinitor<AttachmentAppearance> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly new ModelClassDefinitor<AttachmentAppearance> __ClassDefinitor = null;

        public override object CreateCopy(ECloneOperationScope CloningScope, IMModelClass DirectOwner) { return this.CreateClone(CloningScope, DirectOwner); }
        public new AttachmentAppearance CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((AttachmentAppearance)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public AttachmentAppearance PopulateFrom(AttachmentAppearance SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion
    }
}