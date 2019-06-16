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
namespace Sys.Workflow.engine.impl.scripting
{
    using Sys.Workflow.engine.@delegate;
    using Sys.Workflow.engine.impl.persistence.entity;
    using System.Collections.Concurrent;

    /// 
    /// 
    /// 
    public class ScriptingEngines
    {
        public const string DEFAULT_SCRIPTING_LANGUAGE = "C#";

        //private readonly ScriptEngineManager scriptEngineManager;
        //protected internal ScriptBindingsFactory scriptBindingsFactory;

        protected internal bool cacheScriptingEngines = true;
        protected static internal ConcurrentDictionary<string, dynamic> cachedEngines;

        public ScriptingEngines()
        {
            cachedEngines = new ConcurrentDictionary<string, dynamic>();
        }

        public virtual object Evaluate(string script, IExecutionEntity execution)
        {
            return Evaluate(script, execution, CreateBindings(execution));
        }

        public virtual bool CacheScriptingEngines
        {
            set
            {
                this.cacheScriptingEngines = value;
            }
            get
            {
                return cacheScriptingEngines;
            }
        }

        protected virtual object Evaluate(string script, IExecutionEntity execution, IDictionary<string, object> bindings)
        {
            //CSScriptLib.CSScript.RoslynEvaluator.
            return null;
        }

        protected internal virtual IDictionary<string, object> CreateBindings(IVariableScope variableScope)
        {
            if (variableScope == null)
            {
                return null;
            }

            return variableScope.Variables;
        }
    }
}