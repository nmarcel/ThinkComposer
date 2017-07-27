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
using Instrumind.Common.Visualization;
using Instrumind.Common.Visualization.Widgets;

using Instrumind.ThinkComposer.ApplicationProduct;
using Instrumind.ThinkComposer.MetaModel.Configurations;
using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.MetaModel.InformationMetaModel;
using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;
using Instrumind.ThinkComposer.Model;
using Instrumind.ThinkComposer.Model.GraphModel;
using Instrumind.ThinkComposer.Model.VisualModel;

/// Provides features for report generation.
namespace Instrumind.ThinkComposer.Composer.Reporting
{
    /// <summary>
    /// Generates comprehensive Standard reports from Compositions.
    /// </summary>
    public partial class ReportStandardGenerator
    {
        // -----------------------------------------------------------------------------------------
        public void CreateListOfMarkers(ReportStandardPagesMaker PagesMaker, string GroupKey, IEnumerable<MarkerAssignment> Source)
        {
            // Create initial Header
            var ColumnHeaderDefs = new Dictionary<string, Capsule<string, double, Func<IMModelClass, object>>>();

            if (this.Configuration.CompositeIdea_Markers_List.Definitor)
                ColumnHeaderDefs.Add(MarkerAssignment.__Definitor.TechName, Capsule.Create(MarkerAssignment.__Definitor.Name, 0.25, (Func<IMModelClass, object>)null));

            if (this.Configuration.CompositeIdea_Markers_List.PropPictogram)
                ColumnHeaderDefs.Add(SimplePresentationElement.__Pictogram.TechName, Capsule.Create("Pict.", 0.05, (Func<IMModelClass, object>)null));

            if (this.Configuration.CompositeIdea_Markers_List.PropName)
                ColumnHeaderDefs.Add(SimplePresentationElement.__Name.TechName, Capsule.Create(SimplePresentationElement.__Name.Name, 0.2, (Func<IMModelClass, object>)null));

            if (this.Configuration.CompositeIdea_Markers_List.PropTechName)
                ColumnHeaderDefs.Add(SimplePresentationElement.__TechName.TechName, Capsule.Create(SimplePresentationElement.__TechName.Name, 0.2, (Func<IMModelClass, object>)null));

            if (this.Configuration.CompositeIdea_Markers_List.PropSummary)
                ColumnHeaderDefs.Add(SimplePresentationElement.__Summary.TechName, Capsule.Create(SimplePresentationElement.__Summary.Name, 0.3, (Func<IMModelClass, object>)null));

            var Header = CreateListHeader(PagesMaker, ColumnHeaderDefs,
                                          this.Configuration.FmtListFieldLabel,
                                          this.Configuration.FmtFieldLabelBackground);

            PagesMaker.PageBreakStartCreator = (() => CreateListHeader(PagesMaker, ColumnHeaderDefs,
                                                                       this.Configuration.FmtListFieldLabel,
                                                                       this.Configuration.FmtFieldLabelBackground,
                                                                       this.Configuration.FmtListLinesForeground).Item1);
            PagesMaker.AppendContent(Header.Item1, false, GroupKey);

            var ColumnValueDefs = Header.Item2;
            var VerticalAlign = VerticalAlignment.Stretch;
            var HorizontalAlign = HorizontalAlignment.Stretch;

            // Travel Records...
            Source.ForEachIndexing((record, index) =>
            {
                int ColIndex = 0;
                FrameworkElement Cell = null;
                var RecordFrame = new StackPanel(); // Grid is too expensive for each record
                RecordFrame.Orientation = Orientation.Horizontal;

                if (this.Configuration.CompositeIdea_Markers_List.Definitor)
                {
                    Cell = CreateListCell(record.Definitor.Name, ColumnValueDefs.ElementAt(ColIndex).Value.Value0, HorizontalAlign, VerticalAlign);
                    RecordFrame.Children.Add(Cell);
                }

                if (this.Configuration.CompositeIdea_Markers_List.PropPictogram)
                {
                    ColIndex++;
                    Cell = CreateListCell(record.Descriptor != null && record.Descriptor.Pictogram != null
                                          ? record.Descriptor.Pictogram : record.Definitor.Pictogram,
                                          ColumnValueDefs.ElementAt(ColIndex).Value.Value0, HorizontalAlign, VerticalAlign);
                    RecordFrame.Children.Add(Cell);
                }

                if (this.Configuration.CompositeIdea_Markers_List.PropName)
                {
                    ColIndex++;
                    Cell = CreateListCell(record.Descriptor == null ? "" : record.Descriptor.Name.RemoveNewLines(), ColumnValueDefs.ElementAt(ColIndex).Value.Value0, HorizontalAlign, VerticalAlign);
                    RecordFrame.Children.Add(Cell);
                }

                if (this.Configuration.CompositeIdea_Markers_List.PropTechName)
                {
                    ColIndex++;
                    Cell = CreateListCell(record.Descriptor == null ? "" : record.Descriptor.TechName, ColumnValueDefs.ElementAt(ColIndex).Value.Value0, HorizontalAlign, VerticalAlign);
                    RecordFrame.Children.Add(Cell);
                }

                if (this.Configuration.CompositeIdea_Markers_List.PropSummary)
                {
                    ColIndex++;
                    Cell = CreateListCell(record.Descriptor == null ? "" : record.Descriptor.Summary, ColumnValueDefs.ElementAt(ColIndex).Value.Value0, HorizontalAlign, VerticalAlign);
                    RecordFrame.Children.Add(Cell);
                }

                PagesMaker.AppendContent(RecordFrame, false, (index > 0 ? null : GroupKey));
            });

            PagesMaker.PageBreakStartCreator = null;
        }

        // -----------------------------------------------------------------------------------------
        public void CreateCollectionOfIdeaCounterpartLinks(ReportStandardPagesMaker PagesMaker, string RelatedPrefix, string SelfPrefix,
                                                           IEnumerable<Tuple<RoleBasedLink,RoleBasedLink>> Source, string GroupKey)
        {
            RelatedPrefix = RelatedPrefix + Environment.NewLine;
            SelfPrefix = SelfPrefix + Environment.NewLine;

            // Notice the order by self-info, later related-info
            Source = Source.OrderBy(tlk => tlk.Item1.OwnerRelationship.Name + "~" + tlk.Item2.AssociatedIdea.Name);

            // Create initial Header
            var ColumnHeaderDefs = new Dictionary<string, Capsule<string, double, Func<IMModelClass, object>>>();

            // Related Idea labels...
            ColumnHeaderDefs.Add("Idea", Capsule.Create(RelatedPrefix + " Idea", 0.25, (Func<IMModelClass, object>)null));
            ColumnHeaderDefs.Add("RemRoleDesc", Capsule.Create(RelatedPrefix + "Link Desc.", 0.1, (Func<IMModelClass, object>)null));
            ColumnHeaderDefs.Add("RemRoleDef", Capsule.Create(RelatedPrefix + "Link Role", 0.075, (Func<IMModelClass, object>)null));
            ColumnHeaderDefs.Add("RemRoleVar", Capsule.Create(RelatedPrefix + "Link Var.", 0.075, (Func<IMModelClass, object>)null));

            // Relationship Idea label...
            ColumnHeaderDefs.Add("Relationship", Capsule.Create("By" + Environment.NewLine + "Relationship", 0.25, (Func<IMModelClass, object>)null));

            // Self Idea labels...
            ColumnHeaderDefs.Add("LocRoleDesc", Capsule.Create(SelfPrefix + "Link Desc.", 0.1, (Func<IMModelClass, object>)null));
            ColumnHeaderDefs.Add("LocRoleDef", Capsule.Create(SelfPrefix + "Link Role", 0.075, (Func<IMModelClass, object>)null));
            ColumnHeaderDefs.Add("LocRoleVar", Capsule.Create(SelfPrefix + "Link Var.", 0.075, (Func<IMModelClass, object>)null));

            var Header = CreateListHeader(PagesMaker, ColumnHeaderDefs,
                                          this.Configuration.FmtListFieldLabel,
                                          this.Configuration.FmtFieldLabelBackground);

            PagesMaker.PageBreakStartCreator = (() => CreateListHeader(PagesMaker, ColumnHeaderDefs,
                                                                       this.Configuration.FmtListFieldLabel,
                                                                       this.Configuration.FmtFieldLabelBackground,
                                                                       this.Configuration.FmtListLinesForeground).Item1);
            PagesMaker.AppendContent(Header.Item1, false, GroupKey);

            var ColumnValueDefs = Header.Item2;
            var VerticalAlign = VerticalAlignment.Stretch;
            var HorizontalAlign = HorizontalAlignment.Stretch;

            // Travel Records...
            Source.ForEachIndexing((record, index) =>
            {
                int ColIndex = 0;
                FrameworkElement Cell = null;
                var RecordFrame = new StackPanel(); // Grid is too expensive for each record
                RecordFrame.Orientation = Orientation.Horizontal;

                // Related Idea info...
                Cell = CreateListCell(record.Item2.AssociatedIdea.ToString(), ColumnValueDefs.ElementAt(ColIndex).Value.Value0, HorizontalAlign, VerticalAlign);
                RecordFrame.Children.Add(Cell);

                ColIndex++;
                Cell = CreateListCell((record.Item2.Descriptor == null ? "" : record.Item2.Descriptor.ToString()),
                                        ColumnValueDefs.ElementAt(ColIndex).Value.Value0,
                                        (record.Item2.Descriptor == null ? HorizontalAlignment.Center : HorizontalAlign), VerticalAlign);
                RecordFrame.Children.Add(Cell);

                ColIndex++;
                Cell = CreateListCell(record.Item2.RoleDefinitor.ToString(), ColumnValueDefs.ElementAt(ColIndex).Value.Value0, HorizontalAlign, VerticalAlign);
                RecordFrame.Children.Add(Cell);

                ColIndex++;
                Cell = CreateListCell(record.Item2.RoleVariant.ToString(), ColumnValueDefs.ElementAt(ColIndex).Value.Value0, HorizontalAlign, VerticalAlign);
                RecordFrame.Children.Add(Cell);

                // Relationship info...
                ColIndex++;
                Cell = CreateListCell(record.Item2.OwnerRelationship.ToString(),
                                      ColumnValueDefs.ElementAt(ColIndex).Value.Value0, HorizontalAlign, VerticalAlign);
                RecordFrame.Children.Add(Cell);

                // Self Idea info...
                ColIndex++;
                Cell = CreateListCell((record.Item1.Descriptor == null ? "" : record.Item1.Descriptor.ToString()),
                                        ColumnValueDefs.ElementAt(ColIndex).Value.Value0, HorizontalAlign, VerticalAlign);
                RecordFrame.Children.Add(Cell);

                ColIndex++;
                Cell = CreateListCell(record.Item1.RoleDefinitor.ToString(), ColumnValueDefs.ElementAt(ColIndex).Value.Value0, HorizontalAlign, VerticalAlign);
                RecordFrame.Children.Add(Cell);

                ColIndex++;
                Cell = CreateListCell(record.Item1.RoleVariant.ToString(), ColumnValueDefs.ElementAt(ColIndex).Value.Value0, HorizontalAlign, VerticalAlign);
                RecordFrame.Children.Add(Cell);

                PagesMaker.AppendContent(RecordFrame, false, (index > 0 ? null : GroupKey));
            });

            PagesMaker.PageBreakStartCreator = null;
        }

        public void CreateCollectionOfIdeaCompanionLinks(ReportStandardPagesMaker PagesMaker,
                                                         IEnumerable<RoleBasedLink> Source, string GroupKey)
        {
            // Notice the order by self-info, later related-info
            Source = Source.OrderBy(tlk => tlk.OwnerRelationship.Name + "~" + tlk.AssociatedIdea.Name);

            // Create initial Header
            var ColumnHeaderDefs = new Dictionary<string, Capsule<string, double, Func<IMModelClass, object>>>();

            ColumnHeaderDefs.Add("Idea", Capsule.Create("Companion Idea", 0.5, (Func<IMModelClass, object>)null));

            ColumnHeaderDefs.Add("Relationship", Capsule.Create("In Relationship", 0.25, (Func<IMModelClass, object>)null));

            ColumnHeaderDefs.Add("RoleDesc", Capsule.Create("Link Desc.", 0.1, (Func<IMModelClass, object>)null));
            ColumnHeaderDefs.Add("RoleDef", Capsule.Create("Link Role", 0.075, (Func<IMModelClass, object>)null));
            ColumnHeaderDefs.Add("RoleVar", Capsule.Create("Link Var.", 0.075, (Func<IMModelClass, object>)null));

            var Header = CreateListHeader(PagesMaker, ColumnHeaderDefs,
                                          this.Configuration.FmtListFieldLabel,
                                          this.Configuration.FmtFieldLabelBackground);

            PagesMaker.PageBreakStartCreator = (() => CreateListHeader(PagesMaker, ColumnHeaderDefs,
                                                                       this.Configuration.FmtListFieldLabel,
                                                                       this.Configuration.FmtFieldLabelBackground,
                                                                       this.Configuration.FmtListLinesForeground).Item1);
            PagesMaker.AppendContent(Header.Item1, false, GroupKey);

            var ColumnValueDefs = Header.Item2;
            var VerticalAlign = VerticalAlignment.Stretch;
            var HorizontalAlign = HorizontalAlignment.Stretch;

            // Travel Records...
            Source.ForEachIndexing((record, index) =>
            {
                int ColIndex = 0;
                FrameworkElement Cell = null;
                var RecordFrame = new StackPanel(); // Grid is too expensive for each record
                RecordFrame.Orientation = Orientation.Horizontal;

                Cell = CreateListCell(record.AssociatedIdea.ToString(), ColumnValueDefs.ElementAt(ColIndex).Value.Value0, HorizontalAlign, VerticalAlign);
                RecordFrame.Children.Add(Cell);

                ColIndex++;
                Cell = CreateListCell(record.OwnerRelationship.ToString(),
                                      ColumnValueDefs.ElementAt(ColIndex).Value.Value0, HorizontalAlign, VerticalAlign);
                RecordFrame.Children.Add(Cell);

                ColIndex++;
                Cell = CreateListCell((record.Descriptor == null ? "" : record.Descriptor.ToString()),
                                        ColumnValueDefs.ElementAt(ColIndex).Value.Value0, HorizontalAlign, VerticalAlign);
                RecordFrame.Children.Add(Cell);

                ColIndex++;
                Cell = CreateListCell(record.RoleDefinitor.ToString(), ColumnValueDefs.ElementAt(ColIndex).Value.Value0, HorizontalAlign, VerticalAlign);
                RecordFrame.Children.Add(Cell);

                ColIndex++;
                Cell = CreateListCell(record.RoleVariant.ToString(), ColumnValueDefs.ElementAt(ColIndex).Value.Value0, HorizontalAlign, VerticalAlign);
                RecordFrame.Children.Add(Cell);

                PagesMaker.AppendContent(RecordFrame, false, (index > 0 ? null : GroupKey));
            });

            PagesMaker.PageBreakStartCreator = null;
        }

        // -----------------------------------------------------------------------------------------
        public void CreateCollectionOfRelationshipLinks(ReportStandardPagesMaker PagesMaker,
                                                        IEnumerable<RoleBasedLink> Source, string GroupKey)
        {
            // Notice the order by self-info, later related-info
            Source = Source.OrderBy(lnk => lnk.RoleDefinitor.RoleType.ToString() + lnk.RoleDefinitor.Name + lnk.AssociatedIdea.Name);

            // Create initial Header
            var ColumnHeaderDefs = new Dictionary<string, Capsule<string, double, Func<IMModelClass, object>>>();

            // Related Idea labels...
            ColumnHeaderDefs.Add("Idea", Capsule.Create("Idea", 0.3, (Func<IMModelClass, object>)null));
            ColumnHeaderDefs.Add("RoleType", Capsule.Create("Type", 0.1, (Func<IMModelClass, object>)null));
            ColumnHeaderDefs.Add("RoleDef", Capsule.Create("Role", 0.2, (Func<IMModelClass, object>)null));
            ColumnHeaderDefs.Add("RoleVar", Capsule.Create("Variant", 0.1, (Func<IMModelClass, object>)null));
            ColumnHeaderDefs.Add("RoleDesc", Capsule.Create("Descriptor", 0.3, (Func<IMModelClass, object>)null));

            var Header = CreateListHeader(PagesMaker, ColumnHeaderDefs,
                                          this.Configuration.FmtListFieldLabel,
                                          this.Configuration.FmtFieldLabelBackground);

            PagesMaker.PageBreakStartCreator = (() => CreateListHeader(PagesMaker, ColumnHeaderDefs,
                                                                       this.Configuration.FmtListFieldLabel,
                                                                       this.Configuration.FmtFieldLabelBackground,
                                                                       this.Configuration.FmtListLinesForeground).Item1);
            PagesMaker.AppendContent(Header.Item1, false, GroupKey);

            var ColumnValueDefs = Header.Item2;
            var VerticalAlign = VerticalAlignment.Stretch;
            var HorizontalAlign = HorizontalAlignment.Stretch;

            // Travel Records...
            Source.ForEachIndexing((record, index) =>
            {
                int ColIndex = 0;
                FrameworkElement Cell = null;
                var RecordFrame = new StackPanel(); // Grid is too expensive for each record
                RecordFrame.Orientation = Orientation.Horizontal;

                Cell = CreateListCell(record.AssociatedIdea.ToString(), ColumnValueDefs.ElementAt(ColIndex).Value.Value0, HorizontalAlign, VerticalAlign);
                RecordFrame.Children.Add(Cell);

                ColIndex++;
                Cell = CreateListCell(record.RoleDefinitor.RoleType.GetFieldName(), ColumnValueDefs.ElementAt(ColIndex).Value.Value0, HorizontalAlign, VerticalAlign);
                RecordFrame.Children.Add(Cell);

                ColIndex++;
                Cell = CreateListCell(record.RoleDefinitor.ToString(), ColumnValueDefs.ElementAt(ColIndex).Value.Value0, HorizontalAlign, VerticalAlign);
                RecordFrame.Children.Add(Cell);

                ColIndex++;
                Cell = CreateListCell(record.RoleVariant.ToString(), ColumnValueDefs.ElementAt(ColIndex).Value.Value0, HorizontalAlign, VerticalAlign);
                RecordFrame.Children.Add(Cell);

                ColIndex++;
                Cell = CreateListCell((record.Descriptor == null ? "" : record.Descriptor.ToString()),
                                        ColumnValueDefs.ElementAt(ColIndex).Value.Value0,
                                        (record.Descriptor == null ? HorizontalAlignment.Center : HorizontalAlign), VerticalAlign);
                RecordFrame.Children.Add(Cell);

                PagesMaker.AppendContent(RecordFrame, false, (index > 0 ? null : GroupKey));
            });

            PagesMaker.PageBreakStartCreator = null;
        }

        // -----------------------------------------------------------------------------------------
        public void CreateCollectionOfComplements(ReportStandardPagesMaker PagesMaker, string GroupKey, IEnumerable<VisualComplement> Source)
        {
            // Create initial Header
            var ColumnHeaderDefs = new Dictionary<string, Capsule<string, double, Func<IMModelClass, object>>>();

            ColumnHeaderDefs.Add(VisualComplement.__Kind.TechName, Capsule.Create(VisualComplement.__Kind.Name, 0.2, (Func<IMModelClass, object>)null));

            ColumnHeaderDefs.Add(VisualComplement.__Content.TechName, Capsule.Create(VisualComplement.__Content.Name, 0.8, (Func<IMModelClass, object>)null));

            var Header = CreateListHeader(PagesMaker, ColumnHeaderDefs,
                                          this.Configuration.FmtListFieldLabel,
                                          this.Configuration.FmtFieldLabelBackground);

            PagesMaker.PageBreakStartCreator = (() => CreateListHeader(PagesMaker, ColumnHeaderDefs,
                                                                       this.Configuration.FmtListFieldLabel,
                                                                       this.Configuration.FmtFieldLabelBackground,
                                                                       this.Configuration.FmtListLinesForeground).Item1);
            PagesMaker.AppendContent(Header.Item1, false, GroupKey);

            var ColumnValueDefs = Header.Item2;
            var VerticalAlign = VerticalAlignment.Stretch;
            var HorizontalAlign = HorizontalAlignment.Stretch;

            // Travel Records...
            Source.ForEachIndexing((record, index) =>
            {
                int ColIndex = 0;
                FrameworkElement Cell = null;
                var RecordFrame = new StackPanel(); // Grid is too expensive for each record
                RecordFrame.Orientation = Orientation.Horizontal;

                Cell = CreateListCell(record.Kind.Name, ColumnValueDefs.ElementAt(ColIndex).Value.Value0, HorizontalAlign, VerticalAlign);
                RecordFrame.Children.Add(Cell);

                ColIndex++;
                Cell = CreateListCell(record.ContentAsText, ColumnValueDefs.ElementAt(ColIndex).Value.Value0, HorizontalAlign, VerticalAlign);
                RecordFrame.Children.Add(Cell);

                PagesMaker.AppendContent(RecordFrame, false, (index > 0 ? null : GroupKey));
            });

            PagesMaker.PageBreakStartCreator = null;
        }

        // -----------------------------------------------------------------------------------------
    }
}
