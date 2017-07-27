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
// File   : WorkCommandExpositor.cs
// Object : Instrumind.Common.Visualization.WorkCommandExpositor (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.06.30 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;

/// Specialized WPF components and features across Instrumind products.
namespace Instrumind.Common.Visualization
{
    /// <summary>
    /// Represents an intermediary for component functionality, exposing: Command, CommandSource, Binding and Target.
    /// </summary>
    public class WorkCommandExpositor : SimplePresentationElement, ICommandSource
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public WorkCommandExpositor(string Name, string TechName, string Summary, string PictogramLocation,
                                    EShellCommandCategory Category, string AreaKey, string GroupKey,
                                    ICommand Command, Func<DocumentEngine, bool> SwitchInitializer,
                                    Func<DocumentEngine, Tuple<double, double, double, double>> RangeInitializer = null)
            : this(Name, TechName, Summary, PictogramLocation, Category, AreaKey, GroupKey, Command, ECommandExpositorStyle.ListBox,
                   null, SwitchInitializer, RangeInitializer)
        {
        }

        public WorkCommandExpositor(string Name, string TechName, string Summary, string PictogramLocation,
                                    EShellCommandCategory Category, string AreaKey, string GroupKey, ICommand Command,
                                    ECommandExpositorStyle ShowOptionsAsComboBox = ECommandExpositorStyle.ComboBox,
                                    Func<IEnumerable<object>> OptionsGetter = null)
            : this(Name, TechName, Summary, PictogramLocation, Category, AreaKey, GroupKey, Command, ShowOptionsAsComboBox, OptionsGetter, null, null)
        {
        }

        private WorkCommandExpositor(string Name, string TechName, string Summary, string PictogramLocation,
                                     EShellCommandCategory Category, string AreaKey, string GroupKey, ICommand Command,
                                     ECommandExpositorStyle MultiOptionSelectorStyle = ECommandExpositorStyle.ComboBox,
                                     Func<IEnumerable<object>> OptionsGetter = null, Func<DocumentEngine, bool> SwitchInitializer = null,
                                     Func<DocumentEngine, Tuple<double, double, double, double>> RangeInitializer = null)
             : base(Name, TechName, Summary, (PictogramLocation.IsAbsent() ? null : Display.GetAppImage(PictogramLocation)))
        {
            General.ContractRequiresNotNull(Command);

            this.Category = Category;
            this.AreaKey = AreaKey;
            this.GroupKey = GroupKey;
            this.Command = Command;

            var WorkingCommand = this.Command as WorkCommand;
            if (WorkingCommand != null)
                WorkingCommand.CommandExpositor = this;

            this.MultiOptionSelectorStyle = MultiOptionSelectorStyle;
            this.OptionsGetter = OptionsGetter;
            this.SwitchInitializer = SwitchInitializer;
            this.RangeInitializer = RangeInitializer;
        }

        /// <summary>
        /// General purpose classification of the command.
        /// </summary>
        public EShellCommandCategory Category { get; protected set; }

        /// <summary>
        /// Area code of the command (e.g.: "Edition", "Preferences").
        /// </summary>
        public string AreaKey { get; protected set; }

        /// <summary>
        /// Group code of the command (e.g.: "Style", "Alignment").
        /// </summary>
        public string GroupKey { get; protected set; }

        /// <summary>
        /// Function to provide the command parameter.
        /// </summary>
        public Func<object> CommandParameterExtractor { get; set; }

        /// <summary>
        /// Function to provide the command target.
        /// </summary>
        public Func<IInputElement> CommandTargetExtractor { get; set; }

        /// <summary>
        /// Indicates how to show the options provided by the options-getter (if any) with a ComboBox, else with a ListView.
        /// </summary>
        public ECommandExpositorStyle MultiOptionSelectorStyle { get; set; }

        /// <summary>
        /// Getter of options set to be shown, from which the one selected will be passed to the command.
        /// </summary>
        public Func<IEnumerable<object>> OptionsGetter { get; set; }

        /// <summary>
        /// Initializer for switch control to be shown.
        /// </summary>
        public Func<DocumentEngine, bool> SwitchInitializer { get; set; }

        /// <summary>
        /// Initializer for slider (with range-min, range-max, range-step and current-value) control to be shown.
        /// </summary>
        public Func<DocumentEngine, Tuple<double, double, double, double>> RangeInitializer { get; set; }

        /// <summary>
        /// Indicates to redirect menu/command-source navigation to the root after execution.
        /// Thus, users must not to go back manually.
        /// </summary>
        public bool GoesToRootAfterExecute { get; set; }

        #region ICommandSource Members

        public ICommand Command { get; protected set; }

        public object CommandParameter
        {
            get
            {
                if (CommandParameterExtractor != null)
                    return CommandParameterExtractor();

                return null;
            }
        }

        public IInputElement CommandTarget
        {
            get
            {
                if (CommandTargetExtractor != null)
                    return CommandTargetExtractor();

                return null;
            }
        }

        #endregion
    }
}