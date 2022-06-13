
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.Abstractions.Mocks;

namespace Up4All.Framework.MessageBus.ServiceBus
{
    public class ServiceBusStandaloneQueueClientMocked : MessageBusStandaloneQueueClientMock, IMessageBusStandaloneQueueClient, IServiceBusClient, IDisposable
    {
        public ServiceBusStandaloneQueueClientMocked() : base()
        {
        }

        public override Task RegisterHandlerAsync(Func<ReceivedMessage, Task<MessageReceivedStatusEnum>> handler, Func<Exception, Task> errorHandler, Func<Task> onIdle = null, bool autoComplete = false)
        {
            return Task.CompletedTask;
        }

        public override void RegisterHandler(Func<ReceivedMessage, MessageReceivedStatusEnum> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false)
        {

        }

        public override Task Send(MessageBusMessage message)
        {


            return Task.CompletedTask;
        }

        public override Task Send(IEnumerable<MessageBusMessage> messages)
        {

            return Task.CompletedTask;
        }

        public void Dispose()
        {

        }

        public override Task Close()
        {
            return Task.CompletedTask;
        }
    }
}
