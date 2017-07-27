using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Instrumind.Common.Visualization
{
    public class DocumentPaginatorWrapper : DocumentPaginator
    {
        Size WorkingPageSize;
        Size WorkingMargin;
        DocumentPaginator WorkingPaginator;

        public DocumentPaginatorWrapper(DocumentPaginator paginator, Size pageSize, Size margin)
        {
            WorkingPageSize = pageSize;
            WorkingMargin = margin;
            WorkingPaginator = paginator;

            WorkingPaginator.PageSize = new Size(WorkingPageSize.Width - margin.Width * 2,
                                                 WorkingPageSize.Height - margin.Height * 2);
        }

        Rect Move(Rect rect)
        {
            if (rect.IsEmpty)
                return rect;
            else
                return new Rect(rect.Left + WorkingMargin.Width, rect.Top + WorkingMargin.Height,
                                rect.Width, rect.Height);
        }

        public override DocumentPage GetPage(int pageNumber)
        {
            var page = WorkingPaginator.GetPage(pageNumber);

            // Create a wrapper visual for transformation and add extras
            var newpage = new ContainerVisual();

            ContainerVisual ContainedPage = new ContainerVisual();

            ContainedPage.Children.Add(page.Visual);

            newpage.Children.Add(ContainedPage);

            if (WorkingMargin.Width > 0 || WorkingMargin.Height > 0)
                newpage.Transform = new TranslateTransform(WorkingMargin.Width, WorkingMargin.Height);

            var Result = new DocumentPage(newpage, WorkingPageSize,Move(page.BleedBox), Move(page.ContentBox));
            return Result;
        }

        public override bool IsPageCountValid { get { return WorkingPaginator.IsPageCountValid; } }

        public override int PageCount
        {
            get { return WorkingPaginator.PageCount; }
        }

        public override Size PageSize
        {
            get { return WorkingPaginator.PageSize; }
            set { WorkingPaginator.PageSize = value; }
        }

        public override IDocumentPaginatorSource Source { get { return WorkingPaginator.Source; } }

    }
}
