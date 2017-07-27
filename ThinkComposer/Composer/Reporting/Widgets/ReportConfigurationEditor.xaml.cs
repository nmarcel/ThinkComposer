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

namespace Instrumind.ThinkComposer.Composer.Reporting.Widgets
{
    /// <summary>
    /// Interaction logic for ReportConfigurationEditor.xaml
    /// </summary>
    public partial class ReportConfigurationEditor : UserControl
    {
        public ReportConfigurationEditor()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var EditPanel = this.GetNearestVisualDominantOfType<EntityEditPanel>();
            if (EditPanel == null)
                return;

            EditPanel.BtnOK.ButtonText = "Generate!";
            EditPanel.BtnOK.ToolTip = "Generates the Report based on the configured parameters and saves them.";
            EditPanel.BtnAdvanced.SetVisible(false);
            EditPanel.ShowAdvancedMembers = true;
        }

        private void TextFormatSampler_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var Source = sender as TextFormatSampler;
            if (Source == null || CompositionEngine.ActiveEntityEditor == null)
                return;

            var Result = TextFormatSelector.SelectFor(Source.Format);
            if (Result == null || !Result.Item1)
                return;

            CompositionEngine.ActiveEntityEditor.StartCommandVariation("Change Text Format");

            Source.Format = Result.Item2;

            CompositionEngine.ActiveEntityEditor.CompleteCommandVariation();
        }

        private void CbxRelatedFrom_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!CbxRelatedFrom.IsChecked.IsTrue())
                this.CbxIncludeTargetCompanions.IsChecked = false;
        }

        private void CbxIncludeTargetCompanions_Checked(object sender, RoutedEventArgs e)
        {
            if (CbxIncludeTargetCompanions.IsChecked.IsTrue())
                this.CbxRelatedFrom.IsChecked = true;
        }

        private void CbxRelatedTo_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!CbxRelatedTo.IsChecked.IsTrue())
                this.CbxIncludeOriginCompanions.IsChecked = false;
        }

        private void CbxIncludeOriginCompanions_Checked(object sender, RoutedEventArgs e)
        {
            if (CbxIncludeOriginCompanions.IsChecked.IsTrue())
                this.CbxRelatedTo.IsChecked = true;
        }

        private void CbxDetails_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!CbxDetails.IsChecked.IsTrue())
            {
                this.CbxDetailsIncludeLinksTarget.IsChecked = false;
                this.CbxDetailsIncludeAttachmentsContent.IsChecked = false;
                this.CbxDetailsIncludeTablesData.IsChecked = false;
            }
        }

        private void CbxDetailsInclusion_Checked(object sender, RoutedEventArgs e)
        {
            if (CbxDetailsIncludeLinksTarget.IsChecked.IsTrue()
                || CbxDetailsIncludeAttachmentsContent.IsChecked.IsTrue()
                || CbxDetailsIncludeTablesData.IsChecked.IsTrue())
                this.CbxDetails.IsChecked = true;
        }
    }
}
