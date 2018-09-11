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
namespace org.activiti.engine.management
{

    /// <summary>
    /// Structure containing meta data (column names, column types, etc.) about a certain database table.
    /// 
    /// 
    /// </summary>
    public class TableMetaData
    {

        protected internal string tableName;

        protected internal IList<string> columnNames = new List<string>();

        protected internal IList<string> columnTypes = new List<string>();

        public TableMetaData()
        {

        }

        public TableMetaData(string tableName)
        {
            this.tableName = tableName;
        }

        public virtual void addColumnMetaData(string columnName, string columnType)
        {
            columnNames.Add(columnName);
            columnTypes.Add(columnType);
        }

        public virtual string TableName
        {
            get
            {
                return tableName;
            }
            set
            {
                this.tableName = value;
            }
        }


        public virtual IList<string> ColumnNames
        {
            get
            {
                return columnNames;
            }
            set
            {
                this.columnNames = value;
            }
        }


        public virtual IList<string> ColumnTypes
        {
            get
            {
                return columnTypes;
            }
            set
            {
                this.columnTypes = value;
            }
        }


    }

}