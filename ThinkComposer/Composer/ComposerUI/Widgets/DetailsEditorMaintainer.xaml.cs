using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.ApplicationProduct;
using Instrumind.ThinkComposer.MetaModel;
using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.MetaModel.InformationMetaModel;
using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;
using Instrumind.ThinkComposer.Model.GraphModel;
using Instrumind.ThinkComposer.Model.InformationModel;
using Instrumind.ThinkComposer.Model.VisualModel;
using Instrumind.ThinkComposer.Definitor;

namespace Instrumind.ThinkComposer.Composer.ComposerUI.Widgets
{
    /// <summary>
    /// Interaction logic for DetailsEditorMaintainer.xaml
    /// </summary>
    public partial class DetailsEditorMaintainer : UserControl
    {
        public static readonly DependencyProperty TitleProperty;
        public static readonly DependencyProperty IsRestrictedToOnlyEditProperty;

        /// <summary>
        /// Static constructor.
        /// </summary>
        static DetailsEditorMaintainer()
        {
            DetailsEditorMaintainer.TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(DetailsEditorMaintainer),
                new FrameworkPropertyMetadata("Title", new PropertyChangedCallback(OnTitleChanged)));

            DetailsEditorMaintainer.IsRestrictedToOnlyEditProperty = DependencyProperty.Register("IsRestrictedToOnlyEdit", typeof(bool), typeof(DetailsEditorMaintainer),
                new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsRestrictedToOnlyEditChanged)));

        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public DetailsEditorMaintainer()
        {
            InitializeComponent();

            this.ShowCustomLookZone = true;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        protected IList<DetailEditingCard> DetailsSource = null;
        public Idea SourceIdea { get; protected set; }
        public VisualSymbol SourceSymbol { get; protected set; }
        public CompositionEngine SourceEngine { get; protected set; }
        public bool IsLocal { get; protected set; }
        public bool AccessOnlyTables { get; protected set; }
        public DetailDesignator InitialDesignatorToEdit { get; protected set; }

        public bool ShowCustomLookZone { get; set; }

        public void SetDetailsSource(Idea SourceIdea, VisualSymbol SourceSymbol, IList<DetailEditingCard> Source,
                                     bool IsLocal, bool AccessOnlyTables = false, DetailDesignator InitialDesignatorToEdit = null)
        {
            General.ContractRequires(SourceSymbol == null || SourceIdea == SourceSymbol.OwnerRepresentation.RepresentedIdea);

            this.SourceIdea = SourceIdea;
            this.SourceSymbol = SourceSymbol;
            this.DetailsSource = Source;
            this.IsLocal = IsLocal;
            this.AccessOnlyTables = AccessOnlyTables;
            this.InitialDesignatorToEdit = InitialDesignatorToEdit;

            this.SourceEngine = SourceIdea.EditEngine as CompositionEngine;

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
                            if (Card.Designator.Value == InitialDesignatorToEdit)
                            {
                                editor.DetailsListBox.SelectedItem = Card;

                                if (Card.CanEditDesignator)
                                    editor.BtnEditDesignator_Click(
                                        editor.DetailsListBox.ItemContainerGenerator.ContainerFromItem(Card),
                                        null);
                                else
                                    Console.WriteLine("Attention: To change a Designation go to the Idea Definition.");

                                break;
                            }
                    });
        }

        public string Title
        {
            get { return (string)GetValue(DetailsEditorMaintainer.TitleProperty); }
            set { SetValue(DetailsEditorMaintainer.TitleProperty, value); }
        }

        public bool IsRestrictedToOnlyEdit
        {
            get { return (bool)GetValue(DetailsEditorMaintainer.IsRestrictedToOnlyEditProperty); }
            set { SetValue(DetailsEditorMaintainer.IsRestrictedToOnlyEditProperty, value); }
        }

        private static void OnTitleChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            ((DetailsEditorMaintainer)depobj).DetailsToolPanel.Title = evargs.NewValue as string;
        }

        private static void OnIsRestrictedToOnlyEditChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var IsRestricted = (bool)evargs.NewValue;
            ((DetailsEditorMaintainer)depobj).DetailsToolPanel.CanAdd = !IsRestricted;
            ((DetailsEditorMaintainer)depobj).DetailsToolPanel.CanDelete = !IsRestricted;
            ((DetailsEditorMaintainer)depobj).DetailsToolPanel.CanMoveItems = !IsRestricted;
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
            if (!this.DetailsListBox.HasItems || this.DetailsListBox.SelectedItem == null)
                return;

            var EditCard = this.DetailsListBox.SelectedItem as DetailEditingCard;
            var Designator = (EditCard.DetailContent == null ? EditCard.Designator : EditCard.DetailContent.ContentDesignator as Assignment<DetailDesignator>);
            var Look = (this.SourceSymbol == null ? null : this.SourceIdea.GetDetailLook(Designator.Value, this.SourceSymbol));

            /*-
            if (!ProductDirector.ConfirmImmediateApply("Detail Content", "IdeaEditing.DetailEdit", "ApplyDialogChangesDirectly"))
                return; */

            var Result = this.SourceEngine.EditIdeaDetail(Designator, this.SourceIdea, EditCard.DetailContent, Look);
            if (Result.Item1)
            {
                EditCard.DetailContent = Result.Item2;
                EditCard.DetailContent.UpdateDesignatorIdentification();
                EditCard.SetContent();
            }
        }

        private void DetailsToolPanel_AddClicked(object arg1, RoutedEventArgs arg2)
        {
            var Owner = Ownership.Create<IdeaDefinition, Idea>(this.SourceIdea);
            var CreatedDetail = SourceEngine.CreateIdeaDetail(Owner, this.SourceIdea,
                                                              this.DetailsSource.Select(card => card.Designator.Value),
                                                              this.AccessOnlyTables);
            if (CreatedDetail == null)
                return;

            var NewEditCard = new DetailEditingCard(false, new Assignment<DetailDesignator>(CreatedDetail.Designation, this.IsLocal), CreatedDetail);
            this.DetailsSource.Add(NewEditCard);
            this.DetailsListBox.SelectedItem = NewEditCard;
        }

        private void DetailsToolPanel_DeleteClicked(object arg1, RoutedEventArgs arg2)
        {
            if (!this.DetailsListBox.HasItems || !this.DetailsListBox.SelectedIndex.IsInRange(0, this.DetailsListBox.Items.Count))
                return;

            var Card = this.DetailsListBox.SelectedItem as DetailEditingCard;

            // IMPORTANT: System details cannot be deleted, unless they remains on a Converted Idea.
            if (Card.Designator != null && Card.Designator.Value != null
                && Card.Designator.Value.Alterability == EAlterability.System
                && Card.Designator.Value.IsIn(this.SourceIdea.IdeaDefinitor.DetailDesignators))
            {
                Display.DialogMessage("Attention!", "The '" + Card.Designator.Value.Name + "' detail cannot be removed.",
                                      EMessageType.Information);
                return;
            }

            this.DetailsSource.Remove(Card);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        private void BtnAccessContent_Click(object sender, RoutedEventArgs e)
        {
            var Row = ((FrameworkElement)e.OriginalSource).GetNearestVisualDominantOfType<ListBoxItem>();
            if (Row == null || Row.Content == null)
                return;

            var SelectedEditCard = Row.Content as DetailEditingCard;
            if (SelectedEditCard == null || SelectedEditCard.DetailContent is Table)    // Tables are accessed thru Editing.
                return;

            var Annex = SelectedEditCard.DetailContent as Attachment;
            if (Annex != null)
            {
                this.SourceEngine.ExternalEditDetailAttachment(SelectedEditCard.Designator, this.SourceIdea, Annex,
                                                               (AttachmentAppearance)SelectedEditCard.EditingLook);
                return;
            }

            var Reference = SelectedEditCard.DetailContent as Link;
            if (Reference == null)
            {
                var LinkDetDesignator = SelectedEditCard.Designator.Value as LinkDetailDesignator;
                if (LinkDetDesignator != null && LinkDetDesignator.DeclaringLinkType is InternalLinkType)
                    this.SourceEngine.GoToInternalLink(LinkDetDesignator, LinkDetDesignator.Initializer as MModelPropertyDefinitor, this.SourceIdea);
                else
                    Console.WriteLine("Cannot go to the link designated by '" + SelectedEditCard.Designator.Value.ToStringAlways() + "'.");
            }
            else
                this.SourceEngine.GoToLink(Reference, this.SourceIdea);
        }

        private void BtnExtractContent_Click(object sender, RoutedEventArgs e)
        {
            var Row = ((FrameworkElement)e.OriginalSource).GetNearestVisualDominantOfType<ListBoxItem>();
            if (Row == null || Row.Content == null)
                return;

            var SelectedEditCard = Row.Content as DetailEditingCard;
            if (SelectedEditCard == null)
                return;

            var Annex = SelectedEditCard.DetailContent as Attachment;
            if (Annex != null)
            {
                this.SourceEngine.ExportAttachment(Annex, this.SourceIdea);
                return;
            }

            var Matrix = SelectedEditCard.DetailContent as Table;
            if (Matrix != null)
            {
                DomainServices.ExportTableDataToFile(Matrix, null);
                return;
            }

            Console.WriteLine("Nothing to extract.");
        }

        private void BtnEditDesignator_Click(object sender, RoutedEventArgs e)
        {
            var Row = (sender is ListBoxItem
                       ? (ListBoxItem)sender
                       : ((FrameworkElement)e.OriginalSource).GetNearestVisualDominantOfType<ListBoxItem>(true));
            if (Row == null || Row.Content == null)
                return;

            var SelectedEditCard = Row.Content as DetailEditingCard;
            if (SelectedEditCard == null)
                return;

            /*? if (!ProductDirector.ConfirmImmediateApply("Detail Designator", "IdeaEditing.DetailDesignatorEdit", "ApplyDialogChangesDirectly"))
                    return; */

            SelectedEditCard.Designator.Value.Name = SelectedEditCard.Name;
            var EditResult = DomainServices.EditDetailDesignator(SelectedEditCard.Designator.Value, false, this.SourceEngine, true);
            if (EditResult.Item1.IsTrue())
            {
                SelectedEditCard.Name = SelectedEditCard.Designator.Value.Name;
                SelectedEditCard.Summary = SelectedEditCard.Designator.Value.Summary;

                if (EditResult.Item2 != null && SelectedEditCard.Designator.Value is TableDetailDesignator)
                {
                    var DetailTable = SelectedEditCard.DetailContent as Table;

                    if (DetailTable != null)
                        Table.UpdateTableFrom(DetailTable, EditResult.Item2);
                    else
                    {
                        DetailTable = Table.CreateTableFrom(SelectedEditCard.Designator, EditResult.Item2, SourceIdea);
                        SelectedEditCard.DetailContent = DetailTable;
                        SelectedEditCard.DetailContent.UpdateDesignatorIdentification();
                        SelectedEditCard.SetContent();

                        // Not needed? this.SourceIdea.Details.AddNew(DetailTable);
                    }
                }
            }
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        private void DetailsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.DetailsToolPanel.CanDelete = !this.IsRestrictedToOnlyEdit;

            this.DetailsToolPanel.BtnMoveTop.IsEnabled = (this.DetailsListBox.HasItems && this.DetailsListBox.SelectedIndex > 0);
            this.DetailsToolPanel.BtnMoveUp.IsEnabled = (this.DetailsListBox.HasItems && this.DetailsListBox.SelectedIndex > 0);
            this.DetailsToolPanel.BtnMoveDown.IsEnabled = (this.DetailsListBox.HasItems && this.DetailsListBox.SelectedIndex < this.DetailsListBox.Items.Count - 1);
            this.DetailsToolPanel.BtnMoveBottom.IsEnabled = (this.DetailsListBox.HasItems && this.DetailsListBox.SelectedIndex < this.DetailsListBox.Items.Count - 1);

            var EditingCard = (e.AddedItems.Count < 1 ? null : e.AddedItems[0] as DetailEditingCard);
            if (EditingCard == null)
                return;

            this.DetailsToolPanel.BtnDefine.IsEnabled = (EditingCard.Designator.Value is TableDetailDesignator);

            // For Converted Ideas which still have globally-defined details...
            if (this.IsRestrictedToOnlyEdit && !EditingCard.Designator.IsLocal
                && !this.SourceIdea.IdeaDefinitor.DetailDesignators.Any(dsn => dsn.Equals(EditingCard.Designator.Value)))
                this.DetailsToolPanel.CanDelete = true;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        private void ComboTableLayout_Loaded(object sender, RoutedEventArgs e)
        {
            var Selector = (sender == null || !(sender is ComboBox) ? null : (ComboBox)sender);
            var EditingCard = (sender == null || !(sender is Control) ? null : ((Control)sender).DataContext as DetailEditingCard);
            if (Selector == null || EditingCard == null)
                return;

            Selector.SelectedItem = EditingCard.TableLayout;
        }

        private void ComboTableLayout_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var Selector = (sender == null || !(sender is ComboBox) ? null : (ComboBox)sender);
            var EditingCard = (sender == null || !(sender is Control) ? null : ((Control)sender).DataContext as DetailEditingCard);
            if (Selector == null || EditingCard == null)
                return;

            EditingCard.TableLayout = (ETableLayoutStyle)(e.AddedItems[0]);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}
