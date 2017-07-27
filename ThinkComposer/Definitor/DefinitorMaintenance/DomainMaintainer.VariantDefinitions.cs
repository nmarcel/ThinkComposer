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
// File   : DomainMaintainer.VariantDefinitions.cs
// Object : Instrumind.ThinkComposer.Composer.DefinitorMaintenance.DomainMaintainer (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2011.10.12 Néstor Sánchez A.  Creation
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
    /// Provides maintenance services for a Domain entity. Variant Definitions part.
    /// </summary>
    public static partial class DomainMaintainer
    {
        public static void SetVariantDefinitionsMaintainer(ItemsGridMaintainer<Domain, SimplePresentationElement> TargetMaintainer)
        {
            TargetMaintainer.CreateItemOperation = VariantDefinitionCreate;
            TargetMaintainer.DeleteItemOperation = VariantDefinitionDelete;
            TargetMaintainer.EditItemOperation = VariantDefinitionEdit;
            TargetMaintainer.CloneItemOperation = VariantDefinitionClone;
        }

        public static SimplePresentationElement VariantDefinitionCreate(Domain OwnerEntity, IList<SimplePresentationElement> EditedList)
        {
            if (!ProductDirector.ValidateEditionPermission(AppExec.LIC_EDITION_PROFESSIONAL, "create Link-Role Variant Definitions"))
                return null;

            int NewNumber = EditedList.Count + 1;
            string NewName = "VariantDef" + NewNumber.ToString();
            var Definitor = new SimplePresentationElement(NewName, NewName.TextToIdentifier());

            if (VariantDefinitionEdit(OwnerEntity, EditedList, Definitor))
                return Definitor;

            return null;
        }

        public static bool VariantDefinitionEdit(Domain OwnerEntity, IList<SimplePresentationElement> EditedList, SimplePresentationElement VariantDef)
        {
            /*- if (!ProductDirector.ConfirmImmediateApply("Variant Definition", "DomainEdit.VariantDefinition", "ApplyDialogChangesDirectly"))
                return false; */

            var InstanceController = EntityInstanceController.AssignInstanceController(VariantDef);
            InstanceController.StartEdit();

            var EditPanel = Display.CreateEditPanel(VariantDef, null, true, null, null, true, false, true /*, Expositor*/);

            return InstanceController.Edit(EditPanel, "Edit Link-Role Variant Definition - " + VariantDef.ToString()).IsTrue();
        }

        public static bool VariantDefinitionDelete(Domain OwnerEntity, IList<SimplePresentationElement> EditedList, SimplePresentationElement VariantDef)
        {
            var Result = Display.DialogMessage("Confirmation", "Are you sure you want to Delete the '" + VariantDef.Name + "' Variant Definition?",
                                               EMessageType.Question, System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxResult.No);
            return (Result == MessageBoxResult.Yes);
        }

        public static SimplePresentationElement VariantDefinitionClone(Domain OwnerEntity, IList<SimplePresentationElement> EditedList, SimplePresentationElement VariantDef)
        {
            var Result = VariantDef.CreateClone(ECloneOperationScope.Deep, null);

            var NamesWereEquivalent = (Result.TechName == Result.Name.TextToIdentifier());
            Result.Name = Result.Name + "(copy)";   // Auto-update of TechName when equivalents
            if (!NamesWereEquivalent) Result.TechName = Result.TechName + "_copy";

            return Result;
        }
    }
}
