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
// File   : VisualComplement.cs
// Object : Instrumind.ThinkComposer.Model.VisualModel.VisualComplement (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2011.08.14 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.Composer;
using Instrumind.ThinkComposer.Definitor;
using Instrumind.ThinkComposer.Definitor.DefinitorUI;
using Instrumind.ThinkComposer.MetaModel;
using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;

/// Base abstractions for the visual representation of Graph entities
namespace Instrumind.ThinkComposer.Model.VisualModel
{
    /// <summary>
    /// Individual visual object exposing attached information, such as note, callout, legend and info-card.
    /// </summary>
    public partial class VisualComplement : VisualObject, IModelEntity, IModelClass<VisualComplement>
    {
        public static DrawingGroup GenerateLegend(VisualComplement Complement, View Target)
        {
            var Result = new DrawingGroup();

            var LabelCaptionFormat = new TextFormat("Arial", 9, Brushes.Black, false, false, false, TextAlignment.Right);
            var LabelContentFormat = new TextFormat("Arial", 9, Brushes.Black, true, false, false, TextAlignment.Left);

            var Foreground = Complement.GetPropertyField<Brush>(PROP_FIELD_FOREGROUND);
            var Background = Complement.GetPropertyField<Brush>(PROP_FIELD_BACKGROUND);
            var Pencil = new Pen(Foreground, Complement.GetPropertyField<double>(PROP_FIELD_LINETHICK));
            Pencil.DashStyle = Complement.GetPropertyField<DashStyle>(PROP_FIELD_LINEDASH);

            var Shape = new RectangleGeometry(Complement.BaseArea, 5, 5);

            double VerticalOffset = Complement.BaseArea.Top;
            double Margin = 2.0;
            double VerticalTab = 60;
            double LabelHeight = 18;

            using (var Context = Result.Append())
            {
                Context.DrawGeometry(Background, Pencil, Shape);

                if (Complement.BaseArea.Width <= VerticalTab)
                {
                    DrawLabel(Context, "...", LabelContentFormat, Complement.BaseArea.Left, Complement.BaseArea.Right, VerticalOffset - 2.0, LabelHeight, Margin);
                    goto Exit;
                }

                // Text "Domain"
                DrawLabel(Context, "Domain:", LabelCaptionFormat, Complement.BaseArea.Left, Complement.BaseArea.Left + VerticalTab, VerticalOffset, LabelHeight, Margin);
                DrawLabel(Context, Complement.Target.OwnerGlobal.OwnerCompositeContainer.CompositeContentDomain.Name, LabelContentFormat,
                          Complement.BaseArea.Left + VerticalTab, Complement.BaseArea.Right, VerticalOffset, LabelHeight, Margin);

                // Text "Summary"
                var SummaryHeight = LabelHeight;

                if (!Complement.Target.OwnerGlobal.OwnerCompositeContainer.CompositeContentDomain.Summary.Trim().IsAbsent())
                {
                    SummaryHeight = LabelHeight * 2.65;
                    VerticalOffset += LabelHeight;
                    if (VerticalOffset + SummaryHeight >= Complement.BaseArea.Bottom)
                    {
                        Context.DrawGeometry(Background, Pencil, new LineGeometry(new Point(Complement.BaseArea.Left + VerticalTab, Complement.BaseArea.Top),
                                                                 new Point(Complement.BaseArea.Left + VerticalTab, Complement.BaseArea.Bottom)));
                        goto Exit;
                    }
                    Context.DrawGeometry(Background, Pencil, new LineGeometry(new Point(Complement.BaseArea.Left, VerticalOffset), new Point(Complement.BaseArea.Right, VerticalOffset)));
                    DrawLabel(Context, "Summary:", LabelCaptionFormat, Complement.BaseArea.Left, Complement.BaseArea.Left + VerticalTab, VerticalOffset, SummaryHeight, Margin);
                    DrawLabel(Context, Complement.Target.OwnerGlobal.OwnerCompositeContainer.CompositeContentDomain.Summary, LabelContentFormat,
                              Complement.BaseArea.Left + VerticalTab, Complement.BaseArea.Right, VerticalOffset, SummaryHeight, Margin);
                }

                VerticalOffset += SummaryHeight;

                var CalcHeight = VerticalOffset;

                if (VerticalOffset < Complement.BaseArea.Bottom)
                    Context.DrawGeometry(Background, Pencil, new LineGeometry(new Point(Complement.BaseArea.Left, VerticalOffset), new Point(Complement.BaseArea.Right, VerticalOffset)));
                else
                    CalcHeight = Complement.BaseArea.Bottom;

                Context.DrawGeometry(Background, Pencil, new LineGeometry(new Point(Complement.BaseArea.Left + VerticalTab, Complement.BaseArea.Top),
                                                         new Point(Complement.BaseArea.Left + VerticalTab, CalcHeight)));

                // Legends zone
                if (VerticalOffset + SummaryHeight + Display.ICOSIZE_LIT >= Complement.BaseArea.Bottom) goto Exit;

                var UsedIdeaDefinitions = Complement.Target.OwnerGlobal.ViewChildren
                                                .Where(child => child.Key is VisualElement)
                                                    .Select(child => ((VisualElement)child.Key).OwnerRepresentation.RepresentedIdea.IdeaDefinitor)
                                                        .Distinct().OrderBy(idef => idef.Name);
                
                var UsedConHeight = DrawColumn(Context, "Concepts", Pencil,
                                               new Rect(Complement.BaseArea.Left, VerticalOffset,
                                                        Complement.BaseArea.Width / 2.0, Complement.BaseArea.Bottom - VerticalOffset),
                                               UsedIdeaDefinitions.Where(idea => idea is ConceptDefinition));

                var UsedRelHeight = DrawColumn(Context, "Relationships", Pencil,
                                               new Rect(Complement.BaseArea.Left + Complement.BaseArea.Width / 2.0, VerticalOffset,
                                                        Complement.BaseArea.Width / 2.0, Complement.BaseArea.Bottom - VerticalOffset),
                                               GetRelationshipDefsAndVariations(UsedIdeaDefinitions.CastAs<RelationshipDefinition,IdeaDefinition>(), Pencil));

                var UsedHeight = Math.Max(UsedConHeight, UsedRelHeight) + 1;

                var MaxHeight = (VerticalOffset + UsedHeight).EnforceMaximum(Complement.BaseArea.Bottom);
                Context.DrawGeometry(Background, Pencil, new LineGeometry(new Point(Complement.BaseArea.Left + Complement.BaseArea.Width / 2.0, VerticalOffset),
                                                                          new Point(Complement.BaseArea.Left + Complement.BaseArea.Width / 2.0, MaxHeight)));

                Context.DrawGeometry(Background, Pencil, new LineGeometry(new Point(Complement.BaseArea.Left, MaxHeight), new Point(Complement.BaseArea.Right, MaxHeight)));

                VerticalOffset += UsedHeight;

                // Versioning zone
                var VersionInfo = Complement.Target.OwnerGlobal.OwnerCompositeContainer.CompositeContentDomain.Version;

                var VerticalTab2Ini = 150;
                var VerticalTab2End = 210;

                // Text "Creator"
                if (VerticalOffset + LabelHeight >= Complement.BaseArea.Bottom) goto Exit;
                var LocalZoneTop = VerticalOffset;

                Context.DrawGeometry(Background, Pencil, new LineGeometry(new Point(Complement.BaseArea.Left + VerticalTab, LocalZoneTop),
                                                                          new Point(Complement.BaseArea.Left + VerticalTab, Complement.BaseArea.Bottom)));

                DrawLabel(Context, "Creator:", LabelCaptionFormat, Complement.BaseArea.Left, Complement.BaseArea.Left + VerticalTab, VerticalOffset, LabelHeight, Margin);
                DrawLabel(Context, VersionInfo.Creator, LabelContentFormat,
                          Complement.BaseArea.Left + VerticalTab, Complement.BaseArea.Left + VerticalTab2Ini, VerticalOffset, LabelHeight, Margin);

                // Text "Last Modifier"
                VerticalOffset += LabelHeight; if (VerticalOffset + LabelHeight < Complement.BaseArea.Bottom)
                {
                    Context.DrawGeometry(Background, Pencil, new LineGeometry(new Point(Complement.BaseArea.Left, VerticalOffset), new Point(Complement.BaseArea.Right, VerticalOffset)));
                    DrawLabel(Context, "Last Modifier:", LabelCaptionFormat, Complement.BaseArea.Left, Complement.BaseArea.Left + VerticalTab, VerticalOffset, LabelHeight, Margin);
                    DrawLabel(Context, VersionInfo.LastModifier, LabelContentFormat,
                              Complement.BaseArea.Left + VerticalTab, Complement.BaseArea.Left + VerticalTab2Ini, VerticalOffset, LabelHeight, Margin);
                }

                if (Complement.BaseArea.Right - (Complement.BaseArea.Left + VerticalTab2End) >= 10)
                {
                    Context.DrawGeometry(Background, Pencil, new LineGeometry(new Point(Complement.BaseArea.Left + VerticalTab2Ini, LocalZoneTop),
                                                                              new Point(Complement.BaseArea.Left + VerticalTab2Ini, Complement.BaseArea.Bottom)));
                    Context.DrawGeometry(Background, Pencil, new LineGeometry(new Point(Complement.BaseArea.Left + VerticalTab2End, LocalZoneTop),
                                                                              new Point(Complement.BaseArea.Left + VerticalTab2End, Complement.BaseArea.Bottom)));

                    // Text "Creation"
                    VerticalOffset = LocalZoneTop;
                    DrawLabel(Context, "Creation:", LabelCaptionFormat, Complement.BaseArea.Left + VerticalTab2Ini, Complement.BaseArea.Left + VerticalTab2End, VerticalOffset, LabelHeight, Margin);
                    DrawLabel(Context, VersionInfo.Creation.ToString(), LabelContentFormat,
                              Complement.BaseArea.Left + VerticalTab2End, Complement.BaseArea.Right, VerticalOffset, LabelHeight, Margin);

                    // Text "Last Modification"
                    VerticalOffset += LabelHeight; if (VerticalOffset + LabelHeight >= Complement.BaseArea.Bottom) goto Exit;
                    //Supressed: Context.DrawGeometry(Background, Pencil, new LineGeometry(new Point(Complement.BaseArea.Left, BaseLineOffset), new Point(Complement.BaseArea.Right, BaseLineOffset)));
                    DrawLabel(Context, "Modification:", LabelCaptionFormat, Complement.BaseArea.Left + VerticalTab2Ini, Complement.BaseArea.Left + VerticalTab2End, VerticalOffset, LabelHeight, Margin);
                    DrawLabel(Context, VersionInfo.LastModification.ToString(), LabelContentFormat,
                              Complement.BaseArea.Left + VerticalTab2End, Complement.BaseArea.Right, VerticalOffset, LabelHeight, Margin);
                }

            Exit: ;
            }

            return Result;
        }

        public static IEnumerable<IRecognizableElement> GetRelationshipDefsAndVariations(IEnumerable<RelationshipDefinition> RelationshipDefs,
                                                                                         Pen Pencil, bool GroupByPlug = true)
        {
            var Result = new List<IRecognizableElement>();

            Pencil = new Pen(Pencil.Brush, 0.5);

            foreach (var RelDef in RelationshipDefs)
            {
                Result.Add(RelDef);

                var CountOfOriginVariants = RelDef.OriginOrParticipantLinkRoleDef.AllowedVariants.Count;
                var CountOfTargetVariants = (RelDef.TargetLinkRoleDef == null ? 0 : RelDef.TargetLinkRoleDef.AllowedVariants.Count);

                if (CountOfOriginVariants > 1 || (CountOfOriginVariants == 1 && CountOfTargetVariants > 1))
                    CreateVariantsRepresentations(Result, "Origin",
                                                  RelDef.OriginOrParticipantLinkRoleDef.AllowedVariants,
                                                  RelDef.DefaultConnectorsFormat.TailPlugs, Pencil, GroupByPlug);

                if (CountOfTargetVariants > 1 || (CountOfTargetVariants == 1 && CountOfOriginVariants > 1))
                    CreateVariantsRepresentations(Result, "Target",
                                                  RelDef.TargetLinkRoleDef.AllowedVariants,
                                                  RelDef.DefaultConnectorsFormat.HeadPlugs, Pencil, GroupByPlug);
            }

            return Result;
        }

        private static void CreateVariantsRepresentations(List<IRecognizableElement> Result, string NamePrefix,
                                                          IEnumerable<SimplePresentationElement> AllowedVariants,
                                                          IDictionary<SimplePresentationElement, string> Plugs,
                                                          Pen Pencil, bool GroupByPlug)
        {
            var VarReps = AllowedVariants.Select(avar => GetRelDefVariantRepresentation(NamePrefix, avar, Plugs, Pencil));

            if (GroupByPlug)
            {
                var PlugGroups = VarReps.GroupBy(vr => vr.Item2);
                foreach (var PlugGroup in PlugGroups)
                {
                    var VarRep = PlugGroup.First().Item1;
                    if (PlugGroup.CountsAtLeast(2))
                    {
                        PlugGroup.Skip(1).ForEach(vr =>
                        {
                            VarRep.Name = VarRep.Name + "; " + vr.Item1.Name.CutBetween("[" + NamePrefix + "]", null, true);
                            VarRep.TechName = VarRep.TechName + "__" + vr.Item1.TechName;
                        });
                        VarRep.Summary = "";
                        Result.Add(VarRep);
                    }
                    else
                        Result.Add(VarRep);
                }
            }
            else
                Result.AddRange(VarReps.Select(vr => vr.Item1));
        }

        public static Tuple<SimplePresentationElement, string> GetRelDefVariantRepresentation(string NamePrefix, IIdentifiableElement RelDefVariant,
                                                                                              IDictionary<SimplePresentationElement, string> PlugDefs, Pen Pencil)
        {
            var PlugCode = PlugDefs.GetMatchingOrFirst((key, value) => key.TechName == RelDefVariant.TechName);
            var Result = Tuple.Create(new SimplePresentationElement("[" + NamePrefix + "] " + RelDefVariant.Name,
                                                                    RelDefVariant.TechName, RelDefVariant.Summary),
                                      PlugCode);

            var AreaWidth = MasterDrawer.BASE_WIDTH;
            var TargetPosition = MasterDrawer.SAMPLE_CONN_TARGET;
            var SourcePosition = new Point(TargetPosition.X + AreaWidth, TargetPosition.Y);

            var ShowEmptyPlugLine = (PlugCode == Plugs.None);
            var PlugDraw = PlugDrawer.CreatePlug(PlugCode, Pencil, TargetPosition, SourcePosition, 0.75,
                                                 ShowEmptyPlugLine, ShowEmptyPlugLine).ToDrawingImage();

            Result.Item1.Pictogram = PlugDraw;

            return Result;
        }

        public static double DrawColumn(DrawingContext Context, string Caption, Pen LinePencil, Rect AvailableArea,
                                        IEnumerable<IRecognizableElement> Items)
        {
            double UsedHeight = 0;
            double MaxPicWidth = Display.ICOSIZE_LIT * 3;
            double SubItemsIdentation = 0;

            var ItemsFormat = new TextFormat("Arial", 9, Brushes.Black);
            var SubitemsFormat = new TextFormat("Arial", 8, Brushes.Black);
            var CaptionFormat = new TextFormat("Arial", 9, Brushes.Black, true, false, false, TextAlignment.Center);
            var ItemsArea = new Rect(AvailableArea.X + 2, AvailableArea.Y + 2,
                                     AvailableArea.Width - 4, AvailableArea.Height - 4);

            if (ItemsArea.Width < MaxPicWidth)
                return 0;

            UsedHeight += 14; if (ItemsArea.Top + UsedHeight > ItemsArea.Bottom) return 0;
            Context.DrawGeometry(Brushes.White, LinePencil, new LineGeometry(new Point(AvailableArea.Left, ItemsArea.Top + UsedHeight),
                                                                             new Point(AvailableArea.Right, ItemsArea.Top + UsedHeight)));
            Context.DrawText(CaptionFormat.GenerateFormattedText(Caption, ItemsArea.Width, 14), ItemsArea.TopLeft);

            foreach (var Item in Items)
            {
                var DisplayArea = ItemsArea;
                var DisplayFormat = ItemsFormat;
                
                if (Item is SimplePresentationElement)    // Denotes "subitem" like Relationship's Link-Role Variants
                {
                    DisplayArea = new Rect(ItemsArea.X + SubItemsIdentation, ItemsArea.Y,
                                           ItemsArea.Width - SubItemsIdentation, ItemsArea.Height);
                    DisplayFormat = SubitemsFormat;
                }

                UsedHeight += 2;
                if (UsedHeight + Display.ICOSIZE_LIT > DisplayArea.Height)
                    break;

                if (Item.Pictogram != null)
                    Context.DrawImage(Item.Pictogram, new Rect(((DisplayArea.X + MaxPicWidth / 2.0) - Item.Pictogram.GetWidth() / 2.0).EnforceMinimum(DisplayArea.X),
                                                               DisplayArea.Y + UsedHeight,
                                                               Item.Pictogram.GetWidth().EnforceMaximum(MaxPicWidth),
                                                               Item.Pictogram.GetHeight().EnforceMaximum(Display.ICOSIZE_LIT)));

                Context.DrawText(DisplayFormat.GenerateFormattedText(Item.Name /* Too long: + " -- " + Item.Summary */,
                                                                     DisplayArea.Width - (MaxPicWidth + 2), Display.ICOSIZE_LIT),
                                 new Point(DisplayArea.X + MaxPicWidth + 2, DisplayArea.Y + UsedHeight));

                UsedHeight += Display.ICOSIZE_LIT + 2;
            }

            return UsedHeight;
        }
    }
}