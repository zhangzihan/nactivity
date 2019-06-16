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
    public class Signal : BaseElement
    {

        public const string SCOPE_GLOBAL = "global";
        public const string SCOPE_PROCESS_INSTANCE = "processInstance";

        protected internal string name;

        protected internal string scope;

        public Signal()
        {
        }

        public Signal(string id, string name)
        {
            this.id = id;
            this.name = name;
        }

        public virtual string Name
        {
            get
            {
                return name;
            }
            set
            {
                this.name = value;
            }
        }


        public virtual string Scope
        {
            get
            {
                return scope;
            }
            set
            {
                this.scope = value;
            }
        }


        public override BaseElement Clone()
        {
            Signal clone = new Signal
            {
                Values = this
            };
            return clone;
        }

        public override BaseElement Values
        {
            set
            {
                base.Values = value;
                var val = value as Signal;
                Name = val.Name;
                Scope = val.Scope;
            }
        }
    }

}