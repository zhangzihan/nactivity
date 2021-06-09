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
namespace Sys.Workflow.Bpmn.Models
{
    public class Association : Artifact
    {

        protected internal AssociationDirection associationDirection = AssociationDirection.NONE;
        protected internal string sourceRef;
        protected internal string targetRef;

        public virtual AssociationDirection AssociationDirection
        {
            get
            {
                return associationDirection;
            }
            set
            {
                this.associationDirection = value;
            }
        }


        public virtual string SourceRef
        {
            get
            {
                return sourceRef;
            }
            set
            {
                this.sourceRef = value;
            }
        }


        public virtual string TargetRef
        {
            get
            {
                return targetRef;
            }
            set
            {
                this.targetRef = value;
            }
        }


        public override BaseElement Clone()
        {
            Association clone = new Association
            {
                Values = this
            };
            return clone;
        }

        public override BaseElement Values
        {
            set
            {
                base.Values = value;
                var val = value as Association;

                SourceRef = val.SourceRef;
                TargetRef = val.TargetRef;

                if (val.AssociationDirection is object)
                {
                    AssociationDirection = val.AssociationDirection;
                }
            }
        }
    }

}