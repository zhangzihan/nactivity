using System.Collections.Generic;

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
namespace Sys.Workflow.bpmn.model
{

    public class SendTask : TaskWithFieldExtensions
    {

        protected internal string type;
        protected internal string implementationType;
        protected internal string operationRef;

        public virtual string Type
        {
            get
            {
                return type;
            }
            set
            {
                this.type = value;
            }
        }


        public virtual string Implementation
        {
            get;
            set;
        }


        public virtual string ImplementationType
        {
            get
            {
                return implementationType;
            }
            set
            {
                this.implementationType = value;
            }
        }


        public virtual string OperationRef
        {
            get
            {
                return operationRef;
            }
            set
            {
                this.operationRef = value;
            }
        }


        public override BaseElement Clone()
        {
            SendTask clone = new SendTask
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
                var val = value as SendTask;
                Type = val.Type;
                ImplementationType = val.ImplementationType;
                OperationRef = val.OperationRef;

                fieldExtensions = new List<FieldExtension>();
                if (val.FieldExtensions != null && val.FieldExtensions.Count > 0)
                {
                    foreach (FieldExtension extension in val.FieldExtensions)
                    {
                        fieldExtensions.Add(extension.Clone() as FieldExtension);
                    }
                }
            }
        }
    }

}