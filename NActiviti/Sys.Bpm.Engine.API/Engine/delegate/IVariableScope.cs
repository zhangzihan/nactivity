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

namespace Sys.Workflow.engine.@delegate
{
    using Sys.Workflow.engine.history;
    using Sys.Workflow.engine.impl.persistence.entity;
    using System.Collections;

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
        /// Similar to <seealso cref="GetVariables()"/>, but limited to only the variables with the provided names.
        /// </summary>
        IDictionary<string, object> GetVariables(IEnumerable<string> variableNames);

        /// <summary>
        /// Similar to <seealso cref="GetVariableInstances()"/>, but limited to only the variables with the provided names.
        /// </summary>
        IDictionary<string, IVariableInstance> GetVariableInstances(IEnumerable<string> variableNames);

        /// <summary>
        /// Similar to <seealso cref="GetVariables(ICollection)"/>, but with a flag that indicates that all 
        /// variables should be fetched when fetching the specific variables.
        /// 
        /// If set to false, only the specific variables will be fetched.
        /// Dependening on the use case, this can be better for performance, as it avoids fetching and processing 
        /// the other variables. However, if the other variables are needed further on, getting them in
        /// one go is probably better (and the variables are cached during one <seealso cref="Command"/> execution).
        /// </summary>
        IDictionary<string, object> GetVariables(IEnumerable<string> variableNames, bool fetchAllVariables);

        /// <summary>
        /// Similar to <seealso cref="GetVariables(ICollection, bool)"/> but returns the variables 
        /// as instances of the <seealso cref="IVariableInstance"/> interface,
        /// which gives more information than only the the value (type, execution id, etc.)
        /// </summary>
        IDictionary<string, IVariableInstance> GetVariableInstances(IEnumerable<string> variableNames, bool fetchAllVariables);

        /// <summary>
        /// Returns the variable local to this scope only.
        /// So, in contrary to <seealso cref="GetVariables()"/>, the variables from the parent scope won't be returned. 
        /// </summary>
        IDictionary<string, object> VariablesLocal { get; set; }

        /// <summary>
        /// Returns the variables local to this scope as instances of the <seealso cref="IVariableInstance"/> interface,
        /// which provided additional information about the variable.
        /// </summary>
        IDictionary<string, IVariableInstance> VariableInstancesLocal { get; }

        /// <summary>
        /// Similar to <seealso cref="GetVariables(ICollection)"/>, but only for variables local to this scope.
        /// </summary>
        IDictionary<string, object> GetVariablesLocal(IEnumerable<string> variableNames);

        /// <summary>
        /// Similar to <seealso cref="GetVariableInstances(ICollection)"/>, but only for variables local to this scope.
        /// </summary>
        IDictionary<string, IVariableInstance> GetVariableInstancesLocal(IEnumerable<string> variableNames);

        /// <summary>
        /// Similar to <seealso cref="GetVariables(ICollection, bool)"/>, but only for variables local to this scope.
        /// </summary>
        IDictionary<string, object> GetVariablesLocal(IEnumerable<string> variableNames, bool fetchAllVariables);

        /// <summary>
        /// Similar to <seealso cref="GetVariableInstances(ICollection, bool)"/>, but only for variables local to this scope.
        /// </summary>
        IDictionary<string, IVariableInstance> GetVariableInstancesLocal(IEnumerable<string> variableNames, bool fetchAllVariables);

        /// <summary>
        /// Returns the variable value for one specific variable.
        /// Will look in parent scopes when the variable does not exist on this particular scope. 
        /// </summary>
        object GetVariable(string variableName);

        /// <summary>
        /// Similar to <seealso cref="GetVariable(string)"/>, but returns a <seealso cref="IVariableInstance"/> instance,
        /// which contains more information than just the value. 
        /// </summary>
        IVariableInstance GetVariableInstance(string variableName);

        /// <summary>
        /// Similar to <seealso cref="GetVariable(string)"/>, but has an extra flag that indicates whether or not 
        /// all variables need to be fetched when getting one variable.
        /// 
        /// By default true (for backwards compatibility reasons), which means that calling <seealso cref="GetVariable(string)"/>
        /// will fetch all variables, of the current scope and all parent scopes.
        /// Setting this flag to false can thus be better for performance. However, variables are cached, and 
        /// if other variables are used later on, setting this true might actually be better for performance.
        /// </summary>
        object GetVariable(string variableName, bool fetchAllVariables);

        /// <summary>
        /// Similar to <seealso cref="GetVariable(string, bool)"/>, but returns an instance of <seealso cref="IVariableInstance"/>, 
        /// which has some additional information beyond the value. 
        /// </summary>
        IVariableInstance GetVariableInstance(string variableName, bool fetchAllVariables);

        /// <summary>
        /// Returns the value for the specific variable and only checks this scope and not any parent scope.
        /// </summary>
        object GetVariableLocal(string variableName);

        /// <summary>
        /// Similar to <seealso cref="GetVariableLocal(string)"/>, but returns an instance of <seealso cref="IVariableInstance"/>, 
        /// which has some additional information beyond the value. 
        /// </summary>
        IVariableInstance GetVariableInstanceLocal(string variableName);

        /// <summary>
        /// Similar to <seealso cref="GetVariableLocal(string)"/>, but has an extra flag that indicates whether or not 
        /// all variables need to be fetched when getting one variable.
        /// 
        /// By default true (for backwards compatibility reasons), which means that calling <seealso cref="GetVariableLocal(string)"/>
        /// will fetch all variables, of the current scope.
        /// Setting this flag to false can thus be better for performance. However, variables are cached, and 
        /// if other variables are used later on, setting this true might actually be better for performance.
        /// </summary>
        object GetVariableLocal(string variableName, bool fetchAllVariables);

        /// <summary>
        /// Similar to <seealso cref="GetVariableLocal(string, bool)"/>, but returns an instance of <seealso cref="IVariableInstance"/>, 
        /// which has some additional information beyond the value.
        /// </summary>
        IVariableInstance GetVariableInstanceLocal(string variableName, bool fetchAllVariables);

        /// <summary>
        /// Typed version of the <seealso cref="GetVariable(string)"/> method. 
        /// </summary>
        T GetVariable<T>(string variableName);

        /// <summary>
        /// Typed version of the <seealso cref="GetVariableLocal(string)"/> method.
        /// </summary>
        T GetVariableLocal<T>(string variableName);

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
        void SetVariable(string variableName, object value);

        /// <summary>
        /// Similar to <seealso cref="SetVariable(string, object)"/>, but with an extra flag to indicate whether 
        /// all variables should be fetched while doing this or not.
        /// 
        /// The variable will be put on the highest possible scope. For an execution this is the process instance execution.
        /// If this is not wanted, use the <seealso cref="SetVariableLocal(string, object)"/> method instead. 
        /// 
        /// The default (e.g. when calling <seealso cref="SetVariable(string, object)"/>), is <i>true</i>, for backwards
        /// compatibility reasons. However, in some use cases, it might make sense not to fetch any other variables
        /// when setting one variable (for example when doing nothing more than just setting one variable).
        /// </summary>
        void SetVariable(string variableName, object value, bool fetchAllVariables);

        /// <summary>
        /// Similar to <seealso cref="SetVariable(string, object)"/>, but the variable is set to this scope specifically.
        /// </summary>
        object SetVariableLocal(string variableName, object value);

        /// <summary>
        /// Similar to <seealso cref="SetVariableLocal(string, object, bool)"/>, but the variable is set to this scope specifically. 
        /// </summary>
        object SetVariableLocal(string variableName, object value, bool fetchAllVariables);



        /// <summary>
        /// Returns whether this scope or any parent scope has variables. 
        /// </summary>
        bool HasVariables();

        /// <summary>
        /// Returns whether this scope has variables. 
        /// </summary>
        bool HasVariablesLocal();

        /// <summary>
        /// Returns whether this scope or any parent scope has a specific variable. 
        /// </summary>
        bool HasVariable(string variableName);

        /// <summary>
        /// Returns whether this scope has a specific variable. 
        /// </summary>
        bool HasVariableLocal(string variableName);

        /// <summary>
        /// Removes the variable and creates a new;@link HistoricVariableUpdateEntity}
        /// </summary>
        void RemoveVariable(string variableName);

        /// <summary>
        /// Removes the local variable and creates a new <seealso cref="IHistoricVariableUpdate"/>.
        /// </summary>
        void RemoveVariableLocal(string variableName);

        /// <summary>
        /// Removes the variables and creates a new
        /// <seealso cref="IHistoricVariableUpdate"/> for each of them.
        /// </summary>
        void RemoveVariables(IEnumerable<string> variableNames);

        /// <summary>
        /// Removes the local variables and creates a new
        /// <seealso cref="IHistoricVariableUpdate"/> for each of them.
        /// </summary>
        void RemoveVariablesLocal(IEnumerable<string> variableNames);

        /// <summary>
        /// Removes the (local) variables and creates a new
        /// <seealso cref="IHistoricVariableUpdate"/> for each of them.
        /// </summary>
        void RemoveVariables();

        /// <summary>
        /// Removes the (local) variables and creates a new
        /// <seealso cref="IHistoricVariableUpdate"/> for each of them.
        /// </summary>
        void RemoveVariablesLocal();

        /// <summary>
        /// Similar to <seealso cref="SetVariable(string, object)"/>, but the variable is transient:
        /// 
        /// - no history is kept for the variable
        /// - the variable is only available until a waitstate is reached in the process
        /// - transient variables 'shadow' persistent variable (when getVariable('abc') 
        ///   where 'abc' is both persistent and transient, the transient value is returned.
        /// </summary>
        void SetTransientVariable(string variableName, object variableValue);

        /// <summary>
        /// Similar to <seealso cref="SetVariableLocal(string, object)"/>, but for a transient variable.
        /// See <seealso cref="SetTransientVariable(string, object)"/> for the rules on 'transient' variables. 
        /// </summary>
        void SetTransientVariableLocal(string variableName, object variableValue);

        /// <summary>
        /// Similar to <seealso cref="SetVariables(Map)"/>, but for transient variables.
        /// See <seealso cref="SetTransientVariable(string, object)"/> for the rules on 'transient' variables. 
        /// </summary>
        IDictionary<string, object> TransientVariables { set; get; }

        /// <summary>
        /// Similar to <seealso cref="GetVariable(string)"/>, including the searching via the parent scopes, but
        /// for transient variables only.
        /// See <seealso cref="SetTransientVariable(string, object)"/> for the rules on 'transient' variables.
        /// </summary>
        object GetTransientVariable(string variableName);


        /// <summary>
        /// Similar to <seealso cref="SetVariablesLocal(Map)"/>, but for transient variables.
        /// See <seealso cref="SetTransientVariable(string, object)"/> for the rules on 'transient' variables. 
        /// </summary>
        IDictionary<string, object> TransientVariablesLocal { set; get; }

        /// <summary>
        /// Similar to <seealso cref="GetVariableLocal(string)"/>, but for a transient variable.
        /// See <seealso cref="SetTransientVariable(string, object)"/> for the rules on 'transient' variables. 
        /// </summary>
        object GetTransientVariableLocal(string variableName);


        /// <summary>
        /// Removes a specific transient variable (also searching parent scopes).
        /// See <seealso cref="SetTransientVariable(string, object)"/> for the rules on 'transient' variables. 
        /// </summary>
        void RemoveTransientVariableLocal(string variableName);

        /// <summary>
        /// Removes a specific transient variable.
        /// See <seealso cref="SetTransientVariable(string, object)"/> for the rules on 'transient' variables. 
        /// </summary>
        void RemoveTransientVariable(string variableName);

        /// <summary>
        /// Remove all transient variable of this scope and its parent scopes.
        /// See <seealso cref="SetTransientVariable(string, object)"/> for the rules on 'transient' variables.
        /// </summary>
        void RemoveTransientVariables();

        /// <summary>
        /// Removes all local transient variables.
        /// See <seealso cref="SetTransientVariable(string, object)"/> for the rules on 'transient' variables.
        /// </summary>
        void RemoveTransientVariablesLocal();

    }
}