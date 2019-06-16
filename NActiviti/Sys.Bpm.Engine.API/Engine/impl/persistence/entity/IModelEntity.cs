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

    using Sys.Workflow.engine.impl.db;
    using Sys.Workflow.engine.repository;

    /// 
    /// 
    public interface IModelEntity : IModel, IHasRevision, IEntity
    {
        new string Id { get; set; }

        new DateTime CreateTime { get; set; }

        new DateTime LastUpdateTime { get; set; }

        string EditorSourceValueId { get; set; }


        string EditorSourceExtraValueId { get; set; }


    }

}