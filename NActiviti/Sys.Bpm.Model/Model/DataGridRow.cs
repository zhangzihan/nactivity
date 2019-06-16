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
namespace Sys.Workflow.Bpmn.Models
{

    public class DataGridRow
    {

        protected internal int index;
        protected internal IList<DataGridField> fields = new List<DataGridField>();

        public virtual int Index
        {
            get
            {
                return index;
            }
            set
            {
                this.index = value;
            }
        }


        public virtual IList<DataGridField> Fields
        {
            get
            {
                return fields;
            }
            set
            {
                this.fields = value;
            }
        }


        public virtual DataGridRow Clone()
        {
            DataGridRow clone = new DataGridRow
            {
                Values = this
            };
            return clone;
        }

        public virtual DataGridRow Values
        {
            set
            {
                Index = value.Index;

                fields = new List<DataGridField>();
                if (value.Fields != null && value.Fields.Count > 0)
                {
                    foreach (DataGridField field in value.Fields)
                    {
                        fields.Add(field.Clone() as DataGridField);
                    }
                }
            }
        }
    }

}