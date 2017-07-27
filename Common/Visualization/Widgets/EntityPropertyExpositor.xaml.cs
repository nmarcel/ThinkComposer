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
// Project: Instrumind ThinkComposer v1.07
// File   : EntityPropertyExpositor.cs
// Object : Instrumind.Common.Visualization.Widgets.EntityPropertyExpositor (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.09.15 Néstor Sánchez A.  Creation
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
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
    /// Exposes a property of an Entity, showing its label and value, either just for display or also for editing.
    /// Automatically changes its editing features, based on the target property metadata.
    /// </summary>
    // POSTPONED: Refactor shared/duplicate code with RecordFieldExpositor in a common base class.
    public partial class EntityPropertyExpositor : UserControl
    {
        public const double FONT_SIZE = 10.0;

        public static readonly DependencyProperty ExposedPropertyProperty;
        public static readonly DependencyProperty OrientationProperty;
        public static readonly DependencyProperty LabelMinWidthProperty;
        public static readonly DependencyProperty LabelMinHeightProperty;
        public static readonly DependencyProperty ShowLabelProperty;
        public static readonly DependencyProperty TabOrderProperty;

        static EntityPropertyExpositor()
        {
            EntityPropertyExpositor.ExposedPropertyProperty = DependencyProperty.Register("ExposedProperty", typeof(string), typeof(EntityPropertyExpositor),
                new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnExposedPropertyChanged)));

            EntityPropertyExpositor.OrientationProperty = DependencyProperty.Register("Orientation", typeof(Orientation), typeof(EntityPropertyExpositor),
                new FrameworkPropertyMetadata(Orientation.Horizontal, new PropertyChangedCallback(OnOrientationChanged)));

            EntityPropertyExpositor.LabelMinWidthProperty = DependencyProperty.Register("LabelMinWidth", typeof(double), typeof(EntityPropertyExpositor),
                new FrameworkPropertyMetadata(double.NaN, new PropertyChangedCallback(OnLabelMinWidthChanged)));

            EntityPropertyExpositor.LabelMinHeightProperty = DependencyProperty.Register("LabelMinHeight", typeof(double), typeof(EntityPropertyExpositor),
                new FrameworkPropertyMetadata(double.NaN, new PropertyChangedCallback(OnLabelMinHeightChanged)));

            EntityPropertyExpositor.ShowLabelProperty = DependencyProperty.Register("ShowLabel", typeof(bool), typeof(EntityPropertyExpositor),
                new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnShowLabelChanged)));

            EntityPropertyExpositor.TabOrderProperty = DependencyProperty.Register("TabOrder", typeof(int), typeof(EntityPropertyExpositor),
                new FrameworkPropertyMetadata(int.MaxValue, new PropertyChangedCallback(OnTabOrderChanged)));
        }

        public EntityPropertyExpositor()
        {
            InitializeComponent();

            // Initializes with the most commonly used Control.
            SetValueEditorControl(new TextBox(),
                                     TextBox.TextProperty,
                                     (exp) => (exp.ValueEditor as TextBox).Text,
                                     (exp, val) => (exp.ValueEditor as TextBox).Text = val as string,
                                     (exp) =>
                                         {
                                             var ctl = exp.ValueEditor as TextBox;
                                             ctl.FontSize = FONT_SIZE;
                                             var PropertyDef = this.SourceMemberDefinitor as MModelPropertyDefinitor;
                                             if (PropertyDef == null)
                                             {
                                                 ctl.IsEnabled = false;
                                                 return;
                                             }

                                             ctl.TextWrapping = TextWrapping.WrapWithOverflow;

                                             if (PropertyDef.Kind == EPropertyKind.Description)
                                             {
                                                 ctl.AcceptsReturn = true;
                                                 ctl.MinHeight = 60.0;
                                             }

                                             /* ctl.TextChanged +=
                                                 ((sender, evargs) =>
                                                    {
                                                        Console.WriteLine(((TextBox)sender).Text);
                                                    }); */
                                         },
                                     (exp) =>
                                         {
                                             var ctl = exp.ValueEditor as TextBox;
                                             var PropertyDef = this.SourceMemberDefinitor as MModelPropertyDefinitor;
                                             if (PropertyDef == null)
                                                 return;
                                             // Select if appropriate (this doesn't work on GotFocus event the first time)
                                             if (ctl.IsFocused && PropertyDef.Kind != EPropertyKind.Description)
                                                 ctl.SelectAll();
                                         });

            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                this.LabelExpositor.Text = "";
                SetEditedValue("");
            }

            this.Loaded += EntityPropertyExpositor_Loaded;
            this.Unloaded += EntityPropertyExpositor_Unloaded;
        }

        public EntityPropertyExpositor(string ExposedPropertyTechName)
             : this()
        {
            this.ExposedProperty = ExposedPropertyTechName;
        }

        void EntityPropertyExpositor_Loaded(object sender, RoutedEventArgs e)
        {
            if (LoadProcessed)
            {
                if (this.SourceParentView != null)
                {
                    this.ShowAdvancedMembers = this.SourceParentView.ShowAdvancedMembers;
                    if (this.MemberController != null && this.MemberController.Definition.IsAdvanced)
                    {
                        this.SourceParentView.ShowAdvancedMembersChanged -= OnParentViewShowAdvancedMembersChanged; // Forces only one subscription
                        this.SourceParentView.ShowAdvancedMembersChanged += OnParentViewShowAdvancedMembersChanged;
                    }
                }
                return;
            }

            this.IsExplicitlyDisabled = !this.IsEnabled;

            this.SourceParentView = this.GetNearestDominantOfType<IEntityView>();
            //-? this.ParentViewChild = this.GetNearestDominantOfType<IEntityViewChild>();   // Extension: This could have more levels

            if (this.SourceParentView != null)
            {
                this.ExpositionAction = this.ExpositionAction.NullDefault(() => Expose());
                this.SourceParentView.EntityExposedForView += this.ExpositionAction;
                Expose();

                this.ShowAdvancedMembers = this.SourceParentView.ShowAdvancedMembers;
                if (this.MemberController != null && this.MemberController.Definition.IsAdvanced)
                    this.SourceParentView.ShowAdvancedMembersChanged += OnParentViewShowAdvancedMembersChanged;

                LoadProcessed = true;
            }
        }
        private bool LoadProcessed = false;

        void EntityPropertyExpositor_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.SourceParentView != null)
                {
                    this.SourceParentView.EntityExposedForView -= this.ExpositionAction;

                    if (this.MemberController != null && this.MemberController.Definition.IsAdvanced)
                        this.SourceParentView.ShowAdvancedMembersChanged -= OnParentViewShowAdvancedMembersChanged;
                }

                if (this.ValueEditor != null)
                {
                    this.ValueEditor.LostFocus -= ValueEditor_LostFocus;

                    if (this.MemberController != null)
                        this.MemberController.InstanceController.ControlledInstance.PropertyChanged -= WorkingEntityObject_PropertyChanged;
                }
            }
            catch (Exception Problem)
            {
                // this happens when the Loaded event was never fired, hence no event handler was attached nor parent-window was populated.
            }

            Suppress();
        }

        private void OnParentViewShowAdvancedMembersChanged(bool show)
        {
            this.ShowAdvancedMembers = show;
        }

        private bool IsExplicitlyDisabled = false;

        private Action ExpositionAction;

        public bool ShowAdvancedMembers
        {
            get { return this.ShowAdvancedMembers_; }
            protected set
            {
                if (this.ShowAdvancedMembers_ == value)
                    return;

                if (this.MemberController != null && this.MemberController.Definition.IsAdvanced)
                    this.SetVisible(value);

                this.ShowAdvancedMembers_ = value;
            }
        }
        protected bool ShowAdvancedMembers_ = false;

        /// <summary>
        /// For when editing a collection: Extra data items (values) associated to the regular items (keys) mantained by this Expositor.
        /// </summary>
        public IDictionary<object, object> ExtraDataItems { get; set; }

        /// <summary>
        /// For when editing a collection: Available values to be selected as any of the Extra data items.
        /// </summary>
        public IEnumerable ExtraDataAvailableValues { get; set; }

        /// <summary>
        /// For when editing a collection: Operation for updating a modified Extra data item.
        /// Its arguments are: Standard item being edited and the extra-data object updated.
        /// </summary>
        public Action<object, object> ExtraDataUpdater { get; set; }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public Control ValueEditor { get; protected set; }
        public DependencyProperty ValueBindingProperty { get; protected set; }
        public Func<EntityPropertyExpositor, object> ValueEditorGetter { get; protected set; }
        public Action<EntityPropertyExpositor, object> ValueEditorSetter { get; protected set; }
        public Action<EntityPropertyExpositor> ValueEditorInitializer { get; protected set; }
        public Action<EntityPropertyExpositor> ValueEditorStart { get; protected set; }

        public void SetValueEditorControl(Control ValueEditor,
                                          DependencyProperty BindingProperty,
                                          Func<EntityPropertyExpositor, object> Getter,
                                          Action<EntityPropertyExpositor, object> Setter,
                                          Action<EntityPropertyExpositor> Initializer,
                                          Action<EntityPropertyExpositor> EditStart)
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
            /*T if (this.SourceInstance == null)
                    return; */

            object Value = this.SourceMemberDefinitor.Read(this.SourceInstance);

            if (this.PropertyBinding != null && this.PropertyBinding.Converter != null)
                Value = this.PropertyBinding.Converter.Convert(Value, this.SourceMemberDefinitor.DataType,
                                                               null, CultureInfo.CurrentCulture);

            /*T if (this.SourceMemberDefinitor.TechName == "Name" && Value.ToStringAlways() == "")
                Console.WriteLine("Warning: Name is EmptyString."); */

            this.PreviousEditedValue = Value;

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

        public IEntityView SourceParentView { get; protected set; }

        public MMemberController MemberController { get; protected set; }

        /// <summary>
        /// When not null, contains the text to be shown as label.
        /// Else the source memeber-definitor name is used.
        /// </summary>
        public string LabelText
        {
            get { return this.LabelText_; }
            set
            {
                if (this.LabelText == value)
                    return;

                this.LabelText_ = value;

                if (value == null && this.SourceMemberDefinitor != null)
                    this.LabelExpositor.Text = this.SourceMemberDefinitor.Name;
            }
        }
        private string LabelText_ = null;

        public string ExposedProperty
        {
            get { return (string)GetValue(EntityPropertyExpositor.ExposedPropertyProperty); }
            set { SetValue(EntityPropertyExpositor.ExposedPropertyProperty, value); }
        }
        private static void OnExposedPropertyChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var Target = (EntityPropertyExpositor)depobj;
            var PropName = (string)evargs.NewValue;

            Target.LabelExpositor.Text = PropName;
            Target.Expose(PropName);
        }

        public Orientation Orientation
        {
            get { return (Orientation)GetValue(EntityPropertyExpositor.OrientationProperty); }
            set { SetValue(EntityPropertyExpositor.OrientationProperty, value); }
        }
        private static void OnOrientationChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var Target = (EntityPropertyExpositor)depobj;
            var Direction = (Orientation)evargs.NewValue;

            if (Direction == Orientation.Horizontal)
            {
                Target.LabelExpositor.TextAlignment = TextAlignment.Right;
                DockPanel.SetDock(Target.ExpoLabelPanel, Dock.Left);
            }
            else
            {
                Target.LabelExpositor.TextAlignment = TextAlignment.Left;
                DockPanel.SetDock(Target.ExpoLabelPanel, Dock.Top);
            }
        }

        public double LabelMinWidth
        {
            get { return (double)GetValue(EntityPropertyExpositor.LabelMinWidthProperty); }
            set { SetValue(EntityPropertyExpositor.LabelMinWidthProperty, value); }
        }
        private static void OnLabelMinWidthChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var Target = (EntityPropertyExpositor)depobj;
            Target.ExpoLabelPanel.MinWidth = (double)evargs.NewValue;
        }

        public double LabelMinHeight
        {
            get { return (double)GetValue(EntityPropertyExpositor.LabelMinHeightProperty); }
            set { SetValue(EntityPropertyExpositor.LabelMinHeightProperty, value); }
        }
        private static void OnLabelMinHeightChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var Target = (EntityPropertyExpositor)depobj;
            Target.ExpoLabelPanel.MinHeight = (double)evargs.NewValue;
        }

        public bool ShowLabel
        {
            get { return (bool)GetValue(EntityPropertyExpositor.ShowLabelProperty); }
            set { SetValue(EntityPropertyExpositor.ShowLabelProperty, value); }
        }
        private static void OnShowLabelChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var Target = (EntityPropertyExpositor)depobj;
            Target.LabelExpositor.SetVisible((bool)evargs.NewValue);
        }

        public int TabOrder
        {
            get { return (int)GetValue(EntityPropertyExpositor.TabOrderProperty); }
            set { SetValue(EntityPropertyExpositor.TabOrderProperty, value); }
        }
        private static void OnTabOrderChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var Target = (EntityPropertyExpositor)depobj;
            //? Target.ValueEditor.TabIndex = (int)evargs.NewValue;
        }

        /// <summary>
        /// Exposes the property indicated with the optionals supplied name into the expositor control.
        /// Returns indication of success.
        /// </summary>
        public bool Expose(string ExposedPropertyTechName = null)
        {
            if (ExposedPropertyTechName == null)
                ExposedPropertyTechName = this.ExposedProperty;

            if (ExposedPropertyTechName.IsAbsent() || this.SourceParentView == null)
                return false;

            // If the property is referenced as "dominant.property" then the dominant property is accessed via binding-path
            var BindingParts = ExposedPropertyTechName.Segment(".", true);
            string BindingPropertyName = BindingParts.Last();
            string BindingPropertyDominant = (BindingParts.Length > 1 ? BindingParts[1] : "");

            if (BindingPropertyDominant.IsAbsent())
            {
                var ViewChild = this.GetNearestDominantOfType<IEntityViewChild>();
                if (ViewChild != null)
                {
                    BindingPropertyDominant = ViewChild.ChildPropertyName;
                    ExposedPropertyTechName = BindingPropertyDominant + "." + ExposedPropertyTechName;
                }
            }

            this.MemberController = null;

            if (this.SourceParentView.AssociatedEntity.Controller == null)
                EntityInstanceController.SetInstanceController(this.SourceParentView.AssociatedEntity);

            if (BindingPropertyDominant.IsAbsent())
            {
                this.MemberController = this.SourceParentView.AssociatedEntity.Controller.GetMemberController(BindingPropertyName);
                if (this.MemberController == null)
                {
                    // This happens when a property-expositor created for a particular entity-view is exposed after (via post-call) that entity-view changed its source entity.
                    //T Console.WriteLine("Cannot expose property: " + BindingPropertyName);
                    return false;
                }

                this.PropertyDominantDefinition = null;
                this.SourceMemberDefinitor = this.MemberController.Definition;
            }
            else
            {
                var ChildEntityMemberController = this.SourceParentView.AssociatedEntity.Controller.GetMemberController(BindingPropertyDominant);
                this.MemberController = this.SourceParentView.AssociatedEntity.Controller.GetMemberController(ExposedPropertyTechName);

                if (this.MemberController == null)
                {
                    // This happens when a property-expositor created for a particular entity-view is exposed after (via post-call) that entity-view changed its source entity.
                    //T Console.WriteLine("Cannot expose property-dominant: " + BindingPropertyDominant);
                    return false;
                }

                // MModelClassDefinitor ClassDef = MModelClassDefinitor.GetDefinitor(this.MemberController.Definition.DeclaringType);
                MModelClassDefinitor ClassDef = MModelClassDefinitor.GetDefinitor(ChildEntityMemberController.Definition.DeclaringType);
                this.PropertyDominantDefinition = Tuple.Create<MModelPropertyDefinitor, MModelClassDefinitor>((MModelPropertyDefinitor)ChildEntityMemberController.Definition, ClassDef);
                this.SourceMemberDefinitor = ClassDef.Members.FirstOrDefault(pro => pro.TechName == BindingPropertyName);

                if (this.SourceMemberDefinitor == null)
                    throw new UsageAnomaly("Cannot load member-definitor for '" + ExposedPropertyTechName + "'", this.PropertyDominantDefinition);
            }

            this.MemberController.InstanceController.ControlledInstance.PropertyChanged += WorkingEntityObject_PropertyChanged;

            // Determine display if is Advanced member
            if (this.MemberController.Definition.IsAdvanced)
                this.SetVisible(this.ShowAdvancedMembers);

            /* Removed to consume less resources // Prepare extras
            this.LabelPreExpositor.SetVisible(this.MemberController.IndicatePreExpoLabel);
            this.LabelPreExpositor.ToolTip = this.MemberController.PreExpoLabelTip;
            this.LabelPostExpositor.SetVisible(this.MemberController.IndicatePostExpoLabel);
            this.LabelPostExpositor.ToolTip = this.MemberController.PostExpoLabelTip; */

            this.ValuePreExpositor.SetVisible(this.MemberController.IndicatePreExpoValue);

            /* CANCELLED: Showing data-type related icon should be for user-defined Fields on Forms
            this.ValuePreExpositor.Source = Display.GetLibImage(Display.DataTypeRelatedIcons.GetValueOrDefault(this.MemberController.Definition.DataType)
                                                                .NullDefault(Display.DataTypeRelatedIcons[typeof(object)])); */
            this.ValuePreExpositor.ToolTip = this.MemberController.PreExpoValueTip;

            this.ValuePostExpositorButtons.SetVisible(this.MemberController.IndicatePostExpoValue);
            if (this.MemberController.ComplexOptionsProviders != null && this.MemberController.ComplexOptionsProviders.Length > 0)
            {
                this.ValuePostExpositorButtons.SetOptions(this.MemberController.ComplexOptionsProviders);
                this.ValuePostExpositorButtons.OperationParameterExtractor = (() => this.GetEditedValue());
            }

            this.LabelExpositor.Text = this.LabelText.NullDefault(this.SourceMemberDefinitor.Name);
            this.LabelExpositor.ToolTip = this.MemberController.ExpoLabelTip;

            // Determine source entity and editor
            if (this.PropertyDominantDefinition == null)
                this.SourceInstance = this.SourceParentView.AssociatedEntity;
            else
                this.SourceInstance = this.PropertyDominantDefinition.Item1.Read(this.SourceParentView.AssociatedEntity) as IModelEntity;

            if (this.SourceInstance == null)
            {
                //T Console.WriteLine("Cannot expose property because its source-instance is null.");
                return false;
            }

            // Set the appropriate editor control
            EstablishValueEditorControl();

            this.ValueEditor.LostFocus += ValueEditor_LostFocus;

            // Set the data binding
            this.PropertyBinding = new Binding();
            this.PropertyBinding.Converter = this.MemberController.Definition.BindingValueConverter;

            this.PropertyBinding.Source = this.SourceInstance;
            this.PropertyBinding.Path = new PropertyPath(BindingPropertyName);

            if (!this.MemberController.IsEditableNow || this.MemberController is MCollectionController)
                this.PropertyBinding.Mode = BindingMode.OneWay;

            if (!this.IsExplicitlyDisabled)
                this.IsEnabled = (this.PropertyBinding.Source != null && MemberController.Definition.IsEditControlled);

            this.ValueEditor.IsEnabled = this.IsEnabled;

            this.ValueEditor.SetBinding(this.ValueBindingProperty, this.PropertyBinding);

            this.ValueEditor.ToolTip = this.MemberController.ExpoLabelTip;

            this.InitializeEditor();
            this.StartEdit();

            return true;
        }

        void WorkingEntityObject_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != this.ExposedProperty || this.MemberController == null || this.MemberController.Definition == null)
                return;

            //T TxtTest.Text = this.MemberController.InstanceController.ControlledInstance.GetHashCode().ToString();

            // PREVIOUS: Discarded because worked on binding propagation (which wasn't listened by the ImagePicker)
            // this.MemberController.InstanceController.Retrieve(this.ExposedProperty);

            var Value = this.MemberController.Definition.Read(this.SourceInstance);

            if (this.PropertyBinding != null && this.PropertyBinding.Converter != null)
                Value = this.PropertyBinding.Converter.Convert(Value, this.SourceMemberDefinitor.DataType,
                                                               null, CultureInfo.CurrentCulture);

            this.SetEditedValue(Value);
        }

        public void ValueEditor_LostFocus(object sender, RoutedEventArgs args)
        {
            if (!this.CanProcessLostFocus)
                return;

            if (sender == null && args == null)
                UpdateTargetEntityProperty();
            else
                this.PostCall(expo => UpdateTargetEntityProperty());    // Post-called to let the binding be applied.
        }

        public void UpdateTargetEntityProperty()
        {
            // Notice that a not enabled ValueEditor implies a non updatable property.
            // e.g.: Consider read-only fields such as GUIDs that may produce a cast exception (string to Guid)

            if (this.SourceParentView.AssociatedEntity == null
                || !this.ValueEditor.IsEnabled)
                return;

            var Value = this.GetEditedValue();

            if (Value == this.PreviousEditedValue)
                return;

            // If casteable, then write property
            if ((this.MemberController.Definition.DataType.IsValueType && Value != null)
                || ((Value == null || this.MemberController.Definition.DataType.IsAssignableFrom(Value.GetType()))
                    && this.MemberController.Definition.DataType != typeof(object)))
                this.SourceParentView.ReactToMemberEdited(this.MemberController.Definition, Value);

            this.PreviousEditedValue = Value;
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
        protected MModelMemberDefinitor SourceMemberDefinitor { get; set; }

        /// <summary>
        /// References the source instance having the source property.
        /// </summary>
        protected IModelEntity SourceInstance { get; set; }

        /// <summary>
        /// References the definitor of the (property) dominant of the source property.
        /// </summary>
        protected Tuple<MModelPropertyDefinitor, MModelClassDefinitor> PropertyDominantDefinition = null;

        /// <summary>
        /// Binding object for the property being exposed.
        /// </summary>
        protected Binding PropertyBinding = null;

        /// <summary>
        /// Current items available for selection.
        /// </summary>
        public IEnumerable CurrentAvailableItems { get; protected set; }

        /// <summary>
        /// Refreshes the current items available for selection from the provided source, and returns them.
        /// </summary>
        public IEnumerable RetrieveAvailableItems()
        {
            this.CurrentAvailableItems = this.MemberController.ExternalItemsSourceGetter(this.SourceInstance);

            // This update works for the assignments after the first one.
            var ItemsValueEditor = ValueEditor as ItemsControl;
            if (ItemsValueEditor != null)
                ItemsValueEditor.ItemsSource = this.CurrentAvailableItems;

            return this.CurrentAvailableItems;
        }

        /// <summary>
        /// Establishes the value expositor based on the Exposed Property type and the supplied Property Controller.
        /// </summary>
        // IMPORTANT: *** ALWAYS MAINTAIN THIS UPDATED IN CORRELATION (AS POSSIBLE) WITH RecordFieldExpositor.EstablishValueEditorControl() ***
        public void EstablishValueEditorControl()
        {
            if (this.MemberController.Definition.DataType.InheritsFrom(typeof(EditableCollection)))
            {
                SetValueEditorAsListBox((this.MemberController.ExternalItemsSourceGetter == null ? null : RetrieveAvailableItems()),
                                        this.MemberController.ExternalItemsSourceSelectedValuePath,
                                        this.MemberController.ExternalItemsSourceDisplayMemberPath,
                                        this.MemberController.CanCollectionBeEmpty,
                                        this.MemberController.EmptyCollectionTitle);
                return;
            }

            if (this.MemberController.ExternalItemsSourceGetter != null)
            {
                SetValueEditorAsComboBox(RetrieveAvailableItems(),
                                         this.MemberController.ExternalItemsSourceSelectedValuePath,
                                         this.MemberController.ExternalItemsSourceDisplayMemberPath);
                return;
            }

            if (this.MemberController.Definition is MModelPropertyDefinitor
                && (((MModelPropertyDefinitor)this.MemberController.Definition).RangeMax > ((MModelPropertyDefinitor)this.MemberController.Definition).RangeMin)
                && this.MemberController.Definition.DataType.IsOneOf(typeof(byte), typeof(sbyte), typeof(short), typeof(ushort),
                                                                     typeof(int), typeof(uint), typeof(long), typeof(ulong),
                                                                     typeof(float), typeof(double)))
            {
                SetValueEditorAsSlider();
                return;
            }

            if (this.MemberController.Definition.DataType.IsOneOf(typeof(byte), typeof(sbyte), typeof(short), typeof(ushort),
                                                                  typeof(int), typeof(uint), typeof(long), typeof(ulong),
                                                                  typeof(float), typeof(double), typeof(decimal)))
            {
                SetValueEditorAsMaskEditNumber(this.MemberController.Definition.DataType);
                return;
            }

            if (this.MemberController.Definition.DataType.IsEnum && this.MemberController.ExternalItemsSourceGetter == null)
            {
                var ExternalSource = Enum.GetValues(this.MemberController.Definition.DataType);

                SetValueEditorAsComboBox(ExternalSource, null, null);
                return;
            }

            if (this.MemberController.Definition.DataType == typeof(bool))
            {
                SetValueEditorAsCheckBox();
                return;
            }

            if (this.MemberController.Definition.DataType == typeof(DateTime))
            {
                SetValueEditorAsMaskEditDateTime();
                return;
            }

            if (this.MemberController.Definition.DataType == typeof(ImageSource))
            {
                SetValueEditorAsImagePicker();
                return;
            }

            if (this.MemberController.Definition.DataType.InheritsFrom(typeof(Brush)))
            {
                SetValueEditorAsColorizer();
                return;
            }

            this.ValueEditor.FontSize = FONT_SIZE;
        }

        private void ValueEditor_GotFocus(object sender, RoutedEventArgs e)
        {
            if (this.SourceMemberDefinitor == null
                || this.SourceInstance == null)     // This can happen after set to null a sub-entity
                return;

            StartEdit();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public void SetValueEditorAsComboBox(IEnumerable ExternalItemsSource, string ExternalItemsSelectedValuePath, string ExternalItemsDisplayMemberPath)
        {
            var Selector = new ComboBox();
            Selector.HorizontalContentAlignment = HorizontalAlignment.Stretch;

            var FirstItem = ExternalItemsSource.Cast<object>().FirstOrDefault();

            if (typeof(FormalPresentationElement).IsAssignableFrom(this.MemberController.Definition.DataType)
                || (FirstItem == null || typeof(FormalPresentationElement).IsAssignableFrom(FirstItem.GetType())))
                Selector.ItemTemplate = Display.GetResource<DataTemplate>("TplFormalPresentationElement");
            else
                if (typeof(SimplePresentationElement).IsAssignableFrom(this.MemberController.Definition.DataType)
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
                                         var SelectionValue = exp.SourceMemberDefinitor.Read(exp.SourceInstance);

                                         if (this.PropertyBinding != null && this.PropertyBinding.Converter != null)
                                             SelectionValue = exp.PropertyBinding.Converter.Convert(SelectionValue, exp.SourceMemberDefinitor.DataType,
                                                                                                    null, CultureInfo.CurrentCulture);

                                         if (Ctl.SelectedValuePath.IsAbsent())
                                             Ctl.SelectedItem = SelectionValue;
                                         else
                                         {
                                             var SelectionIndex = ExternalItemsSource.GetMatchingIndex(SelectionValue, ExternalItemsSelectedValuePath);
                                             if (SelectionIndex >= 0)
                                                 Ctl.SelectedIndex = SelectionIndex;
                                         }

                                         Ctl.SelectionChanged +=
                                             ((sender, args) =>
                                             {
                                                 if (args == null || args.AddedItems == null || args.AddedItems.Count < 1)
                                                     return;

                                                 object Value = args.AddedItems[0];

                                                 if (exp.PropertyBinding != null && exp.PropertyBinding.Converter != null)
                                                     Value = exp.PropertyBinding.Converter.ConvertBack(Value, exp.SourceMemberDefinitor.DataType,
                                                                                                       null, CultureInfo.CurrentCulture);

                                                 if (Value != null && !this.MemberController.Definition.DataType.IsAssignableFrom(Value.GetType()))
                                                     if (this.MemberController.Definition.DataType == typeof(string))
                                                         if (Value != null)
                                                             if (Value is IIdentifiableElement)
                                                                 Value = ((IIdentifiableElement)Value).TechName;
                                                             else
                                                                 Value = Value.ToString();

                                                 // This must be set for explicit change. Databinding didn't work, apparently by not supporting (Brush) inheritance.
                                                 exp.SourceMemberDefinitor.Write(exp.SourceInstance, Value);
                                             });
                                     },
                                     null);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public void SetValueEditorAsListBox(IEnumerable ExternalItemsSource, string ExternalItemsSelectedValuePath,
                                            string ExternalItemsDisplayMemberPath, bool CanCollectionBeEmpty, string EmptyCollectionTitle)
        {
            var Selector = new SelectionListBox();
            Selector.ExtraDataAvailableValues = this.ExtraDataAvailableValues;
            Selector.ExtraDataItems = this.ExtraDataItems;
            Selector.ExtraDataUpdater = this.ExtraDataUpdater;
            
            SetValueEditorControl(Selector, SelectionListBox.SelectedObjectsProperty,
                                     (exp) => (exp.ValueEditor as SelectionListBox).SelectedObjects,
                                     (exp, val) => (exp.ValueEditor as SelectionListBox).SelectedObjects = (IList)val,
                                     (exp) =>
                                     {
                                         var Ctl = exp.ValueEditor as SelectionListBox;
                                         Ctl.CanUnselectAll = CanCollectionBeEmpty;
                                         if (!EmptyCollectionTitle.IsAbsent())
                                            Ctl.EmptySelectionTitle = EmptyCollectionTitle;
                                         Ctl.AvailableObjects = ExternalItemsSource;
                                         Ctl.SelectedObjects = exp.SourceMemberDefinitor.Read(exp.SourceInstance) as IList;
                                     },
                                     null);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
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

                                         object Value = exp.SourceMemberDefinitor.Read(exp.SourceInstance);
                                         Ctl.Value = Convert.ToDecimal(Value);

                                         // This must be set for explicit change. Databinding didn't work, apparently by type clash (e.g.: decimal vs double).
                                        Ctl.EditingAction = ((value) => exp.SourceMemberDefinitor.Write(exp.SourceInstance, Convert.ChangeType(value, exp.SourceMemberDefinitor.DataType)));
                                     },
                                     null);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public void SetValueEditorAsCheckBox()
        {
            var Editor = new CheckBox();
            Editor.Margin = new Thickness(2, 3, 2, 2);

            SetValueEditorControl(Editor, CheckBox.IsCheckedProperty,
                                     (exp) => (exp.ValueEditor as CheckBox).IsChecked.IsTrue(),
                                     (exp, val) => (exp.ValueEditor as CheckBox).IsChecked = (bool)val,
                                     (exp) =>
                                     {
                                         var Ctl = exp.ValueEditor as CheckBox;
                                         Ctl.IsChecked = (bool)exp.SourceMemberDefinitor.Read(exp.SourceInstance);

                                         Action<object, RoutedEventArgs> Handler =
                                             ((sender, evargs) =>
                                                 {
                                                     exp.SourceMemberDefinitor.Write(exp.SourceInstance, Ctl.IsChecked.IsTrue());

                                                     // Raised in order to force update
                                                     Ctl.PostCall(ctl => ctl.RaiseEvent(new RoutedEventArgs(LostFocusEvent, ctl)));
                                                 });

                                         Ctl.Checked += new RoutedEventHandler(Handler);
                                         Ctl.Unchecked += new RoutedEventHandler(Handler);
                                     },
                                     null);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public void SetValueEditorAsSlider()
        {
            var Editor = new Slider();

            SetValueEditorControl(Editor, Slider.ValueProperty,
                                     (exp) => (exp.ValueEditor as Slider).Value,
                                     (exp, val) => (exp.ValueEditor as Slider).Value = Convert.ToDouble(val),
                                     (exp) =>
                                     {
                                         var Ctl = exp.ValueEditor as Slider;
                                         Ctl.Value = Convert.ToDouble(exp.SourceMemberDefinitor.Read(exp.SourceInstance));

                                         var PropDef = ((MModelPropertyDefinitor)exp.SourceMemberDefinitor);
                                         var Ticks = (PropDef.RangeIntervals == null
                                                      ? (PropDef.RangeMax - PropDef.RangeMin) / PropDef.RangeStep
                                                      : (double)PropDef.RangeIntervals.Length);
                                         var CalcWidth = (Ticks * Display.CHAR_PXWIDTH_DEFAULT * 1.5).EnforceMaximum(250.0);
                                         Ctl.Minimum = PropDef.RangeMin;
                                         Ctl.Maximum = PropDef.RangeMax;
                                         if (PropDef.RangeIntervals == null)
                                         {
                                             Ctl.SmallChange = PropDef.RangeStep;
                                             Ctl.LargeChange = PropDef.RangeStep;
                                             Ctl.TickFrequency = PropDef.RangeStep;
                                         }
                                         else
                                             Ctl.Ticks = new DoubleCollection(PropDef.RangeIntervals);

                                         Ctl.AutoToolTipPrecision = (Ticks > ((PropDef.RangeMax - PropDef.RangeMin) + 1) || PropDef.RangeStep.HasDecimals()
                                                                     ? 2 : 0);
                                         Ctl.AutoToolTipPlacement = AutoToolTipPlacement.BottomRight;
                                         Ctl.TickPlacement = ((PropDef.RangeIntervals == null
                                                               ? ((PropDef.RangeMax - PropDef.RangeMin) / PropDef.RangeStep.EnforceMinimum(0.5))
                                                               : (double)PropDef.RangeIntervals.Length) >= CalcWidth
                                                              ? TickPlacement.None : TickPlacement.Both);
                                         Ctl.IsSnapToTickEnabled = true;
                                         Ctl.Width = CalcWidth;
                                         Ctl.HorizontalAlignment = HorizontalAlignment.Left;

                                         /*-
                                         var Binder = new Binding();
                                         Binder.Source = Ctl;
                                         Binder.Path = new PropertyPath("Value");
                                         Binder.Mode = BindingMode.OneWay;
                                         Ctl.SetValue(Slider.ToolTipProperty, Binder);
                                          * 
                                         var TipLabel = new TextBlock();
                                         TipLabel.DataContext = Ctl;
                                         TipLabel.SetBinding(TextBlock.TextProperty, "Value");
                                         var CtlTip = new ToolTip();
                                         CtlTip.Content = TipLabel;
                                         CtlTip.StaysOpen = true;
                                         Ctl.ToolTip = TipLabel; */

                                         /*- 
                                         Ctl.ValueChanged += ((sender, args) =>
                                             {
                                                 exp.SourceMemberDefinitor.Write(exp.InstanceSource,
                                                 Convert.ChangeType(Ctl.Value, exp.SourceMemberDefinitor.DataType));

                                                 // Raised in order to force update
                                                 Ctl.PostCall(ctl => ctl.RaiseEvent(new RoutedEventArgs(LostFocusEvent, ctl)));
                                             }); */
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
                                         Ctl.Value = (DateTime)exp.SourceMemberDefinitor.Read(exp.SourceInstance);
                                     },
                                     null);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public void SetValueEditorAsImagePicker()
        {
            var Selector = new ImagePicker(this.MemberController);

            SetValueEditorControl(Selector, ImagePicker.SelectedImageProperty,
                                     (exp) => (exp.ValueEditor as ImagePicker).SelectedImage,
                                     (exp, val) => (exp.ValueEditor as ImagePicker).SelectedImage = val as ImageSource,
                                     (exp) =>
                                     {
                                         var Ctl = exp.ValueEditor as ImagePicker;
                                         Ctl.SelectedImage = exp.SourceMemberDefinitor.Read(exp.SourceInstance) as ImageSource;
                                     },
                                     null);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public void SetValueEditorAsColorizer()
        {
            var Selector = new PaletteDropColorizerButton();
            Selector.ClickDropsColorizer = true;

            SetValueEditorControl(Selector, PaletteDropColorizerButton.CurrentBrushProperty,
                                    (exp) => (exp.ValueEditor as PaletteDropColorizerButton).CurrentBrush,
                                    (exp, val) => (exp.ValueEditor as PaletteDropColorizerButton).CurrentBrush = val as Brush,
                                    (exp) =>
                                    {
                                        var Ctl = exp.ValueEditor as PaletteDropColorizerButton;
                                        Ctl.CurrentBrush = exp.SourceMemberDefinitor.Read(exp.SourceInstance) as Brush;

                                        // This must be set for explicit change. Databinding didn't work, apparently by not supporting (Brush) inheritance.
                                        Ctl.SelectionAction = (brush) => exp.SourceMemberDefinitor.Write(exp.SourceInstance, brush);
                                    },
                                    null);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public override string ToString()
        {
            return "EntPro-Expositor[" + this.ExposedProperty.ToStringAlways() + "]=" + this.GetEditedValue().ToStringAlways();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}
