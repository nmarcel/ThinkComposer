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
// File   : ReportHtmlGenerator.cs
// Object : Instrumind.ThinkComposer.Composer.Reporting.ReportHtmlGenerator (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2012.09.26 Néstor Sánchez A.  Creation
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
using Instrumind.ThinkComposer.MetaModel;
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
    /// Generates comprehensive HTML reports from Compositions.
    /// </summary>
    public partial class ReportHtmlGenerator
    {
        // -----------------------------------------------------------------------------------------
        public void CreateListOfMarkers(IRecognizableComposite SourceOwner, IEnumerable<MarkerAssignment> Source)
        {
            var TableColDefs = new List<Capsule<string, string, double, Func<MarkerAssignment, IMModelClass>>>();
            var TableRowVals = new List<List<string>>();

            if (this.Configuration.CompositeIdea_Markers_List.Definitor)
                TableColDefs.Add(new Capsule<string, string, double, Func<MarkerAssignment, IMModelClass>>
                                               (MarkerDefinition.__Name.TechName,   // Think twice, this is ok
                                                MarkerAssignment.__Definitor.Name.RemoveNewLines().ToHtmlEncoded(), 25.0,
                                                rec => rec.Definitor));

            if (this.Configuration.CompositeIdea_Markers_List.PropPictogram)
                TableColDefs.Add(new Capsule<string, string, double, Func<MarkerAssignment, IMModelClass>>
                                               (SimplePresentationElement.__Pictogram.TechName,
                                                "Pict.", 5.0, rec => (rec.Descriptor != null && rec.Descriptor.Pictogram != null
                                                                      ? rec.Descriptor : rec.Definitor)));

            if (this.Configuration.CompositeIdea_Markers_List.PropName)
                TableColDefs.Add(new Capsule<string, string, double, Func<MarkerAssignment, IMModelClass>>
                                               (SimplePresentationElement.__Name.TechName,
                                                SimplePresentationElement.__Name.Name.RemoveNewLines().ToHtmlEncoded(), 20.0,
                                                rec => rec.Descriptor));

            if (this.Configuration.CompositeIdea_Markers_List.PropTechName)
                TableColDefs.Add(new Capsule<string, string, double, Func<MarkerAssignment, IMModelClass>>
                                               (SimplePresentationElement.__TechName.TechName,
                                                SimplePresentationElement.__TechName.Name.RemoveNewLines().ToHtmlEncoded(), 20.0,
                                                rec => rec.Descriptor));

            if (this.Configuration.CompositeIdea_Markers_List.PropSummary)
                TableColDefs.Add(new Capsule<string, string, double, Func<MarkerAssignment, IMModelClass>>
                                               (SimplePresentationElement.__Summary.TechName,
                                                SimplePresentationElement.__Summary.Name.RemoveNewLines().ToHtmlEncoded(), 30.0,
                                                rec => rec.Descriptor));

            this.PageWriteTable("tbl_list_objects", SourceOwner, Source, TableColDefs.ToArray());
        }

        // -----------------------------------------------------------------------------------------
        public void CreateCollectionOfIdeaCounterpartLinks(string RelatedPrefix, string SelfPrefix,
                                                           IEnumerable<Tuple<RoleBasedLink,RoleBasedLink>> Source)
        {
            RelatedPrefix = (RelatedPrefix + Environment.NewLine).ToHtmlEncoded();
            SelfPrefix = (SelfPrefix + Environment.NewLine).ToHtmlEncoded();

            // Notice the order by self-info, later related-info
            Source = Source.OrderBy(tlk => tlk.Item1.OwnerRelationship.Name + "~" + tlk.Item2.AssociatedIdea.Name);

            this.PageWriteTable("tbl_list_objects",
                                Source.Select(rec => General.Enumerate(rec.Item2.AssociatedIdea.ToString().ToHtmlEncoded(),
                                                                       (rec.Item2.Descriptor == null ? "" : rec.Item2.Descriptor.ToString().ToHtmlEncoded()),
                                                                       rec.Item2.RoleDefinitor.ToString().ToHtmlEncoded(),
                                                                       rec.Item2.RoleVariant.ToString().ToHtmlEncoded(),
                                                                       rec.Item2.OwnerRelationship.ToString().ToHtmlEncoded(),
                                                                       (rec.Item1.Descriptor == null ? "" : rec.Item1.Descriptor.ToString().ToHtmlEncoded()),
                                                                       rec.Item1.RoleDefinitor.ToString().ToHtmlEncoded(),
                                                                       rec.Item1.RoleVariant.ToString().ToHtmlEncoded())),
                                Capsule.Create("Idea", RelatedPrefix + " Idea", 25.0),
                                Capsule.Create("RemRoleDesc", RelatedPrefix + " Link Desc.", 10.0),
                                Capsule.Create("RemRoleDef", RelatedPrefix + " Link Role", 7.5),
                                Capsule.Create("RemRoleVar", RelatedPrefix + " Link Var.", 7.5),
                                Capsule.Create("Relationship", ("By" + Environment.NewLine + "Relationship").ToHtmlEncoded(), 25.0),
                                Capsule.Create("RoleDesc", SelfPrefix + "Link Desc.", 10.0),
                                Capsule.Create("RoleDef", SelfPrefix + "Link Role", 7.5),
                                Capsule.Create("RoleVar", SelfPrefix + "Link Var.", 7.5));
        }

        public void CreateCollectionOfIdeaCompanionLinks(IEnumerable<RoleBasedLink> Source)
        {
            // Notice the order by self-info, later related-info
            Source = Source.OrderBy(tlk => tlk.OwnerRelationship.Name + "~" + tlk.AssociatedIdea.Name);

            this.PageWriteTable("tbl_list_objects",
                                Source.Select(rec => General.Enumerate(rec.AssociatedIdea.ToString().ToHtmlEncoded(),
                                                                       rec.OwnerRelationship.ToString().ToHtmlEncoded(),
                                                                       (rec.Descriptor == null ? "" : rec.Descriptor.ToString().ToHtmlEncoded()),
                                                                       rec.RoleDefinitor.ToString().ToHtmlEncoded(),
                                                                       rec.RoleVariant.ToString().ToHtmlEncoded())),
                                Capsule.Create("Idea", "Companion Idea", 50.0),
                                Capsule.Create("Relationship", "In Relationship", 25.0),
                                Capsule.Create("RoleDesc", "Link Desc.", 10.0),
                                Capsule.Create("RoleDef", "Link Role", 7.5),
                                Capsule.Create("RoleVar", "Link Var.", 7.5));
        }

        // -----------------------------------------------------------------------------------------
        public void CreateCollectionOfRelationshipLinks(IEnumerable<RoleBasedLink> Source)
        {
            // Notice the order by self-info, later related-info
            Source = Source.OrderBy(lnk => lnk.RoleDefinitor.RoleType.ToString() + lnk.RoleDefinitor.Name + lnk.AssociatedIdea.Name);

            this.PageWriteTable("tbl_list_objects",
                                Source.Select(rec => General.Enumerate(rec.AssociatedIdea.ToString().ToHtmlEncoded(),
                                                                       rec.RoleDefinitor.RoleType.GetFieldName().ToHtmlEncoded(),
                                                                       rec.RoleDefinitor.ToString().ToHtmlEncoded(),
                                                                       rec.RoleVariant.ToString().ToHtmlEncoded(),
                                                                       (rec.Descriptor == null ? "" : rec.Descriptor.ToString().ToHtmlEncoded()))),
                                Capsule.Create("Idea", "Idea", 30.0),
                                Capsule.Create("RoleType", "Type", 10.0),
                                Capsule.Create("RoleDef", "Role", 20.0),
                                Capsule.Create("RoleVar", "Variant", 30.0),
                                Capsule.Create("RoleDesc", "Descriptor", 30.0));
        }

        // -----------------------------------------------------------------------------------------
        public void CreateCollectionOfComplements(IEnumerable<VisualComplement> Source)
        {
            this.PageWriteTable("tbl_list_objects",
                                Source.Select(rec => General.Enumerate(rec.Kind.Name.RemoveNewLines().ToHtmlEncoded(),
                                                                       rec.ContentAsText)),
                                Capsule.Create(VisualComplement.__Kind.TechName,
                                               VisualComplement.__Kind.Name.RemoveNewLines().ToHtmlEncoded(), 20.0),
                                Capsule.Create(VisualComplement.__Content.TechName,
                                               VisualComplement.__Content.Name.RemoveNewLines().ToHtmlEncoded(), 80.0));
        }

        // -----------------------------------------------------------------------------------------
    }
}
