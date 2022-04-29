
using Azure.Messaging.ServiceBus;

using Microsoft.Extensions.Options;

using System;
using System.Linq;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions;
using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.Abstractions.Options;

namespace Up4All.Framework.MessageBus.ServiceBus
{
    public class ServiceBusSubscribeClient : MessageBusSubscribeClient, IServiceBusClient , IDisposable
    {
        private readonly ServiceBusClient _client;
        private readonly MessageBusOptions _opts;
        private ServiceBusProcessor _processor;

        public ServiceBusSubscribeClient(IOptions<MessageBusOptions> messageOptions) : base(messageOptions)
        {
            _opts = messageOptions.Value;
            _client = this.CreateClient(messageOptions.Value.ConnectionString, messageOptions.Value.ConnectionAttempts);
        }

        public override void RegisterHandler(Func<ReceivedMessage, MessageReceivedStatusEnum> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false)
        {
            _processor = _client.CreateProcessor(_opts.TopicName, _opts.SubscriptionName, new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = autoComplete
            });

            _processor.RegisterHandleMessage(handler, errorHandler, onIdle, autoComplete);
            _processor.StartProcessingAsync().Wait();
        }

        public override async Task Close()
        {
            await _processor?.CloseAsync();            
            await _client?.DisposeAsync().AsTask();
        }

        public void Dispose()
        {
            Close().Wait();
        }

        
    }
}
