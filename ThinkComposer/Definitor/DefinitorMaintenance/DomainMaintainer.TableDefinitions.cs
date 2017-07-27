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
// File   : DomainMaintainer.TableDefinitions.cs
// Object : Instrumind.ThinkComposer.Composer.DefinitorMaintenance.DomainMaintainer (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2010.02.22 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;
using Instrumind.Common.Visualization.Widgets;

using Instrumind.ThinkComposer;
using Instrumind.ThinkComposer.ApplicationProduct;
using Instrumind.ThinkComposer.Composer;
using Instrumind.ThinkComposer.Definitor.DefinitorMaintenance;
using Instrumind.ThinkComposer.Definitor.DefinitorUI.Widgets;
using Instrumind.ThinkComposer.Model;
using Instrumind.ThinkComposer.Model.GraphModel;
using Instrumind.ThinkComposer.MetaModel;
using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.MetaModel.InformationMetaModel;

/// Maintenance services of Domain related Definitions.
namespace Instrumind.ThinkComposer.Definitor.DefinitorMaintenance
{
    /// <summary>
    /// Provides maintenance services for a Domain entity. Table-Structure Definitions part.
    /// </summary>
    public static partial class DomainMaintainer
    {
        public static void SetTableDefinitionsMaintainer(ItemsGridMaintainer<Domain, TableDefinition> TargetMaintainer)
        {
            TargetMaintainer.CreateItemOperation = TableDefinitionCreate;
            TargetMaintainer.DeleteItemOperation = TableDefinitionDelete;
            TargetMaintainer.EditItemOperation = TableDefinitionEdit;
            TargetMaintainer.CloneItemOperation = TableDefinitionClone;
        }

        public static TableDefinition TableDefinitionCreate(Domain OwnerEntity, IList<TableDefinition> EditedList)
        {
            if (!ProductDirector.ValidateEditionPermission(AppExec.LIC_EDITION_PROFESSIONAL, "create Table-Structure Definitions"))
                return null;

            int NewNumber = EditedList.Count + 1;
            string NewName = "TableDef" + NewNumber.ToString();
            var Definitor = new TableDefinition(OwnerEntity, NewName, NewName.TextToIdentifier());

            if (TableDefinitionEdit(OwnerEntity, EditedList, Definitor))
                return Definitor;

            return null;
        }

        public static bool TableDefinitionEdit(Domain OwnerEntity, IList<TableDefinition> EditedList, TableDefinition TableDef)
        {
            return DomainServices.EditTableDefinition(OwnerEntity.EditEngine, TableDef).IsTrue();
        }

        public static bool TableDefinitionDelete(Domain OwnerEntity, IList<TableDefinition> EditedList, TableDefinition TableDef)
        {
            var Result = Display.DialogMessage("Confirmation", "Are you sure you want to Delete the '" + TableDef.Name + "' Table-Structure Definition?",
                                               EMessageType.Question, System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxResult.No);
            return (Result == MessageBoxResult.Yes);
        }

        public static TableDefinition TableDefinitionClone(Domain OwnerEntity, IList<TableDefinition> EditedList, TableDefinition TableDef)
        {
            var Result = new TableDefinition();
            Result.PopulateFrom(TableDef, null, ECloneOperationScope.Deep);

            var NamesWereEquivalent = (Result.TechName == Result.Name.TextToIdentifier());
            Result.Name = Result.Name + "(copy)";   // Auto-update of TechName when equivalents
            if (!NamesWereEquivalent) Result.TechName = Result.TechName + "_copy";

            return Result;
        }
    }
}
