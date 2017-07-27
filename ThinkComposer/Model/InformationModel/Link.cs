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
// File   : Link.cs
// Object : Instrumind.ThinkComposer.Model.InformationModel.Link (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2010.02.09 Néstor Sánchez A.  Creation
//

using System;
using System.ComponentModel;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.Model.GraphModel;

/// Base abstractions for the Information enrichment of Graph entities
namespace Instrumind.ThinkComposer.Model.InformationModel
{
    /// <summary>
    /// References an external object (such as: File, Folder, Web adress) or an internal one (such as an Idea property).
    /// </summary>
    [Serializable]
    public abstract class Link : ContainedDetail
    {
        /// <summary>
        /// Static Constructor.
        /// </summary>
        static Link()
        {
            __ClassDefinitor = new ModelClassDefinitor<Link>("Link", ContainedDetail.__ClassDefinitor, "Link",
                                                             "References an external object (such as: File, Folder, Web adress) or an internal one (such as an Idea property).");
            __ClassDefinitor.Pictogram = Display.GetAppImage("link.png");
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public Link(Idea OwnerContainer)
             : base(OwnerContainer)
        {
        }

        /// <summary>
        /// Gets the address of the resource.
        /// </summary>
        [Description("Address of the resource.")]
        public abstract string TargetAddress { get; }

        /// <summary>
        /// Returns the underlying reference.
        /// </summary>
        public abstract object Target { get; }

        /// <summary>
        /// Returns the kind of this detail.
        /// </summary>
        [Description("Returns the kind of this detail.")]
        public override ModelDefinition Kind { get { return __ClassDefinitor; } }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelClass<LinkReference> Members

        public new MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public new ModelClassDefinitor<Link> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly new ModelClassDefinitor<Link> __ClassDefinitor = null;

        public new Link CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((Link)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public Link PopulateFrom(Link SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion
    }
}