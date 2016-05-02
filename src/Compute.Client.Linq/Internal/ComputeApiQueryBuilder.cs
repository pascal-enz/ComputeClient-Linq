using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DD.CBU.Compute.Api.Contracts.General;
using DD.CBU.Compute.Api.Contracts.Requests;

namespace DD.CBU.Compute.Api.Client.Linq.Internal
{
    using QueryExecutors;

    /// <summary>
    /// A helper class to build Linq Query instances.
    /// </summary>
    /// <typeparam name="TFilter">The Compute API Filter pbject type.</typeparam>
    /// <typeparam name="TElement">The element type.</typeparam>
    /// <typeparam name="TId">The element identifier type.</typeparam>
    internal sealed class ComputeApiQueryBuilder<TFilter, TElement, TId> where TFilter : FilterableRequest
    {
        /// <summary>
        /// The callback method to get a single result.
        /// </summary>
        private Func<TId, Task<TElement>> _getCallback;

        /// <summary>
        /// The callback method to get a query result.
        /// </summary>
        private Func<TFilter, PageableRequest, Task<PagedResponse<TElement>>> _listCallback;

        /// <summary>
        /// The parameter mapping table.
        /// </summary>
        private IDictionary<string, string> _parameterMap = new Dictionary<string, string>();

        /// <summary>
        /// The required filters.
        /// </summary>
        private IDictionary<string, object> _filters = new Dictionary<string, object>();

        /// <summary>
        /// Sets the callback method to get a single result.
        /// </summary>
        /// <param name="callback">The callback method.</param>
        /// <returns>The Linq Query builder instance.</returns>
        public ComputeApiQueryBuilder<TFilter, TElement, TId> OnGet(Func<TId, Task<TElement>> callback)
        {
            _getCallback = callback;
            return this;
        }

        /// <summary>
        /// Sets the callback method to get a query result.
        /// </summary>
        /// <param name="callback">The callback method.</param>
        /// <returns>The Linq Query builder instance.</returns>
        public ComputeApiQueryBuilder<TFilter, TElement, TId> OnQuery(Func<TFilter, PageableRequest, Task<PagedResponse<TElement>>> callback)
        {
            _listCallback = callback;
            return this;
        }

        /// <summary>
        /// Sets a custom property to parameter name map.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <returns>The Linq Query builder instance.</returns>
        public ComputeApiQueryBuilder<TFilter, TElement, TId> MapParameter(string propertyName, string parameterName)
        {
            _parameterMap.Add(propertyName, parameterName);
            return this;
        }

        /// <summary>
        /// Sets a custom property to parameter name map.
        /// </summary>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="value">The filter value.</param>
        /// <returns>The Linq Query builder instance.</returns>
        public ComputeApiQueryBuilder<TFilter, TElement, TId> Filter(string parameterName, object value)
        {
            _filters.Add(parameterName, value);
            return this;
        }

        /// <summary>
        /// Builds the Linq Query instance.
        /// </summary>
        /// <returns>The Linq Query instance.</returns>
        public IQueryable<TElement> Build()
        {
            var executor = new ComputeApiQueryExecutor<TFilter, TElement, TId>(_getCallback, _listCallback, _parameterMap, _filters);
            var provider = new ComputeApiQueryProvider<TElement>(executor);
            return provider.CreateQuery<TElement>(null);
        }
    }
}
