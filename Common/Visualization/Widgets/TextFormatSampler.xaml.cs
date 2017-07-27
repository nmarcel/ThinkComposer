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
    /// Interaction logic for TextFormatSampler.xaml
    /// </summary>
    public partial class TextFormatSampler : Border
    {
        public static readonly DependencyProperty FormatProperty;

        static TextFormatSampler()
        {
            TextFormatSampler.FormatProperty = DependencyProperty.Register("Format", typeof(TextFormat), typeof(TextFormatSampler),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnFormatChanged)));
        }

        public TextFormatSampler()
        {
            InitializeComponent();
        }

        public TextFormat Format
        {
            get { return (TextFormat)GetValue(FormatProperty); }
            set { SetValue(FormatProperty, value); }
        }

        private static void OnFormatChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            TextFormatSampler Target = depobj as TextFormatSampler;
            //- TextFormat NewFormat = evargs.NewValue as TextFormat;

            Target.Refresh();
        }

        public void Refresh()
        {
            this.SampleText.Foreground = this.Format.ForegroundBrush;
            this.SampleText.FontFamily = this.Format.CurrentTypeface.FontFamily;
            this.SampleText.FontSize = this.Format.FontSize;
            this.SampleText.FontWeight = this.Format.CurrentTypeface.Weight;
            this.SampleText.FontStyle = this.Format.CurrentTypeface.Style;
            this.SampleText.TextAlignment = this.Format.Alignment;

            // This contains both: underline and strikethrough
            this.SampleText.TextDecorations = this.Format.GetCurrentDecorations();
        }
    }
}
