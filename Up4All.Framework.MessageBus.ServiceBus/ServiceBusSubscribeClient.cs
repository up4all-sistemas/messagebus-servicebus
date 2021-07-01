using Azure.Messaging.ServiceBus;

using Microsoft.Extensions.Options;

using System;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions;
using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.Abstractions.Options;
using Up4All.Framework.MessageBus.ServiceBus.Consumers;

namespace Up4All.Framework.MessageBus.ServiceBus
{
    public class ServiceBusSubscribeClient : MessageBusSubscribeClient, IServiceBusClient, IDisposable
    {
        private readonly ServiceBusClient _client;
        private QueueMessageReceiver _receiver;

        public ServiceBusSubscribeClient(IOptions<MessageBusOptions> messageOptions) : base(messageOptions)
        {
            _client = this.GetConnection(MessageBusOptions);
        }

        public override void RegisterHandler(Func<ReceivedMessage, MessageReceivedStatusEnum> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false)
        {
            _receiver = new QueueMessageReceiver($"{MessageBusOptions.TopicName}/subscriptions/{MessageBusOptions.SubscriptionName}", _client, handler, errorHandler, autoComplete);
            _receiver.Start().Wait();
        }

        public void Dispose()
        {
            _receiver?.Dispose();
        }

        public override Task Close()
        {
            _receiver?.Stop();
            return Task.CompletedTask;
        }
    }
}
