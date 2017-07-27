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
using Instrumind.Common.Visualization;
using Instrumind.Common.Visualization.Widgets;

using Instrumind.ThinkComposer.Composer;
using Instrumind.ThinkComposer.Composer.ComposerUI;
using Instrumind.ThinkComposer.Model.GraphModel;
using Instrumind.ThinkComposer.Model.VisualModel;

namespace Instrumind.ThinkComposer.ApplicationProduct.Widgets
{
    /// <summary>
    /// Interaction logic for WidgetPropertyGrid.xaml
    /// </summary>
    public partial class WidgetInterrelationsPanel : UserControl, IEntityView
    {
        public WidgetInterrelationsPanel()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Sets the Target Entity.
        /// </summary>
        public void SetTarget(IModelEntity TargetEntity)
        {
            if (this.WorkingEntity == TargetEntity)
                return;

            this.FinishExposeProperties();

            if (TargetEntity == null || TargetEntity.EditEngine == null)
            {
                this.WorkingEntity = null; // By propagation clears the sources+targets trees.
                this.PropertiesPanel.Children.Clear();
                return;
            }

            this.WorkingEntity = TargetEntity;

            if (this.WorkingEntity == null)
                return;

            this.ExposeProperties();
        }

        /// <summary>
        /// Gets the Definitor of the Working Entity.
        /// </summary>
        public MModelClassDefinitor WorkingEntityDefinitor { get { return (this.WorkingEntity == null ? null : this.WorkingEntity.ClassDefinition); } }

        /// <summary>
        /// References the Working Entity.
        /// </summary>
        public IModelEntity WorkingEntity
        {
            get { return this.WorkingEntity_;  }
            set
            {
                if (this.WorkingEntity_ == value)
                    return;

                this.WorkingEntity_ = value;

                VisualRepresentation Representator = null;

                if (value != null)
                {
                    var WorkingIdea = this.WorkingEntity_ as Idea;
                    var WorkingView = ((CompositionEngine)this.WorkingEntity_.EditEngine).CurrentView;

                    if (WorkingIdea != null && WorkingView != null)
                        Representator = WorkingView.ViewChildren.Where(child => child.Key is VisualElement
                                                                                && ((VisualElement)child.Key).OwnerRepresentation
                                                                                        .RepresentedIdea == WorkingIdea)
                                                                .Select(child => ((VisualElement)child.Key).OwnerRepresentation)
                                                                    .FirstOrDefault();
                }

                this.TargetedRepresentation = Representator;
            }
        }
        private IModelEntity WorkingEntity_ = null;

        public VisualRepresentation TargetedRepresentation
        {
            get { return this.WorkingRepresentation_;  }
            set
            {
                if (this.WorkingRepresentation_ != null && this.WorkingRepresentation_.DisplayingView != null)
                {
                    this.WorkingRepresentation_.DisplayingView.OriginRepresentationsTrace.Clear();
                    this.WorkingRepresentation_.DisplayingView.TargetRepresentationsTrace.Clear();
                }

                this.WorkingRepresentation_ = value;

                if (value != null
                    && this.WorkingRepresentation_ != null && this.WorkingRepresentation_.DisplayingView != null)
                {
                    this.WorkingRepresentation_.DisplayingView.OriginRepresentationsTrace.Add(value);
                    this.WorkingRepresentation_.DisplayingView.TargetRepresentationsTrace.Add(value);
                }

                this.InterrelatedOriginsTree.ItemsSource = (value == null ? null : value.OriginRepresentations);
                this.InterrelatedTargetsTree.ItemsSource = (value == null ? null : value.TargetRepresentations);
            }
        }
        private VisualRepresentation WorkingRepresentation_ = null;

        /// <summary>
        /// Exposes the properties of the Target Object for user editing.
        /// </summary>
        public void ExposeProperties()
        {
            if (this.WorkingEntity == null)
            {
                this.PropertiesPanel.Children.Clear();
                return;
            }

            /*? CRASHES IN SOME SLOW LAPTOPS... (too many resources needed for propagation needed?)
            // Post-called to allow rise of the Unload event for the previously cleared expositors.
            this.PostCall(
                (widgetpanel) =>
                    {
                        widgetpanel.PropertiesPanel.Children.Clear();

                        if (this.WorkingEntityDefinitor == null)
                            return;

                        var Properties = this.WorkingEntityDefinitor.Properties.Where(prop => prop.IsDirectlyEditable)
                                                .OrderBy(prop => !prop.IsEditControlled).ThenBy(prop => prop.IsAdvanced)
                                                    .ThenBy(prop => prop.Name).ToList();

                        foreach (var Property in Properties)
                        {
                            var Expositor = new EntityPropertyExpositor(Property.TechName);
                            widgetpanel.PropertiesPanel.Children.Add(Expositor);
                        }

                        var ExpositionHandler = widgetpanel.EntityExposedForView;
                        if (ExpositionHandler != null)
                            ExpositionHandler();
                    }); */
        }

        public EntityPropertyExpositor GetExpositorOf(string PropertyName)
        {
            foreach (var Item in this.PropertiesPanel.Children)
            {
                var Expositor = Item as EntityPropertyExpositor;
                if (Expositor == null)
                    continue;

                if (Expositor.ExposedProperty == PropertyName)
                    return Expositor;
            }

            return null;
        }

        /// <summary>
        /// Finishes the exposition of the properties of the Target Object for user editing.
        /// </summary>
        public void FinishExposeProperties()
        {
            // Let the current properties be updated
            // (needed, because of no lost-focus triggered)
            foreach (var Item in this.PropertiesPanel.Children)
            {
                var Expositor = Item as EntityPropertyExpositor;
                if (Expositor == null)
                    continue;

                Expositor.UpdateTargetEntityProperty();
            }
        }

        #region IEntityView

        public IModelEntity AssociatedEntity { get { return this.WorkingEntity; } }

        public event Action EntityExposedForView;

        public bool ShowAdvancedMembers { get { return true; } }

        public event Action<bool> ShowAdvancedMembersChanged;

        public void ShowMessage(string Title, string Message, EMessageType MessageType = EMessageType.Information, IDictionary<string, object> AttachedData = null)
        {
            Console.WriteLine(Title + ": " + Message);
        }

        public void ReactToMemberEdited(MModelMemberDefinitor MemberDef, object Value)
        {
            if (!this.WorkingEntity.GetType().InheritsFrom(MemberDef.DeclaringType))
                return;

            MemberDef.Write(this.WorkingEntity, Value);
        }

        #endregion

        private void InterrelationsTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var CurrentView = ApplicationProduct.ProductDirector.WorkspaceDirector.ActiveDocument.ActiveDocumentView as View;
            if (CurrentView == null)
                return;

            var TargetRepresentation = e.NewValue as VisualRepresentation;
            if (TargetRepresentation == null)
                return;

            // Avoid infinite-loop
            if (IsNavigating)
                return;

            this.IsNavigating = true;

            var TargetIdea = TargetRepresentation.RepresentedIdea;

            /* CANCELLED: Selection prevents user to edit node
            if (TargetIdea == null)
                CurrentView.ClearAllSelectedObjects();
            else
            {
                var SelectableRepresentations = CurrentView.VisualRepresentations.Where(rep => rep.RepresentedIdea == TargetIdea);

                if (SelectableRepresentations.Any() && !SelectableRepresentations.Any(rep => rep.RepresentedIdea.IsSelected))
                    CurrentView.Manipulator.ApplySelection(SelectableRepresentations.First().MainSymbol, false,
                                                           (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)));
            } */

            IsNavigating = false;
        }
        private bool IsNavigating = false;

        private void InterrelationsTree_OnItemMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            /*-
            var TreeItem = e.Source as TreeViewItem;
            if (TreeItem == null || TreeItem.Header == null)
                return;

            var PointedView = TreeItem.Header as View;
            if (PointedView != null)
            {
                var Eng = PointedView.Engine;
                Eng.ShowCompositeAsView(PointedView.OwnerCompositeContainer);
            } */
        }

        private void InterrelationsTree_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            /*+ PENDING: WHAT TO DO HERE? (Something useful)
            var Source = e.OriginalSource as FrameworkElement;
            if (Source == null)
                return;

            var TreeItem= Source.GetNearestVisualDominantOfType<TreeViewItem>(true);
            if (TreeItem == null || TreeItem.Header == null)
                return;

            MessageBox.Show(TreeItem.Header.ToStringAlways(), "Edit..."); */
        }

        /* Too complicated...
        private void InterrelatedTargetsTree_PreviewKeyDown(object sender, KeyEventArgs evargs)
        {
            OperationResult<Concept> Creation = null;

            var WorkingView = ((CompositionEngine)this.WorkingEntity_.EditEngine).CurrentView;

            if (evargs.Key == System.Windows.Input.Key.Enter)
                Creation = ConceptCreationCommand.CreateAutoConceptAsSibling(WorkingView.SelectedRepresentations.First(), null, true, true);

            if (evargs.Key == System.Windows.Input.Key.Tab)
                Creation = ConceptCreationCommand.CreateAutoConceptFromRepresentation(WorkingView.SelectedRepresentations.First(), null,
                                                                                      !(Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)));

            if (Creation == null || !Creation.WasSuccessful)
                return;

            WorkingView.UnselectAllObjects();
            var Symbol = Creation.Result.VisualRepresentators.First().MainSymbol;
            WorkingView.SelectObject(Symbol);

            // Must be postcalled to avoid collision of oning pressed key with Auto creation logic.
            WorkingView.Presenter.PostCall(pres => pres.OwnerView.EditInPlace(Symbol));
        } */
    }
}
