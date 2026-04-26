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
namespace Sys.Workflow.Engine.Impl.Interceptor
{
    using System.Threading.Tasks;

    /// 
    public abstract class AbstractCommandInterceptor : ICommandInterceptor
    {
        public abstract T Execute<T>(CommandConfig config, ICommand<T> command);

        /// <summary>
        /// Default async implementation delegates to the next interceptor in the chain.
        /// Subclasses should override this to add async-specific behavior.
        /// </summary>
        public virtual Task<T> ExecuteAsync<T>(CommandConfig config, ICommand<T> command)
        {
            return Next.ExecuteAsync(config, command);
        }

        /// <summary>
        /// will be initialized by the <seealso cref="Sys.Workflow.Engine.ProcessEngineConfiguration ProcessEngineConfiguration"/>
        /// </summary>
        private ICommandInterceptor next;

        public virtual ICommandInterceptor Next
        {
            get
            {
                return next;
            }
            set
            {
                this.next = value;
            }
        }

    }

}