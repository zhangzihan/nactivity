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
namespace org.activiti.engine.cfg
{
	using org.activiti.engine.impl.cfg;

	/// <summary>
	/// Implementations of this class can be plugged into a <seealso cref="ProcessEngineConfigurationImpl"/>. Such implementations can configure the engine in any way programmatically possible.
	/// 
	/// 
	/// </summary>
	public interface IProcessEngineConfigurator
	{

	  /// <summary>
	  /// Called <b>before</b> any initialisation has been done. This can for example be useful to change configuration settings before anything that uses those properties is created.
	  /// 
	  /// Allows to tweak the process engine by passing the <seealso cref="ProcessEngineConfigurationImpl"/> which allows tweaking it programmatically.
	  /// 
	  /// An example is the jdbc url. When a <seealso cref="IProcessEngineConfigurator"/> instance wants to change it, it needs to do it in this method, or otherwise the datasource would already have been created
	  /// with the 'old' value for the jdbc url.
	  /// </summary>
	  void beforeInit(ProcessEngineConfigurationImpl processEngineConfiguration);

	  /// <summary>
	  /// Called when the engine boots up, before it is usable, but after the initialisation of internal objects is done.
	  /// 
	  /// Allows to tweak the process engine by passing the <seealso cref="ProcessEngineConfigurationImpl"/> which allows tweaking it programmatically.
	  /// 
	  /// An example is the ldap user/group manager, which is an addition to the engine. No default properties need to be overridden for this (otherwise the
	  /// <seealso cref="#beforeInit(ProcessEngineConfigurationImpl)"/> method should be used) so the logic contained in this method is executed after initialisation of the default objects.
	  /// 
	  /// Probably a better name would be 'afterInit' (cfr <seealso cref="#beforeInit(ProcessEngineConfigurationImpl)"/>), but not possible due to backwards compatibility.
	  /// </summary>
	  void configure(ProcessEngineConfigurationImpl processEngineConfiguration);

	  /// <summary>
	  /// When the <seealso cref="IProcessEngineConfigurator"/> instances are used, they are first ordered by this priority number (lowest to highest). If you have dependencies between
	  /// <seealso cref="IProcessEngineConfigurator"/> instances, use the priorities accordingly to order them as needed.
	  /// </summary>
	  int Priority {get;}

	}

}