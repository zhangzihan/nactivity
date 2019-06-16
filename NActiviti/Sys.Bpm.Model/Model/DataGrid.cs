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

    public class DataGrid : IComplexDataType
    {

        protected internal IList<DataGridRow> rows = new List<DataGridRow>();

        public virtual IList<DataGridRow> Rows
        {
            get
            {
                return rows;
            }
            set
            {
                this.rows = value;
            }
        }


        public virtual DataGrid Clone()
        {
            DataGrid clone = new DataGrid
            {
                Values = this
            };
            return clone;
        }

        public virtual DataGrid Values
        {
            set
            {
                rows = new List<DataGridRow>();
                if (value.Rows != null && value.Rows.Count > 0)
                {
                    foreach (DataGridRow row in value.Rows)
                    {
                        rows.Add(row.Clone());
                    }
                }
            }
        }
    }

}