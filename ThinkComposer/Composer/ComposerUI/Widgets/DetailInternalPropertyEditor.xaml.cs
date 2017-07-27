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
    /// Interaction logic for DetailInternalPropertyEditor.xaml
    /// PENDING... MAKE THIS WORK FOR EDITING AN INTERNAL PROPERTY OF ANY TYPE
    /// (USE PROPERTY-EXPOSITOR AND APPROPRIATE EDITING CONTROLS)
    /// </summary>
    public partial class DetailInternalPropertyEditor : UserControl
    {
        // -----------------------------------------------------------------------------------------
        public static void Edit(DocumentEngine Engine, MModelPropertyDefinitor Property, Idea TargetIdea)
        {
            Engine.StartCommandVariation("Edit Internal Property");
            DialogOptionsWindow EditingWindow = null;
            var Editor = new DetailInternalPropertyEditor("Edit internal property...", Property, TargetIdea);
            var Changed = Display.OpenContentDialogWindow<DetailInternalPropertyEditor>(ref EditingWindow, "Edit Property", Editor);
            if (Changed.IsTrue())
            {
                TargetIdea.UpdateVersion();
                Engine.CompleteCommandVariation();
            }
            else
                Engine.DiscardCommandVariation();
        }

        // -----------------------------------------------------------------------------------------
        public DetailInternalPropertyEditor()
        {
            InitializeComponent();
        }

        public DetailInternalPropertyEditor(string Title, MModelPropertyDefinitor Property, Idea TargetIdea)
             : this()
        {
            this.Property = Property;
            this.TargetIdea = TargetIdea;
        }

        public MModelPropertyDefinitor Property { get; protected set; }
        public Idea TargetIdea { get; protected set; }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.PropertyName.Text = this.Property.Name;
            this.PropertyName.ToolTip = this.Property.Summary;

            this.PropertyValue.Text = this.Property.Read(this.TargetIdea).ToStringAlways();
            this.PropertyValue.Focus();
            this.PropertyValue.SelectAll();
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Property.Write(this.TargetIdea, this.PropertyValue.Text);
            }
            catch (Exception Problem)
            {
                Console.WriteLine("Cannot store the given Value into Property '{0}'.\nProblem: {1}.", this.Property, Problem.Message);
            }

            // PENDING: VALIDATE ENTERED LOCATION
            var ParentWindow = this.GetNearestDominantOfType<Window>();
            ParentWindow.DialogResult = true;
            ParentWindow.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            // Here the ParentWindow.Close() is automatic because the calling button was defined as "Cancel" button.
        }

    }
}
