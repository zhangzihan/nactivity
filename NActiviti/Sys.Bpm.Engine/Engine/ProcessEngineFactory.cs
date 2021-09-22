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
    using Sys.Workflow.Engine.Impl.Cfg;
    using Microsoft.Extensions.Configuration;

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
    public abstract class ProcessEngineFactory
    {
        private static readonly ILogger<ProcessEngineFactory> log = ProcessEngineServiceProvider.LoggerService<ProcessEngineFactory>();

        private static readonly object syncRoot = new object();

        public const string NAME_DEFAULT = "default";

        private readonly static ConcurrentDictionary<string, IProcessEngine> processEngines = new ConcurrentDictionary<string, IProcessEngine>();
        private readonly static IDictionary<string, IProcessEngineInfo> processEngineInfosByName = new Dictionary<string, IProcessEngineInfo>();
        private readonly static IDictionary<string, IProcessEngineInfo> processEngineInfosByResourceUrl = new Dictionary<string, IProcessEngineInfo>();
        private readonly static IList<IProcessEngineInfo> processEngineInfos = new List<IProcessEngineInfo>();

        /// <summary>
        /// Registers the given process engine. No <seealso cref="IProcessEngineInfo"/> will be available for this process engine. An engine that is registered will be closed when the <seealso cref="ProcessEngines#destroy()"/>
        /// is called.
        /// </summary>
        public static void RegisterProcessEngine(IProcessEngine processEngine)
        {
            processEngines.AddOrUpdate(processEngine.Name, processEngine, (key, old) => processEngine);
            InitProcessEngineFromResource(processEngine);
        }

        /// <summary>
        /// Unregisters the given process engine.
        /// </summary>
        public static void Unregister(IProcessEngine processEngine)
        {
            processEngines.TryRemove(processEngine.Name, out _);
        }

        public static void InitProcessEngineFromResource()
        {

        }

        private static IProcessEngineInfo InitProcessEngineFromResource(IProcessEngine processEngine)
        {
            IProcessEngineInfo processEngineInfo;
            try
            {
                log.LogInformation($"initialised process engine {processEngine.Name}");
                processEngineInfo = new ProcessEngineInfoImpl(processEngine.Name, null);
                processEngineInfosByName[processEngine.Name] = processEngineInfo;
            }
            catch (Exception e)
            {
                log.LogError(e, $"Exception while initializing process engine: {e.Message}");
                _ = new ProcessEngineInfoImpl(null, e.StackTrace);
                throw e;
            }

            processEngineInfos.Add(processEngineInfo);

            return processEngineInfo;
        }

        /// <summary>
        /// Get initialization results. </summary>
        public static IList<IProcessEngineInfo> ProcessEngineInfos
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
        public static IProcessEngineInfo GetProcessEngineInfo(string processEngineName)
        {
            processEngineInfosByName.TryGetValue(processEngineName, out var pe);

            return pe;
        }

        public static IProcessEngine DefaultProcessEngine
        {
            get
            {
                return GetProcessEngine(NAME_DEFAULT);
            }
        }

        public static IProcessEngine CreateProcessEngine(ProcessEngineConfiguration processEngineConfig)
        {
            if (processEngineConfig is null)
            {
                throw new ArgumentNullException(nameof(processEngineConfig));
            }
            IProcessEngine engine = processEngineConfig.BuildProcessEngine();

            return engine;
        }

        public static IProcessEngine CreateProcessEngine(string processEngineName)
        {
            if (string.IsNullOrWhiteSpace(processEngineName))
            {
                throw new ArgumentException($"“{nameof(processEngineName)}”不能为 null 或空白。", nameof(processEngineName));
            }

            ProcessEngineConfiguration processEngineConfig = new StandaloneProcessEngineConfiguration(
                    new HistoryServiceImpl(),
                    new TaskServiceImpl(),
                    new DynamicBpmnServiceImpl(),
                    new RepositoryServiceImpl(),
                    new RuntimeServiceImpl(),
                    new ManagementServiceImpl(),
                    //sp.GetService<IAsyncExecutor>(),
                    null,
                    ProcessEngineServiceProvider.Resolve<IConfiguration>())
            {
                ProcessEngineName = processEngineName
            };
            return CreateProcessEngine(processEngineConfig);
        }

        /// <summary>
        /// obtain a process engine by name.
        /// </summary>
        /// <param name="processEngineName">
        ///          is the name of the process engine or null for the default process engine. </param>
        public static IProcessEngine GetProcessEngine(string processEngineName)
        {
            processEngines.TryGetValue(processEngineName, out var engine);

            return engine;
        }

        /// <summary>
        /// provides access to process engine to application clients in a managed server environment.
        /// </summary>
        public static IDictionary<string, IProcessEngine> ProcessEngines
        {
            get
            {
                return processEngines;
            }
        }

        /// <summary>
        /// closes all process engines. This method should be called when the server shuts down.
        /// </summary>
        public static void Destroy()
        {
            lock (syncRoot)
            {
                IDictionary<string, IProcessEngine> engines = new Dictionary<string, IProcessEngine>(processEngines);
                processEngines.Clear();

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
            }
        }
    }
}