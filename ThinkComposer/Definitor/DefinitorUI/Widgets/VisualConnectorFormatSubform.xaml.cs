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
using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;

namespace Instrumind.ThinkComposer.Definitor.DefinitorUI.Widgets
{
    /// <summary>
    /// Interaction logic for VisualConnectorsFormatSubform.xaml
    /// </summary>
    public partial class VisualConnectorsFormatSubform : UserControl, IEntityViewChild
    {
        public VisualConnectorsFormatSubform()
        {
            InitializeComponent();
        }

        public VisualConnectorsFormatSubform(string ChildPropertyName, VisualConnectorsFormat TargetFormat)
             : this()
        {
            this.ChildPropertyName = ChildPropertyName;
            this.TargetFormat = TargetFormat;
        }

        public string ChildPropertyName { get; protected set; }

        public IEntityView ParentEntityView { get; set; }

        public void Refresh()
        {
        }

        public bool Apply()
        {
            return true;
        }

        public VisualConnectorsFormat TargetFormat { get; protected set; }
    }
}
