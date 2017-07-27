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
// File   : DomainServices.cs
// Object : Instrumind.ThinkComposer.Definitor.DomainServices (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2010.02.24 Néstor Sánchez A.  Creation
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;
using Instrumind.Common.Visualization.Widgets;

using Instrumind.ThinkComposer.ApplicationProduct;
using Instrumind.ThinkComposer.Composer;
using Instrumind.ThinkComposer.Composer.Generation;
using Instrumind.ThinkComposer.Definitor.DefinitorMaintenance;
using Instrumind.ThinkComposer.Definitor.DefinitorUI;
using Instrumind.ThinkComposer.Definitor.DefinitorUI.Widgets;
using Instrumind.ThinkComposer.MetaModel;
using Instrumind.ThinkComposer.MetaModel.Configurations;
using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.MetaModel.InformationMetaModel;
using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;
using Instrumind.ThinkComposer.Model;
using Instrumind.ThinkComposer.Model.GraphModel;
using Instrumind.ThinkComposer.Model.InformationModel;
using Instrumind.ThinkComposer.Model.VisualModel;

/// Manages metadata about user defined Graphs and Visuals.
namespace Instrumind.ThinkComposer.Definitor
{
    /// <summary>
    /// Provides services for the alteration and application of Domains toward their consuming Compositions.
    /// </summary>
    public static class DomainServices
    {
        // Keys of the Tabs shown
        public const string TABKEY_DEF_CONCEPTS = "DEF_CON";
        public const string TABKEY_DEF_RELATIONSHIPS = "DEF_REL";
        public const string TABKEY_DEF_MARKINGS = "DEF_MRK";
        public const string TABKEY_DEF_FORMAT = "DEF_FMT";
        public const string TABKEY_DEF_DETAILS = "DEF_DET";
        public const string TABKEY_DEF_ARRANGE = "DEF_ARN";
        public const string TABKEY_DEF_OUTTEMPLATE = "DEF_OTP";
        public const string TABKEY_DEF_NOMENCLATURE = "DEF_NOM";

        public const string TABKEY_TBD_COLDEFS = "TBL_COLDEFS";
        public const string TABKEY_TBD_STR_DEF = "TBL_STRUCT_DEF";

        public const string TABKEY_IDEF_CLUSTER_CONCEPT = "IDEF_CLUCON";
        public const string TABKEY_IDEF_CLUSTER_RELATIONSHIP = "IDEF_CLUREL";

        // ===============================================================================================================================================================
        /* POSTPONED: For when a Composition can have more than one domain
        /// <summary>
        /// Opens an interactive editor for manage all the Composition related Domains.
        /// </summary>
        public static void ManageDomains()
        {
        } */

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static DialogOptionsWindow SelectionWindow = null;

        /// <summary>
        /// Exposes to the user a line/connector format (brush + thickness + dash-style) selector for the supplied Initial format.
        /// Returns indication of success (true, false=cancelled) plus the new brush (which can be null).
        /// </summary>
        public static Tuple<bool, Brush, double, DashStyle> DialogSelectFormat(Brush InitialBrush, double InitialThickness, DashStyle InitialDashStyle)
        {
            Brush SelectedBrush = InitialBrush;
            double SelectedThickness = InitialThickness;
            DashStyle SelectedDashStyle = InitialDashStyle;
            var BrushSelector = new BrushSelector(InitialBrush);

            var MainPanel = new StackPanel();

            var InitialPanel = new Grid();
            InitialPanel.ColumnDefinitions.Add(new ColumnDefinition());
            InitialPanel.ColumnDefinitions.Add(new ColumnDefinition());

            var DashStylePanel = new StackPanel();
            InitialPanel.Children.Add(DashStylePanel);
            Grid.SetColumn(DashStylePanel, 0);

            DashStylePanel.Children.Add(new TextBlock { Text = "Dash-Style", Margin = new Thickness(2) });
            var Combo = new ComboBox();
            Combo.Margin = new Thickness(2);
            Combo.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            Combo.ItemTemplate = Display.GetResource<DataTemplate>("TplSimplePresentationElement");
            Combo.ItemsSource = MasterDrawer.AvailableDashStyles;
            Combo.SelectedItem = MasterDrawer.AvailableDashStyles.First(ds => ds.TechName == Display.DeclaredDashStyles.FirstOrInitial(reg => reg.Item1 == InitialDashStyle).Item2);
            Combo.SelectionChanged += ((sender, args) =>
            {
                if (args.AddedItems == null || args.AddedItems.Count < 1) return;
                SelectedDashStyle = Display.DeclaredDashStyles.First(reg => reg.Item2 == ((SimplePresentationElement)args.AddedItems[0]).TechName).Item1;
            });
            DashStylePanel.Children.Add(Combo);

            var ThicknessPanel = new StackPanel();
            InitialPanel.Children.Add(ThicknessPanel);
            Grid.SetColumn(ThicknessPanel, 1);

            ThicknessPanel.Children.Add(new TextBlock { Text = "Thickness", Margin = new Thickness(2) });
            Combo = new ComboBox();
            Combo.Margin = new Thickness(2);
            Combo.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            Combo.ItemTemplate = Display.GetResource<DataTemplate>("TplSimplePresentationElement");
            Combo.ItemsSource = MasterDrawer.AvailableThicknesses;
            Combo.SelectedItem = MasterDrawer.AvailableThicknesses.FirstOrInitial(ds => ds.TechName == InitialThickness.ToString(CultureInfo.InvariantCulture.NumberFormat));
            Combo.SelectionChanged += ((sender, args) =>
            {
                if (args.AddedItems == null || args.AddedItems.Count < 1) return;
                SelectedThickness = double.Parse(((SimplePresentationElement)args.AddedItems[0]).TechName, CultureInfo.InvariantCulture.NumberFormat);
            });
            ThicknessPanel.Children.Add(Combo);

            MainPanel.Children.Add(InitialPanel);

            BrushSelector.SelectionAction =
                (selectedbrush =>
                {
                    if (selectedbrush == null)  // If selection cancelled, then exit
                        return;

                    SelectedBrush = selectedbrush.Item1;
                    SelectionWindow.Close();
                });

            MainPanel.Children.Add(new TextBlock { Text = "Color brush" });
            MainPanel.Children.Add(BrushSelector);

            var Answer = Display.OpenContentDialogWindow<StackPanel>(ref SelectionWindow, "Format...", MainPanel).IsTrue();

            var Result = new Tuple<bool, Brush, double, DashStyle>(Answer, SelectedBrush, SelectedThickness, SelectedDashStyle);
            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static TableDefinition CreateTableDefinition(EntityEditEngine Engine, Domain OwnerDomain, TableDefinition CloningSource = null)
        {
            TableDefinition TableDef = null;

            if (CloningSource == null)
            {
                string NewDefName = "Table-Structure Definition " + (OwnerDomain.TableDefinitions.Count + 1).ToString();
                TableDef = new TableDefinition(OwnerDomain, NewDefName, NewDefName.TextToIdentifier(), "Definition of Table-Structure");
            }
            else
            {
                TableDef = CloningSource.CreateClone(ECloneOperationScope.Deep, null);
                TableDef.Name = "Clone of " + TableDef.Name + " - " + DateTime.Now.ToString("yyyyMMddHHmmss"); ;
                TableDef.TechName = TableDef.Name.TextToIdentifier();
            }

            if (!DomainServices.EditTableDefinition(Engine, TableDef).IsTrue())
                return null;

            OwnerDomain.TableDefinitions.Add(TableDef);

            return TableDef;
        }

        /// <summary>
        /// Opens the properties interactive editor of the specified Engine and Table-Structure Definition.
        /// Optionally, indication for including table-structure and an initial opened tab key can be specified.
        /// Returns success status of the dialog window invocation.
        /// </summary>
        public static bool? EditTableDefinition(EntityEditEngine Engine, TableDefinition Target,
                                                bool IncludeStructure = true, string OpenedTabKey = null)
        {
            /*- if (!ProductDirector.ConfirmImmediateApply("Table-Structure Definition", "DomainEdit.TableDefinition", "ApplyDialogChangesDirectly"))
                return false; */

            General.ContractRequiresNotNull(Target);

            var InstanceController = EntityInstanceController.AssignInstanceController(Target, TableDefinitionMaintainer.ApplyTableDefStructureAlter,
                                                                                       ECloneOperationScope.DeepAndEquivalent);
            InstanceController.StartEdit();

            var TitleKind = "Descriptor";
            var SpecTabs = (IEnumerable<TabItem>)null;

            if (IncludeStructure)
            {
                TitleKind = "Definition";

                var StructureSubform = new TableStructureSubform(Target);
                TableDefinitionMaintainer.SetFieldDefinitionsMaintainer(StructureSubform.FieldsMaintainer);
                StructureSubform.FieldsMaintainer.VisualControl.CanEditItemsDirectly = false;

                SpecTabs = TabbedEditPanel.CreateTab(TABKEY_TBD_COLDEFS, "Structure", "Elements composing the table structure, such as Fields, Unique-Key, etc.",
                                                     StructureSubform).IntoEnumerable();
            }

            var EditPanel = Display.CreateEditPanel(Target, SpecTabs, true, OpenedTabKey, null, true, true, true /*+ Add Tech-Spec when useful */);

            var Result = InstanceController.Edit(EditPanel, "Edit Table-Structure " + TitleKind + " - " + Target.ToString());

            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Opens the properties interactive editor of the specified Detail Designator.
        /// Optionally, an indication of table data import enabled can be specified.
        /// Returns success status of the dialog window invocation and optional loaded table data.
        /// </summary>
        public static Tuple<bool?, List<List<object>>> EditDetailDesignator(DetailDesignator Designator, bool AtGlobalEditing,
                                                                            EntityEditEngine Editor, bool CanImportTableData = false)
        {
            General.ContractRequiresNotNull(Designator);

            FrameworkElement ImportWidget = null;

            var InstanceController = EntityInstanceController.AssignInstanceController(Designator);
            InstanceController.StartEdit();

            var SpecTabs = new List<TabItem>();

            var DesignatorOwner = Designator.Owner.GetValue(ideadef => ideadef.OwnerDomain.BaseContentRoot, idea => idea);

            var TableDesignator = Designator as TableDetailDesignator;
            if (TableDesignator != null)
            {
                if (CanImportTableData)
                    ImportWidget = CreateTransferWidget(TableDesignator, DesignatorOwner, true, true, null, null,
                                                        (widget) =>
                                                        {
                                                            var EntEdit = widget.GetNearestVisualDominantOfType<EntityEditPanel>();
                                                            if (EntEdit == null) return;
                                                            EntEdit.BtnOK_Click(widget, null);
                                                        });

                if (TableDesignator.Owner.IsGlobal == AtGlobalEditing)
                {
                    var TableStructTab = new TableDetailDesignatorStructSubform(TableDesignator, ImportWidget);
                    SpecTabs.Add(TabbedEditPanel.CreateTab(TABKEY_TBD_STR_DEF, "Table-Structure", "Defines a Table-Structure or selects a predefined one.",
                                                           TableStructTab));
                }
            }

            var EditPanel = Display.CreateEditPanel(Designator, SpecTabs, true, null, null, false, false, false, true, ImportWidget);

            var DialogResult = InstanceController.Edit(EditPanel, "Edit Detail Designator - " + Designator.Name);

            if (DialogResult.IsTrue() && TableDesignator != null
                && (TableDesignator.DeclaringTableDefinition == null
                    || TableDesignator.DeclaringTableDefinition.FieldDefinitions.Count < 1))
            {
                Display.DialogMessage("Attention!",
                                      "No Table-Structure with Field Definitions was specified.", EMessageType.Warning);
                DialogResult = false;   // Cancels if no table-definition is assigned.
            }

            var Result = Tuple.Create(DialogResult, LastLoadedTableData);
            LastLoadedTableData = null;

            return Result;
        }

        private static Uri LastTableDataLocation = null;

        private static List<List<object>> LastLoadedTableData = null;

        /// <summary>
        /// Creates and returns a widget for data transfer operations (import/export)
        /// </summary>
        public static FrameworkElement CreateTransferWidget(TableDetailDesignator WorkingDesignator, Idea TargetTableOwner,
                                                            bool CreateTableDefIfNecessary = true, bool CanUserChooseCompatibleTableDef = false,
                                                            Action<TableDefinition, List<List<object>>, bool> ApplyImportOperation = null,
                                                            Table ExportSource = null, Action<UIElement> ImmediateApply = null)
        {
            var TransferWidget = new Border();
            // TransferWidget.Background = Display.GetResource<Brush, EntitledPanel>("ExpositorBrush");
            TransferWidget.CornerRadius = new CornerRadius(3);
            TransferWidget.Padding = new Thickness(4);
            TransferWidget.Margin = new Thickness(2);

            var TransferFileName = new TextBox();
            TransferFileName.MinWidth = 300;
            TransferFileName.IsReadOnly = true;
            TransferFileName.FontSize = 10;
            TransferFileName.Margin = new Thickness(0, 0, 2, 0);

            var ImportText = new TextBlock();
            ImportText.Text = "Load" + (ImmediateApply == null ? "" : " (Now!)");
            ImportText.TextAlignment = TextAlignment.Right;
            ImportText.Margin = new Thickness(2);
            ImportText.ToolTip = "Indicates source of Table data for loading.";
            ImportText.FontSize = 10;
            ImportText.Width = 57;

            if (ApplyImportOperation != null)
            {
                ImportText.SetVisible(false);
                TransferFileName.SetVisible(false);
            }

            var TransferFileHasHeaderChecker = new CheckBox();
            TransferFileHasHeaderChecker.Content = "Headered";
            TransferFileHasHeaderChecker.FontSize = 10;
            TransferFileHasHeaderChecker.ToolTip = "Indicates whether the file's first row has column names.";
            TransferFileHasHeaderChecker.IsChecked = true;
            TransferFileHasHeaderChecker.Margin = new Thickness(2, 4, 2, 0);
            DockPanel.SetDock(TransferFileHasHeaderChecker, Dock.Right);

            var TransferFileAppendChecker = new CheckBox();
            TransferFileAppendChecker.Content = "Append";
            TransferFileAppendChecker.FontSize = 10;
            TransferFileAppendChecker.ToolTip = "Indicates whether to append records at the end, else reset the current list of records.";
            TransferFileAppendChecker.IsChecked = false;
            TransferFileAppendChecker.Margin = new Thickness(2, 4, 2, 0);
            DockPanel.SetDock(TransferFileAppendChecker, Dock.Right);

            var ImportButton = new PaletteButton("Import...", Display.GetAppImage("folder_table.png"), Summary: "Select file to be loaded.");

            ImportButton.Margin = new Thickness(0, 0, 4, 0);
            ImportButton.FontSize = 10;
            ImportButton.FontWeight = FontWeights.Bold;
            ImportButton.BorderBrush = Brushes.LightGray;
            ImportButton.BorderThickness = new Thickness(1);
            ImportButton.Click +=
                ((sender, evargs) =>
                {
                    LastTableDataLocation = Display.DialogGetOpenFile("Import data from...", ".txt",
                                                                      "Tab-separated (multiple values per line) files (*.tab, *.tsv, *.dat, *.txt)|*.tab;*.tsv;*.dat;*.txt|" +
                                                                      "Comma-separated (multiple values per line) files (*.csv)|*.csv|" +
                                                                      "List/plain-text (single value per line) files (*.lis, *.lst, *.prn)|*.lis;*.lst;*.prn");
                    if (LastTableDataLocation == null)
                        return;

                    TransferFileName.Text = LastTableDataLocation.LocalPath;

                    var Result = ImportTableDataFromFile(LastTableDataLocation, TransferFileHasHeaderChecker.IsChecked.IsTrue(),
                                                         WorkingDesignator.DeclaringTableDefinition, CreateTableDefIfNecessary, CanUserChooseCompatibleTableDef);

                    //- Display.DialogMessage("Attention", "Cannot import data file.\nPossible incompatible Table structure.", EMessageType.Warning);
                    if (Result != null)
                    {
                        /* Cancelled: Do not change what the user has entered.
                        var FileName = Path.GetFileNameWithoutExtension(LastTableDataLocation.LocalPath);
                        WorkingDesignator.Name = FileName; */

                        LastLoadedTableData = Result.Item2;

                        if (ApplyImportOperation != null)
                            ApplyImportOperation(Result.Item1, Result.Item2, TransferFileAppendChecker.IsChecked.IsTrue());
                        else
                        {
                            TransferWidget.Tag = Tuple.Create(Result.Item1, Result.Item2);

                            if (ImmediateApply != null)
                                ImmediateApply(TransferWidget);   // Applied immediately, not giving chance to the user to change the table structure.
                        }
                    }
                });
            DockPanel.SetDock(ImportButton, Dock.Right);

            TransferFileName.MouseLeftButtonUp +=
                ((sender, evargs) => ImportButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent)));

            PaletteButton ExportButton = null;

            if (ExportSource != null)
            {
                ExportButton = new PaletteButton("Export...", Display.GetAppImage("table_save.png"), Summary: "Select file to export data into.");

                ExportButton.Margin = new Thickness(0, 0, 4, 0);
                ExportButton.FontSize = 10;
                ExportButton.FontWeight = FontWeights.Bold;
                ExportButton.BorderBrush = Brushes.LightGray;
                ExportButton.BorderThickness = new Thickness(1);
                ExportButton.Click +=
                    ((sender, evargs) =>
                    {
                        var Result = ExportTableDataToFile(ExportSource, null, TransferFileHasHeaderChecker.IsChecked.IsTrue(),
                                                           TransferFileAppendChecker.IsChecked.IsTrue());

                        if (Result != null && Result.Item1 != null)
                            TransferFileName.Text = Result.Item1.LocalPath;
                    });
                DockPanel.SetDock(ExportButton, Dock.Right);
            }

            var TransferPanel = new DockPanel();

            TransferPanel.Children.Add(ImportText);

            if (ApplyImportOperation != null)
                TransferPanel.Children.Add(TransferFileAppendChecker);

            TransferPanel.Children.Add(TransferFileHasHeaderChecker);

            if (ExportButton != null)
                TransferPanel.Children.Add(ExportButton);

            TransferPanel.Children.Add(ImportButton);

            TransferPanel.Children.Add(TransferFileName);

            TransferWidget.Child = TransferPanel;

            return TransferWidget;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Exports the data of the Source-Table into the supplied file Location (if null, then ask for file), considering indication of headering and appending.
        /// If the file name ends with '.csv' it is treated as separated by Commas and with quoted values, else as separated by Tabs and with unquoted values.
        /// Returns error message or null if successful.
        /// </summary>
        public static Tuple<Uri,string> ExportTableDataToFile(Table SourceTable, Uri Location = null, bool IsHeadered = true, bool Append = false)
        {
            General.ContractRequiresNotNull(SourceTable);

            var UseUI = (Location == null);
            if (UseUI)
            {
                Location = Display.DialogGetSaveFile("Export data to...", ".txt",
                                                     "Tab-separated (multiple values per line) files (*.tab, *.tsv, *.dat, *.txt)|*.tab;*.tsv;*.dat;*.txt|" +
                                                     "Comma-separated (multiple values per line) files (*.csv)|*.csv|" +
                                                     "List/plain-text (single value per line) files (*.lis, *.lst, *.prn)|*.lis;*.lst;*.prn",
                                                     SourceTable.Designation.TechName);
                if (Location == null)
                    return null;
            }

            var Delimiter = General.STR_DATA_DELIMITER;
            var HasQuotedValues = false;
            var ReplaceDelimiterInData = false;
            var Extension = Path.GetExtension(Location.LocalPath).NullDefault("").ToLower().Replace(".","");
            var TrimSpaces = true;

            if (Extension == "csv")
            {
                Delimiter = General.GetCurrentTextListDelimiter();
                HasQuotedValues = true;
            }
            else
                if (Extension.IsOneOf("tab", "tsv", "dat", "txt"))
                {
                    Delimiter = "\t";
                    ReplaceDelimiterInData = true;
                }
                else
                {
                    Delimiter = General.STR_DATA_DELIMITER;
                    IsHeadered = false;
                    TrimSpaces = false;
                }

            try
            {
                var Header = (IsHeadered ? SourceTable.Definition.FieldDefinitions.Select(fdef => fdef.TechName) : null);

                General.SaveFileDelimited(Location.LocalPath, SourceTable.GetRecordsForExportAsString(HasQuotedValues),
                                          Header, Delimiter, Append, HasQuotedValues, TrimSpaces, ReplaceDelimiterInData);
            }
            catch (Exception Problem)
            {
                if (UseUI)
                    Display.DialogMessage("Error!", "Cannot export data.\nProblem: " + Problem.Message, EMessageType.Warning);

                return Tuple.Create((Uri)null, Problem.Message);
            }

            if (UseUI)
                Display.DialogMessage("Attention", "Data was exported successfully!");

            return Tuple.Create(Location, (string)null);
        }
        
        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Loads a data file from the supplied Location, considering indication of headering and initial table-definition.
        /// If the file name ends with '.csv' it is treated as separated by Commas and with quoted values, else as separated by Tabs and with unquoted values.
        /// Returns a compatible table-definition (if the initial was not suitable) plus the loaded data.
        /// Returns null if no records are found.
        /// </summary>
        public static Tuple<TableDefinition, List<List<object>>> ImportTableDataFromFile(Uri Location, bool IsHeadered, TableDefinition InitialDefinition,
                                                                                         bool CreateTableDefIfNecessary, bool CanUserChooseCompatibleTableDef)
        {
            General.ContractRequiresNotNull(Location, InitialDefinition);

            var WorkingDefinition = InitialDefinition;
            var Delimiter = "\t";
            var HasQuotedValues = false;
            var Extension = Path.GetExtension(Location.LocalPath).NullDefault("").Replace(".", "");

            Tuple<List<List<string>>, List<string>> TextRecords = null;

            if (Extension.IsOneOf("lis", "lst", "prn"))
            {
                var SingleFieldLines = File.ReadAllLines(Location.LocalPath).Select(line => line.IntoList()).ToList();
                TextRecords = Tuple.Create(SingleFieldLines, new List<string>());
                CanUserChooseCompatibleTableDef = false;

                if (WorkingDefinition.FieldDefinitions.Count != 1 ||
                    WorkingDefinition.FieldDefinitions[0].FieldType.ContainerType != typeof(string))
                {
                    WorkingDefinition = InitialDefinition.OwnerDomain.TableDefinitions
                                            .FirstOrDefault(tdef => tdef.FieldDefinitions.Count == 1
                                                                    && tdef.FieldDefinitions[0].FieldType.ContainerType == typeof(string));
                    if (WorkingDefinition == null)
                    {
                        WorkingDefinition = InitialDefinition;
                        CanUserChooseCompatibleTableDef = true;
                    }
                }
            }
            else
            {
                if (Extension == "csv")
                {
                    Delimiter = General.GetCurrentTextListDelimiter();
                    HasQuotedValues = true;
                }

                TextRecords = General.LoadFileDelimitedIntoStrings(Location.LocalPath, Delimiter, HasQuotedValues, IsHeadered);
            }

            if (TextRecords.Item1.Count < 1)
            {
                Display.DialogMessage("Attention", "The selected file has no data records.");
                return null;
            }

            var TypingResult = GenerateTypedRecordsList(TextRecords.Item1);

            var FileName = Path.GetFileNameWithoutExtension(Location.LocalPath);

            var CompatibleDefinition = GetCompatibleTableDefinitionFor(TypingResult.Item2, WorkingDefinition, false /* CanUserChooseCompatibleTableDef */,
                                                                       CreateTableDefIfNecessary, FileName, TextRecords.Item2);
            if (CompatibleDefinition == null)
                return null;

            return Tuple.Create(CompatibleDefinition, TypingResult.Item1);
        }

        /// <summary>
        /// Gets a compatible Table-Structure Definition for the supplied Types-Structure record and Default Definition where each field value has a well suited data-type.
        /// The returned Table-Structure Definition is either a preexisting one, a new created one if requested or null because of cancel.
        /// </summary>
        public static TableDefinition GetCompatibleTableDefinitionFor(List<BasicDataType> TypesStructure, TableDefinition DefaultDefinition,
                                                                      bool CanSelectPreexistentDef, bool CreateTableDefIfNecessary, string ProposedTableName,
                                                                      List<string> ProposedFieldNames)
        {
            General.ContractRequiresNotNull(TypesStructure, DefaultDefinition, ProposedFieldNames);
            General.ContractRequiresNotAbsent(ProposedTableName);

            TableDefinition Result = null;
            var CompatibleDefinitions = new List<TableDefinition>();
            var InitialDefinitions = DefaultDefinition.Concatenate(DefaultDefinition.OwnerDomain.TableDefinitions
                                                .Where(tdef => tdef.FieldDefinitions.Count >= TypesStructure.Count
                                                               && tdef != DefaultDefinition).ToArray());

            ProposedFieldNames.ForEach(text => text = text.ToStringAlways().Trim());
            ProposedFieldNames.ReplaceDuplicates((fnames, duplicatename) => duplicatename + "_" + duplicatename.GetHashCode().ToString());

            foreach (var Definition in InitialDefinitions)
            {
                var IsCompatible = Definition.IsCompatibleWith(TypesStructure.Select(btyp => btyp.ContainerType).ToList(), false, true);

                if (IsCompatible)
                    CompatibleDefinitions.Add(Definition);
            }

            var DefaultDefinitionIsCompatible = DefaultDefinition.IsIn(CompatibleDefinitions);

            if (CanSelectPreexistentDef && CompatibleDefinitions.Count > 0)
            {
                var DefinitionOptions = new List<IRecognizableElement>();

                if (CreateTableDefIfNecessary)
                    DefinitionOptions.Add(new SimplePresentationElement("<NEW TABLE-STRUCTURE DEFINITION>", Guid.NewGuid().ToString(),
                                                                        "Creates a new Table-Structure Definition.", Display.GetAppImage("table_new.png")));

                DefinitionOptions.AddRange(CompatibleDefinitions);

                var Answer = Display.DialogMultiOption("Table-Structure Definition assignation", "Select a Table-Structure Definition" +
                                                        (CreateTableDefIfNecessary ? " or create a new one" : "") + ".", null, null, true,
                                                        (DefaultDefinitionIsCompatible ? DefaultDefinition : DefinitionOptions.First()).TechName,
                                                        DefinitionOptions.ToArray());
                if (Answer == null)
                    return null;

                if (Answer == DefinitionOptions.First().TechName)
                {
                    Result = CreateCompatibleTableDefinition(DefaultDefinition.OwnerDomain, "Table-Structure Definition " + ProposedTableName,
                                                             TypesStructure, ProposedFieldNames);

                    if (!DomainServices.EditTableDefinition(DefaultDefinition.EditEngine, Result).IsTrue())
                        return null;

                    DefaultDefinition.OwnerDomain.TableDefinitions.AddNew(Result);
                }
                else
                    Result = CompatibleDefinitions.First(tdef => tdef.TechName == Answer);
            }
            else
                if (CanSelectPreexistentDef && DefaultDefinitionIsCompatible)
                    Result = DefaultDefinition;
                else
                    if (CreateTableDefIfNecessary)
                    {
                        Result = CreateCompatibleTableDefinition(DefaultDefinition.OwnerDomain, "Table-Structure Definition " + ProposedTableName,
                                                                 TypesStructure, ProposedFieldNames);
                        DefaultDefinition.OwnerDomain.TableDefinitions.AddNew(Result);
                    }

            return Result;
        }

        public static TableDefinition CreateCompatibleTableDefinition(Domain Owner, string Name,
                                                                      List<BasicDataType> TypesStructure, List<string> ProposedFieldNames)
        {
            var NewTableDef = new TableDefinition(Owner, Name, Name.TextToIdentifier(), "Definition of Table");
            NewTableDef.Alterability = EAlterability.Definition;

            for (int FieldIndex = 0; FieldIndex < TypesStructure.Count; FieldIndex++)
            {
                var FieldName = (FieldIndex < ProposedFieldNames.Count && ProposedFieldNames[FieldIndex].Length > 0
                                 ? ProposedFieldNames[FieldIndex].TextToIdentifier() : "Field-" + (FieldIndex + 1).ToString());
                NewTableDef.FieldDefinitions.Add(new FieldDefinition(NewTableDef, FieldName, FieldName.TextToIdentifier(),
                                                                     TypesStructure[FieldIndex], "Field: " + FieldName));
            }
            NewTableDef.AlterStructure();

            return NewTableDef;
        }

        /// <summary>
        /// From a supplied list of records composed of string fields, generates and returns:
        /// A list of records composed of (typed) field values and the list of common compatible types per field.
        /// Optionally, a Limit about the records to process can be established (deafult = 0, all the records).
        /// </summary>
        public static Tuple<List<List<object>>, List<BasicDataType>> GenerateTypedRecordsList(List<List<string>> SourceList, int Limit = 0)
        {
            var TypedRecords = new List<List<object>>(SourceList.Count);
            var TypesList = new List<BasicDataType>();
            var TypeNumber = DataType.PredefinedDataTypes.Where(datatype => datatype.TechName == "Number");
            var TypeDateTime = DataType.PredefinedDataTypes.Where(datatype => datatype.TechName == "DateTime");

            foreach (var RecordFields in SourceList)
            {
                var TypedRecordFields = new List<object>();
                TypedRecords.Add(TypedRecordFields);

                for(int FieldIndex=0; FieldIndex<RecordFields.Count; FieldIndex++)
                {
                    var Conversion = DataType.ConvertToMostSuitedBasicDataType(RecordFields[FieldIndex], true);
                    if (FieldIndex >= TypesList.Count)
                        TypesList.Add(Conversion.Item2);
                    else
                        TypesList[FieldIndex] = DataType.GetWiderBasicDataTypeRespect(TypesList[FieldIndex], Conversion.Item2);

                    TypedRecordFields.Add(Conversion.Item1);
                }

                if (Limit > 0 && TypedRecords.Count >= Limit)
                    break;
            }

            // Second pass to make all fields of a column have the same wider basic data-type.
            for(int RecordIndex=0; RecordIndex<TypedRecords.Count; RecordIndex++)
                for(int FieldIndex=0; FieldIndex<TypedRecords[RecordIndex].Count; FieldIndex++)
                {
                    Type Kind = (TypedRecords[RecordIndex][FieldIndex] == null
                                 ? typeof(string) : TypedRecords[RecordIndex][FieldIndex].GetType());

                    if (Kind != TypesList[FieldIndex].ContainerType)
                    {
                        var TextValue = SourceList[RecordIndex][FieldIndex];
                        object NewValue = null;
                        if (TypesList[FieldIndex].TryParseValueFrom(TextValue, out NewValue))
                            TypedRecords[RecordIndex][FieldIndex] = NewValue;
                        else
                            Console.WriteLine("Cannot convert to type [{0}] the text value '{1}'.", TypesList[FieldIndex], TextValue);
                    }
                }

            return Tuple.Create(TypedRecords, TypesList);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Opens an interactive editor for the specified Domain.
        /// </summary>
        public static bool DomainEdit(Domain Target)
        {
            if (Target == null)
                return false;

            var InstanceController = EntityInstanceController.AssignInstanceController(Target);
            InstanceController.RegisterPostApplyEditOperation("UpdateDomainDependants",
                                                              (current, previous) => UpdateDomainDependants((Domain)current));
            InstanceController.StartEdit();

            var Extras = new List<UIElement>();

            var Expositor = new EntityPropertyExpositor(Domain.__ViewBackgroundBrush.TechName);
            Expositor.LabelMinWidth = 170;
            Extras.Add(Expositor);

            Expositor = new EntityPropertyExpositor(Domain.__ViewBackgroundImage.TechName);
            Expositor.LabelMinWidth = 170;
            Extras.Add(Expositor);

            Expositor = new EntityPropertyExpositor(Domain.__ViewGridSize.TechName);
            Expositor.LabelMinWidth = 170;
            Extras.Add(Expositor);

            // Show summary of total Idea Definitions in the Domain...
            var Summarizer = new StackPanel();
            Summarizer.Background = Display.GetResource<Brush, EntitledPanel>("PanelBrush");
            Summarizer.Orientation = Orientation.Horizontal;
            Summarizer.Margin = new Thickness(4);

            int TotConceptDefs = 0, TotRelationshipDefs = 0;
            Target.Definitions.ForEach(ideadef => { if (ideadef is ConceptDefinition) TotConceptDefs++; else TotRelationshipDefs++; });

            var Foreground = Display.GetResource<Brush, EntitledPanel>("PanelTextBrush");
            var Text = new TextBlock();
            Text.Text = "Total Idea Definitions: " + (TotConceptDefs + TotRelationshipDefs).ToString();
            Text.Foreground = Foreground;
            Text.Margin = new Thickness(2);
            Text.FontWeight = FontWeights.Bold;
            Summarizer.Children.Add(Text);

            Text = new TextBlock();
            Text.Text = "(" + TotConceptDefs.ToString() + " Concept Definitions + " + TotRelationshipDefs.ToString() + " Relationship Definitions)";
            Text.Foreground = Foreground;
            Text.Margin = new Thickness(2);
            Summarizer.Children.Add(Text);

            Extras.Add(Summarizer);
            // ......................................................
            var TemplatingSubform = new DomainTemplatingSubform(Target);
            TemplatingSubform.ToolTip = "Templates for file generation.";
            var TemplatingTab = TabbedEditPanel.CreateTab(DomainServices.TABKEY_DEF_OUTTEMPLATE, "Output-Templates", TemplatingSubform.ToolTip as string, TemplatingSubform);

            var EditPanel = Display.CreateEditPanel(Target, TemplatingTab.IntoEnumerable(), true, null,
                                                    Display.TABKEY_TECHSPEC + General.STR_SEPARATOR + DomainServices.TABKEY_DEF_OUTTEMPLATE,
                                                    true, false, true, true, Extras.ToArray());

            var Result = InstanceController.Edit(EditPanel, "Define Domain - " + Target.ToString()).IsTrue();
            return Result;
        }

        /// <summary>
        /// Edits the Template stored by the specified property of the supplied Domain instance.
        /// </summary>
        public static void EditBaseTemplates(Domain Source, MModelCollectionDefinitor CollectionDef, Type ExpectedSourceIdeaType,  string LangTechName = null, bool CanSelectTestSource = true)
        {
            DialogOptionsWindow Dialog = null;

            // Make a local command if no within one
            var LocalCommandVar = (Source.EditEngine.CurrentVariatingCommand == null
                                   ? Source.EditEngine.StartCommandVariation("Edit Template")
                                   : null);

            var TemplatesEd = new TemplateEditor();
            TemplatesEd.Initialize(Source, LangTechName.NullDefault(Source.ExternalLanguages[0].TechName), ExpectedSourceIdeaType,
                                   Source, CollectionDef, null, true,
                                   Tuple.Create<string, ImageSource, string, Action<string>>("Insert predefined...", Display.GetAppImage("page_white_wrench.png"), "Inserts a system predefined Output-Template text, at the current selection.",
                                                                                             text => { var tpl = GetPredefinedOutputTemplate(); if (tpl != null) TemplatesEd.SteSyntaxEditor.ReplaceTextAtSelection(tpl); }),
                                   Tuple.Create<string, ImageSource, string, Action<string>>("Test...", Display.GetAppImage("page_code.png"), "Test the Template against a source object.",
                                                                                             text =>
                                                                                             {
                                                                                                 Idea SourceIdea = null;

                                                                                                 if (ExpectedSourceIdeaType == typeof(Concept))
                                                                                                     SourceIdea = DomainMaintainer.RememberedTemplateTestConcept.GetValueOrDefault(Source.OwnerComposition)
                                                                                                                                        .NullDefault(Source.OwnerComposition.DeclaredIdeas
                                                                                                                                                       .FirstOrDefault(idea => idea is Concept));
                                                                                                 else
                                                                                                     if (ExpectedSourceIdeaType == typeof(Relationship))
                                                                                                         SourceIdea = DomainMaintainer.RememberedTemplateTestRelationship.GetValueOrDefault(Source.OwnerComposition)
                                                                                                                                            .NullDefault(Source.OwnerComposition.DeclaredIdeas
                                                                                                                                                           .FirstOrDefault(idea => idea is Relationship));

                                                                                                 var TestingIdea = TemplateTester.TestTemplate(ExpectedSourceIdeaType, null, CollectionDef.Name, text,
                                                                                                                                               Source.OwnerComposition, SourceIdea, CanSelectTestSource);

                                                                                                 if (TestingIdea != null)
                                                                                                     if (ExpectedSourceIdeaType == typeof(Concept))
                                                                                                         DomainMaintainer.RememberedTemplateTestConcept[Source.OwnerComposition] = TestingIdea;
                                                                                                     else
                                                                                                         if (ExpectedSourceIdeaType == typeof(Relationship))
                                                                                                             DomainMaintainer.RememberedTemplateTestRelationship[Source.OwnerComposition] = TestingIdea;
                                                                                             }));

            var EditAccepted = Display.OpenContentDialogWindow(ref Dialog, "Edit " + CollectionDef.Name + " of Domain '" + Source.Name + "'",
                                                               TemplatesEd, 600, 700).IsTrue();
            if (EditAccepted)
                TemplatesEd.Apply();

            if (LocalCommandVar != null)
            {
                Source.EditEngine.CompleteCommandVariation();

                if (!EditAccepted)
                    Source.EditEngine.Undo();
            }
        }

        public static string GetPredefinedOutputTemplate()
        {
            var Route = "Instrumind.ThinkComposer.ApplicationProduct.BaseTemplates.";
            var TemplateResources = AppDomain.CurrentDomain.GetAssemblies().Where(asm => asm.FullName.Contains(AppExec.BASE_NAMESPACE))
                                        .SelectMany(asm => asm.GetManifestResourceNames().Where(res => res.EndsWith("." + Domain.TEMPLATE_FILE_EXT))
                                                             .Select(res => new SimplePresentationElement(
                                                                                    res.CutBetween(Route, null, true)
                                                                                        .Replace("_", " ").Replace("." + Domain.TEMPLATE_FILE_EXT, ""),
                                                                                    res, "", Display.GetAppImage("page_white_code_red.png"))))
                                            .OrderBy(res => res.Name).ToArray();

            if (TemplateResources == null || TemplateResources.Length < 1)
                return null;

            string SelectedResource = Display.DialogMultiOption("Predefined Output-Templates", "Select which text, predefined in " + AppExec.ApplicationName + ", will overwrite the previous...",
                                                                null, null, true, TemplateResources[0].TechName, TemplateResources);
            if (SelectedResource == null)
                return null;

            var TemplateText = Display.GetEmbeddedResourceString(SelectedResource);

            return TemplateText;
        }
        
        /// <summary>
        /// Propagates changes to related objects of the supplied just-updated Domain.
        /// Returns indication of success.
        /// </summary>
        // NOTE: This must be execute inside an editing Command.
        public static bool UpdateDomainDependants(Domain Current, Domain Previous = null)
        {
            // Detect unused Idea-Def Clusters
            if (Previous != null)
            {
                //! Pending: Make sure cluster-def renaming is supported!
                var RemovedConDefClusters = Previous.ConceptDefClusters.Where(def => !def.IsIn(Current.ConceptDefClusters));
                var AffectedConDefClusters = Current.ConceptDefinitions.Where(def => def.Cluster.IsIn(RemovedConDefClusters)).ToList();
                AffectedConDefClusters.ForEach(def => def.Cluster = null);

                var RemovedRelDefClusters = Previous.RelationshipDefClusters.Where(def => !def.IsIn(Current.RelationshipDefClusters));
                var AffectedRelDefClusters = Current.RelationshipDefinitions.Where(def => def.Cluster.IsIn(RemovedRelDefClusters)).ToList();
                AffectedRelDefClusters.ForEach(def => def.Cluster = null);
            }

            // Update ideas palette
            ProductDirector.UpdatePalettes(Current.EditEngine as DocumentEngine);

            // PENDING...
            // Things to update:
            // - Revalidation of existent relationships vs changed relationship rules
            // - Containment

            // Update dependent views
            var Presenters = ProductDirector.CompositionDirector.Visualizer.GetAllViews(Current.OwnerComposition)
                                .Select(docview => (ViewPresenter)docview.PresenterControl);

            var PrevAlterability = Current.EditEngine.CurrentVariatingCommand.AlterExistenceStatusWhileVariating;
            Current.EditEngine.CurrentVariatingCommand.AlterExistenceStatusWhileVariating = false;

            foreach (var Presenter in Presenters)
            {
                Presenter.OwnerView.UnselectAllObjects();   // If no called, selection indicators may remain until reopen the view
                Presenter.OwnerView.ShowAll();
            }

            Current.EditEngine.CurrentVariatingCommand.AlterExistenceStatusWhileVariating = PrevAlterability;

            return true;
        }

        /// <summary>
        /// Opens an interactive editor to mantain the Concepts for the specified Domain
        /// </summary>
        public static void DefineDomainConcepts(Domain Target, ConceptDefinition InitialTarget = null)
        {
            if (Target == null)
                return;

            var InstanceController = EntityInstanceController.AssignInstanceController(Target);
            InstanceController.RegisterPostApplyEditOperation("UpdateDomainDependants",
                                                              (current, previous) => UpdateDomainDependants((Domain)current));
            InstanceController.StartEdit();

            /*T Target.ConceptDefinitions.CollectionChanged +=
                ((sender, args) => Console.WriteLine("Changing.")); */

            var ConceptDefMaintainer = ItemsGridMaintainer.CreateItemsGridControl(Target, Target.ConceptDefinitions,
                                           null /*A (item) => ProductDirector.ConfirmImmediateApply("Domain Concepts Definition", "DomainEdit.DomainConceptsDefinition", "ApplyTypingChangesDirectly", "This text-box")*/ );
            ConceptDefMaintainer.VisualControl.CanEditItemsDirectly = false;
            DomainMaintainer.SetConceptDefinitionsMaintainer(ConceptDefMaintainer);

            if (InitialTarget != null)
                ConceptDefMaintainer.VisualControl.PostCall(vctl => ConceptDefMaintainer.EditItem(InitialTarget));

            var EditPanel = Display.CreateEditPanel(Target, ConceptDefMaintainer.VisualControl);

            if (InstanceController.Edit(EditPanel, "Define Concepts").IsTrue())
                ProductDirector.ContentTreeControl.Refresh();
        }

        /// <summary>
        /// Opens an interactive editor to mantain the Relationships for the specified Domain
        /// </summary>
        public static void DefineDomainRelationships(Domain Target, RelationshipDefinition InitialTarget = null)
        {
            if (Target == null)
                return;

            var InstanceController = EntityInstanceController.AssignInstanceController(Target);
            InstanceController.RegisterPostApplyEditOperation("UpdateDomainDependants",
                                                              (current, previous) => UpdateDomainDependants((Domain)current));
            InstanceController.StartEdit();

            var RelationshipDefMaintainer = ItemsGridMaintainer.CreateItemsGridControl(Target, Target.RelationshipDefinitions,
                                            null /*A (item) => ProductDirector.ConfirmImmediateApply("Domain Relationships Definition", "DomainEdit.DomainRelationshipsDefinition", "ApplyTypingChangesDirectly", "This text-box")*/ );
            RelationshipDefMaintainer.VisualControl.CanEditItemsDirectly = false;
            DomainMaintainer.SetRelationshipDefinitionsMaintainer(RelationshipDefMaintainer);

            if (InitialTarget != null)
                RelationshipDefMaintainer.VisualControl.PostCall(vctl => RelationshipDefMaintainer.EditItem(InitialTarget));

            var EditPanel = Display.CreateEditPanel(Target, RelationshipDefMaintainer.VisualControl);

            if (InstanceController.Edit(EditPanel, "Define Relationships").IsTrue())
                ProductDirector.ContentTreeControl.Refresh();
        }

        /// <summary>
        /// Opens an interactive editor to mantain the Markers for the specified Domain
        /// </summary>
        public static void DefineDomainMarkers(Domain Target, MarkerDefinition InitialTarget = null)
        {
            if (Target == null)
                return;

            var InstanceController = EntityInstanceController.AssignInstanceController(Target);
            InstanceController.RegisterPostApplyEditOperation("UpdateDomainDependants",
                                                              (current, previous) => UpdateDomainDependants((Domain)current));
            InstanceController.StartEdit();

            var MarkerDefMaintainer = ItemsGridMaintainer.CreateItemsGridControl(Target, Target.MarkerDefinitions,
                                           null /*A (item) => ProductDirector.ConfirmImmediateApply("Domain Markers Definition", "DomainEdit.DomainMarkersDefinition", "ApplyTypingChangesDirectly", "This text-box")*/ );
            MarkerDefMaintainer.VisualControl.CanEditItemsDirectly = false;
            DomainMaintainer.SetMarkerDefinitionsMaintainer(MarkerDefMaintainer);

            if (InitialTarget != null)
                MarkerDefMaintainer.VisualControl.PostCall(vctl => MarkerDefMaintainer.EditItem(InitialTarget));

            var EditPanel = Display.CreateEditPanel(Target, MarkerDefMaintainer.VisualControl);

            InstanceController.Edit(EditPanel, "Define Markers");
        }

        /// <summary>
        /// Opens an interactive editor to mantain the Link-Role Variants for the specified Domain
        /// </summary>
        public static void DefineDomainLinkRoleVariants(Domain Target, SimplePresentationElement InitialTarget = null)
        {
            if (Target == null)
                return;

            var InstanceController = EntityInstanceController.AssignInstanceController(Target);
            InstanceController.RegisterPostApplyEditOperation("UpdateDomainDependants",
                                                              (current, previous) => UpdateDomainDependants((Domain)current));
            InstanceController.StartEdit();

            var VariationDefMaintainer = ItemsGridMaintainer.CreateItemsGridControl(Target, Target.LinkRoleVariants,
                                           null /*A (item) => ProductDirector.ConfirmImmediateApply("Domain Variants Definition", "DomainEdit.DomainVariantsDefinition", "ApplyTypingChangesDirectly", "This text-box")*/ );
            VariationDefMaintainer.VisualControl.CanEditItemsDirectly = false;
            DomainMaintainer.SetVariantDefinitionsMaintainer(VariationDefMaintainer);

            if (InitialTarget != null)
                VariationDefMaintainer.VisualControl.PostCall(vctl => VariationDefMaintainer.EditItem(InitialTarget));

            var EditPanel = Display.CreateEditPanel(Target, VariationDefMaintainer.VisualControl);

            InstanceController.Edit(EditPanel, "Define Link-Role Variants");
        }

        /// <summary>
        /// Opens an interactive editor to mantain the External Languages declared for the specified Domain
        /// </summary>
        public static void DefineDomainExternalLanguages(Domain Target, ExternalLanguageDeclaration InitialTarget = null)
        {
            if (Target == null)
                return;

            var InstanceController = EntityInstanceController.AssignInstanceController(Target);
            /*? InstanceController.RegisterPostApplyEditOperation("UpdateDomainDependants",
                                                              (current, previous) => UpdateDomainDependants((Domain)current)); */
            InstanceController.StartEdit();

            var ExternalLangDeclMaintainer = ItemsGridMaintainer.CreateItemsGridControl(Target, Target.ExternalLanguages,
                                           null /*A (item) => ProductDirector.ConfirmImmediateApply("Domain External Languages", "DomainEdit.DomainExternalLanguages", "ApplyTypingChangesDirectly", "This text-box")*/ );
            ExternalLangDeclMaintainer.VisualControl.CanEditItemsDirectly = false;
            DomainMaintainer.SetExternalLanguagesMaintainer(ExternalLangDeclMaintainer);

            if (InitialTarget != null)
                ExternalLangDeclMaintainer.VisualControl.PostCall(vctl => ExternalLangDeclMaintainer.EditItem(InitialTarget));

            var EditPanel = Display.CreateEditPanel(Target, ExternalLangDeclMaintainer.VisualControl);

            InstanceController.Edit(EditPanel, "Declare External Languages");
        }

        /// <summary>
        /// Opens an interactive editor to mantain the Base Tables for the specified Domain
        /// </summary>
        public static void EditDomainBaseTables(Domain Target)
        {
            if (Target == null)
                return;

            var Engine = (CompositionEngine)Target.EditEngine;
            Engine.EditConceptProperties(Target.BaseContentRoot, null, true);
        }

        /// <summary>
        /// Opens an interactive editor to mantain the Tables for the specified Domain
        /// </summary>
        public static void EditDomainTableDefinitions(Domain Target, TableDefinition InitialTarget = null)
        {
            if (Target == null)
                return;

            var TableDefs = Target.TableDefinitions.Where(tdef => tdef.Alterability != EAlterability.System
                                                               && tdef.Alterability != EAlterability.Definition).ToList();

            var InstanceController = EntityInstanceController.AssignInstanceController(Target);
            InstanceController.RegisterPostApplyEditOperation("UpdateDomainDependants",
                                                              (current, previous) =>
                                                              {
                                                                  var CurrentDomain = (Domain)current;
                                                                  // Updates skipping the filetered content
                                                                  CurrentDomain.TableDefinitions.UpdateOnlyListContentFrom(TableDefs,
                                                                                                                           item => item.Alterability != EAlterability.System);
                                                                  return UpdateDomainDependants(CurrentDomain);
                                                              });
            InstanceController.StartEdit();

            var TableDefMaintainer = ItemsGridMaintainer.CreateItemsGridControl(Target, TableDefs /* without filter: Agent.TableDefinitions */,
                                           null /*A (item) => ProductDirector.ConfirmImmediateApply("Domain Tables Definition", "DomainEdit.DomainTablesDefinition", "ApplyTypingChangesDirectly", "This text-box")*/ );
            TableDefMaintainer.VisualControl.CanEditItemsDirectly = false;
            DomainMaintainer.SetTableDefinitionsMaintainer(TableDefMaintainer);

            if (InitialTarget != null)
                TableDefMaintainer.VisualControl.PostCall(vctl => TableDefMaintainer.EditItem(InitialTarget));

            var EditPanel = Display.CreateEditPanel(Target, TableDefMaintainer.VisualControl);

            var Result = InstanceController.Edit(EditPanel, "Edit Table-Structure Definitions");
        }

        /// <summary>
        /// Opens an interactive editor to mantain the Idea-Definition Clusters for the specified Domain
        /// </summary>
        public static bool DefineDomainIdeaDefClusters(Domain Target, string ExplicitKindClusterKey = null)
        {
            if (Target == null)
                return false;

            var InstanceController = EntityInstanceController.AssignInstanceController(Target);
            InstanceController.RegisterPostApplyEditOperation("UpdateDomainDependants",
                                                              (current, previous) => UpdateDomainDependants((Domain)current, (Domain)previous));
            InstanceController.StartEdit();

            var TabbedContainer = new TabbedEditPanel();

            if (ExplicitKindClusterKey == null || ExplicitKindClusterKey == TABKEY_IDEF_CLUSTER_CONCEPT)
            {
                var IdeaDefClusterMaintainer = ItemsGridMaintainer.CreateItemsGridControl(Target, Target.ConceptDefClusters, null);
                IdeaDefClusterMaintainer.VisualControl.CanEditItemsDirectly = false;
                DomainMaintainer.SetConceptDefClustersMaintainer(IdeaDefClusterMaintainer);
                TabbedContainer.AddTab(TABKEY_IDEF_CLUSTER_CONCEPT, "For Concepts", null, IdeaDefClusterMaintainer.VisualControl);
            }

            if (ExplicitKindClusterKey == null || ExplicitKindClusterKey == TABKEY_IDEF_CLUSTER_RELATIONSHIP)
            {
                var IdeaDefClusterMaintainer = ItemsGridMaintainer.CreateItemsGridControl(Target, Target.RelationshipDefClusters, null);
                IdeaDefClusterMaintainer.VisualControl.CanEditItemsDirectly = false;
                DomainMaintainer.SetRelationshipDefClustersMaintainer(IdeaDefClusterMaintainer);
                TabbedContainer.AddTab(TABKEY_IDEF_CLUSTER_RELATIONSHIP, "For Relationships", null, IdeaDefClusterMaintainer.VisualControl);
            }

            var EditPanel = Display.CreateEditPanel(Target, TabbedContainer);

            var Result = InstanceController.Edit(EditPanel, "Declare " + (ExplicitKindClusterKey == null
                                                                          ? "Idea"
                                                                          : (ExplicitKindClusterKey == TABKEY_IDEF_CLUSTER_CONCEPT
                                                                             ? "Concept"
                                                                             : "Relationship")) + " Definition Clusters");
            return Result.IsTrue();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Shows a message to confirm the re-declaration of a Table to an alternative Table-Structure Definition.
        /// Hence, the user is notified about a data-loss operation and can stop or continue, plus indicating "don't show/ask again".
        /// </summary>
        /// <param name="TableDefName">Name of the Table-Structure Definition declaring the detail Table.</param>
        /// <param name="DesignatedName">Name designated to the detail Table.</param>
        /// <param name="ConfigScope">Scope key for grouping configuration information.</param>
        /// <param name="ConfigOptionValueCode">Option code that will be used to get a default option, and later to save the last selected one</param>
        /// <returns>Indication of continuation (true) or cancellation (false)</returns>
        public static bool ConfirmTableDeclarationChange(string TableDefName, string DesignatedName, string ConfigScope, string ConfigOptionValueCode)
        {
            var Result = Display.DialogPersistableMultiOption("Warning", "Confirm change of the '" + TableDefName + "' definition which declares the Table-Structure.\n" +
                                                              "This will requiere to Delete all data previously stored (if any) in the Table(s) designated as '" + DesignatedName + "'", null,
                                                              ConfigScope, ConfigOptionValueCode, "Cancel",
                                                              new SimplePresentationElement("OK", "OK", "Proceed, apply changes and delete related Data.", Display.GetAppImage("accept.png")),
                                                              new SimplePresentationElement("Cancel", "Cancel", "Do not change anything.", Display.GetAppImage("cancel.png")));
            return (Result != null && Result.Item1 == "OK");
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Creates a Link designator, for the supplied Owner and Name, and returns it.
        /// </summary>
        public static LinkDetailDesignator CreateLinkDesignation(Ownership<IdeaDefinition,Idea> Owner, string Name)
        {
            // PENDING: Allow the user to restrict the ResourceLinkType that can be referenced.
            var Result = new LinkDetailDesignator(Owner, Name, Name.TextToIdentifier());

            return Result;
        }

        /// <summary>
        /// Creates a Attachment designator, for the supplied Owner and Name, and returns it.
        /// </summary>
        public static AttachmentDetailDesignator CreateAttachmentDesignation(Ownership<IdeaDefinition,Idea> Owner, string Name)
        {
            // PENDING: Allow the user to restrict the FileDataType that can be attached.
            var Result = new AttachmentDetailDesignator(Owner, Name, Name.TextToIdentifier());

            return Result;
        }

        /// <summary>
        /// Creates a Table designator, for the supplied Engine, Owner and Name, and returns it or null if cancelled.
        /// </summary>
        public static TableDetailDesignator CreateTableDesignation(EntityEditEngine Engine, Ownership<IdeaDefinition, Idea> Owner, string Name)
        {
            string NewDsnName = Name;

            var OwnerDomain = Owner.GetValue<Domain>(ideadef => ideadef.OwnerDomain, idea => idea.IdeaDefinitor.OwnerDomain);
            var TableDef = (OwnerDomain.TableDefinitions.Count > 0 ? OwnerDomain.TableDefinitions[0] : OwnerDomain.DefaultTableDef);
            var Result = new TableDetailDesignator(Owner, TableDef, false, NewDsnName, NewDsnName.TextToIdentifier());

            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}