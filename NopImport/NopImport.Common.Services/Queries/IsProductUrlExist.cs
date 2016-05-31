using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Conventions;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;
using NopImport.Model.Data;

namespace NopImport.Common.Services.Queries
{
    public class IsProductUrlExist : IGetItemQuery<bool>
    {
        private readonly string _url;

        public IsProductUrlExist(string url)
        {
            _url = url;
        }

        public bool Execute(ISession session)
        {
            var output = session.QueryOver<Product>().Where(x => x.Url == _url).RowCount() > 0;
            //session.Query<Product>().Any(x => x.Url == _url);
            return output;
        }
    }
}
