using System.Threading.Tasks;

using DD.CBU.Compute.Api.Contracts.General;

namespace DD.CBU.Compute.Api.Client.Linq.Internal.QueryExecutors
{
    /// <summary>
    /// Abstraction layer for the query executor.
    /// </summary>
    /// <typeparam name="TElement">The element type.</typeparam>
    internal interface IQueryExecutor<TElement>
    {
        /// <summary>
        /// Executes a <see cref="ParsedRequest"/>.
        /// </summary>
        /// <param name="request">The parsed request to execute.</param>
        /// <returns>The result of the expression execution.</returns>
        object Execute(ParsedRequest request);

        /// <summary>
        /// Executes a <see cref="ParsedRequest"/> and returns a paged result.
        /// </summary>
        /// <param name="request">The parsed request to execute.</param>
        /// <returns>The result of the expression execution.</returns>
        Task<PagedResponse<TElement>> ToPagedResponse(ParsedRequest request);
    }
}