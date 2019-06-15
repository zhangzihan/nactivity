using System;

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

namespace org.activiti.engine
{
    /// <summary>
    /// Runtime exception indicating the requested class was not found or an error occurred while loading the class.
    /// 
    /// 
    /// </summary>
    public class ActivitiClassLoadingException : ActivitiException
    {

        private const long serialVersionUID = 1L;
        protected internal string className;

        public ActivitiClassLoadingException(string className, Exception cause) : base(GetExceptionMessageMessage(className, cause), cause)
        {
            this.className = className;
        }

        /// <summary>
        /// Returns the name of the class this exception is related to.
        /// </summary>
        public virtual string ClassName
        {
            get
            {
                return className;
            }
        }

        private static string GetExceptionMessageMessage(string className, Exception cause)
        {
            if (cause is Exception)
            {
                return "Class not found: " + className;
            }
            else
            {
                return "Could not load class: " + className;
            }
        }

    }

}