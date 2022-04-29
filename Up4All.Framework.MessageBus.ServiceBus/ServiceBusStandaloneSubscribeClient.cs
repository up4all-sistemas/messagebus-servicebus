
using Azure.Messaging.ServiceBus;

using System;
using System.Linq;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions;
using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Messages;

namespace Up4All.Framework.MessageBus.ServiceBus
{
    public class ServiceBusStandaloneSubscribeClient : MessageBusStandaloneSubscribeClient, IServiceBusClient , IDisposable
    {
        private readonly ServiceBusClient _client;
        private readonly string _topicName;
        private readonly string _subscriptionName;
        private ServiceBusProcessor _processor;

        public ServiceBusStandaloneSubscribeClient(string connectionString, string topicName, string subscriptionName, int connectionAttempts = 8) : base(connectionString, topicName, subscriptionName)
        {
            _topicName = topicName;
            _subscriptionName = subscriptionName;
            _client = this.CreateClient(connectionString, connectionAttempts);
        }

        public override void RegisterHandler(Func<ReceivedMessage, MessageReceivedStatusEnum> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false)
        {
            _processor = _client.CreateProcessor(_topicName, _subscriptionName, new ServiceBusProcessorOptions
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
