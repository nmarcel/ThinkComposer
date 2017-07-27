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
// File   : TableDefinitionMaintainer.StructureDefinitions.cs
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
    /// Maintenance services for Table-Structure Definitions. Structure Definitions part.
    /// </summary>
    public static partial class TableDefinitionMaintainer
    {
        public static void SetFieldDefinitionsMaintainer(ItemsGridMaintainer<TableDefinition, FieldDefinition> TargetMaintainer)
        {
            TargetMaintainer.CreateItemOperation = FieldDefinitionCreate;
            TargetMaintainer.DeleteItemOperation = FieldDefinitionDelete;
            TargetMaintainer.EditItemOperation = FieldDefinitionEdit;
            TargetMaintainer.CloneItemOperation = FieldDefinitionClone;
        }

        public static FieldDefinition FieldDefinitionCreate(TableDefinition OwnerTableDef, IList<FieldDefinition> EditedList)
        {
            int NewNumber = EditedList.Count + 1;
            string NewName = "Field" + NewNumber.ToString();
            var Definitor = new FieldDefinition(OwnerTableDef, NewName, NewName.TextToIdentifier(), DataType.DataTypeText);

            if (FieldDefinitionEdit(OwnerTableDef, EditedList, Definitor))
                return Definitor;

            return null;
        }

        public static bool FieldDefinitionEdit(TableDefinition OwnerTableDef, IList<FieldDefinition> EditedList, FieldDefinition FieldDef)
        {
            var InstanceController = EntityInstanceController.AssignInstanceController(FieldDef);
            InstanceController.StartEdit();

            var ExtraControls = new List<UIElement>();
            MModelPropertyDefinitor ExposedProperty = null;

            // Declare expositor for hidden field
            var HideInDiagramExpositor = new EntityPropertyExpositor(FieldDefinition.__HideInDiagram.TechName);
            HideInDiagramExpositor.LabelMinWidth = 90;

            // Declare expositor for available values-sources
            var AvailableValuesSourcesExpositor = new EntityPropertyExpositor();
            ExposedProperty = FieldDefinition.__ValuesSource;

            AvailableValuesSourcesExpositor.ExposedProperty = ExposedProperty.TechName;
            AvailableValuesSourcesExpositor.LabelMinWidth = 90;
            AvailableValuesSourcesExpositor.IsEnabled = (FieldDef.FieldType is NumberType ||
                                                         FieldDef.FieldType.IsOneOf(DataType.DataTypeTableRecordRef, DataType.DataTypeText));  // Initial setting

            // Declare expositor for available Ideas
            var IdeaReferencingPropertyExpositor = new EntityPropertyExpositor();
            ExposedProperty = FieldDefinition.__IdeaReferencingProperty;

            IdeaReferencingPropertyExpositor.ExposedProperty = ExposedProperty.TechName;
            IdeaReferencingPropertyExpositor.LabelMinWidth = 90;
            IdeaReferencingPropertyExpositor.SetAvailable(FieldDef.FieldType.IsOneOf(DataType.DataTypeIdeaRef, DataType.DataTypeText));  // Initial setting

            // Declare button for table-definition assignation button
            var TableDefAssignationPanel = new StackPanel();
            // TableDefAssignationPanel.Orientation = Orientation.Horizontal;

            var TableDefAssignSingleRecordCbx = new EntityPropertyExpositor(FieldDefinition.__ContainedTableIsSingleRecord.TechName);
            TableDefAssignSingleRecordCbx.LabelMinWidth = 90;
            /* var TableDefAssignSingleRecordCbx = new CheckBox();
            TableDefAssignSingleRecordCbx.Content = FieldDefinition.__ContainedTableIsSingleRecord.Name; //  "Is Single-Record";
            TableDefAssignSingleRecordCbx.ToolTip = FieldDefinition.__ContainedTableIsSingleRecord.Summary;
            TableDefAssignSingleRecordCbx.FontSize = 8;
            TableDefAssignSingleRecordCbx.Margin = new Thickness(2); */
            TableDefAssignSingleRecordCbx.IsEnabled = FieldDef.FieldType.IsEqual(DataType.DataTypeTable);

            var TableDefAssignationButtonArea = new StackPanel();
            TableDefAssignationButtonArea.Orientation = Orientation.Horizontal;

            var TableDefAssignationButtonPrefix = new TextBlock();
            TableDefAssignationButtonPrefix.Text = "Table-Structure";
            TableDefAssignationButtonPrefix.TextAlignment = TextAlignment.Right;
            TableDefAssignationButtonPrefix.FontSize = 10;
            TableDefAssignationButtonPrefix.Width = 90;
            TableDefAssignationButtonPrefix.Margin = new Thickness(0,6,2,2);
            TableDefAssignationButtonPrefix.SetAvailable(FieldDef.FieldType == DataType.DataTypeTable);  // Initial setting

            var TableDefAssignationButton = new PaletteButton("Definition...", Display.GetAppImage("table.png"));
            TableDefAssignationButton.Margin = new Thickness(2 /*45*/, 2, 2, 2);
            TableDefAssignationButton.IsEnabled = FieldDef.FieldType.IsEqual(DataType.DataTypeTable);

            TableDefAssignationButtonArea.Children.Add(TableDefAssignationButtonPrefix);
            TableDefAssignationButtonArea.Children.Add(TableDefAssignationButton);

            TableDefAssignationButton.Click +=
                ((sender, args) =>
                    {
                        var DsnName = OwnerTableDef.Name + " - " + FieldDef.Name;

                        if (FieldDef.ContainedTableDesignator == null)
                        {
                            var ContainedTableDef = new TableDefinition(OwnerTableDef.OwnerDomain, DsnName, DsnName.TextToIdentifier());
                            FieldDef.ContainedTableDesignator = new TableDetailDesignator(Ownership.Create<IdeaDefinition, Idea>(OwnerTableDef.OwnerDomain),
                                                                                          ContainedTableDef, true /* Very important! */,
                                                                                          DsnName, DsnName.TextToIdentifier(), "", null, FieldDef);
                        }
                        else
                            if (FieldDef.ContainedTableDesignator.Name != DsnName)
                            {
                                FieldDef.ContainedTableDesignator.Name = DsnName;
                                FieldDef.ContainedTableDesignator.TechName = DsnName.TextToIdentifier();
                            }

                        var TableDefAssigner = new TableDetailDesignatorStructSubform(FieldDef.ContainedTableDesignator, null, true);

                        DialogOptionsWindow TableDesfAssignationWindow = null;  // Do not move outside this lambda

                        OwnerTableDef.EditEngine.StartCommandVariation("Edit Field-Definition type assignment of Table-Structure Definition");

                        var Response = Display.OpenContentDialogWindow(ref TableDesfAssignationWindow,
                                                                       "Table-Structure for Field '" + FieldDef.Name + "'",
                                                                       TableDefAssigner);
                        OwnerTableDef.EditEngine.CompleteCommandVariation();

                        if (Response.IsTrue())
                            FieldDef.ContainedTableDesignator.DeclaringTableDefinition.AlterStructure();
                        else
                            OwnerTableDef.EditEngine.Undo();
                    });

            // Declare expositor for field-type
            var FieldTypeExpositor = new EntityPropertyExpositor();
            ExposedProperty = FieldDefinition.__FieldType;
            
            FieldDef.PropertyChanged +=
                ((sender, args) =>
                    {
                        if (args.PropertyName != FieldDefinition.__FieldType.TechName
                            || FieldDef.FieldType == null)
                            return;

                        // Postcalls to be applied after the load initialization.

                        var CanSelectValues = (FieldDef.FieldType is NumberType ||
                                               FieldDef.FieldType.IsOneOf(DataType.DataTypeTableRecordRef, DataType.DataTypeText));
                        AvailableValuesSourcesExpositor.PostCall(
                            expo =>
                            {
                                expo.SetAvailable(CanSelectValues);

                                if (!CanSelectValues)
                                    FieldDef.ValuesSource = null;
                            });

                        var CanSelectIdeas = FieldDef.FieldType.IsOneOf(DataType.DataTypeIdeaRef, DataType.DataTypeText);
                        IdeaReferencingPropertyExpositor.PostCall(
                            expo =>
                            {
                                expo.SetAvailable(CanSelectIdeas);

                                if (!CanSelectIdeas)
                                    FieldDef.IdeaReferencingProperty = null;
                            });

                        var CanAssignTableDef = FieldDef.FieldType.IsEqual(DataType.DataTypeTable);
                        TableDefAssignationButton.PostCall(
                            ctrl =>
                            {
                                // ctrl.SetAvailable(CanAssignTableDef);
                                ctrl.IsEnabled = CanAssignTableDef;
                                /* Cancelled (better is to save user's data)...
                                if (!CanAssignTableDef)
                                {
                                    FieldDef.DeclaringTableDefinition = null;
                                    FieldDef.DeclaringTableDefIsOwned = false;
                                } */
                            });
                        TableDefAssignSingleRecordCbx.PostCall(ctrl => ctrl.IsEnabled = CanAssignTableDef);
                    });

            FieldTypeExpositor.ExposedProperty = ExposedProperty.TechName;
            FieldTypeExpositor.LabelMinWidth = 90;

            // Add the just created extra controls
            ExtraControls.Add(HideInDiagramExpositor);
            ExtraControls.Add(FieldTypeExpositor);
            ExtraControls.Add(AvailableValuesSourcesExpositor);
            ExtraControls.Add(IdeaReferencingPropertyExpositor);

            TableDefAssignationPanel.Children.Add(TableDefAssignationButtonArea);
            // POSTPONED: TableDefAssignationPanel.Children.Add(TableDefAssignSingleRecordCbx);
            ExtraControls.Add(TableDefAssignationPanel);

            var Result = InstanceController.Edit(Display.CreateEditPanel(FieldDef, null, true, null, null, true, false, false, true, ExtraControls.ToArray()),
                                                 "Edit Field Definition - " + FieldDef.ToString(), InitialWidth : 700).IsTrue();
            return Result;
        }

        public static bool FieldDefinitionDelete(TableDefinition OwnerEntity, IList<FieldDefinition> EditedList, FieldDefinition FieldDef)
        {
            var Result = Display.DialogMessage("Confirmation", "Are you sure you want to Delete the '" + FieldDef.Name + "' Field Definition?",
                                               EMessageType.Question, System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxResult.No);
            return (Result == MessageBoxResult.Yes);
        }

        public static FieldDefinition FieldDefinitionClone(TableDefinition OwnerEntity, IList<FieldDefinition> EditedList, FieldDefinition FieldDef)
        {
            var Result = new FieldDefinition();
            Result.PopulateFrom(FieldDef, null, ECloneOperationScope.Deep);

            var NamesWereEquivalent = (Result.TechName == Result.Name.TextToIdentifier());
            Result.Name = Result.Name + "(copy)";   // Auto-update of TechName when equivalents
            if (!NamesWereEquivalent) Result.TechName = Result.TechName + "_copy";

            return Result;
        }
    }
}
