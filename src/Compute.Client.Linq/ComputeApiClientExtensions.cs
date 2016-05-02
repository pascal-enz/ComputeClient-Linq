using System;
using DD.CBU.Compute.Api.Client.Interfaces;

namespace DD.CBU.Compute.Api.Client.Linq
{
    /// <summary>
    /// Provides extension methods for <see cref="IComputeApiClient"/> instances.
    /// </summary>
    public static class ComputeApiClientExtensions
    {
        /// <summary>
        /// Returns queryable accesssors for the supplied API client.
        /// </summary>
        /// <param name="client">The API client instance.</param>
        /// <returns>The queruable accesor.</returns>
        public static Queryables AsQueryable(this IComputeApiClient client)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            return new Queryables(client);
        }
    }
}
