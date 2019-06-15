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
namespace org.activiti.engine.impl.persistence.entity.data.impl
{

    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.persistence.entity.data.impl.cachematcher;
    using org.activiti.engine.runtime;
    using System.Linq;

    /// 
    public class MybatisExecutionDataManager : AbstractDataManager<IExecutionEntity>, IExecutionDataManager
    {

        protected internal PerformanceSettings performanceSettings;

        protected internal ICachedEntityMatcher<IExecutionEntity> executionsByParentIdMatcher = new ExecutionsByParentExecutionIdEntityMatcher();

        protected internal ICachedEntityMatcher<IExecutionEntity> executionsByProcessInstanceIdMatcher = new ExecutionsByProcessInstanceIdEntityMatcher();

        protected internal ISingleCachedEntityMatcher<IExecutionEntity> subProcessInstanceBySuperExecutionIdMatcher = new SubProcessInstanceExecutionBySuperExecutionIdMatcher();

        protected internal ICachedEntityMatcher<IExecutionEntity> executionsWithSameRootProcessInstanceIdMatcher = new ExecutionsWithSameRootProcessInstanceIdMatcher();

        protected internal ICachedEntityMatcher<IExecutionEntity> inactiveExecutionsInActivityAndProcInstMatcher = new InactiveExecutionsInActivityAndProcInstMatcher();

        protected internal ICachedEntityMatcher<IExecutionEntity> inactiveExecutionsByProcInstMatcher = new InactiveExecutionsByProcInstMatcher();

        protected internal ICachedEntityMatcher<IExecutionEntity> inactiveExecutionsInActivityMatcher = new InactiveExecutionsInActivityMatcher();

        protected internal ICachedEntityMatcher<IExecutionEntity> executionByProcessInstanceMatcher = new ExecutionByProcessInstanceMatcher();

        protected internal ICachedEntityMatcher<IExecutionEntity> executionsByRootProcessInstanceMatcher = new ExecutionsByRootProcessInstanceMatcher();

        protected internal ICachedEntityMatcher<IExecutionEntity> executionsByParentExecutionIdAndActivityIdEntityMatcher = new ExecutionsByParentExecutionIdAndActivityIdEntityMatcher();

        protected internal ICachedEntityMatcher<IExecutionEntity> processInstancesByProcessDefinitionMatcher = new ProcessInstancesByProcessDefinitionMatcher();

        public MybatisExecutionDataManager(ProcessEngineConfigurationImpl processEngineConfiguration) : base(processEngineConfiguration)
        {
            this.performanceSettings = processEngineConfiguration.PerformanceSettings;
        }

        public override Type ManagedEntityClass
        {
            get
            {
                return typeof(ExecutionEntityImpl);
            }
        }

        public override IExecutionEntity Create()
        {
            return ExecutionEntityImpl.CreateWithEmptyRelationshipCollections();
        }

        public override IExecutionEntity FindById<IExecutionEntity>(KeyValuePair<string, object> id)
        {
            if (performanceSettings.EnableEagerExecutionTreeFetching)
            {
                return (IExecutionEntity)FindByIdAndFetchExecutionTree(id.Value?.ToString());
            }
            else
            {
                return base.FindById<IExecutionEntity>(id);
            }
        }

        protected internal virtual IExecutionEntity FindByIdAndFetchExecutionTree(string executionId)
        {
            // If it's in the cache, the tree must have been fetched before
            IExecutionEntity cachedEntity = (IExecutionEntity)EntityCache.FindInCache(ManagedEntityClass, executionId);
            if (cachedEntity != null)
            {
                return cachedEntity;
            }

            // Fetches execution tree. This will store them in the cache.
            IList<IExecutionEntity> executionEntities = (IList<IExecutionEntity>)GetList("selectExecutionsWithSameRootProcessInstanceId", new { executionId }, executionsWithSameRootProcessInstanceIdMatcher, true);

            foreach (IExecutionEntity executionEntity in executionEntities)
            {
                if (executionId.Equals(executionEntity.Id))
                {
                    return executionEntity;
                }
            }
            return null;
        }

        public virtual IExecutionEntity FindSubProcessInstanceBySuperExecutionId(string superExecutionId)
        {
            if (performanceSettings.EnableEagerExecutionTreeFetching)
            {
                FindByIdAndFetchExecutionTree(superExecutionId);
            }

            return GetEntity("selectSubProcessInstanceBySuperExecutionId", new { superExecutionId }, subProcessInstanceBySuperExecutionIdMatcher, !performanceSettings.EnableEagerExecutionTreeFetching);
        }

        public virtual IList<IExecutionEntity> FindChildExecutionsByParentExecutionId(string parentExecutionId)
        {
            if (performanceSettings.EnableEagerExecutionTreeFetching)
            {
                FindByIdAndFetchExecutionTree(parentExecutionId);
                return GetListFromCache(executionsByParentIdMatcher, parentExecutionId);
            }
            else
            {
                return (IList<IExecutionEntity>)GetList("selectExecutionsByParentExecutionId", new { parentExecutionId }, executionsByParentIdMatcher, true);
            }
        }

        public virtual IList<IExecutionEntity> FindChildExecutionsByProcessInstanceId(string processInstanceId)
        {
            if (performanceSettings.EnableEagerExecutionTreeFetching)
            {
                FindByIdAndFetchExecutionTree(processInstanceId);
                return GetListFromCache(executionsByProcessInstanceIdMatcher, processInstanceId);
            }
            else
            {
                return (IList<IExecutionEntity>)GetList("selectChildExecutionsByProcessInstanceId", new { processInstanceId }, executionsByProcessInstanceIdMatcher, true);
            }
        }

        public virtual IList<IExecutionEntity> FindExecutionsByParentExecutionAndActivityIds(string parentExecutionId, ICollection<string> activityIds)
        {
            IDictionary<string, object> parameters = new Dictionary<string, object>(2)
            {
                ["parentExecutionId"] = parentExecutionId,
                ["activityIds"] = activityIds
            };

            if (performanceSettings.EnableEagerExecutionTreeFetching)
            {
                FindByIdAndFetchExecutionTree(parentExecutionId);
                return GetListFromCache(executionsByParentExecutionIdAndActivityIdEntityMatcher, parameters);
            }
            else
            {
                return (IList<IExecutionEntity>)GetList("selectExecutionsByParentExecutionAndActivityIds", parameters, executionsByParentExecutionIdAndActivityIdEntityMatcher, true);
            }
        }

        public virtual IList<IExecutionEntity> FindExecutionsByRootProcessInstanceId(string rootProcessInstanceId)
        {
            if (performanceSettings.EnableEagerExecutionTreeFetching)
            {
                FindByIdAndFetchExecutionTree(rootProcessInstanceId);
                return GetListFromCache(executionsByRootProcessInstanceMatcher, rootProcessInstanceId);
            }
            else
            {
                return (IList<IExecutionEntity>)GetList("selectExecutionsByRootProcessInstanceId", new { rootProcessInstanceId }, executionsByRootProcessInstanceMatcher, true);
            }
        }

        public virtual IList<IExecutionEntity> FindExecutionsByProcessInstanceId(string processInstanceId)
        {
            if (performanceSettings.EnableEagerExecutionTreeFetching)
            {
                FindByIdAndFetchExecutionTree(processInstanceId);
                return GetListFromCache(executionByProcessInstanceMatcher, processInstanceId);
            }
            else
            {
                return (IList<IExecutionEntity>)GetList("selectExecutionsByProcessInstanceId", new { processInstanceId }, executionByProcessInstanceMatcher, true);
            }
        }

        public virtual ICollection<IExecutionEntity> FindInactiveExecutionsByProcessInstanceId(string processInstanceId)
        {
            Dictionary<string, object> @params = new Dictionary<string, object>(2)
            {
                ["processInstanceId"] = processInstanceId,
                ["isActive"] = false
            };

            if (performanceSettings.EnableEagerExecutionTreeFetching)
            {
                FindByIdAndFetchExecutionTree(processInstanceId);
                return GetListFromCache(inactiveExecutionsByProcInstMatcher, @params);
            }
            else
            {
                return GetList("selectInactiveExecutionsForProcessInstance", @params, inactiveExecutionsByProcInstMatcher, true);
            }
        }

        public virtual ICollection<IExecutionEntity> FindInactiveExecutionsByActivityIdAndProcessInstanceId(string activityId, string processInstanceId)
        {
            Dictionary<string, object> @params = new Dictionary<string, object>(3)
            {
                ["activityId"] = activityId,
                ["processInstanceId"] = processInstanceId,
                ["isActive"] = false
            };

            if (performanceSettings.EnableEagerExecutionTreeFetching)
            {
                FindByIdAndFetchExecutionTree(processInstanceId);
                return GetListFromCache(inactiveExecutionsInActivityAndProcInstMatcher, @params);
            }
            else
            {
                return GetList("selectInactiveExecutionsInActivityAndProcessInstance", @params, inactiveExecutionsInActivityAndProcInstMatcher, true);
            }
        }

        public virtual IList<string> FindProcessInstanceIdsByProcessDefinitionId(string processDefinitionId)
        {
            return DbSqlSession.SelectList<ExecutionEntityImpl, string>("selectProcessInstanceIdsByProcessDefinitionId", new { processDefinitionId }, false);
        }

        public virtual long FindExecutionCountByQueryCriteria(IExecutionQuery executionQuery)
        {
            return DbSqlSession.SelectOne<ExecutionEntityImpl, long?>("selectExecutionCountByQueryCriteria", executionQuery).GetValueOrDefault();


        }

        public virtual IList<IExecutionEntity> FindExecutionsByQueryCriteria(IExecutionQuery executionQuery, Page page)
        {
            return DbSqlSession.SelectList<ExecutionEntityImpl, IExecutionEntity>("selectExecutionsByQueryCriteria", executionQuery, page, !performanceSettings.EnableEagerExecutionTreeFetching); // False -> executions should not be cached if using executionTreeFetching
        }

        public virtual long FindProcessInstanceCountByQueryCriteria(IProcessInstanceQuery executionQuery)
        {
            return DbSqlSession.SelectOne<ExecutionEntityImpl, long?>("selectProcessInstanceCountByQueryCriteria", executionQuery).GetValueOrDefault();
        }

        public virtual IList<IProcessInstance> FindProcessInstanceByQueryCriteria(IProcessInstanceQuery executionQuery)
        {
            return DbSqlSession.SelectList<ExecutionEntityImpl, IProcessInstance>("selectProcessInstanceByQueryCriteria", executionQuery, !performanceSettings.EnableEagerExecutionTreeFetching); // False -> executions should not be cached if using executionTreeFetching
        }

        public virtual IList<IProcessInstance> FindProcessInstanceAndVariablesByQueryCriteria(IProcessInstanceQuery query)
        {
            ProcessInstanceQueryImpl executionQuery = query as ProcessInstanceQueryImpl;

            // paging doesn't work for combining process instances and variables due
            // to an outer join, so doing it in-memory
            if (executionQuery.FirstResult < 0 || executionQuery.MaxResults <= 0)
            {
                return new List<IProcessInstance>();
            }

            int firstResult = executionQuery.FirstResult;
            int maxResults = executionQuery.MaxResults;

            // setting max results, limit to 20000 results for performance reasons
            if (executionQuery.ProcessInstanceVariablesLimit != null)
            {
                executionQuery.MaxResults = executionQuery.ProcessInstanceVariablesLimit.GetValueOrDefault();
            }
            else
            {
                executionQuery.MaxResults = ProcessEngineConfiguration.ExecutionQueryLimit;
            }
            executionQuery.FirstResult = 0;

            IList<IProcessInstance> instanceList = DbSqlSession.SelectListWithRawParameterWithoutFilter<ExecutionEntityImpl, IProcessInstance>("selectProcessInstanceWithVariablesByQueryCriteria", executionQuery, executionQuery.FirstResult, executionQuery.MaxResults);

            if (instanceList != null && instanceList.Count > 0)
            {
                if (firstResult > 0)
                {
                    if (firstResult <= instanceList.Count)
                    {
                        int toIndex = firstResult + Math.Min(maxResults, instanceList.Count - firstResult);
                        return instanceList.Skip(firstResult).Take(toIndex).ToList();
                    }
                    else
                    {
                        return new List<IProcessInstance>();
                    }
                }
                else
                {
                    int toIndex = Math.Min(maxResults, instanceList.Count);
                    return instanceList.Skip(0).Take(toIndex).ToList();
                }
            }
            return new List<IProcessInstance>();
        }

        public virtual IList<IExecution> FindExecutionsByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults)
        {
            return DbSqlSession.SelectListWithRawParameter<ExecutionEntityImpl, IExecution>("selectExecutionByNativeQuery", parameterMap, firstResult, maxResults);
        }

        public virtual IList<IProcessInstance> FindProcessInstanceByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults)
        {
            return DbSqlSession.SelectListWithRawParameter<ExecutionEntityImpl, IProcessInstance>("selectExecutionByNativeQuery", parameterMap, firstResult, maxResults);
        }

        public virtual long FindExecutionCountByNativeQuery(IDictionary<string, object> parameterMap)
        {
            return DbSqlSession.SelectOne<ExecutionEntityImpl, long?>("selectExecutionCountByNativeQuery", parameterMap).GetValueOrDefault();
        }

        public virtual void UpdateExecutionTenantIdForDeployment(string deploymentId, string newTenantId)
        {
            Dictionary<string, object> @params = new Dictionary<string, object>
            {
                ["deploymentId"] = deploymentId,
                ["tenantId"] = newTenantId
            };
            DbSqlSession.Update<ExecutionEntityImpl>("updateExecutionTenantIdForDeployment", @params);
        }

        public virtual void UpdateProcessInstanceLockTime(string processInstanceId, DateTime lockDate, DateTime expirationTime)
        {
            Dictionary<string, object> @params = new Dictionary<string, object>
            {
                ["id"] = processInstanceId,
                ["lockTime"] = lockDate,
                ["expirationTime"] = expirationTime
            };

            int result = DbSqlSession.Update<ExecutionEntityImpl>("updateProcessInstanceLockTime", @params);
            if (result == 0)
            {
                throw new ActivitiOptimisticLockingException("Could not lock process instance");
            }
        }

        public virtual void UpdateAllExecutionRelatedEntityCountFlags(bool newValue)
        {
            DbSqlSession.Update<ExecutionEntityImpl>("updateExecutionRelatedEntityCountEnabled", new { isCountEnabled = newValue });
        }

        public virtual void ClearProcessInstanceLockTime(string processInstanceId)
        {
            DbSqlSession.Update<ExecutionEntityImpl>("clearProcessInstanceLockTime", new
            {
                id = processInstanceId
            });
        }
    }

}