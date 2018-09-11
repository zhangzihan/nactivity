using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

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
namespace org.activiti.bpmn.converter
{


    /// 
    public class IndentingXMLStreamWriter : DelegatingXMLStreamWriter
    {

        private static readonly object SEEN_NOTHING = new object();
        private static readonly object SEEN_ELEMENT = new object();
        private static readonly object SEEN_DATA = new object();

        private object state = SEEN_NOTHING;
        private Stack<object> stateStack = new Stack<object>();

        private string indentStep = "  ";
        private int depth = 0;

        public IndentingXMLStreamWriter(XElement writer) : base(writer)
        {
        }

        /// <summary>
        /// Return the current indent step.
        /// 
        /// <para>
        /// Return the current indent step: each start tag will be indented by this number of spaces times the number of ancestors that the element has.
        /// </para>
        /// </summary>
        /// <returns> The number of spaces in each indentation step, or 0 or less for no indentation. </returns>
        /// <seealso cref= #setIndentStep(int)
        /// </seealso>
        /// @deprecated Only return the length of the indent string. 
        public virtual int getIndentStep()
        {
            return indentStep.Length;
        }

        /// <summary>
        /// Set the current indent step.
        /// </summary>
        /// <param name="indentStep">
        ///          The new indent step (0 or less for no indentation). </param>
        /// <seealso cref= #getIndentStep()
        /// </seealso>
        /// @deprecated Should use the version that takes string. 
        public virtual void setIndentStep(int indentStep)
        {
            StringBuilder s = new StringBuilder();
            for (; indentStep > 0; indentStep--)
            {
                s.Append(' ');
            }
            setIndentStep(s.ToString());
        }

        public virtual void setIndentStep(string s)
        {
            this.indentStep = s;
        }

        private void onStartElement()
        {
            stateStack.Push(SEEN_ELEMENT);
            state = SEEN_NOTHING;
            if (depth > 0)
            {
                base.writeCharacters("\n");
            }
            doIndent();
            depth++;
        }

        private void onEndElement()
        {
            depth--;
            if (state == SEEN_ELEMENT)
            {
                base.writeCharacters("\n");
                doIndent();
            }
            state = stateStack.Pop();
        }

        private void onEmptyElement()
        {
            state = SEEN_ELEMENT;
            if (depth > 0)
            {
                base.writeCharacters("\n");
            }
            doIndent();
        }

        /// <summary>
        /// Print indentation for the current level.
        /// </summary>
        /// <exception cref="org.xml.sax.SAXException">
        ///              If there is an error writing the indentation characters, or if a filter further down the chain raises an exception. </exception>
        private void doIndent()
        {
            if (depth > 0)
            {
                for (int i = 0; i < depth; i++)
                {
                    base.writeCharacters(indentStep);
                }
            }
        }

        public override void writeStartDocument()
        {
            base.writeStartDocument();
            base.writeCharacters("\n");
        }

        public override void writeStartDocument(string version)
        {
            base.writeStartDocument(version);
            base.writeCharacters("\n");
        }


        public override void writeStartDocument(string encoding, string version)
        {
            base.writeStartDocument(encoding, version);
            base.writeCharacters("\n");
        }

        public override void writeStartElement(string localName)
        {
            onStartElement();
            base.writeStartElement(localName);
        }

        public override void writeStartElement(string namespaceURI, string localName)
        {
            onStartElement();
            base.writeStartElement(namespaceURI, localName);
        }

        public override void writeStartElement(string prefix, string localName, string namespaceURI)
        {
            onStartElement();
            base.writeStartElement(prefix, localName, namespaceURI);
        }

        public override void writeEmptyElement(string namespaceURI, string localName)
        {
            onEmptyElement();
            base.writeEmptyElement(namespaceURI, localName);
        }

        public override void writeEmptyElement(string prefix, string localName, string namespaceURI)
        {
            onEmptyElement();
            base.writeEmptyElement(prefix, localName, namespaceURI);
        }

        public override void writeEmptyElement(string localName)
        {
            onEmptyElement();
            base.writeEmptyElement(localName);
        }

        public override void writeEndElement()
        {
            onEndElement();
            base.writeEndElement();
        }

        public override void writeCharacters(string text)
        {
            state = SEEN_DATA;
            base.writeCharacters(text);
        }

        public override void writeCharacters(char[] text, int start, int len)
        {
            state = SEEN_DATA;
            base.writeCharacters(text, start, len);
        }

        public override void writeCData(string data)
        {
            state = SEEN_DATA;
            base.writeCData(data);
        }
    }
}