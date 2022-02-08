﻿
using Azure.Messaging.ServiceBus;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions;
using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Messages;

namespace Up4All.Framework.MessageBus.ServiceBus
{
    public class ServiceBusStandaloneQueueClient : MessageBusStandaloneQueueClient, IServiceBusClient, IDisposable
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _queueClient;
        private readonly string _queueName;
        private ServiceBusProcessor _processor;

        public ServiceBusStandaloneQueueClient(string connectionString, string queuename) : base(connectionString, queuename)
        {
            _queueName = queuename;
            (_client, _queueClient) = CreateClient(connectionString, queuename);
        }

        public override void RegisterHandler(Func<ReceivedMessage, MessageReceivedStatusEnum> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false)
        {
            _processor = _client.CreateProcessor(_queueName, new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = autoComplete
            });

            _processor.RegisterHandleMessage(handler, errorHandler, onIdle, autoComplete);
            _processor.StartProcessingAsync().Wait();
        }

        public override async Task Send(MessageBusMessage message)
        {
            await _queueClient.SendMessageAsync(this.PrepareMesssage(message));
        }

        public override async Task Send(IEnumerable<MessageBusMessage> messages)
        {
            var sbMessages = messages.Select(x => this.PrepareMesssage(x));
            await _queueClient.SendMessagesAsync(sbMessages.ToList());
        }

        public void Dispose()
        {
            Close().Wait();
        }

        public override async Task Close()
        {
            await _processor?.CloseAsync();
            await _queueClient?.CloseAsync();
            await _queueClient?.DisposeAsync().AsTask();
        }

        private (ServiceBusClient, ServiceBusSender) CreateClient(string connectionString, string queueName)
        {
            var client = new ServiceBusClient(connectionString);
            var queueClient = client.CreateSender(queueName);
            return (client, queueClient);
        }


    }
}
