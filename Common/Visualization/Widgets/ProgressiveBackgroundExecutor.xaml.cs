using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace Instrumind.Common.Visualization.Widgets
{
    // *************
    // PENDING TEST!
    // *************
    /// <summary>
    /// Asynchronous executor of Background-Worker based tasks with non-UI objects
    /// (For generating WPF objects use ProgressiveThreadedExecutorControl)
    /// </summary>
    public partial class ProgressiveBackgroundExecutor : UserControl
    {
        public static void Execute(string Title, Func<BackgroundWorker, bool> WorkTask, Action EndTask)
        {
            DialogOptionsWindow Dialog = null;
            Display.OpenContentDialogWindow<ProgressiveBackgroundExecutor>(ref Dialog, Title, null, double.NaN, double.NaN, WorkTask, EndTask);
        }

        private BackgroundWorker Worker = null;

        private Func<BackgroundWorker, bool> WorkTask { get; set; } //Task returns false when cancelled, else true.
        private Action EndTask { get; set; }

        public ProgressiveBackgroundExecutor(Func<BackgroundWorker, bool> WorkTask, Action EndTask)
        {
            General.ContractRequiresNotNull(WorkTask, EndTask);

            this.WorkTask = WorkTask;
            this.EndTask = EndTask;

            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                this.Worker = new BackgroundWorker();
                this.Worker.WorkerReportsProgress = true;
                this.Worker.WorkerSupportsCancellation = true;
                this.Worker.DoWork += Worker_DoWork;
                this.Worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
                this.Worker.ProgressChanged += Worker_ProgressChanged;

                this.PgsProgress.Value = 0;

                var ParentDialog = this.GetNearestVisualDominantOfType<DialogOptionsWindow>();
                ParentDialog.Closing += ParentDialog_Closing;

                this.Worker.RunWorkerAsync();
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        void ParentDialog_Closing(object sender, CancelEventArgs e)
        {
            if (this.Worker.IsBusy)
                this.Worker.CancelAsync();

            this.Worker.ProgressChanged -= Worker_ProgressChanged;
            this.Worker.RunWorkerCompleted -= Worker_RunWorkerCompleted;
            this.Worker.DoWork -= Worker_DoWork;
            this.Worker = null;
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Cancel = !this.WorkTask(this.Worker);
        }

        void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (!this.IsLoaded)
                return;

            this.PgsProgress.Value = e.ProgressPercentage;
            this.TxtProgress.Text = e.UserState.ToStringAlways();
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.EndTask();

            Display.GetCurrentWindow().Close();
        }
        
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Display.GetCurrentWindow().Close();
        }
    }
}
