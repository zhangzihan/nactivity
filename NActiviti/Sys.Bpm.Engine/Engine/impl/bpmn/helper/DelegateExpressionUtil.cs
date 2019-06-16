using System.Collections.Generic;

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
namespace Sys.Workflow.Engine.Impl.Bpmn.Helper
{

    using Sys.Workflow.Engine.Delegate;
    using Sys.Workflow.Engine.Impl.Bpmn.Parser;
    using Sys.Workflow.Engine.Impl.Cfg;
    using Sys.Workflow.Engine.Impl.Contexts;

    /// 
    public class DelegateExpressionUtil
    {

        public static object ResolveDelegateExpression(IExpression expression, IVariableScope variableScope)
        {
            return ResolveDelegateExpression(expression, variableScope, null);
        }

        public static object ResolveDelegateExpression(IExpression expression, IVariableScope variableScope, IList<FieldDeclaration> fieldDeclarations)
        {

            // Note: we can't cache the result of the expression, because the
            // execution can change: eg. delegateExpression='${mySpringBeanFactory.randomSpringBean()}'
            object @delegate = expression.GetValue(variableScope);

            if (fieldDeclarations != null && fieldDeclarations.Count > 0)
            {

                DelegateExpressionFieldInjectionMode injectionMode = Context.ProcessEngineConfiguration.DelegateExpressionFieldInjectionMode;
                if (injectionMode.Equals(DelegateExpressionFieldInjectionMode.COMPATIBILITY))
                {
                    ClassDelegate.ApplyFieldDeclaration(fieldDeclarations, @delegate, true);
                }
                else if (injectionMode.Equals(DelegateExpressionFieldInjectionMode.MIXED))
                {
                    ClassDelegate.ApplyFieldDeclaration(fieldDeclarations, @delegate, false);
                }

            }

            return @delegate;
        }

    }

}