
using Azure.Messaging.ServiceBus;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions;
using Up4All.Framework.MessageBus.Abstractions.Messages;

namespace Up4All.Framework.MessageBus.ServiceBus
{
    public class ServiceBusStandaloneTopicClient : MessageBusStandaloneTopicClient, IServiceBusClient
    {
        private readonly ServiceBusSender _client;

        public ServiceBusStandaloneTopicClient(string connectionString, string topicName) : base(connectionString, topicName)
        {
            _client = CreateClient(connectionString, topicName);
        }

        public override async Task Send(MessageBusMessage message)
        {
            await _client.SendMessageAsync(this.PrepareMesssage(message));
        }

        public override async Task Send(IEnumerable<MessageBusMessage> messages)
        {
            var sbMessages = messages.Select(x => this.PrepareMesssage(x));
            await _client.SendMessagesAsync(sbMessages.ToList());
        }

        private ServiceBusSender CreateClient(string connectionString, string topicName)
        {
            var client = new ServiceBusClient(connectionString);
            var topicClient = client.CreateSender(topicName);
            return topicClient;
        }
    }
}
