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
namespace Sys.Workflow.Engine.Impl.Scripting
{
    using CSScriptLib;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Sys.Workflow.Engine.Delegate;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using System;
    using System.Collections.Concurrent;
    using System.Text;

    /// <summary>
    /// 
    /// </summary>
    class ScriptingEngines : IScriptingEngines
    {
        //private readonly ScriptEngineManager scriptEngineManager;
        //protected internal ScriptBindingsFactory scriptBindingsFactory;

        public const string DEFAULT_SCRIPTING_LANGUAGE = "C#";
        protected internal bool cacheScriptingEngines = true;
        protected static internal ConcurrentDictionary<string, MethodDelegate<object>> cachedEngines;

        /// <summary>
        /// 
        /// </summary>
        public ScriptingEngines()
        {
            cachedEngines = new ConcurrentDictionary<string, MethodDelegate<object>>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="script"></param>
        /// <param name="execution"></param>
        /// <returns></returns>
        public virtual object Evaluate(string script, IExecutionEntity execution)
        {
            return Evaluate(script, execution, CreateBindings(execution));
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool CacheScriptingEngines
        {
            get
            {
                return cacheScriptingEngines;
            }
            set
            {
                this.cacheScriptingEngines = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="script"></param>
        /// <param name="execution"></param>
        /// <param name="bindings"></param>
        /// <returns></returns>
        protected virtual object Evaluate(string script, IExecutionEntity execution, IDictionary<string, object> bindings)
        {
            script = $"object Evaluator(Sys.Workflow.Engine.Impl.Persistence.Entity.IExecutionEntity args)" +
            $"{{{script} return true;}}";
            MethodDelegate<object> func;
            if (CacheScriptingEngines)
            {
                func = cachedEngines.GetOrAdd(script, script =>
                {
                    return CSScript.Evaluator
                        .ReferenceDomainAssemblies()
                        .CreateDelegate<object>(script);
                });
            }
            else
            {
                func = CSScript.Evaluator
                        .ReferenceDomainAssemblies()
                        .CreateDelegate<object>(script);
            }

            var result = func(execution);

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="variableScope"></param>
        /// <returns></returns>
        protected internal virtual IDictionary<string, object> CreateBindings(IVariableScope variableScope)
        {
            if (variableScope is null)
            {
                return null;
            }

            return variableScope.Variables;
        }
    }
}