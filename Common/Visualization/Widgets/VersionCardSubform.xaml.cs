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
    public partial class VersionCardSubform : UserControl, IEntityViewChild
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public VersionCardSubform()
        {
            InitializeComponent();
        }

        public VersionCardSubform(string ChildPropertyName)
             : this()
        {
            this.ChildPropertyName = ChildPropertyName;
        }

        public IEntityView ParentEntityView { get; set; }

        public string ChildPropertyName { get; protected set; }

        public void Refresh()
        {

        }

        public bool Apply()
        {
            return true;
        }
    }
}
