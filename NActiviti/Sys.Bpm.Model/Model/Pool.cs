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
    public class Pool : BaseElement
    {

        protected internal string name;
        protected internal string processRef;
        protected internal bool executable = true;

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


        public virtual string ProcessRef
        {
            get
            {
                return processRef;
            }
            set
            {
                this.processRef = value;
            }
        }


        public virtual bool Executable
        {
            get
            {
                return this.executable;
            }
            set
            {
                this.executable = value;
            }
        }


        public override BaseElement Clone()
        {
            Pool clone = new Pool
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
                var val = value as Pool;
                Name = val.Name;
                ProcessRef = val.ProcessRef;
                Executable = val.Executable;
            }
        }
    }

}