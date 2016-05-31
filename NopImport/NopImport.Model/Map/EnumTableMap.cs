using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using NopImport.Model.Data;

namespace NopImport.Model.Map
{
    public class EnumTableMap<T> : ClassMap<EnumTable<T>> where T : struct , IConvertible
    {
        protected EnumTableMap()
        {
            Table(typeof(T).Name);
            Id(q => q.Id).GeneratedBy.Assigned();
            Map(q => q.Name).Length(20).Not.Nullable();
        }
    }
}
