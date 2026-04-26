using System;
namespace Sys.Workflow.Bpmn.Models
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
                if (value is not DataObject val) throw new ArgumentException($"Expected {nameof(DataObject)}, got {value?.GetType().Name}", nameof(value));

                Id = value.Id;
                Name = val.Name;
                ItemSubjectRef = val.ItemSubjectRef;
            }
        }
    }

}