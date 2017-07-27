using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

/// Library of standard Instrumind WPF custom and user controls.
namespace Instrumind.Common.Visualization.Widgets
{
    /// <summary>
    /// Interaction logic for AutoCollapsePanel.xaml
    /// </summary>
    public partial class AutoCollapsePanel : UserControl, IShellVisualContent
    {
        public const double INITIAL_SIZE = 20.0;

        public static readonly DependencyProperty OrientationProperty;
        public static readonly DependencyProperty CanCollapseProperty;

        static AutoCollapsePanel()
        {
            AutoCollapsePanel.OrientationProperty = DependencyProperty.Register("Orientation", typeof(Orientation), typeof(AutoCollapsePanel),
                            new FrameworkPropertyMetadata(Orientation.Horizontal, new PropertyChangedCallback(OnOrientationChanged)));

            AutoCollapsePanel.CanCollapseProperty = DependencyProperty.Register("CanCollapse", typeof(bool), typeof(AutoCollapsePanel),
                            new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnCanCollapseChanged)));
        }

        private double PreviousSize;

        /// <summary>
        /// Constructor.
        /// </summary>
        public AutoCollapsePanel()
        {
            this.CanCollapse = true;

            InitializeComponent();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="Key">Key/Code for identify the panel.</param>
        /// <param name="Title">Title of the panel.</param>
        /// <param name="Type">Purpose type of the supplied content.</param>
        /// <param name="Content">Visual content to be hosted.</param>
        /// <param name="CanCollapse">Indicates whether the panel is collapsable.</param>
        public AutoCollapsePanel(string Key = "", string Title = "", EShellVisualContentType Type = EShellVisualContentType.PaletteContent,
                                 UIElement Content = null, bool CanCollapse = true) : this()
        {
            this.Key = Key;
            this.Title = Title;
            this.ContentType = Type;
            this.Content = Content;
            this.CanCollapse = CanCollapse;
        }

        bool Applied = false;
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (Applied)
                return;

            Applied = true;

            var AltContentBrd = this.GetTemplateChild<Border>("AltContentBorder", true);
            AltContentBrd.Child = this.AltContent;

            if (this.Orientation == Orientation.Horizontal)
            {
                this.PreviousSize = this.Height;
                this.Height = CollapsedSize;
            }
            else
            {
                this.PreviousSize = this.Width;
                this.Width = CollapsedSize;
            }
        }

        public double CollapsedSize { get; set; }

        public Orientation Orientation
        {
            get { return (Orientation)GetValue(AutoCollapsePanel.OrientationProperty); }
            set { SetValue(AutoCollapsePanel.OrientationProperty, value); }
        }

        public bool CanCollapse
        {
            get { return (bool)GetValue(AutoCollapsePanel.CanCollapseProperty); }
            set { SetValue(AutoCollapsePanel.CanCollapseProperty, value); }
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            Show();
        }

        public void Show()
        {
            if (this.CanCollapse)
                if (this.Orientation == Orientation.Horizontal)
                    this.Height = this.PreviousSize;
                else
                    this.Width = this.PreviousSize;
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            if (this.CanCollapse && !ChangeAppliedImmediately)
                if (this.Orientation == Orientation.Horizontal)
                {
                    this.PreviousSize = this.Height;
                    this.Height = CollapsedSize;
                }
                else
                {
                    this.PreviousSize = this.Width;
                    this.Width = CollapsedSize;
                }

            ChangeAppliedImmediately = false;
        }

        private static void OnOrientationChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            AutoCollapsePanel Target = depobj as AutoCollapsePanel;
            Orientation NewOrientation = (Orientation)evargs.NewValue;

            if (NewOrientation == Orientation.Horizontal)
            {
                Target.Width = double.NaN;
                Target.Height = Target.CollapsedSize;
            }
            else
            {
                Target.Height = double.NaN;
                Target.Width = Target.CollapsedSize;
            }
        }

        private static void OnCanCollapseChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var Target = depobj as AutoCollapsePanel;
            bool NewCanCollapse = (bool)evargs.NewValue;

            Target.CanCollapse = NewCanCollapse;

            if (NewCanCollapse)
            {
                ChangeAppliedImmediately = true;

                if (Target.Orientation == Orientation.Horizontal)
                {
                    Target.PreviousSize = Target.Height;
                    Target.Height = Target.CollapsedSize;
                }
                else
                {
                    Target.PreviousSize = Target.Width;
                    Target.Width = Target.CollapsedSize;
                }
            }
            else
            {
                if (Target.Orientation == Orientation.Horizontal)
                    Target.Height = Target.PreviousSize;
                else
                    Target.Width = Target.PreviousSize;
            }

            if (Target.AtCanCollapseChanged != null)
                Target.AtCanCollapseChanged(NewCanCollapse);
        }

        private static bool ChangeAppliedImmediately = false;

        /// <summary>
        /// Action to be called after the Can-Collapse property has changed, passing its new state.
        /// </summary>
        public Action<bool> AtCanCollapseChanged { get; set; }

        public string Key { get; protected set; }

        public string Title { get; protected set; }

        public EShellVisualContentType ContentType { get; protected set; }

        public FrameworkElement ContentObject { get { return this; } }

        public FrameworkElement AltContent { get; set; }

        public void Discard()
        {
        }
    }
}
