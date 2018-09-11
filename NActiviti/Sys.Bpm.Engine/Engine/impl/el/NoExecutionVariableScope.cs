using System;
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

namespace org.activiti.engine.impl.el
{

    using org.activiti.engine.@delegate;
    using org.activiti.engine.impl.persistence.entity;

    /// <summary>
    /// Variable-scope only used to resolve variables when NO execution is active but expression-resolving is needed. This occurs eg. when start-form properties have default's defined. Even though
    /// variables are not available yet, expressions should be resolved anyway.
    /// 
    /// 
    /// 
    /// </summary>
    public class NoExecutionVariableScope : IVariableScope
    {

        private static readonly NoExecutionVariableScope INSTANCE = new NoExecutionVariableScope();

        /// <summary>
        /// Since a <seealso cref="NoExecutionVariableScope"/> has no state, it's safe to use the same instance to prevent too many useless instances created.
        /// </summary>
        public static NoExecutionVariableScope SharedInstance
        {
            get
            {
                return INSTANCE;
            }
        }

        public virtual IDictionary<string, object> Variables
        {
            get
            {
                return new Dictionary<string, object>();
            }
            set
            {
                throw new System.NotSupportedException("No execution active, no variables can be set");
            }
        }

        public virtual IDictionary<string, object> VariablesLocal
        {
            get
            {
                return new Dictionary<string, object>();
            }
            set
            {
                throw new System.NotSupportedException("No execution active, no variables can be set");
            }
        }

        public virtual IDictionary<string, object> getVariables(ICollection<string> variableNames)
        {
            return new Dictionary<string, object>();
        }

        public virtual IDictionary<string, object> getVariables(ICollection<string> variableNames, bool fetchAllVariables)
        {
            return new Dictionary<string, object>();
        }

        public virtual IDictionary<string, object> getVariablesLocal(ICollection<string> variableNames)
        {
            return new Dictionary<string, object>();
        }

        public virtual IDictionary<string, object> getVariablesLocal(ICollection<string> variableNames, bool fetchAllVariables)
        {
            return new Dictionary<string, object>();
        }

        public virtual object getVariable(string variableName)
        {
            return null;
        }

        public virtual object getVariable(string variableName, bool fetchAllVariables)
        {
            return null;
        }

        public virtual object getVariableLocal(string variableName)
        {
            return null;
        }

        public virtual object getVariableLocal(string variableName, bool fetchAllVariables)
        {
            return null;
        }

        public virtual T getVariable<T>(string variableName)
        {
            return default(T);
        }

        public virtual T getVariableLocal<T>(string variableName)
        {
            return default(T);
        }

        public virtual IDictionary<string, IVariableInstance> VariableInstances
        {
            get
            {
                return null;
            }
        }

        public virtual IDictionary<string, IVariableInstance> getVariableInstances(ICollection<string> variableNames)
        {
            return null;
        }

        public virtual IDictionary<string, IVariableInstance> getVariableInstances(ICollection<string> variableNames, bool fetchAllVariables)
        {
            return null;
        }

        public virtual IDictionary<string, IVariableInstance> VariableInstancesLocal
        {
            get
            {
                return null;
            }
        }

        public virtual IDictionary<string, IVariableInstance> getVariableInstancesLocal(ICollection<string> variableNames)
        {
            return null;
        }

        public virtual IDictionary<string, IVariableInstance> getVariableInstancesLocal(ICollection<string> variableNames, bool fetchAllVariables)
        {
            return null;
        }

        public virtual IVariableInstance getVariableInstance(string variableName)
        {
            return null;
        }

        public virtual IVariableInstance getVariableInstance(string variableName, bool fetchAllVariables)
        {
            return null;
        }

        public virtual IVariableInstance getVariableInstanceLocal(string variableName)
        {
            return null;
        }

        public virtual IVariableInstance getVariableInstanceLocal(string variableName, bool fetchAllVariables)
        {
            return null;
        }

        public virtual ISet<string> VariableNames
        {
            get
            {
                return new HashSet<string>();
            }
        }

        public virtual ISet<string> VariableNamesLocal
        {
            get
            {
                return new HashSet<string>();
            }
        }

        public virtual void setVariable(string variableName, object value)
        {
            throw new System.NotSupportedException("No execution active, no variables can be set");
        }

        public virtual void setVariable(string variableName, object value, bool fetchAllVariables)
        {
            throw new System.NotSupportedException("No execution active, no variables can be set");
        }

        public virtual object setVariableLocal(string variableName, object value)
        {
            throw new System.NotSupportedException("No execution active, no variables can be set");
        }

        public virtual object setVariableLocal(string variableName, object value, bool fetchAllVariables)
        {
            throw new System.NotSupportedException("No execution active, no variables can be set");
        }



        public virtual bool hasVariables()
        {
            return false;
        }

        public virtual bool hasVariablesLocal()
        {
            return false;
        }

        public virtual bool hasVariable(string variableName)
        {
            return false;
        }

        public virtual bool hasVariableLocal(string variableName)
        {
            return false;
        }

        public virtual void createVariableLocal(string variableName, object value)
        {
            throw new System.NotSupportedException("No execution active, no variables can be created");
        }

        public virtual void createVariablesLocal<T1>(IDictionary<string, T1> variables)
        {
            throw new System.NotSupportedException("No execution active, no variables can be created");
        }

        public virtual void removeVariable(string variableName)
        {
            throw new System.NotSupportedException("No execution active, no variables can be removed");
        }

        public virtual void removeVariableLocal(string variableName)
        {
            throw new System.NotSupportedException("No execution active, no variables can be removed");
        }

        public virtual void removeVariables()
        {
            throw new System.NotSupportedException("No execution active, no variables can be removed");
        }

        public virtual void removeVariablesLocal()
        {
            throw new System.NotSupportedException("No execution active, no variables can be removed");
        }

        public virtual void removeVariables(ICollection<string> variableNames)
        {
            throw new System.NotSupportedException("No execution active, no variables can be removed");
        }

        public virtual void removeVariablesLocal(ICollection<string> variableNames)
        {
            throw new System.NotSupportedException("No execution active, no variables can be removed");
        }

        public virtual IDictionary<string, object> TransientVariablesLocal
        {
            set
            {
                throw new System.NotSupportedException("No execution active, no variables can be set");
            }
            get
            {
                return null;
            }
        }

        public virtual void setTransientVariableLocal(string variableName, object variableValue)
        {
            throw new System.NotSupportedException("No execution active, no variables can be set");
        }

        public virtual IDictionary<string, object> TransientVariables
        {
            set
            {
                throw new System.NotSupportedException("No execution active, no variables can be set");
            }
            get
            {
                return null;
            }
        }

        public virtual void setTransientVariable(string variableName, object variableValue)
        {
            throw new System.NotSupportedException("No execution active, no variables can be set");
        }

        public virtual object getTransientVariableLocal(string variableName)
        {
            return null;
        }


        public virtual object getTransientVariable(string variableName)
        {
            return null;
        }


        public virtual void removeTransientVariableLocal(string variableName)
        {
            throw new System.NotSupportedException("No execution active, no variables can be removed");
        }

        public virtual void removeTransientVariablesLocal()
        {
            throw new System.NotSupportedException("No execution active, no variables can be removed");
        }

        public virtual void removeTransientVariable(string variableName)
        {
            throw new System.NotSupportedException("No execution active, no variables can be removed");
        }

        public virtual void removeTransientVariables()
        {
            throw new System.NotSupportedException("No execution active, no variables can be removed");
        }
    }

}