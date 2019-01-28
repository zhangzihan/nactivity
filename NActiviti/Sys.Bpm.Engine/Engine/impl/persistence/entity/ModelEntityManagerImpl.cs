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

        public override IModelEntity findById<IModelEntity>(KeyValuePair<string, object> entityId)
        {
            return modelDataManager.findById<IModelEntity>(entityId);
        }

        public override void insert(IModelEntity model)
        {
            ((IModelEntity)model).CreateTime = Clock.CurrentTime;
            ((IModelEntity)model).LastUpdateTime = Clock.CurrentTime;

            base.insert(model);
        }

        public virtual void updateModel(IModelEntity updatedModel)
        {
            updatedModel.LastUpdateTime = Clock.CurrentTime;
            update(updatedModel);
        }

        public override void delete(KeyValuePair<string, object> modelId)
        {
            IModelEntity modelEntity = findById<IModelEntity>(modelId);
            base.delete(modelEntity);
            deleteEditorSource(modelEntity);
            deleteEditorSourceExtra(modelEntity);
        }

        public virtual void insertEditorSourceForModel(string modelId, byte[] modelSource)
        {
            IModelEntity model = findById<IModelEntity>(new KeyValuePair<string, object>("id", modelId));
            if (model != null)
            {
                ByteArrayRef @ref = new ByteArrayRef(model.EditorSourceValueId);
                @ref.setValue("source", modelSource);

                if (ReferenceEquals(model.EditorSourceValueId, null))
                {
                    model.EditorSourceValueId = @ref.Id;
                    updateModel(model);
                }
            }
        }

        public virtual void deleteEditorSource(IModelEntity model)
        {
            if (!ReferenceEquals(model.EditorSourceValueId, null))
            {
                ByteArrayRef @ref = new ByteArrayRef(model.EditorSourceValueId);
                @ref.delete();
            }
        }

        public virtual void deleteEditorSourceExtra(IModelEntity model)
        {
            if (!ReferenceEquals(model.EditorSourceExtraValueId, null))
            {
                ByteArrayRef @ref = new ByteArrayRef(model.EditorSourceExtraValueId);
                @ref.delete();
            }
        }

        public virtual void insertEditorSourceExtraForModel(string modelId, byte[] modelSource)
        {
            IModelEntity model = findById<IModelEntity>(new KeyValuePair<string, object>("id", modelId));
            if (model != null)
            {
                ByteArrayRef @ref = new ByteArrayRef(model.EditorSourceExtraValueId);
                @ref.setValue("source-extra", modelSource);

                if (ReferenceEquals(model.EditorSourceExtraValueId, null))
                {
                    model.EditorSourceExtraValueId = @ref.Id;
                    updateModel(model);
                }
            }
        }

        public virtual IList<IModel> findModelsByQueryCriteria(ModelQueryImpl query, Page page)
        {
            return modelDataManager.findModelsByQueryCriteria(query, page);
        }

        public virtual long findModelCountByQueryCriteria(ModelQueryImpl query)
        {
            return modelDataManager.findModelCountByQueryCriteria(query);
        }

        public virtual byte[] findEditorSourceByModelId(string modelId)
        {
            IModelEntity model = findById<IModelEntity>(new KeyValuePair<string, object>("id", modelId));
            if (model == null || ReferenceEquals(model.EditorSourceValueId, null))
            {
                return null;
            }

            ByteArrayRef @ref = new ByteArrayRef(model.EditorSourceValueId);
            return @ref.Bytes;
        }

        public virtual byte[] findEditorSourceExtraByModelId(string modelId)
        {
            IModelEntity model = findById<IModelEntity>(new KeyValuePair<string, object>("id", modelId));
            if (model == null || ReferenceEquals(model.EditorSourceExtraValueId, null))
            {
                return null;
            }

            ByteArrayRef @ref = new ByteArrayRef(model.EditorSourceExtraValueId);
            return @ref.Bytes;
        }

        public virtual IList<IModel> findModelsByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults)
        {
            return modelDataManager.findModelsByNativeQuery(parameterMap, firstResult, maxResults);
        }

        public virtual long findModelCountByNativeQuery(IDictionary<string, object> parameterMap)
        {
            return modelDataManager.findModelCountByNativeQuery(parameterMap);
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