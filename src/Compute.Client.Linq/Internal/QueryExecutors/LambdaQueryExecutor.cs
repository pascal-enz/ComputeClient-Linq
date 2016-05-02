using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

using DD.CBU.Compute.Api.Contracts.General;

namespace DD.CBU.Compute.Api.Client.Linq.Internal.QueryExecutors
{
    /// <summary>
    /// Executes an expression tree as a compiled Lambda statement.
    /// </summary>
    /// <typeparam name="TElement">The element type.</typeparam>
    internal sealed class LambdaQueryExecutor<TElement> : IQueryExecutor<TElement>
    {
        /// <summary>
        /// Executes a <see cref="ParsedRequest"/>.
        /// </summary>
        /// <param name="request">The parsed request to execute.</param>
        /// <returns>The result of the expression execution.</returns>
        public object Execute(ParsedRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var lambda = Expression.Lambda(request.Expression);
            return lambda.Compile().DynamicInvoke();
        }

        /// <summary>
        /// Executes a <see cref="ParsedRequest"/> and returns a paged result.
        /// </summary>
        /// <param name="request">The parsed request to execute.</param>
        /// <returns>The result of the expression execution.</returns>
        public Task<PagedResponse<TElement>> ToPagedResponse(ParsedRequest request)
        {
            throw new InvalidOperationException("Method 'ToPagedResponse' cannot be used after Enumerable extension methods like Single, Select or ToList.");
        }
    }
}
