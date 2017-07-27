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
// File   : VisualRepresentation.cs
// Object : Instrumind.ThinkComposer.Model.VisualModel.VisualRepresentation (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.09.15 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;
using Instrumind.ThinkComposer.Model.GraphModel;

/// Base abstractions for the visual representation of Graph entities
namespace Instrumind.ThinkComposer.Model.VisualModel
{
    /// <summary>
    /// Groups Visual Elements to conform the exposed representation of an Idea.
    /// </summary>
    [Serializable]
    public abstract class VisualRepresentation : UniqueElement, IModelEntity, IModelClass<VisualRepresentation>
    {
        /// <summary>
        /// Limit for the nested levels of related visual-representations to show.
        /// </summary>
        // PENDING TO USE (Do not use the '*RepresentationsTrace' variables, else create a nesting count to test against):
        // public const int RELATED_NESTED_LEVEL_LIMIT = 8;

        /// <summary>
        /// Opacity for objects selected as vanishing
        /// </summary>
        public const double SELECTION_VANISHING_OPACITY = 0.35;

        /// <summary>
        /// Static Constructor.
        /// </summary>
        static VisualRepresentation()
        {
            __ClassDefinitor = new ModelClassDefinitor<VisualRepresentation>("VisualRepresentation", UniqueElement.__ClassDefinitor, "Visual Representation",
                                                                             "Groups Visual Elements to conform the exposed representation of an Idea.");
            __ClassDefinitor.DeclareProperty(__DisplayingView);
            __ClassDefinitor.DeclareProperty(__IsShortcut);
            //- __ClassDefinitor.DeclareProperty(__SynonymIndex);
            __ClassDefinitor.DeclareProperty(__AreRelatedTargetsShown);
            __ClassDefinitor.DeclareProperty(__AreRelatedOriginsShown);

            __ClassDefinitor.DeclareCollection(__VisualParts);
            __ClassDefinitor.DeclareCollection(__CustomFormatValues);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public VisualRepresentation(View DisplayingView)
        {
            this.VisualParts = new EditableList<VisualElement>(__VisualParts.TechName, this);

            this.DisplayingView = DisplayingView;

            this.Initialize();
        }

        /// <summary>
        /// Initializes the instance for use after creation or deserialization.
        /// </summary>
        [OnDeserialized] 
        private void Initialize(StreamingContext context = default(StreamingContext))
        {
            if (this.LocalFormatValues != null)
                this.LocalFormatValues.OnDeserialization(this); // Needed to correctly deserialize Dictionaries.

            if (this.CustomFormatValues == null)
                this.CustomFormatValues = new EditableDictionary<string, object>(this, __CustomFormatValues.TechName, ref this.LocalFormatValues);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the View visually containing this visual object.
        /// </summary>
        public View GetDisplayingView()
        {
            return this.DisplayingView;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the graphic parts (drawing and z-order) of this representation, plus any others needed to make it comprehensive.
        /// For example, a Relationship only is meaningful if, appart of showing its main-symbol and connectors, it also includes the connected symbols.
        /// </summary>
        public abstract IDictionary<Drawing, int> CreateIntegralGraphic();

        /// <summary>
        /// Updates the visual presentation of this representation on the owner View.
        /// </summary>
        public void Render()
        {
            this.DisplayingView.ShowRepresentator(this, null);
        }

        /// <summary>
        /// Clears this representation from the visual presentation on the owner View, disconnecting associated connectors.
        /// </summary>
        public void Clear()
        {
            // Symbols later
            var Parts = this.VisualParts.OrderBy(vp => vp is VisualSymbol).ToList();
            foreach (var Part in Parts)
            {
                var Connector = Part as VisualConnector;
                if (Connector != null)
                {
                    // This maybe redundant if symbol is delete first, because the prior symbol deletion also deletes related connectors,
                    // but it is better to delete any possible non symbol related (untied) connectors.
                    Connector.ClearElement();
                    Connector.Disconnect();
                }
                else
                {
                    var Symbol = Part as VisualSymbol;
                    if (Symbol != null)
                    {
                        var Origins = Symbol.TargetConnections.ToList();
                        var Targets = Symbol.OriginConnections.ToList();

                        foreach (var OriginConnection in Origins)
                        {
                            OriginConnection.ClearElement();

                            OriginConnection.Disconnect();
                            Symbol.TargetConnections.Remove(OriginConnection);
                        }

                        foreach (var TargetConnection in Targets)
                        {
                            TargetConnection.ClearElement();

                            TargetConnection.Disconnect();
                            Symbol.OriginConnections.Remove(TargetConnection);
                        }

                        var Complements = Symbol.AttachedComplements.ToList();
                        foreach (var Complement in Complements)
                        {
                            Symbol.OwnerRepresentation.DisplayingView.Clear(Complement);
                            Symbol.RemoveComplement(Complement);
                        }

                        Symbol.ClearElement();
                    }
                }

                this.VisualParts.Remove(Part);
            }

            this.DisplayingView.Clear(Parts);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Represented Idea by this visual representation.
        /// </summary>
        [Description("Represented Idea by this visual representation.")]
        public abstract Idea RepresentedIdea { get; }

        /// <summary>
        /// Gets or sets the View visually containing this visual object.
        /// </summary>
        public View DisplayingView { get { return __DisplayingView.Get(this); } set { __DisplayingView.Set(this, value); } }
        protected View DisplayingView_;
        public static readonly ModelPropertyDefinitor<VisualRepresentation, View> __DisplayingView =
                   new ModelPropertyDefinitor<VisualRepresentation, View>("DisplayingView", EEntityMembership.InternalBulk, null, EPropertyKind.Common, ins => ins.DisplayingView_, (ins, val) => ins.DisplayingView_ = val, false, false,
                                                                          "Displaying View", "View showing this visual representation.");

        /// <summary>
        /// Indicates that this visual object points to an Idea contained outside the current (Idea) Container.
        /// </summary>
        public bool IsShortcut { get { return __IsShortcut.Get(this); } set { __IsShortcut.Set(this, value); } }
        protected bool IsShortcut_ = false;
        public static readonly ModelPropertyDefinitor<VisualRepresentation, bool> __IsShortcut =
                   new ModelPropertyDefinitor<VisualRepresentation, bool>("IsShortcut", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.IsShortcut_, (ins, val) => ins.IsShortcut_ = val, false, false,
                                                                          "Is Shortcut", "Indicates that this visual object points to an Idea contained outside the current (Idea) Container.");

        /*- /// <summary>
        /// One-based index, indicating that this visual object is another representation of an already represented Idea within the same View. Zero when unique.
        /// </summary>
        public int SynonymIndex { get { return __SynonymIndex.Get(this); } set { __SynonymIndex.Set(this, value); } }
        protected int SynonymIndex_;
        public static readonly ModelPropertyDefinitor<VisualRepresentation, int> __SynonymIndex =
                   new ModelPropertyDefinitor<VisualRepresentation, int>("SynonymIndex", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.SynonymIndex_, (ins, val) => ins.SynonymIndex_ = val, false, false,
                                                                         "Synonym Index", "One-based index, indicating that this visual object is another representation of an already represented Idea within the same View. Zero when unique."); */

        /// <summary>
        /// Indicates whether the related Targets (and participants) representations are shown.
        /// </summary>
        public bool AreRelatedTargetsShown { get { return __AreRelatedTargetsShown.Get(this); } set { __AreRelatedTargetsShown.Set(this, value); } }
        protected bool AreRelatedTargetsShown_ = true;
        public static readonly ModelPropertyDefinitor<VisualRepresentation, bool> __AreRelatedTargetsShown =
                   new ModelPropertyDefinitor<VisualRepresentation, bool>("AreRelatedTargetsShown", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.AreRelatedTargetsShown_, (ins, val) => ins.AreRelatedTargetsShown_ = val, false, true,
                                                                          "Are Related Targets Shown", "Indicates whether the related Target (and participant) representations are shown.");

        /// <summary>
        /// Indicates whether the related Origins (and participants) representations are shown.
        /// </summary>
        public bool AreRelatedOriginsShown { get { return __AreRelatedOriginsShown.Get(this); } set { __AreRelatedOriginsShown.Set(this, value); } }
        protected bool AreRelatedOriginsShown_ = true;
        public static readonly ModelPropertyDefinitor<VisualRepresentation, bool> __AreRelatedOriginsShown =
                   new ModelPropertyDefinitor<VisualRepresentation, bool>("AreRelatedOriginsShown", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.AreRelatedOriginsShown_, (ins, val) => ins.AreRelatedOriginsShown_ = val, false, true,
                                                                          "Are Related Origins Shown", "Indicates whether the related Origin representations are shown.");

        /// <summary>
        /// DEPRECATED. Now use CustomFormatValues which is undoable/redoable.
        /// Contains the Formats applied locally, independently of the Idea Definition default.
        /// </summary>
        // IMPORTANT: This will be used as the source dictionary of the new CustomFormatValues editable dictionary.
        public Dictionary<string, object> LocalFormatValues = null;

        /// <summary>
        /// Contains the Formats applied locally, independently of the Idea Definition default.
        /// </summary>
        public EditableDictionary<string, object> CustomFormatValues { get; protected set; }
        public static ModelDictionaryDefinitor<VisualRepresentation, string, object> __CustomFormatValues =
                   new ModelDictionaryDefinitor<VisualRepresentation, string, object>("CustomFormatValues", EEntityMembership.InternalCoreExclusive, ins => ins.CustomFormatValues, (ins, coll) => ins.CustomFormatValues = coll,
                                                                                      "Custom-Format Values", "Contains the Formats applied locally, independently of the Idea Definition default.");

        /// <summary>
        /// The collection of visual parts composing this representation.
        /// </summary>
        internal EditableList<VisualElement> VisualParts { get; set; }
        public static ModelListDefinitor<VisualRepresentation, VisualElement> __VisualParts =
                   new ModelListDefinitor<VisualRepresentation, VisualElement>("VisualParts", EEntityMembership.InternalCoreExclusive, ins => ins.VisualParts, (ins, coll) => ins.VisualParts = coll,
                                                                               "Visual Parts", "The collection of visual parts composing this representation.");

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the major symbol of this representation.
        /// The body symbol for Concepts, or the main-symbol for Relationships.
        /// </summary>
        [Description("Gets the major symbol of this representation. " +
                     "The body symbol for Concepts, or the main-symbol for Relationships.")]
        public VisualSymbol MainSymbol
        {
            get
            {
                // IMPORTANT: NOTICE THAT THIS ONLY WORKS FOR SYMBOLS BEING ADDED ONLY ONCE.
                //            NO UPDATES ARE DETECTED IN THE CURRENT STATE.
                if (this.MainSymbol_ == null)
                    foreach(var Part in this.VisualParts)
                        if (Part is VisualSymbol)
                           this.MainSymbol_ = (VisualSymbol)Part;

                /*T if (this.MainSymbol_ == null)
                    Console.WriteLine("Representation has no symbol: {0}.", this); */

                return this.MainSymbol_;
            }
        }
        [NonSerialized]
        private VisualSymbol MainSymbol_ = null;

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Indicates wether this visual representation is selected for later applying a command.
        /// </summary>
        public bool IsSelected
        {
            get { return this.IsSelected_; }
            set
            {
                if (this.IsSelected_ == value)
                    return;

                if (!this.DisplayingView.EditEngine.IsVariating)
                    throw new UsageAnomaly("Selection changes must be applied within a Command");

                this.IsSelected_ = value;
                
                // Notice that this overwrites a selection sate made by another Idea representation
                if (this.RepresentedIdea != null)
                    this.RepresentedIdea.IsSelected = this.IsSelected_;

                this.NotifyPropertyChange("IsSelected");
            }
        }
        [NonSerialized]
        protected bool IsSelected_ = false;

        /// <summary>
        /// Indicates wether this visual representation is Vanished for later deletion.
        /// </summary>
        public bool IsVanished
        {
            get { return this.IsVanished_; }
            set
            {
                if (this.IsVanished_ == value)
                    return;

                if (!this.DisplayingView.EditEngine.IsVariating)
                    throw new UsageAnomaly("Vanishing changes must be applied within a Command");

                this.IsVanished_ = value;

                // Notice that this overwrites a vanishing sate made by another Idea representation
                this.RepresentedIdea.IsVanished = this.IsVanished_;

                this.NotifyPropertyChange("IsVanished");
            }
        }
        [NonSerialized]
        protected bool IsVanished_ = false;

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Add the supplied visual element to the collection of parts.
        /// </summary>
        public void AddVisualPart(VisualElement Part)
        {
            if (this.VisualParts.Count > 0 && Part.RepresentationPartType == EVisualRepresentationPart.ConceptBodySymbol)
                throw new UsageAnomaly("A Concept representation just can have one part: A Concept body-symbol part.");

            if (Part.RepresentationPartType == EVisualRepresentationPart.RelationshipCentralSymbol &&
                this.VisualParts.Count(part => part.RepresentationPartType == EVisualRepresentationPart.RelationshipCentralSymbol) > 0)
                throw new UsageAnomaly("A Relationship representation can have at most one part of kind Relationship Main-Symbol.");

            this.VisualParts.Add(Part);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the interrelations (targeted and originated connections) to and from this symbol.
        /// Intended for use in a navigation UI control.
        /// </summary>
        public IEnumerable<object> InterrelatedRepresentations
        {
            get
            {
                yield return new SimplePresentationElement("Sources", "Sources", "Objects referencing to this symbol", Display.GetAppImage("rel_sources.png"));

                foreach (var Representator in this.OriginRepresentations)
                    yield return Representator;

                yield return new SimplePresentationElement("Targets", "Targets", "Objects referenced from this symbol", Display.GetAppImage("rel_targets.png"));

                foreach (var Representator in this.TargetRepresentations)
                    yield return Representator;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the origin representations which targets to this one.
        /// </summary>
        public IEnumerable<VisualRepresentation> OriginRepresentations
        {
            get
            {
                var Result = this.MainSymbol.OriginConnections
                                    .Select(vrep => vrep.OriginSymbol.OwnerRepresentation)
                                        .Distinct();
                return Result;
            }
        }

        /// <summary>
        /// Safely, for recursive consumers, gets the origin representations which targets to this one.
        /// </summary>
        public IEnumerable<VisualRepresentation> SafeOriginRepresentations
        {
            get
            {
                var Trace = this.DisplayingView.OriginRepresentationsTrace;
                if (Trace.Contains(this))
                    return Enumerable.Empty<VisualRepresentation>();

                Trace.Add(this);

                var Result = this.MainSymbol.OriginConnections
                                    .Select(vrep => vrep.OriginSymbol.OwnerRepresentation)
                                        .Distinct();
                return Result;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the target representations originated from this one.
        /// </summary>
        public IEnumerable<VisualRepresentation> TargetRepresentations
        {
            get
            {
                var Result = this.MainSymbol.TargetConnections
                                    .Select(vrep => vrep.TargetSymbol.OwnerRepresentation)
                                        .Distinct();
                return Result;
            }
        }

        /// <summary>
        /// Safely, for recursive consumers, gets the target representations originated from this one.
        /// </summary>
        public IEnumerable<VisualRepresentation> SafeTargetRepresentations
        {
            get
            {
                var Trace = this.DisplayingView.TargetRepresentationsTrace;
                if (Trace.Contains(this))
                    return Enumerable.Empty<VisualRepresentation>();

                Trace.Add(this);

                var Result = this.MainSymbol.TargetConnections
                                    .Select(vrep => vrep.TargetSymbol.OwnerRepresentation)
                                        .Distinct();
                return Result;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the current collection of visual representations that are exclusively pointed from this one.
        /// </summary>
        public IEnumerable<VisualRepresentation> GetExclusivePointedVisualRepresentators(View TargetView)
        {
            var Result = new List<VisualRepresentation>();

            // Search over all of the target representators
            foreach (var TargetRepresentator in this.TargetRepresentations.Where(vrep => vrep.DisplayingView == TargetView))
            {
                bool IsPointedFromAnotherSymbol = false;

                // Seach over all of the origin representators of the currently evaluated one
                // Notice that auto-references are considered.
                foreach(var OriginRepresentator in TargetRepresentator.OriginRepresentations.Where(vrep => vrep.DisplayingView == TargetView))
                    if (OriginRepresentator != this && OriginRepresentator != TargetRepresentator)
                    {
                        IsPointedFromAnotherSymbol = true;
                        break;
                    }

                // Add to the result list the currently evaluated representator because it is only (exclusively) pointed from "this" representator.
                if (!IsPointedFromAnotherSymbol)
                    Result.Add(TargetRepresentator);
            }

            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelClass<VisualRepresentation> Members

        public new MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public new ModelClassDefinitor<VisualRepresentation> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly new ModelClassDefinitor<VisualRepresentation> __ClassDefinitor = null;

        public override object CreateCopy(ECloneOperationScope CloningScope, IMModelClass DirectOwner) { return this.CreateClone(CloningScope, DirectOwner); }
        public new VisualRepresentation CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((VisualRepresentation)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public VisualRepresentation PopulateFrom(VisualRepresentation SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public override string ToString()
        {
            return "Visual Representation of Idea '" + this.RepresentedIdea.ToString() + "' on View '" + this.DisplayingView.ToString() + "'";
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}