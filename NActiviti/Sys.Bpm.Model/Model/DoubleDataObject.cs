using System;

namespace org.activiti.bpmn.model
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

        public override BaseElement clone()
        {
            DoubleDataObject clone = new DoubleDataObject();
            clone.Values = this;
            return clone;
        }
    }

}