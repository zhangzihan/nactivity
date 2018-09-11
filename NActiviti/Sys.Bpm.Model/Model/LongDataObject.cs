using System;

namespace org.activiti.bpmn.model
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

        public override BaseElement clone()
        {
            LongDataObject clone = new LongDataObject();
            clone.Values = this;
            return clone;
        }
    }

}