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
// File   : ReportHtmlSection.cs
// Object : Instrumind.ThinkComposer.Composer.Reporting.ReportHtmlSection (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2012.09.26 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Instrumind.Common;
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.MetaModel.Configurations;
using Instrumind.ThinkComposer.Model;
using Instrumind.ThinkComposer.Model.GraphModel;

/// Provides features for report generation.
namespace Instrumind.ThinkComposer.Composer.Reporting
{
    /// <summary>
    /// Represents an HTML page to be generated.
    /// </summary>
    internal class ReportHtmlPage
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public ReportHtmlPage(IRecognizableComposite Source, ReportHtmlGenerator Generator)
        {
            this.Source = Source;

            Generator.CreateUniqueRelativeLocation(Source); // This already was created for root (Composition).

            this.PhysicalLocation = Generator.GetPhysicalLocationOf(Source);
            this.PageContentDir = Path.Combine(Path.GetDirectoryName(this.PhysicalLocation),
                                               Path.GetFileNameWithoutExtension(this.PhysicalLocation)
                                               + ReportHtmlGenerator.CONTENT_FOLDER_SUFFIX + "\\");
        }

        /// <summary>
        /// Physical path of the page file.
        /// </summary>
        public string PhysicalLocation { get; private set; }

        /// <summary>
        /// Physical path of the page's content directory.
        /// </summary>
        public string PageContentDir { get; private set; }

        /// <summary>
        /// Source entity represented by this page.
        /// </summary>
        public IRecognizableComposite Source { get; private set; }

        /// <summary>
        /// Current identation level of this page.
        /// </summary>
        public int IndentLevel { get; set; }

        /// <summary>
        /// HTML content of this page.
        /// </summary>
        public readonly List<string> Content = new List<string>();
    }
}
