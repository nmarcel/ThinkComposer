// -------------------------------------------------------------------------------------------
// Instrumind ThinkComposer
//
// Copyright (C) 2011-2015 Néstor Marcel Sánchez Ahumada.
// http://thinkcomposer.codeplex.com
//
// This file is part of ThinkComposer, which is free software licensed under the GNU General Public License.
// It is provided without any warranty. You should find a copy of the license in the root directory of this software product.
// -------------------------------------------------------------------------------------------
//
// Project: Instrumind ThinkComposer v1.0
// File   : General.cs
// Object : Instrumind.Common.Licensing (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2012.01.03 Néstor Sánchez A.  Creation
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Documents;
using System.Xml.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;

using Instrumind.Common.Visualization;

/// Library of general purpose components and features across Instrumind products.
namespace Instrumind.Common
{
    /// <summary>
    /// Provides methods for create and validate license key codes.
    /// </summary>
    public static class Licensing
    {
        public const string LIC_REG_LICENSE = "license";
        public const string LIC_FLD_PRODAPP = "prodapp";
        public const string LIC_FLD_PRODVER = "prodver";
        public const string LIC_FLD_USER = "user";
        public const string LIC_FLD_TYPE = "type";
        public const string LIC_FLD_EDITION = "edition";
        public const string LIC_FLD_MODE = "mode";
        public const string LIC_FLD_EXPIRATION = "expiration";

        // public const string LIC_KEYCODE_PREFIX = "[BEGIN-KEY-CODE]";
        // public const string LIC_KEYCODE_SUFFIX = "[END-KEY-CODE]";

        public const string STR_LICENSE_VALIDCHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 ";

        public const char STR_LICENSE_REPLACCHAR = '_';

        public const string LIC_PUBKEY = "<RSAKeyValue><Modulus>oKEvsLScxTEI7Gu03pjbWRGxC/5JGOxpNo/7+" +
                                         "WXyceHw9hBbVA5tizvhex8OFQl+NDDJcTeS8M7bek8JAcaeAisYRglghK1i" +
                                         "YWeYb/7qavVjjPLWXJv1aiOFl8DK2LhhY4eWnS/AQ4CcQrB/UriqMBd90/t" +
                                         "JW+ZYbuJASd7e7Mc=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

        public static RSA PublicKey = null;

        // SEE: http://jclement.ca/devel/dotnet/reallysimplelicensing.html

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Static constructor
        /// </summary>
        static Licensing()
        {
            PublicKey = RSA.Create();
            PublicKey.FromXmlString(LIC_PUBKEY);
        }

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Converts a License Document (XML) into a Key Code (Base64 compressed text)
        /// </summary>
        // IMPORTANT: This must be equivalent to that in the website
        public static string LicenseDocumentToKeyCode(string Document)
        {
            var DocBytes = Document.StringToBytes();
            var DocCompressed = BytesHandling.Compress(DocBytes);
            var KeyCode = Convert.ToBase64String(DocCompressed).Intercalate(50, Environment.NewLine);

            return KeyCode;
        }

        /// <summary>
        /// Converts a Key Code (Base64 compressed text) into a License Document (XML)
        /// </summary>
        public static string LicenseKeyCodeToDocument(string KeyCode)
        {
            if (KeyCode.IsAbsent())
                return "";

            var KeyCodeCompressed = Convert.FromBase64String(KeyCode.RemoveNewLines("").Replace(" ", ""));
            if (KeyCodeCompressed.Length < 1)
                throw new Exception("Compressed key-code is corrupt.");

            var DocBytes = BytesHandling.Decompress(KeyCodeCompressed);
            var Document = DocBytes.BytesToString();

            return Document;
        }

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Validates and extract License information from the supplied license-document and Product-Version (major version number segment).
        /// Returns the License User, Type and Edition (codes), plus Mode and Expiration date (EMPTY_DATE if not set).
        /// </summary>
        public static Tuple<string, string, string, string, DateTime> ExtractValidLicense(string LicDocument, string ProductVersion,
                                                                                          RSA PublicSigningKey = null)
        {
            if (PublicSigningKey == null)
                PublicSigningKey = PublicKey;

            /*- var CompressedKeyDoc = Convert.FromBase64String(KeyDocument);
            var KeyDoc = BytesHandling.Decompress(CompressedKeyDoc).BytesToStringUnicode(); */

            if (!SignedDocumentIsValid(LicDocument, PublicSigningKey))
                return null;

            var LicDoc = XDocument.Parse(LicDocument);
            var License = LicDoc.Element(LIC_REG_LICENSE);

            var LicProdApp = License.Element(LIC_FLD_PRODAPP).Value.Trim();
            if (LicProdApp != AppExec.ApplicationName)
                throw new Exception("License-Key is for product-application '" + LicProdApp + "'.");

            var LicProdVer = License.Element(LIC_FLD_PRODVER).Value.Trim();
            if (LicProdVer != AppExec.ApplicationVersionMajorNumber)
                throw new Exception("License-Key is for (major) version '" + LicProdVer + "'.");

            var LicUser = License.Element(LIC_FLD_USER).Value;
            var LicType = License.Element(LIC_FLD_TYPE).Value.ToUpper().Trim();
            var LicEdition = License.Element(LIC_FLD_EDITION).Value.ToUpper().Trim();
            var LicMode = License.Element(LIC_FLD_MODE).Value.ToUpper().Trim();
            var LicExpiration = License.Element(LIC_FLD_EXPIRATION);

            var Expiration = (LicExpiration.ToStringAlways().IsAbsent()
                              ? General.EMPTY_DATE
                              : DateTime.ParseExact(LicExpiration.Value, "yyyyMMdd", CultureInfo.InvariantCulture));

            return Tuple.Create(LicUser, LicType, LicEdition, LicMode, Expiration);
        }

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Creates and returns a signed License document based on the supplied parameters.
        /// </summary>
        // IMPORTANT: THIS MUST BE EQUIVALENT TO THE WEB-SITE METHOD TO GENERATE LICENSE KEYS
        public static string CreateLicenseInfoSignedDocument(string LicUserName, string LicTypeCode, string LicEditionCode,
                                                             string LicModeCode, DateTime LicExpiration,
                                                             string ProdApplication, string ProdVersion,    // Major Version Number (before the first '.')
                                                             string PrivateSigningKey)
        {
            /* Before...
            LicUserName = LicUserName.Trim().Replace("   ", " ").Replace("  ", " ").ToUpperInvariant()
                          .Regularize(STR_LICENSE_REPLACCHAR, STR_LICENSE_VALIDCHARS); */
            LicUserName = LicUserName.Trim();   // Not uppercased
            LicTypeCode = LicTypeCode.Trim().ToUpper();
            LicEditionCode = LicEditionCode.Trim().ToUpper();
            LicModeCode = LicModeCode.Trim().ToUpper();

            var Document = new XDocument(
                            new XElement(LIC_REG_LICENSE,
                                            new XElement(LIC_FLD_PRODAPP, ProdApplication),
                                            new XElement(LIC_FLD_PRODVER, ProdVersion),
                                            new XElement(LIC_FLD_USER, LicUserName),
                                            new XElement(LIC_FLD_TYPE, LicTypeCode),
                                            new XElement(LIC_FLD_EDITION, LicEditionCode),
                                            new XElement(LIC_FLD_MODE, LicModeCode),
                                            new XElement(LIC_FLD_EXPIRATION, LicExpiration.ToString("yyyyMMdd"))));

            var CryptoKey = RSA.Create();
            CryptoKey.FromXmlString(PrivateSigningKey);

            Document = General.SignXmlDocument(CryptoKey, Document);

            var Result = Document.ToString();
            return Result;
        }

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns indication of whether the supplied License Document is valid or not base on the specified Public-Key.
        /// </summary>
        public static bool SignedDocumentIsValid(string LicDocument, RSA PublicSigningKey = null)
        {
            if (PublicSigningKey == null)
                PublicSigningKey = PublicKey;

            var XmlDoc = XDocument.Parse(LicDocument).ToXmlDocument();

            var Result = General.VerifySignedXmlDocument(PublicSigningKey, XmlDoc);

            return Result;
        }

        //------------------------------------------------------------------------------------------
        /* SAMPLE...
        <license>
          <firstname>NESTOR</firstname>
          <lastname>SANCHEZ</lastname>
          <company>INSTRUMIND</company>
          <type>NOC</type>
          <edition>ULT</edition>
          <mode>PRM</mode>
          <expiration>20990724</expiration>
          <Signature xmlns="http://www.w3.org/2000/09/xmldsig#">
            <SignedInfo>
              <CanonicalizationMethod Algorithm="http://www.w3.org/TR/2001/REC-xml-c14n-20010315" />
              <SignatureMethod Algorithm="http://www.w3.org/2000/09/xmldsig#rsa-sha1" />
              <Reference URI="">
                <Transforms>
                  <Transform Algorithm="http://www.w3.org/2000/09/xmldsig#enveloped-signature" />
                </Transforms>
                <DigestMethod Algorithm="http://www.w3.org/2000/09/xmldsig#sha1" />
                <DigestValue>+S3STs3JG+DEd3S3mg8fj3M4syc=</DigestValue>
              </Reference>
            </SignedInfo>
            <SignatureValue>mNtfLh1Ddl5dWHOW0VVAMuXgWMowHtqalBl5pe/HRDdl0a2eWQAJoCUL0MrhHWzz36Q1kdp2L/ZNRyUeC9ZCnMxs9D71NyEL+neVfqBLquTf2Hu4CT6x6jR/zhD/FW67ikX/IZtkIChL2PcFhpxYAxcnQ9C+Tkf7IUJ5yVYv2C0=</SignatureValue>
          </Signature>
        </license> */

        //------------------------------------------------------------------------------------------
        /* OBSOLETE
        /// <summary>
        /// When valid, returns the license Type and Edition codes of the specified parameters, else returns null.
        /// </summary>
        public static Tuple<string, string> ValidateAndRegisterLicense(string FirstName, string LastName, string Company,
                                                                       string LicenseKeyCode)
        {
            try
            {
                FirstName = FirstName.Trim().ToUpperInvariant().Regularize(STR_LICENSE_REPLACCHAR, STR_LICENSE_VALIDCHARS);
                LastName = LastName.Trim().ToUpperInvariant().Regularize(STR_LICENSE_REPLACCHAR, STR_LICENSE_VALIDCHARS);
                Company = Company.Trim().ToUpperInvariant().Regularize(STR_LICENSE_REPLACCHAR, STR_LICENSE_VALIDCHARS);
                LicenseKeyCode = LicenseKeyCode.Trim();
                var PosSep = LicenseKeyCode.IndexOf("=");
                if (PosSep < 0)
                    return null;

                var LicKeyDigestValue = LicenseKeyCode.GetLeft(PosSep);
                var LicKeySignatureValue = LicenseKeyCode.Substring(PosSep + 1);

                foreach (var LicType in AppExec.LicenseTypes)
                    foreach( var LicEdition in AppExec.LicenseEditions)
                    {
                        var LicenseDoc = CreateUnvalidatedLicenseDocument(FirstName, LastName, Company, LicKeyDigestValue, LicKeySignatureValue,
                                                                          LicType.TechName, LicEdition.TechName);

                        if (SignedDocumentIsValid(LicenseDoc, PublicKey))
                        {
                            General.StringToFile(AppExec.LicenseFilePath, LicenseDoc);
                            return Tuple.Create(LicType.TechName, LicEdition.TechName);
                        }
                    }

                Display.DialogMessage("Attention!", "The provided registration information is not valid!", EMessageType.Error);
            }
            catch (Exception Problem)
            {
                Display.DialogMessage("Attention!", "Cannot validate or register License information.\nProblem:" + Problem.Message,
                                      EMessageType.Error);
            }

            return null;
        }

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Creates and returns a document, which may be a valid signed License document.
        /// </summary>
        public static string CreateUnvalidatedLicenseDocument(string FirstName, string LastName, string Company,
                                                              string LicKeyDigestValue, string LicKeySignatureValue,
                                                              string LicTypeCode, string LicEditionCode)
        {
            var Document = "<license><firstname>" + FirstName + "</firstname><lastname>" + LastName + "</lastname>" +
                           "<company>" + Company + "</company><type>" + LicTypeCode + "</type><edition>" + LicEditionCode + "</edition>" +
                           "<Signature xmlns='http://www.w3.org/2000/09/xmldsig#'><SignedInfo><CanonicalizationMethod Algorithm='http://www.w3.org/TR/2001/REC-xml-c14n-20010315' />" +
                           "<SignatureMethod Algorithm='http://www.w3.org/2000/09/xmldsig#rsa-sha1' /><Reference URI=''><Transforms><Transform Algorithm='http://www.w3.org/2000/09/xmldsig#enveloped-signature' /></Transforms>" +
                           "<DigestMethod Algorithm='http://www.w3.org/2000/09/xmldsig#sha1' /><DigestValue>" + LicKeyDigestValue + "</DigestValue></Reference></SignedInfo>" +
                           "<SignatureValue>" + LicKeySignatureValue + "</SignatureValue></Signature></license>";

            Document = Document.Replace((char)39, '"');

            return Document;
        }

                 //------------------------------------------------------------------------------------------
        /// <summary>
        /// Loads and validates the registered license, if any.
        /// Returns First-Name, Last-Name, Company, License-Type and License-Edition, or null if not valid/registered.
        /// </summary>
        public static Tuple<string, string, string, string, string> LoadValidRegisteredLicense(bool AnnounceProblem = false)
        {
            try
            {
                var LicenseDocument = General.FileToString(AppExec.LicenseFilePath);

                if (!SignedDocumentIsValid(LicenseDocument))
                {
                    if (AnnounceProblem)
                        Display.DialogMessage("Attention!", "The currently registered License is not valid.",
                                              EMessageType.Error);

                    return null;
                }
            }
            catch (Exception Problem)
            {
                if (AnnounceProblem)
                    Display.DialogMessage("Attention!", "Cannot detect a valid License registration.\nProblem:" + Problem.Message,
                                          EMessageType.Error);
            }

            return null;
        }
        */

        //------------------------------------------------------------------------------------------
    }
}
