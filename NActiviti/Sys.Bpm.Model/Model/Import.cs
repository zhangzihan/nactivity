namespace Sys.Workflow.bpmn.model
{
    public class Import : BaseElement
    {

        protected internal string importType;
        protected internal string location;
        protected internal string @namespace;

        public virtual string ImportType
        {
            get
            {
                return importType;
            }
            set
            {
                this.importType = value;
            }
        }


        public virtual string Location
        {
            get
            {
                return location;
            }
            set
            {
                this.location = value;
            }
        }


        public virtual string Namespace
        {
            get
            {
                return @namespace;
            }
            set
            {
                this.@namespace = value;
            }
        }


        public override BaseElement Clone()
        {
            Import clone = new Import
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
                var val = value as Import;

                ImportType = val.ImportType;
                Location = val.Location;
                Namespace = val.Namespace;
            }
        }
    }

}