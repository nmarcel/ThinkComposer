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
// Object : Instrumind.Common.General (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.06.23 Néstor Sánchez A.  Creation
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Security.Principal;
using System.Text;
using System.Windows.Documents;
using System.Xml;
using System.Xml.Linq;

using Microsoft.Win32;

// Better, but still uses some heavy DLLs
// using OfficeOutlook = NetOffice.OutlookApi;
// using OfficeWord = NetOffice.WordApi;

// OLD Style (not recommended, because is early-binding based, hence needing exact assembly matches)...
// using OfficeOutlook = Microsoft.Office.Interop.Outlook;
// using OfficeWord = Microsoft.Office.Interop.Word; 

using Instrumind.Common.EntityBase;
using Instrumind.Common.Visualization;

/// Library of general purpose components and features across Instrumind products.
namespace Instrumind.Common
{
    /// <summary>
    /// Provides common features such as encryption, serialization, strings handling extensions and global constants. Specials part.
    /// </summary>
    public static partial class General
    {
        // -----------------------------------------------------------------------------------------
        #region Contracts

        /// <summary>
        /// Ensures, after execution, that the provided expression is true, else throws an exception.
        /// </summary>
        /// <param name="Expression">Expression result</param>
        public static void ContractAssert(params bool[] Expressions)
        {
            foreach (bool Expression in Expressions)
                if (!Expression)
                    throw new UsageAnomaly("Assert failed for the evaluated expression.", Expression);
        }

        /// <summary>
        /// Ensures, before execution, that the provided expresion is true, else throws an exception.
        /// </summary>
        /// <param name="Expression">Expression result</param>
        public static void ContractRequires(params bool[] Expressions)
        {
            foreach (bool Expression in Expressions)
                if (!Expression)
                    throw new UsageAnomaly("Expression did not accomplish requirement.", Expression);
        }

        /// <summary>
        /// Ensures, before execution, that the provided object is not null, else throws an exception.
        /// </summary>
        /// <param name="Reference">Referenced object</param>
        public static void ContractRequiresNotNull(params object[] References)
        {
            foreach (object Reference in References)
                if (Reference == null)
                    throw new UsageAnomaly("Required reference is null.", Reference);
        }

        /// <summary>
        /// Ensures, before execution, that the provided string is not empty or null, else throws an exception.
        /// </summary>
        /// <param name="Reference">Referenced string</param>
        public static void ContractRequiresNotAbsent(params string[] References)
        {
            foreach (string Reference in References)
                if (Reference.IsAbsent())
                    throw new UsageAnomaly("Required string is empty or null.", Reference);
        }

        #endregion

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Asynchronously downloads, from the Source address, a file to the specified Target.
        /// Plus, a progress-informer and progress-finisher can be specified.
        /// </summary>
        public static void DownloadFileAsync(System.Uri Source, string Target,
                                             Func<System.Net.DownloadProgressChangedEventArgs, bool> ProgressInformer,
                                             Action<OperationResult<bool>> ProgressFinisher)
        {
            var Client = new System.Net.WebClient();

            Client.DownloadProgressChanged +=
                ((sender, evargs) =>
                {
                    if (!ProgressInformer(evargs))
                        Client.CancelAsync();
                });

            Client.DownloadFileCompleted +=
                ((sender, evargs) =>
                {
                    if (evargs.Cancelled || evargs.Error != null)
                    {
                        ProgressFinisher(OperationResult.Failure<bool>("Download " + (evargs.Error == null ? "Cancelled" : "Failed") + "." +
                                                                (evargs.Error == null
                                                                 ? "" :
                                                                 "\nProblem:" + evargs.Error.Message)));
                        return;
                    }

                    ProgressFinisher(OperationResult.Success(true));
                });

            Client.DownloadFileAsync(Source, Target);
        }

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Sends an e-mail message via Microsoft Outlook.
        /// Returns an error message if failed, or null if succeeded.
        /// </summary>
        public static string SendMailTo(string Recipient, string Subject, string Body, string AttachmentPath = null)
        {
            try
            {
                dynamic Sender = Activator.CreateInstance(Type.GetTypeFromProgID("Outlook.Application"));
                dynamic Message = Sender.CreateItem(0   /* Mail Item */);
                Message.To = Recipient;
                Message.Subject = Subject;
                Message.Body = Body.NullDefault("");

                if (!AttachmentPath.IsAbsent())
                    Message.Attachments.Add(AttachmentPath, 1 /* By Value */, 1,
                                            AttachmentPath);

                Message.Display(false);
            }
            catch (Exception Problem)
            {
                return Problem.Message;
            }

            return null;
        }


        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Generates a new NotifyCollectionChangedEventArgs instance based on the supplied one, plus displacing index by the specified Index-offset.
        /// This is useful for propagate changes in the source collection when it is exposed for consumption together with other collections.
        /// </summary>
        public static NotifyCollectionChangedEventArgs GenerateDisplacedNotifyCollectionChangedEventArgs(NotifyCollectionChangedEventArgs Change, int IndexOffset)
        {
            NotifyCollectionChangedEventArgs Result = null;
            var DisplacedNewStart = (Change.NewStartingIndex < 0 ? Change.NewStartingIndex : (Change.NewStartingIndex + IndexOffset)); // Invalid when Action is Reset
            var DisplacedOldStart = (Change.OldStartingIndex < 0 ? Change.OldStartingIndex : (Change.OldStartingIndex + IndexOffset)); // Invalid when Action is Reset

            if (Change.Action.IsOneOf(NotifyCollectionChangedAction.Reset, NotifyCollectionChangedAction.Add, NotifyCollectionChangedAction.Remove))
            {
                if (Change.Action == NotifyCollectionChangedAction.Reset && (Change.NewItems == null || Change.NewItems.Count < 1))
                    Result = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                else
                {
                    var AffectedItems = (Change.Action == NotifyCollectionChangedAction.Remove ? Change.OldItems : Change.NewItems);
                    var AffectedStart = (Change.Action == NotifyCollectionChangedAction.Remove ? DisplacedOldStart : DisplacedNewStart);

                    if (Change.NewItems != null && Change.NewItems.Count > 1)
                    {
                        if (Change.NewStartingIndex >= 0)
                            Result = new NotifyCollectionChangedEventArgs(Change.Action, AffectedItems, AffectedStart);
                        else
                            Result = new NotifyCollectionChangedEventArgs(Change.Action, AffectedItems);
                    }
                    else
                    {
                        var AffectedIndex = (Change.Action == NotifyCollectionChangedAction.Remove ? DisplacedOldStart : DisplacedNewStart);

                        if (AffectedIndex >= 0)
                            Result = new NotifyCollectionChangedEventArgs(Change.Action, AffectedItems[0], AffectedIndex);
                        else
                            Result = new NotifyCollectionChangedEventArgs(Change.Action, AffectedItems[0]);
                    }
                }
            }
            else
                if (Change.Action == NotifyCollectionChangedAction.Replace)
                    if (Change.NewItems != null && Change.NewItems.Count > 1)
                    {
                        if (Change.OldStartingIndex >= 0)
                            Result = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, Change.NewItems, Change.OldItems, DisplacedOldStart);
                        else
                            Result = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, Change.NewItems, Change.OldItems, IndexOffset);
                    }
                    else
                    {
                        if (Change.OldStartingIndex >= 0)
                            Result = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, Change.NewItems[0], Change.OldItems[0], DisplacedNewStart);
                        else
                            Result = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, Change.NewItems[0], Change.OldItems[0]);
                    }
                else
                    if (Change.Action == NotifyCollectionChangedAction.Move)
                        if (Change.NewItems != null && Change.NewItems.Count > 1)
                            Result = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, Change.NewItems, DisplacedNewStart, DisplacedOldStart);
                        else
                            Result = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, Change.NewItems[0], DisplacedNewStart, DisplacedOldStart);

            if (Result == null)
                throw new InternalAnomaly("Cannot generate a coherent index-displaced 'NotifyCollectionChangedEventArgs' object from the supplied one.", Change);

            return Result;

            /* NOTES... (taken from http://nitoprograms.blogspot.com/2009/07/interpreting-notifycollectionchangedeve.html)

               In short, the value of the Action property determines the validity of other properties in this class.
               NewItems and OldItems are null when they are invalid; NewStartingIndex and OldStartingIndex are -1 when they are invalid.

               If Action is NotifyCollectionChangedAction.Add, then NewItems contains the items that were added.
               In addition, if NewStartingIndex is not -1, then it contains the index where the new items were added.

               If Action is NotifyCollectionChangedAction.Remove, then OldItems contains the items that were removed.
               In addition, if OldStartingIndex is not -1, then it contains the index where the old items were removed.

               If Action is NotifyCollectionChangedAction.Replace, then OldItems contains the replaced items and NewItems contains the replacement items.
               In addition, NewStartingIndex and OldStartingIndex are equal, and if they are not -1, then they contain the index where the items were replaced.

               If Action is NotifyCollectionChangedAction.Move, then NewItems and OldItems are logically equivalent
               (i.e., they are SequenceEqual, even if they are different instances), and they contain the items that moved.
               In addition, OldStartingIndex contains the index where the items were moved from, and NewStartingIndex contains the index where the items were moved to.
               Note that a Move operation is logically treated as a Remove followed by an Add, so NewStartingIndex is interpreted as though the items had already been removed.

               If Action is NotifyCollectionChangedAction.Reset, then no other properties are valid.
             */
        }

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Converts this supplied XML Element into XElement.
        /// </summary>
        public static XElement ToXElement(this XmlElement Element)
        {
            var Subtree = Element.CreateNavigator().ReadSubtree();
            var Result = XElement.Load(Subtree);
            return Result;
        }

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the supplied XDocument as XmlDocument
        /// </summary>
        public static XmlDocument ToXmlDocument(this XDocument Document)
        {
            var Result = new XmlDocument();
            using (var Reader = Document.CreateReader())
            {
                Result.Load(Reader);
            }
            return Result;
        }

        /// <summary>
        /// Returns the supplied XmlDocument as XDocument
        /// </summary>
        public static XDocument ToXDocument(this XmlDocument Document)
        {
            using (var Reader = new XmlNodeReader(Document))
            {
                Reader.MoveToContent();
                return XDocument.Load(Reader);
            }
        }

        //------------------------------------------------------------------------------------------
        [DllImport("msi.dll", CharSet = CharSet.Unicode)]
        public static extern Int32 MsiGetProductInfo(string product, string property, [Out] StringBuilder valueBuf, ref Int32 len);

        /// <summary>
        /// Gets the installation date of a product, from the supplied Product-Id Guid
        /// </summary>
        public static DateTime GetProductInstallationDate(string ProductId)
        {
            var Result = default(DateTime);
            Int32 Length = 512;
            var TextBuilder = new StringBuilder(Length);

            if (!ProductId.StartsWith("{") || !ProductId.EndsWith("}"))
                ProductId = "{" + ProductId + "}";

            try
            {
                // Other extractable info: "VersionString", "VersionMajor", "VersionMinor"
                MsiGetProductInfo(ProductId, "InstallDate", TextBuilder, ref Length);
                var CapturedDate = TextBuilder.ToString();
                Result = DateTime.ParseExact(CapturedDate, "yyyyMMdd", CultureInfo.InvariantCulture);
            }
            catch (Exception Problem)
            {
                Result = EMPTY_DATE;
                Console.WriteLine("Cannot get Product installation date. Problem: " + Problem.Message);
            }

            return Result;
        }

        //------------------------------------------------------------------------------------------
        /* For creating pairs of public/private keys (RSA)...
         *             RSA key = RSA.Create();

            StringToFile(pubFile, key.ToXmlString(false));
            StringToFile(privateFile, key.ToXmlString(true));

         * SEE http://jclement.ca/devel/dotnet/reallysimplelicensing.html
         */

        /// <summary>
        /// Returns and XDocument with a signature attached to the supplied one.
        /// </summary>
        // IMPORTANT: This must be equivalent to that of the website source code
        public static XDocument SignXmlDocument(RSA SigningPrivKey, XDocument OriginalDocument)
        {
            var Document = OriginalDocument.ToXmlDocument();
            var SignedDoc = new SignedXml(Document);
            SignedDoc.SigningKey = SigningPrivKey;
            SignedDoc.SignedInfo.CanonicalizationMethod = SignedXml.XmlDsigCanonicalizationUrl;

            // Add reference to XML data
            var DataReference = new Reference("");
            DataReference.AddTransform(new XmlDsigEnvelopedSignatureTransform(false));
            SignedDoc.AddReference(DataReference);

            // Build signature
            SignedDoc.ComputeSignature();

            // Attach signature to XML Document
            XmlElement sig = SignedDoc.GetXml();
            Document.DocumentElement.AppendChild(sig);
            var Result = Document.ToXDocument();
            return Result;
        }

        public const string XML_SIGNED_DOC_FLD_SIGNATURE = "Signature";

        /// <summary>
        /// Idicates whether the supplied Document is valid based on the provided Key.
        /// </summary>
        public static bool VerifySignedXmlDocument(RSA SigningPubKey, XmlDocument Document)
        {
            SignedXml SignedDoc = new SignedXml(Document);

            try
            {
                // Find signature node
                XmlNode sig = Document.GetElementsByTagName(XML_SIGNED_DOC_FLD_SIGNATURE, SignedXml.XmlDsigNamespaceUrl)[0];
                SignedDoc.LoadXml((XmlElement)sig);
            }
            catch
            {
                // Not signed!
                return false;
            }

            var Result = SignedDoc.CheckSignature(SigningPubKey);
            return Result;
        }

        // -------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gives permission to the specified Directory-Path.
        /// Note: SecurityId = new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null);
        /// </summary>
        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand)]
        public static void SetDirectoryPermission(string DirectoryPath, SecurityIdentifier SecurityId, bool ForAllUsers = false)
        {
            DirectoryInfo directoryInfo;
            DirectorySecurity directorySecurity;
            AccessRule rule;

            if (!Directory.Exists(DirectoryPath))
                directoryInfo = Directory.CreateDirectory(DirectoryPath);
            else
                directoryInfo = new DirectoryInfo(DirectoryPath);

            bool modified;
            directorySecurity = directoryInfo.GetAccessControl();

            if (ForAllUsers)
                rule = new FileSystemAccessRule(
                    SecurityId,
                    FileSystemRights.Write |
                    FileSystemRights.ReadAndExecute |
                    FileSystemRights.Modify,
                    InheritanceFlags.ContainerInherit |
                    InheritanceFlags.ObjectInherit,
                    PropagationFlags.InheritOnly,
                    AccessControlType.Allow);
            else
                rule = new FileSystemAccessRule(
                        SecurityId,
                        FileSystemRights.Write |
                        FileSystemRights.ReadAndExecute |
                        FileSystemRights.Modify,
                        AccessControlType.Allow);

            directorySecurity.ModifyAccessRule(AccessControlModification.Add, rule, out modified);
            directoryInfo.SetAccessControl(directorySecurity);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        // PENDING: SaveFlowDocumentAsWordDocument
        // SEE: http://msdn.microsoft.com/en-us/library/bb448854.aspx

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Save the Word document in the specified document-location as PDF, using Microsoft Word.
        /// </summary>
        public static string SaveWordDocumentAsPDF(string DocumentLocation)
        {
            string ErrorMessage = null;

            try
            {
                // Create a new Microsoft Word application object 
                dynamic WordApp = Activator.CreateInstance(Type.GetTypeFromProgID("Word.Application"));

                // Get a Word file 
                FileInfo wordFile = new FileInfo(DocumentLocation);

                WordApp.Visible = false;
                WordApp.ScreenUpdating = false;

                // Cast as Object for word Open method 
                Object filename = (Object)wordFile.FullName;

                // Use the dummy value as a placeholder for optional arguments 
                dynamic doc = WordApp.Documents.Open(filename);
                doc.Activate();

                object outputFileName = wordFile.FullName.Replace(".docx", ".pdf");
                object fileFormat = 17 /* PDF */;

                // Save document into PDF Format 
                doc.SaveAs(outputFileName, fileFormat);

                // Close the Word document, but leave the Word application open. 
                // doc has to be cast to type _Document so that it will find the 
                // correct Close method.                 
                int saveChanges = 0 /* do not save changes*/;
                doc.Close(saveChanges);
                doc = null;

                // word has to be cast to type _Application so that it will find 
                // the correct Quit method. 
                WordApp.Quit();
                WordApp = null;
            }
            catch (Exception Problem)
            {
                ErrorMessage = Problem.Message;
            }

            return ErrorMessage;
        }

        // -------------------------------------------------------------------------------------------------------------
        public static void LogException(Exception Problem, bool IsWarning = false)
        {
            LogMessage("EXCEPTION. Message: " + Problem.Message + ".\nStack-Trace: " + Problem.StackTrace,
                       IsWarning ? EventLogEntryType.Warning : EventLogEntryType.Error);
        }

        public static void LogMessage(string Message, EventLogEntryType Kind = EventLogEntryType.Error)
        {
            if (!EventLog.SourceExists(AppExec.ApplicationName))
                EventLog.CreateEventSource(AppExec.ApplicationName, "Application");

            EventLog.WriteEntry(AppExec.ApplicationName, Message, Kind);
        }

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the supplied bytes as the most suitable content by mime-type.
        /// </summary>
        public static object BytesToAppropriateObject(byte[] Bytes)
        {
            object Result = null;
            var MimeType = GetMimeTypeFromContent(Bytes);

            if (MimeType.StartsWith("image/"))
                Result = Bytes.ToImageSource();
            else
                if (MimeType.StartsWith("text/"))
                    Result = Bytes.BytesToString();
                else
                    Result = Bytes;

            return Result;
        }

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the Mime-Type associated to the supplied Extension.
        /// </summary>
        // SEE MIME-TYPES IN: http://msdn.microsoft.com/en-us/library/ms775147%28VS.85%29.aspx
        public static string GetMimeTypeFromFileExtension(string FileExtension)
        {
            string MimeCode = "application/octet-stream";
            FileExtension = FileExtension.Trim().ToLower();
            FileExtension = (FileExtension.StartsWith(".") ? FileExtension : "." + FileExtension);

            Microsoft.Win32.RegistryKey RegKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(FileExtension);

            if (RegKey != null && RegKey.GetValue("Content Type") != null)
                MimeCode = RegKey.GetValue("Content Type").ToString();

            return MimeCode;
        }

        // -------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the Mime-Type associated to the content of the supplied FileName.
        /// </summary>
        public static string GetMimeTypeFromFileContent(string FileName)
        {
            if (!File.Exists(FileName))
                throw new FileNotFoundException(FileName + " not found");

            byte[] buffer = new byte[256];
            using (FileStream fs = new FileStream(FileName, FileMode.Open))
            {
                if (fs.Length >= 256)
                    fs.Read(buffer, 0, 256);
                else
                    fs.Read(buffer, 0, (int)fs.Length);
            }

            var Result = GetMimeTypeFromContent(buffer);
            return Result;
        }

        /// <summary>
        /// Returns the Mime-Type associated to the content of the supplied Content bytes.
        /// </summary>
        public static string GetMimeTypeFromContent(byte[] Content)
        {
            string Result = null;

            try
            {
                System.UInt32 MimeType;
                FindMimeFromData(0, null, Content, (uint)Content.Length, null, 0, out MimeType, 0);
                System.IntPtr MimeTypePtr = new IntPtr(MimeType);
                Result = Marshal.PtrToStringUni(MimeTypePtr);
                Marshal.FreeCoTaskMem(MimeTypePtr);
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot detect mime-type. Problem: " + e.Message);
                Result = "unknown/unknown";
            }

            if (Result == "text/plain")
            {
                var Text = Content.BytesToString().ToStrings(255);
                if (Text.IsRegularlySeparatedBy('\t'))
                    Result = "text/tsv";
            }

            //T Console.WriteLine("Detected mime-type: " + Result);
            return Result;
        }

        [DllImport(@"urlmon.dll", CharSet = CharSet.Auto)]
        private extern static System.UInt32 FindMimeFromData(System.UInt32 pBC,
                                                             [MarshalAs(UnmanagedType.LPStr)] System.String pwzUrl,
                                                             [MarshalAs(UnmanagedType.LPArray)] byte[] pBuffer,
                                                             System.UInt32 cbSize,
                                                             [MarshalAs(UnmanagedType.LPStr)] System.String pwzMimeProposed,
                                                             System.UInt32 dwMimeFlags,
                                                             out System.UInt32 ppwzMimeOut,
                                                             System.UInt32 dwReserverd);

        // -------------------------------------------------------------------------------------------------------------
    }
}