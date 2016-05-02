using System;
using System.Linq;
using System.Threading.Tasks;

using DD.CBU.Compute.Api.Contracts.General;

namespace DD.CBU.Compute.Api.Client.Linq
{
    using Internal;

    /// <summary>
    /// Provides useful extension methods for <see cref="IQueryable"/> instances.
    /// </summary>
    public static class QueryableExtensions
    {
        /// <summary>
        /// Executes a queryable and returns the result as a Compute API paged response.
        /// </summary>
        /// <typeparam name="TResult">The element type.</typeparam>
        /// <param name="source">The source queryable.</param>
        /// <returns>The paged response.</returns>
        public static PagedResponse<TResult> ToPagedResponse<TResult>(this IQueryable<TResult> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return Task.Run(() => source.ToPagedResponseAsync()).Result;
        }

        /// <summary>
        /// Executes a queryable and returns the result as a Compute API paged response as an async task.
        /// </summary>
        /// <typeparam name="TResult">The element type.</typeparam>
        /// <param name="source">The source queryable.</param>
        /// <returns>The paged response as an async <see cref="Task"/>.</returns>
        public static async Task<PagedResponse<TResult>> ToPagedResponseAsync<TResult>(this IQueryable<TResult> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var query = source as ComputeApiQuery<TResult>;
            if (query == null)
                throw new InvalidOperationException("Method 'ToPagedResponse' can only be used for IQueryable instances of Compute API objects.");

            var request = RequestParser<TResult>.Parse(query.Expression);
            if (request.ExecuteAsLambda)
                throw new InvalidOperationException("Method 'ToPagedResponse' cannot be used after Enumerable extension methods like Single, Select or ToList.");

            return await (query.Provider as ComputeApiQueryProvider<TResult>)
                .GetQueryExecutor(false)
                .ToPagedResponse(request);
        }
    }
}
