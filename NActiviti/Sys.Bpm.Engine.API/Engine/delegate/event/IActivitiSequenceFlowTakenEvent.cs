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
namespace org.activiti.engine.@delegate.@event
{
    /// <summary>
    /// An <seealso cref="IActivitiEvent"/> that indicates a certain sequence flow has been taken.
    /// 
    /// 
    /// </summary>
    public interface IActivitiSequenceFlowTakenEvent : IActivitiEvent
    {

        string Id { get; }

        string SourceActivityId { get; }

        string SourceActivityName { get; }

        string SourceActivityType { get; }

        string SourceActivityBehaviorClass { get; }

        string TargetActivityId { get; }

        string TargetActivityName { get; }

        string TargetActivityType { get; }

        string TargetActivityBehaviorClass { get; }
    }

}