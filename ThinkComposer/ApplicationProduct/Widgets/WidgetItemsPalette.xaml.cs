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

using Instrumind.ThinkComposer.Composer;
using Instrumind.ThinkComposer.MetaModel;
using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;

namespace Instrumind.ThinkComposer.ApplicationProduct.Widgets
{
    /// <summary>
    /// Interaction logic for WidgetItemsPalette.xaml
    /// </summary>
    public partial class WidgetItemsPalette : ListBox
    {
        public DocumentEngine CurrentEngine { get; protected set; }

        public IRecognizableElement Palette { get; protected set; }

        public WidgetItemsPalette()
        {
            InitializeComponent();

            // this.ItemBorderBrush = Display.GetResource<Brush, EntitledPanel>("ItemBorderBrush");
            this.ItemSelectionBrush = Display.GetResource<Brush, EntitledPanel>("ItemSelectionBrush");
        }

        public WidgetItemsPalette(DocumentEngine Engine, IRecognizableElement Palette)
             : this()
        {
            this.CurrentEngine = Engine;
            this.Palette = Palette;

            var OwnerPanel = this.GetNearestVisualDominantOfType<EntitledPanel>();
            if (OwnerPanel == null)
                return;

            OwnerPanel.CreationText = "Create new Definition...";
        }

        private void ListBox_Loaded(object sender, RoutedEventArgs e)
        {
            var CompoEngine = this.CurrentEngine as CompositionEngine;
            if (CompoEngine == null)
                return;

            var OwnerPanel = this.GetNearestVisualDominantOfType<EntitledPanel>();
            if (OwnerPanel == null)
                return;

            if (this.Palette.IsIn(CompoEngine.GetExposedConceptsPalettes()))
            {
                OwnerPanel.CreateAction = (() => CompoEngine.ApplyPaletteItemCreation(this.Palette));
                OwnerPanel.CreationText = "Create new " + ConceptDefinition.__ClassDefinitor.Name;
            }

            if (this.Palette.IsIn(CompoEngine.GetExposedRelationshipsPalettes()))
            {
                OwnerPanel.CreateAction = (() => CompoEngine.ApplyPaletteItemCreation(this.Palette));
                OwnerPanel.CreationText = "Create new " + RelationshipDefinition.__ClassDefinitor.Name;
            }

            if (this.Palette.IsIn(CompoEngine.TargetComposition.CompositeContentDomain.MarkerClusters))
            {
                OwnerPanel.CreateAction = (() => CompoEngine.ApplyPaletteItemCreation(this.Palette));
                OwnerPanel.CreationText = "Create new " + MarkerDefinition.__ClassDefinitor.Name;
            }
        }

        // private Brush ItemBorderBrush = null;
        private Brush ItemSelectionBrush = null;

        public void ClearSelection()
        {
            if (PreviousRow != null)
            {
                var ItemBorder = Display.GetTemplateChild<Border>(PreviousRow, "ItemBorder");
                ItemBorder.BorderBrush = Brushes.Transparent;
            }
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            ClearSelection();

            // NOTE: The "Preview" event was used because the normal one is managed by the ListBoxItems.
            e.Handled = true;

            var Engine = ProductDirector.WorkspaceDirector.ActiveDocumentEngine as CompositionEngine;
            if (Engine == null)
                return;

            Engine.DoCancelOperation();

            var Row = ((FrameworkElement)e.OriginalSource).GetNearestVisualDominantOfType<ListBoxItem>();
            if (Row == null || Row.Content == null)
                return;

            this.SelectedItem = this.ItemContainerGenerator.ItemFromContainer(Row);

            if (this.SelectedItem == null)
                return;

            var ItemBorder = Display.GetTemplateChild<Border>(Row, "ItemBorder");
            ItemBorder.BorderBrush = this.ItemSelectionBrush;
            PreviousRow = Row;

            this.CurrentEngine.ApplyPaletteItemSelection(this.SelectedItem as IRecognizableElement, this.Palette, e.ClickCount > 1);
            this.SelectedItem = null;
        }

        protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseRightButtonDown(e);

            var Engine = ProductDirector.WorkspaceDirector.ActiveDocumentEngine as CompositionEngine;
            if (Engine == null)
                return;

            Engine.DoCancelOperation();

            e.Handled = true;
        }

        private static ListBoxItem PreviousRow = null;
    }
}
