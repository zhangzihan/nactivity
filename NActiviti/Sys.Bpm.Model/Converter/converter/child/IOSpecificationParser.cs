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
namespace Sys.Workflow.Bpmn.Converters.Childs
{
    using Microsoft.Extensions.Logging;
    using Sys.Workflow.Bpmn.Constants;
    using Sys.Workflow.Bpmn.Converters.Utils;
    using Sys.Workflow.Bpmn.Models;
    using Sys.Workflow.Model;

    /// 
    public class IOSpecificationParser : BaseChildElementParser
    {
        private static readonly ILogger log = BpmnModelLoggerFactory.LoggerService<IOSpecificationParser>();

        public override string ElementName
        {
            get
            {
                return BpmnXMLConstants.ELEMENT_IOSPECIFICATION;
            }
        }

        public override void ParseChildElement(XMLStreamReader xtr, BaseElement parentElement, BpmnModel model)
        {

            if (parentElement is not Activity && parentElement is not Process)
            {
                return;
            }

            IOSpecification ioSpecification = new IOSpecification();
            BpmnXMLUtil.AddXMLLocation(ioSpecification, xtr);
            bool readyWithIOSpecification = false;
            try
            {
                while (!readyWithIOSpecification && xtr.HasNext())
                {
                    //xtr.next();

                    if (xtr.IsStartElement() && BpmnXMLConstants.ELEMENT_DATA_INPUT.Equals(xtr.LocalName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        DataSpec dataSpec = new DataSpec();
                        BpmnXMLUtil.AddXMLLocation(dataSpec, xtr);
                        dataSpec.Id = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_ID);
                        dataSpec.Name = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_NAME);
                        dataSpec.ItemSubjectRef = ParseItemSubjectRef(xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_ITEM_SUBJECT_REF), model);
                        ioSpecification.DataInputs.Add(dataSpec);

                    }
                    else if (xtr.IsStartElement() && BpmnXMLConstants.ELEMENT_DATA_OUTPUT.Equals(xtr.LocalName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        DataSpec dataSpec = new DataSpec();
                        BpmnXMLUtil.AddXMLLocation(dataSpec, xtr);
                        dataSpec.Id = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_ID);
                        dataSpec.Name = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_NAME);
                        dataSpec.ItemSubjectRef = ParseItemSubjectRef(xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_ITEM_SUBJECT_REF), model);
                        ioSpecification.DataOutputs.Add(dataSpec);

                    }
                    else if (xtr.IsStartElement() && BpmnXMLConstants.ELEMENT_DATA_INPUT_REFS.Equals(xtr.LocalName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        string dataInputRefs = xtr.ElementText;
                        if (!string.IsNullOrWhiteSpace(dataInputRefs))
                        {
                            ioSpecification.DataInputRefs.Add(dataInputRefs.Trim());
                        }

                    }
                    else if (xtr.IsStartElement() && BpmnXMLConstants.ELEMENT_DATA_OUTPUT_REFS.Equals(xtr.LocalName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        string dataOutputRefs = xtr.ElementText;
                        if (!string.IsNullOrWhiteSpace(dataOutputRefs))
                        {
                            ioSpecification.DataOutputRefs.Add(dataOutputRefs.Trim());
                        }

                    }
                    else if (xtr.EndElement && ElementName.Equals(xtr.LocalName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        readyWithIOSpecification = true;
                    }

                    if (xtr.IsEmptyElement && ElementName.Equals(xtr.LocalName, StringComparison.OrdinalIgnoreCase))
                    {
                        readyWithIOSpecification = true;
                    }
                } 
            }
            catch (Exception e)
            {
                log.LogWarning(e, "Error parsing ioSpecification child elements");
            }

            if (parentElement is Process)
            {
                ((Process)parentElement).IoSpecification = ioSpecification;
            }
            else
            {
                ((Activity)parentElement).IoSpecification = ioSpecification;
            }
        }

        protected internal virtual string ParseItemSubjectRef(string itemSubjectRef, BpmnModel model)
        {
            string result = null;
            if (!string.IsNullOrWhiteSpace(itemSubjectRef))
            {
                int indexOfP = itemSubjectRef.IndexOf(':');
                if (indexOfP != -1)
                {
                    string prefix = itemSubjectRef.Substring(0, indexOfP);
                    string resolvedNamespace = model.GetNamespace(prefix);
                    result = resolvedNamespace + ":" + itemSubjectRef.Substring(indexOfP + 1);
                }
                else
                {
                    result = model.TargetNamespace + ":" + itemSubjectRef;
                }
            }
            return result;
        }
    }

}