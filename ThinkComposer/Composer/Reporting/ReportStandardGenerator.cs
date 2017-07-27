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
// File   : ReportStandardGenerator.cs
// Object : Instrumind.ThinkComposer.Composer.Reporting.ReportStandardGenerator (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2012.01.29 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Text;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;
using Instrumind.Common.Visualization.Widgets;

using Instrumind.ThinkComposer.ApplicationProduct;
using Instrumind.ThinkComposer.MetaModel.Configurations;
using Instrumind.ThinkComposer.MetaModel.InformationMetaModel;
using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;
using Instrumind.ThinkComposer.Model;
using Instrumind.ThinkComposer.Model.GraphModel;
using Instrumind.ThinkComposer.Composer.Reporting.Widgets;

/// Provides features for report generation.
namespace Instrumind.ThinkComposer.Composer.Reporting
{
    /// <summary>
    /// Generates comprehensive Standard reports from Compositions.
    /// </summary>
    public partial class ReportStandardGenerator
    {
        public const double PAGE_EDGES_PADDING = 2.0;

        public Composition SourceComposition { get; protected set; }
        public ReportConfiguration OriginalConfiguration { get; protected set; }    // That from the original UI thread
        public ReportConfiguration Configuration { get; protected set; }    // For the worker thread

        public int PageCount { get; protected set; }

        public string GeneratedDocumentTempFilePath { get; protected set; }

        private ThreadWorker<int> CurrentWorker { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ReportStandardGenerator(Composition SourceComposition, ReportConfiguration NewConfiguration)
        {
            this.SourceComposition = SourceComposition;

            if (NewConfiguration == null)
                NewConfiguration = new ReportConfiguration();

            this.OriginalConfiguration = NewConfiguration;
        }

        /// <summary>
        /// Generates a Report based on the current Configuration.
        /// Returns operation-result.
        /// </summary>
        // IMPORTANT: See this page about the VisualsToXpsDocument class:
        // http://msdn.microsoft.com/en-us/library/system.windows.xps.visualstoxpsdocument.aspx
        public OperationResult<int> Generate(ThreadWorker<int> Worker)
        {
            General.ContractRequiresNotNull(Worker);

            try
            {
                this.Configuration = this.OriginalConfiguration.GenerateDeepClone();    // Cloned to allow inter-thread use.

                var Result = new FixedDocument();
                IsAtGenerationStart = true;

                this.CurrentWorker = Worker;
                this.CurrentWorker.ReportProgress(0, "Starting.");

                // IMPORTANT: This page must be created to determine initial dimensions,
                //            even when the user selected to exclude it from the document.
                var TitlePage = this.CreateTitlePage(this.Configuration.DocSection_TitlePage);

                if (this.Configuration.DocSection_TitlePage)
                    Result.Pages.Add(this.CreatePageContainer(TitlePage));

                this.CurrentWorker.ReportProgress(1, "Generating Composition content.");

                // IMPORTANT: This ReportPagesMaker creation must be after the first page to get the page's dimensions.
                var PagesMaker = new ReportStandardPagesMaker(this);

                /* PENDING
                if (this.Configuration.DocSection_TableOfContents)
                    this.CreateTableOfContents().ForEach(page => Result.Pages.Add(this.CreatePageContainer(page))); */

                if (this.Configuration.DocSection_Composition)
                    this.CreateCompositeContent(PagesMaker, this.SourceComposition, 0.0, 1.0, 90.0);

                this.CurrentWorker.ReportProgress(90, "Generating Domain content.");
                if (this.Configuration.DocSection_Domain)
                    this.CreateDomainContent(PagesMaker, this.SourceComposition.CompositeContentDomain);

                this.CurrentWorker.ReportProgress(93, "Paginating.");
                var Pages = PagesMaker.GetPages();

                foreach (var Page in Pages)
                    Result.Pages.Add(this.CreatePageContainer(Page));

                this.CurrentWorker.ReportProgress(97, "Saving to temporal XPS document.");
                var FileName = General.GenerateRandomFileName(this.SourceComposition.TechName + "_TMP", "xps");
                this.GeneratedDocumentTempFilePath = Path.Combine(AppExec.ApplicationUserTemporalDirectory, FileName);

                Display.SaveDocumentAsXPS(Result, this.GeneratedDocumentTempFilePath);

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
        public double WorkingPageContentWidth { get; protected set; }

        public double WorkingPageContentHeight { get; protected set; }

        /// <summary>
        /// Indicates that the report generation is starting.
        /// This MUST be setted before the first page generation.
        /// </summary>
        public static bool IsAtGenerationStart { get; protected set; }

        public FixedPage CreatePage(FrameworkElement Content, bool VerticallyCentered = false, bool CountPage = true)
        {
            var Page = new FixedPage();

            if (CountPage)
                this.PageCount++;

            Page.PrintTicket = Configuration.PrintingTicket;
            Page.Width = Configuration.PrintingCapabilities.OrientedPageMediaWidth
                                .NullDefaultTo(Configuration.PrintingCapabilities.PageImageableArea.ExtentWidth);
            Page.Height = Configuration.PrintingCapabilities.OrientedPageMediaHeight
                                .NullDefaultTo(Configuration.PrintingCapabilities.PageImageableArea.ExtentHeight);
            Page.Margin = new Thickness((ReportConfiguration.DEFAULT_PAGE_MARGIN_CMS / 2.56) * Display.WPF_DPI);
            //- Page.HorizontalAlignment = HorizontalAlignment.Stretch;

            var PageFrame = new DockPanel();
            //- PageFrame.HorizontalAlignment = HorizontalAlignment.Stretch;
            PageFrame.Width = Page.Width - (Page.Margin.Left + Page.Margin.Right);
            PageFrame.Height = Page.Height - (Page.Margin.Top + Page.Margin.Bottom);

            var PageHeader = CreatePageHeader();
            var PageFooter = CreatePageFooter();

            var ContentFrame = new DockPanel();
            //- ContentFrame.HorizontalAlignment = HorizontalAlignment.Stretch;
            ContentFrame.Children.Add(Content);

            if (!VerticallyCentered)
            {
                ContentFrame.Children.Add(new Border());    // Filler
                DockPanel.SetDock(Content, Dock.Top);
            }

            PageHeader.UpdateVisualization(PageFrame.Width, PageFrame.Height);
            PageFooter.UpdateVisualization(PageFrame.Width, PageFrame.Height);

            PageFrame.Children.Add(PageHeader);
            PageFrame.Children.Add(PageFooter);
            PageFrame.Children.Add(ContentFrame);

            DockPanel.SetDock(PageHeader, Dock.Top);
            DockPanel.SetDock(PageFooter, Dock.Bottom);

            Page.Children.Add(PageFrame);

            Page.UpdateLayout();

            if (IsAtGenerationStart
                || this.WorkingPageContentHeight <= 0
                || this.WorkingPageContentWidth <= 0)
            {
                //- PageHeader.UpdateVisualization(PageFrame.Width, PageFrame.Height);
                //- PageFooter.UpdateVisualization(PageFrame.Width, PageFrame.Height);

                this.WorkingPageContentWidth = PageFrame.Width;
                this.WorkingPageContentHeight = PageFrame.Height - (PageHeader.ActualHeight + PageHeader.Margin.Top + PageHeader.Margin.Bottom +
                                                                    PageFooter.ActualHeight + PageFooter.Margin.Top + PageFooter.Margin.Bottom +
                                                                    16);    // This extra height is needed to avoid page bottom overflow

                if (IsAtGenerationStart)
                    IsAtGenerationStart = false;
            }

            return Page;
        }

        public PageContent CreatePageContainer(FixedPage Page)
        {
            var Result = new PageContent();
            Result.Child = Page;

            return Result;
        }

        // -----------------------------------------------------------------------------------------
        public FrameworkElement CreatePageHeader()
        {
            var Result = new DockPanel();
            Result.Margin = new Thickness(0, 0, 0, PAGE_EDGES_PADDING);

            var LabelLeft = CreateText(this.Configuration.PageHeader_Left, this.Configuration.FmtPageHeaderLabels,
                                       HorizontalAlignment.Left, VerticalAlignment.Top, null, TextWrapping.Wrap, true);

            var LabelCenter = CreateText(this.Configuration.PageHeader_Center, this.Configuration.FmtPageHeaderLabels,
                                         HorizontalAlignment.Center, VerticalAlignment.Top, null, TextWrapping.Wrap, true);

            var LabelRight = CreateText(this.Configuration.PageHeader_Right, this.Configuration.FmtPageHeaderLabels,
                                        HorizontalAlignment.Right, VerticalAlignment.Top, null, TextWrapping.Wrap, true);

            Result.Children.Add(LabelLeft);
            Result.Children.Add(LabelRight);
            Result.Children.Add(LabelCenter);

            DockPanel.SetDock(LabelLeft, Dock.Left);
            DockPanel.SetDock(LabelRight, Dock.Right);

            return Result;
        }

        public FrameworkElement CreatePageFooter()
        {
            var Result = new DockPanel();
            Result.Margin = new Thickness(0, PAGE_EDGES_PADDING, 0, 0);

            var LabelLeft = CreateText(this.Configuration.PageFooter_Left, this.Configuration.FmtPageFooterLabels,
                                       HorizontalAlignment.Left, VerticalAlignment.Bottom, null, TextWrapping.Wrap, true);

            var LabelCenter = CreateText(this.Configuration.PageFooter_Center, this.Configuration.FmtPageFooterLabels,
                                         HorizontalAlignment.Center, VerticalAlignment.Bottom, null, TextWrapping.Wrap, true);

            var LabelRight = CreateText(this.Configuration.PageFooter_Right, this.Configuration.FmtPageFooterLabels,
                                        HorizontalAlignment.Right, VerticalAlignment.Bottom, null, TextWrapping.Wrap, true);

            Result.Children.Add(LabelLeft);
            Result.Children.Add(LabelRight);
            Result.Children.Add(LabelCenter);

            DockPanel.SetDock(LabelLeft, Dock.Left);
            DockPanel.SetDock(LabelRight, Dock.Right);

            return Result;
        }

        // -----------------------------------------------------------------------------------------
        public FrameworkElement CreateText(string Text, TextFormat Format,
                                           HorizontalAlignment HorizontalAlign = HorizontalAlignment.Stretch,
                                           VerticalAlignment VerticalAlign = VerticalAlignment.Center,
                                           Brush Background = null, TextWrapping Wrapping = TextWrapping.Wrap,
                                           bool InterpretTextAsVariable = false)
        {
            var Result = new Border();
            Result.VerticalAlignment = VerticalAlign;
            Result.HorizontalAlignment = HorizontalAlign;
            /* Result.HorizontalAlignment = (HorizontalAlign == null || !HorizontalAlign.HasValue
                                          ? Format.Alignment.ToHorizontalAlignment() : HorizontalAlign.Value); */
            Result.Padding = new Thickness(CARD_TEXT_PADDING);
            Result.Background = Background;

            var Content = new TextBlock();

            if (InterpretTextAsVariable
                && Text.StartsWith(ReportingManager.VARIABLE_REF_INI)
                && Text.EndsWith(ReportingManager.VARIABLE_REF_END))
                Text = GetVariableValue(Text);

            Content.Text = Text;

            Content.Foreground = Format.ForegroundBrush;
            Content.FontFamily = Format.CurrentTypeface.FontFamily;
            Content.FontSize = Format.FontSize;
            Content.TextAlignment = Format.Alignment;
            Content.FontWeight = Format.CurrentTypeface.Weight;
            Content.FontStyle = Format.CurrentTypeface.Style;
            Content.TextDecorations = Format.GetCurrentDecorations();
            Content.TextWrapping = TextWrapping.Wrap;

            Result.Child = Content;

            return Result;
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
    }
}
