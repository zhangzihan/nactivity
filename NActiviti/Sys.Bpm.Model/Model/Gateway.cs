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
    public abstract class Gateway : FlowNode
    {

        protected internal string defaultFlow;

        public virtual string DefaultFlow
        {
            get
            {
                return defaultFlow;
            }
            set
            {
                this.defaultFlow = value;
            }
        }


        public override abstract BaseElement Clone();

        public override BaseElement Values
        {
            set
            {
                base.Values = value;
                var val = value as Gateway;

                DefaultFlow = val.DefaultFlow;
            }
        }
    }

}