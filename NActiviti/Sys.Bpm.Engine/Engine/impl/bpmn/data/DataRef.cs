﻿/* Licensed under the Apache License, Version 2.0 (the "License");
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
namespace Sys.Workflow.Engine.Impl.Bpmn.Datas
{
    /// <summary>
    /// Implementation of the BPMN 2.0 'dataInputRef' and 'dataOutputRef'
    /// 
    /// 
    /// </summary>
    public class DataRef
    {
        /// <summary>
        /// 
        /// </summary>
        protected internal string idRef;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idRef"></param>
        public DataRef(string idRef)
        {
            this.idRef = idRef;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string IdRef
        {
            get
            {
                return this.idRef;
            }
        }
    }
}