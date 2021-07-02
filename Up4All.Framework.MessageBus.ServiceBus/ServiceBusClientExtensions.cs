
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

using System;
using System.Linq;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Messages;

namespace Up4All.Framework.MessageBus.ServiceBus
{
    public static class ServiceBusClientExtensions
    {

        public static Message PrepareMesssage(this IServiceBusClient client, MessageBusMessage message)
        {
            var sbMessage = new Message(message.Body);
            if (message.UserProperties.Any())
                foreach (var prop in message.UserProperties)
                    sbMessage.UserProperties.Add(prop.Key, prop.Value);

            return sbMessage;
        }

        public static void RegisterHandleMessage(this IReceiverClient client, Func<ReceivedMessage, MessageReceivedStatusEnum> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false)
        {
            client.RegisterMessageHandler(async (message, cancellationToken) =>
            {
                try
                {
                    var receivedMessage = new ReceivedMessage();
                    receivedMessage.AddBody(message.Body);

                    if (message.UserProperties.Any())
                        foreach (var prop in message.UserProperties)
                            receivedMessage.UserProperties.Add(prop);

                    var result = handler(receivedMessage);

                    if (result == MessageReceivedStatusEnum.Deadletter)
                        await client.DeadLetterAsync(message.SystemProperties.LockToken);
                    else if (result == MessageReceivedStatusEnum.Abandoned)
                        await client.AbandonAsync(message.SystemProperties.LockToken);

                    if (!autoComplete)
                        await client.CompleteAsync(message.SystemProperties.LockToken);
                }
                catch (Exception)
                {
                    await client.AbandonAsync(message.SystemProperties.LockToken);
                    throw;
                }
            }, new MessageHandlerOptions((ex) =>
            {
                errorHandler(ex.Exception);
                return Task.CompletedTask;
            })
            {
                AutoComplete = autoComplete,
                MaxConcurrentCalls = 1,
                MaxAutoRenewDuration = TimeSpan.FromMinutes(5)
            });
        }
    }




}
