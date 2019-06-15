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
namespace org.activiti.engine.impl.persistence.deploy
{
    using Microsoft.Extensions.Caching.Memory;
    using Newtonsoft.Json.Linq;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using Sys.Bpm;
    using Sys.Workflow;
    using Sys.Workflow.Cache;
    using System;
    using System.Collections.Concurrent;

    /// <summary>
    /// Default cache: keep everything in memory, unless a limit is set.
    /// 
    /// 
    /// </summary>
    public class ProcessDefinitionInfoCache
    {
        private IMemoryCache cache;
        protected internal ICommandExecutor commandExecutor;
        private readonly int sizeLimit;

        /// <summary>
        /// Cache with no limit </summary>
        public ProcessDefinitionInfoCache(ICommandExecutor commandExecutor) : this(commandExecutor, -1)
        {
        }

        /// <summary>
        /// Cache which has a hard limit: no more elements will be cached than the limit. </summary>
        public ProcessDefinitionInfoCache(ICommandExecutor commandExecutor, int limit)
        {
            this.commandExecutor = commandExecutor;
            sizeLimit = limit;

            cache = ProcessEngineServiceProvider.Resolve<MemoryCacheProvider>().Create(limit);
        }

        public virtual ProcessDefinitionInfoCacheObject Get(string processDefinitionId)
        {
            ICommand<object> cacheCommand = new CommandAnonymousInnerClass(this, processDefinitionId);

            ProcessDefinitionInfoCacheObject infoCacheObject;
            if (Context.CommandContext != null)
            {
                infoCacheObject = RetrieveProcessDefinitionInfoCacheObject(processDefinitionId, Context.CommandContext);
            }
            else
            {
                infoCacheObject = commandExecutor.Execute(cacheCommand) as ProcessDefinitionInfoCacheObject;
            }

            return infoCacheObject;
        }

        private class CommandAnonymousInnerClass : ICommand<object>
        {
            private readonly ProcessDefinitionInfoCache outerInstance;

            private readonly string processDefinitionId;

            public CommandAnonymousInnerClass(ProcessDefinitionInfoCache outerInstance, string processDefinitionId)
            {
                this.outerInstance = outerInstance;
                this.processDefinitionId = processDefinitionId;
            }


            public virtual object Execute(ICommandContext commandContext)
            {
                return outerInstance.RetrieveProcessDefinitionInfoCacheObject(processDefinitionId, commandContext);
            }
        }

        public virtual void Add(string id, ProcessDefinitionInfoCacheObject obj)
        {
            cache.Set(id, obj);
        }

        public virtual void Remove(string id)
        {
            cache.Remove(id);
        }

        public virtual void Clear()
        {
            cache.Dispose();

            cache = ProcessEngineServiceProvider.Resolve<MemoryCacheProvider>().Create(sizeLimit);
        }

        // For testing purposes only
        public virtual int Size()
        {
            return (cache as MemoryCache).Count;
        }

        protected internal virtual ProcessDefinitionInfoCacheObject RetrieveProcessDefinitionInfoCacheObject(string processDefinitionId, ICommandContext commandContext)
        {
            IProcessDefinitionInfoEntityManager infoEntityManager = commandContext.ProcessDefinitionInfoEntityManager;
            ObjectMapper objectMapper = commandContext.ProcessEngineConfiguration.ObjectMapper;

            ProcessDefinitionInfoCacheObject cacheObject = cache.GetOrCreate(processDefinitionId, (key) => new ProcessDefinitionInfoCacheObject()
            {
                Revision = 0,
                InfoNode = objectMapper.CreateObjectNode()
            });

            IProcessDefinitionInfoEntity infoEntity = infoEntityManager.FindProcessDefinitionInfoByProcessDefinitionId(processDefinitionId);
            if (infoEntity != null && infoEntity.Revision != cacheObject.Revision)
            {
                cacheObject.Revision = infoEntity.Revision;
                if (!(infoEntity.InfoJsonId is null))
                {
                    byte[] infoBytes = infoEntityManager.FindInfoJsonById(infoEntity.InfoJsonId);
                    try
                    {
                        JToken infoNode = objectMapper.ReadTree(infoBytes);
                        cacheObject.InfoNode = infoNode;
                    }
                    catch (Exception e)
                    {
                        throw new ActivitiException("Error reading json info node for process definition " + processDefinitionId, e);
                    }
                }
            }
            else if (infoEntity == null)
            {
                cacheObject.Revision = 0;
                cacheObject.InfoNode = objectMapper.CreateObjectNode();
            }

            return cacheObject;
        }

    }
}