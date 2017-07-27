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
using System.Xml.Linq;

namespace Instrumind.ThinkComposer.ApplicationProduct
{
    /// <summary>
    /// Interaction logic for AppOptions.xaml
    /// </summary>
    public partial class AppRegistration : UserControl
    {
        public AppRegistration()
        {
            InitializeComponent();

            // this.BtnTest.SetVisible(false);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.PostCall(about => about.GetNearestVisualDominantOfType<Window>().ResizeMode = ResizeMode.NoResize);

            this.TxtLicensedUser.Text = AppExec.CurrentLicenseUserName;
            this.TxtLicType.Text = AppExec.CurrentLicenseType.Name;
            this.TxtLicEdition.Text = AppExec.CurrentLicenseEdition.Name;
            this.TxtLicMode.Text = AppExec.CurrentLicenseMode.Name;
            this.TxtLicExpiration.Text = (AppExec.CurrentLicenseExpiration == General.EMPTY_DATE ? "NEVER" : AppExec.CurrentLicenseExpiration.ToShortDateString());
        }

        /* private void BtnAccept_Click(object sender, RoutedEventArgs e)
        {
            var Registration = Licensing.ValidateAndRegisterLicense("", "", "", "");

            if (Registration == null)
                return;

            Display.DialogMessage("Attention!", "Product license has been successfully registered!\n" +
                                                "Type: " + AppExec.LicenseTypes.GetByTechName(Registration.Item1).Name +
                                                ", Edition:" + AppExec.LicenseEditions.GetByTechName(Registration.Item2).Name,
                                                EMessageType.Information);

            var SelectedDocument = ProductDirector.WorkspaceDirector.ShellProvider.MainSelector.SelectedItem as ISphereModel;
            ProductDirector.EntitleApplication(SelectedDocument.ToStringAlways());

            Display.GetCurrentWindow().Close();
        } */

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Display.GetCurrentWindow().Close();
        }

        private void TxtLocation_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.BtnSelectFile_Click(null, null);
        }

        private void BtnSelectFile_Click(object sender, RoutedEventArgs e)
        {
            var FileLocation = Display.DialogGetOpenFile("Select License-Key file", "." + AppExec.LICENSE_EXTENSION,
                                                         "License files (*." + AppExec.LICENSE_EXTENSION + ")|*." + AppExec.LICENSE_EXTENSION);
            if (FileLocation == null)
                return;

            try
            {
                this.TxtLocation.Text = FileLocation.LocalPath;
                this.TxtKeyCode.Text = General.FileToString(FileLocation.LocalPath);
            }
            catch (Exception Problem)
            {
                Display.DialogMessage("Attention!", "Cannot load file.\nProblem: " + Problem.Message, EMessageType.Error);
                return;
            }

            ProcessRegistration();
        }

        private void BtnPasteFromClipboard_Click(object sender, RoutedEventArgs e)
        {
            this.TxtKeyCode.Text = General.Execute(() => Clipboard.GetText().ToStringAlways(),
                                                   "Cannot access Windows Clipboard!").Result.ToStringAlways();
            this.TxtLocation.Text = "";

            ProcessRegistration();
        }

        public void ProcessRegistration()
        {
            try
            {
                var Document = Licensing.LicenseKeyCodeToDocument(this.TxtKeyCode.Text);
                var License = Licensing.ExtractValidLicense(Document, AppExec.ApplicationVersionMajorNumber);

                AppExec.CurrentLicenseUserName = License.Item1;
                AppExec.CurrentLicenseType = AppExec.LicenseTypes.GetByTechName(License.Item2);
                AppExec.CurrentLicenseEdition = AppExec.LicenseEditions.GetByTechName(License.Item3);
                AppExec.CurrentLicenseMode = AppExec.LicenseModes.GetByTechName(License.Item4);
                AppExec.CurrentLicenseExpiration = License.Item5;

                General.StringToFile(AppExec.LicenseFilePath, this.TxtKeyCode.Text);

                this.TxtLicensedUser.Text = AppExec.CurrentLicenseUserName;
                this.TxtLicType.Text = AppExec.CurrentLicenseType.Name;
                this.TxtLicEdition.Text = AppExec.CurrentLicenseEdition.Name;
                this.TxtLicMode.Text = AppExec.CurrentLicenseMode.Name;
                this.TxtLicExpiration.Text = (AppExec.CurrentLicenseExpiration == General.EMPTY_DATE ? "NEVER" : AppExec.CurrentLicenseExpiration.ToShortDateString());

                this.BrdLicense.Background = Brushes.Honeydew;

                Display.DialogMessage("Attention!", "Product License has been successfully registered!",
                                                    EMessageType.Information);
            }
            catch (Exception Problem)
            {
                Display.DialogMessage("Attention!", "Cannot validate and register License.\nProblem: " + Problem.Message,
                                      EMessageType.Error);
            }

            //? Display.GetCurrentWindow().Close();
        }

        private void BtnBuyLicense_Click(object sender, RoutedEventArgs e)
        {
            AppExec.CallExternalProcess(ProductDirector.WEBSITE_URL + ProductDirector.WEBSITE_URLPAGE_BUY +
                                        "?LicType=" + AppExec.CurrentLicenseType.TechName +
                                        "&LicEdition=" + AppExec.CurrentLicenseEdition);
        }
    }
}
