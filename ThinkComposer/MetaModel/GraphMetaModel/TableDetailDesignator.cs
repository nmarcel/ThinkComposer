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
// File   : TableDetailDesignator.cs
// Object : Instrumind.ThinkComposer.MetaModel.GraphMetaModel.TableDetailDesignator (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.11.08 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.MetaModel.InformationMetaModel;
using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;
using Instrumind.ThinkComposer.Model;
using Instrumind.ThinkComposer.Model.GraphModel;
using Instrumind.ThinkComposer.Model.InformationModel;

/// Base abstractions for the user metadefinition of Graph schemas
namespace Instrumind.ThinkComposer.MetaModel.GraphMetaModel
{
    /// <summary>
    /// Associates Table-Structure Definitions to an Idea, Idea-Definition or Field-Definition (Contained Table field type)
    /// </summary>
    // IMPORTANT: When the direct owner is a Field-Definition (not a Idea or Idea-Definition), set the Owner as the Domain.
    [Serializable]
    public class TableDetailDesignator : DetailDesignator, IModelEntity, IModelClass<TableDetailDesignator>
    {
        public const bool PRESERVE_COMPATIBLE_VALUES_DEFAULT = true;

        public static new string KindName { get { return Table.__ClassDefinitor.TechName; } }
        public static new string KindTitle { get { return Table.__ClassDefinitor.Name; } }
        public static new string KindSummary { get { return Table.__ClassDefinitor.Summary; } }
        public static new ImageSource KindPictogram { get { return Display.GetAppImage("table.png"); } }

        /// <summary>
        /// Static Constructor.
        /// </summary>
        static TableDetailDesignator()
        {
            __ClassDefinitor = new ModelClassDefinitor<TableDetailDesignator>("TableDetailDesignator", DetailDesignator.__ClassDefinitor, "Table Detail Designator",
                                                                              "Associates Table-Structure Definitions to an Idea, Idea-Definition or Field-Definition (Contained Table field type).");
            __ClassDefinitor.DeclareProperty(__DeclaringTableDef);
            __ClassDefinitor.DeclareProperty(__TableLook);
            __ClassDefinitor.DeclareProperty(__TableDefIsOwned);
            __ClassDefinitor.DeclareProperty(__ContainedTableSubOwner);

            __DeclaringTableDef.ForcedOwnershipIndicator = __TableDefIsOwned;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public TableDetailDesignator(Ownership<IdeaDefinition,Idea> Owner, TableDefinition DeclaringTableDef, bool TableDefIsOwned,
                                     string Name, string TechName, string Summary = "", ICopyable DefaultDetail = null,
                                     FieldDefinition ContainedTableSubOwner = null, ImageSource Pictogram = null)
            : base(Owner, Name, TechName, Summary, DefaultDetail, Pictogram)
        {
            // NOTE: Table-Structure Definition is not required until needed.

            this.DeclaringTableDefinition = DeclaringTableDef;
            this.TableDefIsOwned = TableDefIsOwned;
            this.TableLook = new TableAppearance();
            this.ContainedTableSubOwner = ContainedTableSubOwner;

            /* Cancelled: Better start as global/shared pointing to the default "Standard" table-struct.
            if (Owner != null)
                this.TableDefIsOwned = !Owner.IsGlobal; */
        }

        /// <summary>
        /// Protected Constructor for Agent descendants.
        /// </summary>
        protected TableDetailDesignator()
                : base()
        {
        }

        public Domain OwnerDomain { get { return this.Owner.GetValue(ideadef => ideadef.OwnerDomain, idea => idea.IdeaDefinitor.OwnerDomain); } }

        public string AssignerOwnerName { get { return ((IFormalizedElement)this.Owner.Owner).Name; } }

        /// <summary>
        /// Gets the predefined detail appearance.
        /// </summary>
        public override DetailAppearance DetailLook { get { return this.TableLook; } }

        public override IRecognizableElement Definitor { get { return this.DeclaringTableDefinition; } set { this.DeclaringTableDefinition = value as TableDefinition; } }

        public override IEnumerable<IRecognizableElement> AvailableDefinitors
        {
            get
            {
                var TableDefs = (this.Owner.GetValue<Composition>(ideadef => ideadef.OwnerDomain.OwnerComposition,
                                                                  idea => idea.IdeaDefinitor.OwnerDomain.OwnerComposition)).GetTableDefinitions();

                // This filter allows to manage separately the Custom-Fields.
                if (this.Alterability != EAlterability.System)
                    TableDefs = TableDefs.Where(tdef => tdef.Alterability != EAlterability.System);

                return TableDefs;
            }
        }

        /// <summary>
        /// Table-Structure Definition which declares the data structure of this Table.
        /// If reassigned, it automatically propagate structural changes to dependants.
        /// </summary>
        [Description("Table-Structure Definition which declares the data structure of this Table.")]
        public TableDefinition DeclaringTableDefinition { get { return __DeclaringTableDef.Get(this); } set { __DeclaringTableDef.Set(this, value); } }
        protected TableDefinition DeclaringTableDef_ = null;
        public static readonly ModelPropertyDefinitor<TableDetailDesignator, TableDefinition> __DeclaringTableDef =
                   new ModelPropertyDefinitor<TableDetailDesignator, TableDefinition>("DeclaringTableDef", EEntityMembership.External, null, EPropertyKind.Common,
                       ins => ins.DeclaringTableDef_,
                       (ins, val) =>
                       {
                           var PreviousTableDef = ins.DeclaringTableDef_;
                           ins.DeclaringTableDef_ = val;
                           if (val == null || PreviousTableDef == val)
                               return;

                           var PreserveCompatibleValues = AppExec.GetConfiguration<bool>("TableDetailDesignator", "StructureReplace.PreserveCompatibleValues", PRESERVE_COMPATIBLE_VALUES_DEFAULT);

                           // Propagate the replacement of the designated table-definition to the dependants.
                           var Dependents = ins.GetDesignatedTables();
                           foreach (var Dependant in Dependents)
                                Dependant.ApplyStructuralAlter(PreviousTableDef.StorageFieldDefinitions,
                                                               ins.DeclaringTableDef_.StorageFieldDefinitions,
                                                               ins.DeclaringTableDef_.UniqueKeyFieldDefs,
                                                               PreserveCompatibleValues);
                       }, false, false,
                       "Declaring Table-Structure Definition",
                       "Table-Structure Definition which declares the fields composing this Table.");

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the indirectly related Tables whose structure is defined by this Table-Structure Definition, via intermediate Table detail designators.
        /// </summary>
        public IEnumerable<Table> GetDesignatedTables(bool IncludeDomainTables = true, bool IncludeNestedTables = true)
        {
            if (this.Owner == null)
                return Enumerable.Empty<Table>();

            var OwnerComposition = this.Owner.GetValue(ideadef => ideadef.OwnerDomain.OwnerComposition, idea => idea.OwnerComposition);

            var Ideas = (OwnerComposition == null ? Enumerable.Empty<Idea>() : OwnerComposition.DeclaredIdeas);
            if (IncludeDomainTables && OwnerComposition != null)
                Ideas = Ideas.Concatenate(OwnerComposition.CompositeContentDomain.BaseContentRoot);

            var DetTables = Ideas.SelectMany(idea => idea.GetTables(IncludeNestedTables).Where(tab => tab.Designation == this))
                                    .Distinct().ToList();
            return DetTables;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Predefined table appearance.
        /// </summary>
        public TableAppearance TableLook { get { return __TableLook.Get(this); } set { __TableLook.Set(this, value); } }
        protected TableAppearance TableLook_ = null;
        public static readonly ModelPropertyDefinitor<TableDetailDesignator, TableAppearance> __TableLook =
                   new ModelPropertyDefinitor<TableDetailDesignator, TableAppearance>("TableLook", EEntityMembership.External, null, EPropertyKind.Common, ins => ins.TableLook_, (ins, val) => ins.TableLook_ = val, false, false,
                                                                                      "Table Look", "Predefined table appearance.");

        /// <summary>
        /// Indicates that the Table Definition belongs to the detail's owner (not shared).
        /// </summary>
        public bool TableDefIsOwned { get { return __TableDefIsOwned.Get(this); } set { __TableDefIsOwned.Set(this, value); } }
        protected bool TableDefIsOwned_ = false;
        public static readonly ModelPropertyDefinitor<TableDetailDesignator, bool> __TableDefIsOwned =
                   new ModelPropertyDefinitor<TableDetailDesignator, bool>("TableDefIsOwned", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.TableDefIsOwned_, (ins, val) => ins.TableDefIsOwned_ = val, false, false,
                                                                           "Table Definition Is Owned", "Indicates that the Table Definition belongs to the detail's owner (not shared).");

        /// <summary>
        /// If set, references the Field-Definition sub-owner of a field contained Table.
        /// IMPORTANT: In this case, the Owner must point to the related Domain.
        /// </summary>
        public FieldDefinition ContainedTableSubOwner { get { return __ContainedTableSubOwner.Get(this); } set { __ContainedTableSubOwner.Set(this, value); } }
        protected FieldDefinition ContainedTableSubOwner_ = null;
        public static readonly ModelPropertyDefinitor<TableDetailDesignator, FieldDefinition> __ContainedTableSubOwner =
                   new ModelPropertyDefinitor<TableDetailDesignator, FieldDefinition>("ContainedTableSubOwner", EEntityMembership.External, // IMPORTANT: If not declared as External, an infinite-loop can happen
                                                                                      null, EPropertyKind.Common, ins => ins.ContainedTableSubOwner_, (ins, val) => ins.ContainedTableSubOwner_ = val, false, false,
                                                                                      "Contained Table Sub-Owner", "If set, references the Field-Definition sub-owner of a field contained Table. IMPORTANT: In this case, the Owner must point to the related Domain.");

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelClass<TableDesignator> Members

        public new MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public new ModelClassDefinitor<TableDetailDesignator> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly new ModelClassDefinitor<TableDetailDesignator> __ClassDefinitor = null;

        public new TableDetailDesignator CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((TableDetailDesignator)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public TableDetailDesignator PopulateFrom(TableDetailDesignator SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}