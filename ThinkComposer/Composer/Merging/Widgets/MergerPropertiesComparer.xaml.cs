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
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.MetaModel.Configurations;

namespace Instrumind.ThinkComposer.Composer.Merging.Widgets
{
    /// <summary>
    /// Interaction logic for MergerPropertiesComparer.xaml
    /// </summary>
    public partial class MergerPropertiesComparer : UserControl
    {
        public MergerPropertiesComparer()
        {
            InitializeComponent();
        }

        public void CompareObjects(SchemaMemberSelection SourceSelection, SchemaMemberSelection TargetSelection)
        {
            /*T Display.DialogMessage("Comparing...", "Source=[" + (SourceSelection == null ? "<UNMATCHED>" : SourceSelection.MemberNameCaption) +
                                               "], Target=[" + (TargetSelection == null ? "<UNMATCHED>" : TargetSelection.MemberNameCaption) + "]"); */

            this.PropertiesGrid.RowDefinitions.Clear();
            this.PropertiesGrid.Children.Clear();

            if (SourceSelection != null && SourceSelection.RefMember != null
                && TargetSelection != null && TargetSelection.RefMember != null
                && SourceSelection.RefMember.GetType() != TargetSelection.RefMember.GetType())
                return; // Instances must have same type!

            if ((SourceSelection != null && SourceSelection.RefMember is SchemaMemberGroup)
                || (TargetSelection != null && TargetSelection.RefMember is SchemaMemberGroup))
                return;

            var Properties = SourceSelection.Get(sel => sel.RefMember as IModelEntity)
                                                .Get(ins => ins.ClassDefinition)
                                                    .Get(cld => cld.Properties)
                                                        .NullDefault(Enumerable.Empty<MModelPropertyDefinitor>())
                              .Concat(TargetSelection.Get(sel => sel.RefMember as IModelEntity)
                                                        .Get(ins => ins.ClassDefinition)
                                                            .Get(cld => cld.Properties)
                                                                .NullDefault(Enumerable.Empty<MModelPropertyDefinitor>()))
                              .Distinct().OrderBy(prop => prop.Name);

            var SourceInstance = SourceSelection.Get(sel => sel.RefMember as IModelEntity);
            var TargetInstance = TargetSelection.Get(sel => sel.RefMember as IModelEntity);

            foreach(var PropDef in Properties)
            {
                var SourceValue = (SourceInstance == null ? null : PropDef.Read(SourceInstance));
                var TargetValue = (TargetInstance == null ? null : PropDef.Read(TargetInstance));
                if (SourceValue == null && TargetValue == null)
                    continue;

                var ActionCode = MergingManager.MERGE_ACTION_UPDATE;
                /* Cancelled: No property level merge will be provided yet
                var ActionCode = (MergingManager.PreferredComparer(SourceValue, TargetValue)
                                  ? MergingManager.MERGE_ACTION_NONE
                                  : MergingManager.MERGE_ACTION_UPDATE); */

                AddComparisonRow(PropDef, ActionCode, SourceValue, TargetValue,
                                 SourceSelection != null && TargetSelection != null);
            }
        }

        public void AddComparisonRow(MModelPropertyDefinitor PropertyDef, string MergeActionCode,
                                     object SourceValue, object TargetValue, bool IsSymmetric)
        {
            var RowDef = new RowDefinition();
            RowDef.MaxHeight = 24;
            this.PropertiesGrid.RowDefinitions.Add(RowDef);
            var RowIndex = this.PropertiesGrid.RowDefinitions.Count - 1;

            var ExpoProperty = new TextBlock();
            ExpoProperty.Text = PropertyDef.Name;
            ExpoProperty.Margin = new Thickness(1);
            ExpoProperty.ToolTip = PropertyDef.Summary;
            Grid.SetRow(ExpoProperty, RowIndex);
            Grid.SetColumn(ExpoProperty, 0);
            this.PropertiesGrid.Children.Add(ExpoProperty);

            if (SourceValue != null)
            {
                var ExpoSourceValue = new TextBlock();
                ExpoSourceValue.Text = SourceValue.ToStringAlways();
                ExpoSourceValue.Margin = new Thickness(1);
                ExpoSourceValue.Foreground = Brushes.Blue;
                ExpoSourceValue.Background = Brushes.White;
                Grid.SetRow(ExpoSourceValue, RowIndex);
                Grid.SetColumn(ExpoSourceValue, 1);
                this.PropertiesGrid.Children.Add(ExpoSourceValue);
            }

            /* Cancelled: No property level merge will be provided yet
            if (MergeActionCode != MergingManager.MERGE_ACTION_NONE
                && IsSymmetric)
            {
                var ExpoAction = new Image();
                ExpoAction.Source = MergingManager.MergeActions.GetByTechName(MergeActionCode).Get(mac => mac.Pictogram);
                ExpoAction.HorizontalAlignment = HorizontalAlignment.Center;
                ExpoAction.VerticalAlignment = VerticalAlignment.Top;
                Grid.SetRow(ExpoAction, RowIndex);
                Grid.SetColumn(ExpoAction, 2);
                this.PropertiesGrid.Children.Add(ExpoAction);

                var ExpoCheck = new CheckBox();
                ExpoCheck.IsChecked = (MergeActionCode == MergingManager.MERGE_ACTION_UPDATE);
                ExpoCheck.HorizontalAlignment = HorizontalAlignment.Center;
                Grid.SetRow(ExpoCheck, RowIndex);
                Grid.SetColumn(ExpoCheck, 3);
                this.PropertiesGrid.Children.Add(ExpoCheck);
            } */

            if (TargetValue != null)
            {
                var ExpoTargetValue = new TextBlock();
                ExpoTargetValue.Text = TargetValue.ToStringAlways();
                ExpoTargetValue.Margin = new Thickness(1);
                ExpoTargetValue.Foreground = Brushes.Blue;
                ExpoTargetValue.Background = Brushes.White;
                Grid.SetRow(ExpoTargetValue, RowIndex);
                Grid.SetColumn(ExpoTargetValue, 2);
                this.PropertiesGrid.Children.Add(ExpoTargetValue);
            }
        }
    }

    /* Cancelled: No property level merge will be provided yet
    public class PropertyMergeSelector
    {
        public MModelPropertyDefinitor PropertyDef { get; set; }

        public SimplePresentationElement MergeAction { get; set; }

        public bool IsSelected { get; set; }
    } */
}
