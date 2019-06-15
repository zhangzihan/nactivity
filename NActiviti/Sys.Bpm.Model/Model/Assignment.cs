namespace org.activiti.bpmn.model
{
    public class Assignment : BaseElement
    {

        protected internal string from;
        protected internal string to;

        public virtual string From
        {
            get
            {
                return from;
            }
            set
            {
                this.from = value;
            }
        }


        public virtual string To
        {
            get
            {
                return to;
            }
            set
            {
                this.to = value;
            }
        }


        public override BaseElement Clone()
        {
            Assignment clone = new Assignment
            {
                Values = this
            };
            return clone;
        }

        public override BaseElement Values
        {
            set
            {
                var val = value as Assignment;

                From = val.From;
                To = val.To;
            }
        }
    }

}