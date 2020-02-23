using System;
using System.Threading.Tasks;

namespace Sys.Workflow.Contexts
{
    public interface IApplicationEventSubscriber
    {
        Task SubscribeAsync<T>(Action<T> handler) where T : class;
    }
}