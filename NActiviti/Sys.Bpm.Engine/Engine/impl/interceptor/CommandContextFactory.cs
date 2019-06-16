/*
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *       http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace Sys.Workflow.Engine.Impl.Interceptor
{
    using Sys.Workflow.Engine.Impl.Cfg;

    /// 
    public class CommandContextFactory
    {

        protected internal ProcessEngineConfigurationImpl processEngineConfiguration;

        public virtual CommandContext<T1> CreateCommandContext<T1>(ICommand<T1> cmd)
        {
            return new CommandContext<T1>(cmd, processEngineConfiguration);
        }

        // getters and setters
        // //////////////////////////////////////////////////////

        public virtual ProcessEngineConfigurationImpl ProcessEngineConfiguration
        {
            get
            {
                return processEngineConfiguration;
            }
            set
            {
                this.processEngineConfiguration = value;
            }
        }

    }
}