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

using Microsoft.Extensions.Logging;
using Sys.Workflow;
using System.Threading.Tasks;

namespace Sys.Workflow.Engine.Impl.Interceptor
{
    /// 
    public class LogInterceptor : AbstractCommandInterceptor
    {
        private static readonly ILogger log = ProcessEngineServiceProvider.LoggerService<ILogger<LogInterceptor>>();

        public override T Execute<T>(CommandConfig config, ICommand<T> command)
        {
            if (!log.IsEnabled(LogLevel.Debug))
            {
                // do nothing here if we cannot log
                return Next.Execute(config, command);
            }

            log.LogDebug("\n");
            log.LogDebug($"--- starting {command.GetType().Name} --------------------------------------------------------");
            try
            {

                return Next.Execute(config, command);

            }
            finally
            {
                log.LogDebug($"--- {command.GetType().Name} finished --------------------------------------------------------");
                log.LogDebug("\n");
            }
        }

        public override async Task<T> ExecuteAsync<T>(CommandConfig config, ICommand<T> command)
        {
            if (!log.IsEnabled(LogLevel.Debug))
            {
                return await Next.ExecuteAsync(config, command).ConfigureAwait(false);
            }

            log.LogDebug("\n");
            log.LogDebug($"--- async starting {command.GetType().Name} --------------------------------------------------------");
            try
            {
                return await Next.ExecuteAsync(config, command).ConfigureAwait(false);
            }
            finally
            {
                log.LogDebug($"--- {command.GetType().Name} async finished --------------------------------------------------------");
                log.LogDebug("\n");
            }
        }
    }

}