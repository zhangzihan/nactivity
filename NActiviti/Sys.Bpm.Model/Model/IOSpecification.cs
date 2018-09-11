using System.Collections.Generic;

namespace org.activiti.bpmn.model
{

    public class IOSpecification : BaseElement
    {

        protected internal IList<DataSpec> dataInputs = new List<DataSpec>();
        protected internal IList<DataSpec> dataOutputs = new List<DataSpec>();
        protected internal IList<string> dataInputRefs = new List<string>();
        protected internal IList<string> dataOutputRefs = new List<string>();

        public virtual IList<DataSpec> DataInputs
        {
            get
            {
                return dataInputs;
            }
            set
            {
                this.dataInputs = value;
            }
        }


        public virtual IList<DataSpec> DataOutputs
        {
            get
            {
                return dataOutputs;
            }
            set
            {
                this.dataOutputs = value;
            }
        }


        public virtual IList<string> DataInputRefs
        {
            get
            {
                return dataInputRefs;
            }
            set
            {
                this.dataInputRefs = value;
            }
        }


        public virtual IList<string> DataOutputRefs
        {
            get
            {
                return dataOutputRefs;
            }
            set
            {
                this.dataOutputRefs = value;
            }
        }


        public override BaseElement clone()
        {
            IOSpecification clone = new IOSpecification();
            clone.Values = this;
            return clone;
        }

        public override BaseElement Values
        {
            set
            {
                var val = value as IOSpecification;

                dataInputs = new List<DataSpec>();
                if (val.DataInputs != null && val.DataInputs.Count > 0)
                {
                    foreach (DataSpec dataSpec in val.DataInputs)
                    {
                        dataInputs.Add(dataSpec.clone() as DataSpec);
                    }
                }

                dataOutputs = new List<DataSpec>();
                if (val.DataOutputs != null && val.DataOutputs.Count > 0)
                {
                    foreach (DataSpec dataSpec in val.DataOutputs)
                    {
                        dataOutputs.Add(dataSpec.clone() as DataSpec);
                    }
                }

                dataInputRefs = new List<string>(val.DataInputRefs);
                dataOutputRefs = new List<string>(val.DataOutputRefs);
            }
        }
    }

}