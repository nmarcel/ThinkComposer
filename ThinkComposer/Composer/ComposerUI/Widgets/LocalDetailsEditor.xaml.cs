using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Instrumind.Common;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.MetaModel.InformationMetaModel;
using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;
using Instrumind.ThinkComposer.Model.GraphModel;
using Instrumind.ThinkComposer.Model.InformationModel;
using Instrumind.ThinkComposer.Model.VisualModel;

namespace Instrumind.ThinkComposer.Composer.ComposerUI.Widgets
{
    /// <summary>
    /// Interaction logic for DetailsListEditor.xaml
    /// </summary>
    public partial class LocalDetailsEditor : UserControl, IEntityViewChild
    {
        public static LocalDetailsEditor CreateLocalDetailsEditor(Idea IdeaSource, VisualSymbol SymbolSource,
                                                                  bool AccessToGlobalDetails, bool AccessOnlyTables = false,
                                                                  DetailDesignator InitialDesignatorToEdit = null)
        {
            return (new LocalDetailsEditor(IdeaSource, SymbolSource, AccessToGlobalDetails, AccessOnlyTables, InitialDesignatorToEdit));
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public LocalDetailsEditor()
        {
            InitializeComponent();
        }

        public LocalDetailsEditor(Idea IdeaSource, VisualSymbol SymbolSource, bool AccessToGlobalDetails,
                                  bool AccessOnlyTables = false, DetailDesignator InitialDesignatorToEdit = null)
             : this()
        {
            this.IdeaSource = IdeaSource;
            this.SymbolSource = SymbolSource;
            this.AccessToGlobalDetails = AccessToGlobalDetails;

            if (AccessOnlyTables)
                this.EntitlingPanel.SetVisible(false);

            if (this.AccessToGlobalDetails)
            {
                this.VisualGlobalDetailsSource = DetailEditingCard.GenerateGlobalDetailsFor(this.IdeaSource, this.SymbolSource);
                this.GlobalDetailsMaintainer.SetDetailsSource(this.IdeaSource, this.SymbolSource, this.VisualGlobalDetailsSource, false,
                                                             false, InitialDesignatorToEdit);
                this.GlobalDetailsMaintainer.ShowCustomLookZone = true; // (this.SymbolSource != null);
            }
            else
            {
                this.LocalDetailsMaintainer.Title = "Designations...";
                this.AppearanceTitlePanel.SetVisible(false);
                this.BtnSwitchDetailsScopeOrder.SetVisible(false);
                this.GlobalDetailsMaintainer.SetVisible(false);
            }

            this.VisualLocalDetailsSource = DetailEditingCard.GenerateLocalDetailsFor(this.IdeaSource, this.SymbolSource, this.VisualGlobalDetailsSource);
            this.LocalDetailsMaintainer.SetDetailsSource(this.IdeaSource, this.SymbolSource, this.VisualLocalDetailsSource, true,
                                                         AccessOnlyTables, InitialDesignatorToEdit);
            this.LocalDetailsMaintainer.ShowCustomLookZone = true; // (this.SymbolSource != null);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.SymbolSource == null)
                return;

            DockPanel.SetDock(this.GlobalDetailsMaintainer,
                              (VisualSymbolFormat.GetShowGlobalDetailsFirst(this.SymbolSource)
                              ? Dock.Top : Dock.Bottom));
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public Idea IdeaSource { get; protected set; }
        public VisualSymbol SymbolSource { get; protected set; }
        public bool AccessToGlobalDetails { get; protected set; }

        public EditableList<DetailEditingCard> VisualGlobalDetailsSource { get; protected set; }
        public EditableList<DetailEditingCard> VisualLocalDetailsSource { get; protected set; }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public bool UpdateRelatedDetails(Idea CurrentIdeaInstance, VisualSymbol TargetSymbol)
        {
            // Here, all global details are preexistent (no adds or deletes are allowed while editing details from Symbol)
            if (this.VisualGlobalDetailsSource != null)
                foreach (var GlobalDetailEdit in this.VisualGlobalDetailsSource)
                {
                    var Index = CurrentIdeaInstance.Details.IndexOfMatch(condet => condet.Designation.IsEqual(GlobalDetailEdit.Designator.Value));

                    if (Index < 0)
                    {
                        if (!(GlobalDetailEdit.Designator.Value is LinkDetailDesignator)
                            && GlobalDetailEdit.DetailContent != null)
                            CurrentIdeaInstance.Details.Add(GlobalDetailEdit.DetailContent);
                    }
                    else
                        CurrentIdeaInstance.Details[Index] = GlobalDetailEdit.DetailContent;

                    // Remember that a custom detail can exist for a non-existent detail (internal-property links)
                    if (TargetSymbol != null)
                        CurrentIdeaInstance.UpdateCustomLookFor(GlobalDetailEdit.Designator.Value, TargetSymbol, GlobalDetailEdit.EditingLook);
                }

            // Mantain local details (add, update, delete)
            foreach (var LocalDetailEdit in this.VisualLocalDetailsSource)
                // Add the new details
                if (!LocalDetailEdit.IsPreexistent)
                {
                    var LinkDsn = LocalDetailEdit.Designator.Value as LinkDetailDesignator;

                    if (LocalDetailEdit.DetailContent != null &&
                        (!(LocalDetailEdit.DetailContent is Link) || (LocalDetailEdit.DetailContent is ResourceLink)))
                    {
                        LocalDetailEdit.Designator.Value.Name = LocalDetailEdit.Name;   // Because of binding fail/misuse.
                        CurrentIdeaInstance.Details.Add(LocalDetailEdit.DetailContent);

                        if (TargetSymbol != null)
                            CurrentIdeaInstance.UpdateCustomLookFor(LocalDetailEdit.Designator.Value, TargetSymbol, LocalDetailEdit.EditingLook);
                    }
                }
                else
                {
                    // Update the preexistent details
                    var Index = CurrentIdeaInstance.Details.IndexOfMatch(condet => condet.Designation.IsEqual(LocalDetailEdit.Designator.Value));

                    if (Index >= 0)
                    {
                        CurrentIdeaInstance.Details[Index] = LocalDetailEdit.DetailContent;
                        CurrentIdeaInstance.Details[Index].Designation.Name = LocalDetailEdit.Name;
                        CurrentIdeaInstance.Details[Index].Designation.Summary = LocalDetailEdit.Summary;

                        if (TargetSymbol != null)
                            CurrentIdeaInstance.UpdateCustomLookFor(LocalDetailEdit.Designator.Value, TargetSymbol, LocalDetailEdit.EditingLook);
                    }
                }

            // Delete the no longer existent details
            var AllInternalDetailEdits = (this.VisualGlobalDetailsSource == null
                                          ? this.VisualLocalDetailsSource
                                          : this.VisualGlobalDetailsSource.Concat(this.VisualLocalDetailsSource)).ToList();

            for (int Index = CurrentIdeaInstance.Details.Count - 1; Index >= 0; Index--)
            {
                var TargetDetailContent = CurrentIdeaInstance.Details[Index];

                if (!AllInternalDetailEdits.Any(det => det.Designator.Value.IsEqual(TargetDetailContent.Designation)))
                {
                    CurrentIdeaInstance.Details.RemoveAt(Index);
                    CurrentIdeaInstance.DetailsCustomLooks.Remove(TargetDetailContent.Designation);
                }
            }

            // Detect the no longer used designations of custom-formats.
            var UnusedCustFmtDesignators = new List<DetailDesignator>();

            foreach (var CustomFormat in CurrentIdeaInstance.DetailsCustomLooks)
                if (!AllInternalDetailEdits.Any(det => (det.Designator.Value.IsEqual(CustomFormat.Key))))
                    UnusedCustFmtDesignators.Add(CustomFormat.Key);

            // Delete the no longer used custom-formats by designation.
            foreach (var UnusedCustFmtAsn in UnusedCustFmtDesignators)
                CurrentIdeaInstance.DetailsCustomLooks.Remove(UnusedCustFmtAsn);

            // Update the sort of the details
            CurrentIdeaInstance.Details.UpdateSortFrom(this.VisualLocalDetailsSource, ((targetitem, sorteditem) => targetitem.Designation.IsEqual(sorteditem.Designator.Value)));

            return true;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        private void BtnSwitchDetailsScopeOrder_Click(object sender, RoutedEventArgs e)
        {
            if (this.SymbolSource == null)
                return;

            VisualSymbolFormat.SetShowGlobalDetailsFirst(this.SymbolSource, !VisualSymbolFormat.GetShowGlobalDetailsFirst(this.SymbolSource));
            DockPanel.SetDock(this.GlobalDetailsMaintainer, (VisualSymbolFormat.GetShowGlobalDetailsFirst(this.SymbolSource) ? Dock.Top : Dock.Bottom));
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public string ChildPropertyName
        {
            get { return Idea.__Details.TechName; }
        }

        public IEntityView ParentEntityView { get; set; }

        public void Refresh()
        {
        }

        public bool Apply()
        {
            return true;
        }
    }
}
