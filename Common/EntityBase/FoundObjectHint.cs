using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.Visualization;

/// Provides a foundation of structures and services for business entities management, considering its life cycle, edition and persistence mapping.
namespace Instrumind.Common.EntityBase
{
    public class FoundObjectHint
    {
        public IMModelClass SourceObject { get; set; }

        public string SourceObjectType { get; set; }

        public string SourceObjectName
        {
            get
            {
                if (this.SourceObject is IIdentifiableElement)
                    return ((IIdentifiableElement)this.SourceObject).Name.RemoveNewLines();

                return this.SourceObject.ToStringAlways();
            }
        }

        public string LocationPath { get; set; }

        public string TextExcerpt { get; set; }

        public int TextPosition { get; set; }

        public bool MustBeDisplacedOnReplace { get; set; }

        public string SourcePropertyName
        {
            get
            {
                if (SourceAccessor != null)
                    return SourceAccessor.Name;

                return "Text";
            }
        }

        /// <summary>
        /// Property-definitor or null for Visual Complements.
        /// </summary>
        public MModelPropertyDefinitor SourceAccessor { get; set; }

        /// <summary>
        /// When source property is a rich-document (XAML/RTF), it references the Temporal-Document
        /// and the precise text-run containing the searched text.
        /// </summary>
        public Tuple<FlowDocument, Run> RichDocumentRunSource { get; set; }

        // ========================================================================================
        internal static Tuple<FlowDocument, IList<Run>> RegisterTemporalDocument(IMModelClass SourceObject, string PropertyName, string DocumentSource)
        {
            if (TemporalDocuments == null)
                TemporalDocuments = new List<Tuple<IMModelClass, string, FlowDocument, IList<Run>>>();

            var Document = Display.XamlRichTextToFlowDocument(DocumentSource).NullDefault(new FlowDocument());

            var DocRuns = Document.GetAllRuns();
            TemporalDocuments.Add(Tuple.Create(SourceObject,PropertyName, Document, DocRuns));

            return Tuple.Create(Document, DocRuns);
        }

        public static void ClearTemporalDocuments()
        {
            TemporalDocuments = null;
        }

        public static Tuple<FlowDocument, IList<Run>> GetTemporalDocumentRunsFor(IMModelClass SourceObject, string PropertyName)
        {
            var Result = (TemporalDocuments == null ? null
                          : TemporalDocuments.FirstOrDefault(doc => doc.Item1 == SourceObject && doc.Item2 == PropertyName));

            return (Result == null ? null : Tuple.Create(Result.Item3, Result.Item4));
        }

        public static void ApplyChanges(IEnumerable<FoundObjectHint> ChangedHints)
        {
            var AffectedDocs = ChangedHints.Where(hint => hint.RichDocumentRunSource != null)
                                    .Select(hint => Tuple.Create(hint.SourceObject, hint.SourceAccessor, hint.RichDocumentRunSource.Item1)).Distinct();

            foreach (var Doc in AffectedDocs)
            {
                var Range = new TextRange(Doc.Item3.ContentStart, Doc.Item3.ContentEnd);
                using (var Torrent = new MemoryStream())
                {
                    Range.Save(Torrent, DataFormats.Xaml);
                    var Text = Torrent.StreamToString();
                    Doc.Item2.Write(Doc.Item1, Text);
                }
            }
        }

        private static List<Tuple<IMModelClass, string, FlowDocument, IList<Run>>> TemporalDocuments = null;
    }
}
