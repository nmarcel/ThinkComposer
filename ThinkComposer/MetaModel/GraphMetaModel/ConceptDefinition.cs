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
// File   : ConceptDefinition.cs
// Object : Instrumind.ThinkComposer.MetaModel.GraphMetaModel.ConceptDefinition (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.07.14 Néstor Sánchez A.  Creation
//

using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;

/// Base abstractions for the user metadefinition of Graph schemas
namespace Instrumind.ThinkComposer.MetaModel.GraphMetaModel
{
    /// <summary>
    /// Represents the definition of a Concept type.
    /// </summary>
    [Serializable]
    public class ConceptDefinition : IdeaDefinition, IModelEntity, IModelClass<ConceptDefinition>
    {
        public const string CONDEF_CLUSTER_EMPTY_ID = "c5f684f8-88e9-4020-b3b8-32944a673266";

        public static readonly FormalPresentationElement ConceptDef_Cluster_None = new FormalPresentationElement("<NONE>", CONDEF_CLUSTER_EMPTY_ID, "");

        /// <summary>
        /// Default pictogram of this class.
        /// </summary>
        public static readonly ImageSource PredefinedDefaultPictogram = Display.GetAppImage("imtc_concept.png");

        /// <summary>
        /// Static Constructor.
        /// </summary>
        static ConceptDefinition()
        {
            ConceptDef_Cluster_None.GlobalId = new Guid(CONDEF_CLUSTER_EMPTY_ID);

            __ClassDefinitor = new ModelClassDefinitor<ConceptDefinition>("ConceptDefinition", IdeaDefinition.__ClassDefinitor, "Concept Definition",
                                                                          "Represents the definition of a Concept type.");
            __ClassDefinitor.DeclareProperty(__AncestorConceptDef);
            __ClassDefinitor.DeclareProperty(__AutomaticCreationConceptDef);
            __ClassDefinitor.DeclareProperty(__AutomaticCreationRelationshipDef);
            __ClassDefinitor.DeclareProperty(__AutomaticCreationPositioningMode);
            __ClassDefinitor.DeclareProperty(__AutomaticCreationPositioningIsRadialized);

            __AutomaticCreationConceptDef.ItemsSourceGetter = ((ctx) =>
            {
                var ConceptDef = ctx as ConceptDefinition;
                if (ConceptDef == null)
                    return null;

                return ConceptDef.OwnerDomain.ConceptDefinitions;
            });
            __AutomaticCreationConceptDef.ItemsSourceSelectedValuePath = FormalElement.__TechName.TechName;
            __AutomaticCreationConceptDef.ItemsSourceDisplayMemberPath = FormalElement.__Name.TechName;

            __AutomaticCreationRelationshipDef.ItemsSourceGetter = ((ctx) =>
            {
                var ConceptDef = ctx as ConceptDefinition;
                if (ConceptDef == null)
                    return null;

                return ConceptDef.OwnerDomain.RelationshipDefinitions;
            });
            __AutomaticCreationRelationshipDef.ItemsSourceSelectedValuePath = FormalElement.__TechName.TechName;
            __AutomaticCreationRelationshipDef.ItemsSourceDisplayMemberPath = FormalElement.__Name.TechName;

            if (ConceptDefinition.AutoLocatingModes == null)
                ConceptDefinition.AutoLocatingModes = General.GetEnumMembers<EAutoPositioningMode>().ToArray();

            __AutomaticCreationPositioningMode.ItemsSourceGetter = ((ctx) => ConceptDefinition.AutoLocatingModes);
            __AutomaticCreationPositioningMode.ItemsSourceSelectedValuePath = "Item1";
            __AutomaticCreationPositioningMode.ItemsSourceDisplayMemberPath = "Item2";
            __AutomaticCreationPositioningMode.BindingValueConverter =
                new GenericConverter<EAutoPositioningMode, Tuple<EAutoPositioningMode, string, string>>(alm => ConceptDefinition.AutoLocatingModes.First(reg => reg.Item1 == alm),
                                                                                                        tup => tup.Item1);
        }

        private static Tuple<EAutoPositioningMode, string, string>[] AutoLocatingModes = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="OwnerComposite">Composite Idea definition owning this new one.</param>
        /// <param name="AncestorConceptDef">Concept Idea definition from which this one inherits from.</param>
        /// <param name="Name">Name of the ConceptDefinition.</param>
        /// <param name="TechName">Technical Name of the ConceptDefinition.</param>
        /// <param name="RepresentativeShape">Shape visually representing the ConceptDefinition.</param>
        /// <param name="Summary">Summary of the ConceptDefinition.</param>
        /// <param name="Pictogram">Image representing the ConceptDefinition.</param>
        public ConceptDefinition(IdeaDefinition OwnerComposite, ConceptDefinition AncestorConceptDef,
                                 string Name, string TechName, string RepresentativeShape, string Summary = "", ImageSource Pictogram = null)
            : base(OwnerComposite, Name, TechName, RepresentativeShape, Summary, Pictogram)
        {
            this.AncestorConceptDef = AncestorConceptDef;

            this.DefaultSymbolFormat = new VisualSymbolFormat(Brushes.WhiteSmoke, Brushes.DimGray);
            this.DefaultSymbolFormat.InitialWidth = ApplicationProduct.ProductDirector.DefaultConceptBodySymbolSize.Width;
            this.DefaultSymbolFormat.InitialHeight = ApplicationProduct.ProductDirector.DefaultConceptBodySymbolSize.Height;

            this.AutomaticCreationConceptDef_ = this;
            this.AutomaticCreationRelationshipDef_ = this.OwnerDomain.RelationshipDefinitions.FirstOrDefault();
        }

        /// <summary>
        /// Internal Constructor for Agents and Cloning.
        /// </summary>
        internal ConceptDefinition()
        {
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Graphic representation of the object.
        /// </summary>
        public override ImageSource Pictogram { get { return base.Pictogram.NullDefault(this.DefaultPictogram); } set { base.Pictogram = value; } }

        /// <summary>
        /// Returns the predefined default pictogram of this Concept definition.
        /// </summary>
        public override ImageSource DefaultPictogram { get { return PredefinedDefaultPictogram; } }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// References the ancestor Concept definition of this one.
        /// </summary>
        public ConceptDefinition AncestorConceptDef { get { return __AncestorConceptDef.Get(this); } set { __AncestorConceptDef.Set(this, value); } }
        protected ConceptDefinition AncestorConceptDef_ = null;
        public static readonly ModelPropertyDefinitor<ConceptDefinition, ConceptDefinition> __AncestorConceptDef =
                   new ModelPropertyDefinitor<ConceptDefinition, ConceptDefinition>("AncestorConceptDef", EEntityMembership.External, null, EPropertyKind.Common, ins => ins.AncestorConceptDef_, (ins, val) => ins.AncestorConceptDef_ = val, false, true,
                                                                                    "Ancestor Concept Definition", "References the ancestor Concept definition of this one.");
        public override IdeaDefinition AncestorIdeaDef { get { return this.AncestorConceptDef; } }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Definition of the Concept to be automatically created.
        /// </summary>
        public ConceptDefinition AutomaticCreationConceptDef
        {
            get { return __AutomaticCreationConceptDef.Get(this); } 
            set { __AutomaticCreationConceptDef.Set(this, value); }
        }
        protected ConceptDefinition AutomaticCreationConceptDef_ = null;
        public static readonly ModelPropertyDefinitor<ConceptDefinition, ConceptDefinition> __AutomaticCreationConceptDef =
                   new ModelPropertyDefinitor<ConceptDefinition, ConceptDefinition>("AutomaticCreationConceptDef", EEntityMembership.External, null, EPropertyKind.Common, ins => ins.AutomaticCreationConceptDef_, (ins, val) => { ins.AutomaticCreationConceptDef_ = val; },  false, false,
                                                                                    "Automatic Creation Definition of Concept to Create", "Definition of the Concept to be automatically created.");

        /// <summary>
        /// Definition of the Relationship to associate Concepts with the automatically created ones.
        /// </summary>
        public RelationshipDefinition AutomaticCreationRelationshipDef
        { 
            get { return __AutomaticCreationRelationshipDef.Get(this); }
            set { __AutomaticCreationRelationshipDef.Set(this, value); }
        }
        protected RelationshipDefinition AutomaticCreationRelationshipDef_ = null;
        public static readonly ModelPropertyDefinitor<ConceptDefinition, RelationshipDefinition> __AutomaticCreationRelationshipDef =
                   new ModelPropertyDefinitor<ConceptDefinition, RelationshipDefinition>("AutomaticCreationRelationshipDef", EEntityMembership.External, null, EPropertyKind.Common, ins => ins.AutomaticCreationRelationshipDef_, (ins, val) => ins.AutomaticCreationRelationshipDef_ = val, false, false,
                                                                                         "Automatic Creation Associating Relationship Definition", "Definition of the Relationship to associate Concepts with the automatically created ones.");

        /// <summary>
        /// Idicates the ways to accommodate the symbols of automatically created Concepts.
        /// </summary>
        public EAutoPositioningMode AutomaticCreationPositioningMode
        {
            get { return __AutomaticCreationPositioningMode.Get(this); }
            set { __AutomaticCreationPositioningMode.Set(this, value); }
        }
        protected EAutoPositioningMode AutomaticCreationPositioningMode_ = EAutoPositioningMode.VerticalAlternated;
        public static readonly ModelPropertyDefinitor<ConceptDefinition, EAutoPositioningMode> __AutomaticCreationPositioningMode =
                   new ModelPropertyDefinitor<ConceptDefinition, EAutoPositioningMode>("AutomaticCreationPositioningMode", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.AutomaticCreationPositioningMode_, (ins, val) => ins.AutomaticCreationPositioningMode_ = val, false, false,
                                                                                       "Automatic Creation Positioning Mode", "Idicates the ways to accommodate the symbols of automatically created Concepts.");

        /// <summary>
        /// Indicates to position automatically created Concepts around in a radialized (semi elliptical) style.
        /// </summary>
        public bool AutomaticCreationPositioningIsRadialized
        {
            get { return __AutomaticCreationPositioningIsRadialized.Get(this); }
            set { __AutomaticCreationPositioningIsRadialized.Set(this, value); }
        }
        protected bool AutomaticCreationPositioningIsRadialized_ = true;
        public static readonly ModelPropertyDefinitor<ConceptDefinition, bool> __AutomaticCreationPositioningIsRadialized =
                   new ModelPropertyDefinitor<ConceptDefinition, bool>("AutomaticCreationPositioningIsRadialized", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.AutomaticCreationPositioningIsRadialized_, (ins, val) => ins.AutomaticCreationPositioningIsRadialized_ = val, false, false,
                                                                       "Automatic Creation Positioning Is Radial", "Indicates to position automatically created Concepts around in a radial (semi elliptical) style.");

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelClass<ConceptDefinition> Members

        public new MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public new ModelClassDefinitor<ConceptDefinition> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly new ModelClassDefinitor<ConceptDefinition> __ClassDefinitor = null;

        public new ConceptDefinition CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((ConceptDefinition)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public ConceptDefinition PopulateFrom(ConceptDefinition SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion

    }
}