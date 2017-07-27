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

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;

namespace Instrumind.Common.Visualization.Widgets
{
    /// <summary>
    /// Simple Rich-Text editor.
    /// </summary>
    public partial class RichTextEditor : UserControl, IEntityViewChild
    {
        public const string ABSENT_TEXT = "\r\n";

        public static FrameworkElement CreateRichTextBoxPresenter(IModelEntity SourceEntity, MModelPropertyDefinitor SourceProperty)
        {
            if (SourceEntity == null || SourceProperty.DataType != typeof(string))
                throw new UsageAnomaly("A valid source-entity and a string source-property must be provided to create a Rich-Text Editor.");

            var RichEditor = new RichTextEditor(SourceEntity, SourceProperty);

            return RichEditor;
        }

        // -------------------------------------------------------------------------------------------------------------------------------------------
        public IModelEntity SourceEntity { get; protected set; }

        public MModelPropertyDefinitor SourceProperty { get; protected set; }

        public Window ParentWindow { get; protected set; }

        public RichTextEditor()
        {
            InitializeComponent();

            var FindCommand = new GenericCommand("Find", obj => this.BtnFind_Click(null, null));
            this.InputBindings.Add(new KeyBinding(FindCommand, Key.F, ModifierKeys.Control));

            var FindNextCommand = new GenericCommand("FindNext", obj => this.BtnFind_Click(Key.F3, null));
            this.InputBindings.Add(new KeyBinding(FindNextCommand, Key.F3, ModifierKeys.None));

            var FindReplaceCommand = new GenericCommand("FindReplace", obj => this.BtnFindReplace_Click(null, null));
            this.InputBindings.Add(new KeyBinding(FindReplaceCommand, Key.H, ModifierKeys.Control));

            TextFont.ItemsSource = Display.AvailableFontFamilies;
            TextSize.ItemsSource = Display.AvailableFontSizes;

            SpellCheck.IsEnabled = AppExec.GetConfiguration<bool>("IdeaEditing", "DoSpellCheck", true);
        }

        public RichTextEditor(IModelEntity SourceEntity, MModelPropertyDefinitor SourceProperty)
             : this()
        {
            this.SourceEntity = SourceEntity;
            this.SourceProperty = SourceProperty;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //- this.ParentEntityView = this.GetNearestDominantOfType<IEntityView>();

            var SelectedFontIndex = Display.AvailableFontFamilies.IndexOfMatch(font => font.Source == this.TextEditor.FontFamily.Source);
            if (SelectedFontIndex >= 0)
                TextFont.SelectedIndex = SelectedFontIndex;

            TextSize.SelectedItem = Convert.ToInt32(this.TextEditor.FontSize);

            WorkCommand NewCommand = null;

            NewCommand = new GenericCommand("ToggleBold");
            NewCommand.Apply = (obj => { if (this.TextEditor.Selection.IsEmpty) this.TextEditor.CurrentSwitchBold = this.TextBold.IsChecked.IsTrue(); });
            NewCommand.CanApply = (obj => true);
            this.TextBold.CommandBindings.Add(new CommandBinding(NewCommand));

            NewCommand = new GenericCommand("ToggleUnderline");
            NewCommand.Apply = (obj => { if (this.TextEditor.Selection.IsEmpty) this.TextEditor.CurrentSwitchUnderline = this.TextUnderline.IsChecked.IsTrue(); });
            NewCommand.CanApply = (obj => true);
            this.TextUnderline.CommandBindings.Add(new CommandBinding(NewCommand));

            NewCommand = new GenericCommand("ToggleItalic");
            NewCommand.Apply = (obj => { if (this.TextEditor.Selection.IsEmpty) this.TextEditor.CurrentSwitchItalic = this.TextItalic.IsChecked.IsTrue(); });
            NewCommand.CanApply = (obj => true);
            this.TextItalic.CommandBindings.Add(new CommandBinding(NewCommand));

            //T trying to avoid resizing excess (not working)...  this.TextEditor.PostCall(rte => rte.MaxWidth = double.PositiveInfinity);

            this.ParentWindow = this.GetNearestDominantOfType<Window>();

            if (this.ParentWindow != null)
            {
                this.ParentWindow.Deactivated -= ParentWindow_Deactivated;
                this.ParentWindow.Deactivated += ParentWindow_Deactivated;
            }
        }

        // ..................................................................
        // This section supress unmark of selected text when focus is changed to other window (such as the find/replace one)
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (this.ParentWindow != null)
                this.ParentWindow.Deactivated -= ParentWindow_Deactivated;
        }

        void ParentWindow_Deactivated(object sender, EventArgs e)
        {
            if (this.IsVisible)
                this.TextFont.Focus();  // Focus any other control
        }

        private void TextEditor_LostFocus(object sender, RoutedEventArgs e)
        {
            e.Handled = true;   // Prevents selection mark clearing
        }
        // ..................................................................

        public void ReadFromTarget(IModelEntity Instance)
        {
            if (Instance == null)
                return;

            var PropertyValue = this.SourceProperty.Read(Instance) as string;
            if (PropertyValue.IsAbsent() || PropertyValue == ABSENT_TEXT)
                return;
            
            var Text = new TextRange(this.TextEditor.Document.ContentStart, this.TextEditor.Document.ContentEnd);
            using (var Torrent = PropertyValue.StringToStream())
                Text.Load(Torrent, DataFormats.Xaml);
        }

        public void SaveToTarget(IModelEntity Instance)
        {
            if (Instance == null)
                return;

            var Text = new TextRange(this.TextEditor.Document.ContentStart, this.TextEditor.Document.ContentEnd);
            string PropertyValue = null;

            if (Text.Text != ABSENT_TEXT)
                using (var Torrent = new MemoryStream())
                {
                    Text.Save(Torrent, DataFormats.Xaml);
                    PropertyValue = Torrent.StreamToString();
                }

            this.SourceProperty.Write(Instance, PropertyValue);
        }

        private void TextFont_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count < 1)
                return;

            var SelectedFont = e.AddedItems[0] as FontFamily;
            if (SelectedFont == null)
                return;

            if (this.TextEditor.Selection.IsEmpty)
                this.TextEditor.CurrentFontFamily = SelectedFont;
            else
                this.TextEditor.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, SelectedFont);

            this.TextEditor.Focus();
        }

        private void TextSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count < 1)
                return;

            var SelectedSize = (int)e.AddedItems[0];
            if (SelectedSize < 1)
                return;

            if (this.TextEditor.Selection.IsEmpty)
                this.TextEditor.CurrentFontSize = SelectedSize;
            else
                this.TextEditor.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, (double)SelectedSize);

            this.TextEditor.Focus();
        }

        private void ForeColor_Click(object sender, RoutedEventArgs e)
        {
            var CurrentColor = this.TextEditor.Selection.GetPropertyValue(TextElement.ForegroundProperty) as Brush;
            var Result = Display.DialogSelectBrush(CurrentColor);
            if (Result == null || !Result.Item1)
                return;

            if (this.TextEditor.Selection.IsEmpty)
                this.TextEditor.CurrentForeColor = Result.Item2;
            else
                this.TextEditor.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, Result.Item2);
        }

        private void BackColor_Click(object sender, RoutedEventArgs e)
        {
            var CurrentColor = this.TextEditor.Selection.GetPropertyValue(TextElement.BackgroundProperty) as Brush;
            var Result = Display.DialogSelectBrush(CurrentColor);
            if (Result == null || !Result.Item1)
                return;

            if (this.TextEditor.Selection.IsEmpty)
                this.TextEditor.CurrentBackColor = Result.Item2;
            else
                this.TextEditor.Selection.ApplyPropertyValue(TextElement.BackgroundProperty, Result.Item2);
        }

        public string ChildPropertyName
        {
            get { return this.SourceProperty.TechName; }
        }

        public IEntityView ParentEntityView { get; set; }

        public void Refresh()
        {
            ReadFromTarget(this.ParentEntityView.AssociatedEntity);
        }

        public bool Apply()
        {
            SaveToTarget(this.ParentEntityView.AssociatedEntity);
            return true;
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            /* PENDING
            var DialogResult = Display.DialogPrintSetup();

            if (DialogResult == null)
                return;

            var Text = new TextRange(this.TextEditor.Document.ContentStart, this.TextEditor.Document.ContentEnd);

            if (Text.Text == ABSENT_TEXT)
                FormalInstance.Description = null;
            else
                using (var Torrent = new MemoryStream())
                {
                    Text.Save(Torrent, DataFormats.Xaml);
                    FormalInstance.Description = Torrent.StreamToString();
                }

            // PENDING (?): APPLY THE SPECIFIED SETTINGS.
            var SourceDocument = this.CurrentView.ToDocument(DialogResult.Item1.PrintableArea.Width,
                                                             DialogResult.Item1.PrintableArea.Height,
                                                             null, // Old (now can use Info-Card): this.CurrentView.OwnerCompositeContainer.OwnerComposition.Name,
                                                             null, // Old (now can use Info-Card): this.CurrentView.Name,
                                                             DialogResult.Item1.Landscape, false,   // Borders are not necessary
                                                             DialogResult.Item1.Margins.Left,
                                                             DialogResult.Item1.Margins.Top,
                                                             DialogResult.Item1.Margins.Right,
                                                             DialogResult.Item1.Margins.Bottom);

            var DocTitle = this.sour .CurrentView.OwnerCompositeContainer.OwnerComposition.Name + " - " + this.CurrentView.Name;
            PrintPreviewControl = new PrintPreviewer(SourceDocument, DocTitle);
            Display.OpenContentDialogWindow(ref WinPrintPreview, "Print Preview of: " + DocTitle, PrintPreviewControl);
            */
        }
        /* private static PrintPreviewer PrintPreviewControl = null;
        private static DialogOptionsWindow WinPrintPreview = null; */

        private void Btn_SpellCheck(object sender, RoutedEventArgs e)
        {
            SpellCheck.IsEnabled = !SpellCheck.IsEnabled;
            AppExec.SetConfiguration<bool>("IdeaEditing", "DoSpellCheck", SpellCheck.IsEnabled);
        }

        private void BtnFind_Click(object sender, RoutedEventArgs e)
        {
            FindAndReplacePanel.FindOrReplace(FindOperation, null,
                                              (this.TextEditor.Selection.IsEmpty
                                               ? this.TextEditor.Document.ContentStart.GetOffsetToPosition(this.TextEditor.CaretPosition)
                                               : -1),
                                              !this.TextEditor.Selection.IsEmpty,
                                              sender.IsEqual(Key.F3));
        }

        private void BtnFindReplace_Click(object sender, RoutedEventArgs e)
        {
            FindAndReplacePanel.FindOrReplace(FindOperation, ReplaceOperation,
                                              (this.TextEditor.Selection.IsEmpty
                                               ? this.TextEditor.Document.ContentStart.GetOffsetToPosition(this.TextEditor.CaretPosition)
                                               : -1),
                                              !this.TextEditor.Selection.IsEmpty);
        }

        private int FindOperation(FindAndReplacePanel.FindReplaceOptions Options, int Position)
        {
            // IMPORTANT: Position here represents the WPF so called 'symbols' (Rich-Text/XAML codes), not chars.
            var SourceRange = (Options.OnlyInSelection
                               ? this.TextEditor.Selection
                               : new TextRange(this.TextEditor.Document.ContentStart, this.TextEditor.Document.ContentEnd));

            if (SourceRange == null)
                return -1;

            var CaseComparer = (Options.IsCaseSensitive
                                ? StringComparison.Ordinal  // Must use 'ordinal' to avoid considering hyphens as part of words (culture dependant).
                                : StringComparison.OrdinalIgnoreCase);

            var FoundRange = SourceRange.Start.GetPositionAtOffset(Position).FindText(Options.FindText, CaseComparer);

            // This loop is just to skip the non-whole-words cases
            while (FoundRange != null)
            {
                Position = SourceRange.Start.GetOffsetToPosition(FoundRange.Start);

                if (!(Options.ConsiderWholeWord &&
                      ((Char.IsLetterOrDigit(FoundRange.PreviousChar()) || FoundRange.PreviousChar() == '_') ||
                       (Char.IsLetterOrDigit(FoundRange.NextChar()) || FoundRange.NextChar() == '_'))))
                {
                    // Do not select! to not affect in-selection find/replace.
                    var AbsolutePosition = FoundRange.Start;
                    this.TextEditor.PostCall(
                        ted =>
                        {
                            ted.CaretPosition = AbsolutePosition;
                            var Container = AbsolutePosition.Parent as FrameworkContentElement;
                            if (Container != null)
                            {
                                Container.BringIntoView();
                                ted.Focus();
                            }
                        });

                    return Position;
                }

                FoundRange = FoundRange.End.GetNextContextPosition(LogicalDirection.Forward).FindText(Options.FindText);
            }

            return -1;
        }

        private void ReplaceOperation(FindAndReplacePanel.FindReplaceOptions Options, int Position)
        {
            var SourceRange = (Options.OnlyInSelection
                         ? this.TextEditor.Selection
                         : (new TextRange(this.TextEditor.Document.ContentStart, this.TextEditor.Document.ContentEnd)));

            var SourceText = SourceRange.Text;

            if (SourceRange == null || Position < 0 || SourceRange.Start.GetPositionAtOffset(Position) == null)
                return;

            var FoundRange = new TextRange(SourceRange.Start.GetPositionAtOffset(Position),
                                           SourceRange.Start.GetPositionAtOffset(Position + Options.FindText.Length - 1));
            FoundRange.Text = Options.ReplaceText;
        }

    }
}
