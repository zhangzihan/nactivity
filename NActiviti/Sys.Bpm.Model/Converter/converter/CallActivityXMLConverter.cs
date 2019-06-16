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
namespace Sys.Workflow.bpmn.converter
{
    using Sys.Workflow.bpmn.constants;
    using Sys.Workflow.bpmn.converter.child;
    using Sys.Workflow.bpmn.converter.util;
    using Sys.Workflow.bpmn.model;

    /// 
    public class CallActivityXMLConverter : BaseBpmnXMLConverter
    {

        protected internal IDictionary<string, BaseChildElementParser> childParserMap = new Dictionary<string, BaseChildElementParser>();

        public CallActivityXMLConverter()
        {
            InParameterParser inParameterParser = new InParameterParser(this);
            childParserMap[inParameterParser.ElementName] = inParameterParser;
            OutParameterParser outParameterParser = new OutParameterParser(this);
            childParserMap[outParameterParser.ElementName] = outParameterParser;
        }

        public override Type BpmnElementType
        {
            get
            {
                return typeof(CallActivity);
            }
        }

        public override string XMLElementName
        {
            get
            {
                return BpmnXMLConstants.ELEMENT_CALL_ACTIVITY;
            }
        }
        protected internal override BaseElement ConvertXMLToElement(XMLStreamReader xtr, BpmnModel model)
        {
            CallActivity callActivity = new CallActivity();
            BpmnXMLUtil.AddXMLLocation(callActivity, xtr);
            callActivity.CalledElement = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_CALL_ACTIVITY_CALLEDELEMENT);
            callActivity.BusinessKey = xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_CALL_ACTIVITY_BUSINESS_KEY);
            bool.TryParse(xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_CALL_ACTIVITY_INHERIT_BUSINESS_KEY), out callActivity.inheritBusinessKey);
            bool.TryParse(xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_CALL_ACTIVITY_INHERITVARIABLES), out callActivity.inheritVariables);
            ParseChildElements(XMLElementName, callActivity, childParserMap, model, xtr);
            return callActivity;
        }
        protected internal override void WriteAdditionalAttributes(BaseElement element, BpmnModel model, XMLStreamWriter xtw)
        {
            CallActivity callActivity = (CallActivity)element;
            if (!string.IsNullOrWhiteSpace(callActivity.CalledElement))
            {
                xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_CALL_ACTIVITY_CALLEDELEMENT, callActivity.CalledElement);
            }
            if (!string.IsNullOrWhiteSpace(callActivity.BusinessKey))
            {
                WriteQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_CALL_ACTIVITY_BUSINESS_KEY, callActivity.BusinessKey, xtw);
            }
            if (callActivity.InheritBusinessKey)
            {
                WriteQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_CALL_ACTIVITY_INHERIT_BUSINESS_KEY, "true", xtw);
            }
            xtw.WriteAttribute(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_CALL_ACTIVITY_INHERITVARIABLES, callActivity.InheritVariables.ToString());
        }
        protected internal override bool WriteExtensionChildElements(BaseElement element, bool didWriteExtensionStartElement, XMLStreamWriter xtw)
        {
            CallActivity callActivity = (CallActivity)element;
            didWriteExtensionStartElement = WriteIOParameters(BpmnXMLConstants.ELEMENT_CALL_ACTIVITY_IN_PARAMETERS, callActivity.InParameters, didWriteExtensionStartElement, xtw);
            didWriteExtensionStartElement = WriteIOParameters(BpmnXMLConstants.ELEMENT_CALL_ACTIVITY_OUT_PARAMETERS, callActivity.OutParameters, didWriteExtensionStartElement, xtw);
            return didWriteExtensionStartElement;
        }
        protected internal override void WriteAdditionalChildElements(BaseElement element, BpmnModel model, XMLStreamWriter xtw)
        {
        }
        private bool WriteIOParameters(string elementName, IList<IOParameter> parameterList, bool didWriteExtensionStartElement, XMLStreamWriter xtw)
        {

            if (parameterList.Count == 0)
            {
                return didWriteExtensionStartElement;
            }

            foreach (IOParameter ioParameter in parameterList)
            {
                if (!didWriteExtensionStartElement)
                {
                    xtw.WriteStartElement(BpmnXMLConstants.BPMN_PREFIX, BpmnXMLConstants.ELEMENT_EXTENSIONS, BpmnXMLConstants.BPMN2_NAMESPACE);
                    didWriteExtensionStartElement = true;
                }

                xtw.WriteStartElement(BpmnXMLConstants.ACTIVITI_EXTENSIONS_PREFIX, elementName, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE);
                if (!string.IsNullOrWhiteSpace(ioParameter.Source))
                {
                    WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_IOPARAMETER_SOURCE, ioParameter.Source, xtw);
                }
                if (!string.IsNullOrWhiteSpace(ioParameter.SourceExpression))
                {
                    WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_IOPARAMETER_SOURCE_EXPRESSION, ioParameter.SourceExpression, xtw);
                }
                if (!string.IsNullOrWhiteSpace(ioParameter.Target))
                {
                    WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_IOPARAMETER_TARGET, ioParameter.Target, xtw);
                }

                xtw.WriteEndElement();
            }

            return didWriteExtensionStartElement;
        }

        public class InParameterParser : BaseChildElementParser
        {
            private readonly CallActivityXMLConverter outerInstance;

            public InParameterParser(CallActivityXMLConverter outerInstance)
            {
                this.outerInstance = outerInstance;
            }


            public override string ElementName
            {
                get
                {
                    return BpmnXMLConstants.ELEMENT_CALL_ACTIVITY_IN_PARAMETERS;
                }
            }
            public override void ParseChildElement(XMLStreamReader xtr, BaseElement parentElement, BpmnModel model)
            {
                string source = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_IOPARAMETER_SOURCE);
                string sourceExpression = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_IOPARAMETER_SOURCE_EXPRESSION);
                string target = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_IOPARAMETER_TARGET);
                if ((!string.IsNullOrWhiteSpace(source) || !string.IsNullOrWhiteSpace(sourceExpression)) && !string.IsNullOrWhiteSpace(target))
                {

                    IOParameter parameter = new IOParameter();
                    if (!string.IsNullOrWhiteSpace(sourceExpression))
                    {
                        parameter.SourceExpression = sourceExpression;
                    }
                    else
                    {
                        parameter.Source = source;
                    }

                    parameter.Target = target;

                    ((CallActivity)parentElement).InParameters.Add(parameter);
                }
            }
        }

        public class OutParameterParser : BaseChildElementParser
        {
            private readonly CallActivityXMLConverter outerInstance;

            public OutParameterParser(CallActivityXMLConverter outerInstance)
            {
                this.outerInstance = outerInstance;
            }


            public override string ElementName
            {
                get
                {
                    return BpmnXMLConstants.ELEMENT_CALL_ACTIVITY_OUT_PARAMETERS;
                }
            }
            public override void ParseChildElement(XMLStreamReader xtr, BaseElement parentElement, BpmnModel model)
            {
                string source = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_IOPARAMETER_SOURCE);
                string sourceExpression = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_IOPARAMETER_SOURCE_EXPRESSION);
                string target = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_IOPARAMETER_TARGET);
                if ((!string.IsNullOrWhiteSpace(source) || !string.IsNullOrWhiteSpace(sourceExpression)) && !string.IsNullOrWhiteSpace(target))
                {

                    IOParameter parameter = new IOParameter();
                    if (!string.IsNullOrWhiteSpace(sourceExpression))
                    {
                        parameter.SourceExpression = sourceExpression;
                    }
                    else
                    {
                        parameter.Source = source;
                    }

                    parameter.Target = target;

                    ((CallActivity)parentElement).OutParameters.Add(parameter);
                }
            }
        }
    }

}