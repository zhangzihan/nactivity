using System;

namespace Sys.Workflow.Bpmn.Models
{
    public class IntegerDataObject : ValuedDataObject
    {

        public override object Value
        {
            set
            {
                this.value = Convert.ToInt32(value.ToString());
            }
        }

        public override BaseElement Clone()
        {
            IntegerDataObject clone = new IntegerDataObject
            {
                Values = this
            };
            return clone;
        }
    }

}