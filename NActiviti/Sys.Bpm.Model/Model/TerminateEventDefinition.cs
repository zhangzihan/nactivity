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
namespace org.activiti.bpmn.model
{
    public class TerminateEventDefinition : EventDefinition
    {

        /// <summary>
        /// When true, this event will terminate all parent process instances (in the case of using call activity),
        /// thus ending the whole process instance.
        /// 
        /// By default false (BPMN spec compliant): the parent scope is terminated (subprocess: embedded or call activity)
        /// </summary>
        protected internal bool terminateAll;

        /// <summary>
        /// When true (and used within a multi instance), this event will terminate all multi instance instances 
        /// of the embedded subprocess/call activity this event is used in. 
        /// 
        /// In case of nested multi instance, only the first parent multi instance structure will be destroyed.
        /// In case of 'true' and not being in a multi instance construction: executes the default behavior.
        /// 
        /// Note: if terminate all is set to true, this will have precedence over this.
        /// </summary>
        protected internal bool terminateMultiInstance;

        public override BaseElement clone()
        {
            TerminateEventDefinition clone = new TerminateEventDefinition();
            clone.Values = this;
            return clone;
        }

        public override BaseElement Values
        {
            set
            {
                base.Values = value;
                var val = value as TerminateEventDefinition;
                this.terminateAll = val.TerminateAll;
                this.terminateMultiInstance = val.TerminateMultiInstance;
            }
        }

        public virtual bool TerminateAll
        {
            get
            {
                return terminateAll;
            }
            set
            {
                this.terminateAll = value;
            }
        }


        public virtual bool TerminateMultiInstance
        {
            get
            {
                return terminateMultiInstance;
            }
            set
            {
                this.terminateMultiInstance = value;
            }
        }


    }

}