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
    public class MapExceptionEntry
    {

        internal string errorCode;
        internal string className;
        internal bool andChildren;

        public MapExceptionEntry(string errorCode, string className, bool andChildren)
        {

            this.errorCode = errorCode;
            this.className = className;
            this.andChildren = andChildren;
        }

        public virtual string ErrorCode
        {
            get
            {
                return errorCode;
            }
            set
            {
                this.errorCode = value;
            }
        }


        public virtual string ClassName
        {
            get
            {
                return className;
            }
            set
            {
                this.className = value;
            }
        }


        public virtual bool AndChildren
        {
            get
            {
                return andChildren;
            }
            set
            {
                this.andChildren = value;
            }
        }


    }

}