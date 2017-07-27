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
// File   : Domain.cs
// Object : Instrumind.ThinkComposer.MetaModel.Domain (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.07.11 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.ApplicationProduct;
using Instrumind.ThinkComposer.Composer;
using Instrumind.ThinkComposer.Composer.Generation;
using Instrumind.ThinkComposer.MetaModel.Configurations;
using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.MetaModel.InformationMetaModel;
using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;
using Instrumind.ThinkComposer.Model;
using Instrumind.ThinkComposer.Model.GraphModel;
using Instrumind.ThinkComposer.Model.InformationModel;
using Instrumind.ThinkComposer.Model.VisualModel;

/// Metadata shared abstractions which conform a Domain definition: Primitives for Composition creation.
namespace Instrumind.ThinkComposer.MetaModel
{
    /// <summary>
    /// Unifies a set of metadefinitions about a business area, which rules the creation of Composite-Content.
    /// This considers the definition of: Graph schematization, visual representation and information structures.
    /// </summary>
    [Serializable]
    public class Domain : ConceptDefinition, ISphereModel, IModelEntity, IModelClass<Domain>
    {
        /// <summary>
        /// File extension of Domain file names.
        /// </summary>
        public const string FILE_EXTENSION_DOMAIN = "tdom";

        /// <summary>
        /// Document suffix for ThinkComposer Domain Graph.
        /// </summary>
        public const string DOCSUF_DOMAIN = "TDG";

        /// <summary>
        /// Tech-Name of the standard (by default) variant.
        /// </summary>
        public const string STD_LNKVAR_TECHNAME = "Link";

        /// <summary>
        /// Name of the standard (by default) variant.
        /// </summary>
        public const string STD_LNKVAR_NAME = "Link";

        /// <summary>
        /// Location of the external-language template resources
        /// </summary>
        public const string BASETEMPLATE_LOCATION = "Instrumind.ThinkComposer.ApplicationProduct.BaseTemplates";

        /// <summary>
        /// File extension of Template files
        /// </summary>
        public const string TEMPLATE_FILE_EXT = "tct";

        // External languages codes
        public const string EXTLANG_CODE_TEXT = "Text";
        public const string EXTLANG_CODE_XML = "XML";

        /// <summary>
        /// Default pictogram of this class.
        /// </summary>
        public static new readonly ImageSource DefaultPictogram = Display.GetAppImage("book.png");

        public static Brush ViewStandardBackground = Brushes.White; //T Display.GetResource<Brush, Instrumind.Common.Visualization.Widgets.EntitledPanel>("PanelBrush");
        // Display.GetGradientBrush(Colors.Azure, Colors.PowderBlue, Colors.Azure)
        // Display.GetGradientBrush(Colors.AliceBlue, Colors.LightBlue);

        /// <summary>
        /// Complements provided to be attached in View diagrams.
        /// </summary>
        public static readonly List<SimplePresentationElement> StandardComplementDefinitions = new List<SimplePresentationElement>();

        public static readonly SimplePresentationElement ComplementDefText = new SimplePresentationElement("Text", "Text", "Free text (no linkable).", Display.GetAppImage("text_label.png"));
        public static readonly SimplePresentationElement ComplementDefImage = new SimplePresentationElement("Image", "Image", "Free image for background or illustration (no linkable).", Display.GetAppImage("picture_tree.png"));
        public static readonly SimplePresentationElement ComplementDefCallout = new SimplePresentationElement("Callout", "Callout", "Comments a pointed Idea.", Display.GetAppImage("comp_callout.png"));
        public static readonly SimplePresentationElement ComplementDefQuote = new SimplePresentationElement("Quote", "Quote", "Appends an oval quote to a pointed Idea.", Display.GetAppImage("comp_quote.png"));
        public static readonly SimplePresentationElement ComplementDefGroupRegion = new SimplePresentationElement("Group Region", "GroupRegion", "Appends a rectangular region to a pointed Idea (the owner) for grouping objects over (inside the rectangle).", Display.GetAppImage("comp_region.png"));
        public static readonly SimplePresentationElement ComplementDefGroupLine = new SimplePresentationElement("Group Line", "GroupLine", "Appends a thick line to a pointed Idea (the owner) for grouping objects over (partially).", Display.GetAppImage("comp_rod.png"));
        public static readonly SimplePresentationElement ComplementDefNote = new SimplePresentationElement("Note", "Note", "Note to stick over the diagram.", Display.GetAppImage("comp_note.png"));
        public static readonly SimplePresentationElement ComplementDefStamp = new SimplePresentationElement("Stamp", "Stamp", "Text to stamp over the diagram.", Display.GetAppImage("comp_stamp.png"));
        public static readonly SimplePresentationElement ComplementDefInfoCard = new SimplePresentationElement("Info-Card", "InfoCard", "Board with information about the diagram view.", Display.GetAppImage("comp_infocard.png"));
        public static readonly SimplePresentationElement ComplementDefLegend = new SimplePresentationElement("Legend", "Legend", "Board displaying used symbols and connectors, plus Domain information.", Display.GetAppImage("comp_legend.png"));

        public static readonly Table Unassigned_BaseTable = new Table(null, (new TableDetailDesignator(null, null, false, "<NONE>", "_NONE_", "Unassigned")).Assign<DetailDesignator>(false));

        public static readonly MModelPropertyDefinitor Unassigned_IdeaReferencingPropertyProperty =
            new ModelPropertyDefinitor<SimplePresentationElement,string>("_NONE_", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common,
                                                                         (Func<SimplePresentationElement,string>)null, (Action<SimplePresentationElement,string>)null, false, false,
                                                                         "<NONE>", "Do not reference Ideas");

        /// <summary>
        /// Static Constructor.
        /// </summary>
        static Domain()
        {
            __ClassDefinitor = new ModelClassDefinitor<Domain>("Domain", ConceptDefinition.__ClassDefinitor, "Domain",
                                                               "Unifies a set of metadefinitions about a business area, which rules the creation of Composite-Content. " +
                                                               "This considers the definition of: Graph schematization, visual representation and information structures.");
            __ClassDefinitor.DeclareProperty(__DefaultTableDef);
            __ClassDefinitor.DeclareProperty(__DefaultTableDefCategory);
            __ClassDefinitor.DeclareProperty(__BaseContentRoot);
            __ClassDefinitor.DeclareProperty(__ReportingConfiguration);
            __ClassDefinitor.DeclareProperty(__ViewBackgroundBrush);
            __ClassDefinitor.DeclareProperty(__ViewBackgroundImage);
            __ClassDefinitor.DeclareProperty(__ViewGridSize);
            __ClassDefinitor.DeclareProperty(__CurrentExternalLanguage);

            __ClassDefinitor.DeclareCollection(__ConceptDefCategories);
            __ClassDefinitor.DeclareCollection(__RelationshipDefCategories);
            __ClassDefinitor.DeclareCollection(__LinkRoleVariants);
            __ClassDefinitor.DeclareCollection(__MarkerDefinitions);
            __ClassDefinitor.DeclareCollection(__TableDefCategories);
            __ClassDefinitor.DeclareCollection(__FieldDefCategories);
            __ClassDefinitor.DeclareCollection(__ConceptDefClusters);
            __ClassDefinitor.DeclareCollection(__RelationshipDefClusters);
            __ClassDefinitor.DeclareCollection(__MarkerClusters);
            __ClassDefinitor.DeclareCollection(__ExternalLanguages);
            __ClassDefinitor.DeclareCollection(__OutputTemplatesForConcepts);
            __ClassDefinitor.DeclareCollection(__OutputTemplatesForRelationships);

            __ViewGridSize.RangeMin = View.GRID_SIZE_MIN;
            __ViewGridSize.RangeMax = View.GRID_SIZE_MAX;
            __ViewGridSize.RangeStep = 1.0;

            StandardComplementDefinitions.Add(ComplementDefText);
            StandardComplementDefinitions.Add(ComplementDefImage);
            StandardComplementDefinitions.Add(ComplementDefCallout);
            StandardComplementDefinitions.Add(ComplementDefGroupRegion);
            StandardComplementDefinitions.Add(ComplementDefQuote);
            StandardComplementDefinitions.Add(ComplementDefGroupLine);
            StandardComplementDefinitions.Add(ComplementDefNote);
            StandardComplementDefinitions.Add(ComplementDefStamp);
            StandardComplementDefinitions.Add(ComplementDefInfoCard);
            StandardComplementDefinitions.Add(ComplementDefLegend);

            Unassigned_BaseTable.Designation.GlobalId = Guid.Empty;

            CreateApplicableGraphicStyles();
        }

        /// <summary>
        /// Creates a new Domain definition and assigns the specified Entity editor.
        /// </summary>
        public static Domain Create(EntityEditEngine EntityEditor)
        {
            var NewDomain = new Domain("Basic", "Basic", "Elementary Domain for general-purpose Compositions.");

            // Populates the initial default definitions
            GenericConceptDefinition = new ConceptDefinition(NewDomain, null, Concept.__ClassDefinitor.Name, Concept.__ClassDefinitor.TechName,
                                                             Shapes.Capsule, Concept.__ClassDefinitor.Summary);
            NewDomain.ConceptDefinitions.Add(GenericConceptDefinition);

            GenericRelationshipDefinition = new RelationshipDefinition(NewDomain, null, Relationship.__ClassDefinitor.Name, Relationship.__ClassDefinitor.TechName,
                                                                       Shapes.Ellipse, Relationship.__ClassDefinitor.Summary, null,
                                                                       new LinkRoleDefinition(ERoleType.Origin, "Origin", "Origin", "Source or participant of the relationship."),
                                                                       new LinkRoleDefinition(ERoleType.Target, "Target", "Target", "Destination of the relationship."));
            NewDomain.RelationshipDefinitions.Add(GenericRelationshipDefinition);
            GenericRelationshipDefinition.DefaultSymbolFormat = new VisualSymbolFormat(Brushes.WhiteSmoke, Brushes.Black);
            GenericRelationshipDefinition.DefaultSymbolFormat.SetTextFormat(ETextPurpose.Title, new TextFormat("Arial", 9, Brushes.Black, false, false, false, TextAlignment.Center));
            GenericRelationshipDefinition.DefaultConnectorsFormat = new VisualConnectorsFormat(NewDomain.LinkRoleVariants.First(), Plugs.None, NewDomain.LinkRoleVariants.First(), Plugs.SimpleArrow, Brushes.Black);

            // Establish root concept for domain's base content.
            NewDomain.BaseContentRoot = new Concept(null, GenericConceptDefinition, "Domain base content", "DomainBaseContent");

            // Populates visual clusters
            NewDomain.MarkerClusters.Add(new SimplePresentationElement("User Defined", MarkerDefinition.USERDEF_CODE, "User defined markers.", Display.GetAppImage("asterisk_orange.png")));
            NewDomain.MarkerClusters.Add(new SimplePresentationElement("Flags", "Flag", "Colorized flag markers.", Display.GetAppImage("flag_red.png")));
            NewDomain.MarkerClusters.Add(new SimplePresentationElement("Awards", "Award", "Colorized award markers.", Display.GetAppImage("award_star_gold_1.png")));
            NewDomain.MarkerClusters.Add(new SimplePresentationElement("Control", "Control", "Controlling markers.", Display.GetAppImage("tasks_control.png")));
            NewDomain.MarkerClusters.Add(new SimplePresentationElement("Tags", "Tag", "Colorized tag markers.", Display.GetAppImage("tag_red.png")));

            /* Cancelled to support the already existing (cluster unrelated Idea Definitions)
            NewDomain.ConceptClusters.Add(new SimplePresentationElement(Concept.__ClassDefinitor.Name.Pluralize(), Concept.__ClassDefinitor.TechName, "", Display.GetAppImage("imtc_concept.png")));
            NewDomain.RelationshipClusters.Add(new SimplePresentationElement(Relationship.__ClassDefinitor.Name.Pluralize(), Relationship.__ClassDefinitor.TechName, "", Display.GetAppImage("imtc_relationship.png"))); */

            // DEPRECATED: PopulateCommonDomain(NewDomain);

            // This must be populated immediately after being assigned to a Composition
            NewDomain.OwnerComposition = null;

            return NewDomain;
        }

        /* DEPRECATED
        private static void PopulateCommonDomain(Domain NewDomain)
        {
            // Predefined Concepts...
            ConceptDefinition ConceptDef = null;
            RelationshipDefinition RelationshipDef = null;

            ConceptDef = new ConceptDefinition(NewDomain, GenericConceptDefinition, "Process", "Process", Shapes.Gears, "A Process.");
            NewDomain.ConceptDefinitions.Add(ConceptDef);
            ConceptDef.DefaultSymbolFormat = new VisualSymbolFormat(Display.GetContrastedBrush(Colors.DeepSkyBlue, Colors.SteelBlue), Brushes.Turquoise);
            ConceptDef.DefaultSymbolFormat.SetTextFormat(ETextPurpose.Title, new TextFormat("Tahoma", 11, Brushes.White));

            ConceptDef = new ConceptDefinition(NewDomain, GenericConceptDefinition, "Use Case", "UseCase", Shapes.Ellipse, "An Use Case.");
            NewDomain.ConceptDefinitions.Add(ConceptDef);
            ConceptDef.DefaultSymbolFormat = new VisualSymbolFormat(Display.GetContrastedBrush(Colors.LightSalmon, Colors.PeachPuff), Brushes.Firebrick, 1);
            ConceptDef.DefaultSymbolFormat.SetTextFormat(ETextPurpose.Title, new TextFormat("Verdana", 11, Brushes.Black));

            ConceptDef = new ConceptDefinition(NewDomain, GenericConceptDefinition, "Actor", "Actor", Shapes.Person, "An Actor.");
            NewDomain.ConceptDefinitions.Add(ConceptDef);
            ConceptDef.DefaultSymbolFormat = new VisualSymbolFormat(Display.GetContrastedBrush(Colors.LimeGreen, Colors.PaleGreen), Brushes.Green, 1);
            ConceptDef.DefaultSymbolFormat.SetTextFormat(ETextPurpose.Title, new TextFormat("Verdana", 10, Brushes.Black));

            ConceptDef = new ConceptDefinition(NewDomain, GenericConceptDefinition, "Component", "Component", Shapes.Component, "A Component.");
            NewDomain.ConceptDefinitions.Add(ConceptDef);
            ConceptDef.DefaultSymbolFormat = new VisualSymbolFormat(Display.GetContrastedBrush(Colors.Beige, Colors.Gold), Brushes.Goldenrod);
            ConceptDef.DefaultSymbolFormat.SetTextFormat(ETextPurpose.Title, new TextFormat("Tahoma", 10, Brushes.Navy));

            ConceptDef = new ConceptDefinition(NewDomain, GenericConceptDefinition, "Part", "Part", Shapes.Piece, "A Part.");
            NewDomain.ConceptDefinitions.Add(ConceptDef);
            ConceptDef.DefaultSymbolFormat = new VisualSymbolFormat(Display.GetContrastedBrush(Colors.LightSeaGreen, Colors.MediumAquamarine), Brushes.Black);
            ConceptDef.DefaultSymbolFormat.SetTextFormat(ETextPurpose.Title, new TextFormat("Verdana", 10, Brushes.White));

            ConceptDef = new ConceptDefinition(NewDomain, GenericConceptDefinition, "Table", "Table", Shapes.RectCrossedVertical, "A Table.");
            NewDomain.ConceptDefinitions.Add(ConceptDef);
            ConceptDef.DefaultSymbolFormat = new VisualSymbolFormat(Display.GetContrastedBrush(Colors.PowderBlue, Colors.CadetBlue), Brushes.DarkBlue);
            ConceptDef.DefaultSymbolFormat.SetTextFormat(ETextPurpose.Title, new TextFormat("Verdana", 10, Brushes.White));

            ConceptDef = new ConceptDefinition(NewDomain, GenericConceptDefinition, "Class", "Class", Shapes.RectCrossedHorizontal, "A Class.");
            NewDomain.ConceptDefinitions.Add(ConceptDef);
            ConceptDef.DefaultSymbolFormat = new VisualSymbolFormat(Brushes.CornflowerBlue, Brushes.Navy);
            ConceptDef.DefaultSymbolFormat.SetTextFormat(ETextPurpose.Title, new TextFormat("Verdana", 11, Brushes.White));

            ConceptDef = new ConceptDefinition(NewDomain, GenericConceptDefinition, "DataBase", "DataBase", Shapes.Drum, "A DataBase.");
            NewDomain.ConceptDefinitions.Add(ConceptDef);
            ConceptDef.DefaultSymbolFormat = new VisualSymbolFormat(Display.GetContrastedBrush(Colors.PapayaWhip, Colors.Orange), Brushes.DarkRed, 1);
            ConceptDef.DefaultSymbolFormat.SetTextFormat(ETextPurpose.Title, new TextFormat("Verdana", 11, Brushes.White));

            ConceptDef = new ConceptDefinition(NewDomain, GenericConceptDefinition, "Folder", "Folder", Shapes.Folder, "A Folder.");
            NewDomain.ConceptDefinitions.Add(ConceptDef);
            ConceptDef.DefaultSymbolFormat = new VisualSymbolFormat(Display.GetContrastedBrush(Colors.BurlyWood, Colors.PapayaWhip), Brushes.Sienna, 2, 1.0);
            ConceptDef.DefaultSymbolFormat.SetTextFormat(ETextPurpose.Title, new TextFormat("Arial", 11, Brushes.Black));

            // Predefined Relationships...
            RelationshipDef = new RelationshipDefinition(NewDomain, GenericRelationshipDefinition, "Message", "Message", Shapes.Envelope, "Message.", null,
                                                         new LinkRoleDefinition(ERoleType.Origin, "Emitter", "Emitter", "Emitter."),
                                                         new LinkRoleDefinition(ERoleType.Target, "Receiver", "Receiver", "Receiver."));
            NewDomain.RelationshipDefinitions.Add(RelationshipDef);
            RelationshipDef.DefaultSymbolFormat = new VisualSymbolFormat(Brushes.Honeydew, Brushes.MediumTurquoise);
            RelationshipDef.DefaultSymbolFormat.SetTextFormat(ETextPurpose.Title, new TextFormat("Arial", 8, Brushes.Black));
            RelationshipDef.DefaultConnectorsFormat = new VisualConnectorsFormat(NewDomain.LinkRoleVariants.First(), Plugs.FilledCircle, NewDomain.LinkRoleVariants.First(), Plugs.FilledArrow, Brushes.CadetBlue);

            RelationshipDef = new RelationshipDefinition(NewDomain, GenericRelationshipDefinition, "Invocation", "Invocation", Shapes.Card, "Invocation.", null,
                                                         new LinkRoleDefinition(ERoleType.Origin, "Source", "Source", "Source."),
                                                         new LinkRoleDefinition(ERoleType.Target, "Target", "Target", "Target."));
            NewDomain.RelationshipDefinitions.Add(RelationshipDef);
            RelationshipDef.IsSimple = true;
            RelationshipDef.HideCentralSymbolWhenSimple = true;
            RelationshipDef.ShowNameIfHidingCentralSymbol = true;
            RelationshipDef.DefaultSymbolFormat = new VisualSymbolFormat(Brushes.Azure, Brushes.CadetBlue);
            RelationshipDef.DefaultSymbolFormat.SetTextFormat(ETextPurpose.Title, new TextFormat("Arial", 8, Brushes.Black));
            RelationshipDef.DefaultConnectorsFormat = new VisualConnectorsFormat(NewDomain.LinkRoleVariants.First(), Plugs.EmptyCircle, NewDomain.LinkRoleVariants.First(), Plugs.EmptyArrow, Brushes.Blue);
            RelationshipDef.DefaultConnectorsFormat.LabelLinkVariant = true;
            RelationshipDef.DefaultConnectorsFormat.LabelLinkDefinitor = true;
            RelationshipDef.DefaultConnectorsFormat.LabelLinkDescriptor = true;

            RelationshipDef = new RelationshipDefinition(NewDomain, GenericRelationshipDefinition, "Dependency", "Dependency", Shapes.HexagonHorizontal, "Dependency.", null,
                                                         new LinkRoleDefinition(ERoleType.Origin, "Dependant", "Dependant", "Dependant."),
                                                         new LinkRoleDefinition(ERoleType.Target, "Dominant", "Dominant", "Dominant."));
            NewDomain.RelationshipDefinitions.Add(RelationshipDef);
            RelationshipDef.DefaultSymbolFormat = new VisualSymbolFormat(Brushes.PeachPuff, Brushes.DarkRed);
            RelationshipDef.DefaultSymbolFormat.SetTextFormat(ETextPurpose.Title, new TextFormat("Arial", 8, Brushes.DarkRed));
            RelationshipDef.DefaultConnectorsFormat = new VisualConnectorsFormat(NewDomain.LinkRoleVariants.First(), Plugs.LineDoubleDash, NewDomain.LinkRoleVariants.First(), Plugs.SimpleArrow, Brushes.Red,
                                                                                 Display.SegmentedLineStyle);

            RelationshipDef = new RelationshipDefinition(NewDomain, GenericRelationshipDefinition, "Inheritance", "Inheritance", Shapes.Triangle, "Inheritance.", null,
                                                         new LinkRoleDefinition(ERoleType.Origin, "Descendant", "Descendant", "Descendant.", null, NewDomain.LinkRoleVariants.GetByTechName("0..*")),
                                                         new LinkRoleDefinition(ERoleType.Target, "Ancestor", "Ancestor", "Ancestor.", null, NewDomain.LinkRoleVariants.GetByTechName("0..1")));
            NewDomain.RelationshipDefinitions.Add(RelationshipDef);
            RelationshipDef.DefaultSymbolFormat = new VisualSymbolFormat(Brushes.Beige, Brushes.Sienna);
            RelationshipDef.DefaultSymbolFormat.SetTextFormat(ETextPurpose.Title, new TextFormat("Arial", 8, Brushes.Black));
            RelationshipDef.DefaultConnectorsFormat = new VisualConnectorsFormat(NewDomain.LinkRoleVariants.First(), Plugs.None, NewDomain.LinkRoleVariants.First(), Plugs.EmptyArrow, Brushes.Red);

            RelationshipDef = new RelationshipDefinition(NewDomain, GenericRelationshipDefinition, "Association", "Association", Shapes.Rhomb, "Association.", null,
                                                         new LinkRoleDefinition(ERoleType.Origin, "Associator", "Associator", "Associator."),
                                                         new LinkRoleDefinition(ERoleType.Target, "Associated", "Associated", "Associated."));
            NewDomain.RelationshipDefinitions.Add(RelationshipDef);
            RelationshipDef.DefaultSymbolFormat = new VisualSymbolFormat(Brushes.Yellow, Brushes.Goldenrod);
            RelationshipDef.DefaultSymbolFormat.SetTextFormat(ETextPurpose.Title, new TextFormat("Arial", 8, Brushes.DarkBlue));
            RelationshipDef.DefaultConnectorsFormat = new VisualConnectorsFormat(NewDomain.LinkRoleVariants.First(), Plugs.None, NewDomain.LinkRoleVariants.First(), Plugs.FilledArrow, Brushes.Gray);
        } */

        /// <summary>
        /// Colors used as base (initial set) for creating selectable styles.
        /// </summary>
        public static readonly List<Color> BaseStylesForegroundColors = new List<Color>
                { Colors.Black, Colors.Gray, Colors.SlateGray, Colors.RoyalBlue, Colors.Blue, Colors.SteelBlue, Colors.Teal, Colors.Green,
                  Colors.LimeGreen, Colors.Sienna, Colors.Purple, Colors.Red, Colors.OrangeRed, Colors.Orange, Colors.Gold, Colors.Yellow,
                  Colors.PaleVioletRed, Colors.DarkRed, Colors.Crimson, Colors.SteelBlue, Colors.DodgerBlue, Colors.MediumTurquoise, Colors.SteelBlue, Colors.Olive };

        /// <summary>
        /// Colors used as base (final set) for creating selectable styles.
        /// </summary>
        public static readonly List<Color> BaseStylesFinalColors = new List<Color>()
                { Colors.DarkGray, Colors.LightGray, Colors.LightSteelBlue, Colors.DodgerBlue, Colors.SkyBlue, Colors.LightBlue, Colors.MediumAquamarine, Colors.LightSeaGreen, 
                  Colors.LightGreen, Colors.Tan, Colors.Plum, Colors.LightPink, Colors.PeachPuff, Colors.PapayaWhip, Colors.Cornsilk, Colors.LightYellow,
                  Colors.LavenderBlush, Colors.DarkSalmon, Colors.LightCoral, Colors.Gainsboro, Colors.PaleTurquoise, Colors.Honeydew, Colors.YellowGreen, Colors.Linen };

        /// <summary>
        /// Contains the applicable graphic styles for lines and shapes.
        /// Each style is a FrameworkElement whose Tag property has: line-foreground, line-thickness, line-dash-style, shape-background.
        /// </summary>
        public static List<Tuple<Brush, double, DashStyle, Brush>> ApplicableGraphicStyles { get; private set; }

        /// <summary>
        /// Gets an standard non specialized ConceptDefinition, for making free Compositions.
        /// </summary>
        public static ConceptDefinition GenericConceptDefinition { get; private set; }

        /// <summary>
        /// Gets an standard non specialized RelationshipDefinition, for making free Compositions.
        /// </summary>
        public static RelationshipDefinition GenericRelationshipDefinition { get; private set; }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Name">Name of the Domain.</param>
        /// <param name="TechName">Technical Name of the Domain.</param>
        /// <param name="Summary">Summary of the Domain.</param>
        /// <param name="Pictogram">Image representing the Domain.</param>
        private Domain(string Name, string TechName, string Summary = "", ImageSource Pictogram = null)
            : base(null, null, Name, TechName, Shapes.Folder, Summary, Pictogram)
        {
            this.OwnerComposition = null; // Initially, for generic root domain. The later will be created via cloning.
            this.CompositeContentDomain = this;

            this.Version = new VersionCard();

            // MIME-Type: "application/x-instrumind-thinkcomposer-domain"
            this.Classification = new ClassificationCard(FileDataType.FileTypeDomain.MimeType);

            // .........................................................................................................
            this.ConceptDefCategories = new EditableList<MetaCategory<ConceptDefinition>>(__ConceptDefCategories.TechName, this);
            this.RelationshipDefCategories = new EditableList<MetaCategory<RelationshipDefinition>>(__RelationshipDefCategories.TechName, this);
            this.LinkRoleVariants = new EditableList<SimplePresentationElement>(__LinkRoleVariants.TechName, this);
            this.MarkerDefinitions = new EditableList<MarkerDefinition>(__MarkerDefinitions.TechName, this);
            this.TableDefCategories = new EditableList<MetaCategory<TableDefinition>>(__TableDefCategories.TechName, this);
            this.FieldDefCategories = new EditableList<MetaCategory<FieldDefinition>>(__FieldDefCategories.TechName, this);
            this.MarkerClusters = new EditableList<SimplePresentationElement>(__MarkerClusters.TechName, this);

            this.DeclareExtraCollections();

            // Declaration of standard Categories...
            this.DefaultTableDefCategory = new MetaCategory<TableDefinition>("Domain", "DOM", "Domain's standard Table-Structure Definitions");

            // Declaration of standard Tables...
            this.DefaultTableDef = new TableDefinition(this, "Standard", "Standard", "General purpose Table-Structure Definition having ID, Name and Description");
            this.DefaultTableDef.FieldDefinitions.Add(new FieldDefinition(DefaultTableDef, "ID", "Id", DataType.DataTypeText, "Identification number/code"));
            this.DefaultTableDef.FieldDefinitions.Add(new FieldDefinition(DefaultTableDef, "Name", "Name", DataType.DataTypeText, "Name"));
            this.DefaultTableDef.FieldDefinitions.Add(new FieldDefinition(DefaultTableDef, "Description", "Desc", DataType.DataTypeTextLong, "Detailed Description"));
            /*T this.DefaultTableDef.FieldDefinitions.Add(new FieldDefinition(DefaultTableDef, "Creation", "Creation", DataType.DataTypeDate, "Creation Date"));
            this.DefaultTableDef.FieldDefinitions.Add(new FieldDefinition(DefaultTableDef, "Active", "Active", DataType.DataTypeSwitch, "Is Active?"));
            //T this.DefaultTableDef.FieldDefinitions.Add(new FieldDefinition(DefaultTableDef, "Status", "Status", this.GetDataType("ValidationState"), "Validation status."));
            this.DefaultTableDef.FieldDefinitions.Add(new FieldDefinition(DefaultTableDef, "Comment", "Comment", DataType.DataTypeTextLong, "Remarks")); */
            this.DefaultTableDef.AlterStructure();

            this.DefaultTableDef.Categories.Add(DefaultTableDefCategory);

            // IMPORTANT: Put this as the first-one (else the Domain's custom-fields table-def is the default structure for creating tables)
            this.TableDefinitions.Insert(0, this.DefaultTableDef);

            ModelFixes.ModelRev3_AddSingleFieldTableDefs(this);

            // Declaration of standard Link-Role Variants...
            this.LinkRoleVariants.Add(new SimplePresentationElement(STD_LNKVAR_NAME, STD_LNKVAR_TECHNAME, "Standard relationship link-role variant."));
            this.LinkRoleVariants.Add(new SimplePresentationElement("1..1", "1..1", "Multiplicity: Only One occurrence."));
            this.LinkRoleVariants.Add(new SimplePresentationElement("0..1", "0..1", "Multiplicity: Zero or One occurrences."));
            this.LinkRoleVariants.Add(new SimplePresentationElement("1..*", "1..N", "Multiplicity: One to Unlimited occurrences."));     // Notice that (currently) the Tech-Name cannot accept '*'.
            this.LinkRoleVariants.Add(new SimplePresentationElement("0..*", "0..N", "Multiplicity: Zero to Unlimited occurrences."));    // Notice that (currently) the Tech-Name cannot accept '*'.

            // Declaration of standard Marker Definitions...
            // Flags...
            this.MarkerDefinitions.Add(new MarkerDefinition("Normal", "Normal", "Regular state.", Display.GetAppImage("flag_white.png"), "Flag"));
            this.MarkerDefinitions.Add(new MarkerDefinition("Correct", "Correct", "Correct state.", Display.GetAppImage("flag_green.png"), "Flag", Brushes.Green));
            this.MarkerDefinitions.Add(new MarkerDefinition("Incorrect", "Incorrect", "Incorrect state.", Display.GetAppImage("flag_red.png"), "Flag", Brushes.Red));
            this.MarkerDefinitions.Add(new MarkerDefinition("Warning", "Warning", "Warning state.", Display.GetAppImage("flag_yellow.png"), "Flag", Brushes.Yellow));
            this.MarkerDefinitions.Add(new MarkerDefinition("Dangerous", "Dangerous", "Dangerous state.", Display.GetAppImage("flag_orange.png"), "Flag", Brushes.Orange));
            this.MarkerDefinitions.Add(new MarkerDefinition("Strange", "Strange", "Strange state.", Display.GetAppImage("flag_purple.png"), "Flag", Brushes.Orange));
            this.MarkerDefinitions.Add(new MarkerDefinition("Sensitive", "Sensitive", "Sensitive state.", Display.GetAppImage("flag_pink.png"), "Flag", Brushes.Orange));
            this.MarkerDefinitions.Add(new MarkerDefinition("Extinct", "Extinct", "Extinct state.", Display.GetAppImage("flag_black.png"), "Flag", Brushes.Black, Brushes.White));
            this.MarkerDefinitions.Add(new MarkerDefinition("Special", "Special", "Special-Case state.", Display.GetAppImage("flag_blue.png"), "Flag", Brushes.RoyalBlue, Brushes.WhiteSmoke));
            this.MarkerDefinitions.Add(new MarkerDefinition("Unknown", "Unknown", "Unknown state.", Display.GetAppImage("flag_gray.png"), "Flag", Brushes.Gray));

            // Awards...
            // this.MarkerDefinitions.Add(new MarkerDefinition("Star Gold 1", "Star Gold_1", "Star Gold 1 award.", Display.GetImage("award_star_gold_1.png"), "Award"));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Star Gold 2", "Star Gold_2", "Star Gold 2 award.", Display.GetImage("award_star_gold_2.png"), "Award"));
            this.MarkerDefinitions.Add(new MarkerDefinition("Star Gold", "Star Gold", "Star Gold award.", Display.GetAppImage("award_star_gold_3.png"), "Award"));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Star Silver 1", "Star_Silver_1", "Star Silver 1 award.", Display.GetImage("award_star_silver_1.png"), "Award"));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Star Silver 2", "Star_Silver_2", "Star Silver 2 award.", Display.GetImage("award_star_silver_2.png"), "Award"));
            this.MarkerDefinitions.Add(new MarkerDefinition("Star Silver", "Star_Silver", "Star Silver award.", Display.GetAppImage("award_star_silver_3.png"), "Award"));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Star Bronze 1", "Star_Bronze_1", "Star Bronze 1 award.", Display.GetImage("award_star_bronze_1.png"), "Award"));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Star Bronze 2", "Star_Bronze_2", "Star Bronze 2 award.", Display.GetImage("award_star_bronze_2.png"), "Award"));
            this.MarkerDefinitions.Add(new MarkerDefinition("Star Bronze", "Star_Bronze", "Star Bronze award.", Display.GetAppImage("award_star_bronze_3.png"), "Award"));

            this.MarkerDefinitions.Add(new MarkerDefinition("Medal Gold", "Medal_Gold", "Medal Gold award.", Display.GetAppImage("medal_gold_1.png"), "Award"));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Medal Gold 2", "Medal_Gold_2", "Medal Gold 2 award.", Display.GetImage("medal_gold_2.png"), "Award"));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Medal Gold 3", "Medal_Gold_3", "Medal Gold 3 award.", Display.GetImage("medal_gold_3.png"), "Award"));
            this.MarkerDefinitions.Add(new MarkerDefinition("Medal Silver", "Medal_Silver", "Medal Silver award.", Display.GetAppImage("medal_silver_1.png"), "Award"));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Medal Silver 2", "Medal_Silver_2", "Medal Silver 2 award.", Display.GetImage("medal_silver_2.png"), "Award"));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Medal Silver 3", "Medal_Silver_3", "Medal Silver 3 award.", Display.GetImage("medal_silver_3.png"), "Award"));
            this.MarkerDefinitions.Add(new MarkerDefinition("Medal Bronze", "Medal_Bronze", "Medal Bronze award.", Display.GetAppImage("medal_bronze_1.png"), "Award"));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Medal Bronze 2", "Medal_Bronze_2", "Medal Bronze 2 award.", Display.GetImage("medal_bronze_2.png"), "Award"));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Medal Bronze 3", "Medal_Bronze_3", "Medal Bronze 3 award.", Display.GetImage("medal_bronze_3.png"), "Award"));

            // Control...
            this.MarkerDefinitions.Add(new MarkerDefinition("Task Init", "Task_Init", "Task just initiated.", Display.GetAppImage("task_init.png"), "Control"));
            this.MarkerDefinitions.Add(new MarkerDefinition("Task 25%", "Task_25pc", "Task at 25%.", Display.GetAppImage("task_25pc.png"), "Control"));
            this.MarkerDefinitions.Add(new MarkerDefinition("Task 33%", "Task_33pc", "Task at 33%.", Display.GetAppImage("task_33pc.png"), "Control"));
            this.MarkerDefinitions.Add(new MarkerDefinition("Task 50%", "Task_50pc", "Task at 50%.", Display.GetAppImage("task_50pc.png"), "Control"));
            this.MarkerDefinitions.Add(new MarkerDefinition("Task 66%", "Task_66pc", "Task at 66%.", Display.GetAppImage("task_66pc.png"), "Control"));
            this.MarkerDefinitions.Add(new MarkerDefinition("Task 75%", "Task_75pc", "Task at 75%.", Display.GetAppImage("task_75pc.png"), "Control"));
            this.MarkerDefinitions.Add(new MarkerDefinition("Task Completed", "Task_Comp", "Task fully completed.", Display.GetAppImage("task_comp.png"), "Control"));
            this.MarkerDefinitions.Add(new MarkerDefinition("Task Paused", "Task_Paused", "Task paused.", Display.GetAppImage("task_paused.png"), "Control"));
            
            this.MarkerDefinitions.Add(new MarkerDefinition("Priority 1", "Priority_1", "Priority 1.", Display.GetAppImage("priority_1_blue.png"), "Control"));
            this.MarkerDefinitions.Add(new MarkerDefinition("Priority 2", "Priority_2", "Priority 2.", Display.GetAppImage("priority_2_blue.png"), "Control"));
            this.MarkerDefinitions.Add(new MarkerDefinition("Priority 3", "Priority_3", "Priority 3.", Display.GetAppImage("priority_3_blue.png"), "Control"));
            this.MarkerDefinitions.Add(new MarkerDefinition("Priority 4", "Priority_4", "Priority 4.", Display.GetAppImage("priority_4_blue.png"), "Control"));

            // Tags...
            this.MarkerDefinitions.Add(new MarkerDefinition("White", "White", "White tag.", Display.GetAppImage("tag_white.png"), "Tag"));
            this.MarkerDefinitions.Add(new MarkerDefinition("Green", "Green", "Green tag.", Display.GetAppImage("tag_green.png"), "Tag", Brushes.Green));
            this.MarkerDefinitions.Add(new MarkerDefinition("Red", "Red", "Red tag.", Display.GetAppImage("tag_red.png"), "Tag", Brushes.Red));
            this.MarkerDefinitions.Add(new MarkerDefinition("Yellow", "Yellow", "Yellow tag.", Display.GetAppImage("tag_yellow.png"), "Tag", Brushes.Yellow));
            this.MarkerDefinitions.Add(new MarkerDefinition("Orange", "Orange", "Orange tag.", Display.GetAppImage("tag_orange.png"), "Tag", Brushes.Orange));
            this.MarkerDefinitions.Add(new MarkerDefinition("Black", "Black", "Black tag.", Display.GetAppImage("tag_black.png"), "Tag", Brushes.Black, Brushes.White));
            this.MarkerDefinitions.Add(new MarkerDefinition("Blue", "Blue", "Blue tag.", Display.GetAppImage("tag_blue.png"), "Tag", Brushes.RoyalBlue, Brushes.WhiteSmoke));
            this.MarkerDefinitions.Add(new MarkerDefinition("Gray", "Gray", "Gray tag.", Display.GetAppImage("tag_gray.png"), "Tag", Brushes.Gray));

            // User-Defined markers (not created to save space)...
            /*
            this.MarkerDefinitions.Add(new MarkerDefinition("Evilgrin", "Evilgrin", "Evilgrin emoticon.", Display.GetImage("emoticon_evilgrin.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Grin", "Grin", "Grin emoticon.", Display.GetImage("emoticon_grin.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Happy", "Happy", "Happy emoticon.", Display.GetImage("emoticon_happy.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Smile", "Smile", "Smile emoticon.", Display.GetImage("emoticon_smile.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Surprised", "Surprised", "Surprised emoticon.", Display.GetImage("emoticon_surprised.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Tongue", "Tongue", "Tongue emoticon.", Display.GetImage("emoticon_tongue.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Unhappy", "Unhappy", "Unhappy emoticon.", Display.GetImage("emoticon_unhappy.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Waii", "Waii", "Waii emoticon.", Display.GetImage("emoticon_waii.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Wink", "Wink", "Wink emoticon.", Display.GetImage("emoticon_wink.png"), MarkerDefinition.USERDEF_CODE));

            this.MarkerDefinitions.Add(new MarkerDefinition("Accept", "accept", "accept marker.", Display.GetImage("accept.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Anchor", "anchor", "anchor marker.", Display.GetImage("anchor.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Arrow branch", "arrow_branch", "arrow branch marker.", Display.GetImage("arrow_branch.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Arrow divide", "arrow_divide", "arrow divide marker.", Display.GetImage("arrow_divide.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Arrow down", "arrow_down", "arrow down marker.", Display.GetImage("arrow_down.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Arrow in", "arrow_in", "arrow in marker.", Display.GetImage("arrow_in.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Arrow inout", "arrow_inout", "arrow inout marker.", Display.GetImage("arrow_inout.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Arrow join", "arrow_join", "arrow join marker.", Display.GetImage("arrow_join.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Arrow left", "arrow_left", "arrow left marker.", Display.GetImage("arrow_left.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Arrow merge", "arrow_merge", "arrow merge marker.", Display.GetImage("arrow_merge.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Arrow out", "arrow_out", "arrow out marker.", Display.GetImage("arrow_out.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Arrow redo", "arrow_redo", "arrow redo marker.", Display.GetImage("arrow_redo.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Arrow refresh", "arrow_refresh", "arrow refresh marker.", Display.GetImage("arrow_refresh.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Arrow refresh small", "arrow_refresh_small", "arrow refresh small marker.", Display.GetImage("arrow_refresh_small.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Arrow right", "arrow_right", "arrow right marker.", Display.GetImage("arrow_right.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Arrow rotate anticlockwise", "arrow_rotate_anticlockwise", "arrow rotate anticlockwise marker.", Display.GetImage("arrow_rotate_anticlockwise.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Arrow rotate clockwise", "arrow_rotate_clockwise", "arrow rotate clockwise marker.", Display.GetImage("arrow_rotate_clockwise.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Arrow switch", "arrow_switch", "arrow switch marker.", Display.GetImage("arrow_switch.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Arrow turn left", "arrow_turn_left", "arrow turn left marker.", Display.GetImage("arrow_turn_left.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Arrow turn right", "arrow_turn_right", "arrow turn right marker.", Display.GetImage("arrow_turn_right.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Arrow undo", "arrow_undo", "arrow undo marker.", Display.GetImage("arrow_undo.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Arrow up", "arrow_up", "arrow up marker.", Display.GetImage("arrow_up.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Asterisk orange", "asterisk_orange", "asterisk orange marker.", Display.GetImage("asterisk_orange.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Asterisk yellow", "asterisk_yellow", "asterisk yellow marker.", Display.GetImage("asterisk_yellow.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Attach", "attach", "attach marker.", Display.GetImage("attach.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Basket", "basket", "basket marker.", Display.GetImage("basket.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Bell", "bell", "bell marker.", Display.GetImage("bell.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Bomb", "bomb", "bomb marker.", Display.GetImage("bomb.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Book", "book", "book marker.", Display.GetImage("book.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Book addresses", "book_addresses", "book addresses marker.", Display.GetImage("book_addresses.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Box", "box", "box marker.", Display.GetImage("box.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Brick", "brick", "brick marker.", Display.GetImage("brick.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Bricks", "bricks", "bricks marker.", Display.GetImage("bricks.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Briefcase", "briefcase", "briefcase marker.", Display.GetImage("briefcase.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Bug", "bug", "bug marker.", Display.GetImage("bug.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Cake", "cake", "cake marker.", Display.GetImage("cake.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Calculator", "calculator", "calculator marker.", Display.GetImage("calculator.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Calendar", "calendar", "calendar marker.", Display.GetImage("calendar.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Camera", "camera", "camera marker.", Display.GetImage("camera.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Cancel", "cancel", "cancel marker.", Display.GetImage("cancel.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Car", "car", "car marker.", Display.GetImage("car.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Cart", "cart", "cart marker.", Display.GetImage("cart.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Chart line", "chart_line", "chart line marker.", Display.GetImage("chart_line.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Chart organisation", "chart_organisation", "chart organisation marker.", Display.GetImage("chart_organisation.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Chart pie", "chart_pie", "chart pie marker.", Display.GetImage("chart_pie.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Clock", "clock", "clock marker.", Display.GetImage("clock.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Clock red", "clock_red", "clock red marker.", Display.GetImage("clock_red.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Cog", "cog", "cog marker.", Display.GetImage("cog.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Coins", "coins", "coins marker.", Display.GetImage("coins.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Color swatch", "color_swatch", "color swatch marker.", Display.GetImage("color_swatch.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Color wheel", "color_wheel", "color wheel marker.", Display.GetImage("color_wheel.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Comment", "comment", "comment marker.", Display.GetImage("comment.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Connect", "connect", "connect marker.", Display.GetImage("connect.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Controller", "controller", "controller marker.", Display.GetImage("controller.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Cross", "cross", "cross marker.", Display.GetImage("cross.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Cup", "cup", "cup marker.", Display.GetImage("cup.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Cut", "cut", "cut marker.", Display.GetImage("cut.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Cut red", "cut_red", "cut red marker.", Display.GetImage("cut_red.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Date", "date", "date marker.", Display.GetImage("date.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Disconnect", "disconnect", "disconnect marker.", Display.GetImage("disconnect.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Disk", "disk", "disk marker.", Display.GetImage("disk.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Disk multiple", "disk_multiple", "disk multiple marker.", Display.GetImage("disk_multiple.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Door", "door", "door marker.", Display.GetImage("door.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Door in", "door_in", "door in marker.", Display.GetImage("door_in.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Door open", "door_open", "door open marker.", Display.GetImage("door_open.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Door out", "door_out", "door out marker.", Display.GetImage("door_out.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Drink", "drink", "drink marker.", Display.GetImage("drink.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Drink empty", "drink_empty", "drink empty marker.", Display.GetImage("drink_empty.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Email", "email", "email marker.", Display.GetImage("email.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Error", "error", "error marker.", Display.GetImage("error.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Exclamation", "exclamation", "exclamation marker.", Display.GetImage("exclamation.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Eye", "eye", "eye marker.", Display.GetImage("eye.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Female", "female", "female marker.", Display.GetImage("female.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Film", "film", "film marker.", Display.GetImage("film.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Find", "find", "find marker.", Display.GetImage("find.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Group", "group", "group marker.", Display.GetImage("group.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Heart", "heart", "heart marker.", Display.GetImage("heart.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Help", "help", "help marker.", Display.GetImage("help.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Hourglass", "hourglass", "hourglass marker.", Display.GetImage("hourglass.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("House", "house", "house marker.", Display.GetImage("house.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Images", "images", "images marker.", Display.GetImage("images.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Information", "information", "information marker.", Display.GetImage("information.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Key", "key", "key marker.", Display.GetImage("key.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Keyboard", "keyboard", "keyboard marker.", Display.GetImage("keyboard.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Layers", "layers", "layers marker.", Display.GetImage("layers.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Layout content", "layout_content", "layout content marker.", Display.GetImage("layout_content.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Lightbulb", "lightbulb", "lightbulb marker.", Display.GetImage("lightbulb.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Lightbulb off", "lightbulb_off", "lightbulb off marker.", Display.GetImage("lightbulb_off.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Lightning", "lightning", "lightning marker.", Display.GetImage("lightning.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Link", "link", "link marker.", Display.GetImage("link.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Lock", "lock", "lock marker.", Display.GetImage("lock.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Lorry", "lorry", "lorry marker.", Display.GetImage("lorry.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Lorry flatbed", "lorry_flatbed", "lorry flatbed marker.", Display.GetImage("lorry_flatbed.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Magnifier", "magnifier", "magnifier marker.", Display.GetImage("magnifier.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Male", "male", "male marker.", Display.GetImage("male.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Map", "map", "map marker.", Display.GetImage("map.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Money", "money", "money marker.", Display.GetImage("money.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Money dollar", "money_dollar", "money dollar marker.", Display.GetImage("money_dollar.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Money euro", "money_euro", "money euro marker.", Display.GetImage("money_euro.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Money pound", "money_pound", "money pound marker.", Display.GetImage("money_pound.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Money yen", "money_yen", "money yen marker.", Display.GetImage("money_yen.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Monitor", "monitor", "monitor marker.", Display.GetImage("monitor.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Music", "music", "music marker.", Display.GetImage("music.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("New", "new", "new marker.", Display.GetImage("new.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Newspaper", "newspaper", "newspaper marker.", Display.GetImage("newspaper.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Note", "note", "note marker.", Display.GetImage("note.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Paintbrush", "paintbrush", "paintbrush marker.", Display.GetImage("paintbrush.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Paintcan", "paintcan", "paintcan marker.", Display.GetImage("paintcan.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Pencil", "pencil", "pencil marker.", Display.GetImage("pencil.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Phone", "phone", "phone marker.", Display.GetImage("phone.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Photo", "photo", "photo marker.", Display.GetImage("photo.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Pill", "pill", "pill marker.", Display.GetImage("pill.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Printer", "printer", "printer marker.", Display.GetImage("printer.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Rainbow", "rainbow", "rainbow marker.", Display.GetImage("rainbow.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Related close", "related_close", "related close marker.", Display.GetImage("related_close.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Related open", "related_open", "related open marker.", Display.GetImage("related_open.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Ruby", "ruby", "ruby marker.", Display.GetImage("ruby.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Server", "server", "server marker.", Display.GetImage("server.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Shading", "shading", "shading marker.", Display.GetImage("shading.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Shape square", "shape_square", "shape square marker.", Display.GetImage("shape_square.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Sound", "sound", "sound marker.", Display.GetImage("sound.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Sport 8ball", "sport_8ball", "sport 8ball marker.", Display.GetImage("sport_8ball.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Sport basketball", "sport_basketball", "sport basketball marker.", Display.GetImage("sport_basketball.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Sport football", "sport_football", "sport football marker.", Display.GetImage("sport_football.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Sport golf", "sport_golf", "sport golf marker.", Display.GetImage("sport_golf.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Sport raquet", "sport_raquet", "sport raquet marker.", Display.GetImage("sport_raquet.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Sport shuttlecock", "sport_shuttlecock", "sport shuttlecock marker.", Display.GetImage("sport_shuttlecock.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Sport soccer", "sport_soccer", "sport soccer marker.", Display.GetImage("sport_soccer.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Sport tennis", "sport_tennis", "sport tennis marker.", Display.GetImage("sport_tennis.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Star", "star", "star marker.", Display.GetImage("star.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Sum", "sum", "sum marker.", Display.GetImage("sum.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Table multiple", "table_multiple", "table multiple marker.", Display.GetImage("table_multiple.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Telephone", "telephone", "telephone marker.", Display.GetImage("telephone.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Television", "television", "television marker.", Display.GetImage("television.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Thumb down", "thumb_down", "thumb down marker.", Display.GetImage("thumb_down.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Thumb up", "thumb_up", "thumb up marker.", Display.GetImage("thumb_up.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Tick", "tick", "tick marker.", Display.GetImage("tick.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Time", "time", "time marker.", Display.GetImage("time.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Time add", "time_add", "time add marker.", Display.GetImage("time_add.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Time delete", "time_delete", "time delete marker.", Display.GetImage("time_delete.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("User", "user", "user marker.", Display.GetImage("user.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("User comment", "user_comment", "user comment marker.", Display.GetImage("user_comment.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("User female", "user_female", "user female marker.", Display.GetImage("user_female.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("User gray", "user_gray", "user gray marker.", Display.GetImage("user_gray.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("User green", "user_green", "user green marker.", Display.GetImage("user_green.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("User orange", "user_orange", "user orange marker.", Display.GetImage("user_orange.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("User red", "user_red", "user red marker.", Display.GetImage("user_red.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("User suit", "user_suit", "user suit marker.", Display.GetImage("user_suit.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Vcard", "vcard", "vcard marker.", Display.GetImage("vcard.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Wand", "wand", "wand marker.", Display.GetImage("wand.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Weather clouds", "weather_clouds", "weather clouds marker.", Display.GetImage("weather_clouds.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Weather cloudy", "weather_cloudy", "weather cloudy marker.", Display.GetImage("weather_cloudy.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Weather lightning", "weather_lightning", "weather lightning marker.", Display.GetImage("weather_lightning.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Weather rain", "weather_rain", "weather rain marker.", Display.GetImage("weather_rain.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Weather snow", "weather_snow", "weather snow marker.", Display.GetImage("weather_snow.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Weather sun", "weather_sun", "weather sun marker.", Display.GetImage("weather_sun.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Webcam", "webcam", "webcam marker.", Display.GetImage("webcam.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("World", "world", "world marker.", Display.GetImage("world.png"), MarkerDefinition.USERDEF_CODE));
            this.MarkerDefinitions.Add(new MarkerDefinition("Wrench", "wrench", "wrench marker.", Display.GetImage("wrench.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Wrench orange", "wrench_orange", "wrench orange marker.", Display.GetImage("wrench_orange.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Zoom", "zoom", "zoom marker.", Display.GetImage("zoom.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Zoom in", "zoom_in", "zoom in marker.", Display.GetImage("zoom_in.png"), MarkerDefinition.USERDEF_CODE));
            // this.MarkerDefinitions.Add(new MarkerDefinition("Zoom out", "zoom_out", "zoom out marker.", Display.GetImage("zoom_out.png"), MarkerDefinition.USERDEF_CODE));
            */
        }

        /// <summary>
        /// Internal Constructor for Agents and Clonning.
        /// </summary>
        internal Domain()
        {
        }

        /// <summary>
        /// Initializes the instance for use after creation or deserialization.
        /// </summary>
        [OnDeserialized]
        protected void Initialize(StreamingContext context = default(StreamingContext))
        {
            if (this.TemplateComposition_ != null)
                this.OwnerComposition_ = this.TemplateComposition_;

            if (this.ViewBackgroundBrush_ == null || this.ViewBackgroundImage_ == null)
            {
                this.ViewBackgroundBrush = ViewStandardBackground;
                this.ViewBackgroundImage_ = new ImageAssignment();
            }

            this.DeclareExtraCollections();

            // Initialize for old Domains
            if (this.ConceptDefClusters == null)
                this.ConceptDefClusters = new EditableList<FormalPresentationElement>(__ConceptDefClusters.TechName, this);

            if (this.RelationshipDefClusters == null)
                this.RelationshipDefClusters = new EditableList<FormalPresentationElement>(__RelationshipDefClusters.TechName, this);

            if (this.ViewGridSize == default(double))
                this.ViewGridSize = View.GRID_SIZE_INI;
        }

        /// <summary>
        /// Serializes the last model revision performed.
        /// </summary>
        public int ModelRevision { get; set; }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public void DeclareExtraCollections()
        {
            // External Languages
            if (this.ExternalLanguages == null)
                this.ExternalLanguages = new EditableList<ExternalLanguageDeclaration>(__ExternalLanguages.TechName, this);

            if (!this.ExternalLanguages.Any(exl => exl.TechName == EXTLANG_CODE_TEXT))
                this.ExternalLanguages.Add(new ExternalLanguageDeclaration("Text", EXTLANG_CODE_TEXT, "Plain-text language."));

            if (!this.ExternalLanguages.Any(exl => exl.TechName == EXTLANG_CODE_XML))
                this.ExternalLanguages.Add(new ExternalLanguageDeclaration("XML", EXTLANG_CODE_XML, "XML language."));

            // Output-Templates
            // NOTE: Domain (as IdeaDefinition) already has declared its output-templates collection.
            if (this.OutputTemplatesForConcepts == null)
                this.OutputTemplatesForConcepts = new EditableList<TextTemplate>(__OutputTemplatesForConcepts.TechName, this);

            if (this.OutputTemplatesForRelationships == null)
                this.OutputTemplatesForRelationships = new EditableList<TextTemplate>(__OutputTemplatesForRelationships.TechName, this);

            // Populate initial templates
            foreach (var Language in this.ExternalLanguages)
                this.DeclareOutputTemplate(Language);

            // Idea-Def Clusters
            if (this.ConceptDefClusters == null)
                this.ConceptDefClusters = new EditableList<FormalPresentationElement>(__ConceptDefClusters.TechName, this);

            if (this.RelationshipDefClusters == null)
                this.RelationshipDefClusters = new EditableList<FormalPresentationElement>(__RelationshipDefClusters.TechName, this);
        }

        protected void DeclareOutputTemplate(ExternalLanguageDeclaration Language)
        {
            string TemplateText = null;

            var TargetTemplate = this.OutputTemplates.FirstOrDefault(tpl => tpl.Language.IsEqual(Language));
            if (TargetTemplate == null || TargetTemplate.Text.IsAbsent())
            {
                TemplateText = Display.GetEmbeddedResourceString(BASETEMPLATE_LOCATION + "." + Language.TechName + "_Composition." + Domain.TEMPLATE_FILE_EXT).NullDefault("");

                if (TargetTemplate == null)
                    this.OutputTemplates.Add(new TextTemplate(Language, TemplateText));
                else
                    TargetTemplate.Text = TemplateText;
            }

            TemplateText = Display.GetEmbeddedResourceString(BASETEMPLATE_LOCATION + "." + Language.TechName + "_IdeaBase." + Domain.TEMPLATE_FILE_EXT).NullDefault("");
            var PlaceMark = GenerationManager.GENPAR_PREFIX + GenerationManager.GENKEY_POS_EXTENSION;
            var ExtensionPosition = TemplateText.IndexOf(PlaceMark, StringComparison.OrdinalIgnoreCase);

            TargetTemplate = this.OutputTemplatesForConcepts.FirstOrDefault(tpl => tpl.Language.IsEqual(Language));
            if (TargetTemplate == null || TargetTemplate.Text.IsAbsent())
            {
                var TplExtension = Display.GetEmbeddedResourceString(BASETEMPLATE_LOCATION + "." + Language.TechName + "_ConceptExtension").NullDefault("");

                var TplText = (ExtensionPosition < 0
                               ? TemplateText + TplExtension
                               : TemplateText.ReplaceAt(ExtensionPosition, PlaceMark.Length, TplExtension));

                if (TargetTemplate == null)
                    this.OutputTemplatesForConcepts.Add(new TextTemplate(Language, TplText));
                else
                    TargetTemplate.Text = TplText;
            }

            TargetTemplate = this.OutputTemplatesForRelationships.FirstOrDefault(tpl => tpl.Language.IsEqual(Language));
            if (TargetTemplate == null || TargetTemplate.Text.IsAbsent())
            {
                var TplExtension = Display.GetEmbeddedResourceString(BASETEMPLATE_LOCATION + "." + Language.TechName + "_RelationshipExtension").NullDefault("");

                var TplText = (ExtensionPosition < 0
                               ? TemplateText + TplExtension
                               : TemplateText.ReplaceAt(ExtensionPosition, PlaceMark.Length, TplExtension));

                if (TargetTemplate == null)
                    this.OutputTemplatesForRelationships.Add(new TextTemplate(Language, TplText));
                else
                    TargetTemplate.Text = TplText;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Indicates wether this Domain is selected for later applying a command.
        /// </summary>
        public bool IsSelected
        {
            get { return this.IsSelected_; }
            set
            {
                if (this.IsSelected_ == value)
                    return;

                if (!this.EditEngine.IsVariating)
                    throw new UsageAnomaly("Selection changes must be applied within a Command");

                this.IsSelected_ = value;

                this.NotifyPropertyChange("IsSelected");
            }
        }
        [NonSerialized]
        protected bool IsSelected_ = false;

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the Data Type having the supplied Key (a TechName)
        /// </summary>
        public DataType GetDataType(string Key)
        {
            return this.AvailableDataTypes.FirstOrDefault(dt => dt.TechName == Key);
        }

        /// <summary>
        /// Table-Structure Definition used as the by-default for new Tables.
        /// </summary>
        public TableDefinition DefaultTableDef { get { return __DefaultTableDef.Get(this); } protected set { __DefaultTableDef.Set(this, value); } }
        protected TableDefinition DefaultTableDef_ = null;
        private static ModelPropertyDefinitor<Domain, TableDefinition> __DefaultTableDef =
                   new ModelPropertyDefinitor<Domain, TableDefinition>("DefaultTableDef", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.DefaultTableDef_, (ins, val) => ins.DefaultTableDef_ = val, true, false,
                                                                       "Default Table-Structure Definition", "Table-Structure Definition used as the by-default for new Tables.");

        /// <summary>
        /// Category used to classify Table-Structure Definitions by default.
        /// </summary>
        public MetaCategory<TableDefinition> DefaultTableDefCategory { get { return __DefaultTableDefCategory.Get(this); } protected set { __DefaultTableDefCategory.Set(this, value); } }
        protected MetaCategory<TableDefinition> DefaultTableDefCategory_ = null;
        private static ModelPropertyDefinitor<Domain, MetaCategory<TableDefinition>> __DefaultTableDefCategory =
                   new ModelPropertyDefinitor<Domain, MetaCategory<TableDefinition>>("DefaultTableDefCategory", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.DefaultTableDefCategory_, (ins, val) => ins.DefaultTableDefCategory_ = val, true, false,
                                                                                     "Default Table-Structure Definition Category", "Category used to classify Table-Structure Definitions by default.");

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Graphic representation of the object.
        /// </summary>
        public override ImageSource Pictogram
        {
            // Gets the stored image only if it is Bitmap-based (assuming an external source).
            get { return (base.Pictogram == null || base.Pictogram is DrawingImage ? DefaultPictogram : base.Pictogram); }
            set { base.Pictogram = value; }
        }

        /// <summary>
        /// Sets the Composition owning this Domain.
        /// </summary>
        public void SetOwnerComposition(Composition OwnerComposition)
        {
            this.OwnerComposition_ = OwnerComposition;
        }

        /// <summary>
        /// Composition owning this Domain instance.
        /// Note: Domains are shared via Copy and later Consolidation/Resync (not referencing the same centralized instance).
        /// </summary>
        [Description("Composition owning this Domain instance.")]
        public Composition OwnerComposition
        {
            get { return this.OwnerComposition_; }
            private set { this.OwnerComposition_ = value; }
        }
        // IMPORTANT: This must be non-serialized in case no Template is required.
        [NonSerialized]
        private Composition OwnerComposition_ = null;
        // IMPORTANT: This must be set to null if no Template should be attached when the Domain is saved externally.
        private Composition TemplateComposition_ = null;

        /// <summary>
        /// Indicates whether the owner Composition of this Domain will be used as Template for creating new ones.
        /// Use this method PRIOR to serialize the Domain.
        /// </summary>
        public void SetTemplateSaving(bool Save)
        {
            if (Save)
                this.TemplateComposition_ = this.OwnerComposition_;
            else
                this.TemplateComposition_ = null;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the Data types of the Domain (predefined plus user-defined).
        /// </summary>
        public IEnumerable<DataType> AvailableDataTypes
        {
            get
            {
                foreach (var PredefDataType in DataType.PredefinedDataTypes)
                    yield return PredefDataType;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the Shapes of the Domain (predefined plus user-defined).
        /// </summary>
        public IEnumerable<SimplePresentationElement> AvailableShapes
        {
            get
            {
                return Shapes.PredefinedShapes;
                /* foreach (var PredefShape in Shapes.PredefinedShapes)
                    yield return PredefShape; */

                /* foreach (var UserdefShape in this.UserDefinedShapes)
                    yield return UserdefShape; */
            }
        }

        /* Postponed... 
        /// <summary>
        /// Shapes of the Domain defined by users.
        /// </summary>
        public EditableList<SimplePresentationElement> UserDefinedShapes { get; protected set; }
        public static ModelListDefinitor<Domain, SimplePresentationElement> __UserDefinedShapes =
                   new ModelListDefinitor<Domain, SimplePresentationElement>("UserDefinedShapes", EEntityMembership.InternalCoreExclusive, ins => ins.UserDefinedShapes, (ins, coll) => ins.UserDefinedShapes = coll,
                                                                             "User-Defined Shapes", "Shapes of the Domain defined by users."); */

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the Plugs of the Domain (predefined plus user-defined).
        /// </summary>
        public IEnumerable<SimplePresentationElement> AvailablePlugs
        {
            get
            {
                foreach (var PredefPlug in Plugs.PredefinedPlugs)
                    yield return PredefPlug;

                /* foreach (var UserdefPlug in this.UserDefinedPlugs)
                    yield return UserdefPlug; */
            }
        }

        /* Postponed... 
        /// <summary>
        /// Plugs of the Domain defined by users.
        /// </summary>
        public EditableList<SimplePresentationElement> UserDefinedPlugs { get; protected set; }
        public static ModelListDefinitor<Domain, SimplePresentationElement> __UserDefinedPlugs =
                   new ModelListDefinitor<Domain, SimplePresentationElement>("UserDefinedPlugs", EEntityMembership.InternalCoreExclusive, ins => ins.UserDefinedPlugs, (ins, coll) => ins.UserDefinedPlugs = coll,
                                                                             "User-Defined Plugs", "Plugs of the Domain defined by users."); */

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Categories of Concept definitions.
        /// </summary>
        public EditableList<MetaCategory<ConceptDefinition>> ConceptDefCategories { get; protected set; }
        public static ModelListDefinitor<Domain, MetaCategory<ConceptDefinition>> __ConceptDefCategories =
                   new ModelListDefinitor<Domain, MetaCategory<ConceptDefinition>>("ConceptDefCategories", EEntityMembership.InternalCoreExclusive, ins => ins.ConceptDefCategories, (ins, coll) => ins.ConceptDefCategories = coll,
                                                                                   "Concept Def. Categories", "Categories of Concept definitions.");

        /// <summary>
        /// Categories of Relationship definitions.
        /// </summary>
        public EditableList<MetaCategory<RelationshipDefinition>> RelationshipDefCategories { get; protected set; }
        public static ModelListDefinitor<Domain, MetaCategory<RelationshipDefinition>> __RelationshipDefCategories =
                   new ModelListDefinitor<Domain, MetaCategory<RelationshipDefinition>>("RelationshipDefCategories", EEntityMembership.InternalCoreExclusive, ins => ins.RelationshipDefCategories, (ins, coll) => ins.RelationshipDefCategories = coll,
                                                                                        "Relationship Def. Categories", "Categories of Relationship definitions.");

        /// <summary>
        /// Predefined variants available for Link-Role Definitions (e.g.: Used for declaring Multiplicities/Cardinalities).
        /// </summary>
        public EditableList<SimplePresentationElement> LinkRoleVariants { get; protected set; }
        public static ModelListDefinitor<Domain, SimplePresentationElement> __LinkRoleVariants =
                   new ModelListDefinitor<Domain, SimplePresentationElement>("LinkRoleVariants", EEntityMembership.InternalCoreExclusive, ins => ins.LinkRoleVariants, (ins, coll) => ins.LinkRoleVariants = coll,
                                                                             "Link-Role Variants", "Predefined variants available for Link-Role Definitions (e.g.: Used for declaring Multiplicities/Cardinalities).");

        /// <summary>
        /// Simple Idea classification marker definitions.
        /// </summary>
        public EditableList<MarkerDefinition> MarkerDefinitions { get; protected set; }
        public static ModelListDefinitor<Domain, MarkerDefinition> __MarkerDefinitions =
                   new ModelListDefinitor<Domain, MarkerDefinition>("MarkerDefinitions", EEntityMembership.InternalCoreExclusive, ins => ins.MarkerDefinitions, (ins, coll) => ins.MarkerDefinitions = coll,
                                                                    "Marker Definitions", "Simple Idea classification marker definitions.");

        /// <summary>
        /// Categories of Table-Structure definitions.
        /// </summary>
        public EditableList<MetaCategory<TableDefinition>> TableDefCategories { get; protected set; }
        public static ModelListDefinitor<Domain, MetaCategory<TableDefinition>> __TableDefCategories =
                   new ModelListDefinitor<Domain, MetaCategory<TableDefinition>>("TableDefCategories", EEntityMembership.InternalCoreExclusive, ins => ins.TableDefCategories, (ins, coll) => ins.TableDefCategories = coll,
                                                                                 "Table-Structure Def. Categories", "Categories of Table-Structure definitions.");

        /// <summary>
        /// Categories of Field definitions.
        /// </summary>
        public EditableList<MetaCategory<FieldDefinition>> FieldDefCategories { get; protected set; }
        public static ModelListDefinitor<Domain, MetaCategory<FieldDefinition>> __FieldDefCategories =
                   new ModelListDefinitor<Domain, MetaCategory<FieldDefinition>>("FieldDefCategories", EEntityMembership.InternalCoreExclusive, ins => ins.FieldDefCategories, (ins, coll) => ins.FieldDefCategories = coll,
                                                                                 "Field Def. Categories", "Categories of Field definitions.");

        /// <summary>
        /// Simple clusters for grouping Markers (i.e. visually).
        /// </summary>
        public EditableList<SimplePresentationElement> MarkerClusters { get; protected set; }
        public static ModelListDefinitor<Domain, SimplePresentationElement> __MarkerClusters =
                   new ModelListDefinitor<Domain, SimplePresentationElement>("MarkerClusters", EEntityMembership.InternalCoreExclusive, ins => ins.MarkerClusters, (ins, coll) => ins.MarkerClusters = coll,
                                                                             "Marker Clusters", "Simple clusters for grouping Marker Definitions on palettes (i.e. visually).");

        /// <summary>
        /// Simple clusters for grouping Concepts.
        /// </summary>
        public EditableList<FormalPresentationElement> ConceptDefClusters { get; protected set; }
        public static ModelListDefinitor<Domain, FormalPresentationElement> __ConceptDefClusters =
                   new ModelListDefinitor<Domain, FormalPresentationElement>("ConceptDefClusters", EEntityMembership.InternalCoreExclusive, ins => ins.ConceptDefClusters, (ins, coll) => ins.ConceptDefClusters = coll,
                                                                             "Concept-Def Clusters", "Formal clusters for grouping Concept Definitions on palettes (i.e. visually).");

        /// <summary>
        /// Formal clusters for grouping Relationships.
        /// </summary>
        public EditableList<FormalPresentationElement> RelationshipDefClusters { get; protected set; }
        public static ModelListDefinitor<Domain, FormalPresentationElement> __RelationshipDefClusters =
                   new ModelListDefinitor<Domain, FormalPresentationElement>("RelationshipDefClusters", EEntityMembership.InternalCoreExclusive, ins => ins.RelationshipDefClusters, (ins, coll) => ins.RelationshipDefClusters = coll,
                                                                             "Relationship-Def Clusters", "Formal clusters for grouping Relationship Definitions on palettes (i.e. visually).");

        /// <summary>
        /// Languages defined for external file generation.
        /// </summary>
        public EditableList<ExternalLanguageDeclaration> ExternalLanguages { get; protected set; }
        public static ModelListDefinitor<Domain, ExternalLanguageDeclaration> __ExternalLanguages =
                   new ModelListDefinitor<Domain, ExternalLanguageDeclaration>("ExternalLanguages", EEntityMembership.InternalCoreExclusive, ins => ins.ExternalLanguages, (ins, coll) => ins.ExternalLanguages = coll,
                                                                               "External Languages", "Languages defined for external file generation.");

        /// <summary>
        /// External Language currently used to perform file generation and its configuration editing.
        /// </summary>
        public ExternalLanguageDeclaration CurrentExternalLanguage { get { return __CurrentExternalLanguage.Get(this); } set { __CurrentExternalLanguage.Set(this, value); } }
        protected ExternalLanguageDeclaration CurrentExternalLanguage_ = null;
        private static ModelPropertyDefinitor<Domain, ExternalLanguageDeclaration> __CurrentExternalLanguage =
                   new ModelPropertyDefinitor<Domain, ExternalLanguageDeclaration>("CurrentExternalLanguage", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common,
                                                                                   ins =>
                                                                                       {
                                                                                           if (ins.CurrentExternalLanguage_ == null || !ins.CurrentExternalLanguage_.IsIn(ins.ExternalLanguages))
                                                                                               ins.CurrentExternalLanguage_ = ins.ExternalLanguages.FirstOrDefault();

                                                                                           return ins.CurrentExternalLanguage_;
                                                                                       },
                                                                                   (ins, val) => ins.CurrentExternalLanguage_ = val, false, true,
                                                                                   "Current External Language", "External Language currently used to perform file generation and its configuration editing.");

        /// <summary>
        /// Idea owning the Domain's predefined base-content (such as Base Tables).
        /// </summary>
        public Concept BaseContentRoot { get { return __BaseContentRoot.Get(this); } private set { __BaseContentRoot.Set(this, value); } }
        protected Concept BaseContentRoot_ = null;
        private static ModelPropertyDefinitor<Domain, Concept> __BaseContentRoot =
                   new ModelPropertyDefinitor<Domain, Concept>("BaseContentRoot", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.BaseContentRoot_, (ins, val) => ins.BaseContentRoot_ = val, false, true,
                                                               "Base-Content Root", "Idea owning the Domain's predefined base-content (such as Base Tables).");

        /// <summary>
        /// Gets the predefined Tables of the Domain (e.g.: Countries, Currencies, Product Types, etc.)
        /// </summary>
        public IEnumerable<Table> BaseTables { get { return this.BaseContentRoot.Details.CastAs<Table, ContainedDetail>(); } }

        /// <summary>
        /// Stores configuration information about Report generation.
        /// </summary>
        public ReportConfiguration ReportingConfiguration { get { return __ReportingConfiguration.Get(this); } set { __ReportingConfiguration.Set(this, value); } }
        protected ReportConfiguration ReportingConfiguration_ = null;
        private static ModelPropertyDefinitor<Domain, ReportConfiguration> __ReportingConfiguration =
                   new ModelPropertyDefinitor<Domain, ReportConfiguration>("ReportingConfiguration", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.ReportingConfiguration_, (ins, val) => ins.ReportingConfiguration_ = val, false, true,
                                                                           "Reporting Configuration", "Stores configuration information about Report generation.");

        /// <summary>
        /// Stores configuration information about File Geneartion.
        /// </summary>
        public FileGenerationConfiguration GenerationConfiguration { get { return __GenerationConfiguration.Get(this); } set { __GenerationConfiguration.Set(this, value); } }
        protected FileGenerationConfiguration GenerationConfiguration_ = null;
        private static ModelPropertyDefinitor<Domain, FileGenerationConfiguration> __GenerationConfiguration =
                   new ModelPropertyDefinitor<Domain, FileGenerationConfiguration>("GenerationConfiguration", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common,
                                                                                   ins =>
                                                                                   {
                                                                                       if (ins.GenerationConfiguration_ == null)
                                                                                           ins.GenerationConfiguration_ = new FileGenerationConfiguration();

                                                                                       return ins.GenerationConfiguration_;
                                                                                   }, (ins, val) => ins.GenerationConfiguration_ = val, false, true,
                                                                                   "Generation Configuration", "Stores configuration information about File Geneartion.");

        /// <summary>
        /// Brush to be initially assigned to the View's background
        /// </summary>
        // IMPORTANT: This brush cannot come null to the final View, because hit-testing would not work if so (must be at least transparent, better white).
        public Brush ViewBackgroundBrush { get { return __ViewBackgroundBrush.Get(this); } set { __ViewBackgroundBrush.Set(this, value); } }
        protected StoreBox<Brush> ViewBackgroundBrush_ = ViewStandardBackground.Store<Brush>();
        public static readonly ModelPropertyDefinitor<Domain, Brush> __ViewBackgroundBrush =
                   new ModelPropertyDefinitor<Domain, Brush>("ViewBackgroundBrush", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common,
                                                                ins => ins.ViewBackgroundBrush_.NullDefault(ViewStandardBackground.Store<Brush>()),     // Supports null Brush from old Domains
                                                                (ins, stb) =>
                                                                {
                                                                    if (stb == null || stb.Value == null)
                                                                        stb = ViewStandardBackground.Store<Brush>();

                                                                    ins.ViewBackgroundBrush_ = stb;
                                                                }, false, false,
                                                             "View's default Background Brush", "Brush to be initially assigned to the View's background.");

        /// <summary>
        /// Image to be initially assigned to the View's background.
        /// </summary>
        public ImageSource ViewBackgroundImage { get { return __ViewBackgroundImage.Get(this); } set { __ViewBackgroundImage.Set(this, value); } }
        protected ImageAssignment ViewBackgroundImage_ = new ImageAssignment();
        public static readonly ModelPropertyDefinitor<Domain, ImageSource> __ViewBackgroundImage =
                           new ModelPropertyDefinitor<Domain, ImageSource>("ViewBackgroundImage", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.ViewBackgroundImage_.Image, (ins, val) => ins.ViewBackgroundImage_.Image = val, false, false,
                                                                           "View's default Background Image", "Image to be initially assigned to the View's background. " +
                                                                                                              "If bigger than " + View.BACKGROUND_IMAGE_MAX_TILE_SIZE + "x" + View.BACKGROUND_IMAGE_MAX_TILE_SIZE +
                                                                                                              " then it is adjusted to fit in the View, else it is repeated/tiled.");

        /// <summary>
        /// Size to be initially assigned to the View's grid.
        /// </summary>
        public double ViewGridSize { get { return __ViewGridSize.Get(this); } set { __ViewGridSize.Set(this, value); } }
        protected double ViewGridSize_ = View.GRID_SIZE_INI;
        public static readonly ModelPropertyDefinitor<Domain, double> __ViewGridSize =
                           new ModelPropertyDefinitor<Domain, double>("ViewGridSize", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.ViewGridSize_, (ins, val) => ins.ViewGridSize_ = val, false, false,
                                                                      "View Grid Size", "Size to be initially assigned to the View's grid.");

        public static readonly string TemplateForCompositionSummary = "Text template for file generation from Composition (to create root files, such as Table-Of-Contents, Configuration, 'Readme.txt', etc.).";

        /// <summary>
        /// Text templates used for file generation from Concepts.
        /// </summary>
        public EditableList<TextTemplate> OutputTemplatesForConcepts { get; protected set; }
        public static ModelListDefinitor<Domain, TextTemplate> __OutputTemplatesForConcepts =
                   new ModelListDefinitor<Domain, TextTemplate>("OutputTemplatesForConcepts", EEntityMembership.InternalCoreExclusive, ins => ins.OutputTemplatesForConcepts, (ins, coll) => ins.OutputTemplatesForConcepts = coll,
                                                                "Concepts base Output-Templates", "Text templates used for file generation from Concepts. Can be superseded or extended by Idea (Concept) Definition templates.");
        public TextTemplate CurrentTemplateForConcepts { get { return this.OutputTemplatesForConcepts.FirstOrDefault(tpl => tpl.Language == this.OwnerDomain.CurrentExternalLanguage); } }

        /// <summary>
        /// Text templates used for file generation from Relationships.
        /// </summary>
        public EditableList<TextTemplate> OutputTemplatesForRelationships { get; protected set; }
        public static ModelListDefinitor<Domain, TextTemplate> __OutputTemplatesForRelationships =
                   new ModelListDefinitor<Domain, TextTemplate>("OutputTemplatesForRelationships", EEntityMembership.InternalCoreExclusive, ins => ins.OutputTemplatesForRelationships, (ins, coll) => ins.OutputTemplatesForRelationships = coll,
                                                                "Relationships base Output-Templates", "Text templates used for file generation from Relationships. Can be superseded or extended by Idea (Relationship) Definition templates.");
        public TextTemplate CurrentTemplateForRelationships { get { return this.OutputTemplatesForRelationships.FirstOrDefault(tpl => tpl.Language == this.OwnerDomain.CurrentExternalLanguage); } }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /* POSTPONED: (Create a separate class for these, so they can be specified at Idea, Idea-Def Cluster or Domain level).
        /// <summary>
        /// Name given to Ideas in user's Domain.
        /// </summary>
        public string NomenclatureForIdeas { get { return __NomenclatureForIdeas.Get(this); } set { __NomenclatureForIdeas.Set(this, value); } }
        protected string NomenclatureForIdeas_ = null;
        private static ModelPropertyDefinitor<Domain, string> __NomenclatureForIdeas =
                   new ModelPropertyDefinitor<Domain, string>("NomenclatureForIdeas", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.NomenclatureForIdeas_, (ins, val) => ins.NomenclatureForIdeas_ = val, false, false,
                                                              "Ideas", "Name given to Ideas in user's Domain.");

        /// <summary>
        /// Name given to Concepts in user's Domain.
        /// </summary>
        public string NomenclatureForConcepts { get { return __NomenclatureForConcepts.Get(this); } set { __NomenclatureForConcepts.Set(this, value); } }
        protected string NomenclatureForConcepts_ = null;
        private static ModelPropertyDefinitor<Domain, string> __NomenclatureForConcepts =
                   new ModelPropertyDefinitor<Domain, string>("NomenclatureForConcepts", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.NomenclatureForConcepts_, (ins, val) => ins.NomenclatureForConcepts_ = val, false, false,
                                                              "Concepts", "Name given to Concepts in user's Domain.");

        /// <summary>
        /// Name given to Relationships in user's Domain.
        /// </summary>
        public string NomenclatureForRelationships { get { return __NomenclatureForRelationships.Get(this); } set { __NomenclatureForRelationships.Set(this, value); } }
        protected string NomenclatureForRelationships_ = null;
        private static ModelPropertyDefinitor<Domain, string> __NomenclatureForRelationships =
                   new ModelPropertyDefinitor<Domain, string>("NomenclatureForRelationships", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.NomenclatureForRelationships_, (ins, val) => ins.NomenclatureForRelationships_ = val, false, false,
                                                              "Relationships", "Name given to Relationships in user's Domain.");

        /// <summary>
        /// Name given to Links of Relationships in user's Domain.
        /// </summary>
        public string NomenclatureForLinks { get { return __NomenclatureForLinks.Get(this); } set { __NomenclatureForLinks.Set(this, value); } }
        protected string NomenclatureForLinks_ = null;
        private static ModelPropertyDefinitor<Domain, string> __NomenclatureForLinks =
                   new ModelPropertyDefinitor<Domain, string>("NomenclatureForLinks", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.NomenclatureForLinks_, (ins, val) => ins.NomenclatureForLinks_ = val, false, false,
                                                              "Links", "Name given to Links of Relationships in user's Domain.");

        /// <summary>
        /// Name given to Origins in user's Domain.
        /// </summary>
        public string NomenclatureForOrigins { get { return __NomenclatureForOrigins.Get(this); } set { __NomenclatureForOrigins.Set(this, value); } }
        protected string NomenclatureForOrigins_ = null;
        private static ModelPropertyDefinitor<Domain, string> __NomenclatureForOrigins =
                   new ModelPropertyDefinitor<Domain, string>("NomenclatureForOrigins", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.NomenclatureForOrigins_, (ins, val) => ins.NomenclatureForOrigins_ = val, false, false,
                                                              "Origins", "Name given to Origins in user's Domain.");

        /// <summary>
        /// Name given to Targets in user's Domain.
        /// </summary>
        public string NomenclatureForTargets { get { return __NomenclatureForTargets.Get(this); } set { __NomenclatureForTargets.Set(this, value); } }
        protected string NomenclatureForTargets_ = null;
        private static ModelPropertyDefinitor<Domain, string> __NomenclatureForTargets =
                   new ModelPropertyDefinitor<Domain, string>("NomenclatureForTargets", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.NomenclatureForTargets_, (ins, val) => ins.NomenclatureForTargets_ = val, false, false,
                                                              "Targets", "Name given to Targets in user's Domain.");

        /// <summary>
        /// Name given to Relationship Origins (i.e. 'Related From') in user's Domain.
        /// </summary>
        public string NomenclatureForRelationshipOrigins { get { return __NomenclatureForRelationshipOrigins.Get(this); } set { __NomenclatureForRelationshipOrigins.Set(this, value); } }
        protected string NomenclatureForRelationshipOrigins_ = null;
        private static ModelPropertyDefinitor<Domain, string> __NomenclatureForRelationshipOrigins =
                   new ModelPropertyDefinitor<Domain, string>("NomenclatureForRelationshipOrigins", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.NomenclatureForRelationshipOrigins_, (ins, val) => ins.NomenclatureForRelationshipOrigins_ = val, false, false,
                                                              "Relationship Origins", "Name given to Relationship Origins (i.e. 'Related From') in user's Domain.");

        /// <summary>
        /// Name given to Relationship Targets (i.e. 'Related To') in user's Domain.
        /// </summary>
        public string NomenclatureForRelationshipTargets { get { return __NomenclatureForRelationshipTargets.Get(this); } set { __NomenclatureForRelationshipTargets.Set(this, value); } }
        protected string NomenclatureForRelationshipTargets_ = null;
        private static ModelPropertyDefinitor<Domain, string> __NomenclatureForRelationshipTargets =
                   new ModelPropertyDefinitor<Domain, string>("NomenclatureForRelationshipTargets", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.NomenclatureForRelationshipTargets_, (ins, val) => ins.NomenclatureForRelationshipTargets_ = val, false, false,
                                                              "Relationship Targets", "Name given to Relationship Targets (i.e. 'Related To') in user's Domain.");


         * /// <summary>
        /// Name given to Markers in user's Domain.
        /// </summary>
        public string NomenclatureForMarkers { get { return __NomenclatureForMarkers.Get(this); } set { __NomenclatureForMarkers.Set(this, value); } }
        protected string NomenclatureForMarkers_ = null;
        private static ModelPropertyDefinitor<Domain, string> __NomenclatureForMarkers =
                   new ModelPropertyDefinitor<Domain, string>("NomenclatureForMarkers", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.NomenclatureForMarkers_, (ins, val) => ins.NomenclatureForMarkers_ = val, false, false,
                                                              "Markers", "Name given to Markers in user's Domain.");

                 /// <summary>
        /// Name given to Complements in user's Domain.
        /// </summary>
        public string NomenclatureForComplements { get { return __NomenclatureForComplements.Get(this); } set { __NomenclatureForComplements.Set(this, value); } }
        protected string NomenclatureForComplements_ = null;
        private static ModelPropertyDefinitor<Domain, string> __NomenclatureForComplements =
                   new ModelPropertyDefinitor<Domain, string>("NomenclatureForComplements", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.NomenclatureForComplements_, (ins, val) => ins.NomenclatureForComplements_ = val, false, false,
                                                              "Complements", "Name given to Complements in user's Domain.");
        */

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IPersistable Members

        public EExistenceStatus ExistenceStatus
        {
            get { return this.EditEngine.ExistenceStatus; }
        }

        #endregion

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IIdentificationScope Members

        public IdentificationController IdentificationScopeController { get { return this.IdentificationScopeController_; } protected set { this.IdentificationScopeController_ = value; } }
        [NonSerialized]
        private IdentificationController IdentificationScopeController_ = null;

        #endregion

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region ISphereModel Members

        public string Title { get { return this.Name + " <<" + Domain.__ClassDefinitor.Name + ">>" +
                                           (this.OwnerComposition.ExistenceStatus == EExistenceStatus.Modified ? General.STR_SIGNAL_MODIFICATION : ""); } }

        public string Overview { get { return this.Summary.AbsentDefault(this.Name); } }

        public ImageSource Icon { get { return this.Pictogram; } }

        public IDocumentView ActiveDocumentView { get { return null; } }

        public DocumentEngine DocumentEditEngine { get { return (DocumentEngine)this.OwnerComposition.EditEngine; } }

        public Uri Location
        {
            get
            {
                if (this.Location_ == null)
                    if (!Uri.TryCreate("/" + this.TechName + "." + DOCSUF_DOMAIN, UriKind.Relative, out Location_))
                        throw new UsageAnomaly("Cannot create Domain-Graph location from key.", this);

                return this.Location_;
            }
            set
            {
                this.Location_ = value;
            }
        }
        [NonSerialized]
        protected Uri Location_ = null;

        public string SimplifiedLocation
        {
            get
            {
                return (this.EditEngine == null || this.DocumentEditEngine.SimplifiedLocation.IsAbsent()
                        ? "[No Document (No Folder)]"
                        : this.DocumentEditEngine.SimplifiedLocation);
            }
        }

        public System.Collections.IEnumerable NavigableItems { get { return null; } }

        #endregion

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelClass<Domain> Members
        
        public new MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public new ModelClassDefinitor<Domain> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly new ModelClassDefinitor<Domain> __ClassDefinitor = null;

        public new Domain CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((Domain)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public Domain PopulateFrom(Domain SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public override void UpdateVersion()
        {
            base.UpdateVersion();

            if (this.OwnerComposition != null)
                this.OwnerComposition.UpdateVersion();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        private static void CreateApplicableGraphicStyles()
        {
            var ForegroundColors = BaseStylesForegroundColors.Concat(BaseStylesFinalColors).ToList();
            var BackgroundColors = BaseStylesFinalColors.Concat(BaseStylesForegroundColors).ToList();

            var Styles = GenerateGraphicStyles(ForegroundColors.Select(color => color.GetInterpolatedColor(Colors.White, 0.25)).ToList(),
                                               BackgroundColors.Select(color => color.GetInterpolatedColor(Colors.White, 0.25)).ToList());

            /*Too much controls...
            var Styles = GenerateGraphicStyles(ForegroundColors, BackgroundColors)
                            .Concat(GenerateGraphicStyles(ForegroundColors.Select(color => color.GetInterpolatedColor(Colors.White, 0.5)).ToList(),
                                          BackgroundColors.Select(color => color.GetInterpolatedColor(Colors.White, 0.5)).ToList()))
                            .Concat(GenerateGraphicStyles(ForegroundColors.Select(color => color.GetInterpolatedColor(Colors.Black, 0.3)).ToList(),
                                          BackgroundColors.Select(color => color.GetInterpolatedColor(Colors.Black, 0.3)).ToList()));*/
            
            ApplicableGraphicStyles = Styles.ToList();
        }

        private static IEnumerable<Tuple<Brush, double, DashStyle, Brush>> GenerateGraphicStyles(IList<Color> ForegroundColors, IList<Color> BackgroundColors)
        {
            var Colorings = new List<Tuple<Brush, Brush>>();

            ForegroundColors.ForEachIndexing(
                (color, index) => Colorings.Add(new Tuple<Brush, Brush>(new SolidColorBrush(color),
                                                                        Display.GetGradientBrush(BackgroundColors[index].GetInterpolatedColor(Colors.White, 0.85),
                                                                                                 BackgroundColors[index],
                                                                                                 true, 0.0, 1.0))), BackgroundColors.Count);

            ForegroundColors.ForEachIndexing(
                (color, index) => Colorings.Add(new Tuple<Brush, Brush>(new SolidColorBrush(color), new SolidColorBrush(BackgroundColors[index]))), BackgroundColors.Count);

            // This goes at end, so take the first foreground-colors-count * 2
            ForegroundColors.ForEachIndexing(
                (color, index) => Colorings.Add(new Tuple<Brush, Brush>(new SolidColorBrush(color),
                                                                        Brushes.White /* Do not use Transparent! */)),
                BackgroundColors.Count);

            var Dashes = new List<DashStyle>() { DashStyles.Solid, DashStyles.Dash /*- , DashStyles.Dot*/ };

            var Thicknesses = new List<double>() { 1.0, 2.0, 0.0 /*- , 0.5, 3.0*/ };

            var Styles = Thicknesses.SelectMany(
                                thick => Dashes.Take(thick == 0.0
                                                     ? 1 : Dashes.Count)
                                            .SelectMany(dash => Colorings.Take(ForegroundColors.Count * (thick == 0.0 ? 2 : 3)) // This skips the white-foreground on white-background case
                                                                    .Select(brs => new Tuple<Brush, double, DashStyle, Brush>(brs.Item1, thick, dash, brs.Item2))));

            return Styles;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}