// -------------------------------------------------------------------------------------------
// Instrumind ThinkComposer v1.0
// Copyright (C) 2009-2011 Néstor Marcel Sánchez Ahumada, Santiago, Chile.
// http://www.Instrumind.com
//
// All rights reserved.
// Todos los derechos reservados.
// -------------------------------------------------------------------------------------------
//
// This source code is private property of Néstor Marcel Sánchez Ahumada. It is prohibited the
// use or copy of any part of its content without the written permission of the owner.
// Protected by intellectual property laws and related international treaties.
//
// Este código fuente es propiedad privada de Néstor Marcel Sánchez Ahumada. Está prohibido el
// uso o copia de cualquier parte de su contenido sin el permiso escrito del dueño.
// Protegido por leyes de propiedad intelectual y tratados internacionales relacionados.
// -------------------------------------------------------------------------------------------
//
// Project: Instrumind ThinkComposer v1.0
// File   : ProductDirector.cs
// Object : Instrumind.ThinkComposer.ProductDirector (Class)
//
// Date       Author             Comments
// ---------- ------------------ -------------------------------------------------------------
// 2009.06.30 Néstor Sánchez A.  Start
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;
using Instrumind.Common.Visualization.Widgets;

using Instrumind.ThinkComposer.ApplicationProduct.Widgets;
using Instrumind.ThinkComposer.Definitor;
using Instrumind.ThinkComposer.Definitor.DefinitorMaintenance;
using Instrumind.ThinkComposer.Composer;
using Instrumind.ThinkComposer.MetaModel;
using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.MetaModel.InformationMetaModel;
using Instrumind.ThinkComposer.Model;
using Instrumind.ThinkComposer.Model.GraphModel;
using Instrumind.ThinkComposer.Model.InformationModel;
using Instrumind.ThinkComposer.Model.VisualModel;
using System.Windows.Input;
using System.Windows.Media.Effects;

/// Main manager for the Instrumind ThinkComposer product application.
namespace Instrumind.ThinkComposer.ApplicationProduct
{
    /// <summary>
    /// Centralized manager for the application Product.
    /// Brings to life it using the provided shell.
    /// </summary>
    public static partial class ProductDirector
    {
        // -------------------------------------------------------------------------------------------------------------
        /*********************************************************************************
         //! CRITICAL CODE: Do not rename nor move.
         // This part is entangled with predefined Domains, thru some
         // serialized event-handlers with Action<T>, Func<T> and lambdas.
         *********************************************************************************/
        public static void UpdatePalettes(DocumentEngine DocEngine)
        {
            if (DocEngine == null)
                return;

            var IdeaPalettes = new Dictionary<IRecognizableElement, IEnumerable<IRecognizableElement>>();

            foreach (var Palette in DocEngine.GetExposedElementsPalettes())
                IdeaPalettes.Add(Palette, DocEngine.GetExposedItemsOfElementPalette(Palette));

            CompositionDirector.IdeaPalette.UpdatePalettes(DocEngine, IdeaPalettes);

            var MarkerPalettes = new Dictionary<IRecognizableElement, IEnumerable<IRecognizableElement>>();

            foreach (var Palette in DocEngine.GetExposedMarkersPalettes())
                MarkerPalettes.Add(Palette, DocEngine.GetExposedItemsOfMarkerPalette(Palette));

            CompositionDirector.MarkerPalette.UpdatePalettes(DocEngine, MarkerPalettes);

            var ComplementPalettes = new Dictionary<IRecognizableElement, IEnumerable<IRecognizableElement>>();

            foreach (var Palette in DocEngine.GetExposedComplementsPalettes())
                ComplementPalettes.Add(Palette, DocEngine.GetExposedItemsOfComplementPalette(Palette));

            CompositionDirector.ComplementPalette.UpdatePalettes(DocEngine, ComplementPalettes);
        }

        // -------------------------------------------------------------------------------------------------------------
        /*********************************************************************************
         //! CRITICAL CODE: Do not rename nor move.
         // This part is entangled with predefined Domains, thru some
         // serialized event-handlers with Action<T>, Func<T> and lambdas.
         *********************************************************************************/
        public static void PopulateMenuPalette(WidgetPalette Palette, WorkSphere Sphere, params EShellCommandCategory[] AssignableCommandCategories)
        {
            var GroupingHeaderBrush = Display.GetResource<Brush, EntitledPanel>("HeaderBrush");  // PanelTextBrush also looks good
            var GroupingBodyBrush = new SolidColorBrush(Color.FromRgb(242, 246, 248));  // Display.GetGradientBrush(Color.FromRgb(252, 252, 252), Color.FromRgb(247, 247, 247)); //Display.GetResource<Brush, EntitledPanel>("PanelBrush");

            // Adds the required tab items if these already does not exists in the palette's tab control.
            foreach (var Area in Sphere.CommandAreas)
            {
                Panel TargetAreaPanel = null;
                bool TabExists = false;
                bool MenuItemExposed = false;

                // Determines whether the current Area is already exposed as a TabItem.
                foreach (TabItem Item in Palette.PaletteTab.Items)
                    if (Item.Name == Area.TechName)
                    {
                        TargetAreaPanel = Item.Content as Panel;
                        TabExists = true;
                        break;
                    }

                // Creates a new Panel if no TabItem was found.
                if (!TabExists)
                {
                    var WrappingPanel = new WrapPanel();
                    WrappingPanel.Orientation = Orientation.Horizontal;
                    TargetAreaPanel = WrappingPanel;
                }

                // Create Groups
                foreach (var Group in Sphere.CommandGroups[Area.TechName])
                {
                    StackPanel TargetGroupContainer = null;
                    WrapPanel TargetGroupingPanel = null;

                    // Determines whether the current Group is already exposed as a GroupBox.
                    foreach (var Element in TargetAreaPanel.Children)
                        if (Element is GroupBox &&
                            ((StackPanel)Element).Tag.ToString() == Group.TechName)
                        {
                            TargetGroupContainer = Element as StackPanel;
                            var Body = (Border)TargetGroupContainer.Children[1];  // Children[0] is the Header
                            TargetGroupingPanel = (WrapPanel)Body.Child;
                            break;
                        }

                    // Creates a new Panel if no TabItem was found.
                    if (TargetGroupContainer == null)
                    {
                        TargetGroupContainer = new StackPanel();
                        TargetGroupContainer.Tag = Group.TechName;
                        TargetGroupContainer.Orientation = Orientation.Horizontal;
                        TargetGroupContainer.VerticalAlignment = VerticalAlignment.Stretch;
                        TargetGroupContainer.Margin = new Thickness(1, 0, 1, 0);

                        var GroupingHeader = new Border();
                        GroupingHeader.CornerRadius = new CornerRadius(3.0, 0, 0, 3.0);
                        GroupingHeader.Background = GroupingHeaderBrush;
                        GroupingHeader.Padding = new Thickness(0, 4, 1, 4);
                        GroupingHeader.ToolTip = Group.Name;

                        var GroupingTitle = new TextBlock();
                        GroupingTitle.Text = Group.Name;
                        GroupingTitle.FontFamily = new FontFamily("Arial"); // new FontFamily("Verdana");
                        GroupingTitle.FontSize = 11;
                        // GroupingTitle.FontWeight = FontWeights.Thin;
                        GroupingTitle.Foreground = Brushes.White;
                        GroupingTitle.TextAlignment = TextAlignment.Right;
                        GroupingTitle.HorizontalAlignment = HorizontalAlignment.Stretch;
                        GroupingTitle.LayoutTransform = new RotateTransform(-90.0);
                        GroupingHeader.Child = GroupingTitle;
                        TargetGroupContainer.Children.Add(GroupingHeader);

                        var GroupingBody = new Border();
                        GroupingBody.CornerRadius = new CornerRadius(0, 3.0, 3.0, 0);
                        GroupingBody.Background = GroupingBodyBrush;
                        GroupingBody.BorderThickness = new Thickness(0);
                        // GroupingBody.BorderBrush = Brushes.LightGray;
                        // GroupingBody.BorderThickness = new Thickness(0,1,1,1);

                        /* Rarely needed
                        GroupingHeader.MouseLeftButtonDown +=
                            ((sender, mevargs) => GroupingBody.SetVisible(!GroupingBody.IsVisible.IsTrue())); */

                        TargetGroupContainer.Children.Add(GroupingBody);

                        TargetGroupingPanel = new WrapPanel();
                        TargetGroupingPanel.Orientation = Orientation.Vertical;
                        GroupingBody.Child = TargetGroupingPanel;
                    }

                    TargetAreaPanel.Children.Add(TargetGroupContainer);

                    // Populates the GroupBox with the associated Command Expositors.
                    foreach (KeyValuePair<string, WorkCommandExpositor> ExpositorReg in Sphere.CommandExpositors)
                    {
                        var Expositor = ExpositorReg;

                        if (Expositor.Value.AreaKey == Area.TechName && Expositor.Value.GroupKey == Group.TechName
                            && AssignableCommandCategories.Contains(Expositor.Value.Category))
                        {
                            Control NewControl = null;

                            var Options = (Expositor.Value.OptionsGetter == null ? null : Expositor.Value.OptionsGetter());

                            if (Expositor.Value.SwitchInitializer != null)
                            {
                                var SwitchChecker = new CheckBox();
                                SwitchChecker.Content = Expositor.Value.Name;
                                SwitchChecker.ToolTip = Expositor.Value.Summary;
                                SwitchChecker.Margin = new Thickness(2, 4, 2, 4);
                                SwitchChecker.FontSize = 10.0;
                                /* WPF still has minor problems with fonts over shadow
                                var Shadow = new DropShadowEffect();
                                Shadow.Color = Colors.Gray;
                                Shadow.Opacity = 0.2;
                                SwitchChecker.Effect = Shadow; */
                                SwitchChecker.Command = ExpositorReg.Value.Command;

                                // Important to pass as parameter instead of negating model property
                                // (which, after undos/redos, can be incoherent with the check-box state shown)
                                Expositor.Value.CommandParameterExtractor = (() => SwitchChecker.IsChecked);

                                /* var Switcher = new RoutedEventHandler((sender, evargs) =>
                                    {
                                        var CanExec = Expositor.Value.Command.CanExecute(null);
                                        if (CanExec)
                                            Expositor.Value.Command.Execute(SwitchChecker.IsChecked);
                                    });

                                SwitchChecker.Checked += Switcher;
                                SwitchChecker.Unchecked += Switcher; */

                                NewControl = SwitchChecker;
                                MenuToolbarControlsUpdater.AddOrReplace(Area.TechName + "." + Group.TechName + "." + ExpositorReg.Key,
                                    () =>
                                    {
                                        SwitchChecker.IsChecked = Expositor.Value.SwitchInitializer(WorkspaceDirector.ActiveDocumentEngine);
                                    });
                            }
                            else
                                if (Expositor.Value.OptionsGetter == null)
                                    NewControl = new PaletteButton(Expositor.Value);
                                else
                                    if (Expositor.Value.ShowOptionsAsComboBox)
                                    {
                                        var SelectorButton = new ComboBox();
                                        SelectorButton.ToolTip = Expositor.Value.Name;
                                        SelectorButton.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                                        //? SelectorCombo.MaxWidth = ApplicationShell.MainWindow.PREDEF_INITIAL_TOOLTAB_WIDTH - 48;
                                        SelectorButton.BorderBrush = Brushes.Transparent;
                                        SelectorButton.Background = Brushes.Transparent;
                                        SelectorButton.Height = 22;
                                        SelectorButton.Padding = new Thickness(1, 1, 1, 1);
                                        ScrollViewer.SetVerticalScrollBarVisibility(SelectorButton, ScrollBarVisibility.Visible);
                                        ScrollViewer.SetHorizontalScrollBarVisibility(SelectorButton, ScrollBarVisibility.Disabled);

                                        var FirstOption = Options.FirstOrDefault();
                                        if (FirstOption == null || typeof(FormalPresentationElement).IsAssignableFrom(FirstOption.GetType()))
                                            SelectorButton.ItemTemplate = Display.GetResource<DataTemplate>("TplFormalPresentationElement");
                                        else
                                            if (typeof(SimplePresentationElement).IsAssignableFrom(FirstOption.GetType()))
                                                SelectorButton.ItemTemplate = Display.GetResource<DataTemplate>("TplSimplePresentationElement");

                                        SelectorButton.Loaded +=
                                            ((sender, args) =>
                                            {
                                                var Items = Expositor.Value.OptionsGetter();
                                                SelectorButton.ItemsSource = Items;
                                                if (Items.Length > 0)
                                                    SelectorButton.SelectedItem = Items[0];
                                                SelectorButton.Tag = true;   // Indicates ready for selection
                                            });

                                        SelectorButton.SelectionChanged +=
                                            ((sender, selargs) =>
                                            {
                                                if (Expositor.Value.Command.CanExecute(null) && SelectorButton.Tag != null)
                                                    Expositor.Value.Command.Execute(selargs.AddedItems != null && selargs.AddedItems.Count > 0
                                                                                    ? selargs.AddedItems[0] : null);
                                            });

                                        NewControl = SelectorButton;
                                    }
                                    else
                                    {
                                        var SelectorList = new ListBox();
                                        SelectorList.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                                        SelectorList.Width = ApplicationShell.MainWindow.PREDEF_INITIAL_TOOLTAB_WIDTH - 48;
                                        SelectorList.BorderBrush = Brushes.Transparent;
                                        SelectorList.FontSize = 11;
                                        SelectorList.Margin = new Thickness(0, -2, 0, 0);
                                        SelectorList.Cursor = Cursors.Hand;
                                        ScrollViewer.SetVerticalScrollBarVisibility(SelectorList, ScrollBarVisibility.Visible);
                                        ScrollViewer.SetHorizontalScrollBarVisibility(SelectorList, ScrollBarVisibility.Disabled);

                                        SelectorList.Loaded +=
                                            ((sender, args) =>
                                            {
                                                SelectorList.ItemsSource = Expositor.Value.OptionsGetter();
                                            });

                                        SelectorList.SelectionChanged +=
                                            ((sender, selargs) =>
                                            {
                                                if ((Mouse.LeftButton == MouseButtonState.Pressed || Keyboard.IsKeyDown(Key.Enter))
                                                    && Expositor.Value.Command.CanExecute(null))
                                                {
                                                    Expositor.Value.Command.Execute(selargs.AddedItems != null && selargs.AddedItems.Count > 0
                                                                                    ? selargs.AddedItems[0] : null);
                                                    if (Expositor.Value.GoesToRootAfterExecute)
                                                        Palette.PaletteTab.PostCall(ptab => ptab.SelectedIndex = 0);
                                                }
                                            });

                                        SelectorList.KeyDown +=
                                            ((sender, keyargs) =>
                                            {
                                                if (Keyboard.IsKeyDown(Key.Enter)
                                                    && Expositor.Value.Command.CanExecute(null))
                                                    Expositor.Value.Command.Execute(SelectorList.SelectedItem);
                                            });

                                        /* Looks bad (hides some lines)
                                        SelectorList.MouseMove +=
                                            ((sender, args) =>
                                                {
                                                    // var Position = Mouse.GetPosition(sender as IInputElement);
                                                    var Container = Display.GetNearestVisualDominantOfType<ListBoxItem>(args.OriginalSource as DependencyObject);
                                                    if (Container == LastRecentDocContainer)
                                                        return;

                                                    LastRecentDocContainer = Container;
                                                    if (LastRecentDocContainer == null)
                                                        ToolTipService.SetToolTip(SelectorList, null);
                                                    else
                                                    {
                                                        // PENDING: Make this work as supposed to be.
                                                        ToolTipService.SetToolTip(SelectorList, LastRecentDocContainer.Content);
                                                        ToolTipService.SetPlacementTarget(SelectorList, LastRecentDocContainer);
                                                        ToolTipService.SetPlacement(SelectorList, PlacementMode.Relative);
                                                        ToolTipService.SetVerticalOffset(SelectorList, 0);
                                                        ToolTipService.SetHorizontalOffset(SelectorList, 0);
                                                        ToolTipService.SetInitialShowDelay(SelectorList, 1);
                                                        ToolTipService.SetBetweenShowDelay(SelectorList, 1);
                                                        ToolTipService.SetShowDuration(SelectorList, 30000);
                                                    }
                                                }); */

                                        NewControl = SelectorList;
                                    }

                            TargetGroupingPanel.Children.Add(NewControl);
                            MenuItemExposed = true;
                        }
                    }
                }

                // For possible assigned commands invocators creates a new TabItem, if no one was found related to the current Area.
                if (!TabExists && MenuItemExposed)
                {
                    var NewTab = new TabItem();
                    NewTab.Name = Area.TechName;
                    NewTab.Header = Area.Name;
                    NewTab.Content = TargetAreaPanel;
                    Palette.PaletteTab.Items.Add(NewTab);
                }
            }

        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /*********************************************************************************
         //! CRITICAL CODE: Do not rename nor move.
         // This part is entangled with predefined Domains, thru some
         // serialized event-handlers with Action<T>, Func<T> and lambdas.
         *********************************************************************************/

        private static Dictionary<string, Action> MenuToolbarControlsUpdater = new Dictionary<string, Action>();

        public static void UpdateMenuToolbar()
        {
            var Engine = WorkspaceDirector.ActiveDocumentEngine as CompositionEngine;
            if (Engine == null || Engine.CurrentView == null)
                return;

            Engine.CurrentView.Presenter
                .PostCall(vpres =>
                {
                    foreach (var ControlUpdater in MenuToolbarControlsUpdater)
                        ControlUpdater.Value();
                });
        }

        // -------------------------------------------------------------------------------------------------------------
    }

}