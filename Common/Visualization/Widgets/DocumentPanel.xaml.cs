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
    /// Interaction logic for DocumentPanel.xaml
    /// </summary>
    public partial class DocumentPanel : UserControl, IShellVisualContent
    {
        public DocumentPanel()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="Key">Key/Code for identify the panel.</param>
        /// <param name="Content">Visual content to be hosted.</param>
        public DocumentPanel(string Key = "", UIElement Content = null) : this()
        {
            this.Key = Key;
            this.Content = Content;
        }

        #region IShellVisualContent Members

        public string Key { get; protected set; }

        public string Title { get { return this.Key; } }

        public EShellVisualContentType ContentType  { get { return EShellVisualContentType.DocumentContent; } }

        public FrameworkElement ContentObject { get { return this; } }

        public void Discard()
        {
        }

        #endregion
    }
}
