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
namespace Sys.Workflow.Engine.Impl.Persistence.Deploies
{
    using Microsoft.Extensions.Caching.Memory;
    using Newtonsoft.Json.Linq;
    using Sys.Workflow;
    using Sys.Workflow.Caches;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using System;
    using System.Threading;

    /// <summary>
    /// Default cache: keep everything in memory, unless a limit is set.
    /// 
    /// 
    /// </summary>
    public class ProcessDefinitionInfoCache
    {
        /// <summary>
        /// 缓存命中后距上次回源校验 revision 的最大间隔。
        /// 在该间隔内直接信任缓存，避免引擎每推进一步（条件、用户任务、服务任务等）都多一次数据库往返；
        /// 流程定义 info 变更后最多延迟该时长被感知。
        /// </summary>
        private static readonly TimeSpan RevisionCheckInterval = TimeSpan.FromSeconds(60);

        // volatile：Clear 采用原子替换发布新实例，保证并发读能及时看到
        private volatile IMemoryCache cache;
        protected internal ICommandExecutor commandExecutor;
        private readonly int sizeLimit;
        private readonly MemoryCacheProvider memoryCacheProvider;

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

            memoryCacheProvider = ProcessEngineServiceProvider.Resolve<MemoryCacheProvider>();
            cache = memoryCacheProvider.Create(limit);
        }

        public virtual ProcessDefinitionInfoCacheObject Get(string processDefinitionId)
        {
            ICommand<object> cacheCommand = new CommandAnonymousInnerClass(this, processDefinitionId);

            ProcessDefinitionInfoCacheObject infoCacheObject;
            if (Context.CommandContext is object)
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
            // 配置了上限（SizeLimit>0）时 MemoryCache 强制要求每个缓存项声明 Size，否则抛 InvalidOperationException；
            // 每个条目按 1 计数，此时上限即最大缓存条目数；无上限时该 Size 不参与统计，无副作用
            cache.Set(id, obj, new MemoryCacheEntryOptions { Size = 1 });
        }

        public virtual void Remove(string id)
        {
            cache.Remove(id);
        }

        public virtual void Clear()
        {
            // 原子替换：先构造新实例再一次性换掉引用，避免并发读写拿到已 Dispose 的旧实例；
            // 旧实例可能有并发读仍在使用，不主动 Dispose，交由 GC 回收
            _ = Interlocked.Exchange(ref cache, memoryCacheProvider.Create(sizeLimit));
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

            // 缓存命中且距上次 revision 校验未超过间隔时直接返回缓存，不再回源查库；
            // 并发下可能出现多线程同时判定过期，最坏只是多一次校验查询，不影响正确性。
            if (DateTime.UtcNow - cacheObject.LastRevisionCheckTime < RevisionCheckInterval)
            {
                return cacheObject;
            }

            IProcessDefinitionInfoEntity infoEntity = infoEntityManager.FindProcessDefinitionInfoByProcessDefinitionId(processDefinitionId);
            if (infoEntity is object && infoEntity.Revision != cacheObject.Revision)
            {
                cacheObject.Revision = infoEntity.Revision;
                if (infoEntity.InfoJsonId is not null)
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
            else if (infoEntity is null)
            {
                cacheObject.Revision = 0;
                cacheObject.InfoNode = objectMapper.CreateObjectNode();
            }

            // 本次已回源校验（无论 revision 是否变化、实体是否存在），记录校验时间，间隔内不再重复查库
            cacheObject.LastRevisionCheckTime = DateTime.UtcNow;

            return cacheObject;
        }

    }
}