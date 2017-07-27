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
// File   : AppExec.cs
// Object : Instrumind.Common.AppExec (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.06.23 Néstor Sánchez A.  Creation
//

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows;

/// Library of general purpose components and features across Instrumind products.
namespace Instrumind.Common
{
    /// <summary>
    /// Provides general services, for the application execution, such as rule checking, logging, error management and licensing.
    /// </summary>
    public static class AppExec
    {
        /// <summary>
        /// Static constructor.
        /// </summary>
        static AppExec()
        {
        }

        /// <summary>
        /// Application exit code: Fatal crash.
        /// </summary>
        public const int APP_EXITCODE_CRASH = -1;

        /// <summary>
        /// Base namespace for Instrumind products.
        /// </summary>
        public const string BASE_NAMESPACE = "Instrumind";

        /// <summary>
        /// Prefix for the user data directory.
        /// </summary>
        public const string USERDATA_DIR_PREFIX = "My ";

        /// <summary>
        /// Clipart subdirectory name
        /// </summary>
        public const string CLIPART_SUBDIR = "OpenClipart2";

        /// <summary>
        /// Temporal subdirectory name
        /// </summary>
        public const string TEMPORAL_SUBDIR = "_temp";  // MUST BE THE SAME AS IN InstallerCustomAction.Installer1.cs (Commit method)

        /// <summary>
        /// Unregistered copy text
        /// </summary>
        public const string UNREGISTERED_COPY = "<PUBLIC>"; // "<UNREGISTERED-COPY>";

        /// <summary>
        /// Extension of the License file.
        /// </summary>
        public const string LICENSE_EXTENSION = "lic";

        /// <summary>
        /// Extension of the Configuration file.
        /// </summary>
        public const string CONFIG_EXTENSION = "cfg";

        /// <summary>
        /// Extension of list files.
        /// </summary>
        public const string LISTS_EXTENSION = "lst";

        /// <summary>
        /// Extension of the Log file.
        /// </summary>
        public const string LOG_EXTENSION = "log";

        // License modes (MUST MATCH RESELLER CODES OR SUFFIX)
        public const string LIC_MODE_BETA = "BET";
        public const string LIC_MODE_TRIAL = "TRL";
        public const string LIC_MODE_PERMANENT = "PRM";
        public const string LIC_MODE_SUBSCRIPTION = "SUB";

        public static readonly SimpleElement[] LicenseModes = { new SimpleElement("BETA", LIC_MODE_BETA),
                                                                new SimpleElement("TRIAL", LIC_MODE_TRIAL),
                                                                new SimpleElement("PERMANENT", LIC_MODE_PERMANENT),
                                                                new SimpleElement("SUBSCRIPTION", LIC_MODE_SUBSCRIPTION)};

        // License types (MUST MATCH RESELLER CODES OR SUFFIX)
        public const string LIC_TYPE_EDUCATIONAL = "EDU";
        public const string LIC_TYPE_NONCOMMERCIAL = "NOC";
        public const string LIC_TYPE_COMMERCIAL = "COM";

        public static readonly SimpleElement[] LicenseTypes = { new SimpleElement("EDUCATIONAL", LIC_TYPE_EDUCATIONAL),
                                                                new SimpleElement("NON-COMMERCIAL", LIC_TYPE_NONCOMMERCIAL),
                                                                new SimpleElement("COMMERCIAL", LIC_TYPE_COMMERCIAL) };

        // License functionality levels (MUST MATCH RESELLER CODES OR SUFFIX)
        public const string LIC_EDITION_FREE = "FRE";
        public const string LIC_EDITION_LITE = "LIT";
        public const string LIC_EDITION_STANDARD = "STD";
        public const string LIC_EDITION_PROFESSIONAL = "PRO";
        public const string LIC_EDITION_ULTIMATE = "ULT";

        public static readonly SimpleElement[] LicenseEditions = { /* Not necessary, limitations are for saving: new SimpleElement("VIEWER", LIC_EDITION_VIEWER), */
                                                                   new SimpleElement("FREE", LIC_EDITION_FREE),
                                                                   new SimpleElement("LITE", LIC_EDITION_LITE),
                                                                   new SimpleElement("STANDARD", LIC_EDITION_STANDARD),
                                                                   new SimpleElement("PROFESSIONAL", LIC_EDITION_PROFESSIONAL),
                                                                   new SimpleElement("ULTIMATE", LIC_EDITION_ULTIMATE) };
        /// <summary>
        /// Current license mode of the application.
        /// </summary>
        public static SimpleElement CurrentLicenseMode = null;

        /// <summary>
        /// Current license type of the application.
        /// </summary>
        public static SimpleElement CurrentLicenseType = null;

        /// <summary>
        /// Current license edition of the application.
        /// </summary>
        public static SimpleElement CurrentLicenseEdition = null;

        /// <summary>
        /// Current license agreement of the application.
        /// </summary>
        public static string LicenseAgreement = "[LICENSE-AGREEMENT]";

        /// <summary>
        /// Name of the application license file
        /// </summary>
        public static string LicenseFilePath { get; private set; }

        /// <summary>
        /// Current license expiration date of the application.
        /// </summary>
        public static DateTime CurrentLicenseExpiration = General.EMPTY_DATE;

        /// <summary>
        /// Name of the licensed user-name as obtained from registration.
        /// </summary>
        public static string CurrentLicenseUserName = UNREGISTERED_COPY;

        /// <summary>
        /// Name of the application configuration file
        /// </summary>
        public static string ConfigurationFilePath { get; private set; }

        /// <summary>
        /// Name of the application recent-documents file
        /// </summary>
        public static string RecentDocumentsFilePath { get; private set; }

        /// <summary>
        /// Name of the application log file
        /// </summary>
        public static string LogFilePath { get; private set; }

        /// <summary>
        /// Name of the product application.
        /// </summary>
        public static string ApplicationName { get; set; }

        /// <summary>
        /// Complete version number of the application (version, maj-rev, min-rev, build).
        /// </summary>
        public static string ApplicationVersion { get; set; }

        /// <summary>
        /// Name of the product definitions.
        /// </summary>
        public static string ApplicationDefinitions { get; set; }

        /// <summary>
        /// Gets the Major version segment of the application version number.
        /// </summary>
        public static string ApplicationVersionMajorNumber { get { return ApplicationVersion.CutBetween(null, ".", true); } }

        /// <summary>
        /// Gets or sets the application standard content type for its documents.
        /// If none is specified, then the generic Instrumind content type is used.
        /// </summary>
        public static string ApplicationContentTypeCode
        {
            get
            {
                if (ApplicationContentTypeCode_ == null)
                    ApplicationContentTypeCode_ = Company.GENERIC_CONTENTTYPE_CODE;

                return ApplicationContentTypeCode_;
            }
            set
            {
                ApplicationContentTypeCode_ = value;
            }
        }
        private static string ApplicationContentTypeCode_ = null;

        /// <summary>
        /// Start date and time of the session.
        /// </summary>
        public static DateTime SessionStart { get; private set; }

        /// <summary>
        /// Directory path for storing data shared by the company's applications (common to all users, writable with UAC/Admin privileges).
        /// </summary>
        public static string ApplicationsCommonDataDirectory { get; private set; }

        /// <summary>
        /// Directory path for storing application data (common to all users, writable with UAC/Admin privileges).
        /// </summary>
        public static string ApplicationSpecificDataDirectory { get; private set; }

        /// <summary>
        /// Directory path for storing application definitions (common to all users, writable with UAC/Admin privileges).
        /// </summary>
        public static string ApplicationSpecificDefinitionsDirectory { get; private set; }

        /// <summary>
        /// Directory path for storing clipart data (common to all users, writable with UAC/Admin privileges).
        /// </summary>
        public static string ApplicationsClipartDirectory { get; private set; }

        /// <summary>
        /// Directory path for storing temporal data (per user).
        /// </summary>
        public static string ApplicationUserTemporalDirectory { get; private set; }

        /// <summary>
        /// Directory path for storing user data (per user).
        /// </summary>
        public static string ApplicationUserDirectory { get; private set; }

        /// <summary>
        /// Gets or sets the user of the application.
        /// If none is specified, then the windows user logged in is obtained.
        /// </summary>
        public static string SessionUserName
        {
            get
            {
                if (UserName_ == null)
                    UserName_ = Environment.UserName;

                return UserName_;
            }
            set
            {
                UserName_ = value;
            }
        }
        private static string UserName_ = null;

        /// <summary>
        /// Directory path for storing user data.
        /// </summary>
        public static string UserDataDirectory { get; private set; }

        /// <summary>
        /// Name given to user documents (e.g.: "Files" to conform directory "My Files")
        /// </summary>
        public static string UserDataNaming { get; private set; }

        /// <summary>
        /// Character for separate Scope from Code.
        /// This character must not be a Valid Identifier characer.
        /// Example: "Application/DefaultFileName:Composition"
        /// </summary>
        public const string CONFIG_SCOPESEPARATOR = "/";

        /// <summary>
        /// Character for separate Code sub-segments/tokens.
        /// This character can be a Valid Identifier characer.
        /// Example: "Application/Dialogs.Title.Color:Red"
        /// </summary>
        public const string CONFIG_CODESEPARATOR = ".";

        /// <summary>
        /// Character for assign a (left-sided) Value to a (right-sided) Configuration.
        /// Also can be used as assignment continuation for multi-line assignments.
        /// This character must not be a Valid Identifier characer.
        /// Examples:
        /// -Single-line: "Application/DefaultFileName:Composition"
        /// -Multi-line: "Template/Header:This is the document header"
        ///              ":This is the second line of the document header"
        /// </summary>
        public const string CONFIG_ASSIGNATOR = ":";

        /// <summary>
        /// <summary>
        /// Collection of current configuration Scope+Code and Values.
        /// </summary>
        private static readonly Dictionary<string, string> CurrentConfiguration = new Dictionary<string, string>();

        /// <summary>
        /// Dispatcher used for UI work from other threads.
        /// </summary>
        public static System.Windows.Threading.Dispatcher DispatcherForUI { get; private set; }

        /// Indicates whether this class has been initialized.
        /// </summary>
        private static bool Initialized = false;

        /// <summary>
        /// Initialization of internal resources.
        /// Must be called at the start of the application execution.
        /// </summary>
        public static void Initialize(string AppName, string AppVersion, string AppDefinitions, string UserDataName)
        {
            if (Initialized)
                Console.WriteLine("Reinitializing");

            General.ContractRequiresNotAbsent(AppName, AppVersion, UserDataName);
            General.ContractAssert(AppName.IsValidIdentifier());
            General.ContractAssert(UserDataName.IsValidIdentifier());

            ApplicationName = AppName;
            ApplicationVersion = AppVersion;    // Very important. License version is check against this
            ApplicationDefinitions = AppDefinitions;
            UserDataNaming = UserDataName;

            string DetectedProblem = "";
            Exception DetectedError = null;
            SessionStart = DateTime.Now;

            // MUST BE THE SAME FOLDERS AS IN InstallerCustomAction.Installer1.cs (Commit method)

            // This is: - "C:\ProgramData\" on Windows Vista and Windows 7
            //          - "C:\Documents and Settings\<User-Account>\Application Data\" on Windows XP
            ApplicationsCommonDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), General.TextToIdentifier(Company.NAME_SIMPLE));

            ApplicationsClipartDirectory = Path.Combine(ApplicationsCommonDataDirectory, CLIPART_SUBDIR);

            // This is: "C:\Users\<User-Account>\AppData\Local\Instrumind" on Windows 7
            ApplicationUserDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), General.TextToIdentifier(Company.NAME_SIMPLE));

            // My Documents\My Compositions
            UserDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), USERDATA_DIR_PREFIX + UserDataNaming);

            try
            {
                // Create [Common]\Instrumind
                if (!Directory.Exists(ApplicationsCommonDataDirectory))
                    Directory.CreateDirectory(ApplicationsCommonDataDirectory);

                // Create [Common]\Instrumind\ThinkComposer (currently not used, as it cannot be saved without UAC/admin privileges)
                ApplicationSpecificDataDirectory = Path.Combine(ApplicationsCommonDataDirectory, AppExec.ApplicationName.TextToIdentifier());
                if (!Directory.Exists(ApplicationSpecificDataDirectory))
                    Directory.CreateDirectory(ApplicationSpecificDataDirectory);

                // Create [Common]\Instrumind\Domains
                ApplicationSpecificDefinitionsDirectory = Path.Combine(ApplicationsCommonDataDirectory, AppExec.ApplicationDefinitions.TextToIdentifier());
                if (!Directory.Exists(ApplicationSpecificDefinitionsDirectory))
                    Directory.CreateDirectory(ApplicationSpecificDefinitionsDirectory);

                // Create [User]\Instrumind
                if (!Directory.Exists(ApplicationUserDirectory))
                    Directory.CreateDirectory(ApplicationUserDirectory);

                // Create [User]\Instrumind\_temp
                ApplicationUserTemporalDirectory = Path.Combine(ApplicationUserDirectory, TEMPORAL_SUBDIR);
                if (!Directory.Exists(ApplicationUserTemporalDirectory))
                    Directory.CreateDirectory(ApplicationUserTemporalDirectory);

                // Create [User]\Instrumind\ThinkComposer
                ApplicationUserDirectory = Path.Combine(ApplicationUserDirectory, AppExec.ApplicationName.TextToIdentifier());
                if (!Directory.Exists(ApplicationUserDirectory))
                    Directory.CreateDirectory(ApplicationUserDirectory);

                // Third: Create ...\MyCompositions
                if (!Directory.Exists(UserDataDirectory))
                    Directory.CreateDirectory(UserDataDirectory);
            }
            catch (Exception Anomaly)
            {
                DetectedProblem = "Cannot create Application Directory.";
                DetectedError = Anomaly;
            }

            if (DetectedError == null)
            {
                LicenseFilePath = ApplicationUserDirectory + "\\" + AppExec.ApplicationName.TextToIdentifier() + "." + LICENSE_EXTENSION;
                ConfigurationFilePath = ApplicationUserDirectory + "\\" + AppExec.ApplicationName.TextToIdentifier() + "." + CONFIG_EXTENSION;
                RecentDocumentsFilePath = ApplicationUserDirectory + "\\" + "RecentDocuments." + LISTS_EXTENSION;

                try
                {
                    // new StreamWriter(new FileStream(LogFileName, FileMode.Append, FileAccess.Write, FileShare.Read));
                    LogWriter = TextWriter.Synchronized(new StreamWriter(ConfigurationFilePath, true));
                }
                catch (Exception Anomaly)
                {
                    DetectedProblem = "Cannot open or create Configuration file.";
                    DetectedError = Anomaly;
                }
            }

            if (DetectedError == null)
            {
                if (LogWriter != null)
                    LogWriter.Close();

                LogFilePath = ApplicationUserDirectory + "\\" + AppExec.ApplicationName.TextToIdentifier() + "." + LOG_EXTENSION;

                try
                {
                    // new StreamWriter(new FileStream(LogFileName, FileMode.Append, FileAccess.Write, FileShare.Read));
                    LogWriter = TextWriter.Synchronized(new StreamWriter(LogFilePath, true));
                }
                catch (Exception Anomaly)
                {
                    DetectedProblem = "Cannot open Log file.";
                    DetectedError = Anomaly;
                }
            }

            if (DetectedError != null)
            {
                // At this point, there could not be an application unhandled exception event attached,
                // so this problem will just be logged to the console.
                var Here = System.Reflection.MethodBase.GetCurrentMethod();

                LogException(new WarningAnomaly(new ExternalAnomaly(DetectedProblem, DetectedError)),
                             Here.DeclaringType.FullName + "." + Here.Name);

                LogWriter = null;

            }

            Initialized = true;
        }

        // -------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Invokes all the static constructors of the loaded types.
        /// Should be called after WPF application visual initialization (when resources are available).
        /// </summary>
        public static void InvokeAllStaticConstructors()
        {
            foreach (Assembly InspectedAssembly in AppDomain.CurrentDomain.GetAssemblies())
                foreach (Type ExportedType in InspectedAssembly.GetExportedTypes())
                    System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(ExportedType.TypeHandle);
        }

        // -------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Sets the dependency-object from which to use its dispatcher to make UI calls from other threads.
        /// It Cannot be the main Window, must be a child control.
        /// </summary>
        public static void SetSourceOfDispatcherForUI(DependencyObject Source)
        {
            DispatcherForUI = Source.Dispatcher;
        }

        // -------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Obtains and returns the requested configuration value.
        /// </summary>
        /// <typeparam name="TConfig">Type of the configuration value requested.</typeparam>
        /// <param name="Scope">Configuration scope.</param>
        /// <param name="Code">Key code that identificates the value.</param>
        /// <param name="Default">Optional default value when configuration is not found.</param>
        /// <param name="IgnoreFault">Indicates to ignore a fault (such as Scope+Code type clash), else the fault is thrown.</param>
        /// <returns>The configuration value obtained.</returns>
        public static TConfig GetConfiguration<TConfig>(string Scope, string Code, TConfig Default = default(TConfig), bool IgnoreFault = true)
        {
            TConfig Result = default(TConfig);
            var ConfigKey = Scope + CONFIG_SCOPESEPARATOR + Code;

            try
            {
                if (CurrentConfiguration != null && CurrentConfiguration.ContainsKey(ConfigKey))
                    Result = (TConfig)Convert.ChangeType(CurrentConfiguration[ConfigKey], typeof(TConfig));
                else
                    Result = Default;
            }
            catch (Exception Problem)
            {
                if (IgnoreFault)
                    Console.WriteLine("Cannot get configuration value for key [{0}]. Cause: {1}", ConfigKey, Problem);
                else
                    throw new UsageAnomaly("Cannot get configuration value for key [" + ConfigKey + "].", Problem);

                Result = Default;
            }

            return Result;
        }

        /// <summary>
        /// Sets the supplied configuration value.
        /// </summary>
        /// <typeparam name="TConfig">Type of the configuration value.</typeparam>
        /// <param name="Scope">Configuration scope.</param>
        /// <param name="Code">Key code that identificates the value.</param>
        /// <param name="Value">Configuration value to store.</param>
        /// <param name="SaveNow">Optionally, makes persistent (All) the configuration information.</param>
        public static void SetConfiguration<TConfig>(string Scope, string Code, TConfig Value, bool SaveNow = false)
        {
            General.ContractRequiresNotAbsent(Scope, Code);
            General.ContractRequires(Scope.IsValidIdentifier(), Code.IsValidIdentifier());
            var ConfigKey = Scope + CONFIG_SCOPESEPARATOR + Code;

            try
            {
                var StringValue = (string)Convert.ChangeType(Value, typeof(string));
                CurrentConfiguration[ConfigKey] = StringValue;
            }
            catch (Exception Problem)
            {
                throw new UsageAnomaly("Cannot set configuration value for key [" + ConfigKey + "].", Problem);
            }

            if (SaveNow)
                AppExec.StoreConfigurationTo();
        }

        /// <summary>
        /// Loads the application configuration from the specified file,
        /// or from the default configuration file-path if absent.
        /// </summary>
        public static void LoadConfigurationFrom(string ConfigFile = null)
        {
            if (ConfigFile.IsAbsent())
                ConfigFile = ConfigurationFilePath;

            StringReader Reader = new StringReader(General.FileToString(ConfigFile));
            string Line;
            string LastKey = "";

            // OPTIMIZATION: Use StringBuilder when storing multi-line configurations.
            while ((Line = Reader.ReadLine()) != null)
            {
                var AssignatorPos = Line.IndexOf(CONFIG_ASSIGNATOR);
                if (AssignatorPos < 0)
                    continue;

                var Key = Line.GetLeft(AssignatorPos);
                var Value = Line.Substring(AssignatorPos + 1);

                if (!Key.IsAbsent())
                {
                    CurrentConfiguration[Key] = Value;
                    LastKey = Key;
                }
                else
                    if (!LastKey.IsAbsent())
                        CurrentConfiguration[LastKey] = CurrentConfiguration[LastKey] + Environment.NewLine + Value;
            }

            Reader.Close();
        }

        /// <summary>
        /// Saves the application configuration to the specified file,
        /// or to the default configuration file-path if absent.
        /// </summary>
        public static void StoreConfigurationTo(string ConfigFile = null)
        {
            if (ConfigFile.IsAbsent())
                if (ConfigurationFilePath.IsAbsent())
                    return; // Case of the "Admin-Utils".
                else
                    ConfigFile = ConfigurationFilePath;

            StreamWriter Writer = new StreamWriter(new FileStream(ConfigFile,
                                                                  FileMode.Create,
                                                                  FileAccess.Write));

            foreach (var Configuration in CurrentConfiguration)
            {
                var ValueLines = Configuration.Value.Segment(Environment.NewLine);

                for (int Index = 0; Index < ValueLines.Length; Index++)
                    Writer.WriteLine((Index == 0 ? Configuration.Key : "") + CONFIG_ASSIGNATOR + ValueLines[Index]);
            }

            Writer.Flush();
            Writer.Close();
        }

        // -------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the application's temporary folder.
        /// </summary>
        // IMPORTANT:
        // Changes in Configuration may be used only until next application execution (see DetailAttachmentEditor).
        public static string GetTempFolder()
        {
            var Result = AppExec.GetConfiguration<string>("Application", "TempFolder", Path.GetTempPath());
            return Result;
        }

        /// <summary>
        /// Gets the earliest installation date, either from product-code or product-directories creation dates.
        /// </summary>
        public static DateTime GetEarliestInstallationDate(string ProductCode)
        {
            var AppProductDate = (new DirectoryInfo(ApplicationSpecificDataDirectory)).CreationTime;
            var UserProductDate = (new DirectoryInfo(ApplicationUserDirectory)).CreationTime;
            var InstallationDate = General.GetProductInstallationDate(ProductCode);

            InstallationDate = (InstallationDate < AppProductDate
                                ? InstallationDate : AppProductDate);
            InstallationDate = (InstallationDate < UserProductDate
                                ? InstallationDate : UserProductDate);

            return InstallationDate;
        }

        // -------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Calls the specified external File-Name and returns the generated Process object or null if failed.
        /// </summary>
        public static Process CallExternalProcess(string FileName, bool AnnounceFailure = true)
        {
            try
            {
                FileName = "\"" + FileName + "\"";  // This supports space in the file (such as in "c:\the dir\the file.html");
                return Process.Start(FileName);
            }
            catch (Exception Problem)
            {
                if (AnnounceFailure)
                    Console.WriteLine("Problem detected while calling external process: {0}", Problem.Message);
            }

            return null;
        }

        // -------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the name of the method that did the current invocation,
        /// prefixed with its type if not explicity excluded.
        /// Plus, the considered invocator method could be required to exist within a namespace.
        /// </summary>
        /// <param name="IncludeType">Indicates whether to include the type name at the beginning.</param>
        /// <param name="BaseNamespace">Namespace to consider for determine the invocator type (when not provided "Instrumind" is assumed).</param>
        /// <returns>Invocator, formatted as "[Type.]Method".</returns>
        public static string GetInvocator(bool IncludeType = true, string BaseNamespace=BASE_NAMESPACE)
        {
            string Invocator = "";
            StackFrame Invocation = null;
            System.Reflection.MethodBase InvocatorMethod = null;
            bool Found = false;
            var Trace = new StackTrace();

            for (int Ind = 1; Ind < Trace.FrameCount; Ind++)
            {
                Invocation = Trace.GetFrame(Ind);
                InvocatorMethod = Invocation.GetMethod();

                // IMPORTANT: Skip the current method. This may not work after obfuscation.
                if (InvocatorMethod.Name != "GetInvocator"
                    && InvocatorMethod.DeclaringType.Namespace.StartsWith(BaseNamespace))
                {
                    Invocation = Trace.GetFrame(Ind + 1);

                    if (!Found)
                    {
                        Found = true;
                        Invocator = Invocation.GetMethod().Name;
                    }
                    else
                        if (Invocator != Invocation.GetMethod().Name)
                            break;
                }
            }

            if (!Found)
                throw new InternalAnomaly("Cannot detect method call invocator from StackTrace within the specified namespace.",
                                           new DataWagon("Trace", Trace).Add("Namespace", BaseNamespace));

            Invocator = InvocatorMethod.Name;
            if (IncludeType)
                Invocator = InvocatorMethod.DeclaringType.FullName + "." + Invocator;

            return Invocator;
        }

        //------------------------------------------------------------------------------------------
        #region Logging

        /// <summary>
        /// Log registration policy: Save nothing in the log file.
        /// </summary>
        public const string LOGREG_NONE = "NONE";

        /// <summary>
        /// Log registration policy: Save only errors in the log file.
        /// </summary>
        public const string LOGREG_ERRORS = "ERRORS";

        /// <summary>
        /// Log registration policy: Save errors and warnings in the log file.
        /// This should be the production default setting.
        /// </summary>
        public const string LOGREG_PROBLEMS = "PROBLEMS";

        /// <summary>
        /// Log registration policy: Save relevant functionaly use, plus errors and warnings in the log file.
        /// Use this option to follow user activities and determine feature usage statistics.
        /// </summary>
        public const string LOGREG_USAGE = "USAGE";

        /// <summary>
        /// Log registration policy: Save all notifications (relevant functionality use, plus informative messages, errors and warnings) in the log file.
        /// This option can generate a lot of information and is recommended just for debugging purposes.
        /// </summary>
        public const string LOGREG_ALL = "ALL";

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Log type of entry: Logging events.
        /// </summary>
        public const string LOGTYPE_LOG = "LOG";

        /// <summary>
        /// Log type of entry: Errors.
        /// </summary>
        public const string LOGTYPE_ERROR = "ERR";

        /// <summary>
        /// Log type of entry: Warnings.
        /// </summary>
        public const string LOGTYPE_WARNING = "WRN";

        /// <summary>
        /// Log type of entry: Relevant functionality.
        /// </summary>
        public const string LOGTYPE_FUNCTION = "FNC";

        /// <summary>
        /// Log type of entry: Informative message.
        /// </summary>
        public const string LOGTYPE_MESSAGE = "MSG";

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Standard policy for writing log messages to file.
        /// </summary>
        public static string LogRegistrationPolicy = LOGREG_PROBLEMS;

        /// <summary>
        /// Writer for the application log
        /// </summary>
        private static TextWriter LogWriter = null;

        /// <summary>
        /// Writes the supplied message into the application log.
        /// </summary>
        /// <param name="Message">Text to be logged</param>
        /// <param name="Origin">Origin of the message</param>
        public static void LogMessage(string Message, string Origin = null)
        {
            General.ContractRequiresNotAbsent(Message);

            if (Origin.IsAbsent())
                if (LogRegistrationPolicy == LOGREG_ALL)
                    Origin = GetInvocator();    // Just obtain invocator when policy requires to log all (reflection is expensive)
                else
                    Origin = General.UNSPECIFIED;

            LogWrite(LOGTYPE_MESSAGE, Message, Origin);
        }

        /// <summary>
        /// Writes the supplied exception into the application log.
        /// </summary>
        /// <param name="Excep">Exception to be logged</param>
        /// <param name="Origin">Origin of the exception</param>
        public static void LogException(Exception Excep, string Origin = null)
        {
            string LogType = LOGTYPE_ERROR;
            string Message = "ERROR";

            if (Excep is WarningAnomaly)
            {
                LogType = LOGTYPE_WARNING;
                Excep = ((WarningAnomaly)Excep).UnderlyingAnomaly;
                Message = "WARNING";
            }

            var Anomaly = Excep as ControlledAnomaly;

            Message = Message + ": " + Excep.Message
                    + (Anomaly == null || Anomaly.AssociatedEvidence == null ? "" : Environment.NewLine + "\tEVIDENCE: " + Anomaly.AssociatedEvidence.ToString())
                    + (Excep.InnerException == null ? "" : Environment.NewLine + "\tINTERNAL: " + Excep.InnerException.Message)
                    + (Excep.StackTrace == null ? "" : Environment.NewLine + "\tCALLS: " + Excep.StackTrace.ToString());

            //T Clipboard.SetText(Message);
            
            if (Origin.IsAbsent())
                if (Excep.TargetSite != null)
                    Origin = Excep.TargetSite.DeclaringType.FullName + "." + Excep.TargetSite.Name;
                else
                    Origin = "Unknown-Origin";

            LogWrite(LogType, Message, Origin);
        }

        private static bool FirstWritingOfTheSession = true;

        /// <summary>
        /// Writes a new entry to the log, which always goes to the system console and then
        /// to the log file depending upon log registration policy.
        /// The lines is composed of: DateTime, Type of entry, Origin, Text.
        /// </summary>
        /// <param name="EntryType">Type of log entry</param>
        /// <param name="Text">Text to log</param>
        /// <param name="Origin">Originator of the entry (e.g. a type.method)</param>
        private static void LogWrite(string EntryType, string Text, string Origin)
        {
            string LogLine;

            if (FirstWritingOfTheSession)
            {
                FirstWritingOfTheSession = false;

                LogLine = String.Format("{0}\t{1}\t{2}\t{3}", SessionStart.AsCommonDateTime(true),
                                                              "LOG", "SYSTEM",
                                                              "*".Replicate(10) + "LOG START. Product Version:" + AppExec.ApplicationVersion + ", OS: "
                                                                                + Environment.OSVersion.ToString() + " " + "*".Replicate(10));
                //T Console.WriteLine(LogLine);
                LogFileWrite(EntryType, LogLine);
            }

            LogLine = String.Format("{0}\t{1}\t{2}\t{3}", DateTime.Now.AsCommonDateTime(true), EntryType, Origin, Text);
            //T Console.WriteLine(LogLine);
            LogFileWrite(EntryType, LogLine);
        }

        /// <summary>
        /// Writes a text into the log file depending on registration policy.
        /// </summary>
        /// <param name="EntryType">Type of log entry</param>
        /// <param name="Text">Text to log</param>
        private static void LogFileWrite(string EntryType, string Text)
        {
            bool WriteToFile = (LogRegistrationPolicy != LOGREG_NONE)
                                && ((LogRegistrationPolicy == LOGREG_ALL)
                                     || (EntryType == LOGTYPE_LOG)
                                     || (LogRegistrationPolicy == LOGREG_USAGE && EntryType != LOGTYPE_MESSAGE)
                                     || (LogRegistrationPolicy == LOGREG_ERRORS && EntryType == LOGTYPE_ERROR));

            if (WriteToFile && LogWriter!=null)
            {
                try
                {
                    LogWriter.WriteLine(Text);
                    LogWriter.Flush();
                }
                catch (Exception Problem)
                {
                    Console.WriteLine("Cannot write log. Problem: {0}.", Problem.Message);
                }
            }
        }

        #endregion

        //------------------------------------------------------------------------------------------
        public static string GenerateDetailedErrorInfo(Exception Error)
        {
            var Text = new StringBuilder(Company.NAME_SIMPLE + " " + ApplicationName); Text.AppendLine();
            Text.AppendLine();

            Text.AppendLine("APPLICATION ERROR REPORT");
            Text.AppendLine("=".Replicate(100));
            Text.AppendLine("Product Version...: " + ApplicationVersion);
            Text.AppendLine("Error Message.....: " + Error.Message.AbsentDefault("<EMPTY>"));

            Text.AppendLine();
            Text.AppendLine("=".Replicate(100));
            Text.AppendLine("ENVIRONMENT");
            Text.AppendLine("Product Version...: " + ApplicationVersion);
            Text.AppendLine("Available Memory..: " + Environment.WorkingSet);
            Text.AppendLine("OS Version........: " + Environment.OSVersion);
            Text.AppendLine(".NET Version......: " + Environment.Version);
            Text.AppendLine("Culture...........: " + System.Globalization.CultureInfo.CurrentCulture.Name);

            Text.AppendLine();
            Text.AppendLine("=".Replicate(100));
            Text.AppendLine("SOURCE EXCEPTION...");

            Action<Exception> Analyzer = null;
            Analyzer = ((excep) =>
                {
                    if (excep == null) return;

                    Text.AppendLine(General.INDENT_TEXT + "Message....: " + excep.Message);
                    Text.AppendLine(General.INDENT_TEXT + "Type.......: " + excep.GetType().FullName);
                    Text.AppendLine(General.INDENT_TEXT + "Stack-Trace: " + excep.StackTrace);

                    if (excep.InnerException != null)
                    {
                        Text.AppendLine(General.INDENT_TEXT + "Inner-Exception" + (".".Replicate(50)));
                        Analyzer(excep.InnerException);
                    }
                });

            Analyzer(Error);

            Text.AppendLine("=".Replicate(100));
            Text.AppendLine("[END]");
            
            var Result = Text.ToString();
            return Result;
        }
    }
}