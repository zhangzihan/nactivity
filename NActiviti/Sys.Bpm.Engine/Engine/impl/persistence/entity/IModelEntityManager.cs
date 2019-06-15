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
namespace org.activiti.engine.impl.persistence.entity
{

    using org.activiti.engine.repository;

    /// 
    public interface IModelEntityManager : IEntityManager<IModelEntity>
    {

        void InsertEditorSourceForModel(string modelId, byte[] modelSource);

        void InsertEditorSourceExtraForModel(string modelId, byte[] modelSource);

        IList<IModel> FindModelsByQueryCriteria(IModelQuery query, Page page);

        long FindModelCountByQueryCriteria(IModelQuery query);

        byte[] FindEditorSourceByModelId(string modelId);

        byte[] FindEditorSourceExtraByModelId(string modelId);

        IList<IModel> FindModelsByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults);

        long FindModelCountByNativeQuery(IDictionary<string, object> parameterMap);

        void UpdateModel(IModelEntity updatedModel);

        void DeleteEditorSource(IModelEntity model);

        void DeleteEditorSourceExtra(IModelEntity model);
    }
}