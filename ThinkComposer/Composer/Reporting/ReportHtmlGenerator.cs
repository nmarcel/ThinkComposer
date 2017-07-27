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
// File   : ReportHtmlGenerator.cs
// Object : Instrumind.ThinkComposer.Composer.Reporting.ReportHtmlGenerator (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2012.09.25 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.Model;
using Instrumind.ThinkComposer.MetaModel.Configurations;

/// Provides features for report generation.
namespace Instrumind.ThinkComposer.Composer.Reporting
{
    /// <summary>
    /// Generates comprehensive HTML reports from Compositions.
    /// </summary>
    public partial class ReportHtmlGenerator
    {
        public const string CONTENT_FOLDER_SUFFIX = ".content";

        public const string STYLE_SHEET_FILE = "imtc_style.css";

        public const int NAVLINK_TEXT_LIMIT = 35;

        public Composition SourceComposition { get; protected set; }

        public ReportConfiguration OriginalConfiguration { get; protected set; }    // That from the original UI thread
        public ReportConfiguration Configuration { get; protected set; }    // For the worker thread

        public int PageCount { get; protected set; }

        // How (and where) the final generated file is going to be named, which may differ from the source Composition name.
        // (must be asked before generation to set proper content folder name).
        public string TargetFilePath { get; protected set; }

        // Where the root document is generated
        public string GeneratedTempWorkingDocumentFile { get; protected set; }

        // Where to work. E.g.: "C:\users\<user>\temp\targetfile_tmp01027954\"
        public string GeneratedTempWorkingDocumentDir { get; protected set; }

        private ThreadWorker<int> CurrentWorker { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ReportHtmlGenerator(Composition SourceComposition, string TargetFilePath, ReportConfiguration NewConfiguration)
        {
            this.SourceComposition = SourceComposition;
            this.TargetFilePath = TargetFilePath;

            if (NewConfiguration == null)
                NewConfiguration = new ReportConfiguration();

            this.OriginalConfiguration = NewConfiguration;
        }

        /// <summary>
        /// Generates a Report based on the current Configuration.
        /// Returns operation-result.
        /// </summary>
        public OperationResult<int> Generate(ThreadWorker<int> Worker)
        {
            General.ContractRequiresNotNull(Worker);

            try
            {
                this.Configuration = this.OriginalConfiguration.GenerateDeepClone();    // Cloned to allow inter-thread use.

                var TargetFileName = Path.GetFileNameWithoutExtension(this.TargetFilePath);
                var TempFolderName = General.GenerateRandomFileName(TargetFileName + "_TMP", null) + "\\";

                this.GeneratedTempWorkingDocumentDir = Path.Combine(AppExec.ApplicationUserTemporalDirectory, TempFolderName);

                this.CurrentWorker = Worker;
                this.CurrentWorker.ReportProgress(0, "Starting.");

                General.DeleteDirectoryAndContents(this.GeneratedTempWorkingDocumentDir);

                Directory.CreateDirectory(this.GeneratedTempWorkingDocumentDir);

                // Generate Root location and style sheet
                this.CreateUniqueRelativeLocation(this.SourceComposition, null, TargetFileName);

                this.CreateHtmlStyleSheet();

                // ************************* START MAIN-PAGE ********************************
                var Page = this.PushPage(this.SourceComposition);

                if (this.Configuration.DocSection_TitlePage)
                    this.CreateTitleSection(this.Configuration.DocSection_TitlePage);

                /* PENDING
                if (this.Configuration.DocSection_TableOfContents)
                    this.CreateTableOfContents(); */

                this.CurrentWorker.ReportProgress(1, "Generating Composition content.");
                if (this.Configuration.DocSection_Composition)
                    this.CreateCompositeContent(this.SourceComposition, 1.0, 85.0);

                this.CurrentWorker.ReportProgress(85, "Generating Domain content.");
                if (this.Configuration.DocSection_Domain)
                    this.CreateDomainContent(this.SourceComposition.CompositeContentDomain);

                this.CurrentWorker.ReportProgress(90, "Writing root HTML content.");

                this.PopPage();
                this.GeneratedTempWorkingDocumentFile = this.PrepareHtmlDocument(Page);

                // ************************* END MAIN-PAGE ********************************

                this.WriteHtmlDocuments(90);

                this.CurrentWorker.ReportProgress(100, "Generation complete.");
                this.CurrentWorker = null;
            }
            catch (Exception Problem)
            {
                this.CurrentWorker = null;
                return OperationResult.Failure<int>("Cannot execute generation.\nProblem: " + Problem.Message);
            }

            return OperationResult.Success<int>(0, "Generation complete.");
        }

        // -----------------------------------------------------------------------------------------
        private readonly Stack<ReportHtmlPage> PagesStack = new Stack<ReportHtmlPage>();

        internal ReportHtmlPage CurrentPage { get { return this.PagesStack.Peek(); } }

        internal ReportHtmlPage PushPage(IRecognizableComposite Source)
        {
            this.PageCount++;

            var Page = new ReportHtmlPage(Source, this);
            this.PagesStack.Push(Page);

            this.PageWrite("<HTML>");
            this.IncreaseIndent();

            this.CreatePageHeader(Source);

            this.PageWrite("<BODY>");
            this.IncreaseIndent();

            return Page;
        }

        internal ReportHtmlPage PopPage()
        {
            this.DecreaseIndent();
            this.PageWrite("</BODY>");

            this.CreatePageFooter();

            this.DecreaseIndent();
            this.PageWrite("</HTML>");

            var Page = this.PagesStack.Pop();

            return Page;
        }

        internal int PageWrite(string Text, bool ReplaceQuotesByDoubleQuotes = true)
        {
            var Page = this.CurrentPage;

            if (ReplaceQuotesByDoubleQuotes)
                Text = Text.Replace("'", "\"");

            Text = General.INDENT_TEXT.Replicate(Page.IndentLevel) + Text;

            Page.Content.Add(Text);

            var Result = Page.Content.Count - 1;
            return Result;
        }

        internal void IncreaseIndent()
        {
            if (this.PagesStack.Count < 1)
                return;

            this.CurrentPage.IndentLevel++;
        }

        internal void DecreaseIndent()
        {
            if (this.PagesStack.Count < 1)
                return;

            this.CurrentPage.IndentLevel--;
        }

        // -----------------------------------------------------------------------------------------
        public void CreatePageHeader(IRecognizableComposite Source)
        {
            this.PageWrite("<HEAD>");
            this.IncreaseIndent();

            var Location = this.GetRelativeLocationOf(this.SourceComposition, STYLE_SHEET_FILE, this.CurrentPage.Source);
            this.PageWrite("<link rel='stylesheet' href='" + Location + "' type='text/css' />");

            if (Source != null)
            {
                var Route = Source.GetContainmentNodes(true);
                var Navigator = Route.GetConcatenationIndexed((item, index) => "<a href=\"" +
                                                                               GetRelativeLocationOf(item, null, Source) +
                                                                               "\">" + item.NameCaption.GetTruncatedWithEllipsis(NAVLINK_TEXT_LIMIT) + "</a>", " / ");
                // looks ugly ni chrome
                this.PageWrite("<p class=\"extras\">At: / " + Navigator + "</p>");
            }

            this.PageWrite("<table style='width: " + HTML_STD_PAGE_WIDTH.ToString() + "px;'>");
            this.PageWrite("<col style='width: 30%;'/>");
            this.PageWrite("<col style='width: 40%;'/>");
            this.PageWrite("<col style='width: 30%;'/>");
            this.PageWrite("<td class='header-left'>" + InterpretText(this.Configuration.PageHeader_Left) + "</td>");
            this.PageWrite("<td class='header-center'>" + InterpretText(this.Configuration.PageHeader_Center) + "</td>");
            this.PageWrite("<td class='header-right'>" + InterpretText(this.Configuration.PageHeader_Right) + "</td>");
            this.PageWrite("</table>");

            this.DecreaseIndent();
            this.PageWrite("</HEAD>");
        }

        public void CreatePageFooter()
        {
            this.PageWrite("<FOOTER>");
            this.IncreaseIndent();

            this.PageWrite("<table style='width: " + HTML_STD_PAGE_WIDTH.ToString() + "px;'>");
            this.PageWrite("<col style='width: 30%;'/>");
            this.PageWrite("<col style='width: 40%;'/>");
            this.PageWrite("<col style='width: 30%;'/>");
            this.PageWrite("<td class='footer-left'>" + InterpretText(this.Configuration.PageFooter_Left) + "</td>");
            this.PageWrite("<td class='footer-center'>" + InterpretText(this.Configuration.PageFooter_Center) + "</td>");
            this.PageWrite("<td class='footer-right'>" + InterpretText(this.Configuration.PageFooter_Right) + "</td>");
            this.PageWrite("</table>");

            this.DecreaseIndent();
            this.PageWrite("</FOOTER>");
        }

        // -----------------------------------------------------------------------------------------
        public string InterpretText(string Text)
        {
            if (Text.StartsWith(ReportingManager.VARIABLE_REF_INI)
                && Text.EndsWith(ReportingManager.VARIABLE_REF_END))
                Text = GetVariableValue(Text);

            return Text;
        }

        public void CreateTextLabel(string DivId, string Text)
        {
            //- style='width: 1000px; overflow: hidden; white-space: nowrap;'
            var Label = "<br/><div id='" + DivId + "'>" + Text + "</div><br/>";
            this.PageWrite(Label);
        }

        // -----------------------------------------------------------------------------------------
        public string GetVariableValue(string VariableName)
        {
            var Result = VariableName;

            if (VariableName.IndexOf(ReportingManager.VARIABLE_REF_INI) < 0 || VariableName.IndexOf(ReportingManager.VARIABLE_REF_END) < 0)
                return VariableName;

            VariableName = VariableName.CutBetween(ReportingManager.VARIABLE_REF_INI, ReportingManager.VARIABLE_REF_END).ToUpper().Trim()
                            .Replace(" ", "_").Replace("-", "_");

            if (VariableName == "DOMAIN")
                Result = this.SourceComposition.CompositeContentDomain.Name;

            if (VariableName == "COMPOSITION" || VariableName == "COMPOSITION_NAME")
                Result = this.SourceComposition.Name;

            if (VariableName == "COMPOSITION_TECHNAME")
                Result = this.SourceComposition.TechName;

            if (VariableName == "COMPOSITION_SUMMARY")
                Result = this.SourceComposition.Summary;

            if (VariableName == "VERSION")
                Result = "Version: " + this.SourceComposition.Version.VersionNumber;

            if (VariableName == "AUTHOR")
                Result = "Creator: " + this.SourceComposition.Version.Creator +
                         " /Modifier:" + this.SourceComposition.Version.LastModifier;

            if (VariableName == "DATE")
                Result = "Creation: " + this.SourceComposition.Version.Creation.ToShortDateString() +
                         " /Modification:" + this.SourceComposition.Version.LastModification.ToShortDateString();

            if (VariableName == "PAGE")
                Result = this.PageCount.ToString();

            return Result;
        }

        // -----------------------------------------------------------------------------------------
        /// <summary>
        /// Registered locations as Key=[Source], Value=[Dictionary{MemberName, RelativeUniqueName}]
        /// Example: Key=[Saturn], Value=[{"Pictogram", "pictogram"}, {"Details.RingsImage", "details_ringsimage2"}]
        /// For the source-object (without referencing memeber) use "." as memeber-name.
        /// </summary>
        private readonly Dictionary<object, Dictionary<string, string>> RegisteredLocations
                   = new Dictionary<object, Dictionary<string, string>>();

        /// <summary>
        /// Registered dependents (such as Markers or Complements) of a composite Idea.
        /// </summary>
        private Dictionary<object, IRecognizableComposite> RegisteredDependents
              = new Dictionary<object, IRecognizableComposite>();

        /// <summary>
        /// Creates a unique location for the specified Source object, its relative-name and an optional specified member-name and relative-name.
        /// Example: Owner=IdeaX, Source=MarkerZ, "Pictogram", "Pictogram.png"
        /// Returns true if created, or false if already created.
        /// </summary>
        public bool CreateUniqueRelativeLocation(IRecognizableComposite Owner, object Source, string MemberName, string RelativeName)
        {
            if (!this.RegisteredDependents.ContainsKey(Source))
                this.RegisteredDependents.Add(Source, Owner);

            return this.CreateUniqueRelativeLocationDirect(Source, MemberName, RelativeName);
        }

        /// <summary>
        /// Creates a unique location for the specified Source Recognizable-Composite with the optional specified member-name and relative-name.
        /// Example: Source=IdeaX, MemberName="MainView", InitialRelativeName="MainView.png"
        /// Returns true if created, or false if already created.
        /// </summary>
        public bool CreateUniqueRelativeLocation(IRecognizableComposite Source, string MemberName = null, string InitialRelativeName = null)
        {
            return this.CreateUniqueRelativeLocationDirect(Source, MemberName, InitialRelativeName);
        }

        /// <summary>
        /// Creates a unique location for the specified Source object with the specified member-name and initial relative-name.
        /// Example: MarkerX, "Pictogram", "Pictogram.png"
        /// Returns true if created, or false if already created.
        /// </summary>
        public bool CreateUniqueRelativeLocationDirect(object Source, string MemberName, string InitialRelativeName)
        {
            if (MemberName == null)
                MemberName = ".";
            else
                MemberName = MemberName.TextToUrlIdentifier();

            if (this.RegisteredLocations.ContainsKey(Source)
                && this.RegisteredLocations[Source].ContainsKey(MemberName))
                return false;

            string RelativeUniqueLocation = null;

            if ((Source == this.SourceComposition
                 || (Source is IRecognizableComposite && ((IRecognizableComposite)Source).CompositeParent == null))
                && MemberName == ".")
                RelativeUniqueLocation = Path.GetFileNameWithoutExtension(this.TargetFilePath).TextToUrlIdentifier();
            else
            {
                var RelativeName = (InitialRelativeName.IsAbsent()
                                    ? (MemberName == "."
                                       ? (Source is IIdentifiableElement
                                          ? ((IIdentifiableElement)Source).TechName
                                          : Source.ToStringAlways()).TextToUrlIdentifier()
                                       : MemberName)
                                    : InitialRelativeName);

                RelativeUniqueLocation = GenerateDerivedUniqueRelativeName(Source, RelativeName);
            }

            Dictionary<string, string> SubDict = null;

            if (this.RegisteredLocations.ContainsKey(Source))
                SubDict = this.RegisteredLocations[Source];
            else
            {
                SubDict = new Dictionary<string, string>();
                this.RegisteredLocations.Add(Source, SubDict);
            }

            SubDict.Add(MemberName, RelativeUniqueLocation);

            return true;
        }

        /// <summary>
        /// Gets the physical (file) location of the specified Target object and optional Member-Name.
        /// </summary>
        public string GetPhysicalLocationOf(object Target, string MemberName = null)
        {
            if (MemberName == null)
                MemberName = ".";
            else
                MemberName = MemberName.TextToUrlIdentifier();

            var Location = this.RegisteredLocations[Target][MemberName];

            if (!(Target is IRecognizableComposite))    // if is dependent of a composite
                Target = this.RegisteredDependents[Target];

            if (MemberName == ".")
                Location = Location + ".html";
            else
                Location = this.RegisteredLocations[Target]["."] + CONTENT_FOLDER_SUFFIX + "\\" + Location;

            var CurrentTarget = ((IRecognizableComposite)Target).CompositeParent;
            while (CurrentTarget != null)
            {
                Location = this.RegisteredLocations[CurrentTarget]["."] + CONTENT_FOLDER_SUFFIX + "\\" + Location;
                CurrentTarget = CurrentTarget.CompositeParent;
            }

            Location = Path.Combine(this.GeneratedTempWorkingDocumentDir, Location);

            return Location;
        }

        // Pending References: Dictionary[html-page, List<{page-content-line, target-object, member-name, temp-refcode}>]
        internal readonly Dictionary<ReportHtmlPage,List<Tuple<int, object, string, string>>> PendingReferences =
                      new Dictionary<ReportHtmlPage,List<Tuple<int,object,string,string>>>();

        public void RegisterPendingReference(int PageContentLine, string TempRefCode, object Target, string MemberName = null)
        {
            List<Tuple<int, object, string, string>> PagePendingRefs = null;

            if (this.PendingReferences.ContainsKey(this.CurrentPage))
                PagePendingRefs = this.PendingReferences[this.CurrentPage];
            else
            {
                PagePendingRefs = new List<Tuple<int, object, string, string>>();
                this.PendingReferences.Add(this.CurrentPage, PagePendingRefs);
            }

            PagePendingRefs.Add(Tuple.Create(PageContentLine, Target, MemberName, TempRefCode));
        }

        /// <summary>
        /// Gets the relative location (link identifier) of the specified Target object, plus optional Member-Name and Requester.
        /// Returns null when target has generation pending.
        /// </summary>
        public string GetRelativeLocationOf(object Target, string MemberName = null, IRecognizableComposite Requester = null)
        {
            if (MemberName == null)
                MemberName = ".";
            else
                MemberName = MemberName.TextToUrlIdentifier();

            if (!this.RegisteredLocations.ContainsKey(Target)
                || !this.RegisteredLocations[Target].ContainsKey(MemberName))
                return null;    // not yet registered

            var RelativeUniqueLocation = this.RegisteredLocations[Target][MemberName];

            if (MemberName == ".")
                RelativeUniqueLocation = RelativeUniqueLocation + ".html";

            var CompositeTarget = ((Target is IRecognizableComposite)
                                    ? (IRecognizableComposite)Target
                                    : this.RegisteredDependents[Target]);

            var AccessPath = (MemberName == "." ? "" :
                              this.RegisteredLocations[CompositeTarget]["."] + CONTENT_FOLDER_SUFFIX + "/");

            CompositeTarget = CompositeTarget.CompositeParent;
            while (CompositeTarget != null)
            {
                AccessPath =  this.RegisteredLocations[CompositeTarget]["."] + CONTENT_FOLDER_SUFFIX + "/" + AccessPath;
                CompositeTarget = CompositeTarget.CompositeParent;
            }

            RelativeUniqueLocation = AccessPath + RelativeUniqueLocation;

            if (Requester == null && this.CurrentPage.Source != this.SourceComposition)
                Requester = this.CurrentPage.Source;

            if (Requester == null)
                RelativeUniqueLocation = "./" + RelativeUniqueLocation;
            else
                RelativeUniqueLocation = "../".Replicate(Requester.CompositeDepthLevel - 1) + RelativeUniqueLocation;

            return RelativeUniqueLocation;
        }

        /// <summary>
        /// Returns a new relative-name, appending a nummeric suffix, if the provided one is not unique.
        /// </summary>
        private string GenerateDerivedUniqueRelativeName(object Source, string RelativeName)
        {
            int Suffix = 1;
            string NewRelativeName = RelativeName;

            if (this.RegisteredDependents.ContainsKey(Source))
                Source = this.RegisteredDependents[Source];

            if (this.RegisteredLocations.ContainsKey(Source))
                while (this.RegisteredLocations[Source].ContainsValue(NewRelativeName))
                {
                    Suffix++;

                    var LastDotPos = RelativeName.LastIndexOf(".");
                    if (LastDotPos > 0 && LastDotPos < RelativeName.Length - 1)
                        NewRelativeName = RelativeName.GetLeft(LastDotPos) +
                                            Suffix.ToString() + "." + RelativeName.Substring(LastDotPos + 1);
                    else
                        NewRelativeName = Suffix.ToString() + RelativeName;
                };

            return NewRelativeName;
        }

        // -----------------------------------------------------------------------------------------
        // Rows=[Value], ColumnDefinitions=[Tech-Name, Name, Width-Percentage, Row-Field-Extractor]
        public void PageWriteTable<TRow>(string ClassName, IRecognizableComposite SourceOwner, IEnumerable<TRow> Rows,
                                         params Capsule<string, string, double, Func<TRow, IMModelClass>>[] ColumnDefinitions)
              where TRow : IMModelClass
        {
            var Records = Rows.Select(
                    row =>
                        {
                            var Values = new List<string>();

                            // Travel Colum-Defs...
                            foreach (var ColValueDef in ColumnDefinitions)
                            {
                                var Entity = (ColValueDef.Value3 == null
                                              ? row : ColValueDef.Value3(row));

                                var Spec = "";
                                if (Entity != null)
                                {
                                    var ColDef = Entity.ClassDefinition.GetPropertyDef(ColValueDef.Value0, false);

                                    var Value = (ColDef == null ? null : ColDef.Read(Entity));
                                    Spec = GetRecordValueAsHtmlString(Value, SourceOwner, row);
                                }

                                Values.Add(Spec);
                            }

                            return (IEnumerable<string>)Values;
                        });

            var ColDefs = ColumnDefinitions.Select(
                            def => Capsule.Create(def.Value0, def.Value1, def.Value2)).ToArray();

            this.PageWriteTable(ClassName, Records, ColDefs);
        }

        // -----------------------------------------------------------------------------------------
        // Rows=[Value], ColumnDefinitions=[Tech-Name, Name, Width-Percentage]
        public void PageWriteTable(string ClassName, IEnumerable<IEnumerable<string>> Rows,
                                   params Capsule<string, string, double>[] ColumnDefinitions)
        {
            if (!ColumnDefinitions.Any() || !Rows.Any())
                return;

            // Recalc column widths
            var PercentsSum = 0.0;

            foreach (var ColDef in ColumnDefinitions)
                PercentsSum += ColDef.Value2;

            if (PercentsSum != 100.0)
                foreach (var ColDef in ColumnDefinitions)
                    ColDef.Value2 = ((ColDef.Value2 * 100) / PercentsSum);

            // Generate
            this.PageWrite("<table class='" + ClassName + "'>");
                this.IncreaseIndent();

                foreach (var ColDef in ColumnDefinitions)
                {
                    // Notice the use of Culture.InvariantCulture.NumberFormat as the HTML parsers requires standard number formatting.
                    var Spec = "<col style='width: " + Math.Round(ColDef.Value2, 2).ToString(CultureInfo.InvariantCulture.NumberFormat) + "%;' />";
                    this.PageWrite(Spec);
                }

                this.PageWrite("<thead>");
                    this.IncreaseIndent();

                    this.PageWrite("<tr>");
                        this.IncreaseIndent();

                        foreach (var ColDef in ColumnDefinitions)
                        {
                            var Spec = "<th>" + ColDef.Value1 + "</th>";
                            this.PageWrite(Spec);
                        }

                        this.DecreaseIndent();
                    this.PageWrite("</tr>");

                    this.DecreaseIndent();
                this.PageWrite("</thead>");

                this.PageWrite("<tbody>");
                    this.IncreaseIndent();

                    foreach (var Row in Rows)
                    {
                        this.PageWrite("<tr>");
                            this.IncreaseIndent();

                            foreach (var Cell in Row)
                            {
                                this.PageWrite("<td>");
                                    this.IncreaseIndent();
                                    this.PageWrite(Cell);
                                    this.DecreaseIndent();
                                this.PageWrite("</td>");
                            }

                            this.DecreaseIndent();
                        this.PageWrite("</tr>");
                    }

                    this.DecreaseIndent();
                this.PageWrite("</tbody>");

                this.DecreaseIndent();
            this.PageWrite("</table>");
        }

        // -----------------------------------------------------------------------------------------
        private List<string> ValidDirectories = new List<string>();

        public void PrepareToWrite(string Location = null)
        {
            if (Location.IsAbsent())
                Location = this.CurrentPage.PageContentDir;
            else
                Location = Path.GetDirectoryName(Location);

            if (ValidDirectories.Contains(Location))
                return;

            if (!Directory.Exists(Location))
                Directory.CreateDirectory(Location);

            ValidDirectories.Add(Location);
        }

        // -----------------------------------------------------------------------------------------
        internal string PrepareHtmlDocument(ReportHtmlPage Page)
        {
            this.PrepareToWrite(Page.PhysicalLocation);

            this.GeneratedPages.Add(Tuple.Create(Page.PhysicalLocation, Page));

            return Page.PhysicalLocation;
        }

        internal readonly List<Tuple<string, ReportHtmlPage>> GeneratedPages = new List<Tuple<string, ReportHtmlPage>>();

        internal void WriteHtmlDocuments(double ProgressPercentage)
        {
            var ProgressStep = ((100.0 - ProgressPercentage) / (double)this.GeneratedPages.Count);

            foreach (var Page in this.GeneratedPages)
            {
                this.CurrentWorker.ReportProgress((int)ProgressPercentage, "Writing HTML Document: " + Page.Item1);

                if (this.PendingReferences.ContainsKey(Page.Item2))
                    foreach(var PendingRef in this.PendingReferences[Page.Item2])
                    {
                        var ValidReference = this.GetRelativeLocationOf(PendingRef.Item2, PendingRef.Item3, Page.Item2.Source);

                        Page.Item2.Content[PendingRef.Item1] =
                            Page.Item2.Content[PendingRef.Item1].Replace(PendingRef.Item4, ValidReference);
                    }

                File.WriteAllLines(Page.Item1, Page.Item2.Content, Encoding.UTF8);

                ProgressPercentage += ProgressStep;
            }
        }

        // -----------------------------------------------------------------------------------------
    }
}
