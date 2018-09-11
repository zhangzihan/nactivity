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

        public static ICommandContext CommandContext
        {
            get
            {
                Stack<ICommandContext> stack = getStack<ICommandContext>(commandContextThreadLocal);
                if (stack.Count == 0)
                {
                    return null;
                }
                return stack.Peek();
            }
            set
            {
                getStack(commandContextThreadLocal).Push(value);
            }
        }

        public static IActivitiEngineAgenda Agenda
        {
            get
            {
                return CommandContext.Agenda;
            }
        }


        public static void removeCommandContext()
        {
            getStack(commandContextThreadLocal).Pop();
        }

        public static ProcessEngineConfigurationImpl ProcessEngineConfiguration
        {
            get
            {
                Stack<ProcessEngineConfigurationImpl> stack = getStack(processEngineConfigurationStackThreadLocal);
                if (stack.Count == 0)
                {
                    return null;
                }
                return stack.Peek();
            }
            set
            {
                getStack(processEngineConfigurationStackThreadLocal).Push(value);
            }
        }


        public static void removeProcessEngineConfiguration()
        {
            getStack(processEngineConfigurationStackThreadLocal).Pop();
        }

        public static ITransactionContext TransactionContext
        {
            get
            {
                Stack<ITransactionContext> stack = getStack(transactionContextThreadLocal);
                if (stack.Count == 0)
                {
                    return null;
                }
                return stack.Peek();
            }
            set
            {
                getStack(transactionContextThreadLocal).Push(value);
            }
        }


        public static void removeTransactionContext()
        {
            getStack(transactionContextThreadLocal).Pop();
        }

        protected internal static Stack<T> getStack<T>(ThreadLocal<Stack<T>> threadLocal)
        {
            Stack<T> stack = threadLocal.Value;
            if (stack == null)
            {
                stack = new Stack<T>();
                threadLocal.Value = stack;
            }
            return stack;
        }

        public static JToken getBpmnOverrideElementProperties(string id, string processDefinitionId)
        {
            JToken definitionInfoNode = getProcessDefinitionInfoNode(processDefinitionId);
            JToken elementProperties = null;
            if (definitionInfoNode != null)
            {
                elementProperties = ProcessEngineConfiguration.DynamicBpmnService.getBpmnElementProperties(id, definitionInfoNode);
            }
            return elementProperties;
        }

        public static JToken getLocalizationElementProperties(string language, string id, string processDefinitionId, bool useFallback)
        {
            JToken definitionInfoNode = getProcessDefinitionInfoNode(processDefinitionId);
            JToken localizationProperties = null;
            if (definitionInfoNode != null)
            {
                if (!useFallback)
                {
                    localizationProperties = ProcessEngineConfiguration.DynamicBpmnService.getLocalizationElementProperties(language, id, definitionInfoNode);

                }
                else
                {
                    List<CultureInfo> candidateLocales = new List<CultureInfo>();
                    candidateLocales.AddRange(resourceBundleControl.getCandidateLocales(id, new CultureInfo("zh-cn")));//Locale.forLanguageTag(language)));
                    foreach (CultureInfo locale in candidateLocales)
                    {
                        localizationProperties = ProcessEngineConfiguration.DynamicBpmnService.getLocalizationElementProperties("zh-cn", id, definitionInfoNode);// locale.toLanguageTag(), id, definitionInfoNode);

                        if (localizationProperties != null)
                        {
                            break;
                        }
                    }
                }
            }
            return localizationProperties;
        }

        public static void removeBpmnOverrideContext()
        {
            if (bpmnOverrideContextThreadLocal.IsValueCreated)
            {
                var item = bpmnOverrideContextThreadLocal.Value;

                bpmnOverrideContextThreadLocal.Values.Remove(item);
            }
        }

        protected internal static JToken getProcessDefinitionInfoNode(string processDefinitionId)
        {
            IDictionary<string, JToken> bpmnOverrideMap = BpmnOverrideContext;
            if (!bpmnOverrideMap.ContainsKey(processDefinitionId))
            {
                ProcessDefinitionInfoCacheObject cacheObject = ProcessEngineConfiguration.DeploymentManager.ProcessDefinitionInfoCache.get(processDefinitionId);

                addBpmnOverrideElement(processDefinitionId, cacheObject.InfoNode);
            }

            return BpmnOverrideContext[processDefinitionId];
        }

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

        protected internal static void addBpmnOverrideElement(string id, JToken infoNode)
        {
            IDictionary<string, JToken> bpmnOverrideMap = bpmnOverrideContextThreadLocal.Value;
            if (bpmnOverrideMap == null)
            {
                bpmnOverrideMap = new Dictionary<string, JToken>();
                bpmnOverrideContextThreadLocal.Value = bpmnOverrideMap;
            }
            bpmnOverrideMap[id] = infoNode;
        }

        public class ResourceBundleControl
        {
            IList<CultureInfo> locales = new List<CultureInfo>()
            {
                new CultureInfo("zh-cn"),
                new CultureInfo("zh-hans"),
                new CultureInfo("en-us")
            };

            public virtual IList<CultureInfo> getCandidateLocales(string baseName, CultureInfo locale)
            {
                return locales;
                //return base.getCandidateLocales(baseName, locale);

            }
        }
    }

}