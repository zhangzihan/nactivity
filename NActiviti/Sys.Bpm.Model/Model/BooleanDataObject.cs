using System;

namespace Sys.Workflow.Bpmn.Models
{
    public class BooleanDataObject : ValuedDataObject
    {

        public override object Value
        {
            set
            {
                this.value = Convert.ToBoolean(value.ToString());
            }
        }

        public override BaseElement Clone()
        {
            BooleanDataObject clone = new BooleanDataObject
            {
                Values = this
            };
            return clone;
        }
    }

}