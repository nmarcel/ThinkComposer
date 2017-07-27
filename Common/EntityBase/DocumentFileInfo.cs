using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.Visualization;

namespace Instrumind.Common.EntityBase
{
    /// <summary>
    /// Simple document file information record.
    /// </summary>
    public class DocumentFileInfo : IRecognizableElement
    {
        public Guid Id { get; set; }
        public string FileLocation { get; set; }
        public string FileName { get { return Path.GetFileName(FileLocation); } }
        public string Name { get; set; }
        public string TechName { get; set; }
        public string Summary { get; set; }
        public ImageSource Pictogram { get; set; }
        public ImageSource Snapshot { get; set; }
        public string VersionNumRev { get; set; }
        public string VersionCreation { get; set; }
        public string VersionLastModif  { get; set; }
        public string SortKey { get; set; }

        public int CompareTo(object obj)
        {
            return this.FileLocation.CompareTo(obj as DocumentFileInfo);
        }
    }
}
