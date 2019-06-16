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
namespace Sys.Workflow.engine.impl.bpmn.parser
{

    using Sys.Workflow.bpmn.model;
    using Sys.Workflow.engine.impl.bpmn.data;
    using Sys.Workflow.engine.impl.webservice;

    /// <summary>
    /// A XML importer
    /// 
    /// 
    /// </summary>
    public interface IXMLImporter
    {

        /// <summary>
        /// Imports the definitions in the XML declared in element
        /// </summary>
        /// <param name="element">
        ///          the declarations to be imported </param>
        void ImportFrom(Import theImport, string sourceSystemId);

        IDictionary<string, IStructureDefinition> Structures { get; }

        IDictionary<string, WSService> Services { get; }

        IDictionary<string, WSOperation> Operations { get; }
    }
}