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
namespace Sys.Workflow.Bpmn.Models
{
    public class AdhocSubProcess : SubProcess
    {

        public const string ORDERING_PARALLEL = "Parallel";
        public const string ORDERING_SEQUENTIALL = "Sequential";

        protected internal string completionCondition;
        protected internal string ordering = ORDERING_PARALLEL;
        protected internal bool cancelRemainingInstances = true;

        public virtual string CompletionCondition
        {
            get
            {
                return completionCondition;
            }
            set
            {
                this.completionCondition = value;
            }
        }


        public virtual string Ordering
        {
            get
            {
                return ordering;
            }
            set
            {
                this.ordering = value;
            }
        }


        public virtual bool HasParallelOrdering()
        {
            return !ORDERING_SEQUENTIALL.Equals(ordering);
        }

        public virtual bool HasSequentialOrdering()
        {
            return ORDERING_SEQUENTIALL.Equals(ordering);
        }

        public virtual bool CancelRemainingInstances
        {
            get
            {
                return cancelRemainingInstances;
            }
            set
            {
                this.cancelRemainingInstances = value;
            }
        }

    }

}