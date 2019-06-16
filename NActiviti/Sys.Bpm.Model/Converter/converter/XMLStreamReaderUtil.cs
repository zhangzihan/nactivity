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
using Microsoft.Extensions.Logging;
using Sys.Workflow.Model;
using System;
using System.IO;

namespace Sys.Workflow.Bpmn.Converters
{
    /// 
    public class XMLStreamReaderUtil
    {
        private static readonly ILogger logger = BpmnModelLoggerFactory.LoggerService<XMLStreamReaderUtil>();

        public static string moveDown(XMLStreamReader xtr)
        {
            try
            {
                while (xtr.HasNext())
                {
                    //xtr.next();

                    if (xtr.IsStartElement())
                    {
                        return xtr.LocalName;
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Error while moving down in XML document");
            }

            return null;
        }

        public static bool MoveToEndOfElement(XMLStreamReader xtr, string elementName)
        {
            try
            {
                while (xtr.HasNext())
                {
                    //xtr.next();

                    if (xtr.IsEmptyElement || xtr.EndElement)
                    {
                        if (xtr.LocalName == elementName)
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogWarning(e, $"Error while moving to end of element {elementName}");
            }
            return false;
        }

        public static XMLStreamReader CreateStreamReader(string bpmnXML)
        {
            MemoryStream ms = new MemoryStream();
            StreamWriter sw = new StreamWriter(ms);
            sw.Write(bpmnXML);
            sw.Flush();
            ms.Seek(0, SeekOrigin.Begin);

            return new XMLStreamReader(ms);
        }
    }
}