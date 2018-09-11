using System;

namespace org.activiti.bpmn.model
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

        public override BaseElement clone()
        {
            IntegerDataObject clone = new IntegerDataObject();
            clone.Values = this;
            return clone;
        }
    }

}