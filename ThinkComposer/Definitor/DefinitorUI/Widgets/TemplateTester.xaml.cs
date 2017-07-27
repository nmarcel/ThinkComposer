using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

using Instrumind.Common;
using Instrumind.Common.Visualization;
using Instrumind.Common.Visualization.Widgets;

using Instrumind.ThinkComposer.Composer.Generation;
using Instrumind.ThinkComposer.MetaModel.Configurations;
using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.Model.GraphModel;
using Instrumind.ThinkComposer.Model;

namespace Instrumind.ThinkComposer.Definitor.DefinitorUI.Widgets
{
    /// <summary>
    /// Interaction logic for TemplateTester.xaml
    /// </summary>
    public partial class TemplateTester : UserControl
    {
        public static Idea TestTemplate(Type ExpectedSourceIdeaKind, IdeaDefinition ExpectedSourceIdeaDefinition,
                                        string SourceTemplateName, string SourceTemplateText,
                                        Composition SourceComposition, Idea SourceIdea = null, bool CanSelectSource = true)
        {
            DialogOptionsWindow Dialog = null;

            var TemplateEd = new TemplateTester(ExpectedSourceIdeaKind, ExpectedSourceIdeaDefinition,
                                                SourceTemplateText, SourceComposition, SourceIdea, CanSelectSource);

            Display.OpenContentDialogWindow(ref Dialog, "Test of " + SourceTemplateName, TemplateEd, 600, 750);

            return TemplateEd.SourceIdea;
        }

        public Type ExpectedSourceIdeaKind { get; set; }

        public IdeaDefinition ExpectedSourceIdeaDefinition { get; set; }

        public Composition SourceComposition { get; set; }

        public Idea SourceIdea { get; set; }

        public string SourceTemplateText { get; set; }

        public string PreviewName { get; set; }

        public string PreviewFileExtension { get; set; }

        public TemplateTester()
        {
            InitializeComponent();
        }

        public TemplateTester(Type ExpectedSourceIdeaKind, IdeaDefinition ExpectedSourceIdeaDefinition,
                              string SourceTemplateText, Composition SourceComposition,
                              Idea SourceIdea = null, bool CanSelectSource = true)
             : this()
        {
            this.ExpectedSourceIdeaKind = ExpectedSourceIdeaKind;
            this.ExpectedSourceIdeaDefinition = ExpectedSourceIdeaDefinition;
            this.SourceTemplateText = SourceTemplateText;
            this.SourceComposition = SourceComposition;
            this.SourceIdea = SourceIdea;

            this.TisSourceSelector.CompositesSource = this.SourceComposition.IntoEnumerable();
            this.TisSourceSelector.Value = this.SourceIdea.NullDefault(this.SourceComposition);
            this.TisSourceSelector.HideSelectNullItemActioner = true;

            if (CanSelectSource)
                this.TisSourceSelector.EditingAction = (source => this.GeneratePreview(source as Idea));
            else
                this.TisSourceSelector.IsEnabled = false;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.GeneratePreview(this.TisSourceSelector.Value as Idea);
        }

        public void GeneratePreview(Idea Source)
        {
            this.SourceIdea = Source;   // Update possible change by selector

            try
            {
                var Result = FileGenerator.GenerateFilePreview(Source, this.SourceTemplateText, true);

                this.TxbPreview.Text = Result.GeneratedText;
                this.PreviewName = Result.FileName;
                this.PreviewFileExtension = Path.GetExtension(Result.FileName);

                if (Source.GetType() == this.ExpectedSourceIdeaKind
                    && (this.ExpectedSourceIdeaDefinition == null || (this.ExpectedSourceIdeaDefinition.IsEqual(Source.IdeaDefinitor))))
                {
                    this.TxtStatus.Background = Brushes.LightGreen;
                    this.TxtStatus.Text = "Template is correct. The generated text is:";
                }
                else
                {
                    this.TxtStatus.Background = Brushes.LightYellow;
                    this.TxtStatus.Text = "Template is correct, but was tested against a " +
                                          Source.GetType().Name + 
                                          (this.ExpectedSourceIdeaDefinition == null
                                           ? ""
                                           : " ('" + Source.IdeaDefinitor.Name + "')") +
                                          ". The generated text is:";
                }

                this.BtnSaveTo.IsEnabled = true;
            }
            catch (Exception Problem)
            {
                this.TxbPreview.Text = Problem.Message;

                this.TxtStatus.Background = Brushes.Tomato;
                this.TxtStatus.Text = "Template is invalid or cannot be tested. Detected error:";
                this.BtnSaveTo.IsEnabled = false;
            }
        }

        private void BtnSaveTo_Click(object sender, RoutedEventArgs e)
        {
            var Filter = this.PreviewName + " files (*" + this.PreviewFileExtension + ")|*" + this.PreviewFileExtension + "|All files (*.*)|*.*";
            var Location = Display.DialogGetSaveFile("Save " + this.PreviewName + " to...", this.PreviewFileExtension, Filter,
                                                     this.SourceComposition.ToString() + this.PreviewFileExtension);
            if (Location == null)
                return;

            try
            {
                General.StringToFile(Location.LocalPath, this.TxbPreview.Text);
            }
            catch (Exception Problem)
            {
                Display.DialogMessage("Attention!", "Cannot save file.\nProblem: " + Problem.Message, EMessageType.Error);
                return;
            }
        }

        /* private void BtnOK_Click(object sender, RoutedEventArgs e)
        {

        } */

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            var ParentWindow = this.GetNearestVisualDominantOfType<Window>();
            if (ParentWindow == null)
                return;

            ParentWindow.Close();
        }
    }
}
