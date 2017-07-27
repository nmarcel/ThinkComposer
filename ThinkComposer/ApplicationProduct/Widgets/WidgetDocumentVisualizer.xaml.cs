using System;
using System.Collections.Generic;
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
using System.Windows.Threading;

using Instrumind.Common;
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.Composer;
using Instrumind.ThinkComposer.Model.VisualModel;

namespace Instrumind.ThinkComposer.ApplicationProduct.Widgets
{
    /// <summary>
    /// Interaction logic for WidgetDocumentVisualizer.xaml
    /// </summary>
    public partial class WidgetDocumentVisualizer : UserControl, IDocumentVisualizer
    {
        public const double TABITEM_HEADER_HEIGHT = 20.0;

        public WidgetDocumentVisualizer()
        {
            InitializeComponent();

            this.ViewsTab.SelectionChanged += new SelectionChangedEventHandler(ViewsTab_SelectionChanged);

            // Clears the design-time tabs.
            this.ViewsTab.Items.Clear();
        }

        void ViewsTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;   // This avoids to call a parent's SelectionChanged event (such as in another Tab).

            var Item = (sender as TabControl).SelectedItem as TabItem;
            if (Item == null)
                return;

            if (e.RemovedItems != null && e.RemovedItems.Count > 0)
            {
                var PrevTab = e.RemovedItems[0] as TabItem;
                if (PrevTab != null)
                {
                    var PrevView = RegisteredViews.FirstOrDefault(view => view != null && view.GlobalId.Equals((Guid)PrevTab.Tag));
                    if (PrevView != null)
                        PrevView.ReactToDeactivation();
                }
            }
            var Target = RegisteredViews.FirstOrDefault(view => view != null && view.GlobalId.Equals((Guid)Item.Tag));

            if (Target != null && this.PostViewActivation != null)
                this.ActiveView = Target;
        }

        protected List<IDocumentView> RegisteredViews = new List<IDocumentView>();

        public TabItem FindTabItem(Guid Key)
        {
            foreach (var Item in this.ViewsTab.Items)
                if (((TabItem)Item).Tag.Equals(Key))
                    return Item as TabItem;

            return null;
        }

        #region IDocumentVisualizer Members

        public IEnumerable<IDocumentView> GetAllViews(ISphereModel ParentDocument = null)
        {
            if (ParentDocument == null)
                return this.RegisteredViews;

            return this.RegisteredViews.Where(view => view.ParentDocument == ParentDocument);
        }

        public ScrollViewer PutView(IDocumentView DocView)
        {
            General.ContractRequiresNotNull(DocView);

            Grid HostingGridPanel = null;
            ScrollViewer ScrollPresenter = null;
            var Target = FindTabItem(DocView.GlobalId);

            if (Target == null)
            {
                Target = new TabItem();
                Target.Background = Brushes.WhiteSmoke;
                Target.Tag = DocView.GlobalId;

                ScrollPresenter = new ScrollViewer();
                ScrollPresenter.ScrollChanged += new ScrollChangedEventHandler(ScrollPresenter_ScrollChanged);
                ScrollPresenter.PreviewMouseWheel += new MouseWheelEventHandler(ScrollPresenter_PreviewMouseWheel);
                ScrollPresenter.PreviewKeyDown += ((sdr, args) => args.Handled = ((CompositionEngine)DocView.EditEngine).ReactToKeyDown(args.Key));
                ScrollPresenter.Margin = new Thickness(1);

                ScrollPresenter.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                ScrollPresenter.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

                // Avoids the ugly dotted border to appear (even over the menu-bar)
                ScrollPresenter.FocusVisualStyle = null;
                
                HostingGridPanel = new Grid();

                DocView.PresenterHostingGrid = HostingGridPanel;

                Target.Content = HostingGridPanel;
                HostingGridPanel.Children.Add(ScrollPresenter);
                HostingGridPanel.Children.Add(DocView.TopCanvas);

                this.ViewsTab.Items.Add(Target);
                this.RegisteredViews.Add(DocView);

                DocView.HostingScrollViewer = ScrollPresenter;

                // IMPORTANT: This is intended to detect when the View (plus its HostingScrollViewer) is discarded via Undo.
                //            Therefore, no other properties can be trusted to be populated anymore (such as Name or GlobalId).
                DocView.PropertyChanged +=
                    ((sdr, args) =>
                        {
                            if (args.PropertyName == View.__HostingScrollViewer.TechName
                                && DocView.HostingScrollViewer == null)
                            {
                                DiscardViewControls(Target, true);

                                DocView.PropertyChanged -= DocView_PropertyChanged; 
                                this.RegisteredViews.Remove(DocView);
                            }
                        });
            }
            else
            {
                HostingGridPanel = (Grid)Target.Content;

                ScrollPresenter = (ScrollViewer)HostingGridPanel.Children[0];

                DocView.PropertyChanged -= DocView_PropertyChanged;

                DocView.HostingScrollViewer = ScrollPresenter;
            }

            DocView.HostingScrollViewer.Background = DocView.GetBackgroundImageBrush();

            // IMPORTANT: This cannot be null, because hit-testing does not work if no brush is present (must be at least transparent, better white).
            HostingGridPanel.Background = DocView.BackgroundWorkingBrush;   //T Display.GetGradientBrush(Colors.Azure, Colors.DodgerBlue, Colors.LightGreen)

            Target.Header = DocView.Title;
            Target.MouseDoubleClick += Target_MouseDoubleClick;

            if (DocView.PresenterControl == null)
                throw new UsageAnomaly("Cannot show Document content because its View has no Presenter Control.", DocView);

            // If pointing to an already opened tab...
            if (DocView.PresenterControl.Parent != null)
            {
                Target.IsSelected = true;
                DocView.PropertyChanged += DocView_PropertyChanged;
                return ScrollPresenter;
            }

            // Assignments for new tab...
            ScrollPresenter.Content = DocView.PresenterControl;
            DocView.PresenterHostingGrid = HostingGridPanel;

            DocView.PresenterControl.PostCall((DocViewvpres) =>
            {
                /*- var FactorX = (DocView.PageDisplayScale / 100.0);
                var FactorY = (DocView.PageDisplayScale / 100.0); */

                //- var FactorX = DocView.HostingScrollViewer.ExtentWidth / DocView.PresenterControl.ActualWidth.NaNDefault(1.0).EnforceMinimum(1.0);
                //- var FactorY = DocView.HostingScrollViewer.ExtentHeight / DocView.PresenterControl.ActualHeight.NaNDefault(1.0).EnforceMinimum(1.0);

                DocView.HostingScrollViewer.ScrollToHorizontalOffset(DocView.LastScrollOffset.X /*- * FactorX*/);
                DocView.HostingScrollViewer.ScrollToVerticalOffset(DocView.LastScrollOffset.Y /*- * FactorY*/);

                //T Console.WriteLine("VPW={0}, VPH={1}, CX={2}, CY={3}", src.ViewportWidth, src.ViewportHeight, DocView.ViewCenter.X, DocView.ViewCenter.Y);
            });

            Target.IsSelected = true;
            DocView.PropertyChanged += DocView_PropertyChanged;

            return ScrollPresenter;
        }

        void Target_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var Target = sender as TabItem;
            if (Target == null || Target.Tag == null || !(Target.Tag is Guid))
                return;

            var Position = e.GetPosition(Target);
            if (Position.Y > TABITEM_HEADER_HEIGHT)    // If double-clicking beyond the header... exit
                return;

            var ViewId = (Guid)Target.Tag;

            var TargetView = this.RegisteredViews.FirstOrDefault(doc => doc.GlobalId == ViewId);
            if (TargetView == null)
                return;

            var Context = TargetView.EditEngine;
            var EditingView = (View)TargetView;

            // IMPORTANT: This avoid crashes by a double-click over the close button [X] of the tab header.
            if (EditingView.IsClosing)
                return;

            CompositionEngine.EditViewProperties(EditingView);
        }

        void ScrollPresenter_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (this.ActiveView == null)
                return;

            this.ActiveView.ReactToMouseWheel(sender, e);
            e.Handled = true;
        }

        void ScrollPresenter_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (this.ActiveView != null && this.ActiveView.HostingScrollViewer == sender)
                this.ActiveView.LastScrollOffset = new Point(e.HorizontalOffset, e.VerticalOffset);

            /*T Console.WriteLine(DateTime.Now.ToString("HH:MM:SS.fff") + "----- ScrollChangedEvent just Occurred--------------------------------------------------------------------------");
            Console.WriteLine("ExtentHeight is now " + e.ExtentHeight.ToString());
            Console.WriteLine("ExtentWidth is now " + e.ExtentWidth.ToString());
            Console.WriteLine("ExtentHeightChange was " + e.ExtentHeightChange.ToString());
            Console.WriteLine("ExtentWidthChange was " + e.ExtentWidthChange.ToString());
            Console.WriteLine("HorizontalOffset={0}, VerticalOffset={1}", e.HorizontalOffset, e.VerticalOffset);
            Console.WriteLine("HorizontalChange was " + e.HorizontalChange.ToString());
            Console.WriteLine("VerticalChange was " + e.VerticalChange.ToString());
            Console.WriteLine("ViewportHeight is now " + e.ViewportHeight.ToString());
            Console.WriteLine("ViewportWidth is now " + e.ViewportWidth.ToString());
            Console.WriteLine("ViewportHeightChange was " + e.ViewportHeightChange.ToString());
            Console.WriteLine("ViewportWidthChange was " + e.ViewportWidthChange.ToString()); */
        }

        void DocView_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var DocView = sender as IDocumentView;
            if (DocView == null)
                return;

            var Target = FindTabItem(DocView.GlobalId);
            if (Target == null)
                return;

            if (e.PropertyName.ToUpper().IsOneOf("NAME", "TITLE"))
                Target.Header = DocView.Title;
        }

        public void DiscardView(Guid Key)
        {
            this.DiscardView(Key, true);
        }

        private void DiscardView(Guid Key, bool RemoveTab)
        {
            General.ContractRequiresNotNull(Key);

            var DocView = RegisteredViews.FirstOrDefault(view => view.GlobalId.Equals(Key));
            if (DocView == null)
                return;
                // Cancelled:
                // throw new UsageAnomaly("There is no view to discard with the supplied key.", Key);

            var Target = FindTabItem(Key);

            if (Target == null)
                throw new InternalAnomaly("Cannot find TabItem associated to View's key.", Key);

            var Engine = (CompositionEngine)DocView.EditEngine;

            /* Old style: If the remaining tab is not the main, then open it.
            if (RemoveTab && this.ViewsTab.Items.Count == 1
                && ((TabItem)this.ViewsTab.Items[0]).Tag.Equals(Engine.TargetComposition.RootView.GlobalId))
            {
                //? System.Media.SystemSounds.Exclamation.Play();
                Console.WriteLine("Cannot close document view. At least one must remain open.");
                return;
            } */

            if (CloseConfirmation != null && !CloseConfirmation(Key))
                return;

            // New style: If no other tabs remains open, then close the Composition.
            if (RemoveTab && this.ViewsTab.Items.Count == 1)
            {
                DocView.PresenterControl.PostCall(vpres => ProductDirector.CompositionDirector.CloseComposition(Engine));
                return;
            }

            DocView.Close();
            DocView.PropertyChanged -= DocView_PropertyChanged;

            if (Target != null)
                DiscardViewControls(Target, RemoveTab);

            this.RegisteredViews.Remove(DocView);
        }

        private void DiscardViewControls(TabItem Target, bool RemoveTab)
        {
            var HostingGrid = Target.Content as Grid;
            var HostingScrollPresenter = (HostingGrid == null || HostingGrid.Children.Count < 1
                                          ? null
                                          : HostingGrid.Children[0] as ScrollViewer);

            if (HostingScrollPresenter != null)
                HostingScrollPresenter.Content = null;

            if (HostingGrid != null)
                HostingGrid.Children.Clear();

            Target.Content = null;

            // Notice that this reassigns the Active-View.
            if (RemoveTab)
                this.ViewsTab.Items.Remove(Target);
        }

        public void DiscardAllViews(ISphereModel ParentDocument = null)
        {
            var Targets = RegisteredViews.OrderBy(v => !(v == this.ActiveView)).ToList();

            foreach (var Item in Targets)
                if (ParentDocument == null || Item.ParentDocument == ParentDocument)
                    DiscardView(Item.GlobalId, false);

            // All tabs are cleared together to avoid reassign the Active-View
            this.ViewsTab.Items.Clear();
        }

        public IDocumentView ActiveView
        {
            get { return this.ActiveView_; }
            set
            {
                if (this.ActiveView_ == value)
                    return;

                if (!this.RegisteredViews.Contains(value))
                    throw new UsageAnomaly("Cannot activate a non registered Document View.", value);

                this.ViewsTab.SelectedItem = FindTabItem(value.GlobalId);
                this.ActiveView_ = value;

                if (this.PostViewActivation != null)
                    this.PostViewActivation(this.ActiveView_);
            }
        }
        protected IDocumentView ActiveView_ = null;

        public Action<IDocumentView> PostViewActivation { get { return this.PostViewActivation_; } set { this.PostViewActivation_ = value; } }
        [NonSerialized]
        private Action<IDocumentView> PostViewActivation_ = null;

        public Func<Guid, bool> CloseConfirmation { get { return this.CloseConfirmation_; } set { this.CloseConfirmation_ = value; } }
        [NonSerialized]
        private Func<Guid, bool> CloseConfirmation_ = null;

        public void ScrollSegment(Orientation Direction, double Offset)
        {
            if (this.ViewsTab.SelectedItem == null || ((TabItem)this.ViewsTab.SelectedItem).Content == null)
                return;

            var HostingGrid = ((TabItem)this.ViewsTab.SelectedItem).Content as Grid;
            if (HostingGrid == null)
                return;

            var ScrollPresenter = HostingGrid.Children[0] as ScrollViewer;
            if (ScrollPresenter == null)
                return;

            if (Direction == Orientation.Vertical)
                ScrollPresenter.ScrollToVerticalOffset(ScrollPresenter.VerticalOffset + Offset);
            else
                ScrollPresenter.ScrollToHorizontalOffset(ScrollPresenter.HorizontalOffset + Offset);
        }

        #endregion

        private void TabCloser_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var TargetTab = ((FrameworkElement)sender).TemplatedParent as TabItem;
            if (TargetTab == null)
                return;

            var ViewId = (Guid)TargetTab.Tag;
            this.DiscardView(ViewId);
        }

    }
}
