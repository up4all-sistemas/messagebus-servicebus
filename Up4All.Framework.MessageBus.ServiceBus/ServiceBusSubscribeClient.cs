
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Options;

using System;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions;
using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.Abstractions.Options;

namespace Up4All.Framework.MessageBus.ServiceBus
{
    public class ServiceBusSubscribeClient : MessageBusSubscribeClient, IServiceBusClient , IDisposable
    {
        private readonly IReceiverClient _client;        

        public ServiceBusSubscribeClient(IOptions<MessageBusOptions> messageOptions) : base(messageOptions)
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
            var client = new SubscriptionClient(MessageBusOptions.ConnectionString, MessageBusOptions.TopicName, MessageBusOptions.SubscriptionName, ReceiveMode.PeekLock, RetryPolicy.Default);
            client.PrefetchCount = 1;
            return client;
        }

        
    }
}
