// -------------------------------------------------------------------------------------------
// Instrumind ThinkComposer
//
// Copyright (C) 2011-2015 Néstor Marcel Sánchez Ahumada.
// http://thinkcomposer.codeplex.com
//
// This file is part of ThinkComposer, which is free software licensed under the GNU General Public License.
// It is provided without any warranty. You should find a copy of the license in the root directory of this software product.
// -------------------------------------------------------------------------------------------
//
// Project: Instrumind ThinkComposer v1.0
// File   : IndividualEditPanel.cs
// Object : Instrumind.Common.Visualization.Widgets.IndividualEditPanel (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.07.29 Néstor Sánchez A.  Creation
//

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

/// Library of standard Instrumind WPF custom and user controls.
namespace Instrumind.Common.Visualization.Widgets
{
    /// <summary>
    /// Provides a visual editing container panel for entity data.
    /// Intended to be used inside a dialog window.
    /// </summary>
    public partial class EntityEditPanel : UserControl, IEntityView
    {
        public EntityEditPanel()
        {
            InitializeComponent();
        }

        public EntityEditPanel(IModelEntity AssociatedEntity)
             : this()
        {
            this.AssociatedEntity = AssociatedEntity;

            //  PENDING: Enable/show when there is helping content.
            this.HasHelpForShowing = false;

            this.Loaded += new RoutedEventHandler(IndividualEditPanel_Loaded);
        }

        public TabbedEditPanel ContentPanel { get { return (TabbedEditPanel)this.Content; } }

        public PaletteButton BtnApply { get; protected set; }
        public PaletteButton BtnRefresh { get; protected set; }
        public PaletteButton BtnHelp { get; protected set; }
        public PaletteToggleButton BtnAdvanced { get; protected set; }
        public PaletteButton BtnOK { get; protected set; }
        public PaletteButton BtnCancel { get; protected set; }

        void IndividualEditPanel_Loaded(object sender, RoutedEventArgs e)
        {
            this.ParentWindow = Window.GetWindow(this);
            if (this.ParentWindow == null)
                throw new InternalAnomaly("Window cannot be obtained for this UserControl.");

            this.BtnApply = this.GetTemplateChild<PaletteButton>("BtnApply");
            BtnApply.SetVisible(this.ShowApply, true);

            //this.BtnRefresh = this.GetTemplateChild<PaletteButton>("BtnRefresh");
            //BtnRefresh.SetVisible(this.ShowRefresh, false);

            this.BtnAdvanced = this.GetTemplateChild<PaletteToggleButton>("BtnAdvanced");

            this.BtnHelp = this.GetTemplateChild<PaletteButton>("BtnHelp");
            BtnHelp.SetVisible(this.HasHelpForShowing, false);

            this.BtnOK = this.GetTemplateChild<PaletteButton>("BtnOK");
            this.BtnCancel = this.GetTemplateChild<PaletteButton>("BtnCancel");

            this.ShowAdvancedMembers = AppExec.GetConfiguration<bool>("UserInterface", "ShowAdvancedProperties", false);
            this.BtnAdvanced.IsChecked = this.ShowAdvancedMembers;

            this.ParentWindow.Loaded += new RoutedEventHandler(ParentWindow_Loaded);
            this.ParentWindow.Closing += new System.ComponentModel.CancelEventHandler(ParentWindow_Closing);
            this.ParentWindow.Closed += new EventHandler(ParentWindow_Closed);

            this.BtnHelp.SetVisible(this.HasHelpForShowing);

            // Asked here because may have been setted before template load
            if (!this.CanCancelEditing)
                this.BtnCancel.IsEnabled = false;

            // Append extra buttons
            var PnlButtons = this.GetTemplateChild<Panel>("PnlButtons");
            if (PnlButtons != null && this.ExtraButtons != null && this.ExtraButtons.Count > 0)
                foreach (var Extra in this.ExtraButtons)
                {
                    var LocalExtra = Extra;
                    var NewButton = new PaletteButton(LocalExtra.Item1, LocalExtra.Item3, Summary: LocalExtra.Item2);
                    NewButton.Margin = new Thickness(2);
                    NewButton.Click += ((sndr, args) => LocalExtra.Item4(this.AssociatedEntity));
                    PnlButtons.Children.Add(NewButton);
                }

            // Set header
            var Header = Display.GetTemplateChild<Border>(this, "BrdHeader");
            if (Header == null)
                return;

            Header.Child = this.HeaderContent;
            Header.SetVisible(this.HeaderContent != null);
        }

        protected Window ParentWindow = null;

        void ParentWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.AssociatedEntity == null)
                throw new InternalAnomaly("Entity not yet associated while loading view.");
        }

        void ParentWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // PENDING: CHECK FOR PENDING CHANGES AND ASK IF LOOSE THEM
        }

        void ParentWindow_Closed(object sender, EventArgs e)
        {
            // PENDING: UNSUBSCRIBE FROM EVENTS

            if (this.ParentWindow is DialogOptionsWindow && this.ParentWindow.DialogResult == null)
                this.ParentWindow.DialogResult = false;
        }

        /// <summary>
        /// Associates, and optionally refreshes, the dependeant Entity View children
        /// </summary>
        public void AssociateEntityViewChildren(bool Refresh = true)
        {
            this.AssociatedEntity.Controller.ResetDependantEntityViewChildren();

            var ViewChildren = this.ContentPanel.Tabs.Where(tabreg => tabreg.Value.Content is IEntityViewChild)
                                                            .Select(tab => (IEntityViewChild)tab.Value.Content);

            foreach (var ViewChild in ViewChildren)
            {
                this.AssociatedEntity.Controller.RegisterDependantEntityViewChild(ViewChild);
                ViewChild.ParentEntityView = this;

                if (Refresh)
                    ViewChild.Refresh();
            }
        }

        /// <summary>
        /// Indicates whether the editing can be cancelled via the Cancel button.
        /// </summary>
        public bool CanCancelEditing
        {
            get { return this.CanCancelEditing_; }
            set
            {
                this.CanCancelEditing_ = value;

                // Notice that this is created on load via template
                if (this.BtnCancel != null)
                    this.BtnCancel.IsEnabled = false;
            }
        }
        protected bool CanCancelEditing_ = true;

        /// <summary>
        /// Indicates whether to show the Advanced members.
        /// </summary>
        public bool ShowAdvancedMembers
        {
            get { return this.ShowAdvancedMembers_; }
            set
            {
                this.ShowAdvancedMembers_ = value;

                var Handler = this.ShowAdvancedMembersChanged;
                if (Handler != null)
                    Handler(this.ShowAdvancedMembers_);
            }
        }
        protected bool ShowAdvancedMembers_;

        public event Action<bool> ShowAdvancedMembersChanged;

        /// <summary>
        /// Indicates whether to show the Help button.
        /// </summary>
        public bool HasHelpForShowing = true;

        /// <summary>
        /// Indicates whether to show the Refresh button.
        /// </summary>
        public bool ShowRefresh = true;

        /// <summary>
        /// Indicates whether to show the Apply button.
        /// </summary>
        public bool ShowApply = false;

        #region IEntityView Members

        public IModelEntity AssociatedEntity
        {
            get
            {
                return this.AssociatedEntity_;
            }
            set
            {
                if (this.AssociatedEntity_ != value)
                {
                    this.AssociatedEntity_ = value;

                    var Annouce = EntityExposedForView;
                    if (Annouce != null)
                        Annouce();
                }
            }
        }
        protected IModelEntity AssociatedEntity_ = null;

        public event Action EntityExposedForView;

        public void ShowMessage(string Title, string Message, EMessageType MessageType = EMessageType.Information, IDictionary<string, object> AttachedData = null)
        {
            Display.DialogMessage(Title, Message, MessageType, MessageBoxButton.OK, MessageBoxResult.OK, AttachedData);
        }

        public void ReactToMemberEdited(MModelMemberDefinitor MemberDef, object Value)
        {
        }
        
        #endregion

        public void BtnAdvanced_Click(object sender, RoutedEventArgs e)
        {
            this.ShowAdvancedMembers = this.BtnAdvanced.IsChecked.IsTrue();

            AppExec.SetConfiguration<bool>("UserInterface", "ShowAdvancedProperties", this.ShowAdvancedMembers, true);
        }

        public void BtnHelp_Click(object sender, RoutedEventArgs e)
        {
            this.AssociatedEntity.Controller.Help();
        }

        public void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            this.AssociatedEntity.Controller.StartEdit();
        }

        public void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            // This forces the LostFocus event of the current TextBlock to be triggered,
            // hence updating its binding source.
            var BtnApply = Display.GetTemplateChild<PaletteButton>(this, "BtnApply");
            if (BtnApply != null)
                BtnApply.Focus();

            // // An alternative is...
             
            // TextBox textBox = Keyboard.FocusedElement as TextBox;
            // if (textBox != null)
            // {
            //     BindingExpression be =
            //       textBox.GetBindingExpression(TextBox.TextProperty);
            //     if (be != null)
            //       be.UpdateSource();
            // }

            if (this.ApplyChanges())
                Display.DialogMessage("Attention!", this.BtnApplyMessage, EMessageType.Information);
        }

        public string BtnApplyMessage = "Changes were applied.";

        private bool ApplyChanges()
        {
            // Select the parameters to pass to the Apply operation (the editing panels)
            var EditPanels = new List<object>();

            if (this.Content is TabbedEditPanel)
                EditPanels.AddRange(((TabbedEditPanel)this.Content).Tabs.Select(tab => tab.Value.Content).Cast<object>());
            else
                EditPanels.Add(this.Content);

            // Call the apply operation
            if (!this.AssociatedEntity.Controller.ApplyEdit(EditPanels.ToArray()))
            {
                ShowErrors();
                return false;
            }

            return true;
        }

        public void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            // This forces the LostFocus event of the current TextBlock to be triggered,
            // hence updating its binding source.
            var BtnOk = Display.GetTemplateChild<PaletteButton>(this, "BtnOK");
            if (BtnOk != null)
                BtnOk.Focus();

            // // An alternative is...

            // TextBox textBox = Keyboard.FocusedElement as TextBox;
            // if (textBox != null)
            // {
            //     BindingExpression be =
            //       textBox.GetBindingExpression(TextBox.TextProperty);
            //     if (be != null)
            //       be.UpdateSource();
            // }

            /* // Select the parameters to pass to the Apply operation (the editing panels)
            var EditPanels = new List<object>();

            if (this.Content is TabbedEditPanel)
                EditPanels.AddRange(((TabbedEditPanel)this.Content).Tabs.Select(tab => tab.Value.Content).Cast<object>());
            else
                EditPanels.Add(this.Content);

            // Call the Apply operation
            if (!this.AssociatedEntity.Controller.ApplyEdit(EditPanels.ToArray()))
            {
                ShowErrors();
                return;
            } */

            if (!this.ApplyChanges())
                return;

            if (this.ParentWindow is DialogOptionsWindow)
                this.ParentWindow.DialogResult = true;

            this.ParentWindow.Close();
            // See ParentWindow_Closing and ParentWindow_Closed
        }

        public void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.ParentWindow.Close();
            // See ParentWindow_Closing and ParentWindow_Closed
        }

        private List<Tuple<string, string, ImageSource, Action<IMModelClass>>> ExtraButtons = null;
        public void AppendExtraButton(string ButtonText, string ButtonToolTip, ImageSource ButtonImage,
                                      Action<IMModelClass> Operation)
        {
            if (this.ExtraButtons == null)
                this.ExtraButtons = new List<Tuple<string, string, ImageSource, Action<IMModelClass>>>();

            this.ExtraButtons.Add(Tuple.Create(ButtonText, ButtonToolTip, ButtonImage, Operation));
        }

        public void ShowErrors()
        {
            // PENDING
            var Errors = this.AssociatedEntity.Controller.Errors;
        }

        public UIElement HeaderContent
        {
            get { return this.HeaderContent_; }
            set
            {
                this.HeaderContent_ = value;
            }
        }
        private UIElement HeaderContent_ = null;
    }
}
