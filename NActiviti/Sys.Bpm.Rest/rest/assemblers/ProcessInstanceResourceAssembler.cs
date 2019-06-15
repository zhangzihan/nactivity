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
using org.activiti.cloud.services.rest.api.resources;
using org.activiti.cloud.services.rest.controllers;
using org.springframework.hateoas.mvc;
using System;

namespace org.activiti.cloud.services.rest.assemblers
{
    /// <summary>
    /// 
    /// </summary>
    public class ProcessInstanceResourceAssembler : ResourceAssemblerSupport<ProcessInstance, ProcessInstanceResource>
    {
        /// <summary>
        /// 
        /// </summary>
        public ProcessInstanceResourceAssembler() : base(typeof(ProcessInstanceControllerImpl), typeof(ProcessInstanceResource))
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="processInstance"></param>
        /// <returns></returns>
        public override ProcessInstanceResource ToResource(ProcessInstance processInstance)
        {
            //throw new NotImplementedException();
            //Link processInstancesRel = linkTo(methodOn(typeof(ProcessInstanceControllerImpl)).getProcessInstances(null)).withRel("processInstances");
            //Link selfLink = linkTo(methodOn(typeof(ProcessInstanceControllerImpl)).getProcessInstanceById(processInstance.Id)).withSelfRel();
            //Link variablesLink = linkTo(methodOn(typeof(ProcessInstanceVariableControllerImpl)).getVariables(processInstance.Id)).withRel("variables");
            //Link homeLink = linkTo(typeof(HomeControllerImpl)).withRel("home");
            return new ProcessInstanceResource(processInstance); //selfLink, variablesLink, processInstancesRel, homeLink);
        }
    }

}