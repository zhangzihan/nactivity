using System;

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
    using Microsoft.Extensions.Logging;
    using Sys.Workflow.engine.impl.cfg;
    using Sys.Workflow.engine.impl.context;
    using Sys.Workflow;

    /// 
    /// 
    public class CommandContextInterceptor : AbstractCommandInterceptor
    {
        private static readonly ILogger<CommandContextInterceptor> log = ProcessEngineServiceProvider.LoggerService<CommandContextInterceptor>();

        protected internal CommandContextFactory commandContextFactory;
        protected internal ProcessEngineConfigurationImpl processEngineConfiguration;

        public CommandContextInterceptor()
        {
        }

        public CommandContextInterceptor(CommandContextFactory commandContextFactory, ProcessEngineConfigurationImpl processEngineConfiguration)
        {
            this.commandContextFactory = commandContextFactory;
            this.processEngineConfiguration = processEngineConfiguration;
        }

        public override T Execute<T>(CommandConfig config, ICommand<T> command)
        {
            ICommandContext context = Context.CommandContext;

            bool contextReused = false;
            // We need to check the exception, because the transaction can be in a
            // rollback state, and some other command is being fired to compensate (eg. decrementing job retries)
            if (!config.ContextReusePossible || context == null || context.Exception != null)
            {
                context = commandContextFactory.CreateCommandContext<T>(command);
            }
            else
            {
                log.LogDebug($"Valid context found. Reusing it for the current command '{command.GetType().FullName}'");
                contextReused = true;
                context.Reused = true;
            }

            try
            {
                // Push on stack
                Context.CommandContext = context;
                Context.ProcessEngineConfiguration = processEngineConfiguration;

                return next.Execute(config, command);

            }
            catch (NullReferenceException e)
            {
                context.SetException(e);
            }
            catch (Exception e)
            {
                context.SetException(e);
            }
            finally
            {
                try
                {
                    if (!contextReused)
                    {
                        context.Close();
                    }
                }
                finally
                {

                    // Pop from stack
                    Context.RemoveCommandContext();
                    Context.RemoveProcessEngineConfiguration();
                    Context.RemoveBpmnOverrideContext();
                }
            }

            return default;
        }

        public virtual CommandContextFactory CommandContextFactory
        {
            get
            {
                return commandContextFactory;
            }
            set
            {
                this.commandContextFactory = value;
            }
        }


        public virtual ProcessEngineConfigurationImpl ProcessEngineConfiguration
        {
            get
            {
                return processEngineConfiguration;
            }
        }

        public virtual ProcessEngineConfigurationImpl ProcessEngineContext
        {
            set
            {
                this.processEngineConfiguration = value;
            }
        }
    }

}