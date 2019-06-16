using System.Collections.Generic;

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
namespace Sys.Workflow.engine.impl.persistence.entity.data
{

    using Sys.Workflow.engine.task;

    /// 
    public interface ICommentDataManager : IDataManager<ICommentEntity>
    {

        IList<IComment> FindCommentsByTaskId(string taskId);

        IList<IComment> FindCommentsByTaskIdAndType(string taskId, string type);

        IList<IComment> FindCommentsByType(string type);

        IList<IEvent> FindEventsByTaskId(string taskId);

        IList<IEvent> FindEventsByProcessInstanceId(string processInstanceId);

        void DeleteCommentsByTaskId(string taskId);

        void DeleteCommentsByProcessInstanceId(string processInstanceId);

        IList<IComment> FindCommentsByProcessInstanceId(string processInstanceId);

        IList<IComment> FindCommentsByProcessInstanceId(string processInstanceId, string type);

        IComment FindComment(string commentId);

        IEvent FindEvent(string commentId);
    }
}