// -------------------------------------------------------------------------------------------
// Instrumind ThinkComposer
//
// Copyright (C) Néstor Marcel Sánchez Ahumada. Santiago, Chile.
// http://thinkcomposer.codeplex.com
//
// This file is part of ThinkComposer, which is free software licensed under the GNU General Public License.
// It is provided without any warranty. You should find a copy of the license in the root directory of this software product.
// -------------------------------------------------------------------------------------------
//
// Project: Instrumind ThinkComposer v1.0
// File   : EntityPropertyExpositor.cs
// Object : Instrumind.Common.Visualization.Widgets.RecordFieldExpositor (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2011.01.28 Néstor Sánchez A.  Creation
//

using System;
using System.Collections;
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
using System.Windows.Shapes;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;
using Instrumind.Common.Visualization.Widgets;

using Instrumind.ThinkComposer.MetaModel;
using Instrumind.ThinkComposer.MetaModel.InformationMetaModel;
using Instrumind.ThinkComposer.Model.InformationModel;
using Instrumind.ThinkComposer.Model.GraphModel;
using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;

namespace Instrumind.ThinkComposer.Composer.ComposerUI.Widgets
{
    /// <summary>
    /// Exposes a Field of a Table-Record, showing its label and value, either just for display or also for editing.
    /// Automatically changes its editing features, based on the target field metadata.
    /// </summary>
    // POSTPONED: Refactor shared/duplicate code with EntityPropertyExpositor in a common base class.
    public partial class RecordFieldExpositor : UserControl
    {
        public const double FONT_SIZE = 10.0;

        public static readonly DependencyProperty ExposedFieldProperty;
        public static readonly DependencyProperty OrientationProperty;
        public static readonly DependencyProperty LabelMinWidthProperty;
        public static readonly DependencyProperty LabelMinHeightProperty;
        public static readonly DependencyProperty TabOrderProperty;

        static RecordFieldExpositor()
        {
            RecordFieldExpositor.ExposedFieldProperty = DependencyProperty.Register("ExposedField", typeof(string), typeof(RecordFieldExpositor),
                new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnExposedFieldChanged)));

            RecordFieldExpositor.OrientationProperty = DependencyProperty.Register("Orientation", typeof(Orientation), typeof(RecordFieldExpositor),
                new FrameworkPropertyMetadata(Orientation.Horizontal, new PropertyChangedCallback(OnOrientationChanged)));

            RecordFieldExpositor.LabelMinWidthProperty = DependencyProperty.Register("LabelMinWidth", typeof(double), typeof(RecordFieldExpositor),
                new FrameworkPropertyMetadata(double.NaN, new PropertyChangedCallback(OnLabelMinWidthChanged)));

            RecordFieldExpositor.LabelMinHeightProperty = DependencyProperty.Register("LabelMinHeight", typeof(double), typeof(RecordFieldExpositor),
                new FrameworkPropertyMetadata(double.NaN, new PropertyChangedCallback(OnLabelMinHeightChanged)));

            RecordFieldExpositor.TabOrderProperty = DependencyProperty.Register("TabOrder", typeof(int), typeof(RecordFieldExpositor),
                new FrameworkPropertyMetadata(int.MaxValue, new PropertyChangedCallback(OnTabOrderChanged)));
        }

        public RecordFieldExpositor()
        {
            InitializeComponent();

            // Initializes with the most commonly used Control.
            SetValueEditorControl(new ExtendedEditText(),
                                     ExtendedEditText.ValueProperty,
                                     (exp) => (exp.ValueEditor as ExtendedEditText).Value,
                                     (exp, val) => (exp.ValueEditor as ExtendedEditText).Value = val as string,
                                     (exp) =>
                                         {
                                             var Ctl = exp.ValueEditor as ExtendedEditText;
                                             Ctl.FontSize = FONT_SIZE;

                                             if (exp.SourceFieldDefinitor == null)
                                             {
                                                 Ctl.IsEnabled = false;
                                                 return;
                                             }

                                             // This must be set for explicit change. Databinding didn't work.
                                             Ctl.EditingAction = ((value) => exp.InstanceSource.SetStoredValue(exp.SourceFieldDefinitor, value));
                                             
                                             Ctl.Editor.TextWrapping = TextWrapping.WrapWithOverflow;

                                             if (exp.SourceFieldDefinitor.FieldType is BasicDataType)
                                             {
                                                 var BasicKind = (BasicDataType)exp.SourceFieldDefinitor.FieldType;

                                                 if (BasicKind.DisplayMinLength > TextType.MAX_SINGLELINE_TEXT_LENGTH
                                                     || (BasicKind is TextType && ((TextType)BasicKind).SizeLimit > BasicDataType.MAX_SINGLELINE_TEXT_LENGTH))
                                                 {
                                                     Ctl.Editor.AcceptsReturn = true;
                                                     Ctl.MinHeight = 18 * 3;
                                                     Ctl.MaxHeight = 18 * 6;
                                                     Ctl.Editor.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                                                     Ctl.Editor.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                                                 }

                                                 var TextKind = BasicKind as TextType;
                                                 if (TextKind != null)
                                                 {
                                                     Ctl.MaxLength = TextKind.SizeLimit;

                                                     if (exp.SourceFieldDefinitor.ValuesSource != null
                                                         && exp.SourceFieldDefinitor.ValuesSource is Table)
                                                     {
                                                         Ctl.ValuesSource = exp.SourceFieldDefinitor.ValuesSource;
                                                         Ctl.ValuesSourceMemberPath = "Label";
                                                     }

                                                     if (exp.SourceFieldDefinitor.IdeaReferencingProperty != null
                                                         && exp.SourceFieldDefinitor.IdeaReferencingProperty != Domain.Unassigned_IdeaReferencingPropertyProperty
                                                         && exp.InstanceSource != null && exp.InstanceSource.OwnerTable != null
                                                         && exp.InstanceSource.OwnerTable.OwnerIdea.OwnerComposition != null)
                                                     {
                                                         Ctl.CompositesSource = exp.InstanceSource.OwnerTable.OwnerIdea.OwnerComposition.CompositeMembers;
                                                         Ctl.CompositesSourceMemberPath = exp.SourceFieldDefinitor.IdeaReferencingProperty.TechName;
                                                     }
                                                 }
                                             }
                                         },
                                     (exp) =>
                                         {
                                             var ctl = exp.ValueEditor as ExtendedEditText;
                                             if (this.SourceFieldDefinitor == null)
                                                 return;

                                             // Select if appropriate (this doesn't work on GotFocus event the first time)
                                             if (ctl.IsFocused &&
                                                 !(this.SourceFieldDefinitor.FieldType is BasicDataType
                                                   && ((BasicDataType)this.SourceFieldDefinitor.FieldType).DisplayMinLength > TextType.MAX_SINGLELINE_TEXT_LENGTH))
                                                 ctl.Editor.SelectAll();
                                         });

            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                this.LabelExpositor.Text = "";
                SetEditedValue("");
            }

            this.ValueEditor.IsEnabled = false; // Enable only when pointing to a valid instance source

            this.Loaded += RecordFieldExpositor_Loaded;
            this.Unloaded += RecordFieldExpositor_Unloaded;
        }

        public RecordFieldExpositor(FieldDefinition Definitor)
             : this()
        {
            this.SourceFieldDefinitor = Definitor;
            this.ExposedField = Definitor.TechName;
        }

        void RecordFieldExpositor_Loaded(object sender, RoutedEventArgs e)
        {
            if (!this.IsVisible)
                return;

            this.IsExplicitlyDisabled = !this.IsEnabled;

            Expose();
        }

        void RecordFieldExpositor_Unloaded(object sender, RoutedEventArgs e)
        {
            if (this.ValueEditor != null)
                this.ValueEditor.LostFocus -= ValueEditor_LostFocus;

            Suppress();
        }

        private bool IsExplicitlyDisabled = false;

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public FrameworkElement ValueEditor { get; protected set; }
        public DependencyProperty ValueBindingProperty { get; protected set; }
        public Func<RecordFieldExpositor, object> ValueEditorGetter { get; protected set; }
        public Action<RecordFieldExpositor, object> ValueEditorSetter { get; protected set; }
        public Action<RecordFieldExpositor> ValueEditorInitializer { get; protected set; }
        public Action<RecordFieldExpositor> ValueEditorStart { get; protected set; }

        public void SetValueEditorControl(Control ValueEditor,
                                          DependencyProperty BindingProperty,
                                          Func<RecordFieldExpositor, object> Getter,
                                          Action<RecordFieldExpositor, object> Setter,
                                          Action<RecordFieldExpositor> Initializer,
                                          Action<RecordFieldExpositor> EditStart)
        {
            this.ValueEditor = ValueEditor;
            this.ValueBindingProperty = BindingProperty;

            this.ValueEditorGetter = Getter;
            this.ValueEditorSetter = Setter;
            this.ValueEditorInitializer = Initializer;
            this.ValueEditorStart = EditStart;

            this.ValueEditorBorder.Child = this.ValueEditor;
        }

        public object GetEditedValue()
        {
            if (ValueEditorGetter == null)
                return null;

            return ValueEditorGetter(this);
        }

        public void SetEditedValue(object Value)
        {
            if (ValueEditorSetter == null)
                return;

            ValueEditorSetter(this, Value);
        }

        public void InitializeEditor()
        {
            if (ValueEditorInitializer == null)
                return;

            ValueEditorInitializer(this);
        }

        public void StartEdit()
        {
            if (ValueEditorStart == null)
                return;

            ValueEditorStart(this);
        }

        private object PreviousEditedValue = null;

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public new void Focus()
        {
            this.ValueEditor.Focus();
        }

        public string ExposedField
        {
            get { return (string)GetValue(RecordFieldExpositor.ExposedFieldProperty); }
            set { SetValue(RecordFieldExpositor.ExposedFieldProperty, value); }
        }
        private static void OnExposedFieldChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var Target = (RecordFieldExpositor)depobj;
            var PropName = (string)evargs.NewValue;

            Target.LabelExpositor.Text = PropName;
            Target.Expose(PropName);
        }

        public Orientation Orientation
        {
            get { return (Orientation)GetValue(RecordFieldExpositor.OrientationProperty); }
            set { SetValue(RecordFieldExpositor.OrientationProperty, value); }
        }
        private static void OnOrientationChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var Target = ((RecordFieldExpositor)depobj).ExpoLabelPanel;
            var Direction = (Orientation)evargs.NewValue;

            if (Direction == Orientation.Horizontal)
                DockPanel.SetDock(Target, Dock.Left);
            else
                DockPanel.SetDock(Target, Dock.Top);
        }

        public double LabelMinWidth
        {
            get { return (double)GetValue(RecordFieldExpositor.LabelMinWidthProperty); }
            set { SetValue(RecordFieldExpositor.LabelMinWidthProperty, value); }
        }
        private static void OnLabelMinWidthChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var Target = (RecordFieldExpositor)depobj;
            Target.ExpoLabelPanel.MinWidth = (double)evargs.NewValue;
        }

        public double LabelMinHeight
        {
            get { return (double)GetValue(RecordFieldExpositor.LabelMinHeightProperty); }
            set { SetValue(RecordFieldExpositor.LabelMinHeightProperty, value); }
        }
        private static void OnLabelMinHeightChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var Target = (RecordFieldExpositor)depobj;
            Target.ExpoLabelPanel.MinHeight = (double)evargs.NewValue;
        }

        public int TabOrder
        {
            get { return (int)GetValue(RecordFieldExpositor.TabOrderProperty); }
            set { SetValue(RecordFieldExpositor.TabOrderProperty, value); }
        }
        private static void OnTabOrderChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var Target = (RecordFieldExpositor)depobj;
        }

        /// <summary>
        /// Exposes the property indicated with the optionals supplied name into the expositor control.
        /// </summary>
        public void Expose(string ExposedFieldTechName = null)
        {
            if (ExposedFieldTechName == null)
                ExposedFieldTechName = this.ExposedField;

            this.LabelExpositor.Text = this.SourceFieldDefinitor.Name; //- + ":";
            this.LabelExpositor.ToolTip = this.SourceFieldDefinitor.Summary;

            if (ExposedFieldTechName.IsAbsent() || this.InstanceSource == null)
                return;

            // Set the appropriate editor control
            EstablishValueEditorControl();

            this.ValueEditor.LostFocus += ValueEditor_LostFocus;

            // Set the data binding
            this.PropertyBinding = new Binding();

            this.PropertyBinding.Source = this.InstanceSource;
            this.PropertyBinding.Path = new PropertyPath(ExposedFieldTechName);

            if (!this.IsExplicitlyDisabled)
                this.IsEnabled = (this.PropertyBinding.Source != null);

            this.ValueEditor.IsEnabled = this.IsEnabled;

            this.ValueEditor.SetBinding(this.ValueBindingProperty, this.PropertyBinding);

            this.ValueEditor.ToolTip = this.SourceFieldDefinitor.Summary;

            this.InitializeEditor();
            this.StartEdit();
        }

        void ValueEditor_LostFocus(object sender, RoutedEventArgs e)
        {
            var CurrentEditedValue = this.GetEditedValue();

            if (!this.CanProcessLostFocus || CurrentEditedValue == this.PreviousEditedValue)
                return;

            this.PreviousEditedValue = CurrentEditedValue;
        }

        public bool CanProcessLostFocus = true;

        /// <summary>
        /// Disconnects for editing the previously exposed property.
        /// </summary>
        public void Suppress()
        {
            // PENDING: Capture the in-editing value before unbind (otherwise it is lost between tab-item changes)
            // This unbind is to prevent memory leaks.
            //? BindingOperations.ClearBinding(this.ValueEditor, this.ValueBindingProperty);
        }

        /// <summary>
        /// References the direct definitor of the source property.
        /// </summary>
        protected FieldDefinition SourceFieldDefinitor { get; set; }

        /// <summary>
        /// References the source table-record instance having the source field.
        /// </summary>
        public TableRecord InstanceSource { get; set; }

        /// <summary>
        /// Binding object for the property being exposed.
        /// </summary>
        protected Binding PropertyBinding = null;

        /// <summary>
        /// Establishes the value expositor based on the Exposed Property type and the supplied Property Controller.
        /// </summary>
        // IMPORTANT: *** ALWAYS MAINTAIN THIS UPDATED IN CORRELATION (AS POSSIBLE) WITH EntityPropertyExpositor.EstablishValueEditorControl() ***
        public void EstablishValueEditorControl()
        {
            if (this.SourceFieldDefinitor.FieldType.IsEqual(DataType.DataTypeTable))
            {
                SetValueEditorAsTableEditorLauncher();
                return;
            }

            if (this.SourceFieldDefinitor.FieldType.IsEqual(DataType.DataTypePicture))
            {
                SetValueEditorAsPictureSelectorLauncher();
                return;
            }

            if (this.SourceFieldDefinitor.ValuesSource != null
                && this.SourceFieldDefinitor.FieldType.IsEqual(DataType.DataTypeTableRecordRef))
            {
                SetValueEditorAsComboBox(this.SourceFieldDefinitor.ValuesSource, null, "Label");
                return;
            }

            if (this.SourceFieldDefinitor.FieldType.IsEqual(DataType.DataTypeIdeaRef))
            {
                SetValueEditorAsTreeItemSelector();
                return;
            }

            // Must be before SetValueEditorAsComboBox, which is for choice-type (a supertype of the switch-type)
            if (this.SourceFieldDefinitor.FieldType.IsEqual(DataType.DataTypeSwitch)
                || this.SourceFieldDefinitor.FieldType.ContainerType == typeof(bool))
            {
                SetValueEditorAsCheckBox();
                return;
            }

            // Must be after SetValueEditorAsCheckBox, which is for booleans (implemented as choice-type) 
            if (this.SourceFieldDefinitor.FieldType is ChoiceType)
            {
                SetValueEditorAsComboBox(((ChoiceType)this.SourceFieldDefinitor.FieldType).GetRegisteredOptions(),
                                         "Item1", "Item2");
                return;
            }

            if (this.SourceFieldDefinitor.FieldType.ContainerType
                    .IsOneOf(typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong),
                             typeof(float), typeof(double), typeof(decimal)))
            {
                SetValueEditorAsMaskEditNumber(this.SourceFieldDefinitor.FieldType.ContainerType);
                return;
            }

            if (this.SourceFieldDefinitor.FieldType.ContainerType == typeof(DateTime))
            {
                SetValueEditorAsMaskEditDateTime();
                return;
            }

            var ValueEditorControl = this.ValueEditor as Control;
            if (ValueEditorControl == null)
                return;

            ValueEditorControl.FontSize = FONT_SIZE;
        }

        private void ValueEditor_GotFocus(object sender, RoutedEventArgs e)
        {
            if (this.SourceFieldDefinitor == null)
                return;

            StartEdit();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public void SetValueEditorAsComboBox(IEnumerable ExternalItemsSource, string ExternalItemsSelectedValuePath, string ExternalItemsDisplayMemberPath)
        {
            var Selector = new ComboBox();
            Selector.HorizontalContentAlignment = HorizontalAlignment.Stretch;

            var FirstItem = ExternalItemsSource.Cast<object>().First();

            if (typeof(FormalPresentationElement).IsAssignableFrom(this.SourceFieldDefinitor.FieldType.ContainerType)
                || (FirstItem == null || typeof(FormalPresentationElement).IsAssignableFrom(FirstItem.GetType())))
                Selector.ItemTemplate = Display.GetResource<DataTemplate>("TplFormalPresentationElement");
            else
                if (typeof(SimplePresentationElement).IsAssignableFrom(this.SourceFieldDefinitor.FieldType.ContainerType)
                    || (FirstItem == null || typeof(SimplePresentationElement).IsAssignableFrom(FirstItem.GetType())))
                    Selector.ItemTemplate = Display.GetResource<DataTemplate>("TplSimplePresentationElement");

            SetValueEditorControl(Selector, ComboBox.SelectedItemProperty,
                                     (exp) => (exp.ValueEditor as ComboBox).SelectedItem,
                                     (exp, val) => (exp.ValueEditor as ComboBox).SelectedItem = val,
                                     (exp) =>
                                     {
                                         var Ctl = exp.ValueEditor as ComboBox;

                                         if (!ExternalItemsSelectedValuePath.IsAbsent())
                                            Ctl.SelectedValuePath = ExternalItemsSelectedValuePath;

                                         // Note: DisplayMemberPath cannot be setted when an ItemTemplate is also setted.
                                         if (!ExternalItemsDisplayMemberPath.IsAbsent()
                                             && Ctl.ItemTemplate == null)
                                            Ctl.DisplayMemberPath = ExternalItemsDisplayMemberPath;

                                         Ctl.ItemsSource = ExternalItemsSource;
                                         var SelectionValue = exp.GetEditedValue();

                                         if (Ctl.SelectedValuePath.IsAbsent())
                                             Ctl.SelectedItem = SelectionValue;
                                         else
                                         {
                                             var SelectionIndex = ExternalItemsSource.GetMatchingIndex(SelectionValue, ExternalItemsSelectedValuePath);
                                             if (SelectionIndex >= 0)
                                                 Ctl.SelectedIndex = SelectionIndex;
                                         }
                                     },
                                     null);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public void SetValueEditorAsTreeItemSelector()
        {
            var Selector = new TreeItemSelector();

            SetValueEditorControl(Selector, TreeItemSelector.ValueProperty,
                                     (exp) => (exp.ValueEditor as TreeItemSelector).Value,
                                     (exp, val) => (exp.ValueEditor as TreeItemSelector).Value = (IRecognizableComposite)val,
                                     (exp) =>
                                     {
                                        var Ctl = exp.ValueEditor as TreeItemSelector;
                                        Ctl.FontSize = FONT_SIZE;

                                        if (exp.SourceFieldDefinitor == null)
                                        {
                                            Ctl.IsEnabled = false;
                                            return;
                                        }

                                        // This must be set for explicit change. Databinding didn't work.
                                        Ctl.EditingAction = ((value) => exp.InstanceSource.SetStoredValue(exp.SourceFieldDefinitor, value));

                                        if (exp.InstanceSource != null && exp.InstanceSource.OwnerTable != null
                                            && exp.InstanceSource.OwnerTable.OwnerIdea.OwnerComposition != null)
                                            Ctl.CompositesSource = exp.InstanceSource.OwnerTable.OwnerIdea.OwnerComposition.CompositeMembers;
                                     },
                                     null);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public void SetValueEditorAsTableEditorLauncher()
        {
            var Launcher = new PaletteButton();
            Launcher.ButtonText = "...";
            Launcher.ButtonShowImage = false;   // Launcher.ButtonImage = Display.GetAppImage("table_edit.png");
            Launcher.ToolTip = "Edit data values";
            Launcher.Cursor = Cursors.Hand;

            SetValueEditorControl(Launcher, Button.TagProperty,
                                     (exp) => (exp.ValueEditor as PaletteButton).Tag,
                                     (exp, val) => (exp.ValueEditor as PaletteButton).Tag = (Table)val,
                                     (exp) =>
                                     {
                                         var Ctl = exp.ValueEditor as PaletteButton;
                                         Ctl.FontSize = FONT_SIZE;

                                         if (exp.SourceFieldDefinitor == null)
                                         {
                                             Ctl.IsEnabled = false;
                                             return;
                                         }

                                         var StoredTable = exp.InstanceSource.GetStoredValue(exp.SourceFieldDefinitor) as Table;
                                         if (StoredTable != null)
                                             Ctl.ButtonText = StoredTable.RecordsLabel;

                                         // This must be set for explicit change. Databinding didn't work.
                                         Ctl.Click += ((sender, args) =>
                                                            {
                                                                exp.InstanceSource.OwnerTable.OwnerIdea.EditEngine.StartCommandVariation("Edit Field contained Table");

                                                                if (StoredTable == null)
                                                                {
                                                                    StoredTable = new Table(exp.InstanceSource.OwnerTable.OwnerIdea,
                                                                                            exp.SourceFieldDefinitor.ContainedTableDesignator.Assign<DetailDesignator>(false));

                                                                    // This must be set for explicit change. Databinding didn't work.
                                                                    exp.InstanceSource.SetStoredValue(exp.SourceFieldDefinitor, StoredTable);
                                                                    Ctl.ButtonText = StoredTable.RecordsLabel;
                                                                }

                                                                var Changed = DetailTableEditor.Edit(StoredTable, exp.SourceFieldDefinitor.ContainedTableDesignator);
                                                                exp.InstanceSource.OwnerTable.OwnerIdea.EditEngine.CompleteCommandVariation();

                                                                if (Changed)
                                                                    Ctl.ButtonText = StoredTable.RecordsLabel;
                                                                else
                                                                    exp.InstanceSource.OwnerTable.OwnerIdea.EditEngine.Undo();
                                                            });
                                     },
                                     null);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public void SetValueEditorAsPictureSelectorLauncher()
        {
            //! PENDING: Show image

            var Launcher = new ImagePickerSimple();
            Launcher.ToolTip = "Select picture";
            Launcher.Cursor = Cursors.Hand;

            SetValueEditorControl(Launcher, ImagePickerSimple.SelectedImageProperty,
                                     (exp) => (exp.ValueEditor as ImagePickerSimple).SelectedImage,
                                     (exp, val) => (exp.ValueEditor as ImagePickerSimple).SelectedImage = (ImageSource)val,
                                     (exp) =>
                                     {
                                         var Ctl = exp.ValueEditor as ImagePickerSimple;
                                         var Picture = exp.InstanceSource.GetStoredValue(exp.SourceFieldDefinitor) as ImageAssignment;
                                         if (Picture != null)
                                             Ctl.SelectedImage = Picture.Image;

                                         Ctl.ImagePickerActionFieldName = exp.SourceFieldDefinitor.TechName;
                                         Ctl.ImagePickerActionFieldSource = exp.InstanceSource;
                                         Ctl.ImagePickerSelectAction =
                                                ((fname, fsource, ctl) =>
                                                {
                                                    //T Display.DialogMessage("Field: [" + fname.NullDefault("?") + "]", "Value: [" + fvalue.NullDefault("") + "]");
                                                    var SourceRecord = fsource as TableRecord;
                                                    if (SourceRecord == null)
                                                        return;

                                                    var Selection = Display.DialogGetImageFromFile();
                                                    if (Selection != null)
                                                    {
                                                        exp.SourceFieldDefinitor.OwnerTableDef.EditEngine.StartCommandVariation("Edit Field contained Picture");

                                                        // This must be set for explicit change. Databinding didn't work.

                                                        // IMPORTANT: Always store a whole new StoredPicture (ImageAssignment),
                                                        // and don't just change the StoredPicture.Image property, in order to support undo/redo.
                                                        var StoredPicture = Selection.AssignImage();
                                                        SourceRecord.SetStoredValue(exp.SourceFieldDefinitor, StoredPicture);

                                                        var ImgCtl = ctl as ImagePickerSimple;
                                                        if (ImgCtl != null)
                                                            ImgCtl.SelectedImage = Selection;

                                                        exp.SourceFieldDefinitor.OwnerTableDef.EditEngine.CompleteCommandVariation();
                                                    }
                                                });
                                         
                                         /*? var Ctl = exp.ValueEditor as ImagePickerSimple;

                                         var StoredPicture = Ctl.SelectedImage;

                                         // This must be set for explicit change. Databinding didn't work.
                                         Ctl.MouseLeftButtonDown += ((sender, args) =>
                                         {
                                             var Selection = Display.DialogGetImageFromFile();
                                             if (Selection != null)
                                             {
                                                 exp.InstanceSource.OwnerTable.OwnerIdea.EditEngine.StartCommandVariation("Edit Field contained Picture");

                                                 if (StoredPicture == null)
                                                 {
                                                     StoredPicture = new ImageAssignment();
                                                     Ctl.Tag = StoredPicture;

                                                     // This must be set for explicit change. Databinding didn't work.
                                                     exp.InstanceSource.SetStoredValue(exp.SourceFieldDefinitor, StoredPicture);
                                                 }

                                                 StoredPicture.Image = Selection;
                                                 Ctl.Source = Selection;

                                                 exp.InstanceSource.OwnerTable.OwnerIdea.EditEngine.CompleteCommandVariation();
                                             }
                                         }); */
                                     },
                                     null);
        }

        public void SetValueEditorAsMaskEditNumber(Type NumericDataType)
        {
            var Editor = new MaskEditNumber();
            var Limits = NumericDataType.SelectCorresponding(General.NumericTypesLimits);

            Editor.MinLimit = Limits.Item1;
            Editor.MaxLimit = Limits.Item2;
            Editor.IntegerDigits = Limits.Item3;
            Editor.DecimalDigits = Limits.Item4;

            SetValueEditorControl(Editor, MaskEditNumber.ValueProperty,
                                     (exp) => (exp.ValueEditor as MaskEditNumber).Value,
                                     (exp, val) => (exp.ValueEditor as MaskEditNumber).Value = Convert.ToDecimal(val),
                                     (exp) =>
                                     {
                                         var Ctl = exp.ValueEditor as MaskEditNumber;
                                         Ctl.Value = (decimal)Convert.ChangeType(exp.InstanceSource.GetStoredValue(exp.SourceFieldDefinitor), typeof(decimal));

                                         // This must be set for explicit change. Databinding didn't work.
                                         Ctl.EditingAction =
                                             ((value) => exp.InstanceSource.SetStoredValue(exp.SourceFieldDefinitor,
                                                                                           Convert.ChangeType(value, exp.SourceFieldDefinitor.FieldType.ContainerType)));

                                         if (exp.SourceFieldDefinitor.ValuesSource != null && exp.SourceFieldDefinitor.ValuesSource is Table)
                                         {
                                             Ctl.ValuesSource = exp.SourceFieldDefinitor.ValuesSource;
                                             Ctl.ValuesSourceMemberPath = "Label";
                                             Ctl.ValuesSourceNumericConverter = TableRecord.TableRecordToNumericConverter;
                                         }
                                     },
                                     null);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public void SetValueEditorAsCheckBox()
        {
            var Editor = new CheckBox();

            SetValueEditorControl(Editor, CheckBox.IsCheckedProperty,
                                     (exp) => (exp.ValueEditor as CheckBox).IsChecked.IsTrue(),
                                     (exp, val) => (exp.ValueEditor as CheckBox).IsChecked = (bool)val,
                                     (exp) =>
                                     {
                                         var Ctl = exp.ValueEditor as CheckBox;
                                         Ctl.IsChecked = (bool?)exp.InstanceSource.GetStoredValue(exp.SourceFieldDefinitor);
                                     },
                                     null);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public void SetValueEditorAsMaskEditDateTime()
        {
            var Editor = new MaskEditDateTime();

            SetValueEditorControl(Editor, MaskEditDateTime.ValueProperty,
                                     (exp) => (exp.ValueEditor as MaskEditDateTime).Value,
                                     (exp, val) => (exp.ValueEditor as MaskEditDateTime).Value = (DateTime)val,
                                     (exp) =>
                                     {
                                         var Ctl = exp.ValueEditor as MaskEditDateTime;
                                         Ctl.Value = (DateTime)exp.InstanceSource.GetStoredValue(exp.SourceFieldDefinitor);

                                         // This must be set for explicit change. Databinding didn't work.
                                         Ctl.EditingAction =
                                             ((value) => exp.InstanceSource.SetStoredValue(exp.SourceFieldDefinitor, value));
                                     },
                                     null);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public override string ToString()
        {
            return "RecFld-Expositor[" + this.ExposedField.ToStringAlways() + "]=" + this.GetEditedValue().ToStringAlways();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}
