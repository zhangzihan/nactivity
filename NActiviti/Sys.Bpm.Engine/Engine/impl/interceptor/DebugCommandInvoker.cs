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
namespace Sys.Workflow.engine.impl.interceptor
{
    using Microsoft.Extensions.Logging;
    using Sys.Workflow.engine.impl.agenda;
    using Sys.Workflow;

    /// 
    public class DebugCommandInvoker : CommandInvoker
    {
        public override void ExecuteOperation(AbstractOperation runnable)
        {
            //if (runnable is AbstractOperation)
            //{
            //    AbstractOperation operation = (AbstractOperation)runnable;

            //    if (operation.Execution != null)
            //    {
            //        logger.LogInformation($"Execution tree while executing operation {operation.GetType()} :");
            //        ExecutionTree eTree = ExecutionTreeUtil.buildExecutionTree(operation.Execution);
            //        logger.LogInformation($"\r\n{eTree.ToString()}");

            //        string root = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);

            //        lock (syncRoot)
            //        {
            //            string file = Path.Combine(root, $"task_trees\\{DateTime.Now.ToString("yyyyMMdd")}.txt");
            //            Directory.CreateDirectory(Path.GetDirectoryName(file));

            //            File.AppendAllText(file, $"{(File.Exists(file) ? "\r\n" : "")}{eTree.ToString()}");
            //        }
            //    }
            //}

            base.ExecuteOperation(runnable);
        }
    }
}