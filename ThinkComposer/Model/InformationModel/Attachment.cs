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
// File   : Attachment.cs
// Object : Instrumind.ThinkComposer.Model.InformationModel.Attachment (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.08.27 Néstor Sánchez A.  Creation
//

using System;
using System.ComponentModel;

using Instrumind.Common;
using Instrumind.Common.Visualization;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;

using Instrumind.ThinkComposer.Model.GraphModel;
using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;

/// Base abstractions for the conformation of the Graph schema
namespace Instrumind.ThinkComposer.Model.InformationModel
{
    /// <summary>
    /// Stores embedded content, such as images, files, etc.
    /// </summary>
    [Serializable]
    public class Attachment : ContainedDetail, IModelEntity, IModelClass<Attachment>
    {
        /// <summary>
        /// Static Constructor.
        /// </summary>
        static Attachment()
        {
            __ClassDefinitor = new ModelClassDefinitor<Attachment>("Attachment", ContainedDetail.__ClassDefinitor, "Attachment",
                                                                   "Represents an arbitrary embedded object, obtained from external source, such as: image, video, data file, etc.");
            __ClassDefinitor.Pictogram = Display.GetAppImage("page_attach.png");

            __ClassDefinitor.DeclareProperty(__AssignedDesignator);
            __ClassDefinitor.DeclareProperty(__Content);
            __ClassDefinitor.DeclareProperty(__Source);
            __ClassDefinitor.DeclareProperty(__MimeType);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public Attachment(Idea OwnerContainer, Assignment<DetailDesignator> Designator, string Source, string MimeType = null)
             : base(OwnerContainer)
        {
            this.AssignedDesignator = Designator;
            this.Source = Source;

            if (MimeType.IsAbsent())
                this.MimeType = General.GetMimeTypeFromFileExtension(System.IO.Path.GetExtension(Source));
            else
                this.MimeType = MimeType;
        }

        /// <summary>
        /// Attachment designator.
        /// </summary>
        public override Assignment<DetailDesignator> AssignedDesignator { get { return __AssignedDesignator.Get(this); } set { __AssignedDesignator.Set(this, value); } }
        protected Assignment<DetailDesignator> AssignedDesignator_;
        public static readonly ModelPropertyDefinitor<Attachment, Assignment<DetailDesignator>> __AssignedDesignator =
                   new ModelPropertyDefinitor<Attachment, Assignment<DetailDesignator>>("AssignedDesignator", EEntityMembership.External, null, EPropertyKind.Common, ins => ins.AssignedDesignator_, (ins, val) => ins.AssignedDesignator_ = val, false, true,
                                                                                    "Assigned Designator", "Attachment content designator");
        public override MAssignment ContentDesignator { get { return this.AssignedDesignator; } }
        [Description("Attachment designator.")]
        public AttachmentDetailDesignator Designator { get { return (AttachmentDetailDesignator)this.ContentDesignator.AssignedValue; } }

        /// <summary>
        /// If not null, references the Attachment of this Idea.
        /// </summary>
        public byte[] Content { get { return __Content.Get(this); } set { __Content.Set(this, value); } }
        protected byte[] Content_;
        public static readonly ModelPropertyDefinitor<Attachment, byte[]> __Content =
                   new ModelPropertyDefinitor<Attachment, byte[]>("Content", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.Content_, (ins, val) => ins.Content_ = val, false, true,
                                                                                    "Content", "If not null, references the attachment content of this Idea.");

        /// <summary>
        /// Location-of/route-to the resource origin.
        /// </summary>
        public string Source { get { return __Source.Get(this); } set { __Source.Set(this, value); } }
        protected string Source_;
        public static readonly ModelPropertyDefinitor<Attachment, string> __Source =
                   new ModelPropertyDefinitor<Attachment, string>("Source", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.Source_, (ins, val) => ins.Source_ = val, false, true,
                                                                                    "Source", "Location-of/route-to the resource origin.");

        /// <summary>
        /// Detected MIME-Type when the attachment was loaded.
        /// </summary>
        public string MimeType { get { return __MimeType.Get(this); } set { __MimeType.Set(this, value); } }
        protected string MimeType_;
        public static readonly ModelPropertyDefinitor<Attachment, string> __MimeType =
                   new ModelPropertyDefinitor<Attachment, string>("MimeType", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.MimeType_, (ins, val) => ins.MimeType_ = val, false, true,
                                                                                    "MIME-Type", "Detected MIME-Type when the attachment was loaded.");

        /// <summary>
        /// Indicates whether the contained/referenced detail has a known format, therefore can be manipulated or shown.
        /// </summary>
        public override bool ValueHasKnownFormat { get { return MimeType.StartsWith("image/"); } }

        /// <summary>
        /// Returns the kind of this detail.
        /// </summary>
        [Description("Returns the kind of this detail.")]
        public override ModelDefinition Kind { get { return __ClassDefinitor; } }

        /// <summary>
        /// Updates the related Designator identification information based on the Content.
        /// </summary>
        public override void UpdateDesignatorIdentification()
        {
            if (this.AssignedDesignator == null)
                return;

            this.Designation.TechName = this.Source;
            this.Designation.Name = this.Source.GetSimplifiedResourceName();
        }

        public override string ToString()
        {
            return ("Detail-Type" + Idea.SYNOP_SEPARATOR + "Attachment" + Idea.SYNOP_SEPARATOR +
                    "Mime-Type" + Idea.SYNOP_SEPARATOR + this.MimeType + Idea.SYNOP_SEPARATOR + "Source" + Idea.SYNOP_SEPARATOR + this.Source);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelClass<Attachment> Members

        public new MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public new ModelClassDefinitor<Attachment> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly new ModelClassDefinitor<Attachment> __ClassDefinitor = null;

        public override object CreateCopy(ECloneOperationScope CloningScope, IMModelClass DirectOwner) { return this.CreateClone(CloningScope, DirectOwner); }
        public new Attachment CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((Attachment)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public Attachment PopulateFrom(Attachment SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion

    }
}