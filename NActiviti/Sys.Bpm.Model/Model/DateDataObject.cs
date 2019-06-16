using System;

namespace Sys.Workflow.bpmn.model
{

    public class DateDataObject : ValuedDataObject
    {

        public override object Value
        {
            set
            {
                this.value = (DateTime)value;
            }
        }

        public override BaseElement Clone()
        {
            DateDataObject clone = new DateDataObject
            {
                Values = this
            };
            return clone;
        }
    }

}