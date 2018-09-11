using System.Text;

namespace org.activiti.bpmn.model
{
    public class ExtensionAttribute
    {

        protected internal string name;
        protected internal string value;
        protected internal string namespacePrefix;
        protected internal string @namespace;

        public ExtensionAttribute()
        {
        }

        public ExtensionAttribute(string name)
        {
            this.name = name;
        }

        public ExtensionAttribute(string @namespace, string name)
        {
            this.@namespace = @namespace;
            this.name = name;
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


        public virtual string Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
            }
        }


        public virtual string NamespacePrefix
        {
            get
            {
                return namespacePrefix;
            }
            set
            {
                this.namespacePrefix = value;
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


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (!string.ReferenceEquals(namespacePrefix, null))
            {
                sb.Append(namespacePrefix);
                if (!string.ReferenceEquals(name, null))
                {
                    sb.Append(":").Append(name);
                }
            }
            else
            {
                sb.Append(name);
            }
            if (!string.ReferenceEquals(value, null))
            {
                sb.Append("=").Append(value);
            }
            return sb.ToString();
        }

        public virtual ExtensionAttribute clone()
        {
            ExtensionAttribute clone = new ExtensionAttribute();
            clone.Values = this;
            return clone;
        }

        public virtual ExtensionAttribute Values
        {
            set
            {
                Name = value.Name;
                Value = value.Value;
                NamespacePrefix = value.NamespacePrefix;
                Namespace = value.Namespace;
            }
        }
    }

}