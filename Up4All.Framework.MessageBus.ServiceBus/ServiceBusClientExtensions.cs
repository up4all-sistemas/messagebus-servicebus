
using Azure.Messaging.ServiceBus;

using Microsoft.Extensions.Logging;

using Polly;

using System;
using System.Linq;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.Abstractions.Options;

namespace Up4All.Framework.MessageBus.ServiceBus
{
    public static class ServiceBusClientExtensions
    {

        public static (ServiceBusClient, ServiceBusSender) CreateClient(this IServiceBusClient sbclient, MessageBusOptions opts)
        {
            return CreateClient(sbclient, opts.ConnectionString, opts.QueueName, opts.ConnectionAttempts);
        }

        public static (ServiceBusClient, ServiceBusSender) CreateClient(this IServiceBusClient sbclient, string connectionString, string queueName, int attempts)
        {
            var logger = CreateLogger<IServiceBusClient>();

            var result = Policy
                .Handle<Exception>()
                .WaitAndRetry(attempts, retryAttempt =>
                {
                    TimeSpan wait = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                    logger.LogInformation($"Failed to create connect in ServiceBus server, retrying in {wait}");
                    return wait;
                })
                .ExecuteAndCapture(() =>
                {
                    logger.LogDebug($"Creating connection to ServiceBus server");
                    var client = new ServiceBusClient(connectionString);
                    var queueClient = client.CreateSender(queueName);
                    return (client, queueClient);
                });

            if (result.Outcome == OutcomeType.Successful)
                return result.Result;

            throw result.FinalException;
        }

        public static ServiceBusClient CreateClient(this IServiceBusClient sbclient, string connectionString, int attempts)
        {
            var logger = CreateLogger<IServiceBusClient>();

            var result = Policy
                .Handle<Exception>()
                .WaitAndRetry(attempts, retryAttempt =>
                {
                    TimeSpan wait = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                    return wait;
                })
                .ExecuteAndCapture(() =>
                {
                    return new ServiceBusClient(connectionString);
                });

            if (result.Outcome == OutcomeType.Successful)
                return result.Result;

            throw result.FinalException;
        }

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

                onIdle?.Invoke();

                return Task.CompletedTask;
            };

            client.ProcessErrorAsync += (ex) => {
                errorHandler(ex.Exception);
                return Task.CompletedTask;
            };
        }

        private static ILogger<T> CreateLogger<T>()
        {
            return LoggerFactory
                    .Create(cfg => { })
                    .CreateLogger<T>();
        }
    }




}
