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

using Instrumind.ThinkComposer.Composer;
using Instrumind.ThinkComposer.Composer.ComposerUI;
using Instrumind.ThinkComposer.Model;
using Instrumind.ThinkComposer.Model.GraphModel;
using Instrumind.ThinkComposer.Model.VisualModel;

namespace Instrumind.ThinkComposer.ApplicationProduct.Widgets
{
    /// <summary>
    /// Interaction logic for WidgetNavTree.xaml
    /// </summary>
    public partial class WidgetNavTree : UserControl
    {
        public WorkspaceManager WorkspaceDirector { get; protected set; }

        public WidgetNavTree()
        {
            InitializeComponent();
        }

        public WidgetNavTree(WorkspaceManager WorkspaceDirector)
            : this()
        {
            this.WorkspaceDirector = WorkspaceDirector;
        }

        // DO NOT MESS WITH THIS METHOD SIGNATURE (TO AVOID DESERIALIZATION CRASH)
        public void SetSource(ISphereModel Source)
        {
            this.Source = Source;
            Refresh();

            if (this.SourceChanged != null)
                this.SourceChanged(Source);
        }

        public ISphereModel Source { get; private set; }

        public Action<ISphereModel> SourceChanged { get; set; }

        public void Refresh()
        {
            this.NavTree.ItemsSource = null;

            if (Source != null)
                this.NavTree.ItemsSource = Source.NavigableItems;
        }

        private void NavTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // IMPORTANT: THIS IS CALLED WITH BUBBLING (PARENTS CALLED AFTER CONTAINED CHILDREN)
            //T MessageBox.Show(((IIdentifiableElement)e.NewValue).Name, "Selected item");

            this.CurrentView = this.WorkspaceDirector.ActiveDocument.ActiveDocumentView as View;
            if (this.CurrentView == null)
                return;

            var TargetEntity = e.NewValue as IModelEntity;
            if (TargetEntity == null)
                return;

            // Avoid infinite-loop
            if (IsNavigating)
                return;

            this.IsNavigating = true;
            
            this.TargetIdea = TargetEntity as Idea;

            if (TargetIdea == null)
            {
                var TargetView = TargetEntity as View;

                if (TargetView == null)
                {
                    this.CurrentView.UnselectAllObjects();
                    ApplicationProduct.ProductDirector.EditorInterrelationsControl.SetTarget(null);
                }
                else
                {
                    if (this.IsMouseOver && Mouse.LeftButton == MouseButtonState.Pressed)
                        TargetView.Engine.ShowView(TargetView);

                    ProductDirector.EditorInterrelationsControl.SetTarget(TargetView);
                }
            }

            this.IsNavigating = false;
            e.Handled = true;   // Stop undesired bubbling
        }
        private bool IsNavigating = false;
        private Idea TargetIdea;
        private View CurrentView;

        private void NavTree_MouseMove(object sender, MouseEventArgs e)
        {
            var NewPointingPosition = e.GetPosition(Application.Current.MainWindow);  // Must be in relation to main-window due to autoscroll

            if (this.TargetIdea == null || this.CurrentView == null
                || this.CurrentView.Engine.RunningMouseCommand != null
                || Mouse.LeftButton != MouseButtonState.Pressed
                || NewPointingPosition.IsNear(PreviousPointingPosition, 4))
                return;

            var SourceItem = Display.GetNearestVisualDominantOfType<TreeViewItem>(e.OriginalSource as FrameworkElement);
            if (SourceItem == null)
                return;
                
            var PointedIdea = SourceItem.Header as Idea;
            if (PointedIdea == null || PointedIdea != this.TargetIdea)
                return;

            this.CurrentView.Engine.RunningMouseCommand = new ShortcutCreationCommand(this.CurrentView.Engine, this.TargetIdea);
            this.CurrentView.Engine.RunningMouseCommand.Execute();
        }

        private Point PreviousPointingPosition = default(Point);

        private void NavTree_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.WorkspaceDirector.ActiveDocument == null)
                return;

            this.CurrentView = this.WorkspaceDirector.ActiveDocument.ActiveDocumentView as View;

            if (this.CurrentView != null && this.CurrentView.Engine.RunningMouseCommand is ShortcutCreationCommand)
            {
                this.CurrentView.Engine.RunningMouseCommand.Terminate();
                this.CurrentView.Engine.RunningMouseCommand = null;
            }

            this.PreviousPointingPosition = e.GetPosition(Application.Current.MainWindow);  // Must be in relation to main-window due to autoscroll

            if (!(e.OriginalSource is FrameworkElement))
                return;

            var SourceItem = Display.GetNearestVisualDominantOfType<TreeViewItem>(e.OriginalSource as FrameworkElement);
            if (SourceItem == null)
                return;

            SourceItem.Focus();
        }

        private void NavTree_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.PreviousPointingPosition = default(Point);
            Idea PointedIdea = null;

            if (e.OriginalSource is FrameworkElement)
            {
                var SourceItem = Display.GetNearestVisualDominantOfType<TreeViewItem>(e.OriginalSource as FrameworkElement);

                if (SourceItem != null)
                {
                    var PointedView = SourceItem.Header as View;

                    if (PointedView == null)
                        PointedIdea = SourceItem.Header as Idea;
                    else
                        if (PointedView == this.CurrentView)
                        {
                            CompositionEngine.EditViewProperties(PointedView);
                            return;
                        }
                }
            }

            if (PointedIdea != null && PointedIdea == this.TargetIdea)
            {
                // Is clicking for jump...
                this.IsNavigating = true;

                var PointingShortcut = (this.CurrentView.Manipulator.CurrentTargetedObject is VisualElement
                                        && ((VisualElement)this.CurrentView.Manipulator.CurrentTargetedObject).OwnerRepresentation.IsShortcut
                                        && ((VisualElement)this.CurrentView.Manipulator.CurrentTargetedObject).OwnerRepresentation.RepresentedIdea == TargetIdea);

                if (PointingShortcut)
                {
                    ApplicationProduct.ProductDirector.EditorInterrelationsControl.SetTarget(TargetIdea);
                    // Do not "jump", it could interfere with resizing/moving of shortcut symbols
                }
                else
                    if (TargetIdea.OwnerContainer != null)
                        if (TargetIdea.OwnerContainer.CompositeActiveView == this.CurrentView
                            && this.WorkspaceDirector.ActiveDocument.ActiveDocumentView == this.CurrentView)
                        {
                            var SelectableSymbols = this.CurrentView.ViewChildren
                                                        .Where(child => child.Key is VisualSymbol
                                                                        && ((VisualSymbol)child.Key).OwnerRepresentation
                                                                            .RepresentedIdea == TargetIdea)
                                                        .Select(child => (VisualSymbol)child.Key);

                            if (SelectableSymbols.Any() && !SelectableSymbols.Any(symb => symb.OwnerRepresentation.RepresentedIdea.IsSelected))
                                this.CurrentView.Manipulator.ApplySelection(SelectableSymbols.First(), false,
                                                                            (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)));

                            ApplicationProduct.ProductDirector.EditorInterrelationsControl.SetTarget(TargetIdea);
                            this.CurrentView.Presenter.BringIntoView(
                                  TargetIdea.VisualRepresentators.First(vrep => vrep.DisplayingView == this.CurrentView).MainSymbol.BaseArea);
                        }
                        else
                        {
                            var Representator = TargetIdea.MainRepresentator;

                            if (Representator != null && !CompositionEngine.IsSelectingByRectangle
                                /*- && this.IsMouseOver && Mouse.LeftButton == MouseButtonState.Pressed*/ )
                            {
                                Representator.DisplayingView.Engine.ShowView(Representator.DisplayingView);

                                Representator.DisplayingView.Presenter.PostCall(
                                    vpres =>
                                    {
                                        vpres.OwnerView.Manipulator.ApplySelection(Representator.MainSymbol);
                                        vpres.OwnerView.Presenter.BringIntoView(Representator.MainSymbol.BaseArea);
                                    });
                            }
                        }

                this.TargetIdea = null;
                this.IsNavigating = false;
            }
        }

        private void NavTree_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!(e.OriginalSource is FrameworkElement))
                return;

            var SourceItem = Display.GetNearestVisualDominantOfType<TreeViewItem>(e.OriginalSource as FrameworkElement);
            if (SourceItem == null)
                return;

            SourceItem.Focus(); // Lost when the context-menu is shown
        }

        private void NavTree_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!(e.OriginalSource is FrameworkElement))
                return;

            var SourceItem = Display.GetNearestVisualDominantOfType<TreeViewItem>(e.OriginalSource as FrameworkElement);
            if (SourceItem == null)
                return;

            Border ItemBorder = null;

            var ItemPresenter = SourceItem.Template.FindName("PART_Header", SourceItem) as ContentPresenter;
            if (ItemPresenter != null)
                ItemBorder = VisualTreeHelper.GetChild(ItemPresenter, 0) as Border;

            if (ItemBorder != null)
                ItemBorder.BorderBrush = Brushes.Black;
            else
                SourceItem.Focus(); // Remains visible until contex menu gains focus.

            // var PointedIdea = SourceItem.Header as Idea;
            ((CompositionEngine)this.WorkspaceDirector.ActiveDocumentEngine)
                                      .ShowContextMenu(this, SourceItem.Header as UniqueElement, null,
                                                        () => { if (ItemBorder != null) ItemBorder.BorderBrush = null; });
        }

        private void NavTree_MouseEnter(object sender, MouseEventArgs e)
        {
            ProductDirector.ShowAssistance("Click over an object to Go to it in the diagram View. Drag for create a shortcut of the targeted Idea.");
        }

        private void NavTree_MouseLeave(object sender, MouseEventArgs e)
        {
            ProductDirector.ShowAssistance();
        }

        /* CANCELLED/POSTPONED: For searching the TreeView should be expandend to reach the deepest levels and found items would be opening views (problematic)
        private void TxtNavFind_GotFocus(object sender, RoutedEventArgs e)
        {
            this.TxtNavFind.SelectAll();
        }

        private void TxtNavFind_LostFocus(object sender, RoutedEventArgs e)
        {
            this.TxtNavFind.Text = "...";
        }

        private void TxtNavFind_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.TxtNavFind.Text.IsAbsent() || this.TxtNavFind.Text=="...")
                return;


        }

        public TreeViewItem FindItem(IEnumerable<TreeViewItem> Items, string SearchedUpperText)
        {
            foreach (var Item in Items)
            {
                var Identifiable = Item.Header as IIdentifiableElement;
                if (Identifiable != null && Identifiable.Name.ToUpper().IndexOf(SearchedUpperText)>=0)
                    return Item;

                var FoundChild = FindItem(Item.Items, SearchedUpperText);
                if (FoundChild != null)
                    return FoundChild;
            }

            return null;
        } */
    }
}

