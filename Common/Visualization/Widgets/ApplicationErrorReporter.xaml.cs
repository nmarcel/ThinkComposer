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

using Instrumind.Common;

namespace Instrumind.Common.Visualization.Widgets
{
    /// <summary>
    /// Interaction logic for ApplicationErrorReporter.xaml
    /// </summary>
    public partial class ApplicationErrorReporter : UserControl
    {
        public Exception SourceException { get; protected set; }
        public string Title { get; protected set; }
        public string Subtitle { get; protected set; }
        public string DetailedReport { get; protected set; }

        public static void Show(Exception Error, string Title, string Subtitle)
        {
            var Reporter = new ApplicationErrorReporter(Error, Title, Subtitle);
            DialogOptionsWindow WinDialog = null;

            Display.OpenContentDialogWindow(ref WinDialog, "Problem...", Reporter);
        }

        public ApplicationErrorReporter()
        {
            InitializeComponent();
        }

        public ApplicationErrorReporter(Exception SourceError, string Title, string Subtitle)
            : this()
        {
            this.DataContext = this;

            this.SourceException = SourceError;
            this.Title = Title;
            this.Subtitle = Subtitle;
            this.DetailedReport = AppExec.GenerateDetailedErrorInfo(this.SourceException);

            // this.BtnCopyToClipboard.ToolTip = this.BtnCopyToClipboard.ToolTip.ToStringAlways().Replace("[SUPPORT-EMAIL]", Company.SUPPORT_EMAIL);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var ParentWindow = this.GetNearestDominantOfType<Window>();
            ParentWindow.ResizeMode = ResizeMode.NoResize;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            var ParentWindow = this.GetNearestDominantOfType<Window>();
            ParentWindow.Close();
        }

        private void BtnSendReport_Click(object sender, RoutedEventArgs e)
        {
            var ParentWindow = this.GetNearestDominantOfType<Window>();
            ParentWindow.Cursor = Cursors.Wait;
            /*
            var SendingResult = General.SendMailTo(Company.SUPPORT_EMAIL, "Error Report", this.DetailedReport);

            ParentWindow.Cursor = Cursors.Arrow;

            if (!SendingResult.IsAbsent())
                Display.DialogMessage("Error!", "Cannot send e-mail via Outlook.\n\nProblem: " + SendingResult, EMessageType.Warning); */
        }

        private void BtnCopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            General.Execute(() => Clipboard.SetText(this.DetailedReport), "Cannot access Windows Clipboard!");
        }

    }
}
