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
    /// An exception indicating that the object that is required or actioned on does not exist.
    /// 
    /// 
    /// </summary>
    public class ActivitiObjectNotFoundException : ActivitiException
    {

        private const long serialVersionUID = 1L;

        private Type objectClass;

        public ActivitiObjectNotFoundException(string message) : base(message)
        {
        }

        public ActivitiObjectNotFoundException(string message, Type objectClass) : this(message, objectClass, null)
        {
        }

        public ActivitiObjectNotFoundException(Type objectClass) : this(null, objectClass, null)
        {
        }

        public ActivitiObjectNotFoundException(string message, Type objectClass, Exception cause) : base(message, cause)
        {
            this.objectClass = objectClass;
        }

        /// <summary>
        /// The class of the object that was not found. Contains the interface-class of the activiti-object that was not found.
        /// </summary>
        public virtual Type ObjectClass
        {
            get
            {
                return objectClass;
            }
        }
    }

}