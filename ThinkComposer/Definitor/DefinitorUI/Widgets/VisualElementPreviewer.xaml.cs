using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Instrumind.Common;
using Instrumind.Common.Visualization;
using Instrumind.Common.Visualization.Widgets;

using Instrumind.ThinkComposer.ApplicationProduct.Widgets;
using Instrumind.ThinkComposer.MetaModel;
using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;
using Instrumind.ThinkComposer.Model.VisualModel;

namespace Instrumind.ThinkComposer.Definitor.DefinitorUI.Widgets
{
    /// <summary>
    /// Interaction logic for VisualElementPreviewer.xaml
    /// </summary>
    public partial class VisualElementPreviewer : UserControl
    {
        public IdeaDefinition SourceIdeaDef { get; private set; }

        public RelationshipDefinition SourceRelationshipDef { get { return this.SourceIdeaDef as RelationshipDefinition; } }

        public EntityPropertyExpositor ExpoSymbolLineBrush { get; protected set; }
        public EntityPropertyExpositor ExpoSymbolLineThickness { get; protected set; }
        public EntityPropertyExpositor ExpoSymbolLineDash { get; protected set; }
        public EntityPropertyExpositor ExpoSymbolMainBackground { get; protected set; }

        public EntityPropertyExpositor ExpoConnectorsLineBrush { get; protected set; }
        public EntityPropertyExpositor ExpoConnectorsLineThickness { get; protected set; }
        public EntityPropertyExpositor ExpoConnectorsLineDash { get; protected set; }
        public EntityPropertyExpositor ExpoConnectorsMainBackground { get; protected set; }

        public VisualElementPreviewer(EntityPropertyExpositor ExpoSymbolLineBrush, EntityPropertyExpositor ExpoSymbolLineThickness,
                                      EntityPropertyExpositor ExpoSymbolLineDash, EntityPropertyExpositor ExpoSymbolMainBackground,
                                      EntityPropertyExpositor ExpoConnectorsLineBrush = null, EntityPropertyExpositor ExpoConnectorsLineThickness = null,
                                      EntityPropertyExpositor ExpoConnectorsLineDash = null, EntityPropertyExpositor ExpoConnectorsMainBackground = null)
        {
            InitializeComponent();

            this.ExpoSymbolLineBrush = ExpoSymbolLineBrush;
            this.ExpoSymbolLineThickness = ExpoSymbolLineThickness;
            this.ExpoSymbolLineDash = ExpoSymbolLineDash;
            this.ExpoSymbolMainBackground = ExpoSymbolMainBackground;

            this.ExpoConnectorsLineBrush = ExpoConnectorsLineBrush;
            this.ExpoConnectorsLineThickness = ExpoConnectorsLineThickness;
            this.ExpoConnectorsLineDash = ExpoConnectorsLineDash;
            this.ExpoConnectorsMainBackground = ExpoConnectorsMainBackground;

            this.CbxDetailsPoster.Checked += ShowPreview;
            this.CbxDetailsPoster.Unchecked += ShowPreview;

            this.LvsPredefStyle.MaxWidth = ((Domain.BaseStylesForegroundColors.Count / 2.0) * WidgetsHelper.COLOR_SAMPLE_SIZE.Width) + 36;
            this.LvsPredefStyle.LbxSelector.ItemsSource = WidgetsHelper.GenerateGraphicStyleSamples();
            this.LvsPredefStyle.LbxSelector.SelectionChanged += LbxSelector_SelectionChanged;

            this.PopupSelectPredefStyle.MaxWidth = this.LvsPredefStyle.MaxWidth;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.ShowPreview();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            this.DetachSource();
        }

        public void AttachSource(IdeaDefinition SourceIdeaDef)
        {
            this.DetachSource();

            if (SourceIdeaDef == null)
                return;

            this.SourceIdeaDef = SourceIdeaDef;

            this.SourceIdeaDef.PropertyChanged += ShowPreview;
            this.SourceIdeaDef.DefaultSymbolFormat.PropertyChanged += ShowPreview;

            if (this.SourceRelationshipDef != null)
            {
                this.SourceRelationshipDef.DefaultConnectorsFormat.PropertyChanged += ShowPreview;
                this.SourceRelationshipDef.DefaultConnectorsFormat.HeadPlugs.CollectionChanged += ShowPreview;
                this.SourceRelationshipDef.DefaultConnectorsFormat.TailPlugs.CollectionChanged += ShowPreview;

                this.SourceRelationshipDef.OriginOrParticipantLinkRoleDef.AllowedVariants.CollectionChanged += ShowPreview;
                this.SourceRelationshipDef.TargetLinkRoleDef.AllowedVariants.CollectionChanged += ShowPreview;
            }
            else
            {
                this.BrdOrigin.SetVisible(false);
                this.BrdTarget.SetVisible(false);
            }
        }

        public void DetachSource()
        {
            if (this.SourceIdeaDef == null)
                return;

            if (this.SourceRelationshipDef != null)
            {
                this.SourceRelationshipDef.DefaultConnectorsFormat.PropertyChanged -= ShowPreview;
                this.SourceRelationshipDef.DefaultConnectorsFormat.HeadPlugs.CollectionChanged -= ShowPreview;
                this.SourceRelationshipDef.DefaultConnectorsFormat.TailPlugs.CollectionChanged -= ShowPreview;

                this.SourceRelationshipDef.OriginOrParticipantLinkRoleDef.AllowedVariants.CollectionChanged -= ShowPreview;
                this.SourceRelationshipDef.TargetLinkRoleDef.AllowedVariants.CollectionChanged -= ShowPreview;
            }

            this.SourceIdeaDef.DefaultSymbolFormat.PropertyChanged -= ShowPreview;
            this.SourceIdeaDef.PropertyChanged -= ShowPreview;
        }

        public double SymbolWidth { get { return this.SourceIdeaDef.DefaultSymbolFormat.InitialWidth.SubstituteFor(0, 128)
                                                    .EnforceRange(VisualSymbolFormat.SYMBOL_MIN_INI_SIZE, VisualSymbolFormat.SYMBOL_MAX_INI_WIDTH); } }

        public double SymbolHeight { get { return this.SourceIdeaDef.DefaultSymbolFormat.InitialHeight.SubstituteFor(0, 64)
                                                    .EnforceRange(VisualSymbolFormat.SYMBOL_MIN_INI_SIZE, VisualSymbolFormat.SYMBOL_MAX_INI_WIDTH); } }

        public Point GetSymbolCenter() { return new Point((this.BrdPreviewContainer.ActualWidth -
                                                           (this.BrdPreviewContainer.Padding.Left + this.BrdPreviewContainer.Padding.Right)) / 2.0,
                                                          this.SymbolHeight / 2.0); }

        private Drawing DrawCentralSymbol()
        {
            var MaxDetPosterHeight = (this.BrdPreviewContainer.ActualHeight
                                      - (this.BrdPreviewContainer.Padding.Top +
                                         this.BrdPreviewContainer.Padding.Bottom)) - this.SymbolHeight;

            var LocalSymbolWidth = SymbolWidth;

            // Adjust of the WPF-Bug which incorrectly draws non-solid dash-styles of non-filled geometries.
            if (this.SourceIdeaDef.DefaultSymbolFormat.LineDash != DashStyles.Solid &&
                this.SourceIdeaDef.RepresentativeShape.IsOneOf(Shapes.XMark, Shapes.StraightParallelLines, Shapes.StraightUnderLine,
                                                               Shapes.BracketsCurly, Shapes.BracketsCurved, Shapes.BracketsSquare))
                LocalSymbolWidth = LocalSymbolWidth * 1.5;

            var SymDraw = MasterDrawer.CreateDrawingSymbol(this.SourceIdeaDef.RepresentativeShape,
                                                           this.SourceIdeaDef.DefaultPictogram,
                                                           this.SourceIdeaDef.DefaultPictogram,
                                                           this.SourceIdeaDef.DefaultSymbolFormat.LineBrush,
                                                           this.SourceIdeaDef.DefaultSymbolFormat.LineThickness,
                                                           this.SourceIdeaDef.DefaultSymbolFormat.LineDash,
                                                           this.SourceIdeaDef.DefaultSymbolFormat.LineJoin,
                                                           this.SourceIdeaDef.DefaultSymbolFormat.LineCap,
                                                           this.SourceIdeaDef.DefaultSymbolFormat.UsePictogramAsSymbol,
                                                           this.SourceIdeaDef.DefaultSymbolFormat.MainBackground,
                                                           this.SourceIdeaDef.DefaultSymbolFormat.DetailsPosterIsHanging,
                                                           this.SourceIdeaDef.DefaultSymbolFormat.Opacity,
                                                           this.GetSymbolCenter(), LocalSymbolWidth, SymbolHeight,
                                                           (this.CbxDetailsPoster.IsChecked.IsTrue()
                                                            && (LocalSymbolWidth >= VisualSymbol.MIN_SYMBOL_DISPLAY_SIZE.Width
                                                                && MaxDetPosterHeight >= VisualSymbol.MIN_SYMBOL_DISPLAY_SIZE.Height)
                                                            ? VisualSymbol.INITIAL_DETAILS_POSTER_HEIGHT
                                                                .EnforceMaximum(MaxDetPosterHeight)
                                                                    .EnforceMinimum(VisualSymbol.MIN_SYMBOL_DISPLAY_SIZE.Height * 2.0)
                                                                     .SubstituteFor(VisualSymbol.MIN_SYMBOL_DISPLAY_SIZE.Height * 2.0, 0.0)
                                                            : 0.0),
                                                           this.SourceIdeaDef.DefaultSymbolFormat.AsMultiple,
                                                           this.SourceIdeaDef.DefaultSymbolFormat.InitiallyFlippedHorizontally,
                                                           this.SourceIdeaDef.DefaultSymbolFormat.InitiallyFlippedVertically,
                                                           this.SourceIdeaDef.DefaultSymbolFormat.InitiallyTilted);

            var WorkingPictogram = (this.SourceIdeaDef.DefaultSymbolFormat.UseDefinitorPictogramAsNullDefault
                                    ? this.SourceIdeaDef.Pictogram
                                    : Display.GetAppImage("page_white.png"));

            var TitleFormat = this.SourceIdeaDef.DefaultSymbolFormat.GetTextFormat(ETextPurpose.Title);
            MasterDrawer.GenerateHeadingContent(SymDraw.Item1, SymDraw.Item2,
                                                WorkingPictogram, this.SourceIdeaDef.DefaultSymbolFormat.UsePictogramAsSymbol, this.SourceIdeaDef.DefaultSymbolFormat.PictogramVisualDisposition,
                                                this.SourceIdeaDef.DefaultSymbolFormat.GetTextFormat(ETextPurpose.Title), this.SourceIdeaDef.DefaultSymbolFormat.GetTextFormat(ETextPurpose.Subtitle),
                                                this.SourceIdeaDef.DefaultSymbolFormat.SubtitleVisualDisposition, this.SourceIdeaDef.DefaultSymbolFormat.UseNameAsMainTitle,
                                                this.SourceIdeaDef.Name, this.SourceIdeaDef.TechName);

            if (SymDraw.Item3 != Rect.Empty)
                using (var Context = SymDraw.Item1.Append())
                {
                    var TargetArea = new Rect(SymDraw.Item3.X, SymDraw.Item3.Y + VisualSymbol.DETS_MARGIN_SIZE,
                                              SymDraw.Item3.Width, (SymDraw.Item3.Height - VisualSymbol.DETS_MARGIN_SIZE));

                    if (TargetArea.IsValid() && TargetArea.Height > 0 && TargetArea.Width > 0)
                    {
                        var TitleDesignateArea = VisualSymbol.DrawDetailTitle(Context, "Detail Heading", true,
                                                                              this.SourceIdeaDef.DefaultSymbolFormat.GetTextFormat(ETextPurpose.DetailHeading),
                                                                              TargetArea, VisualSymbol.DETS_PADDING_SIZE,
                                                                              this.SourceIdeaDef.DefaultSymbolFormat.DetailHeadingBackground,
                                                                              this.SourceIdeaDef.DefaultSymbolFormat.DetailHeadingForeground);

                        TargetArea = new Rect(SymDraw.Item3.X, SymDraw.Item3.Y + TitleDesignateArea.Height + VisualSymbol.DETS_MARGIN_SIZE,
                                              SymDraw.Item3.Width, SymDraw.Item3.Height - (TitleDesignateArea.Height + VisualSymbol.DETS_MARGIN_SIZE));

                        if (TargetArea.IsValid() && TargetArea.Height > 0 && TargetArea.Width > 0)
                        {
                            var LabelArea = MasterDrawer.DrawTextLabel(Context, "Detail Caption",
                                                                       this.SourceIdeaDef.DefaultSymbolFormat.GetTextFormat(ETextPurpose.DetailCaption),
                                                                       TargetArea, this.SourceIdeaDef.DefaultSymbolFormat.DetailCaptionBackground,
                                                                       new Pen(this.SourceIdeaDef.DefaultSymbolFormat.DetailCaptionForeground, 1.0),
                                                                       Padding.Top, Padding.Left, Padding.Right);

                            TargetArea = new Rect(SymDraw.Item3.X, SymDraw.Item3.Y + (TitleDesignateArea.Height + VisualSymbol.DETS_MARGIN_SIZE + LabelArea.Height),
                                                  SymDraw.Item3.Width, SymDraw.Item3.Height - (TitleDesignateArea.Height + VisualSymbol.DETS_MARGIN_SIZE + LabelArea.Height));

                            if (TargetArea.IsValid() && TargetArea.Height > 0 && TargetArea.Width > 0)
                                MasterDrawer.DrawTextLabel(Context, "Detail Content",
                                                           this.SourceIdeaDef.DefaultSymbolFormat.GetTextFormat(ETextPurpose.DetailContent),
                                                           TargetArea, this.SourceIdeaDef.DefaultSymbolFormat.DetailContentBackground,
                                                           new Pen(this.SourceIdeaDef.DefaultSymbolFormat.DetailContentForeground, 1.0),
                                                           Padding.Top, Padding.Left, Padding.Right, false);
                        }
                    }
                }

            var Result = SymDraw.Item1;

            // Compensates adjust of the WPF-Bug which incorrectly draws non-solid dash-styles of non-filled geometries.
            if (LocalSymbolWidth != SymbolWidth)
            {
                var CompensatedResult = new DrawingGroup();
                CompensatedResult.Children.Add(Result);
                CompensatedResult.Transform = new TranslateTransform(-(LocalSymbolWidth / 1.5), 0);
                Result = CompensatedResult;
            }
            
            return Result;
        }

        public void ShowPreview(object Sender = null, EventArgs Args = null)
        {
            if (this.SourceIdeaDef == null)
                return;

            try
            {
                if (this.SourceRelationshipDef != null && this.SourceRelationshipDef.IsDirectional)
                {
                    this.TxbOrigin.Text = "Origin";
                    this.TxbTarget.Text = "Target";
                }
                else
                {
                    this.TxbOrigin.Text = "Participant";
                    this.TxbTarget.Text = "Participant";
                }

                var Result = new DrawingGroup();

                if (this.SourceRelationshipDef == null)
                    Result.Children.Add(DrawCentralSymbol());
                else
                {
                    var CentralSymbolCenter = this.GetSymbolCenter();
                    // Discarded: From right to left, as the palette and drop-into-view samples
                    // From left to right, as the tabs in the Idea-Definition editing window.
                    var PreviewOrigin = new Point(this.BrdPreviewContainer.Padding.Left, CentralSymbolCenter.Y);
                    var PreviewTarget = new Point(this.BrdPreviewContainer.ActualWidth - (this.BrdPreviewContainer.Padding.Left +
                                                                                          this.BrdPreviewContainer.Padding.Right),
                                                  CentralSymbolCenter.Y);

                    var OriginRoleLink = this.SourceRelationshipDef.GetLinkForRole(ERoleType.Origin);
                    var TargetRoleLink = this.SourceRelationshipDef.GetLinkForRole(ERoleType.Target);

                    var LineBrush = this.SourceRelationshipDef.DefaultConnectorsFormat.LineBrush;
                    var LineThickness = this.SourceRelationshipDef.DefaultConnectorsFormat.LineThickness;
                    var LineDash = this.SourceRelationshipDef.DefaultConnectorsFormat.LineDash;
                    var LineJoin = this.SourceRelationshipDef.DefaultConnectorsFormat.LineJoin;

                    var OriginVariant = this.SourceRelationshipDef.OriginOrParticipantLinkRoleDef.AllowedVariants.FirstOrDefault();
                    var TailPlug = this.SourceRelationshipDef.DefaultConnectorsFormat.TailPlugs.GetMatchingOrFirst((key, value) => key.TechName == OriginVariant.TechName);
                    var TargetVariant = this.SourceRelationshipDef.TargetLinkRoleDef.NullDefault(this.SourceRelationshipDef.OriginOrParticipantLinkRoleDef).AllowedVariants.FirstOrDefault();
                    var HeadPlug = this.SourceRelationshipDef.DefaultConnectorsFormat.HeadPlugs.GetMatchingOrFirst((key, value) => key.TechName == TargetVariant.TechName);

                    var LabelOriginPos = new Point((PreviewOrigin.X + CentralSymbolCenter.X) / 2.0,
                                                   (PreviewOrigin.Y + CentralSymbolCenter.Y) / 2.0);

                    var LabelTargetPos = new Point((CentralSymbolCenter.X + PreviewTarget.X) / 2.0,
                                                   (CentralSymbolCenter.Y + PreviewTarget.Y) / 2.0);

                    if (this.SourceRelationshipDef.IsSimple
                        && this.SourceRelationshipDef.HideCentralSymbolWhenSimple)
                    {
                        var UniqueConnector = MasterDrawer.CreateDrawingConnector(TailPlug,
                                                                                  (this.SourceRelationshipDef.IsDirectional ? HeadPlug : TailPlug),
                                                                                  this.SourceRelationshipDef.DefaultConnectorsFormat.LineBrush,
                                                                                  this.SourceRelationshipDef.DefaultConnectorsFormat.LineThickness,
                                                                                  this.SourceRelationshipDef.DefaultConnectorsFormat.LineDash,
                                                                                  this.SourceRelationshipDef.DefaultConnectorsFormat.LineJoin,
                                                                                  this.SourceRelationshipDef.DefaultConnectorsFormat.LineCap,
                                                                                  this.SourceRelationshipDef.DefaultConnectorsFormat.PathStyle,
                                                                                  this.SourceRelationshipDef.DefaultConnectorsFormat.PathCorner,
                                                                                  this.SourceRelationshipDef.DefaultConnectorsFormat.MainBackground,
                                                                                  this.SourceRelationshipDef.DefaultConnectorsFormat.Opacity,
                                                                                  PreviewTarget, PreviewOrigin, null,
                                                                                  VisualConnector.VISUAL_MAGNITUDE_ADJUSTMENT);

                        Result.Children.Add(UniqueConnector);
                    }
                    else
                    {
                        Result.Children.Add(DrawCentralSymbol());

                        var CentralSymbolTarget = new Point(CentralSymbolCenter.X - SymbolWidth / 2.0, CentralSymbolCenter.Y);
                        var CentralSymbolOrigin = new Point(CentralSymbolCenter.X + SymbolWidth / 2.0, CentralSymbolCenter.Y);

                        LabelOriginPos = new Point((PreviewOrigin.X + CentralSymbolTarget.X) / 2.0,
                                                   (PreviewOrigin.Y + CentralSymbolTarget.Y) / 2.0);
                        LabelTargetPos = new Point((CentralSymbolOrigin.X + PreviewTarget.X) / 2.0,
                                                   (CentralSymbolOrigin.Y + PreviewTarget.Y) / 2.0);

                        var OriginConnector = MasterDrawer.CreateDrawingConnector(TailPlug, Plugs.None,
                                                                                  this.SourceRelationshipDef.DefaultConnectorsFormat.LineBrush,
                                                                                  this.SourceRelationshipDef.DefaultConnectorsFormat.LineThickness,
                                                                                  this.SourceRelationshipDef.DefaultConnectorsFormat.LineDash,
                                                                                  this.SourceRelationshipDef.DefaultConnectorsFormat.LineJoin,
                                                                                  this.SourceRelationshipDef.DefaultConnectorsFormat.LineCap,
                                                                                  this.SourceRelationshipDef.DefaultConnectorsFormat.PathStyle,
                                                                                  this.SourceRelationshipDef.DefaultConnectorsFormat.PathCorner,
                                                                                  this.SourceRelationshipDef.DefaultConnectorsFormat.MainBackground,
                                                                                  this.SourceRelationshipDef.DefaultConnectorsFormat.Opacity,
                                                                                  CentralSymbolTarget, PreviewOrigin, null,
                                                                                  VisualConnector.VISUAL_MAGNITUDE_ADJUSTMENT);

                        var TargetConnector = MasterDrawer.CreateDrawingConnector(Plugs.None,
                                                                                  (this.SourceRelationshipDef.IsDirectional ? HeadPlug : TailPlug),
                                                                                  this.SourceRelationshipDef.DefaultConnectorsFormat.LineBrush,
                                                                                  this.SourceRelationshipDef.DefaultConnectorsFormat.LineThickness,
                                                                                  this.SourceRelationshipDef.DefaultConnectorsFormat.LineDash,
                                                                                  this.SourceRelationshipDef.DefaultConnectorsFormat.LineJoin,
                                                                                  this.SourceRelationshipDef.DefaultConnectorsFormat.LineCap,
                                                                                  this.SourceRelationshipDef.DefaultConnectorsFormat.PathStyle,
                                                                                  this.SourceRelationshipDef.DefaultConnectorsFormat.PathCorner,
                                                                                  this.SourceRelationshipDef.DefaultConnectorsFormat.MainBackground,
                                                                                  this.SourceRelationshipDef.DefaultConnectorsFormat.Opacity,
                                                                                  PreviewTarget, CentralSymbolOrigin, null,
                                                                                  VisualConnector.VISUAL_MAGNITUDE_ADJUSTMENT);

                        Result.Children.Insert(0, OriginConnector);
                        Result.Children.Insert(0, TargetConnector);
                    }

                    using (var Context = Result.Append())
                    {
                        var LinkDescriptorLabel = (this.SourceRelationshipDef.DefaultConnectorsFormat.LabelLinkDescriptor
                                                   ? "Descriptor" : null);

                        var LinkDefinitorLabel = (this.SourceRelationshipDef.DefaultConnectorsFormat.LabelLinkDefinitor
                                                  ? "Definitor" : null);

                        var LinkRoleVariantLabel = (this.SourceRelationshipDef.DefaultConnectorsFormat.LabelLinkVariant
                                                    ? "Variant" : null);

                        MasterDrawer.PutConnectorLabeling(Context, this.SourceRelationshipDef, LabelOriginPos,
                                        this.SourceRelationshipDef.DefaultSymbolFormat.GetTextFormat(ETextPurpose.Extra),
                                        this.SourceRelationshipDef.DefaultConnectorsFormat.MainBackground,
                                        this.SourceRelationshipDef.DefaultConnectorsFormat.LineBrush,
                                        (LinkDescriptorLabel == null ? null : this.TxbOrigin.Text + "-" + LinkDescriptorLabel),
                                        LinkDefinitorLabel, LinkRoleVariantLabel);

                        MasterDrawer.PutConnectorLabeling(Context, this.SourceRelationshipDef, LabelTargetPos,
                                                          this.SourceRelationshipDef.DefaultSymbolFormat.GetTextFormat(ETextPurpose.Extra),
                                                          this.SourceRelationshipDef.DefaultConnectorsFormat.MainBackground,
                                                          this.SourceRelationshipDef.DefaultConnectorsFormat.LineBrush,
                                                          (LinkDescriptorLabel == null ? null : this.TxbTarget.Text + "-" + LinkDescriptorLabel),
                                                          LinkDefinitorLabel, LinkRoleVariantLabel);
                    }
                }

                this.ImgPreview.Source = Result.ToDrawingImage();
            }
            catch (Exception Problem)
            {
                // Console.WriteLine("Cannot show preview.");
            }
        }

        private void BtnSelectPredefStyle_Click(object sender, RoutedEventArgs e)
        {
            this.PopupSelectPredefStyle.IsOpen = !this.PopupSelectPredefStyle.IsOpen;
        }

        void LbxSelector_SelectionChanged(object sender, SelectionChangedEventArgs selargs)
        {
            var Param = (selargs.AddedItems != null && selargs.AddedItems.Count > 0
                            ? selargs.AddedItems[0] as FrameworkElement : null);

            // Contains: line-foreground, line-thickness, line-dash-style, shape-background
            var Style = (Param == null ? null : Param.Tag as Tuple<Brush, double, DashStyle, Brush>);

            if (Style != null)
            {
                // IMPORTANT: Notice the way of writing the value, depending of whether the expositors are visible or not.
                //            This is required because they're not automatically refreshed when the entity is changed.
                if (this.ExpoSymbolLineBrush.IsVisible)
                {
                    this.ExpoSymbolLineBrush.SetEditedValue(Style.Item1);

                    var Thick = MasterDrawer.AvailableThicknesses.First(
                                    th => th.TechName == Style.Item2.ToString(CultureInfo.InvariantCulture.NumberFormat));
                    this.ExpoSymbolLineThickness.SetEditedValue(Thick);

                    var Dash = MasterDrawer.AvailableDashStyles.First(
                                ds => ds.TechName == Display.DeclaredDashStyles.First(
                                        reg => reg.Item1 == Style.Item3).Item2);
                    this.ExpoSymbolLineDash.SetEditedValue(Dash);

                    this.ExpoSymbolMainBackground.SetEditedValue(Style.Item4);
                }
                else
                {
                    this.SourceIdeaDef.DefaultSymbolFormat.LineBrush = Style.Item1;

                    this.SourceIdeaDef.DefaultSymbolFormat.LineThickness = Style.Item2;

                    this.SourceIdeaDef.DefaultSymbolFormat.LineDash = Style.Item3;

                    this.SourceIdeaDef.DefaultSymbolFormat.MainBackground = Style.Item4;
                }

                var SourceRelDef = this.SourceIdeaDef as RelationshipDefinition;

                if (SourceRelDef != null && this.ExpoConnectorsLineBrush != null && this.ExpoConnectorsLineDash != null
                    && this.ExpoConnectorsLineThickness != null && this.ExpoConnectorsMainBackground != null)
                {
                    // IMPORTANT: Notice the way of writing the value, depending of whether the expositors are visible or not.
                    //            This is required because they're not automatically refreshed when the entity is changed.
                    if (this.ExpoConnectorsLineBrush.IsVisible)
                    {
                        this.ExpoConnectorsLineBrush.SetEditedValue(Style.Item1);

                        var Thick = MasterDrawer.AvailableThicknesses.First(
                                        th => th.TechName == Style.Item2.ToString(CultureInfo.InvariantCulture.NumberFormat));
                        this.ExpoConnectorsLineThickness.SetEditedValue(Thick);

                        var Dash = MasterDrawer.AvailableDashStyles.First(
                                    ds => ds.TechName == Display.DeclaredDashStyles.First(
                                            reg => reg.Item1 == Style.Item3).Item2);
                        this.ExpoConnectorsLineDash.SetEditedValue(Dash);

                        this.ExpoConnectorsMainBackground.SetEditedValue(Style.Item4);
                    }
                    else
                    {
                        SourceRelDef.DefaultConnectorsFormat.LineBrush = Style.Item1;

                        SourceRelDef.DefaultConnectorsFormat.LineThickness = Style.Item2;

                        SourceRelDef.DefaultConnectorsFormat.LineDash = Style.Item3;

                        SourceRelDef.DefaultConnectorsFormat.MainBackground = Style.Item4;
                    }
                }
            }

            this.PopupSelectPredefStyle.IsOpen = false;
        }

        private void PopupSelectPredefStyle_MouseLeave(object sender, MouseEventArgs e)
        {
            this.PopupSelectPredefStyle.IsOpen = false;
        }
    }
}
