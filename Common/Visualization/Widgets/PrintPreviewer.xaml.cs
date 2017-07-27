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
using System.IO;

namespace Instrumind.Common.Visualization.Widgets
{
    /// <summary>
    /// Interaction logic for PrintPreviewer.xaml
    /// </summary>
    public partial class PrintPreviewer : UserControl
    {
        public FixedDocument SourceDocument { get; protected set; }
        public string SourceDocumentPath { get; protected set; }

        public string DocumentTitle { get; protected set; }
        public bool AllowFindText { get; set; }

        public PrintPreviewer()
        {
            InitializeComponent();
        }

        public PrintPreviewer(FixedDocument SourceDocument, string SourceDocumentPath,
                              string DocumentTitle, bool AllowFindText = false)
            : this()
        {
            this.SourceDocument = SourceDocument;
            this.SourceDocumentPath = SourceDocumentPath;

            this.DocumentTitle = DocumentTitle;
            this.AllowFindText = AllowFindText;

            this.DocumentViewer.Document = this.SourceDocument;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.BtnSaveAsPDF.SetVisible(Display.CanConvertXpsToPdf);

            this.DocumentViewer.PostCall(docv => docv.Focus());

            if (this.AllowFindText)
                return;

            var DocContentControl = this.DocumentViewer.Template.FindName("PART_FindToolBarHost", this.DocumentViewer) as ContentControl;
            DocContentControl.Visibility = Visibility.Collapsed;
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

        private void BtnSaveAsXPS_Click(object sender, RoutedEventArgs e)
        {
            var Location = Display.DialogGetSaveFile("Save as XPS", "xps", "XPS document (*.xps)|*.xps", this.DocumentTitle);
            if (Location == null)
                return;

            string Result = null;

            if (File.Exists(this.SourceDocumentPath))
                try
                {
                    File.Copy(this.SourceDocumentPath, Location.LocalPath, true);
                }
                catch (Exception Problem)
                {
                    Result = Problem.Message;
                }
            else
                Result = Display.SaveDocumentAsXPS(this.SourceDocument, Location.LocalPath);

            if (!Result.IsAbsent())
                Display.DialogMessage("Attention!", "Cannot save document as XPS.\nProblem: " + Result, EMessageType.Error);
        }

        private void BtnSaveAsPDF_Click(object sender, RoutedEventArgs e)
        {
            var Location = Display.DialogGetSaveFile("Save as PDF", ".pdf", "PDF document (*.pdf)|*.pdf", this.DocumentTitle);
            if (Location == null)
                return;

            string Result = null;

            try
            {
                var TemporalCreation = !File.Exists(this.SourceDocumentPath);
                if (TemporalCreation)
                {
                    this.SourceDocumentPath = Path.Combine(AppExec.ApplicationUserTemporalDirectory, General.GenerateRandomFileName("TMP_", "xps"));
                    Result = Display.SaveDocumentAsXPS(this.SourceDocument, this.SourceDocumentPath);
                }

                if (Result.IsAbsent())
                    Result = Display.ConvertXPStoPDF(this.SourceDocumentPath, Location.LocalPath);

                if (TemporalCreation && File.Exists(this.SourceDocumentPath))
                    File.Delete(this.SourceDocumentPath);
            }
            catch (Exception Problem)
            {
                Result = Problem.Message;
            }

            if (!Result.IsAbsent())
                Display.DialogMessage("Attention!", "Cannot save document as PDF.\nProblem: " + Result, EMessageType.Error);
        }
    }
}
