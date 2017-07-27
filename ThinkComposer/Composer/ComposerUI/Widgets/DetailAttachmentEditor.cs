using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

using Instrumind.Common;
using Instrumind.Common.Visualization;
using Instrumind.Common.Visualization.Widgets;

using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.MetaModel.InformationMetaModel;
using Instrumind.ThinkComposer.Model.GraphModel;
using Instrumind.ThinkComposer.Model.InformationModel;

namespace Instrumind.ThinkComposer.Composer.ComposerUI.Widgets
{
    /// <summary>
    /// Non-Visual detail attachment editing launcher.
    /// </summary>
    class DetailAttachmentEditor
    {
        public const int TEMPFILES_MAXINTENTS = 5;
        public const int TEMPFILES_RANDDIGITS = 10;
        public const string TEMPFILES_MIDFIX = "__TEMP";
        public static readonly string TEMPFILES_FILTER = "*" + TEMPFILES_MIDFIX + "?".Replicate(TEMPFILES_RANDDIGITS) + ".*";

        /// <summary>
        /// Static Constructor.
        /// </summary>
        static DetailAttachmentEditor()
        {
            TempFolder = AppExec.GetTempFolder();
        }

        protected static string TempFolder;
        protected static FileSystemWatcher FilesWatcher;
        protected static Dispatcher MainWindowDispatcher;

        /// <summary>
        /// Collection of exposed attachments as temporal files, where Key=FileName and Value=Attachment.
        /// </summary>
        protected static readonly Dictionary<string, Attachment> ExposedAttachmentFiles = new Dictionary<string, Attachment>();

        static void WatchExposedFiles()
        {
            if (FilesWatcher != null)
                return;

            FilesWatcher = new FileSystemWatcher(TempFolder, TEMPFILES_FILTER);
            FilesWatcher.NotifyFilter = NotifyFilters.LastWrite;
            FilesWatcher.Changed += new FileSystemEventHandler(FilesWatcher_Changed);
            FilesWatcher.EnableRaisingEvents = true;
        }

        static void FilesWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            var Worker = new Thread((param) => LoadAttachmentFile(param as string));
            Worker.Start(e.Name);
        }

        private static List<string> CurrentAttachmentBeingLoaded = new List<string>();

        public static void LoadAttachmentFile(string OriginalFileName)
        {
            Exception Failure = null;
            var FileName = OriginalFileName.ToLower();

            try
            {
                AppExec.DispatcherForUI.Invoke((Action)(() => Mouse.OverrideCursor = Cursors.Wait));

                lock (CurrentAttachmentBeingLoaded)
                {
                    if (CurrentAttachmentBeingLoaded.Contains(FileName) || !ExposedAttachmentFiles.ContainsKey(FileName))
                        return;

                    CurrentAttachmentBeingLoaded.Add(FileName);
                }

                Failure = General.TryMultipleTimes((intent) =>
                    {
                        if (intent > 1)
                            Thread.Sleep(1000);

                        var DetailAttachment = ExposedAttachmentFiles[FileName];

                        var FilePath = TempFolder + OriginalFileName;

                        try
                        {
                            DetailAttachment.Content = General.FileToBytes(FilePath);

                            AppExec.DispatcherForUI.Invoke((Action)(() =>
                                Console.WriteLine("Attachment temporal-file [{0}] has been reloaded after external modification.", FilePath)));

                            try
                            {
                                var FileRef = new FileInfo(FilePath);
                                FileRef.Delete();
                                ExposedAttachmentFiles.Remove(FileName);

                                AppExec.DispatcherForUI.Invoke((Action)(() =>
                                    Console.WriteLine("Attachment temporal-file [{0}] has been deleted.", FilePath)));
                            }
                            catch
                            {
                                AppExec.DispatcherForUI.Invoke((Action)(() =>
                                    Console.WriteLine("Attachment temporal-file [{0}] remains in-use by external application.", FilePath)));
                            }
                        }
                        catch
                        {
                            AppExec.DispatcherForUI.Invoke((Action)(() =>
                                Console.WriteLine("Cannot reaload Attachment temporal-file [{0}].", FilePath)));
                        }
                    },
                    TEMPFILES_MAXINTENTS);

                CurrentAttachmentBeingLoaded.Remove(FileName);
            }
            finally
            {
                AppExec.DispatcherForUI.Invoke((Action)(() => Mouse.OverrideCursor = null));

                if (Failure != null)
                    AppExec.DispatcherForUI.Invoke((Action)(() =>
                        Console.WriteLine("Cannot load temporal-file [{0}] while editing Attachment.\nProblem: {1}", FileName, Failure.Message)));

                // throw new ExternalAnomaly("Cannot load Temporal-file while editing Attachment.", Failure);
            }
        }

        public static ExternalAnomaly TryLoadPendingAttachmentsFor(CompositionEngine Engine)
        {
            foreach (var RegAttachment in ExposedAttachmentFiles)
                try
                {
                    LoadAttachmentFile(RegAttachment.Value.Source);
                }
                catch(Exception Failure)
                {
                    return new ExternalAnomaly("Cannot load Attachment edited by an external application.", Failure);
                }

            return null;
        }

        public static void StopWatchingAllAttachments()
        {
            if (FilesWatcher == null)
                return;

            FilesWatcher.EnableRaisingEvents = false;
        }
        
        public static void StopWatchingAttachmentsFor(CompositionEngine Engine)
        {
            var FilesToUnwatch = new List<string>();

            foreach (var RegAttachment in ExposedAttachmentFiles)
                if (RegAttachment.Value.OwnerIdea != null && RegAttachment.Value.OwnerIdea.EditEngine == Engine)
                    FilesToUnwatch.Add(RegAttachment.Key);

            foreach (var RegFile in FilesToUnwatch)
                ExposedAttachmentFiles.Remove(RegFile);
        }

        public static void ExposeAttachmentAsTemporalFile(Attachment AttachmentDetail)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            Exception Failure = null;

            try
            {
                Failure = General.TryMultipleTimes((intent) =>
                            {
                                if (intent > 1)
                                    Thread.Sleep(1000);

                                // FORMAT:  <ORIGINAL-FILENAME>__TEMP<RANDOM-NUMBER>.<ORIGINAL-EXTENSION>
                                // SAMPLE:  "Balance2009__TEMP0543991407.XLSX"
                                var TempFileName = General.GenerateRandomFileName(Path.GetFileNameWithoutExtension(AttachmentDetail.Source) + TEMPFILES_MIDFIX,
                                                                                  Path.GetExtension(AttachmentDetail.Source), TEMPFILES_RANDDIGITS);
                                var TempFilePath = TempFolder + TempFileName;
                                General.BytesToFile(TempFilePath, AttachmentDetail.Content);

                                WatchExposedFiles();

                                try
                                {
                                    Process.Start(TempFilePath);

                                    Thread.Sleep(1000);
                                    ExposedAttachmentFiles.Add(TempFileName.ToLower(), AttachmentDetail);
                                }
                                catch (Exception Problem)
                                {
                                    Console.WriteLine("Process-calling error for [{0}].\nProblem: {1}.", TempFilePath, Problem.Message);
                                }
                            },
                            TEMPFILES_MAXINTENTS);
            }
            finally
            {
                Mouse.OverrideCursor = null;

                if (Failure != null)
                    Console.WriteLine("Cannot generate Temporal-file for edit Attachment. Problem: " + Failure.Message);
                // throw new ExternalAnomaly("Cannot generate Temporal-file for edit Attachment.", Failure);
            }
        }
    }
}
