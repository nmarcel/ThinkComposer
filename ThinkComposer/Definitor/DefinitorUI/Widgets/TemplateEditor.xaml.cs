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
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.Composer.Generation;
using Instrumind.ThinkComposer.MetaModel;
using Instrumind.ThinkComposer.MetaModel.Configurations;

namespace Instrumind.ThinkComposer.Definitor.DefinitorUI.Widgets
{
    /// <summary>
    /// Interaction logic for TemplateEditor.xaml
    /// </summary>
    public partial class TemplateEditor : UserControl, IEntityViewChild
    {
        public Domain BaseDomain { get; protected set; }

        public Type IdeaSourceType { get; protected set; }

        public IModelEntity SourceEntity { get; protected set; }

        public EntityEditEngine EditEngine { get { return (this.SourceEntity == null ? null : this.SourceEntity.EditEngine); } }

        public MModelCollectionDefinitor TemplatesCollectionDef { get; protected set; }

        public MModelCollectionDefinitor BaseTemplatesCollectionDef { get; protected set; }

        public IList<TextTemplate> WorkingTemplatesCollection { get; protected set; }

        public TextTemplate CurrentTemplate
        {
            get
            {
                var Template = this.WorkingTemplatesCollection.FirstOrDefault(tpl => tpl.Language == this.BaseDomain.CurrentExternalLanguage);

                if (Template == null)
                {
                    Template = new TextTemplate(this.BaseDomain.CurrentExternalLanguage, "", this.BaseTemplatesCollectionDef != null);
                    this.WorkingTemplatesCollection.Add(Template);
                }

                return Template;
            }
        }


        public TemplateEditor()
        {
            InitializeComponent();
        }

        public void Initialize(Domain BaseDomain, string DefaultLanguageTechName, Type IdeaSourceType,
                               IModelEntity SourceEntity, MModelCollectionDefinitor TemplatesCollectionDef, MModelCollectionDefinitor BaseTemplatesCollectionDef,
                               bool ShowWindowButtons = false, params Tuple<string, ImageSource, string, Action<string>>[] TextEditorExtraButtons)
        {
            this.BaseDomain = BaseDomain;
            this.IdeaSourceType = IdeaSourceType;
            this.SourceEntity = SourceEntity;
            this.TemplatesCollectionDef = TemplatesCollectionDef;
            this.BaseTemplatesCollectionDef = BaseTemplatesCollectionDef;

            this.WorkingTemplatesCollection = this.TemplatesCollectionDef.Read(this.SourceEntity) as IList<TextTemplate>;

            this.CbxLanguage.ItemsSource = this.BaseDomain.ExternalLanguages;
            this.CbxLanguage.SelectionChanged += ((sdr, args) => this.TemplateLanguageSelection(args.AddedItems[0] as ExternalLanguageDeclaration));

            if (this.BaseTemplatesCollectionDef != null)
            {
                this.ChbExtendsBaseTemplate.Content = TextTemplate.__ExtendsBaseTemplate.Name;
                this.ChbExtendsBaseTemplate.ToolTip = TextTemplate.__ExtendsBaseTemplate.Summary;
            }
            else
            {
                this.BtnEditBaseTemplate.SetVisible(false);
                this.ChbExtendsBaseTemplate.SetVisible(false);
            }

            /*- Action BaseTemplateExtensionChange = 
                    () =>
                    {
                        var Editor = this;  // Notice the indirect reference, due to CurrentTemplate change depending on Language
                        Editor.CurrentTemplate.ExtendsBaseTemplate = Editor.ChbExtendsBaseTemplate.IsChecked.IsTrue();
                    };

            this.ChbExtendsBaseTemplate.Checked += ((sdr, args) => BaseTemplateExtensionChange());
            this.ChbExtendsBaseTemplate.Unchecked += ((sdr, args) => BaseTemplateExtensionChange()); */

            this.SteSyntaxEditor.Initialize(this.EditEngine, this.SourceEntity.ToString(), this.TemplatesCollectionDef.TechName, this.TemplatesCollectionDef.Name,
                                            () => this.CurrentTemplate.Text,
                                            (text) => this.CurrentTemplate.Text = text,
                                            ShowWindowButtons, TextEditorExtraButtons);
            this.SteSyntaxEditor.SyntaxName = "Template";
            this.SteSyntaxEditor.SyntaxFileExtension = "." + Domain.TEMPLATE_FILE_EXT;

            this.PostCall(ted => ted.CbxLanguage.SelectedItem = ted.BaseDomain.CurrentExternalLanguage);
        }

        public void TemplateLanguageSelection(ExternalLanguageDeclaration NewLanguage)
        {
            this.Apply();

            this.BaseDomain.CurrentExternalLanguage = NewLanguage;

            this.Refresh();
        }

        private void BtnEditBaseTemplate_Click(object sender, RoutedEventArgs e)
        {
            DomainServices.EditBaseTemplates(this.BaseDomain, this.BaseTemplatesCollectionDef, this.IdeaSourceType);
        }

        public IEntityView ParentEntityView
        {
            get { return this.SteSyntaxEditor.ParentEntityView; }
            set { this.SteSyntaxEditor.ParentEntityView = value; }
        }

        public string ChildPropertyName { get { return this.SteSyntaxEditor.ChildPropertyName; }}

        public void Refresh()
        {
            this.SteSyntaxEditor.Refresh();
            this.ChbExtendsBaseTemplate.IsChecked = this.CurrentTemplate.ExtendsBaseTemplate;
        }

        public bool Apply()
        {
            this.CurrentTemplate.ExtendsBaseTemplate = this.ChbExtendsBaseTemplate.IsChecked.IsTrue();
            return this.SteSyntaxEditor.Apply();
        }
    }
}
