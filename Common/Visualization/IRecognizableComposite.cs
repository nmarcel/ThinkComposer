using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Instrumind.Common.Visualization
{
    public interface IRecognizableComposite : IRecognizableElement
    {
        string NameCaption { get; }

        string DescriptiveCaption { get; }

        IRecognizableElement Definitor { get; }

        IEnumerable<IRecognizableComposite> CompositeMembers { get; }

        IRecognizableComposite CompositeParent { get; }

        int CompositeDepthLevel { get; }

        /// <summary>
        /// Gets the containment route, optionally including the Composition root node, of this Idea using the specified property.
        /// Only "Name", "TechName" and "GlobalId" are supported.
        /// </summary>
        string GetContainmentRoute(string PropertyName, bool IncludeRoot = false);

        /// <summary>
        /// Gets the containment nodes, optionally including the Composition root node, of this Idea.
        /// </summary>
        IList<IRecognizableComposite> GetContainmentNodes(bool IncludeRoot = false);
    }
}
