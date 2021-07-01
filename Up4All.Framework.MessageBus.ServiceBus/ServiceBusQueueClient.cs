using Azure.Messaging.ServiceBus;

using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions;
using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.Abstractions.Options;
using Up4All.Framework.MessageBus.ServiceBus.Consumers;

namespace Up4All.Framework.MessageBus.ServiceBus
{
    public class ServiceBusQueueClient : MessageBusQueueClient, IServiceBusClient, IDisposable
    {
        private readonly ServiceBusClient _client;
        private QueueMessageReceiver _receiver;

        public ServiceBusQueueClient(IOptions<MessageBusOptions> messageOptions) : base(messageOptions)
        {
            _client = this.GetConnection(MessageBusOptions);
        }

        public override void RegisterHandler(Func<ReceivedMessage, MessageReceivedStatusEnum> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false)
        {
            _receiver = new QueueMessageReceiver(MessageBusOptions.QueueName, _client, handler, errorHandler, autoComplete);
            _receiver.Start().Wait();
        }

        public override async Task Send(MessageBusMessage message)
        {
            var sender = _client.CreateSender(MessageBusOptions.QueueName);
            await sender.SendMessageAsync(this.PrepareMesssage(message));
        }

        public override async Task Send(IEnumerable<MessageBusMessage> messages)
        {
            var sender = _client.CreateSender(MessageBusOptions.QueueName);
            var sbMessages = messages.Select(x => this.PrepareMesssage(x));
            await sender.SendMessagesAsync(sbMessages);
        }

        public void Dispose()
        {
            _client?.DisposeAsync().GetAwaiter().GetResult();
            _receiver?.Dispose();
        }

        public override Task Close()
        {
            _receiver?.Dispose();
            return Task.CompletedTask;
        }

        
    }
}
