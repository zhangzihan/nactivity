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
namespace org.activiti.bpmn.model
{
    public class GraphicInfo
    {

        protected internal double x;
        protected internal double y;
        protected internal double height;
        protected internal double width;
        protected internal BaseElement element;
        protected internal bool? expanded;
        protected internal int xmlRowNumber;
        protected internal int xmlColumnNumber;

        public virtual double X
        {
            get
            {
                return x;
            }
            set
            {
                this.x = value;
            }
        }


        public virtual double Y
        {
            get
            {
                return y;
            }
            set
            {
                this.y = value;
            }
        }


        public virtual double Height
        {
            get
            {
                return height;
            }
            set
            {
                this.height = value;
            }
        }


        public virtual double Width
        {
            get
            {
                return width;
            }
            set
            {
                this.width = value;
            }
        }


        public virtual bool? Expanded
        {
            get
            {
                return expanded;
            }
            set
            {
                this.expanded = value;
            }
        }


        public virtual BaseElement Element
        {
            get
            {
                return element;
            }
            set
            {
                this.element = value;
            }
        }


        public virtual int XmlRowNumber
        {
            get
            {
                return xmlRowNumber;
            }
            set
            {
                this.xmlRowNumber = value;
            }
        }


        public virtual int XmlColumnNumber
        {
            get
            {
                return xmlColumnNumber;
            }
            set
            {
                this.xmlColumnNumber = value;
            }
        }

    }

}