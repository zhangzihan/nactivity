using System.Collections.Generic;

namespace org.activiti.bpmn.model
{

    public class Interface : BaseElement
    {

        protected internal string name;
        protected internal string implementationRef;
        protected internal IList<Operation> operations = new List<Operation>();

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


        public virtual IList<Operation> Operations
        {
            get
            {
                return operations;
            }
            set
            {
                this.operations = value;
            }
        }


        public override BaseElement clone()
        {
            Interface clone = new Interface();
            clone.Values = this;
            return clone;
        }

        public override BaseElement Values
        {
            set
            {
                base.Values = value;
                var val = value as Interface;

                Name = val.Name;
                ImplementationRef = val.ImplementationRef;

                operations = new List<Operation>();
                if (val.Operations != null && val.Operations.Count > 0)
                {
                    foreach (Operation operation in val.Operations)
                    {
                        operations.Add(operation.clone() as Operation);
                    }
                }
            }
        }
    }

}