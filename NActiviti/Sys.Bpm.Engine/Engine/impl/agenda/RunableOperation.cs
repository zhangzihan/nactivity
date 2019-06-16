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
namespace Sys.Workflow.engine.impl.agenda
{
    using System;

    /// <summary>
    /// 
    /// </summary>
    public sealed class RunableOperation : AbstractOperation
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="run"></param>
        public RunableOperation(Action run)
        {
            this.Run = run;
        }

        /// <inheritdoc />
        protected override void RunOperation()
        {

        }
    }

}