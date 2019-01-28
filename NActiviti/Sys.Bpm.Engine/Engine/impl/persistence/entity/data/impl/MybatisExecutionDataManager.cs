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

        public override IExecutionEntity create()
        {
            return ExecutionEntityImpl.createWithEmptyRelationshipCollections();
        }

        public override IExecutionEntity findById<IExecutionEntity>(KeyValuePair<string, object> id)
        {
            if (performanceSettings.EnableEagerExecutionTreeFetching)
            {
                return (IExecutionEntity)findByIdAndFetchExecutionTree(id.Value?.ToString());
            }
            else
            {
                return base.findById<IExecutionEntity>(id);
            }
        }

        protected internal virtual IExecutionEntity findByIdAndFetchExecutionTree(string executionId)
        {

            // If it's in the cache, the tree must have been fetched before
            IExecutionEntity cachedEntity = (IExecutionEntity)EntityCache.findInCache(ManagedEntityClass, executionId);
            if (cachedEntity != null)
            {
                return cachedEntity;
            }

            // Fetches execution tree. This will store them in the cache.
            IList<IExecutionEntity> executionEntities = (IList<IExecutionEntity>)getList("selectExecutionsWithSameRootProcessInstanceId", new { executionId }, executionsWithSameRootProcessInstanceIdMatcher, true);

            foreach (IExecutionEntity executionEntity in executionEntities)
            {
                if (executionId.Equals(executionEntity.Id))
                {
                    return executionEntity;
                }
            }
            return null;
        }

        public virtual IExecutionEntity findSubProcessInstanceBySuperExecutionId(string superExecutionId)
        {
            if (performanceSettings.EnableEagerExecutionTreeFetching)
            {
                findByIdAndFetchExecutionTree(superExecutionId);
            }

            return getEntity("selectSubProcessInstanceBySuperExecutionId", new { superExecutionId }, subProcessInstanceBySuperExecutionIdMatcher, !performanceSettings.EnableEagerExecutionTreeFetching);
        }

        public virtual IList<IExecutionEntity> findChildExecutionsByParentExecutionId(string parentExecutionId)
        {
            if (performanceSettings.EnableEagerExecutionTreeFetching)
            {
                findByIdAndFetchExecutionTree(parentExecutionId);
                return getListFromCache(executionsByParentIdMatcher, parentExecutionId);
            }
            else
            {
                return (IList<IExecutionEntity>)getList("selectExecutionsByParentExecutionId", new { parentExecutionId }, executionsByParentIdMatcher, true);
            }
        }

        public virtual IList<IExecutionEntity> findChildExecutionsByProcessInstanceId(string processInstanceId)
        {
            if (performanceSettings.EnableEagerExecutionTreeFetching)
            {
                findByIdAndFetchExecutionTree(processInstanceId);
                return getListFromCache(executionsByProcessInstanceIdMatcher, processInstanceId);
            }
            else
            {
                return (IList<IExecutionEntity>)getList("selectChildExecutionsByProcessInstanceId", new { processInstanceId }, executionsByProcessInstanceIdMatcher, true);
            }
        }

        public virtual IList<IExecutionEntity> findExecutionsByParentExecutionAndActivityIds(string parentExecutionId, ICollection<string> activityIds)
        {
            IDictionary<string, object> parameters = new Dictionary<string, object>(2);
            parameters["parentExecutionId"] = parentExecutionId;
            parameters["activityIds"] = activityIds;

            if (performanceSettings.EnableEagerExecutionTreeFetching)
            {
                findByIdAndFetchExecutionTree(parentExecutionId);
                return getListFromCache(executionsByParentExecutionIdAndActivityIdEntityMatcher, parameters);
            }
            else
            {
                return (IList<IExecutionEntity>)getList("selectExecutionsByParentExecutionAndActivityIds", parameters, executionsByParentExecutionIdAndActivityIdEntityMatcher, true);
            }
        }

        public virtual IList<IExecutionEntity> findExecutionsByRootProcessInstanceId(string rootProcessInstanceId)
        {
            if (performanceSettings.EnableEagerExecutionTreeFetching)
            {
                findByIdAndFetchExecutionTree(rootProcessInstanceId);
                return getListFromCache(executionsByRootProcessInstanceMatcher, rootProcessInstanceId);
            }
            else
            {
                return (IList<IExecutionEntity>)getList("selectExecutionsByRootProcessInstanceId", new { rootProcessInstanceId }, executionsByRootProcessInstanceMatcher, true);
            }
        }

        public virtual IList<IExecutionEntity> findExecutionsByProcessInstanceId(string processInstanceId)
        {
            if (performanceSettings.EnableEagerExecutionTreeFetching)
            {
                findByIdAndFetchExecutionTree(processInstanceId);
                return getListFromCache(executionByProcessInstanceMatcher, processInstanceId);
            }
            else
            {
                return (IList<IExecutionEntity>)getList("selectExecutionsByProcessInstanceId", new { processInstanceId }, executionByProcessInstanceMatcher, true);
            }
        }

        public virtual ICollection<IExecutionEntity> findInactiveExecutionsByProcessInstanceId(string processInstanceId)
        {
            Dictionary<string, object> @params = new Dictionary<string, object>(2);
            @params["processInstanceId"] = processInstanceId;
            @params["isActive"] = false;

            if (performanceSettings.EnableEagerExecutionTreeFetching)
            {
                findByIdAndFetchExecutionTree(processInstanceId);
                return getListFromCache(inactiveExecutionsByProcInstMatcher, @params);
            }
            else
            {
                return getList("selectInactiveExecutionsForProcessInstance", @params, inactiveExecutionsByProcInstMatcher, true);
            }
        }

        public virtual ICollection<IExecutionEntity> findInactiveExecutionsByActivityIdAndProcessInstanceId(string activityId, string processInstanceId)
        {
            Dictionary<string, object> @params = new Dictionary<string, object>(3);
            @params["activityId"] = activityId;
            @params["processInstanceId"] = processInstanceId;
            @params["isActive"] = false;

            if (performanceSettings.EnableEagerExecutionTreeFetching)
            {
                findByIdAndFetchExecutionTree(processInstanceId);
                return getListFromCache(inactiveExecutionsInActivityAndProcInstMatcher, @params);
            }
            else
            {
                return getList("selectInactiveExecutionsInActivityAndProcessInstance", @params, inactiveExecutionsInActivityAndProcInstMatcher, true);
            }
        }

        public virtual IList<string> findProcessInstanceIdsByProcessDefinitionId(string processDefinitionId)
        {
            return DbSqlSession.selectList<ExecutionEntityImpl, string>("selectProcessInstanceIdsByProcessDefinitionId", new { processDefinitionId }, false);
        }

        public virtual long findExecutionCountByQueryCriteria(ExecutionQueryImpl executionQuery)
        {
            return DbSqlSession.selectOne<ExecutionEntityImpl, long?>("selectExecutionCountByQueryCriteria", executionQuery).GetValueOrDefault();


        }

        public virtual IList<IExecutionEntity> findExecutionsByQueryCriteria(ExecutionQueryImpl executionQuery, Page page)
        {
            return DbSqlSession.selectList<ExecutionEntityImpl, IExecutionEntity>("selectExecutionsByQueryCriteria", executionQuery, page, !performanceSettings.EnableEagerExecutionTreeFetching); // False -> executions should not be cached if using executionTreeFetching
        }

        public virtual long findProcessInstanceCountByQueryCriteria(ProcessInstanceQueryImpl executionQuery)
        {
            return DbSqlSession.selectOne<ExecutionEntityImpl, long?>("selectProcessInstanceCountByQueryCriteria", executionQuery).GetValueOrDefault();
        }

        public virtual IList<IProcessInstance> findProcessInstanceByQueryCriteria(ProcessInstanceQueryImpl executionQuery)
        {
            return DbSqlSession.selectList<ExecutionEntityImpl, IProcessInstance>("selectProcessInstanceByQueryCriteria", executionQuery, !performanceSettings.EnableEagerExecutionTreeFetching); // False -> executions should not be cached if using executionTreeFetching
        }

        public virtual IList<IProcessInstance> findProcessInstanceAndVariablesByQueryCriteria(ProcessInstanceQueryImpl executionQuery)
        {
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

            IList<IProcessInstance> instanceList = DbSqlSession.selectListWithRawParameterWithoutFilter<ExecutionEntityImpl, IProcessInstance>("selectProcessInstanceWithVariablesByQueryCriteria", executionQuery, executionQuery.FirstResult, executionQuery.MaxResults);

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

        public virtual IList<IExecution> findExecutionsByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults)
        {
            return DbSqlSession.selectListWithRawParameter<ExecutionEntityImpl, IExecution>("selectExecutionByNativeQuery", parameterMap, firstResult, maxResults);
        }

        public virtual IList<IProcessInstance> findProcessInstanceByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults)
        {
            return DbSqlSession.selectListWithRawParameter<ExecutionEntityImpl, IProcessInstance>("selectExecutionByNativeQuery", parameterMap, firstResult, maxResults);
        }

        public virtual long findExecutionCountByNativeQuery(IDictionary<string, object> parameterMap)
        {
            return DbSqlSession.selectOne<ExecutionEntityImpl, long?>("selectExecutionCountByNativeQuery", parameterMap).GetValueOrDefault();
        }

        public virtual void updateExecutionTenantIdForDeployment(string deploymentId, string newTenantId)
        {
            Dictionary<string, object> @params = new Dictionary<string, object>();
            @params["deploymentId"] = deploymentId;
            @params["tenantId"] = newTenantId;
            DbSqlSession.update<ExecutionEntityImpl>("updateExecutionTenantIdForDeployment", @params);
        }

        public virtual void updateProcessInstanceLockTime(string processInstanceId, DateTime lockDate, DateTime expirationTime)
        {
            Dictionary<string, object> @params = new Dictionary<string, object>();
            @params["id"] = processInstanceId;
            @params["lockTime"] = lockDate;
            @params["expirationTime"] = expirationTime;

            int result = DbSqlSession.update<ExecutionEntityImpl>("updateProcessInstanceLockTime", @params);
            if (result == 0)
            {
                throw new ActivitiOptimisticLockingException("Could not lock process instance");
            }
        }

        public virtual void updateAllExecutionRelatedEntityCountFlags(bool newValue)
        {
            DbSqlSession.update<ExecutionEntityImpl>("updateExecutionRelatedEntityCountEnabled", new { isCountEnabled = newValue });
        }

        public virtual void clearProcessInstanceLockTime(string processInstanceId)
        {
            Dictionary<string, object> @params = new Dictionary<string, object>();
            @params["id"] = processInstanceId;
            DbSqlSession.update<ExecutionEntityImpl>("clearProcessInstanceLockTime", @params);
        }
    }

}