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
namespace org.activiti.engine.impl.scripting
{


    using org.activiti.engine.@delegate;

    /// 
    /// 
    /// 
    public class ScriptingEngines
    {

        public const string DEFAULT_SCRIPTING_LANGUAGE = "juel";
        public const string GROOVY_SCRIPTING_LANGUAGE = "groovy";

        private readonly ScriptEngineManager scriptEngineManager;
        protected internal ScriptBindingsFactory scriptBindingsFactory;

        protected internal bool cacheScriptingEngines = true;
        protected internal IDictionary<string, ScriptEngine> cachedEngines;

        public ScriptingEngines(ScriptBindingsFactory scriptBindingsFactory) : this(new ScriptEngineManager())
        {
            this.scriptBindingsFactory = scriptBindingsFactory;
        }

        public ScriptingEngines(ScriptEngineManager scriptEngineManager)
        {
            this.scriptEngineManager = scriptEngineManager;
            cachedEngines = new Dictionary<string, ScriptEngine>();
        }

        public virtual ScriptingEngines addScriptEngineFactory(ScriptEngineFactory scriptEngineFactory)
        {
            scriptEngineManager.registerEngineName(scriptEngineFactory.EngineName, scriptEngineFactory);
            return this;
        }

        public virtual IList<ScriptEngineFactory> ScriptEngineFactories
        {
            set
            {
                if (value != null)
                {
                    foreach (ScriptEngineFactory scriptEngineFactory in value)
                    {
                        scriptEngineManager.registerEngineName(scriptEngineFactory.EngineName, scriptEngineFactory);
                    }
                }
            }
        }

        public virtual object evaluate(string script, string language, IVariableScope variableScope)
        {
            return evaluate(script, language, createBindings(variableScope));
        }

        public virtual object evaluate(string script, string language, IVariableScope variableScope, bool storeScriptVariables)
        {
            return evaluate(script, language, createBindings(variableScope, storeScriptVariables));
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


        protected internal virtual object evaluate(string script, string language, Bindings bindings)
        {
            ScriptEngine scriptEngine = getEngineByName(language);
            try
            {
                return scriptEngine.eval(script, bindings);
            }
            catch (ScriptException e)
            {
                throw new ActivitiException("problem evaluating script: " + e.Message, e);
            }
        }

        protected internal virtual ScriptEngine getEngineByName(string language)
        {
            ScriptEngine scriptEngine = null;

            if (cacheScriptingEngines)
            {
                scriptEngine = cachedEngines[language];
                if (scriptEngine == null)
                {
                    scriptEngine = scriptEngineManager.getEngineByName(language);

                    if (scriptEngine != null)
                    {
                        // ACT-1858: Special handling for groovy engine regarding GC
                        if (GROOVY_SCRIPTING_LANGUAGE.Equals(language))
                        {
                            try
                            {
                                scriptEngine.Context.setAttribute("#jsr223.groovy.engine.keep.globals", "weak", ScriptContext.ENGINE_SCOPE);
                            }
                            catch (Exception)
                            {
                                // ignore this, in case engine doesn't support the
                                // passed attribute
                            }
                        }

                        // Check if script-engine allows caching, using "THREADING"
                        // parameter as defined in spec
                        object threadingParameter = scriptEngine.Factory.getParameter("THREADING");
                        if (threadingParameter != null)
                        {
                            // Add engine to cache as any non-null result from the
                            // threading-parameter indicates at least MT-access
                            cachedEngines[language] = scriptEngine;
                        }
                    }
                }
            }
            else
            {
                scriptEngine = scriptEngineManager.getEngineByName(language);
            }

            if (scriptEngine == null)
            {
                throw new ActivitiException("Can't find scripting engine for '" + language + "'");
            }
            return scriptEngine;
        }

        /// <summary>
        /// override to build a spring aware ScriptingEngines </summary>
        protected internal virtual Bindings createBindings(IVariableScope variableScope)
        {
            return scriptBindingsFactory.createBindings(variableScope);
        }

        /// <summary>
        /// override to build a spring aware ScriptingEngines </summary>
        protected internal virtual Bindings createBindings(IVariableScope variableScope, bool storeScriptVariables)
        {
            return scriptBindingsFactory.createBindings(variableScope, storeScriptVariables);
        }

        public virtual ScriptBindingsFactory ScriptBindingsFactory
        {
            get
            {
                return scriptBindingsFactory;
            }
            set
            {
                this.scriptBindingsFactory = value;
            }
        }

    }

}