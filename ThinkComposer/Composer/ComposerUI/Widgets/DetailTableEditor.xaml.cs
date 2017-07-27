using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.Visualization;
using Instrumind.Common.Visualization.Widgets;

using Instrumind.ThinkComposer.Definitor;
using Instrumind.ThinkComposer.Definitor.DefinitorUI;
using Instrumind.ThinkComposer.MetaModel;
using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.MetaModel.InformationMetaModel;
using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;
using Instrumind.ThinkComposer.Model.GraphModel;
using Instrumind.ThinkComposer.Model.InformationModel;

namespace Instrumind.ThinkComposer.Composer.ComposerUI.Widgets
{
    /// <summary>
    /// Interaction logic for DetailTableEditor.xaml
    /// </summary>
    public partial class DetailTableEditor : UserControl
    {
        /// <summary>
        /// Exposes to the user a data editor, for the the supplied Detail Table, Designator and Custom-Look, and returns indication of change.
        /// </summary>
        public static bool Edit(Table DetailTable, TableDetailDesignator Designator, TableAppearance CustomLook = null, bool ForceSingleRecord = false)
        {
            if (DetailTable.Definition.FieldDefinitions.Count < 1)
            {
                if (Display.DialogMessage("Warning!", "No declared fields in the '" + DetailTable.Definition.Name +"' Table-Structure.\n\n" +
                                                      "Do you want to declare them for " + (Designator.Owner.IsGlobal ? "all Ideas of type '" : "the Idea '") +
                                                      ((IIdentifiableElement)Designator.Owner.Owner).Name + "' ?",
                                                      EMessageType.Warning, MessageBoxButton.YesNo)
                    != MessageBoxResult.Yes)
                    return false;

                var EditResult = DomainServices.EditDetailDesignator(Designator, Designator.Owner.IsGlobal, Designator.EditEngine);
                if (!EditResult.Item1.IsTrue())
                    return false;
            }

            var WorkTable = DetailTable.CreateClone(ECloneOperationScope.Deep, null);
            var Editor = new DetailTableEditor(WorkTable, Designator, CustomLook.NullDefault(new TableAppearance()), ForceSingleRecord);

            DialogOptionsWindow EditingWindow = null;   // Do not declare as static to allow multiple dialogs open!
            var Changed = Display.OpenContentDialogWindow<DetailTableEditor>(ref EditingWindow, "Edit data of table '" + WorkTable.Designation.Name + "'", Editor).IsTrue();

            if (Editor.ApplyChanges)
                DetailTable.UpdateContentFrom(WorkTable);

            return Editor.ApplyChanges; //? || Changed);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        public DetailTableEditor()
        {
            InitializeComponent();
        }

        public DetailTableEditor(Table SourceTable, TableDetailDesignator SourceDesignator, TableAppearance SourceCustomLook, bool ForceSingleRecord = false)
             : this()
        {
            this.SourceTable = SourceTable;
            this.SourceDesignator = SourceDesignator;
            this.SourceCustomLook = SourceCustomLook;

            // Operations: Append-Record and Reset-Records...
            Tuple<Action<TableRecord>,Action> RecordOperations = null;

            if (this.SourceCustomLook.IsMultiRecord && !ForceSingleRecord)
                RecordOperations = SetMultiRecordEditor();
            else
                RecordOperations = SetSingleRecordEditor();

            var ImportWidget = DomainServices.CreateTransferWidget(SourceDesignator, SourceTable.OwnerIdea, false, false,
                    (compatdef, datarecords, append) =>
                    {
                        if (compatdef == null)
                        {
                            Display.DialogMessage("Attention", "Cannot load data with incomptaible table-structure.",
                                                  EMessageType.Warning);
                            return;
                        }

                        if (!append)
                            RecordOperations.Item2();   // Reset-Records

                        foreach (var datarecord in datarecords)
                            RecordOperations.Item1(new TableRecord(SourceTable, datarecord));   // Append-Record
                    },
                    this.SourceTable);

            ImportWidget.MaxWidth = 300;
            ImportWidget.HorizontalAlignment = HorizontalAlignment.Left;
            DockPanel.SetDock(ImportWidget, Dock.Left);
            this.BottomPanel.Children.Add(ImportWidget);
        }

        public Table SourceTable { get; protected set; }

        public TableDetailDesignator SourceDesignator { get; protected set; }

        public TableAppearance SourceCustomLook { get; protected set; }

        public ItemsGridMaintainer<Table, TableRecord> MultiRecordMaintainer { get; protected set; }

        public SingleTableRecordEditor<Table, TableRecord> SingleRecordMaintainer { get; protected set; }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        public Tuple<Action<TableRecord>,Action> SetSingleRecordEditor()
        {
            this.SingleRecordMaintainer = new SingleTableRecordEditor<Table, TableRecord>(this.SourceTable.Definition);

            // Start with an empty record
            if (this.SourceTable.Count < 1)
            {
                var NewRecord = new TableRecord(this.SourceTable);
                this.SourceTable.Add(NewRecord);
            }

            this.SingleRecordMaintainer.TargetItem = this.SourceTable[0];

            this.BackPanel.Children.Clear();
            this.BackPanel.Children.Add(this.SingleRecordMaintainer);

            return Tuple.Create<Action<TableRecord>, Action>((record) => this.SingleRecordMaintainer.AppendRecord(record),
                                                             () => this.SingleRecordMaintainer.Reset());
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        public Tuple<Action<TableRecord>,Action> SetMultiRecordEditor()
        {
            // Create columns
            var ColDefs = new List<Tuple<IIdentifiableElement, DataGridColumn>>();

            foreach (var FieldDef in this.SourceTable.Definition.FieldDefinitions)
                ColDefs.Add(Tuple.Create<IIdentifiableElement, DataGridColumn>(FieldDef, CreateGridColumn(FieldDef, this.SourceTable.OwnerIdea.OwnerComposition)));

            // Call the control creator.
            this.MultiRecordMaintainer = ItemsGridMaintainer.CreateItemsGridMaintainer(this.SourceTable, this.SourceTable, null, ColDefs.ToArray());
            this.MultiRecordMaintainer.CreateItemOperation = ((ownent, edlist) => new TableRecord(this.SourceTable));
            this.MultiRecordMaintainer.CloneItemOperation = ((ownent, edlist, tblrec) => tblrec.CreateClone(ECloneOperationScope.Deep, null));
            this.MultiRecordMaintainer.DeleteItemOperation = ((ownent, edlist, tblrec) => true);
            this.MultiRecordMaintainer.EditItemOperation = null;
            this.MultiRecordMaintainer.VisualControl.EditEndingOperation = this.ReactToFieldEditEnding;

            /*? ((tblrec) =>
                {
                    Console.WriteLine("Label: [{0}]", tblrec.Label);
                    return Display.DialogMessage("PENDING...", "Edit this user-defined Table Record!").IsTrue();
                }); */

            this.MultiRecordMaintainer.VisualControl.Loaded += MultiRecordEditor_Loaded;

            this.BackPanel.Children.Clear();
            this.BackPanel.Children.Add(this.MultiRecordMaintainer.VisualControl);

            return Tuple.Create<Action<TableRecord>, Action>((record) => this.MultiRecordMaintainer.AppendRecord(record),
                                                             () => this.MultiRecordMaintainer.ResetRecords());
        }

        void ReactToFieldEditEnding(DataGridRow Row, IIdentifiableElement FieldDefinitor)
        {
            var Record = Row.Item as TableRecord;
            if (Record == null)
                return;

            object Value = Record.GetStoredValue(FieldDefinitor.TechName);
            //T Console.WriteLine("Edited: Item={0}, Field={1}, Value={2}", Row.Item, FieldDefinitor.TechName, Value);
        }

        void MultiRecordEditor_Loaded(object sender, RoutedEventArgs e)
        {
            this.MultiRecordMaintainer.VisualControl.SetReadyForEditing();

            // Start with an empty record
            if (this.MultiRecordMaintainer.BindedDataSource.Count < 1)
            {
                var NewRecord = new TableRecord(this.SourceTable);
                this.MultiRecordMaintainer.BindedDataSource.Add(NewRecord);
                this.MultiRecordMaintainer.VisualControl.SelectedItem = NewRecord;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        public bool ApplyChanges { get; protected set; }

        private void BtnAccept_Click(object sender, RoutedEventArgs e)
        {
            this.ApplyChanges = true;
            Window.GetWindow(this).Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).Close();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Creates and returns a data-grid-column for the supplied table Field definitor.
        /// </summary>
        public static DataGridColumn CreateGridColumn(FieldDefinition Definitor, IRecognizableComposite ReferencesContext)
        {
            DataGridColumn Result = null;
            FrameworkElementFactory CellEditFactory = null;
            FrameworkElementFactory CellShowFactory = null;
            var EditingTemplate = new DataTemplate();
            var ShowingTemplate = new DataTemplate();
            var Column = new ExtendedDataGridTemplateColumn();

            var ShowBinder = new Binding(Definitor.TechName);
            ShowBinder.ConverterCulture = System.Globalization.CultureInfo.CurrentCulture;
            ShowBinder.Mode = BindingMode.OneWay;

            CellShowFactory = new FrameworkElementFactory(typeof(TextBlock));
            CellShowFactory.SetValue(TextBlock.TextProperty, ShowBinder);

            var EditBinder = new Binding(Definitor.TechName);
            EditBinder.ConverterCulture = System.Globalization.CultureInfo.CurrentCulture;
            EditBinder.Mode = BindingMode.TwoWay;

            if (Definitor.FieldType is BasicDataType)
            {
                var BasicKind = ((BasicDataType)Definitor.FieldType);

                ShowBinder.StringFormat = BasicKind.DisplayFormat;
                CellShowFactory.SetValue(TextBlock.TextAlignmentProperty, BasicKind.DisplayAlignment);
                EditBinder.StringFormat = BasicKind.DisplayFormat;

                if (BasicKind is TextType)
                {
                    var TextKind = BasicKind as TextType;
                    CellEditFactory = new FrameworkElementFactory(typeof(ExtendedEditText));

                    CellEditFactory.SetValue(ExtendedEditText.StorageFieldNameProperty, Definitor.TechName);
                    CellEditFactory.SetValue(ExtendedEditText.ApplyDirectAccessProperty, true);
                    CellEditFactory.SetValue(ExtendedEditText.ValueProperty, EditBinder);
                    CellEditFactory.SetValue(ExtendedEditText.MaxLengthProperty, TextKind.SizeLimit);
                    //- CellEditFactory.SetValue(ExtendedEditText.PaddingProperty, new Thickness(0, -1, 0, 0));

                    if (Definitor.ValuesSource != null && Definitor.ValuesSource is Table
                        && Definitor.ValuesSource != Domain.Unassigned_BaseTable)
                    {
                        CellEditFactory.SetValue(ExtendedEditText.ValuesSourceProperty, Definitor.ValuesSource);
                        CellEditFactory.SetValue(ExtendedEditText.ValuesSourceMemberPathProperty, "Label");
                    }

                    if (ReferencesContext != null && Definitor.IdeaReferencingProperty != null
                        && Definitor.IdeaReferencingProperty != Domain.Unassigned_IdeaReferencingPropertyProperty)
                    {
                        CellEditFactory.SetValue(ExtendedEditText.CompositesSourceProperty, ReferencesContext.CompositeMembers);
                        CellEditFactory.SetValue(ExtendedEditText.CompositesSourceMemberPathProperty, Definitor.IdeaReferencingProperty.TechName);
                    }
                }
                else
                    if (BasicKind is NumberType)
                    {
                        var NumberKind = BasicKind as NumberType;
                        CellEditFactory = new FrameworkElementFactory(typeof(MaskEditNumber));

                        CellEditFactory.SetValue(MaskEditNumber.StorageFieldNameProperty, Definitor.TechName);
                        CellEditFactory.SetValue(MaskEditNumber.ApplyDirectAccessProperty, true);
                        CellEditFactory.SetValue(MaskEditNumber.FormatProperty, NumberKind.DisplayFormat);
                        CellEditFactory.SetValue(MaskEditNumber.IntegerDigitsProperty, NumberKind.IntegerDigits);
                        CellEditFactory.SetValue(MaskEditNumber.DecimalDigitsProperty, NumberKind.DecimalDigits);
                        CellEditFactory.SetValue(MaskEditNumber.MinLimitProperty, NumberKind.MinLimit);
                        CellEditFactory.SetValue(MaskEditNumber.MaxLimitProperty, NumberKind.MaxLimit);
                        CellEditFactory.SetValue(MaskEditNumber.ValueProperty, EditBinder);

                        if (Definitor.ValuesSource != null && Definitor.ValuesSource is Table)
                        {
                            CellEditFactory.SetValue(MaskEditNumber.ValuesSourceProperty, Definitor.ValuesSource);
                            CellEditFactory.SetValue(MaskEditNumber.ValuesSourceMemberPathProperty, "Label");
                            CellEditFactory.SetValue(MaskEditNumber.ValuesSourceNumericConverterProperty, TableRecord.TableRecordToNumericConverter);
                        }
                    }
                    else
                        if (BasicKind is DateTimeType)
                        {
                            var DateTimeKind = BasicKind as DateTimeType;
                            CellEditFactory = new FrameworkElementFactory(typeof(MaskEditDateTime));

                            CellEditFactory.SetValue(MaskEditDateTime.StorageFieldNameProperty, Definitor.TechName);
                            CellEditFactory.SetValue(MaskEditDateTime.ApplyDirectAccessProperty, true);

                            if (!DateTimeKind.HasDatePart)
                                CellEditFactory.SetValue(MaskEditDateTime.HasDateProperty, false);
                            else
                                if (!DateTimeKind.HasTimePart)
                                    CellEditFactory.SetValue(MaskEditDateTime.HasTimeProperty, false);

                            CellEditFactory.SetValue(MaskEditDateTime.ValueProperty, EditBinder);
                        }
                        else
                            if (BasicKind is ChoiceType)
                            {
                                var ChoiceKind = BasicKind as ChoiceType;

                                // Notice the use of IsEquivalent() to preserve comparability after deserialization
                                if (ChoiceKind.IsEquivalent(DataType.DataTypeSwitch))
                                {
                                    CellEditFactory = new FrameworkElementFactory(typeof(ExtendedEditCheckBox));
                                    CellShowFactory = new FrameworkElementFactory(typeof(ExtendedEditCheckBox));

                                    CellEditFactory.SetValue(ExtendedEditCheckBox.StorageFieldNameProperty, Definitor.TechName);
                                    CellEditFactory.SetValue(ExtendedEditCheckBox.ApplyDirectAccessProperty, true);
                                    CellShowFactory.SetValue(ExtendedEditCheckBox.StorageFieldNameProperty, Definitor.TechName);
                                    CellShowFactory.SetValue(ExtendedEditCheckBox.ApplyDirectAccessProperty, true);

                                    CellEditFactory.SetValue(ExtendedEditCheckBox.IsCheckedProperty, EditBinder);
                                    CellEditFactory.SetValue(ExtendedEditCheckBox.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                                    CellShowFactory.SetValue(ExtendedEditCheckBox.IsCheckedProperty, ShowBinder);
                                    CellShowFactory.SetValue(ExtendedEditCheckBox.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                                }
                                else
                                {
                                    CellEditFactory = new FrameworkElementFactory(typeof(ExtendedEditComboBox));
                                    //- CellShowFactory = new FrameworkElementFactory(typeof(ExtendedEditComboBox));

                                    CellEditFactory.SetValue(ExtendedEditComboBox.StorageFieldNameProperty, Definitor.TechName);
                                    CellEditFactory.SetValue(ExtendedEditComboBox.ApplyDirectAccessProperty, true);
                                    //- CellShowFactory.SetValue(ExtendedEditComboBox.StorageFieldNameProperty, Definitor.TechName);
                                    //- CellShowFactory.SetValue(ExtendedEditComboBox.ApplyDirectAccessProperty, true);

                                    CellEditFactory.SetValue(ExtendedEditComboBox.ItemsSourceProperty, ChoiceKind.GetRegisteredOptions());
                                    CellEditFactory.SetValue(ExtendedEditComboBox.SelectedValuePathProperty, "Item1");  // The Choice/Option
                                    CellEditFactory.SetValue(ExtendedEditComboBox.DisplayMemberPathProperty, "Item2");  // The Name
                                    //- CellShowFactory.SetValue(ExtendedEditComboBox.ItemsSourceProperty, ChoiceKind.GetRegisteredOptions());
                                    //- CellShowFactory.SetValue(ExtendedEditComboBox.SelectedValuePathProperty, "Item1");  // The Choice/Option
                                    //- CellShowFactory.SetValue(ExtendedEditComboBox.DisplayMemberPathProperty, "Item2");  // The Name
                                    //- CellShowFactory.SetValue(ExtendedEditComboBox.IsReadOnlyProperty, true);
                                    ShowBinder.Path = new PropertyPath(Definitor.TechName + ".Item2");

                                    // Special case: Binds KVP instead on only the Key. So, update of values is made directly.
                                    // Really, for the Extended-Edit-Controls, the binding is not needed.
                                    EditBinder.Mode = BindingMode.OneWay;
                                    CellEditFactory.SetValue(ExtendedEditComboBox.SelectedItemProperty, EditBinder);
                                    //- CellShowFactory.SetValue(ExtendedEditComboBox.SelectedItemProperty, ShowBinder);
                                }
                            }
            }
            else
                if (Definitor.FieldType is TableRecordLinkType)
                {
                    CellEditFactory = new FrameworkElementFactory(typeof(ExtendedEditComboBox));

                    CellEditFactory.SetValue(ExtendedEditComboBox.StorageFieldNameProperty, Definitor.TechName);
                    CellEditFactory.SetValue(ExtendedEditComboBox.ApplyDirectAccessProperty, true);

                    CellEditFactory.SetValue(ExtendedEditComboBox.ItemsSourceProperty, Definitor.ValuesSource);
                    CellEditFactory.SetValue(ExtendedEditComboBox.DisplayMemberPathProperty, "Label");
                    ShowBinder.Path = new PropertyPath(Definitor.TechName + ".Label");

                    /*? Didn't work (editable cell):
                    CellShowFactory.SetValue(ExtendedEditComboBox.IsReadOnlyProperty, false);
                    CellShowFactory.SetValue(ExtendedEditComboBox.IsEditableProperty, true); */

                    // Really, for the Extended-Edit-Controls, the binding is not needed.
                    EditBinder.Mode = BindingMode.OneWay;
                    CellEditFactory.SetValue(ExtendedEditComboBox.SelectedItemProperty, EditBinder);

                    // Didn't work
                    // ShowBinder.Converter = new GenericConverter<TableRecord, string>(trec => (trec == null ? null : trec.Label), str => null);
                }
                else
                    if (Definitor.FieldType is IdeaLinkType)
                    {
                        /* Binding only works on record-fields, so don't use as in the next way...
                        var SpecBinder = new MultiBinding();
                        SpecBinder.Bindings.Add(new Binding("NameCaption"));
                        SpecBinder.Bindings.Add(new Binding("DescriptiveCaption"));
                        SpecBinder.StringFormat = "{}{0} {1}";
                        SpecBinder.ConverterCulture = System.Globalization.CultureInfo.CurrentCulture;
                        SpecBinder.Mode = BindingMode.OneWay;
                        CellShowFactory.SetValue(TextBlock.TextProperty, SpecBinder); */

                        CellEditFactory = new FrameworkElementFactory(typeof(TreeItemSelector));
                        CellShowFactory = new FrameworkElementFactory(typeof(TreeItemSelector));

                        CellEditFactory.SetValue(TreeItemSelector.StorageFieldNameProperty, Definitor.TechName);
                        CellEditFactory.SetValue(TreeItemSelector.ApplyDirectAccessProperty, true);
                        CellEditFactory.SetValue(TreeItemSelector.ValueProperty, EditBinder);
                        CellShowFactory.SetValue(TreeItemSelector.StorageFieldNameProperty, Definitor.TechName);
                        CellShowFactory.SetValue(TreeItemSelector.ApplyDirectAccessProperty, true);
                        CellShowFactory.SetValue(TreeItemSelector.ValueProperty, EditBinder);

                        if (ReferencesContext != null)
                            CellEditFactory.SetValue(TreeItemSelector.CompositesSourceProperty, ReferencesContext.CompositeMembers);
                    }
                    else
                        if (Definitor.FieldType is TableType)
                        {
                            EditBinder.Path = new PropertyPath(".");  // This references the Table-Record, not a field within it.
                            ShowBinder.Path = new PropertyPath(Definitor.TechName + ".RecordsLabel");

                            CellEditFactory = new FrameworkElementFactory(typeof(PaletteButton));
                            CellShowFactory = new FrameworkElementFactory(typeof(TextBlock));

                            CellEditFactory.SetValue(PaletteButton.ButtonTextProperty, ShowBinder);
                            CellEditFactory.SetValue(PaletteButton.ButtonShowImageProperty, false);
                            // CellEditFactory.SetValue(PaletteButton.ButtonImageProperty, Display.GetAppImage("table_edit.png"));
                            CellEditFactory.SetValue(PaletteButton.ToolTipProperty, "Edit data values");
                            CellEditFactory.SetValue(PaletteButton.ButtonActionFieldNameProperty, Definitor.TechName);
                            CellEditFactory.SetValue(PaletteButton.ButtonActionFieldSourceProperty, EditBinder);
                            CellEditFactory.SetValue(PaletteButton.ButtonClickActionProperty, (Action<string, object>)
                                ((fname, fsource) =>
                                 {
                                     //T Display.DialogMessage("Field: [" + fname.NullDefault("?") + "]", "Value: [" + fvalue.NullDefault("") + "]");
                                     var SourceRecord = fsource as TableRecord;
                                     if (SourceRecord == null)
                                         return;

                                     var StoredTable = SourceRecord.GetStoredValue(fname) as Table;
                                     if (Definitor.ContainedTableDesignator == null)
                                         return;

                                     if (StoredTable == null)
                                     {
                                         StoredTable = new Table(SourceRecord.OwnerTable.OwnerIdea,
                                                                 Definitor.ContainedTableDesignator.Assign<DetailDesignator>(false));

                                         // This must be set for explicit change. Databinding didn't work.
                                         SourceRecord.SetStoredValue(Definitor, StoredTable);
                                         //-? Ctl.ButtonText = StoredTable.RecordsLabel;
                                     }

                                     Definitor.OwnerTableDef.EditEngine.StartCommandVariation("Edit Field contained Table");

                                     var Changed = DetailTableEditor.Edit(StoredTable, Definitor.ContainedTableDesignator, null, Definitor.ContainedTableIsSingleRecord);
                                     Definitor.OwnerTableDef.EditEngine.CompleteCommandVariation();

                                     if (!Changed)
                                         Definitor.OwnerTableDef.EditEngine.Undo();
                                 }));
                            CellEditFactory.SetValue(PaletteButton.CursorProperty, Cursors.Hand);

                            CellShowFactory.SetValue(TextBlock.TextProperty, ShowBinder);
                            CellShowFactory.SetValue(TextBlock.CursorProperty, Cursors.Hand);
                        }
                        else
                            if (Definitor.FieldType is PictureType)
                            {
                                EditBinder.Path = new PropertyPath(".");  // This references the Table-Record, not a field within it.
                                ShowBinder.Path = new PropertyPath(Definitor.TechName + ".Image");
                                // EditBinder.Path = new PropertyPath(Definitor.TechName + ".Image");
                                // ShowBinder.Path = new PropertyPath(Definitor.TechName + ".Image");

                                CellEditFactory = new FrameworkElementFactory(typeof(ImagePickerSimple));
                                CellShowFactory = new FrameworkElementFactory(typeof(ImagePickerSimple));

                                CellEditFactory.SetValue(ImagePickerSimple.CursorProperty, Cursors.Hand);
                                CellEditFactory.SetValue(ImagePickerSimple.SelectedImageProperty, ShowBinder);
                                CellEditFactory.SetValue(ImagePickerSimple.ImagePickerActionFieldNameProperty, Definitor.TechName);
                                CellEditFactory.SetValue(ImagePickerSimple.ImagePickerActionFieldSourceProperty, EditBinder);

                                CellShowFactory.SetValue(ImagePickerSimple.CursorProperty, Cursors.Hand);
                                CellShowFactory.SetValue(ImagePickerSimple.SelectedImageProperty, ShowBinder);
                                CellShowFactory.SetValue(ImagePickerSimple.ImagePickerActionFieldNameProperty, Definitor.TechName);
                                CellShowFactory.SetValue(ImagePickerSimple.ImagePickerActionFieldSourceProperty, EditBinder);
                                CellShowFactory.SetValue(ImagePickerSimple.ToolTipProperty, "Edit picture");

                                Action<string, object, object> ImagePickAction =
                                    ((fname, fsource, ctl) =>
                                     {
                                         //T Display.DialogMessage("Field: [" + fname.NullDefault("?") + "]", "Value: [" + fvalue.NullDefault("") + "]");
                                         var SourceRecord = fsource as TableRecord;
                                         if (SourceRecord == null)
                                             return;

                                        //  var StoredPicture = SourceRecord.GetStoredValue(fname) as ImageAssignment;
                                        //- var StoredPicture = fsource as ImageAssignment;

                                         var Selection = Display.DialogGetImageFromFile();
                                         if (Selection != null)
                                         {
                                             Definitor.OwnerTableDef.EditEngine.StartCommandVariation("Edit Field contained Picture");

                                             // This must be set for explicit change. Databinding didn't work.

                                             // IMPORTANT: Always store a whole new StoredPicture (ImageAssignment),
                                             // and don't just change the StoredPicture.Image property, in order to support undo/redo.
                                             var StoredPicture = Selection.AssignImage();
                                             SourceRecord.SetStoredValue(Definitor, StoredPicture);

                                             var ImgCtl = ctl as ImagePickerSimple;
                                             if (ImgCtl != null)
                                                ImgCtl.SelectedImage = Selection;

                                             Definitor.OwnerTableDef.EditEngine.CompleteCommandVariation();
                                         }
                                     });

                                CellEditFactory.SetValue(ImagePickerSimple.ImagePickerSelectActionProperty, ImagePickAction);
                                CellShowFactory.SetValue(ImagePickerSimple.ImagePickerSelectActionProperty, ImagePickAction);

                                /*- //! PENDING: NOT WORKING! SOLUTION: CREATE A CUSTOM IMAGE-CONTROL
                                var MouseHandler = new MouseButtonEventHandler(new Action<object, MouseButtonEventArgs>(
                                    (obj, args) =>
                                    {
                                        var Ctl = (obj as Image).NullDefault(args.Source as Image);

                                        var StoredPicture = Ctl.Tag as ImageAssignment;
                                        
                                        var Selection = Display.DialogGetImageFromFile();
                                        if (Selection != null)
                                        {
                                            Definitor.OwnerTableDef.EditEngine.StartCommandVariation("Edit Field contained Picture");

                                            if (StoredPicture == null)
                                            {
                                                StoredPicture = new ImageAssignment();
                                                Ctl.Tag = StoredPicture;

                                                // This must be set for explicit change. Databinding didn't work.
                                                //? exp.InstanceSource.SetStoredValue(exp.SourceFieldDefinitor, StoredPicture);
                                            }

                                            StoredPicture.Image = Selection;
                                            Ctl.Source = Selection;

                                            Definitor.OwnerTableDef.EditEngine.CompleteCommandVariation();
                                        }
                                    }));

                                CellEditFactory.AddHandler(Image.MouseLeftButtonDownEvent, MouseHandler, true);
                                CellShowFactory.AddHandler(Image.MouseLeftButtonDownEvent, MouseHandler, true);  */
                            }

            // PENDING: IMPLEMENT FOR IMAGE TYPE
            // SEE: http://stackoverflow.com/questions/1951839/how-do-i-show-image-in-wpf-datagrid-column-programmatically

            // If Column and Factory were determined, then assign template
            if (Column != null && CellEditFactory != null)
            {
                // Setting of the editing template
                EditingTemplate.VisualTree = CellEditFactory;
                Column.CellEditingTemplate = EditingTemplate;

                // Setting of the showing template
                ShowingTemplate.VisualTree = CellShowFactory;
                Column.CellTemplate = ShowingTemplate;

                // Final assign
                Column.IsReadOnly = false;
                Result = Column;
            }

            if (Result == null)
            {
                // For unknown types assign a read-only text column
                Console.WriteLine("Editing set to read-only for column '{0}' with type '{1}'.", Definitor.TechName, Definitor.FieldType);

                var TextColumn = new DataGridTextColumn();
                TextColumn.EditingElementStyle = new Style(typeof(TextBox));
                TextColumn.EditingElementStyle.Setters.Add(new Setter(TextBox.PaddingProperty, new Thickness(0, -1, 0, 0)));
                TextColumn.IsReadOnly = true;

                var Binder = new Binding(Definitor.TechName);
                Binder.Mode = BindingMode.OneWay;

                TextColumn.Binding = Binder;

                Result = TextColumn;
            }

            var Entitler = new TextBlock();
            Entitler.Text = Definitor.Name;
            Entitler.ToolTip = Definitor.Summary;
            Entitler.Tag = Definitor;

            Result.Header = Entitler;
            Result.MinWidth = Definitor.GetEstimatedColumnPixelsWidth();

            return Result;
        }
    }
}
