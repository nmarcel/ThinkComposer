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
using System.Globalization;
using System.Security.Cryptography;

namespace AdminUtils
{
    /// <summary>
    /// Interaction logic for LicenseGenerator.xaml
    /// </summary>
    public partial class LicenseGenerator : UserControl
    {
        private string PrivateKey = null;

        public LicenseGenerator()
        {
            InitializeComponent();

            AppExec.ApplicationVersion = "1.0";
            AppExec.ApplicationName = "ThinkComposer";

            this.CbxType.ItemsSource = AppExec.LicenseTypes;
            this.CbxType.DisplayMemberPath = SimpleElement.__Name.TechName;
            this.CbxType.DisplayMemberPath = SimpleElement.__Name.TechName;
            this.CbxType.SelectedIndex = 0;

            this.CbxEdition.ItemsSource = AppExec.LicenseEditions;
            this.CbxEdition.DisplayMemberPath = SimpleElement.__Name.TechName;
            this.CbxEdition.SelectedIndex = 0;

            this.CbxMode.ItemsSource = AppExec.LicenseModes;
            this.CbxMode.DisplayMemberPath = SimpleElement.__Name.TechName;
            this.CbxMode.SelectedIndex = 0;
        }

        private void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.PrivateKey.IsAbsent())
                {
                    var Location = Display.DialogGetOpenFile("Select Key file...", ".key", "Keys (*.key)|*.key");
                    if (Location == null)
                        return;

                    PrivateKey = General.FileToString(Location.LocalPath);
                }

                // IMPORTANT: Document (XML) and Key Code must be equivalent always to avoid mistakes.
                var LicenseDoc = Licensing.CreateLicenseInfoSignedDocument(this.TxtUser.Text,
                                                                           ((SimpleElement)this.CbxType.SelectedItem).TechName,
                                                                           ((SimpleElement)this.CbxEdition.SelectedItem).TechName,
                                                                           ((SimpleElement)this.CbxMode.SelectedItem).TechName,
                                                                           DateTime.ParseExact(this.TxtExpiration.Text.Trim(), "yyyyMMdd", CultureInfo.InvariantCulture),
                                                                           AppExec.ApplicationName, AppExec.ApplicationVersionMajorNumber, PrivateKey);

                this.TxtLicenseDocument.Text = LicenseDoc;
            }
            catch (Exception Problem)
            {
                Display.DialogMessage("Error!", "Cannot load key file or generate license document.\nProblem: " + Problem.Message, EMessageType.Error);
            }
        }

        private void BtnValDocAndTransToKey_Click(object sender, RoutedEventArgs e)
        {
            if (!Licensing.SignedDocumentIsValid(this.TxtLicenseDocument.Text))
            {
                Display.DialogMessage("Attention!", "License Document (XML) is not Valid.", EMessageType.Error);
                return;
            }

            // IMPORTANT: Document (XML) and Key Code must be equivalent always to avoid mistakes.
            var Info = Licensing.ExtractValidLicense(this.TxtLicenseDocument.Text, "1");
            this.TxtUser.Text = Info.Item1;
            this.CbxType.SelectedItem = AppExec.LicenseTypes.FirstOrDefault(item => item.TechName == Info.Item2);
            this.CbxEdition.SelectedItem = AppExec.LicenseEditions.FirstOrDefault(item => item.TechName == Info.Item3);
            this.CbxMode.SelectedItem = AppExec.LicenseModes.FirstOrDefault(item => item.TechName == Info.Item4);
            this.TxtExpiration.Text = Info.Item5.ToString("yyyyMMdd");

            this.TxtLicenseKeyCode.Text = Licensing.LicenseDocumentToKeyCode(this.TxtLicenseDocument.Text);

            /*
            var ValBytes = this.TxtLicenseDocument.Text.StringToBytes();
            var ValCompZip = BytesHandling.Compress(ValBytes, true);
            var ValCompDef = BytesHandling.Compress(ValBytes, false);
            var ValBase64Zip = Convert.ToBase64String(ValCompZip);
            var ValBase64Def = Convert.ToBase64String(ValCompDef);

            Display.DialogMessage("Sizes...", "Original  =" + this.TxtLicenseDocument.Text.Length +
                                              "\nBytes UTF8=" + ValBytes.Length +
                                              "\nComp   Zip=" + ValCompZip.Length +
                                              "\nComp   Def=" + ValCompDef.Length +
                                              "\nCompB64Zip=" + ValBase64Zip.Length +
                                              "\nCompB64Def=" + ValBase64Def.Length); */
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // IMPORTANT: Document (XML) and Key Code must be equivalent always to avoid mistakes.
                this.TxtLicenseDocument.Text = Licensing.LicenseKeyCodeToDocument(this.TxtLicenseKeyCode.Text);
            }
            catch (Exception Problem)
            {
                Display.DialogMessage("Error!", "Cannot convert License Key Code to Document. Problem: " + Problem.Message, EMessageType.Error);
            }

            if (!Licensing.SignedDocumentIsValid(this.TxtLicenseDocument.Text))
            {
                Display.DialogMessage("Attention!", "License document is not valid", EMessageType.Error);
                return;
            }

            var Location = Display.DialogGetSaveFile("Save License Key file...", "." + AppExec.LICENSE_EXTENSION,
                                                     "Licenses (*." + AppExec.LICENSE_EXTENSION + ")|*." + AppExec.LICENSE_EXTENSION);
            if (Location == null)
                return;

            try
            {
                General.StringToFile(Location.LocalPath, this.TxtLicenseKeyCode.Text);
            }
            catch (Exception Problem)
            {
                Display.DialogMessage("Error!", "Cannot save License Key file. Problem: " + Problem.Message, EMessageType.Error);
            }
        }
    }
}
