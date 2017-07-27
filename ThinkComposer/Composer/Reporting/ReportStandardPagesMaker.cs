using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

using Instrumind.Common;
using Instrumind.Common.Visualization;

namespace Instrumind.ThinkComposer.Composer.Reporting
{
    public class ReportStandardPagesMaker
    {
        public const double SECTION_HEADER_MARGIN = 16;

        public const string PAGE_TOP_CONTENT_TAG = "$PAGE_TOP$";   // Tag code which indicates that a content is at page-top

        public ReportStandardGenerator OwnerGenerator { get; protected set; }

        public double PageRemainingHeight { get; protected set; }

        public bool IsAtPageStart { get; protected set; }

        public double NestingMargin { get; set; }

        protected List<Dictionary<FrameworkElement, string>> PageBlocks = new List<Dictionary<FrameworkElement, string>>();

        public Func<FrameworkElement> PageBreakStartCreator = null;

        public ReportStandardPagesMaker(ReportStandardGenerator OwnerGenerator)
        {
            this.OwnerGenerator = OwnerGenerator;
            this.PageRemainingHeight = this.OwnerGenerator.WorkingPageContentHeight;
            this.IsAtPageStart = true;
        }

        private Dictionary<FrameworkElement, string> CurrentPageBlock = new Dictionary<FrameworkElement, string>();

        public void AppendPageBreak()
        {
            this.PageRemainingHeight = 0;
        }

        // NOTE: Always enforce limits for scalable content (such as images)
        public void AppendContent(FrameworkElement Content, bool InsertSectionMargin = false, string GroupKey = null, bool EnforceLimits = false)
        {
            if (!this.IsAtPageStart && InsertSectionMargin)
            {
                var Filler = new Border();
                Filler.Height = SECTION_HEADER_MARGIN;
                this.AppendContent(Filler);
            }

            this.AppendContent(Content, GroupKey, false, EnforceLimits);
        }

        // NOTE: Always enforce limits for scalable content (such as images)
        protected void AppendContent(FrameworkElement Content, string GroupKey, bool IsRecursiveHeaderCreationCall, bool EnforceLimits = false)
        {
            this.IsAtPageStart = false;

            Content.Margin = new Thickness(Content.Margin.Left + this.NestingMargin, Content.Margin.Top,
                                           Content.Margin.Right, Content.Margin.Bottom);

            Content.UpdateVisualization(this.OwnerGenerator.WorkingPageContentWidth - this.NestingMargin,
                                        this.PageRemainingHeight + 1 /* + 1*/, EnforceLimits);  // The "+ N" is provided to detect overflow

            if ((this.PageRemainingHeight - (Content.ActualHeight + 1)) <= 0
                && !IsRecursiveHeaderCreationCall)
            {
                this.PageBlocks.Add(this.CurrentPageBlock);
                var PreviousPageBlock = this.CurrentPageBlock;
                this.CurrentPageBlock = new Dictionary<FrameworkElement, string>();

                this.PageRemainingHeight = this.OwnerGenerator.WorkingPageContentHeight;

                if (GroupKey != null && PreviousPageBlock.Count > 0
                    && PreviousPageBlock.Last().Value == GroupKey
                    && PreviousPageBlock.ElementAt((int)Math.Ceiling((double)PreviousPageBlock.Count / 2.0) - 1).Value != GroupKey)
                {
                    var ShiftedGroupMembers = PreviousPageBlock.Reverse().TakeWhile(part => part.Value == GroupKey).Reverse();

                    // Recalculate sizes
                    this.PageRemainingHeight -= ShiftedGroupMembers.Sum(member => member.Key.ActualHeight);

                    Content.UpdateVisualization(this.OwnerGenerator.WorkingPageContentWidth - this.NestingMargin,
                                                this.PageRemainingHeight + 1 /* + 1*/, EnforceLimits);  // The "+ N" is provided to detect overflow

                    this.PageRemainingHeight -= Content.ActualHeight;

                    ShiftedGroupMembers = ShiftedGroupMembers
                                                .Concatenate(new KeyValuePair<FrameworkElement, string>(Content, GroupKey));

                    ShiftedGroupMembers.ForEach(member =>
                        {
                            PreviousPageBlock.Remove(member.Key);
                            this.CurrentPageBlock.Add(member.Key, member.Value);
                        });

                    return;
                }

                // Recalculate sizes
                Content.UpdateVisualization(this.OwnerGenerator.WorkingPageContentWidth - this.NestingMargin,
                                            this.PageRemainingHeight + 1 /* + 1*/, EnforceLimits);  // The "+ N" is provided to detect overflow

                if (this.PageBreakStartCreator != null)
                    this.AppendContent(this.PageBreakStartCreator(), null, true, EnforceLimits);

                this.IsAtPageStart = true;  // Notice that is setted after the recursive AppendContent call
            }

            if (!(Content.Tag.ToStringAlways() == PAGE_TOP_CONTENT_TAG
                  && this.PreviousContentTag == PAGE_TOP_CONTENT_TAG))
            {
                this.PageRemainingHeight -= Content.ActualHeight;
                this.CurrentPageBlock.Add(Content, GroupKey);
            }

            this.PreviousContentTag = Content.Tag as string;
        }
        private string PreviousContentTag = null;

        private StackPanel GenerateFrame(Dictionary<FrameworkElement, string> PageBlock)
        {
            var Result = new StackPanel();
            PageBlock.ForEach(reg => Result.Children.Add(reg.Key));
            return Result;
        }

        public IEnumerable<FixedPage> GetPages()
        {
            if (this.CurrentPageBlock.Count > 0)
                this.PageBlocks.Add(this.CurrentPageBlock);

            var Result = new List<FixedPage>();
            this.PageBlocks.ForEach(block => Result.Add(this.OwnerGenerator.CreatePage(this.GenerateFrame(block))));

            return Result;
        }
    }
}
