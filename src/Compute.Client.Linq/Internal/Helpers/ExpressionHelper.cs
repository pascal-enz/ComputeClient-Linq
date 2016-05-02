using System;
using System.Linq;
using System.Linq.Expressions;

namespace DD.CBU.Compute.Api.Client.Linq.Internal.Helpers
{
    /// <summary>
    /// Provides commonly used helper methods to work with expression trees.
    /// </summary>
    internal static class ExpressionHelper
    {
        /// <summary>
        /// Extracts the static value from an expression.
        /// </summary>
        /// <param name="node">The node to extract the value from.</param>
        /// <returns>The extracted value.</returns>
        public static object ExtractValue(Expression node)
        {
            var constantExpression = node as ConstantExpression;
            if (constantExpression != null)
            {
                return constantExpression.Value;
            }

            return Expression.Lambda(node).Compile().DynamicInvoke();
        }

        /// <summary>
        /// Extractes the field name from the supplied node.
        /// </summary>
        /// <param name="node">The node to extract the field name from.</param>
        /// <returns>The field name.</returns>
        public static string ExtractFieldName(Expression node)
        {
            var visitor = new ExpressionFinder(e => e.NodeType == ExpressionType.MemberAccess);
            visitor.Visit(node);

            if (visitor.Results.Count > 0)
            {
                return string.Join(".", visitor.Results.Reverse().Select(e => (e as MemberExpression).Member.Name));
            }

            throw new InvalidOperationException("Propety access expression expected.");
        }

        /// <summary>
        /// Tries to find the query provider in an expression tree.
        /// </summary>
        /// <param name="node">The node to search in.</param>
        /// <returns>The source query provider or null.</returns>
        public static IQueryProvider FindQueryProvider(Expression node)
        {
            var visitor = new ExpressionFinder(e => e.NodeType == ExpressionType.Constant && e.Type.IsGenericType && e.Type.GetGenericTypeDefinition() == typeof(ComputeApiQuery<>));
            visitor.Visit(node);

            if (visitor.Results.Count == 0)
            {
                return null;
            }

            var query = (IQueryable)((ConstantExpression)visitor.Results.First()).Value;
            return query.Provider;
        }
    }
}
