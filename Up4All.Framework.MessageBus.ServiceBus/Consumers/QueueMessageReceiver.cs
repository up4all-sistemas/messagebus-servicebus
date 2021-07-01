
using Azure.Messaging.ServiceBus;

using System;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Messages;

namespace Up4All.Framework.MessageBus.ServiceBus.Consumers
{
    public class QueueMessageReceiver : IDisposable
    {
        private readonly ServiceBusClient _client;
        private readonly Func<ReceivedMessage, MessageReceivedStatusEnum> _handler;
        private readonly Action<Exception> _errorHandler;
        private readonly ServiceBusProcessor _processor;
        private readonly bool _autoComplete;

        public QueueMessageReceiver(string entityName, ServiceBusClient client, Func<ReceivedMessage, MessageReceivedStatusEnum> handler, Action<Exception> errorHandler, bool autoComplete = false)
        {
            _client = client;
            _handler = handler;
            _errorHandler = errorHandler;
            _autoComplete = autoComplete;

            _processor = _client.CreateProcessor(entityName, new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = autoComplete,
                MaxConcurrentCalls = 1,
                PrefetchCount = 1,
                ReceiveMode = ServiceBusReceiveMode.PeekLock
            });

            _processor.ProcessMessageAsync += _processor_ProcessMessageAsync;
            _processor.ProcessErrorAsync += _processor_ProcessErrorAsync;            
        }

        public async Task Start()
        {
            await _processor.StartProcessingAsync();
        }

        public async Task Stop()
        {
            await _processor.StopProcessingAsync();
        }

        public void Dispose()
        {
            Stop().Wait();
        }

        private System.Threading.Tasks.Task _processor_ProcessErrorAsync(ProcessErrorEventArgs arg)
        {
            if (_errorHandler != null)
                _errorHandler(arg.Exception);

            return System.Threading.Tasks.Task.CompletedTask;
        }

        private async System.Threading.Tasks.Task _processor_ProcessMessageAsync(ProcessMessageEventArgs arg)
        {
            try
            {
                var body = arg.Message.Body;
                var properties = arg.Message.ApplicationProperties;

                var message = new ReceivedMessage();
                message.AddBody(body.ToArray());

                foreach (var prop in properties)
                    message.UserProperties.Add(prop);

                var response = _handler(message);

                if (response == MessageReceivedStatusEnum.Deadletter)
                    await arg.DeadLetterMessageAsync(arg.Message);

                if(!_autoComplete)
                    await arg.CompleteMessageAsync(arg.Message);
            }
            catch (Exception ex)
            {
                await arg.AbandonMessageAsync(arg.Message);
                _errorHandler(ex);
            }
        }
    }
}
