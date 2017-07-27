// -------------------------------------------------------------------------------------------
// Instrumind ThinkComposer
//
// Copyright (C) 2011-2015 Néstor Marcel Sánchez Ahumada.
// http://thinkcomposer.codeplex.com
//
// This file is part of ThinkComposer, which is free software licensed under the GNU General Public License.
// It is provided without any warranty. You should find a copy of the license in the root directory of this software product.
// -------------------------------------------------------------------------------------------
//
// Project: Instrumind ThinkComposer v1.0
// File   : EntitledPanel.cs
// Object : Instrumind.Common.Visualization.Widgets.EntitledPanel (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.06.19 Néstor Sánchez A.  Creation
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Instrumind.Common.Visualization;

/// Library of standard Instrumind WPF custom and user controls.
namespace Instrumind.Common.Visualization.Widgets
{
    /// <summary>
    /// Entitled control for expand/collapse its content.
    /// </summary>
    public partial class EntitledPanel : UserControl, IShellVisualContent, INotifyPropertyChanged
    {
        public static readonly DependencyProperty TitleProperty;
        public static readonly DependencyProperty ShowTitleProperty;
        public static readonly DependencyProperty CanMinimizeProperty;

        /// <summary>
        /// Static constructor.
        /// </summary>
        static EntitledPanel()
        {
            EntitledPanel.TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(EntitledPanel),
                new FrameworkPropertyMetadata("EntitledPanel's Title", new PropertyChangedCallback(OnTitleChanged)));

            EntitledPanel.ShowTitleProperty = DependencyProperty.Register("ShowTitle", typeof(bool), typeof(EntitledPanel),
                new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnShowTitleChanged)));

            EntitledPanel.CanMinimizeProperty = DependencyProperty.Register("CanMinimize", typeof(bool), typeof(EntitledPanel),
                new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnCanMinimizeChanged)));
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public EntitledPanel()
        {
            InitializeComponent();

            this.Height = Double.NaN;
            this.Width = Double.NaN;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="Key">Key/Code for identify the panel.</param>
        /// <param name="Title">Title of the panel.</param>
        /// <param name="Type">Purpose type of the supplied content.</param>
        /// <param name="Content">Visual content to be hosted.</param>
        /// <param name="ShowTitle">Indicates to show the title.</param>
        /// <param name="CanMinimize">Indicates that the panel can be minimized.</param>
        public EntitledPanel(string Key = "", string Title = "", EShellVisualContentType Type = EShellVisualContentType.EditingContent,
                             UIElement Content = null, bool ShowTitle = true, bool CanMinimize = false) : this()
        {
            this.Key = Key;
            this.Title = Title;
            this.ContentType = Type;
            this.Content = Content;
            this.ShowTitle = ShowTitle;
            this.CanMinimize = CanMinimize;

            this.CreationText = "Create new...";
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var TextLabel = this.GetTemplateChild<TextBlock>("TxtFindLabel");
            if (TextLabel == null)
                return;

            TextLabel.Text = this.FindTextLabel;
        }

        /// <summary>
        /// Title for the panel.
        /// </summary>
        public string Title
        {
            get { return (string)GetValue(EntitledPanel.TitleProperty); }
            set { SetValue(EntitledPanel.TitleProperty, value); }
        }

        private static void OnTitleChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var textblock = Display.GetTemplateChild<TextBlock>(depobj, "TitleTextBlock");
            if (textblock == null)
                return;

            textblock.Text = evargs.NewValue as string;
        }

        /// <summary>
        /// Indicates whether to show the title of the panel.
        /// </summary>
        public bool ShowTitle
        {
            get { return (bool)GetValue(EntitledPanel.ShowTitleProperty); }
            set { SetValue(EntitledPanel.ShowTitleProperty, value); }
        }

        private static void OnShowTitleChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var Entitler = Display.GetTemplateChild<Border>(depobj, "TitleBorder");
            if (Entitler == null)
                return;

            var Value = (bool)evargs.NewValue;
            Entitler.SetVisible(Value);

            var ContentBrd = Display.GetTemplateChild<Border>(depobj, "ContentBorder");
            if (ContentBrd == null)
                return;

            ContentBrd.CornerRadius = (Value ? new CornerRadius(0,0,3,3): new CornerRadius());
        }

        /// <summary>
        /// Indicates whether the panel can be minimized (if not, the associated button is hidden).
        /// </summary>
        public bool CanMinimize
        {
            get { return (bool)GetValue(EntitledPanel.CanMinimizeProperty); }
            set { SetValue(EntitledPanel.CanMinimizeProperty, value); }
        }

        private static void OnCanMinimizeChanged(DependencyObject depobj, DependencyPropertyChangedEventArgs evargs)
        {
            var Expander = Display.GetTemplateChild<DockPanel>(depobj, "ExpanderBorder");
            if (Expander == null)
                return;

            Expander.SetVisible((bool)evargs.NewValue);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            OnTitleChanged(this, new DependencyPropertyChangedEventArgs(TitleProperty, Title, Title));
            OnShowTitleChanged(this, new DependencyPropertyChangedEventArgs(ShowTitleProperty, ShowTitle, ShowTitle));
        }

        private double PreviousContainerHeight = 0;
        private void ExpanderBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var container = this.Template.FindName("ContentBorder", this) as Border;
            var expander = this.Template.FindName("ExpanderText", this) as TextBlock;

            if (container == null || expander == null)
                return;

            if (PreviousContainerHeight == 0)
            {
                PreviousContainerHeight = container.Height;
                container.Height = 0;
                expander.Text = "q";
            }
            else
            {
                container.Height = PreviousContainerHeight;
                PreviousContainerHeight = 0;
                expander.Text = "p";
            }
        }

        private void TitleBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount < 2)
               return;

            // var ParentGrid = Display.GetNearestVisualDominantOfType<Grid>(this);
            // PENDING: Select Row (or RowDefinition?) and set current height to 100 or alike.
        }

        public string Key { get; protected set; }

        public EShellVisualContentType ContentType { get; protected set; }

        public FrameworkElement ContentObject { get { return this; } }

        public void Discard()
        {
        }

        // -------------------------------------------------------------------------------------------------------------
        public Action CreateAction
        {
            get { return this.CreateAction_; }
            set
            {
                this.CreateAction_ = value;

                var Handler = this.PropertyChanged;
                if (Handler != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("CanCreateItems"));
                    this.PostCall(pnl => pnl.PropertyChanged(pnl, new PropertyChangedEventArgs("CreationText")));
                }
            }
        }
        private Action CreateAction_ = null;

        public string CreationText { get; set; }

        public bool CanCreateItems { get { return (this.CreateAction != null); } }

        private void BtnCreate_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.CreateAction == null)
                return;

            this.CreateAction();
        }

        // -------------------------------------------------------------------------------------------------------------
        public Action SortAction
        {
            get { return this.SortAction_; }
            set
            {
                this.SortAction_ = value;

                var Handler = this.PropertyChanged;
                if (Handler != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("CanSortItems"));
                    this.PostCall(pnl => pnl.PropertyChanged(pnl, new PropertyChangedEventArgs("SortText")));
                }
            }
        }
        private Action SortAction_ = null;

        public string SortText { get; set; }

        public bool CanSortItems { get { return (this.SortAction != null); } }

        private void BtnSort_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.SortAction == null)
                return;

            this.SortAction();
        }

        // -------------------------------------------------------------------------------------------------------------
        public Action<string> FindAction
        {
            get { return this.FindAction_; }
            set
            {
                this.FindAction_ = value;

                var Handler = this.PropertyChanged;
                if (Handler != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("CanFindItems"));
                    this.PostCall(pnl => pnl.PropertyChanged(pnl, new PropertyChangedEventArgs("FindTextTip")));
                }
            }
        }
        private Action<string> FindAction_ = null;

        public string FindTextTip { get; set; }

        public string FindTextLabel { get; set; }

        public bool CanFindItems { get { return (this.FindAction != null); } }

        private void TxbFind_GotFocus(object sender, RoutedEventArgs e)
        {
            var TextLabel = this.GetTemplateChild<TextBlock>("TxtFindLabel");
            var TextEdit = this.GetTemplateChild<TextBox>("TxbFind");

            TextLabel.Text = "";
            TextEdit.SelectAll();
        }

        private void TxbFind_LostFocus(object sender, RoutedEventArgs e)
        {
            var TextLabel = this.GetTemplateChild<TextBlock>("TxtFindLabel");
            var TextEdit = this.GetTemplateChild<TextBox>("TxbFind");
            if (TextLabel == null || TextEdit == null)
                return;

            if (TextEdit.Text == "")
                TextLabel.Text = this.FindTextLabel;
        }

        private void TxbFind_KeyDown(object sender, KeyEventArgs e)
        {
            if (!e.Key.IsOneOf(System.Windows.Input.Key.Enter, System.Windows.Input.Key.Tab))
                return;

            var Source = this.GetTemplateChild<TextBox>("TxbFind");
            if (this.FindAction == null || Source == null)
                return;

            this.FindAction(Source.Text);
            e.Handled = true;
        }

        private void TxbFind_TextChanged(object sender, TextChangedEventArgs e)
        {
            var TextLabel = this.GetTemplateChild<TextBlock>("TxtFindLabel");
            var TextEdit = this.GetTemplateChild<TextBox>("TxbFind");
            if (TextLabel == null || TextEdit == null)
                return;

            if (TextEdit.Text == "")
                TextLabel.Text = this.FindTextLabel;
            else
                if (TextLabel.Text != "")
                    TextLabel.Text = "";
        }

        // -------------------------------------------------------------------------------------------------------------
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
