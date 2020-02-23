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
namespace Sys.Workflow.Engine.Impl.Persistence.Entity
{
    using Newtonsoft.Json;
    using Sys.Workflow.Engine.Delegate;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.EL;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Variable;
    using Sys.Workflow;
    using System.Collections.ObjectModel;
    using System.Linq;

    /// 
    /// 
    /// 
    /// 
    [Serializable]
    public abstract class VariableScopeImpl : AbstractEntity, IVariableScope
    {
        public override abstract PersistentState PersistentState { get; }

        private const long serialVersionUID = 1L;

        // The cache used when fetching all variables
        protected IDictionary<string, IVariableInstanceEntity> variableInstances; // needs to be null, the logic depends on it for checking if vars were already fetched

        // The cache is used when fetching/setting specific variables
        private IDictionary<string, IVariableInstanceEntity> usedVariablesCache = new Dictionary<string, IVariableInstanceEntity>();

        private IDictionary<string, IVariableInstance> transientVariabes;

        private ELContext cachedElContext;

        protected internal abstract IList<IVariableInstanceEntity> LoadVariableInstances();

        protected internal abstract VariableScopeImpl ParentVariableScope { get; }

        protected internal abstract void InitializeVariableInstanceBackPointer(IVariableInstanceEntity variableInstance);

        protected internal virtual void EnsureVariableInstancesInitialized()
        {
            if (variableInstances == null)
            {
                variableInstances = new Dictionary<string, IVariableInstanceEntity>();

                ICommandContext commandContext = Context.CommandContext;
                if (commandContext == null)
                {
                    throw new ActivitiException("lazy loading outside command context");
                }
                IEnumerable<IVariableInstanceEntity> variableInstancesList = LoadVariableInstances();
                foreach (IVariableInstanceEntity variableInstance in variableInstancesList)
                {
                    variableInstances[variableInstance.Name] = variableInstance;
                }
            }
        }

        [JsonIgnore]
        public virtual IDictionary<string, object> Variables
        {
            get
            {
                return CollectVariables(new Dictionary<string, object>());
            }
            set
            {
                if (value != null)
                {
                    foreach (string variableName in value.Keys)
                    {
                        SetVariable(variableName, value[variableName]);
                    }
                }
            }
        }

        [JsonIgnore]
        public virtual IDictionary<string, IVariableInstance> VariableInstances
        {
            get
            {
                return CollectVariableInstances(new Dictionary<string, IVariableInstance>());
            }
        }

        public virtual IDictionary<string, object> GetVariables(IEnumerable<string> variableNames)
        {
            return GetVariables(variableNames, true);
        }

        public virtual IDictionary<string, IVariableInstance> GetVariableInstances(IEnumerable<string> variableNames)
        {
            return GetVariableInstances(variableNames, true);
        }

        public virtual IDictionary<string, object> GetVariables(IEnumerable<string> variableNames, bool fetchAllVariables)
        {

            IDictionary<string, object> requestedVariables = new Dictionary<string, object>();
            ISet<string> variableNamesToFetch = new HashSet<string>(variableNames);

            // Transient variables 'shadow' any existing variables.
            // The values in the fetch-cache will be more recent, so they can override any existing ones
            foreach (string variableName in variableNames)
            {
                if (transientVariabes != null && transientVariabes.ContainsKey(variableName))
                {
                    requestedVariables[variableName] = transientVariabes[variableName].Value;
                    variableNamesToFetch.Remove(variableName);
                }
                else if (usedVariablesCache.ContainsKey(variableName))
                {
                    requestedVariables[variableName] = usedVariablesCache[variableName].Value;
                    variableNamesToFetch.Remove(variableName);
                }
            }

            if (fetchAllVariables)
            {
                // getVariables() will go up the execution hierarchy, no need to do
                // it here also, the cached values will already be applied too
                IDictionary<string, object> allVariables = Variables;
                foreach (string variableName in variableNamesToFetch)
                {
                    requestedVariables[variableName] = allVariables[variableName];
                }
                return requestedVariables;

            }
            else
            {
                // Go up if needed
                IVariableScope parent = ParentVariableScope;
                if (parent != null)
                {
                    requestedVariables.PutAll(parent.GetVariables(variableNamesToFetch, fetchAllVariables));
                }

                // Fetch variables on this scope
                IList<IVariableInstanceEntity> variables = GetSpecificVariables(variableNamesToFetch);
                foreach (IVariableInstanceEntity variable in variables)
                {
                    requestedVariables[variable.Name] = variable.Value;
                }

                return requestedVariables;
            }
        }

        public virtual IDictionary<string, IVariableInstance> GetVariableInstances(IEnumerable<string> variableNames, bool fetchAllVariables)
        {
            IDictionary<string, IVariableInstance> requestedVariables = new Dictionary<string, IVariableInstance>();
            ISet<string> variableNamesToFetch = new HashSet<string>(variableNames);

            // The values in the fetch-cache will be more recent, so they can override any existing ones
            foreach (string variableName in variableNames)
            {
                if (transientVariabes != null && transientVariabes.ContainsKey(variableName))
                {
                    requestedVariables[variableName] = transientVariabes[variableName];
                    variableNamesToFetch.Remove(variableName);
                }
                else if (usedVariablesCache.ContainsKey(variableName))
                {
                    requestedVariables[variableName] = usedVariablesCache[variableName];
                    variableNamesToFetch.Remove(variableName);
                }
            }

            if (fetchAllVariables)
            {
                // getVariables() will go up the execution hierarchy, no need to do it here
                // also, the cached values will already be applied too
                IDictionary<string, IVariableInstance> allVariables = VariableInstances;
                foreach (string variableName in variableNamesToFetch)
                {
                    requestedVariables[variableName] = allVariables[variableName];
                }
                return requestedVariables;

            }
            else
            {

                // Go up if needed
                IVariableScope parent = ParentVariableScope;
                if (parent != null)
                {
                    requestedVariables.PutAll(parent.GetVariableInstances(variableNamesToFetch, fetchAllVariables));
                }

                // Fetch variables on this scope
                IList<IVariableInstanceEntity> variables = GetSpecificVariables(variableNamesToFetch);
                foreach (IVariableInstanceEntity variable in variables)
                {
                    requestedVariables[variable.Name] = variable;
                }

                return requestedVariables;

            }

        }

        protected internal virtual IDictionary<string, object> CollectVariables(Dictionary<string, object> variables)
        {
            EnsureVariableInstancesInitialized();
            VariableScopeImpl parentScope = ParentVariableScope;
            if (parentScope != null)
            {
                variables.PutAll(parentScope.CollectVariables(variables));
            }

            foreach (IVariableInstanceEntity variableInstance in variableInstances.Values)
            {
                variables[variableInstance.Name] = variableInstance.Value;
            }

            foreach (string variableName in usedVariablesCache.Keys)
            {
                variables[variableName] = usedVariablesCache[variableName].Value;
            }

            if (transientVariabes != null)
            {
                foreach (string variableName in transientVariabes.Keys)
                {
                    variables[variableName] = transientVariabes[variableName].Value;
                }
            }

            return variables;
        }

        protected internal virtual IDictionary<string, IVariableInstance> CollectVariableInstances(Dictionary<string, IVariableInstance> variables)
        {
            EnsureVariableInstancesInitialized();
            VariableScopeImpl parentScope = ParentVariableScope;
            if (parentScope != null)
            {
                variables.PutAll(parentScope.CollectVariableInstances(variables));
            }

            foreach (IVariableInstance variableInstance in variableInstances.Values)
            {
                variables[variableInstance.Name] = variableInstance;
            }

            foreach (string variableName in usedVariablesCache.Keys)
            {
                variables[variableName] = usedVariablesCache[variableName];
            }

            if (transientVariabes != null)
            {
                variables.PutAll(transientVariabes);
            }

            return variables;
        }

        public virtual object GetVariable(string variableName)
        {
            return GetVariable(variableName, true);
        }

        public virtual IVariableInstance GetVariableInstance(string variableName)
        {
            return GetVariableInstance(variableName, true);
        }

        /// <summary>
        /// The same operation as <seealso cref="VariableScopeImpl#getVariable(String)"/>, 
        /// but with an extra parameter to indicate whether or not all variables need to be fetched.
        /// 
        /// Note that the default Activiti way (because of backwards compatibility) is to fetch all the variables 
        /// when doing a get/set of variables. So this means 'true' is the default value for this method,
        /// and in fact it will simply delegate to <seealso cref="#getVariable(String)"/>. 
        /// This can also be the most performant, if you're doing a lot of variable gets in the same transaction (eg in service tasks).
        /// 
        /// In case 'false' is used, only the specific variable will be fetched.
        /// </summary>
        public virtual object GetVariable(string variableName, bool fetchAllVariables)
        {
            object value = null;
            IVariableInstance variable = GetVariableInstance(variableName, fetchAllVariables);
            if (variable != null)
            {
                value = variable.Value;
            }
            return value;
        }

        public virtual IVariableInstance GetVariableInstance(string variableName, bool fetchAllVariables)
        {
            // Transient variable
            if (transientVariabes != null && transientVariabes.ContainsKey(variableName))
            {
                return transientVariabes[variableName];
            }

            // Check the local single-fetch cache
            if (usedVariablesCache.ContainsKey(variableName))
            {
                return usedVariablesCache[variableName];
            }

            if (fetchAllVariables == true)
            {
                EnsureVariableInstancesInitialized();
                variableInstances.TryGetValue(variableName, out IVariableInstanceEntity variableInstance);
                if (variableInstance != null)
                {
                    return variableInstance;
                }

                // Go up the hierarchy
                IVariableScope parentScope = ParentVariableScope;
                if (parentScope != null)
                {
                    return parentScope.GetVariableInstance(variableName, true);
                }

                return null;
            }
            else
            {

                if (variableInstances != null && variableInstances.ContainsKey(variableName))
                {
                    return variableInstances[variableName];
                }

                IVariableInstanceEntity variable = GetSpecificVariable(variableName);
                if (variable != null)
                {
                    usedVariablesCache[variableName] = variable;
                    return variable;
                }

                // Go up the hierarchy
                IVariableScope parentScope = ParentVariableScope;
                if (parentScope != null)
                {
                    return parentScope.GetVariableInstance(variableName, false);
                }

                return null;
            }
        }

        protected internal abstract IVariableInstanceEntity GetSpecificVariable(string variableName);

        public virtual object GetVariableLocal(string variableName)
        {
            return GetVariableLocal(variableName, true);
        }

        public virtual IVariableInstance GetVariableInstanceLocal(string variableName)
        {
            return GetVariableInstanceLocal(variableName, true);
        }

        public virtual object GetVariableLocal(string variableName, bool fetchAllVariables)
        {
            object value = null;
            IVariableInstance variable = GetVariableInstanceLocal(variableName, fetchAllVariables);
            if (variable != null)
            {
                value = variable.Value;
            }
            return value;
        }

        public virtual IVariableInstance GetVariableInstanceLocal(string variableName, bool fetchAllVariables)
        {

            if (transientVariabes != null && transientVariabes.ContainsKey(variableName))
            {
                return transientVariabes[variableName];
            }

            if (usedVariablesCache.ContainsKey(variableName))
            {
                return usedVariablesCache[variableName];
            }

            if (fetchAllVariables == true)
            {

                EnsureVariableInstancesInitialized();

                variableInstances.TryGetValue(variableName, out IVariableInstanceEntity variableInstance);
                if (variableInstance != null)
                {
                    return variableInstance;
                }
                return null;

            }
            else
            {

                if (variableInstances != null && variableInstances.ContainsKey(variableName))
                {
                    IVariableInstanceEntity v = variableInstances[variableName];
                    if (v != null)
                    {
                        return variableInstances[variableName];
                    }
                }

                IVariableInstanceEntity variable = GetSpecificVariable(variableName);
                if (variable != null)
                {
                    usedVariablesCache[variableName] = variable;
                    return variable;
                }

                return null;
            }
        }

        public virtual bool HasVariables()
        {
            if (transientVariabes != null && transientVariabes.Count > 0)
            {
                return true;
            }

            EnsureVariableInstancesInitialized();
            if (variableInstances.Count > 0)
            {
                return true;
            }
            IVariableScope parentScope = ParentVariableScope;
            if (parentScope != null)
            {
                return parentScope.HasVariables();
            }
            return false;
        }

        public virtual bool HasVariablesLocal()
        {
            if (transientVariabes != null && transientVariabes.Count > 0)
            {
                return true;
            }
            EnsureVariableInstancesInitialized();
            return variableInstances.Count > 0;
        }

        public virtual bool HasVariable(string variableName)
        {
            if (HasVariableLocal(variableName))
            {
                return true;
            }
            IVariableScope parentScope = ParentVariableScope;
            if (parentScope != null)
            {
                return parentScope.HasVariable(variableName);
            }
            return false;
        }

        public virtual bool HasVariableLocal(string variableName)
        {
            if (transientVariabes != null && transientVariabes.ContainsKey(variableName))
            {
                return true;
            }
            EnsureVariableInstancesInitialized();
            return variableInstances.ContainsKey(variableName);
        }

        protected internal virtual ISet<string> CollectVariableNames(ISet<string> variableNames)
        {
            if (transientVariabes != null)
            {
                transientVariabes.Keys.ToList().ForEach(x =>
                {
                    variableNames.Add(x);
                });
            }

            EnsureVariableInstancesInitialized();
            VariableScopeImpl parentScope = ParentVariableScope;
            if (parentScope != null)
            {
                parentScope.CollectVariableNames(variableNames).ToList().ForEach(x =>
                {
                    variableNames.Add(x);
                });
            }
            foreach (IVariableInstanceEntity variableInstance in variableInstances.Values)
            {
                variableNames.Add(variableInstance.Name);
            }
            return variableNames;
        }

        [JsonIgnore]
        public virtual ISet<string> VariableNames
        {
            get
            {
                return CollectVariableNames(new HashSet<string>());
            }
        }

        [JsonIgnore]
        public virtual IDictionary<string, object> VariablesLocal
        {
            get
            {
                IDictionary<string, object> variables = new Dictionary<string, object>();
                EnsureVariableInstancesInitialized();
                foreach (IVariableInstanceEntity variableInstance in variableInstances.Values)
                {
                    variables[variableInstance.Name] = variableInstance.Value;
                }
                foreach (string variableName in usedVariablesCache.Keys)
                {
                    variables[variableName] = usedVariablesCache[variableName].Value;
                }
                if (transientVariabes != null)
                {
                    foreach (string variableName in transientVariabes.Keys)
                    {
                        variables[variableName] = transientVariabes[variableName].Value;
                    }
                }
                return variables;
            }
            set
            {
                if (value != null)
                {
                    foreach (string variableName in value.Keys)
                    {
                        SetVariableLocal(variableName, value[variableName]);
                    }
                }
            }
        }

        [JsonIgnore]
        public virtual IDictionary<string, IVariableInstance> VariableInstancesLocal
        {
            get
            {
                IDictionary<string, IVariableInstance> variables = new Dictionary<string, IVariableInstance>();
                EnsureVariableInstancesInitialized();
                foreach (IVariableInstanceEntity variableInstance in variableInstances.Values)
                {
                    variables[variableInstance.Name] = variableInstance;
                }
                foreach (string variableName in usedVariablesCache.Keys)
                {
                    variables[variableName] = usedVariablesCache[variableName];
                }
                if (transientVariabes != null)
                {
                    variables.PutAll(transientVariabes);
                }
                return variables;
            }
        }

        public virtual IDictionary<string, object> GetVariablesLocal(IEnumerable<string> variableNames)
        {
            return GetVariablesLocal(variableNames, true);
        }

        public virtual IDictionary<string, IVariableInstance> GetVariableInstancesLocal(IEnumerable<string> variableNames)
        {
            return GetVariableInstancesLocal(variableNames, true);
        }

        public virtual IDictionary<string, object> GetVariablesLocal(IEnumerable<string> variableNames, bool fetchAllVariables)
        {
            IDictionary<string, object> requestedVariables = new Dictionary<string, object>();

            // The values in the fetch-cache will be more recent, so they can override any existing ones
            ISet<string> variableNamesToFetch = new HashSet<string>(variableNames);
            foreach (string variableName in variableNames)
            {
                if (transientVariabes != null && transientVariabes.ContainsKey(variableName))
                {
                    requestedVariables[variableName] = transientVariabes[variableName].Value;
                    variableNamesToFetch.Remove(variableName);
                }
                else if (usedVariablesCache.ContainsKey(variableName))
                {
                    requestedVariables[variableName] = usedVariablesCache[variableName].Value;
                    variableNamesToFetch.Remove(variableName);
                }
            }

            if (fetchAllVariables == true)
            {

                IDictionary<string, object> allVariables = VariablesLocal;
                foreach (string variableName in variableNamesToFetch)
                {
                    requestedVariables[variableName] = allVariables[variableName];
                }

            }
            else
            {

                IList<IVariableInstanceEntity> variables = GetSpecificVariables(variableNamesToFetch);
                foreach (IVariableInstanceEntity variable in variables)
                {
                    requestedVariables[variable.Name] = variable.Value;
                }

            }

            return requestedVariables;
        }

        public virtual IDictionary<string, IVariableInstance> GetVariableInstancesLocal(IEnumerable<string> variableNames, bool fetchAllVariables)
        {
            IDictionary<string, IVariableInstance> requestedVariables = new Dictionary<string, IVariableInstance>();

            // The values in the fetch-cache will be more recent, so they can override any existing ones
            ISet<string> variableNamesToFetch = new HashSet<string>(variableNames);
            foreach (string variableName in variableNames)
            {
                if (transientVariabes != null && transientVariabes.ContainsKey(variableName))
                {
                    requestedVariables[variableName] = transientVariabes[variableName];
                    variableNamesToFetch.Remove(variableName);
                }
                else if (usedVariablesCache.ContainsKey(variableName))
                {
                    requestedVariables[variableName] = usedVariablesCache[variableName];
                    variableNamesToFetch.Remove(variableName);
                }
            }

            if (fetchAllVariables == true)
            {

                IDictionary<string, IVariableInstance> allVariables = VariableInstancesLocal;
                foreach (string variableName in variableNamesToFetch)
                {
                    requestedVariables[variableName] = allVariables[variableName];
                }
            }
            else
            {

                IList<IVariableInstanceEntity> variables = GetSpecificVariables(variableNamesToFetch);
                foreach (IVariableInstanceEntity variable in variables)
                {
                    requestedVariables[variable.Name] = variable;
                }
            }

            return requestedVariables;
        }

        protected internal abstract IList<IVariableInstanceEntity> GetSpecificVariables(IEnumerable<string> variableNames);

        [JsonIgnore]
        public virtual ISet<string> VariableNamesLocal
        {
            get
            {
                ISet<string> variableNames = new HashSet<string>();
                if (transientVariabes != null)
                {
                    transientVariabes.Keys.ToList().ForEach(x =>
                    {
                        variableNames.Add(x);
                    });
                }
                EnsureVariableInstancesInitialized();
                variableInstances.Keys.ToList().ForEach(x =>
                {
                    variableNames.Add(x);
                });
                return variableNames;
            }
        }

        [JsonIgnore]
        public virtual IDictionary<string, IVariableInstanceEntity> VariableInstanceEntities
        {
            get
            {
                EnsureVariableInstancesInitialized();
                return new ReadOnlyDictionary<string, IVariableInstanceEntity>(variableInstances);
            }
        }

        public virtual IDictionary<string, IVariableInstanceEntity> UsedVariablesCache
        {
            get
            {
                return usedVariablesCache;
            }
        }

        public virtual void CreateVariablesLocal<T1>(IDictionary<string, T1> variables)
        {
            if (variables != null)
            {
                foreach (var entry in variables)
                {
                    CreateVariableLocal(entry.Key, entry.Value);
                }
            }
        }

        public virtual void RemoveVariables()
        {
            EnsureVariableInstancesInitialized();
            ISet<string> variableNames = new HashSet<string>(variableInstances.Keys);
            foreach (string variableName in variableNames)
            {
                RemoveVariable(variableName);
            }
        }

        public virtual void RemoveVariablesLocal()
        {
            IList<string> variableNames = new List<string>(VariableNamesLocal);
            foreach (string variableName in variableNames)
            {
                RemoveVariableLocal(variableName);
            }
        }

        public virtual void RemoveVariables(IEnumerable<string> variableNames)
        {
            if (variableNames != null)
            {
                foreach (string variableName in variableNames)
                {
                    RemoveVariable(variableName);
                }
            }
        }

        public virtual void RemoveVariablesLocal(IEnumerable<string> variableNames)
        {
            if (variableNames != null)
            {
                foreach (string variableName in variableNames)
                {
                    RemoveVariableLocal(variableName);
                }
            }
        }

        public virtual void SetVariable(string variableName, object value)
        {
            SetVariable(variableName, value, SourceActivityExecution, true);
        }

        /// <summary>
        /// The default <seealso cref="#setVariable(String, Object)"/> fetches all variables 
        /// (for historical and backwards compatible reasons) while setting the variables.
        /// 
        /// Setting the fetchAllVariables parameter to true is the default behaviour 
        /// (ie fetching all variables) Setting the fetchAllVariables parameter to false does not do that.
        /// 
        /// </summary>
        public virtual void SetVariable(string variableName, object value, bool fetchAllVariables)
        {
            SetVariable(variableName, value, SourceActivityExecution, fetchAllVariables);
        }

        /// <summary>
        /// Sets a variable as high as possible (highest parent).
        /// </summary>
        ///  <param name="sourceExecution"> The execution where the variable was originally set, used for history data. </param>
        ///  <param name="fetchAllVariables"> If true, all existing variables will be fetched when setting the variable. </param>
        protected internal virtual void SetVariable(string variableName, object value, IExecutionEntity sourceExecution, bool fetchAllVariables)
        {
            if (fetchAllVariables == true)
            {

                // If it's in the cache, it's more recent
                if (usedVariablesCache.ContainsKey(variableName))
                {
                    UpdateVariableInstance(usedVariablesCache[variableName], value, sourceExecution);
                }

                // If the variable exists on this scope, replace it
                if (HasVariableLocal(variableName))
                {
                    SetVariableLocal(variableName, value, sourceExecution, true);
                    return;
                }

                // Otherwise, go up the hierarchy (we're trying to put it as high as possible)
                VariableScopeImpl parentVariableScope = ParentVariableScope;
                if (parentVariableScope != null)
                {
                    if (sourceExecution == null)
                    {
                        parentVariableScope.SetVariable(variableName, value);
                    }
                    else
                    {
                        parentVariableScope.SetVariable(variableName, value, sourceExecution, true);
                    }
                    return;
                }

                // We're as high as possible and the variable doesn't exist yet, so
                // we're creating it
                if (sourceExecution != null)
                {
                    CreateVariableLocal(variableName, value, sourceExecution);
                }
                else
                {
                    CreateVariableLocal(variableName, value);
                }

            }
            else
            {

                // Check local cache first
                if (usedVariablesCache.ContainsKey(variableName))
                {

                    UpdateVariableInstance(usedVariablesCache[variableName], value, sourceExecution);

                }
                else if (variableInstances != null && variableInstances.ContainsKey(variableName))
                {

                    UpdateVariableInstance(variableInstances[variableName], value, sourceExecution);

                }
                else
                {
                    // Not in local cache, check if defined on this scope
                    // Create it if it doesn't exist yet
                    IVariableInstanceEntity variable = GetSpecificVariable(variableName);
                    if (variable != null)
                    {
                        UpdateVariableInstance(variable, value, sourceExecution);
                        usedVariablesCache[variableName] = variable;
                    }
                    else
                    {

                        VariableScopeImpl parent = ParentVariableScope;
                        if (parent != null)
                        {
                            if (sourceExecution == null)
                            {
                                parent.SetVariable(variableName, value, fetchAllVariables);
                            }
                            else
                            {
                                parent.SetVariable(variableName, value, sourceExecution, fetchAllVariables);
                            }

                            return;
                        }

                        variable = CreateVariableInstance(variableName, value, sourceExecution);
                        usedVariablesCache[variableName] = variable;

                    }

                }

            }

        }

        public virtual object SetVariableLocal(string variableName, object value)
        {
            return SetVariableLocal(variableName, value, SourceActivityExecution, true);
        }

        /// <summary>
        /// The default <seealso cref="#setVariableLocal(String, Object)"/> fetches all variables (for historical and backwards compatible reasons) while setting the variables.
        /// 
        /// Setting the fetchAllVariables parameter to true is the default behaviour (ie fetching all variables) Setting the fetchAllVariables parameter to false does not do that.
        /// 
        /// </summary>
        public virtual object SetVariableLocal(string variableName, object value, bool fetchAllVariables)
        {
            return SetVariableLocal(variableName, value, SourceActivityExecution, fetchAllVariables);
        }

        public virtual object SetVariableLocal(string variableName, object value, IExecutionEntity sourceActivityExecution, bool fetchAllVariables)
        {

            if (fetchAllVariables == true)
            {

                // If it's in the cache, it's more recent
                if (usedVariablesCache.ContainsKey(variableName))
                {
                    UpdateVariableInstance(usedVariablesCache[variableName], value, sourceActivityExecution);
                }

                EnsureVariableInstancesInitialized();

                variableInstances.TryGetValue(variableName, out IVariableInstanceEntity variableInstance);
                if (variableInstance == null)
                {
                    usedVariablesCache.TryGetValue(variableName, out variableInstance);
                }

                if (variableInstance == null)
                {
                    CreateVariableLocal(variableName, value);
                }
                else
                {
                    UpdateVariableInstance(variableInstance, value, sourceActivityExecution);
                }

                return null;
            }
            else
            {

                if (usedVariablesCache.ContainsKey(variableName))
                {
                    UpdateVariableInstance(usedVariablesCache[variableName], value, sourceActivityExecution);
                }
                else if (variableInstances != null && variableInstances.ContainsKey(variableName))
                {
                    UpdateVariableInstance(variableInstances[variableName], value, sourceActivityExecution);
                }
                else
                {

                    IVariableInstanceEntity variable = GetSpecificVariable(variableName);
                    if (variable != null)
                    {
                        UpdateVariableInstance(variable, value, sourceActivityExecution);
                    }
                    else
                    {
                        variable = CreateVariableInstance(variableName, value, sourceActivityExecution);
                    }
                    usedVariablesCache[variableName] = variable;

                }

                return null;

            }
        }

        public virtual void CreateVariableLocal(string variableName, object value)
        {
            CreateVariableLocal(variableName, value, SourceActivityExecution);
        }

        /// <summary>
        /// only called when a new variable is created on this variable scope. This method is also responsible for propagating the creation of this variable to the history.
        /// </summary>
        protected internal virtual void CreateVariableLocal(string variableName, object value, IExecutionEntity sourceActivityExecution)
        {
            EnsureVariableInstancesInitialized();

            if (variableInstances.ContainsKey(variableName))
            {
                throw new ActivitiException("variable '" + variableName + "' already exists. Use setVariableLocal if you want to overwrite the value");
            }

            CreateVariableInstance(variableName, value, sourceActivityExecution);
        }

        public virtual void RemoveVariable(string variableName)
        {
            RemoveVariable(variableName, SourceActivityExecution);
        }

        protected internal virtual void RemoveVariable(string variableName, IExecutionEntity sourceActivityExecution)
        {
            EnsureVariableInstancesInitialized();
            if (variableInstances.ContainsKey(variableName))
            {
                RemoveVariableLocal(variableName);
                return;
            }
            VariableScopeImpl parentVariableScope = ParentVariableScope;
            if (parentVariableScope != null)
            {
                if (sourceActivityExecution == null)
                {
                    parentVariableScope.RemoveVariable(variableName);
                }
                else
                {
                    parentVariableScope.RemoveVariable(variableName, sourceActivityExecution);
                }
            }
        }

        public virtual void RemoveVariableLocal(string variableName)
        {
            RemoveVariableLocal(variableName, SourceActivityExecution);
        }

        protected internal virtual IExecutionEntity SourceActivityExecution
        {
            get
            {
                return null;
            }
        }

        protected internal virtual void RemoveVariableLocal(string variableName, IExecutionEntity sourceActivityExecution)
        {
            EnsureVariableInstancesInitialized();
            variableInstances.TryGetValue(variableName, out IVariableInstanceEntity variableInstance);
            variableInstances.Remove(variableName);
            if (variableInstance != null)
            {
                DeleteVariableInstanceForExplicitUserCall(variableInstance, sourceActivityExecution);
            }
        }

        protected internal virtual void DeleteVariableInstanceForExplicitUserCall(IVariableInstanceEntity variableInstance, IExecutionEntity sourceActivityExecution)
        {
            Context.CommandContext.VariableInstanceEntityManager.Delete(variableInstance);
            variableInstance.Value = null;

            // Record historic variable deletion
            Context.CommandContext.HistoryManager.RecordVariableRemoved(variableInstance);

            // Record historic detail
            Context.CommandContext.HistoryManager.RecordHistoricDetailVariableCreate(variableInstance, sourceActivityExecution, ActivityIdUsedForDetails);
        }

        protected internal virtual void UpdateVariableInstance(IVariableInstanceEntity variableInstance, object value, IExecutionEntity sourceActivityExecution)
        {

            // Always check if the type should be altered. It's possible that the
            // previous type is lower in the type
            // checking chain (e.g. serializable) and will return true on
            // isAbleToStore(), even though another type
            // higher in the chain is eligible for storage.

            IVariableTypes variableTypes = Context.ProcessEngineConfiguration.VariableTypes;

            IVariableType newType = variableTypes.FindVariableType(value);

            if (newType != null && !newType.Equals(variableInstance.Type))
            {
                variableInstance.Value = null;
                variableInstance.Type = newType;
                variableInstance.ForceUpdate();
                variableInstance.Value = value;
            }
            else
            {
                variableInstance.Value = value;
            }

            Context.CommandContext.HistoryManager.RecordHistoricDetailVariableCreate(variableInstance, sourceActivityExecution, ActivityIdUsedForDetails);

            Context.CommandContext.HistoryManager.RecordVariableUpdate(variableInstance);
        }

        protected internal virtual IVariableInstanceEntity CreateVariableInstance(string variableName, object value, IExecutionEntity sourceActivityExecution)
        {
            IVariableTypes variableTypes = Context.ProcessEngineConfiguration.VariableTypes;

            IVariableType type = variableTypes.FindVariableType(value);

            IVariableInstanceEntity variableInstance = Context.CommandContext.VariableInstanceEntityManager.Create(variableName, type, value);
            InitializeVariableInstanceBackPointer(variableInstance);
            Context.CommandContext.VariableInstanceEntityManager.Insert(variableInstance);

            if (variableInstances != null)
            {
                variableInstances[variableName] = variableInstance;
            }

            // Record historic variable
            Context.CommandContext.HistoryManager.RecordVariableCreate(variableInstance);

            // Record historic detail
            Context.CommandContext.HistoryManager.RecordHistoricDetailVariableCreate(variableInstance, sourceActivityExecution, ActivityIdUsedForDetails);

            return variableInstance;
        }


        /*
         * Transient variables
         */

        public virtual IDictionary<string, object> TransientVariablesLocal
        {
            set
            {
                foreach (string variableName in value.Keys)
                {
                    SetTransientVariableLocal(variableName, value[variableName]);
                }
            }
            get
            {
                if (transientVariabes != null)
                {
                    IDictionary<string, object> variables = new Dictionary<string, object>();
                    foreach (string variableName in transientVariabes.Keys)
                    {
                        variables[variableName] = transientVariabes[variableName].Value;
                    }
                    return variables;
                }
                else
                {
                    return new Dictionary<string, object>();
                }
            }
        }

        public virtual void SetTransientVariableLocal(string variableName, object variableValue)
        {
            if (transientVariabes == null)
            {
                transientVariabes = new Dictionary<string, IVariableInstance>();
            }
            transientVariabes[variableName] = new TransientVariableInstance(variableName, variableValue);
        }

        public virtual IDictionary<string, object> TransientVariables
        {
            set
            {
                foreach (string variableName in value.Keys)
                {
                    SetTransientVariable(variableName, value[variableName]);
                }
            }
            get
            {
                return CollectTransientVariables(new Dictionary<string, object>());
            }
        }

        public virtual void SetTransientVariable(string variableName, object variableValue)
        {
            VariableScopeImpl parentVariableScope = ParentVariableScope;
            if (parentVariableScope != null)
            {
                parentVariableScope.SetTransientVariable(variableName, variableValue);
                return;
            }
            SetTransientVariableLocal(variableName, variableValue);
        }

        public virtual object GetTransientVariableLocal(string variableName)
        {
            if (transientVariabes != null)
            {
                return transientVariabes[variableName].Value;
            }
            return null;
        }


        public virtual object GetTransientVariable(string variableName)
        {
            if (transientVariabes != null && transientVariabes.ContainsKey(variableName))
            {
                return transientVariabes[variableName].Value;
            }

            VariableScopeImpl parentScope = ParentVariableScope;
            if (parentScope != null)
            {
                return parentScope.GetTransientVariable(variableName);
            }

            return null;
        }


        protected internal virtual IDictionary<string, object> CollectTransientVariables(Dictionary<string, object> variables)
        {
            VariableScopeImpl parentScope = ParentVariableScope;
            if (parentScope != null)
            {
                variables.PutAll(parentScope.CollectVariables(variables));
            }

            if (transientVariabes != null)
            {
                foreach (string variableName in transientVariabes.Keys)
                {
                    variables[variableName] = transientVariabes[variableName].Value;
                }
            }

            return variables;
        }

        public virtual void RemoveTransientVariableLocal(string variableName)
        {
            if (transientVariabes != null)
            {
                transientVariabes.Remove(variableName);
            }
        }

        public virtual void RemoveTransientVariablesLocal()
        {
            if (transientVariabes != null)
            {
                transientVariabes.Clear();
            }
        }

        public virtual void RemoveTransientVariable(string variableName)
        {
            if (transientVariabes != null && transientVariabes.ContainsKey(variableName))
            {
                RemoveTransientVariableLocal(variableName);
                return;
            }
            VariableScopeImpl parentVariableScope = ParentVariableScope;
            if (parentVariableScope != null)
            {
                parentVariableScope.RemoveTransientVariable(variableName);
            }
        }

        public virtual void RemoveTransientVariables()
        {
            RemoveTransientVariablesLocal();
            VariableScopeImpl parentVariableScope = ParentVariableScope;
            if (parentVariableScope != null)
            {
                parentVariableScope.RemoveTransientVariablesLocal();
            }
        }

        /// <summary>
        /// Execution variable updates have activity instance ids, but historic task variable updates don't.
        /// </summary>
        protected internal virtual bool ActivityIdUsedForDetails
        {
            get
            {
                return true;
            }
        }

        // getters and setters
        // //////////////////////////////////////////////////////

        public virtual ELContext CachedElContext
        {
            get
            {
                return cachedElContext;
            }
            set
            {
                this.cachedElContext = value;
            }
        }


        public virtual T GetVariable<T>(string variableName)
        {
            return (T)GetVariable(variableName);
        }

        public virtual T GetVariableLocal<T>(string variableName)
        {
            return (T)GetVariableLocal(variableName);
        }
    }

}