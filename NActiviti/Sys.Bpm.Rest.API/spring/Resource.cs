using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace org.springframework.hateoas
{

    /// <summary>
    /// 
    /// </summary>
    public class Resource<T> : ResourceSupport
    {
        private T content;


        /// <summary>
        /// 
        /// </summary>
        protected Resource()
        {
            this.content = default(T);
        }

        /// <summary>
        /// 
        /// </summary>

        public Resource(T content, IEnumerable<Link> links)
        {
            this.content = content;
            this.links = (links ?? new List<Link>()).ToList();
        }


        /// <summary>
        /// 资源主体
        /// </summary>
        public T Content
        {
            get => content;
            set => content = value;
        }
    }
}
