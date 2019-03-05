using org.springframework.hateoas;

namespace org.activiti.cloud.services.rest.api.resources
{

    public class HomeResource : ResourceSupport
    {
        private readonly string welcome = "Welcome to an instance of the Activiti Process Engine";

        public virtual string Welcome
        {
            get
            {
                return welcome;
            }
        }
    }

}