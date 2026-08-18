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
namespace Sys.Workflow.Engine.Impl.Asyncexecutor
{
    /// <summary>
    /// 当任务队列已满且工作线程数已达上限、无法接收新任务时抛出，
    /// 对应 Java 的 <c>java.util.concurrent.RejectedExecutionException</c>。
    /// </summary>
    [Serializable]
    public class RejectedExecutionException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        public RejectedExecutionException()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public RejectedExecutionException(string message) : base(message)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public RejectedExecutionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
