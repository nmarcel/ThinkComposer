using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Instrumind.Common;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;
using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;

namespace Instrumind.ThinkComposer.Definitor.DefinitorUI.Widgets
{
    /// <summary>
    /// Interaction logic for LinkRoleDefSpecSubform.xaml
    /// </summary>
    public partial class LinkRoleDefSpecSubform : UserControl, IEntityViewChild, INotifyPropertyChanged
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public LinkRoleDefSpecSubform()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor for edit an entity child property.
        /// </summary>
        public LinkRoleDefSpecSubform(string ChildPropertyName, bool CanSpecifyDirectionType, LinkRoleDefinition TargetLinkRoleDef, RelationshipDefinition ParentRelDef)
            : this()
        {
            this.ChildPropertyName = ChildPropertyName;
            this.CanSpecifyDirectionType = CanSpecifyDirectionType;
            this.TargetLinkRoleDef = TargetLinkRoleDef;
            this.ParentRelDef = ParentRelDef;

            this.RelationshipIsDirectional = ParentRelDef.IsDirectional;    // Just to force visual update

            // PENDING: EXPOSE WHEN SUPPORTED.
            // this.ExpoMaxConnections.SetVisible(!this.TargetLinkRoleDef.OwnerRelationshipDef.IsSimple);
            this.ExpoMaxConnections.SetVisible(false);

            // PENDING: EXPOSE WHEN SUPPORTED.
            this.ExpoRelatedIdeasAreOrdered.SetVisible(false);

            this.Loaded += new RoutedEventHandler(LinkRoleDefSpecSubform_Loaded);
        }

        void LinkRoleDefSpecSubform_Loaded(object sender, RoutedEventArgs e)
        {
            this.ExpoAllowedVariants.ExtraDataAvailableValues = Plugs.PredefinedPlugs;

            this.FormalElementWidget.ExpoPictogram.SetVisible(false);

            // IMPORTANT: Notice the use of this.ParentRelDef (the edited Agent) instead of
            //            this.TargetLinkRoleDef.OwnerRelationshipDef (the original Entity).
            var ConnFmt = this.ParentRelDef.DefaultConnectorsFormat;
            this.ExpoAllowedVariants.ExtraDataItems =
                (this.TargetLinkRoleDef == null
                 ? new Dictionary<object,object>()
                 : this.TargetLinkRoleDef.AllowedVariants
                    .Join((this.TargetLinkRoleDef.RoleType == ERoleType.Target ? ConnFmt.HeadPlugs : ConnFmt.TailPlugs),
                          cardn => cardn, plug => plug.Key,
                          (cardnl, plgkvp) => new KeyValuePair<object, object>(cardnl, plgkvp.Value))
                    .ToDictionary(kvp => kvp.Key,
                                  kvp => (object)Plugs.PredefinedPlugs.FirstOrDefault(plgdef => plgdef.TechName == kvp.Value.ToStringAlways())));

            this.ExpoAllowedVariants.ExtraDataUpdater =
                ((item, extra) =>
                {
                    VariantsUpdater((SimplePresentationElement)item, (SimplePresentationElement)extra);
                    //T MessageBox.Show("Item=[" + item.ToStringAlways() + "]("+item.GetType().Name+")\n" +
                    //                  "Extra=[" + extra.ToStringAlways() + "]("+extra.GetType().Name+")",
                    //                  "Update of...");
                });
        }

        public void VariantsUpdater(SimplePresentationElement Variant, SimplePresentationElement Plug)
        {
            // IMPORTANT: Notice the use of this.ParentRelDef (the edited Agent) instead of
            //            this.TargetLinkRoleDef.OwnerRelationshipDef (the original Entity).
            var ConnFmt = this.ParentRelDef.DefaultConnectorsFormat;
            IDictionary<SimplePresentationElement, string> TargetCollection = null;

            if (this.TargetLinkRoleDef.RoleType == ERoleType.Target)
                TargetCollection = ConnFmt.HeadPlugs;
            else
                TargetCollection = ConnFmt.TailPlugs;

            TargetCollection.AddOrReplace(Variant, Plug.TechName);

            // PENDING: In the Editor update (at the whole Relationship Definition level),
            //          the unused Head and Tail Plugs per Variant should be deleted
            //          accordingly to the Link-Role-Def's Allowed Variants.
        }

        public string ChildPropertyName { get; protected set; }

        public IEntityView ParentEntityView { get; set; }

        public void Refresh()
        {
        }

        public bool Apply()
        {
            return true;
        }

        public LinkRoleDefinition TargetLinkRoleDef { get; protected set; }

        public RelationshipDefinition ParentRelDef = null;

        public bool CanSpecifyDirectionType { get; protected set; }

        public bool RelationshipIsDirectional
        {
            get { return this.ParentRelDef.IsDirectional; }
            set
            {
                this.ParentRelDef.IsDirectional = value;

                if (this.CanSpecifyDirectionType)
                    this.EditingPanel.SetVisible(value);
                    // THIS DOESN'T WORK PROPERLY (WHEN STARTING IS-DIRECTIONAL=FALSE IT CANNOT BE RE-ENABLED AGAIN)
                    // this.EditingPanel.IsEnabled = value;

                // Console.WriteLine("IsDirectional={0}. CanSpecifyDirectionType={1}.", value, this.CanSpecifyDirectionType);
                var Handler = PropertyChanged;
                if (Handler != null)
                    Handler(this, new PropertyChangedEventArgs("RelationshipIsDirectional"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
