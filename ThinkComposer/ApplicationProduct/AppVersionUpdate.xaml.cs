using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.Visualization;
using Instrumind.Common.Visualization.Widgets;

namespace Instrumind.ThinkComposer.ApplicationProduct
{
    /// <summary>
    /// Interaction logic for AppVersionUpdate.xaml
    /// </summary>
    public partial class AppVersionUpdate : UserControl
    {
        public bool IsWorking { get; set; }
        public bool IsCancelled { get; set; }
        public string VersionFileText { get; protected set; }
        public IList<string> VersionFileLines { get; protected set; }

        public static void ShowAppVersionUpdate(string VersionFileText = null)
        {
            DialogOptionsWindow Dialog = null;
            Display.OpenContentDialogWindow<AppVersionUpdate>(ref Dialog, "Version Update", null, double.NaN, double.NaN, VersionFileText);
        }

        public AppVersionUpdate(string VersionFileText)
        {
            this.VersionFileText = VersionFileText;

            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.TxtProduct.Text = ProductDirector.APPLICATION_NAME;
            this.TxtCurrentVersion.Text = ProductDirector.APPLICATION_VERSION;

            this.IsCancelled = false;
            this.IsWorking = true;
            this.BrdUpdate.SetVisible(false, false);
            this.PgsStatus.Value = 0;

            // When not called from app-start (which already detected, downloaded and read the version file)
            if (this.VersionFileText == null)
                try
                {
                    if (File.Exists(ProductDirector.VersioningSourceLocal))
                        File.Delete(ProductDirector.VersioningSourceLocal);

                    General.DownloadFileAsync(new Uri(ProductDirector.VersioningSourceRemote, UriKind.Absolute),
                                              ProductDirector.VersioningSourceLocal, evargs => true, result => DoDetect(result));
                }
                catch (Exception Problem)
                {
                    Display.DialogMessage("Error!", "Cannot perform detection of new version.\n\nProblem: " + Problem.Message);
                }
            else
            {
                // Beign there also means that the update was accepted by user
                this.VersionFileLines = General.ToStrings(this.VersionFileText);
                this.TxtNewestVersion.Text = this.VersionFileLines[0].Substring(1);
                this.TxbChanges.Text = VersionFileText;

                this.BtnUpdate_Click(this, null);
            }
        }

        private void DoDetect(OperationResult<bool> DetectionResult)
        {
            this.TxtStatus.Foreground = Brushes.White;

            try
            {
                if (DetectionResult.WasSuccessful)
                {
                    this.TxtStatus.Background = Brushes.Green;
                    this.TxtStatus.Text = "Detection complete.";

                    var VersionFileText = General.FileToString(ProductDirector.VersioningSourceLocal);
                    this.VersionFileLines = General.ToStrings(VersionFileText);
                    this.TxtNewestVersion.Text = this.VersionFileLines[0].Substring(1).Trim();
                    this.TxbChanges.Text = VersionFileText;

                    if (ProductDirector.ProductUpdateIsNeeded(this.VersionFileLines))
                    {
                        this.BrdUpdate.SetVisible(true);
                        this.TxtStatus.Text = "There is a new version available!  Update when ready.";
                    }
                    else
                        this.TxtStatus.Text = "You are using the latest version available. No update required.";
                }
                else
                {
                    this.TxtStatus.Background = Brushes.Red;
                    this.TxtStatus.Text = "Detection failed. Try again later.";
                }
            }
            catch (Exception Problem)
            {
                this.TxtStatus.Background = Brushes.Red;
                this.TxtStatus.Text = "Detection failed. Cannot process request.";
                Display.DialogMessage("Error!", "Detection failed.\n\nProblem: " + Problem.Message);
            }

            this.PgsStatus.Value = 0;
            this.IsWorking = false;
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (!ProductDirector.AppTerminationConfirmation())
                return;

            this.IsCancelled = false;
            this.IsWorking = true;
            this.BrdUpdate.SetVisible(false, false);
            this.PgsStatus.Value = 0;

            this.TxtStatus.Background = Brushes.Blue;
            this.TxtStatus.Foreground = Brushes.White;
            this.TxtStatus.Text = "Downloading new version";

            ProductDirector.LaunchNewVersionSetupOnExit = false;

            try
            {
                if (File.Exists(ProductDirector.VersioningSourceLocal))
                    File.Delete(ProductDirector.VersioningSourceLocal);

                General.DownloadFileAsync(new Uri(ProductDirector.SetupSourceRemote, UriKind.Absolute),
                                          ProductDirector.SetupSourceLocal,
                                          evargs =>
                                          {
                                              this.PgsStatus.Value = evargs.ProgressPercentage;
                                              return !this.IsCancelled;
                                          },
                                          result =>
                                          {
                                              this.TxtStatus.Foreground = Brushes.White;
                                              this.IsWorking = false;

                                              if (result.WasSuccessful)
                                              {
                                                  this.TxtStatus.Background = Brushes.Green;
                                                  this.TxtStatus.Text = "Download complete.";

                                                  /* var Result = Display.DialogMessage("Attention", "Now the application will be closed and the installer will be started.",
                                                                                     EMessageType.Information, MessageBoxButton.OKCancel, MessageBoxResult.OK);
                                                  if (Result != MessageBoxResult.OK)
                                                      return; */

                                                  ProductDirector.LaunchNewVersionSetupOnExit = true;
                                                  Application.Current.MainWindow.PostCall(wnd => wnd.Close());
                                              }
                                              else
                                              {
                                                  this.TxtStatus.Background = Brushes.Red;
                                                  this.TxtStatus.Text = "Download failed. Try again later.";
                                                  Display.DialogMessage("Error!", "Download failed.\n\nProblem: " + result.Message);
                                              }
                                          });
            }
            catch (Exception Problem)
            {
                Display.DialogMessage("Error!", "Cannot start download and update of new version.\n\nProblem: " + Problem.Message);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!this.IsWorking)
            {
                Display.GetCurrentWindow().Close();
                return;
            }

            this.IsCancelled = true;
        }
    }
}
