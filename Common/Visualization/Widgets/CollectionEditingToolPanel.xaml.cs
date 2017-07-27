using System;
using System.Collections;
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
    /// Interaction logic for CollectionEditingToolPanel.xaml
    /// </summary>
    public partial class CollectionEditingToolPanel : UserControl
    {
        public static readonly DependencyProperty TitleProperty;
        public static readonly DependencyProperty TitleBrushProperty;
        public static readonly DependencyProperty CanAddProperty;
        public static readonly DependencyProperty CanDeleteProperty;
        public static readonly DependencyProperty CanEditProperty;
        public static readonly DependencyProperty CanDefineProperty;
        public static readonly DependencyProperty CanMoveItemsProperty;

        /// <summary>
        /// Static constructor.
        /// </summary>
        static CollectionEditingToolPanel()
        {
            CollectionEditingToolPanel.TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(CollectionEditingToolPanel),
                new FrameworkPropertyMetadata("Title", new PropertyChangedCallback(OnTitleChanged)));

            CollectionEditingToolPanel.TitleBrushProperty = DependencyProperty.Register("TitleBrush", typeof(Brush), typeof(CollectionEditingToolPanel),
                new FrameworkPropertyMetadata(Brushes.Black, new PropertyChangedCallback(OnTitleBrushChanged)));

            CollectionEditingToolPanel.CanAddProperty = DependencyProperty.Register("CanAdd", typeof(bool), typeof(CollectionEditingToolPanel),
                new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnCanAddChanged)));

            CollectionEditingToolPanel.CanDeleteProperty = DependencyProperty.Register("CanDelete", typeof(bool), typeof(CollectionEditingToolPanel),
                new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnCanDeleteChanged)));

            CollectionEditingToolPanel.CanEditProperty = DependencyProperty.Register("CanEdit", typeof(bool), typeof(CollectionEditingToolPanel),
                new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnCanEditChanged)));

            CollectionEditingToolPanel.CanDefineProperty = DependencyProperty.Register("CanDefine", typeof(bool), typeof(CollectionEditingToolPanel),
                new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnCanDefineChanged)));

            CollectionEditingToolPanel.CanMoveItemsProperty = DependencyProperty.Register("CanMoveItems", typeof(bool), typeof(CollectionEditingToolPanel),
                new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnCanMoveItemsChanged)));
        }

        public CollectionEditingToolPanel()
        {
            InitializeComponent();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the tagetted list-box.
        /// </summary>
        public ListBox TargetListBox { get; set; }

        /// <summary>
        /// Gets the target list-box items source as list.
        /// </summary>
        public IList ItemsList { get { return this.TargetListBox.ItemsSource as IList; } }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        // Events with parameters: sender and routed-event-args.

        public event Action<object, RoutedEventArgs> AddClicked;
        public event Action<object, RoutedEventArgs> DeleteClicked;
        public event Action<object, RoutedEventArgs> EditClicked;
        public event Action<object, RoutedEventArgs> DefineClicked;

        /*-?
        public event Action<object, RoutedEventArgs> MoveTopClicked;
        public event Action<object, RoutedEventArgs> MoveUpClicked;
        public event Action<object, RoutedEventArgs> MoveDownClicked;
        public event Action<object, RoutedEventArgs> MoveBottomClicked; */

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Title for the panel.
        /// </summary>
        public string Title
        {
            get { return (string)GetValue(CollectionEditingToolPanel.TitleProperty); }
            set { SetValue(CollectionEditingToolPanel.TitleProperty, value); }
        }

        public Brush TitleBrush
        {
            get { return (Brush)GetValue(CollectionEditingToolPanel.TitleBrushProperty); }
            set { SetValue(CollectionEditingToolPanel.TitleBrushProperty, value); }
        }

        public bool CanAdd
        {
            get { return (bool)GetValue(CollectionEditingToolPanel.CanAddProperty); }
            set { SetValue(CollectionEditingToolPanel.CanAddProperty, value); }
        }

        public bool CanDelete
        {
            get { return (bool)GetValue(CollectionEditingToolPanel.CanDeleteProperty); }
            set { SetValue(CollectionEditingToolPanel.CanDeleteProperty, value); }
        }

        public bool CanEdit
        {
            get { return (bool)GetValue(CollectionEditingToolPanel.CanEditProperty); }
            set { SetValue(CollectionEditingToolPanel.CanEditProperty, value); }
        }

        public bool CanDefine
        {
            get { return (bool)GetValue(CollectionEditingToolPanel.CanDefineProperty); }
            set { SetValue(CollectionEditingToolPanel.CanDefineProperty, value); }
        }

        public bool CanMoveItems
        {
            get { return (bool)GetValue(CollectionEditingToolPanel.CanMoveItemsProperty); }
            set { SetValue(CollectionEditingToolPanel.CanMoveItemsProperty, value); }
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        private static void OnTitleChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            ((CollectionEditingToolPanel)depobj).TitleTextBlock.Text = evargs.NewValue as string;
        }

        private static void OnTitleBrushChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            ((CollectionEditingToolPanel)depobj).TitleTextBlock.Foreground = evargs.NewValue as Brush;
        }

        private static void OnCanAddChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            ((CollectionEditingToolPanel)depobj).BtnAdd.Visibility = ((bool)evargs.NewValue ? Visibility.Visible : Visibility.Hidden);
        }

        private static void OnCanDeleteChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            ((CollectionEditingToolPanel)depobj).BtnDelete.Visibility = ((bool)evargs.NewValue ? Visibility.Visible : Visibility.Hidden);
        }

        private static void OnCanEditChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            ((CollectionEditingToolPanel)depobj).BtnEdit.Visibility = ((bool)evargs.NewValue ? Visibility.Visible : Visibility.Hidden);
        }

        private static void OnCanDefineChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            ((CollectionEditingToolPanel)depobj).BtnDefine.Visibility = ((bool)evargs.NewValue ? Visibility.Visible : Visibility.Hidden);
        }

        private static void OnCanMoveItemsChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            Visibility NewVisibility = ((bool)evargs.NewValue ? Visibility.Visible : Visibility.Hidden);

            ((CollectionEditingToolPanel)depobj).BtnMoveTop.Visibility = NewVisibility;
            ((CollectionEditingToolPanel)depobj).BtnMoveUp.Visibility = NewVisibility;
            ((CollectionEditingToolPanel)depobj).BtnMoveDown.Visibility = NewVisibility;
            ((CollectionEditingToolPanel)depobj).BtnMoveBottom.Visibility = NewVisibility;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var Handler = AddClicked;
            if (Handler != null)
                AddClicked(sender, e);
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var Handler = DeleteClicked;
            if (Handler != null)
                DeleteClicked(sender, e);
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var Handler = EditClicked;
            if (Handler != null)
                EditClicked(sender, e);
        }

        private void BtnDefine_Click(object sender, RoutedEventArgs e)
        {
            var Handler = DefineClicked;
            if (Handler != null)
                DefineClicked(sender, e);
        }

        private void BtnMoveTop_Click(object sender, RoutedEventArgs e)
        {
            if (!this.TargetListBox.HasItems || this.TargetListBox.SelectedIndex < 1)
                return;

            var MovingItem = this.TargetListBox.SelectedItem;
            this.ItemsList.Remove(MovingItem);
            this.ItemsList.Insert(0, MovingItem);

            this.TargetListBox.SelectedIndex = 0;
        }

        private void BtnMoveUp_Click(object sender, RoutedEventArgs e)
        {
            if (!this.TargetListBox.HasItems || this.TargetListBox.SelectedIndex < 1)
                return;

            var MovingItem = this.TargetListBox.SelectedItem;
            var NewSelectedIndex = this.TargetListBox.SelectedIndex - 1;
            this.ItemsList.Remove(MovingItem);
            this.ItemsList.Insert(NewSelectedIndex, MovingItem);

            this.TargetListBox.SelectedIndex = NewSelectedIndex;
        }

        private void BtnMoveDown_Click(object sender, RoutedEventArgs e)
        {
            if (!this.TargetListBox.HasItems || this.TargetListBox.SelectedIndex >= this.TargetListBox.Items.Count)
                return;

            var MovingItem = this.TargetListBox.SelectedItem;
            var NewSelectedIndex = this.TargetListBox.SelectedIndex + 1;
            this.ItemsList.Remove(MovingItem);
            this.ItemsList.Insert(NewSelectedIndex, MovingItem);

            this.TargetListBox.SelectedIndex = NewSelectedIndex;
        }

        private void BtnMoveBottom_Click(object sender, RoutedEventArgs e)
        {
            if (!this.TargetListBox.HasItems || this.TargetListBox.SelectedIndex >= this.TargetListBox.Items.Count)
                return;

            var MovingItem = this.TargetListBox.SelectedItem;
            this.ItemsList.Remove(MovingItem);
            this.ItemsList.Add(MovingItem);

            this.TargetListBox.SelectedIndex = this.TargetListBox.Items.Count - 1;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}
