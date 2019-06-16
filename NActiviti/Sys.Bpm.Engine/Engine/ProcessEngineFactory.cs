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
namespace Sys.Workflow.Engine
{
    using Microsoft.Extensions.Logging;
    using Sys.Workflow.Engine.Impl;
    using Sys.Workflow;
    using System.Collections.Concurrent;

    /// <summary>
    /// Helper for initializing and closing process engines in server environments. <br>
    /// All created <seealso cref="IProcessEngine"/>s will be registered with this class. <br>
    /// The activiti-webapp-init webapp will call the <seealso cref="#init()"/> method when the webapp is deployed and it will call the <seealso cref="#destroy()"/> method when the webapp is destroyed, using a
    /// context-listener ( <code>Sys.Workflow.Impl.Servlet.Listener.ProcessEnginesServletContextListener</code> ). That way, all applications can just use the <seealso cref="#getProcessEngines()"/> to obtain
    /// pre-initialized and cached process engines. <br>
    /// <br>
    /// Please note that there is <b>no lazy initialization</b> of process engines, so make sure the context-listener is configured or <seealso cref="IProcessEngine"/>s are already created so they were registered on
    /// this class.<br>
    /// <br>
    /// The <seealso cref="#init()"/> method will try to build one <seealso cref="IProcessEngine"/> for each activiti.cfg.xml file found on the classpath. If you have more then one, make sure you specify different
    /// process.engine.name values.
    /// 
    /// 
    /// 
    /// </summary>
    public class ProcessEngineFactory
    {
        private static readonly ILogger<ProcessEngineFactory> log = ProcessEngineServiceProvider.LoggerService<ProcessEngineFactory>();

        private static ProcessEngineFactory processEngineFactory;

        public static ProcessEngineFactory Instance
        {
            get
            {
                if (processEngineFactory == null)
                {
                    processEngineFactory = new ProcessEngineFactory();
                }

                return processEngineFactory;
            }
        }

        private ProcessEngineFactory()
        {
        }

        public const string NAME_DEFAULT = "default";

        protected internal static bool isInitialized;
        protected internal static ConcurrentDictionary<string, IProcessEngine> processEngines = new ConcurrentDictionary<string, IProcessEngine>();
        protected internal static IDictionary<string, IProcessEngineInfo> processEngineInfosByName = new Dictionary<string, IProcessEngineInfo>();
        protected internal static IDictionary<string, IProcessEngineInfo> processEngineInfosByResourceUrl = new Dictionary<string, IProcessEngineInfo>();
        protected internal static IList<IProcessEngineInfo> processEngineInfos = new List<IProcessEngineInfo>();

        /// <summary>
        /// Initializes all process engines that can be found on the classpath for resources <code>activiti.cfg.xml</code> (plain Activiti style configuration) and for resources
        /// <code>activiti-context.xml</code> (Spring style configuration).
        /// </summary>
        public void Init()
        {
            lock (typeof(ProcessEngineFactory))
            {
                if (!Initialized)
                {
                    if (processEngines == null)
                    {
                        processEngines = new ConcurrentDictionary<string, IProcessEngine>();
                    }
                    log.LogInformation("Initializing process engine using configuration '{}'");
                    InitProcessEngineFromResource();
                    Initialized = true;
                }
                else
                {
                    log.LogInformation("Process engines already initialized");
                }
            }
        }

        /// <summary>
        /// Registers the given process engine. No <seealso cref="IProcessEngineInfo"/> will be available for this process engine. An engine that is registered will be closed when the <seealso cref="ProcessEngines#destroy()"/>
        /// is called.
        /// </summary>
        public static void RegisterProcessEngine(IProcessEngine processEngine)
        {
            processEngines.AddOrUpdate(processEngine.Name, processEngine, (key, old) => processEngine);
        }

        /// <summary>
        /// Unregisters the given process engine.
        /// </summary>
        public static void Unregister(IProcessEngine processEngine)
        {
            processEngines.TryRemove(processEngine.Name, out _);
        }

        private IProcessEngineInfo InitProcessEngineFromResource()
        {
            IProcessEngineInfo processEngineInfo = null;
            string processEngineName = null;
            try
            {
                log.LogInformation("initializing process engine");
                IProcessEngine processEngine = BuildProcessEngine();
                processEngineName = processEngine.Name;
                log.LogInformation($"initialised process engine {processEngineName}");
                processEngineInfo = new ProcessEngineInfoImpl(processEngineName, null);
                processEngines.AddOrUpdate(processEngineName, processEngine, (name, engine) => engine);
                processEngineInfosByName[processEngineName] = processEngineInfo;
            }
            catch (Exception e)
            {
                log.LogError(e, $"Exception while initializing process engine: {e.Message}");
                processEngineInfo = new ProcessEngineInfoImpl(null, GetExceptionString(e));
                throw e;
            }

            processEngineInfos.Add(processEngineInfo);

            return processEngineInfo;
        }

        private string GetExceptionString(Exception e)
        {
            Console.WriteLine(e.StackTrace);
            return e.StackTrace;
        }

        private IProcessEngine BuildProcessEngine()
        {
            try
            {
                ProcessEngineConfiguration engineConfig = ProcessEngineServiceProvider.Resolve<ProcessEngineConfiguration>();

                return engineConfig.BuildProcessEngine();
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"buld process engine failed.{ex.Message}");
                throw ex;
            }
        }

        /// <summary>
        /// Get initialization results. </summary>
        public IList<IProcessEngineInfo> ProcessEngineInfos
        {
            get
            {
                return processEngineInfos;
            }
        }

        /// <summary>
        /// Get initialization results. Only info will we available for process engines which were added in the <seealso cref="ProcessEngines#init()"/>. No <seealso cref="IProcessEngineInfo"/> is available for engines which were
        /// registered programatically.
        /// </summary>
        public IProcessEngineInfo GetProcessEngineInfo(string processEngineName)
        {
            processEngineInfosByName.TryGetValue(processEngineName, out var pe);

            return pe;
        }

        public IProcessEngine DefaultProcessEngine
        {
            get
            {
                return GetProcessEngine(NAME_DEFAULT);
            }
        }

        /// <summary>
        /// obtain a process engine by name.
        /// </summary>
        /// <param name="processEngineName">
        ///          is the name of the process engine or null for the default process engine. </param>
        public IProcessEngine GetProcessEngine(string processEngineName)
        {
            if (!Initialized)
            {
                Init();
            }

            processEngines.TryGetValue(processEngineName, out var engine);

            return engine;
        }

        /// <summary>
        /// retries to initialize a process engine that previously failed.
        /// </summary>
        public IProcessEngineInfo Retry()
        {
            log.LogDebug($"retying initializing of resource.");
            try
            {
                return InitProcessEngineFromResource();
            }
            catch (Exception e)
            {
                throw new ActivitiIllegalArgumentException("retry failed.", e);
            }
        }

        /// <summary>
        /// provides access to process engine to application clients in a managed server environment.
        /// </summary>
        public IDictionary<string, IProcessEngine> ProcessEngines
        {
            get
            {
                return processEngines;
            }
        }

        /// <summary>
        /// closes all process engines. This method should be called when the server shuts down.
        /// </summary>
        public void Destroy()
        {
            lock (typeof(ProcessEngineFactory))
            {
                if (Initialized)
                {
                    IDictionary<string, IProcessEngine> engines = new Dictionary<string, IProcessEngine>(processEngines);
                    processEngines = new ConcurrentDictionary<string, IProcessEngine>();

                    foreach (string processEngineName in engines.Keys)
                    {
                        IProcessEngine processEngine = engines[processEngineName];
                        try
                        {
                            processEngine.Close();
                        }
                        catch (Exception e)
                        {
                            log.LogError(e, $"exception while closing {(processEngineName is null ? "the default process engine" : "process engine " + processEngineName)}");
                        }
                    }

                    processEngineInfosByName.Clear();
                    processEngineInfosByResourceUrl.Clear();
                    processEngineInfos.Clear();

                    Initialized = false;
                }
            }
        }

        public bool Initialized
        {
            get
            {
                return isInitialized;
            }
            set
            {
                isInitialized = value;
            }
        }

    }

}