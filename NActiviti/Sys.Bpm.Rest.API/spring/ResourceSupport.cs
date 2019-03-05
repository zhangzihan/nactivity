using System.Collections.Generic;

namespace org.springframework.hateoas
{
    public abstract class ResourceSupport
    {
        protected IList<Link> links = new List<Link>();

        public ResourceSupport add(Link link)
        {
            this.links.Add(link);

            return this;
        }

        public IList<Link> Links
        {
            get => links;
            set => links = value;
        }
    }
}
