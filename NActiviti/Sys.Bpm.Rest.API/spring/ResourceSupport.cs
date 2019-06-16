using System.Collections.Generic;

namespace Sys.Workflow.Hateoas
{

    /// <summary>
    /// 
    /// </summary>
    public abstract class ResourceSupport
    {
        /// <summary>
        /// links
        /// </summary>
        protected IList<Link> links = new List<Link>();


        /// <summary>
        /// 添加链接资源
        /// </summary>
        public ResourceSupport Add(Link link)
        {
            this.links.Add(link);

            return this;
        }

        /// <summary>
        /// 资源链接
        /// </summary>

        public IList<Link> Links
        {
            get => links;
            set => links = value;
        }
    }
}
