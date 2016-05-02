using System.Linq.Expressions;
using System.Collections.Generic;

using DD.CBU.Compute.Api.Contracts.Requests;

namespace DD.CBU.Compute.Api.Client.Linq.Internal
{
    /// <summary>
    /// Represents a single parsed filter expression.
    /// </summary>
    internal class ParsedRequestFilter
    {
        /// <summary>
        /// Gets or sets the field name.
        /// </summary>
        public string Field { get; set; }

        /// <summary>
        /// Gets or sets the operator.
        /// </summary>
        public FilterOperator Operator { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public object Value { get; set; }
    }

    /// <summary>
    /// Represents a single parsed order-by expression.
    /// </summary>
    internal class ParsedRequestOrderBy
    {
        /// <summary>
        /// Gets or sets the field name.
        /// </summary>
        public string Field { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether descending order should be applied.
        /// </summary>
        public bool IsDescending { get; set; }
    }

    /// <summary>
    /// The result of a request parsing operation.
    /// </summary>
    internal class ParsedRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParsedRequest"/> class.
        /// </summary>
        public ParsedRequest()
        {
            Skip = 0;
            Take = null;
            OrderBy = new List<ParsedRequestOrderBy>();
            Filters = new List<ParsedRequestFilter>();
            IsCount = false;
            AllowGetRequest = true;
            ExecuteAsLambda = false;
        }

        /// <summary>
        /// Gets or sets the skip segments.
        /// </summary>
        public int Skip { get; set; }

        /// <summary>
        /// Gets or sets the take segments.
        /// </summary>
        public int? Take { get; set; }

        /// <summary>
        /// Gets the orderBy segments.
        /// </summary>
        public List<ParsedRequestOrderBy> OrderBy { get; set; }

        /// <summary>
        /// Gets the filter segments.
        /// </summary>
        public List<ParsedRequestFilter> Filters { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a count is requested.
        /// </summary>
        public bool IsCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a get request for a single item is allowed.
        /// </summary>
        public bool AllowGetRequest { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the expression should be executed as Lambda.
        /// </summary>
        public bool ExecuteAsLambda { get; set; }

        /// <summary>
        /// Gets or sets the translated expression.
        /// </summary>
        public Expression Expression { get; set; }
    }
}