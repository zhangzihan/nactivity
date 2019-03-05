using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace org.springframework.hateoas
{
    public class Resource<T> : ResourceSupport
    {
        private T content;

        protected Resource()
        {
            this.content = default(T);
        }

        public Resource(T content, IEnumerable<Link> links)
        {
            this.content = content;
            this.links = (links ?? new List<Link>()).ToList();
        }

        public T Content
        {
            get => content;
            set => content = value;
        }
    }
}
