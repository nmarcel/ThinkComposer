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
// File   : TableDefinitionMaintainer.cs
// Object : Instrumind.ThinkComposer.Composer.DefinitorMaintenance.TableDefinitionMaintainer (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2010.01.07 Néstor Sánchez A.  Creation
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
using Instrumind.ThinkComposer.MetaModel.InformationMetaModel;
using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;

/// Maintenance services of Domain related Definitions.
namespace Instrumind.ThinkComposer.Definitor.DefinitorMaintenance
{
    /// <summary>
    /// Maintenance services for Table-Structure Definitions.
    /// </summary>
    public static partial class TableDefinitionMaintainer
    {
        public const bool PRESERVE_COMPATIBLE_VALUES_DEFAULT = true;

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Detects Table-Structure Definition structural changes, and aplying them to the original-entity.
        /// Plus, inform those changes to the registered dependant Designators of this Table-Structure Definition.
        /// </summary>
        public static bool ApplyTableDefStructureAlter(TableDefinition CurrentEntityState, TableDefinition PreviousEntityState,
                                                       IEnumerable<object> Parameters = null)  // Parameters required for instance-controller pre-apply invocation
        {
            //! Fix crash after deleting field which is not removed at the stored value cells level
            var OmitWarning = (Parameters != null && Parameters.Any() && Parameters.First() is bool && (bool)Parameters.First());

            // Ensure deletion of nonexistent field definitions from complementary field collections.
            var NonexistentUniqueKeyFields = CurrentEntityState.UniqueKeyFieldDefs.Where(fld => !CurrentEntityState.FieldDefinitions.Contains(fld)).ToList();
            NonexistentUniqueKeyFields.ForEach(nonex => CurrentEntityState.UniqueKeyFieldDefs.Remove(nonex));

            var NonexistentDominantRefFields = CurrentEntityState.DominantRefFieldDefs.Where(fld => !CurrentEntityState.FieldDefinitions.Contains(fld)).ToList();
            NonexistentDominantRefFields.ForEach(nonex => CurrentEntityState.DominantRefFieldDefs.Remove(nonex));

            var NonexistentLabelFields = CurrentEntityState.LabelFieldDefs.Where(fld => !CurrentEntityState.FieldDefinitions.Contains(fld)).ToList();
            NonexistentLabelFields.ForEach(nonex => CurrentEntityState.LabelFieldDefs.Remove(nonex));

            // Detect changes between original-entity field sets and those of the edited-agent.
            // Notice the safe non-by-instance comparison made by GlobalId (a GUID), which is not recreated at cloning.
            // Here, a change in relevant properties, such as Data-Type or Unique-Key fields order can happen.
            var DetectedFieldDefinitionChanges = PreviousEntityState.FieldDefinitions.GetDifferencesFrom(CurrentEntityState.FieldDefinitions, (current, changed) => current.GlobalId == changed.GlobalId, General.DetermineDifferences, General.IsEquivalent);
            var DetectedUniqueKeyFieldsChanges = PreviousEntityState.UniqueKeyFieldDefs.GetDifferencesFrom(CurrentEntityState.UniqueKeyFieldDefs, (current, changed) => current.GlobalId == changed.GlobalId, General.DetermineDifferences, General.IsEquivalent);
            var DetectedDominantRefFieldsChanges = PreviousEntityState.DominantRefFieldDefs.GetDifferencesFrom(CurrentEntityState.DominantRefFieldDefs, (current, changed) => current.GlobalId == changed.GlobalId, General.DetermineDifferences, General.IsEquivalent);
            var DetectedLabelFieldsChanges = PreviousEntityState.LabelFieldDefs.GetDifferencesFrom(CurrentEntityState.LabelFieldDefs, (current, changed) => current.GlobalId == changed.GlobalId, General.DetermineDifferences, General.IsEquivalent);
            var DetectedFieldDefDataTypeChanges = PreviousEntityState.FieldDefinitions.Where(
                                                    current =>
                                                    {
                                                        var Changed = CurrentEntityState.FieldDefinitions.FirstOrDefault(changed => changed.GlobalId == current.GlobalId);
                                                        if (Changed == null)
                                                            return false;

                                                        var AreDifferent = !current.FieldType.IsEqual(Changed.FieldType);
                                                        return AreDifferent;
                                                    }).ToList();

            // If no changes were made, then exit and proceed with non table-structure-changes Apply.
            if (DetectedFieldDefinitionChanges == null && DetectedUniqueKeyFieldsChanges == null
                && DetectedDominantRefFieldsChanges == null && DetectedLabelFieldsChanges == null
                && DetectedFieldDefDataTypeChanges.Count < 1)
                return true;

            /* Determine if structural alterations (which implies Table data change/loss) were made.
             * ( The items of the Tuples reporting Field-Definition changes are...
             *   [1]Created (informing new position),  [2]Deleted (informing previous position),
             *   [3]Moved (informing new and previous positions), [4]Modified Data (informing new item position, plus field-name, new and previous values) )*/
 
            var FieldsSetAlteration = (DetectedFieldDefinitionChanges != null
                                       && (DetectedFieldDefinitionChanges.Item1.Count > 0
                                           || DetectedFieldDefinitionChanges.Item2.Count > 0
                                           || DetectedFieldDefinitionChanges.Item4.Any(chg => chg.Value.Any(fld => fld.Key == FieldDefinition.__FieldType.TechName))));

            // Notice that Moved field-defs are not considered.
            // HOWEVER: These key field-defs should be used to Sort by default.
            var UniqueKeyAlteration = (DetectedUniqueKeyFieldsChanges != null
                                       && (DetectedUniqueKeyFieldsChanges.Item1.Count > 0
                                           || DetectedUniqueKeyFieldsChanges.Item2.Count > 0));

            if (FieldsSetAlteration || UniqueKeyAlteration)
            {
                var AffectedObjects = new List<SimpleElement>();
                var DepDesignators = PreviousEntityState.GetDependentDesignators(); // Old: CurrentEntityState
                foreach(var Dependant in DepDesignators)
                    AffectedObjects.Add(Dependant.Owner.GetValue(defn => new SimpleElement("Idea Definition: " + defn.Name, "", defn.Summary),
                                                                 idea => new SimpleElement("Idea: " + idea.Name + " - Detail: " + Dependant.Name, "", idea.Summary)));

                if (!OmitWarning && AffectedObjects.Count > 0
                    && !Display.DialogMultiItem("Confirmation", "The structural changes just made will affect the next Idea-Definitions/Ideas...",
                                                null, null, true, AffectedObjects.ToArray()))
                    return false;
            }

            var PreserveCompatibleValues = AppExec.GetConfiguration<bool>("TableDefinition", "StructureAlter.PreserveCompatibleValues", PRESERVE_COMPATIBLE_VALUES_DEFAULT);

            if (!FieldsSetAlteration && !UniqueKeyAlteration)
                return true;

            CurrentEntityState.AlterStructure(PreserveCompatibleValues, PreviousEntityState.FieldDefinitions);     // Alter the entity actual structure and propagate to dependants.

            return true;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}
