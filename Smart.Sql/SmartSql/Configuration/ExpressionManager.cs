using Spring.Expressions;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Sys.Expressions
{
    public static class ExpressionManager
    {
        public static object GetValue(object context, string expression, IDictionary<string, object> variables)
        {
            Regex expToken = new Regex(@"[#]{(\w+)}", RegexOptions.IgnoreCase);

            string expr = expression;
            if (expToken.IsMatch(expr))
            {
                expr = expToken.Replace(expr, match =>
                {
                    var token = match.Groups[1].Value;

                    if(!variables.ContainsKey(token))
                    {
                        return "";
                    }

                    return $"{variables[token]}";
                });
            }

            IExpression express = Expression.Parse(expr);

            var res = express.GetValue(context, variables);

            return res;
        }
    }
}
