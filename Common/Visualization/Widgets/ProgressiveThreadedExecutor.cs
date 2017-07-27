using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Instrumind.Common.Visualization.Widgets
{
    /// <summary>
    /// Asynchronous executor of STA-Thread based tasks with UI objects
    /// (For generating non-WPF objects use ProgressiveBackgroundExecutor)
    /// </summary>
    public class ProgressiveThreadedExecutor<TResult>
    {
        /// <summary>
        /// Starts execution of progressive task. Returns indication of successful start.
        /// </summary>
        public static bool Execute(string Title, Func<ThreadWorker<TResult>, OperationResult<TResult>> WorkTask, Action<OperationResult<TResult>> EndTask)
        {
            try
            {
                DialogOptionsWindow Dialog = null;
                var Executor = new ProgressiveThreadedExecutor<TResult>(WorkTask, EndTask);
                Display.OpenContentDialogWindow<ProgressiveThreadedExecutorControl>(ref Dialog, Title, Executor.VisualControl, double.NaN, double.NaN);
            }
            catch (Exception Problem)
            {
                Display.DialogMessage("Attention!", "Report generation cannot be initiated!.\nProblem: " + Problem.Message,
                                      EMessageType.Error);
                return false;
            }

            return true;
        }

        private ProgressiveThreadedExecutorControl VisualControl = null;

        private ThreadWorker<TResult> Worker = null;

        private Func<ThreadWorker<TResult>, OperationResult<TResult>> WorkTask { get; set; } // Receives worker and returns an operation-result.
        private Action<OperationResult<TResult>> EndTask { get; set; }  // Receives indication of completion (true) or cancellation (false), plus operation-result.

        public ProgressiveThreadedExecutor(Func<ThreadWorker<TResult>, OperationResult<TResult>> WorkTask, Action<OperationResult<TResult>> EndTask)
        {
            General.ContractRequiresNotNull(WorkTask, EndTask);

            this.WorkTask = WorkTask;
            this.EndTask = EndTask;

            this.VisualControl = new ProgressiveThreadedExecutorControl(this.Closing);

            this.Worker = new ThreadWorker<TResult>(this.VisualControl.Dispatcher);

            this.Worker.ExecutionFinished += Worker_RunWorkerCompleted;
            this.Worker.ProgressChanged += this.VisualControl.Worker_ProgressChanged;

            this.Worker.Start(this.WorkTask);
        }

        void Worker_RunWorkerCompleted(OperationResult<TResult> Completion)
        {
            this.VisualControl.PostCall(vctl => this.EndTask(Completion));

            if (this.VisualControl.ParentWindow != null)
                this.VisualControl.ParentWindow.Close();
        }

        void Closing()
        {
            if (this.Worker.IsBusy)
                this.Worker.Cancel();

            this.Worker.ProgressChanged -= this.VisualControl.Worker_ProgressChanged;
            this.Worker.ExecutionFinished -= Worker_RunWorkerCompleted;
            this.Worker = null;
        }
    }

}
