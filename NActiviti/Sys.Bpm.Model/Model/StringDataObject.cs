namespace org.activiti.bpmn.model
{
    public class StringDataObject : ValuedDataObject
    {

        public override object Value
        {
            set
            {
                this.value = value.ToString();
            }
        }

        public override BaseElement clone()
        {
            StringDataObject clone = new StringDataObject();
            clone.Values = this;
            return clone;
        }
    }

}