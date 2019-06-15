namespace org.activiti.bpmn.model
{
    public class DataStoreReference : FlowElement
    {

        protected internal string dataState;
        protected internal string itemSubjectRef;
        protected internal string dataStoreRef;

        public virtual string DataState
        {
            get
            {
                return dataState;
            }
            set
            {
                this.dataState = value;
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


        public virtual string DataStoreRef
        {
            get
            {
                return dataStoreRef;
            }
            set
            {
                this.dataStoreRef = value;
            }
        }


        public override BaseElement Clone()
        {
            DataStoreReference clone = new DataStoreReference
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
                var val = value as DataStoreReference;

                DataState = val.DataState;
                ItemSubjectRef = val.ItemSubjectRef;
                DataStoreRef = val.DataStoreRef;
            }
        }

    }

}