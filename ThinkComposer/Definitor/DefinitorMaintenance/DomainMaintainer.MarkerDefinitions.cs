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
// File   : DomainMaintainer.MarkerDefinitions.cs
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

/// Maintenance services of Domain related Definitions.
namespace Instrumind.ThinkComposer.Definitor.DefinitorMaintenance
{
    /// <summary>
    /// Provides maintenance services for a Domain entity. Marker Definitions part.
    /// </summary>
    public static partial class DomainMaintainer
    {
        public static void SetMarkerDefinitionsMaintainer(ItemsGridMaintainer<Domain, MarkerDefinition> TargetMaintainer)
        {
            TargetMaintainer.CreateItemOperation = MarkerDefinitionCreate;
            TargetMaintainer.DeleteItemOperation = MarkerDefinitionDelete;
            TargetMaintainer.EditItemOperation = MarkerDefinitionEdit;
            TargetMaintainer.CloneItemOperation = MarkerDefinitionClone;
        }

        public static MarkerDefinition MarkerDefinitionCreate(Domain OwnerEntity, IList<MarkerDefinition> EditedList)
        {
            if (!ProductDirector.ValidateEditionPermission(AppExec.LIC_EDITION_LITE, "create Marker Definitions"))
                return null;

            int NewNumber = EditedList.Count + 1;
            string NewName = "MarkerDef" + NewNumber.ToString();
            var Definitor = new MarkerDefinition(NewName, NewName.TextToIdentifier(), "", Display.GetAppImage("rosette.png"));

            if (MarkerDefinitionEdit(OwnerEntity, EditedList, Definitor))
                return Definitor;

            return null;
        }

        public static bool MarkerDefinitionEdit(Domain OwnerEntity, IList<MarkerDefinition> EditedList, MarkerDefinition MarkerDef)
        {
            /*- if (!ProductDirector.ConfirmImmediateApply("Marker Definition", "DomainEdit.MarkerDefinition", "ApplyDialogChangesDirectly"))
                return false; */

            var InstanceController = EntityInstanceController.AssignInstanceController(MarkerDef);
            InstanceController.StartEdit();

            /* POSTPONED: Ability to edit and reorganize Markers via its Cluster-Key.
            var Expositor = new EntityPropertyExpositor(MarkerDefinition.__ClusterKey.TechName);
            Expositor.LabelMinWidth = 90; */

            var EditPanel = Display.CreateEditPanel(MarkerDef, null, true, null, null, true, false, true /*, Expositor*/);

            return InstanceController.Edit(EditPanel, "Edit Marker Definition - " + MarkerDef.ToString()).IsTrue();
        }

        public static bool MarkerDefinitionDelete(Domain OwnerEntity, IList<MarkerDefinition> EditedList, MarkerDefinition MarkerDef)
        {
            var Result = Display.DialogMessage("Confirmation", "Are you sure you want to Delete the '" + MarkerDef.Name + "' Marker Definition?",
                                               EMessageType.Question, System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxResult.No);
            return (Result == MessageBoxResult.Yes);
        }

        public static MarkerDefinition MarkerDefinitionClone(Domain OwnerEntity, IList<MarkerDefinition> EditedList, MarkerDefinition MarkerDef)
        {
            var Result = new MarkerDefinition();
            Result.PopulateFrom(MarkerDef, null, ECloneOperationScope.Deep);

            var NamesWereEquivalent = (Result.TechName == Result.Name.TextToIdentifier());
            Result.Name = Result.Name + "(copy)";   // Auto-update of TechName when equivalents
            if (!NamesWereEquivalent) Result.TechName = Result.TechName + "_copy";

            return Result;
        }
    }
}
