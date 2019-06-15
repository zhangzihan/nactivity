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
using System.Xml.Linq;

namespace org.activiti.bpmn.converter
{
    using Sys.Bpm;
    using System.Linq;
    using System.Xml.Linq;

    /// 
    public abstract class DelegatingXMLStreamWriter : XMLStreamWriter
    {
        public DelegatingXMLStreamWriter(XElement writer) : base(writer.CreateWriter())
        {
        }

        public virtual INamespaceContext NamespaceContext
        {
            set
            {
                //writer.NamespaceContext = value;
            }
            get
            {
                return null;
                //return writer.NamespaceContext;
            }
        }

        //public virtual object GetProperty(string name)
        //{
        //    return element.Attribute(name)?.Value;
        //}
    }

}