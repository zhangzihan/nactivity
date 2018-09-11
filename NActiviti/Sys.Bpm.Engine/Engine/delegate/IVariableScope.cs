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

namespace org.activiti.engine.@delegate
{

    using org.activiti.engine.history;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;

    /// <summary>
    /// Interface for class that acts as a scope for variables: i.e. the implementation
    /// can be used to set and get variables.
    /// 
    /// Typically, executions (and thus process instances) and tasks are the primary use case
    /// to get and set variables. The <seealso cref="IDelegateExecution"/> for example is often used
    /// in <seealso cref="IJavaDelegate"/> implementation to get and set variables.
    /// 
    /// Variables are typically stored on the 'highest parent'. For executions, this
    /// means that when called on an execution the variable will be stored on the process instance
    /// execution. Variables can be stored on the actual scope itself though, by calling the xxLocal methods.
    /// 
    /// 
    /// 
    /// </summary>
    public interface IVariableScope
    {

        /// <summary>
        /// Returns all variables. 
        /// This will include all variables of parent scopes too. 
        /// </summary>
        IDictionary<string, object> Variables { get; set; }

        /// <summary>
        /// Returns all variables, as instances of the <seealso cref="IVariableInstance"/> interface,
        /// which gives more information than only the the value (type, execution id, etc.)
        /// </summary>
        IDictionary<string, IVariableInstance> VariableInstances { get; }

        /// <summary>
        /// Similar to <seealso cref="#getVariables()"/>, but limited to only the variables with the provided names.
        /// </summary>
        IDictionary<string, object> getVariables(ICollection<string> variableNames);

        /// <summary>
        /// Similar to <seealso cref="#getVariableInstances()"/>, but limited to only the variables with the provided names.
        /// </summary>
        IDictionary<string, IVariableInstance> getVariableInstances(ICollection<string> variableNames);

        /// <summary>
        /// Similar to <seealso cref="#getVariables(Collection))"/>, but with a flag that indicates that all 
        /// variables should be fetched when fetching the specific variables.
        /// 
        /// If set to false, only the specific variables will be fetched.
        /// Dependening on the use case, this can be better for performance, as it avoids fetching and processing 
        /// the other variables. However, if the other variables are needed further on, getting them in
        /// one go is probably better (and the variables are cached during one <seealso cref="Command"/> execution).
        /// </summary>
        IDictionary<string, object> getVariables(ICollection<string> variableNames, bool fetchAllVariables);

        /// <summary>
        /// Similar to <seealso cref="#getVariables(Collection, boolean)"/> but returns the variables 
        /// as instances of the <seealso cref="IVariableInstance"/> interface,
        /// which gives more information than only the the value (type, execution id, etc.)
        /// </summary>
        IDictionary<string, IVariableInstance> getVariableInstances(ICollection<string> variableNames, bool fetchAllVariables);

        /// <summary>
        /// Returns the variable local to this scope only.
        /// So, in contrary to <seealso cref="#getVariables()"/>, the variables from the parent scope won't be returned. 
        /// </summary>
        IDictionary<string, object> VariablesLocal { get; set; }

        /// <summary>
        /// Returns the variables local to this scope as instances of the <seealso cref="IVariableInstance"/> interface,
        /// which provided additional information about the variable.
        /// </summary>
        IDictionary<string, IVariableInstance> VariableInstancesLocal { get; }

        /// <summary>
        /// Similar to <seealso cref="#getVariables(Collection)"/>, but only for variables local to this scope.
        /// </summary>
        IDictionary<string, object> getVariablesLocal(ICollection<string> variableNames);

        /// <summary>
        /// Similar to <seealso cref="#getVariableInstances(Collection)"/>, but only for variables local to this scope.
        /// </summary>
        IDictionary<string, IVariableInstance> getVariableInstancesLocal(ICollection<string> variableNames);

        /// <summary>
        /// Similar to <seealso cref="#getVariables(Collection, boolean)"/>, but only for variables local to this scope.
        /// </summary>
        IDictionary<string, object> getVariablesLocal(ICollection<string> variableNames, bool fetchAllVariables);

        /// <summary>
        /// Similar to <seealso cref="#getVariableInstances(Collection, boolean)"/>, but only for variables local to this scope.
        /// </summary>
        IDictionary<string, IVariableInstance> getVariableInstancesLocal(ICollection<string> variableNames, bool fetchAllVariables);

        /// <summary>
        /// Returns the variable value for one specific variable.
        /// Will look in parent scopes when the variable does not exist on this particular scope. 
        /// </summary>
        object getVariable(string variableName);

        /// <summary>
        /// Similar to <seealso cref="#getVariable(String)"/>, but returns a <seealso cref="IVariableInstance"/> instance,
        /// which contains more information than just the value. 
        /// </summary>
        IVariableInstance getVariableInstance(string variableName);

        /// <summary>
        /// Similar to <seealso cref="#getVariable(String)"/>, but has an extra flag that indicates whether or not 
        /// all variables need to be fetched when getting one variable.
        /// 
        /// By default true (for backwards compatibility reasons), which means that calling <seealso cref="#getVariable(String)"/>
        /// will fetch all variables, of the current scope and all parent scopes.
        /// Setting this flag to false can thus be better for performance. However, variables are cached, and 
        /// if other variables are used later on, setting this true might actually be better for performance.
        /// </summary>
        object getVariable(string variableName, bool fetchAllVariables);

        /// <summary>
        /// Similar to <seealso cref="#getVariable(String, boolean)"/>, but returns an instance of <seealso cref="IVariableInstance"/>, 
        /// which has some additional information beyond the value. 
        /// </summary>
        IVariableInstance getVariableInstance(string variableName, bool fetchAllVariables);

        /// <summary>
        /// Returns the value for the specific variable and only checks this scope and not any parent scope.
        /// </summary>
        object getVariableLocal(string variableName);

        /// <summary>
        /// Similar to <seealso cref="#getVariableLocal(String)"/>, but returns an instance of <seealso cref="IVariableInstance"/>, 
        /// which has some additional information beyond the value. 
        /// </summary>
        IVariableInstance getVariableInstanceLocal(string variableName);

        /// <summary>
        /// Similar to <seealso cref="#getVariableLocal(String)"/>, but has an extra flag that indicates whether or not 
        /// all variables need to be fetched when getting one variable.
        /// 
        /// By default true (for backwards compatibility reasons), which means that calling <seealso cref="#getVariableLocal(String)"/>
        /// will fetch all variables, of the current scope.
        /// Setting this flag to false can thus be better for performance. However, variables are cached, and 
        /// if other variables are used later on, setting this true might actually be better for performance.
        /// </summary>
        object getVariableLocal(string variableName, bool fetchAllVariables);

        /// <summary>
        /// Similar to <seealso cref="#getVariableLocal(String, boolean)"/>, but returns an instance of <seealso cref="IVariableInstance"/>, 
        /// which has some additional information beyond the value.
        /// </summary>
        IVariableInstance getVariableInstanceLocal(string variableName, bool fetchAllVariables);

        /// <summary>
        /// Typed version of the <seealso cref="#getVariable(String)"/> method. 
        /// </summary>
        T getVariable<T>(string variableName);

        /// <summary>
        /// Typed version of the <seealso cref="#getVariableLocal(String)"/> method.
        /// </summary>
        T getVariableLocal<T>(string variableName);

        /// <summary>
        /// Returns all the names of the variables for this scope and all parent scopes. 
        /// </summary>
        ISet<string> VariableNames { get; }

        /// <summary>
        /// Returns all the names of the variables for this scope (no parent scopes).
        /// </summary>
        ISet<string> VariableNamesLocal { get; }

        /// <summary>
        /// Sets the variable with the provided name to the provided value.
        /// 
        /// <para>
        /// A variable is set according to the following algorithm:
        /// 
        /// </para>
        /// <para>
        /// <li>If this scope already contains a variable by the provided name as a
        /// <strong>local</strong> variable, its value is overwritten to the provided
        /// value.</li>
        /// <li>If this scope does <strong>not</strong> contain a variable by the
        /// provided name as a local variable, the variable is set to this scope's
        /// parent scope, if there is one. If there is no parent scope (meaning this
        /// scope is the root scope of the hierarchy it belongs to), this scope is
        /// used. This applies recursively up the parent scope chain until, if no scope
        /// contains a local variable by the provided name, ultimately the root scope
        /// is reached and the variable value is set on that scope.</li>
        /// </para>
        /// <para>
        /// In practice for most cases, this algorithm will set variables to the scope
        /// of the execution at the process instance’s root level, if there is no
        /// execution-local variable by the provided name.
        /// 
        /// </para>
        /// </summary>
        /// <param name="variableName">
        ///          the name of the variable to be set </param>
        /// <param name="value">
        ///          the value of the variable to be set </param>
        void setVariable(string variableName, object value);

        /// <summary>
        /// Similar to <seealso cref="#setVariable(String, Object)"/>, but with an extra flag to indicate whether 
        /// all variables should be fetched while doing this or not.
        /// 
        /// The variable will be put on the highest possible scope. For an execution this is the process instance execution.
        /// If this is not wanted, use the <seealso cref="#setVariableLocal(String, Object)"/> method instead. 
        /// 
        /// The default (e.g. when calling <seealso cref="#setVariable(String, Object)"/>), is <i>true</i>, for backwards
        /// compatibility reasons. However, in some use cases, it might make sense not to fetch any other variables
        /// when setting one variable (for example when doing nothing more than just setting one variable).
        /// </summary>
        void setVariable(string variableName, object value, bool fetchAllVariables);

        /// <summary>
        /// Similar to <seealso cref="#setVariable(String, Object)"/>, but the variable is set to this scope specifically.
        /// </summary>
        object setVariableLocal(string variableName, object value);

        /// <summary>
        /// Similar to <seealso cref="#setVariableLocal(String, Object, boolean)"/>, but the variable is set to this scope specifically. 
        /// </summary>
        object setVariableLocal(string variableName, object value, bool fetchAllVariables);



        /// <summary>
        /// Returns whether this scope or any parent scope has variables. 
        /// </summary>
        bool hasVariables();

        /// <summary>
        /// Returns whether this scope has variables. 
        /// </summary>
        bool hasVariablesLocal();

        /// <summary>
        /// Returns whether this scope or any parent scope has a specific variable. 
        /// </summary>
        bool hasVariable(string variableName);

        /// <summary>
        /// Returns whether this scope has a specific variable. 
        /// </summary>
        bool hasVariableLocal(string variableName);

        /// <summary>
        /// Removes the variable and creates a new;@link HistoricVariableUpdateEntity}
        /// </summary>
        void removeVariable(string variableName);

        /// <summary>
        /// Removes the local variable and creates a new <seealso cref="IHistoricVariableUpdate"/>.
        /// </summary>
        void removeVariableLocal(string variableName);

        /// <summary>
        /// Removes the variables and creates a new
        /// <seealso cref="IHistoricVariableUpdate"/> for each of them.
        /// </summary>
        void removeVariables(ICollection<string> variableNames);

        /// <summary>
        /// Removes the local variables and creates a new
        /// <seealso cref="IHistoricVariableUpdate"/> for each of them.
        /// </summary>
        void removeVariablesLocal(ICollection<string> variableNames);

        /// <summary>
        /// Removes the (local) variables and creates a new
        /// <seealso cref="IHistoricVariableUpdate"/> for each of them.
        /// </summary>
        void removeVariables();

        /// <summary>
        /// Removes the (local) variables and creates a new
        /// <seealso cref="IHistoricVariableUpdate"/> for each of them.
        /// </summary>
        void removeVariablesLocal();

        /// <summary>
        /// Similar to <seealso cref="#setVariable(String, Object)"/>, but the variable is transient:
        /// 
        /// - no history is kept for the variable
        /// - the variable is only available until a waitstate is reached in the process
        /// - transient variables 'shadow' persistent variable (when getVariable('abc') 
        ///   where 'abc' is both persistent and transient, the transient value is returned.
        /// </summary>
        void setTransientVariable(string variableName, object variableValue);

        /// <summary>
        /// Similar to <seealso cref="#setVariableLocal(String, Object)"/>, but for a transient variable.
        /// See <seealso cref="#setTransientVariable(String, Object)"/> for the rules on 'transient' variables. 
        /// </summary>
        void setTransientVariableLocal(string variableName, object variableValue);

        /// <summary>
        /// Similar to <seealso cref="#setVariables(Map)"/>, but for transient variables.
        /// See <seealso cref="#setTransientVariable(String, Object)"/> for the rules on 'transient' variables. 
        /// </summary>
        IDictionary<string, object> TransientVariables { set; get; }

        /// <summary>
        /// Similar to <seealso cref="#getVariable(String)"/>, including the searching via the parent scopes, but
        /// for transient variables only.
        /// See <seealso cref="#setTransientVariable(String, Object)"/> for the rules on 'transient' variables.
        /// </summary>
        object getTransientVariable(string variableName);


        /// <summary>
        /// Similar to <seealso cref="#setVariablesLocal(Map)"/>, but for transient variables.
        /// See <seealso cref="#setTransientVariable(String, Object)"/> for the rules on 'transient' variables. 
        /// </summary>
        IDictionary<string, object> TransientVariablesLocal { set; get; }

        /// <summary>
        /// Similar to <seealso cref="#getVariableLocal(String)"/>, but for a transient variable.
        /// See <seealso cref="#setTransientVariable(String, Object)"/> for the rules on 'transient' variables. 
        /// </summary>
        object getTransientVariableLocal(string variableName);


        /// <summary>
        /// Removes a specific transient variable (also searching parent scopes).
        /// See <seealso cref="#setTransientVariable(String, Object)"/> for the rules on 'transient' variables. 
        /// </summary>
        void removeTransientVariableLocal(string variableName);

        /// <summary>
        /// Removes a specific transient variable.
        /// See <seealso cref="#setTransientVariable(String, Object)"/> for the rules on 'transient' variables. 
        /// </summary>
        void removeTransientVariable(string variableName);

        /// <summary>
        /// Remove all transient variable of this scope and its parent scopes.
        /// See <seealso cref="#setTransientVariable(String, Object)"/> for the rules on 'transient' variables.
        /// </summary>
        void removeTransientVariables();

        /// <summary>
        /// Removes all local transient variables.
        /// See <seealso cref="#setTransientVariable(String, Object)"/> for the rules on 'transient' variables.
        /// </summary>
        void removeTransientVariablesLocal();

    }
}