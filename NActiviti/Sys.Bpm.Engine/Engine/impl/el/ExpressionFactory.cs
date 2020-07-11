/* Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *      http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Text.RegularExpressions;

namespace Sys.Workflow.Engine.Impl.EL
{
    /// <summary>
    /// 
    /// </summary>
    class ExpressionFactory : IExpressionFactory
    {
        private static readonly Regex EXPR_PATTERN = new Regex(@"\${(.*?)}", RegexOptions.Multiline);
        private static readonly Regex RETURN_PATTERN = new Regex(@"^\s*return\s+|;\s*return\s+", RegexOptions.Multiline);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="elContext"></param>
        /// <param name="expression"></param>
        /// <param name="expectedType"></param>
        /// <returns></returns>
        public IValueExpression CreateValueExpression(ELContext elContext, string expression, Type expectedType)
        {
            if (EXPR_PATTERN.IsMatch(expression) || RETURN_PATTERN.IsMatch(expression) == false)
            {
                return new ValueExpression(expression, expectedType);
            }

            return new ScriptingExpression("C#", expression, expectedType);
        }
    }
}