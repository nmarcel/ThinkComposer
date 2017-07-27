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
    /// Interaction logic for ArrangeTabForConceptDef.xaml
    /// </summary>
    public partial class ArrangeTabForConceptDef : UserControl, IEntityViewChild
    {
        public ConceptDefinition ConceptDef { get; protected set; }

        private ArrangeTabForConceptDef()
        {
            InitializeComponent();

            var AutoCrePropsPrefix = "Automatic Creation ";
            ExpoAutomaticCreationConceptDef.LabelText = ConceptDefinition.__AutomaticCreationConceptDef.Name.Remove(AutoCrePropsPrefix);
            ExpoAutomaticCreationRelationshipDef.LabelText = ConceptDefinition.__AutomaticCreationRelationshipDef.Name.Remove(AutoCrePropsPrefix);

            ExpoAutomaticCreationPositioningIsRadialized.LabelText = ConceptDefinition.__AutomaticCreationPositioningIsRadialized.Name.Remove(AutoCrePropsPrefix);
            ExpoAutomaticCreationPositioningMode.LabelText = ConceptDefinition.__AutomaticCreationPositioningMode.Name.Remove(AutoCrePropsPrefix);
        }

        public ArrangeTabForConceptDef(ConceptDefinition ConceptDef)
             : this()
        {
            this.ConceptDef = ConceptDef;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!this.IsLoaded)
                return;

            this.AutoCreationPropsGroup.SetAvailable(this.ConceptDef.CanAutomaticallyCreateRelatedConcepts);
            this.ExpoAutoGrpConceptDef.SetAvailable(this.ConceptDef.CanAutomaticallyCreateGroupedConcepts);

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

            this.ConceptDef.PropertyChanged += OnDefinitionPropChanged;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.ConceptDef.PropertyChanged -= OnDefinitionPropChanged;
            }
            catch(Exception Problem)
            {
                // this happens when the Loaded event was never fired, hence no event handler was attached nor parent-window was populated.
            }
        }

        private void OnDefinitionPropChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == IdeaDefinition.__CanAutomaticallyCreateRelatedConcepts.TechName)
                AutoCreationPropsGroup.SetAvailable(this.ConceptDef.CanAutomaticallyCreateRelatedConcepts);

            if (args.PropertyName == IdeaDefinition.__CanAutomaticallyCreateGroupedConcepts.TechName)
                ExpoAutoGrpConceptDef.SetAvailable(this.ConceptDef.CanAutomaticallyCreateGroupedConcepts);
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
