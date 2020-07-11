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

    /// <summary>
    /// 
    /// </summary>
    public class BeansResolverFactory : IResolverFactory, IResolver
    {
        /// <summary>
        /// 
        /// </summary>
        protected internal ProcessEngineConfigurationImpl processEngineConfiguration;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="processEngineConfiguration"></param>
        /// <param name="variableScope"></param>
        /// <returns></returns>
        public virtual IResolver CreateResolver(ProcessEngineConfigurationImpl processEngineConfiguration, IVariableScope variableScope)
        {
            this.processEngineConfiguration = processEngineConfiguration;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual bool ContainsKey(object key)
        {
            return processEngineConfiguration.Beans.ContainsKey(key);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual object Get(object key)
        {
            return processEngineConfiguration.Beans[key];
        }
    }

}