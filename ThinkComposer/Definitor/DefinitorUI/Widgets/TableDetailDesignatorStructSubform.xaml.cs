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

using Instrumind.ThinkComposer.MetaModel;
using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.MetaModel.InformationMetaModel;
using Instrumind.ThinkComposer.Definitor.DefinitorMaintenance;

namespace Instrumind.ThinkComposer.Definitor.DefinitorUI.Widgets
{
    /// <summary>
    /// Interaction logic for TableDetailDesignatorStructureTab.xaml
    /// </summary>
    public partial class TableDetailDesignatorStructSubform : UserControl, IEntityViewChild
    {
        public TableDetailDesignator SourceDesignator { get; protected set; }

        // Contains, in its Tag property, a tuple with the imported Table-Definition and Data
        public FrameworkElement AssociatedImportWidget { get; protected set; }

        public bool DirectApply { get; protected set; }

        public Domain RelatedDomain { get; protected set; }

        public IEnumerable<object> AvailabeTableStructures { get; protected set; }

        public TableDefinition OwnedTableDefinition { get; protected set; }

        public TableDefinition InitialTableDefinitionClone { get { return this.InitialTableDefinitionClone_; } protected set { this.InitialTableDefinitionClone_ = value; } }
        private TableDefinition InitialTableDefinitionClone_ = null;

        public TableDetailDesignatorStructSubform()
        {
            InitializeComponent();
        }

        public TableDetailDesignatorStructSubform(TableDetailDesignator SourceDesignator, FrameworkElement AssociatedImportWidget = null, bool DirectApply = false)
             : this()
        {
            this.SourceDesignator = SourceDesignator;
            this.AssociatedImportWidget = AssociatedImportWidget;

            this.DirectApply = DirectApply;

            this.PnlButtons.SetVisible(DirectApply);

            // Clone for detect changes
            this.InitialTableDefinitionClone = (this.SourceDesignator.DeclaringTableDefinition == null ? null :
                                                this.SourceDesignator.DeclaringTableDefinition.CreateClone(ECloneOperationScope.DeepAndEquivalent, null));
            
            this.RelatedDomain = this.SourceDesignator.OwnerDomain;

            this.AvailabeTableStructures = this.RelatedDomain.TableDefinitions.Where(tdef => tdef.Alterability != EAlterability.System);;

            if (this.SourceDesignator.TableDefIsOwned)
                this.OwnedTableDefinition = this.SourceDesignator.DeclaringTableDefinition;

            if (this.OwnedTableDefinition == null)
            {
                var LocalName = this.SourceDesignator.AssignerOwnerName + " - " + this.SourceDesignator.Name;
                this.OwnedTableDefinition = new TableDefinition(this.RelatedDomain, LocalName, LocalName.TextToIdentifier());
                this.OwnedTableDefinition.Alterability = EAlterability.Definition;  // This prevents the table-def to be edited from the Domain.
            }

            this.FrmTableStructEditor.SetSourceTableDef(this.OwnedTableDefinition);
            TableDefinitionMaintainer.SetFieldDefinitionsMaintainer(this.FrmTableStructEditor.FieldsMaintainer);    // Don't forget to assign the new maintainer
            this.FrmTableStructEditor.FieldsMaintainer.VisualControl.CanEditItemsDirectly = false;

            this.CbxCopySourceTableStruct.ItemsSource = this.AvailabeTableStructures;

            if (this.AvailabeTableStructures != null && this.AvailabeTableStructures.Any())
            {
                this.CbxReferencedTableStruct.ItemsSource = this.AvailabeTableStructures;

                if (this.SourceDesignator.DeclaringTableDefinition != null
                    && this.AvailabeTableStructures.Contains(this.SourceDesignator.DeclaringTableDefinition)
                    && !this.SourceDesignator.TableDefIsOwned)
                    this.CbxReferencedTableStruct.SelectedItem = this.SourceDesignator.DeclaringTableDefinition;
                else
                    this.CbxReferencedTableStruct.SelectedItem = this.AvailabeTableStructures.First();
            }
            else
            {
                this.RbReferenced.IsEnabled = false;
                this.TxbReferencedTableStruct.IsEnabled = false;
                this.CbxReferencedTableStruct.IsEnabled = false;
            }

            if (this.SourceDesignator.TableDefIsOwned || !this.RbReferenced.IsEnabled)
                this.RbOwned.IsChecked = true;
            else
                this.RbReferenced.IsChecked = true;
        }

        private void BtnCopyFrom_Click(object sender, RoutedEventArgs e)
        {
            this.CbxCopySourceTableStruct.SetVisible(true);
            this.CbxCopySourceTableStruct.PostCall(cbx => cbx.IsDropDownOpen = true);
        }

        private void RbOwned_Checked(object sender, RoutedEventArgs e)
        {
            this.BtnCopyFrom.IsEnabled = true;
            this.FrmTableStructEditor.IsEnabled = true;
            this.FrmTableStructEditor.SetAvailable(true);

            this.TxbReferencedTableStruct.IsEnabled = false;
            this.CbxReferencedTableStruct.IsEnabled = false;

            this.SourceDesignator.TableDefIsOwned = true;
        }

        private void RbReferenced_Checked(object sender, RoutedEventArgs e)
        {
            this.TxbReferencedTableStruct.IsEnabled = true;
            this.CbxReferencedTableStruct.IsEnabled = true;

            this.BtnCopyFrom.IsEnabled = false;
            this.FrmTableStructEditor.IsEnabled = false;
            this.FrmTableStructEditor.SetAvailable(false /*, Display.VISUAL_UNAVAILABILITY_OPACITY / 2.0*/);
            this.CbxCopySourceTableStruct.SetVisible(false, false);

            this.SourceDesignator.TableDefIsOwned = false;
        }

        private void CbxCopySourceTableStruct_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var CloningSource = this.CbxCopySourceTableStruct.SelectedItem as TableDefinition;
            if (CloningSource == null)
                return;

            this.OwnedTableDefinition = CloningSource.CreateClone(ECloneOperationScope.Deep, null);
            this.OwnedTableDefinition.Name = this.SourceDesignator.AssignerOwnerName + " - " + this.SourceDesignator.Name;
            // Old: "Clone of " + this.OwnedTableDefinition.Name + " - " + DateTime.Now.ToString("yyyyMMddhhmmss"); ;
            this.OwnedTableDefinition.TechName = this.OwnedTableDefinition.Name.TextToIdentifier();

            this.FrmTableStructEditor.SetSourceTableDef(this.OwnedTableDefinition);
            TableDefinitionMaintainer.SetFieldDefinitionsMaintainer(this.FrmTableStructEditor.FieldsMaintainer);    // Don't forget to assign the new maintainer
            this.FrmTableStructEditor.FieldsMaintainer.VisualControl.CanEditItemsDirectly = false;

            this.CbxCopySourceTableStruct.SetVisible(false, false);
            this.CbxCopySourceTableStruct.SelectedItem = null;
        }

        public IEntityView ParentEntityView { get; set; }

        public string ChildPropertyName { get; set; }

        public void Refresh() { }

        public bool Apply()
        {
            if (this.AssociatedImportWidget != null)
            {
                var ImportData = this.AssociatedImportWidget.Tag as Tuple<TableDefinition, List<List<object>>>;

                if (ImportData != null)
                {
                    this.SourceDesignator.TableDefIsOwned = true;
                    this.SourceDesignator.DeclaringTableDefinition = ImportData.Item1;

                    return true;
                }
            }

            TableDefinition NewTableDef = null;

            // IMPORTANT: Do not evaluate [Any-Radio-Button].IsChecked because state changes at load/unload of the tab.
            if (this.SourceDesignator.TableDefIsOwned)
                NewTableDef = this.OwnedTableDefinition;
            else
                NewTableDef = this.CbxReferencedTableStruct.SelectedItem as TableDefinition;

            // No?
            this.RelatedDomain.TableDefinitions.AddNew(NewTableDef);

            bool Proceed = true;

            if (this.InitialTableDefinitionClone != null)
                Proceed = TableDefinitionMaintainer.ApplyTableDefStructureAlter(NewTableDef,
                                                                                this.InitialTableDefinitionClone,
                                                                                true.IntoEnumerable().Cast<object>());

            // Must be applied last
            if (Proceed)
                this.SourceDesignator.DeclaringTableDefinition = NewTableDef;

            return Proceed;
        }

        private void BtnAccept_Click(object sender, RoutedEventArgs e)
        {
            var Presenter = this.GetNearestVisualDominantOfType<Window>();
            if (Presenter == null)
                return;

            Presenter.DialogResult = (this.DirectApply ? this.Apply() : true);
            Presenter.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            var Presenter = this.GetNearestVisualDominantOfType<Window>();
            if (Presenter == null)
                return;

            Presenter.DialogResult = false;
            Presenter.Close();
        }
    }
}
