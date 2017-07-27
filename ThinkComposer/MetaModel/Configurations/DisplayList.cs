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
    /// Stores display preferences about object views of List style.
    /// </summary>
    [Serializable]
    public class DisplayList : IModelEntity, IModelClass<DisplayList>
    {
        /// <summary>
        /// Static constructor
        /// </summary>
        static DisplayList()
        {
            __ClassDefinitor = new ModelClassDefinitor<DisplayList>("DisplayList", null, "Display List",
                                                                    "Stores display preferences about object views of List style.");
            __ClassDefinitor.DeclareProperty(__Show);
            __ClassDefinitor.DeclareProperty(__PropName);
            __ClassDefinitor.DeclareProperty(__PropTechName);
            __ClassDefinitor.DeclareProperty(__PropSummary);
            __ClassDefinitor.DeclareProperty(__PropPictogram);
            __ClassDefinitor.DeclareProperty(__Definitor);
        }

        public DisplayList()
        {
            this.Show = true;

            this.PropName = true;
            this.PropTechName = false;
            this.PropSummary = true;
            this.PropPictogram = true;
            this.Definitor = true;
        }

        /// <summary>
        /// Indicates whether to show the object.
        /// </summary>
        public bool Show { get { return __Show.Get(this); } set { __Show.Set(this, value); } }
        protected bool Show_;
        public static readonly ModelPropertyDefinitor<DisplayList, bool> __Show =
                   new ModelPropertyDefinitor<DisplayList, bool>("Show", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.Show_, (ins, val) => ins.Show_ = val, false, false,
                                                                 "Show", "Indicates whether to show the object.");

        /// <summary>
        /// Indicates whether to show the Name/Title property.
        /// </summary>
        public bool PropName { get { return __PropName.Get(this); } set { __PropName.Set(this, value); } }
        protected bool PropName_;
        public static readonly ModelPropertyDefinitor<DisplayList, bool> __PropName =
                   new ModelPropertyDefinitor<DisplayList, bool>("PropName", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.PropName_, (ins, val) => ins.PropName_ = val, false, false,
                                                                 "Name/Title", "Indicates whether to show the Name/Title property.");

        /// <summary>
        /// Indicates whether to show the Tech-Name property.
        /// </summary>
        public bool PropTechName { get { return __PropTechName.Get(this); } set { __PropTechName.Set(this, value); } }
        protected bool PropTechName_;
        public static readonly ModelPropertyDefinitor<DisplayList, bool> __PropTechName =
                   new ModelPropertyDefinitor<DisplayList, bool>("PropTechName", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.PropTechName_, (ins, val) => ins.PropTechName_ = val, false, false,
                                                                 "Tech-Name", "Indicates whether to show the Tech-Name property.");

        /// <summary>
        /// Indicates whether to show the Summary property.
        /// </summary>
        public bool PropSummary { get { return __PropSummary.Get(this); } set { __PropSummary.Set(this, value); } }
        protected bool PropSummary_;
        public static readonly ModelPropertyDefinitor<DisplayList, bool> __PropSummary =
                   new ModelPropertyDefinitor<DisplayList, bool>("PropSummary", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.PropSummary_, (ins, val) => ins.PropSummary_ = val, false, false,
                                                                 "Summary", "Indicates whether to show the Summary property.");

        /// <summary>
        /// Indicates whether to show the Pictogram property.
        /// </summary>
        public bool PropPictogram { get { return __PropPictogram.Get(this); } set { __PropPictogram.Set(this, value); } }
        protected bool PropPictogram_;
        public static readonly ModelPropertyDefinitor<DisplayList, bool> __PropPictogram =
                   new ModelPropertyDefinitor<DisplayList, bool>("PropPictogram", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.PropPictogram_, (ins, val) => ins.PropPictogram_ = val, false, false,
                                                                 "Pictogram", "Indicates whether to show the Pictogram property.");

        /// <summary>
        /// Indicates whether to show the Definition of the object.
        /// </summary>
        public bool Definitor { get { return __Definitor.Get(this); } set { __Definitor.Set(this, value); } }
        protected bool Definitor_;
        public static readonly ModelPropertyDefinitor<DisplayList, bool> __Definitor =
                   new ModelPropertyDefinitor<DisplayList, bool>("Definitor", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.Definitor_, (ins, val) => ins.Definitor_ = val, false, false,
                                                                 "Definition", "Indicates whether to show the Definition of the object.");

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelClass<DisplayList> Members

        public MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public ModelClassDefinitor<DisplayList> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly ModelClassDefinitor<DisplayList> __ClassDefinitor = null;

        public virtual object CreateCopy(ECloneOperationScope CloningScope, IMModelClass DirectOwner) { return this.CreateClone(CloningScope, DirectOwner); }
        public DisplayList CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((DisplayList)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public DisplayList PopulateFrom(DisplayList SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

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
