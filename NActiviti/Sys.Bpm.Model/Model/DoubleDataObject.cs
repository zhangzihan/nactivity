using System;

namespace Sys.Workflow.bpmn.model
{
    public class DoubleDataObject : ValuedDataObject
    {

        public override object Value
        {
            set
            {
                this.value = Convert.ToDouble(value.ToString());
            }
        }

        public override BaseElement Clone()
        {
            DoubleDataObject clone = new DoubleDataObject
            {
                Values = this
            };
            return clone;
        }
    }

}