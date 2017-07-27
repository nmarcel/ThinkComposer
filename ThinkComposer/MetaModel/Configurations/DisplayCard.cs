using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;

/// Provides configuration information for meta-models.
namespace Instrumind.ThinkComposer.MetaModel.Configurations
{
    /// <summary>
    /// Stores display preferences about object views of Card style.
    /// </summary>
    [Serializable]
    public class DisplayCard : IModelEntity, IModelClass<DisplayCard>
    {
        /// <summary>
        /// Static constructor
        /// </summary>
        static DisplayCard()
        {
            __ClassDefinitor = new ModelClassDefinitor<DisplayCard>("DisplayCard", null, "Display Card",
                                                                    "Stores display preferences about object views of Card style.");
            __ClassDefinitor.DeclareProperty(__Show);
            __ClassDefinitor.DeclareProperty(__Route);
            __ClassDefinitor.DeclareProperty(__Definitor);
            __ClassDefinitor.DeclareProperty(__PropGlobalId);
            __ClassDefinitor.DeclareProperty(__PropName);
            __ClassDefinitor.DeclareProperty(__PropTechName);
            __ClassDefinitor.DeclareProperty(__PropSummary);
            __ClassDefinitor.DeclareProperty(__PropTechSpec);
            __ClassDefinitor.DeclareProperty(__PropPictogram);
            __ClassDefinitor.DeclareProperty(__PropDescription);
            __ClassDefinitor.DeclareProperty(__PropVersioning);
        }

        public DisplayCard()
        {
            this.Show = true;

            this.Route = true;
            this.Definitor = true;
            this.PropGlobalId = true;
            this.PropName = true;
            this.PropTechName = true;
            this.PropSummary = true;
            this.PropTechSpec = true;
            this.PropPictogram = true;
            this.PropDescription = true;
            this.PropVersioning = true;
        }

        /// <summary>
        /// Indicates whether to show the object.
        /// </summary>
        public bool Show { get { return __Show.Get(this); } set { __Show.Set(this, value); } }
        protected bool Show_;
        public static readonly ModelPropertyDefinitor<DisplayCard, bool> __Show =
                   new ModelPropertyDefinitor<DisplayCard, bool>("Show", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.Show_, (ins, val) => ins.Show_ = val, false, false,
                                                                 "Show", "Indicates whether to show the object.");

        /// <summary>
        /// Indicates whether to show the Route of the object.
        /// </summary>
        public bool Route { get { return __Route.Get(this); } set { __Route.Set(this, value); } }
        protected bool Route_;
        public static readonly ModelPropertyDefinitor<DisplayCard, bool> __Route =
                   new ModelPropertyDefinitor<DisplayCard, bool>("Route", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.Route_, (ins, val) => ins.Route_ = val, false, false,
                                                                 "Route", "Indicates whether to show the Route of the object.");

        /// <summary>
        /// Indicates whether to show the Definition of the object.
        /// </summary>
        public bool Definitor { get { return __Definitor.Get(this); } set { __Definitor.Set(this, value); } }
        protected bool Definitor_;
        public static readonly ModelPropertyDefinitor<DisplayCard, bool> __Definitor =
                   new ModelPropertyDefinitor<DisplayCard, bool>("Definitor", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.Definitor_, (ins, val) => ins.Definitor_ = val, false, false,
                                                                 "Definition", "Indicates whether to show the Definition of the object.");

        /// <summary>
        /// Indicates whether to show the Global-Id property.
        /// </summary>
        public bool PropGlobalId { get { return __PropGlobalId.Get(this); } set { __PropGlobalId.Set(this, value); } }
        protected bool PropGlobalId_;
        public static readonly ModelPropertyDefinitor<DisplayCard, bool> __PropGlobalId =
                   new ModelPropertyDefinitor<DisplayCard, bool>("PropGlobalId", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.PropGlobalId_, (ins, val) => ins.PropGlobalId_ = val, false, false,
                                                                 "Global-Id", "Indicates whether to show the Global-Id property.");

        /// <summary>
        /// Indicates whether to show the Name/Title property.
        /// </summary>
        public bool PropName { get { return __PropName.Get(this); } set { __PropName.Set(this, value); } }
        protected bool PropName_;
        public static readonly ModelPropertyDefinitor<DisplayCard, bool> __PropName =
                   new ModelPropertyDefinitor<DisplayCard, bool>("PropName", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.PropName_, (ins, val) => ins.PropName_ = val, false, false,
                                                                 "Name/Title", "Indicates whether to show the Name/Title property.");

        /// <summary>
        /// Indicates whether to show the Tech-Name property.
        /// </summary>
        public bool PropTechName { get { return __PropTechName.Get(this); } set { __PropTechName.Set(this, value); } }
        protected bool PropTechName_;
        public static readonly ModelPropertyDefinitor<DisplayCard, bool> __PropTechName =
                   new ModelPropertyDefinitor<DisplayCard, bool>("PropTechName", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.PropTechName_, (ins, val) => ins.PropTechName_ = val, false, false,
                                                                 "Tech-Name", "Indicates whether to show the Tech-Name property.");

        /// <summary>
        /// Indicates whether to show the Summary property.
        /// </summary>
        public bool PropSummary { get { return __PropSummary.Get(this); } set { __PropSummary.Set(this, value); } }
        protected bool PropSummary_;
        public static readonly ModelPropertyDefinitor<DisplayCard, bool> __PropSummary =
                   new ModelPropertyDefinitor<DisplayCard, bool>("PropSummary", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.PropSummary_, (ins, val) => ins.PropSummary_ = val, false, false,
                                                                 "Summary", "Indicates whether to show the Summary property.");

        /// <summary>
        /// Indicates whether to show the Tech-Spec property.
        /// </summary>
        public bool PropTechSpec { get { return __PropTechSpec.Get(this); } set { __PropTechSpec.Set(this, value); } }
        protected bool PropTechSpec_;
        public static readonly ModelPropertyDefinitor<DisplayCard, bool> __PropTechSpec =
                   new ModelPropertyDefinitor<DisplayCard, bool>("PropTechSpec", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.PropTechSpec_, (ins, val) => ins.PropTechSpec_ = val, false, false,
                                                                 "Tech-Spec", "Indicates whether to show the Tech-Spec property.");

        /// <summary>
        /// Indicates whether to show the Pictogram property.
        /// </summary>
        public bool PropPictogram { get { return __PropPictogram.Get(this); } set { __PropPictogram.Set(this, value); } }
        protected bool PropPictogram_;
        public static readonly ModelPropertyDefinitor<DisplayCard, bool> __PropPictogram =
                   new ModelPropertyDefinitor<DisplayCard, bool>("PropPictogram", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.PropPictogram_, (ins, val) => ins.PropPictogram_ = val, false, false,
                                                                 "Pictogram", "Indicates whether to show the Pictogram property.");

        /// <summary>
        /// Indicates whether to show the Description property.
        /// </summary>
        public bool PropDescription { get { return __PropDescription.Get(this); } set { __PropDescription.Set(this, value); } }
        protected bool PropDescription_;
        public static readonly ModelPropertyDefinitor<DisplayCard, bool> __PropDescription =
                   new ModelPropertyDefinitor<DisplayCard, bool>("PropDescription", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.PropDescription_, (ins, val) => ins.PropDescription_ = val, false, false,
                                                                 "Description", "Indicates whether to show the Description property.");


        /// <summary>
        /// Indicates whether to show the Versioning property.
        /// </summary>
        public bool PropVersioning { get { return __PropVersioning.Get(this); } set { __PropVersioning.Set(this, value); } }
        protected bool PropVersioning_;
        public static readonly ModelPropertyDefinitor<DisplayCard, bool> __PropVersioning =
                   new ModelPropertyDefinitor<DisplayCard, bool>("PropVersioning", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.PropVersioning_, (ins, val) => ins.PropVersioning_ = val, false, false,
                                                                 "Versioning", "Indicates whether to show the Versioning property.");

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelClass<DisplayCard> Members

        public MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public ModelClassDefinitor<DisplayCard> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly ModelClassDefinitor<DisplayCard> __ClassDefinitor = null;

        public virtual object CreateCopy(ECloneOperationScope CloningScope, IMModelClass DirectOwner) { return this.CreateClone(CloningScope, DirectOwner); }
        public DisplayCard CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((DisplayCard)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public DisplayCard PopulateFrom(DisplayCard SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelEntity Members

        public EntityEditEngine EditEngine { get { return EntityEditEngine.ObtainEditEngine(this, EditEngine_); } set { EditEngine_ = value; } }
        [NonSerialized]
        private EntityEditEngine EditEngine_ = null;

        public virtual void RefreshEntity() { }

        public virtual MEntityInstanceController Controller { get { return this.Controller_; } set { this.Controller_ = value; } }
        [NonSerialized]
        private MEntityInstanceController Controller_ = null;

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

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}
