using Microsoft.Extensions.Logging;
using Sys;
using System.Threading;

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
namespace org.activiti.engine.impl.interceptor
{
    /// <summary>
    /// Intercepts <seealso cref="ActivitiOptimisticLockingException"/> and tries to run the same command again. The number of retries and the time waited between retries is configurable.
    /// 
    /// 
    /// </summary>
    public class RetryInterceptor : AbstractCommandInterceptor
    {
        private static readonly ILogger log = ProcessEngineServiceProvider.LoggerService<RetryInterceptor>();

        protected internal int numOfRetries = 3;
        protected internal int waitTimeInMs = 50;
        protected internal int waitIncreaseFactor = 5;       

        public override T execute<T>(CommandConfig config, ICommand<T> command)
        {
            long waitTime = waitTimeInMs;
            int failedAttempts = 0;

            do
            {
                if (failedAttempts > 0)
                {
                    log.LogInformation($"Waiting for {waitTime}ms before retrying the command.");
                    waitBeforeRetry(waitTime);
                    waitTime *= waitIncreaseFactor;
                }

                try
                {
                    // try to execute the command
                    return next.execute(config, command);
                }
                catch (ActivitiOptimisticLockingException e)
                {
                    log.LogInformation("Caught optimistic locking exception: " + e);
                }

                failedAttempts++;
            } while (failedAttempts <= numOfRetries);

            throw new ActivitiException(numOfRetries + " retries failed with ActivitiOptimisticLockingException. Giving up.");
        }

        protected internal virtual void waitBeforeRetry(long waitTime)
        {
            try
            {
                Thread.Sleep((int)waitTime);
            }
            catch (ThreadInterruptedException e)
            {
                log.LogDebug("I am interrupted while waiting for a retry.");
            }
        }

        public virtual int NumOfRetries
        {
            set
            {
                this.numOfRetries = value;
            }
            get
            {
                return numOfRetries;
            }
        }

        public virtual int WaitIncreaseFactor
        {
            set
            {
                this.waitIncreaseFactor = value;
            }
            get
            {
                return waitIncreaseFactor;
            }
        }

        public virtual int WaitTimeInMs
        {
            set
            {
                this.waitTimeInMs = value;
            }
            get
            {
                return waitTimeInMs;
            }
        }
    }
}