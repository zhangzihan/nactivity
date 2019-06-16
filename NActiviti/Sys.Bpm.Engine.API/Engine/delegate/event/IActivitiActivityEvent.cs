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
namespace Sys.Workflow.engine.@delegate.@event
{
	/// <summary>
	/// An <seealso cref="IActivitiEvent"/> related to an activity within an execution;
	/// 
	/// 
	/// 
	/// </summary>
	public interface IActivitiActivityEvent : IActivitiEvent
	{

	  /// <returns> the id of the activity this event is related to. This corresponds to an id defined in the process definition. </returns>
	  string ActivityId {get;}

	  /// <returns> the name of the activity this event is related to. </returns>
	  string ActivityName {get;}

	  /// <returns> the type of the activity (if set during parsing). </returns>
	  string ActivityType {get;}

	  /// <returns> the behaviourclass of the activity (if it could be determined) </returns>
	  string BehaviorClass {get;}

	}

}