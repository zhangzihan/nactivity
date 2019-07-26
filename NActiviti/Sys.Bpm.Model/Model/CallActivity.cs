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
namespace Sys.Workflow.Bpmn.Models
{

    public class CallActivity : Activity
    {

        protected internal string calledElement;
        protected internal bool inheritVariables = true;
        protected internal IList<IOParameter> inParameters;
        protected internal IList<IOParameter> outParameters;
        protected internal string businessKey;
        protected internal bool inheritBusinessKey = true;

        public virtual string CalledElement
        {
            get
            {
                return calledElement;
            }
            set
            {
                this.calledElement = value;
            }
        }


        public virtual bool InheritVariables
        {
            get
            {
                return inheritVariables;
            }
            set
            {
                this.inheritVariables = value;
            }
        }


        public virtual IList<IOParameter> InParameters
        {
            get
            {
                if (inParameters is null)
                {
                    inParameters = new List<IOParameter>();
                }

                return inParameters;
            }
            set
            {
                this.inParameters = value;
            }
        }


        public virtual IList<IOParameter> OutParameters
        {
            get
            {
                if (outParameters is null)
                {
                    outParameters = new List<IOParameter>();
                }

                return outParameters;
            }
            set
            {
                this.outParameters = value;
            }
        }


        public virtual string BusinessKey
        {
            get
            {
                return businessKey;
            }
            set
            {
                this.businessKey = value;
            }
        }


        public virtual bool InheritBusinessKey
        {
            get
            {
                return inheritBusinessKey;
            }
            set
            {
                this.inheritBusinessKey = value;
            }
        }


        public override BaseElement Clone()
        {
            CallActivity clone = new CallActivity
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
                var val = value as CallActivity;

                CalledElement = val.CalledElement;
                BusinessKey = val.BusinessKey;
                InheritBusinessKey = val.InheritBusinessKey;

                inParameters = new List<IOParameter>();
                if (val.InParameters != null && val.InParameters.Count > 0)
                {
                    foreach (IOParameter parameter in val.InParameters)
                    {
                        inParameters.Add(parameter.Clone() as IOParameter);
                    }
                }

                outParameters = new List<IOParameter>();
                if (val.OutParameters != null && val.OutParameters.Count > 0)
                {
                    foreach (IOParameter parameter in val.OutParameters)
                    {
                        outParameters.Add(parameter.Clone() as IOParameter);
                    }
                }
            }
        }
    }

}