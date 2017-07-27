using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;

using Instrumind.Common;
using Instrumind.Common.Visualization;
using Instrumind.Common.Visualization.Widgets;

using Instrumind.ThinkComposer.MetaModel.Configurations;
using Instrumind.ThinkComposer.Model.GraphModel;
using Instrumind.ThinkComposer.Model;

namespace Instrumind.ThinkComposer.Composer.Generation.Widgets
{
    /// <summary>
    /// Interaction logic for GeneratorSelectionEditor.xaml
    /// </summary>
    public partial class GeneratorSelectionEditor : UserControl
    {
        public Composition SourceComposition { get; protected set; }

        public FileGenerationConfiguration SourceConfiguration { get; protected set; }

        private GeneratorSelectionEditor()
        {
            InitializeComponent();
        }

        public GeneratorSelectionEditor(Composition SourceComposition)
             : this()
        {
            this.SourceComposition = SourceComposition;
            this.SourceConfiguration = SourceComposition.CompositeContentDomain.GenerationConfiguration;

            if (this.SourceConfiguration.TargetDirectory.IsAbsent())
                this.SourceConfiguration.TargetDirectory = Path.Combine(AppExec.UserDataDirectory, this.SourceComposition.TechName);

            if (this.SourceConfiguration.Language == null
                || !this.SourceConfiguration.Language.IsIn(this.SourceComposition.CompositeContentDomain.ExternalLanguages))
                this.SourceConfiguration.Language = SourceComposition.CompositeContentDomain.CurrentExternalLanguage;

            this.CbxLanguage.ItemsSource = SourceComposition.CompositeContentDomain.ExternalLanguages;

            this.DataContext = this.SourceConfiguration;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var EditPanel = this.GetNearestVisualDominantOfType<EntityEditPanel>();
            if (EditPanel == null)
                return;

            this.SourceConfiguration.CurrentSelection = IdeaSelection.CreateSelectionTree(this.SourceComposition, this.SourceConfiguration.ExcludedIdeasGlobalIds,
                                                                                          (idea => Tuple.Create(idea is Concept, idea is Concept, (IRecognizableElement)null)));

            this.TvSelection.ItemsSource = this.SourceConfiguration.CurrentSelection.IntoEnumerable();

            EditPanel.BtnApply.SetVisible(true, true);
            EditPanel.BtnApply.ButtonText = "Save configuration";
            EditPanel.BtnApply.ToolTip = "Saves the configuration parameters of the generation (but does not start it).";
            EditPanel.BtnApplyMessage = "Configuration saved.";

            EditPanel.BtnOK.ButtonText = "Generate!";
            EditPanel.BtnOK.ToolTip = "Generates files from the selected objects, based on the configuration parameters (which are saved).";

            EditPanel.BtnAdvanced.SetVisible(false);
            EditPanel.ShowAdvancedMembers = true;
        }

        private void BtnSelectDirectory_Click(object sender, RoutedEventArgs e)
        {
            var Result = Display.DialogGetOpenFolder("Select target directory...", this.SourceConfiguration.TargetDirectory.SubstituteFor("", null),
                                                     "GenerationFolder");
            if (Result == null)
                return;

            this.SourceConfiguration.TargetDirectory = Result.OriginalString;
        }

        private void TxbTargetDirectory_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.BtnSelectDirectory_Click(this, null);
        }

        private void BtnSelectAllIdeas_Click(object sender, RoutedEventArgs e)
        {
            this.SourceConfiguration.CurrentSelection.ApplySelector(idea => true);
        }

        private void BtnSelectAllConcepts_Click(object sender, RoutedEventArgs e)
        {
            this.SourceConfiguration.CurrentSelection.ApplySelector(idea => idea is Concept);
        }

        private void BtnSelectAllRelationships_Click(object sender, RoutedEventArgs e)
        {
            this.SourceConfiguration.CurrentSelection.ApplySelector(idea => idea is Relationship);
        }

        private void BtnUnselectAll_Click(object sender, RoutedEventArgs e)
        {
            this.SourceConfiguration.CurrentSelection.ApplySelector(idea => false);
        }
    }
}
