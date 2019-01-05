using Spring.Expressions;
using System.Collections.Generic;

namespace Sys.Expressions
{
    public static class ExpressionManager
    {
        public static object GetValue(object context, string expression, IDictionary<string, object> variables)
        {
            IExpression express = Expression.Parse(expression);

            var res = express.GetValue(context, variables);

            return res;
        }
    }
}
