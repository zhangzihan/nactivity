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

using org.activiti.cloud.services.api.model;
using org.springframework.hateoas;

namespace org.activiti.cloud.services.rest.api.resources
{
    public class ProcessInstanceResource : Resource<ProcessInstance>
    {

        public ProcessInstanceResource(ProcessInstance processInstance, params Link[] links) : base(processInstance, links)
        {
        }
    }
}