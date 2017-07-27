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
using Instrumind.Common.Visualization;
using Instrumind.Common.Visualization.Widgets;

using Instrumind.ThinkComposer.MetaModel.Configurations;
using Instrumind.ThinkComposer.Model;
using Instrumind.ThinkComposer.Model.GraphModel;

namespace Instrumind.ThinkComposer.Composer.Merging.Widgets
{
    /// <summary>
    /// Interaction logic for MergePreparer.xaml
    /// </summary>
    public partial class MergePreparer : UserControl
    {
        public Composition SourceComposition { get; protected set; }
        public Composition TargetComposition { get; protected set; }

        public SchemaMemberSelection SourceSelectionRoot { get; protected set; }
        public SchemaMemberSelection TargetSelectionRoot { get; protected set; }

        public MergePreparer()
        {
            InitializeComponent();
        }

        public MergePreparer(Composition SourceComposition, Composition TargetComposition,
                             SchemaMemberSelection SourceSelectionRoot, SchemaMemberSelection TargetSelectionRoot)
             : this()
        {
            General.ContractRequiresNotNull(SourceComposition, TargetComposition, SourceSelectionRoot, TargetSelectionRoot);

            this.SourceComposition = SourceComposition;
            this.SourceSelectionRoot = SourceSelectionRoot;

            this.TargetComposition = TargetComposition;
            this.TargetSelectionRoot = TargetSelectionRoot;

            this.MergerCompoBrowserTarget.IsForSelection = false;

            //- this.DataContext = this.TargetConfiguration;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var EditPanel = this.GetNearestVisualDominantOfType<EntityEditPanel>();
            if (EditPanel == null)
                return;

            // this.SourceConfiguration.CurrentSelection = IdeaSelection.CreateSelectionTree(this.SourceComposition, this.SourceConfiguration.ExcludedIdeasGlobalIds);

            this.MergerCompoBrowserSource.Title.Text = "From (external): " + this.SourceComposition.Name;
            this.MergerCompoBrowserSource.Title.ToolTip = this.SourceComposition.Summary;
            this.MergerCompoBrowserSource.Source.Text = this.SourceComposition.Engine.Location.NullDefault(this.SourceComposition.Engine.DomainLocation).Get(loc => loc.LocalPath);
            this.MergerCompoBrowserSource.Source.ToolTip = this.SourceComposition.Engine.Location.NullDefault(this.SourceComposition.Engine.DomainLocation).Get(loc => loc.LocalPath);
            this.MergerCompoBrowserSource.SelectedMemberOperation = (sel => CompareProperties(sel, null));

            this.MergerCompoBrowserSource.TvSelection.ItemsSource = this.SourceSelectionRoot.IntoEnumerable();

            this.MergerCompoBrowserTarget.Title.Text = "To (this): " + this.TargetComposition.Name;
            this.MergerCompoBrowserTarget.Title.ToolTip = this.TargetComposition.Summary;
            this.MergerCompoBrowserTarget.Source.Text = this.TargetComposition.Engine.Location.NullDefault(this.TargetComposition.Engine.DomainLocation).Get(loc => loc.LocalPath);
            this.MergerCompoBrowserTarget.Source.ToolTip = this.TargetComposition.Engine.Location.NullDefault(this.TargetComposition.Engine.DomainLocation).Get(loc => loc.LocalPath);
            this.MergerCompoBrowserTarget.SelectedMemberOperation = (sel => CompareProperties(null, sel));

            this.MergerCompoBrowserTarget.TvSelection.ItemsSource = this.TargetSelectionRoot.IntoEnumerable();

            EditPanel.BtnOK.ButtonText = "Merge!";
            EditPanel.BtnOK.ToolTip = "Merge this Composition/Domain whith the one specified, acquiring the selected Ideas and property values.";

            EditPanel.BtnAdvanced.SetVisible(false);
            EditPanel.ShowAdvancedMembers = true;
        }

        public void CompareProperties(SchemaMemberSelection SourceSelection, SchemaMemberSelection TargetSelection)
        {
            General.ContractRequires(SourceSelection != null || TargetSelection != null);

            if (SourceSelection == null)
            {
                SourceSelection = TargetSelection.FindEquivalentCounterpartAtSameLevel(this.SourceSelectionRoot, MergingManager.PreferredComparer);
                if (SourceSelection != null)
                    this.MergerCompoBrowserSource.TvSelection.PostCall(tv =>
                        {
                            if (this.MergerCompoBrowserSource.LastPointedItem != null)
                                this.MergerCompoBrowserSource.LastPointedItem.IsPointed = false;
                            this.MergerCompoBrowserSource.LastPointedItem = SourceSelection;
                            SourceSelection.IsPointed = true;

                            var Container = tv.FindContainerByItemInAllChildren(SourceSelection);
                            if (Container != null)
                                Container.BringIntoView();
                        });
            }

            if (TargetSelection == null)
            {
                TargetSelection = SourceSelection.FindEquivalentCounterpartAtSameLevel(this.TargetSelectionRoot, MergingManager.PreferredComparer);
                if (TargetSelection != null)
                    this.MergerCompoBrowserTarget.TvSelection.PostCall(tv =>
                    {
                        if (this.MergerCompoBrowserTarget.LastPointedItem != null)
                            this.MergerCompoBrowserTarget.LastPointedItem.IsPointed = false;
                        this.MergerCompoBrowserTarget.LastPointedItem = TargetSelection;
                        TargetSelection.IsPointed = true;

                        var Container = tv.FindContainerByItemInAllChildren(TargetSelection);
                        if (Container != null)
                            Container.BringIntoView();
                    });
            }

            this.MergerPropertiesComparer.CompareObjects(SourceSelection, TargetSelection);
        }

        private void BtnSelectAllIdeas_Click(object sender, RoutedEventArgs e)
        {
            this.SourceSelectionRoot.ApplySelector(idea => true);
        }

        private void BtnSelectAllConcepts_Click(object sender, RoutedEventArgs e)
        {
            this.SourceSelectionRoot.ApplySelector(idea => idea is Concept);
        }

        private void BtnSelectAllRelationships_Click(object sender, RoutedEventArgs e)
        {
            this.SourceSelectionRoot.ApplySelector(idea => idea is Relationship);
        }

        private void BtnUnselectAll_Click(object sender, RoutedEventArgs e)
        {
            this.SourceSelectionRoot.ApplySelector(idea => false);
        }

    }
}
