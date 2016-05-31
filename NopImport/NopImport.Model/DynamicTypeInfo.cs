using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NopImport.Model
{
    public class DynamicTypeInfo
    {
        public DynamicTypeInfo(string name, Type parent)
        {
            Name = name;
            Parent = parent;
        }

        public string Name { get; set; }

        public Type Parent { get; set; }
    }
}
