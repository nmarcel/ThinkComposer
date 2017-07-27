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
    /// <summary>
    /// Visual control for the asynchronous executor of STA-Thread based tasks with UI objects
    /// (For generating non-WPF objects use ProgressiveBackgroundExecutor)
    /// </summary>
    public partial class ProgressiveThreadedExecutorControl : UserControl
    {
        public Window ParentWindow { get; protected set; }

        private Action Closing;

        public ProgressiveThreadedExecutorControl(Action Closing)
        {
            this.Closing = Closing;

            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;

            this.PgsProgress.Value = 0;
            this.TxtProgress.Text = "Initializing";

            this.ParentWindow = this.GetNearestVisualDominantOfType<DialogOptionsWindow>();
            this.ParentWindow.Closing += ParentDialog_Closing;

            Mouse.OverrideCursor = null;
        }

        void ParentDialog_Closing(object sender, CancelEventArgs e)
        {
            this.Closing();
        }

        public void Worker_ProgressChanged(int Percentage, string Message)
        {
            if (!this.IsLoaded)
                return;

            this.PgsProgress.Value = Percentage;
            this.TxtProgress.Text = Message;
            this.TxtPercentage.Text = Percentage.ToString() + " %";
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.ParentWindow.Close();
        }
    }
}
