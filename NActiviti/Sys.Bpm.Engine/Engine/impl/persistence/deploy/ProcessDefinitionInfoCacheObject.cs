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
using Newtonsoft.Json.Linq;
using System;

namespace Sys.Workflow.Engine.Impl.Persistence.Deploies
{
    /// 
    public class ProcessDefinitionInfoCacheObject
    {

        protected internal string id;
        protected internal int revision;
        protected internal JToken infoNode;
        /// <summary>
        /// 上次回源校验 revision 的时间（UTC）。默认最小值，保证新建的缓存对象首次访问时必回源校验
        /// </summary>
        protected internal DateTime lastRevisionCheckTime = DateTime.MinValue;

        public virtual string Id
        {
            get
            {
                return id;
            }
            set
            {
                this.id = value;
            }
        }


        public virtual int Revision
        {
            get
            {
                return revision;
            }
            set
            {
                this.revision = value;
            }
        }


        public virtual JToken InfoNode
        {
            get
            {
                return infoNode;
            }
            set
            {
                this.infoNode = value;
            }
        }


        public virtual DateTime LastRevisionCheckTime
        {
            get
            {
                return lastRevisionCheckTime;
            }
            set
            {
                this.lastRevisionCheckTime = value;
            }
        }

    }
}