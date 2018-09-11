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
namespace org.activiti.engine.impl.variable
{
	/// 
	public interface IVariableType
	{

	  /// <summary>
	  /// name of variable type (limited to 100 characters length)
	  /// </summary>
	  string TypeName {get;}

	  /// <summary>
	  /// <para>
	  /// Indicates if this variable type supports caching.
	  /// </para>
	  /// <para>
	  /// If caching is supported, the result of <seealso cref="#getValue(ValueFields)"/> is saved for the duration of the session and used for subsequent reads of the variable's value.
	  /// </para>
	  /// <para>
	  /// If caching is not supported, all reads of a variable's value require a fresh call to <seealso cref="#getValue(ValueFields)"/>.
	  /// </para>
	  /// </summary>
	  /// <returns> whether variables of this type are cacheable. </returns>
	  bool Cachable {get;}

	  /// <returns> whether this variable type can store the specified value. </returns>
	  bool isAbleToStore(object value);

	  /// <summary>
	  /// Stores the specified value in the supplied <seealso cref="IValueFields"/>.
	  /// </summary>
	  void setValue(object value, IValueFields valueFields);

	  /// <returns> the value of a variable based on the specified <seealso cref="IValueFields"/>. </returns>
	  object getValue(IValueFields valueFields);

	}

}