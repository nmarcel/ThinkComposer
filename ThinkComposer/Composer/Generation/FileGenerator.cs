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
// File   : FileGenerator.cs
// Object : Instrumind.ThinkComposer.Composer.Generation.FileGenerator (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2013.01.09 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Text;

using DotLiquid;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;
using Instrumind.Common.Visualization.Widgets;

using Instrumind.ThinkComposer.ApplicationProduct;
using Instrumind.ThinkComposer.MetaModel.Configurations;
using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.MetaModel.InformationMetaModel;
using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;
using Instrumind.ThinkComposer.Model;
using Instrumind.ThinkComposer.Model.GraphModel;

/// Provides features for file generation.
namespace Instrumind.ThinkComposer.Composer.Generation
{
    /// <summary>
    /// Generates files from Compositions based on Idea content and their Definition template.
    /// </summary>
    public partial class FileGenerator
    {
        // -----------------------------------------------------------------------------------------
        public const string DEFAULT_INITIAL_TEMPLATE_NAME = ""; // This must be empty or at least enclosed in special chars

        static FileGenerator()
        {
            DotLiquid.Template.NamingConvention = new DotLiquid.NamingConventions.CSharpNamingConvention();
        }

        public static GenerationResult GenerateFilePreview(Idea Source, ExternalLanguageDeclaration Language,
                                                           bool FailWhenInvalid = false)
        {
            var TemplateText = Source.IdeaDefinitor.GetGenerationFinalTemplate(Language);
            var Result = GenerateFilePreview(Source, TemplateText, FailWhenInvalid);
            return Result;
        }

        public static GenerationResult GenerateFilePreview(Idea Source, string SourceTemplateText, bool FailWhenInvalid = false)
        {
            GenerationResult Result = null;
            System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

            try
            {
                var CompiledTemplate = CreateCompiledTemplate(Source.IdeaDefinitor.ToString(), SourceTemplateText, FailWhenInvalid).Result;

                var Parameters = new RenderParameters();
                Parameters.LocalVariables = DotLiquid.Hash.FromAnonymousObject(Source);
                Parameters.RethrowErrors = true;

                var GeneratedOutput = CompiledTemplate.Render(Parameters);
                Result = new GenerationResult((Source.OwnerComposition.CompositeContentDomain.GenerationConfiguration.UseIdeaTechNameForFileNaming
                                               ? Source.TechName : Source.Name) + GenerationManager.DEFAULT_GEN_EXT, GeneratedOutput);
            }
            finally
            {
                System.Windows.Input.Mouse.OverrideCursor = null;
            }

            return Result;
        }

        /// <summary>
        /// Creates a compiled Template, plus possible sub-templates, from the supplied source-template-text.
        /// </summary>
        public static OperationResult<Template> CreateCompiledTemplate(string SourceDefName, string SourceTemplateText, bool FailWhenInvalid = false)
        {
            Template Result = null;

            try
            {
                var ContainedTemplateTexts = GetContainedTemplateTexts(SourceTemplateText);

                foreach(var Subtemplate in ContainedTemplateTexts)
                {
                    var CompiledTemplate = DotLiquid.Template.Parse(Subtemplate.Value);

                    if (Result == null) // If at the first one
                        Result = CompiledTemplate;

                    if (Subtemplate.Key != "")  // Register subtemplates with name (including the first-one/main if is not anonymous)
                        DotLiquid.Tags.Inject.RegisterSubTemplate(Subtemplate.Key, CompiledTemplate);
                }
            }
            catch (Exception Problem)
            {
                var Message = "Template from '" + SourceDefName + "' is invalid. Problem: " + Problem.Message;
                Console.WriteLine(Message);

                if (FailWhenInvalid)
                    throw new BusinessAnomaly(Message);

                return OperationResult.Failure<Template>(Message);
            }

            return OperationResult.Success(Result);
        }

        public static Dictionary<string,string> GetContainedTemplateTexts(string SourceTemplateText)
        {
            var Result = new Dictionary<string,string>();
            var Reader = new StringReader(SourceTemplateText);
            var Builder = new StringBuilder();
            var Declarer = GenerationManager.GENPAR_PREFIX + GenerationManager.GENKEY_SEC_SUBTEMPLATE + GenerationManager.GENPAR_ASSIGN;
            var CurrentSubtemplateName = DEFAULT_INITIAL_TEMPLATE_NAME;
            var NewSubtemplateName = "";
            bool AddSubtemplate = false;
            string Line = null;

            do
            {
                Line = Reader.ReadLine();

                if (Line == null)
                    AddSubtemplate = true;
                else
                    if (Line.TrimStart().ToUpper().StartsWith(Declarer))
                    {
                        NewSubtemplateName = Line.Substring(Line.IndexOf(Declarer) + Declarer.Length + 1);
                        if (NewSubtemplateName.IsAbsent())
                            throw new UsageAnomaly("Subtemplate has no name declared.");

                        AddSubtemplate = true;
                    }
                    else
                        Builder.AppendLine(Line);

                if (AddSubtemplate)
                {
                    AddSubtemplate = false;

                    if (Builder.Length > 0)
                    {
                        Result.Add(CurrentSubtemplateName, Builder.ToString());
                        Builder.Clear();
                    }

                    CurrentSubtemplateName = NewSubtemplateName;
                }

            } while (Line != null);

            return Result;
        }

        // -----------------------------------------------------------------------------------------
        public Composition SourceComposition { get; protected set; }
        public ExternalLanguageDeclaration Language { get; protected set; }
        public FileGenerationConfiguration Configuration { get; protected set; }

        private Template CompositionTemplate = null;
        private Dictionary<IdeaDefinition, Template> CompiledTemplates = new Dictionary<IdeaDefinition, Template>();

        private ThreadWorker<int> CurrentWorker { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public FileGenerator(Composition SourceComposition, ExternalLanguageDeclaration Language, FileGenerationConfiguration NewConfiguration)
        {
            this.SourceComposition = SourceComposition;
            this.Language = Language;

            if (NewConfiguration == null)
                NewConfiguration = new FileGenerationConfiguration();

            this.Configuration = NewConfiguration;
        }

        /// <summary>
        /// Generates Files based on the current Configuration.
        /// Returns operation-result.
        /// </summary>
        public OperationResult<int> Generate(ThreadWorker<int> Worker)
        {
            General.ContractRequiresNotNull(Worker);
            var GeneratedFiles = 0;

            try
            {
                this.CurrentWorker = Worker;
                this.CurrentWorker.ReportProgress(0, "Starting.");

                // Get Composition level template
                var CompoTextTemplate = this.SourceComposition.IdeaDefinitor.GetGenerationFinalTemplate(this.Language);
                this.CompositionTemplate = (CompoTextTemplate.IsAbsent()
                                            ? null : CreateCompiledTemplate(this.SourceComposition.CompositeContentDomain.ToString(), CompoTextTemplate, true).Result);

                // Determine excluded ideas
                var ExcludedIdeas = this.SourceComposition.DeclaredIdeas.Where(idea => idea.GlobalId.ToString().IsIn(this.Configuration.ExcludedIdeasGlobalIds)).ToArray();

                // Travel subgraph
                var WorkingDirExists = false;
                GeneratedFiles = this.GenerateIdeaFiles(this.SourceComposition, ExcludedIdeas, 1.0, 99.0,
                                                        this.Configuration.TargetDirectory, ref WorkingDirExists,
                                                        this.Configuration.CreateCompositionRootDirectory);

                // Finish
                this.CurrentWorker.ReportProgress(100, "Generation complete.");
                this.CurrentWorker = null;
            }
            catch (Exception Problem)
            {
                this.CurrentWorker = null;
                return OperationResult.Failure<int>("Cannot complete generation.\nProblem: " + Problem.Message, Result: GeneratedFiles);
            }

            return OperationResult.Success<int>(GeneratedFiles, "A total of " + GeneratedFiles.ToString() + " files have been generated at:\n" + this.Configuration.TargetDirectory);
        }

        // -----------------------------------------------------------------------------------------
        public int GenerateIdeaFiles(Idea SourceIdea, IEnumerable<Idea> ExcludedIdeas, double ProgressPercentageStart, double ProgressPercentageEnd,
                                     string WorkingDirectory, ref bool WorkingDirExists, bool CreateContentDir = true)
        {
            // Generate file, if selected
            var FilesGenerated = 0;
            var FileName = SourceIdea.TechName;

            if (!(SourceIdea.IsIn(ExcludedIdeas) || ((SourceIdea is Relationship) && !this.Configuration.GenerateFilesForRelationships)))
            {
                // Determine compiled-template to use
                Template CompiledTemplate = null;

                if (SourceIdea is Composition)
                    CompiledTemplate = this.CompositionTemplate;
                else
                    if (this.CompiledTemplates.ContainsKey(SourceIdea.IdeaDefinitor))
                        CompiledTemplate = this.CompiledTemplates[SourceIdea.IdeaDefinitor];
                    else
                    {
                        var TemplateText = SourceIdea.IdeaDefinitor.GetGenerationFinalTemplate(this.Language);
                        if (!TemplateText.IsAbsent())
                        {
                            CompiledTemplate = CreateCompiledTemplate(SourceIdea.IdeaDefinitor.ToString(), TemplateText, true).Result;
                            this.CompiledTemplates.Add(SourceIdea.IdeaDefinitor, CompiledTemplate);
                        }
                    }

                // Apply template to source Idea
                if (CompiledTemplate != null)
                {
                    var GeneratedOutput = CompiledTemplate.Render(DotLiquid.Hash.FromAnonymousObject(SourceIdea));
                    var GeneratedResult = new GenerationResult((this.Configuration.UseIdeaTechNameForFileNaming
                                                                ? SourceIdea.TechName : SourceIdea.Name) +
                                                               GenerationManager.DEFAULT_GEN_EXT, GeneratedOutput);

                    // Create directory and save file, if needed
                    if (!WorkingDirExists)
                    {
                        if (!Directory.Exists(WorkingDirectory))
                            Directory.CreateDirectory(WorkingDirectory);

                        WorkingDirExists = true;
                    }

                    FileName = GeneratedResult.FileName;
                    var GenerationPath = Path.Combine(WorkingDirectory, FileName);
                    General.StringToFile(GenerationPath, GeneratedResult.GeneratedText);

                    FilesGenerated = 1;
                }
            }

            // Determine Ideas to generate
            if (SourceIdea.CompositeIdeas.Count > 0)
            {
                var SelectedIdeas = SourceIdea.CompositeIdeas.Where(ideasel => !ideasel.IsIn(ExcludedIdeas) || ideasel.CompositeIdeas.Count > 0
                                                                               || (this.Configuration.GenerateFilesForRelationships && ideasel is Relationship)).ToList();

                // Create content directory
                var ContentDirectory = (CreateContentDir
                                        ? Path.Combine(WorkingDirectory, Path.GetFileNameWithoutExtension(FileName) + this.Configuration.CompositeContentSubdirSuffix.Trim())
                                        : WorkingDirectory);
                var ContentDirExists = !CreateContentDir;

                // Determine progress
                var ProgressStep = ((ProgressPercentageEnd - ProgressPercentageStart) / (double)SelectedIdeas.Count);
                var ProgressPercentage = ProgressPercentageStart; 
                
                // Travel composites
                foreach (var SelectedIdea in SelectedIdeas)
                {
                    ProgressPercentage += ProgressStep;
                    FilesGenerated += GenerateIdeaFiles(SelectedIdea, ExcludedIdeas, ProgressPercentage, (ProgressPercentage + ProgressStep),
                                                        ContentDirectory, ref ContentDirExists);
                }
            }

            return FilesGenerated;
        }

        // -----------------------------------------------------------------------------------------
    }
}
