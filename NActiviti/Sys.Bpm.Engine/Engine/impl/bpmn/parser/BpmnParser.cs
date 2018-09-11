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
namespace org.activiti.engine.impl.bpmn.parser
{
    using org.activiti.engine.impl.bpmn.parser.factory;
    using org.activiti.engine.impl.cfg;

    /// <summary>
    /// Parser for BPMN 2.0 process models.
    /// 
    /// There is only one instance of this parser in the process engine. This <seealso cref="BpmnParser"/> creates <seealso cref="BpmnParse"/> instances that can be used to actually parse the BPMN 2.0 XML process definitions.
    /// 
    /// 
    /// 
    /// </summary>
    public class BpmnParser
    {

        /// <summary>
        /// The namepace of the BPMN 2.0 diagram interchange elements.
        /// </summary>
        public const string BPMN_DI_NS = "http://www.omg.org/spec/BPMN/20100524/DI";

        /// <summary>
        /// The namespace of the BPMN 2.0 diagram common elements.
        /// </summary>
        public const string BPMN_DC_NS = "http://www.omg.org/spec/DD/20100524/DC";

        /// <summary>
        /// The namespace of the generic OMG DI elements (don't ask me why they didn't use the BPMN_DI_NS ...)
        /// </summary>
        public const string OMG_DI_NS = "http://www.omg.org/spec/DD/20100524/DI";

        protected internal IActivityBehaviorFactory activityBehaviorFactory;
        protected internal IListenerFactory listenerFactory;
        protected internal IBpmnParseFactory bpmnParseFactory;
        protected internal BpmnParseHandlers bpmnParserHandlers;

        /// <summary>
        /// Creates a new <seealso cref="BpmnParse"/> instance that can be used to parse only one BPMN 2.0 process definition.
        /// </summary>
        public virtual BpmnParse createParse()
        {
            return bpmnParseFactory.createBpmnParse(this);
        }

        public virtual IActivityBehaviorFactory ActivityBehaviorFactory
        {
            get
            {
                return activityBehaviorFactory;
            }
            set
            {
                this.activityBehaviorFactory = value;
            }
        }


        public virtual IListenerFactory ListenerFactory
        {
            get
            {
                return listenerFactory;
            }
            set
            {
                this.listenerFactory = value;
            }
        }


        public virtual IBpmnParseFactory BpmnParseFactory
        {
            get
            {
                return bpmnParseFactory;
            }
            set
            {
                this.bpmnParseFactory = value;
            }
        }


        public virtual BpmnParseHandlers BpmnParserHandlers
        {
            get
            {
                return bpmnParserHandlers;
            }
            set
            {
                this.bpmnParserHandlers = value;
            }
        }

    }

}