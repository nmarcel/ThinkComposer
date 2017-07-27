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
using Instrumind.ThinkComposer.Model.VisualModel;
using Instrumind.Common.EntityBase;
using Instrumind.ThinkComposer.Model.GraphModel;

namespace Instrumind.ThinkComposer.Composer.ComposerUI.Widgets
{
    /// <summary>
    /// Interaction logic for FindAndReplaceDialog.xaml
    /// </summary>
    public partial class FindAndReplaceDialog : UserControl
    {
        public bool CanReplace { get; protected set; }
        public IIdentifiableElement StartRoot { get; protected set; }

        public static void Find(bool CanReplace, IIdentifiableElement StartRoot)
        {
            DialogOptionsWindow Dialog = null;
            Display.OpenContentDialogWindow<FindAndReplaceDialog>(ref Dialog, "Find", null, double.NaN, double.NaN, CanReplace, StartRoot);
        }

        public FindAndReplaceDialog(bool CanReplace, IIdentifiableElement StartRoot)
        {
            this.CanReplace = CanReplace;
            this.DataContext = this;

            InitializeComponent();

            if (!this.CanReplace)
            {
                this.LsvResults.SelectionMode = SelectionMode.Single;
                this.LsvResults.Cursor = Cursors.Hand;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var ParentWindow = Display.GetCurrentWindow();
            if (ParentWindow != null && this.CanReplace)
                ParentWindow.Title = "Find and Replace";

            this.TxtFind.Focus();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            FoundObjectHint.ClearTemporalDocuments();
        }

        private void BtnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            this.LsvResults.SelectAll();
        }

        private void BtnUnselectAll_Click(object sender, RoutedEventArgs e)
        {
            this.LsvResults.UnselectAll();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Display.GetCurrentWindow().Close();
        }

        private void BtnFind_Click(object sender, RoutedEventArgs e)
        {
            //T Console.WriteLine("Find!");
            var Engine = ApplicationProduct.ProductDirector.WorkspaceDirector.ActiveDocumentEngine as CompositionEngine;
            if (Engine == null || this.TxtFind.Text.IsAbsent())
                return;

            this.SearchedText = this.TxtFind.Text;

            try
            {
                var Results = Engine.FindTextInObjects(this.SearchedText,
                                                       this.CbxCaseSensitive.IsChecked.IsTrue(),
                                                       this.CbxWholeWord.IsChecked.IsTrue(),
                                                       this.CbxScopeComposition.IsChecked.IsTrue(),
                                                       this.CbxScopeDomain.IsChecked.IsTrue(),
                                                       this.CbxIncludeConcepts.IsChecked.IsTrue(),
                                                       this.CbxIncludeRelationships.IsChecked.IsTrue(),
                                                       this.CbxIncludeMarkers.IsChecked.IsTrue(),
                                                       this.CbxIncludeOther.IsChecked.IsTrue(),
                                                       this.CbxPropName.IsChecked.IsTrue(),
                                                       this.CbxPropTechName.IsChecked.IsTrue(),
                                                       this.CbxPropSummary.IsChecked.IsTrue(),
                                                       this.CbxPropDescription.IsChecked.IsTrue(),
                                                       this.CbxPropDetDesignat.IsChecked.IsTrue(),
                                                       this.CbxPropDetContent.IsChecked.IsTrue(),
                                                       this.CbxPropTechSpec.IsChecked.IsTrue());

                this.LsvResults.ItemsSource = Results;

                this.TxtCount.Text = Results.Count.ToString();

                this.BtnReplace.IsEnabled = (this.CanReplace && Results.Count > 0);
            }
            catch (Exception Problem)
            {
                Display.DialogMessage("Warning!", "Cannot Find text due to an internal error.\n\nProblem: "
                                                  + Problem.Message, EMessageType.Warning);
                AppExec.LogException(Problem, "FindAndReplaceDialog.Find");
            }
        }

        private string SearchedText = null; // Saved from possible user change between find and replace actions.

        private void LsvResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.CanReplace || this.SearchedText == null
                || e.AddedItems == null || e.AddedItems.Count < 1)
                return;

            var Hint = e.AddedItems[0] as FoundObjectHint;

            var Engine = ApplicationProduct.ProductDirector.WorkspaceDirector.ActiveDocumentEngine as CompositionEngine;
            if (Engine == null)
                return;

            if (!Engine.GoToObject(Hint.SourceObject))
                Display.DialogMessage("Sorry!",
                                      "Cannnot automatically go to '" + Hint.SourceObject.ToStringAlways() + "' in:\n\n"
                                      + Hint.LocationPath);
        }

        private void BtnReplace_Click(object sender, RoutedEventArgs e)
        {
            var Engine = ApplicationProduct.ProductDirector.WorkspaceDirector.ActiveDocumentEngine as CompositionEngine;
            if (Engine == null)
                return;

            try
            {
                var Hints = this.LsvResults.SelectedItems.Cast<FoundObjectHint>();

                if (!Hints.Any())
                {
                    Display.DialogMessage("Attention!", "Must select at least one object to be replaced.");
                    return;
                }

                if (Engine.ReplaceTextOfFoundObjects(Hints, this.SearchedText, this.TxtReplace.Text))
                {
                    ApplicationProduct.ProductDirector.ContentTreeControl.Refresh();
                    Display.GetCurrentWindow().Close();
                }
            }
            catch (Exception Problem)
            {
                Display.DialogMessage("Warning!", "Cannot Replace text due to an internal error.\n\nProblem: "
                                                  + Problem.Message, EMessageType.Warning);
                AppExec.LogException(Problem, "FindAndReplaceDialog.Replace");
            }

        }
    }
}
