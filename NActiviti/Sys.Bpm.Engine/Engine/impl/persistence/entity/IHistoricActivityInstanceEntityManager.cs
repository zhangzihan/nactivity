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
namespace Sys.Workflow.Engine.Impl.Persistence.Entity
{

    using Sys.Workflow.Engine.History;

    /// 
    public interface IHistoricActivityInstanceEntityManager : IEntityManager<IHistoricActivityInstanceEntity>
    {

        IList<IHistoricActivityInstanceEntity> FindUnfinishedHistoricActivityInstancesByExecutionAndActivityId(string executionId, string activityId);

        IList<IHistoricActivityInstanceEntity> FindUnfinishedHistoricActivityInstancesByProcessInstanceId(string processInstanceId);

        long FindHistoricActivityInstanceCountByQueryCriteria(IHistoricActivityInstanceQuery historicActivityInstanceQuery);

        IList<IHistoricActivityInstance> FindHistoricActivityInstancesByQueryCriteria(IHistoricActivityInstanceQuery historicActivityInstanceQuery, Page page);

        IList<IHistoricActivityInstance> FindHistoricActivityInstancesByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults);

        long FindHistoricActivityInstanceCountByNativeQuery(IDictionary<string, object> parameterMap);

        void DeleteHistoricActivityInstancesByProcessInstanceId(string historicProcessInstanceId);

    }
}