
using Azure.Messaging.ServiceBus;

using System;
using System.Linq;

using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.Abstractions.Options;

namespace Up4All.Framework.MessageBus.ServiceBus
{
    public static class ServiceBusClientExtensions
    {
        public static ServiceBusClient GetConnection(this IServiceBusClient client, MessageBusOptions opts)
        {
            var sbClient = new ServiceBusClient(opts.ConnectionString, new ServiceBusClientOptions
            {
                RetryOptions = new ServiceBusRetryOptions
                {
                    Mode = ServiceBusRetryMode.Exponential,
                    MaxRetries = 5,
                }
            });

            return sbClient;
        }

        public static ServiceBusMessage PrepareMesssage(this IServiceBusClient client, MessageBusMessage message)
        {
            var sbMessage = new ServiceBusMessage(new BinaryData(message.Body));
            if (message.UserProperties.Any())
                foreach (var prop in message.UserProperties)
                    sbMessage.ApplicationProperties.Add(prop);

            return sbMessage;
        }
    }

    
}
