using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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

using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;

namespace Instrumind.Common.Visualization.Widgets
{
    /// <summary>
    /// Simple Tech-Spec editor.
    /// </summary>
    public partial class SyntaxTextEditor : UserControl, IEntityViewChild
    {
        // -------------------------------------------------------------------------------------------------------------------------------------------
        public EntityEditEngine Engine { get; protected set; }

        public string SourceName { get; protected set; }

        public string SourcePropertyTechName { get; protected set; }

        public string TextTitle { get; protected set; }

        protected Func<string> Reader { get; private set; }

        protected Action<string> Writer { get; private set; }

        public string SyntaxName { get; set; }

        public string SyntaxFileExtension { get; set; }

        public Tuple<string, ImageSource, string, Action<string>>[] ExtraButtons = null;

        public SyntaxTextEditor()
        {
            InitializeComponent();

            var FindCommand = new GenericCommand("Find", obj => this.BtnFind_Click(null, null));
            this.InputBindings.Add(new KeyBinding(FindCommand, Key.F, ModifierKeys.Control));

            var FindNextCommand = new GenericCommand("FindNext", obj => this.BtnFind_Click(Key.F3, null));
            this.InputBindings.Add(new KeyBinding(FindNextCommand, Key.F3, ModifierKeys.None));

            var FindReplaceCommand = new GenericCommand("FindReplace", obj => this.BtnFindReplace_Click(null, null));
            this.InputBindings.Add(new KeyBinding(FindReplaceCommand, Key.H, ModifierKeys.Control));
        }
            
        public void Initialize(EntityEditEngine Engine, string SourceName, string SourcePropertyTechName, string TextTitle,
                               Func<string> Reader, Action<string> Writer, bool ShowWindowButtons = false,
                               params Tuple<string, ImageSource, string, Action<string>>[] ExtraButtons)
        {
            this.SourcePropertyTechName = SourcePropertyTechName;

            this.Reader = Reader;
            this.Writer = Writer;

            this.Engine = Engine;
            this.ExtraButtons = ExtraButtons;

            this.TextEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("TcTemplate");    // "C#");
            this.TextEditor.ShowLineNumbers = true;

            // Bad idea: this.TextEditor.LostFocus += ((sender, args) => this.WriteTextToSource());

            this.TextEditor.TextArea.MouseRightButtonDown +=
                ((sender, args) =>
                {
                    var position = this.TextEditor.GetPositionFromPoint(args.GetPosition(this.TextEditor));
                    if (position.HasValue)
                        this.TextEditor.TextArea.Caret.Position = position.Value;
                });

            this.BtnOK.SetVisible(ShowWindowButtons);
            this.BtnCancel.SetVisible(ShowWindowButtons);

            if (this.ExtraButtons != null && this.ExtraButtons.Length > 0)
                foreach(var Extra in ExtraButtons)
                {
                    var LocalExtra = Extra;
                    var NewButton = new PaletteButton(LocalExtra.Item1, LocalExtra.Item2, Summary: LocalExtra.Item3);
                    NewButton.Click += ((sdr, args) => LocalExtra.Item4(this.TextEditor.Text));

                    this.PnlExtraButtons.Children.Add(NewButton);
                }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!this.IsPopulated)
                this.Refresh();

            this.IsPopulated = true;
        }
        private bool IsPopulated = false;

        /* Already implemented by command
        private void BtnSelectall_Click(object sender, RoutedEventArgs e)
        {
            this.TextEditor.TextArea.Selection = ICSharpCode.AvalonEdit.Editing.Selection.Create(this.TextEditor.TextArea, 0, this.TextEditor.Text.Length);
        } */

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            var ParentWindow = this.GetNearestVisualDominantOfType<Window>();
            if (ParentWindow == null)
                return;

            ParentWindow.DialogResult = true;
            ParentWindow.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            var ParentWindow = this.GetNearestVisualDominantOfType<Window>();
            if (ParentWindow == null)
                return;

            ParentWindow.Close();
        }

        private void BtnSaveTo_Click(object sender, RoutedEventArgs e)
        {
            var Filter = this.SyntaxName + " files (*" + this.SyntaxFileExtension + ")|*" + this.SyntaxFileExtension + "|All files (*.*)|*.*";
            var Location = Display.DialogGetSaveFile("Save " + this.SyntaxName + " to...", this.SyntaxFileExtension, Filter,
                                                     this.SourceName + " - " + this.TextTitle + this.SyntaxFileExtension);
            if (Location == null)
                return;

            try
            {
                General.StringToFile(Location.LocalPath, this.TextEditor.Text);
            }
            catch (Exception Problem)
            {
                Display.DialogMessage("Attention!", "Cannot save file.\nProblem: " + Problem.Message, EMessageType.Error);
                return;
            }
        }

        private void BtnLoadFrom_Click(object sender, RoutedEventArgs e)
        {
            var FileLocation = Display.DialogGetOpenFile("Select " + this.SyntaxName + " file", this.SyntaxFileExtension,
                                                         this.SyntaxName + " files (*" + this.SyntaxFileExtension + ")|*" + this.SyntaxFileExtension + "|All files (*.*)|*.*");
            if (FileLocation == null)
                return;

            try
            {
                this.TextEditor.Text = General.FileToString(FileLocation.LocalPath);
            }
            catch (Exception Problem)
            {
                Display.DialogMessage("Attention!", "Cannot load file.\nProblem: " + Problem.Message, EMessageType.Error);
                return;
            }
        }

        public string Text
        {
            get { return this.TextEditor.Text; }
            set { this.TextEditor.Text = value; }
        }

        public void ReplaceTextAtSelection(string Text)
        {
            // this.TextEditor.Document.Insert(this.TextEditor.CaretOffset, Text);
            this.TextEditor.Document.Replace(this.TextEditor.SelectionStart, this.TextEditor.SelectionLength, Text);
        }

        public IEntityView ParentEntityView { get; set; }

        public string ChildPropertyName { get { return this.SourcePropertyTechName; } }

        public void Refresh()
        {
            if (this.Reader == null)
                return;

            this.TextEditor.Text = this.Reader();
            this.TextEditor.Focus();
            this.TextEditor.Select(0, 0);   // Skips standard behavior of selecting all text of first text-box
        }

        public bool Apply()
        {
            if (this.Writer == null)
                return false;

            this.Engine.StartCommandVariation("Text change");
            this.Writer(this.TextEditor.Text);
            this.Engine.CompleteCommandVariation();

            return true;
        }

        private void BtnFind_Click(object sender, RoutedEventArgs e)
        {
            FindAndReplacePanel.FindOrReplace(FindOperation, null, (this.TextEditor.SelectionLength > 0 ? -1 : this.TextEditor.CaretOffset),
                                              this.TextEditor.SelectionLength > 0, sender.IsEqual(Key.F3));
        }

        private void BtnFindReplace_Click(object sender, RoutedEventArgs e)
        {
            FindAndReplacePanel.FindOrReplace(FindOperation, ReplaceOperation, (this.TextEditor.SelectionLength > 0 ? -1 : this.TextEditor.CaretOffset),
                                              this.TextEditor.SelectionLength > 0);
        }

        private int FindOperation(FindAndReplacePanel.FindReplaceOptions Options, int Position)
        {
            if (Position < 0 || Position >=
                (Options.OnlyInSelection ? this.TextEditor.SelectionLength : this.TextEditor.Text.Length))
                return -1;

            var CaseComparer = (Options.IsCaseSensitive
                                ? StringComparison.Ordinal  // Must use 'ordinal' to avoid considering hyphens as part of words (culture dependant).
                                : StringComparison.OrdinalIgnoreCase);

            var SourceText = (Options.OnlyInSelection
                             ? this.TextEditor.SelectedText
                             : this.TextEditor.Text);

            Position = SourceText.IndexOf(Options.FindText, Position, CaseComparer);

            // This loop is just to skip the non-whole-words cases
            while (Position >= 0)
            {
                if (!(Options.ConsiderWholeWord &&
                      ((Position > 0 && (Char.IsLetterOrDigit(SourceText[Position - 1]) || SourceText[Position - 1] == '_')) ||
                       ((Position + Options.FindText.Length) < SourceText.Length
                        && (Char.IsLetterOrDigit(SourceText[Position + Options.FindText.Length]) || SourceText[Position + Options.FindText.Length] == '_')))))
                {
                    // Do not select! to not affect in-selection find/replace.  //- this.TextEditor.Select(StartPosition, Options.FindText.Length);
                    var AbsolutePosition = ((Options.OnlyInSelection ? this.TextEditor.SelectionStart : 0 ) + Position);
                    this.TextEditor.PostCall(
                        ted =>
                        {
                            ted.CaretOffset = AbsolutePosition;
                            var Location = ted.Document.GetLocation(AbsolutePosition);
                            ted.ScrollTo(Location.Line, Location.Column);
                            ted.Focus();
                        });

                    return Position;
                }

                Position = Position + Options.FindText.Length;
                if (Position >= SourceText.Length)
                    return -1;

                Position = SourceText.IndexOf(Options.FindText, Position, CaseComparer);
            }

            return -1;
        }

        private void ReplaceOperation(FindAndReplacePanel.FindReplaceOptions Options, int Position)
        {
            if (Position < 0
                || (Position + Options.ReplaceText.Length) >=
                    (Options.OnlyInSelection ? this.TextEditor.SelectionLength : this.TextEditor.Text.Length))
                return;

            var AbsolutePosition = ((Options.OnlyInSelection ? this.TextEditor.SelectionStart : 0) + Position);
            this.TextEditor.Document.Replace(AbsolutePosition, Options.FindText.Length, Options.ReplaceText);
        }
    }
}
