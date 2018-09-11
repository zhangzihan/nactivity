using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.rest.api.resources;
using org.activiti.cloud.services.rest.controllers;
using org.springframework.hateoas.mvc;
using System;
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

namespace org.activiti.cloud.services.rest.assemblers
{
    public class TaskResourceAssembler : ResourceAssemblerSupport<Task, TaskResource>
    {

        public TaskResourceAssembler() : base(typeof(TaskControllerImpl), typeof(TaskResource))
        {
        }

        public override TaskResource toResource(Task task)
        {
            throw new NotImplementedException();
            //IList<Link> links = new List<Link>();
            //links.Add(linkTo(methodOn(typeof(TaskControllerImpl)).getTaskById(task.Id)).withSelfRel());
            //if (ASSIGNED != task.Status)
            //{
            //	links.Add(linkTo(methodOn(typeof(TaskControllerImpl)).claimTask(task.Id)).withRel("claim"));
            //}
            //else
            //{
            //	links.Add(linkTo(methodOn(typeof(TaskControllerImpl)).releaseTask(task.Id)).withRel("release"));
            //	links.Add(linkTo(methodOn(typeof(TaskControllerImpl)).completeTask(task.Id, null)).withRel("complete"));
            //}
            //// standalone tasks are not bound to a process instance
            //if (!string.ReferenceEquals(task.ProcessInstanceId, null) && task.ProcessInstanceId.Length > 0)
            //{
            //	links.Add(linkTo(methodOn(typeof(ProcessInstanceControllerImpl)).getProcessInstanceById(task.ProcessInstanceId)).withRel("processInstance"));
            //}
            //if (!string.ReferenceEquals(task.ParentTaskId, null) && task.ParentTaskId.Length > 0)
            //{
            //	links.Add(linkTo(methodOn(typeof(TaskControllerImpl)).getTaskById(task.ParentTaskId)).withRel("parent"));
            //}
            //links.Add(linkTo(typeof(HomeControllerImpl)).withRel("home"));
            //return new TaskResource(task, links);
        }

        public override IList<TaskResource> toResources(IEnumerable<Task> entities)
        {
            return base.toResources(entities);
        }
    }
}