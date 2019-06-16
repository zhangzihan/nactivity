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

namespace Sys.Workflow.engine.repository
{
    /// <summary>
    /// Stores position and dimensions of a diagram node.
    /// 
    /// 
    /// </summary>
    [Serializable]
    public class DiagramNode : DiagramElement
    {

        private const long serialVersionUID = 1L;

        private double? x;
        private double? y;
        private double? width;
        private double? height;

        public DiagramNode() : base()
        {
        }

        public DiagramNode(string id) : base(id)
        {
        }

        public DiagramNode(string id, double? x, double? y, double? width, double? height) : base(id)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public virtual double? X
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


        public virtual double? Y
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


        public virtual double? Width
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


        public virtual double? Height
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


        public override string ToString()
        {
            return base.ToString() + ", x=" + X + ", y=" + Y + ", width=" + Width + ", height=" + Height;
        }

        public override bool Node
        {
            get
            {
                return true;
            }
        }

        public override bool Edge
        {
            get
            {
                return false;
            }
        }

    }

}