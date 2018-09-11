using System;

namespace org.activiti.bpmn.model
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

        public override BaseElement clone()
        {
            DateDataObject clone = new DateDataObject();
            clone.Values = this;
            return clone;
        }
    }

}