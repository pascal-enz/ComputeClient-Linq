using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Reflection;

namespace DD.CBU.Compute.Api.Client.Linq.Internal
{
    using Helpers;

    /// <summary>
    /// Parses an <see cref="Expression"/> into a <see cref="ParsedRequest"/>.
    /// </summary>
    internal class RequestParser<TElement> : ExpressionVisitor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequestParser"/> class.
        /// </summary>
        private RequestParser()
        {
            Result = new ParsedRequest();
        }

        /// <summary>
        /// Translates the supplied expression into a pageable request.
        /// </summary>
        /// <param name="expression">The expression to translate.</param>
        /// <returns>The pageable request.</returns>
        public static ParsedRequest Parse(Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            var visitor = new RequestParser<TElement>();
            visitor.Visit(expression);
            return visitor.Result;
        }

        /// <summary>
        /// Gets the parsed request.
        /// </summary>
        public ParsedRequest Result { get; private set; }

        /// <summary>
        /// Dispatches the expression to one of the more specialized visit methods in this class.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        public override Expression Visit(Expression node)
        {
            Result.Expression = base.Visit(node);
            return Result.Expression;
        }

        /// <summary>
        /// Visits the children of the <see cref="MethodCallExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            // The following methods are supported by Cloud Control API.
            // We extract the information from the expressions to build a Compute API query string later.
            if (!Result.ExecuteAsLambda && node.Method.DeclaringType == typeof(Queryable))
            {
                switch (node.Method.Name)
                {
                    case "Count":
                    case "LongCount":
                        Result.IsCount = true;
                        return Visit(node.Arguments[0]);

                    case "Where":
                        Result.Filters = WhereExpressionVisitor.ExtractFilters(node.Arguments[1]).Union(Result.Filters).ToList();
                        return Visit(node.Arguments[0]);

                    case "ThenBy":
                    case "OrderBy":
                        Result.OrderBy.Insert(0, new ParsedRequestOrderBy { Field = ExpressionHelper.ExtractFieldName(node.Arguments[1]), IsDescending = false });
                        return Visit(node.Arguments[0]);

                    case "ThenByDescending":
                    case "OrderByDescending":
                        Result.OrderBy.Insert(0, new ParsedRequestOrderBy { Field = ExpressionHelper.ExtractFieldName(node.Arguments[1]), IsDescending = true });
                        return Visit(node.Arguments[0]);

                    case "Take":
                        Result.Take = (int)ExpressionHelper.ExtractValue(node.Arguments[1]);
                        return Visit(node.Arguments[0]);

                    case "Skip":
                        Result.Skip = (int)ExpressionHelper.ExtractValue(node.Arguments[1]);
                        return Visit(node.Arguments[0]);

                    case "Single":
                    case "First":
                        Result.Take = 1;
                        Result.AllowGetRequest = true;
                        break;

                    case "SingleOrDefault":
                    case "FirstOrDefault":
                        Result.Take = 1;
                        Result.AllowGetRequest = false;
                        break;
                }
            }

            // We have found a method call which is not supported by Cloud Control, e.g. Single() or Select().
            // We now execute the Cloud Control query, replace the input of the current method call with the
            // API response and signal the query provider to invoke the remaining expression as Lambda in memory.
            List<Expression> arguments = node.Arguments.ToList();
            arguments[0] = ExecuteComputeApiQuery(node);

            Result.ExecuteAsLambda = true;

            return Expression.Call(node.Method, arguments);
        }

        /// <summary>
        /// Resolves the supplied method call expression. If an instance of ComputeApiQueryProvider can be found
        /// in the expression tree, the input expression tree will be sent to Cloud Control API.
        /// </summary>
        /// <param name="node">The method call expression.</param>
        /// <returns>The result as an expression.</returns>
        private Expression ExecuteComputeApiQuery(MethodCallExpression node)
        {
            Expression visitedNode = Visit(node.Arguments[0]);

            // Find the instance of ComputeApiQueryProvider in the expression tree. If it doesn't exist, the query
            // has already been executed and we just return the expression to execute it as lambda in memory.
            var queryProvider = ExpressionHelper.FindQueryProvider(visitedNode) as ComputeApiQueryProvider<TElement>;
            if (queryProvider == null)
            {
                return visitedNode;
            }

            // Send the input expression tree to Cloud Control via ComputeApiQueryExecutor.
            object queryResult = queryProvider.GetQueryExecutor(false).Execute(Result);

            // If this is a count query, we just return the resolved count as constant value.
            if (Result.IsCount)
            {
                return Expression.Constant(queryResult);
            }

            // Cast the result from object to IEnumerable<T>.
            Type queryableType = node.Arguments[0].Type;
            Type elementType = queryableType.GetGenericArguments().Single();
            Type enumerableType = typeof(IEnumerable<>).MakeGenericType(elementType);
            Expression castCall = Expression.Convert(Expression.Constant(queryResult), enumerableType);

            // Call the AsQueryable() method on the IEnumerable<T> input.
            MethodInfo asQueryableMethod = typeof(Queryable)
                .GetMethods()
                .Single(m => m.Name == "AsQueryable" && m.IsGenericMethod)
                .MakeGenericMethod(elementType);

            return Expression.Call(asQueryableMethod, castCall);
        }
    }
}
