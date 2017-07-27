using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Instrumind.Common;

namespace Instrumind.ThinkComposer.Composer.Generation
{
    public class GenerationResult
    {
        public GenerationResult(string FileName, string SourceText, bool PreferGeneratedFilename = true)
        {
            this.FileName = FileName.AbsentDefault(Guid.NewGuid().ToString() + GenerationManager.DEFAULT_GEN_EXT);

            var TextAndParameters = ExtractTextAndParameters(SourceText);

            this.GeneratedText = TextAndParameters.Item1;

            if (PreferGeneratedFilename && TextAndParameters.Item2.ContainsKey(GenerationManager.GENKEY_VAR_FILENAME))
                this.FileName = TextAndParameters.Item2[GenerationManager.GENKEY_VAR_FILENAME];
        }

        public string FileName { get; set; }

        public string GeneratedText { get; private set; }

        private Tuple<string, Dictionary<string, string>> ExtractTextAndParameters(string SourceText)
        {
            var Text = new StringBuilder(SourceText.Length);
            var Parameters = new Dictionary<string, string>();

            var Lines = General.ToStrings(SourceText);
            foreach (var Line in Lines)
                if (Line.TrimStart().StartsWith(GenerationManager.GENPAR_PREFIX))
                {
                    var Declaration = Line.Substring(Line.IndexOf(GenerationManager.GENPAR_PREFIX) +
                                                                  GenerationManager.GENPAR_PREFIX.Length).Segment(GenerationManager.GENPAR_ASSIGN);
                    // Notice that only variable assignments have two segments
                    if (Declaration.Length > 1)
                        Parameters[Declaration[0].Trim().ToUpper()] = Declaration[1];
                }
                else
                    Text.AppendLine(Line);

            var Result = Tuple.Create(Text.ToString(), Parameters);
            return Result;
        }
    }
}
