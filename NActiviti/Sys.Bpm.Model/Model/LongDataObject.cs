using System;

namespace Sys.Workflow.bpmn.model
{
    public class LongDataObject : ValuedDataObject
    {

        public override object Value
        {
            set
            {
                this.value = Convert.ToInt64(value.ToString());
            }
        }

        public override BaseElement Clone()
        {
            LongDataObject clone = new LongDataObject
            {
                Values = this
            };
            return clone;
        }
    }

}