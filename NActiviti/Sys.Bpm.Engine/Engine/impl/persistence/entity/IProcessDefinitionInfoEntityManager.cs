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
using System.Collections.Generic;

namespace org.activiti.engine.impl.persistence.entity
{

    /// 
    public interface IProcessDefinitionInfoEntityManager : IEntityManager<IProcessDefinitionInfoEntity>
    {

        void insertProcessDefinitionInfo(IProcessDefinitionInfoEntity processDefinitionInfo);

        void updateProcessDefinitionInfo(IProcessDefinitionInfoEntity updatedProcessDefinitionInfo);

        void deleteProcessDefinitionInfo(string processDefinitionId);

        void updateInfoJson(string id, byte[] json);

        void deleteInfoJson(IProcessDefinitionInfoEntity processDefinitionInfo);

        new IProcessDefinitionInfoEntity findById<IProcessDefinitionInfoEntity>(KeyValuePair<string, object> id);

        IProcessDefinitionInfoEntity findProcessDefinitionInfoByProcessDefinitionId(string processDefinitionId);

        byte[] findInfoJsonById(string infoJsonId);

    }
}