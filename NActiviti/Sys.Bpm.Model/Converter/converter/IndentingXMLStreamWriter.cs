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
namespace Sys.Workflow.Bpmn.Converters
{


    /// 
    public class IndentingXMLStreamWriter : DelegatingXMLStreamWriter
    {

        private static readonly object SEEN_NOTHING = new object();
        private static readonly object SEEN_ELEMENT = new object();
        private static readonly object SEEN_DATA = new object();

        private object state = SEEN_NOTHING;
        private readonly Stack<object> stateStack = new Stack<object>();

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
        public virtual int GetIndentStep()
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
        public virtual void SetIndentStep(int indentStep)
        {
            StringBuilder s = new StringBuilder();
            for (; indentStep > 0; indentStep--)
            {
                s.Append(' ');
            }
            SetIndentStep(s.ToString());
        }

        public virtual void SetIndentStep(string s)
        {
            this.indentStep = s;
        }

        private void OnStartElement()
        {
            stateStack.Push(SEEN_ELEMENT);
            state = SEEN_NOTHING;
            if (depth > 0)
            {
                base.WriteCharacters("\n");
            }
            DoIndent();
            depth++;
        }

        private void OnEndElement()
        {
            depth--;
            if (state == SEEN_ELEMENT)
            {
                base.WriteCharacters("\n");
                DoIndent();
            }
            state = stateStack.Pop();
        }

        private void OnEmptyElement()
        {
            state = SEEN_ELEMENT;
            if (depth > 0)
            {
                base.WriteCharacters("\n");
            }
            DoIndent();
        }

        /// <summary>
        /// Print indentation for the current level.
        /// </summary>
        /// <exception cref="org.xml.sax.SAXException">
        ///              If there is an error writing the indentation characters, or if a filter further down the chain raises an exception. </exception>
        private void DoIndent()
        {
            if (depth > 0)
            {
                for (int i = 0; i < depth; i++)
                {
                    base.WriteCharacters(indentStep);
                }
            }
        }

        public override void WriteStartDocument()
        {
            base.WriteStartDocument();
            base.WriteCharacters("\n");
        }

        public override void WriteStartDocument(string version)
        {
            base.WriteStartDocument(version);
            base.WriteCharacters("\n");
        }


        public override void WriteStartDocument(string encoding, string version)
        {
            base.WriteStartDocument(encoding, version);
            base.WriteCharacters("\n");
        }

        public override void WriteStartElement(string localName)
        {
            OnStartElement();
            base.WriteStartElement(localName);
        }

        public override void WriteStartElement(string namespaceURI, string localName)
        {
            OnStartElement();
            base.WriteStartElement(namespaceURI, localName);
        }

        public override void WriteStartElement(string prefix, string localName, string namespaceURI)
        {
            OnStartElement();
            base.WriteStartElement(prefix, localName, namespaceURI);
        }

        public override void WriteEmptyElement(string namespaceURI, string localName)
        {
            OnEmptyElement();
            base.WriteEmptyElement(namespaceURI, localName);
        }

        public override void WriteEmptyElement(string prefix, string localName, string namespaceURI)
        {
            OnEmptyElement();
            base.WriteEmptyElement(prefix, localName, namespaceURI);
        }

        public override void WriteEmptyElement(string localName)
        {
            OnEmptyElement();
            base.WriteEmptyElement(localName);
        }

        public override void WriteEndElement()
        {
            OnEndElement();
            base.WriteEndElement();
        }

        public override void WriteCharacters(string text)
        {
            state = SEEN_DATA;
            base.WriteCharacters(text);
        }

        public override void WriteCharacters(char[] text, int start, int len)
        {
            state = SEEN_DATA;
            base.WriteCharacters(text, start, len);
        }

        public override void WriteCData(string data)
        {
            state = SEEN_DATA;
            base.WriteCData(data);
        }
    }
}