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
namespace Sys.Workflow.Engine.Impl.EL
{


    using Sys.Workflow.Engine.Delegate;
    using Sys.Workflow.Engine.Impl.Identities;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;

    /// <summary>
    /// Implementation of an <seealso cref="ELResolver"/> that resolves expressions with the process variables of a given <seealso cref="IVariableScope"/> as context. <br>
    /// Also exposes the currently logged in username to be used in expressions (if any)
    /// 
    /// 
    /// 
    /// </summary>
    public class VariableScopeElResolver : ELResolver
    {
        /// <summary>
        /// 
        /// </summary>
        public static readonly string EXECUTION_KEY = "execution";
        /// <summary>
        /// 
        /// </summary>
        public static readonly string TASK_KEY = "task";
        /// <summary>
        /// 
        /// </summary>
        public static readonly string LOGGED_IN_USER_KEY = "authenticatedUserId";
        /// <summary>
        /// 
        /// </summary>
        protected internal IVariableScope variableScope;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="variableScope"></param>
        public VariableScopeElResolver(IVariableScope variableScope)
        {
            this.variableScope = variableScope;
        }

        /// <summary>
        /// 
        /// </summary>
        public IVariableScope VariableScope => variableScope;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="base"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public override object GetValue(ELContext context, object @base, object property)
        {
            string variable = property.ToString();
            if (@base == null)
            {
                if ((EXECUTION_KEY.Equals(variable) && variableScope is IExecutionEntity) || (TASK_KEY.Equals(variable) && variableScope is ITaskEntity))
                {
                    context.IsPropertyResolved = true;
                    return variableScope;
                }
                else if (EXECUTION_KEY.Equals(variable) && variableScope is ITaskEntity entity)
                {
                    context.IsPropertyResolved = true;
                    return entity.Execution;
                }
                else if (LOGGED_IN_USER_KEY.Equals(variable))
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="base"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public override bool IsReadOnly(ELContext context, object @base, object property)
        {
            if (@base == null)
            {
                return !variableScope.HasVariable(property.ToString());
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="base"></param>
        /// <param name="property"></param>
        /// <param name="value"></param>
        public override void SetValue(ELContext context, object @base, object property, object value)
        {
            if (@base == null)
            {
                string variable = property.ToString();
                if (variableScope.HasVariable(variable))
                {
                    variableScope.SetVariable(variable, value);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        /// <returns></returns>
        public override Type GetCommonPropertyType(ELContext arg0, object arg1)
        {
            return typeof(object);
        }

        //public virtual IEnumerator<FeatureDescriptor> getFeatureDescriptors(ELContext arg0, object arg1)
        //{
        //    return null;
        //}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <returns></returns>
        public override Type GetType(ELContext arg0, object arg1, object arg2)
        {
            return typeof(object);
        }

    }

}