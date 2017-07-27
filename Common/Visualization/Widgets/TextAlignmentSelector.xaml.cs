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
    /// Interaction logic for TextAlignmentSelector.xaml
    /// </summary>
    public partial class TextAlignmentSelector : UserControl
    {
        public TextAlignmentSelector()
        {
            InitializeComponent();

            this.PreviousTarget = this.ContainerPanel.Children[0] as Border;
        }

        public TextAlignment SelectedTextAlignment
        {
            get { return this.SelectedTextAlignment_; }
            set
            {
                if (this.SelectedTextAlignment_ == value)
                    return;

                this.SelectedTextAlignment_ = value;

                var Target = (Border)this.ContainerPanel.Children[AlignmentsDisposition.GetMatchingIndex(this.SelectedTextAlignment_)];

                if (this.PreviousTarget != null)
                {
                    this.PreviousTarget.Background = Brushes.Transparent;
                    this.PreviousTarget.BorderBrush = Brushes.Transparent;
                }

                Target.Background = Brushes.LightGray;
                Target.BorderBrush = Brushes.Black;

                this.PreviousTarget = Target;
            }
        }
        TextAlignment SelectedTextAlignment_ = TextAlignment.Left;

        Border PreviousTarget = null;

        public Action<TextAlignment> SelectionAction = null;

        public readonly TextAlignment[] AlignmentsDisposition = new TextAlignment[] { TextAlignment.Left, TextAlignment.Center,
                                                                                      TextAlignment.Right, TextAlignment.Justify };

        private void Selector_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var Target = sender as Border;
            if (Target == null || !(Target.Parent is Panel))
                return;

            var Parent = Target.Parent as Panel;
            this.SelectedTextAlignment = AlignmentsDisposition[Parent.Children.IndexOf(Target)];

            if (this.SelectionAction != null)
                this.SelectionAction(this.SelectedTextAlignment);
        }
    }
}
