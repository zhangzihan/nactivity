using org.activiti.engine.impl.agenda;
using System;
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
namespace org.activiti.engine
{
    /// <summary>
    /// For each API call (and thus <seealso cref="Command"/>) being executed, a new agenda instance is created.
    /// On this agenda, operations are put, which the <seealso cref="CommandExecutor"/> will keep executing until
    /// all are executed.
    /// 
    /// The agenda also gives easy access to methods to plan new operations when writing
    /// <seealso cref="ActivityBehavior"/> implementations.
    /// 
    /// During a <seealso cref="Command"/> execution, the agenda can always be fetched using <seealso cref="Context#getAgenda()"/>.
    /// </summary>
    public interface IAgenda
    {

        bool Empty { get; }

        AbstractOperation NextOperation { get; }

        /// <summary>
        /// Generic method to plan a <seealso cref="Runnable"/>.
        /// </summary>
        void planOperation(AbstractOperation operation);
    }

}