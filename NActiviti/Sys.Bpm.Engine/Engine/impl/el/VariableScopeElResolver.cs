using System;

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
namespace Sys.Workflow.engine.impl.el
{


    using Sys.Workflow.engine.@delegate;
    using Sys.Workflow.engine.impl.identity;
    using Sys.Workflow.engine.impl.persistence.entity;

    /// <summary>
    /// Implementation of an <seealso cref="ELResolver"/> that resolves expressions with the process variables of a given <seealso cref="IVariableScope"/> as context. <br>
    /// Also exposes the currently logged in username to be used in expressions (if any)
    /// 
    /// 
    /// 
    /// </summary>
    public class VariableScopeElResolver : ELResolver
    {
        public static readonly string EXECUTION_KEY = "execution";
        public static readonly string TASK_KEY = "task";
        public static readonly string LOGGED_IN_USER_KEY = "authenticatedUserId";

        protected internal IVariableScope variableScope;

        public VariableScopeElResolver(IVariableScope variableScope)
        {
            this.variableScope = variableScope;
        }

        public IVariableScope VariableScope => variableScope;

        public override object GetValue(ELContext context, object @base, object property)
        {
            string variable = (string)property; // according to javadoc, can only be a String

            if (@base == null)
            {

                if ((EXECUTION_KEY.Equals(property) && variableScope is IExecutionEntity) || (TASK_KEY.Equals(property) && variableScope is ITaskEntity))
                {
                    context.IsPropertyResolved = true;
                    return variableScope;
                }
                else if (EXECUTION_KEY.Equals(property) && variableScope is ITaskEntity)
                {
                    context.IsPropertyResolved = true;
                    return ((ITaskEntity)variableScope).Execution;
                }
                else if (LOGGED_IN_USER_KEY.Equals(property))
                {
                    context.IsPropertyResolved = true;
                    return Authentication.AuthenticatedUser.Id;
                }
                else
                {
                    if (variableScope.HasVariable(variable))
                    {
                        context.IsPropertyResolved = true; // if not set, the next elResolver in the CompositeElResolver will be called
                        return variableScope.GetVariable(variable);
                    }
                }
            }
            else
            {
                if (variableScope.HasVariable(variable))
                {
                    context.IsPropertyResolved = true; // if not set, the next elResolver in the CompositeElResolver will be called
                    return variableScope.GetVariable(variable);
                }
            }

            // property resolution (eg. bean.value) will be done by the
            // BeanElResolver (part of the CompositeElResolver)
            // It will use the bean resolved in this resolver as base.

            return null;
        }

        public override bool IsReadOnly(ELContext context, object @base, object property)
        {
            if (@base == null)
            {
                string variable = (string)property;
                return !variableScope.HasVariable(variable);
            }
            return true;
        }

        public override void SetValue(ELContext context, object @base, object property, object value)
        {
            if (@base == null)
            {
                string variable = (string)property;
                if (variableScope.HasVariable(variable))
                {
                    variableScope.SetVariable(variable, value);
                }
            }
        }

        public override Type GetCommonPropertyType(ELContext arg0, object arg1)
        {
            return typeof(object);
        }

        //public virtual IEnumerator<FeatureDescriptor> getFeatureDescriptors(ELContext arg0, object arg1)
        //{
        //    return null;
        //}

        public override Type GetType(ELContext arg0, object arg1, object arg2)
        {
            return typeof(object);
        }

    }

}