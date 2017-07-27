using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Instrumind.Common
{
    [AttributeUsage(AttributeTargets.Field)]
    public class FieldNameAttribute : Attribute
    {
        public FieldNameAttribute(string Name)
        {
            this.Name = Name;
        }

        public string Name { get; set; }
    }
}
