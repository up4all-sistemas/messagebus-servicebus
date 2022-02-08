
using Azure.Messaging.ServiceBus;

using System;
using System.Linq;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Messages;

namespace Up4All.Framework.MessageBus.ServiceBus
{
    public static class ServiceBusClientExtensions
    {

        public static ServiceBusMessage PrepareMesssage(this IServiceBusClient client, MessageBusMessage message)
        {
            var sbMessage = new ServiceBusMessage(message.Body);
            if (message.UserProperties.Any())
                foreach (var prop in message.UserProperties)
                    sbMessage.ApplicationProperties.Add(prop.Key, prop.Value);

            return sbMessage;
        }

        public static void RegisterHandleMessage(this ServiceBusProcessor client, Func<ReceivedMessage, MessageReceivedStatusEnum> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false)
        {
            client.ProcessMessageAsync += (arg) =>
            {
                var received = new ReceivedMessage();
                received.AddBody(arg.Message.Body);

                if (arg.Message.ApplicationProperties.Any())
                    received.AddUserProperties(arg.Message.ApplicationProperties.ToDictionary(x => x.Key, x => x.Value));

                try
                {
                    var result = handler(received);

                    if (result == MessageReceivedStatusEnum.Deadletter)
                        arg.DeadLetterMessageAsync(arg.Message).Wait();
                    else if (result == MessageReceivedStatusEnum.Abandoned)
                        arg.AbandonMessageAsync(arg.Message).Wait();

                    if (!autoComplete) arg.CompleteMessageAsync(arg.Message).Wait();
                }
                catch (Exception)
                {
                    arg.AbandonMessageAsync(arg.Message).Wait();
                    throw;
                }

                return Task.CompletedTask;
            };

            client.ProcessErrorAsync += (ex) => {
                errorHandler(ex.Exception);
                return Task.CompletedTask;
            };
        }
    }




}
