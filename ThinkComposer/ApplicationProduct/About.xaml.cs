using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Instrumind.ThinkComposer.ApplicationProduct
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : UserControl
    {
        public About()
        {
            InitializeComponent();

            this.ProductText.Text = this.ProductText.Text.Replace("[VERSION]", ProductDirector.APPLICATION_VERSION);
            this.AuthorText.Text = this.AuthorText.Text.Replace("[YEAR]", ProductDirector.APPLICATION_COPYRIGHT_YEARS);
            this.AuthorText.Text = this.AuthorText.Text.Replace("[AUTHOR]", Company.NAME_LEGAL);

            this.TxtWebsiteCompany.Text = Company.WEBSITE_URL;
            // this.TxtEMailContact.Text = Company.CONTACT_EMAIL;
            // this.TxtEMailSupport.Text = Company.SUPPORT_EMAIL;

            // this.TxtLicensedUser.Text = AppExec.CurrentLicenseUserName;
            // this.TxtLicenseType.Text = AppExec.CurrentLicenseType.Name;
            // this.TxtLicenseEdition.Text = AppExec.CurrentLicenseEdition.Name;
            // this.TxtLicenseMode.Text = AppExec.CurrentLicenseMode.Name;
            // this.TxtLicenseExpiration.Text = (AppExec.CurrentLicenseExpiration == General.EMPTY_DATE ? "NEVER" : AppExec.CurrentLicenseExpiration.ToShortDateString());
            this.TxtLicenseAgreement.Text = AppExec.LicenseAgreement;

            this.PostCall(about => about.GetNearestVisualDominantOfType<Window>().ResizeMode = ResizeMode.NoResize);
        }

        private void ProductText_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount < 1)
                return;

            AppExec.CallExternalProcess(ProductDirector.WEBSITE_URL);
        }

        private void AckIconLibrary_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            AppExec.CallExternalProcess("http://www.famfamfam.com/lab/icons/silk/");
        }

        private void AckIconLibrary2_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            AppExec.CallExternalProcess("http://www.damieng.com/icons/silkcompanion");
        }

        private void TxtWebsiteCompany_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            AppExec.CallExternalProcess(this.TxtWebsiteCompany.Text);
        }
        /*
        private void TxtEMailContact_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            AppExec.CallExternalProcess("mailto:" + this.TxtEMailContact.Text + "?subject=Customer commercial inquiry.");
        }

        private void TxtEMailSupport_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            AppExec.CallExternalProcess("mailto:" + this.TxtEMailSupport.Text + "?subject=Customer support request.");
        }

        private void TxtWebsiteProduct_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            AppExec.CallExternalProcess(this.TxtWebsiteProduct.Text);
        }*/

        private void BtnCheckForNewRelease_Click(object sender, RoutedEventArgs e)
        {
            AppVersionUpdate.ShowAppVersionUpdate();
        }
    }
}
