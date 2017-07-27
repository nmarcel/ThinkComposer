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
    /// Interaction logic for PaletteButtonGroup.xaml
    /// </summary>
    public partial class PaletteButtonGroup : UserControl
    {
        public PaletteButtonGroup()
        {
            InitializeComponent();
        }

        public PaletteButtonGroup(WorkCommandExpositor[] SelectableOptions, bool ShowText = false, double ImageSize = Display.ICOSIZE_LIT, Orientation Direction = Orientation.Horizontal)
            : this((object[])SelectableOptions, ShowText, ImageSize, Direction)
        {
        }

        public PaletteButtonGroup(Tuple<IRecognizableElement, Action<object>>[] SelectableOptions, bool ShowText = false, double ImageSize = Display.ICOSIZE_LIT, Orientation Direction = Orientation.Horizontal)
            : this((object[])SelectableOptions, ShowText, ImageSize, Direction)
        {
        }

        protected PaletteButtonGroup(object[] SelectableOptions, bool ShowText = false, double ImageSize = Display.ICOSIZE_LIT, Orientation Direction = Orientation.Horizontal)
        {
            this.SetOptions(ImageSize, Direction, ShowText, SelectableOptions);
        }

        public void SetOptions(WorkCommandExpositor[] SelectableOptions, bool ShowText = false, double ImageSize = Display.ICOSIZE_LIT, Orientation Direction = Orientation.Horizontal)
        {
            this.SetOptions(ImageSize, Direction, ShowText, SelectableOptions);
        }

        public void SetOptions(Tuple<IRecognizableElement, Action<object>>[] SelectableOptions, bool ShowText = false, double ImageSize = Display.ICOSIZE_LIT, Orientation Direction = Orientation.Horizontal)
        {
            this.SetOptions(ImageSize, Direction, ShowText, SelectableOptions);
        }

        // Notice the change in parameters order respect its overloaded siblings to avoid recursive/infinite-loop calls
        protected void SetOptions(double ImageSize, Orientation Direction, bool ShowText, object[] SelectableOptions)
        {
            General.ContractRequires(SelectableOptions.Length > 0);

            this.BackPanel.Children.Clear();

            foreach (var Declaration in SelectableOptions)
            {
                var NewButton = Display.GenerateButton(Declaration, ImageSize, Orientation.Horizontal);
                NewButton.ButtonShowText = ShowText;

                NewButton.Click +=
                    ((sender, rtdevargs) =>
                    {
                        rtdevargs.Handled = true;
                        Button_Click(NewButton, rtdevargs);
                    });

                this.OptionButtons.Add(NewButton, Declaration);
                this.BackPanel.Children.Add(NewButton);
            }

            this.BackPanel.Orientation = Direction;
        }

        protected Dictionary<PaletteButton, object> OptionButtons = new Dictionary<PaletteButton, object>();

        /// <summary>
        /// Provide this function to pass your desired parameter to the Operation action.
        /// Else this Button is passed.
        /// </summary>
        public Func<object> OperationParameterExtractor;

        protected Action<object> Operation;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var SelectedButton = sender as PaletteButton;
            if (SelectedButton == null)
                return;

            this.Operation = null;
            var OptionButtonSource = OptionButtons[SelectedButton];

            var SourceExpositor = OptionButtonSource as WorkCommandExpositor;
            if (SourceExpositor != null)
                return;     // Exit, because the execution is by Command-Binding

            var SourceDeclaration = OptionButtonSource as Tuple<IRecognizableElement, Action<object>>;
            if (SourceDeclaration != null)
                this.Operation = SourceDeclaration.Item2;

            if (this.Operation != null)
                this.Operation(OperationParameterExtractor == null ? this : OperationParameterExtractor());
        }
    }
}
