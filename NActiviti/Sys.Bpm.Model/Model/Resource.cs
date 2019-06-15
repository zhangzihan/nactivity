namespace org.activiti.bpmn.model
{
    /// <summary>
    /// The Resource class is used to specify resources that can be referenced by
    /// Activities. These Resources can be Human Resources as well as any other
    /// resource assigned to Activities during Process execution time.
    /// 
    /// </summary>
    public class Resource : BaseElement
    {

        protected internal string name;

        public Resource(string resourceId, string resourceName) : base()
        {
            Id = resourceId;
            Name = resourceName;
        }

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


        public override BaseElement Clone()
        {
            return new Resource(Id, Name);
        }
    }

}