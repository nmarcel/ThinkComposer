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
// File   : DocumentEngine.cs
// Object : Instrumind.Common.EntityBase.DocumentEngine (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2013.06.11 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Instrumind.Common;
using Instrumind.Common.Visualization;


/// Provides a foundation of structures and services for business entities management, considering its life cycle, edition and persistence mapping.
namespace Instrumind.Common.EntityBase
{
    /// <summary>
    /// Provides the general mechanism and services for the interactive creation of documents. Search part.
    /// </summary>
    public abstract partial class DocumentEngine : EntityEditEngine, IStorageLocation, INotifyPropertyChanged
    {
        public const int TEXTFOUND_EXCERPT_ADJACENTS_LEN = 10;

        public static string SearchProviderOfTextNameWithoutPropertyAccessor = "[Text]";
        public static Func<IMModelClass, string> SearchProviderOfTextExtractorWithoutPropertyAccessor = null;
        public static Func<IMModelClass, string> SearchProviderOfObjectTypeDetector = null;

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public static void FindTextInTargetProperty(string SearchedText, IMModelClass Target, MModelPropertyDefinitor PropertyAccessor,
                                                    List<FoundObjectHint> OngoingResults, string TraveledPath,
                                                    bool IsCaseSensitive, bool IsInspectingWholeWord,
                                                    FlowDocument CurrentDocument = null, Run CurrentTextRun = null)
        {
            string Text = null;

            if (CurrentTextRun != null)
                Text = CurrentTextRun.Text;
            else
            {
                if (PropertyAccessor != null)
                    Text = PropertyAccessor.Read(Target) as string;
                else
                    if (DocumentEngine.SearchProviderOfTextExtractorWithoutPropertyAccessor != null)
                        Text = DocumentEngine.SearchProviderOfTextExtractorWithoutPropertyAccessor(Target);

                if (Text.IsAbsent())
                    return;

                if (PropertyAccessor != null && PropertyAccessor.HasRichContent)
                {
                    var DocRuns = FoundObjectHint.GetTemporalDocumentRunsFor(Target, (PropertyAccessor == null
                                                                                      ? DocumentEngine.SearchProviderOfTextNameWithoutPropertyAccessor : PropertyAccessor.Name));
                    if (DocRuns == null)
                        DocRuns = FoundObjectHint.RegisterTemporalDocument(Target, PropertyAccessor.Name, Text);

                    if (DocRuns != null)
                        foreach (var DocRun in DocRuns.Item2)
                            FindTextInTargetProperty(SearchedText, Target, PropertyAccessor, OngoingResults, TraveledPath,
                                                     IsCaseSensitive, IsInspectingWholeWord, DocRuns.Item1, DocRun);

                    return;
                }
            }

            var Position = -1;
            var CaseComparer = (IsCaseSensitive
                                ? StringComparison.Ordinal  // Must use 'ordinal' to avoid considering hyphens as part of words (culture dependant).
                                : StringComparison.OrdinalIgnoreCase);

            Position = Text.IndexOf(SearchedText, CaseComparer);

            var ToBeDisplacedOnReplace = false;    // Used when searched-text found more than once in the same property

            /*T if (Text.StartsWith("THE "))
                Console.WriteLine("THE "); */

            while (Position >= 0)
            {
                if (!(IsInspectingWholeWord &&
                      ((Position > 0 && (Char.IsLetterOrDigit(Text[Position - 1]) || Text[Position - 1] == '_')) ||
                       ((Position + SearchedText.Length) < Text.Length
                        && (Char.IsLetterOrDigit(Text[Position + SearchedText.Length]) || Text[Position + SearchedText.Length] == '_')))))
                {

                    var ExcerptStart = (Position - TEXTFOUND_EXCERPT_ADJACENTS_LEN).EnforceMinimum(0);
                    var ExcerptLength = ((Position - ExcerptStart) + SearchedText.Length + TEXTFOUND_EXCERPT_ADJACENTS_LEN).EnforceMaximum(Text.Length - ExcerptStart);
                    var Excerpt = Text.Substring(ExcerptStart, ExcerptLength).RemoveNewLines();

                    var Result = new FoundObjectHint
                    {
                        SourceObject = Target,
                        SourceObjectType = (DocumentEngine.SearchProviderOfObjectTypeDetector == null ? "[Other]" : DocumentEngine.SearchProviderOfObjectTypeDetector(Target)),
                        SourceAccessor = PropertyAccessor,
                        LocationPath = TraveledPath.AbsentDefault("[Root]"),
                        TextExcerpt = Excerpt,
                        TextPosition = Position,
                        MustBeDisplacedOnReplace = ToBeDisplacedOnReplace,
                        RichDocumentRunSource = (CurrentDocument == null || CurrentTextRun == null
                                                 ? null : Tuple.Create(CurrentDocument, CurrentTextRun))
                    };

                    OngoingResults.Add(Result);

                    ToBeDisplacedOnReplace = true;
                }

                Position = Position + SearchedText.Length;
                if (Position >= Text.Length)
                    return;

                Position = Text.IndexOf(SearchedText, Position, CaseComparer);
            }
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}
