using System;
using System.Collections.Generic;
using System.Diagnostics;
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

using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.MetaModel.InformationMetaModel;
using Instrumind.ThinkComposer.Model.GraphModel;
using Instrumind.ThinkComposer.Model.InformationModel;

namespace Instrumind.ThinkComposer.Composer.ComposerUI.Widgets
{
    /// <summary>
    /// Interaction logic for DetailLinkEditor.xaml
    /// </summary>
    public partial class DetailLinkEditor : UserControl
    {
        // -----------------------------------------------------------------------------------------
        /// <summary>
        /// Shows a dialog window for select or enter a link, starting from a supplied designator, idea and initial target.
        /// Returns the new selected link, or null if cancelled.
        /// </summary>
        public static Link ShowDialog(string Title, Assignment<DetailDesignator>/*L*/ Designator, Idea TargetIdea, Link InitialTarget = null)
        {
            DialogOptionsWindow EditingWindow = null;
            var Editor = new DetailLinkEditor(Title, InitialTarget, Designator, TargetIdea);
            var Result = Display.OpenContentDialogWindow<DetailLinkEditor>(ref EditingWindow, "Edit Link", Editor);
            if (Result.IsTrue())
                return Editor.SelectedLinkTarget;

            return null;
        }

        // -----------------------------------------------------------------------------------------
        public DetailLinkEditor()
        {
            InitializeComponent();

            this.ComboPropertySelector.DataContext = this;
            this.AvailableProperties = InternalLinkType.InternalTypeAny.LinkOptions;
            this.ComboPropertySelector.SelectedItem = this.AvailableProperties.First();

            this.Location.Editor.TextWrapping = TextWrapping.Wrap;
            this.Location.ValuesSource = General.GetLastTypedURLs();
        }

        public DetailLinkEditor(string Title, Link InitialTarget, Assignment<DetailDesignator>/*L*/ Designator, Idea TargetIdea)
             : this()
        {
            this.Title.Text = Title;
            this.InitialTarget = InitialTarget;
            this.Designator = Designator;

            var DefaultTarget = ((LinkDetailDesignator)Designator.Value).Initializer;

            if (this.InitialTarget == null)
            {
                var PropLink = new InternalLink(TargetIdea, Designator);

                if (DefaultTarget != null && DefaultTarget is MModelPropertyDefinitor)
                {
                    var PropDef = DefaultTarget.CreateCopy(ECloneOperationScope.Deep, null) as MModelPropertyDefinitor;
                    PropLink.TargetProperty = PropDef;
                }

                this.InitialTarget = PropLink;
            }

            if (this.InitialTarget is ResourceLink)
            {
                this.LinkTargetForResource = InitialTarget as ResourceLink;

                this.LinkTargetForInternal = new InternalLink(TargetIdea, Designator);
                // IMPORTANT: The related designator is shared, therefore it updated when SelectedLinkTarget is getted.

                this.Location.Editor.Text = (LinkTargetForResource == null ? "" : LinkTargetForResource.TargetLocation);

                this.TabLinkExternal.IsSelected = true;
            }
            else
            {
                this.LinkTargetForInternal = this.InitialTarget as InternalLink;

                this.LinkTargetForResource = new ResourceLink(TargetIdea, Designator);
                // IMPORTANT: The related designator is shared, therefore is updated when SelectedLinkTarget is getted.

                if (this.AvailableProperties.Contains(this.LinkTargetForInternal.TargetProperty))
                    this.ComboPropertySelector.SelectedItem = this.LinkTargetForInternal.TargetProperty;
                else
                    this.ComboPropertySelector.SelectedItem = this.AvailableProperties.FirstOrDefault();

                this.TabLinkInternal.IsSelected = true;
            }
        }
        private Link InitialTarget = null;
        private Assignment<DetailDesignator> Designator = null;

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.InitialTarget is ResourceLink)
            {
                this.Location.Focus();
                this.Location.Editor.SelectAll();
            }
            else
            {
                this.TabLinkInternal.IsSelected = true;
                this.ComboPropertySelector.Focus();
            }
        }

        protected ResourceLink LinkTargetForResource = null;
        protected InternalLink LinkTargetForInternal = null;
        public Link SelectedLinkTarget
        {
            get
            {
                if (this.Cancelled)
                    return null;

                if (TabLinkInternal.IsSelected)
                {
                    this.LinkTargetForInternal.TargetProperty = this.ComboPropertySelector.SelectedItem as MModelPropertyDefinitor;

                    // IMPORTANT: The designator is shared, therefore it must be updated.
                    this.LinkTargetForInternal.Designator.DeclaringLinkType = InternalLinkType.InternalTypeAny;

                    return this.LinkTargetForInternal;
                }
                else
                {
                    this.LinkTargetForResource.TargetLocation = this.Location.Editor.Text;

                    // IMPORTANT: The designator is shared, therefore it must be updated.
                    this.LinkTargetForResource.Designator.DeclaringLinkType = ResourceLinkType.ResourceTypeAny;

                    return this.LinkTargetForResource;
                }
            }
            set
            {
                if (value is ResourceLink)
                {
                    this.ComboPropertySelector.SelectedItem = this.AvailableProperties.FirstOrDefault();
                    this.Location.Editor.Text = value.Target as string;
                }
                else
                {
                    this.Location.Editor.Text = "";

                    if (this.AvailableProperties.Count(prop => prop.IsEqual(value.Target)) < 1)
                    {
                        this.ComboPropertySelector.SelectedItem = this.AvailableProperties.FirstOrDefault();
                        return;
                    }

                    this.ComboPropertySelector.SelectedItem = value.Target;
                }
            }
        }

        public IEnumerable<IRecognizableElement> AvailableProperties { get; protected set; }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            // PENDING: VALIDATE ENTERED LOCATION
            var ParentWindow = this.GetNearestDominantOfType<Window>();
            ParentWindow.DialogResult = true;
            ParentWindow.Close();
        }

        private bool Cancelled = false;

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Cancelled = true;
            // Here the ParentWindow.Close() is automatic because the calling button was defined as "Cancel" button.
        }

        private void BtnGetUrl_Click(object sender, RoutedEventArgs e)
        {
            // PENDING 
            var WebURL = General.GetLastTypedURL();

            var NewURL = General.Execute(() => Clipboard.GetText(), "Cannot access Windows Clipboard!").Result;
            if (NewURL.IsAbsent())
                return;

            if (WebURL.IsAbsent())
                WebURL = NewURL;

            if (WebURL.ToLower().StartsWith("www."))
                WebURL = "http://" + WebURL;

            if (!WebURL.ToLower().StartsWith("http://"))
                return;

            this.Location.Editor.Text = WebURL;
        }

        private void BtnSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            var Result = Display.DialogGetOpenFolder("Select folder to be linked...");
            if (Result == null)
                return;

            this.Location.Editor.Text = Result.OriginalString;
        }

        private void BtnSelectFile_Click(object sender, RoutedEventArgs e)
        {
            var Result = Display.DialogGetOpenFile("Select File...");
            if (Result == null)
                return;

            this.Location.Editor.Text = Result.OriginalString;
        }

        private void BtnGo_Click(object sender, RoutedEventArgs e)
        {
            var Target = this.Location.Editor.Text.Trim();
            if (Target.IsAbsent())
                return;

            AppExec.CallExternalProcess(Target);
        }

        private void ComboPropertySelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
            var Selector = (sender == null || !(sender is ComboBox) ? null : (ComboBox)sender);
            if (Selector == null || e.AddedItems.Count < 1)
                return;

            this.LinkTargetForInternal.TargetProperty = Selector.SelectedItem as MModelPropertyDefinitor;
        }
    }
}
