using System;

namespace org.activiti.bpmn.model
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

        public override BaseElement clone()
        {
            BooleanDataObject clone = new BooleanDataObject();
            clone.Values = this;
            return clone;
        }
    }

}