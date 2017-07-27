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
// File   : RelationshipDefinition.cs
// Object : Instrumind.ThinkComposer.MetaModel.GraphMetaModel.RelationshipDefinition (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.09.11 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.Definitor.DefinitorUI;
using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;

/// Base abstractions for the user metadefinition of Graph schemas
namespace Instrumind.ThinkComposer.MetaModel.GraphMetaModel
{
    /// <summary>
    /// Represents the definition of a Relationship type.
    /// </summary>
    [Serializable]
    public class RelationshipDefinition : IdeaDefinition, IModelEntity, IModelClass<RelationshipDefinition>
    {
        public const string RELDEF_CLUSTER_EMPTY_ID = "e85ba8c4-a161-4581-9109-defc3c3a3513";

        public static readonly FormalPresentationElement RelationshipDef_Cluster_None = new FormalPresentationElement("<NONE>", RELDEF_CLUSTER_EMPTY_ID, "");

        /// <summary>
        /// Default pictogram of this class.
        /// </summary>
        public static readonly ImageSource PredefinedDefaultPictogram = Display.GetAppImage("imtc_relationship.png");

        /// <summary>
        /// Static Constructor.
        /// </summary>
        static RelationshipDefinition()
        {
            RelationshipDef_Cluster_None.GlobalId = new Guid(RELDEF_CLUSTER_EMPTY_ID);

            __ClassDefinitor = new ModelClassDefinitor<RelationshipDefinition>("RelationshipDefinition", IdeaDefinition.__ClassDefinitor, "Relationship Definition",
                                                                               "Represents the definition of a Relationship type.");
            __ClassDefinitor.DeclareProperty(__AncestorRelationshipDef);
            __ClassDefinitor.DeclareProperty(__OriginOrParticipantLinkRoleDef);
            __ClassDefinitor.DeclareProperty(__TargetLinkRoleDef);
            __ClassDefinitor.DeclareProperty(__DefaultConnectorsFormat);
            __ClassDefinitor.DeclareProperty(__IsDirectional);
            __ClassDefinitor.DeclareProperty(__IsSimple);
            __ClassDefinitor.DeclareProperty(__HideCentralSymbolWhenSimple);
            __ClassDefinitor.DeclareProperty(__ShowNameIfHidingCentralSymbol);
        }

        public static readonly VisualConnectorsFormat VisualFormatOriginSide = null;
        public static readonly VisualConnectorsFormat VisualFormatTargetSide = null;

        /// <summary>
        /// Constructor.
        /// Note: Initially two link role definitions, Origin and Target, are declared by default,
        /// plus implementing a default 'link' variant.
        /// Also, their links will be linkable to any other Idea regardless of its kind.
        /// </summary>
        /// <param name="OwnerComposite">Composite Idea definition owning this new one.</param>
        /// <param name="AncestorRelationshipDef">Relationship Idea definition from which this one inherits from.</param>
        /// <param name="Name">Name of the RelationshipDefinition.</param>
        /// <param name="TechName">Technical Name of the RelationshipDefinition.</param>
        /// <param name="RepresentativeShape">Shape visually representing the RelationshipDefinition.</param>
        /// <param name="Summary">Summary of the RelationshipDefinition.</param>
        /// <param name="Pictogram">Image representing the RelationshipDefinition.</param>
        /// <param name="OriginOrParticipantLinkRoleDef">Origin Link-Role Definition or the Participant when the Target Link-Role Definition is null.</param>
        /// <param name="TargetLinkRoleDef">Target Link-Role Definition. If null, then the Relationship is non-directional (only with a Participant Link-Role Definition).</param>
        public RelationshipDefinition(IdeaDefinition OwnerComposite, RelationshipDefinition AncestorRelationshipDef,
                                      string Name, string TechName, string RepresentativeShape, string Summary = "", ImageSource Pictogram = null,
                                      LinkRoleDefinition OriginOrParticipantLinkRoleDef = null, LinkRoleDefinition TargetLinkRoleDef = null)
            : base(OwnerComposite, Name, TechName, RepresentativeShape, Summary, Pictogram)
        {
            this.AncestorRelationshipDef = AncestorRelationshipDef;

            if (OriginOrParticipantLinkRoleDef == null)
                OriginOrParticipantLinkRoleDef = new LinkRoleDefinition(ERoleType.Origin, "Origin", "Origin", "Origin or participant of the relationship.");

            if (TargetLinkRoleDef == null)
                TargetLinkRoleDef = new LinkRoleDefinition(ERoleType.Target, "Target", "Target", "Target of the relationship.");

            this.OriginOrParticipantLinkRoleDef = OriginOrParticipantLinkRoleDef;
            this.TargetLinkRoleDef = TargetLinkRoleDef;

            this.OriginOrParticipantLinkRoleDef.OwnerRelationshipDef = this;
            this.TargetLinkRoleDef.OwnerRelationshipDef = this;

            // This at last, because requieres link-roles defined
            this.DefaultSymbolFormat = new VisualSymbolFormat(Brushes.LightGray, Brushes.Gray);
            this.DefaultSymbolFormat.InitialWidth = ApplicationProduct.ProductDirector.DefaultRelationshipCentralSymbolSize.Width;
            this.DefaultSymbolFormat.InitialHeight = ApplicationProduct.ProductDirector.DefaultRelationshipCentralSymbolSize.Height;
            this.DefaultSymbolFormat.SetTextFormat(ETextPurpose.Title, new TextFormat("Arial", 10, Brushes.Black, false, true, false, TextAlignment.Center));

            this.DefaultConnectorsFormat = new VisualConnectorsFormat(this.OwnerDomain.LinkRoleVariants.First(), Plugs.None,
                                                                      this.OwnerDomain.LinkRoleVariants.First(), Plugs.SimpleArrow, Brushes.Black);
        }

        /// <summary>
        /// Internal Constructor for Agents and Cloning.
        /// </summary>
        internal RelationshipDefinition()
        {
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Graphic representation of the object.
        /// </summary>
        public override ImageSource Pictogram { get { return base.Pictogram.NullDefault(this.DefaultPictogram); } set { base.Pictogram = value; } }

        /// <summary>
        /// Returns the predefined default pictogram of this Relationship definition.
        /// </summary>
        public override ImageSource DefaultPictogram { get { return PredefinedDefaultPictogram; } }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// References the ancestor Relationship definition of this one.
        /// </summary>
        public RelationshipDefinition AncestorRelationshipDef { get { return __AncestorRelationshipDef.Get(this); } set { __AncestorRelationshipDef.Set(this, value); } }
        protected RelationshipDefinition AncestorRelationshipDef_ = null;
        public static readonly ModelPropertyDefinitor<RelationshipDefinition, RelationshipDefinition> __AncestorRelationshipDef =
                   new ModelPropertyDefinitor<RelationshipDefinition, RelationshipDefinition>("AncestorRelationshipDef", EEntityMembership.External, null, EPropertyKind.Common, ins => ins.AncestorRelationshipDef_, (ins, val) => ins.AncestorRelationshipDef_ = val, false, true,
                                                                                              "Ancestor Relationship Definition", "References the ancestor Relationship definition of this one.");
        public override IdeaDefinition AncestorIdeaDef { get { return this.AncestorRelationshipDef; } }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Indicates whether this relationship if from an origin to a target, else is between equivalent participants.
        /// </summary>
        public bool IsDirectional { get { return __IsDirectional.Get(this); } set { __IsDirectional.Set(this, value); } }
        protected bool IsDirectional_ = true;
        public static readonly ModelPropertyDefinitor<RelationshipDefinition, bool> __IsDirectional =
                   new ModelPropertyDefinitor<RelationshipDefinition, bool>("IsDirectional", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.IsDirectional_, (ins, val) => ins.IsDirectional_ = val, true, false,
                                                                            "Is Directional", "Indicates whether this relationship if from an origin to a target, else is between equivalent participants.");

        /// <summary>
        /// Definition for the Origin/Source link role. This is the participant role in a non-directional relationship.
        /// </summary>
        public LinkRoleDefinition OriginOrParticipantLinkRoleDef { get { return __OriginOrParticipantLinkRoleDef.Get(this); } set { __OriginOrParticipantLinkRoleDef.Set(this, value); } }
        protected LinkRoleDefinition OriginOrParticipantLinkRoleDef_;
        public static readonly ModelPropertyDefinitor<RelationshipDefinition, LinkRoleDefinition> __OriginOrParticipantLinkRoleDef =
                   new ModelPropertyDefinitor<RelationshipDefinition, LinkRoleDefinition>("OriginOrParticipantLinkRoleDef", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.OriginOrParticipantLinkRoleDef_, (ins, val) => ins.OriginOrParticipantLinkRoleDef_ = val, true, false,
                                                                                          "Origin Or Participant Link Role Definition", "Definition for the Origin/Source link role. This is the participant role in a non-directional relationship.");

        /// <summary>
        /// Definition for the Target/Destination link role. This has no use in a non-directional relationship.
        /// </summary>
        public LinkRoleDefinition TargetLinkRoleDef { get { return __TargetLinkRoleDef.Get(this); } set { __TargetLinkRoleDef.Set(this, value); } }
        protected LinkRoleDefinition TargetLinkRoleDef_;
        public static readonly ModelPropertyDefinitor<RelationshipDefinition, LinkRoleDefinition> __TargetLinkRoleDef =
                   new ModelPropertyDefinitor<RelationshipDefinition, LinkRoleDefinition>("TargetLinkRoleDef", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.TargetLinkRoleDef_, (ins, val) => ins.TargetLinkRoleDef_ = val, true, false,
                                                                                          "Target Link Role Definition", "Definition for the Target/Destination link role. This has no use in a non-directional relationship.");

        /// <summary>
        /// Format to be initially applied to the related visual connectors.
        /// </summary>
        public VisualConnectorsFormat DefaultConnectorsFormat { get { return __DefaultConnectorsFormat.Get(this); } set { __DefaultConnectorsFormat.Set(this, value); } }
        protected VisualConnectorsFormat DefaultConnectorsFormat_;
        public static readonly ModelPropertyDefinitor<RelationshipDefinition, VisualConnectorsFormat> __DefaultConnectorsFormat =
                   new ModelPropertyDefinitor<RelationshipDefinition, VisualConnectorsFormat>("DefaultConnectorsFormat", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.DefaultConnectorsFormat_, (ins, val) => ins.DefaultConnectorsFormat_ = val, false, false,
                                                                                              "Default Connectors Format", "Format to be initially applied to the related visual connectors.");

        /// <summary>
        /// Indicates that only one target and one source can be established.
        /// </summary>
        public bool IsSimple { get { return __IsSimple.Get(this); } set { __IsSimple.Set(this, value); } }
        protected bool IsSimple_ = false;
        public static readonly ModelPropertyDefinitor<RelationshipDefinition, bool> __IsSimple =
                   new ModelPropertyDefinitor<RelationshipDefinition, bool>("IsSimple", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.IsSimple_, (ins, val) => ins.IsSimple_ = val, false, false,
                                                                            "Is Simple", "Indicates that only one target and one source Links can be established.");

        /// <summary>
        /// Hides the Central/Main-Symbol if possible (when only one target and one source are established).
        /// </summary>
        public bool HideCentralSymbolWhenSimple { get { return __HideCentralSymbolWhenSimple.Get(this); } set { __HideCentralSymbolWhenSimple.Set(this, value); } }
        protected bool HideCentralSymbolWhenSimple_;
        public static readonly ModelPropertyDefinitor<RelationshipDefinition, bool> __HideCentralSymbolWhenSimple =
                   new ModelPropertyDefinitor<RelationshipDefinition, bool>("HideCentralSymbolWhenSimple", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.HideCentralSymbolWhenSimple_, (ins, val) => ins.HideCentralSymbolWhenSimple_ = val, false, false,
                                                                            "Hide Central/Main-Symbol When Simple", "Hides the Central/Main-Symbol when the Relationship is defined as Simple.");

        /// <summary>
        /// Indicates to show Relationship name when hiding main-symbol.
        /// </summary>
        public bool ShowNameIfHidingCentralSymbol { get { return __ShowNameIfHidingCentralSymbol.Get(this); } set { __ShowNameIfHidingCentralSymbol.Set(this, value); } }
        protected bool ShowNameIfHidingCentralSymbol_;
        public static readonly ModelPropertyDefinitor<RelationshipDefinition, bool> __ShowNameIfHidingCentralSymbol =
                   new ModelPropertyDefinitor<RelationshipDefinition, bool>("ShowNameIfHidingCentralSymbol", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.ShowNameIfHidingCentralSymbol_, (ins, val) => ins.ShowNameIfHidingCentralSymbol_ = val, false, false,
                                                                            "Show Name If Hiding Central/Main-Symbol", "Indicates to show the Relationship name when hiding the Central/Main-Symbol.");

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns a role link with the supplied role type or the first participant.
        /// </summary>
        public LinkRoleDefinition GetLinkForRole(ERoleType RoleType)
        {
            if (this.IsDirectional && RoleType == ERoleType.Target)
                return this.TargetLinkRoleDef;

            return this.OriginOrParticipantLinkRoleDef;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Indicates whether this Relationship Definition can link supplied origin and target Ideas,
        /// considering its linking roles associability, variants and multiconnectability.
        /// </summary>
        public OperationResult<bool> CanLink(IdeaDefinition OriginIdea, IdeaDefinition TargetIdea,
                                      SimplePresentationElement OriginVariant = null, SimplePresentationElement TargetVariant = null)
        {
            General.ContractRequiresNotNull(OriginIdea, TargetIdea);

            if (!ValidateAssociabililty(OriginIdea, TargetIdea))
                return OperationResult.Failure<bool>("Origin and Target Ideas are not linkable.");

            if (!ValidateVariants(OriginIdea, OriginVariant, TargetIdea, TargetVariant))
                return OperationResult.Failure<bool>("Variants are invalid for the intended linking.");

            if (!ValidateMultiConnectability(OriginIdea, TargetIdea))
                return OperationResult.Failure<bool>("Already exists one non-multiconnectable role used.");

            return OperationResult.Success(true);
        }

        protected bool ValidateVariants(IdeaDefinition OriginIdea, SimplePresentationElement OriginVariant,
                                        IdeaDefinition TargetIdea, SimplePresentationElement TargetVariant)
        {
            // PENDING
            return true;
        }

        protected bool ValidateMultiConnectability(IdeaDefinition OriginIdea, IdeaDefinition TargetIdea)
        {
            // PENDING
            return true;
        }

        protected bool ValidateAssociabililty(IdeaDefinition OriginIdea, IdeaDefinition TargetIdea)
        {
            bool OriginIsAssociable = (this.OriginOrParticipantLinkRoleDef.AssociableIdeaDefs.Count < 1
                                       || this.OriginOrParticipantLinkRoleDef.AssociableIdeaDefs.Any(assocideadef => OriginIdea.IsConsidered(assocideadef)));

            bool TargetIsAssociable = (this.TargetLinkRoleDef.AssociableIdeaDefs.Count < 1
                                       || this.TargetLinkRoleDef.AssociableIdeaDefs.Any(assocideadef => TargetIdea.IsConsidered(assocideadef)));


            return (OriginIsAssociable && TargetIsAssociable);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a sample format (e.g.: for palettes) based on the visualization formats established.
        /// </summary>
        public override Drawing GetSampleDrawing(bool IsForCursor = false, bool ShowUpperLeftPointer = false)
        {
            return MasterDrawer.CreateDrawingSample(this, IsForCursor, ShowUpperLeftPointer);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelClass<RelationshipDefinition> Members

        public new MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public new ModelClassDefinitor<RelationshipDefinition> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly new ModelClassDefinitor<RelationshipDefinition> __ClassDefinitor = null;

        public new RelationshipDefinition CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((RelationshipDefinition)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public RelationshipDefinition PopulateFrom(RelationshipDefinition SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}