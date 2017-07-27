using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.EntityBase;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Data;
using System.Dynamic;

namespace Instrumind.Common.Visualization.Widgets
{
    /// <summary>
    /// Itemgs-grid maintainer services.
    /// </summary>
    public static class ItemsGridMaintainer
    {
        /// <summary>
        /// Default column font size.
        /// </summary>
        public const double DEF_FONT_SIZE = 10.0;

        /// <summary>
        /// Indicates that a row is being refreshed by its maintainer.
        /// (for preventing some actions to be performed between the row-remove + row-insert of the refresh)
        /// </summary>
        // IMPORTANT: This is a nasty trick used as last resource.
        public static bool IsRefreshingMaintainerRow { get; internal set; }

        /// <summary>
        /// Creates and returns a generic ItemsGridControl container for an IModelEntity class collection datasource.
        /// Optionally, a list of explicit Property Tech-Name plus DataGridColumn (null for hide) pairs can be specified (to be attached for matching Context entity properties).
        /// </summary>
        public static ItemsGridMaintainer<TEntity, TItem> CreateItemsGridControl<TEntity, TItem>(TEntity DataSourceOwner, IList<TItem> DataSource, Func<object, bool> DirectEditingConfirmator,
                                                                                                 params Tuple<string, DataGridColumn>[] ColumnControls)
            where TEntity : class, IModelEntity
            where TItem : class, IModelEntity
        {
            // Create columns
            var Definitor = MModelClassDefinitor.GetDefinitor(typeof(TItem));
            var ColDefs = new List<Tuple<IIdentifiableElement, DataGridColumn>>(Definitor.Properties.Count());
            var DisplayProperties = Definitor.Properties.OrderBy(prop => !prop.IsEditControlled).ThenBy(prop => prop.IsAdvanced);

            foreach (var Property in DisplayProperties)
                if (Property.IsDirectlyEditable)
                {
                    var ExplicitColumnControl = (ColumnControls == null ? null : ColumnControls.FirstOrDefault(colctl => colctl.Item1 == Property.TechName));

                    if (ExplicitColumnControl == null)
                        ColDefs.Add(Tuple.Create<IIdentifiableElement, DataGridColumn>(Property, CreateGridColumn(DataSourceOwner, Property)));
                    else
                        if (ExplicitColumnControl.Item2 != null)
                            ColDefs.Add(Tuple.Create<IIdentifiableElement, DataGridColumn>(Property, ExplicitColumnControl.Item2));
                }

            // Call the control creator.
            var Result = CreateItemsGridMaintainer(DataSourceOwner, DataSource, DirectEditingConfirmator, ColDefs.ToArray());
            return Result;
        }

        /// <summary>
        /// Creates and returns a generic ItemsGridControl container using the supplied column definitions.
        /// </summary>
        public static ItemsGridMaintainer<TEntity, TItem> CreateItemsGridMaintainer<TEntity, TItem>(TEntity DataSourceOwner, IList<TItem> DataSource, Func<object, bool> DirectEditingConfirmator,
                                                                                                    params Tuple<IIdentifiableElement, DataGridColumn>[] ColumnDefs)
                where TEntity : class, IModelEntity
        {
            var Result = new ItemsGridMaintainer<TEntity, TItem>(DataSourceOwner, DataSource, DirectEditingConfirmator);

            // Create columns
            if (ColumnDefs != null && ColumnDefs.Length > 0)
                foreach (var ColDef in ColumnDefs)
                    Result.VisualControl.AddColumn(ColDef);

            // Assign items source
            Result.VisualControl.SetDataSource(Result.BindedDataSource);

            return Result;
        }

        /// <summary>
        /// Creates and returns a data-grid-column for the supplied model property definitor.
        /// </summary>
        public static DataGridColumn CreateGridColumn(IModelEntity Context, MModelPropertyDefinitor Definitor)
        {
            DataGridColumn Result = null;

            if (Definitor.DeclaringType == typeof(bool))
            {
                var Column = new DataGridCheckBoxColumn();
                var Binder = new Binding(Definitor.TechName);
                Binder.Mode = BindingMode.TwoWay;
                Column.Binding = Binder;
                Result = Column;
            }
            else
                if (Definitor.DeclaringType == typeof(ImageSource))
                {
                    var Column = new DataGridTemplateColumn();
                    var EditorFactory = new FrameworkElementFactory(typeof(Image));
                    var Binder = new Binding(Definitor.TechName);
                    Binder.Mode = BindingMode.TwoWay;
                    EditorFactory.SetValue(Image.SourceProperty, Binder);
                    var CellTemplate = new DataTemplate();
                    CellTemplate.VisualTree = EditorFactory;
                    Column.CellTemplate = CellTemplate;
                    Result = Column;
                }
                else
                    if (Definitor.ItemsSourceGetter != null)
                    {
                        var Column = new DataGridComboBoxColumn();
                        Column.ItemsSource = Definitor.ItemsSourceGetter(Context);
                        if (!Definitor.ItemsSourceSelectedValuePath.IsAbsent())
                            Column.SelectedValuePath = Definitor.ItemsSourceSelectedValuePath;
                        if (!Definitor.ItemsSourceDisplayMemberPath.IsAbsent())
                            Column.DisplayMemberPath = Definitor.ItemsSourceDisplayMemberPath;
                        var Binder = new Binding(Definitor.TechName);
                        Binder.Mode = BindingMode.TwoWay;
                        Column.SelectedItemBinding = Binder;
                        var ComboStyle = new Style(typeof(ComboBox));
                        ComboStyle.Setters.Add(new Setter(ComboBox.FontSizeProperty, DEF_FONT_SIZE));
                        /*T This may crash? for edit value (as text)
                        ComboStyle.Setters.Add(new Setter(ComboBox.IsEditableProperty, true));
                        ComboStyle.Setters.Add(new Setter(ComboBox.IsReadOnlyProperty, false)); */
                        Column.EditingElementStyle = ComboStyle;
                        Column.ElementStyle = ComboStyle;
                        Result = Column;
                    }
                    else
                    {
                        var Column = new DataGridTextColumn();
                        var Binder = new Binding(Definitor.TechName);
                        Binder.Mode = (Definitor.DataType == typeof(string) || Definitor.BindingValueConverter != null
                                       ? BindingMode.TwoWay : BindingMode.OneWay);
                        Column.Binding = Binder;
                        Column.EditingElementStyle = new Style(typeof(TextBox));
                        Column.EditingElementStyle.Setters.Add(new Setter(TextBox.PaddingProperty, new Thickness(0, -1, 0, 0)));
                        Column.FontSize = DEF_FONT_SIZE;
                        Result = Column;
                    }

            var Entitler = new TextBlock();
            Entitler.Text = Definitor.Name;
            Entitler.ToolTip = Definitor.Summary;
            Entitler.Tag = Definitor;

            Result.Header = Entitler;
            Result.IsReadOnly = !Definitor.IsEditControlled;
            Result.MinWidth = Math.Max(Definitor.Name.Length, Definitor.DisplayMinLength) * Display.CHAR_PXWIDTH_DEFAULT;

            return Result;
        }
    }

    /// <summary>
    /// Generic Maintainer for TItem lists.
    /// </summary>
    public class ItemsGridMaintainer<TEntity, TItem> : IEntityViewChild
           where TEntity : class, IModelEntity
    {
        public TEntity DataSourceOwner { get; protected set; }

        public IList<TItem> OriginalDataSource { get; protected set; }

        public BindingList<TItem> BindedDataSource { get; protected set; }

        public ItemsGridControl VisualControl { get; protected set; }

        /*- /// <summary>
        /// Gets or set a Name for users to refer items being maintained.
        /// </summary>
        public string ItemKindName { get; set; }

        /// <summary>
        /// Gets or set a Tech-Name for users to refer items being maintained.
        /// </summary>
        public string ItemKindTechName { get; set; } */

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Constructor.
        /// </summary>
        public ItemsGridMaintainer(TEntity DataSourceOwner, IList<TItem> OriginalDataSource, Func<object, bool> DirectEditingConfirmator)
        {
            this.DataSourceOwner = DataSourceOwner;
            this.OriginalDataSource = OriginalDataSource;
            this.BindedDataSource = new BindingList<TItem>(OriginalDataSource);

            /*T // For detecting undesired change notifications
            this.BindedDataSource.ListChanged +=
                ((sender, args) =>
                {
                    Console.WriteLine("Bindinglist changed.");
                }); */

            this.VisualControl = new ItemsGridControl(this);
            this.VisualControl.DirectEditingConfirmator = DirectEditingConfirmator;

            // Initialize buttons
            this.VisualControl.ItemAdd.SetVisible(false);
            this.VisualControl.ItemInsert.SetVisible(false);
            this.VisualControl.ItemDelete.SetVisible(false);
            this.VisualControl.ItemEdit.SetVisible(false);
            this.VisualControl.ItemClone.SetVisible(false);

            this.VisualControl.ClipboardCut.SetVisible(false);
            this.VisualControl.ClipboardCopy.SetVisible(false);
            this.VisualControl.ClipboardPaste.SetVisible(false);
            this.VisualControl.SelectAll.SetVisible(false);
            this.VisualControl.UnselectAll.SetVisible(false);

            this.VisualControl.GlobalRefresh.SetVisible(false);
            this.VisualControl.GlobalApply.SetVisible(false);

            // .........................................................................
            this.VisualControl.EditingDataGrid.SelectionChanged += ((sender, scevargs) =>
            {
                var Handler = SelectedItemChanged;

                if (Handler == null)
                    return;

                Handler((TItem)((scevargs.AddedItems == null || scevargs.AddedItems.Count < 1)
                                ? null : scevargs.AddedItems[0]));
            });

            // .........................................................................
            // Set the Button Operation invocators
            this.VisualControl.ItemAdd.Click += ((sender, revargs) =>
            {
                if (CreateItemOperation == null)
                    return;

                var NewItem = CreateItemOperation(this.DataSourceOwner, this.BindedDataSource);
                if (NewItem == null)
                    return;

                this.BindedDataSource.Add(NewItem);

                var Handler = ItemCreated;
                if (Handler == null)
                    return;

                Handler(NewItem);
            });

            this.VisualControl.ItemInsert.Click += ((sender, revargs) =>
            {
                if (CreateItemOperation == null)
                    return;

                var NewItem = CreateItemOperation(this.DataSourceOwner, this.BindedDataSource);
                if (NewItem == null)
                    return;

                this.BindedDataSource.Insert(this.VisualControl.EditingDataGrid.SelectedIndex.EnforceRange(0, this.VisualControl.EditingDataGrid.Items.Count - 1),
                                             NewItem);

                var Handler = ItemCreated;
                if (Handler == null)
                    return;

                Handler(NewItem);
            });

            this.VisualControl.ItemDelete.Click += ((sender, revargs) =>
            {
                if (DeleteItemOperation == null || this.VisualControl.EditingDataGrid.SelectedItem == null)
                    return;

                var Item = (TItem)this.VisualControl.EditingDataGrid.SelectedItem;
                var WasDeleted = DeleteItemOperation(this.DataSourceOwner, this.BindedDataSource, Item);

                if (WasDeleted)
                {
                    this.BindedDataSource.Remove(Item);

                    var Handler = ItemDeleted;
                    if (Handler == null)
                        return;

                    Handler(Item);
                }
            });

            this.VisualControl.ItemEdit.Click += ((sender, revargs) =>
            {
                if (EditItemOperation == null || this.VisualControl.EditingDataGrid.SelectedItem == null)
                    return;

                var Item = (TItem)this.VisualControl.EditingDataGrid.SelectedItem;
                var EditApplied = EditItem(Item);

                /* Remember that a non-applied (no ok/apply button pressed) editing still is valid
                 * because of being pointing the same dependant instance
                if (!EditApplied)
                    return; */
            });

            this.VisualControl.ItemClone.Click += ((sender, revargs) =>
            {
                if (CloneItemOperation == null || this.VisualControl.EditingDataGrid.SelectedItem == null)
                    return;

                var SourceItem = (TItem)this.VisualControl.EditingDataGrid.SelectedItem;
                var NewItem = CloneItemOperation(this.DataSourceOwner, this.BindedDataSource, SourceItem);
                if (NewItem == null)
                    return;

                // Put the clone after the selected original item
                if (this.VisualControl.EditingDataGrid.SelectedIndex < this.VisualControl.EditingDataGrid.Items.Count - 1)
                    this.BindedDataSource.Insert(this.VisualControl.EditingDataGrid.SelectedIndex, NewItem);
                else
                    this.BindedDataSource.Add(NewItem);

                var Handler = ItemCloned;
                if (Handler == null)
                    return;

                Handler(NewItem, SourceItem);
            });

            // .........................................................................
            // Set the Grid Operation invocators
            this.VisualControl.EditingDataGrid.MouseDoubleClick += ((sender, mevargs) =>
            {
                if (this.EditItemOperation == null)
                    return;

                mevargs.Handled = true;

                var Row = ((FrameworkElement)mevargs.OriginalSource).GetNearestVisualDominantOfType<DataGridRow>();
                if (Row == null)
                    return;

                // Display.DialogMessage("Row...", "Item=[" + Row.Item.ToString() +"]");

                // Edit with double-click only if not can-edit-items-directly and pointing to a ComboBox-column
                if (this.VisualControl.EditingDataGrid.CurrentCell == null
                    || !(this.VisualControl.CanEditItemsDirectly &&
                         this.VisualControl.EditingDataGrid.CurrentCell.Column is DataGridComboBoxColumn))
                    this.EditItem((TItem)Row.Item);
            });
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Appends the supplied record to the mantained records collection.
        /// </summary>
        public void AppendRecord(TItem Record)
        {
            this.BindedDataSource.Add(Record);
        }

        /// <summary>
        /// Empties the list of records.
        /// </summary>
        public void ResetRecords()
        {
            this.BindedDataSource.Clear();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Edits the specified Item and refreshes it associated row.
        /// </summary>
        public bool EditItem(TItem Item)
        {
            var EditApplied = EditItemOperation(this.DataSourceOwner, this.BindedDataSource, Item);

            if (EditApplied)
            {
                var Handler = ItemEdited;
                if (Handler != null)
                    Handler(Item);

                // Refresh the row
                // Not working:  this.VisualControl.GetBindingExpression(DataGrid.ItemsSourceProperty).UpdateTarget();
                var SelectedItem = this.VisualControl.EditingDataGrid.SelectedItem;

                var ItemIndex = this.BindedDataSource.IndexOf(Item);

                ItemsGridMaintainer.IsRefreshingMaintainerRow = true;

                this.BindedDataSource.Remove(Item);
                this.BindedDataSource.Insert(ItemIndex, Item);

                ItemsGridMaintainer.IsRefreshingMaintainerRow = false;

                this.VisualControl.EditingDataGrid.SelectedItem = SelectedItem;
            }

            return EditApplied;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        public IAlternativeRecordEditor<TEntity, TItem> AlternativeEditor
        {
            get { return this.AlternativeEditor_; }
            set
            {
                if (this.AlternativeEditor_ == value)
                    return;

                this.VisualControl.AlternativeEditorContainer.Child = null;
                this.AlternativeEditor_ = value;

                if (value == null)
                {
                    this.VisualControl.AlternativeEditorContainer.SetVisible(false);
                    this.VisualControl.EditingDataGrid.SetVisible(true);
                    return;
                }

                if (this.HideGridIfAlternativeEditorExists)
                    this.VisualControl.EditingDataGrid.SetVisible(false);

                this.VisualControl.AlternativeEditorContainer.Child = this.AlternativeEditor_.VisualControl;
                this.AlternativeEditor_.InitializeFromGrid(this);
                this.VisualControl.AlternativeEditorContainer.SetVisible(true);
            }
        }
        public IAlternativeRecordEditor<TEntity, TItem> AlternativeEditor_ = null;

        public bool HideGridIfAlternativeEditorExists = false;

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Occurs when the selected item has changed.
        /// </summary>
        public event Action<TItem> SelectedItemChanged;

        /// <summary>
        /// Occurs after a new-item (passed) has been Created.
        /// </summary>
        public event Action<TItem> ItemCreated;

        /// <summary>
        /// Occurs after a preexistent-item (passed) has been Deleted.
        /// </summary>
        public event Action<TItem> ItemDeleted;

        /// <summary>
        /// Occurs after an existent-item (passed) has been Edited.
        /// </summary>
        public event Action<TItem> ItemEdited;

        /// <summary>
        /// Occurs after a new-item (1st passed) has been Cloned from a preexistent one (2nd passed).
        /// </summary>
        public event Action<TItem, TItem> ItemCloned;

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Operation for adding or inserting a new Item.
        /// It gets the owner entity, edited list and must return a new Item or null if cancelled.
        /// </summary>
        public Func<TEntity, IList<TItem>, TItem> CreateItemOperation
        {
            get { return this.CreateItemOperation_; }
            set
            {
                this.CreateItemOperation_ = value;
                this.VisualControl.ItemAdd.SetVisible(value != null);
                this.VisualControl.ItemInsert.SetVisible(value != null);
                // this.VisualControl.ClipboardPaste.SetVisible(value != null); // Used although the content will not be obtained calling the CreateItemOperation
            }
        }
        protected Func<TEntity, IList<TItem>, TItem> CreateItemOperation_ = null;

        /// <summary>
        /// Operation for deleting an existing Item.
        /// It gets the owner entity and edited list plus the selected-for-deletion Item and must return true for proceed or false to cancel.
        /// </summary>
        public Func<TEntity, IList<TItem>, TItem, bool> DeleteItemOperation
        {
            get { return this.DeleteItemOperation_; }
            set
            {
                this.DeleteItemOperation_ = value;
                this.VisualControl.ItemDelete.SetVisible(value != null);
                // this.VisualControl.ClipboardCut.SetVisible(value != null);
            }
        }
        protected Func<TEntity, IList<TItem>, TItem, bool> DeleteItemOperation_ = null;

        /// <summary>
        /// Operation for edit an existing Item.
        /// It gets the entity owner and edited list plus the selected-for-editing Item, and return indication of completion (true) or cancellation (false).
        /// </summary>
        public Func<TEntity, IList<TItem>, TItem, bool> EditItemOperation
        {
            get { return this.EditItemOperation_; }
            set
            {
                this.EditItemOperation_ = value;
                this.VisualControl.ItemEdit.SetVisible(value != null);
            }
        }
        protected Func<TEntity, IList<TItem>, TItem, bool> EditItemOperation_ = null;

        /// <summary>
        /// Operation for cloning an existing Item.
        /// It gets the entity owner and edited list plust the selected-for-clonation Item and must return the generated clone.
        /// </summary>
        public Func<TEntity, IList<TItem>, TItem, TItem> CloneItemOperation
        {
            get { return this.CloneItemOperation_; }
            set
            {
                this.CloneItemOperation_ = value;
                this.VisualControl.ItemClone.SetVisible(value != null);
                // this.VisualControl.ClipboardCopy.SetVisible(value != null);
            }
        }
        protected Func<TEntity, IList<TItem>, TItem, TItem> CloneItemOperation_ = null;

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Operation for refreshing all items.
        /// It returns a new items source.
        /// </summary>
        public Func<IEnumerable<TItem>> RefreshItemsOperation
        {
            get { return this.RefreshItemsOperation_; }
            set
            {
                this.RefreshItemsOperation_ = value;
                this.VisualControl.GlobalRefresh.SetVisible(value != null);
            }
        }
        protected Func<IEnumerable<TItem>> RefreshItemsOperation_ = null;

        /// <summary>
        /// Operation for Applying all items.
        /// It gets the current items source.
        /// </summary>
        public Action<IEnumerable<TItem>> ApplyItemsOperation
        {
            get { return this.ApplyItemsOperation_; }
            set
            {
                this.ApplyItemsOperation_ = value;
                this.VisualControl.GlobalApply.SetVisible(value != null);
            }
        }
        protected Action<IEnumerable<TItem>> ApplyItemsOperation_ = null;

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------

        public IEntityView ParentEntityView { get; set; }

        public string ChildPropertyName { get; set; }

        public void Refresh() { }

        public bool Apply() { return true; }
    }
}
