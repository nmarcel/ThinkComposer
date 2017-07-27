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
        public void CreateContentList(IEnumerable<IMModelClass> Source, IRecognizableComposite SourceOwner, DisplayList DispList,
                                      string DefinitorTechName, string DefinitorName, string TechPrefixInternalOwner = null, string TechPrefixSource = null,
                                      params Tuple<string, string, double, Func<IMModelClass, object>>[] ExtraColDefs)
        {
            var TableColDefs = new List<Capsule<string, string, double, Func<IMModelClass, object>>>();
            var TableRowVals = new List<List<string>>();

            // Define columns
            if (DispList.PropName)
                TableColDefs.Add(Capsule.Create(FormalPresentationElement.__Name.TechName.RemoveNewLines().ToHtmlEncoded(),
                                                FormalPresentationElement.__Name.Name.RemoveNewLines().ToHtmlEncoded(), 25.0,
                                                (Func<IMModelClass, object>)null));

            if (DispList.PropTechName)
                TableColDefs.Add(Capsule.Create(FormalPresentationElement.__TechName.TechName.RemoveNewLines().ToHtmlEncoded(),
                                                FormalPresentationElement.__TechName.Name.RemoveNewLines().ToHtmlEncoded(), 20.0,
                                                (Func<IMModelClass, object>)null));
                
            if (DispList.PropSummary)
                TableColDefs.Add(Capsule.Create(FormalPresentationElement.__Summary.TechName.RemoveNewLines().ToHtmlEncoded(),
                                                FormalPresentationElement.__Summary.Name.RemoveNewLines().ToHtmlEncoded(),
                                                (!DispList.PropTechName ? 50.0 : 30.0),
                                                (Func<IMModelClass, object>)null));
                
            if (DispList.PropPictogram)
                TableColDefs.Add(Capsule.Create(FormalPresentationElement.__Pictogram.TechName.RemoveNewLines().ToHtmlEncoded(),
                                                "Pict.", 5.0,               // FormalPresentationElement.__Pictogram.Name.RemoveNewLines().ToHtmlEncoded()
                                                (Func<IMModelClass, object>)null));

            if (DispList.Definitor && !(DefinitorTechName.IsAbsent() || DefinitorName.IsAbsent()))
                TableColDefs.Add(Capsule.Create(DefinitorTechName.ToHtmlEncoded(), DefinitorName.ToHtmlEncoded(),
                                                (!DispList.PropPictogram ? 28.0 : 20.0),
                                                (Func<IMModelClass, object>)null));

            if (ExtraColDefs != null)
                foreach (var ExtraColDef in ExtraColDefs)
                    TableColDefs.Add(Capsule.Create(ExtraColDef.Item1.ToHtmlEncoded(), ExtraColDef.Item2.ToHtmlEncoded(), ExtraColDef.Item3, ExtraColDef.Item4));

            // Generate record values
            foreach(var Record in Source)
            {
                var Values = new List<string>();

                string Anchor = null;

                if (this.RegisteredLocations.ContainsKey(Record))
                {
                    var RefLink = this.GetRelativeLocationOf(Record);

                    if (!RefLink.IsAbsent())
                        Anchor = "<a href=" + RefLink + ">@VALUE</a>";
                }

                // Travel Colum-Defs...
                foreach (var ColValueDef in TableColDefs)
                {
                    var ColDef = Record.ClassDefinition.GetPropertyDef(ColValueDef.Value0, false);
                    object Value = null;

                    if (ColDef != null && Record is IRecognizableElement
                        && ColDef.TechName == FormalPresentationElement.__Pictogram.TechName)  // Used as "Formal..." but could be a "Simple..."
                        Value = this.CurrentWorker.AtOriginalThreadInvoke(
                            () =>
                            {
                                var Picture = ((IRecognizableElement)Record).Pictogram;
                                if (Picture == null)
                                    if (Record is IdeaDefinition)
                                        Picture = ((IdeaDefinition)Record).Pictogram;
                                    else
                                        if (Record is Idea && ((Idea)Record).IdeaDefinitor.DefaultSymbolFormat.UseDefinitorPictogramAsNullDefault)
                                            Picture = ((Idea)Record).IdeaDefinitor.Pictogram;

                                if (Picture != null && !Picture.IsFrozen)
                                    Picture.Freeze();

                                return Picture;
                            });
                    else
                        Value = (ColValueDef.Value3 != null
                                 ? ColValueDef.Value3(Record)
                                 : (ColDef == null ? null : ColDef.Read(Record)));

                    var NamePrefix = (TechPrefixInternalOwner.IsAbsent() ? "" : TechPrefixInternalOwner + ".") +
                                     (TechPrefixSource.IsAbsent() ? "" : TechPrefixSource + "_" + Source.IndexOfMatch(item => item == Record).ToString() + ".");
                    var Spec = GetRecordValueAsHtmlString(Value, SourceOwner, Record, null, NamePrefix);

                    if (Values.Count < 1 && Anchor != null)
                        Spec = Anchor.Replace("@VALUE", Spec);

                    Values.Add(Spec);
                }

                TableRowVals.Add(Values);
            };

            this.PageWriteTable("tbl_list_objects", TableRowVals, TableColDefs.Select(def => Capsule.Create(def.Value0, def.Value1, def.Value2)).ToArray());
        }

        // -----------------------------------------------------------------------------------------
        public string GetRecordValueAsHtmlString(object Value, IRecognizableComposite RecordsOwner,
                                                 object RecordObject = null, IRecognizableComposite Requester = null, string MemberNamePrefix = null)
        {
            var Result = "";

            // PENDING: Determine and include link

            if (Value is MAssignment)
                Value = ((MAssignment)Value).AssignedValue;

            if (Value is ImageSource)
            {
                var Picture = this.CurrentWorker.AtOriginalThreadGetFrozen((ImageSource)Value);
                var PictureRef = this.CreateImage(Picture, RecordsOwner, RecordObject, MemberNamePrefix.NullDefault("") + FormalPresentationElement.__Pictogram.TechName);

                Result = "<img Alt='' Src='" + PictureRef + "' style='width:" + ReportConfiguration.LIST_PICT_STD_WIDTH.ToString() + "px; height:" + ReportConfiguration.LIST_PICT_STD_HEIGHT.ToString() + "px;' />";
            }
            else
                Result = (Value is IIdentifiableElement
                          ? ((IIdentifiableElement)Value).Name
                          : Value.ToStringAlways()).ToHtmlEncoded();

            return Result;
        }

        // -----------------------------------------------------------------------------------------
    }
}
