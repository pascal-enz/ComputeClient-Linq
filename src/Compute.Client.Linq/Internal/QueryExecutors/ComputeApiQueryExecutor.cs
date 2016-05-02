using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DD.CBU.Compute.Api.Contracts.General;
using DD.CBU.Compute.Api.Contracts.Requests;

namespace DD.CBU.Compute.Api.Client.Linq.Internal.QueryExecutors
{
    /// <summary>
    /// Executes the parsed expression tree by sending it to the Compute API.
    /// </summary>
    /// <typeparam name="TFilter">The Compute API Filter pbject type.</typeparam>
    /// <typeparam name="TElement">The element type.</typeparam>
    /// <typeparam name="TId">The element identifier type.</typeparam>
    internal sealed class ComputeApiQueryExecutor<TFilter, TElement, TId> : IQueryExecutor<TElement>
        where TFilter : FilterableRequest
    {
        /// <summary>
        /// The callback function to get a query result.
        /// </summary>
        private readonly Func<TFilter, PageableRequest, Task<PagedResponse<TElement>>> _listCallback;

        /// <summary>
        /// The callback function to get a single result.
        /// </summary>
        private readonly Func<TId, Task<TElement>> _getCallback;

        /// <summary>
        /// The parameter mapping table.
        /// </summary>
        private readonly IDictionary<string, string> _parameterMap;

        /// <summary>
        /// The required filters.
        /// </summary>
        private readonly IDictionary<string, object> _filters;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComputeApiQueryExecutor"/> class.
        /// </summary>
        /// <param name="getCallback">The callback function to get a single result.</param>
        /// <param name="listCallback">The callback function to get a query result.</param>
        /// <param name="parameterMap">The parameter mapping table.</param>
        /// <param name="filters">The required filters.</param>
        public ComputeApiQueryExecutor(
            Func<TId, Task<TElement>> getCallback,
            Func<TFilter, PageableRequest, Task<PagedResponse<TElement>>> listCallback,
            IDictionary<string, string> parameterMap,
            IDictionary<string, object> filters)
        {
            if (listCallback == null)
                throw new ArgumentNullException(nameof(listCallback));

            if (parameterMap == null)
                throw new ArgumentNullException(nameof(parameterMap));

            if (filters == null)
                throw new ArgumentNullException(nameof(filters));

            _getCallback = getCallback;
            _listCallback = listCallback;
            _parameterMap = parameterMap;
            _filters = filters;
        }

        /// <summary>
        /// Executes a <see cref="ParsedRequest"/>.
        /// </summary>
        /// <param name="request">The parsed request to execute.</param>
        /// <returns>The result of the expression execution.</returns>
        public object Execute(ParsedRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.ExecuteAsLambda)
                throw new InvalidOperationException("Invalid combination of remote and in-memory operations.");

            TId id;
            if (IsSingleIdRequest(request, out id))
            {
                var item = Task.Run(() => _getCallback(id)).Result;
                return new TElement[] { item };
            }

            var task = _listCallback(GetFilterableRequest(request), GetPageableRequest(request));
            var result = Task.Run(() => task).Result;

            return request.IsCount
                ? (object)result.totalCount
                : (object)result.items ?? Enumerable.Empty<TElement>();
        }

        /// <summary>
        /// Executes a <see cref="ParsedRequest"/> and returns a paged result.
        /// </summary>
        /// <param name="request">The parsed request to execute.</param>
        /// <returns>The result of the expression execution.</returns>
        public async Task<PagedResponse<TElement>> ToPagedResponse(ParsedRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.ExecuteAsLambda)
                throw new InvalidOperationException("Invalid combination of remote and in-memory operations.");

            return await _listCallback(GetFilterableRequest(request), GetPageableRequest(request));
        }

        /// <summary>
        /// Checks if a single item by id is requested.
        /// </summary>
        /// <typeparam name="TValue">The return value type.</typeparam>
        /// <param name="request">The parsed request to execute.</param>
        /// <param name="value">The id value.</param>
        /// <returns>True if a single id equals filter has been specified; otherwise false.</returns>
        private bool IsSingleIdRequest<TValue>(ParsedRequest request, out TValue value)
        {
            var isSingleRequest =
                (_getCallback != null) &&
                (request.AllowGetRequest == true) &&
                (request.Take == null || request.Take == 1) &&
                (request.Skip == 0) &&
                (request.Filters.Count == 1) &&
                (request.Filters[0].Field == "id") &&
                (request.Filters[0].Operator == FilterOperator.Equals);

            if (isSingleRequest)
            {
                object fieldValue = request.Filters[0].Value;
                if (typeof(TValue) == fieldValue.GetType())
                {
                    value = (TValue)fieldValue;
                }
                else if (typeof(TValue) == typeof(Guid))
                {
                    value = (TValue)(object)new Guid(fieldValue.ToString());
                }
                else if (typeof(TValue) == typeof(string))
                {
                    value = (TValue)(object)fieldValue.ToString();
                }
                else
                {
                    value = (TValue)Convert.ChangeType(fieldValue, typeof(TValue));
                }
            }
            else
            {
                value = default(TValue);
            }

            return isSingleRequest;
        }

        /// <summary>
        /// Gets the pageable request.
        /// </summary>
        /// <param name="request">The parsed request to execute.</param>
        /// <returns>The <see cref="PageableRequest"/> instance for the supplied request.</returns>
        private PageableRequest GetPageableRequest(ParsedRequest request)
        {
            if (request.IsCount)
            {
                if (request.Skip > 0 || request.Take.HasValue || request.OrderBy.Count > 0)
                    throw new InvalidOperationException("Skip(), Take() and ordering methods cannot be used in a count query.");

                return new PageableRequest
                {
                    PageSize = 1,
                    PageNumber = 1000000
                };
            }

            if ((request.Skip == 0) && (request.Take == null) && (request.OrderBy.Count == 0))
                return null;

            if (request.Skip < 0)
                throw new InvalidOperationException("Skip() cannot be negatve.");

            if (request.Take <= 0)
                throw new InvalidOperationException("Take() cannot be zero or negatve.");

            if ((request.Skip > 0) && (request.Take == null))
                request.Take = request.Skip;

            if ((request.Take != null) && (request.Skip % request.Take != 0))
                throw new InvalidOperationException("Skip() must be zero or a a multiple of Take().");

            string orderBys = null;
            if (request.OrderBy.Count > 0)
            {
                orderBys = request.OrderBy
                    .Select(orderBy => new { Field = MapParameterName(orderBy.Field), orderBy.IsDescending })
                    .Select(orderBy => orderBy.IsDescending ? orderBy.Field + ".DESCENDING" : orderBy.Field)
                    .Aggregate((current, next) => current + "," + next);
            }

            return new PageableRequest
            {
                PageSize = request.Take,
                PageNumber = request.Take.HasValue ? (request.Skip / request.Take) + 1 : null,
                Order = orderBys
            };
        }

        /// <summary>
        /// Gets the filterable request for the Compute API.
        /// </summary>
        /// <param name="request">The parsed request to execute.</param>
        /// <returns>The <see cref="FilterableRequest{TFilter}"/> instance for the supplied request.</returns>
        private TFilter GetFilterableRequest(ParsedRequest request)
        {
            var filterableRequest = Activator.CreateInstance<TFilter>();
            var filters = new List<Filter>();

            filters.AddRange(_filters.Select(filter => new Filter
            {
                Field = filter.Key,
                Operator = FilterOperator.Equals,
                Value = filter.Value
            }));

            filters.AddRange(request.Filters.Select(filter => new Filter
            {
                Field = MapParameterName(filter.Field),
                Operator = filter.Operator,
                Value = filter.Value
            }));

            filterableRequest.Filters = filters;
            return filterableRequest;
        }

        /// <summary>
        /// Maps a parameter name.
        /// </summary>
        /// <param name="name">The property name from the lambda expression.</param>
        /// <returns>The API parameter name.</returns>
        private string MapParameterName(string name)
        {
            if (_parameterMap != null)
            {
                string mappedName;

                if (_parameterMap.TryGetValue(name, out mappedName))
                {
                    return mappedName;
                }
            }

            return name;
        }
    }
}
