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
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;
using Instrumind.Common.Visualization.Widgets;

using Instrumind.ThinkComposer.ApplicationProduct;
using Instrumind.ThinkComposer.Definitor.DefinitorMaintenance;
using Instrumind.ThinkComposer.Definitor.DefinitorUI.Widgets;
using Instrumind.ThinkComposer.MetaModel;
using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.MetaModel.InformationMetaModel;

namespace Instrumind.ThinkComposer.Definitor.DefinitorUI.Widgets
{
    /// <summary>
    /// Interaction logic for TableStructureSubform.xaml
    /// </summary>
    public partial class TableStructureSubform : UserControl, IEntityViewChild
    {
        public TableDefinition SourceTableDef { get; protected set; }

        public TableStructureSubform()
        {
            InitializeComponent();
        }

        public TableStructureSubform(TableDefinition SourceTableDef)
             : this()
        {
            this.SetSourceTableDef(SourceTableDef);
        }

        public void SetSourceTableDef(TableDefinition SourceTableDef)
        {
            if (this.SourceTableDef == SourceTableDef)
                return;

            this.SourceTableDef = SourceTableDef;

            // Set working data sources
            this.WorkingFieldDefs = SourceTableDef.FieldDefinitions;
            this.WorkingUniqueKeyFields = SourceTableDef.UniqueKeyFieldDefs;
            this.WorkingDominantRefFields = SourceTableDef.DominantRefFieldDefs;
            this.WorkingLabelFields = SourceTableDef.LabelFieldDefs;

            // Remove previous assignments
            if (this.FieldsMaintainer != null)
                this.FieldsMaintainer.ItemDeleted -= FieldsMaintainer_ItemDeleted;

            // Set editing controls
            this.FieldsMaintainer = ItemsGridMaintainer.CreateItemsGridControl(SourceTableDef, WorkingFieldDefs,
                                    null /*- UNNECESSARY DUE TO DEEP-CLONNING: (item) => ProductDirector.ConfirmImmediateApply("Table Fields Definition", "DomainEdit.TableFieldsDefinition",
                                                                                    "ApplyTypingChangesDirectly", "This text-box")*/ );
            this.FieldsMaintainer.ItemDeleted += FieldsMaintainer_ItemDeleted;

            this.BrdFields.Child = this.FieldsMaintainer.VisualControl;

            this.LstbxUniqueKeyFields.ItemsSource = this.WorkingUniqueKeyFields;
            this.LstbxDominantFields.ItemsSource = this.WorkingDominantRefFields;
            this.LstbxLabelFields.ItemsSource = this.WorkingLabelFields;

            // PENDING: Establish drag-drop behavior
        }

        void FieldsMaintainer_ItemDeleted(FieldDefinition DeletedField)
        {
            if (this.WorkingUniqueKeyFields.Contains(DeletedField))
                this.WorkingUniqueKeyFields.Remove(DeletedField);

            if (this.WorkingDominantRefFields.Contains(DeletedField))
                this.WorkingDominantRefFields.Remove(DeletedField);

            if (this.WorkingLabelFields.Contains(DeletedField))
                this.WorkingLabelFields.Remove(DeletedField);
        }

        public ItemsGridMaintainer<TableDefinition, FieldDefinition> FieldsMaintainer { get; protected set; }

        IList<FieldDefinition> WorkingFieldDefs = null;
        IList<FieldDefinition> WorkingUniqueKeyFields = null;
        IList<FieldDefinition> WorkingDominantRefFields = null;
        IList<FieldDefinition> WorkingLabelFields = null;

        private void BtnAddKeyField_Click(object sender, RoutedEventArgs e)
        {
            var SelectedItem = this.FieldsMaintainer.VisualControl.SelectedItem;
            if (SelectedItem == null || this.WorkingUniqueKeyFields.Contains(SelectedItem))
                return;

            this.WorkingUniqueKeyFields.Add(SelectedItem as FieldDefinition);
        }

        private void BtnRemoveKeyField_Click(object sender, RoutedEventArgs e)
        {
            if (this.LstbxUniqueKeyFields.SelectedItem == null)
                return;

            this.WorkingUniqueKeyFields.Remove(this.LstbxUniqueKeyFields.SelectedItem as FieldDefinition);
        }

        private void BtnAddDominantField_Click(object sender, RoutedEventArgs e)
        {
            var SelectedItem = this.FieldsMaintainer.VisualControl.SelectedItem;
            if (SelectedItem == null || this.WorkingDominantRefFields.Contains(SelectedItem))
                return;

            this.WorkingDominantRefFields.Add(SelectedItem as FieldDefinition);
        }

        private void BtnRemoveDominantField_Click(object sender, RoutedEventArgs e)
        {
            if (this.LstbxDominantFields.SelectedItem == null)
                return;

            this.WorkingDominantRefFields.Remove(this.LstbxDominantFields.SelectedItem as FieldDefinition);
        }

        private void BtnAddLabelField_Click(object sender, RoutedEventArgs e)
        {
            var SelectedItem = this.FieldsMaintainer.VisualControl.SelectedItem;
            if (SelectedItem == null || this.WorkingLabelFields.Contains(SelectedItem))
                return;

            this.WorkingLabelFields.Add(SelectedItem as FieldDefinition);
        }

        private void BtnRemoveLabelField_Click(object sender, RoutedEventArgs e)
        {
            if (this.LstbxLabelFields.SelectedItem == null)
                return;

            this.WorkingLabelFields.Remove(this.LstbxLabelFields.SelectedItem as FieldDefinition);
        }

        // -----------------------------------------------------------------------------------------
        public string ChildPropertyName
        {
            get { return TableDefinition.__FieldDefinitions.TechName; }
        }

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
