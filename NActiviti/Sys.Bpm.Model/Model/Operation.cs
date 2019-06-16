using System.Collections.Generic;

namespace Sys.Workflow.bpmn.model
{

    public class Operation : BaseElement
    {

        protected internal string name;
        protected internal string implementationRef;
        protected internal string inMessageRef;
        protected internal string outMessageRef;
        protected internal IList<string> errorMessageRef = new List<string>();

        public virtual string Name
        {
            get
            {
                return name;
            }
            set
            {
                this.name = value;
            }
        }


        public virtual string ImplementationRef
        {
            get
            {
                return implementationRef;
            }
            set
            {
                this.implementationRef = value;
            }
        }


        public virtual string InMessageRef
        {
            get
            {
                return inMessageRef;
            }
            set
            {
                this.inMessageRef = value;
            }
        }


        public virtual string OutMessageRef
        {
            get
            {
                return outMessageRef;
            }
            set
            {
                this.outMessageRef = value;
            }
        }


        public virtual IList<string> ErrorMessageRef
        {
            get
            {
                return errorMessageRef;
            }
            set
            {
                this.errorMessageRef = value;
            }
        }


        public override BaseElement Clone()
        {
            Operation clone = new Operation
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
                var val = value as Operation;

                Name = val.Name;
                ImplementationRef = val.ImplementationRef;
                InMessageRef = val.InMessageRef;
                OutMessageRef = val.OutMessageRef;

                errorMessageRef = new List<string>();
                if (val.ErrorMessageRef != null && val.ErrorMessageRef.Count > 0)
                {
                    ((List<string>)errorMessageRef).AddRange(val.ErrorMessageRef);
                }
            }
        }
    }

}