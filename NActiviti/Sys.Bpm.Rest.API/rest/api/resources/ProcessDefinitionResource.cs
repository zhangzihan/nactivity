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

using Sys.Workflow.Cloud.Services.Api.Model;
using Sys.Workflow.Hateoas;

namespace Sys.Workflow.Cloud.Services.Rest.Api.Resources
{

    /// <summary>
    /// 流程定义资源描述
    /// </summary>
    public class ProcessDefinitionResource : Resource<ProcessDefinition>
    {

        /// <summary>
        /// 
        /// </summary>

        public ProcessDefinitionResource(ProcessDefinition content, params Link[] links) : base(content, links)
        {
        }
    }

}