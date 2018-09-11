namespace org.activiti.bpmn.model
{
    public abstract class ValuedDataObject : DataObject
    {

        protected internal object value;

        public virtual object Value
        {
            get
            {
                return value;
            }
            set { this.value = value; }
        }

        public override abstract BaseElement clone();

        public override BaseElement Values
        {
            set
            {
                base.Values = value;

                var val = value as ValuedDataObject;

                if (val.Value != null)
                {
                    Value = val.Value;
                }
            }
        }

        public virtual string Type
        {
            get
            {
                string structureRef = itemSubjectRef.StructureRef;
                return structureRef.Substring(structureRef.IndexOf(':') + 1);
            }
        }

        public override int GetHashCode()
        {
            if (this.Value == null)
            {
                return base.GetHashCode();
            }

            return this.GetType().GetHashCode() + this.value.GetHashCode();
        }

        public override bool Equals(object o)
        {
            if (this == o)
            {
                return true;
            }
            if (o == null || this.GetType() != o.GetType())
            {
                return false;
            }

            ValuedDataObject otherObject = (ValuedDataObject)o;

            if (!otherObject.ItemSubjectRef.StructureRef.Equals(this.itemSubjectRef.StructureRef))
            {
                return false;
            }
            if (!otherObject.Id.Equals(this.id))
            {
                return false;
            }
            if (!otherObject.Name.Equals(this.name))
            {
                return false;
            }
            if (!otherObject.Value.Equals(this.value.ToString()))
            {
                return false;
            }

            return true;
        }
    }

}