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
using Instrumind.Common.Visualization;
using Instrumind.Common.Visualization.Widgets;

using Instrumind.ThinkComposer.MetaModel.InformationMetaModel;
using Instrumind.ThinkComposer.Model.InformationModel;

namespace Instrumind.ThinkComposer.Composer.ComposerUI.Widgets
{
    /// <summary>
    /// Interaction logic for SingleTableRecordEditor.xaml
    /// </summary>
    public partial class SingleTableRecordEditor : UserControl
    {
        public SingleTableRecordEditor()
        {
            InitializeComponent();
        }

        public virtual void UserControl_Loaded(object sender, RoutedEventArgs e)
        { }

        public virtual void UserControl_Unloaded(object sender, RoutedEventArgs e)
        { }
    }

    /// <summary>
    /// Interaction logic for SingleTableRecordEditor.xaml
    /// </summary>
    public class SingleTableRecordEditor<TEntity, TItem> : SingleTableRecordEditor, IAlternativeRecordEditor<TEntity, TItem>
           where TEntity : class, IModelEntity
    {
        public SingleTableRecordEditor()
             : base()
        {}

        public SingleTableRecordEditor(TableDefinition TargetTableDefinition)
            : this()
        {
            this.TargetTableDefinition = TargetTableDefinition;
        }

        public TableDefinition TargetTableDefinition { get; protected set; }

        public UIElement VisualControl { get { return this; } }

        public ItemsGridMaintainer<TEntity, TItem> SourceGrid { get; protected set; }

        public void InitializeFromGrid(ItemsGridMaintainer<TEntity, TItem> SourceGrid)
        {
            this.SourceGrid = SourceGrid;
            this.SourceGrid.SelectedItemChanged += SourceGrid_SelectedItemChanged;

            this.TargetItem = this.SourceGrid.VisualControl.SelectedItem;
        }

        void SourceGrid_SelectedItemChanged(TItem SelectedItem)
        {
            this.TargetItem = SelectedItem;
        }

        protected void InitializeExpositors()
        {
            this.BackPanel.Children.Clear();

            foreach (var FieldDef in this.TargetTableDefinition.FieldDefinitions)
            {
                var Expositor = new RecordFieldExpositor(FieldDef);
                Expositor.LabelMinWidth = 100.0;
                Expositor.MinWidth = 200.0;
                this.BackPanel.Children.Add(Expositor);
            }
        }

        public void AppendRecord(TItem Record)
        {
            this.SourceGrid.AppendRecord(Record);
            this.SourceGrid.VisualControl.SelectedItem = Record;
        }

        public void Reset()
        {
            this.SourceGrid.ResetRecords();
        }

        public object TargetItem
        {
            get { return this.TargetItem_;  }
            set
            {
                if (this.TargetItem_ == value)
                    return;

                if (this.BackPanel.Children.Count < 1)
                    InitializeExpositors();
                else
                    foreach (var Expositor in this.BackPanel.Children.Cast<RecordFieldExpositor>())
                        Expositor.Suppress();
            
                this.TargetItem_ = value;

                // For all expositors: re-assign source instance.
                foreach (var Expositor in this.BackPanel.Children.Cast<RecordFieldExpositor>())
                {
                    Expositor.InstanceSource = this.TargetItem_ as TableRecord;
                    Expositor.Expose();
                }

                if (this.BackPanel.Children.Count > 0)
                    ((RecordFieldExpositor)this.BackPanel.Children[0]).PostCall(exp => exp.Focus());
            }
        }
        private object TargetItem_ = null;

    }
}
