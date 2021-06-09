using System.Collections.Generic;

namespace Sys.Workflow.Bpmn.Models
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


        public override BaseElement Clone()
        {
            IOSpecification clone = new IOSpecification
            {
                Values = this
            };
            return clone;
        }

        public override BaseElement Values
        {
            set
            {
                var val = value as IOSpecification;

                dataInputs = new List<DataSpec>();
                if (val.DataInputs is object && val.DataInputs.Count > 0)
                {
                    foreach (DataSpec dataSpec in val.DataInputs)
                    {
                        dataInputs.Add(dataSpec.Clone() as DataSpec);
                    }
                }

                dataOutputs = new List<DataSpec>();
                if (val.DataOutputs is object && val.DataOutputs.Count > 0)
                {
                    foreach (DataSpec dataSpec in val.DataOutputs)
                    {
                        dataOutputs.Add(dataSpec.Clone() as DataSpec);
                    }
                }

                dataInputRefs = new List<string>(val.DataInputRefs);
                dataOutputRefs = new List<string>(val.DataOutputRefs);
            }
        }
    }

}