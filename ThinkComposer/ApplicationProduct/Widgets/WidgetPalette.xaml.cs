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

namespace Instrumind.ThinkComposer.ApplicationProduct.Widgets
{
    /// <summary>
    /// Interaction logic for WidgetPalette.xaml
    /// </summary>
    public partial class WidgetPalette : UserControl, IShellVisualContent
    {
        public WidgetPalette()
        {
            InitializeComponent();

            // Clears the design-time tabs.
            this.PaletteTab.Items.Clear();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="Key">Key for identify the panel.</param>
        /// <param name="Title">Title of the panel.</param>
        /// <param name="Type">Purpose type of the supplied content.</param>
        /// <param name="Content">Visual content to be hosted.</param>
        public WidgetPalette(string Key = "", string Title = "", EShellVisualContentType Type = EShellVisualContentType.PaletteContent, string ImageLocation = null) :
            this()
        {
            this.Key = Key;
            this.PaletteTitle.Text = Title;
            this.PaletteTitleContainer.ToolTip = Title;
            this.ContentType = Type;
            if (!ImageLocation.IsAbsent())
                this.PaletteImage.Source = Display.GetAppImage(ImageLocation);
        }
        public string Key { get; protected set; }

        public string Title { get { return this.PaletteTitle.Text; } }

        public EShellVisualContentType ContentType { get; protected set; }

        public FrameworkElement ContentObject { get { return this; } }

        public void Discard()
        {
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            TabItem tabitem = sender as TabItem;

            if (tabitem == null)
                return;

            tabitem.IsSelected = true;
        }

        private void PaletteTitleContainer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // CANCELLED:
            // this.PaletteTab.SetVisible(!this.PaletteTab.IsVisible.IsTrue());
        }
    }
}
