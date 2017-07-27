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
// File   : MetaCategory.cs
// Object : Instrumind.ThinkComposer.MetaModel.MetaCategory (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.09.02 Néstor Sánchez A.  Creation
//

using System;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;

/// Metadata shared abstractions which conform a Domain definition: Primitives for Composition creation.
namespace Instrumind.ThinkComposer.MetaModel
{
    /// <summary>
    /// Categorizator for metadata definitors.
    /// </summary>
    /// <typeparam name="TMetaDefinitor">Type of instances to be categorized</typeparam>
    [Serializable]
    public class MetaCategory<TMetaDefinitor> : MetaDefinition, IModelEntity, IModelClass<MetaCategory<TMetaDefinitor>>
    {
        /// <summary>
        /// Static Constructor.
        /// </summary>
        static MetaCategory()
        {
            __ClassDefinitor = new ModelClassDefinitor<MetaCategory<TMetaDefinitor>>("MetaCategory", MetaDefinition.__ClassDefinitor, "Meta-Category",
                                                                                     "Categorizator for metadata definitors.");
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Name">Name of the MetaCategory.</param>
        /// <param name="TechName">Technical Name of the MetaCategory.</param>
        /// <param name="Summary">Summary of the MetaCategory.</param>
        /// <param name="Pictogram">Image representing the MetaCategory.</param>
        public MetaCategory(string Name, string TechName, string Summary = "", ImageSource Pictogram = null)
             : base(Name, TechName, Summary, Pictogram)
        {
            // IMPORTANT: If Owner is added, then implement IVersionUpdater
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelClass<MetaCategory> Members

        public new MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public new ModelClassDefinitor<MetaCategory<TMetaDefinitor>> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly new ModelClassDefinitor<MetaCategory<TMetaDefinitor>> __ClassDefinitor = null;

        public new MetaCategory<TMetaDefinitor> CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((MetaCategory<TMetaDefinitor>)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public MetaCategory<TMetaDefinitor> PopulateFrom(MetaCategory<TMetaDefinitor> SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}