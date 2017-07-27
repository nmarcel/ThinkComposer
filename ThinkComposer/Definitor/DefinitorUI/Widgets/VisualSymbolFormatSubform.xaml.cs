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
    /// Interaction logic for VisualSymbolFormatDefinitionSubform.xaml
    /// </summary>
    public partial class VisualSymbolFormatSubform : UserControl, IEntityViewChild
    {
        public VisualSymbolFormatSubform()
        {
            InitializeComponent();
        }

        public VisualSymbolFormatSubform(string ChildPropertyName)
             : this()
        {
            this.ChildPropertyName = ChildPropertyName;
        }

        public string ChildPropertyName { get; protected set; }

        public IEntityView ParentEntityView { get; set; }

        public void Refresh()
        {
            var Entity = ParentEntityView.AssociatedEntity;
            var PropDef = Entity.ClassDefinition.GetPropertyDef(this.ChildPropertyName);
            var Value = PropDef.Read(ParentEntityView.AssociatedEntity);

            this.TargetFormat = (VisualSymbolFormat)Value;
            this.TextFormatter.Edit(this.TargetFormat);
        }

        public bool Apply()
        {
            //- this.TextFormatter.UpdateTarget();
            return true;
        }

        public VisualSymbolFormat TargetFormat { get; protected set; }
    }
}
