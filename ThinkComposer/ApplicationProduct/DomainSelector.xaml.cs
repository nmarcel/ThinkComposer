using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.Visualization;
using Instrumind.Common.Visualization.Widgets;

using Instrumind.ThinkComposer.MetaModel;

namespace Instrumind.ThinkComposer.ApplicationProduct
{
    /// <summary>
    /// Interaction logic for DomainSelector.xaml
    /// </summary>
    public partial class DomainSelector : UserControl
    {
        /// <summary>
        /// Allows the user to select a Domain for create a new Composition.
        /// Returns the Domain Location plus indication of use Domain Composition as template (else empty).
        /// </summary>
        public static Tuple<Uri, bool> SelectDomain(string SourceFolder, string Title = null, bool ShowUseTemplate = true)
        {
            DialogOptionsWindow Dialog = null;

            DisplayUseTemplate = ShowUseTemplate;
            Folder = SourceFolder;
            Location = null;
            UseTemplate = false;

            if (!Display.OpenContentDialogWindow<DomainSelector>(ref Dialog, Title.NullDefault("Select Domain...")).IsTrue())
                return null;

            var Result = Tuple.Create(Location, UseTemplate);
            return Result;
        }

        public static IEnumerable<DocumentFileInfo> DetectDomainsInFolder(string SourceFolder)
        {
            Folder = SourceFolder;

            var DetectedFiles = Directory.GetFiles(Folder, "*." + Domain.FILE_EXTENSION_DOMAIN,
                                                   SearchOption.TopDirectoryOnly);

            foreach (var DetectedFile in DetectedFiles)
            {
                var DocumentInfo = DocumentEngine.GetDocumentFileInfoFrom(DetectedFile);
                if (DocumentInfo != null)
                    yield return DocumentInfo;
                else
                    Console.WriteLine("Cannot analyze Domain file '{0}'.", DetectedFile);
            }
        }

        static bool DisplayUseTemplate = true;
        public static string Folder { get; protected set; }
        public static Uri Location { get; protected set; }
        public static bool UseTemplate { get; protected set; }

        public DomainSelector()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            /* OLD screen size adjust */
            var OwnerWindow = this.GetNearestVisualDominantOfType<DialogOptionsWindow>();

            OwnerWindow.ExplicitMaxWidth = (Application.Current.MainWindow.ActualWidth * 0.9).EnforceMinimum(this.MinWidth * 1.1);
            OwnerWindow.ExplicitMaxHeight = (Application.Current.MainWindow.ActualHeight * 0.9).EnforceMinimum(this.MinHeight * 1.3);

            this.CbxUseTemplate.IsChecked = UseTemplate;
            this.CbxUseTemplate.SetVisible(DisplayUseTemplate);
            
            var Files = ShowFiles();
        }

        private int ShowFiles()
        {
            this.TxtLocation.Text = Folder;

            var Files = DetectDomainsInFolder(Folder).ToArray();
            if (Files.Any())
            {
                this.TxtSelectorBackground.Text = "";

                Files = Files.OrderBy(domf => domf.FileName /* Most recent: domf.SortKey*/ ).ToArray();
            }
            else
                this.TxtSelectorBackground.Text = "No Domains were found in the folder.";

            this.LbxSelector.ItemsSource = Files;

            return Files.Length;
        }

        private void LbxSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count < 1)
                return;

            var Selection = e.AddedItems[0] as DocumentFileInfo;

            //T Display.DialogMessage("Selected", Selection.FileName);

            var Route = Path.Combine(Folder, Selection.FileLocation);
            Location = new Uri(Route, UriKind.Absolute);

            var ParentWindow = Display.GetCurrentWindow();
            ParentWindow.DialogResult = true;
            ParentWindow.Close();
        }

        private void TxtLocation_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BtnSelectFolder_Click(null, null);
        }

        private void BtnGotoPredefDomsFolder_Click(object sender, RoutedEventArgs e)
        {
            Folder = AppExec.ApplicationSpecificDefinitionsDirectory;
            ShowFiles();
        }

        private void BtnGotoUserDocsFolder_Click(object sender, RoutedEventArgs e)
        {
            Folder = AppExec.UserDataDirectory;
            ShowFiles();
        }

        private void BtnSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            var Result = Display.DialogGetOpenFolder("Select folder with stored Domains...", TxtLocation.Text);
            if (Result == null)
                return;

            Folder = Result.LocalPath;
            ShowFiles();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Display.GetCurrentWindow().Close();
        }

        private void BtnBasic_Click(object sender, RoutedEventArgs e)
        {
            var ParentWindow = Display.GetCurrentWindow();
            Location = null;    // Meaning 'use the basic domain'.
            ParentWindow.DialogResult = true;
            ParentWindow.Close();
        }

        private void CbxUseTemplate_Checked(object sender, RoutedEventArgs e)
        {
            UseTemplate = this.CbxUseTemplate.IsChecked.IsTrue();
        }
    }
}
