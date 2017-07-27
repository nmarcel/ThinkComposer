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

namespace Instrumind.ThinkComposer.Definitor.DefinitorUI.Widgets
{
    /// <summary>
    /// Interaction logic for ArrangeTabForRelationshipDef.xaml
    /// </summary>
    public partial class ArrangeTabForRelationshipDef : UserControl, IEntityViewChild
    {
        public RelationshipDefinition RelationshipDef { get; protected set; }

        private ArrangeTabForRelationshipDef()
        {
            InitializeComponent();
        }

        public ArrangeTabForRelationshipDef(RelationshipDefinition RelationshipDef)
             : this()
        {
            this.RelationshipDef = RelationshipDef;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!this.IsLoaded)
                return;

            this.ExpoAutoGrpConceptDef.SetAvailable(this.RelationshipDef.CanAutomaticallyCreateGroupedConcepts);

            /* IMPORTANT: The IsEnable property does not work consistent. Do not use it to disable sibling controls.
            this.PostCall(ctl =>
                {
                    ctl.AutoCreationPropsGroup.IsEnabled = ctl.ConceptDef.CanAutomaticallyCreateRelatedConcepts;
                    ctl.ExpoAutoGrpConceptDef.IsEnabled = ctl.ConceptDef.CanAutomaticallyCreateGroupedConcepts;
                }, true); */

            //Binding doesn't work when -at start- the group is disabled
            //var Binder = new Binding("CanAutomaticallyCreateRelatedConcepts");
            //Binder.Source = ConceptDef;
            //Binder.Mode = BindingMode.OneWay;
            //BindingOperations.SetBinding(AutoCreationPropsGroup, GroupBox.IsEnabledProperty, Binder);

            this.RelationshipDef.PropertyChanged += OnDefinitionPropChanged;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.RelationshipDef.PropertyChanged -= OnDefinitionPropChanged;
            }
            catch (Exception Problem)
            {
                // this happens when the Loaded event was never fired, hence no event handler was attached nor parent-window was populated.
            }
        }

        private void OnDefinitionPropChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == IdeaDefinition.__CanAutomaticallyCreateGroupedConcepts.TechName)
                ExpoAutoGrpConceptDef.SetAvailable(this.RelationshipDef.CanAutomaticallyCreateGroupedConcepts);
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
    }
}
