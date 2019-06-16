﻿using System;

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

namespace Sys.Workflow.engine.impl.persistence.entity
{

    using Sys.Workflow.engine.history;

    /// 
    /// 
    public interface IHistoricDetailEntity : IHistoricDetail, IEntity
    {

        new string ProcessInstanceId { get; set; }

        new string ActivityInstanceId { get; set; }

        new string TaskId { get; set; }

        new string ExecutionId { get; set; }

        new DateTime? Time { get; set; }

        string DetailType { get; set; }


    }

}