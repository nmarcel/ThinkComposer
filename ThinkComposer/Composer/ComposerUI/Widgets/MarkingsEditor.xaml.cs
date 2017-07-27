using System;
using System.Collections;
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
using Instrumind.ThinkComposer.Model.GraphModel;
using Instrumind.ThinkComposer.Definitor.DefinitorMaintenance;

namespace Instrumind.ThinkComposer.Composer.ComposerUI.Widgets
{
    /// <summary>
    /// Interaction logic for MarkingsEditor.xaml
    /// </summary>
    public partial class MarkingsEditor : UserControl, IEntityViewChild
    {
        public MarkingsEditor()
        {
            InitializeComponent();
        }

        public MarkingsEditor(Idea TargetIdea,
                              Func<MarkerAssignment> CreationOperation,
                              Action<MarkerAssignment> EditingOperation,
                              MarkerAssignment InitialPointedMarker)
             : this()
        {
            this.TargetIdea = TargetIdea;
            this.CreationOperation = CreationOperation;
            this.EditingOperation = EditingOperation;
            this.InitialPointedMarker = InitialPointedMarker;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.MarkingsListBox.ItemsSource = this.TargetIdea.Markings;
            this.MarkingsToolPanel.TargetListBox = this.MarkingsListBox;

            if (this.InitialPointedMarker != null && this.TargetIdea.Markings.Contains(this.InitialPointedMarker))
            {
                this.MarkingsListBox.SelectedItem = this.InitialPointedMarker;

                if (this.InitialPointedMarker.MarkerHasDescriptor && !this.InitialPointedMarker.Descriptor.Name.IsAbsent())
                    this.MarkingsToolPanel_EditClicked(this, null);
            }
        }

        public Idea TargetIdea { get; protected set; }

        public Func<MarkerAssignment> CreationOperation { get; set; }

        public Action<MarkerAssignment> EditingOperation { get; set; }

        public MarkerAssignment InitialPointedMarker { get; set; }

        private void MarkingsToolPanel_EditClicked(object arg1, RoutedEventArgs arg2)
        {
            if (!this.MarkingsListBox.HasItems || this.MarkingsListBox.SelectedIndex < 0)
                return;

            var Marker = this.MarkingsListBox.SelectedItem as MarkerAssignment;

            this.EditingOperation(Marker);
        }

        private void MarkingsToolPanel_AddClicked(object arg1, RoutedEventArgs arg2)
        {
            var Marker = this.CreationOperation();
            if (Marker == null)
                return;

            this.TargetIdea.Markings.Add(Marker);
        }

        private void MarkingsToolPanel_DeleteClicked(object arg1, RoutedEventArgs arg2)
        {
            if (!this.MarkingsListBox.HasItems || this.MarkingsListBox.SelectedIndex < 0)
                return;

            var Marker = this.MarkingsListBox.SelectedItem as MarkerAssignment;
            this.TargetIdea.Markings.Remove(Marker);
        }

        private void MarkingsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.MarkingsToolPanel.BtnMoveTop.IsEnabled = (this.MarkingsListBox.HasItems && this.MarkingsListBox.SelectedIndex > 0);
            this.MarkingsToolPanel.BtnMoveUp.IsEnabled = (this.MarkingsListBox.HasItems && this.MarkingsListBox.SelectedIndex > 0);
            this.MarkingsToolPanel.BtnMoveDown.IsEnabled = (this.MarkingsListBox.HasItems && this.MarkingsListBox.SelectedIndex < this.MarkingsListBox.Items.Count - 1);
            this.MarkingsToolPanel.BtnMoveBottom.IsEnabled = (this.MarkingsListBox.HasItems && this.MarkingsListBox.SelectedIndex < this.MarkingsListBox.Items.Count - 1);

            var EditingCard = (e.AddedItems.Count < 1 ? null : e.AddedItems[0] as MarkerAssignment);
            if (EditingCard == null)
                return;
        }

        private void ComboMarkingDefinitor_Loaded(object sender, RoutedEventArgs e)
        {
            var Selector = (sender == null || !(sender is ComboBox) ? null : (ComboBox)sender);
            var Assignment = (sender == null || !(sender is Control) ? null : ((Control)sender).DataContext as MarkerAssignment);
            if (Selector == null || Assignment == null)
                return;

            Selector.SelectedItem = Assignment.Definitor;
        }

        private void ComboMarkingDefinitor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var Selector = (sender == null || !(sender is ComboBox) ? null : (ComboBox)sender);
            var Assignment = (sender == null || !(sender is Control) ? null : ((Control)sender).DataContext as MarkerAssignment);
            if (Selector == null || Assignment == null)
                return;

            Assignment.Definitor = (MarkerDefinition)(e.AddedItems[0]);
        }

        private void DescriptorContainer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount < 2)
                return;

            var Row = ((FrameworkElement)e.OriginalSource).GetNearestVisualDominantOfType<ListBoxItem>();
            if (Row == null || Row.Content == null)
                return;

            var Item = this.MarkingsListBox.ItemContainerGenerator.ItemFromContainer(Row);
            if (Item == null)
                return;

            this.MarkingsListBox.SelectedItem = Item;
            MarkingsToolPanel_EditClicked(sender, e);
        }

        private void CbxMarkerHasDescriptor_Click(object sender, RoutedEventArgs e)
        {
            var CbxControl = sender as CheckBox;
            if (CbxControl == null || !CbxControl.IsChecked.IsTrue())
                return;

            var LbxItem = CbxControl.GetNearestVisualDominantOfType<ListBoxItem>();
            if (LbxItem == null)
                return;

            LbxItem.IsSelected = true;

            this.PostCall(mke => mke.MarkingsToolPanel_EditClicked(sender, e));
        }

        public string ChildPropertyName
        {
            get { return Idea.__Markings.TechName; }
        }

        public IEntityView ParentEntityView { get; set; }

        public void Refresh()
        {
        }

        public bool Apply()
        {
            return true;
        }
    }
}
