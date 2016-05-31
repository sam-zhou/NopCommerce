using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using NHibernate.Criterion;

namespace NopImport.Common.Services
{
    public class GetItems<T> : IGetItemsQuery<T> where T : class
    {
        private readonly ICriterion _criterion;

        public GetItems(params Filter[] filters)
        {
            if (filters == null)
                throw new ArgumentNullException("filters");

            _criterion = GetCriterions(new List<Filter>(filters));
        }

        public IEnumerable<T> Execute(ISession session)
        {
            var criteria = session.CreateCriteria<T>();

            if (_criterion != null)
            {
                criteria.Add(_criterion);
            }


            return criteria.List<T>();
        }

        private ICriterion GetCriterions(List<Filter> filters)
        {
            var andFilter = new AndFilter(filters.ToArray());
            return GetCriterion(andFilter);
        }

        private ICriterion GetCriterion(Filter filter)
        {
            if (filter == null)
            {
                return null;
            }

            if (filter is ContainerableFilter)
            {
                return GetContainerableCriterion((ContainerableFilter)filter);
            }


            if (filter is NotFilter)
            {
                var f = (NotFilter)filter;
                return Restrictions.Not(GetCriterion(f.InnerFilter));
            }

            if (filter is EqualsFilter)
            {
                var f = (EqualsFilter)filter;
                return Restrictions.Eq(f.PropertyName, f.Value);
            }

            if (filter is ContainsFilter)
            {
                var f = (ContainsFilter)filter;
                return Restrictions.Like(f.PropertyName, f.Value, MatchMode.Anywhere);
            }

            throw new NotSupportedException("Unsupported filter type: " + filter.GetType());
        }

        private ICriterion GetContainerableCriterion(ContainerableFilter filter)
        {
            var count = filter.Filters.Count;

            if (count == 0)
            {
                return null;
            }

            if (count == 1)
            {
                return GetCriterion(filter.Filters.FirstOrDefault());
            }

            var firstFilter = filter.Filters[0];
            filter.Filters.RemoveAt(0);
            var lhs = GetCriterion(firstFilter);
            var rhs = GetCriterion(filter);


            if (lhs != null)
            {
                if (rhs != null)
                {
                    if (filter is OrFilter)
                    {
                        return Restrictions.Or(lhs, rhs);
                    }
                    return Restrictions.And(lhs, rhs);
                }
                else
                {
                    return lhs;
                }
            }
            else
            {
                return rhs;
            }
        }

    }
}
