namespace Sys.Workflow.Bpmn.Models
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

        public override BaseElement Clone()
        {
            StringDataObject clone = new StringDataObject
            {
                Values = this
            };
            return clone;
        }
    }

}