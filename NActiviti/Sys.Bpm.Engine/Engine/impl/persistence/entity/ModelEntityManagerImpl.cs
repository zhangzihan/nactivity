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

    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.persistence.entity.data;
    using org.activiti.engine.repository;

    /// 
    /// 
    public class ModelEntityManagerImpl : AbstractEntityManager<IModelEntity>, IModelEntityManager
    {

        protected internal IModelDataManager modelDataManager;

        public ModelEntityManagerImpl(ProcessEngineConfigurationImpl processEngineConfiguration, IModelDataManager modelDataManager) : base(processEngineConfiguration)
        {
            this.modelDataManager = modelDataManager;
        }

        protected internal override IDataManager<IModelEntity> DataManager
        {
            get
            {
                return modelDataManager;
            }
        }

        public override IModelEntity FindById<IModelEntity>(KeyValuePair<string, object> entityId)
        {
            return modelDataManager.FindById<IModelEntity>(entityId);
        }

        public override void Insert(IModelEntity model)
        {
            ((IModelEntity)model).CreateTime = Clock.CurrentTime;
            ((IModelEntity)model).LastUpdateTime = Clock.CurrentTime;

            base.Insert(model);
        }

        public virtual void UpdateModel(IModelEntity updatedModel)
        {
            updatedModel.LastUpdateTime = Clock.CurrentTime;
            Update(updatedModel);
        }

        public override void Delete(KeyValuePair<string, object> modelId)
        {
            IModelEntity modelEntity = FindById<IModelEntity>(modelId);
            base.Delete(modelEntity);
            DeleteEditorSource(modelEntity);
            DeleteEditorSourceExtra(modelEntity);
        }

        public virtual void InsertEditorSourceForModel(string modelId, byte[] modelSource)
        {
            IModelEntity model = FindById<IModelEntity>(new KeyValuePair<string, object>("id", modelId));
            if (model != null)
            {
                ByteArrayRef @ref = new ByteArrayRef(model.EditorSourceValueId);
                @ref.SetValue("source", modelSource);

                if (model.EditorSourceValueId is null)
                {
                    model.EditorSourceValueId = @ref.Id;
                    UpdateModel(model);
                }
            }
        }

        public virtual void DeleteEditorSource(IModelEntity model)
        {
            if (!(model.EditorSourceValueId is null))
            {
                ByteArrayRef @ref = new ByteArrayRef(model.EditorSourceValueId);
                @ref.Delete();
            }
        }

        public virtual void DeleteEditorSourceExtra(IModelEntity model)
        {
            if (!(model.EditorSourceExtraValueId is null))
            {
                ByteArrayRef @ref = new ByteArrayRef(model.EditorSourceExtraValueId);
                @ref.Delete();
            }
        }

        public virtual void InsertEditorSourceExtraForModel(string modelId, byte[] modelSource)
        {
            IModelEntity model = FindById<IModelEntity>(modelId);
            if (model != null)
            {
                ByteArrayRef @ref = new ByteArrayRef(model.EditorSourceExtraValueId);
                @ref.SetValue("source-extra", modelSource);

                if (model.EditorSourceExtraValueId is null)
                {
                    model.EditorSourceExtraValueId = @ref.Id;
                    UpdateModel(model);
                }
            }
        }

        public virtual IList<IModel> FindModelsByQueryCriteria(IModelQuery query, Page page)
        {
            return modelDataManager.FindModelsByQueryCriteria(query, page);
        }

        public virtual long FindModelCountByQueryCriteria(IModelQuery query)
        {
            return modelDataManager.FindModelCountByQueryCriteria(query);
        }

        public virtual byte[] FindEditorSourceByModelId(string modelId)
        {
            IModelEntity model = FindById<IModelEntity>(modelId);
            if (model == null || model.EditorSourceValueId is null)
            {
                return null;
            }

            ByteArrayRef @ref = new ByteArrayRef(model.EditorSourceValueId);
            return @ref.Bytes;
        }

        public virtual byte[] FindEditorSourceExtraByModelId(string modelId)
        {
            IModelEntity model = FindById<IModelEntity>(modelId);
            if (model == null || model.EditorSourceExtraValueId is null)
            {
                return null;
            }

            ByteArrayRef @ref = new ByteArrayRef(model.EditorSourceExtraValueId);
            return @ref.Bytes;
        }

        public virtual IList<IModel> FindModelsByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults)
        {
            return modelDataManager.FindModelsByNativeQuery(parameterMap, firstResult, maxResults);
        }

        public virtual long FindModelCountByNativeQuery(IDictionary<string, object> parameterMap)
        {
            return modelDataManager.FindModelCountByNativeQuery(parameterMap);
        }

        public virtual IModelDataManager ModelDataManager
        {
            get
            {
                return modelDataManager;
            }
            set
            {
                this.modelDataManager = value;
            }
        }
    }
}