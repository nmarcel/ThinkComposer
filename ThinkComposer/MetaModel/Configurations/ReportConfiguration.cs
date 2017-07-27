//
// Copyright (C) 2011-2015 Néstor Marcel Sánchez Ahumada.
// http://thinkcomposer.codeplex.com
//
// This file is part of ThinkComposer, which is free software licensed under the GNU General Public License.
// It is provided without any warranty. You should find a copy of the license in the root directory of this software product.
// -------------------------------------------------------------------------------------------
//
// Project: Instrumind ThinkComposer v1.0
// File   : ReportConfiguration.cs
// Object : Instrumind.ThinkComposer.MetaModel.Configurations.ReportConfiguration (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2012.01.29 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Printing;
using System.Text;
using System.Windows.Media;
using System.Windows;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;

/// Provides configuration information for meta-models.
namespace Instrumind.ThinkComposer.MetaModel.Configurations
{
    /// <summary>
    /// Stores report generation parameters.
    /// </summary>
    [Serializable]
    public class ReportConfiguration : IModelEntity, IModelClass<ReportConfiguration>
    {
        public const double PICTOGRAM_MAX_WIDTH = 250;
        public const double PICTOGRAM_MAX_HEIGHT = 250;

        public const double TBLF_PICT_MAX_WIDTH = 128;
        public const double TBLF_PICT_MAX_HEIGHT = 128;

        public const double LIST_PICT_STD_WIDTH = 32;
        public const double LIST_PICT_STD_HEIGHT = 32;

        public const double DEFAULT_PAGE_MARGIN_CMS = 1; // Margin in cms.

        public static readonly Size PAGE_LETTER_SIZE_INCHES = new Size(11, 8.5);

        public static readonly Size DEFAULT_PAGE_SIZE = new Size(PAGE_LETTER_SIZE_INCHES.Width * Display.WPF_DPI,
                                                                 PAGE_LETTER_SIZE_INCHES.Height * Display.WPF_DPI);

        /// <summary>
        /// Static constructor
        /// </summary>
        static ReportConfiguration()
        {
            __ClassDefinitor = new ModelClassDefinitor<ReportConfiguration>("ReportConfiguration", null, "Report Configuration",
                                                                            "Stores report generation parameters.");
            __ClassDefinitor.DeclareProperty(__Document_Title);
            __ClassDefinitor.DeclareProperty(__Document_Subtitle);

            __ClassDefinitor.DeclareProperty(__PageHeader_Left);
            __ClassDefinitor.DeclareProperty(__PageHeader_Center);
            __ClassDefinitor.DeclareProperty(__PageHeader_Right);

            __ClassDefinitor.DeclareProperty(__PageFooter_Left);
            __ClassDefinitor.DeclareProperty(__PageFooter_Center);
            __ClassDefinitor.DeclareProperty(__PageFooter_Right);

            __ClassDefinitor.DeclareProperty(__DocSection_TitlePage);
            __ClassDefinitor.DeclareProperty(__DocSection_TableOfContents);
            __ClassDefinitor.DeclareProperty(__DocSection_Composition);
            __ClassDefinitor.DeclareProperty(__DocSection_Domain);

            __ClassDefinitor.DeclareProperty(__Composition_Card);

            __ClassDefinitor.DeclareProperty(__CompositeIdea_View_Card);
            __ClassDefinitor.DeclareProperty(__CompositeIdea_View_Diagram);

            __ClassDefinitor.DeclareProperty(__CompositeIdea_Concepts_List);
            __ClassDefinitor.DeclareProperty(__CompositeIdea_Concepts_Card);
            __ClassDefinitor.DeclareProperty(__CompositeIdea_Concepts_ReportCompositeContent);

            __ClassDefinitor.DeclareProperty(__CompositeIdea_Relationships_List);
            __ClassDefinitor.DeclareProperty(__CompositeIdea_Relationships_Card);
            __ClassDefinitor.DeclareProperty(__CompositeIdea_Relationships_ReportCompositeContent);

            __ClassDefinitor.DeclareProperty(__CompositeIdea_Markers_List);
            __ClassDefinitor.DeclareProperty(__CompositeIdea_Markers_Card);

            __ClassDefinitor.DeclareProperty(__CompositeIdea_Complements);
            __ClassDefinitor.DeclareProperty(__CompositeIdea_GroupedIdeas_List);

            __ClassDefinitor.DeclareProperty(__CompositeIdea_Details);
            __ClassDefinitor.DeclareProperty(__CompositeIdea_DetailsIncludeLinksTarget);
            __ClassDefinitor.DeclareProperty(__CompositeIdea_DetailsIncludeAttachmentsContent);
            __ClassDefinitor.DeclareProperty(__CompositeIdea_DetailsIncludeTablesData);

            __ClassDefinitor.DeclareProperty(__CompositeIdea_RelatedFrom_Collection);
            __ClassDefinitor.DeclareProperty(__CompositeIdea_IncludeTargetCompanions);
            __ClassDefinitor.DeclareProperty(__CompositeIdea_RelatedTo_Collection);
            __ClassDefinitor.DeclareProperty(__CompositeIdea_IncludeOriginCompanions);

            __ClassDefinitor.DeclareProperty(__CompositeRelationship_Links_Collection);
            __ClassDefinitor.DeclareProperty(__CompositeRelationship_Links_Card);

            __ClassDefinitor.DeclareProperty(__Domain_Concept_Defs);
            __ClassDefinitor.DeclareProperty(__Domain_Concept_Defs_List);
            __ClassDefinitor.DeclareProperty(__Domain_Concept_Defs_Card);

            __ClassDefinitor.DeclareProperty(__Domain_Relationship_Defs);
            __ClassDefinitor.DeclareProperty(__Domain_Relationship_Defs_List);
            __ClassDefinitor.DeclareProperty(__Domain_Relationship_Defs_Card);

            __ClassDefinitor.DeclareProperty(__Domain_LinkRole_Variants);

            __ClassDefinitor.DeclareProperty(__Domain_Marker_Defs);
            __ClassDefinitor.DeclareProperty(__Domain_Marker_Defs_List);
            __ClassDefinitor.DeclareProperty(__Domain_Marker_Defs_Card);

            __ClassDefinitor.DeclareProperty(__Domain_TableStruct_Defs);
            __ClassDefinitor.DeclareProperty(__Domain_BaseTables);

            __ClassDefinitor.DeclareProperty(__FmtPageHeaderLabels);
            __ClassDefinitor.DeclareProperty(__FmtPageFooterLabels);

            __ClassDefinitor.DeclareProperty(__FmtMainTitle);
            __ClassDefinitor.DeclareProperty(__FmtMainSubtitle);

            __ClassDefinitor.DeclareProperty(__FmtSubjectTitle);
            __ClassDefinitor.DeclareProperty(__FmtSubjectSubtitle);

            __ClassDefinitor.DeclareProperty(__FmtSectionTitle);
            __ClassDefinitor.DeclareProperty(__FmtSectionSubtitle);

            __ClassDefinitor.DeclareProperty(__FmtListFieldLabel);
            __ClassDefinitor.DeclareProperty(__FmtListFieldValue);

            __ClassDefinitor.DeclareProperty(__FmtCardFieldLabel);
            __ClassDefinitor.DeclareProperty(__FmtCardFieldValue);

            __ClassDefinitor.DeclareProperty(__FmtExtras);

            __ClassDefinitor.DeclareProperty(__FmtFieldLabelBackground);
            __ClassDefinitor.DeclareProperty(__FmtFieldValueBackground);

            __ClassDefinitor.DeclareProperty(__FmtViewLinesForeground);
            __ClassDefinitor.DeclareProperty(__FmtCardLinesForeground);

            __ClassDefinitor.DeclareProperty(__FmtListLinesForeground);
            __ClassDefinitor.DeclareProperty(__FmtListRowLinesForeground);

            __ClassDefinitor.DeclareProperty(__FmtDetailFieldLabel);
            __ClassDefinitor.DeclareProperty(__FmtDetailFieldValue);
        }

        public ReportConfiguration()
        {
            FmtMainTitle = new TextFormat("Arial", 18, Brushes.Black, true, false, false, TextAlignment.Center);
            FmtMainSubtitle = new TextFormat("Arial", 14, Brushes.Black, true, false, false, TextAlignment.Center);

            Document_Title = "[Composition]";
            Document_Subtitle = "[Domain]";

            PageHeader_Left = "[Domain]";
            PageHeader_Center = "[Composition]";
            PageHeader_Right = "[Version]";

            PageFooter_Left = "[Author]";
            PageFooter_Center = "[Date]";
            PageFooter_Right = "[Page]";

            FmtPageHeaderLabels = new TextFormat("Arial", 9, Brushes.Gray);
            FmtPageFooterLabels = new TextFormat("Arial", 9, Brushes.Gray);

            DocSection_TitlePage = true;
            DocSection_TableOfContents = false;
            DocSection_Composition = true;
            DocSection_Domain = true;

            Composition_Card = new DisplayCard();

            CompositeIdea_View_Card = new DisplayCard { Show = false };
            CompositeIdea_View_Diagram = true;

            CompositeIdea_Concepts_List = new DisplayList();
            CompositeIdea_Concepts_Card = new DisplayCard();
            CompositeIdea_Concepts_ReportCompositeContent = true;

            CompositeIdea_Relationships_List = new DisplayList();
            CompositeIdea_Relationships_Card = new DisplayCard();
            CompositeIdea_Relationships_ReportCompositeContent = true;

            CompositeIdea_Details = true;
            CompositeIdea_DetailsIncludeLinksTarget = true;
            CompositeIdea_DetailsIncludeAttachmentsContent = true;
            CompositeIdea_DetailsIncludeTablesData = true;

            CompositeIdea_Markers_List = new DisplayList();
            CompositeIdea_Markers_Card = new DisplayCard();

            CompositeIdea_RelatedFrom_Collection = true;
            CompositeIdea_IncludeTargetCompanions = true;

            CompositeIdea_RelatedTo_Collection = true;
            CompositeIdea_IncludeOriginCompanions = true;

            CompositeIdea_GroupedIdeas_List = new DisplayList();
            CompositeIdea_Complements = true;

            CompositeRelationship_Links_Collection = true;   // Extend/Specialize for Link-Roles.
            CompositeRelationship_Links_Card = new DisplayCard();   // Extend/Specialize for Link-Roles.

            Domain_Concept_Defs = true;
            Domain_Concept_Defs_List = new DisplayList { Definitor = false };
            Domain_Concept_Defs_Card = new DisplayCard { Definitor = false };

            Domain_Relationship_Defs = true;
            Domain_Relationship_Defs_List = new DisplayList { Definitor = false };
            Domain_Relationship_Defs_Card = new DisplayCard { Definitor = false };

            Domain_LinkRole_Variants = true;

            Domain_Marker_Defs = false;
            Domain_Marker_Defs_List = new DisplayList { Show = false, Definitor = false };
            Domain_Marker_Defs_Card = new DisplayCard();

            Domain_TableStruct_Defs = true;

            Domain_BaseTables = true;

            FmtSubjectTitle = new TextFormat("Arial", 16, Brushes.Black, true, false, true, TextAlignment.Center);
            FmtSubjectSubtitle = new TextFormat("Arial", 12, Brushes.Black, true, false, false, TextAlignment.Center);

            FmtSectionTitle = new TextFormat("Arial", 14, Brushes.Black, true, false, true /*, TextAlignment.Center*/);
            FmtSectionSubtitle = new TextFormat("Arial", 11, Brushes.Black, true, false, false /*, TextAlignment.Center*/);

            FmtListFieldLabel = new TextFormat("Arial", 10, Brushes.Black, true, false, false, TextAlignment.Center);
            FmtListFieldValue = new TextFormat("Arial", 10, Brushes.Black);

            FmtCardFieldLabel = new TextFormat("Arial", 10, Brushes.Black, true);
            FmtCardFieldValue = new TextFormat("Arial", 10, Brushes.Black);

            FmtDetailFieldLabel = new TextFormat("Arial", 9, Brushes.Black, true);
            FmtDetailFieldValue = new TextFormat("Arial", 9, Brushes.Black);

            FmtExtras = new TextFormat("Arial", 8, Brushes.Black);

            FmtFieldLabelBackground = Brushes.WhiteSmoke;
            FmtFieldValueBackground = Brushes.Transparent;

            FmtViewLinesForeground = Brushes.LightGray;
            FmtCardLinesForeground = Brushes.LightGray;

            FmtListLinesForeground = Brushes.LightGray;
            FmtListRowLinesForeground = Brushes.LightGray;
        }

        public PrintTicket PrintingTicket
        {
            get
            {
                if (this.PrintingTicket_ == null)
                {
                    this.PrintingTicket_ = new PrintTicket();   // PrintQueue.UserPrintTicket;
                    this.PrintingTicket_.PageMediaSize = new PageMediaSize(PageMediaSizeName.NorthAmericaLetter);
                }

                return this.PrintingTicket_;
            }

            set { this.PrintingTicket_ = value;  }
        }
        [NonSerialized]
        private PrintTicket PrintingTicket_ = null;

        public PrintCapabilities PrintingCapabilities
        {
            get
            {
                if (this.PrintingCapabilities_ == null)
                    this.PrintingCapabilities_ = LocalPrintServer.GetDefaultPrintQueue().GetPrintCapabilities(PrintingTicket);

                return this.PrintingCapabilities_;
            }
            set { this.PrintingCapabilities_ = value; }
        }
        [NonSerialized]
        public PrintCapabilities PrintingCapabilities_ = null;

        /// <summary>
        /// Text of Document Title.
        /// </summary>
        public string Document_Title { get { return __Document_Title.Get(this); } set { __Document_Title.Set(this, value); } }
        protected string Document_Title_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, string> __Document_Title =
                   new ModelPropertyDefinitor<ReportConfiguration, string>("Document_Title", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.Document_Title_, (ins, val) => ins.Document_Title_ = val, false, false,
                                                                           "Document Title", "Text of the Document Title.");

        /// <summary>
        /// Text of Document Subtitle.
        /// </summary>
        public string Document_Subtitle { get { return __Document_Subtitle.Get(this); } set { __Document_Subtitle.Set(this, value); } }
        protected string Document_Subtitle_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, string> __Document_Subtitle =
                   new ModelPropertyDefinitor<ReportConfiguration, string>("Document_Subtitle", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.Document_Subtitle_, (ins, val) => ins.Document_Subtitle_ = val, false, false,
                                                                           "Document Subtitle", "Text of the Document Subtitle.");

        /// <summary>
        /// Text of the Left Page-Header label.
        /// </summary>
        public string PageHeader_Left { get { return __PageHeader_Left.Get(this); } set { __PageHeader_Left.Set(this, value); } }
        protected string PageHeader_Left_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, string> __PageHeader_Left =
                   new ModelPropertyDefinitor<ReportConfiguration, string>("PageHeader_Left", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.PageHeader_Left_, (ins, val) => ins.PageHeader_Left_ = val, false, false,
                                                                           "Page-Header Left", "Text of the Left Page-Header label.");
        /// <summary>
        /// Text of the Center Page-Header label.
        /// </summary>
        public string PageHeader_Center { get { return __PageHeader_Center.Get(this); } set { __PageHeader_Center.Set(this, value); } }
        protected string PageHeader_Center_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, string> __PageHeader_Center =
                   new ModelPropertyDefinitor<ReportConfiguration, string>("PageHeader_Center", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.PageHeader_Center_, (ins, val) => ins.PageHeader_Center_ = val, false, false,
                                                                           "Page-Header Center", "Text of the Center Page-Header label.");
        /// <summary>
        /// Text of the Right Page-Header label.
        /// </summary>
        public string PageHeader_Right { get { return __PageHeader_Right.Get(this); } set { __PageHeader_Right.Set(this, value); } }
        protected string PageHeader_Right_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, string> __PageHeader_Right =
                   new ModelPropertyDefinitor<ReportConfiguration, string>("PageHeader_Right", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.PageHeader_Right_, (ins, val) => ins.PageHeader_Right_ = val, false, false,
                                                                           "Page-Header Right", "Text of the Right Page-Header label.");

        /// <summary>
        /// Text of the Left Page-Footer label.
        /// </summary>
        public string PageFooter_Left { get { return __PageFooter_Left.Get(this); } set { __PageFooter_Left.Set(this, value); } }
        protected string PageFooter_Left_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, string> __PageFooter_Left =
                   new ModelPropertyDefinitor<ReportConfiguration, string>("PageFooter_Left", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.PageFooter_Left_, (ins, val) => ins.PageFooter_Left_ = val, false, false,
                                                                           "Page-Footer Left", "Text of the Left Page-Footer label.");
        /// <summary>
        /// Text of the Center Page-Footer label.
        /// </summary>
        public string PageFooter_Center { get { return __PageFooter_Center.Get(this); } set { __PageFooter_Center.Set(this, value); } }
        protected string PageFooter_Center_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, string> __PageFooter_Center =
                   new ModelPropertyDefinitor<ReportConfiguration, string>("PageFooter_Center", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.PageFooter_Center_, (ins, val) => ins.PageFooter_Center_ = val, false, false,
                                                                           "Page-Footer Center", "Text of the Center Page-Footer label.");
        /// <summary>
        /// Text of the Right Page-Footer label.
        /// </summary>
        public string PageFooter_Right { get { return __PageFooter_Right.Get(this); } set { __PageFooter_Right.Set(this, value); } }
        protected string PageFooter_Right_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, string> __PageFooter_Right =
                   new ModelPropertyDefinitor<ReportConfiguration, string>("PageFooter_Right", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.PageFooter_Right_, (ins, val) => ins.PageFooter_Right_ = val, false, false,
                                                                           "Page-Footer Right", "Text of the Right Page-Footer label.");

        /// <summary>
        /// Indicates whether to show the Title Page document section.
        /// </summary>
        public bool DocSection_TitlePage { get { return __DocSection_TitlePage.Get(this); } set { __DocSection_TitlePage.Set(this, value); } }
        protected bool DocSection_TitlePage_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, bool> __DocSection_TitlePage =
                   new ModelPropertyDefinitor<ReportConfiguration, bool>("DocSection_TitlePage", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.DocSection_TitlePage_, (ins, val) => ins.DocSection_TitlePage_ = val, false, false,
                                                                         "Title Page", "Indicates whether to show the Title Page document section.");
        /// <summary>
        /// Indicates whether to show the Table of Contents document section.
        /// </summary>
        public bool DocSection_TableOfContents { get { return __DocSection_TableOfContents.Get(this); } set { __DocSection_TableOfContents.Set(this, value); } }
        protected bool DocSection_TableOfContents_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, bool> __DocSection_TableOfContents =
                   new ModelPropertyDefinitor<ReportConfiguration, bool>("DocSection_TableOfContents", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.DocSection_TableOfContents_, (ins, val) => ins.DocSection_TableOfContents_ = val, false, false,
                                                                         "Table of Contents", "Indicates whether to show the Table of Contents document section.");
        /// <summary>
        /// Indicates whether to show the Composition document section.
        /// </summary>
        public bool DocSection_Composition { get { return __DocSection_Composition.Get(this); } set { __DocSection_Composition.Set(this, value); } }
        protected bool DocSection_Composition_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, bool> __DocSection_Composition =
                   new ModelPropertyDefinitor<ReportConfiguration, bool>("DocSection_Composition", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.DocSection_Composition_, (ins, val) => ins.DocSection_Composition_ = val, false, false,
                                                                         "Composition", "Indicates whether to show the Composition document section.");
        /// <summary>
        /// Indicates whether to show the Domain document section.
        /// </summary>
        public bool DocSection_Domain { get { return __DocSection_Domain.Get(this); } set { __DocSection_Domain.Set(this, value); } }
        protected bool DocSection_Domain_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, bool> __DocSection_Domain =
                   new ModelPropertyDefinitor<ReportConfiguration, bool>("DocSection_Domain", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.DocSection_Domain_, (ins, val) => ins.DocSection_Domain_ = val, false, false,
                                                                         "Domain", "Indicates whether to show the Domain document section.");

        /// <summary>
        /// Card style of the Composition.
        /// </summary>
        public DisplayCard Composition_Card { get { return __Composition_Card.Get(this); } set { __Composition_Card.Set(this, value); } }
        protected DisplayCard Composition_Card_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, DisplayCard> __Composition_Card =
                   new ModelPropertyDefinitor<ReportConfiguration, DisplayCard>("Composition_Card", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.Composition_Card_, (ins, val) => ins.Composition_Card_ = val, false, false,
                                                                                "Composition Card", "Card style of the Composition.");

        /// <summary>
        /// Card style of the Idea View.
        /// </summary>
        public DisplayCard CompositeIdea_View_Card { get { return __CompositeIdea_View_Card.Get(this); } set { __CompositeIdea_View_Card.Set(this, value); } }
        protected DisplayCard CompositeIdea_View_Card_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, DisplayCard> __CompositeIdea_View_Card =
                   new ModelPropertyDefinitor<ReportConfiguration, DisplayCard>("CompositeIdea_View_Card", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.CompositeIdea_View_Card_, (ins, val) => ins.CompositeIdea_View_Card_ = val, false, false,
                                                                                "View Card", "Card style of the Idea View.");
        /// <summary>
        /// Indicates whether to show the Idea View Diagram.
        /// </summary>
        public bool CompositeIdea_View_Diagram { get { return __CompositeIdea_View_Diagram.Get(this); } set { __CompositeIdea_View_Diagram.Set(this, value); } }
        protected bool CompositeIdea_View_Diagram_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, bool> __CompositeIdea_View_Diagram =
                   new ModelPropertyDefinitor<ReportConfiguration, bool>("CompositeIdea_View_Diagram", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.CompositeIdea_View_Diagram_, (ins, val) => ins.CompositeIdea_View_Diagram_ = val, false, false,
                                                                         "View Diagram", "Indicates whether to show the Idea View Diagram.");

        /// <summary>
        /// List style of the Idea Markers.
        /// </summary>
        public DisplayList CompositeIdea_Markers_List { get { return __CompositeIdea_Markers_List.Get(this); } set { __CompositeIdea_Markers_List.Set(this, value); } }
        protected DisplayList CompositeIdea_Markers_List_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, DisplayList> __CompositeIdea_Markers_List =
                   new ModelPropertyDefinitor<ReportConfiguration, DisplayList>("CompositeIdea_Markers_List", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.CompositeIdea_Markers_List_, (ins, val) => ins.CompositeIdea_Markers_List_ = val, false, false,
                                                                                "Markers List", "List style of the Idea Markers.");
        /// <summary>
        /// Card style of the Idea Markers.
        /// </summary>
        // Needed?
        public DisplayCard CompositeIdea_Markers_Card { get { return __CompositeIdea_Markers_Card.Get(this); } set { __CompositeIdea_Markers_Card.Set(this, value); } }
        protected DisplayCard CompositeIdea_Markers_Card_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, DisplayCard> __CompositeIdea_Markers_Card =
                   new ModelPropertyDefinitor<ReportConfiguration, DisplayCard>("CompositeIdea_Markers_Card", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.CompositeIdea_Markers_Card_, (ins, val) => ins.CompositeIdea_Markers_Card_ = val, false, false,
                                                                                "Markers Card", "Card style of the Idea Markers.");

        /// <summary>
        /// List style of the Idea Concepts.
        /// </summary>
        public DisplayList CompositeIdea_Concepts_List { get { return __CompositeIdea_Concepts_List.Get(this); } set { __CompositeIdea_Concepts_List.Set(this, value); } }
        protected DisplayList CompositeIdea_Concepts_List_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, DisplayList> __CompositeIdea_Concepts_List =
                   new ModelPropertyDefinitor<ReportConfiguration, DisplayList>("CompositeIdea_Concepts_List", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.CompositeIdea_Concepts_List_, (ins, val) => ins.CompositeIdea_Concepts_List_ = val, false, false,
                                                                                "Concepts List", "List style of the Idea Concepts.");
        /// <summary>
        /// Card style of the Idea Concepts.
        /// </summary>
        // Postponed: Individual selection by Concept-Definion/Type
        public DisplayCard CompositeIdea_Concepts_Card { get { return __CompositeIdea_Concepts_Card.Get(this); } set { __CompositeIdea_Concepts_Card.Set(this, value); } }
        protected DisplayCard CompositeIdea_Concepts_Card_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, DisplayCard> __CompositeIdea_Concepts_Card =
                   new ModelPropertyDefinitor<ReportConfiguration, DisplayCard>("CompositeIdea_Concepts_Card", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.CompositeIdea_Concepts_Card_, (ins, val) => ins.CompositeIdea_Concepts_Card_ = val, false, false,
                                                                                "Concepts Card", "Card style of the Idea Concepts.");
        /// <summary>
        /// Indicates to report the composite content of the Concepts.
        /// </summary>
        public bool CompositeIdea_Concepts_ReportCompositeContent { get { return __CompositeIdea_Concepts_ReportCompositeContent.Get(this); } set { __CompositeIdea_Concepts_ReportCompositeContent.Set(this, value); } }
        protected bool CompositeIdea_Concepts_ReportCompositeContent_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, bool> __CompositeIdea_Concepts_ReportCompositeContent =
                   new ModelPropertyDefinitor<ReportConfiguration, bool>("CompositeIdea_Concepts_ReportCompositeContent", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.CompositeIdea_Concepts_ReportCompositeContent_, (ins, val) => ins.CompositeIdea_Concepts_ReportCompositeContent_ = val, false, false,
                                                                         "Concepts Report Composite Content", "Indicates to report the composite content of the Concepts.");

        /// <summary>
        /// List style of the Idea Relationships.
        /// </summary>
        public DisplayList CompositeIdea_Relationships_List { get { return __CompositeIdea_Relationships_List.Get(this); } set { __CompositeIdea_Relationships_List.Set(this, value); } }
        protected DisplayList CompositeIdea_Relationships_List_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, DisplayList> __CompositeIdea_Relationships_List =
                   new ModelPropertyDefinitor<ReportConfiguration, DisplayList>("CompositeIdea_Relationships_List", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.CompositeIdea_Relationships_List_, (ins, val) => ins.CompositeIdea_Relationships_List_ = val, false, false,
                                                                                "Relationships List", "List style of the Idea Relationships.");
        /// <summary>
        /// Card style of the Idea Relationships.
        /// </summary>
        // Postponed: Individual selection by Relationship-Definion/Type
        public DisplayCard CompositeIdea_Relationships_Card { get { return __CompositeIdea_Relationships_Card.Get(this); } set { __CompositeIdea_Relationships_Card.Set(this, value); } }
        protected DisplayCard CompositeIdea_Relationships_Card_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, DisplayCard> __CompositeIdea_Relationships_Card =
                   new ModelPropertyDefinitor<ReportConfiguration, DisplayCard>("CompositeIdea_Relationships_Card", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.CompositeIdea_Relationships_Card_, (ins, val) => ins.CompositeIdea_Relationships_Card_ = val, false, false,
                                                                                "Relationships Card", "Card style of the Idea Relationships.");
        /// <summary>
        /// Indicates to report the composite content of the Relationships.
        /// </summary>
        public bool CompositeIdea_Relationships_ReportCompositeContent { get { return __CompositeIdea_Relationships_ReportCompositeContent.Get(this); } set { __CompositeIdea_Relationships_ReportCompositeContent.Set(this, value); } }
        protected bool CompositeIdea_Relationships_ReportCompositeContent_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, bool> __CompositeIdea_Relationships_ReportCompositeContent =
                   new ModelPropertyDefinitor<ReportConfiguration, bool>("CompositeIdea_Relationships_ReportCompositeContent", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.CompositeIdea_Relationships_ReportCompositeContent_, (ins, val) => ins.CompositeIdea_Relationships_ReportCompositeContent_ = val, false, false,
                                                                         "Relationships Report Composite Content", "Indicates to report the composite content of the Relationships.");

        /// <summary>
        /// Indicates whether to show Contents of the Idea's Non-Grouping Complements (call-outs, quotes, etc.)
        /// </summary>
        public bool CompositeIdea_Complements { get { return __CompositeIdea_Complements.Get(this); } set { __CompositeIdea_Complements.Set(this, value); } }
        protected bool CompositeIdea_Complements_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, bool> __CompositeIdea_Complements =
                   new ModelPropertyDefinitor<ReportConfiguration, bool>("CompositeIdea_Complements", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.CompositeIdea_Complements_, (ins, val) => ins.CompositeIdea_Complements_ = val, false, false,
                                                                         "Complements", "Indicates whether to show the Idea's Non-Grouping Complements (call-outs, quotes, etc.).");

        /// <summary>
        /// Indicates whether to show Contents of the Idea's Group Region/Line.
        /// </summary>
        public DisplayList CompositeIdea_GroupedIdeas_List { get { return __CompositeIdea_GroupedIdeas_List.Get(this); } set { __CompositeIdea_GroupedIdeas_List.Set(this, value); } }
        protected DisplayList CompositeIdea_GroupedIdeas_List_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, DisplayList> __CompositeIdea_GroupedIdeas_List =
                   new ModelPropertyDefinitor<ReportConfiguration, DisplayList>("CompositeIdea_GroupedIdeas_List", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.CompositeIdea_GroupedIdeas_List_, (ins, val) => ins.CompositeIdea_GroupedIdeas_List_ = val, false, false,
                                                                                "Grouped Ideas", "Indicates whether to show the Idea's grouped Ideas in Group Regions/Lines.");
        /// <summary>
        /// Indicates whether to show Idea Details Contents.
        /// </summary>
        public bool CompositeIdea_Details { get { return __CompositeIdea_Details.Get(this); } set { __CompositeIdea_Details.Set(this, value); } }
        protected bool CompositeIdea_Details_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, bool> __CompositeIdea_Details =
                   new ModelPropertyDefinitor<ReportConfiguration, bool>("CompositeIdea_Details", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.CompositeIdea_Details_, (ins, val) => ins.CompositeIdea_Details_ = val, false, false,
                                                                         "Details", "Indicates whether to show Idea Details.");
        /// <summary>
        /// Indicates whether to include target of Links.
        /// </summary>
        public bool CompositeIdea_DetailsIncludeLinksTarget { get { return __CompositeIdea_DetailsIncludeLinksTarget.Get(this); } set { __CompositeIdea_DetailsIncludeLinksTarget.Set(this, value); } }
        protected bool CompositeIdea_DetailsIncludeLinksTarget_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, bool> __CompositeIdea_DetailsIncludeLinksTarget =
                   new ModelPropertyDefinitor<ReportConfiguration, bool>("CompositeIdea_DetailsIncludeLinksTarget", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.CompositeIdea_DetailsIncludeLinksTarget_, (ins, val) => ins.CompositeIdea_DetailsIncludeLinksTarget_ = val, false, false,
                                                                         "Include Links target", "Indicates whether to include target of Links.");
        /// <summary>
        /// Indicates whether to include content of Attachments.
        /// </summary>
        public bool CompositeIdea_DetailsIncludeAttachmentsContent { get { return __CompositeIdea_DetailsIncludeAttachmentsContent.Get(this); } set { __CompositeIdea_DetailsIncludeAttachmentsContent.Set(this, value); } }
        protected bool CompositeIdea_DetailsIncludeAttachmentsContent_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, bool> __CompositeIdea_DetailsIncludeAttachmentsContent =
                   new ModelPropertyDefinitor<ReportConfiguration, bool>("CompositeIdea_DetailsIncludeAttachmentsContent", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.CompositeIdea_DetailsIncludeAttachmentsContent_, (ins, val) => ins.CompositeIdea_DetailsIncludeAttachmentsContent_ = val, false, false,
                                                                         "Include Attachments content", "Indicates whether to include content of Attachments.");
        /// <summary>
        /// Indicates whether to include data of Tables.
        /// </summary>
        public bool CompositeIdea_DetailsIncludeTablesData { get { return __CompositeIdea_DetailsIncludeTablesData.Get(this); } set { __CompositeIdea_DetailsIncludeTablesData.Set(this, value); } }
        protected bool CompositeIdea_DetailsIncludeTablesData_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, bool> __CompositeIdea_DetailsIncludeTablesData =
                   new ModelPropertyDefinitor<ReportConfiguration, bool>("CompositeIdea_DetailsIncludeTablesData", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.CompositeIdea_DetailsIncludeTablesData_, (ins, val) => ins.CompositeIdea_DetailsIncludeTablesData_ = val, false, false,
                                                                         "Include Tables/Custom-Fields data", "Indicates whether to include data of Tables and Custom-Fields.");

        /// <summary>
        /// Indicates whether to show the Origin/Participant Ideas from which this one is related from/with.
        /// </summary>
        // Immediate origins/participants, not supertree
        public bool CompositeIdea_RelatedFrom_Collection { get { return __CompositeIdea_RelatedFrom_Collection.Get(this); } set { __CompositeIdea_RelatedFrom_Collection.Set(this, value); } }
        protected bool CompositeIdea_RelatedFrom_Collection_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, bool> __CompositeIdea_RelatedFrom_Collection =
                   new ModelPropertyDefinitor<ReportConfiguration, bool>("CompositeIdea_RelatedFrom_Collection", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.CompositeIdea_RelatedFrom_Collection_, (ins, val) => ins.CompositeIdea_RelatedFrom_Collection_ = val, false, false,
                                                                         "Related-From Collection", "Indicates whether to show the Origin/Participant Ideas from which this one is related from/with.");
        /// <summary>
        /// Indicates whether to include also the companion Ideas that also are Targets in the associated Relationships.
        /// </summary>
        public bool CompositeIdea_IncludeTargetCompanions { get { return __CompositeIdea_IncludeTargetCompanions.Get(this); } set { __CompositeIdea_IncludeTargetCompanions.Set(this, value); } }
        protected bool CompositeIdea_IncludeTargetCompanions_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, bool> __CompositeIdea_IncludeTargetCompanions =
                   new ModelPropertyDefinitor<ReportConfiguration, bool>("CompositeIdea_IncludeTargetCompanions", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.CompositeIdea_IncludeTargetCompanions_, (ins, val) => ins.CompositeIdea_IncludeTargetCompanions_ = val, false, false,
                                                                         "Include Target Companions", "Indicates whether to include also the companion Ideas that also are Targets in the associated Relationships.");

        /// <summary>
        /// Indicates whether to show the Target Ideas to which this one is Related to.
        /// </summary>
        // Immediate targets, not subtree
        public bool CompositeIdea_RelatedTo_Collection { get { return __CompositeIdea_RelatedTo_Collection.Get(this); } set { __CompositeIdea_RelatedTo_Collection.Set(this, value); } }
        protected bool CompositeIdea_RelatedTo_Collection_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, bool> __CompositeIdea_RelatedTo_Collection =
                   new ModelPropertyDefinitor<ReportConfiguration, bool>("CompositeIdea_RelatedTo_Collection", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.CompositeIdea_RelatedTo_Collection_, (ins, val) => ins.CompositeIdea_RelatedTo_Collection_ = val, false, false,
                                                                         "Related-To Collection", "Indicates whether to show the Target Ideas to which this one is Related to.");
        /// <summary>
        /// Indicates whether to include also the companion Ideas that also are Origins/Participants in the associated Relationships.
        /// </summary>
        public bool CompositeIdea_IncludeOriginCompanions { get { return __CompositeIdea_IncludeOriginCompanions.Get(this); } set { __CompositeIdea_IncludeOriginCompanions.Set(this, value); } }
        protected bool CompositeIdea_IncludeOriginCompanions_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, bool> __CompositeIdea_IncludeOriginCompanions =
                   new ModelPropertyDefinitor<ReportConfiguration, bool>("CompositeIdea_IncludeOriginCompanions", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.CompositeIdea_IncludeOriginCompanions_, (ins, val) => ins.CompositeIdea_IncludeOriginCompanions_ = val, false, false,
                                                                         "Include Origin Companions", "Indicates whether to include also the companion Ideas that also are Origins/Participants in the associated Relationships.");

        /// <summary>
        /// Indicates whether to show the Relationship Links.
        /// </summary>
        // PENDING: Extend/Specialize for Link-Roles.
        public bool CompositeRelationship_Links_Collection { get { return __CompositeRelationship_Links_Collection.Get(this); } set { __CompositeRelationship_Links_Collection.Set(this, value); } }
        protected bool CompositeRelationship_Links_Collection_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, bool> __CompositeRelationship_Links_Collection =
                   new ModelPropertyDefinitor<ReportConfiguration, bool>("CompositeRelationship_Links_Collection", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.CompositeRelationship_Links_Collection_, (ins, val) => ins.CompositeRelationship_Links_Collection_ = val, false, false,
                                                                         "Relationship Links Collection", "Indicates whether to show the Relationship Links.");
        /// <summary>
        /// Card style of the Relationship Links.
        /// </summary>
        // PENDING: Extend/Specialize for Link-Roles.
        public DisplayCard CompositeRelationship_Links_Card { get { return __CompositeRelationship_Links_Card.Get(this); } set { __CompositeRelationship_Links_Card.Set(this, value); } }
        protected DisplayCard CompositeRelationship_Links_Card_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, DisplayCard> __CompositeRelationship_Links_Card =
                   new ModelPropertyDefinitor<ReportConfiguration, DisplayCard>("CompositeRelationship_Links_Card", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.CompositeRelationship_Links_Card_, (ins, val) => ins.CompositeRelationship_Links_Card_ = val, false, false,
                                                                                "Relationship Links Card", "Card style of the Relationship Links.");

        /// <summary>
        /// Indicates whether to show Domain Concept Definitions.
        /// </summary>
        public bool Domain_Concept_Defs { get { return __Domain_Concept_Defs.Get(this); } set { __Domain_Concept_Defs.Set(this, value); } }
        protected bool Domain_Concept_Defs_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, bool> __Domain_Concept_Defs =
                   new ModelPropertyDefinitor<ReportConfiguration, bool>("Domain_Concept_Defs", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.Domain_Concept_Defs_, (ins, val) => ins.Domain_Concept_Defs_ = val, false, false,
                                                                         "Concept Definitions", "Indicates whether to show Domain Concept Definitions.");
        /// <summary>
        /// List style of the Domain Concept Definitions.
        /// </summary>
        public DisplayList Domain_Concept_Defs_List { get { return __Domain_Concept_Defs_List.Get(this); } set { __Domain_Concept_Defs_List.Set(this, value); } }
        protected DisplayList Domain_Concept_Defs_List_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, DisplayList> __Domain_Concept_Defs_List =
                   new ModelPropertyDefinitor<ReportConfiguration, DisplayList>("Domain_Concept_Defs_List", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.Domain_Concept_Defs_List_, (ins, val) => ins.Domain_Concept_Defs_List_ = val, false, false,
                                                                                "Concept Definitions List", "List style of the Domain Concept Definitions.");
        /// <summary>
        /// Card style of the Domain Concept Definitions.
        /// </summary>
        public DisplayCard Domain_Concept_Defs_Card { get { return __Domain_Concept_Defs_Card.Get(this); } set { __Domain_Concept_Defs_Card.Set(this, value); } }
        protected DisplayCard Domain_Concept_Defs_Card_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, DisplayCard> __Domain_Concept_Defs_Card =
                   new ModelPropertyDefinitor<ReportConfiguration, DisplayCard>("Domain_Concept_Defs_Card", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.Domain_Concept_Defs_Card_, (ins, val) => ins.Domain_Concept_Defs_Card_ = val, false, false,
                                                                                "Concept Definitions Card", "Card style of the Domain Concept Definitions.");

        /// <summary>
        /// Indicates whether to show Domain Relationship Definitions.
        /// </summary>
        public bool Domain_Relationship_Defs { get { return __Domain_Relationship_Defs.Get(this); } set { __Domain_Relationship_Defs.Set(this, value); } }
        protected bool Domain_Relationship_Defs_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, bool> __Domain_Relationship_Defs =
                   new ModelPropertyDefinitor<ReportConfiguration, bool>("Domain_Relationship_Defs", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.Domain_Relationship_Defs_, (ins, val) => ins.Domain_Relationship_Defs_ = val, false, false,
                                                                         "Relationship Definitions", "Indicates whether to show Domain Relationship Definitions.");
        /// <summary>
        /// List style of the Domain Relationship Definitions.
        /// </summary>
        public DisplayList Domain_Relationship_Defs_List { get { return __Domain_Relationship_Defs_List.Get(this); } set { __Domain_Relationship_Defs_List.Set(this, value); } }
        protected DisplayList Domain_Relationship_Defs_List_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, DisplayList> __Domain_Relationship_Defs_List =
                   new ModelPropertyDefinitor<ReportConfiguration, DisplayList>("Domain_Relationship_Defs_List", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.Domain_Relationship_Defs_List_, (ins, val) => ins.Domain_Relationship_Defs_List_ = val, false, false,
                                                                                "Relationship Definitions List", "List style of the Domain Relationship Definitions.");
        /// <summary>
        /// Card style of the Domain Relationship Definitions.
        /// </summary>
        public DisplayCard Domain_Relationship_Defs_Card { get { return __Domain_Relationship_Defs_Card.Get(this); } set { __Domain_Relationship_Defs_Card.Set(this, value); } }
        protected DisplayCard Domain_Relationship_Defs_Card_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, DisplayCard> __Domain_Relationship_Defs_Card =
                   new ModelPropertyDefinitor<ReportConfiguration, DisplayCard>("Domain_Relationship_Defs_Card", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.Domain_Relationship_Defs_Card_, (ins, val) => ins.Domain_Relationship_Defs_Card_ = val, false, false,
                                                                                "Relationship Definitions Card", "Card style of the Domain Relationship Definitions.");

        /// <summary>
        /// Indicates whether to show Domain Link-Role Variants.
        /// </summary>
        public bool Domain_LinkRole_Variants { get { return __Domain_LinkRole_Variants.Get(this); } set { __Domain_LinkRole_Variants.Set(this, value); } }
        protected bool Domain_LinkRole_Variants_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, bool> __Domain_LinkRole_Variants =
                   new ModelPropertyDefinitor<ReportConfiguration, bool>("Domain_LinkRole_Variants", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.Domain_LinkRole_Variants_, (ins, val) => ins.Domain_LinkRole_Variants_ = val, false, false,
                                                                         "Link-Role Variants", "Indicates whether to show Domain Link-Role Variants.");

        /// <summary>
        /// Indicates whether to show Domain Marker Definitions.
        /// </summary>
        public bool Domain_Marker_Defs { get { return __Domain_Marker_Defs.Get(this); } set { __Domain_Marker_Defs.Set(this, value); } }
        protected bool Domain_Marker_Defs_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, bool> __Domain_Marker_Defs =
                   new ModelPropertyDefinitor<ReportConfiguration, bool>("Domain_Marker_Defs", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.Domain_Marker_Defs_, (ins, val) => ins.Domain_Marker_Defs_ = val, false, false,
                                                                         "Marker Definitions", "Indicates whether to show Domain Marker Definitions.");
        /// <summary>
        /// List style of the Domain Marker Definitions.
        /// </summary>
        public DisplayList Domain_Marker_Defs_List { get { return __Domain_Marker_Defs_List.Get(this); } set { __Domain_Marker_Defs_List.Set(this, value); } }
        protected DisplayList Domain_Marker_Defs_List_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, DisplayList> __Domain_Marker_Defs_List =
                   new ModelPropertyDefinitor<ReportConfiguration, DisplayList>("Domain_Marker_Defs_List", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.Domain_Marker_Defs_List_, (ins, val) => ins.Domain_Marker_Defs_List_ = val, false, false,
                                                                                "Marker Definitions List", "List style of the Domain Marker Definitions.");
        /// <summary>
        /// Card style of the Domain Marker Definitions.
        /// </summary>
        public DisplayCard Domain_Marker_Defs_Card { get { return __Domain_Marker_Defs_Card.Get(this); } set { __Domain_Marker_Defs_Card.Set(this, value); } }
        protected DisplayCard Domain_Marker_Defs_Card_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, DisplayCard> __Domain_Marker_Defs_Card =
                   new ModelPropertyDefinitor<ReportConfiguration, DisplayCard>("Domain_Marker_Defs_Card", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.Domain_Marker_Defs_Card_, (ins, val) => ins.Domain_Marker_Defs_Card_ = val, false, false,
                                                                                "Marker Definitions Card", "Card style of the Domain Marker Definitions.");

        /// <summary>
        /// Indicates whether to show Domain Table-Structure Definitions.
        /// </summary>
        public bool Domain_TableStruct_Defs { get { return __Domain_TableStruct_Defs.Get(this); } set { __Domain_TableStruct_Defs.Set(this, value); } }
        protected bool Domain_TableStruct_Defs_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, bool> __Domain_TableStruct_Defs =
                   new ModelPropertyDefinitor<ReportConfiguration, bool>("Domain_TableStruct_Defs", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.Domain_TableStruct_Defs_, (ins, val) => ins.Domain_TableStruct_Defs_ = val, false, false,
                                                                         "Table-Structure Definitions", "Indicates whether to show Domain Table-Structure Definitions.");
        /// <summary>
        /// Indicates whether to show Domain Base Tables.
        /// </summary>
        public bool Domain_BaseTables { get { return __Domain_BaseTables.Get(this); } set { __Domain_BaseTables.Set(this, value); } }
        protected bool Domain_BaseTables_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, bool> __Domain_BaseTables =
                   new ModelPropertyDefinitor<ReportConfiguration, bool>("Domain_BaseTables", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.Domain_BaseTables_, (ins, val) => ins.Domain_BaseTables_ = val, false, false,
                                                                         "Base Tables", "Indicates whether to show Domain Base Tables.");

        /// <summary>
        /// Text format for Page Header Labels.
        /// </summary>
        public TextFormat FmtPageHeaderLabels { get { return __FmtPageHeaderLabels.Get(this); } set { __FmtPageHeaderLabels.Set(this, value); } }
        protected TextFormat FmtPageHeaderLabels_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, TextFormat> __FmtPageHeaderLabels =
                   new ModelPropertyDefinitor<ReportConfiguration, TextFormat>("FmtPageHeaderLabels", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.FmtPageHeaderLabels_, (ins, val) => ins.FmtPageHeaderLabels_ = val, false, false,
                                                                               "Page Header Labels format", "Text format for Page Header Labels.");
        /// <summary>
        /// Text format for Page Footer Labels.
        /// </summary>
        public TextFormat FmtPageFooterLabels { get { return __FmtPageFooterLabels.Get(this); } set { __FmtPageFooterLabels.Set(this, value); } }
        protected TextFormat FmtPageFooterLabels_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, TextFormat> __FmtPageFooterLabels =
                   new ModelPropertyDefinitor<ReportConfiguration, TextFormat>("FmtPageFooterLabels", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.FmtPageFooterLabels_, (ins, val) => ins.FmtPageFooterLabels_ = val, false, false,
                                                                               "Page Footer Labels format", "Text format for Page Footer Labels.");

        /// <summary>
        /// Text format for Main Title data.
        /// </summary>
        public TextFormat FmtMainTitle { get { return __FmtMainTitle.Get(this); } set { __FmtMainTitle.Set(this, value); } }
        protected TextFormat FmtMainTitle_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, TextFormat> __FmtMainTitle =
                   new ModelPropertyDefinitor<ReportConfiguration, TextFormat>("FmtMainTitle", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.FmtMainTitle_, (ins, val) => ins.FmtMainTitle_ = val, false, false,
                                                                               "Main Title format", "Text format for Main Title data.");
        /// <summary>
        /// Text format for Main Subtitle data.
        /// </summary>
        public TextFormat FmtMainSubtitle { get { return __FmtMainSubtitle.Get(this); } set { __FmtMainSubtitle.Set(this, value); } }
        protected TextFormat FmtMainSubtitle_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, TextFormat> __FmtMainSubtitle =
                   new ModelPropertyDefinitor<ReportConfiguration, TextFormat>("FmtMainSubtitle", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.FmtMainSubtitle_, (ins, val) => ins.FmtMainSubtitle_ = val, false, false,
                                                                               "Main Subtitle format", "Text format for Main Subtitle data.");

        /// <summary>
        /// Text format for Subject Title data.
        /// </summary>
        public TextFormat FmtSubjectTitle { get { return __FmtSubjectTitle.Get(this); } set { __FmtSubjectTitle.Set(this, value); } }
        protected TextFormat FmtSubjectTitle_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, TextFormat> __FmtSubjectTitle =
                   new ModelPropertyDefinitor<ReportConfiguration, TextFormat>("FmtSubjectTitle", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.FmtSubjectTitle_, (ins, val) => ins.FmtSubjectTitle_ = val, false, false,
                                                                               "Subject Title format", "Text format for Subjects (Concepts/Relationships, Definitions) Title data.");
        /// <summary>
        /// Text format for Subject Subtitle data.
        /// </summary>
        public TextFormat FmtSubjectSubtitle { get { return __FmtSubjectSubtitle.Get(this); } set { __FmtSubjectSubtitle.Set(this, value); } }
        protected TextFormat FmtSubjectSubtitle_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, TextFormat> __FmtSubjectSubtitle =
                   new ModelPropertyDefinitor<ReportConfiguration, TextFormat>("FmtSubjectSubtitle", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.FmtSubjectSubtitle_, (ins, val) => ins.FmtSubjectSubtitle_ = val, false, false,
                                                                               "Subject Subtitle format", "Text format for Subjects (Concepts/Relationships, Definitions) Subtitle data.");

        /// <summary>
        /// Text format for Section Title data.
        /// </summary>
        public TextFormat FmtSectionTitle { get { return __FmtSectionTitle.Get(this); } set { __FmtSectionTitle.Set(this, value); } }
        protected TextFormat FmtSectionTitle_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, TextFormat> __FmtSectionTitle =
                   new ModelPropertyDefinitor<ReportConfiguration, TextFormat>("FmtSectionTitle", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.FmtSectionTitle_, (ins, val) => ins.FmtSectionTitle_ = val, false, false,
                                                                               "Section Title format", "Text format for Sections (Lists of Concepts/Relationships, Markers, etc.) Title data.");
        /// <summary>
        /// Text format for Section Subtitle data.
        /// </summary>
        public TextFormat FmtSectionSubtitle { get { return __FmtSectionSubtitle.Get(this); } set { __FmtSectionSubtitle.Set(this, value); } }
        protected TextFormat FmtSectionSubtitle_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, TextFormat> __FmtSectionSubtitle =
                   new ModelPropertyDefinitor<ReportConfiguration, TextFormat>("FmtSectionSubtitle", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.FmtSectionSubtitle_, (ins, val) => ins.FmtSectionSubtitle_ = val, false, false,
                                                                               "Section Subtitle format", "Text format for Sections (Lists of Concepts/Relationships, Markers, etc.) Subtitle data.");

        /// <summary>
        /// Text format for List Field-Label data.
        /// </summary>
        public TextFormat FmtListFieldLabel { get { return __FmtListFieldLabel.Get(this); } set { __FmtListFieldLabel.Set(this, value); } }
        protected TextFormat FmtListFieldLabel_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, TextFormat> __FmtListFieldLabel =
                   new ModelPropertyDefinitor<ReportConfiguration, TextFormat>("FmtListFieldLabel", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.FmtListFieldLabel_, (ins, val) => ins.FmtListFieldLabel_ = val, false, false,
                                                                               "List Field-Label format", "Text format for List Field-Label data.");
        /// <summary>
        /// Text format for List Field-Value data.
        /// </summary>
        public TextFormat FmtListFieldValue { get { return __FmtListFieldValue.Get(this); } set { __FmtListFieldValue.Set(this, value); } }
        protected TextFormat FmtListFieldValue_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, TextFormat> __FmtListFieldValue =
                   new ModelPropertyDefinitor<ReportConfiguration, TextFormat>("FmtListFieldValue", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.FmtListFieldValue_, (ins, val) => ins.FmtListFieldValue_ = val, false, false,
                                                                               "List Field-Value format", "Text format for List Field-Value data.");

        /// <summary>
        /// Text format for Card Field-Label data.
        /// </summary>
        public TextFormat FmtCardFieldLabel { get { return __FmtCardFieldLabel.Get(this); } set { __FmtCardFieldLabel.Set(this, value); } }
        protected TextFormat FmtCardFieldLabel_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, TextFormat> __FmtCardFieldLabel =
                   new ModelPropertyDefinitor<ReportConfiguration, TextFormat>("FmtCardFieldLabel", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.FmtCardFieldLabel_, (ins, val) => ins.FmtCardFieldLabel_ = val, false, false,
                                                                               "Card Field-Label format", "Text format for Card Field-Label data.");
        /// <summary>
        /// Text format for Card Field-Value data.
        /// </summary>
        public TextFormat FmtCardFieldValue { get { return __FmtCardFieldValue.Get(this); } set { __FmtCardFieldValue.Set(this, value); } }
        protected TextFormat FmtCardFieldValue_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, TextFormat> __FmtCardFieldValue =
                   new ModelPropertyDefinitor<ReportConfiguration, TextFormat>("FmtCardFieldValue", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.FmtCardFieldValue_, (ins, val) => ins.FmtCardFieldValue_ = val, false, false,
                                                                               "Card Field-Value format", "Text format for Card Field-Value data.");

        /// <summary>
        /// Text format for Detail Field-Label data.
        /// </summary>
        public TextFormat FmtDetailFieldLabel { get { return __FmtDetailFieldLabel.Get(this); } set { __FmtDetailFieldLabel.Set(this, value); } }
        protected TextFormat FmtDetailFieldLabel_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, TextFormat> __FmtDetailFieldLabel =
                   new ModelPropertyDefinitor<ReportConfiguration, TextFormat>("FmtDetailFieldLabel", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.FmtDetailFieldLabel_, (ins, val) => ins.FmtDetailFieldLabel_ = val, false, false,
                                                                               "Detail Field-Label format", "Text format for Detail Field-Label data.");
        /// <summary>
        /// Text format for Detail Field-Value data.
        /// </summary>
        public TextFormat FmtDetailFieldValue { get { return __FmtDetailFieldValue.Get(this); } set { __FmtDetailFieldValue.Set(this, value); } }
        protected TextFormat FmtDetailFieldValue_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, TextFormat> __FmtDetailFieldValue =
                   new ModelPropertyDefinitor<ReportConfiguration, TextFormat>("FmtDetailFieldValue", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.FmtDetailFieldValue_, (ins, val) => ins.FmtDetailFieldValue_ = val, false, false,
                                                                               "Detail Field-Value format", "Text format for Detail Field-Value data.");

        /// <summary>
        /// Color for the background of Field-Labels.
        /// </summary>
        public Brush FmtFieldLabelBackground { get { return __FmtFieldLabelBackground.Get(this); } set { __FmtFieldLabelBackground.Set(this, value); } }
        protected StoreBox<Brush> FmtFieldLabelBackground_ = StoreBox.Store<Brush>(null);
        public static readonly ModelPropertyDefinitor<ReportConfiguration, Brush> __FmtFieldLabelBackground =
                   new ModelPropertyDefinitor<ReportConfiguration, Brush>("FmtFieldLabelBackground", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.FmtFieldLabelBackground_, (ins, val) => ins.FmtFieldLabelBackground_ = val, false, false,
                                                                          "Field-Label Background", "Color for the background of Field-Labels.");
        /// <summary>
        /// Color for the background of Field-Values.
        /// </summary>
        public Brush FmtFieldValueBackground { get { return __FmtFieldValueBackground.Get(this); } set { __FmtFieldValueBackground.Set(this, value); } }
        protected StoreBox<Brush> FmtFieldValueBackground_ = StoreBox.Store<Brush>(null);
        public static readonly ModelPropertyDefinitor<ReportConfiguration, Brush> __FmtFieldValueBackground =
                   new ModelPropertyDefinitor<ReportConfiguration, Brush>("FmtFieldValueBackground", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.FmtFieldValueBackground_, (ins, val) => ins.FmtFieldValueBackground_ = val, false, false,
                                                                          "Field-Value Background", "Color for the background of Field-Values.");

        /// <summary>
        /// Color for Lines of a View.
        /// </summary>
        public Brush FmtViewLinesForeground { get { return __FmtViewLinesForeground.Get(this); } set { __FmtViewLinesForeground.Set(this, value); } }
        protected StoreBox<Brush> FmtViewLinesForeground_ = StoreBox.Store<Brush>(null);
        public static readonly ModelPropertyDefinitor<ReportConfiguration, Brush> __FmtViewLinesForeground =
                   new ModelPropertyDefinitor<ReportConfiguration, Brush>("FmtViewLinesForeground", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.FmtViewLinesForeground_, (ins, val) => ins.FmtViewLinesForeground_ = val, false, false,
                                                                          "View Lines Foreground", "Color for Lines of a View.");
        /// <summary>
        /// Color for Lines of a Card.
        /// </summary>
        public Brush FmtCardLinesForeground { get { return __FmtCardLinesForeground.Get(this); } set { __FmtCardLinesForeground.Set(this, value); } }
        protected StoreBox<Brush> FmtCardLinesForeground_ = StoreBox.Store<Brush>(null);
        public static readonly ModelPropertyDefinitor<ReportConfiguration, Brush> __FmtCardLinesForeground =
                   new ModelPropertyDefinitor<ReportConfiguration, Brush>("FmtCardLinesForeground", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.FmtCardLinesForeground_, (ins, val) => ins.FmtCardLinesForeground_ = val, false, false,
                                                                          "Card Lines Foreground", "Color for Lines of a Card.");
        /// <summary>
        /// Color for Lines (aside from rows) of a List.
        /// </summary>
        public Brush FmtListLinesForeground { get { return __FmtListLinesForeground.Get(this); } set { __FmtListLinesForeground.Set(this, value); } }
        protected StoreBox<Brush> FmtListLinesForeground_ = StoreBox.Store<Brush>(null);
        public static readonly ModelPropertyDefinitor<ReportConfiguration, Brush> __FmtListLinesForeground =
                   new ModelPropertyDefinitor<ReportConfiguration, Brush>("FmtListLinesForeground", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.FmtListLinesForeground_, (ins, val) => ins.FmtListLinesForeground_ = val, false, false,
                                                                          "List Lines Foreground", "Color for Lines (aside from rows) of a List.");
        /// <summary>
        /// Color for Row-Lines of a List.
        /// </summary>
        public Brush FmtListRowLinesForeground { get { return __FmtListRowLinesForeground.Get(this); } set { __FmtListRowLinesForeground.Set(this, value); } }
        protected StoreBox<Brush> FmtListRowLinesForeground_ = StoreBox.Store<Brush>(null);
        public static readonly ModelPropertyDefinitor<ReportConfiguration, Brush> __FmtListRowLinesForeground =
                   new ModelPropertyDefinitor<ReportConfiguration, Brush>("FmtListRowLinesForeground", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.FmtListRowLinesForeground_, (ins, val) => ins.FmtListRowLinesForeground_ = val, false, false,
                                                                          "List Row-Lines Foreground", "Color for Row-Lines of a List.");

        /// <summary>
        /// Text format for Extras data.
        /// </summary>
        public TextFormat FmtExtras { get { return __FmtExtras.Get(this); } set { __FmtExtras.Set(this, value); } }
        protected TextFormat FmtExtras_;
        public static readonly ModelPropertyDefinitor<ReportConfiguration, TextFormat> __FmtExtras =
                   new ModelPropertyDefinitor<ReportConfiguration, TextFormat>("FmtExtras", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.FmtExtras_, (ins, val) => ins.FmtExtras_ = val, false, false,
                                                                               "Extras format", "Text format for Extras data.");

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelClass<ReportConfiguration> Members

        public MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public ModelClassDefinitor<ReportConfiguration> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly ModelClassDefinitor<ReportConfiguration> __ClassDefinitor = null;

        public virtual object CreateCopy(ECloneOperationScope CloningScope, IMModelClass DirectOwner) { return this.CreateClone(CloningScope, DirectOwner); }
        public ReportConfiguration CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((ReportConfiguration)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public ReportConfiguration PopulateFrom(ReportConfiguration SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelEntity Members

        public EntityEditEngine EditEngine { get { return EntityEditEngine.ObtainEditEngine(this, EditEngine_); } set { EditEngine_ = value; } }
        [NonSerialized]
        private EntityEditEngine EditEngine_ = null;

        public virtual void RefreshEntity() { }

        public virtual MEntityInstanceController Controller { get { return this.Controller_; } set { this.Controller_ = value; } }
        [NonSerialized]
        private MEntityInstanceController Controller_ = null;

        #endregion

        #region INotifyPropertyChanged Members

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifies all entity subscriptors that a property, identified by the supplied definitor, has changed.
        /// </summary>
        public void NotifyPropertyChange(MModelPropertyDefinitor PropertyDefinitor)
        {
            this.NotifyPropertyChange(PropertyDefinitor.TechName);
        }

        /// <summary>
        /// Notifies all entity subscriptors that a property, identified by the supplied name, has changed.
        /// </summary>
        public void NotifyPropertyChange(string PropertyName)
        {
            var Handler = PropertyChanged;

            if (Handler != null)
                Handler(this, new PropertyChangedEventArgs(PropertyName));
        }

        #endregion

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}
