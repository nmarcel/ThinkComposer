// -------------------------------------------------------------------------------------------
// Instrumind ThinkComposer v1.0
// Copyright (C) 2009-2011 Néstor Marcel Sánchez Ahumada, Santiago, Chile.
// http://www.Instrumind.com
//
// All rights reserved.
// Todos los derechos reservados.
// -------------------------------------------------------------------------------------------
//
// This source code is private property of Néstor Marcel Sánchez Ahumada. It is prohibited the
// use or copy of any part of its content without the written permission of the owner.
// Protected by intellectual property laws and related international treaties.
//
// Este código fuente es propiedad privada de Néstor Marcel Sánchez Ahumada. Está prohibido el
// uso o copia de cualquier parte de su contenido sin el permiso escrito del dueño.
// Protegido por leyes de propiedad intelectual y tratados internacionales relacionados.
// -------------------------------------------------------------------------------------------
//
// Project: Instrumind ThinkComposer v1.0
// File   : ProductDirector.cs
// Object : Instrumind.ThinkComposer.ProductDirector (Class)
//
// Date       Author             Comments
// ---------- ------------------ -------------------------------------------------------------
// 2009.06.30 Néstor Sánchez A.  Start
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;
using Instrumind.Common.Visualization.Widgets;

using Instrumind.ThinkComposer.ApplicationProduct.Widgets;
using Instrumind.ThinkComposer.Definitor;
using Instrumind.ThinkComposer.Definitor.DefinitorMaintenance;
using Instrumind.ThinkComposer.Composer;
using Instrumind.ThinkComposer.MetaModel;
using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.MetaModel.InformationMetaModel;
using Instrumind.ThinkComposer.Model;
using Instrumind.ThinkComposer.Model.GraphModel;
using Instrumind.ThinkComposer.Model.InformationModel;
using Instrumind.ThinkComposer.Model.VisualModel;
using System.Windows.Input;
using System.Windows.Media.Effects;

/// Main manager for the Instrumind ThinkComposer product application.
namespace Instrumind.ThinkComposer.ApplicationProduct
{
    /// <summary>
    /// Centralized manager for the application Product.
    /// Brings to life it using the provided shell.
    /// </summary>
    public static partial class ProductDirector
    {
        // -------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Obtains and applies the action requested by the application command-line arguments.
        /// </summary>
        public static void ProcessCommandLineArguments()
        {
            //Skips the first argument which is the .exe file name
            var CommandLineArguments = Environment.GetCommandLineArgs();
            ApplicationExecutableFileName = CommandLineArguments.First();
            var Parameters = CommandLineArguments.Skip(1);

            foreach (var Argument in Parameters)
                try
                {
                    var FilePath = Argument.ToStringAlways().Trim();

                    if (Path.GetDirectoryName(FilePath).IsAbsent())
                        FilePath = Path.Combine(Environment.CurrentDirectory, FilePath);

                    var Location = new Uri(FilePath, UriKind.Absolute);

                    if (FilePath.EndsWith("." + Composition.FILE_EXTENSION_COMPOSITION))
                        CompositionDirector.OpenComposition(Location, false);
                    else
                        if (FilePath.EndsWith("." + Domain.FILE_EXTENSION_DOMAIN))
                            CompositionDirector.OpenDomainAndCreateCompositionOfIt(true, Location, false);
                }
                catch (Exception Problem)
                {
                    Display.DialogMessage("Warning!", "Cannot read the file with the specified name.\nProblem: "
                                                      + Problem.Message, EMessageType.Warning);
                    continue;
                }
        }

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Provides top-level error management for any unhandled exceptions detected from the hosting WPF application.
        /// </summary>
        /// <param name="sender">Original sender of the exception.</param>
        /// <param name="evargs">Exception parameters</param>
        public static void ApplicationUnhandledExceptionHandler(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs evargs)
        {
            string Caption = "Error!";
            var MessageType = EMessageType.Error;

            if (evargs.Exception is WarningAnomaly)
            {
                Caption = "Warning";
                MessageType = EMessageType.Warning;
            }

            AppExec.LogException(evargs.Exception);
            Display.DialogMessage(Caption, "PROBLEM: " + evargs.Exception.Message +
                                  (evargs.Exception.InnerException == null ? "" : "\nANOMALY: " + evargs.Exception.InnerException.Message),
                                   MessageType);

            evargs.Handled = true;
            if (!(evargs.Exception is WarningAnomaly))
                Stop(evargs.Exception);
        }

        // -------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Shows the product "About" dialog.
        /// </summary>
        public static void ShowAbout()
        {
            DialogOptionsWindow Dialog = null;
            Display.OpenContentDialogWindow<About>(ref Dialog, "About...");
        }

        /// <summary>
        /// Shows the Help content.
        /// </summary>
        public static void ShowHelp()
        {
            DialogOptionsWindow Dialog = null;
            Display.OpenContentDialogWindow<AppHelp>(ref Dialog, "Help...");
        }

        public static void ShowAssistance(string Message = null)
        {
            if (StatusBarControl == null)
                return;

            Message = Message.NullDefault(STD_TEXT_STATUS);

            if (Message != StatusBarControl.AssistanceText.Text)
                StatusBarControl.AssistanceText.Text = Message;
        }

        public static void EntitleApplication(string CustomTile = null)
        {
            Application.Current.MainWindow.Title = (CustomTile.IsAbsent() ? "" : CustomTile + " - ")
                                                   + APPLICATION_NAME
                                                   + (AppExec.LicenseMode.TechName.IsOneOf(AppExec.LIC_MODE_PERMANENT, AppExec.LIC_MODE_SUBSCRIPTION)
                                                      ? "" : " (" + AppExec.LicenseMode.Name + ")")
                                                   + (AppExec.LicenseType.TechName != AppExec.LIC_TYPE_COMMERCIAL
                                                      ? "" : " (" + AppExec.LicenseType.Name + ")");

            if (!AppExec.LicenseMode.TechName.IsOneOf(AppExec.LIC_MODE_PERMANENT, AppExec.LIC_MODE_SUBSCRIPTION))
                ProductDirector.StatusBarControl.SetModeText(AppExec.LicenseMode.Name.Intercalate());   // Intercalating because "Trial" and "Beta" are short words
            else
                if (AppExec.LicenseType.TechName != AppExec.LIC_TYPE_COMMERCIAL)
                    ProductDirector.StatusBarControl.SetModeText(AppExec.LicenseType.Name,
                                                                 (AppExec.LicenseType.TechName == AppExec.LIC_TYPE_EDUCATIONAL
                                                                  ? WidgetStatusBar.StampStyle.Green
                                                                  : WidgetStatusBar.StampStyle.Blue));
                else
                    ProductDirector.StatusBarControl.SetModeText(null);
        }

        /// <summary>
        /// Shows the product "Options" dialog.
        /// </summary>
        public static void EditOptions()
        {
            DialogOptionsWindow Dialog = null;
            Display.OpenContentDialogWindow<AppOptions>(ref Dialog, "Options...");
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Shows a message about the need to do an immediate application of changes on the editing target, such as the next-dialog or specific control.
        /// Hence, the user is notified about a non-cancellable operation and can stop or continue, plus indicating "don't show/ask again".
        /// </summary>
        /// <param name="DataElementName">Name of the data element to be changed (informative to the user).</param>
        /// <param name="ConfigScope">Scope key for grouping configuration information.</param>
        /// <param name="ConfigOptionValueCode">Option code that will be used to get a default option, and later to save the last selected one.</param>
        /// <param name="EditingTarget">Indicates what Control is/will-be the change applier.</param>
        /// <returns>Indication of continuation (true) or cancellation (false)</returns>
        public static bool ConfirmImmediateApply(string DataElementName, string ConfigScope, string ConfigOptionValueCode, string EditingTarget = "the Next window")
        {
            var Result = Display.DialogPersistableMultiOption("Warning", "Confirm change of " + DataElementName + ".\n" +
                                                              "Changes in " + EditingTarget + " will be applied directly, despite a Cancel on this one.", null,
                                                              ConfigScope, ConfigOptionValueCode, "Cancel",
                                                              new SimplePresentationElement("OK", "OK", "Proceed, and apply changes directly.", Display.GetAppImage("accept.png")),
                                                              new SimplePresentationElement("Cancel", "Cancel", "Do not edit.", Display.GetAppImage("cancel.png")));
            return (Result != null && Result == "OK");
        }

        // -------------------------------------------------------------------------------------------------------------
        public const string UPDATES_BASE = "http://instrumind.blob.core.windows.net/thinkcomposer/";

        public const string VERSIONING_FILE = ProductDirector.APPLICATION_NAME + ".ver";
        public const string VERSIONING_SOURCE = UPDATES_BASE + VERSIONING_FILE;

        public const string SETUP_FILE = "setup_imtc.exe";
        public const string SETUP_SOURCE = UPDATES_BASE + SETUP_FILE;

        public static string LocalVersioningSource
        {
            get
            {
                return Path.Combine(AppExec.ApplicationsTemporalDirectory,
                                    ProductDirector.VERSIONING_FILE);
            }
        }

        public static string LocalSetupSource
        {
            get
            {
                return Path.Combine(AppExec.ApplicationsTemporalDirectory,
                                    ProductDirector.SETUP_FILE);
            }
        }

        public static bool LaunchNewVersionSetupOnExit = false;

        // -------------------------------------------------------------------------------------------------------------
        internal static void ValidateLicense()
        {
            DateTime StartDate = General.EMPTY_DATE;

            var AppExecFieInfo = new FileInfo(ApplicationExecutableFileName);
            StartDate = AppExecFieInfo.CreationTime;

            if (DateTime.Now > AppExec.LicenseExpiration)
            {
                Display.DialogMessage("Attention",
                                      "Application has expired.\n", EMessageType.Error);
                Stop();
                return;
            }
        }

        public static bool ProductUpdateIsNeeded(IList<string> VersionFileLines)
        {
            try
            {
                if (VersionFileLines.Count < 1 || !VersionFileLines[0].StartsWith("#"))
                    throw new Exception("Versioning file does not starts with '#version-number'.");

                var CurVerParts = General.GetVersionNumberParts(APPLICATION_VERSION);
                var CurVerValue = Int32.Parse(CurVerParts.Item1.ToString("00") +
                                              CurVerParts.Item2.ToString("00") +
                                              CurVerParts.Item3.ToString("0000"));

                var NewVerParts = General.GetVersionNumberParts(VersionFileLines[0].Substring(1));
                var NewVerValue = Int32.Parse(NewVerParts.Item1.ToString("00") +
                                              NewVerParts.Item2.ToString("00") +
                                              NewVerParts.Item3.ToString("0000"));

                if (CurVerValue < NewVerValue)
                    return true;

            }
            catch (Exception Problem)
            {
                Console.WriteLine("Cannot determine if update is needed. Problem: " + Problem.Message);
            }

            return false;
        }

        public static void ProductUpdateDaylyDetection()
        {
            // Pending: As in Paint.NET, force to be execute only once per day (not per execution).
            try
            {
                var LastDetection = AppExec.GetConfiguration<DateTime>("Application", "LastUpdateCheck");
                if (LastDetection.Date >= DateTime.Now.Date)
                    return;

                if (File.Exists(ProductDirector.LocalVersioningSource))
                    File.Delete(ProductDirector.LocalVersioningSource);

                General.DownloadFileAsync(new Uri(ProductDirector.VERSIONING_SOURCE, UriKind.Absolute),
                                          ProductDirector.LocalVersioningSource, evargs => true,
                                          result =>
                                          {
                                              try
                                              {
                                                  if (result.WasSuccessful)
                                                  {
                                                      AppExec.SetConfiguration<DateTime>("Application", "LastUpdateCheck", DateTime.Now.Date, true);

                                                      var VersionFileText = General.FileToString(ProductDirector.LocalVersioningSource);
                                                      var VersionFileLines = General.StringToStrings(VersionFileText);

                                                      if (ProductDirector.ProductUpdateIsNeeded(VersionFileLines))
                                                      {
                                                          Console.WriteLine("There is a new version available: " + VersionFileLines[0]);

                                                          /*- var Result = Display.DialogMessage("Attention!", "There is a new version available for update this application.\n" +
                                                                                                               "Do you want to download and install it now?", EMessageType.Question,
                                                                                                               System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxResult.Yes); */

                                                          var Result = Display.DialogPersistableMultiOption("Attention!", "There is a new version available for update this application.\n" +
                                                                                                                          "Do you want to download and install it now?", null,
                                                                          "Application", "AskForUpdateOnStart", "Cancel",
                                                                          new SimplePresentationElement("OK", "OK", "Proceed, and apply changes directly.", Display.GetAppImage("accept.png")),
                                                                          new SimplePresentationElement("Cancel", "Cancel", "Do not edit.", Display.GetAppImage("cancel.png")));

                                                          if (Result != null && Result == "OK") ;
                                                          AppVersionUpdate.ShowAppVersionUpdate(VersionFileText);
                                                      }
                                                      else
                                                          Console.WriteLine("You are using the latest version available. No update required.");
                                                  }
                                                  else
                                                      Console.WriteLine("Unable to detect new application version.");
                                              }
                                              catch (Exception Problem)
                                              {
                                                  Console.WriteLine("Cannot detect new application version. Problem: " + Problem.Message);
                                              }
                                          });
            }
            catch (Exception Problem)
            {
                Console.WriteLine("Error: Cannot start detection of new version.\n\nProblem: " + Problem.Message);
            }
        }

        // -------------------------------------------------------------------------------------------------------------
    }

}