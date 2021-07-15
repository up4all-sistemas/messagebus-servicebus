
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

using System;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions;
using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Messages;

namespace Up4All.Framework.MessageBus.ServiceBus
{
    public class ServiceBusStandaloneSubscribeClient : MessageBusStandaloneSubscribeClient, IServiceBusClient , IDisposable
    {
        private readonly IReceiverClient _client;        

        public ServiceBusStandaloneSubscribeClient(string connectionString, string topicName, string subscriptionName) : base(connectionString, topicName, subscriptionName)
        {
            _client = CreateClient();
        }

        public override void RegisterHandler(Func<ReceivedMessage, MessageReceivedStatusEnum> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false)
        {
            _client.RegisterHandleMessage(handler, errorHandler, onIdle, autoComplete);
        }

        public override async Task Close()
        {
            await _client.CloseAsync();
        }

        public void Dispose()
        {
            Close().Wait();
        }

        private SubscriptionClient CreateClient()
        {
            var client = new SubscriptionClient(ConnectionString, TopicName, SubscriptionName, ReceiveMode.PeekLock, RetryPolicy.Default);
            client.PrefetchCount = 1;            
            return client;
        }

        
    }
}
