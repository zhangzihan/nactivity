﻿using Sys.Workflow.cloud.services.api.model;
using org.springframework.hateoas;
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

namespace Sys.Workflow.cloud.services.rest.api.resources
{

    /// <summary>
    /// 流程任务资源描述
    /// </summary>
    public class TaskResource : Resource<TaskModel>
    {

        /// <summary>
        /// 
        /// </summary>
        public TaskResource(TaskModel content, IEnumerable<Link> links) : base(content, links)
        {
        }
    }
}