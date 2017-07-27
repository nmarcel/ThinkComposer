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
using Instrumind.Common.Visualization;

using Instrumind.Common.EntityBase;

namespace Instrumind.Common.Visualization.Widgets
{
    /// <summary>
    /// Interaction logic for FindAndReplacePanel.xaml
    /// </summary>
    public partial class FindAndReplacePanel : UserControl
    {
        public static FindReplaceOptions LastUsedOptions { get; private set; }

        static FindAndReplacePanel()
        {
            LastUsedOptions = new FindReplaceOptions();
        }

        private static BasicWindow FindingWindow = null;

        private static Window CurrentCallingWindow = null;

        private static void CurrentCallingWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (FindingWindow != null && FindingWindow.IsVisible
                && e.Key == Key.Escape)
            {
                FindingWindow.Close();
                e.Handled = true;
            }
        }

        private static void CurrentCallingWindow_Closed(object sender, EventArgs e)
        {
            if (FindingWindow != null)
                FindingWindow.Close();
        }

        public static void FindOrReplace(Func<FindReplaceOptions, int, int> FindOperation, Action<FindReplaceOptions, int> ReplaceOperation,
                                         int InitialPosition, bool ExistsSelectedText, bool FindNextDirectly = false)
        {
            var CallingWindow = Display.GetCurrentWindow();

            var FirstOpen = (FindingWindow == null);
            var FindingPanel = Display.OpenContentWindow<FindAndReplacePanel>(ref FindingWindow, "Find" + (ReplaceOperation != null ? " and Replace" : ""),
                                                                              FindOperation, ReplaceOperation, InitialPosition, ExistsSelectedText);
            FindingWindow.CanMaximize = false;
            FindingWindow.CanMinimize = false;
            FindingWindow.ResizeMode = ResizeMode.NoResize;

            if (CurrentCallingWindow != CallingWindow)
            {
                if (CurrentCallingWindow != null)
                {
                    CurrentCallingWindow.PreviewKeyDown -= CurrentCallingWindow_PreviewKeyDown;
                    CurrentCallingWindow.Closed -= CurrentCallingWindow_Closed;
                }

                CurrentCallingWindow = CallingWindow;

                CurrentCallingWindow.PreviewKeyDown += CurrentCallingWindow_PreviewKeyDown;
                CurrentCallingWindow.Closed += CurrentCallingWindow_Closed;
            }

            if (FindNextDirectly && !LastUsedOptions.FindText.IsAbsent())
            {
                FindingWindow.ShowInTaskbar = false;
                FindingWindow.WindowState = WindowState.Minimized;

                FindingPanel.PostCall(
                    fp =>
                    {
                        fp.BtnFind_Click(Key.F3, null);
                        FindingWindow.PostCall(fw => fw.Close());
                    });
            }
        }

        public FindAndReplacePanel(Func<FindReplaceOptions, int, int> FindOperation, Action<FindReplaceOptions, int> ReplaceOperation,
                                   int InitialPosition, bool ExistsSelectedText)
        {
            this.FindOperation = FindOperation;
            this.ReplaceOperation = ReplaceOperation;
            this.LastFoundPosition = InitialPosition;

            LastUsedOptions.CanReplace = (ReplaceOperation != null);
            LastUsedOptions.CanConsiderSelection = ExistsSelectedText;
            this.DataContext = LastUsedOptions;

            InitializeComponent();
        }

        public int LastFoundPosition { get; private set; }

        public Func<FindReplaceOptions, int, int> FindOperation { get; protected set; }

        public Action<FindReplaceOptions, int> ReplaceOperation { get; protected set; }

        private TextBox CmbFindTextBox = null;

        public bool PreviouslyFindingInSelection { get; private set; }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.CmbFindTextBox = this.CmbFind.GetTextBox();
            if (this.CmbFindTextBox != null)
            {
                if (this.CmbFind.Items.Count > 0)
                    this.CmbFindTextBox.Text = this.CmbFind.Items[0] as string;

                this.CmbFindTextBox.SelectAll();
                this.CmbFindTextBox.Focus();
            } 
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Display.GetCurrentWindow().Close();
        }

        private void BtnFind_Click(object sender, RoutedEventArgs e)
        {
            if (this.FindOperation == null)
                return;

            LastUsedOptions.FindTextHistory.Remove(LastUsedOptions.FindText);
            LastUsedOptions.FindTextHistory.Insert(0, LastUsedOptions.FindText);

            if (this.PreviouslyFindingInSelection != LastUsedOptions.OnlyInSelection)
                this.LastFoundPosition = -1;

            this.PreviouslyFindingInSelection = LastUsedOptions.OnlyInSelection;

            this.LastFoundPosition = this.FindOperation(LastUsedOptions, this.LastFoundPosition + 1);

            this.BtnReplace.IsEnabled = (this.LastFoundPosition >= 0);
            this.BtnReplaceAll.IsEnabled = (this.LastFoundPosition >= 0);

            if (this.LastFoundPosition >= 0)
                return;

            if (sender.IsEqual(Key.F3))
            {
                System.Media.SystemSounds.Exclamation.Play();
                return;
            }

            Display.DialogMessage("Find result", "Text not found.", EMessageType.Information);

            if (this.CmbFindTextBox != null)
            {
                this.CmbFindTextBox.SelectAll();
                this.CmbFindTextBox.Focus();
            }
        }

        private void BtnReplace_Click(object sender, RoutedEventArgs e)
        {
            if (this.ReplaceOperation == null || this.LastFoundPosition < 0)
                return;

            LastUsedOptions.ReplaceTextHistory.Remove(LastUsedOptions.ReplaceText);
            LastUsedOptions.ReplaceTextHistory.Insert(0, LastUsedOptions.ReplaceText);

            this.ReplaceOperation(LastUsedOptions, this.LastFoundPosition);
        }

        private void BtnReplaceAll_Click(object sender, RoutedEventArgs e)
        {
            while ((this.LastFoundPosition =
                        this.FindOperation(LastUsedOptions, this.LastFoundPosition + 1))
                   >= 0)
                this.ReplaceOperation(LastUsedOptions, this.LastFoundPosition);

            var ParentWindow = this.GetNearestVisualDominantOfType<Window>();
            if (ParentWindow != null)
                ParentWindow.Close();
        }

        public class FindReplaceOptions
        {
            public FindReplaceOptions()
            {
                // Uses EditableList (without variating-instance to supress undo/redo capability) just to notify collection changes
                this.FindTextHistory = new EditableList<string>("FindTextHistory", null);
                this.ReplaceTextHistory = new EditableList<string>("ReplaceTextHistory", null);
            }

            public string FindText { get; set; }
            public IList<string> FindTextHistory { get; private set; }

            public bool CanReplace { get; set; }
            public string ReplaceText { get; set; }
            public IList<string> ReplaceTextHistory { get; private set; }

            public bool IsCaseSensitive { get; set; }

            public bool ConsiderWholeWord { get; set; }

            public bool CanConsiderSelection
            {
                get { return this.CanConsiderSelection_; }
                set
                {
                    this.CanConsiderSelection_ = value;
                    if (!this.CanConsiderSelection)
                        this.OnlyInSelection = false;
                }
            }
            public bool CanConsiderSelection_ = false;
            public bool OnlyInSelection { get; set; }
        }
    }
}
