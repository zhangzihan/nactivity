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

namespace Sys.Workflow.Engine.Impl.Scripting
{

	using Sys.Workflow.Engine.Delegate;
	using Sys.Workflow.Engine.Impl.Cfg;

	/// 
	/// 
	public class ScriptBindingsFactory
	{

	  protected internal ProcessEngineConfigurationImpl processEngineConfiguration;
	  protected internal IList<IResolverFactory> resolverFactories;

	  public ScriptBindingsFactory(ProcessEngineConfigurationImpl processEngineConfiguration, IList<IResolverFactory> resolverFactories)
	  {
		this.processEngineConfiguration = processEngineConfiguration;
		this.resolverFactories = resolverFactories;
	  }

	  public virtual Bindings createBindings(IVariableScope variableScope)
	  {
		return new ScriptBindings(createResolvers(variableScope), variableScope);
	  }

	  public virtual Bindings createBindings(IVariableScope variableScope, bool storeScriptVariables)
	  {
		return new ScriptBindings(createResolvers(variableScope), variableScope, storeScriptVariables);
	  }

	  protected internal virtual IList<IResolver> createResolvers(IVariableScope variableScope)
	  {
		IList<IResolver> scriptResolvers = new List<IResolver>();
		foreach (IResolverFactory scriptResolverFactory in resolverFactories)
		{
		  IResolver resolver = scriptResolverFactory.createResolver(processEngineConfiguration, variableScope);
		  if (resolver != null)
		  {
			scriptResolvers.Add(resolver);
		  }
		}
		return scriptResolvers;
	  }

	  public virtual IList<IResolverFactory> ResolverFactories
	  {
		  get
		  {
			return resolverFactories;
		  }
		  set
		  {
			this.resolverFactories = value;
		  }
	  }

	}

}