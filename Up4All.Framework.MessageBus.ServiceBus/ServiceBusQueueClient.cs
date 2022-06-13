
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

namespace Up4All.Framework.MessageBus.ServiceBus
{
    public class ServiceBusQueueClient : MessageBusQueueClient, IServiceBusClient, IDisposable
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _queueClient;
        private readonly MessageBusOptions _opts;
        private ServiceBusProcessor _processor;

        public ServiceBusQueueClient(IOptions<MessageBusOptions> messageOptions) : base(messageOptions)
        {
            _opts = messageOptions.Value;
            (_client, _queueClient) = this.CreateClient(messageOptions.Value);
        }

        public override async Task RegisterHandlerAsync(Func<ReceivedMessage, Task<MessageReceivedStatusEnum>> handler, Func<Exception, Task> errorHandler, Func<Task> onIdle = null, bool autoComplete = false)
        {
            _processor = CreateProcessor(autoComplete);
            await _processor.RegisterHandleMessageAsync(handler, errorHandler, onIdle, autoComplete);
            await _processor.StartProcessingAsync();
        }

        public override void RegisterHandler(Func<ReceivedMessage, MessageReceivedStatusEnum> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false)
        {
            _processor = CreateProcessor(autoComplete);
            _processor.RegisterHandleMessage(handler, errorHandler, onIdle, autoComplete);
            _processor.StartProcessingAsync().Wait();
        }

        private ServiceBusProcessor CreateProcessor(bool autoComplete)
        {
            return _client.CreateProcessor(_opts.QueueName, new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = autoComplete
            });
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
            await _client?.DisposeAsync().AsTask();
        }

    }
}
