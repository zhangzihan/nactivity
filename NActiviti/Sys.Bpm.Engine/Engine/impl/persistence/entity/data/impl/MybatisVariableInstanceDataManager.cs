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
namespace Sys.Workflow.Engine.Impl.Persistence.Entity.Data.Impl
{

    using Sys.Workflow.Engine.Impl.Cfg;
    using Sys.Workflow.Engine.Impl.Persistence.Entity.Data.Impl.Cachematcher;

    /// <summary>
    /// 
    /// </summary>
    public class MybatisVariableInstanceDataManager : AbstractDataManager<IVariableInstanceEntity>, IVariableInstanceDataManager
    {
        /// <summary>
        /// 
        /// </summary>
        protected internal ICachedEntityMatcher<IVariableInstanceEntity> variableInstanceEntity = new VariableByExecutionIdMatcher();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="processEngineConfiguration"></param>
        public MybatisVariableInstanceDataManager(ProcessEngineConfigurationImpl processEngineConfiguration) : base(processEngineConfiguration)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        public override Type ManagedEntityClass
        {
            get
            {
                return typeof(VariableInstanceEntityImpl);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override IVariableInstanceEntity Create()
        {
            VariableInstanceEntityImpl variableInstanceEntity = new VariableInstanceEntityImpl
            {
                Revision = 0 // For backwards compatibility, variables / HistoricVariableUpdate assumes revision 0 for the first time
            };
            return variableInstanceEntity;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        public virtual IList<IVariableInstanceEntity> FindVariableInstancesByTaskId(string taskId)
        {
            return DbSqlSession.SelectList<VariableInstanceEntityImpl, IVariableInstanceEntity>("selectVariablesByTaskId", new { taskId });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskIds"></param>
        /// <returns></returns>
        public virtual IList<IVariableInstanceEntity> FindVariableInstancesByTaskIds(string[] taskIds)
        {
            return DbSqlSession.SelectList<VariableInstanceEntityImpl, IVariableInstanceEntity>("selectVariablesByTaskIds", new { ids = taskIds });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="executionId"></param>
        /// <returns></returns>
        public virtual IList<IVariableInstanceEntity> FindVariableInstancesByExecutionId(string executionId)
        {
            return (IList<IVariableInstanceEntity>)GetList("selectVariablesByExecutionId", new { executionId }, variableInstanceEntity, true);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="executionIds"></param>
        /// <returns></returns>
        public virtual IList<IVariableInstanceEntity> FindVariableInstancesByExecutionIds(string[] executionIds)
        {
            return DbSqlSession.SelectList<VariableInstanceEntityImpl, IVariableInstanceEntity>("selectVariablesByExecutionIds", new { executionIds });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="executionId"></param>
        /// <param name="variableName"></param>
        /// <returns></returns>
        public virtual IVariableInstanceEntity FindVariableInstanceByExecutionAndName(string executionId, string variableName)
        {
            IDictionary<string, string> @params = new Dictionary<string, string>(2)
            {
                ["executionId"] = executionId,
                ["name"] = variableName
            };
            return DbSqlSession.SelectOne<VariableInstanceEntityImpl, IVariableInstanceEntity>("selectVariableInstanceByExecutionAndName", @params);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="executionId"></param>
        /// <param name="names"></param>
        /// <returns></returns>
        public virtual IList<IVariableInstanceEntity> FindVariableInstancesByExecutionAndNames(string executionId, IEnumerable<string> names)
        {
            return DbSqlSession.SelectList<VariableInstanceEntityImpl, IVariableInstanceEntity>("selectVariableInstancesByExecutionAndNames", new { executionId, names });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="variableName"></param>
        /// <returns></returns>
        public virtual IVariableInstanceEntity FindVariableInstanceByTaskAndName(string taskId, string variableName)
        {
            IDictionary<string, string> @params = new Dictionary<string, string>(2)
            {
                ["taskId"] = taskId,
                ["name"] = variableName
            };
            return DbSqlSession.SelectOne<VariableInstanceEntityImpl, IVariableInstanceEntity>("selectVariableInstanceByTaskAndName", @params);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="names"></param>
        /// <returns></returns>
        public virtual IList<IVariableInstanceEntity> FindVariableInstancesByTaskAndNames(string taskId, IEnumerable<string> names)
        {
            IDictionary<string, object> @params = new Dictionary<string, object>(2)
            {
                ["taskId"] = taskId,
                ["names"] = names
            };
            return DbSqlSession.SelectList<VariableInstanceEntityImpl, IVariableInstanceEntity>("selectVariableInstancesByTaskAndNames", @params);
        }
    }
}