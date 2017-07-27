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
using Instrumind.Common.EntityBase;
using Instrumind.Common.Visualization;
using Instrumind.Common.Visualization.Widgets;

namespace Instrumind.ThinkComposer.ApplicationProduct.Widgets
{
    /// <summary>
    /// Interaction logic for WidgetItemsPaletteGroup.xaml
    /// </summary>
    public partial class WidgetItemsPaletteGroup : StackPanel
    {
        public WidgetItemsPaletteGroup()
        {
            InitializeComponent();
        }

        public void ClearSelection()
        {
            // Clear palettes selection is they are directly in the group or inside an expander
            foreach (var Child in this.Children)
            {
                var Palette = (Child is WidgetItemsPalette
                               ? Child
                               : (Child is Expander && ((Expander)Child).Content is WidgetItemsPalette)
                                 ? ((Expander)Child).Content
                                 : null) as WidgetItemsPalette;

                if (Palette != null)
                    Palette.ClearSelection();
            }
        }

        public void ClearPalettes()
        {
            this.Children.Clear();
        }

        public void UpdatePalettes(DocumentEngine Engine, IDictionary<IRecognizableElement, IEnumerable<IRecognizableElement>> Palettes, params string[] InitialClustersTechNames)
        {
            this.Engine = Engine;

            this.ClearPalettes();

            if (Palettes == null)
                return;

            var UnclusteredPalettes = (Palettes.Count > 1
                                       ? Palettes.Where(plt => plt.Key.Name == String.Empty)
                                       : Palettes);

            var ClusteredPalettes = (Palettes.Count > 1
                                     ? Palettes.Where(plt => plt.Key.Name != String.Empty)
                                     : Enumerable.Empty<KeyValuePair<IRecognizableElement, IEnumerable<IRecognizableElement>>>());

            if (UnclusteredPalettes.Any())
            {
                var VisualPalette = new WidgetItemsPalette(this.Engine, Palettes.First().Key);
                VisualPalette.ItemsSource = UnclusteredPalettes.SelectMany(plt => plt.Value);
                this.Children.Add(VisualPalette);
            }

            if (ClusteredPalettes.Any())
            {
                var ExpanderTextBrush = Display.GetResource<Brush, EntitledPanel>("PanelTextBrush");

                var Selection = ClusteredPalettes.Where(plt => plt.Value.Any());
                if (!InitialClustersTechNames.IsEmpty())
                {
                    var Initials = InitialClustersTechNames.Select(tn => ClusteredPalettes.FirstOrDefault(plt => plt.Key.TechName == tn));
                    Selection = Selection.OrderedInitiallyWith(Initials.ExcludeEmptyItems().ToArray());
                }

                foreach (var Palette in Selection)
                {
                    var VisualPalette = new WidgetItemsPalette(this.Engine, Palette.Key);
                    VisualPalette.ItemsSource = Palette.Value;

                    var ExpandPanel = new Expander();
                    ExpandPanel.Header = Palette.Key.Name;
                    ExpandPanel.Content = VisualPalette;
                    ExpandPanel.IsExpanded = true;
                    ExpandPanel.Foreground = ExpanderTextBrush;

                    this.Children.Add(ExpandPanel);
                }
            }
        }

        public DocumentEngine Engine { get; protected set; }
    }
}
