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
using System;

namespace org.activiti.bpmn.converter
{
    /// 
    public class XMLStreamReaderUtil
    {

        public static string moveDown(XMLStreamReader xtr)
        {
            try
            {
                while (xtr.hasNext())
                {
                    //xtr.next();

                    if (xtr.StartElement)
                    {
                        return xtr.LocalName;
                    }
                }
            }
            catch (Exception e)
            {
                //LOGGER.warn("Error while moving down in XML document", e);
            }

            return null;
        }

        public static bool moveToEndOfElement(XMLStreamReader xtr, string elementName)
        {
            try
            {
                while (xtr.hasNext())
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
                // LOGGER.warn("Error while moving to end of element {}", elementName, e);
            }
            return false;
        }
    }

}