using Spring.Expressions;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Sys.Expressions
{
    public static class ExpressionManager
    {
        private static readonly Regex EXPTOKEN_PATTERN = new Regex(@"[#]{(([\p{L}\p{N}_]+)(\.[\p{L}\p{N}_]+)*)}", RegexOptions.IgnoreCase);

        public static object GetValue(object context, string expression, IDictionary<string, object> variables)
        {

            string expr = expression;
            if (EXPTOKEN_PATTERN.IsMatch(expr))
            {
                expr = EXPTOKEN_PATTERN.Replace(expr, match =>
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
