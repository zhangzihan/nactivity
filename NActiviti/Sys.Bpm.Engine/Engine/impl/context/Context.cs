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

namespace org.activiti.engine.impl.context
{
    using Newtonsoft.Json.Linq;
    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.deploy;
    using System.Globalization;
    using System.Threading;

    /// 
    /// 
    /// 
    public class Context
    {
        protected internal static ThreadLocal<Stack<ICommandContext>> commandContextThreadLocal =
               new ThreadLocal<Stack<ICommandContext>>(true);
        protected internal static ThreadLocal<Stack<ProcessEngineConfigurationImpl>> processEngineConfigurationStackThreadLocal =
            new ThreadLocal<Stack<ProcessEngineConfigurationImpl>>(true);
        protected internal static ThreadLocal<Stack<ITransactionContext>> transactionContextThreadLocal =
            new ThreadLocal<Stack<ITransactionContext>>(true);
        protected internal static ThreadLocal<IDictionary<string, JToken>> bpmnOverrideContextThreadLocal =
            new ThreadLocal<IDictionary<string, JToken>>(true);

        protected internal static ResourceBundleControl resourceBundleControl = new ResourceBundleControl();

        /// <summary>
        /// 
        /// </summary>
        public static ICommandContext CommandContext
        {
            get
            {
                Stack<ICommandContext> stack = GetStack(commandContextThreadLocal);
                if (stack.Count == 0)
                {
                    return null;
                }
                return stack.Peek();
            }
            set
            {
                GetStack(commandContextThreadLocal).Push(value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static IActivitiEngineAgenda Agenda
        {
            get
            {
                return CommandContext.Agenda;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static void RemoveCommandContext()
        {
            GetStack(commandContextThreadLocal)?.Pop();
        }

        /// <summary>
        /// 
        /// </summary>
        public static ProcessEngineConfigurationImpl ProcessEngineConfiguration
        {
            get
            {
                Stack<ProcessEngineConfigurationImpl> stack = GetStack(processEngineConfigurationStackThreadLocal);
                if ((stack?.Count).GetValueOrDefault(0) == 0)
                {
                    return null;
                }
                return stack.Peek();
            }
            set
            {
                GetStack(processEngineConfigurationStackThreadLocal)?.Push(value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static void RemoveProcessEngineConfiguration()
        {
            GetStack(processEngineConfigurationStackThreadLocal)?.Pop();
        }

        /// <summary>
        /// 
        /// </summary>
        public static ITransactionContext TransactionContext
        {
            get
            {
                Stack<ITransactionContext> stack = GetStack(transactionContextThreadLocal);
                if ((stack?.Count).GetValueOrDefault() == 0)
                {
                    return null;
                }
                return stack.Peek();
            }
            set
            {
                GetStack(transactionContextThreadLocal).Push(value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static void RemoveTransactionContext()
        {
            GetStack(transactionContextThreadLocal)?.Pop();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="threadLocal"></param>
        /// <returns></returns>
        protected internal static Stack<T> GetStack<T>(ThreadLocal<Stack<T>> threadLocal)
        {
            Stack<T> stack = threadLocal.Value;
            if (stack == null)
            {
                stack = new Stack<T>();
                threadLocal.Value = stack;
            }
            return stack;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="processDefinitionId"></param>
        /// <returns></returns>
        public static JToken GetBpmnOverrideElementProperties(string id, string processDefinitionId)
        {
            JToken definitionInfoNode = GetProcessDefinitionInfoNode(processDefinitionId);
            JToken elementProperties = null;
            if (definitionInfoNode != null)
            {
                elementProperties = ProcessEngineConfiguration.DynamicBpmnService.GetBpmnElementProperties(id, definitionInfoNode);
            }
            return elementProperties;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="language"></param>
        /// <param name="id"></param>
        /// <param name="processDefinitionId"></param>
        /// <param name="useFallback"></param>
        /// <returns></returns>
        public static JToken GetLocalizationElementProperties(string language, string id, string processDefinitionId, bool useFallback)
        {
            JToken definitionInfoNode = GetProcessDefinitionInfoNode(processDefinitionId);
            JToken localizationProperties = null;
            if (definitionInfoNode != null)
            {
                if (!useFallback)
                {
                    localizationProperties = ProcessEngineConfiguration.DynamicBpmnService.GetLocalizationElementProperties(language, id, definitionInfoNode);

                }
                else
                {
                    List<CultureInfo> candidateLocales = new List<CultureInfo>();
                    candidateLocales.AddRange(resourceBundleControl.GetCandidateLocales(id, new CultureInfo("zh-cn")));//Locale.forLanguageTag(language)));
                    foreach (CultureInfo locale in candidateLocales)
                    {
                        localizationProperties = ProcessEngineConfiguration.DynamicBpmnService.GetLocalizationElementProperties("zh-cn", id, definitionInfoNode);// locale.toLanguageTag(), id, definitionInfoNode);

                        if (localizationProperties != null)
                        {
                            break;
                        }
                    }
                }
            }
            return localizationProperties;
        }

        /// <summary>
        /// 
        /// </summary>
        public static void RemoveBpmnOverrideContext()
        {
            if (bpmnOverrideContextThreadLocal.IsValueCreated)
            {
                var item = bpmnOverrideContextThreadLocal.Value;

                bpmnOverrideContextThreadLocal.Values.Remove(item);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="processDefinitionId"></param>
        /// <returns></returns>
        protected internal static JToken GetProcessDefinitionInfoNode(string processDefinitionId)
        {
            IDictionary<string, JToken> bpmnOverrideMap = BpmnOverrideContext;
            if (!bpmnOverrideMap.ContainsKey(processDefinitionId))
            {
                ProcessDefinitionInfoCacheObject cacheObject = ProcessEngineConfiguration.DeploymentManager.ProcessDefinitionInfoCache.Get(processDefinitionId);

                AddBpmnOverrideElement(processDefinitionId, cacheObject.InfoNode);
            }

            return BpmnOverrideContext[processDefinitionId];
        }

        /// <summary>
        /// 
        /// </summary>
        protected internal static IDictionary<string, JToken> BpmnOverrideContext
        {
            get
            {
                IDictionary<string, JToken> bpmnOverrideMap = bpmnOverrideContextThreadLocal.Value;
                if (bpmnOverrideMap == null)
                {
                    bpmnOverrideMap = new Dictionary<string, JToken>();
                }
                return bpmnOverrideMap;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="infoNode"></param>
        protected internal static void AddBpmnOverrideElement(string id, JToken infoNode)
        {
            IDictionary<string, JToken> bpmnOverrideMap = bpmnOverrideContextThreadLocal.Value;
            if (bpmnOverrideMap == null)
            {
                bpmnOverrideMap = new Dictionary<string, JToken>();
                bpmnOverrideContextThreadLocal.Value = bpmnOverrideMap;
            }
            bpmnOverrideMap[id] = infoNode;
        }

        /// <summary>
        /// 
        /// </summary>
        public class ResourceBundleControl
        {
            private readonly IList<CultureInfo> locales = new List<CultureInfo>()
            {
                new CultureInfo("zh-cn"),
                new CultureInfo("zh-hans"),
                new CultureInfo("en-us")
            };

            /// <summary>
            /// 
            /// </summary>
            /// <param name="baseName"></param>
            /// <param name="locale"></param>
            /// <returns></returns>
            public virtual IList<CultureInfo> GetCandidateLocales(string baseName, CultureInfo locale)
            {
                return locales;
            }
        }
    }
}