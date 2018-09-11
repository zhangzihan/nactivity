using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Expressions
{
    public static class ExpressionManager
    {
        public static object GetValue(object context, string expression, IDictionary<string, object> variables)
        {
            Spring.Expressions.IExpression express = Spring.Expressions.Expression.Parse(expression);

            var res = express.GetValue(context, variables);

            return res;
        }
    }
}
