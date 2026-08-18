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

        // 静态缓存只初始化一次；原来在构造函数里重新 new，二次实例化会清空全局缓存。
        // 值用 Lazy 包装：GetOrAdd 的工厂在并发下可能重入执行多次，
        // Lazy 保证同一脚本的 Roslyn 编译只发生一次，避免重复编译产生无法卸载的程序集。
        protected static internal ConcurrentDictionary<string, Lazy<MethodDelegate<object>>> cachedEngines
            = new ConcurrentDictionary<string, Lazy<MethodDelegate<object>>>();

        // 缓存容量上限：脚本以文本为 key，基数通常有限；
        // 超过上限后直接每次编译、不再写入缓存，防止缓存无界增长产生大量无法卸载的程序集。
        private const int MAX_CACHED_SCRIPTS = 1000;

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
            if (CacheScriptingEngines && cachedEngines.Count < MAX_CACHED_SCRIPTS)
            {
                // Lazy 工厂保证编译只发生一次；取 .Value 触发（或复用已完成的）编译结果
                func = cachedEngines.GetOrAdd(script, key => new Lazy<MethodDelegate<object>>(() =>
                    CSScript.Evaluator
                        .ReferenceDomainAssemblies()
                        .CreateDelegate<object>(key))).Value;
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