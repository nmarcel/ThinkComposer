using System;
using System.Collections.Generic;
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

using Instrumind.ThinkComposer.MetaModel;
using Instrumind.ThinkComposer.Model;
using Instrumind.ThinkComposer.Model.GraphModel;

namespace Instrumind.ThinkComposer.Definitor.DefinitorUI.Widgets
{
    /// <summary>
    /// Interaction logic for DomainTemplatingSubform.xaml
    /// </summary>
    public partial class DomainTemplatingSubform : UserControl, IEntityViewChild
    {
        public Domain Source { get; protected set; }

        public DomainTemplatingSubform()
        {
            InitializeComponent();
        }

        public DomainTemplatingSubform(Domain Source)
             : this()
        {
            this.Source = Source;

            this.BtnCompositionTemplate.ToolTip = Domain.TemplateForCompositionSummary;
            this.BtnConceptsTemplate.ToolTip = Domain.__OutputTemplatesForConcepts.Summary;
            this.BtnRelationshipsTemplate.ToolTip = Domain.__OutputTemplatesForRelationships.Summary;
        }

        private void BtnCompositionTemplate_Click(object sender, RoutedEventArgs e)
        {
            DomainServices.EditBaseTemplates(this.Source, Domain.__OutputTemplates, typeof(Composition), null, false);
        }

        private void BtnConceptsTemplate_Click(object sender, RoutedEventArgs e)
        {
            DomainServices.EditBaseTemplates(this.Source, Domain.__OutputTemplatesForConcepts, typeof(Concept));
        }

        private void BtnRelationshipsTemplate_Click(object sender, RoutedEventArgs e)
        {
            DomainServices.EditBaseTemplates(this.Source, Domain.__OutputTemplatesForRelationships, typeof(Relationship));
        }

        public IEntityView ParentEntityView { get; set; }

        public string ChildPropertyName { get; protected set; }

        public void Refresh()
        {
        }

        public bool Apply()
        {
            return true;
        }
    }
}
