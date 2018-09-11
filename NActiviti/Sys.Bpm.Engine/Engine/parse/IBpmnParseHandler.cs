using System;
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
namespace org.activiti.engine.parse
{

    using org.activiti.bpmn.model;
    using org.activiti.engine.impl.bpmn.parser;
    using org.activiti.engine.impl.bpmn.parser.handler;
    using org.activiti.engine.impl.cfg;

    /// <summary>
    /// Allows to hook into the parsing of one or more elements during the parsing of a BPMN 2.0 process. For more details, see the userguide section on bpmn parse handlers.
    /// 
    /// Instances of this class can be injected into the <seealso cref="ProcessEngineConfigurationImpl"/>. The handler will then be called whenever a BPMN 2.0 element is parsed that matches the types returned by the
    /// <seealso cref="#getHandledTypes()"/> method.
    /// </summary>
    /// <seealso cref= AbstractBpmnParseHandler
    /// 
    ///  </seealso>
    public interface IBpmnParseHandler
    {

        /// <summary>
        /// The types for which this handler must be called during process parsing.
        /// </summary>
        ICollection<Type> HandledTypes { get; }

        /// <summary>
        /// The actual delegation method. The parser will calls this method on a match with the <seealso cref="#getHandledTypes()"/> return value.
        /// </summary>
        /// <param name="bpmnParse">
        ///          The <seealso cref="BpmnParse"/> instance that acts as container for all things produced during the parsing. </param>
        void parse(BpmnParse bpmnParse, BaseElement element);

    }

}