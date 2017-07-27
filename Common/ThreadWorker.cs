using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using System.Windows;

namespace Instrumind.Common
{
    /// <summary>
    /// Executes code on an STA-Thread, allowing creation of WPF objects.
    /// (not like the BackgroundWorker which only runs on MTA-Threads).
    /// </summary>
    public class ThreadWorker<TResult>
    {
        private Thread WorkingThread = null;

        private Dispatcher OriginalThreadDispatcher { get; set; }

        public Func<ThreadWorker<TResult>, OperationResult<TResult>> WorkTask { get; private set; }  // Returns cancellation message, null when completed

        // To be subscribed by WPF Thread
        public event Action<int, string> ProgressChanged;

        // To be subscribed by WPF Thread. Sends completion-status and message.
        public event Action<OperationResult<TResult>> ExecutionFinished;

        public bool IsBusy { get; private set; }

        public ThreadWorker(Dispatcher SourceDispatcher)
        {
            General.ContractRequiresNotNull(SourceDispatcher);

            this.OriginalThreadDispatcher = SourceDispatcher;
        }

        public void Start(Func<ThreadWorker<TResult>, OperationResult<TResult>> WorkTask)
        {
            General.ContractRequiresNotNull(WorkTask);

            this.WorkTask = WorkTask;

            this.WorkingThread = new Thread(Run);
            this.WorkingThread.SetApartmentState(ApartmentState.STA);
            this.WorkingThread.Start();
        }

        private void Run()
        {
            this.IsBusy = true;

            var TaskResult = this.WorkTask(this);

            Thread.MemoryBarrier();
            var Handler = this.ExecutionFinished;
            Thread.MemoryBarrier();

            this.IsBusy = false;

            if (Handler != null)
                this.OriginalThreadDispatcher.BeginInvoke(Handler, TaskResult);
        }

        // To be called by WPF UI
        public void Cancel()
        {
            this.IsBusy = false;
            this.WorkingThread.Abort();

            Thread.MemoryBarrier();
            var Handler = this.ExecutionFinished;
            Thread.MemoryBarrier();

            if (Handler != null)
                this.OriginalThreadDispatcher.BeginInvoke(Handler, OperationResult.Failure<TResult>("Cancelled by user."));
        }

        // To be called by working task
        public void ReportProgress(int Percentage, string StatusMessage)
        {
            Thread.MemoryBarrier();
            var Handler = ProgressChanged;
            Thread.MemoryBarrier();

            if (Handler != null)
                this.OriginalThreadDispatcher.BeginInvoke(Handler, Percentage, StatusMessage);
        }

        // -----------------------------------------------------------------------------------------
        // Methods To be called from alternate thread
        public TReturn AtOriginalThreadInvoke<TReturn>(Func<TReturn> Operation)
        {
            TReturn Result = default(TReturn);

            this.OriginalThreadDispatcher.Invoke(new Action(
                () =>
                {
                    Result = Operation();
                }));

            return Result;
        }

        public TReturn AtOriginalThreadGetFrozen<TReturn>(TReturn Target)
            where TReturn : Freezable
        {
            if (Target == null)
                return null;

            TReturn Result = null;

            this.OriginalThreadDispatcher.Invoke(new Action(
                () =>
                {
                    if (Target.IsFrozen)
                        Result = Target;
                    else
                        Result = (TReturn)Target.GetAsFrozen();
                }));

            return Result;
        }
    }
}
