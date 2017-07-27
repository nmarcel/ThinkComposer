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
// File   : ModelClassController.cs
// Object : Instrumind.Common.EntityDefinition.ModelClassController (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.07.24 Néstor Sánchez A.  Creation
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Instrumind.Common;
using Instrumind.Common.EntityBase;

/// Provides structures, components and services for defining and exposing business entities.
namespace Instrumind.Common.EntityDefinition
{
    /// <summary>
    /// Manages and describes, at a static level, the member properties belonging to a model class.
    /// Intended to be used as a contract for edition, persistence mapping and life cycle control.
    /// </summary>
    /// <typeparam name="TModelClass">Type of the model class being controlled.</typeparam>
    public class ModelClassDefinitor<TModelClass> : MModelClassDefinitor where TModelClass : class, IMModelClass
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public ModelClassDefinitor(string TechName, MModelClassDefinitor AncestorDefinitor = null,
                                   string Name = "", string Summary = "" /*- ,
                                   Func<TModelClass, IList<string>> InstanceValidator = null*/)
             : base(TechName, Name, Summary, typeof(TModelClass), AncestorDefinitor)
        {
            this.RegisterDefinitor(this);
            //- this.InstanceValidator = InstanceValidator;
        }

        /// <summary>
        /// Validator of the model class instance.
        /// </summary>
        public Func<TModelClass, IList<string>> InstanceValidator { get; set; }

        /// <summary>
        /// Registers the new supplied model class property definitor.
        /// </summary>
        public void DeclareProperty(MModelPropertyDefinitor PropertyDefinitor)
        {
            if (this.Properties_.ContainsKey(PropertyDefinitor.TechName))
                throw new UsageAnomaly("Property definitor is already registered for the model class.", PropertyDefinitor);

            // Attach this declarator to its owner/class, and for generate qualified-names, plus maybe assemble interceptors.
            PropertyDefinitor.AssignDeclarator(this);

            // Register property
            this.Properties_.Add(PropertyDefinitor.TechName, PropertyDefinitor);
            MModelClassDefinitor.DeclaredMemberDefinitors_.Add(PropertyDefinitor.QualifiedTechName, PropertyDefinitor);
        }

        /// <summary>
        /// Registers the new supplied model class collection definitor.
        /// </summary>
        public void DeclareCollection(MModelCollectionDefinitor CollectionDefinitor)
        {
            if (this.Collections_.ContainsKey(CollectionDefinitor.TechName))
                throw new UsageAnomaly("Collection definitor is already registered for the model class.", CollectionDefinitor);

            // Attach this declarator to its owner/class, and for generate qualified-names, plus maybe assemble interceptors.
            CollectionDefinitor.AssignDeclarator(this);

            // Register property
            this.Collections_.Add(CollectionDefinitor.TechName, CollectionDefinitor);
            MModelClassDefinitor.DeclaredMemberDefinitors_.Add(CollectionDefinitor.QualifiedTechName, CollectionDefinitor);
        }

        /// <summary>
        /// Evaluates the supplied entity and returns its validation result.
        /// </summary>
        public override IList<string> ValidateEntity(IMModelClass Instance) { return this.Validate(Instance as TModelClass); }

        /// <summary>
        /// Evaluates the supplied instance and returns its validation result.
        /// </summary>
        public IList<string> Validate(TModelClass Instance)
        {
            if (Instance == null)
                return null;

            var Result = new List<string>();

            if (this.AncestorDefinitor != null)
            {
                var Self = this.AncestorDefinitor.ValidateEntity(Instance);
                if (Self != null)
                    Result.AddRange(Self);
            }

            if (this.InstanceValidator != null)
            {
                var Base = this.InstanceValidator(Instance);
                if (Base != null)
                    Result.AddRange(Base);
            }

            return Result;
        }

        /// <summary>
        /// Populates properties of a target model instance, plus returning it, with these from the supplied one.
        /// </summary>
        /// <param name="Target">Target model instance for which populate/overwrite property values.</param>
        /// <param name="Source">Source model instance from which get property values.</param>
        /// 
        /// <param name="TargetOwner">Model instance owning the Target (intended to update owner references of the Source).</param>
        /// <param name="CloningScope">Indicates to make a copy of the populated value through the dependant object hierarchy.</param>
        /// <param name="IsForCloning">Indicates whether the population will be performed on a clone.</param>  // See below
        /// <param name="AsActive">Indicates whether the new clone will be active (i.e. will notify changes and store them for undo/redo).</param>
        /// <param name="MemberNames">Optional explicit member names to be populated (when empty, all members are populated).</param>
        /// <returns>The populated target model instance.</returns>
        public TModelClass PopulateInstance(TModelClass Target, TModelClass Source, IMModelClass TargetOwner,
                                            ECloneOperationScope CloningScope = ECloneOperationScope.Slight,
                                            bool IsForCloning = false, bool AsActive = true, params string[] MemberNames)
        {
            General.ContractRequiresNotNull(Target, Source);

            if (!AsActive)
                MModelClassDefinitor.RegisterPassiveInstance(Target);

            /*T
            var SouCD = Source.ClassDefinition;
            var TarCD = Target.ClassDefinition;
            var AreEq = (SouCD == TarCD);

            string PopulatingKind = null;

            var PopulatingPrefix = " ".Replicate(PopulationLevel * 2);

            if (Target is IMModelEntityAgent)
                PopulatingKind = "ENT->AGT";
            else
                PopulatingKind = "AGT->ENT";

            Console.WriteLine(PopulatingPrefix + "AT-POPULATE (" + PopulatingKind + ")... Source=[{0}:{1}], Target=[{2}:{3}], ClassDefsAreEqual=[{4}], Scope={5}, Members={6}",
                              SouCD, Source.GetHashCode(), TarCD, Target.GetHashCode(), AreEq, CloningScope,
                              (MemberNames == null || MemberNames.Length < 1 ? "<ALL>" : MemberNames.GetConcatenation(null,";"))); */

            if (!Target.ClassDefinition.DeclaringType.InheritsFrom(this.DeclaringType))
                throw new UsageAnomaly("The supplied Target instance is not defined by this model class definitor",
                                       new DataWagon("target-ClassDef", this).Add("Target instance", Target));

            if (!this.IsCompatibleClassDefinition(Source.ClassDefinition))
                throw new UsageAnomaly("Cannot populate a model class instance from an incompatible one (must be of the same type or descendant of it)",
                                       new DataWagon("target-ClassDef", this).Add("source-ClassDef", Source.ClassDefinition));

            // Determine properties to be populated
            var TargetProperties = (MemberNames == null || MemberNames.Length < 1
                                    ? Source.ClassDefinition.Properties    // IMPORTANT: Do not use 'this.Collections' (that way descendants members are not considered!)
                                    : Source.ClassDefinition.Properties.Where(prop => prop.TechName.IsOneOf(MemberNames)));

            // First, populate relevant properties, such as owner referencers
            var InitialProperties = TargetProperties.Where(prop => prop.ReferencesOwner.HasValue).ToArray();

            // PENDING: Treat Sub-Owners properly (reappoint cloned references to the cloned sub-owners, not to the original ones)
            PopulateProperties(Target, Source, (CloningScope == ECloneOperationScope.DeepAndEquivalent ? null : TargetOwner),
                               CloningScope, IsForCloning, InitialProperties);

            // Second, populate all collections (some of the remaining properties depends on some of them)
            var TargetCollections = (MemberNames == null || MemberNames.Length < 1
                                     ? Source.ClassDefinition.Collections   // IMPORTANT: Do not use 'this.Collections' (that way descendants members are not considered!)
                                     : Source.ClassDefinition.Collections.Where(coll => coll.TechName.IsOneOf(MemberNames))).ToArray();
            foreach (var CollDef in TargetCollections)
            {
                var TargetCollection = (EditableCollection)CollDef.Read(Target);
                var SourceCollection = (EditableCollection)CollDef.Read(Source);

                /*T if (TargetCollection != null && TargetCollection.Name == "MarkerDefinitions")
                {
                    Console.WriteLine(CallingPrefix + "........................................................................");
                    Console.WriteLine(CallingPrefix + "MarkerDefs are the same: {0}", TargetCollection.IsEqual(SourceCollection));
                    Console.WriteLine(CallingPrefix + "Source: HC={0}, ItemsCnt={1}", SourceCollection.GetHashCode(), SourceCollection.Count);
                    Console.WriteLine(CallingPrefix + "Target: HC={0}, ItemsCnt={1}", TargetCollection.GetHashCode(), TargetCollection.Count);
                } */

                /*T if (TargetCollection != null && TargetCollection.Name == "TextFormats")
                {
                    Console.WriteLine("Populating coll TextFormats >>>>>>>>>>>>>>>>>>>>>");
                    Console.WriteLine("SourceEnt: {0}, TargetEnt: {1}", Source.GetHashCode(), Target.GetHashCode());
                    Console.WriteLine("TextFormats are the same: {0}", TargetCollection.IsEqual(SourceCollection));
                    Console.WriteLine("SourceColl: HC={0}, ItemsCnt={1}", SourceCollection.GetHashCode(), SourceCollection.Count);
                    Console.WriteLine("TargetColl: HC={0}, ItemsCnt={1}", TargetCollection.GetHashCode(), TargetCollection.Count);
                    Console.WriteLine("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
                } */

                if (SourceCollection == null)
                {
                    CollDef.Write(Target, SourceCollection);
                    continue;
                }

                // IMPORTANT: Remember that SourceCollection, if not cloned,
                // will still be pointing to its original Variating-Instance.

                var DoClone = CollDef.IsCloneableFor(CloningScope, Source);
                if (DoClone.Item1)
                {
                    //T PopulationLevel++;
                    CollDef.Write(Target, SourceCollection.DuplicateFor(Target as IModelEntity, Target, DoClone.Item2));
                    //T PopulationLevel--;
                }
                else
                    CollDef.Write(Target, SourceCollection);    //? Consider: TargetCollection.UpdateContentFrom(SourceCollection);
            }

            // Third, populate the remaining properties (remember that some of these depend on collections)
            var RemainingProperties = TargetProperties.Where(prop => !prop.ReferencesOwner.HasValue).ToArray();

            PopulateProperties(Target, Source, (CloningScope == ECloneOperationScope.DeepAndEquivalent ? null : TargetOwner),
                               CloningScope, IsForCloning, RemainingProperties);

            if (CloningScope != ECloneOperationScope.DeepAndEquivalent)
            {
                // Generate new GUID for Unique-Element's Global-Id.
                var TargetElement = Target as UniqueElement;
                if (TargetElement != null)
                    TargetElement.GlobalId = Guid.NewGuid();
            }

            return Target;
        }

        private static void PopulateProperties(TModelClass Target, TModelClass Source, IMModelClass TargetOwner,
                                               ECloneOperationScope CloningScope, bool IsForCloning, MModelPropertyDefinitor[] TargetProperties)
        {
            foreach (var PropDef in TargetProperties)
            {
                var SourceValue = PropDef.Read(Source);

                /*T if (//T PropDef.TechName.EndsWith("LinkRoleDef") ||
                    PropDef.TechName == "OwnerRelationshipDef")
                    Console.WriteLine("debug!"); */

                /*T if (PropDef.TechName == "ForegroundBrush")
                {
                    Console.WriteLine("Populating prop ForegroundBrush >>>>>>>>>>>>>>>>>>>>>");
                    Console.WriteLine("SourceEnt: {0}, TargetEnt: {1}", Source.GetHashCode(), Target.GetHashCode());
                    if (PropDef.IsStoreBoxBased)
                    {
                        Console.WriteLine("StoreBox-Source: {0}", PropDef.GetStoreBoxContainer(Source).GetHashCode());
                        Console.WriteLine("StoreBox-Target: {0}", PropDef.GetStoreBoxContainer(Target).GetHashCode());
                    }
                    Console.WriteLine("SourceVal: HC={0}", SourceValue.GetHashCode());
                    var TarVal = PropDef.Read(Target);
                    TarVal = TarVal.NullDefault("<NULL>");
                    Console.WriteLine("TargetVal: HC={0} (to be overwritten)", TarVal.GetHashCode());
                    Console.WriteLine("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
                } */

                var DoClone = PropDef.IsCloneableFor(CloningScope, Source);

                if (SourceValue is IMModelClass && DoClone.Item1)
                {
                    //T PopulationLevel++;
                    SourceValue = ((IMModelClass)SourceValue).CreateCopy(DoClone.Item2, Target);
                    //T PopulationLevel--;
                }

                if (PropDef.ReferencesOwner.HasValue && TargetOwner != null)
                {
                    // NOTE: See ModelFixes.ModelRev7_FixOwnerReferences method, which resulted in this piece of code for reassign Owners.
                    var SourceOwnership = SourceValue as MOwnership;
                    var Owner = (SourceOwnership == null ? SourceValue : SourceOwnership.Owner);

                    if (Owner != null && Owner.GetType() == TargetOwner.GetType())
                        if (SourceOwnership != null)
                            SourceValue = SourceOwnership.CreateClone(TargetOwner);
                        else
                            SourceValue = TargetOwner;
                }

                // Refinements...
                // Needed because the initial clone only has duplicated the root properties (shallow copy)
                if (IsForCloning)
                {
                    var SourceAssignment = SourceValue as MAssignment;
                    if (SourceAssignment != null)
                    {
                        if (SourceAssignment.IsLocal && (SourceAssignment.AssignedValue is IMModelClass) && DoClone.Item1)
                            SourceAssignment = ((IMModelClass)SourceAssignment.AssignedValue).CreateCopy(DoClone.Item2, Target).Assign(true);
                        else
                            SourceAssignment = ((MAssignment)SourceValue).CreateClone();

                        SourceValue = SourceAssignment;
                    }

                    if (PropDef.IsStoreBoxBased)
                    {
                        var ClonedStore = PropDef.GetStoreBoxContainer(Target).CreateClone();
                        PropDef.SetStoreBoxBaseContainer(Target, ClonedStore);
                    }
                }

                PropDef.Write(Target, SourceValue);
            }
        }

        //T private static int PopulationLevel;
     }
}