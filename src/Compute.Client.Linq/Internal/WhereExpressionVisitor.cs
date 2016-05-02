using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using DD.CBU.Compute.Api.Contracts.Requests;

namespace DD.CBU.Compute.Api.Client.Linq.Internal
{
    using Helpers;

    /// <summary>
    /// Extracts the filters from the where clauses.
    /// </summary>
    internal sealed class WhereExpressionVisitor : ExpressionVisitor
    {
        /// <summary>
        /// The extracted filters.
        /// </summary>
        private readonly List<ParsedRequestFilter> _filters = new List<ParsedRequestFilter>();

        /// <summary>
        /// The parent property path.
        /// </summary>
        private readonly string _propertyPath;

        /// <summary>
        /// A value indicating whether a logical OR-operator is used.
        /// </summary>
        private bool _isOrElse = false;

        /// <summary>
        /// A value indicating whether a logical AND-operator is used.
        /// </summary>
        private bool _isAndAlso = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="WhereExpressionVisitor"/> class.
        /// </summary>
        /// <param name="propertyPath"></param>
        private WhereExpressionVisitor(string propertyPath)
        {
            _propertyPath = propertyPath ?? string.Empty;
        }

        /// <summary>
        /// Extracts and vaidates the filters from a where lambda expression tree.
        /// </summary>
        /// <param name="expression">The where lambda expression tree.</param>
        /// <param name="propertyPath">The property path.</param>
        /// <returns>The extracted filters.</returns>
        public static IEnumerable<ParsedRequestFilter> ExtractFilters(Expression expression, string propertyPath = null)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            var visitor = new WhereExpressionVisitor(propertyPath);
            visitor.Visit(expression);

            if (visitor._isOrElse && visitor._filters.Select(filter => filter.Field).Distinct().Count() > 1)
            {
                throw new NotSupportedException("Cloud Control API does not support logical OR operator for different fields.");
            }

            if (visitor._isAndAlso && visitor._filters.Select(filter => filter.Field).Distinct().Count() == 1)
            {
                throw new NotSupportedException("Cloud Control API does not support logical AND operator for the same field.");
            }

            return visitor._filters;
        }

        /// <summary>
        /// Visits the children of the System.Linq.Expressions.BinaryExpression.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node.NodeType == ExpressionType.OrElse)
            {
                _isOrElse = true;
                return base.VisitBinary(node);
            }

            if (node.NodeType == ExpressionType.AndAlso)
            {
                _isAndAlso = true;
                return base.VisitBinary(node);
            }

            FilterOperator @operator;
            object value = ExpressionHelper.ExtractValue(node.Right);

            if (value == null)
            {
                switch (node.NodeType)
                {
                    case ExpressionType.Equal:
                        @operator = FilterOperator.Null;
                        break;
                    case ExpressionType.NotEqual:
                        @operator = FilterOperator.NotNull;
                        break;
                    default:
                        throw new NotSupportedException($"Unsupported operator type '{node.NodeType}' for null-value. ");
                }
            }
            else
            {
                switch (node.NodeType)
                {
                    case ExpressionType.Equal:
                        @operator = FilterOperator.Equals;
                        break;
                    case ExpressionType.GreaterThan:
                        @operator = FilterOperator.GreaterThan;
                        break;
                    case ExpressionType.GreaterThanOrEqual:
                        @operator = FilterOperator.GreaterOrEqual;
                        break;
                    case ExpressionType.LessThan:
                        @operator = FilterOperator.LessThan;
                        break;
                    case ExpressionType.LessThanOrEqual:
                        @operator = FilterOperator.LessOrEqual;
                        break;
                    default:
                        throw new NotSupportedException($"Unsupported operator type '{node.NodeType}'. ");
                }
            }

            _filters.Add(new ParsedRequestFilter
            {
                Operator = @operator,
                Field = _propertyPath + ExpressionHelper.ExtractFieldName(node.Left),
                Value = value
            });

            return node;
        }

        /// <summary>
        /// Visits the children of the System.Linq.Expressions.MethodCallExpression.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(Enumerable))
            {
                switch (node.Method.Name)
                {
                    case "Any":
                        var propertyPath = _propertyPath + ExpressionHelper.ExtractFieldName(node.Arguments[0]);
                        _filters.AddRange(WhereExpressionVisitor.ExtractFilters(node.Arguments[1], propertyPath + "."));
                        break;
                    default:
                        throw new NotSupportedException($"Unsupported enumerable method '{node.Method.Name}'.");
                }

                return node;
            }

            if (node.Method.DeclaringType == typeof(string))
            {
                string pattern;

                switch (node.Method.Name)
                {
                    case "StartsWith":
                        pattern = "{0}*";
                        break;
                    case "EndsWith":
                        pattern = "*{0}";
                        break;
                    case "Contains":
                        pattern = "*{0}*";
                        break;
                    default:
                        throw new NotSupportedException($"Unsupported string method '{node.Method.Name}'.");
                }

                _filters.Add(new ParsedRequestFilter
                {
                    Operator = FilterOperator.Like,
                    Field = _propertyPath + ExpressionHelper.ExtractFieldName(node.Object),
                    Value = string.Format(pattern, ExpressionHelper.ExtractValue(node.Arguments[0]))
                });

                return node;
            }

            throw new NotSupportedException($"Unsupported method '{node.Method.Name}'.");
        }
    }
}
