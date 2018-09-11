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

using DryIoc;
using Microsoft.Extensions.Logging;
using Sys;

namespace org.activiti.engine.impl.interceptor
{
    /// 
    public class LogInterceptor : AbstractCommandInterceptor
    {
        private ILogger log = ProcessEngineServiceProvider.LoggerService<ILogger<LogInterceptor>>();

        public override T execute<T>(CommandConfig config, ICommand<T> command)
        {
            //if (!log.IsEnabled(LogLevel.Debug))
            //{
            //    // do nothing here if we cannot log
            //    return next.execute(config, command);
            //}
            log.LogDebug("\n");
            log.LogDebug($"--- starting {command.GetType().Name} --------------------------------------------------------");
            try
            {

                return next.execute(config, command);

            }
            finally
            {
                log.LogDebug($"--- {command.GetType().Name} finished --------------------------------------------------------");
                log.LogDebug("\n");
            }
        }
    }

}