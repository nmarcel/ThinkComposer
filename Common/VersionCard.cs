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
// File   : VersionCard.cs
// Object : Instrumind.Common.VersionCard (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.07.02 Néstor Sánchez A.  Creation
//

using System;
using System.ComponentModel;

using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;

/// Library of general purpose components and features across Instrumind products.
namespace Instrumind.Common
{
    /// <summary>
    /// Stores version control information for versionable objects.
    /// </summary>
    [Serializable]
    public class VersionCard : IModelEntity, IModelClass<VersionCard>
    {
        /// <summary>
        /// Default version number.
        /// </summary>
        public const string DEFAULT_VERSION = "0.0.00";

        /// <summary>
        /// Static Constructor.
        /// </summary>
        static VersionCard()
        {
            __ClassDefinitor = new ModelClassDefinitor<VersionCard>("VersionCard", null,
                                                                    "Version Card", "Stores version control information for versionable objects.");
            __ClassDefinitor.DeclareProperty(__VersionSequence);
            //- __ClassDefinitor.DeclareProperty(__VersionChangeId);
            __ClassDefinitor.DeclareProperty(__Creation);
            __ClassDefinitor.DeclareProperty(__Creator);
            __ClassDefinitor.DeclareProperty(__LastModification);
            __ClassDefinitor.DeclareProperty(__LastModifier);
            __ClassDefinitor.DeclareProperty(__Annotation);
            __ClassDefinitor.DeclareProperty(__VersionNumber);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Creator">Optional creator user name.</param>
        /// <param name="Annotation">Optional versioning notes.</param>
        /// <param name="VersionNumber">Optional user-defined version number (e.g.: "4.5.02")</param>
        public VersionCard(string Creator = "", string Annotation = "", string VersionNumber = VersionCard.DEFAULT_VERSION)
        {
            General.ContractRequiresNotNull(Creator, Annotation, VersionNumber);

            this.VersionSequence_ = 1;
            //- this.VersionChangeId = Guid.NewGuid();
            this.Creation = DateTime.Now;
            this.Creator = Creator.AbsentDefault(AppExec.SessionUserName);
            this.LastModification = this.Creation;
            this.LastModifier = this.Creator;
            this.Annotation = Annotation;
            this.VersionNumber = VersionNumber;
        }

        /// <summary>
        /// Increments the internal version sequence
        /// and stores the supplied arguments, if any.
        /// </summary>
        public void Update(string Modifier = null, string Annotation = null, string VersionNumber = null)
        {
            this.VersionSequence = this.VersionSequence + 1;
            //- this.VersionChangeId = Guid.NewGuid();

            this.LastModification = DateTime.Now;

            if (!Modifier.IsAbsent())
                this.LastModifier = Modifier;   //- .AbsentDefault(AppExec.SessionUserName);

            if (Annotation != null)
                this.Annotation = Annotation;

            if (VersionNumber != null)
                this.VersionNumber = VersionNumber;
        }

        /// <summary>
        /// Sequential number, starting from one. The real version number.
        /// </summary>
        public int VersionSequence { get { return __VersionSequence.Get(this); } set { __VersionSequence.Set(this, value); } }
        protected int VersionSequence_ = 0;
        public static readonly ModelPropertyDefinitor<VersionCard, int> __VersionSequence =
                   new ModelPropertyDefinitor<VersionCard, int>("VersionSequence", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.VersionSequence_, (ins, val) => ins.VersionSequence_ = val, true, false,
                                                                "Version Sequence", "Sequential number, starting from one. The real version number.");

        /*- /// <summary>
        /// Global unique identifier of the last change.
        /// </summary>
        public Guid VersionChangeId { get { return __VersionChangeId.Get(this); } set { __VersionChangeId.Set(this, value); } }
        protected Guid VersionChangeId_ = Guid.Empty;
        public static readonly ModelPropertyDefinitor<VersionCard, Guid> __VersionChangeId =
                   new ModelPropertyDefinitor<VersionCard, Guid>("VersionChangeId", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.VersionChangeId_, (ins, val) => ins.VersionChangeId_ = val, true, true,
                                                                 "Version Change-Id", "Global unique identifier of the last change."); */

        /// <summary>
        /// Date-time of the creation.
        /// </summary>
        public DateTime Creation { get { return __Creation.Get(this); } set { __Creation.Set(this, value); } }
        protected DateTime Creation_;
        public static readonly ModelPropertyDefinitor<VersionCard, DateTime> __Creation =
                   new ModelPropertyDefinitor<VersionCard, DateTime>("Creation", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.Creation_, (ins, val) => ins.Creation_ = val, true, false,
                                                                     "Creation", "Date-time of the creation.");

        /// <summary>
        /// Creator user name.
        /// </summary>
        public string Creator { get { return __Creator.Get(this); } set { __Creator.Set(this, value); } }
        protected string Creator_;
        public static readonly ModelPropertyDefinitor<VersionCard, string> __Creator =
                   new ModelPropertyDefinitor<VersionCard, string>("Creator", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.Creator_, (ins, val) => ins.Creator_ = val, true, false,
                                                                   "Creator", "Creator user name.");

        /// <summary>
        /// Date-time of the last modification.
        /// </summary>
        public DateTime LastModification { get { return __LastModification.Get(this); } set { __LastModification.Set(this, value); } }
        protected DateTime LastModification_;
        public static readonly ModelPropertyDefinitor<VersionCard, DateTime> __LastModification =
                   new ModelPropertyDefinitor<VersionCard, DateTime>("LastModification", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.LastModification_, (ins, val) => ins.LastModification_ = val, true, false,
                                                                     "Last Modification", "Date-time of the last modification.");

        /// <summary>
        /// Last modifier user name.
        /// </summary>
        public string LastModifier { get { return __LastModifier.Get(this); } set { __LastModifier.Set(this, value); } }
        protected string LastModifier_;
        public static readonly ModelPropertyDefinitor<VersionCard, string> __LastModifier =
                   new ModelPropertyDefinitor<VersionCard, string>("LastModifier", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.LastModifier_, (ins, val) => ins.LastModifier_ = val, true, false,
                                                                   "Last Modifier", "Last modifier user name.");

        /// <summary>
        /// Comment in reference to edition activities performed or pending.
        /// </summary>
        public string Annotation { get { return __Annotation.Get(this); } set { __Annotation.Set(this, value); } }
        protected string Annotation_;
        public static readonly ModelPropertyDefinitor<VersionCard, string> __Annotation =
                   new ModelPropertyDefinitor<VersionCard, string>("Annotation", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Description, ins => ins.Annotation_, (ins, val) => ins.Annotation_ = val, true, false,
                                                                   "Annotation", "Comment in reference to edition activities performed or pending.");

        /// <summary>
        /// Optional manual/external generated version number (i.e: 'major-release.minor-release[.build[.revision]]')
        /// </summary>
        public string VersionNumber { get { return __VersionNumber.Get(this); } set { __VersionNumber.Set(this, value); } }
        protected string VersionNumber_;
        public static readonly ModelPropertyDefinitor<VersionCard, string> __VersionNumber =
                   new ModelPropertyDefinitor<VersionCard, string>("VersionNumber", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.VersionNumber_, (ins, val) => ins.VersionNumber_ = val, true, false,
                                                                   "Version Number", "Optional manual/external generated version number (i.e: 'major-release.minor-release[.build[.revision]]')");

        #region IModelClass<VersionCard> Members

        public MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public ModelClassDefinitor<VersionCard> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly ModelClassDefinitor<VersionCard> __ClassDefinitor = null;

        public object CreateCopy(ECloneOperationScope CloningScope, IMModelClass DirectOwner) { return this.CreateClone(CloningScope, DirectOwner); }
        public VersionCard CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((VersionCard)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public VersionCard PopulateFrom(VersionCard SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion

        #region INotifyPropertyChanged Members

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifies all entity subscriptors that a property, identified by the supplied definitor, has changed.
        /// </summary>
        public void NotifyPropertyChange(MModelPropertyDefinitor PropertyDefinitor)
        {
            this.NotifyPropertyChange(PropertyDefinitor.TechName);
        }

        /// <summary>
        /// Notifies all entity subscriptors that a property, identified by the supplied name, has changed.
        /// </summary>
        public void NotifyPropertyChange(string PropertyName)
        {
            var Handler = PropertyChanged;

            if (Handler != null)
                Handler(this, new PropertyChangedEventArgs(PropertyName));
        }

        #endregion


        #region IModelEntity Members

        public EntityEditEngine EditEngine { get { return EntityEditEngine.ObtainEditEngine(this, EditEngine_); } set { EditEngine_ = value; } }
        [NonSerialized]
        protected EntityEditEngine EditEngine_ = null;

        public virtual void RefreshEntity() { }

        public virtual MEntityInstanceController Controller { get { return this.Controller_; } set { this.Controller_ = value; } }
        [NonSerialized]
        private MEntityInstanceController Controller_ = null;

        #endregion

    }
}