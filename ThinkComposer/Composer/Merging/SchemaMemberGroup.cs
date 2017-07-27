using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.Visualization;

namespace Instrumind.ThinkComposer.Composer.Merging
{
    public class SchemaMemberGroup : SimplePresentationElement
    {
        public SchemaMemberGroup(string Name, ImageSource Pictogram = null, Func<IEnumerable<object>> ChildrenGetter = null,
                                 string TechName = null, string Summary = "")
             : base(Name, TechName, Summary, Pictogram)
        {
            this.ChildrenGetter = ChildrenGetter;
        }

        public Func<IEnumerable<object>> ChildrenGetter { get; private set; }

        public bool HasChildren { get { return (this.ChildrenGetter != null && this.ChildrenGetter().Any()); }}
    }
}
