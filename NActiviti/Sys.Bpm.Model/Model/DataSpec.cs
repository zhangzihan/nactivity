namespace org.activiti.bpmn.model
{
    public class DataSpec : BaseElement
    {

        protected internal string name;
        protected internal string itemSubjectRef;
        protected internal bool isCollection;

        public virtual string Name
        {
            get
            {
                return name;
            }
            set
            {
                this.name = value;
            }
        }


        public virtual string ItemSubjectRef
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


        public virtual bool Collection
        {
            get
            {
                return isCollection;
            }
            set
            {
                this.isCollection = value;
            }
        }


        public override BaseElement Clone()
        {
            DataSpec clone = new DataSpec
            {
                Values = this
            };
            return clone;
        }

        public override BaseElement Values
        {
            set
            {
                var val = value as DataSpec;

                Name = val.Name;
                ItemSubjectRef = val.ItemSubjectRef;
                Collection = val.Collection;
            }
        }
    }

}