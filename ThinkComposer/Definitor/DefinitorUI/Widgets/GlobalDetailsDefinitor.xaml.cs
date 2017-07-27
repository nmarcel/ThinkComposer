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
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;
using Instrumind.ThinkComposer.Model.GraphModel;
using Instrumind.ThinkComposer.Model.VisualModel;
using Instrumind.ThinkComposer.Model.InformationModel;
using Instrumind.ThinkComposer.Composer;
using Instrumind.ThinkComposer.Composer.ComposerUI;

namespace Instrumind.ThinkComposer.Definitor.DefinitorUI.Widgets
{
    /// <summary>
    /// Interaction logic for DetailsListEditor.xaml
    /// </summary>
    public partial class GlobalDetailsDefinitor : UserControl, IEntityViewChild
    {
        public static GlobalDetailsDefinitor CreateGlobalDetailsDefinitor(EntityEditEngine EditEngine, IdeaDefinition DefinitorSource,
                                                                          DetailDesignator InitialDesignatorToEdit = null)
        {
            return (new GlobalDetailsDefinitor(EditEngine, DefinitorSource, InitialDesignatorToEdit));
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public GlobalDetailsDefinitor()
        {
            InitializeComponent();
        }

        public GlobalDetailsDefinitor(EntityEditEngine EditEngine, IdeaDefinition DefinitorSource, DetailDesignator InitialDesignatorToEdit = null)
             : this()
        {
            this.DefinitorSource = DefinitorSource;
            this.VisualGlobalDetailDefsSource = DetailDefinitionCard.GenerateGlobalDetailsForDefinitor(this.DefinitorSource);

            this.GlobalDetailDefsMaintainer.SetDetailDefinitionsSource(EditEngine, this.DefinitorSource, this.VisualGlobalDetailDefsSource,
                                                                      true, InitialDesignatorToEdit);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public IdeaDefinition DefinitorSource { get; protected set; }

        public EditableList<DetailDefinitionCard> VisualGlobalDetailDefsSource { get; protected set; }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public bool UpdateRelatedDetailDefinitions(IdeaDefinition TargetIdeaDefinition)
        {
            // Add and update detail designators
            foreach (var GlobalDetailDef in this.VisualGlobalDetailDefsSource)
            {
                if (!GlobalDetailDef.IsPreexistent)
                    TargetIdeaDefinition.DetailDesignators.Add(GlobalDetailDef.Designator.Value);

                var Index = TargetIdeaDefinition.DetailDesignators.IndexOfMatch(detdsn => detdsn.IsEqual(GlobalDetailDef.Designator.Value));

                if (Index >= 0)
                {
                    TargetIdeaDefinition.DetailDesignators[Index].Name = GlobalDetailDef.Name;
                    TargetIdeaDefinition.DetailDesignators[Index].Summary = GlobalDetailDef.Summary;
                }
            }

            // Delete the no longer existent detail designators
            var AllDetailDesignators = this.VisualGlobalDetailDefsSource.ToList();

            for (int Index = TargetIdeaDefinition.DetailDesignators.Count - 1; Index >= 0; Index--)
            {
                var TargetDetailDesignator = TargetIdeaDefinition.DetailDesignators[Index];

                if (!AllDetailDesignators.Any(det => det.Designator.Value.IsEqual(TargetDetailDesignator)))
                {
                    TargetIdeaDefinition.DetailDesignators.RemoveAt(Index);

                    // Propagate deletion to dependent Idea details
                    var Dependents = TargetIdeaDefinition.GetDependentIdeas();
                    foreach (var DependantIdea in Dependents)
                    {
                        var SelectedItems = DependantIdea.Details.Where(det => det.Designation.IsEqual(TargetDetailDesignator)).ToList();

                        foreach (var Item in SelectedItems)
                            DependantIdea.Details.Remove(Item);
                    }
                }
            }

            // Update the sort of the details
            TargetIdeaDefinition.DetailDesignators.UpdateSortFrom(this.VisualGlobalDetailDefsSource, ((targetitem, sorteditem) => targetitem.IsEqual(sorteditem.Designator.Value)));

            return true;
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
