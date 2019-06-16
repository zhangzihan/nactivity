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
using System.Collections.Generic;

namespace Sys.Workflow.engine.impl.persistence.entity
{

    /// 
    public interface IEntityManager<EntityImpl> where EntityImpl : IEntity
    {

        EntityImpl Create();

        /// <summary>
        /// 数据查询,key为查询参数名，value为值.
        /// </summary>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="entityId"></param>
        /// <returns></returns>
        TOut FindById<TOut>(KeyValuePair<string, object> entityId);

        /// <summary>
        /// 但数据查询参数名为id时使用此方法，否则使用自定义参数名的findById的方法.
        /// </summary>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="entityId"></param>
        /// <returns></returns>
        TOut FindById<TOut>(object entityId);

        void Insert(EntityImpl entity);

        void Insert(EntityImpl entity, bool fireCreateEvent);

        EntityImpl Update(EntityImpl entity);

        EntityImpl Update(EntityImpl entity, bool fireUpdateEvent);

        void Delete(KeyValuePair<string, object> entityId);

        void Delete(EntityImpl entity);

        void Delete(EntityImpl entity, bool fireDeleteEvent);

    }
}