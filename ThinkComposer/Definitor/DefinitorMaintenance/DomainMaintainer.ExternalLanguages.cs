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
// File   : DomainMaintainer.ExternalLanguages.cs
// Object : Instrumind.ThinkComposer.Composer.DefinitorMaintenance.DomainMaintainer (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2013.03.26 Néstor Sánchez A.  Creation
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
using Instrumind.ThinkComposer.MetaModel.Configurations;
using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;

/// Maintenance services of Domain related Definitions.
namespace Instrumind.ThinkComposer.Definitor.DefinitorMaintenance
{
    /// <summary>
    /// Provides maintenance services for a Domain entity. External Languages part.
    /// </summary>
    public static partial class DomainMaintainer
    {
        public static void SetExternalLanguagesMaintainer(ItemsGridMaintainer<Domain, ExternalLanguageDeclaration> TargetMaintainer)
        {
            TargetMaintainer.CreateItemOperation = ExternalLanguageCreate;
            TargetMaintainer.DeleteItemOperation = ExternalLanguageDelete;
            TargetMaintainer.EditItemOperation = ExternalLanguageEdit;
            TargetMaintainer.CloneItemOperation = ExternalLanguageClone;
        }

        public static ExternalLanguageDeclaration ExternalLanguageCreate(Domain OwnerEntity, IList<ExternalLanguageDeclaration> EditedList)
        {
            if (!ProductDirector.ValidateEditionPermission(AppExec.LIC_EDITION_PROFESSIONAL, "create External Languages declarations"))
                return null;

            int NewNumber = EditedList.Count + 1;
            string NewName = "ExternalLanguage" + NewNumber.ToString();
            var Definitor = new ExternalLanguageDeclaration(NewName, NewName.TextToIdentifier());

            if (ExternalLanguageEdit(OwnerEntity, EditedList, Definitor))
                return Definitor;

            return null;
        }

        public static bool ExternalLanguageEdit(Domain OwnerEntity, IList<ExternalLanguageDeclaration> EditedList, ExternalLanguageDeclaration ExternalLang)
        {
            var InstanceController = EntityInstanceController.AssignInstanceController(ExternalLang);
            InstanceController.StartEdit();

            var EditPanel = Display.CreateEditPanel(ExternalLang, null, true, null, null, true, false, true, false /* Maybe some day the Tech-Spec could store particularities of the language */);

            var Result = InstanceController.Edit(EditPanel, "Edit External Language declaration - " + ExternalLang.ToString()).IsTrue();

            if (Result)
                OwnerEntity.CurrentExternalLanguage = ExternalLang;

            return Result;
        }

        public static bool ExternalLanguageDelete(Domain OwnerEntity, IList<ExternalLanguageDeclaration> EditedList, ExternalLanguageDeclaration ExternalLang)
        {
            if (EditedList.Count <= 1)
            {
                Display.DialogMessage("Attention!", "At least one External Language must exist.");
                return false;
            }

            var Result = Display.DialogMessage("Confirmation", "Are you sure you want to Delete the '" + ExternalLang.Name + "' External Language declaration?",
                                               EMessageType.Question, System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxResult.No);
            return (Result == MessageBoxResult.Yes);
        }

        public static ExternalLanguageDeclaration ExternalLanguageClone(Domain OwnerEntity, IList<ExternalLanguageDeclaration> EditedList, ExternalLanguageDeclaration ExternalLang)
        {
            var Result = ExternalLang.CreateClone(ECloneOperationScope.Deep, null);

            var NamesWereEquivalent = (Result.TechName == Result.Name.TextToIdentifier());
            Result.Name = Result.Name + "(copy)";   // Auto-update of TechName when equivalents
            if (!NamesWereEquivalent) Result.TechName = Result.TechName + "_copy";

            return Result;
        }
    }
}
