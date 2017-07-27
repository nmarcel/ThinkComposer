using System;
using System.Collections.Generic;
using System.IO;
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

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;

namespace Instrumind.ThinkComposer.Composer.Generation.Widgets
{
    /// <summary>
    /// Interaction logic for FileGenerationPreviewer.xaml
    /// </summary>
    public partial class FileGenerationPreviewer : UserControl
    {
        public FileGenerationPreviewer()
        {
            InitializeComponent();
        }

        public FileGenerationPreviewer(string FileName, string GeneratedText)
             : this()
        {
            this.TxtFileName.Text = FileName;
            this.TxbOutput.Text = GeneratedText;
        }

        /* private void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            DotLiquid.Template.NamingConvention = new DotLiquid.NamingConventions.CSharpNamingConvention();
            var CompiledTemplate = DotLiquid.Template.Parse(this.TemplateText);
            this.TxbOutput.Text = CompiledTemplate.Render(DotLiquid.Hash.FromAnonymousObject(this.Source));
        } */

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var Extension = Path.GetExtension(this.TxtFileName.Text);

            var DialogResponse = Display.DialogGetSaveFile("Save to...", Extension, "Generated files (*" + Extension + ")|*" + Extension +
                                                                                    "|All files (*.*)|*.*", this.TxtFileName.Text);
            if (DialogResponse == null)
                return;

            General.StringToFile(DialogResponse.LocalPath, this.TxbOutput.Text);
        }
    }
}
