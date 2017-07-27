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
// File   : View.cs
// Object : Instrumind.ThinkComposer.Model.VisualModel.View (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.08.24 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.ApplicationProduct;
using Instrumind.ThinkComposer.Model.GraphModel;
using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;

using Instrumind.ThinkComposer.Composer;
using Instrumind.ThinkComposer.Composer.ComposerUI;
using Instrumind.ThinkComposer.MetaModel;

/// Base abstractions for the visual representation of Graph entities
namespace Instrumind.ThinkComposer.Model.VisualModel
{
    /// <summary>
    /// Visual representation of the Composite-Content of an Idea or Composition.
    /// The final displaying of objects is made by a View Presenter.
    /// </summary>
    [Serializable]
    public class View : FormalElement, IDocumentView, IModelEntity, IModelClass<View>, ISelectable, IRecognizableElement, IVersionUpdater
    {
        public const double BORDER_TRESHOLD = 25.0;

        public const double VIEW_CONTENT_MARGIN_FACTOR = 0.1;

        public const double SNAPSHOT_MARGIN = 5.0;

        public const double PRINT_BORDER_THICKNESS = 0.5;

        public const double PRINT_CONTENT_MARGIN = 0;

        public const double EDITBOX_MIN_WIDTH = 50.0;
        public const double EDITBOX_MIN_HEIGHT = 20.0;

        public const double AUTOSIZE_MARGIN_ADJUST_FACTOR = 1.25;

        public const double GRID_SIZE_INI = 8.0;
        public const double GRID_SIZE_MIN = 2.0;
        public const double GRID_SIZE_MAX = 20.0;

        // Maximum size a background image can have to be repeated, else is expanded.
        public const double BACKGROUND_IMAGE_MAX_TILE_SIZE = 500;

        public static Brush PrintFramesBrush = Brushes.LightGray;

        public static Brush GridForeground = Brushes.SkyBlue;

        /// <summary>
        /// Millisecons to pause too-fast-for-user interactions.
        /// </summary>
        public const int INTERACTIVE_SLOW_DOWN = 150;

        /// <summary>
        /// Default pictogram of this class.
        /// </summary>
        public static readonly ImageSource DefaultPictogram = Display.GetAppImage("page_view.png");

        private static Pen SelectionRectanglePen = null;

        /// <summary>
        /// Static Constructor.
        /// </summary>
        static View()
        {
            __ClassDefinitor = new ModelClassDefinitor<View>("View", FormalElement.__ClassDefinitor, "View",
                                                             "Visual representation of the Composite-Content of an Idea or Composition.");
            __ClassDefinitor.DeclareProperty(__HostingScrollViewer);
            __ClassDefinitor.DeclareProperty(__PageDisplayScale);
            __ClassDefinitor.DeclareProperty(__OwnerCompositeContainer);
            __ClassDefinitor.DeclareProperty(__IsOpen);
            __ClassDefinitor.DeclareProperty(__IsOutlined);
            __ClassDefinitor.DeclareProperty(__ShowSmoothEdges);
            __ClassDefinitor.DeclareProperty(__BackgroundBrush);
            __ClassDefinitor.DeclareProperty(__BackgroundImage);
            __ClassDefinitor.DeclareProperty(__ShowContextBackground);
            __ClassDefinitor.DeclareProperty(__ShowContextGrid);
            __ClassDefinitor.DeclareProperty(__ShowIndicators);
            __ClassDefinitor.DeclareProperty(__ShowConceptDefinitionLabels);
            __ClassDefinitor.DeclareProperty(__ShowRelationshipDefinitionLabels);
            __ClassDefinitor.DeclareProperty(__ShowLinkRoleDefNameLabels);
            __ClassDefinitor.DeclareProperty(__ShowLinkRoleDescNameLabels);
            __ClassDefinitor.DeclareProperty(__ShowLinkRoleVariantLabels);
            __ClassDefinitor.DeclareProperty(__ShowMarkers);
            __ClassDefinitor.DeclareProperty(__ShowMarkersTitles);
            __ClassDefinitor.DeclareProperty(__GridSize);
            __ClassDefinitor.DeclareProperty(__GridUsesLines);
            __ClassDefinitor.DeclareProperty(__SnapToGrid);
            __ClassDefinitor.DeclareProperty(__ViewSize);
            __ClassDefinitor.DeclareProperty(__VisualLevelForBackground);
            __ClassDefinitor.DeclareProperty(__VisualLevelForRegions);
            __ClassDefinitor.DeclareProperty(__VisualCountOfFloatings);
            __ClassDefinitor.DeclareProperty(__AutoSizeByEnteredText);

            __ClassDefinitor.DeclareCollection(__ViewChildren);

            __GridSize.RangeMin = GRID_SIZE_MIN;
            __GridSize.RangeMax = GRID_SIZE_MAX;
            __GridSize.RangeStep = 1.0;

            SelectionRectanglePen = new Pen(Brushes.DimGray, 0.5);
            SelectionRectanglePen.DashStyle = Display.SegmentedLineStyle;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public View(Idea OwnerCompositeContainer, string Name, string TechName, string Summary = "", bool IsDescribed = false, bool IsClassified = false, bool IsVersioned = false)
             : base(Name, TechName, Summary, IsDescribed, IsClassified, IsVersioned)
        {
            this.ViewChildren = new EditableList<ViewChild>(__ViewChildren.TechName, this, 256);
            this.OwnerCompositeContainer = OwnerCompositeContainer;
            this.PageDisplayScale = ProductDirector.DefaultPageDisplayScale;

            this.ShowContextGrid = true;
            this.SnapToGrid = true;

            // Calculate size and center
            this.ViewSize = new Size(ProductDirector.DefaultNumberOfViewPages * ProductDirector.STANDARD_VIRTUAL_PAGE_WIDTH * (this.PageDisplayScale / 100.0),
                                     ProductDirector.DefaultNumberOfViewPages * ProductDirector.STANDARD_VIRTUAL_PAGE_HEIGHT * (this.PageDisplayScale / 100.0));

            this.BackgroundSheet = new VisualInert(this);
            // See Show(), where the background content is generated.

            this.ShowSmoothEdges = true;

            this.GridSize = OwnerCompositeContainer.OwnerComposition.CompositeContentDomain.ViewGridSize;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Protected Constructor for Agent descendants.
        /// </summary>
        protected View()
        {
        }

        /// <summary>
        /// Initializes the instance for use after creation or deserialization (AND CLONING BY DESERIALIZE+SERIALIZE).
        /// </summary>
        [OnDeserialized]
        private void Initialize(StreamingContext context = default(StreamingContext))
        {
            if (this.GridSize == 0)
                this.GridSize = GRID_SIZE_INI;

            if (this.BackgroundBrush_ == null || this.BackgroundImage_ == null)
            {
                this.BackgroundBrush_ = new StoreBox<Brush>();
                this.BackgroundImage_ = new ImageAssignment();
            }
        }

        /// <summary>
        /// Initializes the instance for use after creation or deserialization.
        /// </summary>
        public void Initialize()
        {
            // IMPORTANT: This initialization must be executed only after creation or deserialization.
            if (this.IsInitialized)
                return;

            this.Presenter = new ViewPresenter(this);
            this.TopCanvas = new Canvas();
            this.Manipulator = new ViewManipulationManager(this);

            if (!this.ShowSmoothEdges)
                RenderOptions.SetEdgeMode(this.Presenter, EdgeMode.Aliased);

            // Validate levels
            if (this.VisualCountOfFloatings > this.ViewChildren.Count
                || this.VisualLevelForBackground > this.VisualLevelForRegions
                || this.VisualLevelForRegions >= this.ViewChildren.Count
                || this.VisualLevelForBackground >= this.ViewChildren.Count)
                throw new InternalAnomaly("WARNING: Incorrect View visualization levels.");

            this.IsInitialized = true;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public CompositionEngine Engine { get { return (CompositionEngine)this.EditEngine;  } }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// The standard pictogram of all views.
        /// </summary>
        public ImageSource Pictogram { get { return DefaultPictogram; } }

        /// <summary>
        /// Indicates wether this View is selected for later applying a command.
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
        /// Exposes the View's context features such as grid, labels, legends, etc...
        /// </summary>
        public DrawingVisual CreateViewContext()
        {
            var Drawer = new DrawingVisual();

            using (DrawingContext Context = Drawer.RenderOpen())
            {
                /* Put a nice background
                var BackgroundImage = Display.GetAppImage("bg_paper.png");
                var ScreenBrush = new ImageBrush(BackgroundImage);
                ScreenBrush.Viewport = new Rect(0, 0, BackgroundImage.Width, BackgroundImage.Height);
                ScreenBrush.ViewportUnits = BrushMappingMode.Absolute;
                ScreenBrush.TileMode = TileMode.Tile;
                ScreenBrush.Stretch = Stretch.None;
                ScreenBrush.AlignmentX = AlignmentX.Left;
                ScreenBrush.AlignmentY = AlignmentY.Top; */

                // Nice as 3D background:
                // StandardBackground = Display.GetGradientBrush(Colors.Azure, Colors.PowderBlue, Colors.Azure);

                Context.DrawRectangle(Brushes.Transparent /* this.BackgroundWorkBrush / ScreenBrush */, new Pen(Brushes.DimGray, 1),
                                      new Rect(0, 0, this.ViewSize.Width, this.ViewSize.Height));

                // Context.DrawEllipse(this.ShowContextGrid ? Brushes.CadetBlue : Brushes.Bisque, new Pen(Brushes.Blue, 1.0), this.ViewCenter, 500, 500);

                if (this.ShowContextGrid)
                    DrawGrid(Context);

                // PENDING: Creates the initial View Label if stated.
            }

            return Drawer;
        }

        public void DrawGrid(DrawingContext Context)
        {
            var Area = new Rect(0, 0, this.GridSize, this.GridSize);
            var DrawGroup = new DrawingGroup();

            // IMPORTANT: Do not use a low opacity or a thickness less that 1.0 (it would be near invisible at 100% zoom display)

            // Just to make space
            DrawGroup.Children.Add(new GeometryDrawing(Brushes.Transparent, null, new RectangleGeometry(Area)));

            if (this.GridUsesLines)
            {
                var Pencil = new Pen(GridForeground, 1.0);
                Pencil.StartLineCap = PenLineCap.Square;
                Pencil.EndLineCap = PenLineCap.Square;
                Pencil.LineJoin = PenLineJoin.Miter;

                DrawGroup.Children.Add(new GeometryDrawing(null, Pencil,
                                                           new LineGeometry(new Point(), new Point(0, this.GridSize))));
                DrawGroup.Children.Add(new GeometryDrawing(null, Pencil,
                                                           new LineGeometry(new Point(), new Point(this.GridSize, 0))));
                DrawGroup.Opacity = 0.25;
            }
            else
            {
                DrawGroup.Children.Add(new GeometryDrawing(GridForeground, null,
                                                           new RectangleGeometry(new Rect(0, 0, 1.0, 1.0))));
                DrawGroup.Opacity = 0.5;    // must be higher to be noticed
            }

            var DrawBrush = new DrawingBrush(DrawGroup);
            DrawBrush.Viewport = Area;
            DrawBrush.ViewportUnits = BrushMappingMode.Absolute;
            DrawBrush.TileMode = TileMode.Tile;

            Context.DrawRectangle(DrawBrush, null, new Rect(this.ViewSize));
        }

        /* This is VERY-SLOW to show (like 30 secs. on an i7-950 3.2 at 3GHz with 8GB of RAM)
         * Should be: GRID_SIZE = 100.0;
        public void DrawComplexGrid(DrawingContext Context)
        {
            var ColBrush = new SolidColorBrush(Colors.Red);
            var RowBrush = new SolidColorBrush(Colors.Blue);
            ColBrush.Opacity = 0.25;
            RowBrush.Opacity = 0.25;

            var ColPen = new Pen(ColBrush, 0.5);
            var RowPen = new Pen(RowBrush, 0.5);

            // Draw columns
            double ColPos = 0;

            while (ColPos <= this.ViewSize.Width)
            {
                Context.DrawLine(ColPen, new Point(ColPos, 0), new Point(ColPos, this.ViewSize.Height));
                ColPos += GRID_SIZE;
            }

            // Draw rows
            double RowPos = 0;
            var TextTypeface = new Typeface("Arial");
            var Chronometer = System.Diagnostics.Stopwatch.StartNew();
            int Coordinates = 0;

            while (RowPos <= this.ViewSize.Height)
            {
                Context.DrawLine(RowPen, new Point(0, RowPos), new Point(this.ViewSize.Width, RowPos));
                RowPos += GRID_SIZE;

                // Draw coordinates text
                // THIS IS SLOW (approx 0.7 secs. on a i7-950 at 3.06 GHz)
                ColPos = 0;

                while (ColPos <= this.ViewSize.Width)
                {
                    Coordinates++;
                    var ColText = new FormattedText(ColPos.ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, TextTypeface, 8.0, ColBrush);
                    var RowText = new FormattedText(RowPos.ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, TextTypeface, 8.0, RowBrush);

                    Context.DrawText(ColText, new Point(ColPos - 19.0, RowPos - 9.0));
                    Context.DrawText(ColText, new Point(ColPos + 1.0, RowPos + 1.0));
                    Context.DrawText(RowText, new Point(ColPos + 1.0, RowPos - 9.0));
                    Context.DrawText(RowText, new Point(ColPos - 19.0, RowPos + 1.0));

                    ColPos += GRID_SIZE;
                }
            }

            Chronometer.Stop();
            Console.WriteLine("Grid drawing: Coordinates={0}, Time={1}", Coordinates, Chronometer.Elapsed);
        } */

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        // Important for avoiding crashes.
        public bool IsClosing { get { return this.IsClosing_; } protected set { this.IsClosing_ = value; } }
        [NonSerialized]
        private bool IsClosing_ = false;

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public VisualInert BackgroundSheet { get { return this.BackgroundSheet_; } set { this.BackgroundSheet_ = value; } }
        private VisualInert BackgroundSheet_ = null;

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public Grid PresenterHostingGrid { get { return this.PresenterHostingGrid_; } set { this.PresenterHostingGrid_ = value; } }
        [NonSerialized]
        private Grid PresenterHostingGrid_ = null;

        public Canvas TopCanvas { get { return this.TopCanvas_; } set { this.TopCanvas_ = value; } }
        [NonSerialized]
        private Canvas TopCanvas_ = null;

        public ViewManipulationManager Manipulator { get { return this.Manipulator_; } protected set { this.Manipulator_ = value; } }
        [NonSerialized]
        private ViewManipulationManager Manipulator_ = null;

        public bool IsInitialized { get { return this.IsInitialized_; } protected set { this.IsInitialized_ = value; } }
        [NonSerialized]
        private bool IsInitialized_ = false;

        [NonSerialized]
        public bool IsEditingActive = false;

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Renders the content (only visual-element objects) into the supplied context, optionally applying the specified transformation.
        /// Returns indication of content rendered.
        /// </summary>
        public bool GenerateRenderedContent(DrawingContext Context, Transform Transformation = null)
        {
            var VisualObjects = this.ViewChildren.Where(child => child.Key is VisualObject && !(child.Key is VisualInert))
                                .Select(child => (VisualObject)child.Key);

            if (!VisualObjects.Any())
                return false;

            var Result = new DrawingGroup();

            foreach (var VisObj in VisualObjects)
            {
                DrawingGroup PartDraw = null;

                if (VisObj is VisualElement)
                    PartDraw = ((VisualElement)VisObj).CreateDraw(null, false);
                else
                    if (VisObj is VisualComplement)
                        PartDraw = ((VisualComplement)VisObj).CreateDraw(false);

                Result.Children.Add(PartDraw);
            }

            Result.Transform = Transformation;
            Context.DrawDrawing(Result);

            return true;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets all the Complements not tied to a Symbol
        /// </summary>
        public IEnumerable<VisualComplement> GetFreeComplements()
        {
            var Complements = this.ViewChildren.Where(child => child.Key is VisualComplement)
                                                .Select(child => (VisualComplement)child.Key);
            foreach (var Complement in Complements)
                if (Complement.Target.IsGlobal)
                    yield return Complement;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Reacts to a view deactivation (as when focusing another one).
        /// </summary>
        public void ReactToDeactivation()
        {
            // IMPORTANT: Prevents crashes when undoing object creations and the selection adorner remains.
            this.UnselectAllObjects();

            if (this.OwnerCompositeContainer == null)
                return;

            this.Engine.StartCommandVariation("Apply changes to parent Composite-Container.");

            this.OwnerCompositeContainer.UpdateVisualRepresentators();

            this.Engine.CompleteCommandVariation();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the background image of the view, if any.
        /// </summary>
        public Brush GetBackgroundImageBrush()
        {
            if (this.BackgroundWorkingImage == null)
                return null;

            var Result = new ImageBrush(this.BackgroundWorkingImage);

            if (this.BackgroundWorkingImage.GetWidth() <= BACKGROUND_IMAGE_MAX_TILE_SIZE
                && this.BackgroundWorkingImage.GetHeight() <= BACKGROUND_IMAGE_MAX_TILE_SIZE)
            {
                Result.TileMode = TileMode.Tile;
                Result.ViewportUnits = BrushMappingMode.Absolute;
                Result.Viewport = new Rect(0, 0, this.BackgroundWorkingImage.GetWidth(), this.BackgroundWorkingImage.GetHeight());
                Result.Stretch = Stretch.None;
            }
            else
                Result.Stretch = Stretch.UniformToFill;

            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Shows all the View children.
        /// </summary>
        public void ShowAll()
        {
            this.HostingScrollViewer.Background = this.GetBackgroundImageBrush();

            // IMPORTANT: This cannot be null, because hit-testing does not work if no brush is present (must be at least transparent, better white).
            this.PresenterHostingGrid.Background = this.BackgroundWorkingBrush;   //T Display.GetGradientBrush(Colors.Azure, Colors.DodgerBlue, Colors.LightGreen)

            this.BackgroundSheet.Graphic = CreateViewContext();

            // Must be the first and remain at index zero.
            this.Presenter.PutVisual(this.BackgroundSheet, EVisualLevel.Background);

            var Depictions = this.ViewChildren.Where(child => child.Key is VisualObject && ((VisualObject)child.Key != this.BackgroundSheet))
                                    .OrderBy(child => !(child.Key is VisualSymbol)).ToArray();

            //H var Orphans = new List<ViewChild>();

            foreach (var Depiction in Depictions)
                try
                {
                    Show((VisualObject)Depiction.Key);
                }
                catch (Exception Problem)
                {
                    Console.WriteLine("Cannot show visual-object '{0}'. Problem: {1}", Depiction.Key.ToStringAlways(), Problem);
                    //H Orphans.Add(Depiction);
                }

            /*H
            foreach (var Orphan in Orphans)
            {
                Console.WriteLine("Removing orphan depiction '{0}'.", Orphan);
                this.ViewChildren.Remove(Orphan);
            } */

            this.Presenter.PostCall(vpres => vpres.OwnerView.ApplyDisplayScale());
            ProductDirector.UpdateMenuToolbar();
        }

        /// <summary>
        /// Shows on this view the supplied visual representator, either adding or updating it.
        /// </summary>
        public void ShowRepresentator(VisualRepresentation Representator, Nullable<EVisualRepresentationPart> PartType)
        {
            if (PartType == null)
                foreach (var Part in Representator.VisualParts.OrderBy(part => part.RepresentationPartType != EVisualRepresentationPart.ConceptBodySymbol))
                    Show(Part);
            else
                foreach (var Part in Representator.VisualParts.Where(part => part.RepresentationPartType != PartType))
                    Show(Part);
        }

        /// <summary>
        /// Puts the specified Complement in this view at the appropriate level.
        /// </summary>
        public void PutComplement(VisualComplement Complement)
        {
            // NOTE: Previously Info-Cards and Legends were put on Background, but they needed to be sometimes over regions.
            //       So, currently there is no Background related complements.
            this.Presenter.PutVisual(Complement, (Complement.Kind.TechName.IsOneOf(Domain.ComplementDefCallout.TechName,
                                                                                   Domain.ComplementDefQuote.TechName,
                                                                                   Domain.ComplementDefNote.TechName,
                                                                                   Domain.ComplementDefStamp.TechName)
                                                           ? EVisualLevel.Floatings
                                                           : EVisualLevel.Regions));
        }

        /// <summary>
        /// Request showing on this view the supplied visual object, either adding or updating it.
        /// </summary>
        public void Show(VisualObject Depiction)
        {
            if (Depiction == null || this.Presenter == null)
                return;

            if (Depiction.IsRelatedVisible)
                this.Presenter.PutVisual(Depiction);
            else
                this.Presenter.ClearVisual(Depiction);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Clears the specified visual parts from the View.
        /// If nothing (null) is specified, then all the View children are cleared.
        /// </summary>
        public void Clear(IEnumerable<VisualObject> Parts)
        {
            if (Parts == null)
            {
                var TargetRepresentations = this.ViewChildren.Where(child => child.Key is VisualObject)
                                                    .Select(child => (VisualObject)child.Key).ToList();
                foreach (var Representation in TargetRepresentations)
                    Clear(Representation);

                return;
            }

            foreach (var Part in Parts)
                Clear(Part);
        }

        /// <summary>
        /// Clears the supplied visual object from the view.
        /// </summary>
        public void Clear(VisualObject Depiction)
        {
            if (Depiction == null || this.Presenter == null)
                return;

            this.Presenter.ClearVisual(Depiction);
        }

        /// <summary>
        /// Returns all the visual objects found inside the specified Perimeter.
        /// Optionally, an indication of considering only body based objects (such as symbols, but not connectors),
        /// and an indication of partial containment (of at least a whole flank, vertical or horizontal), can be specified.
        /// </summary>
        public IEnumerable<VisualObject> GetVisualObjectsInside(Rect Perimeter, bool ConsiderOnlyBodyBasedObjects = true, bool RequireFullContainment = true)
        {
            foreach (var Child in this.ViewChildren)
            {
                var VisObj = Child.Key as VisualObject;

                if (VisObj == null || VisObj is VisualInert
                    || ConsiderOnlyBodyBasedObjects && VisObj is VisualConnector)
                    continue;

                /* CANCELLED: Because also consider Regions beyond a targeted one.
                // Parameter RequireFullContainment: Indication of full containment within the perimeter
                //                                   (else just the center is needed to be inside).
                var EvalZone = (RequireFullContainment
                                ? VisObj.TotalArea
                                : new Rect(VisObj.BaseCenter.X - 0.5, VisObj.BaseCenter.Y - 0.5, 1.0, 1.0)); */

                if ((RequireFullContainment && (VisObj.TotalArea.Left >= Perimeter.Left && VisObj.TotalArea.Right <= Perimeter.Right
                                                && VisObj.TotalArea.Top >= Perimeter.Top && VisObj.TotalArea.Bottom <= Perimeter.Bottom))
                     || (!RequireFullContainment && ((VisObj.TotalArea.Left <= Perimeter.Right && VisObj.TotalArea.Right >= Perimeter.Left)
                                                     && (VisObj.TotalArea.Top <= Perimeter.Bottom && VisObj.TotalArea.Bottom >= Perimeter.Top))
                                                 && !(VisObj.TotalArea.Left < Perimeter.Left && VisObj.TotalArea.Right > Perimeter.Right // Exclude a region container
                                                      && VisObj.TotalArea.Top < Perimeter.Top && VisObj.TotalArea.Right > Perimeter.Right)))
                    yield return VisObj;
            }
        }

        /// <summary>
        /// Gets the appropriate presentation level of the specified Depiction based on its type.
        /// </summary>
        public EVisualLevel? GetLevelOf(object Depiction)
        {
            var Index = this.ViewChildren.IndexOfMatch(reg => reg.Key.IsEqual(Depiction));
            if (Index < 0)
                return null;

            if (Index <= this.VisualLevelForBackground)
                return EVisualLevel.Background;

            if (Index <= this.VisualLevelForRegions)
                return EVisualLevel.Regions;

            if (Index < this.ViewChildren.Count - (this.VisualCountOfFloatings + this.Presenter.TransientObjectsCount))
                return EVisualLevel.Elements;

            if (Index < this.ViewChildren.Count - this.Presenter.TransientObjectsCount)
                return EVisualLevel.Floatings;

            return EVisualLevel.Transients;
        }

        /// <summary>
        /// Gets the z-order depth index of th specified Depiction.
        /// </summary>
        public int GetZOrderOf(object Depiction)
        {
            var Result = this.ViewChildren.IndexOfMatch(child => child.Key == Depiction);
            return Result;
        }

        /// <summary>
        /// Sends the supplied Target forwards, optionally putting it at the top.
        /// </summary>
        public void SendUpwards(VisualObject Target, bool PutOnTop = false)
        {
            var CurrentIndex = this.ViewChildren.IndexOfMatch(reg => reg.Key.IsEqual(Target));

            // Notice that the item at index zero (the background sheet) is omitted
            if (CurrentIndex <= 0)
                return;

            var Limit = (CurrentIndex <= this.VisualLevelForBackground
                         ? this.VisualLevelForBackground
                         : (CurrentIndex <= this.VisualLevelForRegions
                            ? this.VisualLevelForRegions
                            : (CurrentIndex <= (this.ViewChildren.Count - (this.VisualCountOfFloatings + this.Presenter.TransientObjectsCount)) - 1
                               ? (this.ViewChildren.Count - (this.VisualCountOfFloatings + this.Presenter.TransientObjectsCount)) - 1
                               : (CurrentIndex <= (this.ViewChildren.Count - this.Presenter.TransientObjectsCount) - 1
                                  ? (this.ViewChildren.Count - this.Presenter.TransientObjectsCount) - 1
                                  : this.ViewChildren.Count - 1))));

            if (CurrentIndex >= Limit)
                return;

            var LocalCommand = !this.EditEngine.IsVariating;
            if (LocalCommand)
                this.EditEngine.StartCommandVariation("Bring To-Front/Upwards");

            var TempSended = this.ViewChildren[CurrentIndex];
            this.ViewChildren[CurrentIndex] = null;

            if (PutOnTop)
            {
                for (int MovingIndex = CurrentIndex; MovingIndex < Limit; MovingIndex++)
                {
                    var TempShifted = this.ViewChildren[MovingIndex + 1];
                    this.ViewChildren[MovingIndex + 1] = null;
                    this.ViewChildren[MovingIndex] = ViewChild.Create(TempShifted);
                }

                this.ViewChildren[Limit] = ViewChild.Create(TempSended);
            }
            else
            {
                var TempShifted = this.ViewChildren[CurrentIndex + 1];
                this.ViewChildren[CurrentIndex + 1] = TempSended;
                this.ViewChildren[CurrentIndex] = ViewChild.Create(TempShifted);
            }

            if (LocalCommand)
            {
                this.UpdateVersion();
                this.EditEngine.CompleteCommandVariation();
            }
        }

        /// <summary>
        /// Sends the supplied Target backwards, optionally putting it at the bottom.
        /// </summary>
        public void SendBackwards(VisualObject Target, bool PutOnBottom = false)
        {
            var CurrentIndex = this.ViewChildren.IndexOfMatch(reg => reg.Key.IsEqual(Target));
            if (CurrentIndex < 0)
                return;

            var Limit = (CurrentIndex > (this.ViewChildren.Count - this.Presenter.TransientObjectsCount) - 1
                         ? (this.ViewChildren.Count - this.Presenter.TransientObjectsCount) - 1
                         : (CurrentIndex > (this.ViewChildren.Count - (this.VisualCountOfFloatings + this.Presenter.TransientObjectsCount)) - 1
                            ? (this.ViewChildren.Count - (this.VisualCountOfFloatings + this.Presenter.TransientObjectsCount)) - 1
                            : (CurrentIndex > this.VisualLevelForRegions
                               ? this.VisualLevelForRegions + 1
                               : (CurrentIndex > this.VisualLevelForBackground
                                  ? this.VisualLevelForBackground + 1
                                  : 1))));  // Do not allow to send backwards the Background sheet

            if (CurrentIndex <= Limit)
                return;

            var LocalCommand = !this.EditEngine.IsVariating;
            if (LocalCommand)
                this.EditEngine.StartCommandVariation("Send To-Back/Backwards");

            var TempSended = this.ViewChildren[CurrentIndex];
            this.ViewChildren[CurrentIndex] = null;

            if (PutOnBottom)
            {
                for (int MovingIndex = CurrentIndex; MovingIndex > Limit; MovingIndex--)
                {
                    var TempShifted = this.ViewChildren[MovingIndex - 1];
                    this.ViewChildren[MovingIndex - 1] = null;
                    this.ViewChildren[MovingIndex] = ViewChild.Create(TempShifted);
                }

                this.ViewChildren[Limit] = ViewChild.Create(TempSended);
            }
            else
            {
                var TempShifted = this.ViewChildren[CurrentIndex - 1];
                this.ViewChildren[CurrentIndex - 1] = TempSended;
                this.ViewChildren[CurrentIndex] = ViewChild.Create(TempShifted);
            }

            if (LocalCommand)
            {
                this.UpdateVersion();
                this.EditEngine.CompleteCommandVariation();
            }
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Registers for the specified Target, the supplied Drawings to be shown later as adorner.
        /// </summary>
        public void AttachAdorner(VisualObject Target, IEnumerable<Drawing> Drawings)
        {
            if (Target == null || Drawings == null || !Drawings.Any())
                return;

            var Adorners = new List<Adorner>();
            Drawing AdornerDrawing = null;

            if (Drawings.Count() == 1)
                AdornerDrawing = Drawings.First();
            else
            {
                var Group = new DrawingGroup();
                foreach (var Drawing in Drawings)
                    Group.Children.Add(Drawing);
                AdornerDrawing = Group;
            }

            var Adorner = new SimpleAdorner(this.Presenter, AdornerDrawing);
            Adorner.MouseMove += ((sender, evargs) =>
                                    {
                                        if (!this.Manipulator.IsManipulating
                                            && this.Engine.RunningMouseCommand == null)
                                            if (!this.Manipulator.PointObject(Target))
                                            {
                                                ProductDirector.ShowPointingTo();
                                                ProductDirector.ShowAssistance();
                                            }
                                    });

            this.RegisteredAdorners.AddOrReplace(Target, Adorner);
        }

        /// <summary>
        /// Unregisters for the specified Target, an associated Adorner and removes it from presentation.
        /// </summary>
        public void DetachAdorner(VisualObject Target)
        {
            this.RegisteredAdorners.Remove(Target);
        }

        /// <summary>
        /// Stores, per visual object, an associated visual adorner.
        /// </summary>
        public Dictionary<VisualObject, Adorner> RegisteredAdorners
        {
            get
            {
                if (this.RegisteredAdorners_ == null)
                    this.RegisteredAdorners_ = new Dictionary<VisualObject, Adorner>();

                return this.RegisteredAdorners_;
            }
        }
        [NonSerialized]
        public Dictionary<VisualObject, Adorner> RegisteredAdorners_ = null;

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// References the control that contains the document view.
        /// </summary>
        // IMPORTANT: This is defined as Entity Property just to detect View creation undo,
        //            therefore the container (usually a Tab or child Window) can be removed in reaction to the property change back to null.
        public ScrollViewer HostingScrollViewer
        {
            get { return __HostingScrollViewer.Get(this); }
            set { __HostingScrollViewer.Set(this, value); }
        }
        [NonSerialized]
        protected ScrollViewer HostingScrollViewer_ = null;
        public static readonly ModelPropertyDefinitor<View, ScrollViewer> __HostingScrollViewer =
                   new ModelPropertyDefinitor<View, ScrollViewer>("HostingScrollViewer", EEntityMembership.External, null, EPropertyKind.Common, ins => ins.HostingScrollViewer_, (ins, val) => ins.HostingScrollViewer_ = val, false, true,
                                                                  "Hosting Scroll-Viewer", "References the control that contains the document view.");

        /// <summary>
        /// Presenter for the View.
        /// </summary>
        public ViewPresenter Presenter
        {
            get
            {
                return this.Presenter_;
            }
            protected set
            {
                this.Presenter_ = value;
                UpdateEdgeMode(this.ShowSmoothEdges);
            }
        }
        [NonSerialized]
        private ViewPresenter Presenter_ = null;

        /// <summary>
        /// Layers exposed by this View.
        /// </summary>
        // public List<VisualLayer> Layers;

        /// <summary>
        /// Visual objects presented by this View.
        /// </summary>
        public EditableList<ViewChild> ViewChildren { get; protected set; }
        public static ModelListDefinitor<View, ViewChild> __ViewChildren =
                   new ModelListDefinitor<View, ViewChild>("ViewChildren", EEntityMembership.InternalCoreExclusive, ins => ins.ViewChildren, (ins, coll) => ins.ViewChildren = coll,
                                                           "View Children", "Visual objects presented by this View.");

        /// <summary>
        /// Collection of Visual Groups.
        /// </summary>
        //? public readonly List<VisualGroup> Groups = new List<VisualGroup>();

        /// <summary>
        /// Background visual renderer assigned.
        /// </summary>
        public Visual ContextBackgroundPresenter { get { return this.ContextBackgroundPresenter_; } protected set { this.ContextBackgroundPresenter_ = value; } }
        [NonSerialized]
        private Visual ContextBackgroundPresenter_ = null;

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Action to execute after this document view PageDisplayScale has changed.
        /// </summary>
        public Action<int> PostChangeOfPageDisplalyScale { get { return this.PostChangeOfPageDisplalyScale_; } set { this.PostChangeOfPageDisplalyScale_ = value; } }
        [NonSerialized]
        private Action<int> PostChangeOfPageDisplalyScale_ = null;

        /// <summary>
        /// Fits the content to the view presenter.
        /// </summary>
        public void FitContentIntoView()
        {
            var UsedArea = DetermineContentArea();

            if (UsedArea == Rect.Empty)
                return;

            this.Engine.StartCommandVariation("Fit to View");

            UsedArea = new Rect(UsedArea.Left - UsedArea.Width * (VIEW_CONTENT_MARGIN_FACTOR / 2.0),
                                UsedArea.Top - UsedArea.Height * (VIEW_CONTENT_MARGIN_FACTOR / 2.0),
                                UsedArea.Width * (1.0 + VIEW_CONTENT_MARGIN_FACTOR),
                                UsedArea.Height * (1.0 + VIEW_CONTENT_MARGIN_FACTOR));

            var CurrentScaleX = this.HostingScrollViewer.ViewportWidth / UsedArea.Width;
            var CurrentScaleY = this.HostingScrollViewer.ViewportHeight / UsedArea.Height;
            var NewScale = Math.Min(CurrentScaleX, CurrentScaleY);

            this.PageDisplayScale = Convert.ToInt32(NewScale * 100);

            var NewOffsetX = ((UsedArea.X * NewScale) + ((UsedArea.Width * NewScale) / 2.0))
                              - (this.HostingScrollViewer.ViewportWidth / 2.0);
            var NewOffsetY = ((UsedArea.Y * NewScale) + ((UsedArea.Height * NewScale) / 2.0))
                              - (this.HostingScrollViewer.ViewportHeight / 2.0);

            this.HostingScrollViewer.ScrollToHorizontalOffset(NewOffsetX);
            this.HostingScrollViewer.ScrollToVerticalOffset(NewOffsetY);

            this.Engine.CompleteCommandVariation();
        }

        public void ApplyDisplayScale(bool PointToTarget = false)
        {
            var InitialZoomTarget = (this.HostingScrollViewer.IsMouseOver
                                     ? Mouse.GetPosition(this.HostingScrollViewer)
                                     : new Point(this.HostingScrollViewer.ViewportWidth / 2.0,
                                                 this.HostingScrollViewer.ViewportHeight / 2.0));

            var ZoomTargetPrev = this.HostingScrollViewer.TranslatePoint(InitialZoomTarget, this.Presenter);

            var ScaleX = this.PageDisplayScale / 100.0;
            var ScaleY = this.PageDisplayScale / 100.0;

            var Zoom = new ScaleTransform(ScaleX, ScaleY);

            /* POSTPONED: Some day this should show a trapezoid (although only possible in WPF on 3D)
            var Skew = new SkewTransform(-10, -10); // Do not use more to avoid "jumps".
            var Transformations = new TransformGroup();
            Transformations.Children.Add(Zoom);
            Transformations.Children.Add(Skew);

            this.Presenter.LayoutTransform = Transformations; */

            this.Presenter.LayoutTransform = Zoom;
            this.Presenter.UpdateLayout();

            if (PointToTarget)
            {
                var ZoomTargetPost = this.HostingScrollViewer.TranslatePoint(InitialZoomTarget, this.Presenter);

                var DeltaX = ZoomTargetPost.X - ZoomTargetPrev.X;
                var DeltaY = ZoomTargetPost.Y - ZoomTargetPrev.Y;

                var MultiX = this.HostingScrollViewer.ExtentWidth / this.Presenter.ActualWidth.NaNDefault(1.0).EnforceMinimum(1.0);
                var MultiY = this.HostingScrollViewer.ExtentHeight / this.Presenter.ActualHeight.NaNDefault(1.0).EnforceMinimum(1.0);

                var OffsetX = this.HostingScrollViewer.HorizontalOffset - DeltaX * MultiX;
                var OffsetY = this.HostingScrollViewer.VerticalOffset - DeltaY * MultiY;

                this.HostingScrollViewer.ScrollToHorizontalOffset(OffsetX);
                this.HostingScrollViewer.ScrollToVerticalOffset(OffsetY);
            }

            if (PostChangeOfPageDisplalyScale != null
                && ProductDirector.WorkspaceDirector.ActiveDocument != null
                && ProductDirector.WorkspaceDirector.ActiveDocument.ActiveDocumentView == this)
                PostChangeOfPageDisplalyScale(this.PageDisplayScale);
        }

        /// <summary>
        /// Scaling percentage for the View page.
        /// </summary>
        public int PageDisplayScale
        {
            get { return __PageDisplayScale.Get(this); }
            set
            {
                if (value < (int)ProductDirector.MIN_PAGE_SCALE)
                    value = (int)ProductDirector.MIN_PAGE_SCALE;

                if (value > (int)ProductDirector.MAX_PAGE_SCALE)
                    value = (int)ProductDirector.MAX_PAGE_SCALE;

                if (value == this.PageDisplayScale_)
                    return;

                this.PageDisplayScale_ = value;
                this.ApplyDisplayScale(true);
            }
        }
        protected int PageDisplayScale_ = 100;
        public static readonly ModelPropertyDefinitor<View, int> __PageDisplayScale =
                   new ModelPropertyDefinitor<View, int>("PageDisplayScale", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.PageDisplayScale_, (ins, val) => ins.PageDisplayScale_ = val, false, true,
                                                         "Page Display Scale", "Scaling percentage for displaying the view page.");

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        [field: NonSerialized]
        public event MouseWheelEventHandler MouseWheel;

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region ICompositeDepiction Members

        public Visual Visualizer { get { return this.Presenter; } }

        /// <summary>
        /// References the Idea Composite Container owning this View.
        /// </summary>
        public Idea OwnerCompositeContainer { get { return __OwnerCompositeContainer.Get(this); } set { __OwnerCompositeContainer.Set(this, value); } }
        protected Idea OwnerCompositeContainer_;
        public static readonly ModelPropertyDefinitor<View, Idea> __OwnerCompositeContainer =
                   new ModelPropertyDefinitor<View, Idea>("OwnerCompositeContainer", EEntityMembership.External, true, EPropertyKind.Common, ins => ins.OwnerCompositeContainer_, (ins, val) => ins.OwnerCompositeContainer_ = val, false, true,
                                                          "Owner Composite Container", "References the Idea Composite Container owning this View.");

        /// <summary>
        /// References the visualized composite owner Idea.
        /// </summary>
        public Idea VisualizedCompositeIdea { get { return this.OwnerCompositeContainer; } }

        /// <summary>
        /// Indicates whether the content is presented/expanded or hidden/collapsed.
        /// </summary>
        public bool IsOpen { get { return __IsOpen.Get(this); } set { __IsOpen.Set(this, value); } }
        protected bool IsOpen_;
        public static readonly ModelPropertyDefinitor<View, bool> __IsOpen =
                   new ModelPropertyDefinitor<View, bool>("IsOpen", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.IsOpen_, (ins, val) => ins.IsOpen_ = val, false, true,
                                                          "Is Open", "Indicates whether the content is presented/expanded or hidden/collapsed.");

        /// <summary>
        /// Indicates whether the content is presented surrounded by a border.
        /// </summary>
        public bool IsOutlined { get { return __IsOutlined.Get(this); } set { __IsOutlined.Set(this, value); } }
        protected bool IsOutlined_;
        public static readonly ModelPropertyDefinitor<View, bool> __IsOutlined =
                   new ModelPropertyDefinitor<View, bool>("IsOutlined", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.IsOutlined_, (ins, val) => ins.IsOutlined_ = val, false, true,
                                                          "Is Outlined", "Indicates whether the content is presented surrounded by a border.");

        /// <summary>
        /// Indicates whether to show smoth edges for the displayed shapes, else they are displayed sharpened.
        /// </summary>
        public bool ShowSmoothEdges
        {
            get
            {
                return __ShowSmoothEdges.Get(this);
            }
            set
            {
                if (__ShowSmoothEdges.Set(this, value))
                    UpdateEdgeMode(value);
            }
        }
        protected bool ShowSmoothEdges_ = true;
        public static readonly ModelPropertyDefinitor<View, bool> __ShowSmoothEdges =
                   new ModelPropertyDefinitor<View, bool>("ShowSmoothEdges", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.ShowSmoothEdges_, (ins, val) => ins.ShowSmoothEdges_ = val, false, true,
                                                          "Show Smooth Edges", "Indicates whether to show smoth edges for the displayed shapes, else they are displayed sharpened.");

        #endregion

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        private void UpdateEdgeMode(bool ShowSmooth)
        {
            if (this.Presenter == null)
                return;

            RenderOptions.SetEdgeMode(this.Presenter, ShowSmooth ? EdgeMode.Unspecified : EdgeMode.Aliased);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IDocumentView Members

        public ISphereModel ParentDocument { get { return this.VisualizedCompositeIdea.RelatedDocument; } }

        public string Title { get { return this.Name; } }

        public FrameworkElement PresenterControl { get { return this.Presenter; } }

        public AdornerLayer PresenterLayer { get { return AdornerLayer.GetAdornerLayer(this.Presenter); } }

        /// <summary>
        /// Brush to be applied in the Background (behind the diagram sheet).
        /// </summary>
        public Brush BackgroundBrush { get { return __BackgroundBrush.Get(this); } set { __BackgroundBrush.Set(this, value); } }
        protected StoreBox<Brush> BackgroundBrush_ = new StoreBox<Brush>();
        public static readonly ModelPropertyDefinitor<View, Brush> __BackgroundBrush =
                   new ModelPropertyDefinitor<View, Brush>("BackgroundBrush", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.BackgroundBrush_, (ins, stb) => ins.BackgroundBrush_ = stb, false, false,
                                                           "Background Brush", "Brush to be applied in the Background (behind the diagram sheet)");
        // IMPORTANT: This cannot return null, because hit-testing does not work if no brush is present (must be at least transparent, better white).
        public Brush BackgroundWorkingBrush { get { return this.BackgroundBrush.NullDefault(this.OwnerCompositeContainer.CompositeContentDomain.ViewBackgroundBrush); } }

        /// <summary>
        /// Image to be shown at the Background (behind the diagram sheet, over the background color)
        /// </summary>
        public ImageSource BackgroundImage { get { return __BackgroundImage.Get(this); } set { __BackgroundImage.Set(this, value); } }
        protected ImageAssignment BackgroundImage_ = new ImageAssignment();
        public static readonly ModelPropertyDefinitor<View, ImageSource> __BackgroundImage =
                   new ModelPropertyDefinitor<View, ImageSource>("BackgroundImage", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.BackgroundImage_.Image, (ins, val) => ins.BackgroundImage_.Image = val, false, false,
                                                                 "Background Image", "Image to be shown at the Background (behind the diagram sheet, over the background color). "+
                                                                                     "If bigger than " + View.BACKGROUND_IMAGE_MAX_TILE_SIZE + "x" + View.BACKGROUND_IMAGE_MAX_TILE_SIZE +
                                                                                     " then it is adjusted to fit in the View, else it is repeated/tiled.");
        public ImageSource BackgroundWorkingImage { get { return this.BackgroundImage.NullDefault(this.OwnerCompositeContainer.CompositeContentDomain.ViewBackgroundImage); } }

        /// <summary>
        /// Indicates whether to display the assigned Background.
        /// </summary>
        public bool ShowContextBackground { get { return __ShowContextBackground.Get(this); } set { __ShowContextBackground.Set(this, value); } }
        protected bool ShowContextBackground_;
        public static readonly ModelPropertyDefinitor<View, bool> __ShowContextBackground =
                   new ModelPropertyDefinitor<View, bool>("ShowContextBackground", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.ShowContextBackground_, (ins, val) => ins.ShowContextBackground_ = val, false, true,
                                                          "Show Context Background", "Indicates whether to display the assigned Background.");
        
        /// <summary>
        /// Indicates whether to display the assigned Grid.
        /// </summary>
        public bool ShowContextGrid { get { return __ShowContextGrid.Get(this); } set { __ShowContextGrid.Set(this, value); } }
        protected bool ShowContextGrid_ = false;
        // NOTE: This property is marked with IsDisplayed=false until support for grid show on/off is provided.
        public static readonly ModelPropertyDefinitor<View, bool> __ShowContextGrid =
                   new ModelPropertyDefinitor<View, bool>("ShowContextGrid", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.ShowContextGrid_, (ins, val) => ins.ShowContextGrid_ = val, false, true,
                                                          "Show Context Grid", "Indicates whether to display the assigned Grid.");

        /// <summary>
        /// Indicates whether to display Indicators over the Ideas.
        /// </summary>
        public bool ShowIndicators { get { return __ShowIndicators.Get(this); } set { __ShowIndicators.Set(this, value); } }
        protected bool ShowIndicators_ = true;
        public static readonly ModelPropertyDefinitor<View, bool> __ShowIndicators =
                   new ModelPropertyDefinitor<View, bool>("ShowIndicators", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.ShowIndicators_, (ins, val) => ins.ShowIndicators_ = val, false, true,
                                                          "Show Indicators", "Indicates whether to display Indicators over the Ideas.");

        /// <summary>
        /// Indicates whether to display Labels with the Concept Definition name over the Concept.
        /// </summary>
        public bool ShowConceptDefinitionLabels { get { return __ShowConceptDefinitionLabels.Get(this); } set { __ShowConceptDefinitionLabels.Set(this, value); } }
        protected bool ShowConceptDefinitionLabels_ = false;
        public static readonly ModelPropertyDefinitor<View, bool> __ShowConceptDefinitionLabels =
                   new ModelPropertyDefinitor<View, bool>("ShowConceptDefinitionLabels", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.ShowConceptDefinitionLabels_, (ins, val) => ins.ShowConceptDefinitionLabels_ = val, false, true,
                                                          "Show Concept Definition Labels", "Indicates whether to display Labels with the Concept Definition name over the Concept.");

        /// <summary>
        /// Indicates whether to display Labels with the Relationship Definition name over the Relationship.
        /// </summary>
        public bool ShowRelationshipDefinitionLabels { get { return __ShowRelationshipDefinitionLabels.Get(this); } set { __ShowRelationshipDefinitionLabels.Set(this, value); } }
        protected bool ShowRelationshipDefinitionLabels_ = false;
        public static readonly ModelPropertyDefinitor<View, bool> __ShowRelationshipDefinitionLabels =
                   new ModelPropertyDefinitor<View, bool>("ShowRelationshipDefinitionLabels", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.ShowRelationshipDefinitionLabels_, (ins, val) => ins.ShowRelationshipDefinitionLabels_ = val, false, true,
                                                          "Show Relationship Definition Labels", "Indicates whether to display Labels with the Relationship Definition name over the Relationship.");

        /// <summary>
        /// Indicates whether to display Labels with the Link-Role Definitor name over the Connectors.
        /// </summary>
        public bool ShowLinkRoleDefNameLabels { get { return __ShowLinkRoleDefNameLabels.Get(this); } set { __ShowLinkRoleDefNameLabels.Set(this, value); } }
        protected bool ShowLinkRoleDefNameLabels_ = false;
        public static readonly ModelPropertyDefinitor<View, bool> __ShowLinkRoleDefNameLabels =
                   new ModelPropertyDefinitor<View, bool>("ShowLinkRoleDefNameLabels", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.ShowLinkRoleDefNameLabels_, (ins, val) => ins.ShowLinkRoleDefNameLabels_ = val, false, true,
                                                          "Show Link-Role Definitor Labels", "Indicates whether to display Labels with the Link-Role Definitor name over the Connectors.");

        /// <summary>
        /// Indicates whether to display Labels with the Link-Role Descriptor name over the Connectors.
        /// </summary>
        public bool ShowLinkRoleDescNameLabels { get { return __ShowLinkRoleDescNameLabels.Get(this); } set { __ShowLinkRoleDescNameLabels.Set(this, value); } }
        protected bool ShowLinkRoleDescNameLabels_ = true;
        public static readonly ModelPropertyDefinitor<View, bool> __ShowLinkRoleDescNameLabels =
                   new ModelPropertyDefinitor<View, bool>("ShowLinkRoleDescNameLabels", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.ShowLinkRoleDescNameLabels_, (ins, val) => ins.ShowLinkRoleDescNameLabels_ = val, false, true,
                                                          "Show Link-Role Descriptor Labels", "Indicates whether to display Labels with the Link-Role Descriptor name over the Connectors.");

        /// <summary>
        /// Indicates whether to display Labels with the Link-Role Variant over the Connectors.
        /// </summary>
        public bool ShowLinkRoleVariantLabels { get { return __ShowLinkRoleVariantLabels.Get(this); } set { __ShowLinkRoleVariantLabels.Set(this, value); } }
        protected bool ShowLinkRoleVariantLabels_ = false;
        public static readonly ModelPropertyDefinitor<View, bool> __ShowLinkRoleVariantLabels =
                   new ModelPropertyDefinitor<View, bool>("ShowLinkRoleVariantLabels", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.ShowLinkRoleVariantLabels_, (ins, val) => ins.ShowLinkRoleVariantLabels_ = val, false, true,
                                                          "Show Link-Role Variant Labels", "Indicates whether to display Labels with the Link-Role Variant over the Connectors.");

        /// <summary>
        /// Indicates whether to display Markers over the Ideas.
        /// </summary>
        public bool ShowMarkers { get { return __ShowMarkers.Get(this); } set { __ShowMarkers.Set(this, value); } }
        protected bool ShowMarkers_ = true;
        public static readonly ModelPropertyDefinitor<View, bool> __ShowMarkers =
                   new ModelPropertyDefinitor<View, bool>("ShowMarkers", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.ShowMarkers_, (ins, val) => ins.ShowMarkers_ = val, false, true,
                                                          "Show Markers", "Indicates whether to display Markers over the Ideas.");

        /// <summary>
        /// Indicates whether to display the Title of the Markers over them.
        /// </summary>
        public bool ShowMarkersTitles { get { return __ShowMarkersTitles.Get(this); } set { __ShowMarkersTitles.Set(this, value); } }
        protected bool ShowMarkersTitles_ = true;
        public static readonly ModelPropertyDefinitor<View, bool> __ShowMarkersTitles =
                   new ModelPropertyDefinitor<View, bool>("ShowMarkersTitles", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.ShowMarkersTitles_, (ins, val) => ins.ShowMarkersTitles_ = val, false, true,
                                                          "Show Markers Titles", "Indicates whether to display the Title of the Markers over them.");

        /// <summary>
        /// Indicates that the context Grid should be based on Lines, else on Points.
        /// </summary>
        public bool GridUsesLines { get { return __GridUsesLines.Get(this); } set { __GridUsesLines.Set(this, value); } }
        protected bool GridUsesLines_ = false;
        public static readonly ModelPropertyDefinitor<View, bool> __GridUsesLines =
                   new ModelPropertyDefinitor<View, bool>("GridUsesLines", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.GridUsesLines_, (ins, val) => ins.GridUsesLines_ = val, false, true,
                                                          "Grid uses Lines", "Indicates that the context Grid should be based on Lines, else on Points.");

        /// <summary>
        /// Gets or sets the context Grid size.
        /// </summary>
        public double GridSize { get { return __GridSize.Get(this); } set { __GridSize.Set(this, value); } }
        protected double GridSize_ = GRID_SIZE_INI;
        public static readonly ModelPropertyDefinitor<View, double> __GridSize =
                   new ModelPropertyDefinitor<View, double>("GridSize", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common,
                       ins => { return ins.GridSize_; },
                       (ins, val) => { ins.GridSize_ = val.EnforceRange(GRID_SIZE_MIN, GRID_SIZE_MAX); },
                       true, false, "Grid Size", "Gets or sets the context Grid size (range: " +
                                                 ((int)GRID_SIZE_MIN).ToString() + " to " + ((int)GRID_SIZE_MAX).ToString() + " pixels).");

        /// <summary>
        /// Indicates whether the postioning of objects should be aligned to grid points.
        /// </summary>
        public bool SnapToGrid { get { return __SnapToGrid.Get(this); } set { __SnapToGrid.Set(this, value); } }
        protected bool SnapToGrid_ = false;
        public static readonly ModelPropertyDefinitor<View, bool> __SnapToGrid =
                   new ModelPropertyDefinitor<View, bool>("SnapToGrid", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.SnapToGrid_, (ins, val) => ins.SnapToGrid_ = val, false, true,
                                                          "Snap to Grid", "Indicates whether the postioning of objects should be aligned to grid points.");

        /// <summary>
        /// Size of the View.
        /// </summary>
        public Size ViewSize { get { return __ViewSize.Get(this); } set { __ViewSize.Set(this, value); } }
        protected Size ViewSize_;
        public static readonly ModelPropertyDefinitor<View, Size> __ViewSize =
                   new ModelPropertyDefinitor<View, Size>("ViewSize", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.ViewSize_, (ins, val) => ins.ViewSize_ = val, false, true,
                                                          "View Size", "Size of the View.");

        /// <summary>
        /// Maximum z-order level currently assigned for visual background content.
        /// </summary>
        public int VisualLevelForBackground { get { return __VisualLevelForBackground.Get(this); } set { __VisualLevelForBackground.Set(this, value); } }
        protected int VisualLevelForBackground_ = -1;
        public static readonly ModelPropertyDefinitor<View, int> __VisualLevelForBackground =
                   new ModelPropertyDefinitor<View, int>("VisualLevelForBackground", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.VisualLevelForBackground_, (ins, val) => ins.VisualLevelForBackground_ = val, false, true,
                                                         "Visual Level For Background", "Maximum z-order level currently assigned for visual background content.");

        /// <summary>
        /// Maximum z-order level currently assigned for visual regions content.
        /// </summary>
        public int VisualLevelForRegions { get { return __VisualLevelForRegions.Get(this); } set { __VisualLevelForRegions.Set(this, value); } }
        protected int VisualLevelForRegions_ = -1;
        public static readonly ModelPropertyDefinitor<View, int> __VisualLevelForRegions =
                   new ModelPropertyDefinitor<View, int>("VisualLevelForRegions", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.VisualLevelForRegions_, (ins, val) => ins.VisualLevelForRegions_ = val, false, true,
                                                         "Visual Level For Regions", "Maximum z-order level currently assigned for visual regions content.");

        /// <summary>
        /// Current count of visual floating content.
        /// </summary>
        public int VisualCountOfFloatings { get { return __VisualCountOfFloatings.Get(this); } set { __VisualCountOfFloatings.Set(this, value); } }
        protected int VisualCountOfFloatings_ = 0;
        public static readonly ModelPropertyDefinitor<View, int> __VisualCountOfFloatings =
                   new ModelPropertyDefinitor<View, int>("VisualCountOfFloatings", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.VisualCountOfFloatings_, (ins, val) => ins.VisualCountOfFloatings_ = val, false, true,
                                                         "Visual Count Of Floatings", "Current count of visual floating content.");

        /// <summary>
        /// Automatically adjusts size of objects based on entered text.
        /// </summary>
        public bool AutoSizeByEnteredText { get { return __AutoSizeByEnteredText.Get(this); } set { __AutoSizeByEnteredText.Set(this, value); } }
        protected bool AutoSizeByEnteredText_ = true;
        public static readonly ModelPropertyDefinitor<View, bool> __AutoSizeByEnteredText =
                   new ModelPropertyDefinitor<View, bool>("AutoSizeByEnteredText", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.AutoSizeByEnteredText_, (ins, val) => ins.AutoSizeByEnteredText_ = val, false, true,
                                                          "Auto-Size by Entered Text", "Automatically adjusts size of objects based on entered text.");

        /// <summary>
        /// Central point of the View.
        /// </summary>
        public Point ViewCenter
        {
            get
            {
                var Result = new Point(Math.Ceiling(this.ViewSize.Width / 2.0), Math.Ceiling(this.ViewSize.Height / 2.0));
                return Result;
            }
        }

        /// <summary>
        /// Gets or sets the last scroll offset.
        /// </summary>
        public Point LastScrollOffset
        {
            get
            {
                if (LastScrollOffset_ == null || !LastScrollOffset_.HasValue)
                    LastScrollOffset_ = new Point(ViewCenter.X - (this.HostingScrollViewer == null
                                                                  ? 0.0 : this.HostingScrollViewer.ViewportWidth / 2.0),
                                                  ViewCenter.Y - (this.HostingScrollViewer == null
                                                                  ? 0.0 : this.HostingScrollViewer.ViewportHeight / 2.0));

                return LastScrollOffset_.Value;
            }
            set
            {
                if (this.HostingScrollViewer == null || !this.HostingScrollViewer.IsLoaded)
                    return;

                LastScrollOffset_ = value;
            }
        }
        // Starts null like (and for support) older Composition versions without this field.
        public Nullable<Point> LastScrollOffset_ = null;

        /// <summary>
        /// Gets the central point of the currently presented area of the view.
        /// </summary>
        public Point CurrentPresentationCenter
        {
            get
            {
                var Result = new Point(this.HostingScrollViewer.HorizontalOffset + (this.HostingScrollViewer.ViewportWidth / 2.0),
                                       this.HostingScrollViewer.VerticalOffset + (this.HostingScrollViewer.ViewportHeight / 2.0));
                return Result;
            }
        }

        public void ReactToMouseWheel(object Sender, MouseWheelEventArgs Args)
        {
            var Handler = MouseWheel;

            if (Handler != null)
                Handler(Sender, Args);
        }

        public void Close()
        {
            this.IsClosing = true;
            this.UnselectAllObjects();
        }

        #endregion

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelClass<View> Members

        public new MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public new ModelClassDefinitor<View> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly new ModelClassDefinitor<View> __ClassDefinitor = null;

        public new View CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((View)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public View PopulateFrom(View SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public void ShowOnTop(Control Target, Point ScreenPosition, bool LimitMaxWidth = false,
                              double LimitedMinWidth = 10.0, double LimitedMinHeight = 10.0)
        {
            if (!this.TopCanvas.Children.Contains(Target))
                this.TopCanvas.Children.Add(Target);

            var Position = this.TopCanvas.PointFromScreen(ScreenPosition);
            Canvas.SetLeft(Target, Position.X);
            Canvas.SetTop(Target, Position.Y);

            if (LimitMaxWidth)
                this.TopCanvas.PostCall(
                    tcv =>
                    {
                        Target.MaxWidth = (tcv.ActualWidth - Position.X).EnforceRange(LimitedMinWidth, tcv.ActualWidth);
                        Target.MaxHeight = (tcv.ActualHeight - Position.Y).EnforceRange(LimitedMinHeight, tcv.ActualHeight);
                    });

            TopCanvas.Visibility = Visibility.Visible;
            Target.Visibility = Visibility.Visible;
            Target.Focus();
        }

        // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        public Rect DetermineContentArea()
        {
            var VisualObjects = this.ViewChildren.Where(child => (child.Key is VisualObject) && !(child.Key is VisualInert))
                                                    .Select(child => (VisualObject)child.Key);

            if (!VisualObjects.Any())
                return Rect.Empty;

            double MeasuredLeftLimit = double.MaxValue;
            double MeasuredRightLimit = double.MinValue;
            double MeasuredTopLimit = double.MaxValue;
            double MeasuredBottomLimit = double.MinValue;

            foreach (var VisObj in VisualObjects)
            {
                var ObjectArea = (VisObj.Graphic != null ? VisObj.Graphic.ContentBounds : VisObj.TotalArea);
                if (ObjectArea.IsEmpty)
                    continue;

                MeasuredLeftLimit = Math.Min(MeasuredLeftLimit, ObjectArea.Left);
                MeasuredRightLimit = Math.Max(MeasuredRightLimit, ObjectArea.Right);
                MeasuredTopLimit = Math.Min(MeasuredTopLimit, ObjectArea.Top);
                MeasuredBottomLimit = Math.Max(MeasuredBottomLimit, ObjectArea.Bottom);
            }

            var Result = new Rect(MeasuredLeftLimit, MeasuredTopLimit,
                                  (MeasuredRightLimit - MeasuredLeftLimit) + 1.0,
                                  (MeasuredBottomLimit - MeasuredTopLimit) + 1.0);
            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Selected Visual Representations
        /// </summary>
        public IEnumerable<VisualRepresentation> SelectedRepresentations
        {
            get
            {
                var Result = this.SelectedObjects.Where(visobj => visobj is VisualElement)
                                    .Select(visobj => ((VisualElement)visobj).OwnerRepresentation)
                                        .Distinct();
                return Result;
            }
        }

        /// <summary>
        /// Gets the selected Visual Representation objects.
        /// </summary>
        public IList<VisualObject> SelectedObjects { get { return this.SelectedObjectsList; } }

        /// <summary>
        /// List of selected Visual Representation objects.
        /// </summary>
        protected EditableList<VisualObject> SelectedObjectsList
        {
            get
            {
                // this must be done in order to be regenerated after serialization
                if (this.SelectedObjectsList_ == null)
                {
                    // No variating instance is passed for not collect changes for undo/redo.
                    this.SelectedObjectsList_ = new EditableList<VisualObject>("SelectedObjectList", null);
                }

                return this.SelectedObjectsList_;
            }
        }

        [NonSerialized]
        private EditableList<VisualObject> SelectedObjectsList_ = null;

        // -----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Determines and returns the objects which currently are directly or indirectly selected to be manipulated (moved/resized).
        /// Each item has the visual-object, plus indication of being contained within a region. The resulting list includes possible duplicates.
        /// </summary>
        /// <param name="IncludeRelatedOrigins">Indicates whether the Origins subtree must be considered as to be manipulated.</param>
        /// <param name="IncludeRelatedTargets">Indicates whether the Targets subtree must be considered as to be manipulated.</param>
        /// <param name="IsForVisualization">Indicates that this request is for visualizing and not manipulation (thus, the indirectly/implicitly selected objects must not be included).</param>
        public IList<Tuple<VisualObject,bool>> GetCurrentManipulableObjects(bool IncludeRelatedOrigins, bool IncludeRelatedTargets, bool IsForVisualization = false)
        {
            // IMPORTANT: Do not apply .Distinct() to give the caller a chance to detect duplicates
            var Result = this.SelectedObjects.SelectMany(vobj => vobj.GetMovableMembers(IncludeRelatedOrigins, IncludeRelatedTargets, IsForVisualization))
                                  .ToList();

            return Result;
        }

        // -----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Selects the supplied object for later command applying.
        /// </summary>
        public void SelectObject(VisualObject Target, bool EditProperties = true)
        {
            var LocalCommand = !Target.EditEngine.IsVariating;
            if (LocalCommand)
                Target.EditEngine.StartCommandVariation("Select Object", false);

            Target.IsSelected = true;
            this.SelectedObjectsList.AddNew(Target);

            if (EditProperties && ProductDirector.WorkspaceDirector.ActiveDocument.ActiveDocumentView == this)
            {
                IModelEntity TargetEntity = null;

                if (Target is VisualElement)
                    TargetEntity = ((VisualElement)Target).OwnerRepresentation.RepresentedIdea;
                else
                    if (Target is VisualComplement && !((VisualComplement)Target).Target.IsGlobal)
                        TargetEntity = ((VisualComplement)Target).Target.OwnerLocal.OwnerRepresentation.RepresentedIdea;

                if (TargetEntity != null)
                    ProductDirector.EditorInterrelationsControl.SetTarget(TargetEntity);
            }

            if (LocalCommand)
                Target.EditEngine.CompleteCommandVariation();
        }

        /// <summary>
        /// Unselects the supplied object for command applying.
        /// </summary>
        public void UnselectObject(VisualObject Target)
        {
            var LocalCommand = !Target.EditEngine.IsVariating;
            if (LocalCommand)
                Target.EditEngine.StartCommandVariation("Unselect Object", false);

            Target.IsSelected = false;
            Target.IsVanished = false;
            this.SelectedObjectsList.Remove(Target);

            if (LocalCommand)
                Target.EditEngine.CompleteCommandVariation();
        }

        /// <summary>
        /// Unselects all the selected objects, except by the excluded one (if any), and propagate visual changes.
        /// Returns indication of application.
        /// </summary>
        public bool UnselectAllObjects(VisualObject ExcludedObject = null)
        {
            //T Console.WriteLine(DateTime.Now.ToString("yyMMdd hhmmss") + " Unselected all objects.");

            if (!this.SelectedObjects.Any())
                return false;

            var Selection = this.SelectedObjects.ToList();

            var LocalCommand = !this.EditEngine.IsVariating;
            if (LocalCommand)
                this.EditEngine.StartCommandVariation("Unselect All Objects", false);

            // IMPORTANT: Notice the validation about the existence of elements in VisualRepresentations collection,
            //            in order to not visually re-create representations previously deleted.
            foreach (var VisObject in Selection)
                if (this.ViewChildren.Any(child => child.Key == VisObject) &&
                    VisObject.IsSelected && VisObject != ExcludedObject)
                {
                    this.UnselectObject(VisObject);
                    VisObject.RenderRepresentatedObject();
                }

            if (ExcludedObject == null)
                this.SelectedObjectsList.Clear();
            else
            {
                var Targets = this.SelectedObjectsList.Where(obj => !obj.IsEqual(ExcludedObject)).ToArray();

                foreach (var Target in Targets)
                    this.SelectedObjectsList.Remove(Target);
            }

            ProductDirector.EditorInterrelationsControl.SetTarget(this.SelectedObjects.FirstOrDefault());

            if (LocalCommand)
                this.EditEngine.CompleteCommandVariation(true);

            return true;
        }

        // -----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Representations that will be excluded from Source-Representations enumering to avoid infinite loop.
        /// </summary>
        public List<VisualRepresentation> OriginRepresentationsTrace
        {
            get
            {
                if (this.SourceRepresentationsTrace_ == null)
                    this.SourceRepresentationsTrace_ = new List<VisualRepresentation>();

                return this.SourceRepresentationsTrace_;
            }
        }
        [NonSerialized]
        private List<VisualRepresentation> SourceRepresentationsTrace_ = null;

        /// <summary>
        /// Representations that will be excluded from Target-Representations enumering to avoid infinite loop.
        /// </summary>
        public List<VisualRepresentation> TargetRepresentationsTrace
        {
            get
            {
                if (this.TargetRepresentationsTrace_ == null)
                    this.TargetRepresentationsTrace_ = new List<VisualRepresentation>();

                return this.TargetRepresentationsTrace_;
            }
        }
        [NonSerialized]
        private List<VisualRepresentation> TargetRepresentationsTrace_ = null;

        // -----------------------------------------------------------------------------------------------------------------------
        // Do not put this in the ViewManipulationManager (which could not be already instantiated Views of composite Ideas)
        public void EditPropertiesOfVisualRepresentation(VisualRepresentation TargetRepresentation,
                                                         string OpenedTabKey = null, MarkerAssignment PointedMarkerAssignment = null)
        {
            if (TargetRepresentation.RepresentedIdea is Concept)
                this.Engine
                    .EditConceptProperties(TargetRepresentation.RepresentedIdea as Concept,
                                           TargetRepresentation.MainSymbol, false, false, OpenedTabKey, PointedMarkerAssignment);

            if (TargetRepresentation.RepresentedIdea is Relationship)
                this.Engine.EditRelationshipProperties(TargetRepresentation.MainSymbol, OpenedTabKey, PointedMarkerAssignment);
        }

        // Do not put this in the ViewManipulationManager (which could not be already instantiated Views of composite Ideas)
        public void AppendDetailToVisualRepresentation(VisualRepresentation TargetRepresentation, string PreselectedKind = null)
        {
            var Engine = TargetRepresentation.EditEngine as CompositionEngine;

            // Add the new detail
            Engine.StartCommandVariation("Create Attachment Detail");

            var Detail = Engine.CreateIdeaDetail(Ownership.Create<IdeaDefinition, Idea>(TargetRepresentation.RepresentedIdea),
                                                 TargetRepresentation.RepresentedIdea,
                                                 TargetRepresentation.RepresentedIdea.Details.Select(det => det.Designation),
                                                 false, PreselectedKind);
            if (Detail == null)
            {
                Engine.DiscardCommandVariation();
                return;
            }

            TargetRepresentation.RepresentedIdea.Details.AddNew(Detail);
            TargetRepresentation.RepresentedIdea.UpdateVersion();
            TargetRepresentation.Render();

            Engine.CompleteCommandVariation();
        }

        // -----------------------------------------------------------------------------------------------------------------------
        public VisualObject InPlaceEditingTarget
        {
            get { return this.InPlaceEditingTarget_; }
            protected set { this.InPlaceEditingTarget_ = value; }
        }
        [NonSerialized]
        private VisualObject InPlaceEditingTarget_ = null;

        [NonSerialized]
        private TextBox EditBox = null;

        [NonSerialized]
        private bool InPlaceEditingAcceptsReturn = false;

        public void EditInPlace(VisualObject Target, bool? AcceptsReturn = null, bool TryAgainIfFailed = true)
        {
            /*T
            if (Target is VisualElement)
            {
                var Representator = ((VisualElement)Target).OwnerRepresentation;
                if (Representator is ConceptVisualRepresentation)
                    this.Manipulator.EditProperties(Representator);
            }

            return; */

            if (!TryAgainIfFailed)  // when re-trying
                if (!this.IsEditingInPlace) // if a pending stop-editi-in-place was processed in the meanwhile
                    return;

            this.IsEditingInPlace = true;
            this.InPlaceEditingTarget = Target;
            //T Console.WriteLine("Start edit-in-place " + this.InPlaceEditingTarget.ToStringAlways());

            this.InPlaceEditingAcceptsReturn = (AcceptsReturn != null && AcceptsReturn.HasValue
                                                ? AcceptsReturn.Value
                                                : ((Target is VisualSymbol
                                                   ? VisualSymbolFormat.GetInPlaceEditingIsMultiline((VisualSymbol)Target)
                                                   : true)));

            var TargetSymbol = Target as VisualSymbol;
            this.Manipulator.UnpointObject(TargetSymbol);

            Rect TargetZone = Rect.Empty;

            if (TargetSymbol != null)
                TargetZone = TargetSymbol.BaseContentArea;
            else
            {
                TargetZone = Target.BaseArea;

                if (TargetZone.Width > 10 && TargetZone.Height > 10)
                    TargetZone = new Rect(TargetZone.X + 2, TargetZone.Y + 2,
                                          TargetZone.Width - 4, TargetZone.Height - 4);
            }

            this.Presenter.BringIntoView(TargetZone);

            if (this.EditBox == null)
            {
                this.EditBox = new TextBox();
                this.EditBox.IsUndoEnabled = false;
                this.EditBox.FontFamily = new FontFamily("Arial");
                this.EditBox.FontSize = 9;
                this.EditBox.PreviewKeyDown += EditBox_PreviewKeyDown;
                this.EditBox.KeyDown += EditBox_KeyDown;
                this.EditBox.LostFocus += EditBox_LostFocus;
                this.EditBox.SizeChanged += EditBox_SizeChanged;
            }

            this.EditBox.AcceptsReturn = this.InPlaceEditingAcceptsReturn;

            if (TargetSymbol != null)
            {
                this.EditBox.Text = (VisualSymbolFormat.GetUseNameAsMainTitle(TargetSymbol)
                                     ? TargetSymbol.OwnerRepresentation.RepresentedIdea.Name : TargetSymbol.OwnerRepresentation.RepresentedIdea.TechName);
                this.EditBox.FontFamily = VisualSymbolFormat.GetTextFormat(TargetSymbol, ETextPurpose.Title).CurrentTypeface.FontFamily;
                this.EditBox.FontSize = VisualSymbolFormat.GetTextFormat(TargetSymbol, ETextPurpose.Title).FontSize;
            }
            else
                if (Target is VisualComplement)
                {
                    var TargetComplement = (VisualComplement)Target;
                    if (TargetComplement.IsComplementText)
                    {
                        this.EditBox.Text = TargetComplement.GetPropertyField<string>(VisualComplement.PROP_FIELD_TEXT).ToStringAlways();

                        var Format = TargetComplement.GetPropertyField<TextFormat>(VisualComplement.PROP_FIELD_TEXTFORMAT);
                        if (Format != null)
                        {
                            this.EditBox.FontFamily = Format.CurrentTypeface.FontFamily; // ALT: new FontFamily(Format.FontFamilyName);
                            this.EditBox.FontSize = Format.FontSize;
                        }
                        else
                        {
                            this.EditBox.FontFamily = new FontFamily("Arial");
                            this.EditBox.FontSize = 10.0;
                        }
                    }
                    else
                    {
                        this.EditBox.Text = TargetComplement.GetPropertyField<string>(VisualComplement.PROP_FIELD_TEXT).ToStringAlways();

                        var Format = TargetComplement.GetPropertyField<TextFormat>(VisualComplement.PROP_FIELD_TEXTFORMAT);
                        if (Format != null)
                        {
                            this.EditBox.FontFamily = Format.CurrentTypeface.FontFamily; // ALT: new FontFamily(Format.FontFamilyName);
                            this.EditBox.FontSize = Format.FontSize;
                        }
                        else
                        {
                            this.EditBox.FontFamily = new FontFamily("Arial");
                            this.EditBox.FontSize = 10.0;
                        }
                    }
                }

            if (this.AutoSizeByEnteredText)
            {
                this.EditBox.Width = double.NaN;
                this.EditBox.Height = double.NaN;
                this.EditBox.MinWidth = Math.Max(EDITBOX_MIN_WIDTH, TargetZone.Width);
                this.EditBox.MinHeight = Math.Max(EDITBOX_MIN_HEIGHT, TargetZone.Height);
            }
            else
            {
                this.EditBox.Width = TargetZone.Width;
                this.EditBox.Height = TargetZone.Height;
                this.EditBox.MinWidth = EDITBOX_MIN_WIDTH;
                this.EditBox.MinHeight = EDITBOX_MIN_HEIGHT;
            }

            this.InitialEditBoxCenterPos = default(Point);

            // This post-call is needed due to WPF tricky rendering precedence (or whatever it is!).
            this.HostingScrollViewer.PostCall(
                (scrollviewer) =>
                {
                    try
                    {
                        var Location = Target.Graphic.PointToScreen(new Point(TargetZone.Left, TargetZone.Top));
                        ShowOnTop(this.EditBox, Location, this.AutoSizeByEnteredText, EDITBOX_MIN_WIDTH, EDITBOX_MIN_HEIGHT);
                        this.EditBox.SelectAll();

                        this.EditBox.PostCall(ebx => this.InitialEditBoxCenterPos =
                                                            new Point(Canvas.GetLeft(ebx) + (ebx.ActualWidth / 2.0),
                                                                      Canvas.GetTop(ebx) + (ebx.ActualHeight / 2.0)));
                    }
                    catch (Exception Problem)
                    {
                        /*? if (TryAgainIfFailed)
                        {
                            Target.RenderRepresentatedObject();
                            scrollviewer.PostCall(scv => this.EditInPlace(Target, AcceptsReturn, false));
                        }
                        else */
                            Console.WriteLine("Cannot Edit In-Place, object has pending redisplay (computer is overloaded/slow).");
                    }
                });
        }

        void EditBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.InitialEditBoxCenterPos == default(Point))
                return;

            Canvas.SetLeft(this.EditBox, this.InitialEditBoxCenterPos.X - (this.EditBox.ActualWidth / 2.0));
            Canvas.SetTop(this.EditBox, this.InitialEditBoxCenterPos.Y - (this.EditBox.ActualHeight / 2.0));
        }
        private Point InitialEditBoxCenterPos = default(Point);

        public bool IsEditingInPlace
        {
            get { return this.IsEditingInPlace_; }
            protected set { this.IsEditingInPlace_ = value; }
        }
        [NonSerialized]
        private bool IsEditingInPlace_ = false;

        public void StopEditInplace()
        {
            //T return;

            //T Console.WriteLine("Stop edit-in-place " + this.InPlaceEditingTarget.ToStringAlways());
            var TargetSymbol = this.InPlaceEditingTarget as VisualSymbol;
            var TargetComplement = this.InPlaceEditingTarget as VisualComplement;

            // Save the text editor dimensions
            var TextEditorSize = default(Size);

            if (TargetSymbol != null)
                TextEditorSize = new Size(this.EditBox.ActualWidth, this.EditBox.ActualHeight);

            // These initial invisibility lines must be first in case the InEditingSymbol is null.
            this.TopCanvas.Visibility = Visibility.Collapsed;
            this.EditBox.Visibility = Visibility.Collapsed;

            if (this.InPlaceEditingTarget == null   // The next happens when undoing while edinting-in-place
                || (this.InPlaceEditingTarget.BaseLeft == double.NegativeInfinity
                    && this.InPlaceEditingTarget.BaseArea.Left == double.NegativeInfinity))
                return;

            // Prevent very rare and dangerous case (Unreproducible. Detected once, after changing Composition while editing in-place)
            if (this.InPlaceEditingTarget.EditEngine != EntityEditEngine.ActiveEntityEditor)
            {
                this.EditBox.Text = "";
                return;
            }

            string CurrentText = null;

            if (TargetSymbol != null)
                CurrentText = (VisualSymbolFormat.GetUseNameAsMainTitle(TargetSymbol)
                               ? TargetSymbol.OwnerRepresentation.RepresentedIdea.Name
                               : TargetSymbol.OwnerRepresentation.RepresentedIdea.TechName);
            else
                CurrentText = TargetComplement.GetPropertyField<string>(VisualComplement.PROP_FIELD_TEXT);

            if (!this.EditingWasCancelled
                && CurrentText != this.EditBox.Text)
            {
                this.EditEngine.StartCommandVariation("Edit In-Place");

                var ScaleFactor = (this.PageDisplayScale / 100.0);

                if (TargetSymbol != null)
                {
                    if (VisualSymbolFormat.GetUseNameAsMainTitle(TargetSymbol))
                        TargetSymbol.OwnerRepresentation.RepresentedIdea.Name = this.EditBox.Text;
                    else
                        TargetSymbol.OwnerRepresentation.RepresentedIdea.TechName = this.EditBox.Text;

                    if (this.AutoSizeByEnteredText)
                    {
                        // Determine required space for text
                        var Format = VisualSymbolFormat.GetTextFormat(TargetSymbol, ETextPurpose.Title);
                        var IntendedTextDraw = Format.GenerateFormattedText(this.EditBox.Text,
                                                 (this.ViewSize.Width - TargetSymbol.BaseLeft) * ScaleFactor,
                                                 (this.ViewSize.Height - TargetSymbol.BaseTop) * ScaleFactor);

                        // Consider extra-space due to geometry of shapes and their content rectangle.
                        // PENDING: Consider Pictogram size and its vert/horiz position.
                        var ExtraWidth = (TargetSymbol.BaseWidth - TargetSymbol.BaseContentArea.Width) * ScaleFactor;
                        var ExtraHeight = (TargetSymbol.BaseHeight - TargetSymbol.BaseContentArea.Height) * ScaleFactor;

                        // Determine new sizes and consider extra-width plus margin
                        var AdjustedWidth = ((IntendedTextDraw.WidthIncludingTrailingWhitespace + ExtraWidth)
                                             * AUTOSIZE_MARGIN_ADJUST_FACTOR)
                                             .EnforceMinimum(TargetSymbol.OwnerRepresentation
                                                             .RepresentedIdea.IdeaDefinitor.DefaultSymbolFormat.InitialWidth);
                        var AdjustedHeight = ((IntendedTextDraw.Height + ExtraHeight)
                                             * AUTOSIZE_MARGIN_ADJUST_FACTOR)
                                             .EnforceMinimum(TargetSymbol.OwnerRepresentation
                                                             .RepresentedIdea.IdeaDefinitor.DefaultSymbolFormat.InitialHeight);

                        if (AdjustedWidth != TargetSymbol.BaseWidth || AdjustedHeight != TargetSymbol.BaseHeight)
                        {
                            var PrevCenter = TargetSymbol.BaseCenter;

                            if (TargetSymbol.ResizeTo(AdjustedWidth, AdjustedHeight))
                                TargetSymbol.MoveTo(PrevCenter.X, PrevCenter.Y,
                                                    !TargetSymbol.IsAutoPositionable, true);
                        }
                    }

                    TargetSymbol.RenderElement();
                }
                else
                {
                    // IMPORTANT: The whole Content MUST be assigned in order to be undoable/readoable.

                    if (this.AutoSizeByEnteredText)
                    {
                        // Determine required space for text
                        var Format = TargetComplement.GetPropertyField<TextFormat>(VisualComplement.PROP_FIELD_TEXTFORMAT)
                                        .NullDefault(new TextFormat());

                        var IntendedTextDraw = Format.GenerateFormattedText(this.EditBox.Text,
                                                 (this.ViewSize.Width - TargetComplement.BaseLeft) * ScaleFactor,
                                                 (this.ViewSize.Height - TargetComplement.BaseTop) * ScaleFactor);

                        // Determine new sizes and consider margin
                        var AdjustedWidth = (IntendedTextDraw.WidthIncludingTrailingWhitespace * AUTOSIZE_MARGIN_ADJUST_FACTOR)
                                             .EnforceMinimum(TargetComplement.GetInitialWidth());
                        var AdjustedHeight = (IntendedTextDraw.Height * AUTOSIZE_MARGIN_ADJUST_FACTOR)
                                             .EnforceMinimum(TargetComplement.GetInitialHeight());

                        if (AdjustedWidth != TargetComplement.BaseWidth || AdjustedHeight != TargetComplement.BaseHeight)
                        {
                            var PrevCenter = TargetComplement.BaseCenter;

                            if (TargetComplement.ResizeTo(AdjustedWidth, AdjustedHeight))
                                TargetComplement.MoveTo(PrevCenter.X, PrevCenter.Y, false, true);
                        }
                    }

                    TargetComplement.SetPropertyField(VisualComplement.PROP_FIELD_TEXT, this.EditBox.Text);
                    TargetComplement.Render();
                }

                this.UpdateVersion();
                this.EditEngine.CompleteCommandVariation();
            }

            this.EditingWasCancelled = false;
            this.InPlaceEditingTarget = null;

            this.Presenter.OwnerView.IsEditingInPlace = false;

            // OLD Style (which requires an extra ENTER):
            // Must be post-called to be applied after key processing
            // this.Presenter.PostCall(pres => pres.OwnerView.IsEditingInPlace = false);
        }

        void EditBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            string Insertion = null;
            var CaretIndex = this.EditBox.CaretIndex;
            var IsAtEnd = (CaretIndex >= this.EditBox.Text.Length);

            if (Keyboard.IsKeyDown(Key.Enter) && Keyboard.Modifiers == ModifierKeys.Alt)
                Insertion = Environment.NewLine;

            if (Keyboard.IsKeyDown(Key.Tab) && Keyboard.Modifiers == ModifierKeys.Shift)
                Insertion = ((char)9).ToString();

            if (Insertion != null)
            {
                this.EditBox.Text = this.EditBox.Text.Insert(CaretIndex, Insertion);
                this.EditBox.SelectionStart = (IsAtEnd ? this.EditBox.Text.Length : (CaretIndex + Insertion.Length));

                e.Handled = true;
            }
        }

        void EditBox_KeyDown(object sender, KeyEventArgs e)
        {
            this.EditingWasCancelled = false;

            if ((e.Key == Key.Enter && (!this.InPlaceEditingAcceptsReturn || Keyboard.Modifiers == ModifierKeys.Control))
                || (e.Key == Key.Tab && !(Keyboard.Modifiers == ModifierKeys.Shift))
                || e.Key == Key.Escape)
            {
                if (e.Key == Key.Escape)
                    this.EditingWasCancelled = true;

                StopEditInplace();
            }
        }

        [NonSerialized]
        bool EditingWasCancelled = false;

        void EditBox_LostFocus(object sender, RoutedEventArgs e)
        {
            StopEditInplace();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public Rect SelectByRectangle(Point Position)
        {
            // This evaluation considers that, after deserialization,
            // SelectionRectangleOrigin has it coordinates in zero.
            if (this.SelectionRectangleOrigin == Display.NULL_POINT || this.SelectionRectangleOrigin == default(Point))
            {
                this.Presenter.CaptureMouse();

                this.SelectionRectangleOrigin = Position;
                return Rect.Empty;
            }

            var SelectionRectangle = new Rect(this.SelectionRectangleOrigin, Position);

            var Drawer = new DrawingVisual();
            using (DrawingContext Context = Drawer.RenderOpen())
            {
                Context.DrawRectangle(Brushes.Transparent, SelectionRectanglePen, SelectionRectangle);
            }

            if (this.SelectionRectangleOrigin != default(Point))
                this.Presenter.ClearTransientVisual(VISKEY_SEL_RECT);

            var BorderTresholdRect = new Rect(Position.X - BORDER_TRESHOLD, Position.Y - BORDER_TRESHOLD, BORDER_TRESHOLD * 2.0, BORDER_TRESHOLD * 2.0);

            var PreviousOffsets = new Point(this.HostingScrollViewer.ContentHorizontalOffset, this.HostingScrollViewer.ContentVerticalOffset);
            this.Presenter.BringIntoView(BorderTresholdRect);
            this.Presenter.UpdateLayout();
            var CurrentOffsets = new Point(this.HostingScrollViewer.ContentHorizontalOffset, this.HostingScrollViewer.ContentVerticalOffset);

            if (CurrentOffsets != PreviousOffsets)
                Thread.Sleep(INTERACTIVE_SLOW_DOWN);  // Slow down the scroller. Too fast for the user!

            this.Presenter.PutTransientVisual(VISKEY_SEL_RECT, Drawer);

            return SelectionRectangle;
        }
        [NonSerialized]
        private Point SelectionRectangleOrigin;
        [NonSerialized]
        private Point SelectionRectanglePreviousOrigin;
        private const string VISKEY_SEL_RECT = "SelectionRectangle";

        public void FinishSelectByRectangle(bool ApplySelection, Point MousePosition = default(Point))
        {
            this.Presenter.ReleaseMouseCapture();

            // Select the symbols completely within the rectangle scope
            if (ApplySelection)
            {
                // NOTE: This tricky of the "previous" origin is needed because of the sporadic double-calling of this method
                var PosOrigin = (this.SelectionRectangleOrigin == Display.NULL_POINT || this.SelectionRectangleOrigin == default(Point)
                                 ? this.SelectionRectanglePreviousOrigin : this.SelectionRectangleOrigin);

                if (PosOrigin != default(Point))
                {
                    var SelectionRectangle = new Rect(PosOrigin, MousePosition);

                    var SelectByPartialInsersection = AppExec.GetConfiguration<bool>("View", "DiagramEdit.SelectByPartialIntersection", false);

                    var EnclosedObjects = this.ViewChildren.Where(child => child.Key is VisualObject
                        // The main-symbol will do the job once for the entire representation.
                                                                            && !(child.Key is VisualConnector)
                                                                            && (SelectByPartialInsersection
                                                                                ? SelectionRectangle.IntersectsWith(((VisualObject)child.Key).TotalArea)
                                                                                : SelectionRectangle.Contains(((VisualObject)child.Key).TotalArea)))
                                                            .Select(child => (VisualObject)child.Key)
                                                                .OrderBy(obj => ("000000" + obj.BaseArea.Left.ToString()).GetRight(6) +
                                                                                ("000000" + obj.BaseArea.Top.ToString()).GetRight(6))
                                                                    .ToArray();

                    SelectMultipleObjects(EnclosedObjects);

                    //- Console.WriteLine("EndSelRect. Rect={0}. RectOrig={1}. MousePose={2}.", SelectionRectangle, this.SelectionRectangleOrigin, MousePosition);
                }
            }

            // Reset rectangle
            if (this.SelectionRectangleOrigin != default(Point))
            {
                this.Presenter.ClearTransientVisual(VISKEY_SEL_RECT);
                this.SelectionRectanglePreviousOrigin = this.SelectionRectangleOrigin;
                this.SelectionRectangleOrigin = Display.NULL_POINT;
            }

            CompositionEngine.IsSelectingByRectangle = false;
        }

        /// <summary>
        /// Selects the specified target objects or all if nothing is passed.
        /// </summary>
        public void SelectMultipleObjects(IEnumerable<VisualObject> TargetObjects = null)
        {
            if (TargetObjects == null)
                TargetObjects = this.ViewChildren.Where(child => child.Key is VisualObject
                                                        // The main-symbol will do the job once for the entire representation.
                                                                       && !(child.Key is VisualConnector))
                                                        .Select(child => (VisualObject)child.Key).ToList(); // Do not remove this .ToList()!

            this.EditEngine.StartCommandVariation("Select multiple objects", false);

            // Clear current selection (if not ctrl or shift are pressed)
            if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl)
                && !Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
                this.UnselectAllObjects();

            var First = true;
            foreach (var Target in TargetObjects)
            {
                this.SelectObject(Target, First);
                Target.RenderRepresentatedObject();
                First = false;
            }

            this.EditEngine.CompleteCommandVariation(true);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public HitTestFilterBehavior VisualHitTestFilter(DependencyObject Evaluated)
        {
            // Note: Presenter does not exists for not open Composite-Content views.
            var TransientsCount = (this.Presenter == null ? 0 : this.Presenter.TransientObjectsCount);

            for (int Index = this.ViewChildren.Count - TransientsCount; Index < this.ViewChildren.Count - 1; Index++)
                if (this.ViewChildren[Index].Content == Evaluated)
                    return HitTestFilterBehavior.ContinueSkipSelf;

            return HitTestFilterBehavior.Continue;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the supplied Coordinate snapped/adjusted to the view grid size.
        /// </summary>
        public double GetGridSnappedCoordinate(double Coordinate, bool ApplyScale = true)
        {
            var Interval = (ApplyScale
                            ? this.GridSize * ((double)this.PageDisplayScale / 100.0)
                            : this.GridSize);

            var Div = Coordinate / Interval;

            var Pos = Interval * Math.Floor(Div);
            if (Coordinate > Pos + (Interval / 2.0))
                Pos = Pos + Interval;

            //T Console.WriteLine("Coordinate: Original={0}, New={1}.", Coordinate, Pos);
            Coordinate = Pos;

            return Coordinate;
        }

        /// <summary>
        /// Gets a Rect area snapped/adjusted to the view grid size based on the supplied original-center, width and height.
        /// </summary>
        public Rect GetGridSnappedArea(Point Center, double Width, double Height, bool ApplyScale = true)
        {
            Width = GetGridSnappedCoordinate(Width, ApplyScale);
            Height = GetGridSnappedCoordinate(Height, ApplyScale);

            var Left = Center.X - Width / 2.0;
            var Top = Center.Y - Height / 2.0;

            Left = GetGridSnappedCoordinate(Left, ApplyScale);
            Top = GetGridSnappedCoordinate(Top, ApplyScale);

            var Result = new Rect(Left, Top, Width, Height);

            return Result;
        }

        /// <summary>
        /// Gets the supplied Position snapped/adjusted to the view grid size.
        /// </summary>
        public Point GetGridSnappedPosition(Point Position, bool ApplyScale)
        {
            Position = new Point(GetGridSnappedCoordinate(Position.X, ApplyScale),
                                 GetGridSnappedCoordinate(Position.Y, ApplyScale));

            return Position;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public void Pan(double DeltaX = double.NaN, double DeltaY = double.NaN, bool ByMouse = true)
        {
            if (ByMouse && Mouse.RightButton != MouseButtonState.Pressed)
                return;

            if (DeltaX.IsNan() && DeltaY.IsNan())
            {
                // This requires window-level mouse positions
                var CurrentPosition = Mouse.GetPosition(Display.GetCurrentWindow());

                if (this.PreviousPanPosition == default(Point))
                {
                    this.PreviousPanPosition = CurrentPosition;
                    return;
                }

                DeltaX = (PreviousPanPosition.X - CurrentPosition.X);
                DeltaY = (PreviousPanPosition.Y - CurrentPosition.Y);

                this.PreviousPanPosition = CurrentPosition;
            }

            //T Console.WriteLine("panx={0}, pany={1}, previouspos={2}", OffsetX, OffsetY, this.PreviousPanPosition);

            if (!DeltaX.IsNan() && DeltaX != 0)
            {
                var OffsetX = this.HostingScrollViewer.HorizontalOffset + DeltaX;
                this.HostingScrollViewer.ScrollToHorizontalOffset(OffsetX);
            }

            if (!DeltaY.IsNan() && DeltaY != 0)
            {
                var OffsetY = this.HostingScrollViewer.VerticalOffset + DeltaY;
                this.HostingScrollViewer.ScrollToVerticalOffset(OffsetY);
            }

        }

        [NonSerialized]
        public Point PreviousPanPosition = default(Point);

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Draws the displayable content on the specified drawing Context and Available Area.
        /// Returns indication of content drawn.
        /// </summary>
        public bool DrawContent(DrawingContext Context, Rect AvailableArea,
                                bool WithTransparentBackground = false, Brush Background = null)
        {
            if (AvailableArea.Width < 24 || AvailableArea.Height < 24)
                return false;

            var DisplayZone = new Rect(AvailableArea.Left + SNAPSHOT_MARGIN, AvailableArea.Top + SNAPSHOT_MARGIN,
                                       AvailableArea.Width - SNAPSHOT_MARGIN * 2.0, AvailableArea.Height - SNAPSHOT_MARGIN * 2.0);

            // Draw the content without inerts
            var ContentArea = this.DetermineContentArea();

            var CompositeGroup = new DrawingGroup();
            bool ContentWasRendered = false;

            if (ContentArea != Rect.Empty)
            {
                var Scale = Math.Min(DisplayZone.Width / ContentArea.Width, DisplayZone.Height / ContentArea.Height);

                var OffsetX = (DisplayZone.X + DisplayZone.Width / 2.0) - (ContentArea.Width * Scale) / 2.0;
                var OffsetY = (DisplayZone.Y + DisplayZone.Height / 2.0) - (ContentArea.Height * Scale) / 2.0;
                var Transformation = Display.CreateTransform((-ContentArea.Left * Scale) + OffsetX,
                                                             (-ContentArea.Top * Scale) + OffsetY,
                                                             Scale, Scale);
                CompositeGroup.ClipGeometry = new RectangleGeometry(new Rect(AvailableArea.X + 1, AvailableArea.Y + 1,
                                                                             AvailableArea.Width - 2, AvailableArea.Height - 2));

                using (var CompositeDrawing = CompositeGroup.Open())
                    ContentWasRendered = this.GenerateRenderedContent(CompositeDrawing, Transformation);
            }

            // Draw the background sheet if needed
            if (!WithTransparentBackground)
            {
                Context.DrawGeometry(Background.SubstituteIf(orig => orig.IsTransparent(), this.BackgroundWorkingBrush),
                                     null /*- new Pen(this.BackgroundWorkBrush, 1.0)*/, new RectangleGeometry(AvailableArea));

                var BackImage = this.BackgroundWorkingImage;
                if (BackImage != null)
                    Context.DrawGeometry(this.GetBackgroundImageBrush(), null, new RectangleGeometry(AvailableArea));
            }

            // Draw content
            Context.DrawDrawing(CompositeGroup);

            return (ContentArea != Rect.Empty && ContentWasRendered);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public Visual ToVisualSnapshot(double AvailableWidth = double.NaN, double AvailableHeight = double.NaN,
                                       bool WithTransparentBackground = false, Brush SnapshotBackground = null)
        {
            var Snapshot = ToSnapshot(WithTransparentBackground, AvailableWidth, AvailableHeight, SnapshotBackground);
            var Result = (Snapshot == null ? null : Snapshot.Item1.RenderToDrawingVisual());
            return Result;
        }

        public Tuple<DrawingGroup, Rect> ToSnapshot(bool WithTransparentBackground = false,
                                                    double AvailableWidth = double.NaN,
                                                    double AvailableHeight = double.NaN,
                                                    Brush SnapshotBackground = null)
        {
            var SourceArea = this.DetermineContentArea();
            if (SourceArea == Rect.Empty)
                return null;

            AvailableWidth = AvailableWidth.NaNDefault(SourceArea.Width);
            AvailableHeight = AvailableHeight.NaNDefault(SourceArea.Height);

            var SnapshotZone = new Rect(0, 0, AvailableWidth, AvailableHeight);

            var Snapshot = new DrawingGroup();
            bool Rendered = false;
            using (var Context = Snapshot.Open())
                Rendered = DrawContent(Context, SnapshotZone,
                                       WithTransparentBackground, SnapshotBackground);

            if (!Rendered)
                return null;

            var Result = Tuple.Create(Snapshot, new Rect(SourceArea.X, SourceArea.Y, SnapshotZone.Width, SnapshotZone.Height));
            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets this view as a printable fix document.
        /// </summary>
        public FixedDocument ToDocument(double PrintableAreaWidth, double PrintableAreaHeight,
                                        string HeaderText = null, string FooterText = null, bool ShowBorders = false,
                                        double MarginLeft = double.NaN, double MarginTop = double.NaN,
                                        double MarginRight = double.NaN, double MarginBottom = double.NaN)
        {
            MarginLeft = MarginLeft.NaNDefault(10.0);
            MarginTop = MarginTop.NaNDefault(10.0);
            MarginRight = MarginRight.NaNDefault(MarginLeft);
            MarginBottom = MarginBottom.NaNDefault(MarginTop);

            var Document = new FixedDocument();

            Document.DocumentPaginator.PageSize = new Size(PrintableAreaWidth,
                                                           PrintableAreaHeight);

            var ContentWidth = PrintableAreaWidth - (MarginLeft + MarginTop);
            var ContentHeight = PrintableAreaHeight - (MarginTop + MarginBottom);

            /*- if (AsLandscape)
            {
                ContentWidth = PrintableAreaHeight - (MarginTop + MarginBottom);
                ContentHeight = PrintableAreaWidth - (MarginLeft + MarginTop);
            } */

            FixedPage MainPage = new FixedPage();
            MainPage.Width = Document.DocumentPaginator.PageSize.Width;
            MainPage.Height = Document.DocumentPaginator.PageSize.Height;

            // Frame
            var MainFrame = new Border();
            if (ShowBorders)
            {
                MainFrame.BorderBrush = Brushes.Black;
                MainFrame.BorderThickness = new Thickness(PRINT_BORDER_THICKNESS);
            }
            MainFrame.Width = ContentWidth;
            MainFrame.Height = ContentHeight;

            var MainContainer = new DockPanel();

            // Header
            if (!HeaderText.IsAbsent())
            {
                var HeaderFrame = new Border();
                if (ShowBorders)
                {
                    HeaderFrame.BorderBrush = PrintFramesBrush;
                    HeaderFrame.BorderThickness = new Thickness(PRINT_BORDER_THICKNESS);
                }

                var HeaderContent = new TextBlock();
                HeaderContent.Text = HeaderText;
                HeaderContent.FontSize = 14;
                HeaderContent.FontWeight = FontWeights.Bold;
                HeaderContent.Margin = new Thickness(4);
                HeaderFrame.Child = HeaderContent;

                DockPanel.SetDock(HeaderFrame, Dock.Top);
                MainContainer.Children.Add(HeaderFrame);
            }

            // Footer
            if (!FooterText.IsAbsent())
            {
                var FooterFrame = new Border();
                if (ShowBorders)
                {
                    FooterFrame.BorderBrush = PrintFramesBrush;
                    FooterFrame.BorderThickness = new Thickness(PRINT_BORDER_THICKNESS);
                }

                var FooterContent = new TextBlock();
                FooterContent.Text = FooterText;
                FooterContent.FontSize = 12;
                FooterContent.FontWeight = FontWeights.Bold;
                FooterContent.Margin = new Thickness(2);
                FooterFrame.Child = FooterContent;

                DockPanel.SetDock(FooterFrame, Dock.Bottom);
                MainContainer.Children.Add(FooterFrame);
            }

            // Body
            var BodyFrame = new Border();
            if (ShowBorders)
            {
                BodyFrame.BorderBrush = PrintFramesBrush;
                BodyFrame.BorderThickness = new Thickness(1);
            }

            var BodyContent = new Image();
            BodyContent.Stretch = Stretch.Uniform;
            BodyContent.Margin = new Thickness(PRINT_CONTENT_MARGIN);
            BodyFrame.Child = BodyContent;

            MainContainer.Children.Add(BodyFrame);

            // Add the parts to a panel
            MainFrame.Child = MainContainer;

            /*- if (AsLandscape)
                MainFrame.LayoutTransform = new RotateTransform(-90); */

            // Add the page to the document
            var PrintingPanel = new Grid();

            MainPage.Children.Add(PrintingPanel);

            PrintingPanel.Margin = new Thickness(MarginLeft, MarginTop, MarginRight, MarginBottom);
            PrintingPanel.Children.Add(MainFrame);

            var Snapshot = this.ToSnapshot(true, BodyFrame.Width - (PRINT_CONTENT_MARGIN * 2.0), BodyFrame.Height - (PRINT_CONTENT_MARGIN * 2.0));
            BodyContent.Source = (Snapshot == null ? new DrawingImage() : Snapshot.Item1.ToDrawingImage());
            BodyFrame.Child = BodyContent;

            MainFrame.UpdateLayout();

            // Inject content into the printing page
            var MainPageContent = new PageContent();

            ((System.Windows.Markup.IAddChild)MainPageContent).AddChild(MainPage);
            Document.Pages.Add(MainPageContent);

            return Document;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public void UpdateVersion()
        {
            if (this.Version != null)
                this.Version.Update();

            if (this.OwnerCompositeContainer != null)
                this.OwnerCompositeContainer.UpdateVersion();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}
