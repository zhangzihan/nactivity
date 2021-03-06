﻿/* Licensed under the Apache License, Version 2.0 (the "License");
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
namespace Sys.Workflow.Engine.Impl.Cfg
{
    /// <summary>
    /// Enumeration that represents the transaction propagation behaviours supported by the command executor.
    /// 
    /// 
    /// </summary>
    public enum TransactionPropagation
    {
        /// <summary>
        /// 
        /// </summary>
        REQUIRED,
        /// <summary>
        /// 
        /// </summary>
        REQUIRES_NEW,
        /// <summary>
        /// 
        /// </summary>
        NOT_SUPPORTED,
    }
}