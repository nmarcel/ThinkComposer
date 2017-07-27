using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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

namespace Instrumind.Common.Visualization.Widgets
{
    /// <summary>
    /// Interaction logic for ItemsGridControl.xaml
    /// </summary>
    public partial class ItemsGridControl : UserControl, IEntityViewChild
    {
        public IList ItemsDataSource { get; protected set; }

        public ItemsGridControl()
        {
            InitializeComponent();
        }

        public ItemsGridControl(IEntityViewChild DominantViewChild)
             : this()
        {
            this.DominantViewChild = DominantViewChild;
        }

        protected IEntityViewChild DominantViewChild { get; private set; }

        public void SetDataSource<TItem>(IList<TItem> ItemsDataSource)
        {
            this.ItemsDataSource = ItemsDataSource as IList;
            this.EditingDataGrid.ItemsSource = ItemsDataSource;

            // Autoselect the first row
            if (this.ItemsDataSource.Count > 0)
                this.EditingDataGrid.SelectedIndex = 0;
        }

        public object SelectedItem
        {
            get { return this.EditingDataGrid.SelectedItem; }
            set { this.EditingDataGrid.SelectedItem = value; }
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        public readonly List<Tuple<IIdentifiableElement, DataGridColumn>> ColumnsDescriptors = new List<Tuple<IIdentifiableElement, DataGridColumn>>();

        public void AddColumn(Tuple<IIdentifiableElement, DataGridColumn> ColumnDescriptor)
        {
            this.ColumnsDescriptors.Add(ColumnDescriptor);
            this.EditingDataGrid.Columns.Add(ColumnDescriptor.Item2);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        public bool CanEditItemsDirectly
        {
            get { return !this.EditingDataGrid.IsReadOnly; }
            set
            {
                this.EditingDataGrid.IsReadOnly = !value;

                if (this.EditingDataGrid.IsReadOnly
                    && (this.EditingDataGrid.ToolTip == null || this.EditingDataGrid.ToolTip.ToString().IsAbsent()))
                    this.EditingDataGrid.ToolTip = "Double-click for edit.";
            }
        }

        public void SetReadyForEditing()
        {
            if (!this.EditingDataGrid.HasItems)
                return;

            this.EditingDataGrid.SelectedIndex = 0;

            var Cell = this.EditingDataGrid.GetCell(0, 0);
            if (Cell == null)
                return;

            Cell.Focus();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        public bool ShowGlobalButtons
        {
            get { return this.ShowGlobalButtons_; }
            set
            {
                this.ShowGlobalButtons_ = value;
                var visibility = (value ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed);

                this.SepGlobal.Visibility = visibility;
                this.GlobalRefresh.Visibility = visibility;
                this.GlobalApply.Visibility = visibility;
            }
        }
        private bool ShowGlobalButtons_ = true;

        public bool ShowClipboardButtons
        {
            get { return this.ShowClipboardButtons_; }
            set
            {
                this.ShowClipboardButtons_ = value;
                var visibility = (value ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed);

                this.SepClipboard.Visibility = visibility;
                this.ClipboardCut.Visibility = visibility;
                this.ClipboardCopy.Visibility = visibility;
                this.ClipboardPaste.Visibility = visibility;
                this.SelectAll.Visibility = visibility;
                this.UnselectAll.Visibility = visibility;
            }
        }
        private bool ShowClipboardButtons_ = false;

        public bool ShowMovingButtons
        {
            get { return this.ShowMovingButtons_; }
            set
            {
                this.ShowMovingButtons_ = value;
                var visibility = (value ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed);

                this.SepMove.Visibility = visibility;
                this.MoveTop.Visibility = visibility;
                this.MoveUp.Visibility = visibility;
                this.MoveDown.Visibility = visibility;
                this.MoveBottom.Visibility = visibility;
            }
        }
        private bool ShowMovingButtons_ = true;

        public bool ShowNavigationButtons
        {
            get { return this.ShowNavigationButtons_; }
            set
            {
                this.ShowNavigationButtons_ = value;
                var visibility = (value ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed);

                this.SepNav.Visibility = visibility;
                this.NavFirst.Visibility = visibility;
                this.NavPrevious.Visibility = visibility;
                this.NavNext.Visibility = visibility;
                this.NavLast.Visibility = visibility;
                this.NavArea.Visibility = visibility;
            }
        }
        private bool ShowNavigationButtons_ = true;

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void MoveTop_Click(object sender, RoutedEventArgs e)
        {
            if (!this.EditingDataGrid.HasItems || this.EditingDataGrid.SelectedIndex < 1)
                return;

            var SelectedItem = this.EditingDataGrid.SelectedItem;
            this.ItemsDataSource.Remove(SelectedItem);
            this.ItemsDataSource.Insert(0, SelectedItem);

            this.EditingDataGrid.SelectedIndex = 0;
        }

        private void MoveUp_Click(object sender, RoutedEventArgs e)
        {
            if (!this.EditingDataGrid.HasItems || this.EditingDataGrid.SelectedIndex < 1)
                return;

            var SelectedItem = this.EditingDataGrid.SelectedItem;
            var NewSelectedIndex = this.EditingDataGrid.SelectedIndex - 1;
            this.ItemsDataSource.Remove(SelectedItem);
            this.ItemsDataSource.Insert(NewSelectedIndex, SelectedItem);

            this.EditingDataGrid.SelectedIndex = NewSelectedIndex;
        }

        private void MoveDown_Click(object sender, RoutedEventArgs e)
        {
            if (!this.EditingDataGrid.HasItems || this.EditingDataGrid.SelectedIndex >= this.ItemsDataSource.Count)
                return;

            var SelectedItem = this.EditingDataGrid.SelectedItem;
            var NewSelectedIndex = this.EditingDataGrid.SelectedIndex + 1;
            this.ItemsDataSource.Remove(SelectedItem);
            this.ItemsDataSource.Insert(NewSelectedIndex, SelectedItem);

            this.EditingDataGrid.SelectedIndex = NewSelectedIndex;
        }

        private void MoveBottom_Click(object sender, RoutedEventArgs e)
        {
            if (!this.EditingDataGrid.HasItems || this.EditingDataGrid.SelectedIndex >= this.ItemsDataSource.Count)
                return;

            var SelectedItem = this.EditingDataGrid.SelectedItem;
            this.ItemsDataSource.Remove(SelectedItem);
            this.ItemsDataSource.Add(SelectedItem);

            this.EditingDataGrid.SelectedIndex = this.ItemsDataSource.Count - 1;
        }

        private void NavFirst_Click(object sender, RoutedEventArgs e)
        {
            if (this.EditingDataGrid.Items.Count < 1)
                return;

            this.EditingDataGrid.SelectedIndex = 0;
            this.EditingDataGrid.ScrollIntoView(this.EditingDataGrid.SelectedItem);
        }

        private void NavPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (this.EditingDataGrid.Items.Count < 1
                || this.EditingDataGrid.SelectedIndex < 1)
                return;

            this.EditingDataGrid.SelectedIndex = this.EditingDataGrid.SelectedIndex - 1;
            this.EditingDataGrid.ScrollIntoView(this.EditingDataGrid.SelectedItem);
        }

        private void NavNext_Click(object sender, RoutedEventArgs e)
        {
            if (this.EditingDataGrid.Items.Count < 1
                || this.EditingDataGrid.SelectedIndex >= this.EditingDataGrid.Items.Count - 1)
                return;

            this.EditingDataGrid.SelectedIndex = this.EditingDataGrid.SelectedIndex + 1;
            this.EditingDataGrid.ScrollIntoView(this.EditingDataGrid.SelectedItem);
        }

        private void NavLast_Click(object sender, RoutedEventArgs e)
        {
            if (this.EditingDataGrid.Items.Count < 1)
                return;

            this.EditingDataGrid.SelectedIndex = this.EditingDataGrid.Items.Count - 1;
            this.EditingDataGrid.ScrollIntoView(this.EditingDataGrid.SelectedItem);
        }

        private void NavGotoItemNumber_LostFocus(object sender, RoutedEventArgs e)
        {
            int RecordNumber = -1;
            if (this.EditingDataGrid.Items.Count < 1
                || !Int32.TryParse(this.NavGotoItemNumber.Text, out RecordNumber)
                || !RecordNumber.IsInRange(1,this.EditingDataGrid.Items.Count))
                return;

            RecordNumber--;
            this.EditingDataGrid.SelectedIndex = RecordNumber;
            this.EditingDataGrid.ScrollIntoView(this.EditingDataGrid.SelectedItem);
        }

        private void NavGotoItemNumber_GotFocus(object sender, RoutedEventArgs e)
        {
            this.NavGotoItemNumber.Select(0, this.NavGotoItemNumber.Text.Length);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// One-based Record number (for end-users) representing the current item position.
        /// </summary>
        public int CurrentRecordNumber
        {
            get { return this.EditingDataGrid.SelectedIndex + 1; }
            set
            {
                int Index = value - 1;

                if (Index == this.EditingDataGrid.SelectedIndex - 1 || Index < 0)
                    return;

                this.EditingDataGrid.SelectedIndex = Index;
                if (this.EditingDataGrid.SelectedItem != null)
                    this.EditingDataGrid.ScrollIntoView(this.EditingDataGrid.SelectedItem);
            }
        }

        /// <summary>
        /// For single-click editing. See next event handler for mouse-left-button-Up.
        /// </summary>
        private void DataGridCell_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetSelectedCell(sender as DataGridCell);
        }

        // IMPORTANT: The same code in both event handlers (for mouse-left-button-Down/Up)
        //            achieve the desired easy single-click edit effect. DO NOT REMOVE!
        private void DataGridCell_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SetSelectedCell(sender as DataGridCell);
        }

        public void SetSelectedCell(DataGridCell Cell)
        {
            if (Cell == null || Cell.IsEditing || Cell.IsReadOnly)
                return;

            if (!Cell.IsFocused)
                Cell.Focus();

            if (this.EditingDataGrid.SelectionUnit != DataGridSelectionUnit.FullRow)
            {
                if (!Cell.IsSelected)
                    Cell.IsSelected = true;
            }
            else
            {
                var row = Cell.GetNearestVisualDominantOfType<DataGridRow>();
                if (row != null && !row.IsSelected)
                    row.IsSelected = true;
            }
        }

        public bool AutoGenerateCodeForIIdentifiableElementItems = true;

        public Func<object, bool> DirectEditingConfirmator = null;

        private void ItemsDataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (this.DirectEditingConfirmator != null && !this.DirectEditingConfirmator(e.Row.Item))
            {
                e.Cancel = true;
                return;
            }

            if (this.AutoGenerateCodeForIIdentifiableElementItems)
            {
                PreviousIIdentiableElementItemName = null;

                var Target = e.Row.Item as IMModelClass;
                var ColDef = (e.Column.Header is FrameworkElement ?
                              ((FrameworkElement)e.Column.Header).Tag as MModelPropertyDefinitor :
                              null);

                if (Target == null || ColDef == null || ColDef.TechName != FormalElement.__Name.TechName ||
                    Target.ClassDefinition.GetPropertyDef(FormalElement.__Name.TechName, false) == null)
                    return;

                PreviousIIdentiableElementItemName = ColDef.Read(Target) as string;
            }
        }
        private string PreviousIIdentiableElementItemName = null;

        private void ItemsDataGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            // IMPORTANT: DO NOT CALL HERE TO this.EditingDataGrid.BeginEdit();
            //            THAT WILL PROVOKE UNDESIRABLE LOSS OF EDITED DATA.

            var Cell = this.EditingDataGrid.GetCurrentCell();
            if (Cell == null)
                return;

            var Descriptor = this.ColumnsDescriptors.FirstOrDefault(descrip => descrip.Item2.IsEqual(Cell.Column));
            if (Descriptor == null)
                return;

            Cell.Focus();
            this.EditingDataGrid.BeginEdit();

            // this.EditingDataGrid.CommitEdit(DataGridEditingUnit.Row, false);

            /* T
            var RowIndex =  this.EditingDataGrid.ItemContainerGenerator.IndexFromContainer(Cell);
            var Row = this.EditingDataGrid.GetRow(RowIndex);
            var Record = this.EditingDataGrid.CurrentCell.Item;

            Console.WriteLine("Current-Cell: Column=[{0}], Idx=[{1}], Row={2}, Record={3}", ColDes.Item1.TechName, RowIndex, Row, Record); */
        }

        private void EditingDataGrid_CellSelected(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource.GetType() == typeof(DataGridCell))
            {
                var SourceGrid = (DataGrid)sender;
                SourceGrid.BeginEdit(e);
            } 
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Stops the processing of edit ending of a cell.
        /// </summary>
        public bool DisableCellEditEnding { get; set; }

        public Action<DataGridRow, IIdentifiableElement> EditEndingOperation { get; set; }

        private void ItemsDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            var Definitor = this.ColumnsDescriptors.FirstOrDefault(des => des.Item2 == e.Column).Item1;

            if (Definitor != null && EditEndingOperation != null)
                this.EditEndingOperation(e.Row, Definitor);
            
            if (this.DisableCellEditEnding)
            {
                e.Cancel = true;
                return;
            }

            // SPECIAL CASE: Auto-generate Tech-Name from Name of elements of classes implementing IIdentifiableElement.
            // This is automatically done at class-level, but this DataGrid operates through a BindingList which doesn't writes
            // to the final instance (that with auto-gen) until editing of the row is done (such as when changing to another row, or window close).

            if (this.AutoGenerateCodeForIIdentifiableElementItems && this.PreviousIIdentiableElementItemName != null)
            {
                var ColDef = (e.Column.Header is FrameworkElement ?
                                ((FrameworkElement)e.Column.Header).Tag as MModelPropertyDefinitor :
                                null);
                var Target = e.Row.Item as IIdentifiableElement;
                var Editor = e.EditingElement as TextBox;

                if (Target == null || Editor == null || ColDef == null || ColDef.TechName != FormalElement.__Name.TechName)
                    this.PreviousIIdentiableElementItemName = null;
                else
                {
                    var TechNameWasEquivalentToName = (Target.TechName == PreviousIIdentiableElementItemName.TextToIdentifier());
                    if (TechNameWasEquivalentToName)
                        Target.TechName = Editor.Text.TextToIdentifier();

                    this.PreviousIIdentiableElementItemName = Target.Name;
                }
            }

            /*T var RowIndex = this.ItemsDataGrid.ItemContainerGenerator.IndexFromContainer(e.Row);
            Console.WriteLine("Editing for Cell ['{0}', #{1}] was {2}.", e.Column.DisplayIndex, RowIndex, e.EditAction); */

            // PENDING... Validate duplicates, names, codes, etc...

            // Commit the whole Row
            /*- if (e.EditAction != DataGridEditAction.Commit)
                return;

            this.IsDoingCommitEdit = true;
            this.ItemsDataGrid.CommitEdit(DataGridEditingUnit.Row, true);
            
            this.IsDoingCommitEdit = false; */
        }
        //- private bool IsDoingCommitEdit = false;     // Flag for avoiding recursive loop

        private void ItemsDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            /*T var RowIndex = this.EditingDataGrid.ItemContainerGenerator.IndexFromContainer(e.Row);
            Console.WriteLine("Editing for Row #{0} was {1}.", RowIndex, e.EditAction); */
        }
        
        public IEntityView ParentEntityView
        {
            get { return this.DominantViewChild.ParentEntityView; }
            set { this.DominantViewChild.ParentEntityView = value; }
        }

        public string ChildPropertyName { get { return this.DominantViewChild.ChildPropertyName; } }

        public void Refresh() { this.DominantViewChild.Refresh(); }

        public bool Apply() { return this.DominantViewChild.Apply(); }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}
