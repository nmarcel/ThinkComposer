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
// File   : WorkCommandInteractive.cs
// Object : Instrumind.Common.Visualization.WorkCommandInteractive (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.09.14 Néstor Sánchez A.  Creation
//

using System;

/// Specialized WPF components and features across Instrumind products.
namespace Instrumind.Common.Visualization
{
    /// <summary>
    /// Work command which continues interactive execution after invocation.
    /// </summary>
    public class WorkCommandInteractive<TParam> : WorkCommand
    {
        public WorkCommandInteractive(string Name, Func<TParam, bool, bool> Continuation = null, Action<bool, TParam> Termination = null)
             : base(Name)
        {
            this.ContinuationOperation = Continuation;
            this.TerminationOperation = Termination;

            this.KeepRunning = (this.ContinuationOperation != null);
        }

        /// <summary>
        /// Indicates wheter currently the command should be restarted
        /// </summary>
        public bool RestartAfterTermination = false;

        /// <summary>
        /// Indicates whether the command server must continuing calling the continuation operation after the execution.
        /// </summary>
        public bool KeepRunning = false;

        /// <summary>
        /// Indicates whether the command is expecting continuation calls or its termination.
        /// </summary>
        public bool IsRunning = false;

        public virtual bool Continue(TParam Parameter = default(TParam), bool IsDefinitive = true)
        {
            if (this.ContinuationOperation != null)
                return this.ContinuationOperation(Parameter, IsDefinitive);

            return true;
        }

        public virtual void Terminate(bool IsNormalTermination = false, TParam Parameter = default(TParam))
        {
            this.IsRunning = false;

            if (this.TerminationOperation != null)
                this.TerminationOperation(IsNormalTermination, Parameter);
        }

        /// <summary>
        /// Operation to be executed for command which continue after invocation.
        /// It receives an input argument, plus an indication of definitive(true)/tentative(false) action, and returns an indication whether to continue calling it.
        /// </summary>
        private Func<TParam, bool, bool> ContinuationOperation = null;

        /// <summary>
        /// Operation to be executed when stopping the command.
        /// It receives an indication of normal termination (false if cancel) and an input argument.
        /// </summary>
        private Action<bool, TParam> TerminationOperation = null;

        public virtual void Execute(TParam parameter = default(TParam))
        {
            this.Execute((object)parameter);
        }

        public override void Execute(object parameter = null)
        {
            base.Execute(parameter);

            if (this.KeepRunning)
                this.IsRunning = true;
        }
    }
}