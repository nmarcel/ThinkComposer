using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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

using Instrumind.ThinkComposer.ApplicationProduct;
using Instrumind.ThinkComposer.Definitor;
using Instrumind.ThinkComposer.Definitor.DefinitorUI;
using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;
using Instrumind.ThinkComposer.Model.GraphModel;
using Instrumind.ThinkComposer.Model.InformationModel;
using Instrumind.ThinkComposer.Model.VisualModel;

namespace Instrumind.ThinkComposer.Definitor.DefinitorUI.Widgets
{
    /// <summary>
    /// Interaction logic for DetailsDefinitorMaintainer.xaml
    /// </summary>
    public partial class DetailsDefinitorMaintainer : UserControl
    {
        public static readonly DependencyProperty TitleProperty;
        public static readonly DependencyProperty IsRestrictedToOnlyEditProperty;

        /// <summary>
        /// Static constructor.
        /// </summary>
        static DetailsDefinitorMaintainer()
        {
            DetailsDefinitorMaintainer.TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(DetailsDefinitorMaintainer),
                new FrameworkPropertyMetadata("Title", new PropertyChangedCallback(OnTitleChanged)));

            DetailsDefinitorMaintainer.IsRestrictedToOnlyEditProperty = DependencyProperty.Register("IsRestrictedToOnlyEdit", typeof(bool), typeof(DetailsDefinitorMaintainer),
                new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsRestrictedToOnlyEditChanged)));
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public DetailsDefinitorMaintainer()
        {
            InitializeComponent();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        protected IList<DetailDefinitionCard> DetailsSource = null;
        protected bool DetailsSourceIsGlobal = false;
        public IdeaDefinition SourceDefinitor { get; protected set; }
        public EntityEditEngine SourceEngine { get; protected set; }
        public DetailDesignator InitialDesignatorToEdit { get; protected set; }

        public void SetDetailDefinitionsSource(EntityEditEngine EditEngine, IdeaDefinition SourceDefinitor, IList<DetailDefinitionCard> Source,
                                               bool IsGlobal, DetailDesignator InitialDesignatorToEdit = null)
        {
            this.SourceDefinitor = SourceDefinitor;
            this.SourceEngine = EditEngine;
            this.DetailsSource = Source;
            this.DetailsSourceIsGlobal = IsGlobal;
            this.InitialDesignatorToEdit = InitialDesignatorToEdit;

            this.DetailsListBox.ItemsSource = Source;
            this.DetailsToolPanel.TargetListBox = this.DetailsListBox;

            // Notice that selection is made (see applied style), but focus highlight is only applied when it happens.
            if (this.DetailsSource.Count > 0)
                this.DetailsListBox.SelectedIndex = 0;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.IsLoaded && this.InitialDesignatorToEdit != null)
                this.PostCall(editor =>
                {
                    foreach (var Card in editor.DetailsSource)
                    {
                        if (Card.Designator.Value == InitialDesignatorToEdit
                            && Card.CanEditDesignator)
                        {
                            editor.DetailsListBox.SelectedItem = Card;
                            editor.DetailsToolPanel_EditClicked(this, null);
                            break;
                        }
                    }
                });
        }

        public string Title
        {
            get { return (string)GetValue(DetailsDefinitorMaintainer.TitleProperty); }
            set { SetValue(DetailsDefinitorMaintainer.TitleProperty, value); }
        }

        public bool IsRestrictedToOnlyEdit
        {
            get { return (bool)GetValue(DetailsDefinitorMaintainer.IsRestrictedToOnlyEditProperty); }
            set { SetValue(DetailsDefinitorMaintainer.IsRestrictedToOnlyEditProperty, value); }
        }

        private static void OnTitleChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            ((DetailsDefinitorMaintainer)depobj).DetailsToolPanel.Title = evargs.NewValue as string;
        }

        private static void OnIsRestrictedToOnlyEditChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var IsRestricted = (bool)evargs.NewValue;
            ((DetailsDefinitorMaintainer)depobj).DetailsToolPanel.CanAdd = !IsRestricted;
            ((DetailsDefinitorMaintainer)depobj).DetailsToolPanel.CanDelete = !IsRestricted;
            ((DetailsDefinitorMaintainer)depobj).DetailsToolPanel.CanMoveItems = !IsRestricted;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        protected void ListBoxItem_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var Item = sender as ListBoxItem;
            if (Item == null || Item.IsSelected)
                return;

            Item.IsSelected = true;
        }

        protected void ListBoxItem_DoubleClick(Object sender, MouseButtonEventArgs e)
        {
            var Item = sender as ListBoxItem;
            if (Item == null)
                return;

            DetailsToolPanel_EditClicked(sender, e);
        }

        private void DetailsToolPanel_EditClicked(object arg1, RoutedEventArgs arg2)
        {
            if (!this.DetailsListBox.HasItems || !this.DetailsListBox.SelectedIndex.IsInRange(0, this.DetailsListBox.Items.Count))
                return;

            var SelectedDefCard = this.DetailsListBox.SelectedItem as DetailDefinitionCard;
            if (SelectedDefCard == null)
                return;

            /*- if (!ProductDirector.ConfirmImmediateApply("Detail Designator", "IdeaEditing.DetailDesignatorEdit", "ApplyDialogChangesDirectly"))
                return; */

            SelectedDefCard.Designator.Value.Name = SelectedDefCard.Name;
            var EditResult = DomainServices.EditDetailDesignator(SelectedDefCard.Designator.Value, true, this.SourceEngine);
            if (EditResult.Item1.IsTrue())
            {
                SelectedDefCard.Name = SelectedDefCard.Designator.Value.Name;
                SelectedDefCard.Summary = SelectedDefCard.Designator.Value.Summary;
            }
        }

        private void DetailsToolPanel_AddClicked(object arg1, RoutedEventArgs arg2)
        {
            /*- if (!ProductDirector.ConfirmImmediateApply("IdeaEditing.DetailAdd", "ApplyDialogChangesDirectly"))
                    return; */

            var DetailOptions = new List<IRecognizableElement>();

            DetailOptions.Add(new SimplePresentationElement(AttachmentDetailDesignator.KindTitle, AttachmentDetailDesignator.KindName, AttachmentDetailDesignator.KindSummary, AttachmentDetailDesignator.KindPictogram));
            DetailOptions.Add(new SimplePresentationElement(LinkDetailDesignator.KindTitle, LinkDetailDesignator.KindName, LinkDetailDesignator.KindSummary, LinkDetailDesignator.KindPictogram));

            if (ProductDirector.ValidateEditionPermission(AppExec.LIC_EDITION_LITE, "designate Table detail", false))
                DetailOptions.Add(new SimplePresentationElement(TableDetailDesignator.KindTitle, TableDetailDesignator.KindName, TableDetailDesignator.KindSummary, TableDetailDesignator.KindPictogram));

            var DetailToCreate = Display.DialogMultiOption("Designation of Detail", "Select the type of detail to be designated...", "",
                                                           null, true, TableDetailDesignator.KindName, DetailOptions.ToArray());
            if (DetailToCreate == null)
                return;

            var SelectedDetailOption = DetailOptions.FirstOrDefault(det => det.TechName == DetailToCreate);
            var DesignationName = SelectedDetailOption.Name + " - Detail Definition " + (this.DetailsSource.Count + 1).ToString();
            DetailDefinitionCard NewEditCard = null;
            var Owner = Ownership.Create<IdeaDefinition, Idea>(this.SourceDefinitor);

            DetailDesignator CreatedDesignation = null;

            if (DetailToCreate == TableDetailDesignator.KindName)
                CreatedDesignation = DomainServices.CreateTableDesignation(this.SourceEngine, Owner, DesignationName);
            else
                if (DetailToCreate == AttachmentDetailDesignator.KindName)
                    CreatedDesignation = DomainServices.CreateAttachmentDesignation(Owner, DesignationName);
                else
                    if (DetailToCreate == LinkDetailDesignator.KindName)
                        CreatedDesignation = DomainServices.CreateLinkDesignation(Owner, DesignationName);

            if (CreatedDesignation == null)
                return;

            NewEditCard = new DetailDefinitionCard(false, new Assignment<DetailDesignator>(CreatedDesignation, this.DetailsSourceIsGlobal));

            this.DetailsSource.Add(NewEditCard);
            this.DetailsListBox.SelectedItem = NewEditCard;
        }

        private void DetailsToolPanel_DeleteClicked(object arg1, RoutedEventArgs arg2)
        {
            if (!this.DetailsListBox.HasItems || !this.DetailsListBox.SelectedIndex.IsInRange(0, this.DetailsListBox.Items.Count))
                return;

            this.DetailsSource.Remove(this.DetailsListBox.SelectedItem as DetailDefinitionCard);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        private void DetailsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.DetailsToolPanel.BtnMoveTop.IsEnabled = (this.DetailsListBox.HasItems && this.DetailsListBox.SelectedIndex > 0);
            this.DetailsToolPanel.BtnMoveUp.IsEnabled = (this.DetailsListBox.HasItems && this.DetailsListBox.SelectedIndex > 0);
            this.DetailsToolPanel.BtnMoveDown.IsEnabled = (this.DetailsListBox.HasItems && this.DetailsListBox.SelectedIndex < this.DetailsListBox.Items.Count - 1);
            this.DetailsToolPanel.BtnMoveBottom.IsEnabled = (this.DetailsListBox.HasItems && this.DetailsListBox.SelectedIndex < this.DetailsListBox.Items.Count - 1);

            var DefiningCard = (e.AddedItems.Count < 1 ? null : e.AddedItems[0] as DetailDefinitionCard);
            if (DefiningCard == null)
                return;

            this.DetailsToolPanel.BtnDefine.IsEnabled = (DefiningCard.Designator.Value is TableDetailDesignator);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        private void ComboTableLayout_Loaded(object sender, RoutedEventArgs e)
        {
            var Selector = (sender == null || !(sender is ComboBox) ? null : (ComboBox)sender);
            var DefiningCard = (sender == null || !(sender is Control) ? null : ((Control)sender).DataContext as DetailDefinitionCard);
            if (Selector == null || DefiningCard == null)
                return;

            Selector.SelectedItem = DefiningCard.TableLayout;
        }

        private void ComboTableLayout_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;

            var Selector = (sender == null || !(sender is ComboBox) ? null : (ComboBox)sender);
            var DefiningCard = (sender == null || !(sender is Control) ? null : ((Control)sender).DataContext as DetailDefinitionCard);
            if (Selector == null || DefiningCard == null)
                return;

            DefiningCard.TableLayout = (ETableLayoutStyle)(e.AddedItems[0]);
        }

        private void DetailsToolPanel_Loaded(object sender, RoutedEventArgs e)
        {
            var Target = sender as Instrumind.Common.Visualization.Widgets.CollectionEditingToolPanel;
            Target.BtnEdit.ButtonImage = Display.GetAppImage("page_designate.png");
            Target.BtnEdit.ButtonText = "Designate";
            Target.BtnEdit.ToolTip = "Modify the detail designation.";
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}
