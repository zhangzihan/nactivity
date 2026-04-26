using System;
namespace Sys.Workflow.Bpmn.Models
{
    public class DataStore : BaseElement
    {

        protected internal string name;
        protected internal string dataState;
        protected internal string itemSubjectRef;

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


        public override BaseElement Clone()
        {
            DataStore clone = new DataStore
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
                if (value is not DataStore val) throw new ArgumentException($"Expected {nameof(DataStore)}, got {value?.GetType().Name}", nameof(value));

                Name = val.Name;
                DataState = val.DataState;
                ItemSubjectRef = val.ItemSubjectRef;
            }
        }

    }

}