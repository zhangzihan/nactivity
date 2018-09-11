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
namespace org.activiti.engine.impl.interceptor
{
    using org.activiti.engine.debug;
    using org.activiti.engine.impl.agenda;

    /// 
    public class DebugCommandInvoker : CommandInvoker
    {
        public override void executeOperation(AbstractOperation runnable)
        {
            if (runnable is AbstractOperation)
            {
                AbstractOperation operation = (AbstractOperation)runnable;

                if (operation.Execution != null)
                {
                    //logger.info("Execution tree while executing operation {} :", operation.GetType());
                    //logger.info("{}", Environment.NewLine + ExecutionTreeUtil.buildExecutionTree(operation.Execution));
                }

            }

            base.executeOperation(runnable);
        }

    }

}