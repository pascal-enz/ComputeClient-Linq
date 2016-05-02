using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DD.CBU.Compute.Api.Client.Linq.Internal.Helpers
{
    /// <summary>
    /// A helper class to find expressions in a tree.
    /// </summary>
    internal sealed class ExpressionFinder : ExpressionVisitor
    {
        /// <summary>
        /// The check calllback method.
        /// </summary>
        private readonly Func<Expression, bool> _check;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionFinder"/> class.
        /// </summary>
        /// <param name="check"></param>
        public ExpressionFinder(Func<Expression, bool> check)
        {
            if (check == null)
                throw new ArgumentNullException(nameof(check));

            _check = check;
            Results = new List<Expression>();
        }

        /// <summary>
        /// The found expressions.
        /// </summary>
        public IList<Expression> Results { get; private set; }

        /// <summary>
        /// Dispatches the expression to one of the more specialized visit methods in this class.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        public override Expression Visit(Expression node)
        {
            if (node != null && _check(node))
            {
                Results.Add(node);
            }

            return base.Visit(node);
        }
    }
}
