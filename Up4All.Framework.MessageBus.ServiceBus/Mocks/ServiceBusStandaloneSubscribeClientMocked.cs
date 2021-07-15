
using System;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.Abstractions.Mocks;

namespace Up4All.Framework.MessageBus.ServiceBus
{
    public class ServiceBusStandaloneSubscribeClientMocked : MessageBusSubscribeClientMock, IMessageBusStandaloneConsumer, IServiceBusClient, IDisposable
    {
        public ServiceBusStandaloneSubscribeClientMocked() : base()
        {
        }

        public override void RegisterHandler(Func<ReceivedMessage, MessageReceivedStatusEnum> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false)
        {
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
