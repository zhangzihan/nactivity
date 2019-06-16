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
namespace Sys.Workflow.Engine.Impl.Delegate.Invocation
{
    using Sys.Workflow.Engine.Impl.Interceptor;

    /// <summary>
    /// Default implementation, simply proceeding the call.
    /// 
    /// 
    /// </summary>
    public class DefaultDelegateInterceptor : IDelegateInterceptor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="invocation"></param>
        public virtual void HandleInvocation(DelegateInvocation invocation)
        {
            invocation.Proceed();
        }
    }
}