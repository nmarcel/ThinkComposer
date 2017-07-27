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

namespace Instrumind.Common.Visualization.Widgets
{
    /// <summary>
    /// Interaction logic for FlowDocumentViewer.xaml
    /// </summary>
    public partial class FlowDocumentViewer : UserControl
    {
        public FlowDocument SourceDocument { get; protected set; }
        public string DocumentTitle { get; protected set; }
        public bool AllowFindText { get; set; }

        public FlowDocumentViewer()
        {
            InitializeComponent();
        }

        public FlowDocumentViewer(FlowDocument SourceDocument, string DocumentTitle, bool AllowFindText = false)
             : this()
        {
            this.SourceDocument = SourceDocument;
            this.DocumentTitle = DocumentTitle;
            this.AllowFindText = AllowFindText;

            this.DocumentViewer.Document = this.SourceDocument;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.AllowFindText)
                return;

            /*var DocContentControl = this.DocumentViewer.Template.FindName("PART_FindToolBarHost", this.DocumentViewer) as ContentControl;
            DocContentControl.Visibility = Visibility.Collapsed;*/
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            this.DocumentViewer.Print();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            var ParentWindow = this.GetNearestDominantOfType<Window>();
            ParentWindow.Close();
        }
    }
}
