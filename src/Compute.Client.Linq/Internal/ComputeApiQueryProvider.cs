using System;
using System.Linq;
using System.Linq.Expressions;

namespace DD.CBU.Compute.Api.Client.Linq.Internal
{
    using QueryExecutors;

    /// <summary>
    /// A custom implementation of the <see cref="IQueryProvider"/> contract.
    /// </summary>
    /// <typeparam name="TElement">The element type.</typeparam>
    internal sealed class ComputeApiQueryProvider<TElement> : IQueryProvider
    {
        /// <summary>
        /// The compute API Query Executor.
        /// </summary>
        private readonly IQueryExecutor<TElement> _queryExecutor;

        /// <summary>
        /// Initializes a new instance of the <see cref="LinqQueryProvider"/> class.
        /// </summary>
        /// <param name="queryExecutor">The Compute API Query Executor.</param>
        public ComputeApiQueryProvider(IQueryExecutor<TElement> queryExecutor)
        {
            if (queryExecutor == null)
                throw new ArgumentNullException(nameof(queryExecutor));

            _queryExecutor = queryExecutor;
        }

        /// <summary>
        /// Creates a new <see cref="IQueryable"/> instance for a given <see cref="Expression"/>.
        /// </summary>
        /// <typeparam name="T">The type of the <see cref="IQueryable"/>.</typeparam>
        /// <param name="expression">The <see cref="Expression"/> to create a query for.</param>
        /// <returns>A new IQueryable instance for the provided type.</returns>
        public IQueryable<T> CreateQuery<T>(Expression expression)
        {
            return new ComputeApiQuery<T>(this, expression);
        }

        /// <summary>
        /// Creates a new <see cref="IQueryable"/> instance for a given <see cref="Expression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="Expression"/> to create a query for.</param>
        /// <returns>A new IQueryable instance.</returns>
        public IQueryable CreateQuery(Expression expression)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Executes an <see cref="Expression"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of the element.</typeparam>
        /// <param name="expression">The <see cref="Expression"/> to execute.</param>
        /// <returns>The result of the expression execution.</returns>
        public TResult Execute<TResult>(Expression expression)
        {
            return (TResult)Execute(expression);
        }

        /// <summary>
        /// Executes an <see cref="Expression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="Expression"/> to execute.</param>
        /// <returns>The result of the expression execution.</returns>
        public object Execute(Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            var request = QueryExpressionVisitor<TElement>.Parse(expression);
            return GetQueryExecutor(request.ExecuteAsLambda).Execute(request);
        }

        /// <summary>
        /// Gets the query executor instance.
        /// </summary>
        /// <param name="isLambdaExpression">A value indicating whether a lambda expression should be executed.</param>
        /// <returns>The query executor instance to use.</returns>
        public IQueryExecutor<TElement> GetQueryExecutor(bool isLambdaExpression)
        {
            if (isLambdaExpression)
            {
                return new LambdaQueryExecutor<TElement>();
            }

            return _queryExecutor;
        }
    }
}