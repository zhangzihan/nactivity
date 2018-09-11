namespace org.activiti.bpmn.model
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


        public override BaseElement clone()
        {
            DataObject clone = new DataObject();
            clone.Values = this;
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