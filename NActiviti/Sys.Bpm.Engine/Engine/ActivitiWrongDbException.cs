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
namespace Sys.Workflow.Engine
{
    /// <summary>
    /// Exception that is thrown when the Activiti engine discovers a mismatch between the database schema version and the engine version.
    /// 
    /// The check is done when the engine is created in <seealso cref="ProcessEngineBuilder#buildProcessEngine()"/>.
    /// 
    /// 
    /// </summary>
    public class ActivitiWrongDbException : ActivitiException
    {

        private const long serialVersionUID = 1L;

        internal string libraryVersion;
        internal string dbVersion;

        public ActivitiWrongDbException(string libraryVersion, string dbVersion) : base("version mismatch: activiti library version is '" + libraryVersion + "', db version is " + dbVersion + " Hint: Set <property name=\"databaseSchemaUpdate\" to value=\"true\" or value=\"create-drop\" (use create-drop for testing only!) in bean processEngineConfiguration in activiti.cfg.xml for automatic schema creation")
        {
            this.libraryVersion = libraryVersion;
            this.dbVersion = dbVersion;
        }

        /// <summary>
        /// The version of the Activiti library used.
        /// </summary>
        public virtual string LibraryVersion
        {
            get
            {
                return libraryVersion;
            }
        }

        /// <summary>
        /// The version of the Activiti library that was used to create the database schema.
        /// </summary>
        public virtual string DbVersion
        {
            get
            {
                return dbVersion;
            }
        }
    }

}