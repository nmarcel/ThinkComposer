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

using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.MetaModel.Configurations;

namespace Instrumind.ThinkComposer.Composer.Merging.Widgets
{
    /// <summary>
    /// Interaction logic for MergerCompositionBrowser.xaml
    /// </summary>
    public partial class MergerCompositionBrowser : UserControl
    {
        public MergerCompositionBrowser()
        {
            this.IsForSelection = true;

            InitializeComponent();
        }

        public bool IsForSelection { get; set; }

        public Action<SchemaMemberSelection> SelectedMemberOperation { get; set; }

        public SchemaMemberSelection LastPointedItem { get; set; }

        private void TvSelection_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var Selection = e.NewValue as SchemaMemberSelection;
            if (Selection == null)
                return;
                
            if (this.LastPointedItem != null)
                this.LastPointedItem.IsPointed = false;

            Selection.IsPointed = true;
            this.LastPointedItem = Selection;

            if (this.SelectedMemberOperation == null)
                return;

            SelectedMemberOperation(Selection);
        }
    }
}
