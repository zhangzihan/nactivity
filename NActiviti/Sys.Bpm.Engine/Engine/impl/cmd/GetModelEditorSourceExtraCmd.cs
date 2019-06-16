using System;

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
namespace Sys.Workflow.engine.impl.cmd
{

    using Sys.Workflow.engine.impl.interceptor;

    /// 
    [Serializable]
    public class GetModelEditorSourceExtraCmd : ICommand<byte[]>
    {

        private const long serialVersionUID = 1L;
        protected internal string modelId;

        public GetModelEditorSourceExtraCmd(string modelId)
        {
            this.modelId = modelId;
        }

        public virtual byte[] Execute(ICommandContext commandContext)
        {
            if (ReferenceEquals(modelId, null))
            {
                throw new ActivitiIllegalArgumentException("modelId is null");
            }

            byte[] bytes = commandContext.ModelEntityManager.FindEditorSourceExtraByModelId(modelId);

            return bytes;
        }

    }

}