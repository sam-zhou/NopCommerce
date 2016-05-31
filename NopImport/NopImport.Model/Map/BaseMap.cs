using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using NopImport.Model.Data;

namespace NopImport.Model.Map
{
    public abstract class BaseMap<TEntity> : ClassMap<TEntity> where TEntity : class, IBaseEntity
    {
        protected BaseMap()
        {
            Id(q => q.Id).GeneratedBy.Identity();
        }
    }
}
