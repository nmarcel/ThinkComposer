using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DotLiquid.Exceptions;
using DotLiquid.FileSystems;
using DotLiquid.Util;

using Instrumind.Common;

namespace DotLiquid.Tags
{
    // A modified 'include' tag just to enable recursive calls between template and subtemplates.
    public class Inject : DotLiquid.Block
    {
        private static int MaxNestedStackCalls = 16;

        //- private static readonly Regex Syntax = new Regex(string.Format(@"({0}+)(\s+(?:with|for)\s+({0}+))?", Liquid.QuotedFragment));
        private static readonly Regex Syntax = new Regex(string.Format(@"({0}+)(\s+(?:with)\s+({0}+))(\s+({0}+))?", Liquid.QuotedFragment));

        private string _templateName, _variableName, _modifierName;
        private Dictionary<string, string> _attributes;

        static Inject()
        {
            // Note: 1st call gives FrameCount = 129; each call produced 16 intermediate frames; max desirable recursions = 16.
            MaxNestedStackCalls = (129 + (16 *
                                   AppExec.GetConfiguration<int>("FileGeneration", "MaxNestedStackCalls", MaxNestedStackCalls)
                                                .EnforceMaximum(64)));
        }

        public override void Initialize(string tagName, string markup, List<string> tokens)
        {
            Match syntaxMatch = Syntax.Match(markup);
            if (syntaxMatch.Success)
            {
                _templateName = syntaxMatch.Groups[1].Value.Replace("'","").Replace("\"","");
                _variableName = syntaxMatch.Groups[3].Value;
                _modifierName = syntaxMatch.Groups[4].Value.Trim();
                if (_variableName == string.Empty)
                    _variableName = null;
                _attributes = new Dictionary<string, string>(Template.NamingConvention.StringComparer);
                R.Scan(markup, Liquid.TagAttributes, (key, value) => _attributes[key] = value);
            }
            else
                throw new SyntaxException("Syntax Error in 'inject' tag - Valid syntax: inject [template] with [variable]");

            base.Initialize(tagName, markup, tokens);
        }

        protected override void Parse(List<string> tokens)
        {
        }

        public override void Render(Context context, IndentationTextWriter result)
        {
            IFileSystem fileSystem = context.Registers["file_system"] as IFileSystem ?? Template.FileSystem;
            Template partial = null;
            var workingTemplateName = _templateName;

            var Trace = new System.Diagnostics.StackTrace(false);

            if (Trace.FrameCount > MaxNestedStackCalls)
                throw new UsageAnomaly("Nesting too deep (possible infinite recursion loop).");

            partial = GetRegisteredSubTemplate(_templateName);

            if (partial == null)
            {
                string source = fileSystem.ReadTemplateFile(context, _templateName);
                partial = Template.Parse(source);
                workingTemplateName = _templateName.Substring(1, _templateName.Length - 2); // old: shortenedTemplateName
            }

            object variable = context[_variableName ?? workingTemplateName];
            variable = (variable is DropProxy ? ((DropProxy)variable).GetObject() : variable);

            var Parameters = new RenderParameters();
            Parameters.LocalVariables = DotLiquid.Hash.FromAnonymousObject(variable);
            Parameters.RethrowErrors = true;

            var PreviousRecursionLevel = RecursionLevel;
            if (_modifierName != "keepindent")
            {
                if (_modifierName == "noindent")
                    RecursionLevel = 0;
                else
                    RecursionLevel++;

                result.IndentationDepth = RecursionLevel;
            }

            result.IsAtLineStart = true;

            partial.Render(result, Parameters);

            if (_modifierName != "keepindent")
            {
                if (_modifierName == "noindent")
                    RecursionLevel = PreviousRecursionLevel;
                else
                    RecursionLevel--;

                result.IndentationDepth = RecursionLevel;
            }

            /*- old 'include' tag code
            context.Stack(() =>
            {
                foreach (var keyValue in _attributes)
                    context[keyValue.Key] = context[keyValue.Value];

                if (variable is IEnumerable)
                {
                    ((IEnumerable) variable).Cast<object>().ToList().ForEach(v =>
                    {
                        context[workingTemplateName] = v;
                        partial.Render(result, RenderParameters.FromContext(context));
                    });
                    return;
                }

                context[workingTemplateName] = variable;
                partial.Render(result, RenderParameters.FromContext(context));
            }); */
        }

        public static int RecursionLevel { get; private set; }

        // -----------------------------------------------------------------------------------------
        public static string CurrentConsumerContextId = "";    // Sould be CurrentCompositionGuid

        private static Dictionary<string, Dictionary<string, Template>> RegisteredSubTemplates =
                    new Dictionary<string, Dictionary<string, Template>>();

        public static Template GetRegisteredSubTemplate(string Name)
        {
            // Search by Current-Consumer-Context-Id including a partial match
            var SearchKey = CurrentConsumerContextId;
            if (!RegisteredSubTemplates.ContainsKey(SearchKey))
                SearchKey = RegisteredSubTemplates.Keys.FirstOrDefault(key => key.StartsWith(CurrentConsumerContextId));

            if (SearchKey != default(string)
                && RegisteredSubTemplates[SearchKey].ContainsKey(Name))
                return RegisteredSubTemplates[SearchKey][Name];

            return null;
        }

        public static void RegisterSubTemplate(string Name, Template CompiledTemplate)
        {
            RegisteredSubTemplates.AddNew(CurrentConsumerContextId, new Dictionary<string, Template>());
            var Container = RegisteredSubTemplates[CurrentConsumerContextId];
            Container.AddOrReplace(Name, CompiledTemplate);
        }
    }
}