namespace Sys.Workflow.bpmn.model
{
    public class DataObject : FlowElement
    {
        protected internal ItemDefinition itemSubjectRef;

        public virtual ItemDefinition ItemSubjectRef
        {
            get
            {
                return itemSubjectRef;
            }
            set
            {
                this.itemSubjectRef = value;
            }
        }


        public override BaseElement Clone()
        {
            DataObject clone = new DataObject
            {
                Values = this
            };
            return clone;
        }

        public override BaseElement Values
        {
            set
            {
                base.Values = value;
                var val = value as DataObject;

                Id = value.Id;
                Name = val.Name;
                ItemSubjectRef = val.ItemSubjectRef;
            }
        }
    }

}