using System;
using System.Collections.Generic;

namespace NopImport.Common.Services
{
    /// <summary>
    /// Filter used to search
    /// </summary>
    public abstract class Filter
    {
        /// <summary>
        /// Combines this filter with another using an AND relation.
        /// </summary>
        /// <remarks>
        /// This is a convenience function for creating an <see cref="AndFilter"/>.
        /// </remarks>
        /// <param name='filter'>
        /// The other filter in the AND relationship.
        /// </param>
        public AndFilter And(Filter filter)
        {
            if (filter == null)
                throw new ArgumentNullException("filter");

            return new AndFilter(this, filter);
        }

        /// <summary>
        /// Combines this filter with another using an OR relation.
        /// </summary>
        /// <remarks>
        /// This is a convenience function for creating an <see cref="AndFilter"/>.
        /// </remarks>
        /// <param name='filter'>
        /// The other filter in the OR relationship.
        /// </param>
        public OrFilter Or(Filter filter)
        {
            if (filter == null)
                throw new ArgumentNullException("filter");

            return new OrFilter(this, filter);
        }

        /// <summary>
        /// Negates this filter.
        /// </summary>
        /// <remarks>
        /// This is a convenience function for creating a <see cref="NotFilter"/>.
        /// </remarks>
        public NotFilter Not()
        {
            return new NotFilter(this);
        }
    }


    public abstract class ContainerableFilter : Filter
    {
        /// <summary>
        /// The list of filters that must all be true for this filter to be true.
        /// </summary>
        public List<Filter> Filters { get; set; }


        protected ContainerableFilter(params Filter[] filters)
        {
            if (filters == null)
                throw new ArgumentNullException("filters");

            Filters = new List<Filter>(filters);
        }
    }

    /// <summary>
    /// Filter that enforces an AND relation on all its <see cref="ContainerableFilter.Filters"/>.
    /// </summary>
    public class AndFilter : ContainerableFilter
    {
        public AndFilter(params Filter[] filters)
            : base(filters)
        {

        }
    }

    /// <summary>
    /// Filter that enforces an AND relation on all its <see cref="ContainerableFilter.Filters"/>.
    /// </summary>
    public class OrFilter : ContainerableFilter
    {
        public OrFilter(params Filter[] filters)
            : base(filters)
        {

        }
    }

    public class NotFilter : Filter
    {
        public Filter InnerFilter { get; set; }

        public NotFilter(Filter innerFilter)
        {
            if (innerFilter == null)
                throw new ArgumentNullException("innerFilter");

            InnerFilter = innerFilter;
        }
    }

    public class EqualsFilter : Filter
    {
        public string PropertyName { get; set; }
        public object Value { get; set; }

        public EqualsFilter()
        {
            PropertyName = "";
            Value = "";
        }

        public EqualsFilter(string propertyName, object value)
        {
            PropertyName = propertyName;
            Value = value;
        }
    }

    public class ContainsFilter : Filter
    {
        public string PropertyName { get; set; }
        public string Value { get; set; }

        public ContainsFilter()
        {
            PropertyName = "";
            Value = "";
        }

        public ContainsFilter(string propertyName, string value)
        {
            PropertyName = propertyName;
            Value = value;
        }
    }
}
