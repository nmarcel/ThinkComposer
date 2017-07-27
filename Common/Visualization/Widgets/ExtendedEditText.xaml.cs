using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace Instrumind.Common.Visualization.Widgets
{
    /// <summary>
    /// Interaction logic for ExtendedEditText.xaml
    /// </summary>
    public partial class ExtendedEditText : UserControl, IDynamicStoreDataGridEditor
    {
        public static readonly DependencyProperty StorageFieldNameProperty;
        public static readonly DependencyProperty ApplyDirectAccessProperty;

        public static readonly DependencyProperty ValueProperty;
        public static readonly DependencyProperty MaxLengthProperty;

        public static readonly DependencyProperty ValuesSourceProperty;
        public static readonly DependencyProperty ValuesSourceMemberPathProperty;

        public static readonly DependencyProperty CompositesSourceProperty;
        public static readonly DependencyProperty CompositesSourceMemberPathProperty;

        static ExtendedEditText()
        {
            ExtendedEditText.StorageFieldNameProperty = DependencyProperty.Register("StorageFieldName", typeof(string), typeof(ExtendedEditText),
                                                            new FrameworkPropertyMetadata(null));

            ExtendedEditText.ApplyDirectAccessProperty = DependencyProperty.Register("ApplyDirectAccess", typeof(bool), typeof(ExtendedEditText),
                                                            new FrameworkPropertyMetadata(false));

            ExtendedEditText.ValueProperty = DependencyProperty.Register("Value", typeof(string), typeof(ExtendedEditText),
                                              new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnValueChanged)));

            ExtendedEditText.MaxLengthProperty = DependencyProperty.Register("MaxLength", typeof(int), typeof(ExtendedEditText),
                                                  new FrameworkPropertyMetadata(0, new PropertyChangedCallback(OnMaxLengthChanged)));

            ExtendedEditText.ValuesSourceProperty = DependencyProperty.Register("ValuesSource", typeof(IEnumerable<object>), typeof(ExtendedEditText),
                                              new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnValuesSourceChanged)));

            ExtendedEditText.ValuesSourceMemberPathProperty = DependencyProperty.Register("ValuesSourceMemberPath", typeof(string), typeof(ExtendedEditText),
                                              new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnValuesSourceMemberPathChanged)));

            ExtendedEditText.CompositesSourceProperty = DependencyProperty.Register("CompositesSource", typeof(IEnumerable<IRecognizableComposite>), typeof(ExtendedEditText),
                                              new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnCompositesSourceChanged)));

            ExtendedEditText.CompositesSourceMemberPathProperty = DependencyProperty.Register("CompositesSourceMemberPath", typeof(string), typeof(ExtendedEditText),
                                              new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnCompositesSourceMemberPathChanged)));
        }

        public Window ParentWindow { get; protected set; }

        public DataGridRow ParentRow { get; protected set; }

        public ExtendedEditText()
        {
            InitializeComponent();

            this.ListActioner.SetVisible(false);
            this.TreeActioner.SetVisible(false);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // When exposed inside a pop-up, maybe there is no parent Window, therefore use the main.
            this.ParentWindow = this.GetNearestVisualDominantOfType<Window>().NullDefault(Application.Current.MainWindow);
            this.ParentRow = this.GetNearestVisualDominantOfType<DataGridRow>();

            if (this.ParentWindow != null)
                this.ParentWindow.KeyDown += WhenKeyPressed;

            if (this.ApplyDirectAccess && !this.StorageFieldName.IsAbsent())
                this.Editor.Text = this.PerformDirectRead(this.StorageFieldName, this.ParentRow) as string;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.ParentWindow != null)
                    this.ParentWindow.KeyDown -= WhenKeyPressed;
            }
            catch (Exception Problem)
            {
                // this happens when the Loaded event was never fired, hence no event handler was attached nor parent-window was populated.
            }
        }

        private void WhenKeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.ResolveListActionerHide();
                this.ResolveTreeActionerHide(true);
            }
        }

        /// <summary>
        /// Action to be called just after the value has been edited.
        /// </summary>
        public Action<string> EditingAction = null;

        public string Value
        {
            get { return (string)GetValue(ExtendedEditText.ValueProperty); }
            set { SetValue(ExtendedEditText.ValueProperty, value); }
        }
        private static void OnValueChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            // IMPORTANT: Do not accept null values, because it can fall in
            // an event infinite-loop chain when selected thru popup listbox.
            if (evargs.NewValue == null)   // Drop-down cancelled?
                return;

            var Target = depobj as ExtendedEditText;
            var NewValue = (string)evargs.NewValue;
            var OldValue = (Target.ApplyDirectAccess && !Target.StorageFieldName.IsAbsent()
                            ? Target.PerformDirectRead(Target.StorageFieldName, Target.ParentRow).ToStringAlways()
                            : Target.Editor.Text);

            if (OldValue == NewValue)
                return;

            //T Console.WriteLine("Old-Value=[{0}], New-Value=[{1}]", OldValue, NewValue);

            if (Target.EditingAction != null)
                Target.EditingAction(NewValue);

            if (Target.ApplyDirectAccess && !Target.StorageFieldName.IsAbsent()
                && NewValue != Convert.ToString(Target.PerformDirectRead(Target.StorageFieldName, Target.ParentRow)))
                Target.PerformDirectWrite(Target.StorageFieldName, NewValue, Target.ParentRow);

            Target.Editor.Text = NewValue;
            /*- var Binder = Target.Editor.GetBindingExpression(TextBox.TextProperty);
            if (Binder != null)
                Binder.UpdateTarget(); */

        }

        public IEnumerable<object> ValuesSource
        {
            get { return (IEnumerable<object>)GetValue(ExtendedEditText.ValuesSourceProperty); }
            set { SetValue(ExtendedEditText.ValuesSourceProperty, value); }
        }
        private static void OnValuesSourceChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var Target = depobj as ExtendedEditText;
            var NewValue = (IEnumerable<object>)evargs.NewValue;

            Target.ValuesSource = NewValue;
        }

        public string ValuesSourceMemberPath
        {
            get { return (string)GetValue(ExtendedEditText.ValuesSourceMemberPathProperty); }
            set { SetValue(ExtendedEditText.ValuesSourceMemberPathProperty, value); }
        }
        private static void OnValuesSourceMemberPathChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var Target = depobj as ExtendedEditText;
            var NewValue = (string)evargs.NewValue;

            Target.ValuesSourceMemberPath = NewValue;
        }

        public IEnumerable<IRecognizableComposite> CompositesSource
        {
            get { return (IEnumerable<IRecognizableComposite>)GetValue(ExtendedEditText.CompositesSourceProperty); }
            set { SetValue(ExtendedEditText.CompositesSourceProperty, value); }
        }
        private static void OnCompositesSourceChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var Target = depobj as ExtendedEditText;
            var NewValue = (IEnumerable<IRecognizableComposite>)evargs.NewValue;

            Target.CompositesSource = NewValue;
        }

        public string CompositesSourceMemberPath
        {
            get { return (string)GetValue(ExtendedEditText.CompositesSourceMemberPathProperty); }
            set { SetValue(ExtendedEditText.CompositesSourceMemberPathProperty, value); }
        }
        private static void OnCompositesSourceMemberPathChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var Target = depobj as ExtendedEditText;
            var NewValue = (string)evargs.NewValue;

            Target.CompositesSourceMemberPath = NewValue;
        }

        public int MaxLength
        {
            get { return (int)GetValue(ExtendedEditText.MaxLengthProperty); }
            set { SetValue(ExtendedEditText.MaxLengthProperty, MaxLength); }
        }
        private static void OnMaxLengthChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var Target = depobj as ExtendedEditText;
            var NewValue = (int)evargs.NewValue;

            Target.Editor.MaxLength = NewValue;
        }

        public string StorageFieldName
        {
            get { return (string)GetValue(ExtendedEditText.StorageFieldNameProperty); }
            set { SetValue(ExtendedEditText.StorageFieldNameProperty, value); }
        }

        public bool ApplyDirectAccess
        {
            get { return (bool)GetValue(ExtendedEditText.ApplyDirectAccessProperty); }
            set { SetValue(ExtendedEditText.ApplyDirectAccessProperty, value); }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            this.ResolveListActionerShow();
            this.ResolveTreeActionerShow();

            this.Editor.PostCall(textbox => textbox.SelectAll());
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var Target = ((TextBox)sender).GetNearestVisualDominantOfType<ExtendedEditText>();

            if (Target.EditingAction != null)
                Target.EditingAction(Target.Editor.Text);

            if (CanUpdateAtLostFocus && Target.ApplyDirectAccess && !Target.StorageFieldName.IsAbsent()
                && Target.Editor.Text != Target.PerformDirectRead(Target.StorageFieldName, Target.ParentRow).ToStringAlways())
                Target.PerformDirectWrite(Target.StorageFieldName, Target.Editor.Text, Target.ParentRow);

            Target.ResolveListActionerHide();
            Target.ResolveTreeActionerHide();
        }
        private static bool CanUpdateAtLostFocus = true;

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            this.ResolveListActionerShow();
            this.ResolveTreeActionerShow();
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            /* No! (clumsy)
            this.ResolveListActionerHide();
            this.ResolveTreeActionerHide(); */
        }

        private void ResolveListActionerShow()
        {
            if (this.ValuesSource == null || !this.ValuesSource.Any())
                return;

            this.ListActioner.SetVisible(true);
        }

        private void ResolveListActionerHide()
        {
            if (this.PopupSelectorList.IsMouseOver || this.IsMouseOver)
                return;

            this.ListActioner.SetVisible(false);
            this.PopupSelectorList.IsOpen = false;
        }

        private void ListActioner_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.ParentWindow.Cursor = Cursors.Wait;
            this.PopupSelectorList.IsOpen = !this.PopupSelectorList.IsOpen;
            this.ParentWindow.Cursor = Cursors.Arrow;
        }

        private void LbxSelectorList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!this.IsInitialized || e.AddedItems == null || e.AddedItems.Count < 1
                || this.ValuesSource == null || !this.ValuesSource.Any())
                return;

            //T Console.WriteLine("Selected: " + e.AddedItems[0].ToStringAlways());
            Tuple<bool,object> Extraction = null;

            if (this.ValuesSourceMemberPath.IsAbsent())
                Extraction = Tuple.Create(true, e.AddedItems[0]);
            else
                Extraction = General.ExtractPropertyValue(e.AddedItems[0], this.ValuesSourceMemberPath);

            if (Extraction.Item1)
                this.PostCall(edt =>
                    {
                        // NOTE: The can-update-at-lost-focus static flag must be used to ensure
                        //       no cross firing of events trying to update editor and record.
                        CanUpdateAtLostFocus = false;
                        var Value = Extraction.Item2.ToStringAlways();
                        edt.Editor.Text = Value;

                        if (edt.ApplyDirectAccess && !edt.StorageFieldName.IsAbsent())
                            edt.PerformDirectWrite(this.StorageFieldName, Value, this.ParentRow);

                        this.PostCall(ed => CanUpdateAtLostFocus = true, true);
                    });

            this.PopupSelectorList.IsOpen = false;
        }

        private void ResolveTreeActionerShow()
        {
            if (this.CompositesSource == null || !this.CompositesSource.Any())
                return;

            this.TreeActioner.SetVisible(true);
        }

        private void ResolveTreeActionerHide(bool ForceClose = false)
        {
            if (!ForceClose && (this.TrvSelectorTree.IsMouseOver || this.IsMouseOver))
                return;

            this.TreeActioner.SetVisible(false);
            this.PopupSelectorTree.IsOpen = false;
        }

        private void TreeActioner_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.ParentWindow.Cursor = Cursors.Wait;
            this.PopupSelectorTree.IsOpen = !this.PopupSelectorTree.IsOpen;
            this.ParentWindow.Cursor = Cursors.Arrow;
        }

        private void TrvSelectorTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!this.IsInitialized || !this.TrvSelectorTree.IsLoaded || this.TrvSelectorTree.SelectedItem == null
                || this.CompositesSource == null || !this.CompositesSource.Any() || this.CompositesSourceMemberPath.IsAbsent())
                return;

            //T Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss:fff") + " > Selected: " + this.TrvSelectorTree.SelectedItem.ToStringAlways());

            var ExtractedValue = (this.TrvSelectorTree.SelectedItem is IRecognizableComposite
                                    ? ((IRecognizableComposite)this.TrvSelectorTree.SelectedItem).GetContainmentRoute(CompositesSourceMemberPath)
                                    : this.TrvSelectorTree.SelectedItem.ToStringAlways());

            //- this.PostCall(edt => edt.Value = ExtractedValue);
            this.Value = ExtractedValue;

            this.PopupSelectorTree.IsOpen = false;
        }

        /* POSTPONED: Move around with keys and advance focus with Tab (none of these methods did work!)
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            var ParentController = this.GetNearestVisualDominantOfType<ItemsGridControl>();
            var ParentGrid = ParentController.EditingDataGrid;

            if (ParentGrid == null || (e.Key != Key.Down && e.Key != Key.Up))
                return;

            if (e.Key == Key.Down && ParentGrid.SelectedIndex < ParentGrid.Items.Count - 1)
            {
                //No! ParentGrid.CommitEdit();
                // ParentGrid.PostCall(pgrid => pgrid.SelectedIndex = pgrid.SelectedIndex + 1);
                // EditingElement.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
                //ParentCell.PostCall(pcell => pcell.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down)));

                e.Handled = true;
                //var CurrentColumn = ParentGrid.CurrentColumn;

                //ParentGrid.PostCall(
                //    pgrid =>
                //    {
                //        KeyEventArgs eInsertBack = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.Tab);
                //        eInsertBack.RoutedEvent = UIElement.KeyDownEvent;
                //        InputManager.Current.ProcessInput(eInsertBack);

                //        pgrid.PostCall(grd => { grd.CurrentColumn = CurrentColumn; grd.SelectedIndex = grd.SelectedIndex + 1; });
                //    });

                var Index = ParentGrid.Items.IndexOf(ParentGrid.CurrentItem) + 1;
                // ParentGrid.GetCurrentCell().MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
                ParentGrid.PostCall(
                    pgrid =>
                    {
                        pgrid.ScrollIntoView(pgrid.Items[Index], pgrid.CurrentColumn);
                        var Cell = pgrid.GetCell(Index, pgrid.CurrentColumn.DisplayIndex);
                        ParentController.SetSelectedCell(Cell);

                        var MouseTouch = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left);
                        MouseTouch.RoutedEvent = UIElement.PreviewMouseLeftButtonDownEvent;
                        Cell.PostCall(cell => cell.RaiseEvent(MouseTouch));
                    });
            }

            if (e.Key == Key.Up && ParentGrid.SelectedIndex > 0)
            {
                //No! ParentGrid.CommitEdit();
                // ParentGrid.PostCall(pgrid => pgrid.SelectedIndex = pgrid.SelectedIndex - 1);
                //ParentCell.PostCall(pcell => pcell.MoveFocus(new TraversalRequest(FocusNavigationDirection.Up)));
            }
        } */
    }
}
