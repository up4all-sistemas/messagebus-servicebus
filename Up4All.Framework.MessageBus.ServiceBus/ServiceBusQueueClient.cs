﻿
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions;
using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.Abstractions.Options;

namespace Up4All.Framework.MessageBus.ServiceBus
{
    public class ServiceBusQueueClient : MessageBusQueueClient, IServiceBusClient, IDisposable
    {
        private readonly QueueClient _client;

        public ServiceBusQueueClient(IOptions<MessageBusOptions> messageOptions) : base(messageOptions)
        {
            _client = CreateClient();
        }

        public override void RegisterHandler(Func<ReceivedMessage, MessageReceivedStatusEnum> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false)
        {
            ((IReceiverClient)_client).RegisterHandleMessage(handler, errorHandler, onIdle, autoComplete);
        }

        public override async Task Send(MessageBusMessage message)
        {
            await _client.SendAsync(this.PrepareMesssage(message));
        }

        public override async Task Send(IEnumerable<MessageBusMessage> messages)
        {
            var sbMessages = messages.Select(x => this.PrepareMesssage(x));
            await _client.SendAsync(sbMessages.ToList());
        }

        public void Dispose()
        {
            Close().Wait();
        }

        public override async Task Close()
        {
            await _client?.CloseAsync();
        }

        private QueueClient CreateClient()
        {
            var client = new QueueClient(MessageBusOptions.ConnectionString, MessageBusOptions.QueueName, ReceiveMode.PeekLock, RetryPolicy.Default);
            client.PrefetchCount = 1;
            return client;
        }


    }
}
