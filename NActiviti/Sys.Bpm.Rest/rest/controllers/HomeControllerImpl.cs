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

using Microsoft.AspNetCore.Mvc;
using org.activiti.bpmn.model;
using org.activiti.cloud.services.rest.api;
using org.activiti.cloud.services.rest.api.resources;

namespace org.activiti.cloud.services.rest.controllers
{
    [Route("v1")]
    [ApiController]
    public class HomeControllerImpl : ControllerBase, IHomeController
    {
        [HttpGet]
        public virtual string Get()
        {
            return "Wellcome";
            //return StringContent
            //new ActionResult<Resource>(new Resource())
            
            //    Resource resource = new Resource(new HomeResource(), linkTo(typeof(ProcessDefinitionControllerImpl)).withRel("process-definitions"), linkTo(typeof(ProcessInstanceControllerImpl)).withRel("process-instances"), linkTo(typeof(TaskControllerImpl)).withRel("tasks"));

            //return resource;
        }
    }

}