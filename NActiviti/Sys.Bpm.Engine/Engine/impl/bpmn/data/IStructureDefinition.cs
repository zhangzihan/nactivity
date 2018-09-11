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
namespace org.activiti.engine.impl.bpmn.data
{
	/// <summary>
	/// Represents a definition of a structure used to exchange information
	/// 
	/// 
	/// </summary>
	public interface IStructureDefinition
	{

	  /// <summary>
	  /// Obtains the id of this structure
	  /// </summary>
	  /// <returns> the id of this structure </returns>
	  string Id {get;}

	  /// <returns> a new instance of this structure definition </returns>
	  IStructureInstance createInstance();
	}

}