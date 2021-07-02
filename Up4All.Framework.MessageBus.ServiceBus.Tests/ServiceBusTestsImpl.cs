using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

using Up4All.Framework.MessageBus.Abstractions.Configurations;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Messages;

using Xunit;

namespace Up4All.Framework.MessageBus.ServiceBus.Tests
{
    public class ServiceBusTestsImpl
    {
        private readonly IServiceProvider _provider;
        private readonly IConfiguration _configuration;

        public ServiceBusTestsImpl()
        {
            _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

            var services = new ServiceCollection();

            services.AddMessageBusQueueClient<ServiceBusQueueClient>(_configuration);
            services.AddMessageBusTopicClient<ServiceBusTopicClient>(_configuration);
            services.AddMessageBusSubscribeClient<ServiceBusSubscribeClient>(_configuration);

            _provider = services.BuildServiceProvider();
        }

        [Fact]
        public async void QueueSendMessage()
        {
            var client = _provider.GetRequiredService<IMessageBusQueueClient>();

            var msg = new MessageBusMessage()
            {
            };
            msg.AddBody(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(new { teste = "teste", numero = 10 }));
            msg.UserProperties.Add("proptst", "tst");

            await client.Send(msg);

            Assert.True(true);
        }

        [Fact]
        public void QueueReceiveMessage()
        {
            var client = _provider.GetRequiredService<IMessageBusQueueClient>();

            client.RegisterHandler((msg) =>
            {
                Assert.True(msg != null);
                return Abstractions.Enums.MessageReceivedStatusEnum.Completed;
            }, (ex) => Debug.Print(ex.Message));


            Thread.Sleep(5000);
        }

        [Fact]
        public async void TopicSendMessage()
        {
            var client = _provider.GetRequiredService<IMessageBusPublisher>();

            var msg = new MessageBusMessage();
            msg.AddBody(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(new { teste = "teste", numero = 10 }));
            msg.UserProperties.Add("action", "precatorios-ocr");

            await client.Send(msg);

            Assert.True(true);
        }

        [Fact]
        public void SubscriptionReceiveMessage()
        {
            var client = _provider.GetRequiredService<IMessageBusConsumer>();

            client.RegisterHandler((msg) =>
            {
                Assert.True(msg != null);
                return Abstractions.Enums.MessageReceivedStatusEnum.Completed;
            }, (ex) => Debug.Print(ex.Message));

            Thread.Sleep(5000);
        }
    }
}
