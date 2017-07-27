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
// File   : WorkCommand.cs
// Object : Instrumind.Common.Visualization.WorkCommand (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.08.28 Néstor Sánchez A.  Creation
//

using System;
using System.Windows.Input;

/// Specialized WPF components and features across Instrumind products.
namespace Instrumind.Common.Visualization
{
    /// <summary>
    /// Base ancestor for application Commands.
    /// </summary>
    public abstract class WorkCommand : ICommand
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public WorkCommand(string Name)
        {
            this.Name = Name;

            // IMPORTANT: Initialize must be called from within a descendant final constructor, not from here.
        }

        /// <summary>
        /// Initialization of the command.
        /// </summary>
        public virtual void Initialize()
        {
        }

        /// <summary>
        /// Name.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Assistance text.
        /// </summary>
        public string Assistance { get; protected set; }

        /// <summary>
        /// Original expositor.
        /// </summary>
        public WorkCommandExpositor CommandExpositor { get; set; }

        /// <summary>
        /// Function which determines if this command can be applied/executed.
        /// If null then command can be executed, else it is called to determine that.
        /// Ignored if the CanExecute method is overridden.
        /// </summary>
        public Func<object, bool> CanApply { get; set; }

        /// <summary>
        /// Action to be applied/executed.
        /// Ignored if the Execute method is overridden.
        /// </summary>
        public Action<object> Apply { get; set; }

         #region ICommand Members

        virtual public bool CanExecute(object parameter = null)
        {
            // PENDING... MAKE IT WORK!
            var Result = ((CanApply != null && CanApply(parameter)) || (CanApply == null && Apply != null));
            return Result;
        }

         /*- [field:NonSerialized]
         public event EventHandler CanExecuteChanged; */
        
        [field:NonSerialized]
        public event EventHandler CanExecuteChanged
        { 
            add { CommandManager.RequerySuggested += value; } 
            remove { CommandManager.RequerySuggested -= value; } 
        }

        public virtual void Execute(object parameter = null)
        {
            if (parameter == null && this.CommandExpositor != null
                && this.CommandExpositor.CommandParameterExtractor != null)
                parameter = this.CommandExpositor.CommandParameterExtractor();

            if (Apply != null)
                Apply(parameter);
        }

        #endregion
    }
}