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
using System.Windows;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.Definitor.DefinitorUI;
using Instrumind.ThinkComposer.MetaModel;
using Instrumind.ThinkComposer.Composer;
using Instrumind.ThinkComposer.Definitor;
using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;

/// Base abstractions for the visual representation of Graph entities
namespace Instrumind.ThinkComposer.Model.VisualModel
{
    /// <summary>
    /// Individual visual object exposing attached information, such as note, callout, legend and info-card.
    /// </summary>
    public partial class VisualComplement : VisualObject, IModelEntity, IModelClass<VisualComplement>
    {
        public static DrawingGroup GenerateInfoCard(VisualComplement Complement, View Target)
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

                Context.DrawGeometry(Background, Pencil, new LineGeometry(new Point(Complement.BaseArea.Left + VerticalTab, Complement.BaseArea.Top),
                                                                          new Point(Complement.BaseArea.Left + VerticalTab, Complement.BaseArea.Bottom)));

                // Text "Composition"
                DrawLabel(Context, "Composition:", LabelCaptionFormat, Complement.BaseArea.Left, Complement.BaseArea.Left + VerticalTab, VerticalOffset, LabelHeight, Margin);
                DrawLabel(Context, Complement.Target.OwnerGlobal.OwnerCompositeContainer.OwnerComposition.Name, LabelContentFormat,
                          Complement.BaseArea.Left + VerticalTab, Complement.BaseArea.Right, VerticalOffset, LabelHeight, Margin);

                // Text "View"
                VerticalOffset += LabelHeight; if (VerticalOffset + LabelHeight >= Complement.BaseArea.Bottom) goto Exit;
                Context.DrawGeometry(Background, Pencil, new LineGeometry(new Point(Complement.BaseArea.Left, VerticalOffset), new Point(Complement.BaseArea.Right, VerticalOffset)));
                DrawLabel(Context, "View:", LabelCaptionFormat, Complement.BaseArea.Left, Complement.BaseArea.Left + VerticalTab, VerticalOffset, LabelHeight, Margin);
                DrawLabel(Context, Complement.Target.OwnerGlobal.Name, LabelContentFormat,
                          Complement.BaseArea.Left + VerticalTab, Complement.BaseArea.Right, VerticalOffset, LabelHeight, Margin);

                if (!(Complement.Target.OwnerGlobal.OwnerCompositeContainer is Composition))
                {
                    //-if (VerticalOffset + LabelHeight >= Complement.BaseArea.Bottom) goto Exit;

                    // Text "Composite Idea"
                    VerticalOffset += LabelHeight; if (VerticalOffset + LabelHeight >= Complement.BaseArea.Bottom) goto Exit;
                    Context.DrawGeometry(Background, Pencil, new LineGeometry(new Point(Complement.BaseArea.Left, VerticalOffset), new Point(Complement.BaseArea.Right, VerticalOffset)));
                    DrawLabel(Context, "Composite:", LabelCaptionFormat, Complement.BaseArea.Left, Complement.BaseArea.Left + VerticalTab, VerticalOffset, LabelHeight, Margin);
                    DrawLabel(Context, Complement.Target.OwnerGlobal.OwnerCompositeContainer.Name, LabelContentFormat,
                              Complement.BaseArea.Left + VerticalTab, Complement.BaseArea.Right, VerticalOffset, LabelHeight, Margin);

                    // Text "Type"
                    VerticalOffset += LabelHeight; if (VerticalOffset + LabelHeight >= Complement.BaseArea.Bottom) goto Exit;
                    Context.DrawGeometry(Background, Pencil, new LineGeometry(new Point(Complement.BaseArea.Left, VerticalOffset), new Point(Complement.BaseArea.Right, VerticalOffset)));
                    DrawLabel(Context, "Type:", LabelCaptionFormat, Complement.BaseArea.Left, Complement.BaseArea.Left + VerticalTab, VerticalOffset, LabelHeight, Margin);
                    DrawLabel(Context, Complement.Target.OwnerGlobal.OwnerCompositeContainer.IdeaDefinitor.Name, LabelContentFormat,
                              Complement.BaseArea.Left + VerticalTab, Complement.BaseArea.Right, VerticalOffset, LabelHeight, Margin);

                    //-VerticalOffset += LabelHeight; if (VerticalOffset + LabelHeight >= Complement.BaseArea.Bottom) goto Exit;
                }

                // Text "Summary"
                var SummaryHeight = LabelHeight;

                if (!Complement.Target.OwnerGlobal.OwnerCompositeContainer.Summary.Trim().IsAbsent())
                {
                    SummaryHeight = LabelHeight * 2.65;
                    VerticalOffset += LabelHeight; if (VerticalOffset + SummaryHeight >= Complement.BaseArea.Bottom) goto Exit;
                    Context.DrawGeometry(Background, Pencil, new LineGeometry(new Point(Complement.BaseArea.Left, VerticalOffset), new Point(Complement.BaseArea.Right, VerticalOffset)));
                    DrawLabel(Context, "Summary:", LabelCaptionFormat, Complement.BaseArea.Left, Complement.BaseArea.Left + VerticalTab, VerticalOffset, SummaryHeight, Margin);
                    DrawLabel(Context, Complement.Target.OwnerGlobal.OwnerCompositeContainer.Summary, LabelContentFormat,
                              Complement.BaseArea.Left + VerticalTab, Complement.BaseArea.Right, VerticalOffset, SummaryHeight, Margin);
                }

                var VersionInfo = (Complement.Target.OwnerGlobal.OwnerCompositeContainer.Version != null
                                   ? Complement.Target.OwnerGlobal.OwnerCompositeContainer.Version
                                   : Complement.Target.OwnerGlobal.OwnerCompositeContainer.OwnerComposition.Version);

                var VerticalTab2Ini = 150;
                var VerticalTab2End = 210;

                // Text "Creator"
                VerticalOffset += SummaryHeight; if (VerticalOffset + LabelHeight >= Complement.BaseArea.Bottom) goto Exit;
                var LocalZoneTop = VerticalOffset;
                Context.DrawGeometry(Background, Pencil, new LineGeometry(new Point(Complement.BaseArea.Left, VerticalOffset), new Point(Complement.BaseArea.Right, VerticalOffset)));
                DrawLabel(Context, "Creator:", LabelCaptionFormat, Complement.BaseArea.Left, Complement.BaseArea.Left + VerticalTab, VerticalOffset, LabelHeight, Margin);
                DrawLabel(Context, VersionInfo.Creator, LabelContentFormat,
                          Complement.BaseArea.Left + VerticalTab, Complement.BaseArea.Left + VerticalTab2Ini, VerticalOffset, LabelHeight, Margin);

                // Text "Last Modifier"
                VerticalOffset += LabelHeight; if (VerticalOffset + LabelHeight >= Complement.BaseArea.Bottom) goto Exit;
                Context.DrawGeometry(Background, Pencil, new LineGeometry(new Point(Complement.BaseArea.Left, VerticalOffset), new Point(Complement.BaseArea.Right, VerticalOffset)));
                DrawLabel(Context, "Last Modifier:", LabelCaptionFormat, Complement.BaseArea.Left, Complement.BaseArea.Left + VerticalTab, VerticalOffset, LabelHeight, Margin);
                DrawLabel(Context, VersionInfo.LastModifier, LabelContentFormat,
                          Complement.BaseArea.Left + VerticalTab, Complement.BaseArea.Left + VerticalTab2Ini, VerticalOffset, LabelHeight, Margin);

                if (Complement.BaseArea.Right - (Complement.BaseArea.Left + VerticalTab2End) < 10) goto Exit;
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

            Exit: ;
            }

            return Result;
        }
    }
}