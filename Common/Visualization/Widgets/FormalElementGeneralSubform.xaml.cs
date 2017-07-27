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

using Instrumind.Common.EntityDefinition;

namespace Instrumind.Common.Visualization.Widgets
{
    /// <summary>
    /// Interaction logic for FormalElementSubform.xaml
    /// </summary>
    public partial class FormalElementGeneralSubform : UserControl //-?, IEntityViewChild
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public FormalElementGeneralSubform()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(FormalElementGeneralSubform_Loaded);
        }

        /*-? /// <summary>
        /// Constructor for edit an entity child property.
        /// </summary>
        /// <param name="ChildName">Name of the child property to refer (optional).</param>
        public FormalElementGeneralSubform(string ChildName = null)
            : this()
        {
            this.ChildName = ChildName;
        } */

        void FormalElementGeneralSubform_Loaded(object sender, RoutedEventArgs e)
        {
            this.ExpoName.Focus();
        }

        //-? public string ChildName { get; protected set; }
    }
}
