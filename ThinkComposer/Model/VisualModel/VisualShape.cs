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
// File   : VisualShape.cs
// Object : Instrumind.ThinkComposer.Model.VisualModel.VisualShape (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.08.25 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.Definitor.DefinitorUI;
using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.MetaModel.InformationMetaModel;
using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;
using Instrumind.ThinkComposer.Model.InformationModel;

/// Base abstractions for the visual representation of Graph entities
namespace Instrumind.ThinkComposer.Model.VisualModel
{
    /// <summary>
    /// A drawing, thus a single vector-based shape or a collection of it.
    /// </summary>
    [Serializable]
    public class VisualShape : VisualSymbol, IModelEntity, IModelClass<VisualShape>
    {
        /// <summary>
        /// Static Constructor.
        /// </summary>
        static VisualShape()
        {
            __ClassDefinitor = new ModelClassDefinitor<VisualShape>("VisualShape", VisualSymbol.__ClassDefinitor, "Visual Shape",
                                                                    "A drawing, thus a single shape or a collection of it. Based on its VisualObject ancestor, hence a simple vector-based visual object with no further visual specialization.");
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public VisualShape(VisualRepresentation OwnerRepresentation, EVisualRepresentationPart VisualRepresentationPart,
                           Point CenterPosition = default(Point), double AreaWidth = 0, double AreaHeight = 0)
             : base(OwnerRepresentation, VisualRepresentationPart, CenterPosition, AreaWidth, AreaHeight)
        {
        }

        protected VisualShape()
                : base()
        {
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelClass<VisualShape> Members

        public new MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public new ModelClassDefinitor<VisualShape> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly new ModelClassDefinitor<VisualShape> __ClassDefinitor = null;

        public override object CreateCopy(ECloneOperationScope CloningScope, IMModelClass DirectOwner) { return this.CreateClone(CloningScope, DirectOwner); }
        public new VisualShape CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((VisualShape)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public VisualShape PopulateFrom(VisualShape SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion
    }
}