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
namespace org.activiti.engine.impl.persistence.entity
{
    using Newtonsoft.Json;
    using org.activiti.engine.@delegate;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.el;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.variable;
    using Sys;
    using Sys.Bpm;
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
        protected internal IDictionary<string, IVariableInstanceEntity> variableInstances; // needs to be null, the logic depends on it for checking if vars were already fetched

        // The cache is used when fetching/setting specific variables
        protected internal IDictionary<string, IVariableInstanceEntity> usedVariablesCache = new Dictionary<string, IVariableInstanceEntity>();

        protected internal IDictionary<string, IVariableInstance> transientVariabes;

        protected internal ELContext cachedElContext;

        protected internal abstract IList<IVariableInstanceEntity> loadVariableInstances();

        protected internal abstract VariableScopeImpl ParentVariableScope { get; }

        protected internal abstract void initializeVariableInstanceBackPointer(IVariableInstanceEntity variableInstance);

        protected internal virtual void ensureVariableInstancesInitialized()
        {
            if (variableInstances == null)
            {
                variableInstances = new Dictionary<string, IVariableInstanceEntity>();

                ICommandContext commandContext = Context.CommandContext;
                if (commandContext == null)
                {
                    throw new ActivitiException("lazy loading outside command context");
                }
                ICollection<IVariableInstanceEntity> variableInstancesList = loadVariableInstances();
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
                return collectVariables(new Dictionary<string, object>());
            }
            set
            {
                if (value != null)
                {
                    foreach (string variableName in value.Keys)
                    {
                        setVariable(variableName, value[variableName]);
                    }
                }
            }
        }

        [JsonIgnore]
        public virtual IDictionary<string, IVariableInstance> VariableInstances
        {
            get
            {
                return collectVariableInstances(new Dictionary<string, IVariableInstance>());
            }
        }

        public virtual IDictionary<string, object> getVariables(ICollection<string> variableNames)
        {
            return getVariables(variableNames, true);
        }

        public virtual IDictionary<string, IVariableInstance> getVariableInstances(ICollection<string> variableNames)
        {
            return getVariableInstances(variableNames, true);
        }

        public virtual IDictionary<string, object> getVariables(ICollection<string> variableNames, bool fetchAllVariables)
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
                    requestedVariables.putAll(parent.getVariables(variableNamesToFetch, fetchAllVariables));
                }

                // Fetch variables on this scope
                IList<IVariableInstanceEntity> variables = getSpecificVariables(variableNamesToFetch);
                foreach (IVariableInstanceEntity variable in variables)
                {
                    requestedVariables[variable.Name] = variable.Value;
                }

                return requestedVariables;

            }

        }

        public virtual IDictionary<string, IVariableInstance> getVariableInstances(ICollection<string> variableNames, bool fetchAllVariables)
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
                    requestedVariables.putAll(parent.getVariableInstances(variableNamesToFetch, fetchAllVariables));
                }

                // Fetch variables on this scope
                IList<IVariableInstanceEntity> variables = getSpecificVariables(variableNamesToFetch);
                foreach (IVariableInstanceEntity variable in variables)
                {
                    requestedVariables[variable.Name] = variable;
                }

                return requestedVariables;

            }

        }

        protected internal virtual IDictionary<string, object> collectVariables(Dictionary<string, object> variables)
        {
            ensureVariableInstancesInitialized();
            VariableScopeImpl parentScope = ParentVariableScope;
            if (parentScope != null)
            {
                variables.putAll(parentScope.collectVariables(variables));
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

        protected internal virtual IDictionary<string, IVariableInstance> collectVariableInstances(Dictionary<string, IVariableInstance> variables)
        {
            ensureVariableInstancesInitialized();
            VariableScopeImpl parentScope = ParentVariableScope;
            if (parentScope != null)
            {
                variables.putAll(parentScope.collectVariableInstances(variables));
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
                variables.putAll(transientVariabes);
            }

            return variables;
        }

        public virtual object getVariable(string variableName)
        {
            return getVariable(variableName, true);
        }

        public virtual IVariableInstance getVariableInstance(string variableName)
        {
            return getVariableInstance(variableName, true);
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
        public virtual object getVariable(string variableName, bool fetchAllVariables)
        {
            object value = null;
            IVariableInstance variable = getVariableInstance(variableName, fetchAllVariables);
            if (variable != null)
            {
                value = variable.Value;
            }
            return value;
        }

        public virtual IVariableInstance getVariableInstance(string variableName, bool fetchAllVariables)
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
                ensureVariableInstancesInitialized();
                variableInstances.TryGetValue(variableName, out IVariableInstanceEntity variableInstance);
                if (variableInstance != null)
                {
                    return variableInstance;
                }

                // Go up the hierarchy
                IVariableScope parentScope = ParentVariableScope;
                if (parentScope != null)
                {
                    return parentScope.getVariableInstance(variableName, true);
                }

                return null;

            }
            else
            {

                if (variableInstances != null && variableInstances.ContainsKey(variableName))
                {
                    return variableInstances[variableName];
                }

                IVariableInstanceEntity variable = getSpecificVariable(variableName);
                if (variable != null)
                {
                    usedVariablesCache[variableName] = variable;
                    return variable;
                }

                // Go up the hierarchy
                IVariableScope parentScope = ParentVariableScope;
                if (parentScope != null)
                {
                    return parentScope.getVariableInstance(variableName, false);
                }

                return null;

            }
        }

        protected internal abstract IVariableInstanceEntity getSpecificVariable(string variableName);

        public virtual object getVariableLocal(string variableName)
        {
            return getVariableLocal(variableName, true);
        }

        public virtual IVariableInstance getVariableInstanceLocal(string variableName)
        {
            return getVariableInstanceLocal(variableName, true);
        }

        public virtual object getVariableLocal(string variableName, bool fetchAllVariables)
        {
            object value = null;
            IVariableInstance variable = getVariableInstanceLocal(variableName, fetchAllVariables);
            if (variable != null)
            {
                value = variable.Value;
            }
            return value;
        }

        public virtual IVariableInstance getVariableInstanceLocal(string variableName, bool fetchAllVariables)
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

                ensureVariableInstancesInitialized();

                IVariableInstanceEntity variableInstance = variableInstances[variableName];
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

                IVariableInstanceEntity variable = getSpecificVariable(variableName);
                if (variable != null)
                {
                    usedVariablesCache[variableName] = variable;
                    return variable;
                }

                return null;
            }
        }

        public virtual bool hasVariables()
        {
            if (transientVariabes != null && transientVariabes.Count > 0)
            {
                return true;
            }

            ensureVariableInstancesInitialized();
            if (variableInstances.Count > 0)
            {
                return true;
            }
            IVariableScope parentScope = ParentVariableScope;
            if (parentScope != null)
            {
                return parentScope.hasVariables();
            }
            return false;
        }

        public virtual bool hasVariablesLocal()
        {
            if (transientVariabes != null && transientVariabes.Count > 0)
            {
                return true;
            }
            ensureVariableInstancesInitialized();
            return variableInstances.Count > 0;
        }

        public virtual bool hasVariable(string variableName)
        {
            if (hasVariableLocal(variableName))
            {
                return true;
            }
            IVariableScope parentScope = ParentVariableScope;
            if (parentScope != null)
            {
                return parentScope.hasVariable(variableName);
            }
            return false;
        }

        public virtual bool hasVariableLocal(string variableName)
        {
            if (transientVariabes != null && transientVariabes.ContainsKey(variableName))
            {
                return true;
            }
            ensureVariableInstancesInitialized();
            return variableInstances.ContainsKey(variableName);
        }

        protected internal virtual ISet<string> collectVariableNames(ISet<string> variableNames)
        {
            if (transientVariabes != null)
            {
                transientVariabes.Keys.ToList().ForEach(x =>
                {
                    variableNames.Add(x);
                });
            }

            ensureVariableInstancesInitialized();
            VariableScopeImpl parentScope = ParentVariableScope;
            if (parentScope != null)
            {
                parentScope.collectVariableNames(variableNames).ToList().ForEach(x =>
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
                return collectVariableNames(new HashSet<string>());
            }
        }

        [JsonIgnore]
        public virtual IDictionary<string, object> VariablesLocal
        {
            get
            {
                IDictionary<string, object> variables = new Dictionary<string, object>();
                ensureVariableInstancesInitialized();
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
                        setVariableLocal(variableName, value[variableName]);
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
                ensureVariableInstancesInitialized();
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
                    variables.putAll(transientVariabes);
                }
                return variables;
            }
        }

        public virtual IDictionary<string, object> getVariablesLocal(ICollection<string> variableNames)
        {
            return getVariablesLocal(variableNames, true);
        }

        public virtual IDictionary<string, IVariableInstance> getVariableInstancesLocal(ICollection<string> variableNames)
        {
            return getVariableInstancesLocal(variableNames, true);
        }

        public virtual IDictionary<string, object> getVariablesLocal(ICollection<string> variableNames, bool fetchAllVariables)
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

                IList<IVariableInstanceEntity> variables = getSpecificVariables(variableNamesToFetch);
                foreach (IVariableInstanceEntity variable in variables)
                {
                    requestedVariables[variable.Name] = variable.Value;
                }

            }

            return requestedVariables;
        }

        public virtual IDictionary<string, IVariableInstance> getVariableInstancesLocal(ICollection<string> variableNames, bool fetchAllVariables)
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

                IList<IVariableInstanceEntity> variables = getSpecificVariables(variableNamesToFetch);
                foreach (IVariableInstanceEntity variable in variables)
                {
                    requestedVariables[variable.Name] = variable;
                }

            }

            return requestedVariables;
        }

        protected internal abstract IList<IVariableInstanceEntity> getSpecificVariables(ICollection<string> variableNames);

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
                ensureVariableInstancesInitialized();
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
                ensureVariableInstancesInitialized();
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

        public virtual void createVariablesLocal<T1>(IDictionary<string, T1> variables)
        {
            if (variables != null)
            {
                foreach (var entry in variables)
                {
                    createVariableLocal(entry.Key, entry.Value);
                }
            }
        }



        public virtual void removeVariables()
        {
            ensureVariableInstancesInitialized();
            ISet<string> variableNames = new HashSet<string>(variableInstances.Keys);
            foreach (string variableName in variableNames)
            {
                removeVariable(variableName);
            }
        }

        public virtual void removeVariablesLocal()
        {
            IList<string> variableNames = new List<string>(VariableNamesLocal);
            foreach (string variableName in variableNames)
            {
                removeVariableLocal(variableName);
            }
        }

        public virtual void removeVariables(ICollection<string> variableNames)
        {
            if (variableNames != null)
            {
                foreach (string variableName in variableNames)
                {
                    removeVariable(variableName);
                }
            }
        }

        public virtual void removeVariablesLocal(ICollection<string> variableNames)
        {
            if (variableNames != null)
            {
                foreach (string variableName in variableNames)
                {
                    removeVariableLocal(variableName);
                }
            }
        }

        public virtual void setVariable(string variableName, object value)
        {
            setVariable(variableName, value, SourceActivityExecution, true);
        }

        /// <summary>
        /// The default <seealso cref="#setVariable(String, Object)"/> fetches all variables 
        /// (for historical and backwards compatible reasons) while setting the variables.
        /// 
        /// Setting the fetchAllVariables parameter to true is the default behaviour 
        /// (ie fetching all variables) Setting the fetchAllVariables parameter to false does not do that.
        /// 
        /// </summary>
        public virtual void setVariable(string variableName, object value, bool fetchAllVariables)
        {
            setVariable(variableName, value, SourceActivityExecution, fetchAllVariables);
        }

        /// <summary>
        /// Sets a variable as high as possible (highest parent).
        /// </summary>
        ///  <param name="sourceExecution"> The execution where the variable was originally set, used for history data. </param>
        ///  <param name="fetchAllVariables"> If true, all existing variables will be fetched when setting the variable. </param>
        protected internal virtual void setVariable(string variableName, object value, IExecutionEntity sourceExecution, bool fetchAllVariables)
        {

            if (fetchAllVariables == true)
            {

                // If it's in the cache, it's more recent
                if (usedVariablesCache.ContainsKey(variableName))
                {
                    updateVariableInstance(usedVariablesCache[variableName], value, sourceExecution);
                }

                // If the variable exists on this scope, replace it
                if (hasVariableLocal(variableName))
                {
                    setVariableLocal(variableName, value, sourceExecution, true);
                    return;
                }

                // Otherwise, go up the hierarchy (we're trying to put it as high as possible)
                VariableScopeImpl parentVariableScope = ParentVariableScope;
                if (parentVariableScope != null)
                {
                    if (sourceExecution == null)
                    {
                        parentVariableScope.setVariable(variableName, value);
                    }
                    else
                    {
                        parentVariableScope.setVariable(variableName, value, sourceExecution, true);
                    }
                    return;
                }

                // We're as high as possible and the variable doesn't exist yet, so
                // we're creating it
                if (sourceExecution != null)
                {
                    createVariableLocal(variableName, value, sourceExecution);
                }
                else
                {
                    createVariableLocal(variableName, value);
                }

            }
            else
            {

                // Check local cache first
                if (usedVariablesCache.ContainsKey(variableName))
                {

                    updateVariableInstance(usedVariablesCache[variableName], value, sourceExecution);

                }
                else if (variableInstances != null && variableInstances.ContainsKey(variableName))
                {

                    updateVariableInstance(variableInstances[variableName], value, sourceExecution);

                }
                else
                {

                    // Not in local cache, check if defined on this scope
                    // Create it if it doesn't exist yet
                    IVariableInstanceEntity variable = getSpecificVariable(variableName);
                    if (variable != null)
                    {
                        updateVariableInstance(variable, value, sourceExecution);
                        usedVariablesCache[variableName] = variable;
                    }
                    else
                    {

                        VariableScopeImpl parent = ParentVariableScope;
                        if (parent != null)
                        {
                            if (sourceExecution == null)
                            {
                                parent.setVariable(variableName, value, fetchAllVariables);
                            }
                            else
                            {
                                parent.setVariable(variableName, value, sourceExecution, fetchAllVariables);
                            }

                            return;
                        }

                        variable = createVariableInstance(variableName, value, sourceExecution);
                        usedVariablesCache[variableName] = variable;

                    }

                }

            }

        }

        public virtual object setVariableLocal(string variableName, object value)
        {
            return setVariableLocal(variableName, value, SourceActivityExecution, true);
        }

        /// <summary>
        /// The default <seealso cref="#setVariableLocal(String, Object)"/> fetches all variables (for historical and backwards compatible reasons) while setting the variables.
        /// 
        /// Setting the fetchAllVariables parameter to true is the default behaviour (ie fetching all variables) Setting the fetchAllVariables parameter to false does not do that.
        /// 
        /// </summary>
        public virtual object setVariableLocal(string variableName, object value, bool fetchAllVariables)
        {
            return setVariableLocal(variableName, value, SourceActivityExecution, fetchAllVariables);
        }

        public virtual object setVariableLocal(string variableName, object value, IExecutionEntity sourceActivityExecution, bool fetchAllVariables)
        {

            if (fetchAllVariables == true)
            {

                // If it's in the cache, it's more recent
                if (usedVariablesCache.ContainsKey(variableName))
                {
                    updateVariableInstance(usedVariablesCache[variableName], value, sourceActivityExecution);
                }

                ensureVariableInstancesInitialized();

                IVariableInstanceEntity variableInstance = variableInstances[variableName];
                if (variableInstance == null)
                {
                    variableInstance = usedVariablesCache[variableName];
                }

                if (variableInstance == null)
                {
                    createVariableLocal(variableName, value);
                }
                else
                {
                    updateVariableInstance(variableInstance, value, sourceActivityExecution);
                }

                return null;

            }
            else
            {

                if (usedVariablesCache.ContainsKey(variableName))
                {
                    updateVariableInstance(usedVariablesCache[variableName], value, sourceActivityExecution);
                }
                else if (variableInstances != null && variableInstances.ContainsKey(variableName))
                {
                    updateVariableInstance(variableInstances[variableName], value, sourceActivityExecution);
                }
                else
                {

                    IVariableInstanceEntity variable = getSpecificVariable(variableName);
                    if (variable != null)
                    {
                        updateVariableInstance(variable, value, sourceActivityExecution);
                    }
                    else
                    {
                        variable = createVariableInstance(variableName, value, sourceActivityExecution);
                    }
                    usedVariablesCache[variableName] = variable;

                }

                return null;

            }
        }

        public virtual void createVariableLocal(string variableName, object value)
        {
            createVariableLocal(variableName, value, SourceActivityExecution);
        }

        /// <summary>
        /// only called when a new variable is created on this variable scope. This method is also responsible for propagating the creation of this variable to the history.
        /// </summary>
        protected internal virtual void createVariableLocal(string variableName, object value, IExecutionEntity sourceActivityExecution)
        {
            ensureVariableInstancesInitialized();

            if (variableInstances.ContainsKey(variableName))
            {
                throw new ActivitiException("variable '" + variableName + "' already exists. Use setVariableLocal if you want to overwrite the value");
            }

            createVariableInstance(variableName, value, sourceActivityExecution);
        }

        public virtual void removeVariable(string variableName)
        {
            removeVariable(variableName, SourceActivityExecution);
        }

        protected internal virtual void removeVariable(string variableName, IExecutionEntity sourceActivityExecution)
        {
            ensureVariableInstancesInitialized();
            if (variableInstances.ContainsKey(variableName))
            {
                removeVariableLocal(variableName);
                return;
            }
            VariableScopeImpl parentVariableScope = ParentVariableScope;
            if (parentVariableScope != null)
            {
                if (sourceActivityExecution == null)
                {
                    parentVariableScope.removeVariable(variableName);
                }
                else
                {
                    parentVariableScope.removeVariable(variableName, sourceActivityExecution);
                }
            }
        }

        public virtual void removeVariableLocal(string variableName)
        {
            removeVariableLocal(variableName, SourceActivityExecution);
        }

        protected internal virtual IExecutionEntity SourceActivityExecution
        {
            get
            {
                return null;
            }
        }

        protected internal virtual void removeVariableLocal(string variableName, IExecutionEntity sourceActivityExecution)
        {
            ensureVariableInstancesInitialized();
            IVariableInstanceEntity variableInstance = variableInstances[variableName];
            variableInstances.Remove(variableName);
            if (variableInstance != null)
            {
                deleteVariableInstanceForExplicitUserCall(variableInstance, sourceActivityExecution);
            }
        }

        protected internal virtual void deleteVariableInstanceForExplicitUserCall(IVariableInstanceEntity variableInstance, IExecutionEntity sourceActivityExecution)
        {
            Context.CommandContext.VariableInstanceEntityManager.delete(variableInstance);
            variableInstance.Value = null;

            // Record historic variable deletion
            Context.CommandContext.HistoryManager.recordVariableRemoved(variableInstance);

            // Record historic detail
            Context.CommandContext.HistoryManager.recordHistoricDetailVariableCreate(variableInstance, sourceActivityExecution, ActivityIdUsedForDetails);
        }

        protected internal virtual void updateVariableInstance(IVariableInstanceEntity variableInstance, object value, IExecutionEntity sourceActivityExecution)
        {

            // Always check if the type should be altered. It's possible that the
            // previous type is lower in the type
            // checking chain (e.g. serializable) and will return true on
            // isAbleToStore(), even though another type
            // higher in the chain is eligible for storage.

            IVariableTypes variableTypes = Context.ProcessEngineConfiguration.VariableTypes;

            IVariableType newType = variableTypes.findVariableType(value);

            if (newType != null && !newType.Equals(variableInstance.Type))
            {
                variableInstance.Value = null;
                variableInstance.Type = newType;
                variableInstance.forceUpdate();
                variableInstance.Value = value;
            }
            else
            {
                variableInstance.Value = value;
            }

            Context.CommandContext.HistoryManager.recordHistoricDetailVariableCreate(variableInstance, sourceActivityExecution, ActivityIdUsedForDetails);

            Context.CommandContext.HistoryManager.recordVariableUpdate(variableInstance);
        }

        protected internal virtual IVariableInstanceEntity createVariableInstance(string variableName, object value, IExecutionEntity sourceActivityExecution)
        {
            IVariableTypes variableTypes = Context.ProcessEngineConfiguration.VariableTypes;

            IVariableType type = variableTypes.findVariableType(value);

            IVariableInstanceEntity variableInstance = Context.CommandContext.VariableInstanceEntityManager.create(variableName, type, value);
            initializeVariableInstanceBackPointer(variableInstance);
            Context.CommandContext.VariableInstanceEntityManager.insert(variableInstance);

            if (variableInstances != null)
            {
                variableInstances[variableName] = variableInstance;
            }

            // Record historic variable
            Context.CommandContext.HistoryManager.recordVariableCreate(variableInstance);

            // Record historic detail
            Context.CommandContext.HistoryManager.recordHistoricDetailVariableCreate(variableInstance, sourceActivityExecution, ActivityIdUsedForDetails);

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
                    setTransientVariableLocal(variableName, value[variableName]);
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

        public virtual void setTransientVariableLocal(string variableName, object variableValue)
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
                    setTransientVariable(variableName, value[variableName]);
                }
            }
            get
            {
                return collectTransientVariables(new Dictionary<string, object>());
            }
        }

        public virtual void setTransientVariable(string variableName, object variableValue)
        {
            VariableScopeImpl parentVariableScope = ParentVariableScope;
            if (parentVariableScope != null)
            {
                parentVariableScope.setTransientVariable(variableName, variableValue);
                return;
            }
            setTransientVariableLocal(variableName, variableValue);
        }

        public virtual object getTransientVariableLocal(string variableName)
        {
            if (transientVariabes != null)
            {
                return transientVariabes[variableName].Value;
            }
            return null;
        }


        public virtual object getTransientVariable(string variableName)
        {
            if (transientVariabes != null && transientVariabes.ContainsKey(variableName))
            {
                return transientVariabes[variableName].Value;
            }

            VariableScopeImpl parentScope = ParentVariableScope;
            if (parentScope != null)
            {
                return parentScope.getTransientVariable(variableName);
            }

            return null;
        }


        protected internal virtual IDictionary<string, object> collectTransientVariables(Dictionary<string, object> variables)
        {
            VariableScopeImpl parentScope = ParentVariableScope;
            if (parentScope != null)
            {
                variables.putAll(parentScope.collectVariables(variables));
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

        public virtual void removeTransientVariableLocal(string variableName)
        {
            if (transientVariabes != null)
            {
                transientVariabes.Remove(variableName);
            }
        }

        public virtual void removeTransientVariablesLocal()
        {
            if (transientVariabes != null)
            {
                transientVariabes.Clear();
            }
        }

        public virtual void removeTransientVariable(string variableName)
        {
            if (transientVariabes != null && transientVariabes.ContainsKey(variableName))
            {
                removeTransientVariableLocal(variableName);
                return;
            }
            VariableScopeImpl parentVariableScope = ParentVariableScope;
            if (parentVariableScope != null)
            {
                parentVariableScope.removeTransientVariable(variableName);
            }
        }

        public virtual void removeTransientVariables()
        {
            removeTransientVariablesLocal();
            VariableScopeImpl parentVariableScope = ParentVariableScope;
            if (parentVariableScope != null)
            {
                parentVariableScope.removeTransientVariablesLocal();
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


        public virtual T getVariable<T>(string variableName)
        {
            return (T)getVariable(variableName);
        }

        public virtual T getVariableLocal<T>(string variableName)
        {
            return (T)getVariableLocal(variableName);
        }
    }

}