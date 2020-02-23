using Foundatio.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Sys.Workflow.Cloud.Services.Api.Commands;
using Sys.Workflow.Services.Connectors.Models;
using System;
using System.Threading.Tasks;

namespace Sys.Workflow.Contexts
{
    public class InMemoryApplicationEventPublisher : IApplicationEventPublisher
    {
        private readonly InMemoryMessageBus inMemoryBus;

        public InMemoryApplicationEventPublisher()
        {
            inMemoryBus = new InMemoryMessageBus();
        }

        public IMessageBus MessageBus
        {
            get
            {
                return inMemoryBus;
            }
        }

        public void PublishEvent(IntegrationRequestEvent @event)
        {
            inMemoryBus.PublishAsync(@event);
        }

        public void PublishEvent(ICommand signalCmd)
        {
            inMemoryBus.PublishAsync(signalCmd);
        }
    }

    public class InMemoryApplicationEventSubscriber : IApplicationEventSubscriber
    {
        private readonly IMessageBus inMemoryMessageBus;

        public InMemoryApplicationEventSubscriber(IMessageBus inMemoryMessageBus)
        {
            this.inMemoryMessageBus = inMemoryMessageBus;
        }

        public Task SubscribeAsync<T>(Action<T> handler) where T : class
        {
            return inMemoryMessageBus.SubscribeAsync<T>((context, cancel) =>
            {
                handler(context);

                return Task.CompletedTask;
            });
        }
    }

    internal static class ApplicationEventPublisherExtensions
    {
        public static IServiceCollection UseInMemoryBus(this IServiceCollection services)
        {
            InMemoryApplicationEventPublisher publisher = new InMemoryApplicationEventPublisher();

            services.AddSingleton<IApplicationEventPublisher>(publisher);

            services.AddSingleton<IApplicationEventSubscriber>(new InMemoryApplicationEventSubscriber(publisher.MessageBus));

            return services;
        }
    }
}
