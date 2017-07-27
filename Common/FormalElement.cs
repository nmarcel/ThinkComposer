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
// File   : FormalElement.cs
// Object : Instrumind.Common.FormalElement (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.07.02 Néstor Sánchez A.  Creation
//

using System;
using System.ComponentModel;
using System.Windows.Documents;

using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization.Widgets;

/// Library of general purpose components and features across Instrumind products.
namespace Instrumind.Common
{
    /// <summary>
    /// Standard entity type, globally unique, with Name, Tech-Name, Summary, Tech-Spec, plus rich-text Description and Version information.
    /// This type is "Storable" (can be persisted and then accessed by primary key).
    /// Also, entities can contain summary, classification and version information.
    /// </summary>
    [Serializable]
    public class FormalElement : UniqueElement, IFormalizedElement, ITechSpecifier, IModelEntity, IModelClass<FormalElement>
    {
        /// <summary>
        /// Static Constructor.
        /// </summary>
        static FormalElement()
        {
            __ClassDefinitor = new ModelClassDefinitor<FormalElement>("FormalElement", UniqueElement.__ClassDefinitor, "Formal Element",
                                                                      "Standard entity type, globally unique, with Name, Tech-Name, Summary, Tech-Spec, plus rich-text Description and Version information.");
            __ClassDefinitor.DeclareProperty(__Name);
            __ClassDefinitor.DeclareProperty(__TechName);
            __ClassDefinitor.DeclareProperty(__Summary);
            __ClassDefinitor.DeclareProperty(__TechSpec);
            __ClassDefinitor.DeclareProperty(__Description);
            __ClassDefinitor.DeclareProperty(__Classification);
            __ClassDefinitor.DeclareProperty(__Version);

            __Name.IsIdentificator = true;
            __Name.IsLinkeable = true;

            __TechName.IsIdentificator = true;
            __TechName.IsLinkeable = true;

            __Summary.IsLinkeable = true;

            __TechSpec.IsLinkeable = true;

            // POSTPONED: Until be supported by the internal-link editor
            // __Description.IsLinkeable = true;

            __Description.HasRichContent = true;

            __ClassDefinitor.InstanceValidator = (ins => ins.Name.IsAbsent() ? ("The '" + __Name.Name + "' field is empty").IntoList() : null);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public FormalElement(string Name, string TechName, string Summary = "", bool IsDescribed = false, bool IsClassified = false, bool IsVersioned = false)
        {
            General.ContractRequiresNotNull(Name, Summary);

            this.Name = Name;
            this.TechName = TechName.NullDefault(Name);
            this.Summary = Summary;

            if (IsDescribed && this.Description.IsAbsent())
                this.Description = RichTextEditor.ABSENT_TEXT;

            if (IsClassified)
                this.Classification = new ClassificationCard(AppExec.ApplicationContentTypeCode);

            if (IsVersioned && this.Version == null)
                this.Version = new VersionCard();
        }

        /// <summary>
        /// Protected Constructor for Agent descendants.
        /// </summary>
        protected FormalElement()
        {
            //T Console.WriteLine("Ctor.");
        }

        /// <summary>
        /// Name of the object. Must be schema unique. Intended for user-level usage.
        /// </summary>
        public string Name
        {
            get { return __Name.Get(this); }
            set
            {
                value = value.NullDefault("").Trim();

                var CodeWasEquivalentToName = (this.TechName == this.Name.TextToIdentifier());
                if (__Name.Set(this, value))
                {
                    if (CodeWasEquivalentToName)
                        this.TechName = this.Name.TextToIdentifier();

                    this.NotifyPropertyChange("NameCaption");
                }
            }
        }
        protected string Name_;
        public static readonly ModelPropertyDefinitor<FormalElement, string> __Name =
                   new ModelPropertyDefinitor<FormalElement, string>("Name", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Name, ins => ins.Name_,
                       (ins, val) =>
                       {
                           /*T if (val.IsAbsent())
                               Console.WriteLine("Setting absent Name."); */

                           ins.Name_ = val;
                       },
                       true, false,
                                                                     "Name/Title", "Name or Title of the object.");

        /// <summary>
        /// Technical-Name of the object. Must be schema unique. Intended for machine-level usage as code, identifier or name for files/tables/programs.
        /// </summary>
        public string TechName
        {
            get { return __TechName.Get(this); }
            set
            {
                value = value.NullDefault("").Trim();

                if (value.IsAbsent() && !this.Name.IsAbsent())
                    value = this.Name.TextToIdentifier();

                __TechName.Set(this, value);
            }
        }
        protected string TechName_;
        public static readonly ModelPropertyDefinitor<FormalElement, string> __TechName =
                   new ModelPropertyDefinitor<FormalElement, string>("TechName", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Key,
                                                                     ins => (EntityEditEngine.ActiveEntityEditor != null && EntityEditEngine.ActiveEntityEditor.ReadTechNamesAsProgramIdentifiers
                                                                             ? ins.TechName_.TextToCSharpIdentifier()
                                                                             : ins.TechName_),
                                                                     (ins, val) => ins.TechName_ = val, true, true,
                                                                     "Tech-Name", "Technical-Name of the object. Should be unique. Intended for machine-level usage as code, identifier or name for files/tables/programs.");

        /// <summary>
        /// Summary of the object.
        /// </summary>
        public string Summary { get { return __Summary.Get(this); } set { __Summary.Set(this, value); } }
        protected string Summary_;
        public static readonly ModelPropertyDefinitor<FormalElement, string> __Summary =
                   new ModelPropertyDefinitor<FormalElement, string>("Summary", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Description, ins => ins.Summary_, (ins, val) => ins.Summary_ = val, true, false,
                                                                     "Summary", "Summary of the object.");

        /// <summary>
        /// Technical-Specification of the subject. Intended as a machine-level representation for computation (i.e. for use as script, template, formula or other kind of expression).
        /// </summary>
        public string TechSpec { get { return __TechSpec.Get(this); } set { __TechSpec.Set(this, value); } }
        protected string TechSpec_;
        public static readonly ModelPropertyDefinitor<FormalElement, string> __TechSpec =
                   new ModelPropertyDefinitor<FormalElement, string>("TechSpec", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Description, ins => ins.TechSpec_, (ins, val) => ins.TechSpec_ = val, false, true,
                                                                     "Tech-Spec", "Technical-Specification of the object. Intended as a machine-level representation for computation (i.e. for use as script, template, formula or other kind of expression).");

        /// <summary>
        /// Detailed description text (rich) of the instance.
        /// </summary>
        public string Description { get { return __Description.Get(this); } set { __Description.Set(this, value); } }
        protected string Description_ = null;
        public static readonly ModelPropertyDefinitor<FormalElement, string> __Description =
                   new ModelPropertyDefinitor<FormalElement, string>("Description", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Description, ins => ins.Description_, (ins, val) => ins.Description_ = val, false, false,
                                                                     "Description", "Detailed description (rich) text of the object.");

        /// <summary>
        /// Classification information store (commonly attached only to complex entities like Models or Diagrams).
        /// </summary>
        public ClassificationCard Classification { get { return __Classification.Get(this); } set { __Classification.Set(this, value); } }
        protected ClassificationCard Classification_ = null;
        public static readonly ModelPropertyDefinitor<FormalElement, ClassificationCard> __Classification =
                   new ModelPropertyDefinitor<FormalElement, ClassificationCard>("Classification", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.Classification_, (ins, val) => ins.Classification_ = val, false, false,
                                                                                 "Classification", "Stores the classification information of the object, such as tags.");

        /// <summary>
        /// Version information store (commonly attached only to complex entities like Models or Diagrams).
        /// </summary>
        public VersionCard Version { get { return __Version.Get(this); } set { __Version.Set(this, value); } }
        protected VersionCard Version_ = null;
        public static readonly ModelPropertyDefinitor<FormalElement, VersionCard> __Version =
                   new ModelPropertyDefinitor<FormalElement, VersionCard>("Version", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.Version_, (ins, val) => ins.Version_ = val, false, false,
                                                                          "Version", "Stores the versioning information of the object, such as Creator, Last-Modifier, dates and version number.");

        /// <summary>
        /// Gets the Name for single-line display (without new-line characters).
        /// </summary>
        [Description("Gets the Name for single-line display (without new-line characters).")]
        public string NameCaption { get { return this.Name.RemoveNewLines(); } }

        public override string ToString() { return this.Name; }

        #region IModelClass<FormalElement> Members

        public new MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public new ModelClassDefinitor<FormalElement> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly new ModelClassDefinitor<FormalElement> __ClassDefinitor = null;

        public override object CreateCopy(ECloneOperationScope CloningScope, IMModelClass DirectOwner) { return this.CreateClone(CloningScope, DirectOwner); }
        public new FormalElement CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((FormalElement)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public FormalElement PopulateFrom(FormalElement SourceElement, IMModelClass TargetOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames)
        {
            var Result = this.ClassDefinitor.PopulateInstance(this, SourceElement, TargetOwner, CloningScope);

            // For clones create a non-already "running" version.
            if (SourceElement.Version != null)
                Result.Version = new VersionCard(SourceElement.Version.Creator, SourceElement.Version.Annotation);

            return Result;
        }

        #endregion
    }
}