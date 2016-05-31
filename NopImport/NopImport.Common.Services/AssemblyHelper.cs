using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NopImport.Common.Services
{
    public static class AssemblyHelper
    {
        public static IEnumerable<Type> GetMapableEnumTypes(this Assembly assembly)
        {
            return assembly.GetTypes().Where(q => q.Namespace == assembly.GetName().Name + ".Enum.Mapable");
        }
    }
}
