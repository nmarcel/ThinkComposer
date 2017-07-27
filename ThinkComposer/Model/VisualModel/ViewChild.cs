using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

using Instrumind.Common;

namespace Instrumind.ThinkComposer.Model.VisualModel
{
    /// <summary>
    /// Represents a presentation element of a view.
    /// To force update of the container collection via new object, it can only be created.
    /// </summary>
    [Serializable]
    public class ViewChild
    {
        public static ViewChild Create(object key, ContainerVisual content)
        {
            return (new ViewChild { Key_ = key, Content_ = content });
        }

        public static ViewChild Create(ViewChild source)
        {
            return (new ViewChild { Key_ = source.Key_, Content_ = source.Content_ });
        }

        public object Key { get { return this.Key_; } }
        private object Key_;

        public ContainerVisual Content { get { return this.Content_; } }
        [NonSerialized]
        private ContainerVisual Content_;

        public override string ToString()
        {
            return "Key=[" + Key.ToStringAlways() + "], Content=[HC=" + Content.Get(cnt => cnt.GetHashCode().ToString()) + "]";
        }
    }

    // -------------------------------------------------------------------------------------------
}
