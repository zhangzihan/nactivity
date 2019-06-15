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
namespace org.activiti.engine.impl.cmd
{

    using org.activiti.engine.impl.interceptor;
    using System.Collections.Generic;

    /// 
    [Serializable]
    public class DeleteModelCmd : ICommand<object>
    {

        private const long serialVersionUID = 1L;
        internal string modelId;

        public DeleteModelCmd(string modelId)
        {
            this.modelId = modelId;
        }

        public virtual object Execute(ICommandContext commandContext)
        {
            if (modelId is null)
            {
                throw new ActivitiIllegalArgumentException("modelId is null");
            }
            commandContext.ModelEntityManager.Delete(new KeyValuePair<string, object>("id", modelId));

            return null;
        }

    }

}