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
using Instrumind.Common.EntityBase;
using Instrumind.Common.Visualization;

namespace Instrumind.Common.Visualization.Widgets
{
    /// <summary>
    /// Interaction logic for TextFormatSelector.xaml
    /// </summary>
    public partial class TextFormatSelector : UserControl
    {
        /// <summary>
        /// Exposes to the user a text-format selector for the supplied Initial Text-Format.
        /// Returns indication of success (true, false=cancelled) plus the new text-format.
        /// </summary>
        public static Tuple<bool, TextFormat> SelectFor(TextFormat InitialTextFormat)
        {
            var Selector = new TextFormatSelector(InitialTextFormat);

            Selector.SelectionAction =
                (selectedtextformat =>
                    {
                        Selector.SelectedTextFormat = selectedtextformat;
                        SelectionWindow.Close();
                    });

            var Result = Display.OpenContentDialogWindow<TextFormatSelector>(ref SelectionWindow, "Text format...", Selector).IsTrue();

            return (new Tuple<bool, TextFormat>(Result, Selector.SelectedTextFormat));
        }
        private static DialogOptionsWindow SelectionWindow = null;

        public TextFormatSelector()
        {
            InitializeComponent();
        }

        public TextFormatSelector(TextFormat InitialTextFormat)
             : this()
        {
            this.InitialTextFormat = InitialTextFormat;
        }

        DialogOptionsWindow ParentWindow = null;

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.ParentWindow = this.GetNearestVisualDominantOfType<DialogOptionsWindow>();

            this.FontFamilySelector.ItemsSource = Display.AvailableFontFamilies;
            this.FontSizeSelector.ItemsSource = Display.AvailableFontSizes;

            this.SelectedTextFormat = InitialTextFormat.CreateClone(ECloneOperationScope.Deep, null);
            this.FontNameText.Text = this.SelectedTextFormat.FontFamilyName;
            this.FontSizeText.Text = this.SelectedTextFormat.FontSize.ToString();

            this.FontFamilySelector.SelectionChanged +=
                ((snd, args) =>
                    {
                        if (!this.FontNameText.IsFocused)
                            this.FontNameText.Text = ((FontFamily)this.FontFamilySelector.SelectedItem).Source;

                        GenerateSelectedTextFormat();
                    });

            this.FontSizeSelector.SelectionChanged +=
                ((snd, args) =>
                    {
                        if (!this.FontSizeText.IsFocused)
                            this.FontSizeText.Text = ((int)this.FontSizeSelector.SelectedItem).ToString();

                        GenerateSelectedTextFormat();
                    });

            this.StyleSelector.SelectionAction = (() => GenerateSelectedTextFormat());
            this.AlignmentSelector.SelectionAction = ((alignment) => GenerateSelectedTextFormat());
        }

        TextFormat InitialTextFormat { get; set; }

        TextFormat SelectedTextFormat
        {
            get { return this.SelectedTextFormat_; }
            set
            {
                if (this.SelectedTextFormat_ == value)
                    return;

                this.SelectedTextFormat_ = value;
                this.FormatSampler.Format = this.SelectedTextFormat;
                UpdateSelectors();
            }
        }
        TextFormat SelectedTextFormat_ = null;

        public void UpdateSelectors()
        {
            this.ColorBorder.Background = this.SelectedTextFormat.ForegroundBrush;
            this.FontFamilySelector.SelectedItem = Display.AvailableFontFamilies.FirstOrDefault(fontfam => fontfam.ToString().IsEqual(this.SelectedTextFormat.FontFamilyName));
            this.FontSizeSelector.SelectedItem = (int)this.SelectedTextFormat.FontSize;
            this.StyleSelector.SelectedBold = this.SelectedTextFormat.IsBold;
            this.StyleSelector.SelectedItalic = this.SelectedTextFormat.IsItalic;
            this.StyleSelector.SelectedUnderline = this.SelectedTextFormat.IsUnderline;
            this.StyleSelector.SelectedStrikethrough = this.SelectedTextFormat.IsStrikethrough;
            this.AlignmentSelector.SelectedTextAlignment = this.SelectedTextFormat.Alignment;
        }

        public void GenerateSelectedTextFormat()
        {
            this.SelectedTextFormat.ForegroundBrush = this.ColorBorder.Background;
            this.SelectedTextFormat.FontFamilyName = this.FontFamilySelector.SelectedItem.ToString();
            this.SelectedTextFormat.FontSize = (double)((int)this.FontSizeSelector.SelectedItem);
            this.SelectedTextFormat.IsBold = this.StyleSelector.SelectedBold;
            this.SelectedTextFormat.IsItalic = this.StyleSelector.SelectedItalic;
            this.SelectedTextFormat.IsUnderline = this.StyleSelector.SelectedUnderline;
            this.SelectedTextFormat.IsStrikethrough = this.StyleSelector.SelectedStrikethrough;
            this.SelectedTextFormat.Alignment = this.AlignmentSelector.SelectedTextAlignment;

            this.FormatSampler.Format = this.SelectedTextFormat;
            this.FormatSampler.Refresh();
        }

        private void ColorBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.ColorSelector_Click(this, null);
        }

        private void ColorSelector_Click(object sender, RoutedEventArgs e)
        {
            var Result = Display.DialogSelectBrush(this.SelectedTextFormat.ForegroundBrush);
            if (Result == null || !Result.Item1)
                return;

            this.ColorBorder.Background = Result.Item2;
            GenerateSelectedTextFormat();
        }

        public Action<TextFormat> SelectionAction = null;

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            if (this.SelectionAction == null)
                return;

            if (this.ParentWindow != null)
                this.ParentWindow.DialogResult = true;

            SelectionAction(this.SelectedTextFormat);
        }

        private void FontNameText_TextChanged(object sender, TextChangedEventArgs e)
        {
            var Target = Display.AvailableFontFamilies
                .FirstOrDefault(fontfam => fontfam.Source.ToUpperInvariant()
                                    .StartsWith(this.FontNameText.Text.ToUpperInvariant().Trim()));

            if (Target == null)
                return;

            this.FontFamilySelector.ScrollIntoView(Target);
            this.FontFamilySelector.SelectedItem = Target;
        }

        private void FontSizeText_TextChanged(object sender, TextChangedEventArgs e)
        {
            int Target = Display.AvailableFontSizes
                .FirstOrDefault(fontsize => fontsize.ToString().StartsWith(this.FontSizeText.Text.Trim()));

            if (Target < 1)
                return;

            this.FontSizeSelector.ScrollIntoView(Target);
            this.FontSizeSelector.SelectedItem = Target;
        }

        private void FontFamilySelector_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.BtnOK_Click(this, null);
        }

        private void FontSizeSelector_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.BtnOK_Click(this, null);
        }
    }
}
