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

namespace Instrumind.Common.Visualization.Widgets
{
    /// <summary>
    /// Interaction logic for RichTextBoxImproved.xaml
    /// </summary>
    public partial class RichTextBoxImproved : RichTextBox
    {
        public RichTextBoxImproved()
        {
            InitializeComponent();
        }

        public FontFamily CurrentFontFamily = null;
        private FontFamily PreviousFontFamily = null;

        public double CurrentFontSize = 10.0;
        private double PreviousFontSize = 10.0;

        public Brush CurrentForeColor = Brushes.Black;
        private Brush PreviousForeColor = null;

        public Brush CurrentBackColor = null;
        private Brush PreviousBackColor = null;

        public bool CurrentSwitchBold = false;
        private bool PreviousSwitchBold = false;

        public bool CurrentSwitchUnderline = false;
        private bool PreviousSwitchUnderline = false;

        public bool CurrentSwitchItalic = false;
        private bool PreviousSwitchItalic = false;

        protected override void OnTextInput(TextCompositionEventArgs e)
        {
            if (this.CurrentFontFamily != this.PreviousFontFamily
                || this.CurrentFontSize != this.PreviousFontSize
                || this.CurrentForeColor != this.PreviousForeColor
                || this.CurrentBackColor != this.PreviousBackColor
                || this.CurrentSwitchBold != this.PreviousSwitchBold
                || this.CurrentSwitchUnderline != this.PreviousSwitchUnderline
                || this.CurrentSwitchItalic != this.PreviousSwitchItalic)
            {
                TextPointer textPointer = this.CaretPosition.GetInsertionPosition(LogicalDirection.Forward);

                Run run = new Run(e.Text, textPointer);
                run.FontFamily = this.CurrentFontFamily;
                run.FontSize = this.CurrentFontSize;
                run.Foreground = this.CurrentForeColor;
                run.Background = this.CurrentBackColor;
                run.FontWeight = (this.CurrentSwitchBold ? FontWeights.Bold : FontWeights.Normal);
                run.TextDecorations = (this.CurrentSwitchUnderline ? TextDecorations.Underline : null);
                run.FontStyle = (this.CurrentSwitchItalic ? FontStyles.Italic : FontStyles.Normal);

                this.CaretPosition = run.ElementEnd;

                this.PreviousFontFamily = CurrentFontFamily;
                this.PreviousFontSize = CurrentFontSize;
                this.PreviousForeColor = CurrentForeColor;
                this.PreviousBackColor = CurrentBackColor;
                this.PreviousSwitchBold = CurrentSwitchBold;
                this.PreviousSwitchUnderline = CurrentSwitchUnderline;
                this.PreviousSwitchItalic = CurrentSwitchItalic;
            }
            else
            {
                base.OnTextInput(e);
            }
        }

    }
}
