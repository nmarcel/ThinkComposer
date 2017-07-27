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
// File   : AttachmentDetailDesignator.cs
// Object : Instrumind.ThinkComposer.MetaModel.GraphMetaModel.AttachmentDetailDesignator (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.12.09 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.MetaModel.InformationMetaModel;
using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;
using Instrumind.ThinkComposer.Model.GraphModel;
using Instrumind.ThinkComposer.Model.InformationModel;

/// Base abstractions for the user metadefinition of Graph schemas
namespace Instrumind.ThinkComposer.MetaModel.GraphMetaModel
{
    /// <summary>
    /// Associates an Attachment definition to an Idea.
    /// </summary>
    [Serializable]
    public class AttachmentDetailDesignator : DetailDesignator, IModelEntity, IModelClass<AttachmentDetailDesignator>
    {
        public static new string KindName { get { return Attachment.__ClassDefinitor.TechName; } }
        public static new string KindTitle { get { return Attachment.__ClassDefinitor.Name; } }
        public static new string KindSummary { get { return Attachment.__ClassDefinitor.Summary; } }
        public static new ImageSource KindPictogram { get { return Display.GetAppImage("page_attach.png"); } }

        /// <summary>
        /// Static Constructor.
        /// </summary>
        static AttachmentDetailDesignator()
        {
            __ClassDefinitor = new ModelClassDefinitor<AttachmentDetailDesignator>("AttachmentDetailDesignator", DetailDesignator.__ClassDefinitor, "Attachment Detail Designator",
                                                                                   "Associates an Attachment definition to an Idea.");
            __ClassDefinitor.DeclareProperty(__DeclaringContentType);
            __ClassDefinitor.DeclareProperty(__AttachmentLook);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public AttachmentDetailDesignator(Ownership<IdeaDefinition, Idea> Owner, /*- FileDataType DesignatedContentType, */
                                          string Name, string TechName, string Summary = "", ICopyable DefaultDetail = null, ImageSource Pictogram = null)
            : base(Owner, Name, TechName, Summary, DefaultDetail, Pictogram)
        {
            this.DeclaringContentType = FileDataType.FileTypeAny;
            this.AttachmentLook = new AttachmentAppearance();
        }

        /// <summary>
        /// Protected Constructor for Agent descendants.
        /// </summary>
        protected AttachmentDetailDesignator()
                : base()
        {
        }

        /// <summary>
        /// Gets the predefined detail appearance.
        /// </summary>
        public override DetailAppearance DetailLook { get { return this.AttachmentLook; } }

        public override IRecognizableElement Definitor { get { return FileDataType.FileTypeAny; } set { /* Nothing, else make fully selectable/assignable */ } }

        public override IEnumerable<IRecognizableElement> AvailableDefinitors { get { return AvailableDefinitors_; } }
        public static readonly IRecognizableElement[] AvailableDefinitors_ = { FileDataType.FileTypeAny };

        /// <summary>
        /// Designated Content Type.
        /// </summary>
        public FileDataType DeclaringContentType { get { return __DeclaringContentType.Get(this); } set { __DeclaringContentType.Set(this, value); } }
        protected StoreBox<FileDataType> DeclaringContentType_ = new StoreBox<FileDataType>();
        public static readonly ModelPropertyDefinitor<AttachmentDetailDesignator, FileDataType> __DeclaringContentType =
                   new ModelPropertyDefinitor<AttachmentDetailDesignator, FileDataType>("DeclaringContentType", EEntityMembership.External, null, EPropertyKind.Common, ins => ins.DeclaringContentType_, (ins, stb) => ins.DeclaringContentType_ = stb, false, true, "Declaring Content Type", "Type of the content to be attached.");

        /// <summary>
        /// Predefined attachment appearance.
        /// </summary>
        public AttachmentAppearance AttachmentLook { get { return __AttachmentLook.Get(this); } set { __AttachmentLook.Set(this, value); } }
        protected AttachmentAppearance AttachmentLook_ = null;
        public static readonly ModelPropertyDefinitor<AttachmentDetailDesignator, AttachmentAppearance> __AttachmentLook =
                   new ModelPropertyDefinitor<AttachmentDetailDesignator, AttachmentAppearance>("AttachmentLook", EEntityMembership.External, null, EPropertyKind.Common, ins => ins.AttachmentLook_, (ins, val) => ins.AttachmentLook_ = val, false, true,
                                                                                    "Attachment Look", "Predefined attachment appearance.");

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelClass<TableDesignator> Members

        public new MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public new ModelClassDefinitor<AttachmentDetailDesignator> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly new ModelClassDefinitor<AttachmentDetailDesignator> __ClassDefinitor = null;

        public new AttachmentDetailDesignator CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((AttachmentDetailDesignator)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public AttachmentDetailDesignator PopulateFrom(AttachmentDetailDesignator SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}