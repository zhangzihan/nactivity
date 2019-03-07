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
    using Newtonsoft.Json.Linq;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using Sys.Bpm;
    using System;
    using System.Collections.Concurrent;

    /// <summary>
    /// Default cache: keep everything in memory, unless a limit is set.
    /// 
    /// 
    /// </summary>
    public class ProcessDefinitionInfoCache
    {
        protected internal ConcurrentDictionary<string, ProcessDefinitionInfoCacheObject> cache;
        protected internal ICommandExecutor commandExecutor;

        /// <summary>
        /// Cache with no limit </summary>
        public ProcessDefinitionInfoCache(ICommandExecutor commandExecutor)
        {
            this.commandExecutor = commandExecutor;
            this.cache = new ConcurrentDictionary<string, ProcessDefinitionInfoCacheObject>();
        }

        /// <summary>
        /// Cache which has a hard limit: no more elements will be cached than the limit. </summary>
        public ProcessDefinitionInfoCache(ICommandExecutor commandExecutor, int limit)
        {
            this.commandExecutor = commandExecutor;
            this.cache = new LinkedHashMapAnonymousInnerClass(this, limit + 1);
        }

        private class LinkedHashMapAnonymousInnerClass : ConcurrentDictionary<string, ProcessDefinitionInfoCacheObject>
        {
            private readonly ProcessDefinitionInfoCache outerInstance;

            private int limit;

            // +1 is needed, because the entry is inserted first, before it is removed
            // 0.75 is the default (see javadocs)
            // true will keep the 'access-order', which is needed to have a real LRU cache
            private long serialVersionUID;

            public LinkedHashMapAnonymousInnerClass(ProcessDefinitionInfoCache outerInstance, int limit) : base(StringComparer.OrdinalIgnoreCase)
            {
                this.outerInstance = outerInstance;
                this.limit = limit;
                serialVersionUID = 1L;
            }

            protected internal virtual bool removeEldestEntry(KeyValuePair<string, ProcessDefinitionInfoCacheObject> eldest)
            {
                bool removeEldest = outerInstance.size() > limit;
                //if (removeEldest)
                //{
                //    logger.trace("Cache limit is reached, {} will be evicted", eldest.Key);
                //}
                return removeEldest;
            }

        }

        public virtual ProcessDefinitionInfoCacheObject get(string processDefinitionId)
        {
            ProcessDefinitionInfoCacheObject infoCacheObject = null;
            ICommand<object> cacheCommand = new CommandAnonymousInnerClass(this, processDefinitionId);

            if (Context.CommandContext != null)
            {
                infoCacheObject = retrieveProcessDefinitionInfoCacheObject(processDefinitionId, Context.CommandContext);
            }
            else
            {
                infoCacheObject = commandExecutor.execute(cacheCommand) as ProcessDefinitionInfoCacheObject;
            }

            return infoCacheObject;
        }

        private class CommandAnonymousInnerClass : ICommand<object>
        {
            private readonly ProcessDefinitionInfoCache outerInstance;

            private string processDefinitionId;

            public CommandAnonymousInnerClass(ProcessDefinitionInfoCache outerInstance, string processDefinitionId)
            {
                this.outerInstance = outerInstance;
                this.processDefinitionId = processDefinitionId;
            }


            public virtual object execute(ICommandContext commandContext)
            {
                return outerInstance.retrieveProcessDefinitionInfoCacheObject(processDefinitionId, commandContext);
            }
        }

        public virtual void add(string id, ProcessDefinitionInfoCacheObject obj)
        {
            cache.GetOrAdd(id, (str) => obj);
        }

        public virtual void remove(string id)
        {
            cache.TryRemove(id, out ProcessDefinitionInfoCacheObject pico);
        }

        public virtual void clear()
        {
            cache.Clear();
        }

        // For testing purposes only
        public virtual int size()
        {
            return cache.Count;
        }

        protected internal virtual ProcessDefinitionInfoCacheObject retrieveProcessDefinitionInfoCacheObject(string processDefinitionId, ICommandContext commandContext)
        {
            IProcessDefinitionInfoEntityManager infoEntityManager = commandContext.ProcessDefinitionInfoEntityManager;
            ObjectMapper objectMapper = commandContext.ProcessEngineConfiguration.ObjectMapper;

            ProcessDefinitionInfoCacheObject cacheObject = null;
            if (cache.ContainsKey(processDefinitionId))
            {
                cacheObject = cache[processDefinitionId];
            }
            else
            {
                cacheObject = new ProcessDefinitionInfoCacheObject();
                cacheObject.Revision = 0;
                cacheObject.InfoNode = objectMapper.createObjectNode();
            }

            IProcessDefinitionInfoEntity infoEntity = infoEntityManager.findProcessDefinitionInfoByProcessDefinitionId(processDefinitionId);
            if (infoEntity != null && infoEntity.Revision != cacheObject.Revision)
            {
                cacheObject.Revision = infoEntity.Revision;
                if (!ReferenceEquals(infoEntity.InfoJsonId, null))
                {
                    byte[] infoBytes = infoEntityManager.findInfoJsonById(infoEntity.InfoJsonId);
                    try
                    {
                        JToken infoNode = objectMapper.readTree(infoBytes);
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
                cacheObject.InfoNode = objectMapper.createObjectNode();
            }

            return cacheObject;
        }

    }
}