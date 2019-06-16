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
namespace Sys.Workflow.engine.impl.interceptor
{
	/// <summary>
	/// The command executor for internal usage.
	/// 
	/// 
	/// </summary>
	public interface ICommandExecutor
	{

	  /// <returns> the default <seealso cref="CommandConfig"/>, used if none is provided. </returns>
	  CommandConfig DefaultConfig {get;}

	  /// <summary>
	  /// Execute a command with the specified <seealso cref="CommandConfig"/>.
	  /// </summary>
	  T Execute<T>(CommandConfig config, ICommand<T> command);

	  /// <summary>
	  /// Execute a command with the default <seealso cref="CommandConfig"/>.
	  /// </summary>
	  T Execute<T>(ICommand<T> command);

	}

}