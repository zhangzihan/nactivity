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

    /// 
    public class MybatisVariableInstanceDataManager : AbstractDataManager<IVariableInstanceEntity>, IVariableInstanceDataManager
    {

        protected internal ICachedEntityMatcher<IVariableInstanceEntity> variableInstanceEntity = new VariableByExecutionIdMatcher();

        public MybatisVariableInstanceDataManager(ProcessEngineConfigurationImpl processEngineConfiguration) : base(processEngineConfiguration)
        {
        }

        public override Type ManagedEntityClass
        {
            get
            {
                return typeof(VariableInstanceEntityImpl);
            }
        }

        public override IVariableInstanceEntity create()
        {
            VariableInstanceEntityImpl variableInstanceEntity = new VariableInstanceEntityImpl();
            variableInstanceEntity.Revision = 0; // For backwards compatibility, variables / HistoricVariableUpdate assumes revision 0 for the first time
            return variableInstanceEntity;
        }

        public virtual IList<IVariableInstanceEntity> findVariableInstancesByTaskId(string taskId)
        {
            return DbSqlSession.selectList<VariableInstanceEntityImpl, IVariableInstanceEntity>("selectVariablesByTaskId", new KeyValuePair<string, object>("taskId", taskId));
        }

        public virtual IList<IVariableInstanceEntity> findVariableInstancesByTaskIds(ISet<string> taskIds)
        {
            return DbSqlSession.selectList<VariableInstanceEntityImpl, IVariableInstanceEntity>("selectVariablesByTaskIds", new { ids = taskIds });
        }

        public virtual IList<IVariableInstanceEntity> findVariableInstancesByExecutionId(string executionId)
        {
            return (IList<IVariableInstanceEntity>)getList("selectVariablesByExecutionId", new KeyValuePair<string, object>("executionId", executionId), variableInstanceEntity, true);
        }

        public virtual IList<IVariableInstanceEntity> findVariableInstancesByExecutionIds(ISet<string> executionIds)
        {
            return DbSqlSession.selectList<VariableInstanceEntityImpl, IVariableInstanceEntity>("selectVariablesByExecutionIds", new { executionIds });
        }

        public virtual IVariableInstanceEntity findVariableInstanceByExecutionAndName(string executionId, string variableName)
        {
            IDictionary<string, string> @params = new Dictionary<string, string>(2);
            @params["executionId"] = executionId;
            @params["name"] = variableName;
            return DbSqlSession.selectOne<VariableInstanceEntityImpl, IVariableInstanceEntity>("selectVariableInstanceByExecutionAndName", @params);
        }

        public virtual IList<IVariableInstanceEntity> findVariableInstancesByExecutionAndNames(string executionId, ICollection<string> names)
        {
            IDictionary<string, object> @params = new Dictionary<string, object>(2);
            @params["executionId"] = executionId;
            @params["names"] = names;
            return DbSqlSession.selectList<VariableInstanceEntityImpl, IVariableInstanceEntity>("selectVariableInstancesByExecutionAndNames", @params);
        }

        public virtual IVariableInstanceEntity findVariableInstanceByTaskAndName(string taskId, string variableName)
        {
            IDictionary<string, string> @params = new Dictionary<string, string>(2);
            @params["taskId"] = taskId;
            @params["name"] = variableName;
            return DbSqlSession.selectOne<VariableInstanceEntityImpl, IVariableInstanceEntity>("selectVariableInstanceByTaskAndName", @params);
        }

        public virtual IList<IVariableInstanceEntity> findVariableInstancesByTaskAndNames(string taskId, ICollection<string> names)
        {
            IDictionary<string, object> @params = new Dictionary<string, object>(2);
            @params["taskId"] = taskId;
            @params["names"] = names;
            return DbSqlSession.selectList<VariableInstanceEntityImpl, IVariableInstanceEntity>("selectVariableInstancesByTaskAndNames", @params);
        }

    }

}