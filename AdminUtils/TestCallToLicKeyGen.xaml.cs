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

namespace AdminUtils
{
    /// <summary>
    /// Interaction logic for TestCallToLicKeyGen.xaml
    /// </summary>
    public partial class TestCallToLicKeyGen : UserControl
    {
        public TestCallToLicKeyGen()
        {
            InitializeComponent();

            this.TxbTargetAddress.Text = "http://instrumind.cloudapp.net/Licensing/GenerateLicenseKey/";
        }

        private void BtnCall_Click(object sender, RoutedEventArgs e)
        {
            var Caller = new WebPostSubmitter();
            Caller.Url = this.TxbTargetAddress.Text;

            // PENDING: Get user-entered fields when available
            Caller.PostItems.Add("PID", "4548262");
            Caller.PostItems.Add("PCODE", "IMTC");
            Caller.PostItems.Add("INFO", "");
            Caller.PostItems.Add("REFNO", "1234567");
            Caller.PostItems.Add("REFNOEXT", "");
            Caller.PostItems.Add("PSKU", "");
            Caller.PostItems.Add("TESTORDER", "YES");
            Caller.PostItems.Add("QUANTITY", "7");
            Caller.PostItems.Add("FIRSTNAME", "John");
            Caller.PostItems.Add("LASTNAME", "Doe");
            Caller.PostItems.Add("COMPANY", "ACME Inc.");
            Caller.PostItems.Add("EMAIL", "contact@instrumind.com");
            Caller.PostItems.Add("PHONE", "0123456789");
            Caller.PostItems.Add("FAX", "");
            Caller.PostItems.Add("LANG", "");
            Caller.PostItems.Add("COUNTRY", "United States of America");
            Caller.PostItems.Add("COUNTRY_CODE", "US");
            Caller.PostItems.Add("CITY", "Mountain View");
            Caller.PostItems.Add("ZIPCODE", "1234");
            Caller.PostItems.Add("PRODUCT_OPTIONS_4548262_TEXT[][0]", "Commercial");
            Caller.PostItems.Add("PRODUCT_OPTIONS_4548262_TEXT[][1]", "Professional");
            Caller.PostItems.Add("PRODUCT_OPTIONS_4548262_VALUE[][0]", "TYPEDU");
            Caller.PostItems.Add("PRODUCT_OPTIONS_4548262_VALUE[][1]", "EDISTD");
            Caller.PostItems.Add("PRODUCT_OPTIONS_4548262_PRICE[][0]", "79.5");
            Caller.PostItems.Add("PRODUCT_OPTIONS_4548262_PRICE[][1]", "119.25");
            Caller.PostItems.Add("PRODUCT_OPTIONS_4548262_OPERATOR[][0]", "+");
            Caller.PostItems.Add("PRODUCT_OPTIONS_4548262_OPERATOR[][1]", "+");
            Caller.PostItems.Add("HASH", "60eeb25f2d9014cc8aac5ca3ad07c451");

            Caller.Kind = WebPostSubmitter.EPostKind.Post;

            this.Cursor = Cursors.Wait;
            var Result = Caller.Post();
            this.Cursor = Cursors.Arrow;

            this.TxbResponse.Text = (Result.Item1 ? "OK:" : "Error:") + Environment.NewLine +
                                    Result.Item2;
        }
    }
}
