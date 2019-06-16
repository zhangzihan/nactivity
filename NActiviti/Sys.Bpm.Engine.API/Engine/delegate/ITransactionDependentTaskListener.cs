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

namespace Sys.Workflow.Engine.Delegate
{
    using Sys.Workflow.Bpmn.Models;

    /// <summary>
    /// Callback interface to be notified of transaction events.
    /// 
    /// 
    /// </summary>
    public interface ITransactionDependentTaskListener : IBaseTaskListener
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="processInstanceId"></param>
        /// <param name="executionId"></param>
        /// <param name="task"></param>
        /// <param name="executionVariables"></param>
        /// <param name="customPropertiesMap"></param>
        void Notify(string processInstanceId, string executionId, TaskActivity task, IDictionary<string, object> executionVariables, IDictionary<string, object> customPropertiesMap);
    }

    /// <summary>
    /// 
    /// </summary>
    public static class TransactionDependentTaskListenerFields
    {
        /// <summary>
        /// 
        /// </summary>
        public const string ON_TRANSACTION_COMMITTING = "before-commit";

        /// <summary>
        /// 
        /// </summary>
        public const string ON_TRANSACTION_COMMITTED = "committed";

        /// <summary>
        /// 
        /// </summary>
        public const string ON_TRANSACTION_ROLLED_BACK = "rolled-back";
    }
}