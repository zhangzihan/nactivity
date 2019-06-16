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

namespace Sys.Workflow.engine.impl.db
{
    /// 
    public class ListQueryParameterObject
    {
        protected internal int maxResults = int.MaxValue;
        protected internal int firstResult;
        protected internal object parameter;
        protected internal string databaseType;

        /// <summary>
        /// 
        /// </summary>
        public ListQueryParameterObject()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="firstResult"></param>
        /// <param name="maxResults"></param>
        public ListQueryParameterObject(object parameter, int firstResult, int maxResults)
        {
            this.parameter = parameter;
            this.firstResult = firstResult;
            this.maxResults = maxResults;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int FirstResult
        {
            get
            {
                return firstResult;
            }
            set
            {
                this.firstResult = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int FirstRow
        {
            get
            {
                return firstResult == 0 ? firstResult + 1 : firstResult;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int LastRow
        {
            get
            {
                if (maxResults == int.MaxValue)
                {
                    return maxResults;
                }
                return FirstRow + maxResults;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int MaxResults
        {
            get
            {
                return maxResults;
            }
            set
            {
                this.maxResults = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual object Parameter
        {
            get
            {
                return parameter;
            }
            set
            {
                this.parameter = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string OrderBy
        {
            get
            {
                // the default order column
                return "RES.ID_ asc";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string OrderByColumns
        {
            get
            {
                return OrderBy;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string DatabaseType
        {
            set
            {
                this.databaseType = value;
            }
            get
            {
                return databaseType;
            }
        }
    }
}