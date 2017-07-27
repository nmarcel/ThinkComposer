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
using Instrumind.Common.Visualization.Widgets;

using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;

namespace Instrumind.ThinkComposer.Definitor.DefinitorUI.Widgets
{
    /// <summary>
    /// Interaction logic for TextFormatsMaintainer.xaml
    /// </summary>
    public partial class TextFormatsMaintainer : UserControl
    {
        public TextFormatsMaintainer()
        {
            InitializeComponent();
        }

        public void Edit(VisualSymbolFormat TargetSymbolFormat)
        {
            this.TargetSymbolFormat = TargetSymbolFormat;

            this.WorkingFormats.Clear();

            var Purposes = Enum.GetValues(typeof(ETextPurpose)).Cast<ETextPurpose>()
                            .OrderedInitiallyWith(ETextPurpose.Title, ETextPurpose.Subtitle,
                                                  ETextPurpose.DetailHeading, ETextPurpose.DetailCaption,
                                                  ETextPurpose.DetailContent);

            foreach (var Value in Purposes)
            {
                var ValuePurpose = (ETextPurpose)Value;
                var ValueFormat = this.TargetSymbolFormat.GetTextFormat(ValuePurpose);

                var NewReg = new TextFormatEditRegister{ RegPurpose = ValuePurpose,
                                                         RegFormat = ValueFormat };
                this.WorkingFormats.Add(NewReg);
            }

            this.LbxFormats.ItemsSource = this.WorkingFormats;
        }

        public void UpdateTarget()
        {
            foreach (var RegEdit in this.WorkingFormats)
            {
                var OriginalFormat = this.TargetSymbolFormat.GetTextFormat(RegEdit.RegPurpose);
                var Differences = General.DetermineDifferences(RegEdit.RegFormat, OriginalFormat);

                if (Differences != null)
                    this.TargetSymbolFormat.SetTextFormat(RegEdit.RegPurpose, RegEdit.RegFormat);
            }
        }

        public VisualSymbolFormat TargetSymbolFormat { get; protected set; }

        private readonly List<TextFormatEditRegister> WorkingFormats = new List<TextFormatEditRegister>();

        private void BrdFormat_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var RowItem = Display.GetNearestVisualDominantOfType<ListBoxItem>((DependencyObject)sender);
            var RegEdit = RowItem.Content as TextFormatEditRegister;

            var Result = TextFormatSelector.SelectFor(RegEdit.RegFormat);
            if (Result.Item1)
            {
                RegEdit.RegFormat = Result.Item2;

                var Sampler = Display.GetTemplateChild<TextFormatSampler>(RowItem, "FormatSampler");
                Sampler.Format = Result.Item2;  // Don't know why the databinding didn't work
                Sampler.Refresh();

                // Apply
                var OriginalFormat = this.TargetSymbolFormat.GetTextFormat(RegEdit.RegPurpose);
                var Differences = General.DetermineDifferences(RegEdit.RegFormat, OriginalFormat);

                if (Differences != null)
                    this.TargetSymbolFormat.SetTextFormat(RegEdit.RegPurpose, RegEdit.RegFormat);
            }
        }
    }

    public class TextFormatEditRegister
    {
        public ETextPurpose RegPurpose { get; set; }
        public string RegPurposeName { get { return this.RegPurpose.GetFieldName(); } }
        public string RegPurposeDescription { get { return this.RegPurpose.GetDescription(); } }

        public TextFormat RegFormat { get; set; }
    }
}
