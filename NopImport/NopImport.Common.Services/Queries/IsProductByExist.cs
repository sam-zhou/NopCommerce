using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Conventions;
using NHibernate;
using NHibernate.Criterion;
using NopImport.Model.Data;

namespace NopImport.Common.Services.Queries
{
    public class IsProductByExist : IGetItemQuery<bool>
    {
        private readonly string _url;

        public IsProductByExist(string url)
        {
            _url = url;
        }

        public bool Execute(ISession session)
        {
            var output = session.CreateCriteria<Product>().Add(Restrictions.Eq("Url", _url)).IsAny();
            return output;
        }
    }
}
