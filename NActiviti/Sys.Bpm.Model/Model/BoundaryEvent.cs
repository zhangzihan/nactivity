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
using Newtonsoft.Json;

namespace Sys.Workflow.Bpmn.Models
{

    public class BoundaryEvent : Event
    {
        [JsonIgnore]
        protected internal Activity attachedToRef;
        protected internal string attachedToRefId;
        protected internal bool cancelActivity = true;


        [JsonIgnore]
        public virtual Activity AttachedToRef
        {
            get
            {
                return attachedToRef;
            }
            set
            {
                this.attachedToRef = value;
            }
        }


        public virtual string AttachedToRefId
        {
            get
            {
                return attachedToRefId;
            }
            set
            {
                this.attachedToRefId = value;
            }
        }

        /// <summary>
        /// 是否中断当前执行的边界任务,对应图节点，双实线终端类型，双虚实线非中断类型。
        /// </summary>
        public virtual bool CancelActivity
        {
            get
            {
                return cancelActivity;
            }
            set
            {
                this.cancelActivity = value;
            }
        }


        public override BaseElement Clone()
        {
            BoundaryEvent clone = new BoundaryEvent
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
                var val = value as BoundaryEvent;
                AttachedToRefId = val.AttachedToRefId;
                AttachedToRef = val.AttachedToRef;
                CancelActivity = val.CancelActivity;
            }
        }
    }

}