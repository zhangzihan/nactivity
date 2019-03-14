using org.activiti.engine.repository;
using System.Collections.Generic;

/*
 * Licensed under the Apache License, Version 2.0 (the "License");
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
 *
 */

namespace org.activiti.cloud.services.api.model.converter
{
    /// <summary>
    /// 
    /// </summary>
    public class ProcessDefinitionConverter : IModelConverter<IProcessDefinition, ProcessDefinition>
    {
        private readonly ListConverter listConverter;


        /// <summary>
        /// 
        /// </summary>
        public ProcessDefinitionConverter(ListConverter listConverter)
        {
            this.listConverter = listConverter;
        }

        /// <summary>
        /// 
        /// </summary>

        public virtual ProcessDefinition from(IProcessDefinition source)
        {
            ProcessDefinition processDefinition = null;
            if (source != null)
            {
                processDefinition = new ProcessDefinition(source.Id,
                    source.Name,
                    source.Description,
                    source.Version,
                    source.Category,
                    source.Key,
                    source.DeploymentId,
                    source.TenantId,
                    source.BusinessKey,
                    source.BusinessPath,
                    source.StartForm);
            }
            return processDefinition;
        }

        /// <summary>
        /// 
        /// </summary>

        public virtual IList<ProcessDefinition> from(IList<IProcessDefinition> processDefinitions)
        {
            return listConverter.from(processDefinitions, this);
        }
    }
}