// -------------------------------------------------------------------------------------------
// Instrumind ThinkComposer
//
// Copyright (C) Néstor Marcel Sánchez Ahumada. Santiago, Chile.
// http://thinkcomposer.codeplex.com
//
// This file is part of ThinkComposer, which is free software licensed under the GNU General Public License.
// It is provided without any warranty. You should find a copy of the license in the root directory of this software product.
// -------------------------------------------------------------------------------------------
//
// Project: Instrumind ThinkComposer v1.0
// File   : TableAppearance.cs
// Object : Instrumind.ThinkComposer.MetaModel.VisualMetaModel.TableAppearance (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.12.15 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.Linq;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;

using Instrumind.ThinkComposer.Model.InformationModel;
using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;

/// Base abstractions for the user metadefinition of Visual schemas
namespace Instrumind.ThinkComposer.MetaModel.VisualMetaModel
{
    /// <summary>
    /// Stores appearance data for a table to be displayed as part of a details poster of a symbol.
    /// </summary>
    [Serializable]
    public class TableAppearance : DetailAppearance, IModelEntity, IModelClass<TableAppearance>
    {
        // Default values for variable initializations and detection of user custom changes.
        private const bool DEFVAL_IsMultiRecord = true;
        //? private const int DEFAVAL_MinRecords = 0;
        //? private const int DEFAVAL_MaxRecords = 0;
        private const ETableLayoutStyle DEFVAL_Layout = ETableLayoutStyle.Conventional;
        private const bool DEFVAL_ShowFieldTitles = true;

        /// <summary>
        /// Static Constructor.
        /// </summary>
        static TableAppearance()
        {
            __ClassDefinitor = new ModelClassDefinitor<TableAppearance>("TableAppearance", DetailAppearance.__ClassDefinitor, "Table Appearance",
                                                                        "Stores appearance data for a table to be displayed as part of a details poster of a symbol.");
            __ClassDefinitor.DeclareProperty(__IsMultiRecord);
            //? __ClassDefinitor.DeclareProperty(__MinRecords);
            //? __ClassDefinitor.DeclareProperty(__MaxRecords);
            __ClassDefinitor.DeclareProperty(__Layout);
            __ClassDefinitor.DeclareProperty(__ShowFieldTitles);
            // POSTPONED: __ClassDefinitor.DeclareProperty(__FieldsArrangement);
            // POSTPONED: __ClassDefinitor.DeclareProperty(__RecordsArrangement);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public TableAppearance(bool IsDisplayed = DEFVAL_IsDisplayed, bool ShowTitle = DEFVAL_ShowTitle,
                               bool IsMultiRecord = DEFVAL_IsMultiRecord, ETableLayoutStyle Layout = DEFVAL_Layout, bool ShowFieldTitles = DEFVAL_ShowFieldTitles)
             : base(IsDisplayed, ShowTitle)
        {
            this.IsMultiRecord = IsMultiRecord;
            this.Layout = Layout;
            this.ShowFieldTitles = ShowFieldTitles;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Indicates whether the instance has been changed respect its default values, therefore requiring to be reevaluated for later use.
        /// </summary>
        public override bool IsAltered
        {
            get
            {
                return (base.IsAltered || this.IsMultiRecord != DEFVAL_IsMultiRecord || this.Layout != DEFVAL_Layout || this.ShowFieldTitles != DEFVAL_ShowFieldTitles /* ||
                        this.FieldsArrangement.IsAltered || this.RecordsArrangement.IsAltered*/ );
            }
        }

        /// <summary>
        /// Indicates whether the settings of this instance differs from that supplied one.
        /// </summary>
        public override bool SettingsDiffersFrom(DetailAppearance Other)
        {
            var OtherTableFormat = Other as TableAppearance;

            return (OtherTableFormat == null || base.SettingsDiffersFrom(Other) ||
                    !(this.IsMultiRecord == OtherTableFormat.IsMultiRecord && this.Layout == OtherTableFormat.Layout && this.ShowFieldTitles == OtherTableFormat.ShowFieldTitles));
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns a clone of this detail format.
        /// </summary>
        public override DetailAppearance Clone()
        {
            var Result = this.CreateClone(ECloneOperationScope.Slight, null);
            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Indicates whether the table grid will show multiple records. If not, then show only the first one.
        /// </summary>
        public bool IsMultiRecord
        {
            get { return __IsMultiRecord.Get(this); }
            set
            {
                __IsMultiRecord.Set(this, value);
                /*? if (__IsMultiRecord.Set(this, value) && value && this.MaxRecords == 1)
                    this.MaxRecords = 0;    // unspecified */
            }
        }
        protected bool IsMultiRecord_ = true;
        public static readonly ModelPropertyDefinitor<TableAppearance, bool> __IsMultiRecord =
                   new ModelPropertyDefinitor<TableAppearance, bool>("IsMultiRecord", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.IsMultiRecord_, (ins, val) => ins.IsMultiRecord_ = val, false, true,
                                                                                    "Is Multi-Record", "Indicates whether the table grid will show multiple records. If not, then show only the first one.");

        /*? /// <summary>
        /// Minimum number of records that must be stored (unspecified = 0).
        /// </summary>
        public int MinRecords { get { return __MinRecords.Get(this); } set { __MinRecords.Set(this, value); } }
        protected int MinRecords_ = 1;
        public static readonly ModelPropertyDefinitor<TableFormat, int> __MinRecords =
                   new ModelPropertyDefinitor<TableFormat, int>("MinRecords", null, EPropertyKind.Common, ins => ins.MinRecords_, (ins, val) => ins.MinRecords_ = val, false, false, true,
                                                                    "Minimum Records", "Minimum number of records that must be stored (unspecified = 0).");

        /// <summary>
        /// Maximum number of records that can be stored (unspecified = 0).
        /// </summary>
        public int MaxRecords
        {
            get { return __MaxRecords.Get(this); }
            set
            {
                if (__MaxRecords.Set(this, value))
                    this.IsMultiRecord = (value != 1);
            }
        }
        protected int MaxRecords_ = 0;
        public static readonly ModelPropertyDefinitor<TableFormat, int> __MaxRecords =
                   new ModelPropertyDefinitor<TableFormat, int>("MaxRecords", null, EPropertyKind.Common, ins => ins.MaxRecords_, (ins, val) => ins.MaxRecords_ = val, false, false, true,
                                                                    "Maximum Records", "Maximum number of records that can be stored (unspecified = 0).");
        */

        /// <summary>
        /// Indicates the layout style of the Table when shown.
        /// </summary>
        public ETableLayoutStyle Layout { get { return __Layout.Get(this); } set { __Layout.Set(this, value); } }
        protected ETableLayoutStyle Layout_ = ETableLayoutStyle.Conventional;
        public static readonly ModelPropertyDefinitor<TableAppearance, ETableLayoutStyle> __Layout =
                   new ModelPropertyDefinitor<TableAppearance, ETableLayoutStyle>("Layout", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.Layout_, (ins, val) => ins.Layout_ = val, false, false,
                                                                                                                 "Layout", "Indicates the layout style of the Table when shown.");

        /// <summary>
        /// Indicates whether to show field titles.
        /// </summary>
        public bool ShowFieldTitles { get { return __ShowFieldTitles.Get(this); } set { __ShowFieldTitles.Set(this, value); } }
        protected bool ShowFieldTitles_ = true;
        public static readonly ModelPropertyDefinitor<TableAppearance, bool> __ShowFieldTitles =
                   new ModelPropertyDefinitor<TableAppearance, bool>("ShowFieldTitles", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.ShowFieldTitles_, (ins, val) => ins.ShowFieldTitles_ = val, false, false,
                                                                                                                 "Show Field Titles", "Indicates whether to show field titles.");

        /* POSTPONED:
        /// <summary>
        /// Display arrangement for the collection of Fields.
        /// </summary>
        public CollectionDisplayArrangement<int> FieldsArrangement { get { return __FieldsArrangement.Get(this); } set { __FieldsArrangement.Set(this, value); } }
        protected CollectionDisplayArrangement<int> FieldsArrangement_ = new CollectionDisplayArrangement<int>();
        public static readonly ModelPropertyDefinitor<TableAppearance, CollectionDisplayArrangement<int>> __FieldsArrangement =
                   new ModelPropertyDefinitor<TableAppearance, CollectionDisplayArrangement<int>>("FieldsArrangement", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.FieldsArrangement_, (ins, val) => ins.FieldsArrangement_ = val, false, false, false,
                                                                                                  "Fields Arrangement", "Display arrangement for the collection of Fields.");

        /// <summary>
        /// Display arrangement for the collection of Records.
        /// </summary>
        public CollectionDisplayArrangement<int> RecordsArrangement { get { return __RecordsArrangement.Get(this); } set { __RecordsArrangement.Set(this, value); } }
        protected CollectionDisplayArrangement<int> RecordsArrangement_ = new CollectionDisplayArrangement<int>();
        public static readonly ModelPropertyDefinitor<TableAppearance, CollectionDisplayArrangement<int>> __RecordsArrangement =
                   new ModelPropertyDefinitor<TableAppearance, CollectionDisplayArrangement<int>>("RecordsArrangement", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.RecordsArrangement_, (ins, val) => ins.RecordsArrangement_ = val, false, false, false,
                                                                                              "Records Arrangement", "Display arrangement for the collection of Records."); */

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelClass<TableFormat> Members

        public new MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public new ModelClassDefinitor<TableAppearance> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly new ModelClassDefinitor<TableAppearance> __ClassDefinitor = null;

        public override object CreateCopy(ECloneOperationScope CloningScope, IMModelClass DirectOwner) { return this.CreateClone(CloningScope, DirectOwner); }
        public new TableAppearance CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((TableAppearance)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public TableAppearance PopulateFrom(TableAppearance SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}