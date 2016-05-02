using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DD.CBU.Compute.Api.Client.Linq.Internal
{
    /// <summary>
    /// A custom implementation of the <see cref="IOrderedQueryable"/> contract.
    /// </summary>
    /// <typeparam name="TElement">The element type.</typeparam>
    internal sealed class ComputeApiQuery<TElement> : IOrderedQueryable<TElement>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LinqQuery"/> class.
        /// </summary>
        /// <param name="provider">The <see cref="IQueryProvider"/> instance.</param>
        /// <param name="expression">The <see cref="Expression"/> to execute.</param>
        public ComputeApiQuery(IQueryProvider provider, Expression expression)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            Provider = provider;
            Expression = expression ?? Expression.Constant(this);
        }

        /// <summary>
        /// Gets the element type of the current ODataQuery.
        /// </summary>
        public Type ElementType
        {
            get { return typeof(TElement); }
        }

        /// <summary>
        /// Gets the current <see cref="Expression"/>.
        /// </summary>
        public Expression Expression
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the current <see cref="IQueryProvider"/> instance.
        /// </summary>
        public IQueryProvider Provider
        {
            get;
            private set;
        }

        /// <summary>
        /// Executes the provided query expression.
        /// </summary>
        /// <returns>The result of the query expression execution.</returns>
        public IEnumerator<TElement> GetEnumerator()
        {
            return ((IEnumerable<TElement>)Provider.Execute(Expression)).GetEnumerator();
        }

        /// <summary>
        /// Executes the provided query expression.
        /// </summary>
        /// <returns>The result of the query expression execution.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Provider.Execute(Expression)).GetEnumerator();
        }
    }
}